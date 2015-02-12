//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Abstract base class for node id validators. This class collects nodes
    /// that have been added, removed, and renamed in the DOM subtree and passes
    /// them to derived class for handling.</summary>
    public abstract class IdValidator : Validator
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        /// <remarks>Checks ID uniqueness when the adapter's node is set and throws an
        /// InvalidOperationException if a violation is found</remarks>
        protected override void OnNodeSet()
        {
            ValidateSubtree();

            base.OnNodeSet();
        }

        /// <summary>
        /// Checks all DOM nodes in the subtree for validity</summary>
        protected abstract void ValidateSubtree();

        /// <summary>
        /// Performs clean up or flags an error in case of an id collision</summary>
        /// <param name="node">Node with id that collides with a previous node's id</param>
        /// <param name="uniqueId">Unique id formed from node's original id</param>
        /// <remarks>Default behavior is to throw an InvalidOperationException</remarks>
        protected virtual void OnIdCollision(DomNode node, string uniqueId)
        {
            throw new InvalidOperationException("2 nodes have the same id");
        }

        /// <summary>
        /// Performs custom actions on validation Beginning events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnBeginning(object sender, EventArgs e)
        {
            m_added = new HashSet<DomNode>();
            m_removed = new HashSet<DomNode>();
            m_renamed = new Dictionary<DomNode, string>();
        }

        /// <summary>
        /// Gets a value indicating if the validator is naming nodes</summary>
        protected bool Naming
        {
            get { return m_naming; }
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnding(object sender, EventArgs e)
        {
            try
            {
                RemoveNodes(m_removed, m_renamed);

                m_naming = true;

                AddNodes(m_added, m_renamed);

                RenameNodes(m_renamed);
            }
            finally
            {
                m_naming = false;

                m_added = null;
                m_removed = null;
                m_renamed = null;
            }
        }

        /// <summary>
        /// Performs custom actions on validation Cancelled events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnCancelled(object sender, EventArgs e)
        {
            m_naming = false;

            m_added = null;
            m_removed = null;
            m_renamed = null;

            base.OnCancelled(sender, e);
        }

        /// <summary>
        /// Performs actions for removed nodes</summary>
        /// <param name="removed">Removed nodes</param>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected abstract void RemoveNodes(HashSet<DomNode> removed, Dictionary<DomNode, string> renamed);

        /// <summary>
        /// Performs actions for added nodes</summary>
        /// <param name="added">Added nodes</param>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected abstract void AddNodes(HashSet<DomNode> added, Dictionary<DomNode, string> renamed);

        /// <summary>
        /// Performs actions for renamed nodes</summary>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected abstract void RenameNodes(Dictionary<DomNode, string> renamed);

        /// <summary>
        /// Performs custom actions after an attribute in the DOM subtree changes</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute change event args</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (Validating && !m_naming)
            {
                if (e.AttributeInfo.Equivalent(e.DomNode.Type.IdAttribute))
                {
                    // only store the first renaming of the node, as we only need the original id
                    if (!m_renamed.ContainsKey(e.DomNode))
                        m_renamed.Add(e.DomNode, e.OldValue.ToString());
                }
            }
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to our DOM subtree.
        /// This method is called separately for every DomNode in the added subtree.
        /// The default implementation treats a removal of a DomNode followed by adding
        /// it (even to a different parent) as a no-op.</summary>
        /// <param name="node">Added node</param>
        /// <remarks>Method overrides must call the base method.</remarks>
        protected override void AddNode(DomNode node)
        {
            if (Validating && !m_naming)
            {
                if (node.Type.IdAttribute != null)
                {
                    // add to inserted set if it wasn't previously removed; ie. a node that
                    //  is reparented in a single transaction.
                    if (!m_removed.Remove(node))
                        m_added.Add(node);
                }
            }

            base.AddNode(node);
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from our DOM subtree.
        /// This method is called for each DomNode removed and for that DomNode's children,
        /// and their children and so on, in depth-first order.</summary>
        /// <param name="node">Removed node</param>
        /// <remarks>Method overrides must call the base method.</remarks>
        protected override void RemoveNode(DomNode node)
        {
            if (Validating && !m_naming)
            {
                if (node.Type.IdAttribute != null)
                {
                    // add to removed set if it wasn't first inserted; ie. a temporary
                    //  node (this is probably less likely than the opposite case, above.)
                    if (!m_added.Remove(node))
                        m_removed.Add(node);
                }
            }

            base.RemoveNode(node);
        }

        private HashSet<DomNode> m_added;
        private HashSet<DomNode> m_removed;
        private Dictionary<DomNode, string> m_renamed;
        private bool m_naming;
    }
}
