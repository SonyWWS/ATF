//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Implements the standard Edit Undo and Redo commands</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(StandardEditHistoryCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardEditHistoryCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public StandardEditHistoryCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Register edit undo/redo commands
            m_commandService.RegisterCommand(CommandInfo.EditUndo, this);
            m_commandService.RegisterCommand(CommandInfo.EditRedo, this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True if client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool canDo = false;

            var historyContext = m_contextRegistry.GetActiveContext<IHistoryContext>();
            switch ((StandardCommand)commandTag)
            {
                case StandardCommand.EditUndo:
                    canDo = historyContext != null && historyContext.CanUndo;
                    break;

                case StandardCommand.EditRedo:
                    canDo = historyContext != null && historyContext.CanRedo;
                    break;
            }

            return canDo;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            var historyContext = m_contextRegistry.GetActiveContext<IHistoryContext>();
            switch ((StandardCommand)commandTag)
            {
                case StandardCommand.EditUndo:
                    historyContext.Undo();
                    break;

                case StandardCommand.EditRedo:
                    historyContext.Redo();
                    break;
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="commandState">Command state to update</param>
        void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
        {
            var historyContext = m_contextRegistry.GetActiveContext<IHistoryContext>();
            if (historyContext != null)
            {
                if (commandTag.Equals(StandardCommand.EditUndo))
                {
                    commandState.Text = string.Format("Undo {0}".Localize("{0} is the name of the command"), historyContext.UndoDescription);
                }
                else if (commandTag.Equals(StandardCommand.EditRedo))
                {
                    commandState.Text = string.Format("Redo {0}".Localize("{0} is the name of the command"), historyContext.RedoDescription);
                }
            }
        }

        #endregion

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
    }
}
