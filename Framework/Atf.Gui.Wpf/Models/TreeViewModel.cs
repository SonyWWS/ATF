//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model for a TreeView</summary>
    public class TreeViewModel : AdapterViewModel
    {
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

        #region ShowRoot Property

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
                        && m_ensureVisiblePath.First == Root)
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
            Node node;
            if (m_itemToNodeMap.TryGetValue(item, out node))
            {
                RefreshNode(node);
            }
        }

        /// <summary>
        /// Refreshes parents of given node</summary>
        /// <param name="item">Node whose parents are refreshed</param>
        public void RefreshParents(object item)
        {
            Node node;
            if (m_itemToNodeMap.TryGetValue(item, out node))
            {
                if (node.Parent != null)
                    RefreshNode(node.Parent);
            }
        }

        /// <summary>
        /// Makes sure the tree nodes corresponding to a given path are visible</summary>
        /// <param name="path">Path, identifying tree nodes to show</param>
        /// <param name="select">Whether node specified by the path should be selected</param>
        /// <returns>The node specified by the path or null if it could not be found</returns>
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
            foreach(var node in GetAllNodesInTree())
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
                    if (ShowRoot)
                    {
                        Root.Expanded = false;
                    }
                    else
                    {
                        foreach (var node in Roots)
                            node.Expanded = false;
                    }
                }
            }
            finally
            {
                m_synchronizingSelection = false;
            }

        }

        #endregion

        #region Private Methods

        private void RefreshNode(Node node)
        {
            // Update node properties
            UpdateNode(node);

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
        }

        private void AddPaths(Node node, List<object> path, HashSet<Path<object>> paths)
        {
            if (node.Expanded)
            {
                path.Add(node);
                paths.Add(new AdaptablePath<object>(path));

                foreach (Node child in node.ChildrenInternal)
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
                result.Add(node);
            }
            return result;
        }

        private Node CreateNode(object adaptee, Node parent)
        {
            var node = new Node(adaptee, this, parent);
            node.IsSelectedChanged += new EventHandler(node_IsSelectedChanged);
            m_itemToNodeMap.Add(adaptee, node);
            UpdateNode(node);
            return node;
        }

        private void Load()
        {
            Unload();

            if (m_treeView != null)
            {
                object rootObj = m_treeView.Root;
                Root = CreateNode(rootObj, null);
                Root.Expanded = true;
                UpdateNode(Root);
                OnPropertyChanged(new PropertyChangedEventArgs("Root"));
                OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
            }
        }

        private void Unload()
        {
            EnsureVisiblePath = null;
            Root = null;
            m_itemToNodeMap.Clear();
            m_previousSelection = null;
            OnPropertyChanged(new PropertyChangedEventArgs("Root"));
            OnPropertyChanged(new PropertyChangedEventArgs("Roots"));
            OnPropertyChanged(new PropertyChangedEventArgs("EnsureVisiblePath"));
        }

        private void InsertObject(object child, object parent, int index)
        {
            Node parentNode;
            if (m_itemToNodeMap.TryGetValue(parent, out parentNode))
            {
                if (parentNode.ChildrenInternal != null)
                {
                    Node childNode = CreateNode(child, parentNode);
                    if (index >= 0)
                        parentNode.ChildrenInternal.Insert(index, childNode);
                    else
                        parentNode.ChildrenInternal.Add(childNode);
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
            // copy to List since unbind modifies the item-to-node map
            Node node;
            if (m_itemToNodeMap.TryGetValue(tree, out node))
            {
                Unbind(node);
                node.Parent.ChildrenInternal.Remove(node);

                try
                {
                    m_synchronizingSelection = true;
                    m_selectionContext.Remove(MakePath(node));
                }
                finally
                {
                    m_synchronizingSelection = false;
                }
            }
        }

        // Remove all chilren as atomic operation
        private void RemoveChildren(Node node)
        {
            var pathsToRemove = new List<Path<object>>();

            // copy to List since unbind modifies the item-to-node map
            foreach (Node child in node.ChildrenInternal)
            {
                Unbind(child);
                pathsToRemove.Add(MakePath(child));
            }
            
            node.ChildrenInternal.Clear();

            try
            {
                m_synchronizingSelection = true;
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

        private void UpdateChangedParents()
        {
            if (m_parentsWithRemovedChildren != null)
            {
                foreach (object parent in m_parentsWithRemovedChildren)
                {
                    Node node;
                    if (m_itemToNodeMap.TryGetValue(parent, out node))
                    {
                        RefreshNode(node);
                    }
                }

                foreach (object parent in m_parentsWithAddedChildren)
                {
                    Node node;
                    if (m_itemToNodeMap.TryGetValue(parent, out node))
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
            //    !m_itemToNodeMap.ContainsKey(m_lastHit))
            //{
            //    SetLastHit(null);
            //}
        }

        private void Unbind(Node node)
        {
            m_itemToNodeMap.Remove(node.Adaptee);

            if (node.ChildrenInternal != null)
            {
                foreach (Node child in node.ChildrenInternal)
                    Unbind(child);
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
        /// Searches for a specific node.
        /// This involves creating all nodes in subtree until node is found.</summary>
        /// <param name="item">Node to search for</param>
        /// <returns>Node searched for, or null if not found</returns>
        private Node GetNode(object item)
        {
            Node node = null;
            if (!m_itemToNodeMap.TryGetValue(item, out node))
            {
                // Node not created - have to force creation of all children in tree
                // until we find the item we are looking for!
                node = GetAllNodesInTree().FirstOrDefault(x => x.Adaptee == item);

            }
            return node;
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
            if (m_parentsWithAddedChildren != null)
            {
                Node parentNode;
                if (m_itemToNodeMap.TryGetValue(e.Parent, out parentNode))
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
            if (m_parentsWithRemovedChildren != null)
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
            Node node;
            if (m_itemToNodeMap.TryGetValue(e.Item, out node))
            {
                UpdateNode(m_itemToNodeMap[e.Item]);
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

                    //foreach (var node in m_itemToNodeMap.Values)
                    //    node.IsSelected = false;
                    // Deselect the previous selection
                    foreach (Path<object> path in m_previousSelection)
                    {
                        Node node;
                        if(m_itemToNodeMap.TryGetValue(path.Last, out node))
                        {
                            //System.Diagnostics.Debug.Assert(node.IsSelected);
                            node.IsSelected = false;
                        }
                    }

                    Path<Node> lastNodePath = null;
                    Path<object>[] selectedPaths = m_selectionContext.GetSelection<Path<object>>().ToArray();
                    if (selectedPaths.Length > 0)
                    {
                        foreach (Path<object> path in selectedPaths)
                        {
                            // Expand as much of 'path' as possible, if m_autoExpand is true.
                            Node node = null;

                            if ((m_autoExpand & AutoExpandMode.ExpandSelected) > 0)
                            {
                                lastNodePath = ExpandPath(path);
                                if(lastNodePath != null)
                                    node = lastNodePath.Last;
                            }
                            else
                            {
                                m_itemToNodeMap.TryGetValue(path.Last, out node);
                            }

                            if (node != null)
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

            Node node;
            if (m_itemToNodeMap.TryGetValue(item, out node))
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

        // Called after an atmoic selection operation is completed
        private void RefreshSelection()
        {
            if (m_selectionChangedNodes != null && m_selectionChangedNodes.Count > 0)
            {
                try
                {
                    m_synchronizingSelection = true;

                    if (!m_multiSelectEnabled)
                    {
                        // TODO: test this
                        Node lastSelected = m_selectionChangedNodes.LastOrDefault(x=>x.IsSelected);
                        if(lastSelected != null)
                        {
                            DeselectAll();
                            m_selectionContext.Add(MakePath(lastSelected));
                        }
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
            var pathsToRemove = new List<Path<object>>();
            foreach (Path<object> selectedPath in m_selectionContext.GetSelection<Path<object>>().ToArray())
            {
                Node selectedNode;
                if (m_itemToNodeMap.TryGetValue(selectedPath.Last, out selectedNode))
                {
                    pathsToRemove.Add(selectedPath);
                    selectedNode.IsSelected = false;
                }
            }
            m_selectionContext.RemoveRange(pathsToRemove);
        }

        #endregion

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
        private Dictionary<object, Node> m_itemToNodeMap = new Dictionary<object, Node>();
    }

    /// <summary>
    /// Tree node that can adapt an adaptee and provide a bindable As property</summary>
    public class Node : AdapterViewModel
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="adaptee">Object that is adapted</param>
        /// <param name="owner">Node's owner</param>
        /// <param name="parent">Node's parent</param>
        public Node(object adaptee, TreeViewModel owner, Node parent)
            : base(adaptee)
        {
            m_owner = owner;
            Parent = parent;
        }

        /// <summary>
        /// Gets node's parent</summary>
        public Node Parent { get; private set; }

        /// <summary>
        /// Gets node's children</summary>
        public IEnumerable<Node> Children
        { 
            get 
            {
                if (m_children == null)
                {
                    m_children = m_owner.CreateChildren(this);
                }
                return m_children; 
            } 
        }

        #region Label Property

        /// <summary>
        /// Gets or sets node's label</summary>
        /// <remarks>Default is empty string if node has no label</remarks>
        public string Label
        {
            get { return m_itemInfo.Label; }
            set
            {
                var context = m_owner.As<ILabelEditingContext>();
                if (context == null || !context.CanEditLabel(Adaptee))
                    return;

                ITransactionContext transactionContext = m_owner.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        context.SetLabel(Adaptee, value);
                    }, "Edit Label".Localize());
            }
        }

        private static readonly PropertyChangedEventArgs s_labelArgs
            = ObservableUtil.CreateArgs<Node>(x => x.Label);

        #endregion

        #region IsSelected Property

        /// <summary>
        /// Gets or sets whether node selected</summary>
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                if (m_isSelected != value)
                {
                    m_isSelected = value;
                    OnPropertyChanged(s_isSelectedArgs);
                    IsSelectedChanged.Raise(this, EventArgs.Empty);
                    //System.Diagnostics.Debug.WriteLine(Label + " Selected=" + value);
                }
            }
        }

        private bool m_isSelected;
        private static readonly PropertyChangedEventArgs s_isSelectedArgs
            = ObservableUtil.CreateArgs<Node>(x => x.IsSelected);

        internal event EventHandler IsSelectedChanged;

        #endregion

        #region IsExpanded Property

        /// <summary>
        /// Gets or sets whether this node is expanded in the view</summary>
        public bool Expanded
        {
            get { return m_itemInfo.IsExpandedInView; }
            set
            {
                if (m_itemInfo.IsExpandedInView != value)
                {
                    m_itemInfo.IsExpandedInView = value;

                    //if (m_isExpanded)
                    //{
                    //    TODO: lazy loading
                    //    var children = m_owner.CreateChildren(this);
                    //    m_children = children;
                    //}

                    OnPropertyChanged(s_isExpandedArgs);
                }
            }
        }

        private static readonly PropertyChangedEventArgs s_isExpandedArgs
            = ObservableUtil.CreateArgs<Node>(x => x.Expanded);

        #endregion

        #region IsLeaf Property

        /// <summary>
        /// Gets whether node is a leaf (has no sub-items)</summary>
        public bool IsLeaf
        {
            get { return m_itemInfo.IsLeaf; }
        }

        private static readonly PropertyChangedEventArgs s_isLeafArgs
            = ObservableUtil.CreateArgs<Node>(x => x.IsLeaf);

        #endregion

        #region IsInLabelEditMode Property

        /// <summary>
        /// Gets or sets whether label edit mode is active</summary>
        public bool IsInLabelEditMode
        {
            get { return m_isInLabelEditMode; }
            set
            {
                m_isInLabelEditMode = value;
                OnPropertyChanged(s_isInLabelEditModeArgs);
            }
        }

        private bool m_isInLabelEditMode;
        private static readonly PropertyChangedEventArgs s_isInLabelEditModeArgs
            = ObservableUtil.CreateArgs<Node>(x => x.IsInLabelEditMode);

        #endregion

        /// <summary>
        /// Gets the resource key for associated node image</summary>
        public object ImageKey
        {
            get { return m_itemInfo.ImageKey; }
        }

        private static readonly PropertyChangedEventArgs s_imageKeyArgs
            = ObservableUtil.CreateArgs<Node>(x => x.ImageKey);

        /// <summary>
        /// Gets the resource key for associated node state image</summary>
        public object StateImageKey
        {
            get { return m_itemInfo.StateImageKey; }
        }

        private static readonly PropertyChangedEventArgs s_stateImageKeyArgs
            = ObservableUtil.CreateArgs<Node>(x => x.StateImageKey);
        
        /// <summary>
        /// Gets index of node in list of its parent's nodes</summary>
        public int Index
        {
            get
            {
                if (Parent == null)
                    return 0;
                return Parent.ChildrenInternal.IndexOf(this);
            }
        }

        internal WpfItemInfo ItemInfo { get { return m_itemInfo; } }

        internal void ItemInfoChanged()
        {
            // TODO: could do some checks for efficiency
            OnPropertyChanged(s_isExpandedArgs);
            OnPropertyChanged(s_labelArgs);
            OnPropertyChanged(s_isLeafArgs);
            OnPropertyChanged(s_imageKeyArgs);
            OnPropertyChanged(s_stateImageKeyArgs);
        }

        internal IList<Node> ChildrenInternal { get { return m_children; } }

        private readonly WpfItemInfo m_itemInfo = new WpfItemInfo();
        private readonly TreeViewModel m_owner;
        private ObservableCollection<Node> m_children;
    }

    /// <summary>
    /// Node auto expand enums</summary>
    [Flags]
    public enum AutoExpandMode : byte
    {
        /// <summary>
        /// Auto expand is disabled</summary>
        Disabled = 0,
        /// <summary>
        /// Auto expands nodes if they become selected in the ISelectionContext</summary>
        ExpandSelected = 1,
        /// <summary>
        /// Auto expands newly inserted nodes</summary>
        ExpandInserted = 2,
        /// <summary>
        /// Auto expands newly inserted nodes only if the parent node is selected.
        /// This option overrides ExpandInserted</summary>
        ExpandInsertedIfParentSelected = 4,
        /// <summary>
        /// Default mode</summary>
        Default = ExpandSelected | ExpandInsertedIfParentSelected
    }
}
