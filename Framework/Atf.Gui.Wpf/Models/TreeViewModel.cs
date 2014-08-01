//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Models;
using Sce.Atf.Wpf.Applications;



namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for a TreeView</summary>
    public class TreeViewModel : AdapterViewModel
    {
        /// <summary>
        /// Constructor with a null Adaptee. Use the TreeView property to set the Adaptee.</summary>
        public TreeViewModel()
        {
        }

        /// <summary>
        /// Constructor with adaptee</summary>
        /// <param name="adaptee">Object that is adapted. The adaptee should implement ITreeView.</param>
        public TreeViewModel(object adaptee)
            : base(adaptee)
        {
            TreeView = adaptee as ITreeView;
        }

        /// <summary>
        /// Gets or sets the tree displayed in the control. When setting, consider having the
        /// ITreeView object also implement IItemView, IObservableContext, IValidationContext,
        /// ISelectionContext, IInstancingContext, and IHierarchicalInsertionContext.</summary>
        public ITreeView TreeView
        {
            get { return m_treeView; }
            set
            {
                if (m_treeView != value)
                {
                    Adaptee = value;

                    if (m_treeView != null)
                    {
                        m_itemView = null;

                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted -= new EventHandler<ItemInsertedEventArgs<object>>(tree_ItemInserted);
                            m_observableContext.ItemRemoved -= new EventHandler<ItemRemovedEventArgs<object>>(tree_ItemRemoved);
                            m_observableContext.ItemChanged -= new EventHandler<ItemChangedEventArgs<object>>(tree_ItemChanged);
                            m_observableContext.Reloaded -= new EventHandler(tree_Reloaded);
                            m_observableContext = null;
                        }

                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning -= new EventHandler(validationContext_Beginning);
                            m_validationContext.Ended -= new EventHandler(validationContext_Ended);
                            m_validationContext.Cancelled -= new EventHandler(validationContext_Cancelled);
                            m_validationContext = null;
                        }

                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanging -= new EventHandler(selection_Changing);
                            m_selectionContext.SelectionChanged -= new EventHandler(selection_Changed);
                            m_selectionContext = null;
                        }

                        if (m_labelEditingContext != null)
                        {
                            m_labelEditingContext.BeginLabelEdit -= new EventHandler<BeginLabelEditEventArgs>(labelEditingContext_BeginLabelEdit);
                        }
                    }

                    m_treeView = value;

                    if (m_treeView != null)
                    {
                        m_itemView = Adapters.As<IItemView>(m_treeView);

                        m_observableContext = Adapters.As<IObservableContext>(m_treeView);
                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted += new EventHandler<ItemInsertedEventArgs<object>>(tree_ItemInserted);
                            m_observableContext.ItemRemoved += new EventHandler<ItemRemovedEventArgs<object>>(tree_ItemRemoved);
                            m_observableContext.ItemChanged += new EventHandler<ItemChangedEventArgs<object>>(tree_ItemChanged);
                            m_observableContext.Reloaded += new EventHandler(tree_Reloaded);
                        }

                        m_validationContext = Adapters.As<IValidationContext>(m_treeView);
                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning += new EventHandler(validationContext_Beginning);
                            m_validationContext.Ended += new EventHandler(validationContext_Ended);
                            m_validationContext.Cancelled += new EventHandler(validationContext_Cancelled);
                        }

                        m_selectionContext = Adapters.As<ISelectionContext>(m_treeView);
                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanging += new EventHandler(selection_Changing);
                            m_selectionContext.SelectionChanged += new EventHandler(selection_Changed);
                        }

                        m_labelEditingContext = Adapters.As<ILabelEditingContext>(m_treeView);
                        if (m_labelEditingContext != null)
                        {
                            m_labelEditingContext.BeginLabelEdit += new EventHandler<BeginLabelEditEventArgs>(labelEditingContext_BeginLabelEdit);
                        }
                    }
                }

                Load();
            }
        }

        /// <summary>
        /// Gets or sets whether tree nodes are automatically
        /// expanded to show descendant nodes that are selected or inserted</summary>
        public AutoExpandMode AutoExpand
        {
            get { return m_autoExpand; }
            set { m_autoExpand = value; }
        }
        private AutoExpandMode m_autoExpand = AutoExpandMode.Default;

        /// <summary>
        /// Gets or sets whether multi select is enabled. Default is True.</summary>
        public bool MultiSelectEnabled
        {
            get { return m_multiSelectEnabled; }
            set { m_multiSelectEnabled = value; }
        }
        private bool m_multiSelectEnabled = true;

        /// <summary>
        /// Gets or sets whether to display the root node</summary>
        public bool ShowRoot
        {
            get { return m_showRoot; }
            set
            {
                if (m_showRoot != value)
                {
                    if(m_selectionContext != null)
                        m_selectionContext.Clear();

                    m_showRoot = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
                }
            }
        }

        private bool m_showRoot = true;

        #region SynchronisingSelection Property

        /// <summary>
        /// Gets or sets whether selection is being synchronized</summary>
        public bool SynchronisingSelection
        {
            get { return m_synchronisingSelection; }
            set
            {
                if (m_synchronisingSelection != value)
                {
                    m_synchronisingSelection = value;
                    OnPropertyChanged(s_synchronisingSelectionArgs);
                    SynchronisingSelectionChanged.Raise(this, EventArgs.Empty);
                    //System.Diagnostics.Debug.WriteLine(Label + " Selected=" + value);
                }
            }
        }

        private bool m_synchronisingSelection;
        private static readonly PropertyChangedEventArgs s_synchronisingSelectionArgs
            = ObservableUtil.CreateArgs<TreeViewModel>(x => x.SynchronisingSelection);

        internal event EventHandler SynchronisingSelectionChanged;

        #endregion

        /// <summary>
        /// Gets the root's children</summary>
        public IEnumerable<Node> Roots
        {
            get
            {
                if (Root == null)
                    return EmptyEnumerable<Node>.Instance;

                if (ShowRoot)
                    return new Node[]{Root};
                
                return Root.Children;
            }
        }

        /// <summary>
        /// Gets the root node</summary>
        public Node Root { get; private set; }

        /// <summary>
        /// Gets the visible path. For internal WPF binding only.</summary>
        public Path<Node> EnsureVisiblePath
        {
            get { return m_ensureVisiblePath; }
            private set
            {
                if (m_ensureVisiblePath != value)
                {
                    m_ensureVisiblePath = value;

                    // Special case when not displaying the root node
                    // need to trim this from start of the path so as not to
                    // mess up the tree view binding
                    if (m_ensureVisiblePath != null
                        && !m_showRoot
                        && m_ensureVisiblePath.First == Root
                        && m_ensureVisiblePath.Count > 1)
                    {
                        m_ensureVisiblePath = m_ensureVisiblePath.Suffix(m_ensureVisiblePath.Count - 1);
                    }

                    OnPropertyChanged(s_ensureVisiblePathArgs);
                }
            }
        }

        private Path<Node> m_ensureVisiblePath;
        private static readonly PropertyChangedEventArgs s_ensureVisiblePathArgs
            = ObservableUtil.CreateArgs<TreeViewModel>(x => x.EnsureVisiblePath);

        /// <summary>
        /// Gets or sets whether selection transaction occurring. For internal WPF binding only.</summary>
        public bool InSelectionTransaction
        {
            get { return m_selectionChangedNodes != null; }
            set
            {
                if (!value && m_selectionChangedNodes != null)
                {
                    RefreshSelection();
                    m_selectionChangedNodes = null;
                }
                else if (value && m_selectionChangedNodes == null)
                {
                    m_selectionChangedNodes = new HashSet<Node>();
                }
            }
        }

        #region Public Methods

        /// <summary>
        /// Refreshes the subtrees for any tree nodes corresponding to the given item. The current
        /// expansion of each node is preserved if possible.</summary>
        /// <param name="item">Item which is to be refreshed</param>
        public void Refresh(object item)
        {
            foreach (var node in m_itemToNodesMap[item])
            {
                RefreshNode(node);
            }
        }

        /// <summary>
        /// Refreshes parents of given node</summary>
        /// <param name="item">Node whose parents are refreshed</param>
        public void RefreshParents(object item)
        {
            foreach (var node in m_itemToNodesMap[item])
            {
                if (node.Parent != null)
                    RefreshNode(node.Parent);
            }
        }

        /// <summary>
        /// Expands tree to first leaf node in tree.</summary>
        public Node ExpandToFirstLeaf()
        {
            Node node = Root;
            if (node == null)
                return null;

            Node last;
            do
            {
                last = node;
                last.Expanded = true;
                foreach (Node child in node.Children)
                {
                    node = child;
                    break;
                }
            }
            while (last != node);

            return node;
        }

        /// <summary>
        /// Makes sure the tree nodes corresponding to the path are visible (by expanding their parents)</summary>
        /// <param name="path">Path, identifying tree nodes</param>
        /// <param name="select">Value indicating whether node should be selected</param>
        /// <returns>The node specified by the path or null if it could not be found.</returns>
        public Node Show(Path<object> path, bool select)
        {
            Node node = null;

            Path<Node> nodePath = ExpandPath(path);

            if (nodePath != null)
            {
                node = nodePath.Last;

                if (select)
                {
                    node.IsSelected = true;
                }

                EnsureVisiblePath = nodePath;
            }

            return node;
        }

        /// <summary>
        /// Attempts to show the specified item</summary>
        /// <param name="item">Path identifying tree nodes</param>
        /// <param name="select">Whether node specified by the path should be selected</param>
        /// <returns>The node specified by the path or null if it could not be found</returns>
        public Node Show(object item, bool select)
        {
            Node node = GetNode(item);

            if(node != null)
            {
                Path<object> path = MakePath(node);
                Show(path, select);
            }

            return node;
        }

        /// <summary>
        /// Expands all nodes in the tree</summary>
        public void ExpandAll()
        {
            foreach (var node in GetAllNodesInTree())
                node.Expanded = true;
        }

        /// <summary>
        /// Collapses all nodes in the tree</summary>
        public void CollapseAll()
        {
            try
            {
                m_synchronizingSelection = true;
                if (Root != null)
                {
                    // We want all nodes to be collapsed so that when the user expands a node
                    // the child nodes are not expanded.
                    foreach (var node in GetAllNodesInTree())
                        node.Expanded = false;
                }
            }
            finally
            {
                m_synchronizingSelection = false;
            }
        }

        /// <summary>
        /// Gets the enumeration of expanded nodes, in a depth-first traversal. The root node will be included, even if it's
        /// not visible. Leaf nodes may or may not be considered expanded.</summary>
        /// <returns></returns>
        public IEnumerable<object> GetExpandedItems()
        {
            if (m_treeView != null)
            {
                Stack<object> items = new Stack<object>();
                items.Push(m_treeView.Root);

                while (items.Count > 0)
                {
                    object item = items.Pop();
                    foreach (var node in m_itemToNodesMap[item])
                    {
                        if (node.Expanded)
                        {
                            yield return item;
                            foreach (var child in m_treeView.GetChildren(item))
                                items.Push(child);
                        }
                    }
                }
            }
        }

        public void Expand(object item)
        {
            foreach (var node in m_itemToNodesMap[item])
            {
                node.Expanded = true;
            }
        }

        public void Expand(IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                Expand(item);
            }
        }

        #endregion

        /// <summary>
        /// Raises AdapteeChanged event and performs custom actions after the adapted object has been set</summary>
        /// <param name="oldAdaptee">Previous adaptee reference</param>
        protected override void OnAdapteeChanged(object oldAdaptee)
        {
            base.OnAdapteeChanged(oldAdaptee);
            TreeView = Adaptee as ITreeView;
        }

        #region Private Methods

        private void RefreshNode(Node node)
        {
            // Sync any added/removed children
            if (node.ChildrenInternal != null)
            {
                // get visible paths
                List<object> path = new List<object>();
                HashSet<Path<object>> paths = new HashSet<Path<object>>();
                foreach (Node child in node.ChildrenInternal)
                    AddPaths(child, path, paths);

                // Removes all child nodes efficiently
                RemoveChildren(node);

                // Rebuild nodes
                foreach(object child in TreeView.GetChildren(node.Adaptee))
                {
                    InsertObject(child, node.Adaptee, -1);
                }

                foreach (Node child in node.ChildrenInternal)
                    ExpandPaths(child, path, paths);
            }

            // Update node properties recursively
            UpdateNodeSubTree(node);
        }

        private void AddPaths(Node node, List<object> path, HashSet<Path<object>> paths)
        {
            if (node.Expanded)
            {
                path.Add(node);
                paths.Add(new AdaptablePath<object>(path));

                foreach (Node child in node.Children)
                    AddPaths(child, path, paths);

                path.RemoveAt(path.Count - 1);
            }
        }

        private void ExpandPaths(Node node, List<object> path, HashSet<Path<object>> paths)
        {
            path.Add(node);
            if (paths.Contains(new AdaptablePath<object>(path)))
            {
                node.Expanded = true;
                foreach (Node child in node.ChildrenInternal)
                    ExpandPaths(child, path, paths);
            }

            path.RemoveAt(path.Count - 1);
        }

        internal ObservableCollection<Node> CreateChildren(Node treeNodeViewModel)
        {
            var result = new ObservableCollection<Node>();
            foreach (var child in TreeView.GetChildren(treeNodeViewModel.Adaptee))
            {
                Node node = CreateNode(child, treeNodeViewModel);
                if(node != null)
                    result.Add(node);
            }
            return result;
        }

        private Node CreateNode(object adaptee, Node parent)
        {
            var node = m_itemToNodesMap[adaptee].FirstOrDefault(n => n.Parent == parent);
            if (node != null)
                return node;

            node = new Node(adaptee, this, parent);
            node.IsSelectedChanged += node_IsSelectedChanged;
            m_itemToNodesMap.Add(adaptee, node);
            UpdateNode(node); //to-do: make the caller do this. We're not calling UpdateNode on the cached version above.

            return node;
        }

        private void Load()
        {
            Unload();

            if (m_treeView != null)
            {
                object rootObj = m_treeView.Root;
                if (rootObj != null)
                {
                    Root = CreateNode(rootObj, null);
                    Root.Expanded = true;
                    UpdateNode(Root);
                }
                else
                {
                    Root = null;
                }
                OnPropertyChanged(new PropertyChangedEventArgs("Root"));
                OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
            }
        }

        private void Unload()
        {
            EnsureVisiblePath = null;
            Root = null;
            m_itemToNodesMap.Clear();
            m_previousSelection = new Path<object>[] { };
            OnPropertyChanged(new PropertyChangedEventArgs("Root"));
            OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
            OnPropertyChanged(new PropertyChangedEventArgs("EnsureVisiblePath"));
        }

        private void InsertObject(object child, object parent, int index)
        {
            foreach (var parentNode in m_itemToNodesMap[parent])
            {
                if (parentNode.ChildrenInternal != null)
                {
                    Node childNode = CreateNode(child, parentNode);
                    if (childNode != null)
                    {
                        if (index >= 0)
                            parentNode.ChildrenInternal.Insert(index, childNode);
                        else
                            parentNode.ChildrenInternal.Add(childNode);
                    }
                }

                if (!parentNode.Expanded)
                {
                    if (((m_autoExpand & AutoExpandMode.ExpandInsertedIfParentSelected) > 0)
                        && parentNode.IsSelected)
                        parentNode.Expanded = true;
                    else if ((m_autoExpand & AutoExpandMode.ExpandInserted) > 0)
                        parentNode.Expanded = true;
                }
            }

            // Else if node does not exist in map then it has not yet been generated
        }

        private void RemoveObject(object tree)
        {
            foreach (var node in m_itemToNodesMap[tree])
            {
                var pathsToRemove = new HashSet<Path<object>>();

                foreach (var item in GetSubtree(node))
                {
                    m_itemToNodesMap.Remove(item);
                    pathsToRemove.Add(MakePath(item));
                }

                node.Parent.ChildrenInternal.Remove(node);

                try
                {
                    m_synchronizingSelection = true;
                    if (m_selectionContext != null)
                        m_selectionContext.RemoveRange(pathsToRemove);

                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        // Remove all children as atomic operation
        private void RemoveChildren(Node node)
        {
            var pathsToRemove = new HashSet<Path<object>>();

            // copy to List since unbind modifies the item-to-node map
            foreach (Node child in node.ChildrenInternal)
            {
                foreach (var item in GetSubtree(child))
                {
                    m_itemToNodesMap.Remove(item);
                    pathsToRemove.Add(MakePath(child));
                }
            }
            
            node.ChildrenInternal.Clear();

            try
            {
                m_synchronizingSelection = true;
                if (m_selectionContext != null)
                    m_selectionContext.RemoveRange(pathsToRemove);
            }
            finally
            {
                m_synchronizingSelection = false;
            }
        }

        private void UpdateNode(Node node)
        {
            if (m_itemView != null)
            {
                m_itemView.GetInfo(node.Adaptee, node.ItemInfo);
                OnNodeInfoUpdated(node);
                node.ItemInfoChanged();
            }

            if (m_selectionContext != null && !m_synchronizingSelection)
            {
                try
                {
                    m_synchronizingSelection = true;

                    node.IsSelected = m_selectionContext.SelectionContains(MakePath(node));
                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        private void UpdateNodeSubTree(Node node)
        {
            UpdateNode(node);
            if (node.Expanded)
            {
                foreach (Node child in node.Children)
                    UpdateNodeSubTree(child);
            }
        }

        /// <summary>
        /// This method is called after each GetInfo(), which updates a node's ItemInfo based on its Adaptee.</summary>
        /// <param name="node"></param>
        /// <remarks>Derived classes may want to further modify the ItemInfo of the node, before it responds to those modifications.
        /// Override method to do so.</remarks>
        protected virtual void OnNodeInfoUpdated(Node node)
        {
        }

        private void UpdateChangedParents()
        {
            if (m_parentsWithRemovedChildren != null)
            {
                foreach (object parent in m_parentsWithRemovedChildren)
                {
                    foreach (var node in m_itemToNodesMap[parent])
                    {
                        RefreshNode(node);
                    }
                }

                foreach (object parent in m_parentsWithAddedChildren)
                {
                    foreach (var node in m_itemToNodesMap[parent])
                    {
                        // Pre expand parents if required
                        if (!node.Expanded)
                        {
                            if (((m_autoExpand & AutoExpandMode.ExpandInsertedIfParentSelected) > 0)
                                && node.IsSelected)
                                node.Expanded = true;
                            else if ((m_autoExpand & AutoExpandMode.ExpandInserted) > 0)
                                node.Expanded = true;
                        }

                        RefreshNode(node);
                    }
                }

                m_parentsWithRemovedChildren = null;
                m_parentsWithAddedChildren = null;
            }

            // if last hit is no longer in tree, clear it
            //if (m_lastHit != null &&
            //    !m_itemToNodesMap.ContainsKey(m_lastHit))
            //{
            //    SetLastHit(null);
            //}
        }

        private IEnumerable<Node> GetSubtree(Node node)
        {
            if (node.ChildrenInternal == null || node.ChildrenInternal.Count == 0)
            {
                yield return node;
                yield break;
            }

            var nodes = new Queue<Node>();
            nodes.Enqueue(node);
            while (nodes.Count > 0)
            {
                var item = nodes.Dequeue();
                yield return item;

                if (item.ChildrenInternal != null)
                {
                    foreach (var child in item.ChildrenInternal)
                    {
                        nodes.Enqueue(child);
                    }
                }
            }
        }

        private Path<Node> ExpandPath(Path<object> path)
        {
            Path<Node> result = null;

            if (path[0].Equals(Root.Adaptee)) // match root
            {
                Node current = Root;

                result = new Path<Node>(current);

                if (path.Count > 0)
                    current.Expanded = true;

                for (int i = 1; i < path.Count; i++) // match each item in path to a child node
                {
                    Node matched = null;
                    foreach (Node child in current.Children) // This forces child creation
                    {
                        if (path[i].Equals(child.Adaptee))
                        {
                            matched = child;
                            if (i != path.Count - 1)
                            {
                                child.Expanded = true;
                            }
                            break;
                        }
                    }

                    if (matched == null)
                        return null;

                    current = matched;
                    result += current;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all nodes associated with a given item.
        /// If none found, all nodes in the tree will be generated</summary>
        /// <param name="item">Object with which our desired Node(s) are associated</param>
        /// <returns>The list of Nodes associated with the specified object, or an empty enumerable</returns>
        private IEnumerable<Node> GetNodes(object item)
        {
            if (!m_itemToNodesMap[item].Any())
            {
            // Node not created - have to force creation of all children in tree
            // until we find the item we are looking for!
                GetAllNodesInTree();
            }

            return m_itemToNodesMap[item];
        }

        /// <summary>
        /// Searches for a specific node, associated with the specified object
        /// If not found, all nodes in the tree may be created.
        /// If more than one node was found, the first will be returned.</summary>
        /// <param name="item">Node to search for</param>
        /// <returns>Node searched for, or null if not found</returns>
        private Node GetNode(object item)
        {
            return GetNodes(item).FirstOrDefault();
        }

        /// <summary>
        /// Gets entire tree of nodes and forces creation of all</summary>
        /// <returns>Enumeration of all nodes in tree</returns>
        private IEnumerable<Node> GetAllNodesInTree()
        {
            if (Root != null)
            {
                Stack<Node> nodes = new Stack<Node>();
                nodes.Push(Root);

                while (nodes.Count > 0)
                {
                    Node node = nodes.Pop();
                    yield return node;

                    // Force creation of all children
                    foreach (var child in node.Children)
                        nodes.Push(child);
                }
            }
        }

        private static Path<object> MakePath(Node node)
        {
            return new AdaptablePath<object>(GetLineage(node).Reverse().Select(x => x.Adaptee));
        }

        private static IEnumerable<Node> GetLineage(Node node)
        {
            for (; node != null; node = node.Parent)
                yield return node;
        }

        private static void Swap(ref Node node1, ref Node node2)
        {
            Node temp = node1;
            node1 = node2;
            node2 = temp;
        }

        private static Node GetRoot(Node node)
        {
            while (node.Parent != null)
                node = node.Parent;
            return node;
        }

        #endregion

        #region Event Handlers

        private void tree_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            if (e.Parent == null)
                return;

            if (m_parentsWithAddedChildren != null)
            {
                foreach (var parentNode in m_itemToNodesMap[e.Parent])
                {
                    if (!parentNode.Expanded)
                    {
                        if (((m_autoExpand & AutoExpandMode.ExpandInsertedIfParentSelected) > 0)
                            && parentNode.IsSelected)
                            parentNode.Expanded = true;
                        else if ((m_autoExpand & AutoExpandMode.ExpandInserted) > 0)
                            parentNode.Expanded = true;
                    }

                    m_parentsWithAddedChildren.Add(e.Parent);
                }
                // TODO: what if auto expand is true but parent is not yet created?
            }
            else
            {
                InsertObject(e.Item, e.Parent, e.Index);
            }
        }

        private void tree_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            if (m_parentsWithRemovedChildren != null && e.Parent != null)
            {
                m_parentsWithRemovedChildren.Add(e.Parent);
            }
            else
            {
                RemoveObject(e.Item);
            }
        }

        private void tree_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            foreach (var node in m_itemToNodesMap[e.Item])
            {
                UpdateNode(node);
            }
        }

        private void tree_Reloaded(object sender, EventArgs e)
        {
            Load();
        }

        private void validationContext_Beginning(object sender, EventArgs e)
        {
            m_parentsWithRemovedChildren = new HashSet<object>();
            m_parentsWithAddedChildren = new HashSet<object>();
        }

        private void validationContext_Ended(object sender, EventArgs e)
        {
            UpdateChangedParents();
        }

        private void validationContext_Cancelled(object sender, EventArgs e)
        {
            UpdateChangedParents();
        }

        private void selection_Changing(object sender, EventArgs e)
        {
            m_previousSelection = m_selectionContext.GetSelection<Path<object>>().ToArray();
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            if (!m_synchronizingSelection)
            {
                try
                {
                    m_synchronizingSelection = true;
                    DeselectPreviousSelection();
                    SelectCurrentSelection();
                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        private void labelEditingContext_BeginLabelEdit(object sender, BeginLabelEditEventArgs e)
        {
            object item = e.Item;
            var path = item as Path<object>;
            if (path != null)
            {
                item = path.Last;
            }

            // Issue here as item may be adapter

            foreach (var node in m_itemToNodesMap[item])
            {
                node.IsInLabelEditMode = true;
            }
        }

        private void node_IsSelectedChanged(object sender, EventArgs e)
        {
            if (m_selectionContext != null && !m_synchronizingSelection)
            {
                if(m_selectionChangedNodes != null)
                {
                    m_selectionChangedNodes.Add((Node)sender);
                }
                else
                {
                    try
                    {
                        m_synchronizingSelection = true;

                        var node = (Node)sender;
                        Path<object> path = MakePath(node);
                        if (node.IsSelected)
                        {
                            if (!m_multiSelectEnabled)
                                DeselectAll();

                            m_selectionContext.Add(path);
                        }
                        else
                        {
                            m_selectionContext.Remove(path);
                        }
                    }
                    finally
                    {
                        m_synchronizingSelection = false;
                    }
                }
            }
        }

        // Called after an atomic selection operation is completed
        private void RefreshSelection()
        {
            if (m_selectionContext != null &&
                m_selectionChangedNodes != null &&
                m_selectionChangedNodes.Count > 0)
            {
                try
                {
                    m_synchronizingSelection = true;

                    if (!m_multiSelectEnabled)
                    {
                        var lastSelected = m_selectionChangedNodes.LastOrDefault(x => x.IsSelected);

                        // Deselect all other nodes
                        foreach (var node in m_selectionChangedNodes)
                        {
                            if (node != lastSelected)
                            {
                                var path = MakePath(node);
                                m_selectionContext.Remove(path);
                            }
                        }

                        // Select
                        var selectedPath = MakePath(lastSelected);
                        m_selectionContext.Add(selectedPath);
                    }
                    else
                    {
                        var select = new List<Path<object>>();
                        var unselect = new List<Path<object>>();
                        foreach (Node node in m_selectionChangedNodes)
                        {
                            if (node.IsSelected)
                                select.Add(MakePath(node));
                            else
                                unselect.Add(MakePath(node));
                        }
                        m_selectionContext.RemoveRange(unselect);
                        m_selectionContext.AddRange(select);
                    }
                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        private void DeselectAll()
        {
            if (m_selectionContext == null)
                return;

            var pathsToRemove = new List<Path<object>>();
            foreach (Path<object> selectedPath in m_selectionContext.GetSelection<Path<object>>().ToArray())
            {
                foreach (var selectedNode in m_itemToNodesMap[selectedPath.Last])
                {
                    pathsToRemove.Add(selectedPath);
                    selectedNode.IsSelected = false;
                }
            }
            m_selectionContext.RemoveRange(pathsToRemove);
        }

        private void DeselectPreviousSelection()
        {
            //foreach (var node in m_itemToNodesMap.Values)
            //    node.IsSelected = false;
            // Deselect the previous selection
            foreach (Path<object> path in m_previousSelection)
            {
                foreach (var node in m_itemToNodesMap[path.Last])
                {
                    //System.Diagnostics.Debug.Assert(node.IsSelected);
                    node.IsSelected = false;
                }
            }
        }

        private void SelectCurrentSelection()
        {
            if (m_selectionContext == null || m_selectionContext.SelectionCount == 0)
                return;

            Path<Node> lastNodePath = null;
            Path<object>[] selectedPaths = m_selectionContext.GetSelection<Path<object>>().ToArray();
            if (selectedPaths.Length == 0)
            {
                // If no recognised paths were selected, this could be 
                // a selection set externally of raw objects (not paths)
                var selectedNodes = new HashSet<Node>();
                object[] selectedObjects = m_selectionContext.GetSelection<object>().ToArray();
                foreach (object selected in selectedObjects)
                {
                    // Check each selected object to see if has a mapped tree node
                    foreach (var node in m_itemToNodesMap[selected])
                    {
                        // If so then store it and remove from selection context
                        selectedNodes.Add(node);
                        m_selectionContext.Remove(selected);
                    }
                }

                if(selectedNodes.Count > 0)
                {
                    try
                    {
                        // Force a selection transaction and select all mapped
                        // nodes, this will add them back into the selection context
                        InSelectionTransaction = true;
                        foreach (Node node in selectedNodes)
                        {
                            node.IsSelected = true;
                            m_selectionChangedNodes.Add(node);
                        }
                    }
                    finally
                    {
                        InSelectionTransaction = false;
                    }
                }
            }
            else
            {
                // If the selection contains valid paths then just deal with them directly
                foreach (Path<object> path in selectedPaths)
                {
                    // Expand as much of 'path' as possible, if m_autoExpand is true.
                    var nodes = new List<Node>();

                    if ((m_autoExpand & AutoExpandMode.ExpandSelected) > 0)
                    {
                        lastNodePath = ExpandPath(path);
                        if (lastNodePath != null)
                            nodes.Add(lastNodePath.Last);
                    }
                    else
                    {
                        nodes.AddRange(m_itemToNodesMap[path.Last]);
                    }

                    foreach (var node in nodes)
                    {
                        node.IsSelected = true;
                    }

                    if (!m_multiSelectEnabled)
                        break;
                }

                if (lastNodePath != null
                    && ((m_autoExpand & AutoExpandMode.ExpandSelected) > 0))
                {
                    EnsureVisiblePath = lastNodePath;
                }
            }
        }

        #endregion

        protected ISelectionContext SelectionContext { get { return m_selectionContext; } }

        private ITreeView m_treeView;
        private IItemView m_itemView;
        private ILabelEditingContext m_labelEditingContext;
        private IValidationContext m_validationContext;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;
        private bool m_synchronizingSelection;
        private HashSet<Node> m_selectionChangedNodes;
        private HashSet<object> m_parentsWithRemovedChildren;
        private HashSet<object> m_parentsWithAddedChildren;
        private Path<object>[] m_previousSelection;
        private readonly Multimap<object, Node> m_itemToNodesMap = new Multimap<object, Node>(null);
    }

    public class Node : AdapterViewModel
    {
        public Node(object adaptee, TreeViewModel owner, Node parent)
            : base(adaptee)
        {
            m_owner = owner;
            Parent = parent;
        }

        public Node Parent { get; private set; }

        public IEnumerable<Node> Children
        { 
            get { return m_children ?? (m_children = m_owner.CreateChildren(this)); }
        }

        public string Label
        {
            get { return m_itemInfo.Label; }
            set
            {
                var context = m_owner.As<ILabelEditingContext>();
                if (context == null || !context.CanEditLabel(Adaptee))
                    return;

                var transactionContext = m_owner.As<ITransactionContext>();
                transactionContext.DoTransaction(() => context.SetLabel(Adaptee, value), "Edit Label".Localize());
            }
        }

        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsSelected"));
                    IsSelectedChanged.Raise(this, EventArgs.Empty);
                }
            }
        }


        internal event EventHandler IsSelectedChanged;

        public bool Expanded
        {
            get { return m_itemInfo.IsExpandedInView; }
            set
            {
                if (m_itemInfo.IsExpandedInView != value)
                {
                    m_itemInfo.IsExpandedInView = value;

                    if (value)
                    {
                        // TODO: lazy loading
                        m_children = m_owner.CreateChildren(this);
                    }

                    OnPropertyChanged(new PropertyChangedEventArgs("Expanded"));
                }
            }
        }

        public bool IsLeaf
        {
            get { return m_itemInfo.IsLeaf; }
        }

        /// <summary>
        /// Gets or sets whether label edit mode is active</summary>
        public bool IsInLabelEditMode
        {
            get { return m_isInLabelEditMode; }
            set
            {
                if (m_itemInfo.AllowLabelEdit)
                {
                    m_isInLabelEditMode = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsInLabelEditMode"));
                }
            }
        }

        #region ImageKey Property

        /// <summary>
        /// Gets the resource key for associated node image</summary>
        public object ImageKey
        {
            get { return m_itemInfo.ImageKey; }
        }

        private static readonly PropertyChangedEventArgs s_imageKeyArgs
            = ObservableUtil.CreateArgs<Node>(x => x.ImageKey);

        #endregion

        #region StateImageKey Property

        /// <summary>
        /// Gets the resource key for associated node state image</summary>
        public object StateImageKey
        {
            get { return m_itemInfo.StateImageKey; }
        }

        private static readonly PropertyChangedEventArgs s_stateImageKeyArgs
            = ObservableUtil.CreateArgs<Node>(x => x.StateImageKey);

        #endregion

        #region FontWeight Property

        /// <summary>
        /// Gets or sets node's label</summary>
        /// <remarks>Default is empty string if node has no label</remarks>
        public FontWeight FontWeight
        {
            get { return ItemInfo.FontWeight; }
        }

        #endregion

        #region FontItalicsStyle Property

        /// <summary>
        /// Gets or sets node's label</summary>
        /// <remarks>Default is empty string if node has no label</remarks>
        public FontStyle FontItalicStyle
        {
            get { return ItemInfo.FontItalicStyle; }
        }

        #endregion

        #region HoverText Property

        /// <summary>
        /// Gets or sets node's label</summary>
        /// <remarks>Default is empty string if node has no label</remarks>
        public string HoverText
        {
            get { return m_itemInfo.HoverText; }
            set
            {
                if (m_itemInfo.HoverText == value)
                    return;

                m_itemInfo.HoverText = value;
                OnPropertyChanged(s_hoverTextArgs);
                HoverTextChanged.Raise(this, EventArgs.Empty);
                //System.Diagnostics.Debug.WriteLine(Label + " HoverText=" + value);
            }
        }

        private static readonly PropertyChangedEventArgs s_hoverTextArgs
            = ObservableUtil.CreateArgs<Node>(x => x.HoverText);

        internal event EventHandler HoverTextChanged;

        #endregion

        public bool HasCheck
        {
            get { return m_itemInfo.HasCheck; }
        }

        public bool? CheckState
        {
            get { return m_itemInfo.CheckState; }
            set
            {
                m_itemInfo.CheckState = value;

                var context = m_owner.As<ICheckableItemView>();
                if (context == null)
                    return;

                var transactionContext = m_owner.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        context.SetIsChecked(Adaptee, value);
                    }, Localizer.Localize("Check/Uncheck"));
            }
        }

        public bool IsEnabled
        {
            get { return m_itemInfo.IsEnabled; }
        }
        
        public int Index
        {
            get
            {
                if (Parent == null)
                    return 0;
                return Parent.ChildrenInternal.IndexOf(this);
            }
        }

        /// <summary>
        /// Gets whether this Node should be visible to the user, unless this is the root node,
        /// in which case TreeViewModel.ShowRoot must also be 'true' in order to make this node visible.</summary>
        public bool IsVisible
        {
            get
            {
                if (this == m_owner.Root && !m_owner.ShowRoot)
                    return false;

                if (string.IsNullOrEmpty(m_filterString))
                    return m_itemInfo.IsVisible;

                if (!m_isFilterCached)
                {
                    if (m_itemInfo.IsVisible && Label.ToUpperInvariant().Contains(m_filterString.ToUpperInvariant()))
                    {
                        m_isFiltered = false;
                    }
                    else
                    {
                        m_isFiltered = !Children.Any(node => node.IsVisible);
                    }
                }

                return !m_isFiltered;
            }
        }

        public void ResetVisibilityFilter(string filter, bool isNewFilterASubstring, bool isOldFilterASubstring)
        {
            if (m_isFilterCached)
            {
                if (isNewFilterASubstring && m_isFiltered)
                {
                    m_isFilterCached = false;
                }
                else if (isOldFilterASubstring && !m_isFiltered)
                {
                    m_isFilterCached = false;
                }
                else if (!isOldFilterASubstring && !isNewFilterASubstring)
                {
                    m_isFilterCached = false;
                }
            }

            m_filterString = filter;
            OnPropertyChanged(new PropertyChangedEventArgs("IsVisible"));
        }

        public WpfItemInfo ItemInfo { get { return m_itemInfo; } }

        internal void ItemInfoChanged()
        {
            OnPropertyChanged(ObservableUtil.AllChangedEventArgs);
        }

        internal IList<Node> ChildrenInternal { get { return m_children; } }

        private readonly WpfItemInfo m_itemInfo = new WpfItemInfo();
        private readonly TreeViewModel m_owner;
        private ObservableCollection<Node> m_children;
        private bool m_isFiltered;
        private bool m_isFilterCached;
        private string m_filterString;
        private bool m_isSelected;
        private bool m_isInLabelEditMode;
    }

    /// <summary>
    /// Context which has checkable items
    /// </summary>
    public interface ICheckableItemView
    {
        bool? GetIsChecked(object item);

        void SetIsChecked(object item, bool? value);
    }

    [Flags]
    public enum AutoExpandMode : byte
    {
        /// <summary>
        /// Auto expand is disabled </summary>
        Disabled = 0,
        /// <summary>
        /// Auto expands nodes if they become selected in the ISelectionContext </summary>
        ExpandSelected = 1,
        /// <summary>
        /// Auto expands newly inserted nodes </summary>
        ExpandInserted = 2,
        /// <summary>
        /// Auto expands newly inserted nodes only if the parent node is selected.
        /// This option overrides ExpandInserted</summary>
        ExpandInsertedIfParentSelected = 4,
        /// <summary>
        /// Default mode </summary>
        Default = ExpandSelected | ExpandInsertedIfParentSelected
    }
}
