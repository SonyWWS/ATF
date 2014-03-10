//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Add filtering support for tree editors</summary>
    [Export(typeof(FilteredTreeControlEditor))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class FilteredTreeControlEditor: TreeControlEditor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        [ImportingConstructor]
        public FilteredTreeControlEditor(ICommandService commandService)
            : base(commandService)
        {      
 
  
        }

        /// <summary>
        /// Configures the editor</summary>
        /// <param name="treeControl">Control to display data</param>
        /// <param name="treeControlAdapter">Adapter to drive control. Its ITreeView should
        /// implement IInstancingContext and/or IHierarchicalInsertionContext.</param>
        /// <remarks>Default is to create a TreeControl and TreeControlAdapter,
        /// using the global image lists.</remarks>
        protected override void Configure(
            out TreeControl treeControl,
            out TreeControlAdapter treeControlAdapter)
        {
            treeControl = new TreeControl();
            treeControl.ImageList = ResourceUtil.GetImageList16();
            treeControl.StateImageList = ResourceUtil.GetImageList16();

            treeControlAdapter = new TreeControlAdapter(treeControl);

            treeControl.PreviewKeyDown += treeControl_PreviewKeyDown;
            treeControl.NodeExpandedChanging += treeControl_NodeExpandedChanging;
            treeControl.NodeExpandedChanged += treeControl_NodeExpandedChanged;

 
            m_searchInput = new StringSearchInputUI();
            m_searchInput.Updated += UpdateFiltering;

            m_control = new UserControl();
            m_control.Dock = DockStyle.Fill;
            m_control.SuspendLayout();
            m_control.Name = "Tree View".Localize();
            m_control.Text = "Tree View".Localize();
            m_control.Controls.Add(m_searchInput);
            m_control.Controls.Add(TreeControl);
            m_control.Layout += controls_Layout;
            m_control.ResumeLayout();
        }

       
        /// <summary>
        /// Gets StringSearchInputUI instance</summary>
        public StringSearchInputUI SearchInputUI
        {
            get { return m_searchInput; }
        }


        /// <summary>
        /// Gets the control that hosts the tree view</summary>
        /// <remarks>Use this control to register in ControlHostService if you need the filtering feature</remarks>
        public Control Control
        {
            get { return m_control; }
        }


        #region filtering

        /// <summary>
        /// Callback to determine if an item in the tree is filtered in (return true) or out</summary>
        /// <param name="item">Item tested for filtering</param>
        /// <returns>True if filtered in, false if filtered out</returns>
        public bool DefaultFilter(object item)
        {
            IItemView itemView = TreeView.As<IItemView>();
            if (itemView != null)
            {

                ItemInfo info = new WinFormsItemInfo();
                itemView.GetInfo(item, info);
                return SearchInputUI.IsNullOrEmpty() || SearchInputUI.Matches(info.Label);
            }
            return true; // Don't filter anything if the context doesn't implement IItemView
        }

        /// <summary>
        /// Gets or sets a value indicating whether to restore sub-tree expansion state when collapsing a node</summary>
        public bool RestoreSubExpansion
        {
            get { return m_rememberExpansion; }
            set { m_rememberExpansion = value; }
        }

        /// <summary>
        /// Performs custom actions after filtering finished</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected void UpdateFiltering(object sender, EventArgs e)
        {
            if (TreeView == null || TreeControl == null || TreeView.Root == null)
                return;

            TreeControl.SuspendLayout();
            bool search = !m_searchInput.IsNullOrEmpty();
            if (!m_searching && search) // Search started: remember expansion
            {
                RememberExpansion();
                m_selectedItems = RememberSelection();
            }


            var itemRenderer = this.TreeControl.ItemRenderer;
            itemRenderer.FilteringPattern = m_searchInput.SearchPattern;


            if (TreeView is FilteredTreeView)
            {
                var filteredTreeView = TreeView as FilteredTreeView;
                if (search)
                {
                    filteredTreeView.BuildTreeCache();
                    filteredTreeView.BuildVisibility();
                    filteredTreeView.IsFiltering = true;
                    if (itemRenderer.FilteringStatus == null)
                        itemRenderer.FilteringStatus = filteredTreeView.NodeCurrentFilteringStatus;

                }
                else
                {
                    filteredTreeView.IsFiltering = false;
                }

            }

            m_autoExpanding = true;
            if (search)
            {
                TreeControlAdapter.TreeView = TreeView; // Force reload
                ExpandAllMatches();
            }
            else if (m_searching) // Search stopped: restore expansion
            {
                TreeControlAdapter.TreeView = TreeView; // Force reload
                var currentSelection = RememberSelection();
                if (currentSelection != null && currentSelection.Count() > 0) // if there are selected items during the searching
                    m_selectedItems = currentSelection;
                RestoreExpansion();
                RestoreSelection();
            }
            m_searching = search;
            m_autoExpanding = false;
            TreeControl.ResumeLayout();
        }

        private void RememberExpansion()
        {
            m_expandedItems.Clear();
            foreach (object item in GetSubtree(TreeView.Root))
            {
                if (TreeControlAdapter.IsExpanded(item))
                    m_expandedItems.Add(item);
            }
        }

        private void RestoreExpansion()
        {
            foreach (object item in m_expandedItems)
                TreeControlAdapter.Expand(item);
        }

        private void ExpandAllMatches()
        {
            Tree<object> tree = new Tree<object>(TreeView.Root);
            BuildTree(tree);

            foreach (Tree<object> node in tree.PreOrder)
                if (!node.IsLeaf)
                    TreeControlAdapter.Expand(node.Value);
        }

        private void BuildTree(Tree<object> rootNode)
        {
            foreach (object child in TreeView.GetChildren(rootNode.Value))
            {
                Tree<object> childNode = new Tree<object>(child);
                rootNode.Children.Add(childNode);
                BuildTree(childNode);
            }
        }

        private IEnumerable<object> GetSubtree(object root)
        {
            yield return root;
            foreach (object child in TreeView.GetChildren(root))
                foreach (object decendent in GetSubtree(child))
                    yield return decendent;
        }

        private IEnumerable<object> RememberSelection()
        {
            var selectionContext = Adapters.As<ISelectionContext>(TreeView);
            if (selectionContext != null)
                return selectionContext.Selection;
            return null;
        }

        private void RestoreSelection()
        {
            if (m_selectedItems != null)
            {
                foreach (var item in m_selectedItems)
                {
                    var path = item.As<Path<object>>();
                    if (path != null)
                        TreeControlAdapter.Show(path, true);
                }
            }
        }

        private void controls_Layout(object sender, LayoutEventArgs e)
        {
            //if (TreeControl != null)
            {
                int yoffset = m_searchInput.Visible ? m_searchInput.Height /*+ TreeControl.Margin.Top*/ : 0;
                TreeControl.Bounds = new Rectangle(0, yoffset, m_control.Width, m_control.Height - yoffset);
            }
        }

        /// <summary>
        /// Performs custom actions after key is pressed while focus is on this control</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void treeControl_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Escape && m_searching)
                SearchInputUI.ClearSearch();
        }

        /// <summary>
        /// Performs custom actions before changing a node's Expanded property</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void treeControl_NodeExpandedChanging(object sender, TreeControl.CancelNodeEventArgs e)
        {
            if ((!m_autoExpanding) && m_searching)
            {
                var filteredTreeView = TreeView as FilteredTreeView;
                // 3 states to toggle: 
                if (filteredTreeView.IsNodeOpaque(e.Node) && filteredTreeView.IsNodeCurrentlyOpaque(e.Node))
                {
                    // two cases here: a) if  the node is expanding and all the child nodes are opaue, just expand all its children
                    // if some of the child nodes are visible, and the node is already expanded, then we need to expand all its children too

                    var itsChildren = filteredTreeView.GetChildren(e.Node.Tag); // get visible children
                    if (!itsChildren.Any())
                    {
                        if (!e.Node.Expanded)  /* case a*/
                        {
                            foreach (var child in filteredTreeView.GetUnfilteredChildren(e.Node.Tag))
                                filteredTreeView.AddCurrentVisibleNode(child);
                        }
                    }
                    else if (e.Node.Expanded) // an opaque node is about to collapsing; but we actually want it fully expanded
                    {
                        // this is a manual click-expanding case, make all children of the node visible 
                        bool added = false;
                        foreach (var child in filteredTreeView.GetUnfilteredChildren(e.Node.Tag))
                        {
                            if (filteredTreeView.AddCurrentVisibleNode(child))
                                added = true;
                        }
                        filteredTreeView.RemoveOpaqueNode(e.Node);
                        filteredTreeView.RememberExpansion(e.Node);
                        if (added) // if false, all children are already visible, just collapse the node
                            m_nodeToExpand = e.Node;
                    }

                }
                else if (filteredTreeView.IsNodeOpaque(e.Node) && (!filteredTreeView.IsNodeCurrentlyOpaque(e.Node)) && (e.Node.Expanded))
                {
                    // the opaque node is fakely(fully) expanded, restore to hide non-matched ones
                    filteredTreeView.AddOpaqueNode(e.Node);
                    filteredTreeView.RememberExpansion(e.Node);
                    foreach (var child in e.Node.Children)
                    {
                        if (!filteredTreeView.IsNodeMatched(child))
                            filteredTreeView.RemoveVisibleNode(child.Tag);
                    }
                }
                else if (e.Node.Expanded)
                    filteredTreeView.RememberExpansion(e.Node);
                else if (!filteredTreeView.IsNodeOpaque(e.Node)) // check expanding a node with all childen invisible
                {
                    foreach (var child in filteredTreeView.GetUnfilteredChildren(e.Node.Tag))
                    {
                        filteredTreeView.AddCurrentVisibleNode(child);

                    }
                }

            }
        }

        /// <summary>
        /// Performs custom actions after changing a node's Expanded property</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void treeControl_NodeExpandedChanged(object sender, TreeControl.NodeEventArgs e)
        {
            if (!m_searching)
                return;
            var filteredTreeView = TreeView as FilteredTreeView;
            if (m_nodeToExpand != null)
            {
                var nodeToExpand = m_nodeToExpand;
                m_autoExpanding = true;
                TreeControlAdapter.Expand(m_nodeToExpand.Tag);
                filteredTreeView.RestoreExpansion(TreeControlAdapter, nodeToExpand);
                m_nodeToExpand = null;
                m_autoExpanding = false;
            }
            else if (e.Node.Expanded && RestoreSubExpansion)
            {
                filteredTreeView.RestoreExpansion(TreeControlAdapter, e.Node);
            }
        }


        #endregion
 
        private UserControl m_control;
        private StringSearchInputUI m_searchInput;
        private bool m_searching = false;
        private bool m_autoExpanding = false;
        private TreeControl.Node m_nodeToExpand;
        private List<object> m_expandedItems = new List<object>();
        private IEnumerable<object> m_selectedItems;
        private bool m_rememberExpansion;
    }
}
