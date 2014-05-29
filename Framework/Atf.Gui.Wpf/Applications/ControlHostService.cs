//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Xml;

using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Interop;

using Wws.UI.Docking;

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
        /// Constructor</summary>
        /// <param name="mainWindowAdapter">Main application form</param>
        [ImportingConstructor]
        public ControlHostService(MainWindowAdapter mainWindowAdapter)
        {
            m_mainWindow = mainWindowAdapter.MainWindow as Sce.Atf.Wpf.Controls.MainWindow;
            m_dockPanel = new Wws.UI.Docking.DockPanel();
            m_mainWindow.MainContent = m_dockPanel;
            mainWindowAdapter.Closing += m_mainWindow_Closing;
            mainWindowAdapter.Loaded += m_mainWindow_Loaded;

            // TODO: temporarily stop compiler warning
            // TODO: Needs to be implemented!
            if (DockStateChanged == null) return;
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

        private void m_settingsService_Reloaded(object sender, EventArgs e)
        {
            SetDockPanelState(m_cachedDockPanelState);
            m_stateApplied = true;
        }

        private void m_mainWindow_Loaded(object sender, EventArgs e)
        {
            if (!m_stateApplied)
            {
                SetDockPanelState(null);
            }
        }

        private void m_mainWindow_Closing(object sender, CancelEventArgs e)
        {
            // snapshot docking state before we close anything
            m_cachedDockPanelState = GetDockPanelState();
            // attempt to close all documents
            m_closed = Close(true);
            e.Cancel = !m_closed;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by setting up Settings Service</summary>
        public void Initialize()
        {
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
            dockContent.IsFocusedChanged += new EventHandler<BooleanArgs>(DockContent_IsFocusedChanged);

            ControlInfo contentInfo = new ControlInfo(def.Name, def.Description, def.Id, def.Group, def.ImageSourceKey, dockContent, client);

            m_registeredContents.Add(contentInfo);

            if (m_commandService != null)
            {
                var command = m_commandService.RegisterCommand(
                    new CommandDef(
                        dockContent,
                        StandardMenu.Window,
                        StandardCommandGroup.WindowDocuments,
                        contentInfo.Name,
                        null,
                        "Activate Control".Localize(),
                        null, null, CommandVisibility.Menu),
                    this);

                contentInfo.Command = command;
            }

            ActivateClient(client, true);

            Show(control);

            return contentInfo;
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
                    m_commandService.UnregisterCommand(info.Command, this);
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
                m_dockPanel.ShowContent(info.DockContent);
            }
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

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="command">command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object command)
        {
            ICommandItem commandItem = command as ICommandItem;
            Requires.NotNull(commandItem, "Command specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            return commandItem.CommandTag is IDockContent;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="command">Command</param>
        public void DoCommand(object command)
        {
            ICommandItem commandItem = command as ICommandItem;
            Requires.NotNull(commandItem, "Command specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            IDockContent dockContent = commandItem.CommandTag as IDockContent;
            if (dockContent != null)
            {
                if (m_dockPanel.IsContentVisible(dockContent))
                {
                    m_dockPanel.HideContent(dockContent);
                }
                else
                {
                    m_dockPanel.ShowContent(dockContent);
                    commandItem.IsChecked = true;
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
        /// <returns>True iff user did not cancel close operation</returns>
        public bool Close(bool mainWindowClosing)
        {
            foreach (var info in m_registeredContents.ToArray())
            {
                if (!info.Client.Close(info.Content, mainWindowClosing))
                    return false;
            }
            return true;
        }

        private void DockContent_IsFocusedChanged(object sender, BooleanArgs e)
        {
            //var docContent = sender as IDockContent;
            //if (docContent.IsFocused)
            //{
            //    var fwe = docContent.Content as FrameworkElement;
            //    if (fwe != null)
            //        fwe.Focus();
            //}

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

                    // the dock panel must be cleared of all contents before deserializing
                    //foreach (ControlInfo info in m_controlInfo)
                    //{
                    //    DockContent dockContent = FindContent(info);
                    //    dockContent.DockPanel = null;
                    //    dockContent.DockState = DockState.Unknown;
                    //    dockContent.FloatPane = null;
                    //    dockContent.Pane = null;
                    //}

                    //DeserializeDockContent deserializer = new DeserializeDockContent(StringToDockContent);
                    //m_dockPanel.LoadFromXml(stream, deserializer, true);
                    var reader = XmlReader.Create(stream);

                    try
                    {
                        m_dockPanel.ApplyLayout(reader);
                    }
                    catch
                    {
                        Sce.Atf.Outputs.WriteLine(OutputMessageType.Error, "Could not load window layout".Localize());
                    }

                    // put back any docking content that had no persisted state
                    //foreach (DockContent dockContent in m_dockContent.Values)
                    //    if (dockContent.DockPanel == null)
                    //        dockContent.Show(m_dockPanel);
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
        private IControlHostClient m_activeClient;
        private List<ControlInfo> m_registeredContents = new List<ControlInfo>();
        private Sce.Atf.Wpf.Controls.MainWindow m_mainWindow;
        private IDockContent m_activeDockControl;
        private Wws.UI.Docking.DockPanel m_dockPanel;
        bool m_stateApplied;
        bool m_closed = false;

        [Import(AllowDefault = true)]
        private Sce.Atf.Applications.ISettingsService m_settingsService = null;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;
    }
}
