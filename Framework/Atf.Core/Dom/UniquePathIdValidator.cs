//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapter that ensures that every DOM child node has a unique id, so that
    /// all paths composed of ids are unique</summary>
    public class UniquePathIdValidator : IdValidator
    {
        /// <summary>
        /// Gets or sets the suffix separator character, used by the UniqueNamer to
        /// generate unique names. This property can be set by derived classes in
        /// their OnNodeSet overrides.</summary>
        protected char SuffixSeparator
        {
            get { return m_suffixSeparator; }
            set { m_suffixSeparator = value; }
        }

        /// <summary>
        /// Checks all DOM nodes in the subtree for validity</summary>
        protected override void ValidateSubtree()
        {
            UniqueNamer uniqueNamer = new UniqueNamer(m_suffixSeparator);
            foreach (DomNode node in DomNode.Subtree)
            {
                foreach (DomNode child in node.Children)
                {
                    if (child.Type.IdAttribute != null)
                    {
                        string id = child.GetId();
                        string uniqueId = uniqueNamer.Name(id);
                        if (id != uniqueId)
                            OnIdCollision(child, uniqueId);
                    }
                }
                uniqueNamer.Clear();
            }
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnding(object sender, EventArgs e)
        {
            base.OnEnding(sender, e);
            m_added.Clear();
        }

        /// <summary>
        /// Performs custom actions on validation Cancelled events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnCancelled(object sender, EventArgs e)
        {
            base.OnCancelled(sender, e);
            m_added.Clear();
        }

        /// <summary>
        /// Performs actions for removed nodes</summary>
        /// <param name="removed">Removed nodes</param>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected override void RemoveNodes(HashSet<DomNode> removed, Dictionary<DomNode, string> renamed)
        {
            foreach (DomNode node in removed)
                renamed.Remove(node);
        }

        /// <summary>
        /// Performs actions for added nodes</summary>
        /// <param name="added">Added nodes</param>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected override void AddNodes(HashSet<DomNode> added, Dictionary<DomNode, string> renamed)
        {
            HashSet<DomNode> parents = new HashSet<DomNode>();
            foreach (DomNode node in m_added)
            {
                renamed.Remove(node);
                parents.Add(node.Parent);
            }

            UniqueNamer uniqueNamer = new UniqueNamer(m_suffixSeparator);
            foreach (DomNode parent in parents)
            {
                // first, initialize namer with the names of the pre-existing children
                foreach (DomNode child in parent.Children)
                {
                    if (child.Type.IdAttribute != null &&
                        !m_added.Contains(child))
                    {
                        string id = child.GetId();
                        uniqueNamer.Name(id);
                    }
                }

                // now, generate unique ids for the added children
                foreach (DomNode child in parent.Children)
                {
                    if (child.Type.IdAttribute != null &&
                        m_added.Contains(child))
                    {
                        NameNode(child, uniqueNamer);
                    }
                }

                uniqueNamer.Clear();
            }
        }

        /// <summary>
        /// Performs actions for renamed nodes</summary>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected override void RenameNodes(Dictionary<DomNode, string> renamed)
        {
            HashSet<DomNode> parents = new HashSet<DomNode>();
            foreach (DomNode node in renamed.Keys)
                if (node.Parent != null)
                    parents.Add(node.Parent);

            UniqueNamer uniqueNamer = new UniqueNamer(m_suffixSeparator);
            foreach (DomNode parent in parents)
            {
                // first, initialize namer with the names of the children whose names haven't changed
                foreach (DomNode child in parent.Children)
                {
                    if (child.Type.IdAttribute != null &&
                        !renamed.ContainsKey(child))
                    {
                        string id = child.GetId();
                        uniqueNamer.Name(id);
                    }
                }

                // now, generate unique ids for the added children
                foreach (DomNode child in parent.Children)
                {
                    if (child.Type.IdAttribute != null &&
                        renamed.ContainsKey(child))
                    {
                        NameNode(child, uniqueNamer);
                    }
                }

                uniqueNamer.Clear();
            }
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM subtree.
        /// This method is called separately for every DomNode in the added subtree.
        /// The default implementation treats a removal of a DomNode followed by adding
        /// it (even to a different parent) as a no-op.</summary>
        /// <param name="node">Added node</param>
        /// <remarks>Method overrides must call the base method.</remarks>
        protected override void AddNode(DomNode node)
        {
            if (Validating && !Naming && node.Type.IdAttribute != null)
                m_added.Add(node);

            base.AddNode(node);
        }

        private void NameNode(DomNode node, UniqueNamer namer)
        {
            // if the name isn't unique, make it so
            string id = node.GetId();
            string unique = namer.Name(id);
            if (id != unique)
            {
                node.SetAttribute(node.Type.IdAttribute, unique);
            }
        }

        private char m_suffixSeparator = '_';
        
        //This is the set of unique DomNodes added during the transaction that have an IdAttribute.
        //The base class's m_added can be missing nodes that we care about, because IdValidator
        //  assumes that a DomNode that is removed and then added somewhere else in the subtree
        //  can be ignored.
        readonly HashSet<DomNode> m_added = new HashSet<DomNode>();
    }
}
