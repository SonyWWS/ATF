//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Abstract base class for DOM contexts, which track DOM nodes as they enter
    /// and exit the subtree rooted at the DOM node to which this adapter is bound</summary>
    public abstract class Observer : DomNodeAdapter
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        /// <remarks>Derived classes must call this method if it is overridden</remarks>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += OnAttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;
            
            AddSubtree(DomNode);
        }

        /// <summary>
        /// Performs custom actions after an attribute in the DOM subtree changes</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute change event args</param>
        protected virtual void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions after a child is inserted into the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected virtual void OnChildInserted(object sender, ChildEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions after a child is removed from the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected virtual void OnChildRemoved(object sender, ChildEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM subtree</summary>
        /// <param name="node">Added node</param>
        protected virtual void AddNode(DomNode node)
        {
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from the DOM subtree</summary>
        /// <param name="node">Removed node</param>
        protected virtual void RemoveNode(DomNode node)
        {
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            AddSubtree(e.Child);
            OnChildInserted(sender, e);
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            RemoveSubtree(e.Child);
            OnChildRemoved(sender, e);
        }

        private void AddSubtree(DomNode root)
        {
            foreach (DomNode node in root.Subtree)
                AddNode(node);
        }

        private void RemoveSubtree(DomNode root)
        {
            foreach (DomNode node in root.Subtree)
                RemoveNode(node);
        }
    }
}

