//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class that implements the standard viewing commands on contexts implementing
    /// the IViewingContext interface</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(StandardViewCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardViewCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="scriptingService">Scripting service</param>
        [ImportingConstructor]
        public StandardViewCommands(ICommandService commandService, IContextRegistry contextRegistry, ScriptingService scriptingService)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_scriptingService = scriptingService;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(CommandInfo.ViewFrameSelection, this);
            m_commandService.RegisterCommand(CommandInfo.ViewFrameAll, this);
            
            if (m_scriptingService != null)
                m_scriptingService.SetVariable("atfView", this);
        }

        #endregion

        /// <summary>
        /// Frames the current selection, if the context registry's current context is
        /// adaptable to an IViewingContext and an ISelectionContext</summary>
        public void FrameSelection()
        {
            object activeContext = m_contextRegistry.ActiveContext;
            ISelectionContext selectionContext = activeContext.As<ISelectionContext>();
            if (selectionContext != null)
            {

                FrameSelection(
                    activeContext.As<IViewingContext>(),
                    selectionContext.Selection);
            }
        }

        /// <summary>
        /// Frames the selection</summary>
        /// <param name="viewingContext">Active viewing context</param>
        /// <param name="items">Selected items</param>
        public void FrameSelection(IViewingContext viewingContext, IEnumerable<object> items)
        {
            if (viewingContext != null)
            {
                if (viewingContext.CanFrame(items))
                    viewingContext.Frame(items);
            }
        }

        /// <summary>
        /// Frames all items, if the context registry's current context is
        /// adaptable to an IViewingContext and an IEnumerableContext</summary>
        public void FrameAll()
        {
            object activeContext = m_contextRegistry.ActiveContext;
            IEnumerableContext enumerableContext = activeContext.As<IEnumerableContext>();
            IEnumerable<object> items = (enumerableContext != null) ? enumerableContext.Items : null;
            FrameAll(
                activeContext.As<IViewingContext>(),
                items);

        }

        /// <summary>
        /// Frames items</summary>
        /// <param name="viewingContext">Active viewing context</param>
        /// <param name="items">Items to be framed</param>
        public void FrameAll(IViewingContext viewingContext, IEnumerable<object> items)
        {
            if (items != null &&
                viewingContext != null)
            {
                if (viewingContext.CanFrame(items))
                    viewingContext.Frame(items);
            }
        }

        #region ICommandClient Members

        /// <summary>
        /// Checks if the client can do the command</summary>
        /// <param name="commandTag">Command</param>
        /// <returns><c>True</c> if client can do the command</returns>
        bool ICommandClient.CanDoCommand(object commandTag)
        {
            bool canDo = false;
            if (commandTag is StandardCommand)
            {
                IViewingContext viewingContext = m_contextRegistry.GetActiveContext<IViewingContext>();

                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewFrameSelection:
                        ISelectionContext selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
                        if (viewingContext != null && selectionContext != null)
                            canDo = viewingContext.CanFrame(selectionContext.Selection);
                        break;

                    case StandardCommand.ViewFrameAll:
                        IEnumerableContext enumerableContext = m_contextRegistry.GetActiveContext<IEnumerableContext>();
                        if (viewingContext != null && enumerableContext != null)
                            canDo = viewingContext.CanFrame(enumerableContext.Items);
                        break;
                }
            }

            return canDo;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        void ICommandClient.DoCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.ViewFrameSelection:
                        FrameSelection();
                        break;

                    case StandardCommand.ViewFrameAll:
                        FrameAll();
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

        private ScriptingService m_scriptingService;

        private readonly ICommandService m_commandService;
        private readonly IContextRegistry m_contextRegistry;
    }
}
