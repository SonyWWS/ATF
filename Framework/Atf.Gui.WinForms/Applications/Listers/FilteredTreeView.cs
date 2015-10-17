//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;


namespace Sce.Atf.Applications
{
    using TreeNode = TreeControl.Node;
    using NodeFilteringStatus = TreeItemRenderer.NodeFilteringStatus;

    /// <summary>
    /// Helper class to adapt a data context that implements ITreeView to support filtering items in a tree view.
    /// For performance reasons, it caches the full filtered tree view after applying a filter.</summary>
    public class FilteredTreeView : ITreeView,IItemView,  IAdaptable, IDecoratable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="treeView">Data context of ITreeView to apply a filter</param>
        /// <param name="filterFunc">Callback to determine if an item in the tree is filtered in (return true) or out</param>
        public FilteredTreeView(ITreeView treeView, Predicate<object> filterFunc)
        {
            m_treeView = treeView;
            m_itemView = treeView.As<IItemView>();
            if (filterFunc == null)
                throw new ArgumentNullException("filterFunc cannot be null");
            m_filterFunc = filterFunc;
        }

        /// <summary>
        /// Indicates whether two ITreeView instances are equal</summary>
        /// <param name="first">First ITreeView to compare</param>
        /// <param name="second">Second ITreeView to compare</param>
        /// <returns><c>True</c> if ITreeView instances are equal</returns>
        public static bool Equals(ITreeView first, ITreeView second)
        {
            FilteredTreeView f1 = first.As<FilteredTreeView>();
            if (f1 != null)
                first = f1.m_treeView;
            FilteredTreeView f2 = second.As<FilteredTreeView>();
            if (f2 != null)
                second = f2.m_treeView;
            return first == second;
        }

        #region ITreeView Members

        /// <summary>
        /// Gets tree root</summary>
        public object Root
        {
            get { return m_treeView.Root; }
        }

        /// <summary>
        /// Gets parent's children</summary>
        /// <param name="parent">Parent</param>
        /// <returns>Children objects</returns>
        public IEnumerable<object> GetChildren(object parent)
        {            
            var unfiltered = m_treeView.GetChildren(parent);
            if (IsFiltering && !m_exemptedSet.Contains(parent))
                return unfiltered.Where(item => IsMatched(item) || m_exemptedSet.Contains(item));
            return unfiltered;            
        }
        
        internal NodeFilteringStatus GetNodeFilteringStatus(TreeNode node)
        {
            NodeFilteringStatus stat = NodeFilteringStatus.Normal;
            if(node != null && node.Tag != null && IsFiltering)
            {
                if (!IsFullyExpaned(node.Tag))
                    stat |= NodeFilteringStatus.PartiallyExpanded;

                bool anymatched = IsMatched(node.Tag);
                if (anymatched)
                {
                    stat |= NodeFilteringStatus.Visible;
                    
                    // any child mat
                    if (m_treeView.GetChildren(node.Tag).Any(IsMatched))
                        stat |= NodeFilteringStatus.ChildVisible;
                }               
            }                
            return stat;
        }
              
        internal void SaveExpansion(TreeNode parent)
        {
            if (parent.Tag == null)
                return;
            var expandedItems = new List<object>();
            foreach (var node in GetSubtree(parent))
                if (node.Tag != null && node.Expanded)
                    expandedItems.Add(node.Tag);
            if (expandedItems.Count > 0)
                m_expandedNodeMap[parent.Tag] = expandedItems;
        }

        internal void RestoreExpansion(TreeControl.Node parent, TreeControlAdapter treeAdapter)
        {
            if (parent.Tag == null)
                return;
            List<object> expandedItems;
            if (m_expandedNodeMap.TryGetValue(parent.Tag, out expandedItems))
            {
                m_expandedNodeMap.Remove(parent.Tag);
                expandedItems.ForEach(item => treeAdapter.Expand(item));                
            }
        }

        /// <summary>
        /// Clears matched set.
        /// Must be called when filter changes.</summary>
        internal void ClearCache()
        {
            m_matchedNodes.Clear();
            m_exemptedSet.Clear();            
        }
               

        internal bool IsMatched(object node)
        {            
            // if not filtering or the node is already in the matched set.
            if (!IsFiltering || m_matchedNodes.Contains(node)) 
                return true;
               
            bool result = false;            
            try
            {
                m_matching = true;
                // perform a post-order traversal starting from node.
                // when match found add the matched node and all its ancestors to the m_matchedNodes set
                // and return true otherwise return false.
                // m_matchedNodes is used for speedup future lookups.
                m_tmpNodelist.Clear();
                result = MatchRecv(node);
            }
            finally
            {
                m_matching = false;
            }

            return result;
        }


