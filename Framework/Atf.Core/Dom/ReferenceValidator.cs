//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Validator that tracks references and reference holders to ensure integrity of
    /// references within the DOM data. Checks are only made within validations, which
    /// are signaled by IValidationContexts in the DOM data. The adapter should be
    /// an extension to the DOM's root node. It does the following:
    /// 1. Tracks all DOM node references in the subtree.
    /// 2. Raises notifications if external DOM node references are added or removed.
    /// 3. After validating, raises notification events if referents have been removed,
    /// leaving dangling references.</summary>
    public class ReferenceValidator : Validator
    {
        /// <summary>
        /// Returns a sequence of node-attribute pairs for all references to target</summary>
        /// <param name="target">Reference target</param>
        /// <returns>Sequence of node-attribute pairs for all references to target</returns>
        public IEnumerable<Pair<DomNode, AttributeInfo>> GetReferences(DomNode target)
        {
            List<Pair<DomNode, AttributeInfo>> references;
            if (m_nodeReferenceLists.TryGetValue(target, out references))
                return references;

            return EmptyEnumerable<Pair<DomNode, AttributeInfo>>.Instance;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the validation is suspended.
        /// </summary>
        public bool Suspended { get; set; }

        /// <summary>
        /// Event that is raised when a reference's target Dom node has been removed</summary>
        public event EventHandler<ReferenceEventArgs> ReferentRemoved;

        /// <summary>
        /// Performs actions after a reference's target DOM node has been removed; the
        /// default behavior is to remove the node from its parent</summary>
        /// <param name="e">Event args describing reference</param>
        protected virtual void OnReferentRemoved(ReferenceEventArgs e)
        {
            // check if node has been removed already; this can happen if it
            //  has multiple referents that have been removed
            DomNode parent = e.Owner.Parent;
            if (parent != null)
            {
                // remove node from parent
                e.Owner.RemoveFromParent();
            }
        }

        /// <summary>
        /// Event that is raised when a reference to an external DOM node is added</summary>
        public event EventHandler<ReferenceEventArgs> ExternalReferenceAdded;

        /// <summary>
        /// Performs actions after a reference to an external DOM node is added</summary>
        /// <param name="e">Event args describing reference</param>
        protected virtual void OnExternalReferenceAdded(ReferenceEventArgs e)
        {
        }

        /// <summary>
        /// Event that is raised when a reference to an external DOM node is removed</summary>
        public event EventHandler<ReferenceEventArgs> ExternalReferenceRemoved;

        /// <summary>
        /// Performs actions after a reference to an external DOM node is removed</summary>
        /// <param name="e">Event args describing reference</param>
        protected virtual void OnExternalReferenceRemoved(ReferenceEventArgs e)
        {
            ExternalReferenceRemoved.Raise(this, e);
        }

        /// <summary>
        /// Performs custom actions on validation Beginning events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnBeginning(object sender, EventArgs e)
        {
            m_removed = new HashSet<DomNode>();
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnding(object sender, EventArgs e)
        {
            if (Suspended)
                return;

            while (m_removed.Count > 0)
            {
                // get a snapshot of the removed nodes, so we can clear the set, as dangling
                //  reference handling may result in cascading removals
                DomNode[] removedNodes = m_removed.ToArray();
                m_removed.Clear();

                // update all dangling references to removed node
                foreach (DomNode removedNode in removedNodes)
                {
                    List<Pair<DomNode, AttributeInfo>> referenceList;
                    if (m_nodeReferenceLists.TryGetValue(removedNode, out referenceList))
                    {
                        foreach (Pair<DomNode, AttributeInfo> pair in referenceList.ToArray()) // copy, as list may be modified
                        {
                            ReferenceEventArgs args = new ReferenceEventArgs(pair.First, pair.Second, removedNode);
                            ReferentRemoved.Raise(this, args);
                            OnReferentRemoved(args);
                        }

                        // remove the list of references to  the removed node
                        m_nodeReferenceLists.Remove(removedNode);
                    }
                }
            }
        }

        /// <summary>
        /// Performs custom actions on validation Ended events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnded(object sender, EventArgs e)
        {
            m_removed = null;
        }

        /// <summary>
        /// Performs custom actions on validation Cancelled events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnCancelled(object sender, EventArgs e)
        {
            m_removed = null;
        }

        /// <summary>
        /// Performs custom actions after an attribute in the DOM subtree changes</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute change event args</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (Validating)
            {
                if (e.AttributeInfo.Type.Type == AttributeTypes.Reference)
                {
                    // remove old reference if non-null
                    DomNode oldRef = e.OldValue as DomNode;
                    if (oldRef != null)
                        RemoveReference(e.DomNode, e.AttributeInfo, oldRef);

                    // add new reference if non-null
                    DomNode newRef = e.NewValue as DomNode;
                    if (newRef != null)
                        AddReference(e.DomNode, e.AttributeInfo, newRef);
                }
            }
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM subtree</summary>
        /// <param name="node">Added node</param>
        protected override void AddNode(DomNode node)
        {
            // add all references to tracker
            foreach (AttributeInfo attributeInfo in node.Type.Attributes)
            {
                if (attributeInfo.Type.Type == AttributeTypes.Reference)
                {
                    DomNode reference = node.GetAttribute(attributeInfo) as DomNode;
                    if (reference != null)
                        AddReference(node, attributeInfo, reference);
                }
            }

            if (Validating)
            {
                // if node was previously removed it has been added back
                m_removed.Remove(node);
            }

            base.AddNode(node);
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from the DOM subtree</summary>
        /// <param name="node">Removed node</param>
        protected override void RemoveNode(DomNode node)
        {
            // remove all references from tracker
            foreach (AttributeInfo attributeInfo in node.Type.Attributes)
            {
                if (attributeInfo.Type.Type == AttributeTypes.Reference)
                {
                    DomNode reference = node.GetAttribute(attributeInfo) as DomNode;
                    if (reference != null)
                        RemoveReference(node, attributeInfo, reference);
                }
            }

            if (Validating)
            {
                // add to removed set
                m_removed.Add(node);
            }

            base.RemoveNode(node);
        }

        // adds refs to the tracker, and reports added external refs
        private void AddReference(DomNode owner, AttributeInfo attributeInfo, DomNode target)
        {
            List<Pair<DomNode, AttributeInfo>> referenceList;
            if (!m_nodeReferenceLists.TryGetValue(target, out referenceList))
            {
                referenceList = new List<Pair<DomNode, AttributeInfo>>();
                m_nodeReferenceLists.Add(target, referenceList);
            }
            referenceList.Add(new Pair<DomNode, AttributeInfo>(owner, attributeInfo));

            // if target's root isn't the context's root, then it's an external reference
            //  that is being added.
            DomNode targetRoot = target.GetRoot();
            if (DomNode != targetRoot)
            {
                ReferenceEventArgs e = new ReferenceEventArgs(owner, attributeInfo, target);
                OnExternalReferenceAdded(e);
                ExternalReferenceAdded.Raise(this, e);

            }
        }

        // removes refs from the tracker, and reports removed external refs
        private void RemoveReference(DomNode owner, AttributeInfo attributeInfo, DomNode target)
        {
            List<Pair<DomNode, AttributeInfo>> referenceList;
            if (m_nodeReferenceLists.TryGetValue(target, out referenceList))
            {
                referenceList.Remove(new Pair<DomNode, AttributeInfo>(owner, attributeInfo));
                if (referenceList.Count == 0)
                {
                    m_nodeReferenceLists.Remove(target);
                }
            }

            // if target's root isn't the context's root, then it's an external reference
            //  that is being removed.
            DomNode targetRoot = target.GetRoot();
            if (DomNode != targetRoot)
            {
                ReferenceEventArgs e = new ReferenceEventArgs(owner, attributeInfo, target);
                OnExternalReferenceRemoved(e);
                ExternalReferenceRemoved.Raise(this, e);
            }
        }

        private readonly Dictionary<DomNode, List<Pair<DomNode, AttributeInfo>>> m_nodeReferenceLists =
            new Dictionary<DomNode, List<Pair<DomNode, AttributeInfo>>>();

        private HashSet<DomNode> m_removed;
    }
}
