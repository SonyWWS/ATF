//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class to adapt a TreeControl to a data context that implements ITreeView.
    /// These optional interfaces may also be used:
    /// 1. IItemView is used to determine the item's label, icon, small icon,
    /// whether it's checked, etc.
    /// 2. IObservableContext is used to keep the TreeControl nodes in synch with
    /// the data.
    /// 3. IValidationContext is used to defer updates until the data becomes
    /// stable. This allows more efficient updates of the TreeControl, and saves
    /// IObservableContext implementations the trouble of calculating indices for
    /// ItemInserted and ItemRemoved events. These may be set to -1 in this case,
    /// and the TreeControl can still update itself correctly.</summary>
    public class TreeControlAdapter
    {
        /// <summary>
        /// Constructor that uses the default TreeControl and the default equality comparer</summary>
        public TreeControlAdapter()
            : this(new TreeControl(), null)
        {
        }

        /// <summary>
        /// Constructor that uses the given TreeControl and the default equality comparer</summary>
        /// <param name="treeControl">Tree control to use</param>
        public TreeControlAdapter(TreeControl treeControl)
            : this(treeControl, null)
        {
        }

        /// <summary>
        /// Constructor that uses the given TreeControl and equality comparer</summary>
        /// <param name="treeControl">Tree control to use. Use "new TreeControl()" for the default.</param>
        /// <param name="comparer">The comparer used to compare nodes in the tree, or null to use the
        /// default comparer for type T</param>
        public TreeControlAdapter(TreeControl treeControl, IEqualityComparer<object> comparer)
        {
            m_treeControl = treeControl;
            m_itemToNodeMap = new Multimap<object, TreeControl.Node>(comparer);

            m_treeControl.MouseDown += treeControl_MouseDown;
            m_treeControl.MouseUp += treeControl_MouseUp;
            m_treeControl.DragOver += treeControl_DragOver;
            m_treeControl.DragDrop += treeControl_DragDrop;

            m_treeControl.NodeExpandedChanged += treeControl_NodeExpandedChanged;
            m_treeControl.NodeSelectedChanged += treeControl_NodeSelectedChanged;
            m_treeControl.SelectionChanging += treeControl_SelectionChanging;
            m_treeControl.SelectionChanged += treeControl_SelectionChanged;
        }

        /// <summary>
        /// Gets or sets the tree displayed in the control. When setting, consider having the
        /// ITreeView object also implement IItemView, IObservableContext, IValidationContext,
        /// ISelectionContext, IInstancingContext, and IHierarchicalInsertionContext.</summary>
        public ITreeView TreeView
        {
            get
            {
                return m_treeView;
            }
            set
            {
                if (m_treeView != value)
                {
                    if (m_treeView != null)
                    {
                        m_itemView = null;

                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted -= tree_ItemInserted;
                            m_observableContext.ItemRemoved -= tree_ItemRemoved;
                            m_observableContext.ItemChanged -= tree_ItemChanged;
                            m_observableContext.Reloaded -= tree_Reloaded;
                            m_observableContext = null;
                        }

                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning -= validationContext_Beginning;
                            m_validationContext.Ended -= validationContext_Ended;
                            m_validationContext.Cancelled -= validationContext_Cancelled;
                            m_validationContext = null;
                        }

                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanged -= selection_Changed;
                            m_selectionContext = null;
                        }
                    }

                    m_treeView = value;

                    if (m_treeView != null)
                    {
                        m_itemView = m_treeView.As<IItemView>();

                        m_observableContext = m_treeView.As<IObservableContext>();
                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted += tree_ItemInserted;
                            m_observableContext.ItemRemoved += tree_ItemRemoved;
                            m_observableContext.ItemChanged += tree_ItemChanged;
                            m_observableContext.Reloaded += tree_Reloaded;
                        }

                        m_validationContext = m_treeView.As<IValidationContext>();
                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning += validationContext_Beginning;
                            m_validationContext.Ended += validationContext_Ended;
                            m_validationContext.Cancelled += validationContext_Cancelled;
                        }

                        m_selectionContext = m_treeView.As<ISelectionContext>();
                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanged += selection_Changed;
                        }
                    }
                }

                Load();
            }
        }

        /// <summary>
        /// Gets the adapted tree control</summary>
        public TreeControl TreeControl
        {
            get { return m_treeControl; }
        }

        /// <summary>
        /// Gets the last object that the user clicked or dragged over</summary>
        public object LastHit
        {
            get { return m_lastHit; }
        }

        /// <summary>
        /// Event that is raised after the last hit object changes</summary>
        public event EventHandler LastHitChanged;

        /// <summary>
        /// Raises the LastHitChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnLastHitChanged(EventArgs e)
        {
            LastHitChanged.Raise(this, e);
        }

        /// <summary>
        /// Gets or sets whether tree nodes are automatically
        /// expanded to show descendant nodes that are selected or inserted.
        /// The default is 'true'.</summary>
        public bool AutoExpand
        {
            get { return m_autoExpand; }
            set { m_autoExpand = value; }
        }

        /// <summary>
        /// Expands any tree nodes corresponding to the given item</summary>
        /// <param name="item">Item which is to be expanded</param>
        public void Expand(object item)
        {
            IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[item];
            foreach (TreeControl.Node node in nodes)
                node.Expanded = true;
        }

        /// <summary>
        /// Collapses any tree nodes corresponding to the given item</summary>
        /// <param name="item">Item which is to be collapsed</param>
        public void Collapse(object item)
        {
            IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[item];
            foreach (TreeControl.Node node in nodes)
                node.Expanded = false;
        }

        /// <summary>
        /// Efficiently gets all of the paths of the tree nodes that contain the given item</summary>
        /// <param name="item">The item to search for</param>
        /// <returns>An enumeration of the paths of nodes that contain the item</returns>
        public IEnumerable<Path<object>> GetPaths(object item)
        {
            foreach (TreeControl.Node node in m_itemToNodeMap[item])
                yield return MakePath(node);
        }

        /// <summary>
        /// Finds the node defined exactly by 'path' and expands the tree as far as possible</summary>
        /// <param name="path">The path of objects to search for</param>
        /// <returns>Node whose tag matches the last object in 'path',
        /// or null if the node wasn't found</returns>
        public TreeControl.Node ExpandPath(Path<object> path)
        {
            return ExpandPath(path, false);
        }

        /// <summary>
        /// Gets the item under the given point</summary>
        /// <param name="clientPoint">Point, in client coordinates</param>
        /// <returns>Item under point, or null if none</returns>
        public object GetItemAt(Point clientPoint)
        {
            TreeControl.Node node = m_treeControl.GetNodeAt(clientPoint);
            if (node != null)
                return node.Tag;

            return null;
        }

        /// <summary>
        /// Gets the path from the root to the item under the given point</summary>
        /// <param name="clientPoint">Point, in client coordinates</param>
        /// <returns>Path from the root to the item under the given point, or null if none</returns>
        public Path<object> GetPathAt(Point clientPoint)
        {
            TreeControl.Node node = m_treeControl.GetNodeAt(clientPoint);
            if (node != null)
            {
                List<object> path = new List<object>();
                while (node != null)
                {
                    path.Add(node.Tag);
                    node = node.Parent;
                }
                path.Reverse();
                return new AdaptablePath<object>(path);
            }

            return null;
        }

        /// <summary>
        /// Gets the items corresponding to the selected nodes</summary>
        /// <returns>Items corresponding to the selected nodes</returns>
        public object[] GetSelectedItems()
        {
            List<object> items = new List<object>();
            foreach (TreeControl.Node node in m_treeControl.SelectedNodes)
                items.Add(node.Tag);

            return items.ToArray();
        }

        /// <summary>
        /// Gets the paths from the root to all selected nodes</summary>
        /// <returns>Paths from the root to all selected nodes</returns>
        public Path<object>[] GetSelectedPaths()
        {
            List<Path<object>> paths = new List<Path<object>>();
            foreach (TreeControl.Node node in m_treeControl.SelectedNodes)
                paths.Add(MakePath(node));

            return paths.ToArray();
        }

        /// <summary>
        /// Refreshes the subtrees for any tree nodes corresponding to the given item. The current
        /// expansion of each node is preserved, if possible.</summary>
        /// <param name="item">Item that is to be refreshed</param>
        public void Refresh(object item)
        {
            IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[item];
            foreach (TreeControl.Node node in nodes.ToArray())
                RefreshNode(node);
        }

        /// <summary>
        /// Refreshes all parents of node</summary>
        /// <param name="item">Node whose parents are refreshed</param>
        public void RefreshParents(object item)
        {
            IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[item];
            foreach (TreeControl.Node node in nodes)
            {
                if (node.Parent != null)
                    RefreshNode(node.Parent);
            }
        }

        /// <summary>
        /// Refreshes a node</summary>
        /// <param name="node">Node to be refreshed</param>
        private void RefreshNode(TreeControl.Node node)
        {
            UpdateNode(node);
            if (node.Expanded)
            {
                // get visible paths
                List<object> path = new List<object>();
                HashSet<Path<object>> paths = new HashSet<Path<object>>();
                foreach (TreeControl.Node child in node.Children)
                    AddPaths(child, path, paths);

                node.Expanded = false; // removes all child nodes
                node.Expanded = true;
                foreach (TreeControl.Node child in node.Children)
                    ExpandPaths(child, path, paths);
            }
        }

        private void AddPaths(TreeControl.Node node, List<object> path, HashSet<Path<object>> paths)
        {
            if (node.Expanded)
            {
                path.Add(node.Tag);
                paths.Add(new AdaptablePath<object>(path));

                foreach (TreeControl.Node child in node.Children)
                    AddPaths(child, path, paths);

                path.RemoveAt(path.Count - 1);
            }
        }

        private void ExpandPaths(TreeControl.Node node, List<object> path, HashSet<Path<object>> paths)
        {
            if (node.Tag != null)
            {
                path.Add(node.Tag);
                if (paths.Contains(new AdaptablePath<object>(path)))
                {
                    node.Expanded = true;
                    foreach (TreeControl.Node child in node.Children)
                        ExpandPaths(child, path, paths);
                }

                path.RemoveAt(path.Count - 1);
            }
        }

        /// <summary>
        /// Makes sure the tree nodes corresponding to the path are visible</summary>
        /// <param name="path">Path identifying tree nodes</param>
        /// <param name="select">Whether node should be selected</param>
        /// <returns>The node specified by the path, or null if it could not be found</returns>
        public TreeControl.Node Show(Path<object> path, bool select)
        {
            TreeControl.Node node = ExpandPath(path, false);
            if (node != null)
            {
                if (select)
                {
                    node.Selected = true;
                }
                m_treeControl.EnsureVisible(node);
            }
            return node;
        }

        /// <summary>
        /// Expands and scrolls the node into view that is specified by the given path and
        /// begins a label edit operation. If the path is invalid or the node does not allow
        /// label editing, nothing happens.</summary>
        /// <param name="path">Path identifying a tree node whose label is to be edited</param>
        public void BeginLabelEdit(Path<object> path)
        {
            TreeControl.Node node = Show(path, true);
            m_treeControl.BeginLabelEdit(node);
        }

        /// <summary>
        /// Determines whether or not the first occurrence of the given item is visible in the tree.
        /// It may still be scrolled out of view.</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the given item is visible in the tree</returns>
        public bool IsVisible(object item)
        {
            IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[item];
            foreach (TreeControl.Node node in nodes)
                return true;

            return false;
        }

        /// <summary>
        /// Determines whether or not the node corresponding to the given path is visible.
        /// It may still be scrolled out of view.</summary>
        /// <param name="path">Path to the item</param>
        /// <returns><c>True</c> if the given item is visible in the tree</returns>
        public bool IsVisible(Path<object> path)
        {
            return (ExpandPath(path, true) != null);
        }

        /// <summary>
        /// Determines whether or not the first occurrence of the given item is expanded</summary>
        /// <param name="item">Item</param>
        /// <returns><c>True</c> if the given item is expanded in the tree, or false if the
        /// item cannot be found or is not expanded</returns>
        public bool IsExpanded(object item)
        {
            IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[item];
            foreach (TreeControl.Node node in nodes)
                return node.Expanded;

            return false;
        }

        /// <summary>
        /// Determines whether or not the given item is expanded</summary>
        /// <param name="path">Path to the item</param>
        /// <returns><c>True</c> if the given item is expanded in the tree, or false if the
        /// item cannot be found or is not expanded</returns>
        public bool IsExpanded(Path<object> path)
        {
            TreeControl.Node node = ExpandPath(path, true);
            return (node != null && node.Expanded);
        }

        /// <summary>
        /// Gets the current expanded nodes of the tree</summary>
        /// <returns>Current expanded nodes of the tree</returns>
        public Tree<object> GetExpansion()
        {
            if (m_treeView != null)
            {
                Tree<object> result = new Tree<object>(m_treeView.Root);
                GetExpansion(result, m_treeControl.Root.Children);

                return result;
            }
            return new Tree<object>();
        }

        private void GetExpansion(Tree<object> tree, IEnumerable<TreeControl.Node> nodes)
        {
            foreach (TreeControl.Node node in nodes)
            {
                Tree<object> subTree = new Tree<object>(node.Tag);
                GetExpansion(subTree, node.Children);
                subTree.Parent = tree;
            }
        }

        #region Event Handlers

        private void treeControl_MouseDown(object sender, MouseEventArgs e)
        {
            Point clientPoint = new Point(e.X, e.Y);
            SetLastHit(clientPoint);
        }

        private void treeControl_MouseUp(object sender, MouseEventArgs e)
        {
            Point clientPoint = new Point(e.X, e.Y);
            SetLastHit(clientPoint);
        }

        private void treeControl_DragOver(object sender, DragEventArgs e)
        {
            Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
            SetLastHit(clientPoint);
        }

        private void treeControl_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
            SetLastHit(clientPoint);
        }

        private void treeControl_NodeExpandedChanged(object sender, TreeControl.NodeEventArgs e)
        {
            UpdateNode(e.Node);
            SetChildren(e.Node);
        }

        private void SetChildren(TreeControl.Node parentNode)
        {
            if (m_treeView == null)
                return;

            if (parentNode.Expanded)
            {
                object obj = parentNode.Tag;
                if (obj != null)
                {
                    TreeControl.Node node = null;
                    foreach (object child in m_treeView.GetChildren(obj))
                    {
                        node = parentNode.Add(child);
                        m_itemToNodeMap.Add(child, node);
                        UpdateNode(node);
                    }
                    if (node == null) // no children?
                        parentNode.IsLeaf = true;
                }
            }
            else
            {
                foreach (TreeControl.Node child in parentNode.Children)
                {
                    Unbind(child);
                }

                parentNode.Clear();
            }
        }

        private void treeControl_NodeSelectedChanged(object sender, TreeControl.NodeEventArgs e)
        {
            if (m_selectionContext != null && !m_synchronizingSelection)
            {
                try
                {
                    m_synchronizingSelection = true;

                    Path<object> path = MakePath(e.Node);
                    if (e.Node.Selected)
                        m_selectionContext.AddRange(path);
                    else
                        m_selectionContext.RemoveRange(path);
                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        private void treeControl_SelectionChanging(object sender, EventArgs e)
        {
            m_synchronizingSelection = true;
        }

        private void treeControl_SelectionChanged(object sender, EventArgs e)
        {
            if (m_selectionContext != null)
            {
                List<object> newSelection = new List<object>();
                foreach (TreeControl.Node node in m_treeControl.SelectedNodes)
                    newSelection.Add(MakePath(node));
                m_selectionContext.SetRange(newSelection);
            }

            m_synchronizingSelection = false;
        }

        private void tree_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            if (m_changedParents != null)
            {
                IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[e.Parent];
                foreach (TreeControl.Node node in nodes)
                    node.Expanded = true;

                m_changedParents.Add(e.Parent);
            }
            else
            {
                InsertObject(e.Item, e.Parent, e.Index);
            }
        }

        private void tree_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            if (m_changedParents != null)
            {
                m_changedParents.Add(e.Parent);
            }
            else
            {
                RemoveObject(e.Item);
            }
        }

        private void tree_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            object tree = e.Item;
            IEnumerable<TreeControl.Node> nodes = m_itemToNodeMap[tree];
            foreach (TreeControl.Node node in nodes)
            {
                if (e.Reloaded)
                    RefreshNode(node);
                else
                    UpdateNode(node);
            }
        }

        private void tree_Reloaded(object sender, EventArgs e)
        {
            Load();
        }

        private void validationContext_Beginning(object sender, EventArgs e)
        {
            m_changedParents = new HashSet<object>();
        }

        private void validationContext_Ended(object sender, EventArgs e)
        {
            UpdateChangedParents();
        }

        private void validationContext_Cancelled(object sender, EventArgs e)
        {
            UpdateChangedParents();
        }
        private HashSet<object> m_changedParents;

        private void UpdateChangedParents()
        {
            if (m_changedParents != null)
            {
                foreach (object parent in m_changedParents)
                {
                    Refresh(parent);
                }

                m_changedParents = null;
            }

            // if last hit is no longer in tree, clear it
            if (m_lastHit != null &&
                !m_itemToNodeMap.ContainsKey(m_lastHit))
            {
                SetLastHit(null);
            }
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            if (!m_synchronizingSelection)
            {
                try
                {
                    m_synchronizingSelection = true;

                    m_treeControl.ClearSelection();

                    TreeControl.Node lastSelected = null;
                    foreach (Path<object> path in m_selectionContext.GetSelection<Path<object>>())
                    {
                        // Set the node as selected if the whole path can be found.
                        // Expand as much of 'path' as possible, if m_autoExpand is true.
                        TreeControl.Node node = ExpandPath(path, !m_autoExpand);
                        if (node != null)
                        {
                            lastSelected = node;
                            node.Selected = true;
                        }
                    }

                    if (lastSelected != null)
                        m_treeControl.EnsureVisible(lastSelected);
                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        #endregion

        private void Load()
        {
            Unload();

            if (m_treeView != null)
            {
                object rootObj = m_treeView.Root;
                m_treeControl.Root.Tag = rootObj;

                m_itemToNodeMap.Add(rootObj, m_treeControl.Root);
                UpdateNode(m_treeControl.Root);
                SetChildren(m_treeControl.Root);
            }
        }

        private void Unload()
        {
            m_treeControl.Root.Clear();
            m_treeControl.Root.Tag = null;

            // display nothing
            TreeControl.Node root = m_treeControl.Root;
            root.Label = null;
            root.ImageIndex = -1;
            root.StateImageIndex = -1;

            root.IsLeaf = true;
            root.HasCheck = false;
            root.AllowSelect = false;
            root.AllowLabelEdit = false;

            m_itemToNodeMap.Clear();
        }

        private void InsertObject(object childObj, object parentObj, int index)
        {
            List<TreeControl.Node> parentNodes = new List<TreeControl.Node>(m_itemToNodeMap[parentObj]);
            foreach (TreeControl.Node parentNode in parentNodes)
            {
                if (parentNode.Expanded)
                {
                    TreeControl.Node childNode;
                    if (index >= 0)
                        childNode = parentNode.Insert(index, childObj);
                    else
                        childNode = parentNode.Add(childObj);

                    m_itemToNodeMap.Add(childObj, childNode);
                    UpdateNode(childNode);
                }
                else
                {
                    if (m_autoExpand)
                        parentNode.Expanded = true; // this will cause items to be created
                    else
                        RefreshNode(parentNode); // to make sure that the '+' gets added, for example
                }
            }
        }

        private void RemoveObject(object tree)
        {
            // copy to List since unbind modifies the item-to-node map
            List<TreeControl.Node> nodes = new List<TreeControl.Node>(m_itemToNodeMap[tree]);
            foreach (TreeControl.Node node in nodes)
            {
                Unbind(node);
                node.Parent.Remove(node.Tag);
            }
        }

        private void UpdateNode(TreeControl.Node node)
        {
            ItemInfo info = new WinFormsItemInfo(m_treeControl.ImageList, m_treeControl.StateImageList);
            info.IsExpandedInView = node.Expanded;

            if (m_itemView != null &&
                node.Tag != null)
            {
                m_itemView.GetInfo(node.Tag, info);
            }

            node.Label = info.Label;
            node.FontStyle = info.FontStyle;
            node.ImageIndex = info.ImageIndex;
            node.StateImageIndex = info.StateImageIndex;

            node.IsLeaf = info.IsLeaf;
            node.HasCheck = info.HasCheck;
            node.CheckState = info.GetCheckState();
            node.AllowSelect = info.AllowSelect;
            node.AllowLabelEdit = info.AllowLabelEdit;
            node.HoverText = info.HoverText;

            if (m_selectionContext != null && !m_synchronizingSelection)
            {
                try
                {
                    m_synchronizingSelection = true;

                    node.Selected = m_selectionContext.SelectionContains(MakePath(node));
                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        private void Unbind(TreeControl.Node node)
        {
            if (node.Tag != null)
                m_itemToNodeMap.Remove(node.Tag, node);

            foreach (TreeControl.Node child in node.Children)
                Unbind(child);
        }

        private Path<object> MakePath(TreeControl.Node node)
        {
            List<object> tags = new List<object>();
            for (; node != null; node = node.Parent)
                tags.Add(node.Tag);

            tags.Reverse();
            return new AdaptablePath<object>(tags);
        }

        // expands the node hierarchy for the given path, and if suppressAutoExpand is false,
        //  expands the interior nodes of the path
        private TreeControl.Node ExpandPath(Path<object> path, bool suppressAutoExpand)
        {
            TreeControl.Node current = null;

            if (path[0].Equals(m_treeControl.Root.Tag)) // match root
            {
                current = m_treeControl.Root;
                for (int i = 1; i < path.Count; i++) // match each item in path to a child node
                {
                    TreeControl.Node matched = null;
                    foreach (TreeControl.Node child in current.Children)
                    {
                        if (path[i].Equals(child.Tag))
                        {
                            matched = child;
                            if (i != path.Count - 1)
                            {
                                if (!suppressAutoExpand && !child.Expanded)
                                    child.Expanded = true;
                            }
                            break;
                        }
                    }

                    if (matched == null)
                        return null;

                    current = matched;
                }
            }

            return current;
        }

        private void SetLastHit(Point clientPoint)
        {
            object lastHit = GetItemAt(clientPoint);
            if (lastHit == null)
                lastHit = TreeView;

            SetLastHit(lastHit);
        }

        private void SetLastHit(object lastHit)
        {
            if (!object.Equals(lastHit, m_lastHit))
            {
                m_lastHit = lastHit;
                OnLastHitChanged(EventArgs.Empty);
            }
        }

        private readonly TreeControl m_treeControl;
        private ITreeView m_treeView;
        private IItemView m_itemView;
        private IValidationContext m_validationContext;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;

        private readonly Multimap<object, TreeControl.Node> m_itemToNodeMap;

        private object m_lastHit;

        private bool m_autoExpand = true;
        private bool m_synchronizingSelection;
    }
}
