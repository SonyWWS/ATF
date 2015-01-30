//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Sce.Atf.Adaptation;
using Keys = Sce.Atf.Input.Keys;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that implements source control commands</summary>
    public class SourceControlCommandsBase : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="documentRegistry">Document tracking service</param>
        /// <param name="documentService">File menu service</param>
        protected SourceControlCommandsBase(
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
            None = 0x0000,
            Add = 0x0001,
            CheckIn = 0x0002,
            CheckOut = 0x0004,
            Sync = 0x0008,
            Revert = 0x0010,
            Refresh = 0x0020,
            Reconcile = 0x0040,
            Connection = 0x0080,
            Enabled = 0x0100,
            Default = Add | CheckIn | CheckOut | Sync | Revert | Refresh | Reconcile | Connection | Enabled
        }

        /// <summary>
        /// Gets or sets a value indicating if a standard source control command will be registered. Must
        /// be set before IInitialize is called.</summary>
        public CommandRegister RegisterCommands
        {
            get { return m_registerCommands; }
            set { m_registerCommands = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating if a standard command visibility. Must
        /// be set before IInitialize is called. </summary>
        public CommandVisibility CommandVisibility
        {
            get { return m_commandVisibility; }
            set { m_commandVisibility = value; }
        }

        /// <summary>
        /// Gets or sets the source control service</summary>
        [Import(AllowDefault = true, AllowRecomposition = true)]
        public SourceControlService SourceControlService
        {
            get { return m_sourceControlService; }
            set { m_sourceControlService = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to auto check out a document when edited</summary>
        [Obsolete("Use CheckoutOnEditBehavior instead", false)]
        public bool CheckoutOnEdit
        {
            get { return m_checkoutOnEditBehavior == CheckoutOnEditBehavior.Always; }
            set { m_checkoutOnEditBehavior = value ? CheckoutOnEditBehavior.Always : CheckoutOnEditBehavior.Prompt; }
        }

        /// <summary>
        /// Gets or sets the checkout behavior to be used when a document is first modified (made dirty)</summary>
        public CheckoutOnEditBehavior CheckoutOnEditBehavior
        {
            get { return m_checkoutOnEditBehavior; }
            set { m_checkoutOnEditBehavior = value; }
        }

        #region IInitializable Members

        /// <summary>
        /// Initialize MEF component by registering commands with Command service</summary>
        public virtual void Initialize()
        {
            if ((RegisterCommands & CommandRegister.Enabled) == CommandRegister.Enabled)
            {
                m_commandService.RegisterCommand(
                    Command.Enabled,
                    StandardMenu.File,
                    SourceControlCommandGroup.OnOff,
                    "Source Control/Enable".Localize(),
                    "Enable source control".Localize(),
                    Keys.None,
                    Resources.SourceControlEnableImage,
                    m_commandVisibility,
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
                    m_commandVisibility,
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
                    Resources.DocumentAddImage,
                    m_commandVisibility,
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
                    Resources.DocumentLockImage,
                    m_commandVisibility,
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
                    Resources.DocumentCheckOutImage,
                    m_commandVisibility,
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
                    Resources.DocumentGetLatestImage,
                    m_commandVisibility,
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
                    Resources.DocumentRevertImage,
                    m_commandVisibility,
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
                    Resources.DocumentRefreshImage,
                    m_commandVisibility,
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
        /// Check whether the client can do the command, if it handles it</summary>
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

            if (!SourceControlService.Enabled)
                return false;

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
        /// Do the command</summary>
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
        /// Update command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState commandState)
        {
            if ((commandTag is Command))
            {
                if ((Command)commandTag == Command.Enabled && SourceControlService != null)
                {
                    commandState.Text = SourceControlService.Enabled ? "Disable Source Control".Localize() : "Enable Source Control".Localize();
                }
            }
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        /// <returns>Enumeration of command tags</returns>
        public virtual IEnumerable<object> GetCommands(object context, object target)
        {
            IResource resource = target.As<IResource>();
            return resource != null ? m_commands : EmptyEnumerable<object>.Instance;
        }

        #endregion

        /// <summary>
        /// Get source control context from context registry</summary>
        protected virtual ISourceControlContext SourceControlContext
        {
            get
            {
                return ContextRegistry != null
                    ? ContextRegistry.GetMostRecentContext<ISourceControlContext>()
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
                var result = MessageBoxes.Show(message, "Add Document to Version Control".Localize("this is the title of a dialog box that is asking a question"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    SourceControlService.Add(e.Document.Uri);
                }
            }
        }

        /// <summary>
        /// Handle document dirty changed event</summary>
        /// <param name="sender">Document registry</param>
        /// <param name="e">Event arguments</param>
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

                string message = string.Format("Check out the file\r\n\r\n{0}\r\n\r\nto be able to save the changes?".Localize(),
                    document.Uri.LocalPath);

                var result = MessageBoxes.Show(message, "Check Out File".Localize("this is the title of a dialog box that is asking a question"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                    TestCheckedIn(sender);
            }
        }

        #endregion

        /// <summary>
        /// Perform the Refresh command</summary>
        /// <param name="doing">True to perform the Refresh; false to test whether Refresh can be done</param>
        /// <returns>True iff Refresh can be done or was done</returns>
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
        /// Perform the Reconcile command</summary>
        /// <param name="doing">True to perform the Reconcile; false to test whether Reconcile can be done</param>
        /// <returns>True iff Reconcile can be done or was done</returns>
        protected virtual bool DoReconcile(bool doing)
        {
            return false;
        }

        /// <summary>
        /// Add path to list of source controlled URIs</summary>
        /// <param name="path">Path to add</param>
        /// <param name="uris">List of source controlled URIs</param>
        protected static void GetSourceControlledUri(object path, List<Uri> uris)
        {
            IResource resource = path.As<IResource>();
            if (resource != null)
                uris.Add(resource.Uri);
        }

        /// <summary>
        /// Perform the Add command</summary>
        /// <param name="doing">True to perform the Add; false to test whether Add can be done</param>
        /// <returns>True iff Add can be done or was done</returns>
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
        /// Perform the CheckOut command</summary>
        /// <param name="doing">True to perform the CheckOut; false to test whether CheckOut can be done</param>
        /// <returns>True iff CheckOut can be done or was done</returns>
        protected virtual bool DoCheckOut(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            int checkedOutCount = 0;
            foreach (IResource resource in SourceControlContext.Resources)
            {
                if (GetStatus(resource) != SourceControlStatus.CheckedIn)
                    return false;
                checkedOutCount++;
                if (doing)
                    SourceControlService.CheckOut(resource.Uri);
            }
            return checkedOutCount != 0;
        }

        /// <summary>
        /// Perform the CheckIn command</summary>
        /// <param name="doing">True to perform the CheckIn; false to test whether CheckIn can be done</param>
        /// <returns>True iff CheckIn can be done or was done</returns>
        protected virtual bool DoCheckIn(bool doing)
        {
            if (SourceControlService == null || ContextRegistry == null ||
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

                ShowCheckInDialog(toCheckIns);
            }

            return result;
        }

        /// <summary>
        /// Display Checkin dialog</summary>
        /// <param name="toCheckIns">List of resources to check in</param>
        protected virtual void ShowCheckInDialog(IList<IResource> toCheckIns)
        {
        }

        /// <summary>
        /// Perform the Sync command</summary>
        /// <param name="doing">True to perform the Sync; false to test whether Sync can be done</param>
        /// <returns>True iff Sync can be done or was done</returns>
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
        /// Perform the Revert command</summary>
        /// <param name="doing">True to perform the Revert; false to test whether Revert can be done</param>
        /// <returns>True iff Revert can be done or was done</returns>
        protected virtual bool DoRevert(bool doing)
        {
            if (SourceControlService == null || SourceControlContext == null)
                return false;

            foreach (IResource resource in SourceControlContext.Resources)
            {
                SourceControlStatus status = GetStatus(resource);
                if (status != SourceControlStatus.CheckedOut && status != SourceControlStatus.Added && status != SourceControlStatus.Deleted)
                    return false;
            }

            if (doing)
            {
                // user must confirm revert
                var dialogResult = MessageBoxes.Show("All Changes will be lost. Do you want to proceed?".Localize(),
                        "Proceed with Revert?".Localize(),
                        MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (dialogResult == MessageBoxResult.Yes)
                {
                    foreach (IResource resource in SourceControlContext.Resources.ToArray())
                    {
                        SourceControlStatus status = GetStatus(resource);
                        if (status == SourceControlStatus.CheckedOut || status == SourceControlStatus.Added || status == SourceControlStatus.Deleted)
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
        /// Perform the Connect command</summary>
        /// <param name="doing">True to perform the Connect; false to test whether Connect can be done</param>
        /// <returns>True iff Connect can be done or was done</returns>
        protected virtual bool DoConnection(bool doing)
        {
            if (SourceControlService == null)
                return false;

            return doing
                ? SourceControlService.Connect()
                : SourceControlService.CanConfigure;
        }

        /// <summary>
        /// Get the source control status of an item</summary>
        /// <param name="resource">URI representing the path to item</param>
        /// <returns>Status of item</returns>
        protected virtual SourceControlStatus GetStatus(IResource resource)
        {
            Uri uri = resource.Uri;
            if (uri != null)
                return SourceControlService.GetStatus(new Uri(uri.LocalPath));

            return SourceControlStatus.FileDoesNotExist;
        }

        /// <summary>
        /// Reload document associated with resource</summary>
        /// <param name="resource">Resource to reload</param>
        /// <remarks>Reload a document when it is refreshed, for example</remarks>
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
        /// <remarks>These can be used to adjust settings like shortcuts or where the commands appear
        /// in the user interface (menus, toolbars, context menus), by retrieving a CommandInfo.</remarks>
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
        /// Get current Command</summary>
        protected Command CurrentCommand
        {
            get { return m_currentCommnd; }
        }

        /// <summary>
        /// Test if the resource is checked-in and needs checking out
        /// </summary>
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
        private CommandVisibility m_commandVisibility = CommandVisibility.Menu | CommandVisibility.Toolbar;

        /// <summary>
        /// Get IContextRegistry</summary>
        [Import(AllowDefault = true)]
        protected IContextRegistry ContextRegistry
        {
            get;
            private set;
        }

        [ImportMany]
        private Lazy<IDocumentClient>[] m_documentClients = null;

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
        /// Automatically check out a source-controlled document, without asking the user</summary>
        Always,
        /// <summary>
        /// Prompt the user if he/she wants to check out a document under source-control</summary>
        Prompt,
        /// <summary>
        /// Never check out when a document is first modified</summary>
        Never
    }
}
