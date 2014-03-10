//Sony Computer Entertainment Confidential

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

using WeifenLuo.WinFormsUI.Docking;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service to host controls and documents</summary>
    /// <remarks>This class is for internal use, and should not be instantiated or
    /// accessed directly by clients.</remarks>
    [Export(typeof(IControlHostService))]
    [Export(typeof(IControlRegistry))]
    [Export(typeof(IInitializable))]
    [Export(typeof(ControlHostService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ControlHostService : IControlHostService,
        IControlRegistry,
        ICommandClient,
        IPartImportsSatisfiedNotification,
        IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainForm">Main application Form</param>
        [ImportingConstructor]
        public ControlHostService(Form mainForm)
        {
            m_controls = new ActiveCollection<ControlInfo>();
            m_controls.ActiveItemChanged += new EventHandler(controls_ActiveItemChanged);
            m_controls.ActiveItemChanging += new EventHandler(controls_ActiveItemChanging);
            m_controls.ItemAdded += new EventHandler<ItemInsertedEventArgs<ControlInfo>>(controls_ItemAdded);
            m_controls.ItemRemoved += new EventHandler<ItemRemovedEventArgs<ControlInfo>>(controls_ItemRemoved);

            m_mainForm = mainForm;

            m_dockPanel = new DockPanel();
            m_dockPanel.Dock = DockStyle.Fill;
            m_dockPanel.DockBackColor = SystemColors.AppWorkspace;
            m_dockPanel.ShowDocumentIcon = true;
            m_dockPanel.ActiveContentChanged += new EventHandler(dockPanel_ActiveContentChanged);
        }

        #region IPartImportsSatisfiedNotification Members

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            // try to get toolstrip container for docking panel
            foreach (Control control in m_mainForm.Controls)
            {
                m_toolStripContainer = control as ToolStripContainer;
                if (m_toolStripContainer != null)
                    break;
            }

            if (m_toolStripContainer != null)
            {
                m_dockPanel.DocumentStyle = DocumentStyle.DockingWindow;
                m_toolStripContainer.ContentPanel.Controls.Add(m_dockPanel);
            }
            else
            {
                // main form needs to be MDI container
                m_mainForm.IsMdiContainer = true;

                m_mainForm.Controls.Add(m_dockPanel);
            }
        }

        #endregion

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // subscribe as late as possible to allow other parts to load/save state before controls are created/destroyed
            m_mainForm.Load += new EventHandler(mainForm_Load);
            m_mainForm.Closing += new CancelEventHandler(mainForm_Closing);

            m_dockPanel.SuspendLayout();

            ShowDefaultControls();

            m_dockPanel.ResumeLayout(false);

            if (m_settingsService != null)
            {
                SettingsServices.RegisterSettings(m_settingsService,
                    this,
                    new BoundPropertyDescriptor(this, "DockPanelState", "DockPanelState", null, null));
            }

            m_commandService.RegisterCommand(CommandInfo.WindowTileHorizontal, this);
            m_commandService.RegisterCommand(CommandInfo.WindowTileVertical, this);
            m_commandService.RegisterCommand(CommandInfo.WindowTileTabbed, this);
        }

        #endregion

        #region IControlRegistry Members

        /// <summary>
        /// Gets or sets the active control</summary>
        public ControlInfo ActiveControl
        {
            get { return m_controls.ActiveItem; }
            set { m_controls.ActiveItem = value; }
        }

        /// <summary>
        /// Event that is raised before the active control changes</summary>
        public event EventHandler ActiveControlChanging;

        /// <summary>
        /// Event that is raised after the active control changes</summary>
        public event EventHandler ActiveControlChanged;

        /// <summary>
        /// Gets the open controls, in order of least-recently-active to the active control</summary>
        public IEnumerable<ControlInfo> Controls
        {
            get { return m_controls; }
        }

        /// <summary>
        /// Event that is raised after a control is added; it will be the active control</summary>
        public event EventHandler<ItemInsertedEventArgs<ControlInfo>> ControlAdded;

        /// <summary>
        /// Event that is raised after a control is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<ControlInfo>> ControlRemoved;

        /// <summary>
        /// Removes the given control if it is open</summary>
        /// <param name="control">Control to remove</param>
        /// <returns>True iff the control was removed</returns>
        public bool RemoveControl(ControlInfo control)
        {
            return m_controls.Remove(control);
        }

        #endregion

        #region IControlHostService Members

        /// <summary>
        /// Registers a control so it becomes visible as part of the main form</summary>
        /// <param name="control">Control to register</param>
        /// <param name="info">Control display information</param>
        /// <param name="client">Client that owns the control and will receive notifications
        /// about its status, or null if no notifications are needed</param>
        public void RegisterControl(Control control, ControlInfo info, IControlHostClient client)
        {
            if (control == null)
                throw new ArgumentNullException("control");
            if (info == null)
                throw new ArgumentNullException("info");

            if (FindControlInfo(control) != null)
                throw new ArgumentException("Control already registered");

            // allow null client
            if (client == null)
                client = Global<DefaultClient>.Instance;

            info.Client = client;
            info.Control = control;

            info.Changed += new EventHandler(info_Changed);

            DockContent dockContent = new DockContent(this);
            UpdateDockContent(dockContent, info);

            m_dockContent.Add(info, dockContent);
            m_controls.ActiveItem = info;

            info.HostControl = dockContent;
            
            // Any property we set on this Control needs to be restored in UnregisterControl.
            //  For example, QuadPanelControl was broken by setting Dock property but not restoring it.
            info.OriginalDock = control.Dock;
            control.Dock = DockStyle.Fill;

            dockContent.Controls.Add(control);

            dockContent.FormClosing += new FormClosingEventHandler(dockContent_FormClosing);

            if (IsCenterGroup(info.Group))
            {
                dockContent.Show(m_dockPanel, DockState.Document);

                RegisterMenuCommand(control, "@" + dockContent.Text); // tells m_commandService not to interpret slashes as submenus
            }
            else
            {
                DockState state;
                switch (info.Group)
                {
                    case StandardControlGroup.Left:
                        state = DockState.DockLeft;
                        break;

                    case StandardControlGroup.Right:
                        state = DockState.DockRight;
                        break;

                    case StandardControlGroup.Top:
                        state = DockState.DockTop;
                        break;

                    case StandardControlGroup.Bottom:
                        state = DockState.DockBottom;
                        break;

                    case StandardControlGroup.Floating:
                        state = DockState.Float;
                        break;

                    default:
                        state = DockState.DockLeftAutoHide;
                        break;
                }

                dockContent.Show(m_dockPanel, state);

                RegisterMenuCommand(control, dockContent.Text); // tells m_commandService not to interpret slashes as submenus
            }

            UpdateContent(control);

            ActivateClient(client, true);
            Show(control);
        }
        
        /// <summary>
        /// Unregisters control</summary>
        /// <param name="control">Control to unregister</param>
        public void UnregisterControl(Control control)
        {
            ControlInfo info = FindControlInfo(control);
            if (info != null)
            {
                // Undoes the effects of RegisterControl(), in the reverse order.
                DockContent dockContent = FindContent(info);

                UnregisterMenuCommand(control);

                dockContent.FormClosing -= new FormClosingEventHandler(dockContent_FormClosing);

                dockContent.Controls.Remove(control);
                
                // Restore any properties we set on Control.
                control.Dock = info.OriginalDock;

                m_dockContent.Remove(info);
                m_controls.Remove(info);
                
                info.Changed -= new EventHandler(info_Changed);

                m_uniqueNamer.Retire(dockContent.Text);

                dockContent.Hide();
                dockContent.Dispose();

                info.HostControl = null;
            }
        }

        /// <summary>
        /// Makes a control visible</summary>
        /// <param name="control">Control to make visible</param>
        public void Show(Control control)
        {
            if (control != null)
            {
                ControlInfo controlInfo = FindControlInfo(control);
                if (controlInfo != null)
                {
                    DockContent dockContent = FindContent(controlInfo);
                    dockContent.Show(m_dockPanel);
                }
            }
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (commandTag is Control)
                return true;

            bool canDo = false;
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.WindowTileHorizontal:
                        canDo = true;
                        break;

                    case StandardCommand.WindowTileVertical:
                        canDo = true;
                        break;

                    case StandardCommand.WindowTileTabbed:
                        canDo = true;
                        break;
                }
            }

            return canDo;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            Control control = commandTag as Control;
            if (control != null)
            {
                DockContent dockContent = FindContent(control);
                if (dockContent.Visible)
                    dockContent.Hide();
                else
                    dockContent.Show();
            }

            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.WindowTileHorizontal:
                        TileDocumentContent(DockStyle.Right);
                        break;

                    case StandardCommand.WindowTileVertical:
                        TileDocumentContent(DockStyle.Bottom);
                        break;

                    case StandardCommand.WindowTileTabbed:
                        TileDocumentContent(DockStyle.Fill);
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
            Control control = commandTag as Control;
            if (control != null)
            {
                string menuText = GetControlMenuText(control);
                state.Text = menuText;
                DockContent dockContent = FindContent(control);
                state.Check = dockContent.Visible;
            }
        }

        #endregion

        /// <summary>
        /// Tile all dock panes in the active dock window
        /// </summary>
        /// <param name="dockStyle">Dock style</param>
        public void TileDocumentContent(DockStyle dockStyle)
        {
            DockWindow documentDockWindow = m_dockPanel.DockWindows[DockState.Document];
            DockPane activePane = documentDockWindow.NestedPanes[0];
            int count = m_dockPanel.DocumentsCount;
            foreach (IDockContent dockContent in m_dockPanel.Documents)
            {
                if (count > 1)
                {
                    if (dockContent != null)
                    {
                        DockContentHandler dockContentHandler = dockContent.DockHandler;
                        dockContentHandler.DockTo(activePane, dockStyle, -1, 1.0 / (double)count);
                    }
                }

                count--;
            }
        }

        /// <summary>
        /// Activates default client controls</summary>
        public void ShowDefaultControls()
        {
            ActivateClient(null, true);
        }

        /// <summary>
        /// Closes the control host</summary>
        /// <returns>True if user did not cancel close operation</returns>
        public bool Close()
        {
            List<ControlInfo> controls = new List<ControlInfo>(m_controls);
            // first close all controls that were registered in the center; these are the likely documents
            foreach (ControlInfo info in controls)
            {
                if (IsCenterGroup(info.Group) &&
                    !info.Client.Close(info.Control))
                {
                    return false;
                }
            }
            // close all other controls
            foreach (ControlInfo info in controls)
            {
                if (!IsCenterGroup(info.Group) &&
                    !info.Client.Close(info.Control))
                {
                    return false;
                }
            }

            return true;
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            m_formLoaded = true;
            if (!string.IsNullOrEmpty(m_dockPanelState))
                SetDockPanelState(m_dockPanelState);
        }

        private void mainForm_Closing(object sender, CancelEventArgs e)
        {
            // snapshot docking state before we close anything
            m_dockPanelState = GetDockPanelState();
            // attempt to close all documents
            e.Cancel = !Close();
        }

        private void controls_ActiveItemChanging(object sender, EventArgs e)
        {
            Event.Raise(ActiveControlChanging, this, EventArgs.Empty);
        }

        private void controls_ActiveItemChanged(object sender, EventArgs e)
        {
            Event.Raise(ActiveControlChanged, this, EventArgs.Empty);
        }

        private void controls_ItemAdded(object sender, ItemInsertedEventArgs<ControlInfo> e)
        {
            Event.Raise(ControlAdded, this, e);
        }

        private void controls_ItemRemoved(object sender, ItemRemovedEventArgs<ControlInfo> e)
        {
            Event.Raise(ControlRemoved, this, e);
        }

        private void dockPanel_ActiveContentChanged(object sender, EventArgs e)
        {
            DockContent activeContent = m_dockPanel.ActiveContent as DockContent;

            if (activeContent != m_activeDockContent)
            {
                if (m_activeDockContent != null &&
                    m_activeDockContent.Controls.Count > 0)
                {
                    DockPaneStripBase dockPaneStrip = GetDockPaneStripBase(m_activeDockContent);
                    if (dockPaneStrip != null)
                        dockPaneStrip.MouseUp -= new MouseEventHandler(dockPaneStrip_MouseUp);
                    DeactivateClient(m_activeDockContent.Controls[0]);
                }

                if (activeContent != null &&
                    activeContent.Controls.Count > 0)
                {
                    ActivateClient(activeContent.Controls[0]);

                    ControlInfo activeControlInfo = FindControlInfo(activeContent.Controls[0]);
                    if (activeControlInfo != null)
                    {
                        m_controls.ActiveItem = activeControlInfo;
                    }

                    // update the InActiveGroup property for all registered controls
                    DockPane activePane = activeContent.Pane;
                    foreach (ControlInfo controlInfo in m_controls)
                        controlInfo.InActiveGroup = activePane.Contains(controlInfo.HostControl);

                    DockPaneStripBase dockPaneStrip = GetDockPaneStripBase(activeContent);
                    if (dockPaneStrip != null)
                        dockPaneStrip.MouseUp += new MouseEventHandler(dockPaneStrip_MouseUp);
                }

                m_activeDockContent = activeContent;
            }
        }

        private DockPaneStripBase GetDockPaneStripBase(DockContent dockContent)
        {
            DockPane dockPane = dockContent.Pane;
            if (dockPane != null)
            {
                foreach (Control c in dockPane.Controls)
                {
                    DockPaneStripBase dockPaneStrip = c as DockPaneStripBase;
                    if (dockPaneStrip != null)
                        return dockPaneStrip;
                }
            }
            return null;
        }

        private void dockPaneStrip_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DockPaneStripBase dockPaneStrip = sender as DockPaneStripBase;

                ControlInfo info = FindControlInfo(m_activeDockContent.Controls[0]);
                IEnumerable<object> commands =
                    ContextMenuCommandProvider.GetCommands(m_contextMenuCommandProviders, null, info);

                Point screenPoint = dockPaneStrip.PointToScreen(new Point(e.X, e.Y));

                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

        private void dockContent_FormClosing(object sender, FormClosingEventArgs e)
        {
            DockContent dockContent = (DockContent)sender;

            Control control = dockContent.Controls[0];
            ControlInfo controlInfo = FindControlInfo(control);
            // close document forms if client allows; just hide other forms
            if (controlInfo.Client.Close(control))
            {
                // unregister center controls for the client if they haven't already done it
                if (IsCenterGroup(controlInfo.Group))
                {
                    UnregisterControl(control);
                    if (m_activeDockContent == dockContent)
                        m_activeDockContent = null;
                }
                else
                {
                    dockContent.Hide();
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void info_Changed(object sender, EventArgs e)
        {
            ControlInfo info = (ControlInfo)sender;
            DockContent dockContent = FindContent(info);
            UpdateDockContent(dockContent, info);

            UnregisterMenuCommand(info.Control);
            RegisterMenuCommand(info.Control, dockContent.Text);
        }

        private void ActivateClient(Control control)
        {
            ControlInfo info = FindControlInfo(control);
            if (info != null)
            {
                info.Client.Activate(control);
            }
        }

        private void DeactivateClient(Control control)
        {
            ControlInfo info = FindControlInfo(control);
            if (info != null)
            {
                info.Client.Deactivate(control);
            }
        }

        private void ActivateClient(IControlHostClient client, bool activating)
        {
            foreach (ControlInfo info in m_controls)
            {
                if (client == info.Client)
                {
                    DockContent dockContent = FindContent(info);
                    dockContent.BringToFront();
                }
           }
        }

        private ControlInfo FindControlInfo(Control control)
        {
            foreach (ControlInfo info in m_controls)
                if (info.Control == control)
                    return info;

            return null;
        }

        private DockContent FindContent(Control control)
        {
            ControlInfo controlInfo = FindControlInfo(control);
            return FindContent(controlInfo);
        }

        private DockContent FindContent(ControlInfo controlInfo)
        {
            DockContent dockContent;
            m_dockContent.TryGetValue(controlInfo, out dockContent);
            return dockContent;
        }

        private void UpdateDockContent(DockContent dockContent, ControlInfo info)
        {
            if (!string.IsNullOrEmpty(dockContent.Text))
                m_uniqueNamer.Retire(dockContent.Text);

            // the hosting control's name should be unique
            string displayName = info.Name;
            string uniqueDisplayName = m_uniqueNamer.Name(displayName);
            dockContent.Text = uniqueDisplayName;

            // the persistence ID should be unique to the editor, but not include a document's name
            string persistenceId = GetPersistenceId(info);
            if (persistenceId == displayName)
                persistenceId = uniqueDisplayName;
            dockContent.Name = info.Client.GetType().Name + ":" + persistenceId;

            dockContent.ToolTipText = info.Description;

            dockContent.ShowIcon = info.Image != null;
            dockContent.Icon =
                info.Image == null
                    ? null
                    : GdiUtil.CreateIcon(info.Image, 16, true);
            
            // To update the icon, we need to invalidate this Pane.
            if (dockContent.DockState == DockState.Document)
                dockContent.Pane.Invalidate(true);
        }

        private static string GetPersistenceId(ControlInfo info)
        {
            Control control = info.Control;

            // first, try Name
            string name = info.Name;

            // if no label, try Text
            if (string.IsNullOrEmpty(name))
                name = control.Text;

            // if no label, use generic tag
            if (string.IsNullOrEmpty(name))
                name = "document_panel";

            string label = name;
            if (name.StartsWith("@")) // literal control name
            {
                label = label.TrimStart('@');
            }
            else
            {
                int lastDelimiter = label.LastIndexOfAny(s_pathDelimiters);
                if (lastDelimiter >= 0)
                    label = "document_panel";
            }

            return label;
        }

        private string GetControlMenuText(Control control)
        {
            ControlInfo info = FindControlInfo(control);
            string name = info.Name;
            string label = name;
            if (name.StartsWith("@")) // literal control name
            {
                label = label.TrimStart('@');
            }
            else
            {
                // normal control name; display only the last segment, unless it's a central document
                if (!IsCenterGroup(info.Group))
                {
                    int lastDelimiter = label.LastIndexOfAny(s_pathDelimiters);
                    if (lastDelimiter >= 0)
                        label = label.Substring(lastDelimiter + 1);
                }
            }

            return label;
        }

        private void RegisterMenuCommand(Control control, string text)
        {
            if (m_commandService != null)
            {
                m_commandService.RegisterCommand(
                    new CommandInfo(
                        control,
                        StandardMenu.Window,
                        StandardCommandGroup.WindowDocuments,
                        text,
                        Localizer.Localize("Activate Window")),
                    this);
            }
        }

        private void UnregisterMenuCommand(Control control)
        {
            if (m_commandService != null)
                m_commandService.UnregisterCommand(control, this);
        }

        private static bool IsCenterGroup(StandardControlGroup groupTag)
        {
            return
                groupTag == StandardControlGroup.Center ||
                groupTag == StandardControlGroup.CenterPermanent;
        }

        private void UpdateContent(Control control)
        {
            ControlInfo info = FindControlInfo(control);
            DockContent dockContent = FindContent(info);
            dockContent.CloseButton = !info.Group.Equals(StandardControlGroup.CenterPermanent);
        }

        /// <summary>
        /// Gets or sets docking state</summary>
        public string DockPanelState
        {
            get
            {
                return GetDockPanelState();
            }
            set
            {
                m_dockPanelState = value;
                if (m_formLoaded)
                    SetDockPanelState(value);
            }
        }

        private void SetDockPanelState(string value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    // prepend an xml header, as it is stripped off by the settings service
                    writer.Write(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>");
                    writer.Write(value);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    // the dock panel must be cleared of all contents before deserializing
                    foreach (ControlInfo info in m_controls)
                    {
                        DockContent dockContent = FindContent(info);
                        dockContent.DockState = DockState.Unknown;
                        dockContent.DockPanel = null;
                        dockContent.FloatPane = null;
                        dockContent.Pane = null;
                    }

                    DeserializeDockContent deserializer = new DeserializeDockContent(StringToDockContent);
                    m_dockPanel.LoadFromXml(stream, deserializer, true);

                    // put back any docking content that had no persisted state
                    foreach (DockContent dockContent in m_dockContent.Values)
                        if (dockContent.DockPanel == null)
                            dockContent.Show(m_dockPanel);
                }
            }
        }

        private string GetDockPanelState()
        {
            string result;
            using (MemoryStream stream = new MemoryStream())
            {
                // save state as complete document; this improves the readability of XML
                m_dockPanel.SaveAsXml(stream, Encoding.UTF8);
                // stream is closed, so open a copy
                using (MemoryStream copy = new MemoryStream(stream.GetBuffer()))
                {
                    using (StreamReader reader = new StreamReader(copy))
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            return result;
        }

        private IDockContent StringToDockContent(string id)
        {
            foreach (DockContent dockContent in m_dockContent.Values)
                if (dockContent.Name == id)
                    return dockContent;
            return null;
        }

        private class DockContent : WeifenLuo.WinFormsUI.Docking.DockContent
        {
            public DockContent(ControlHostService controlHostService)
            {
                m_controlHostService = controlHostService;
            }
            private ControlHostService m_controlHostService;

            // override this to persist a unique string id
            protected override string GetPersistString()
            {
                return Name;
            }

            // override this to route command shortcuts to command service
            protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
            {
                bool consumed = false;

                // Route to command service if not a text input control. If it is a text input control,
                //  only route to the command service if the keypress is not standard textbox input.
                Control focusedControl = User32.GetFocusedControl();
                if (
                    ( !(focusedControl is TextBoxBase) && !(focusedControl is ComboBox)) ||
                    !KeysUtil.IsTextBoxInput(focusedControl, keyData)
                    )
                {
                    ICommandService commandService = m_controlHostService.m_commandService;
                    if (commandService != null)
                        consumed = commandService.ProcessKey(keyData);
                }

                // If the command key wasn't processed, then let the base handle it so that certain
                //  default behavior like the access keys (the underlines that appear when Alt is held down)
                //  still work correctly.
                if (!consumed)
                    consumed = base.ProcessCmdKey(ref msg, keyData);

                return consumed;
            }
        }

        private class DefaultClient : IControlHostClient
        {
            public void Activate(Control control) { }
            public void Deactivate(Control control) { }
            public bool Close(Control control) { return true; }
        }

        private Form m_mainForm;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService = null;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;
                
        private Dictionary<ControlInfo, DockContent> m_dockContent = new Dictionary<ControlInfo, DockContent>();
        private ActiveCollection<ControlInfo> m_controls;

        private UniqueNamer m_uniqueNamer = new UniqueNamer('(');

        private DockPanel m_dockPanel;
        private DockContent m_activeDockContent;
        private ToolStripContainer m_toolStripContainer;
        private string m_dockPanelState;
        private bool m_formLoaded;

        private static char[] s_pathDelimiters = new char[] { '/', '\\' };
    }
}
