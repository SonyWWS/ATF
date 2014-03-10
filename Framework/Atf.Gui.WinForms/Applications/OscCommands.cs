//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// A component that provides menu commands that may be useful for users of OscService.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IContextMenuCommandProvider))]
    [Export(typeof(OscCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OscCommands : ICommandClient, IContextMenuCommandProvider, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="oscService">OscService</param>
        [ImportingConstructor]
        public OscCommands(
            ICommandService commandService,
            OscService oscService)
        {
            CommandService = commandService;
            OscService = oscService;
        }

        /// <summary>
        /// Gets the command service</summary>
        protected ICommandService CommandService
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the Open Sound Control service</summary>
        protected OscService OscService
        {
            get;
            private set;
        }

        #region IInitializable Members

        public virtual void Initialize()
        {
            CommandService.RegisterCommand(
                Command.OscInfo,
                StandardMenu.File,
                CommandGroups.Osc,
                "OSC Info",
                "Displays the status of the OSC service and lists the available OSC addresses and associated properties",
                Keys.None,
                null,
                this);

            CommandService.RegisterCommand(
                Command.CopyOscAddressOfPropertyDescriptor,
                null,
                null,
                s_copyOscAddressText,
                "Copies the OSC address of this property to the clipboard".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu, // context menu only
                this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True if client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            var command = (Command)commandTag;
            switch (command)
            {
                case Command.OscInfo:
                    return true;
                case Command.CopyOscAddressOfPropertyDescriptor:
                    return m_oscAddressOfPropertyDescriptor != null;
            }
            return false;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public virtual void DoCommand(object commandTag)
        {
            var command = (Command)commandTag;
            switch (command)
            {
                case Command.OscInfo:
                    var dialog = new OscDialog(OscService, m_commandReceiver);
                    dialog.ShowDialog(m_mainWindow);
                    break;
                case Command.CopyOscAddressOfPropertyDescriptor:
                    Clipboard.SetText(m_oscAddressOfPropertyDescriptor);
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public virtual void UpdateCommand(object commandTag, CommandState state)
        {
            // Is it useful to see the OSC address in the context menu? These addresses can be long
            //  and make the context menu very wide.
            //if ((Command)commandTag == Command.CopyOscAddressOfPropertyDescriptor &&
            //    m_oscAddressOfPropertyDescriptor != null)
            //{
            //    state.Text = s_copyOscAddressText + m_oscAddressOfPropertyDescriptor;
            //}
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        public IEnumerable<object> GetCommands(object context, object target)
        {
            m_oscAddressOfPropertyDescriptor = null;
            var descriptor = target as PropertyDescriptor;
            if (descriptor != null)
            {
                var selectionContext = context.As<ISelectionContext>();
                if (selectionContext != null)
                {
                    object selected = selectionContext.LastSelected;
                    if (selected != null)
                    {
                        string targetKey = descriptor.GetPropertyDescriptorKey();
                        foreach (OscService.OscAddressInfo info in OscService.GetInfos(selected))
                        {
                            if (info.PropertyDescriptor != null &&
                                (info.PropertyDescriptor.GetPropertyDescriptorKey() == targetKey))
                            {
                                m_oscAddressOfPropertyDescriptor = info.Address;
                                yield return Command.CopyOscAddressOfPropertyDescriptor;
                                yield break;
                            }
                        }
                    }
                }
            }
        }

        #endregion

        private enum Command
        {
            // Displays a dialog box that allows for user-configuration of the OscService
            //  and a list of all the known OSC addresses and some status information.
            OscInfo,

            // A context-menu command to offer to copy an OSC address into the clipboard, when the
            //  right-click target object is a property descriptor.
            CopyOscAddressOfPropertyDescriptor
        }

        private enum CommandGroups
        {
            Osc
        }

        private string m_oscAddressOfPropertyDescriptor;

        [Import(AllowDefault = true)]
        private IWin32Window m_mainWindow;

        [Import(AllowDefault = true)]
        private OscCommandReceiver m_commandReceiver;

        private static readonly string s_copyOscAddressText = "Copy OSC Address".Localize();
    }
}
