//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapter that ensures that every DOM node in the subtree has a unique id local to a category</summary>
    /// <remarks>Each DomNode in a Dom-Tree can be assigned a category key for unique id; 
    /// a DomNode has no category key associated by default falls to default category</remarks>
    public class CategoryUniqueIdValidator : IdValidator
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
        /// Get the id category of the given node</summary>
        /// <param name="node">Node</param>
        /// <returns>Id category</returns>
        protected virtual object GetIdCategory(DomNode node)
        {
            return null; 
        }

        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        /// <remarks>Checks ID uniqueness when the adapter's node is set and throws an
        /// InvalidOperationException if a violation is found</remarks>
        protected override void OnNodeSet()
        {
            m_defaultUniqueNamer = new UniqueNamer(m_suffixSeparator);
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
                    var uniqueNamer = GetUniqueNamer(GetIdCategory(node));
                    string uniqueId = uniqueNamer.Name(id);
                    if (id != uniqueId)
                        OnIdCollision(node, uniqueId);                  
                }
            }
        }


        /// <summary>
        /// Performs actions for removed nodes</summary>
        /// <param name="removed">Removed nodes</param>
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected override void RemoveNodes(HashSet<DomNode> removed, Dictionary<DomNode, string> renamed)
        {
            // retire names of removed nodes and their subtrees
            foreach (DomNode node in removed)
            {
                // if the node was renamed, retire the original name and don't
                //  rename it, as it was removed
                string name;
                if (renamed.TryGetValue(node, out name))
                    renamed.Remove(node);
                else
                    name = node.GetId();

                var uniqueNamer = GetUniqueNamer(GetIdCategory(node));
                uniqueNamer.Retire(name);
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
        /// <param name="renamed">Renamed nodes and old ids</param>
        protected override void RenameNodes(Dictionary<DomNode, string> renamed)
        {
            foreach (KeyValuePair<DomNode, string> pair in renamed)
            {
                // retire old name
                var uniqueNamer = GetUniqueNamer(GetIdCategory(pair.Key));
                uniqueNamer.Retire(pair.Value);
                NameNode(pair.Key);
            }
        }

        private void NameNode(DomNode node)
        {
            // if the name isn't unique, make it so
            string id = node.GetId();
            
            if (node.Type.IdAttribute != null)
            {
                var uniqueNamer = GetUniqueNamer(GetIdCategory(node));
                string unique = uniqueNamer.Name(id);
                if (id != unique)
                {
                    node.SetAttribute(node.Type.IdAttribute, unique);
                }
            }
        }

        private UniqueNamer GetUniqueNamer(object category)
        {
            if (category == null)
                return m_defaultUniqueNamer;
            UniqueNamer uniqueNamer;
            if (!m_uniqueNamers.TryGetValue(category, out uniqueNamer))
                m_uniqueNamers.Add(category, new UniqueNamer(m_suffixSeparator));
            return m_uniqueNamers[category];
        }

        private UniqueNamer m_defaultUniqueNamer;
        private Dictionary<object, UniqueNamer> m_uniqueNamers= new Dictionary<object, UniqueNamer>();
        private char m_suffixSeparator = '_';
    }
}
