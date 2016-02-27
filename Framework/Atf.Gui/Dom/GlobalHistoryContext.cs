//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapter that implements a single global history on a DOM tree containing multiple
    /// local HistoryContexts. The adapter tracks all other HistoryContexts in the subtree
    /// rooted at DomNode and passes their transactions to the global HistoryContext, which
    /// must be on the same DomNode as this adapter.</summary>
    public class GlobalHistoryContext : Observer
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            m_historyContext = DomNode.Cast<HistoryContext>(); // mandatory history context
            m_childHistoryContexts = new HashSet<HistoryContext>();
            base.OnNodeSet();
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM subtree</summary>
        /// <param name="node">Added node</param>
        protected override void AddNode(DomNode node)
        {
            // route all other histories into the global one
            foreach (HistoryContext historyContext in node.AsAll<HistoryContext>())
                if (historyContext != m_historyContext)
                {
                    m_childHistoryContexts.Add(historyContext);
                    historyContext.History = m_historyContext.History;
                }
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from the DOM subtree</summary>
        /// <param name="node">Removed node</param>
        protected override void RemoveNode(DomNode node)
        {
            // route all other histories back into a local history
            foreach (HistoryContext historyContext in node.AsAll<HistoryContext>())
                if (historyContext != m_historyContext)
                {
                    m_childHistoryContexts.Remove(historyContext);
                    historyContext.History = new CommandHistory();
                }
        }

        /// <summary>
        /// Synchronize undo/redo status among HistoryContexts that shares the global history
        /// </summary>
        /// <param name="newStatus"> the new status to set</param>
        public virtual void SynchronizeUndoRedoStatus(bool newStatus)
        {
            m_historyContext.UndoingOrRedoing = newStatus;
            foreach (var h in m_childHistoryContexts)
                h.UndoingOrRedoing = newStatus;
        }
        protected HistoryContext m_historyContext;
        private HashSet<HistoryContext> m_childHistoryContexts;
    }
}
