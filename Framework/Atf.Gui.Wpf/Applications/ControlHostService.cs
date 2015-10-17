//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Xml;

using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Input;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Docking;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service to host content in the main dock panel</summary>
    /// <remarks>
    /// DAN: Lots to do to bring this in line with latest ATF version.
    /// Also, it may be worth checking out: http://msdn.microsoft.com/en-gb/magazine/cc785479.aspx#id0090168 and
    /// http://msdn.microsoft.com/en-us/library/cc707819.aspx. </remarks>
    [Export(typeof(IControlHostService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IDockStateProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ControlHostService : 
        Sce.Atf.Wpf.Applications.IControlHostService, 
        ICommandClient, 
        IInitializable,
        IDockStateProvider
    {
        /// <summary>
        /// Constructor with IMainWindow</summary>
        /// <param name="mainWindow">Main application form</param>
        [ImportingConstructor]
        public ControlHostService(IMainWindow mainWindow)
        {
            m_dockPanel = new Sce.Atf.Wpf.Docking.DockPanel();
            m_mainWindow = mainWindow;


            // TODO: Need to implement DockStateChanged. Reference it here to silence the compiler warning.
            if (DockStateChanged == null) return;
        }

        /// <summary>
        /// Get dock panel control</summary>
        public Control DockPanel
        {
            get { return m_dockPanel; }
        }

        #region IDockStateProvider Members

        /// <summary>
        /// Gets or sets the dock panel state</summary>
        object IDockStateProvider.DockState
        {
            get { return DockPanelState; }
            set { DockPanelState = (string)value; }
        }

        /// <summary>
        /// Event raised when the dock state changes</summary>
        public event EventHandler DockStateChanged;

        #endregion

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by setting up Settings Service</summary>
        public void Initialize()
        {
            // subscribe as late as possible to allow other parts to load/save state before controls are created/destroyed
            m_mainWindow.Closing += m_mainWindow_Closing;
            m_mainWindow.Loaded += m_mainWindow_Loaded;

            if (m_dockPanelSite != null)
                m_dockPanelSite.MainContent = m_dockPanel;

            ShowDefaultContents();

            if (m_settingsService != null)
            {
                m_settingsService.Reloaded += m_settingsService_Reloaded;

                m_settingsService.RegisterSettings(
                    this,
                    new BoundPropertyDescriptor(this, () => DockPanelState, "DockPanelState", null, null));
            }
        }

        #endregion

        #region IControlHostService Members

        /// <summary>
        /// Gets the active client</summary>
        public IControlHostClient ActiveClient
        {
            get { return m_activeClient; }
        }

        /// <summary>
        /// Gets the sequence of all registered contents and associated hosting information</summary>
        public IEnumerable<IControlInfo> Contents
        {
            get
            {
                foreach (var info in m_registeredContents)
                    yield return info;
            }
        }

        /// <summary>
        /// Gets or sets the dock panel state</summary>
        public string DockPanelState
        {
            get
            {
                // the cache should be retained if we're in the process of closing
                if (!m_closed)
                    m_cachedDockPanelState = GetDockPanelState();
                return m_cachedDockPanelState;
            }
            set
            {
                m_cachedDockPanelState = (string)value;
                SetDockPanelState(m_cachedDockPanelState);
            }
        }

        /// <summary>
        /// Registers a control with the control host service</summary>
        /// <param name="def">Control definition</param>
        /// <param name="control">Control</param>
        /// <param name="client">Client that owns the control and receives notifications
        /// about its status, or null if no notifications are needed</param>
        /// <returns>IControlInfo for registered control</returns>
        public IControlInfo RegisterControl(ControlDef def, object control, IControlHostClient client)
        {
            Requires.NotNull(def, "def");
            Requires.NotNull(control, "control");
            Requires.NotNullOrEmpty(def.Id, "def.Id");
            Requires.NotNull(client, "client");

            if (m_registeredContents.Any<ControlInfo>(x => x.Id == def.Id))
                throw new ArgumentException("Content with id " + def.Id + " already registered");

            IDockContent dockContent = m_dockPanel.RegisterContent(control, def.Id, ControlGroupToDockTo(def.Group));
            dockContent.IsFocusedChanged += DockContent_IsFocusedChanged;
            dockContent.Closing += DockContentOnClosing; 
            ControlInfo contentInfo = new ControlInfo(def.Name, def.Description, def.Id, def.Group, def.ImageSourceKey, dockContent, client);

            m_registeredContents.Add(contentInfo);

            if (m_commandService != null)
            {
                contentInfo.Command = m_commandService.RegisterCommand(
                    dockContent,
                    StandardMenu.Window,
                    StandardCommandGroup.WindowDocuments,
                    contentInfo.Name,
                    "Activate Control".Localize(),
                    Keys.None, null, CommandVisibility.Menu,
                    this).GetCommandItem();
            }

            ActivateClient(client, true);

            return contentInfo;
        }

        private void DockContentOnClosing(object sender, ContentClosedEventArgs e)
        {
            DockContent dockContent = (DockContent)sender;
            var controlInfo = FindControlInfo(dockContent.Content);
            controlInfo.Client.Close(dockContent.Content, false);
        }
  
        /// <summary>
        /// Unregisters content in control</summary>
        /// <param name="content">Content associated with control</param>
        public void UnregisterContent(object content)
        {
            var info = FindControlInfo(content);
            if (info != null)
            {
                if (m_commandService != null)
                {
                    m_commandService.UnregisterCommand(info.Command.CommandTag, this);
                }
                m_dockPanel.UnregisterContent(info.DockContent);
                info.DockContent.IsFocusedChanged -= DockContent_IsFocusedChanged;
                m_registeredContents.Remove(info);
                if (m_activeDockControl == info.Content)
                    m_activeDockControl = null;
            }
        }

        /// <summary>
        /// Makes a control visible by its associated content</summary>
        /// <param name="content">Content</param>
        public void Show(object content)
        {
            var info = FindControlInfo(content);
            if (info != null)
            {
                if (m_mainWindowLoaded)
                    m_dockPanel.ShowContent(info.DockContent);
                else
                    m_contentToShowOnMainWindowLoad.Add(content);
            }
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="tag">Command tag</param>
        /// <returns>true, if client can do the command</returns>
        public bool CanDoCommand(object tag)
        {
            return tag is IDockContent;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="tag">Command</param>
        public void DoCommand(object tag)
        {
            IDockContent dockContent = tag as IDockContent;
            if (dockContent != null)
            {
                if (m_dockPanel.IsContentVisible(dockContent))
                {
                    m_dockPanel.HideContent(dockContent);
                }
                else
                {
                    m_dockPanel.ShowContent(dockContent);

                    ControlInfo info = m_registeredContents.FirstOrDefault(x => x.Command.CommandTag == tag);
                    if (info != null)
                        info.Command.IsChecked = true;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState commandState) { }

        #endregion

        /// <summary>
        /// Activates default client content</summary>
        public void ShowDefaultContents()
        {
            ActivateClient(null, true);
        }

        /// <summary>
        /// Closes the content host</summary>
        /// <param name="mainWindowClosing">Content host window that is closing</param>
        /// <returns><c>True</c> if user did not cancel close operation</returns>
        public bool Close(bool mainWindowClosing)
        {
            foreach (var info in m_registeredContents.ToArray())
            {
                if (!info.Client.Close(info.Content, mainWindowClosing))
                    return false;
            }
            return true;
        }

        private void m_settingsService_Reloaded(object sender, EventArgs e)
        {
            SetDockPanelState(m_cachedDockPanelState);
            m_stateApplied = true;
        }

        private void m_mainWindow_Loaded(object sender, EventArgs e)
        {
            m_mainWindowLoaded = true;

            if (!m_stateApplied)
            {
                SetDockPanelState(null);
            }

            foreach (var content in m_contentToShowOnMainWindowLoad)
                Show(content);

            m_contentToShowOnMainWindowLoad.Clear();
        }

        private void m_mainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (e.Cancel == false)
            {
                // snapshot docking state before we close anything
                m_cachedDockPanelState = GetDockPanelState();
                // attempt to close all documents
                m_closed = Close(true);
                e.Cancel = !m_closed;
            }
        }

        private void DockContent_IsFocusedChanged(object sender, BooleanArgs e)
        {
            // Deactivate the previously active content, and activate the newly focused one
            IDockContent activeControl = m_dockPanel.GetActiveContent();
            if (activeControl != m_activeDockControl)
            {
                if (m_activeDockControl != null)
                    DeactivateClient(m_activeDockControl.Content);

                if (activeControl != null)
                    ActivateClient(activeControl.Content);

                m_activeDockControl = activeControl;
            }
        }

        private ControlInfo FindControlInfo(object content)
        {
            return m_registeredContents.FirstOrDefault<ControlInfo>(x => x.Content == content);
        }

        private void ActivateClient(object content)
        {
            IControlInfo info = FindControlInfo(content);
            if (info != null)
            {
                m_activeClient = info.Client;
                m_activeClient.Activate(content);
            }
        }

        private void DeactivateClient(object content)
        {
            IControlInfo info = FindControlInfo(content);
            if (info != null)
            {
                info.Client.Deactivate(content);
                m_activeClient = null;
            }
        }

        private void ActivateClient(IControlHostClient client, bool activating)
        {
            foreach (ControlInfo info in m_registeredContents)
            {
                if (client == info.Client)
                {
                    m_dockPanel.ShowContent(info.DockContent);
                }
            }
        }

        private void SetDockPanelState(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                m_dockPanel.ApplyLayout(null);
            }
            else
            {
                using (var stream = new MemoryStream())
                {
                    var writer = new StreamWriter(stream);

                    if (string.Compare(value, 0, "<?xml", 0, 5) != 0)
                    {
                        // prepend an xml header, as it is stripped off by the settings service
                        writer.Write(@"<?xml version=""1.0"" encoding=""utf-8""?>");
                    }
                    writer.Write(value);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    var reader = XmlReader.Create(stream);

                    try
                    {
                        m_dockPanel.ApplyLayout(reader);
                    }
                    catch
                    {
                        Sce.Atf.Outputs.WriteLine(OutputMessageType.Error, "Could not load window layout".Localize());
                    }
                }
            }
        }

        private string GetDockPanelState()
        {
            string result = null;
            using (var stream = new MemoryStream())
            {
                // save state as complete document; this improves the readability of XML
                var writer = XmlWriter.Create(stream);
                m_dockPanel.SaveLayout(writer);
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                var reader = new StreamReader(stream);
                result = reader.ReadToEnd();
            }
            return result;
        }

        private static DockTo ControlGroupToDockTo(Sce.Atf.Applications.StandardControlGroup standardControlGroup)
        {
            DockTo dockTo = DockTo.Top;
            switch (standardControlGroup)
            {
                case Sce.Atf.Applications.StandardControlGroup.Bottom:
                    dockTo = DockTo.Bottom;
                    break;
                case Sce.Atf.Applications.StandardControlGroup.Center:
                    dockTo = DockTo.Center;
                    break;
                case Sce.Atf.Applications.StandardControlGroup.CenterPermanent:
                    dockTo = DockTo.Center;
                    break;
                case Sce.Atf.Applications.StandardControlGroup.Floating:
                    dockTo = DockTo.Center;
                    break;
                case Sce.Atf.Applications.StandardControlGroup.Left:
                    dockTo = DockTo.Left;
                    break;
                case Sce.Atf.Applications.StandardControlGroup.Right:
                    dockTo = DockTo.Right;
                    break;
                case Sce.Atf.Applications.StandardControlGroup.Top:
                    dockTo = DockTo.Top;
                    break;
            }
            return dockTo;
        }

        private string m_cachedDockPanelState;
        private readonly IMainWindow m_mainWindow;
        private IControlHostClient m_activeClient;
        private List<ControlInfo> m_registeredContents = new List<ControlInfo>();
        private IDockContent m_activeDockControl;
        private Sce.Atf.Wpf.Docking.DockPanel m_dockPanel;
        bool m_stateApplied;
        bool m_mainWindowLoaded;
        bool m_closed;
        private HashSet<object> m_contentToShowOnMainWindowLoad = new HashSet<object>();
            
        [Import(AllowDefault = true)]
        private Sce.Atf.Applications.ISettingsService m_settingsService = null;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;

        [Import(AllowDefault = true)]
        private IMainWindowContentSite m_dockPanelSite = null;

    }
}
