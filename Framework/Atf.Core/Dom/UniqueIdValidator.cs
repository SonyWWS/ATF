//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapter that ensures that every DOM node in the subtree has a unique id</summary>
    public class UniqueIdValidator : IdValidator
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
        /// Performs initialization when the adapter's node is set</summary>
        /// <remarks>Checks ID uniqueness when the adapter's node is set and throws an
        /// InvalidOperationException if a violation is found</remarks>
        protected override void OnNodeSet()
        {
            m_uniqueNamer = new UniqueNamer(m_suffixSeparator);

            base.OnNodeSet();
        }

        /// <summary>
        /// Checks all DOM nodes in the subtree for validity</summary>
        protected override void ValidateSubtree()
        {
            foreach (DomNode node in DomNode.Subtree)
            {
                if (node.Type.IdAttribute != null)
                {
                    string id = node.GetId();
                    string uniqueId = m_uniqueNamer.Name(id);
                    if (id != uniqueId)
                        OnIdCollision(node, uniqueId);
                }
            }
        }

        /// <summary>
        /// Performs actions for removed nodes</summary>
        /// <param name="m_removed">Removed nodes</param>
        /// <param name="m_renamed">Renamed nodes and old ids</param>
        protected override void RemoveNodes(HashSet<DomNode> m_removed, Dictionary<DomNode, string> m_renamed)
        {
            // retire names of removed nodes and their subtrees
            foreach (DomNode node in m_removed)
            {
                // if the node was renamed, retire the original name and don't
                //  rename it, as it was removed
                string name;
                if (m_renamed.TryGetValue(node, out name))
                    m_renamed.Remove(node);
                else
                    name = node.GetId();

                m_uniqueNamer.Retire(name);
            }
        }

        /// <summary>
        /// Performs actions for added nodes</summary>
        /// <param name="added">Added nodes</param>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected override void AddNodes(HashSet<DomNode> added, Dictionary<DomNode, string> renamed)
        {
            // uniquely name inserted nodes and their subtrees
            foreach (DomNode node in added)
            {
                // don't bother with rename
                renamed.Remove(node);
                NameNode(node);
            }
        }

        /// <summary>
        /// Performs actions for renamed nodes</summary>
        /// <param name="m_renamed">Renamed nodes and old ids</param>
        protected override void RenameNodes(Dictionary<DomNode, string> m_renamed)
        {
            foreach (KeyValuePair<DomNode, string> pair in m_renamed)
            {
                // retire old name
                m_uniqueNamer.Retire(pair.Value);
                NameNode(pair.Key);
            }
        }

        private void NameNode(DomNode node)
        {
            // if the name isn't unique, make it so
            string id = node.GetId();
            
            // DAN: Edit here to allow nodes with no ID attribute to exist!
            if (node.Type.IdAttribute != null)
            {
                string unique = m_uniqueNamer.Name(id);
                if (id != unique)
                {
                    node.SetAttribute(node.Type.IdAttribute, unique);
                }
            }
        }

        private UniqueNamer m_uniqueNamer;
        private char m_suffixSeparator = '_';
    }
}
