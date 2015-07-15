//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Component that provides the default commands related to document tab Controls,
    /// such as those that appear in the context menu when right-clicking on a tab</summary>
    /// <remarks>To customize, there are two choices:
    /// 1. To remove or re-order the default commands, create a new class that derives from this
    /// one that overrides whatever methods it needs to. Add it to the plugins list.
    /// 2. To simply add new commands, you can create a new CommandPlugin that overrides
    /// GetPopupCommandTags(). See the implementation below for how to determine if a tab control
    /// was right-clicked on.</remarks>
    [Export(typeof(DefaultTabCommands))]
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DefaultTabCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="controlHostService">Control hosting service</param>
        /// <param name="controlRegistry">Control registry</param>
        [ImportingConstructor]
        public DefaultTabCommands(
            ICommandService commandService,
            IControlHostService controlHostService,
            IControlRegistry controlRegistry)
        {
            CommandService = commandService;
            ControlHostService = controlHostService;
            ControlRegistry = controlRegistry;
        }

        /// <summary>
        /// These are the commands that this component registers with the command service.
        /// Override Initialize to register additional commands or to unregister any of these
        /// default commands.</summary>
        protected enum Command
        {
            CloseCurrentTab,
            CloseOtherTabs,
            CopyFullPath,
            OpenContainingFolder
        }

        /// <summary>
        /// Finishes initializing component by registering tab commands</summary>
        public virtual void Initialize()
        {
            CommandService.RegisterCommand(
                    Command.CloseCurrentTab,
                    null,
                    null,
                    "Close".Localize(),
                    "Closes the current Tab panel".Localize(),
                    Keys.None,
                    null,
                    CommandVisibility.ContextMenu,
                    this);

            CommandService.RegisterCommand(
                Command.CloseOtherTabs,
                null,
                null,
                "Close All But This".Localize(),
                "Closes all but the current Tab panel".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            CommandService.RegisterCommand(
                    Command.CopyFullPath,
                    null,
                    null,
                    "Copy Full Path".Localize(),
                    "Copies the file path for the document".Localize(),
                    Keys.None,
                    null,
                    CommandVisibility.ContextMenu,
                    this);

            CommandService.RegisterCommand(
                Command.OpenContainingFolder,
                null,
                null,
                "Open Containing Folder".Localize(),
                "Opens the folder containing the document in Windows Explorer".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            ControlRegistry.ActiveControlChanged += (sender, args) =>
            {
                m_documentExists = false;
                if (IsDocumentControl(ControlRegistry.ActiveControl))
                {
                    string documentPath = GetDocumentPath(ControlRegistry.ActiveControl);
                    try
                    {
                        // for example, circuit group windows are titled "MyFileName:MyGroupName"
                        if (PathUtil.IsValidPath(documentPath))
                            m_documentExists = File.Exists(documentPath);
                    }
                    catch (IOException)
                    {
                    }
                }
            };
        }

        /// <summary>
        /// Gets or sets the method used to test if a ControlInfo should have document-related
        /// commands. If this method returns true, then the ControlInfo's Description should be
        /// the file path of the document; override GetDocumentPath() to customize how the path
        /// is found. By default, document commands are available for tabs that are owned by an
        /// IDocumentClient. This field cannot be null.</summary>
        public Func<ControlInfo, bool> IsDocumentControl =
            controlInfo => controlInfo.Client.Is<IDocumentClient>();

        /// <summary>
        /// Gets the full path name of the document represented by the given ControlInfo.</summary>
        /// <param name="info">The ControlInfo that IsDocumentControl() says is a document</param>
        /// <returns>The full file path of the document represented by 'info', or null</returns>
        protected virtual string GetDocumentPath(ControlInfo info)
        {
            string path = info.Description;
            path = path.Trim(s_invalidFileNameChars);
            return path;
        }

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        /// <returns>Tags for context menu</returns>
        public virtual IEnumerable<object> GetCommands(object context, object target)
        {
            var info = target as ControlInfo;
            if (info != null)
            {
                // if the Name property of the control looks like a file path, return
                //  the tab commands
                if (IsDocumentControl(info))
                {
                    return new object[]
                    {
                        Command.CloseCurrentTab,
                        Command.CloseOtherTabs,
                        Command.CopyFullPath,
                        Command.OpenContainingFolder,
                    };
                }
            }

            return EmptyEnumerable<object>.Instance;
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            ControlInfo[] docInfos = GetDocumentControls();
            if (docInfos.Length > 0)
            {
                switch ((Command)commandTag)
                {
                    case Command.CloseCurrentTab:
                        return ControlRegistry.ActiveControl.Group != StandardControlGroup.CenterPermanent;

                    case Command.CloseOtherTabs:
                        return docInfos.Length > 1;

                    case Command.CopyFullPath:
                    case Command.OpenContainingFolder:
                        return m_documentExists; // Use a cached value to avoid file I/O spam.
                }
            }

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public virtual void DoCommand(object commandTag)
        {
            switch ((Command)commandTag)
            {
                case Command.CloseCurrentTab:
                    Close(ControlRegistry.ActiveControl);
                    break;

                case Command.CloseOtherTabs:
                    CloseOthers(ControlRegistry.ActiveControl);
                    break;

                case Command.CopyFullPath:
                    Clipboard.SetDataObject(GetDocumentPath(ControlRegistry.ActiveControl), true);
                    break;

                case Command.OpenContainingFolder:
                    // Open in Explorer and select the file. http://support.microsoft.com/kb/314853
                    System.Diagnostics.Process.Start("explorer.exe", "/e,/select," + GetDocumentPath(ControlRegistry.ActiveControl));
                    break;
            }

        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command state to update</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
        }

        #endregion

        private void Close(ControlInfo info)
        {
            if (info.Client.Close(info.Control))
            {
                ControlHostService.UnregisterControl(info.Control);
            }
        }

        private void CloseOthers(ControlInfo info)
        {
            ControlInfo[] docInfos = GetDocumentControls();
            foreach (ControlInfo docInfo in docInfos)
                if (docInfo != info)
                    Close(docInfo);
        }

        // Gets info for all controls holding a document
        private ControlInfo[] GetDocumentControls()
        {
            var infos = new List<ControlInfo>();
            foreach (ControlInfo info in ControlHostService.Controls)
                if (IsDocumentControl(info))
                    infos.Add(info);

            return infos.ToArray();
        }

        /// <summary>
        /// Gets the ICommandService used by this component</summary>
        protected ICommandService CommandService { get; private set; }

        /// <summary>
        /// Gets the IControlHostService used by this component</summary>
        protected IControlHostService ControlHostService { get; private set; }

        /// <summary>
        /// Gets the IControlRegistry used by this component</summary>
        protected IControlRegistry ControlRegistry { get; private set; }

        private bool m_documentExists; // Does the active control represent a document that exists?
        private static readonly char[] s_invalidFileNameChars = Path.GetInvalidFileNameChars();
    }
}
