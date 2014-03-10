//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Implements standard Lock and Unlock commands on contexts implementing the
    /// ILockingContext interface</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(StandardLockCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardLockCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public StandardLockCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // Register edit lock menu commands
            m_commandService.RegisterCommand(CommandInfo.EditLock, this);
            m_commandService.RegisterCommand(CommandInfo.EditUnlock, this);
        }

        #endregion

        /// <summary>
        /// Locks the lockable items in the locking context</summary>
        ///<param name="items">Items to lock</param>
        ///<param name="lockingContext">Locking context</param>
        public void Lock(
            IEnumerable<object> items,
            ILockingContext lockingContext)
        {
            foreach (object item in items)
                if (lockingContext.CanSetLocked(item) && !lockingContext.IsLocked(item))
                    lockingContext.SetLocked(item, true);
        }

        /// <summary>
        /// Unlocks the lockable items in the locking context</summary>
        ///<param name="items">Items to unlock</param>
        ///<param name="lockingContext">Locking context</param>
        public void Unlock(
            IEnumerable<object> items,
            ILockingContext lockingContext)
        {
            foreach (object item in items)
                if (lockingContext.CanSetLocked(item) && lockingContext.IsLocked(item))
                    lockingContext.SetLocked(item, false);
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True if client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool canDo = false;
            if (commandTag is StandardCommand)
            {
                ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
                ILockingContext lockingContext = m_contextRegistry.GetActiveContext<ILockingContext>();
                if (selectionContext != null &&
                    lockingContext != null)
                {
                    switch ((StandardCommand)commandTag)
                    {
                        case StandardCommand.EditLock:
                            foreach (object item in selectionContext.Selection)
                            {
                                if (lockingContext.CanSetLocked(item) &&
                                    !lockingContext.IsLocked(item))
                                {
                                    canDo = true;
                                    break;
                                }
                            }
                            break;

                        case StandardCommand.EditUnlock:
                            foreach (object item in selectionContext.Selection)
                            {
                                if (lockingContext.CanSetLocked(item) &&
                                    lockingContext.IsLocked(item))
                                {
                                    canDo = true;
                                    break;
                                }
                            }
                            break;
                    }
                }
            }

            return canDo;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
            ITransactionContext transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
            ILockingContext lockingContext = m_contextRegistry.GetActiveContext<ILockingContext>();
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.EditLock:
                        transactionContext.DoTransaction(delegate
                            {
                                Lock(selectionContext.Selection, lockingContext);
                            },
                            CommandInfo.EditLock.MenuText);
                        break;

                    case StandardCommand.EditUnlock:
                        transactionContext.DoTransaction(delegate
                            {
                                Unlock(selectionContext.Selection, lockingContext);
                            },
                            CommandInfo.EditUnlock.MenuText);
                        break;
                }
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

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
    }
}
