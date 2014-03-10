//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapter that observes a DOM subtree and synchronizes multiple history contexts
    /// with an IDocument adapter on the root node. The root DOM node must be adaptable
    /// to IDocument.</summary>
    public class MultipleHistoryContext : Observer
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            m_document = DomNode.Cast<IDocument>();
            m_document.DirtyChanged += document_DirtyChanged;

            base.OnNodeSet();
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM subtree</summary>
        /// <param name="node">Added node</param>
        protected override void AddNode(DomNode node)
        {
            foreach (IHistoryContext context in node.AsAll<IHistoryContext>())
                context.DirtyChanged += History_DirtyChanged;
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from the DOM subtree</summary>
        /// <param name="node">Removed node</param>
        protected override void RemoveNode(DomNode node)
        {
            foreach (IHistoryContext context in node.AsAll<IHistoryContext>())
                context.DirtyChanged -= History_DirtyChanged;
        }

        private void History_DirtyChanged(object sender, EventArgs e)
        {
            if (!m_synchronizing)
            {
                try
                {
                    // a history Dirty bit has changed; form new document-wide Dirty state
                    //  by summing all the histories in the document.
                    m_synchronizing = true;
                    bool dirty = false;
                    foreach (IHistoryContext context in HistoryContexts)
                        dirty |= context.Dirty;

                    // set the document dirty bit to reflect the sum
                    m_document.Dirty = dirty;
                }
                finally
                {
                    m_synchronizing = false;
                }
            }
        }

        private void document_DirtyChanged(object sender, EventArgs e)
        {
            if (!m_synchronizing)
            {
                try
                {
                    m_synchronizing = true;

                    IDocument document = (IDocument)sender;
                    bool dirty = document.Dirty;
                    foreach (IHistoryContext context in HistoryContexts)
                        context.Dirty = dirty;
                }
                finally
                {
                    m_synchronizing = false;
                }
            }
        }

        private IEnumerable<IHistoryContext> HistoryContexts
        {
            get
            {
                foreach (DomNode node in DomNode.Subtree)
                    foreach (IHistoryContext context in node.AsAll<IHistoryContext>())
                        yield return context;
            }
        }

        private IDocument m_document;
        private bool m_synchronizing;
    }
}
