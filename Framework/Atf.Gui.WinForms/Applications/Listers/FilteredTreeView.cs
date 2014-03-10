//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Helper class to adapt a data context that implements ITreeView to support filtering items in a tree view.
    /// For performance reasons, it caches the full filtered tree view after applying a filter.</summary>
    public class FilteredTreeView : ITreeView, IAdaptable, IDecoratable
    {

        /// <summary>
        /// Constructor</summary>
        /// <param name="treeView">Data context of ITreeView to apply a filter</param>
        /// <param name="filterFunc">Callback to determine if an item in the tree is filtered in (return true) or out</param>
        public FilteredTreeView(ITreeView treeView, Predicate<object> filterFunc)
        {
            m_treeView = treeView;
            m_filterFunc = filterFunc;
        }

        /// <summary>
        /// Indicates whether two ITreeView instances are equal</summary>
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
            var originalChildren = m_treeView.GetChildren(parent);
            if (IsFiltering)
                return originalChildren.Where(child => m_currentVisibleNodes.Contains(child));
            return originalChildren;
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
        /// Gets all decorators of the specified type, or null</summary>
        /// <param name="type">Decorator type</param>
        /// <returns>Enumeration of decorators that are of the specified type</returns>
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

        internal void BuildTreeCache()
        {
            m_fullTree = null;
            if (m_treeView.Root != null)
            {
                m_fullTree = new Tree<object>(m_treeView.Root);
                BuildTree(m_fullTree);
            }

        }

        private void BuildTree(Tree<object> rootNode)
        {
            foreach (object child in m_treeView.GetChildren(rootNode.Value))
            {
                Tree<object> childNode = new Tree<object>(child);
                childNode.Parent = rootNode;
                BuildTree(childNode);
            }
        }

        internal void BuildVisibility()
        {
            // a node should be visible if it is either visible itself or have visible descendants.
            m_visibleNodes.Clear();
            m_opaqueNodes.Clear();
            if (m_fullTree != null)
            {
                foreach (Tree<object> node in m_fullTree.PreOrder)
                {
                    if (m_filterFunc(node.Value))// node is a direct match
                    {
                        var curNode = node;
                        while (curNode != null && !m_visibleNodes.Contains(curNode.Value)) // its parents should be visible too
                        {
                            m_visibleNodes.Add(curNode.Value);
                            curNode = curNode.Parent;

                        }
                    }

                }
                m_currentVisibleNodes = new HashSet<object>(m_visibleNodes);

                // also build node opacity: true for all children nodes visible; false otherwise
                BuildOpacity(m_fullTree);
                m_currentOpaqueNodes = new HashSet<object>(m_opaqueNodes);
            }
        }

        // return true when not all of its children are currently visble
        internal bool IsNodeCurrentlyOpaque(TreeControl.Node node)
        {
            if (node.Tag == null)
                return false;
            return m_currentOpaqueNodes.Contains(node.Tag);
        }

        // return true when not all of its children are visble
        internal bool IsNodeOpaque(TreeControl.Node node)
        {
            if (node.Tag == null)
                return false;
            return m_opaqueNodes.Contains(node.Tag);
        }

        // return true when not all of its children are visble
        internal bool IsNodeMatched(TreeControl.Node node)
        {
            if (node.Tag == null)
                return false;

            return m_visibleNodes.Contains(node.Tag);
        }

        internal TreeItemRenderer.NodeFilteringStatus NodeCurrentFilteringStatus(TreeControl.Node node)
        {
            TreeItemRenderer.NodeFilteringStatus filteringStatus = TreeItemRenderer.NodeFilteringStatus.Normal;
            if (node.Tag != null)
            {
                if (m_currentOpaqueNodes.Contains(node.Tag))
                    filteringStatus |= TreeItemRenderer.NodeFilteringStatus.PartiallyExpanded;
                if (m_visibleNodes.Contains(node.Tag))
                    filteringStatus |= TreeItemRenderer.NodeFilteringStatus.Visible;
                if (GetUnfilteredChildren(node.Tag).Any(i => m_visibleNodes.Contains(i)))
                    filteringStatus |= TreeItemRenderer.NodeFilteringStatus.ChildVisible;

            }
            return filteringStatus;
        }

        internal void RemoveOpaqueNode(TreeControl.Node node)
        {
            if (m_opaqueNodes.Contains(node.Tag))
            {
                m_currentOpaqueNodes.Remove(node.Tag);
            }
        }

        internal void AddOpaqueNode(TreeControl.Node node)
        {
            if (m_opaqueNodes.Contains(node.Tag))
            {
                m_currentOpaqueNodes.Add(node.Tag);
            }
        }

        private void BuildOpacity(Tree<object> parent)
        {
            // a node is opaque if one, but not all,  of its children is not visible
            int numChildrenInvisible = 0;
            foreach (var child in parent.Children)
            {
                if (!m_visibleNodes.Contains(child.Value)) // if the child's is not visible
                    ++numChildrenInvisible;
            }

            if (numChildrenInvisible > 0 && numChildrenInvisible < parent.Children.Count)
            {
                if (!m_opaqueNodes.Contains(parent.Value))
                    m_opaqueNodes.Add(parent.Value);

            }


            foreach (var child in parent.Children)
                BuildOpacity(child);
        }

        internal bool AddCurrentVisibleNode(object item)
        {

            if (item != null && (!m_currentVisibleNodes.Contains(item)))
            {
                return m_currentVisibleNodes.Add(item);
            }
            return false;
        }

        internal void RemoveVisibleNode(object item)
        {

            if (item != null && (m_currentVisibleNodes.Contains(item)))
            {
                m_currentVisibleNodes.Remove(item);
            }

        }

        /// <summary>
        /// Saves subtree's node expansion states and attaches it to the parent node</summary>
        /// <param name="parent">Node whose expansion state is remembered</param>
        public void RememberExpansion(TreeControl.Node parent)
        {
            if (parent.Tag != null)
            {
                var expandedItems = new List<object>();
                foreach (var node in GetSubtree(parent))
                {
                    if (node.Expanded)
                        expandedItems.Add(node.Tag);
                }
                m_expandedItems[parent.Tag] = expandedItems;
            }
        }

        /// <summary>
        /// Restores subtree's node expansion states, if remembered</summary>
        /// <param name="treeControlAdapter">TreeControlAdapter that performs node expansion</param>
        /// <param name="parent">Node whose subtree's expansion state was remembered</param>
        public void RestoreExpansion(TreeControlAdapter treeControlAdapter, TreeControl.Node parent)
        {
            if (parent.Tag != null && m_expandedItems.ContainsKey(parent.Tag))
            {
                foreach (object item in m_expandedItems[parent.Tag])
                    treeControlAdapter.Expand(item);
            }
        }

        private IEnumerable<TreeControl.Node> GetSubtree(TreeControl.Node parent)
        {
            yield return parent;
            foreach (var child in parent.Children)
                foreach (var decendent in GetSubtree(child))
                    yield return decendent;
        }

        internal IEnumerable<object> GetUnfilteredChildren(object parent)
        {
            return m_treeView.GetChildren(parent);
        }

        private readonly ITreeView m_treeView;
        private Tree<object> m_fullTree; // unfiltered full tree
        private readonly Predicate<object> m_filterFunc;
        private HashSet<object> m_visibleNodes = new HashSet<object>();
        private HashSet<object> m_currentVisibleNodes = new HashSet<object>();
        private HashSet<object> m_opaqueNodes = new HashSet<object>();
        private HashSet<object> m_currentOpaqueNodes;
        private Dictionary<object, List<object>> m_expandedItems = new Dictionary<object, List<object>>();
    }
}