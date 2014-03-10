//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that implements source control commands</summary>
    [InheritedExport(typeof(IContextMenuCommandProvider))]
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(SourceControlCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SourceControlCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="documentRegistry">Document tracking service</param>
        /// <param name="documentService">File menu service</param>
        [ImportingConstructor]
        public SourceControlCommands(
            ICommandService commandService,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService)
        {
            m_commandService = commandService;
            m_documentService = documentService;
            m_documentRegistry = documentRegistry;

            documentRegistry.DocumentAdded += documentRegistry_DocumentAdded;
            documentService.DocumentSaved += documentService_DocumentSaved;
        }

        /// <summary>
        /// Flags that determine which commands should appear in the "File/Source Conntrol"  menu</summary>
        [Flags]
        public enum CommandRegister
        {
            None =       0x0000,
            Add =        0x0001,
            CheckIn =    0x0002,
            CheckOut =   0x0004,
            Sync =       0x0008,
            Revert =     0x0010,
            Refresh =    0x0020,
            Reconcile =  0x0040,
            Connection = 0x0080,
            Enabled =    0x0100,
            Default = Add | CheckIn | CheckOut | Sync | Revert | Refresh | Reconcile | Connection | Enabled
        }

        /// <summary>
        /// Gets or sets whether a standard source control command is registered. Must
        /// be set before IInitialize is called.</summary>
        public CommandRegister RegisterCommands
        {
            get { return m_registerCommands; }
            set { m_registerCommands = value; }
        }

        /// <summary>
        /// Gets or sets the source control service</summary>
        [Import(AllowDefault = true, AllowRecomposition = true)]
        public SourceControlService SourceControlService
        {
            get { return m_sourceControlService; }
            set { m_sourceControlService = value;}
        }

        /// <summary>
        /// Gets or sets the checkout behavior to be used when a document is first modified (made dirty)</summary>
        public CheckoutOnEditBehavior CheckoutOnEditBehavior
        {
            get { return m_checkoutOnEditBehavior; }
            set { m_checkoutOnEditBehavior = value; }
        }

        #region IInitializable Members

        public virtual void Initialize()
        {
            m_sourceControlEnableImage = ResourceUtil.GetImage24(Resources.SourceControlEnableImage);
            m_sourceControlDisableImage = ResourceUtil.GetImage24(Resources.SourceControlDisableImage);

            if ((RegisterCommands & CommandRegister.Enabled) == CommandRegister.Enabled)
            {
                m_sourceControlEnableCmd = m_commandService.RegisterCommand(
                    Command.Enabled,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Enable".Localize(),
                    "Enable source control".Localize(),
                    Keys.None,
                    Resources.SourceControlEnableImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.Connection) == CommandRegister.Connection)
            {
                m_commandService.RegisterCommand(
                    Command.Connection,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Open Connection...".Localize(),
                    "Source control connection".Localize(),
                    Keys.None,
                    Resources.SourceControlConnectionImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.Add) == CommandRegister.Add)
            {
                m_commandService.RegisterCommand(
                    Command.Add,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Add".Localize(),
                    "Add to source control".Localize(),
                    Keys.None,
                    Resources.SourceControlAddImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.CheckIn) == CommandRegister.CheckIn)
            {
                m_commandService.RegisterCommand(
                    Command.CheckIn,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Check In".Localize(),
                    "Check in to source control".Localize(),
                    Keys.None,
                    Resources.SourceControlCheckInImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.CheckOut) == CommandRegister.CheckOut)
            {
                m_commandService.RegisterCommand(
                    Command.CheckOut,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Check Out".Localize(),
                    "Check out from source control".Localize(),
                    Keys.None,
                    Resources.SourceControlCheckOutImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.Sync) == CommandRegister.Sync)
            {
                m_commandService.RegisterCommand(
                    Command.Sync,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Get Latest Version".Localize(),
                    "Get latest version from source control".Localize(),
                    Keys.None,
                    Resources.SourceControlGetLatestImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.Revert) == CommandRegister.Revert)
            {
                m_commandService.RegisterCommand(
                    Command.Revert,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Revert".Localize("Revert add or check out from source control"),
                    "Revert add or check out from source control".Localize(),
                    Keys.None,
                    Resources.SourceControlRevertImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.Refresh) == CommandRegister.Refresh)
            {
                m_commandService.RegisterCommand(
                    Command.Refresh,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Refresh Status".Localize(),
                    "Refresh status in source control".Localize(),
                    Keys.None,
                    Resources.SourceControlRefreshImage,
                    CommandVisibility.Menu | CommandVisibility.Toolbar,
                    this);
            }

            if ((RegisterCommands & CommandRegister.Reconcile) == CommandRegister.Reconcile)
            {
                m_commandService.RegisterCommand(
                    Command.Reconcile,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Reconcile Offline Work...".Localize(),
                    "Reconcile Offline Work".Localize(),
                    Keys.None,
                    Resources.SourceControlReconcileImage,
                    CommandVisibility.ApplicationMenu,
                    this);
            }

         }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return false;

            if (SourceControlService == null)
                return false;

            if ((Command)commandTag == Command.Enabled)
                return true;

            // Please do not add an "early exit" condition for (!SourceControlService.Enabled)
            // Doing so prevents file icons from being updated, when source control is disabled. -PaulSki

            const bool doing = false;


            if ((Command)commandTag == Command.Connection)
                return WrapCommandFunction((Command)commandTag, DoConnection, doing);

            if (SourceControlContext == null)
                return false;

            if ((Command)commandTag == Command.Add)
                return WrapCommandFunction((Command)commandTag, DoAdd, doing);

            bool canDo = false;
            foreach (IResource anyResource in SourceControlContext.Resources) // only check when SourceControlContext has resources to take care
            {
                switch ((Command)commandTag)
                {
                    case Command.Refresh:
                        canDo = WrapCommandFunction((Command)commandTag, DoRefresh, doing);
                        break;

                    case Command.Reconcile:
                        canDo = WrapCommandFunction((Command)commandTag, DoReconcile, doing);
                        break;

                    case Command.CheckOut:
                        canDo = WrapCommandFunction((Command)commandTag, DoCheckOut, doing);
                        break;

                    case Command.CheckIn:
                        canDo = WrapCommandFunction((Command)commandTag, DoCheckIn, doing);
                        break;

                    case Command.Sync:
                        canDo = WrapCommandFunction((Command)commandTag, DoSync, doing);
                        break;

                    case Command.Revert:
                        canDo = WrapCommandFunction((Command)commandTag, DoRevert, doing);
                        break;
                }

                break;
            }

            return canDo;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag)
        {
            if (!(commandTag is Command))
                return;

            if (SourceControlService == null)
                return;

            if ((Command)commandTag == Command.Enabled)
            {
                SourceControlService.Enabled = !SourceControlService.Enabled;
                return;
            }

            const bool doing = true;

            if ((Command)commandTag == Command.Connection)
            {
                WrapCommandFunction((Command)commandTag, DoConnection, doing);
                return;
            }

            if (SourceControlContext == null)
                return;

            switch ((Command)commandTag)
            {
                case Command.Refresh:
                    WrapCommandFunction((Command)commandTag, DoRefresh, doing);
                    break;

                case Command.Reconcile:
                    WrapCommandFunction((Command)commandTag, DoReconcile, doing);
                    break;

                case Command.Add:
                    WrapCommandFunction((Command)commandTag, DoAdd, doing);
                    break;

                case Command.CheckOut:
                    WrapCommandFunction((Command)commandTag, DoCheckOut, doing);
                    break;

                case Command.CheckIn:
                    WrapCommandFunction((Command)commandTag, DoCheckIn, doing);
                    break;

                case Command.Sync:
                    WrapCommandFunction((Command)commandTag, DoSync, doing);
                    break;

                case Command.Revert:
                    WrapCommandFunction((Command)commandTag, DoRevert, doing);
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState commandState)
        {
            if ((commandTag is Command))
            {
                if ((Command)commandTag == Command.Enabled && SourceControlService != null)
                {
                    commandState.Text = SourceControlService.Enabled ? "Disable Source Control".Localize() : "Enable Source Control".Localize();
                    m_sourceControlEnableCmd.GetButton().ToolTipText = commandState.Text;
                    m_sourceControlEnableCmd.GetButton().Image = SourceControlService.Enabled ? m_sourceControlEnableImage: m_sourceControlDisableImage;
                }
            }
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        public virtual IEnumerable<object> GetCommands(object context, object target)
        {
            IResource resource = target.As<IResource>();
            return resource != null ? m_commands : EmptyEnumerable<object>.Instance;
        }

        #endregion

        /// <summary>
        /// Gets source control context</summary>
        protected virtual ISourceControlContext SourceControlContext
        {
            get
            {
                return m_contextRegistry != null
                    ? m_contextRegistry.GetMostRecentContext<ISourceControlContext>()
                    : null;
            }
        }

        #region Event Handlers

        private void documentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
        {
            IDocument document = m_documentRegistry.ActiveDocument;
            if (document == null || m_documentService.IsUntitled(document))
                return;

            document.DirtyChanged += OnDocumentDirtyChanged;
        }

        private void documentService_DocumentSaved(object sender, DocumentEventArgs e)
        {
            if (e.Kind != DocumentEventType.SavedAs || SourceControlService == null)
                return;

            if (SourceControlService.GetStatus(e.Document.Uri) == SourceControlStatus.NotControlled)
            {
                string message = string.Format("Add document {0} to version control?".Localize(), e.Document.Uri.AbsolutePath);
                DialogResult result = MessageBox.Show(GetDialogOwner(), message, "Add document to Version Control".Localize(), MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    SourceControlService.Add(e.Document.Uri);
                }
            }

        }

        /// <summary>
        /// Performs custom actions when a document's Dirty property changes</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected virtual void OnDocumentDirtyChanged(object sender, EventArgs e)
        {
            IDocument document = sender as IDocument;
            if (SourceControlService == null || document == null || !document.Dirty || CheckoutOnEditBehavior == CheckoutOnEditBehavior.Never)
                return;

            if (SourceControlService.GetStatus(document.Uri) == SourceControlStatus.CheckedIn)
            {
                if (CheckoutOnEditBehavior == CheckoutOnEditBehavior.Always)
                {
                    TestCheckedIn(sender);
                    return;
                }

                var message = new StringBuilder();
                message.AppendLine("Check out this file to be able to save the changes?".Localize());
                message.AppendLine(document.Uri.LocalPath);

                DialogResult result = MessageBox.Show(
                    GetDialogOwner(),
                    message.ToString(),
                     "Check Out File".Localize(), MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                    TestCheckedIn(sender);
            }
        }

        #endregion

        /// <summary>
        /// Performs refresh of documents' status</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff any documents refreshed or can be refreshed</returns>
        protected virtual bool DoRefresh(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            if (!doing)
            {
                foreach (IResource resource in SourceControlContext.Resources)
                    return true;

                return false;
            }

            List<Uri> uris = new List<Uri>();
            foreach (IResource resource in SourceControlContext.Resources)
                GetSourceControlledUri(resource, uris);
            SourceControlService.RefreshStatus(uris);

            return uris.Count > 0; // something to refresh?
        }

        /// <summary>
        /// Performs reconciliation of documents</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff any documents reconciled or can be reconciled</returns>
        protected virtual bool DoReconcile(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            if (!doing)
            {
                foreach (IResource resource in SourceControlContext.Resources)
                    return true;

                return false;
            }

            List<Uri> uris = new List<Uri>();
            foreach (IResource resource in SourceControlContext.Resources)
                uris.Add(resource.Uri);

            List<Uri> modifiled = new List<Uri>();
            List<Uri> localNotInDepot = new List<Uri>();

            using (new WaitCursor())
            {
                foreach (Uri uri in SourceControlService.GetModifiedFiles(uris))
                {
                    if (SourceControlService.GetStatus(uri) != SourceControlStatus.CheckedOut)
                        modifiled.Add(uri);
                }

                foreach (Uri uri in uris)
                {
                    if (!modifiled.Contains(uri))
                        if (SourceControlService.GetStatus(uri) == SourceControlStatus.FileDoesNotExist)
                            localNotInDepot.Add(uri);
                }
            }

            ReconcileForm form = new ReconcileForm(SourceControlService, modifiled, localNotInDepot);
            if (m_mainForm != null)
                form.Icon = m_mainForm.Icon;
            form.ShowDialog();

            return true;
        }

        /// <summary>
        /// Obtains list of URIs of documents under source control for a given path</summary>
        /// <param name="path">Source control path</param>
        /// <param name="uris">List of URIs of documents under source control</param>
        protected static void GetSourceControlledUri(object path, List<Uri> uris)
        {
            IResource resource = path.As<IResource>();
            if (resource != null)
                uris.Add(resource.Uri);
        }

        /// <summary>
        /// Performs adding documents to source control</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff any documents added or can be added</returns>
        protected virtual bool DoAdd(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            int addedCount = 0;
            foreach (IResource resource in SourceControlContext.Resources)
            {
                SourceControlStatus status = GetStatus(resource);
                if (status != SourceControlStatus.NotControlled &&
                    status != SourceControlStatus.FileDoesNotExist)
                {
                    return false;
                }
                addedCount++;
                if (doing)
                    SourceControlService.Add(resource.Uri);
            }

            return addedCount != 0;
        }

        /// <summary>
        /// Performs checking out documents from source control</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff any documents checked out or can be checked out</returns>
        protected virtual bool DoCheckOut(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            int checkedOutCount = 0;
            foreach (IResource resource in SourceControlContext.Resources)
            {
                if (GetStatus(resource) != SourceControlStatus.CheckedIn)
                    return false;

                if (SourceControlService.IsLocked(resource.Uri))
                    return false;
                checkedOutCount++;
                if (doing)
                    SourceControlService.CheckOut(resource.Uri);
            }
            return checkedOutCount != 0;
        }

        /// <summary>
        /// Performs checking in documents to source control</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff any documents checked in or can be checked in</returns>
        protected virtual bool DoCheckIn(bool doing)
        {
            if (SourceControlService == null || m_contextRegistry == null ||
                SourceControlContext == null || !SourceControlService.AllowCheckIn)
                return false;

            bool result = false;
            List<IResource> toCheckIns = new List<IResource>();

            // return true if resources are added or checked out
            foreach (IResource resource in SourceControlContext.Resources)
            {
                SourceControlStatus status = GetStatus(resource);

                if (status == SourceControlStatus.CheckedOut ||
                    status == SourceControlStatus.Added ||
                    status == SourceControlStatus.Deleted)
                {
                    result = true;
                    if (doing)
                        toCheckIns.Add(resource);
                    else
                        break;
                }
            }

            if (doing)
            {
                if (m_documentService != null)
                {
                    foreach (IResource resource in SourceControlContext.Resources)
                    {
                        // if collection has been modified save it before we check in
                        IDocument document = resource.As<IDocument>();
                        if (document != null)
                        {
                            if (document.Dirty)
                                m_documentService.Save(document);
                        }
                    }
                }

                CheckInForm form = new CheckInForm(SourceControlService, toCheckIns);
                if (m_mainForm != null)
                    form.Icon = m_mainForm.Icon;
                form.ShowDialog(GetDialogOwner());
            }

            return result;
        }

        /// <summary>
        /// Performs synchronizing local files with files under source control</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff any documents synchronized or can be synchronized</returns>
        protected virtual bool DoSync(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            int count = 0;

            foreach (IResource resource in SourceControlContext.Resources)
            {
                ++count;
                if (GetStatus(resource) != SourceControlStatus.CheckedIn)
                    return false;

                if (SourceControlService.IsSynched(resource.Uri))
                    return false;

                if (doing)
                {
                    SourceControlService.GetLatestVersion(resource.Uri);
                    Reload(resource);
                }
            }
            return count != 0;
        }

        /// <summary>
        /// Performs reverting local files to files under source control</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff any documents reverted or can be reverted</returns>
        protected virtual bool DoRevert(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            foreach (IResource resource in SourceControlContext.Resources)
            {
                SourceControlStatus status = GetStatus(resource);
                if (status != SourceControlStatus.CheckedOut && status != SourceControlStatus.Added)
                    return false;
            }

            if (doing)
            {
                // user must confirm revert
                DialogResult dialogResult = MessageBox.Show(m_mainForm,
                        "All changes will be lost. Do you want to proceed?".Localize(),
                        "Proceed with Revert?".Localize(),
                        MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    foreach (IResource resource in SourceControlContext.Resources)
                    {
                        SourceControlStatus status = GetStatus(resource);
                        if (status == SourceControlStatus.CheckedOut || status == SourceControlStatus.Added)
                        {
                            SourceControlService.Revert(resource.Uri);
                            Reload(resource);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Performs connecting to source control</summary>
        /// <param name="doing">True iff doing the command</param>
        /// <returns>True iff successfully connected to source control or can be connected</returns>
        protected virtual bool DoConnection(bool doing)
        {
            if (SourceControlService == null)
                return false;

            return doing
                ? SourceControlService.Connect()
                : SourceControlService.CanConfigure;
        }

        /// <summary>
        /// Obtains source control status of resource</summary>
        /// <param name="resource">Resource</param>
        /// <returns>SourceControlStatus</returns>
        protected virtual SourceControlStatus GetStatus(IResource resource)
        {
            if (resource != null)
            {
                Uri uri = resource.Uri;
                if (uri != null)
                    return SourceControlService.GetStatus(new Uri(uri.LocalPath));
            }

            return SourceControlStatus.FileDoesNotExist;
        }

        /// <summary>
        /// Opens document in source control</summary>
        /// <param name="resource">Document resource, typically URI</param>
        protected virtual void Reload(IResource resource)
        {
            IDocument doc = resource as IDocument;
            if (doc == null)
                return;

            foreach (IDocumentClient client in m_documentClients.GetValues())
            {
                if (client.CanOpen(doc.Uri))
                {
                    if (doc.Dirty)
                        doc.Dirty = false;
                    m_documentService.Close(doc);
                    m_documentService.OpenExistingDocument(client, doc.Uri);
                    break;
                }
            }
        }

        /// <summary>
        /// The command tags that are used with ICommandService</summary>
        /// <remarks>These can be used to adjust settings, such as shortcuts, or where the commands appear
        /// in the user interface (menus, toolbars, context menus) by retrieving a CommandInfo.</remarks>
        public enum Command
        {
            Invalid,
            Enabled,
            Add,
            CheckOut,
            CheckIn,
            Sync,
            Revert,
            Refresh,
            Reconcile,
            Connection
        }

        /// <summary>
        /// Gets current cource control command</summary>
        protected Command CurrentCommand
        {
            get { return m_currentCommnd; }
        }

        /// <summary>
        /// Tests if the resource is checked-in and needs checking out</summary>
        private void TestCheckedIn(object obj)
        {
            if (SourceControlService == null)
                return;
            IResource resource = obj.As<IResource>();
            if (resource == null)
                return;

            if (SourceControlService.GetStatus(resource.Uri) == SourceControlStatus.CheckedIn)
                SourceControlService.CheckOut(resource.Uri);
        }

        private IWin32Window GetDialogOwner()
        {
            return m_mainWindow != null ? m_mainWindow.DialogOwner : m_mainForm;
        }

        private enum SourceControlCommandGroup
        {
            OnOff
        }

        private delegate bool CommandFunction(bool doing);
        private bool WrapCommandFunction(Command command, CommandFunction function, bool doing)
        {
            try
            {
                m_currentCommnd = command;
                return function(doing);
            }
            finally
            {
                m_currentCommnd = Command.Invalid;
            }
        }

        private readonly ICommandService m_commandService;
        private readonly IDocumentService m_documentService;
        private readonly IDocumentRegistry m_documentRegistry;
        private SourceControlService m_sourceControlService;
        private CheckoutOnEditBehavior m_checkoutOnEditBehavior = CheckoutOnEditBehavior.Always;
        private Command m_currentCommnd = Command.Invalid;
        private CommandRegister m_registerCommands = CommandRegister.Default;
        private CommandInfo m_sourceControlEnableCmd;
        private Image m_sourceControlEnableImage;
        private Image m_sourceControlDisableImage;


        [Import(AllowDefault = true)]
        private IContextRegistry m_contextRegistry;

        [ImportMany]
        private Lazy<IDocumentClient>[] m_documentClients;

        [Import(AllowDefault = true)]
        private IMainWindow m_mainWindow;

        [Import(AllowDefault = true)]
        private Form m_mainForm;

        private readonly object[] m_commands = new object[] {
            Command.Add,
            Command.CheckOut,
            Command.CheckIn,
            Command.Sync,
            Command.Revert,
            Command.Refresh,
            Command.Reconcile,
            Command.Connection
        };
    }

    /// <summary>
    /// Checkout behavior to use when a document is first modified (made dirty)</summary>
    public enum CheckoutOnEditBehavior
    {
        /// <summary>
        /// Automatically check out a source controlled document, without asking the user</summary>
        Always,
        /// <summary>
        /// Prompt the user if he/she wants to check out a document under source control</summary>
        Prompt,
        /// <summary>
        /// Never check out when a document is first modified</summary>
        Never
    }
}
