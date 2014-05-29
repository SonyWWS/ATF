//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Class to support editing item labels</summary>
    [Export]
    [Export(typeof(IInitializable))]
    [Export(typeof(ICommandClient))]
    [Export(typeof(IContextMenuCommandProvider))]
    public class EditLabelCommand : 
        IInitializable,
        ICommandClient, 
        IContextMenuCommandProvider
    {
        private enum Commands
        {
            EditLabel
        }

        [Import(typeof(ICommandService))]
        private ICommandService m_commandService = null;

        [Import(typeof(Sce.Atf.Applications.IContextRegistry))]
        private Sce.Atf.Applications.IContextRegistry m_contextRegistry = null;

        private static CommandDef s_renameCommandDef = new CommandDef(
            Commands.EditLabel,
            Sce.Atf.Applications.StandardMenu.Edit,
            null, 
            "Rename".Localize(), 
            null, 
            "Rename".Localize(), 
            null, 
            new InputGesture[]{new KeyGesture(Key.F2)}, 
            Sce.Atf.Applications.CommandVisibility.None);

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering Rename command</summary>
        public void Initialize()
        {
            m_commandService.RegisterCommand(s_renameCommandDef, this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks whether the client can do the command, if it handles it</summary>
        /// <param name="commandObj">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandObj)
        {
            ICommandItem command = commandObj as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            if (command.CommandTag is Commands)
            {
                var target = m_contextRegistry.GetCommandTarget<object>();
                if (target != null)
                {
                    var labelEditingContext = m_contextRegistry.GetActiveContext<ILabelEditingContext>();
                    return labelEditingContext != null && labelEditingContext.CanEditLabel(target);
                }
            }
            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandObj">Command to be done</param>
        public void DoCommand(object commandObj)
        {
            ICommandItem command = commandObj as ICommandItem;
            Requires.NotNull(command, "Object specified is from class that doesn't implement ICommandItem.  Most likely, this is a not a command from WPF, and it should be.");
            switch ((Commands)command.CommandTag)
            {
                case Commands.EditLabel:
                    var target = m_contextRegistry.GetCommandTarget<object>();
                    if (target != null)
                    {
                        var labelEditingContext = m_contextRegistry.GetActiveContext<ILabelEditingContext>();
                        if (labelEditingContext != null)
                            labelEditingContext.EditLabel(target);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command info to update</param>
        public void UpdateCommand(object commandTag, Sce.Atf.Applications.CommandState commandState) { }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets command tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        /// <returns>Command tags for context menu</returns>
        public IEnumerable<object> GetCommands(object context, object target)
        {
            if (target != null && context.Is<ILabelEditingContext>())
                yield return Commands.EditLabel;
        }

        #endregion

    }
}
