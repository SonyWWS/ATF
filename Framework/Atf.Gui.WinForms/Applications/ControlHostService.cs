//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.PropertyEditing;

using WeifenLuo.WinFormsUI.Docking;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service to host controls and documents</summary>
    [Export(typeof(IControlHostService))]
    [Export(typeof(IControlRegistry))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IDockStateProvider))]
    [Export(typeof(ControlHostService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ControlHostService : IControlHostService,
        IControlRegistry,
        ICommandClient,
        IPartImportsSatisfiedNotification,
        IInitializable,
        IDockStateProvider
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainForm">Main application form</param>
        [ImportingConstructor]
        public ControlHostService(Form mainForm)
        {
            m_controls = new ActiveCollection<ControlInfo>();
            m_controls.ActiveItemChanged += controls_ActiveItemChanged;
            m_controls.ActiveItemChanging += controls_ActiveItemChanging;
            m_controls.ItemAdded += controls_ItemAdded;
            m_controls.ItemRemoved += controls_ItemRemoved;

            m_mainForm = mainForm;

            m_dockPanel = new DockPanel();
            m_dockPanel.Dock = DockStyle.Fill;
            m_dockPanel.DockBackColor = mainForm.BackColor;
            m_dockPanel.ShowDocumentIcon = true;
            m_dockPanel.ActiveContentChanged += dockPanel_ActiveContentChanged;
            m_dockPanel.ContentAdded += DockPanelContentAdded;
            m_dockPanel.ContentRemoved += DockPanelContentRemoved;

            // default behavior - when double clicking a float window's title it will
            // maximize the floating window instead of docking the floating window
            DoubleClickFloatWindowTitleMaximizes = true;

            // try a default delay (in milliseconds!)
            MouseOverTabSwitchDelay = 250;

            m_uiLockImage = ResourceUtil.GetImage24(Resources.LockUIImage);
            m_uiUnlockImage = ResourceUtil.GetImage24(Resources.UnlockUIImage);
        }

        /// <summary>
        /// Flags that determine which standard window commands should appear in the Window menu</summary>
        [Flags]
        public enum CommandRegister
        {
            /// <summary>Use no window commands</summary>
            None = 0,
            /// <summary>Use Tile Horizontal window commands</summary>
            WindowTileHorizontal = 1,
            /// <summary>Use Tile Vertical window commands</summary>
            WindowTileVertical = 2,
            /// <summary>Use Tile Tabbed window commands</summary>
            WindowTileTabbed = 4,
            /// <summary>Use Lock window commands</summary>
            UILock = 8,
            /// <summary>Use all window commands</summary>
            Default = UILock | WindowTileHorizontal | WindowTileVertical | WindowTileTabbed
        }

        /// <summary>
        /// Gets or sets whether a standard window command will be registered. Must
        /// be set before IInitialize is called.</summary>
        public CommandRegister RegisteredCommands
        {
            get { return m_registeredCommands; }
            set { m_registeredCommands = value; }
        }

        /// <summary>
        /// Gets or sets whether the user interface is currently locked</summary>
        private bool UILocked
        {
            get { return !m_dockPanel.AllowEndUserDocking; }
            set
            {
                m_dockPanel.AllowEndUserDocking = !value;
                var cmdService = m_commandService as CommandServiceBase;
                if (cmdService != null )
                {
                    if (cmdService.UserSelectedImageSize == CommandServiceBase.ImageSizes.Size16x16)
                    {
                        m_uiLockImage = ResourceUtil.GetImage16(Resources.LockUIImage);
                        m_uiUnlockImage = ResourceUtil.GetImage16(Resources.UnlockUIImage);
                    }
                    else if (cmdService.UserSelectedImageSize == CommandServiceBase.ImageSizes.Size32x32)
                    {
                        m_uiLockImage = ResourceUtil.GetImage32(Resources.LockUIImage);
                        m_uiUnlockImage = ResourceUtil.GetImage32(Resources.UnlockUIImage);
                    }
                }

                if ((RegisteredCommands & CommandRegister.UILock) == CommandRegister.UILock)
                {
                    CommandInfo.UILock.GetButton().Image = value ? m_uiLockImage : m_uiUnlockImage;
                    CommandInfo.UILock.GetButton().ToolTipText = value
                                                                     ? "Unlock UI Layout".Localize()
                                                                     : "Lock UI Layout".Localize();
                }

                if (m_toolStripContainer != null)
                {
                    var toolStrips = m_toolStripContainer.TopToolStripPanel.Controls.AsIEnumerable<ToolStrip>()
                        .Concat(m_toolStripContainer.BottomToolStripPanel.Controls.AsIEnumerable<ToolStrip>())
                        .Concat(m_toolStripContainer.LeftToolStripPanel.Controls.AsIEnumerable<ToolStrip>())
                        .Concat(m_toolStripContainer.RightToolStripPanel.Controls.AsIEnumerable<ToolStrip>());

                    foreach (var toolStrip in toolStrips)
                    {
                        toolStrip.GripStyle = value ? ToolStripGripStyle.Hidden : ToolStripGripStyle.Visible;
                        toolStrip.AllowItemReorder = !value;
                    }                  
                }
            }
        }

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
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
            m_mainForm.Load += mainForm_Load;
            m_mainForm.Shown += MainFormShown;
            m_mainForm.Closing += mainForm_Closing;

            m_dockPanel.SuspendLayout();

            ShowDefaultControls();

            m_dockPanel.ResumeLayout(false);

            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this,
                    new BoundPropertyDescriptor(this, () => DockPanelState, "DockPanelState", null, null));
                m_settingsService.RegisterSettings(this,
                   new BoundPropertyDescriptor(this, () => UILocked, "UILocked", null, null));
            }

            // Turn off the CommandService's polling of these commands.
            CommandInfo.UILock.EnableCheckCanDoEvent(this);
            CommandInfo.WindowTileHorizontal.EnableCheckCanDoEvent(this);
            CommandInfo.WindowTileVertical.EnableCheckCanDoEvent(this);
            CommandInfo.WindowTileTabbed.EnableCheckCanDoEvent(this);

            if (m_commandService != null)
            {
                if ((RegisteredCommands & CommandRegister.UILock) == CommandRegister.UILock)
                    m_commandService.RegisterCommand(CommandInfo.UILock, this);
                if ((RegisteredCommands & CommandRegister.WindowTileHorizontal) == CommandRegister.WindowTileHorizontal)
                    m_commandService.RegisterCommand(CommandInfo.WindowTileHorizontal, this);
                if ((RegisteredCommands & CommandRegister.WindowTileVertical) == CommandRegister.WindowTileVertical)
                    m_commandService.RegisterCommand(CommandInfo.WindowTileVertical, this);
                if ((RegisteredCommands & CommandRegister.WindowTileTabbed) == CommandRegister.WindowTileTabbed)
                    m_commandService.RegisterCommand(CommandInfo.WindowTileTabbed, this);
            }
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
        /// Event that is raised after a control is added; it will become the active control</summary>
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
        /// <param name="control">Control</param>
        /// <param name="info">Control display information</param>
        /// <param name="client">Client that owns the control and receives notifications
        /// about its status, or null if no notifications are needed</param>
        /// <remarks>If IControlHostClient.Close() has been called, the IControlHostService
        /// also calls UnregisterControl. Call RegisterControl again to re-register the Control.</remarks>
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
            info.Changed += info_Changed;

            DockContent dockContent = null;
            bool useControlInfoLayout = true;
            if (info.IsDocument.HasValue && info.IsDocument.Value)
            {
                foreach (DockContent unregisteredContent in m_unregisteredContents)
                {
                    if (unregisteredContent.Name == DocumentContentPrefix + info.Description)
                    {
                        dockContent = unregisteredContent;
                        m_unregisteredContents.Remove(unregisteredContent);
                        useControlInfoLayout = false;
                        break;
                    }
                }
            }

            if (dockContent == null)
            {
                dockContent = new DockContent(this);
                // set persistence id one time only.
                // do not update dockContent.Name            
                // dockContent.Text is used for titlebar.
                dockContent.Name = GetPersistenceId(info);
            }

            if (!string.IsNullOrEmpty(info.HelpUrl))
                dockContent.AddHelp(info.HelpUrl);

   
            UpdateDockContent(dockContent, info);

            m_dockContent.Add(info, dockContent);
            m_controls.ActiveItem = info;

            info.HostControl = dockContent;

            // Any property we set on this Control needs to be restored in UnregisterControl.
            //  For example, QuadPanelControl was broken by setting Dock property but not restoring it.
            info.OriginalDock = control.Dock;
            control.Dock = DockStyle.Fill;

            dockContent.Controls.Add(control);

            dockContent.FormClosing += dockContent_FormClosing;

            ShowDockContent(dockContent, useControlInfoLayout ? info : null);

            if (info.ShowInMenu)
            {
                if (info.IsDocument.HasValue && info.IsDocument.Value)
                {
                    // description( and the tooltip) for a document control by convention is the full path of the document
                    RegisterMenuCommand(info, "@" + info.Description);
                }
                else 
                    RegisterMenuCommand(info, "@" + dockContent.Text); // tells m_commandService not to interpret slashes as submenus
            }

            // Bring all the Controls for this client to the front.
            BringClientToFront(client);

            // Call the IControlHostClient's Activate method. Seems to be required when driving
            //  the app from a script and if the user clicks on another app at the wrong moment.
            ActivateClient(control);
        }

        /// <summary>
        /// Unregisters the control and removes it from its containing form</summary>
        /// <param name="control">Control to be unregistered</param>
        /// <remarks>This method is called by IControlHostService after IControlHostClient.Close() is called.</remarks>
        public void UnregisterControl(Control control)
        {
            ControlInfo info = FindControlInfo(control);
            if (info != null)
            {
                // Undoes the effects of RegisterControl(), in the reverse order.
                DockContent dockContent = FindContent(info);

                if (info.ShowInMenu)
                    UnregisterMenuCommand(control);

                dockContent.FormClosing -= dockContent_FormClosing;

                dockContent.Controls.Remove(control);

                // Restore any properties we set on Control.
                control.Dock = info.OriginalDock;

                m_dockContent.Remove(info);
                m_controls.Remove(info);

                info.Changed -= info_Changed;

                m_uniqueNamer.Retire(dockContent.Text);
                m_idNamer.Retire(dockContent.Name);

                dockContent.Hide();
                dockContent.Dispose();

                info.HostControl = null;
            }
        }

        /// <summary>
        /// Makes a registered control visible</summary>
        /// <param name="control">Control to be made visible</param>
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

        /// <summary>
        /// Hides the given control. Is the equivalent to using the Window menu command to hide a
        /// registered control.</summary>
        /// <param name="control">Previously registered control</param>
        public void Hide(Control control)
        {
            DockContent dockContent = FindContent(control);
            if (dockContent != null && dockContent.Visible)
                dockContent.Hide();
        }

        /// <summary>
        /// Returns an identifier object that represents the tab group that this control is a part of.</summary>
        /// <param name="control">Control</param>
        /// <returns>A tab group identifier or null, if this control isn't a part of any tab group</returns>
        public object GetTabGroup(Control control)
        {
            DockContent dockContent = FindContent(control);
            if (dockContent != null)
                return dockContent.Pane;
            return null;
        }

        /// <summary>
        /// Gets an enumeration of all tab group identifiers</summary>
        /// <returns>Enumeration of all tab group identifiers</returns>
        public IEnumerable<object> GetTabGroups()
        {
            foreach (DockPane pane in m_dockPanel.Panes)
                yield return pane;
        }

        /// <summary>
        /// Gets all of the registered Controls associated with a particular tab group identifier</summary>
        /// <param name="tabGroupID">A tab group identifier</param>
        /// <returns>An enumeration of Controls contained by this tab group. Might be empty, but won't be null.</returns>
        /// <remarks>This is not a hierarchical enumeration. Tab groups can be nested within tab groups, but
        /// only the controls that are registered with this ControlHostService that are contained within the
        /// tab group will be returned.</remarks>
        public IEnumerable<Control> GetControlsInTabGroup(object tabGroupID)
        {
            DockPane pane = tabGroupID as DockPane;
            if (pane != null)
            {
                foreach (KeyValuePair<ControlInfo, DockContent> pair in m_dockContent)
                {
                    if (pair.Value.Pane == tabGroupID)
                    {
                        yield return pair.Key.Control;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an object representing the docking panel that owns this control, or null. A docking panel
        /// contains up to 5 child tab groups (left, top, right, bottom, and center), each of which can
        /// contain further docking panels.</summary>
        /// <param name="control">A registered control</param>
        /// <returns>Object representing the docking panel that owns this control, or null if none</returns>
        public object GetPanel(Control control)
        {
            var panel = GetTabGroup(control) as DockPane;
            return panel != null ? panel.DockPanel : null;
        }

        /// <summary>
        /// Sets the relative size of the specified portion of the given docking panel.</summary>
        /// <param name="panel">An object representing a docking panel (like by calling GetPanel), or null,
        /// to specify the top-level docking panel owned by this ControlHostService.</param>
        /// <param name="group">An enum specifying the left, right, top, or bottom portion of the panel</param>
        /// <param name="portion">The size, as a non-zero fraction of the panel, between 0 and 1 or
        /// as an absolute extent in pixels greater than or equal to 1</param>
        public void SetPanelPortion(object panel, StandardControlGroup group, float portion)
        {
            DockPanel dockPanel;
            if (panel == null)
                dockPanel = m_dockPanel;
            else
                dockPanel = (DockPanel)panel;

            if (portion <= 0)
                throw new ArgumentOutOfRangeException("portion", "Must be greater than 0");

            switch (group)
            {
                case StandardControlGroup.Left:
                    dockPanel.DockLeftPortion = portion;
                    break;
                case StandardControlGroup.Right:
                    dockPanel.DockRightPortion = portion;
                    break;
                case StandardControlGroup.Top:
                    dockPanel.DockTopPortion = portion;
                    break;
                case StandardControlGroup.Bottom:
                    dockPanel.DockBottomPortion = portion;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("group", "Can only be the left, top, bottom, or right portions");
            }
        }

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
                    case StandardCommand.UILock:
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
                if (dockContent.Visible && !dockContent.IsHidden)
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
                    case StandardCommand.UILock:
                        UILocked = m_dockPanel.AllowEndUserDocking;
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
                state.Check = dockContent.Visible && !dockContent.IsHidden;
            }
            else if (commandTag is StandardCommand)
            {
                if ( (StandardCommand) commandTag == StandardCommand.UILock)
                {
                    state.Text = UILocked ? "Unlock UI Layout".Localize() : "Lock UI Layout".Localize();
                }
            }
        }

        #endregion

        #region IDockStateProvider Members

        /// <summary>
        /// Gets or sets the docking state</summary>
        object IDockStateProvider.DockState
        {
            get { return DockPanelState; }

            set
            {
                using (new User32.StopDrawingHelper(m_mainForm.Handle))
                {
                    DockPanelState = (string)value;
                }
                m_mainForm.Invalidate(true);
            }
        }

        /// <summary>
        /// Event raised when the state of DockPanelSuite changes</summary>
        public event EventHandler DockStateChanged;

        #endregion

        /// <summary>
        /// Tiles all dock panes in the active dock window</summary>
        /// <param name="dockStyle">Docking style</param>
        public void TileDocumentContent(DockStyle dockStyle)
        {
            DockWindow documentDockWindow = m_dockPanel.DockWindows[DockState.Document];
            if (documentDockWindow.NestedPanes.Count > 0)
            {
                DockPane activePane = null;
                int count = m_dockPanel.DocumentsCount;
                foreach (IDockContent dockContent in m_dockPanel.Documents)
                {
                    if (dockContent != null)
                    {
                        DockContentHandler dockContentHandler = dockContent.DockHandler;
                        DockPane paneFrom = dockContentHandler.Pane;
                        if (activePane == null)
                            activePane = paneFrom;
                        else
                        {
                            dockContentHandler.DockTo(activePane, dockStyle, -1, 1.0/(double) count);
                            count--;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activates default client controls</summary>
        public void ShowDefaultControls()
        {
            BringClientToFront(null);
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
                if (!m_controls.Contains(info))
                    continue; // related controls may be removed programmatically in-between 
                if (IsCenterGroup(info.Group) &&
                    !info.Client.Close(info.Control))
                {
                    return false;
                }
            }
            // close all other controls
            foreach (ControlInfo info in controls)
            {
                if (!m_controls.Contains(info))
                    continue; 
                if (!IsCenterGroup(info.Group) &&
                    !info.Client.Close(info.Control))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets or sets whether double clicking a floating window's
        /// title maximizes the window or docks the window</summary>
        public static bool DoubleClickFloatWindowTitleMaximizes
        {
            get { return FloatWindow.DoubleClickTitleMaximizes; }
            set { FloatWindow.DoubleClickTitleMaximizes = value; }
        }

        /// <summary>
        /// Gets or sets the delay in milliseconds before a docked tab gets switched to when dragging over it</summary>
        public static int MouseOverTabSwitchDelay
        {
            get { return DockPaneStripBase.MouseOverTabSwitchDelay; }
            set { DockPaneStripBase.MouseOverTabSwitchDelay = value; }
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            m_formLoaded = true;
            if (!string.IsNullOrEmpty(m_dockPanelState))
                SetDockPanelState(m_dockPanelState);
        }

        private void MainFormShown(object sender, EventArgs e)
        {
            m_canFireDockStateChanged = true;
        }

        private void mainForm_Closing(object sender, CancelEventArgs e)
        {
            // Check if another listener has cancelled the event already.
            if (e.Cancel)
                return;

            // close these unregistered DockContents so they are not persisted in saved layout
            foreach (var unregisteredContent in m_unregisteredContents)
            {
                unregisteredContent.DockHandler.Form.Close();
            }

            // snapshot docking state before we close anything;
            m_formLoaded = false; // persuade DockPanelState getter to use the cached instead of the current docking state
            m_dockPanelState = GetDockPanelState();

            m_canFireDockStateChanged = false;

            // Attempt to close all documents.
            e.Cancel = !Close();

            // turn back on or keep off
            m_canFireDockStateChanged = e.Cancel;
        }

        private void controls_ActiveItemChanging(object sender, EventArgs e)
        {
            ActiveControlChanging.Raise(this, EventArgs.Empty);
        }

        private void controls_ActiveItemChanged(object sender, EventArgs e)
        {
            ActiveControlChanged.Raise(this, EventArgs.Empty);
        }

        private void controls_ItemAdded(object sender, ItemInsertedEventArgs<ControlInfo> e)
        {
            // Both Legacy ATF and ATF 3's SkinService subscribe to the ControlAdded event, so this line
            //  was causing the skin to be applied twice.
            //SkinService.ApplyActiveSkin(e.Item.Control);
            ControlAdded.Raise(this, e);
        }

        private void controls_ItemRemoved(object sender, ItemRemovedEventArgs<ControlInfo> e)
        {
            ControlRemoved.Raise(this, e);
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
                        dockPaneStrip.MouseUp -= dockPaneStrip_MouseUp;
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
                        dockPaneStrip.MouseUp += dockPaneStrip_MouseUp;
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
                    m_contextMenuCommandProviders.GetCommands(null, info);

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

            if (info.ShowInMenu)
            {
                UnregisterMenuCommand(info.Control);
                if (info.IsDocument.HasValue && info.IsDocument.Value)
                {
                    // description( and the tooltip) for a document control by convention is the full path of the document
                    RegisterMenuCommand(info, "@" + info.Description);
                }
                else
                {
                    RegisterMenuCommand(info, "@" + dockContent.Text); // tells m_commandService not to interpret slashes as submenus
                }
            }
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

        private void BringClientToFront(IControlHostClient client)
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

            string displayName = info.Name;
            if (info.IsDocument.HasValue && info.IsDocument.Value)
            {
                dockContent.Text = displayName;
                dockContent.Name = DocumentContentPrefix + info.Description;
            }
            else
            {
                // make the hosting control's name unique for non-document controls
                string uniqueDisplayName = m_uniqueNamer.Name(displayName);
                dockContent.Text = uniqueDisplayName;
            }

            dockContent.ToolTipText = info.Description;
            dockContent.ShowIcon = info.Image != null;
            dockContent.Icon =
                info.Image == null
                    ? null
                    : GdiUtil.CreateIcon(info.Image, 16, true);

            bool useCloseButton = !info.Group.Equals(StandardControlGroup.CenterPermanent);
            dockContent.CloseButton = useCloseButton;
            dockContent.CloseButtonVisible = useCloseButton;
            if (info.Docking != null)
            {
                dockContent.DockAreas = (DockAreas)info.Docking.DockAreas;
                dockContent.GroupTag = info.Docking.GroupTag;
                dockContent.Order = info.Docking.Order;
            }

            // To update the icon, we need to invalidate this Pane.
            if (dockContent.DockState == DockState.Document &&
                dockContent.Pane != null)
            {
                dockContent.Pane.Invalidate(true);
            }
        }

        private void ShowDockContent(DockContent dockContent, ControlInfo info)
        {
            if (info == null)
            {
                dockContent.Show(m_dockPanel);
                return;
            }

            DockState state;
            if (IsCenterGroup(info.Group))
            {

                if (dockContent.DockHandler.IsFloat)
                    state = DockState.Float;
                else
                    state = DockState.Document;
            }
            else
            {
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

            }

            if (dockContent.GroupTag != null && state != DockState.Float)
            {
                // try floating window first to see if there is a match
                foreach (var floatWindow in m_dockPanel.FloatWindows)
                {
                    foreach (var pane in floatWindow.VisibleNestedPanes)
                    {
                        if (pane.GroupTag == dockContent.GroupTag)
                        {
                            state = DockState.Float;
                        }
                    }
                }
            }
            dockContent.Show(m_dockPanel, state);

            if (!info.VisibleByDefault)
                dockContent.Hide();
        }

        private string GetPersistenceId(ControlInfo info)
        {
            if (info.IsDocument.HasValue && info.IsDocument.Value)
                return info.Name;

            // first, try Name            
            string name = info.Name;

            // don't use name as a part of id if it is too long 
            bool usedefault
                = string.IsNullOrEmpty(name)
                || name.Length > 64
                || name.IndexOfAny(s_pathDelimiters) > 0
                || name.Contains(".");

            if (usedefault)
                name = "document_panel";
            string id = info.Client.GetType().Name + "_" + name;
            id = m_idNamer.Name(id);
            return id;
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

        private void RegisterMenuCommand(ControlInfo info, string text)
        {
            if (m_commandService != null)
            {
                bool isDocument;
                if (info.IsDocument.HasValue)
                    isDocument = info.IsDocument.Value;
                else
                    isDocument = info.Client is IDocumentClient;

                var commandInfo = new CommandInfo(
                    info.Control,
                    StandardMenu.Window,
                    isDocument ? StandardCommandGroup.WindowDocuments : StandardCommandGroup.WindowGeneral,
                    text,
                    "Activate Window".Localize());

                // CanDoCommand() always returns true, so let's disable polling.
                commandInfo.EnableCheckCanDoEvent(this);

                m_commandService.RegisterCommand(commandInfo, this);
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

        /// <summary>
        /// Converts relative sized portions of the given docking panel to absolute dimensions.</summary>
        private void ConvertToAbsolutePanelPortions()
        {
            DockPanel dockPanel = m_dockPanel;

            int height = dockPanel.ClientRectangle.Height - dockPanel.DockPadding.Bottom - dockPanel.DockPadding.Top;
            int width = dockPanel.ClientRectangle.Width - dockPanel.DockPadding.Left - dockPanel.DockPadding.Right;

            if (dockPanel.DockBottomPortion < 1.0)
                dockPanel.DockBottomPortion *= height;

            if (dockPanel.DockLeftPortion < 1.0)
                dockPanel.DockLeftPortion *= width;

            if (dockPanel.DockRightPortion < 1.0)
                dockPanel.DockRightPortion *= width;

            if (dockPanel.DockTopPortion < 1.0)
                dockPanel.DockTopPortion *= height;
        }

        /// <summary>
        /// Gets or sets the docking state description</summary>
        public string DockPanelState
        {
            get
            {
                // return the previously set state if the main form had not yet loaded,
                // to prevent dock panel state being corrupted for the WindowLayoutService.
                if (!m_formLoaded && !string.IsNullOrEmpty(m_dockPanelState))
                    return m_dockPanelState;

                return GetDockPanelState();
            }
            set
            {
                m_dockPanelState = value;
                if (m_formLoaded)
                {
                    SetDockPanelState(value);
                }
            }
        }

        private void SetDockPanelState(string value)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    // prepend an xml header, if it has been stripped off
                    // Note: It would be better if this was done in the SettingsService as 'value' should be valid XML.
                    int length = Math.Min(value.Length, 20);
                    string xmlheader = StringUtil.RemoveAllWhiteSpace(value.Substring(0, length));
                    if (!xmlheader.StartsWith("<?xmlversion=", StringComparison.OrdinalIgnoreCase))
                        writer.Write(@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>");

                    writer.Write(value);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);

                    // The dock panel must be cleared of all contents before deserializing.
                    foreach (ControlInfo info in m_controls)
                    {
                        DockContent dockContent = FindContent(info);
                        //It's important to set DockState to Unknown before setting DockPanel to null,
                        //  to avoid a crash.
                        dockContent.DockState = DockState.Unknown;
                        dockContent.DockPanel = null;
                        dockContent.FloatPane = null;
                        dockContent.Pane = null;
                    }

                    DeserializeDockContent deserializer = StringToDockContent;
                    m_dockPanel.LoadFromXml(stream, deserializer, true);

                    // Put back any docking content that had no persisted state.
                    // We iterate through m_dockContent rather than m_controls because m_controls can get
                    //  modified inside the loop, when calling ShowDockContent.
                    foreach (var pair in m_dockContent)
                    {
                        DockContent dockContent = pair.Value;
                        if (dockContent.DockPanel == null)
                        {
                            ControlInfo info = pair.Key;
                            UpdateDockContent(dockContent, info);
                            ShowDockContent(dockContent, info);
                        }
                    }

                    // Hide these unregistered DockContents until client code calls RegisterControl().
                    foreach (DockContent unregisteredContent in m_unregisteredContents)
                    {
                        unregisteredContent.Hide();
                    }
                }
            }

            ConvertToAbsolutePanelPortions();
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

        // The callback method from DockPanelSuite, to get a DockContent object.
        private IDockContent StringToDockContent(string id)
        {
            foreach (DockContent dockContent in m_dockContent.Values)
                if (dockContent.Name == id)
                    return dockContent;

            // Save document windows until RegisterControl() is called.
            if (id.StartsWith(DocumentContentPrefix))
            {
                var result = new DockContent(this);
                result.Name = id;
                m_unregisteredContents.Add(result);
                return result;
            }

            return null;
        }

        private void DockPanelContentAdded(object sender, DockContentEventArgs e)
        {
            DockHandlerSubscribe(e.Content);
            OnDockStateChanged();
        }

        private void DockPanelContentRemoved(object sender, DockContentEventArgs e)
        {
            DockHandlerUnsubscribe(e.Content);
            OnDockStateChanged();
        }

        private void DockHandlerSubscribe(IDockContent content)
        {
            if (content == null)
                return;

            DockContentHandler handler = content.DockHandler;
            if (handler == null)
                return;

            handler.DockStateChanged += DockPanelSomethingChanged;

            if (handler.DockPanel != null)
                handler.DockPanel.SizeChanged += DockPanelSomethingChanged;

            if (handler.Form != null)
                handler.Form.SizeChanged += DockPanelSomethingChanged;

            if (handler.FloatPane != null)
                handler.FloatPane.SizeChanged += DockPanelSomethingChanged;

            if (handler.Pane != null)
                handler.Pane.SizeChanged += DockPanelSomethingChanged;

            if (handler.PanelPane != null)
                handler.PanelPane.SizeChanged += DockPanelSomethingChanged;
        }

        private void DockHandlerUnsubscribe(IDockContent content)
        {
            if (content == null)
                return;

            DockContentHandler handler = content.DockHandler;
            if (handler == null)
                return;

            handler.DockStateChanged -= DockPanelSomethingChanged;

            if (handler.DockPanel != null)
                handler.DockPanel.SizeChanged -= DockPanelSomethingChanged;

            if (handler.Form != null)
                handler.Form.SizeChanged -= DockPanelSomethingChanged;

            if (handler.FloatPane != null)
                handler.FloatPane.SizeChanged -= DockPanelSomethingChanged;

            if (handler.Pane != null)
                handler.Pane.SizeChanged -= DockPanelSomethingChanged;

            if (handler.PanelPane != null)
                handler.PanelPane.SizeChanged -= DockPanelSomethingChanged;
        }

        private void DockPanelSomethingChanged(object sender, EventArgs e)
        {
            OnDockStateChanged();
        }

        private void OnDockStateChanged()
        {
            //Outputs.WriteLine(OutputMessageType.Info, "OnDockStateChanged: {0}", DateTime.Now.Ticks);

            if (!m_canFireDockStateChanged)
                return;

            DockStateChanged.Raise(this, EventArgs.Empty);
        }

        private static TabGradient GetTabGradientFromControlGradient(ControlGradient controlGradient)
        {
            return new TabGradient()
            {
                StartColor = controlGradient.StartColor,
                EndColor = controlGradient.EndColor,
                LinearGradientMode = controlGradient.LinearGradientMode,
                TextColor = controlGradient.TextColor
            };
        }

        private static ControlGradient GetControlGradientFromDockPanelGradient(DockPanelGradient dockPanelGradient)
        {
            var controlGradient = new ControlGradient()
            {
                StartColor = dockPanelGradient.StartColor,
                EndColor = dockPanelGradient.EndColor,
                LinearGradientMode = dockPanelGradient.LinearGradientMode
            };

            var tabGradient = dockPanelGradient as TabGradient;
            if (tabGradient != null)
                controlGradient.TextColor = tabGradient.TextColor;

            return controlGradient;
        }

        private class DockPanel : WeifenLuo.WinFormsUI.Docking.DockPanel
        {
            public DockColors DockColors
            {
                get
                {
                    if (Skin == null)
                        Skin = new DockPanelSkin();

                    return new DockColors()
                    {
                        AutoHideDockStripGradient = GetControlGradientFromDockPanelGradient(Skin.AutoHideStripSkin.DockStripGradient),
                        AutoHideTabGradient = GetControlGradientFromDockPanelGradient(Skin.AutoHideStripSkin.TabGradient),
                        DocumentActiveTabGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.DocumentGradient.ActiveTabGradient),
                        DocumentInactiveTabGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.DocumentGradient.InactiveTabGradient),
                        DocumentDockStripGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.DocumentGradient.DockStripGradient),
                        ToolWindowActiveTabGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.ToolWindowGradient.ActiveTabGradient),
                        ToolWindowInactiveTabGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.ToolWindowGradient.InactiveTabGradient),
                        ToolWindowActiveCaptionGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.ToolWindowGradient.ActiveCaptionGradient),
                        ToolWindowInactiveCaptionGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.ToolWindowGradient.InactiveCaptionGradient),
                        ToolWindowDockStripGradient = GetControlGradientFromDockPanelGradient(Skin.DockPaneStripSkin.ToolWindowGradient.DockStripGradient)
                    };
                }
                set
                {
                    if (Skin == null)
                        Skin = new DockPanelSkin();

                    Skin.AutoHideStripSkin = new AutoHideStripSkin()
                    {
                        DockStripGradient = GetTabGradientFromControlGradient(value.AutoHideDockStripGradient),
                        TabGradient = GetTabGradientFromControlGradient(value.AutoHideTabGradient)
                    };

                    Skin.DockPaneStripSkin = new DockPaneStripSkin()
                    {
                        DocumentGradient = new DockPaneStripGradient()
                        {
                            ActiveTabGradient = GetTabGradientFromControlGradient(value.DocumentActiveTabGradient),
                            InactiveTabGradient = GetTabGradientFromControlGradient(value.DocumentInactiveTabGradient),
                            DockStripGradient = GetTabGradientFromControlGradient(value.DocumentDockStripGradient)
                        },
                        ToolWindowGradient = new DockPaneStripToolWindowGradient()
                        {
                            ActiveTabGradient = GetTabGradientFromControlGradient(value.ToolWindowActiveTabGradient),
                            InactiveTabGradient = GetTabGradientFromControlGradient(value.ToolWindowInactiveTabGradient),
                            ActiveCaptionGradient = GetTabGradientFromControlGradient(value.ToolWindowActiveCaptionGradient),
                            InactiveCaptionGradient = GetTabGradientFromControlGradient(value.ToolWindowInactiveCaptionGradient),
                            DockStripGradient = GetTabGradientFromControlGradient(value.ToolWindowDockStripGradient)
                        }
                    };
                }
            }
        }

        private class DockContent : WeifenLuo.WinFormsUI.Docking.DockContent
        {
            public DockContent(ControlHostService controlHostService)
            {
                m_controlHostService = controlHostService;
            }
            private readonly ControlHostService m_controlHostService;

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
                Control focusedControl = WinFormsUtil.GetFocusedControl();
                if (focusedControl is AdaptableControl) // if AdaptableControl is in text editing mode
                {
                    var adaptableControl = focusedControl as AdaptableControl;
                    if (adaptableControl.HasKeyboardFocus)
                        return false;
                }
                   
                if (
                    (!(focusedControl is TextBoxBase) && !(focusedControl is ComboBox)) ||
                    !KeysUtil.IsTextBoxInput(focusedControl, keyData)
                    )
                {
                    ICommandService commandService = m_controlHostService.m_commandService;
                    if (commandService != null)
                    {
                        consumed = commandService.ProcessKey(keyData);
                    }

                    // If the command key wasn't processed, then let the base handle it so that certain
                    //  default behavior like the access keys (the underlines that appear when Alt is held down)
                    //  still work correctly.
                    if (!consumed)
                    {
                        consumed = base.ProcessCmdKey(ref msg, keyData);
                    }
                }

                return consumed;
            }

        }

        private class DefaultClient : IControlHostClient
        {
            public void Activate(Control control) { }
            public void Deactivate(Control control) { }
            public bool Close(Control control) { return true; }
        }

        const string DocumentContentPrefix = "Sce.Atf.DockPanel.DocumentContent,";


        private readonly Form m_mainForm;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

        private readonly Dictionary<ControlInfo, DockContent> m_dockContent = new Dictionary<ControlInfo, DockContent>();
        private readonly List<DockContent> m_unregisteredContents = new List<DockContent>() ;

        private readonly ActiveCollection<ControlInfo> m_controls;
 
        private readonly UniqueNamer m_uniqueNamer = new UniqueNamer('(');
        private readonly UniqueNamer m_idNamer = new UniqueNamer();

        private readonly DockPanel m_dockPanel;
        private DockContent m_activeDockContent;
        private ToolStripContainer m_toolStripContainer;
        private string m_dockPanelState;
        private bool m_formLoaded;
        private bool m_canFireDockStateChanged;

        private Image m_uiLockImage;
        private Image m_uiUnlockImage;

        private CommandRegister m_registeredCommands = CommandRegister.Default;

        private static readonly char[] s_pathDelimiters = new[] { '/', '\\' };
    }
}