        // temp list used to hold path when traversing tree.
        private List<object> m_tmpNodelist = new List<object>();
        private bool MatchRecv(object node)
        {            
            bool result = false;            
            m_tmpNodelist.Add(node);            
            foreach (object child in m_treeView.GetChildren(node))
            {
                result = MatchRecv(child);
                if (result )break;
            }
                  
            if (!result && m_filterFunc(node))
            {
                for (int i = m_tmpNodelist.Count - 1; i >= 0; i--)
                    if (!m_matchedNodes.Add(m_tmpNodelist[i])) break;
                result = true;
            }            
            m_tmpNodelist.RemoveAt(m_tmpNodelist.Count - 1);           
            return result;
        }


        /// <summary>
        /// Check whether calling GetChildren(object parent) will return 
        /// all the children.
        /// GetChildren(object parent) could return all if every child 
        /// either passed the filter or exempt from filtering</summary>
        /// <param name="parent">parent item</param>
        /// <returns>true if all the children will returned</returns>
        internal bool IsFullyExpaned(object parent)
        {            
            return m_exemptedSet.Contains(parent) || !m_treeView.GetChildren(parent).Any(child => !IsMatched(child) && !m_exemptedSet.Contains(child));            
        }


        /// <summary>
        /// Add any child fails the filter to exempted set.</summary>
        /// <param name="parent">parent node</param>
        internal void AddToExemptSet(object parent)
        {
            m_exemptedSet.Add(parent);
            foreach (object child in m_treeView.GetChildren(parent))
                if (!IsMatched(child)) 
                    m_exemptedSet.Add(child);            
        }


        /// <summary>
        /// Remove children from from exempted set</summary>
        /// <param name="parent">parent node</param>
        internal void RemoveFromExemptSet(object parent)
        {
            m_exemptedSet.Remove(parent);
            m_treeView.GetChildren(parent).ForEach(child => m_exemptedSet.Remove(child));            
        }

        #endregion

        #region IItemView Members

        public void GetInfo(object item, ItemInfo info)
        {
            m_itemView.GetInfo(item, info);

            // Call the filtered GetChildren() 
            // to determine if node is leaf or not.
            if (!m_matching && !info.IsLeaf)
                info.IsLeaf = !GetChildren(item).Any();
        }

        #endregion

        #region IAdaptable Members

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null</returns>
        public object GetAdapter(Type type)
        {
            if (typeof(FilteredTreeView).IsAssignableFrom(type))
                return this;
            object converted = m_treeView.As(type);
            if (converted != null)
                return converted;
            return m_treeView;
        }

        #endregion

        #region IDecoratable Members
        /// <summary>
        /// Gets all decorators of the specified type</summary>
        /// <param name="type">Decorator type</param>
        /// <returns>Enumeration of non-null decorators that are of the specified type. The enumeration may be empty.</returns>
        public IEnumerable<object> GetDecorators(Type type)
        {
            object  adapter = GetAdapter(type);
            if (adapter != null)
                yield return adapter;
        }
        #endregion


        /// <summary>
        /// Gets or sets whether filtering is activated</summary>
        public bool IsFiltering { get; set; }

        private IEnumerable<TreeControl.Node> GetSubtree(TreeControl.Node parent)
        {
            yield return parent;
            foreach (TreeControl.Node child in parent.Children)
                foreach (TreeControl.Node decendent in GetSubtree(child))
                    yield return decendent;
        }

        
        private readonly ITreeView m_treeView;
        private readonly IItemView m_itemView;
        private readonly Predicate<object> m_filterFunc;

        //re-entry guard
        private bool m_matching; 

        // exempted from filtering.
        // used for caching all the items that are exempt from filtering.
        // the items in this set will be not filtered out.
        // This dynamic set items will be added and removed
        // as the user interact with TreeControl.
        private HashSet<object> m_exemptedSet = new HashSet<object>();

        // Used for caching all the nodes that matches current filter.
        // it must be cleared whenever search pattern changeds.
        private HashSet<object> m_matchedNodes = new HashSet<object>();

        // Maps subtree item to list of expanded items in the subtree.
        // This map used for restoring the expansion staste of a subtree.               
        private Dictionary<object, List<object>>
            m_expandedNodeMap = new Dictionary<object, List<object>>();        
    }
}