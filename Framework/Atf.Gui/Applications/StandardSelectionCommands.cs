//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class that implements the standard selection commands</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(StandardSelectionCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StandardSelectionCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public StandardSelectionCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(CommandInfo.EditSelectAll, this);
            m_commandService.RegisterCommand(CommandInfo.EditDeselectAll, this);
            m_commandService.RegisterCommand(CommandInfo.EditInvertSelection, this);
        }

        #endregion

        /// <summary>
        /// Selects all enumerable objects of the current context</summary>
        /// <returns>True iff all objects were selected</returns>
        public bool SelectAll()
        {
            object activeContext = m_contextRegistry.ActiveContext;
            return
                SelectAll(
                    activeContext.As<ISelectionContext>(),
                    activeContext.As<IEnumerableContext>());
        }

        /// <summary>
        /// Selects all enumerable objects in the given context</summary>
        /// <param name="selectionContext">Context holding selection</param>
        /// <param name="enumerableContext">Context holding enumeration of selectable objects</param>
        /// <returns>True iff all objects were selected</returns>
        public bool SelectAll(ISelectionContext selectionContext, IEnumerableContext enumerableContext)
        {
            if (selectionContext != null &&
                enumerableContext != null)
            {
                selectionContext.SetRange(enumerableContext.Items);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deselects all objects in the current context</summary>
        /// <returns>True iff all objects were deselected</returns>
        public bool DeselectAll()
        {
            return
                DeselectAll(m_contextRegistry.GetActiveContext<ISelectionContext>());
        }

        /// <summary>
        /// Deselects all objects in the given context</summary>
        /// <param name="selectionContext">Context holding selection</param>
        /// <returns>True iff all objects were deselected</returns>
        public bool DeselectAll(ISelectionContext selectionContext)
        {
            if (selectionContext != null)
            {
                selectionContext.Clear();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Inverts the selection (deselect all selected, and select all deselected objects)</summary>
        /// <returns>True iff selection was inverted</returns>
        public bool InvertSelection()
        {
            return
                InvertSelection(
                    m_contextRegistry.GetActiveContext<ISelectionContext>(),
                    m_contextRegistry.GetActiveContext<IEnumerableContext>());
        }

        /// <summary>
        /// Inverts the selection in the given context (deselect all selected, and select all deselected objects)</summary>
        /// <param name="selectionContext">Context holding selection</param>
        /// <param name="enumerableContext">Context holding enumeration of selectable objects</param>
        /// <returns>True iff selection was inverted</returns>
        public bool InvertSelection(ISelectionContext selectionContext, IEnumerableContext enumerableContext)
        {
            if (selectionContext != null &&
                enumerableContext != null)
            {
                HashSet<object> selected = new HashSet<object>(selectionContext.Selection);
                List<object> inverted = new List<object>(enumerableContext.Items);
                for (int i = 0; i < inverted.Count; )
                {
                    if (selected.Contains(inverted[i]))
                        inverted.RemoveAt(i);
                    else
                        i++;
                }

                selectionContext.SetRange(inverted);
            }

            return false;
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
                IEnumerableContext enumerableContext = m_contextRegistry.GetActiveContext<IEnumerableContext>(); ;

                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.EditSelectAll:
                        if (selectionContext != null && enumerableContext != null)
                        {
                            // test if there are any objects
                            foreach (object item in enumerableContext.Items)
                            {
                                canDo = true;
                                break;
                            }
                        }
                        break;

                    case StandardCommand.EditDeselectAll:
                        canDo =
                            selectionContext != null &&
                            selectionContext.LastSelected != null;
                        break;

                    case StandardCommand.EditInvertSelection:
                        canDo =
                            selectionContext != null && // if selection, and enumerable, we can invert
                            enumerableContext != null;
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
                    case StandardCommand.EditSelectAll:
                        SelectAll();
                        break;

                    case StandardCommand.EditDeselectAll:
                        DeselectAll();
                        break;

                    case StandardCommand.EditInvertSelection:
                        InvertSelection();
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
