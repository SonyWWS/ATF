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
    /// Wraps a TreeListView and allows user to supply data to the
    /// TreeListView through the ITreeListView interface and the View
    /// property of the TreeListViewAdapter</summary>
    public class TreeListViewAdapter
    {
        /// <summary>
        /// Default constructor</summary>
        /// <remarks>Creates a TreeListView with the TreeList style</remarks>
        public TreeListViewAdapter()
            : this(new TreeListView())
        {
        }

        /// <summary>
        /// Constructor supplying TreeListView</summary>
        /// <param name="treeListView">User supplied TreeListView</param>
        public TreeListViewAdapter(TreeListView treeListView)
        {
            m_treeListView = treeListView;

            if (treeListView.TheStyle == TreeListView.Style.VirtualList)
                m_treeListView.RetrieveVirtualNode += TreeListViewRetrieveVirtualNode;

            m_treeListView.NodeChecked += TreeListViewNodeChecked;
            m_treeListView.NodeLazyLoad += TreeListViewNodeLazyLoad;
            m_treeListView.NodeExpandedChanged += TreeListViewNodeExpandedChanged;
            m_treeListView.CanLabelEdit += TreeListViewCanLabelEdit;
            m_treeListView.AfterNodeLabelEdit += TreeListViewAfterNodeLabelEdit;
            m_treeListView.CanPropertyChange += TreeListViewCanPropertyChange;
            m_treeListView.PropertyChanged += TreeListViewPropertyChanged;

            m_treeListView.Control.KeyDown += ControlKeyDown;
            m_treeListView.Control.KeyPress += ControlKeyPress;
            m_treeListView.Control.KeyUp += ControlKeyUp;

            m_treeListView.Control.MouseClick += ControlMouseClick;
            m_treeListView.Control.MouseDoubleClick += ControlMouseDoubleClick;
            m_treeListView.Control.MouseDown += ControlMouseDown;
            m_treeListView.Control.MouseUp += ControlMouseUp;
        }

        /// <summary>
        /// Gets or sets the underlying data model</summary>
        public ITreeListView View
        {
            get { return m_view; }
            set
            {
                if (m_view != value)
                {
                    if (m_view != null)
                    {
                        m_itemView = null;

                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanging -= SelectionContextSelectionChanging;
                            m_selectionContext.SelectionChanged -= SelectionContextSelectionChanged;
                            m_selectionContext = null;
                        }
                        else
                        {
                            m_treeListView.NodeSelected -= TreeListViewNodeSelected;
                        }

                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted -= ObservableContextItemInserted;
                            m_observableContext.ItemChanged -= ObservableContextItemChanged;
                            m_observableContext.ItemRemoved -= ObservableContextItemRemoved;
                            m_observableContext.Reloaded -= ObservableContextReloaded;
                            m_observableContext = null;
                        }

                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning -= ValidationContextBeginning;
                            m_validationContext.Ending -= ValidationContextEnding;
                            m_validationContext.Ended -= ValidationContextEnded;
                            m_validationContext.Cancelled -= ValidationContextCancelled;
                            m_validationContext = null;
                        }
                    }

                    m_view = value;

                    if (m_view != null)
                    {
                        m_itemView = m_view.As<IItemView>();

                        m_selectionContext = m_view.As<ISelectionContext>();
                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanging += SelectionContextSelectionChanging;
                            m_selectionContext.SelectionChanged += SelectionContextSelectionChanged;
                        }
                        else
                        {
                            m_treeListView.NodeSelected += TreeListViewNodeSelected;
                        }

                        m_observableContext = m_view.As<IObservableContext>();
                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted += ObservableContextItemInserted;
                            m_observableContext.ItemChanged += ObservableContextItemChanged;
                            m_observableContext.ItemRemoved += ObservableContextItemRemoved;
                            m_observableContext.Reloaded += ObservableContextReloaded;
                        }

                        m_validationContext = m_view.As<IValidationContext>();
                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning += ValidationContextBeginning;
                            m_validationContext.Ending += ValidationContextEnding;
                            m_validationContext.Ended += ValidationContextEnded;
                            m_validationContext.Cancelled += ValidationContextCancelled;
                        }
                    }
                }

                Load();
            }
        }

        /// <summary>
        /// Gets the underlying TreeListView</summary>
        public TreeListView TreeListView
        {
            get { return m_treeListView; }
        }

        /// <summary>
        /// Gets or sets a string for persisting the TreeListView settings</summary>
        /// <remarks>This method should be supplied to ISettingsService's RegisterSettings
        /// to persist settings, such as column widths.</remarks>
        public string PersistedSettings
        {
            get { return m_treeListView.PersistedSettings; }
            set { m_treeListView.PersistedSettings = value; }
        }

        /// <summary>
        /// Get adapted user data at a point</summary>
        /// <param name="clientPoint">Point</param>
        /// <returns>User data or null if none</returns>
        public object GetItemAt(Point clientPoint)
        {
            TreeListView.Node node = TreeListView.GetNodeAt(clientPoint);
            return node == null ? null : node.Tag;
        }

        /// <summary>
        /// Get the column index of a particular item at a particular point</summary>
        /// <param name="clientPoint">Point</param>
        /// <returns>Column index or -1 if unknown or invalid item</returns>
        public int GetItemColumnIndexAt(Point clientPoint)
        {
            return m_treeListView.GetNodeColumnIndexAt(clientPoint);
        }

        /// <summary>
        /// Gets the last selected item on the underlying TreeListView but
        /// adapted to the underlying user data type</summary>
        public object LastHit
        {
            get
            {
                object lastHit = m_treeListView.LastHit;
                return lastHit.Is<TreeListView.Node>() ? lastHit.As<TreeListView.Node>().Tag : this;
            }
        }

        /// <summary>
        /// Gets or sets the selection as adapted user data</summary>
        public IEnumerable<object> Selection
        {
            get { return m_treeListView.SelectedNodes.Select(node => node.Tag); }

            set
            {
                var nodes = new List<TreeListView.Node>();
                foreach (object item in value)
                {
                    TreeListView.Node node;
                    if (!m_dictNodes.TryGetValue(item, out node))
                        continue;

                    nodes.Add(node);
                }

                if (nodes.Count <= 0)
                    return;

                m_treeListView.SelectedNodes = nodes;
            }
        }

        /// <summary>
        /// Get or sets the virtual list size</summary>
        /// <remarks>Only valid when TreeListView.Style is VirtualList</remarks>
        public int VirtualListSize
        {
            get
            {
                if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
                    throw new InvalidOperationException("property only valid on virtual lists");

                return m_treeListView.VirtualListSize;
            }

            set
            {
                if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
                    throw new InvalidOperationException("property only valid on virtual lists");

                m_treeListView.VirtualListSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the top item</summary>
        public object TopItem
        {
            get
            {
                TreeListView.Node topItem = m_treeListView.TopItem;
                return topItem == null ? null : topItem.Tag;
            }
            set
            {
                TreeListView.Node node;
                if (!m_dictNodes.TryGetValue(value, out node))
                    return;

                m_treeListView.TopItem = node;
            }
        }

        /// <summary>
        /// Places the item text in edit mode</summary>
        /// <param name="item">Item to edit</param>
        public void BeginLabelEdit(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            TreeListView.Node node;
            if (!m_dictNodes.TryGetValue(item, out node))
                return;

            m_treeListView.BeginLabelEdit(node);
        }

        /// <summary>
        /// Get the node that corresponds to the item or null</summary>
        /// <param name="item">Item to search for</param>
        /// <returns>Node that corresponds to the item or null</returns>
        public TreeListView.Node GetNode(object item)
        {
            TreeListView.Node node;
            m_dictNodes.TryGetValue(item, out node);
            return node;
        }

        /// <summary>
        /// Event fired when underlying TreeListView's KeyDown event is triggered</summary>
        public event EventHandler<KeyEventArgs> KeyDown;

        /// <summary>
        /// Event fired when underlying TreeListView's KeyPress event is triggered</summary>
        public event EventHandler<KeyPressEventArgs> KeyPress;

        /// <summary>
        /// Event fired when underlying TreeListView's KeyUp event is triggered</summary>
        public event EventHandler<KeyEventArgs> KeyUp;

        /// <summary>
        /// Event fired when underlying TreelistView is clicked</summary>
        public event EventHandler<MouseEventArgs> MouseClick;

        /// <summary>
        /// Event fired when underlying TreelistView is double clicked</summary>
        public event EventHandler<MouseEventArgs> MouseDoubleClick;

        /// <summary>
        /// Event fired when mouse down event is triggered on the underlying TreelistView</summary>
        public event EventHandler<MouseEventArgs> MouseDown;

        /// <summary>
        /// Event fired when mouse up event is triggered on the underlying TreelistView</summary>
        public event EventHandler<MouseEventArgs> MouseUp;

        /// <summary>
        /// Event fired when TreeListView node is selected</summary>
        public event EventHandler<NodeAdapterEventArgs> ItemSelected;

        /// <summary>
        /// Event fired when TreeListView node is checked</summary>
        public event EventHandler<NodeAdapterEventArgs> ItemChecked;
        
        /// <summary>
        /// Event fired when underlying TreeListView is expanding an item and it needs more user data</summary>
        public event EventHandler<ItemLazyLoadEventArgs> ItemLazyLoad;
        
        /// <summary>
        /// Event fired when the underlying TreeListView (with style set to VirtualList)
        /// is requesting data it doesn't know about</summary>
        public event EventHandler<RetrieveVirtualNodeAdapter> RetrieveVirtualItem;

        /// <summary>
        /// Event fired when underlying TreeListView fires the NodeExpandedChanged event</summary>
        public event EventHandler<NodeAdapterEventArgs> ItemExpandedChanged;

        /// <summary>
        /// Event fired that asks whether an item's label can be edited in-place</summary>
        /// <remarks>The event is fired only if the tree or the column the item is in allows label editing</remarks>
        public event EventHandler<CanItemLabelEditEventArgs> CanItemLabelEdit;

        /// <summary>
        /// Event fired when underlying TreeListView fires the AfterNodeLabelEdit event</summary>
        public event EventHandler<ItemLabelEditEventArgs> AfterItemLabelEdit;

        /// <summary>
        /// Event fired that asks whether an item's property can be changed</summary>
        /// <remarks>The event is fired only if the tree or the column the item is in allows property editing</remarks>
        public event EventHandler<CanItemPropertyChangeEventArgs> CanItemPropertyChange;

        /// <summary>
        /// Event fired when an item's property has changed</summary>
        public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;

        /// <summary>
        /// NodeAdapter event arguments</summary>
        /// <remarks>Adapts node to client data</remarks>
        public class NodeAdapterEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="item">The client's data</param>
            /// <param name="node">The corresponding node</param>
            public NodeAdapterEventArgs(object item, TreeListView.Node node)
            {
                Item = item;
                Node = node;
            }

            /// <summary>
            /// Gets the client's data</summary>
            public object Item { get; private set; }

            /// <summary>
            /// Gets whether the node is checked or not</summary>
            public TreeListView.Node Node { get; private set; }
        }

        /// <summary>
        /// Wrapper around ListView's RetrieveVirtualItem to allow client code
        /// to supply adapted user data</summary>
        /// <remarks>This is used when the underlying TreeListView's style
        /// is set to VirtualList and the underlying ListView is requesting
        /// an item that it doesn't know about. It makes a data query of the TreeListView,
        /// which then queries the TreeListViewAdapter for the data.</remarks>
        public class RetrieveVirtualNodeAdapter : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="index">Index of the data</param>
            public RetrieveVirtualNodeAdapter(int index)
            {
                m_index = index;
            }

            /// <summary>
            /// Gets or sets the user data for the ItemIndex property</summary>
            public object Item { get; set; }

            /// <summary>
            /// Gets the index of the data being requested</summary>
            public int ItemIndex
            {
                get { return m_index; }
            }

            private readonly int m_index;
        }

        /// <summary>
        /// ItemLazyLoad event arguments</summary>
        /// <remarks>Class to wrap underlying TreeListView NodeLazyLoad event</remarks>
        public class ItemLazyLoadEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="item">Data item</param>
            public ItemLazyLoadEventArgs(object item)
            {
                Item = item;
            }

            /// <summary>
            /// Gets the adapted user data</summary>
            public object Item { get; private set; }
        }

        /// <summary>
        /// Event args that check if an item's label can be edited in-place</summary>
        public class CanItemLabelEditEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="item">The item</param>
            public CanItemLabelEditEventArgs(object item)
            {
                Item = item;
                CanEdit = true;
            }

            /// <summary>
            /// Gets the adapted user data</summary>
            public object Item { get; private set; }

            /// <summary>
            /// Gets or sets whether or not the label can be edited in-place</summary>
            public bool CanEdit { get; set; }
        }

        /// <summary>
        /// ItemLabelEdit event arguments</summary>
        /// <remarks>Class to wrap underlying TreeListView AfterNodeLabelEdit event</remarks>
        public class ItemLabelEditEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="item">Item being edited</param>
            /// <param name="label">New label</param>
            public ItemLabelEditEventArgs(object item, string label)
            {
                Item = item;
                Label = label;
            }

            /// <summary>
            /// Gets edit item</summary>
            public object Item { get; private set; }

            /// <summary>
            /// Gets new label</summary>
            public string Label { get; private set; }

            /// <summary>
            /// Gets or sets whether to cancel the edit or not</summary>
            public bool CancelEdit { get; set; }
        }

        /// <summary>
        /// Event args that check if an item's property can be changed</summary>
        public class CanItemPropertyChangeEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="item">Item</param>
            /// <param name="propertyIndex">Property index</param>
            public CanItemPropertyChangeEventArgs(object item, int propertyIndex)
            {
                Item = item;
                PropertyIndex = propertyIndex;
                CanChange = true;
            }

            /// <summary>
            /// Gets the item</summary>
            public object Item { get; private set; }

            /// <summary>
            /// Gets the property index</summary>
            public int PropertyIndex { get; private set; }

            /// <summary>
            /// Gets or sets whether or not the property can be changed</summary>
            public bool CanChange { get; set; }
        }

        /// <summary>
        /// Item property changed event args</summary>
        public class ItemPropertyChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="item">Item that contains changed property</param>
            /// <param name="propertyIndex">Index of property that has changed</param>
            /// <param name="value">New property value</param>
            public ItemPropertyChangedEventArgs(object item, int propertyIndex, object value)
            {
                Item = item;
                PropertyIndex = propertyIndex;
                Value = value;
            }

            /// <summary>
            /// Gets the item</summary>
            public object Item { get; private set; }

            /// <summary>
            /// Gets the index of the property that has changed</summary>
            public int PropertyIndex { get; private set; }

            /// <summary>
            /// Gets the new property value</summary>
            public object Value { get; private set; }

            /// <summary>
            /// Gets or sets whether to cancel the property change</summary>
            public bool CancelChange { get; set; }
        }

        /// <summary>
        /// Method triggered when the underlying TreeListView's KeyDown event is fired. Raises KeyDown event.</summary>
        /// <param name="e">Key event args</param>
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            KeyDown.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when the underlying TreeListView's KeyPress event is fired. Raises KeyPress event.</summary>
        /// <param name="e">Key press event args</param>
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            KeyPress.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when the underlying TreeListView's KeyUp event is fired. Raises KeyUp event.</summary>
        /// <param name="e">Key event args</param>
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            KeyUp.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseClick event is fired on the TreeListView. Raises MouseClick event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseClick(MouseEventArgs e)
        {
            MouseClick.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseDoubleClick event is fired on the TreeListView. Raises MouseDoubleClick event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            MouseDoubleClick.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseDown event is fired on the TreeListView. Raises MouseDown event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            MouseDown.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseUp event is fired on the TreeListView. Raises MouseUp event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            MouseUp.Raise(this, e);
        }

        private void SelectionContextSelectionChanging(object sender, EventArgs e)
        {
        }

        private void SelectionContextSelectionChanged(object sender, EventArgs e)
        {
        }

        private void ObservableContextItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
            {
                if (m_inTransaction)
                    m_queueInserts.Add(e);
                else
                    AddItem(e.Item, e.Parent);
            }
            else
            {
                if (e.Item is object[])
                    VirtualListSize += ((object[])e.Item).Length;
                else
                    VirtualListSize += 1;
            }
        }

        private void ObservableContextItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
            {
                if (m_inTransaction)
                    m_queueUpdates.Add(e);
                else
                    UpdateItem(e.Item);
            }
            else
            {
                UpdateItem(e.Item);
            }
        }

        private void ObservableContextItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
            {
                if (m_inTransaction)
                    m_queueRemoves.Add(e);
                else
                    RemoveItem(e.Item);
            }
            else
            {
                if (e.Item is object[])
                {
                    var items = (object[])e.Item;
                    VirtualListSize -= items.Length;

                    foreach (object item in items)
                    {
                        if (m_dictNodes.ContainsKey(item))
                            m_dictNodes.Remove(item);
                    }
                }
                else
                {
                    VirtualListSize -= 1;

                    if (m_dictNodes.ContainsKey(e.Item))
                        m_dictNodes.Remove(e.Item);
                }
            }
        }

        private void ObservableContextReloaded(object sender, EventArgs e)
        {
            Load();
        }

        private void ValidationContextBeginning(object sender, EventArgs e)
        {
            m_inTransaction = true;
            m_treeListView.UseInsertQueue = true;
        }

        private void ValidationContextEnding(object sender, EventArgs e)
        {
        }

        private void ValidationContextEnded(object sender, EventArgs e)
        {
            if (!m_inTransaction)
                return;

            try
            {
                m_treeListView.BeginUpdate();

                try
                {
                    m_treeListView.UseInsertQueue = true;

                    foreach (var args in m_queueInserts)
                        AddItem(args.Item, args.Parent);
                }
                finally
                {
                    m_treeListView.UseInsertQueue = false;
                    m_treeListView.FlushInsertQueue();
                }

                foreach (var args in m_queueUpdates)
                    UpdateItem(args.Item);

                foreach (var args in m_queueRemoves)
                    RemoveItem(args.Item);
            }
            finally
            {
                m_inTransaction = false;

                m_queueInserts.Clear();
                m_queueUpdates.Clear();
                m_queueRemoves.Clear();
                
                m_treeListView.EndUpdate();
            }
        }

        private void ValidationContextCancelled(object sender, EventArgs e)
        {
            m_queueInserts.Clear();
            m_queueUpdates.Clear();
            m_queueRemoves.Clear();

            m_inTransaction = false;
        }

        private void TreeListViewRetrieveVirtualNode(object sender, TreeListView.RetrieveVirtualNodeEventArgs e)
        {
            var args = new RetrieveVirtualNodeAdapter(e.NodeIndex);

            RetrieveVirtualItem.Raise(this, args);

            if (args.Item == null)
                return;

            // Wrap user data
            TreeListView.Node node =
                CreateNodeForObject(
                    args.Item,
                    m_itemView,
                    m_treeListView.ImageList,
                    m_treeListView.StateImageList,
                    m_dictNodes);

            m_dictNodes[args.Item] = node;
            e.Node = node;
        }

        private void TreeListViewNodeSelected(object sender, ItemSelectedEventArgs<TreeListView.Node> e)
        {
            if (m_selectionContext == null)
                return;

            object item = e.Item.Tag;
            if (item == null)
                return;

            if (e.Selected)
                m_selectionContext.Add(item);
            else
                m_selectionContext.Remove(item);

            ItemSelected.Raise(this, new NodeAdapterEventArgs(item, e.Item));
        }

        private void TreeListViewNodeChecked(object sender, TreeListView.NodeCheckedEventArgs e)
        {
            object item = e.Node.Tag;
            if (item == null)
                return;

            ItemChecked.Raise(this, new NodeAdapterEventArgs(item, e.Node));
        }

        private void TreeListViewNodeLazyLoad(object sender, TreeListView.NodeEventArgs e)
        {
            object item = e.Node.Tag;
            if (item == null)
                return;

            ItemLazyLoad.Raise(this, new ItemLazyLoadEventArgs(item));
        }

        private void TreeListViewNodeExpandedChanged(object sender, TreeListView.NodeEventArgs e)
        {
            object item = e.Node.Tag;
            if (item == null)
                return;

            ItemExpandedChanged.Raise(this, new NodeAdapterEventArgs(item, e.Node));
        }

        private void TreeListViewCanLabelEdit(object sender, TreeListView.CanLabelEditEventArgs e)
        {
            var args = new CanItemLabelEditEventArgs(e.Node.Tag) { CanEdit = e.CanEdit };
            CanItemLabelEdit.Raise(this, args);
            e.CanEdit = args.CanEdit;
        }

        private void TreeListViewAfterNodeLabelEdit(object sender, TreeListView.NodeLabelEditEventArgs e)
        {
            // Relay
            var args = new ItemLabelEditEventArgs(e.Node.Tag, e.Label);
            AfterItemLabelEdit.Raise(this, args);

            e.CancelEdit = args.CancelEdit;
        }

        private void TreeListViewCanPropertyChange(object sender, TreeListView.CanPropertyChangeEventArgs e)
        {
            var args = new CanItemPropertyChangeEventArgs(e.Node.Tag, e.PropertyIndex) { CanChange = e.CanChange };
            CanItemPropertyChange.Raise(this, args);
            e.CanChange = args.CanChange;
        }

        private void TreeListViewPropertyChanged(object sender, TreeListView.PropertyChangedEventArgs e)
        {
            var args = new ItemPropertyChangedEventArgs(e.Node.Tag, e.PropertyIndex, e.Value);
            ItemPropertyChanged.Raise(this, args);
            e.CancelChange = args.CancelChange;
        }

        private void ControlKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void ControlKeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void ControlKeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void ControlMouseClick(object sender, MouseEventArgs e)
        {
            OnMouseClick(e);
        }

        private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnMouseDoubleClick(e);
        }

        private void ControlMouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void ControlMouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);

            if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
                HandleSelectionUpdates();
        }

        private void Load()
        {
            Unload();

            if (m_view == null)
                return;

            if (m_treeListView.TheStyle == TreeListView.Style.VirtualList)
                m_treeListView.VirtualListSize = 0;

            // Add columns & data

            foreach (string columnName in m_view.ColumnNames)
                m_treeListView.Columns.Add(new TreeListView.Column(columnName));

            try
            {
                m_treeListView.BeginUpdate();

                if (m_treeListView.TheStyle != TreeListView.Style.VirtualList)
                {
                    foreach (object root in m_view.Roots)
                    {
                        if (m_inTransaction)
                            m_queueInserts.Add(new ItemInsertedEventArgs<object>(-1, root, null));
                        else
                            AddItem(root, null);
                    }
                }
            }
            finally
            {
                m_treeListView.EndUpdate();
            }
        }

        private void Unload()
        {
            if (m_treeListView == null)
                return;

            try
            {
                m_treeListView.ClearAll();
            }
            finally
            {
                m_selectionStartItem = null;

                m_dictNodes.Clear();
                m_queueInserts.Clear();
                m_queueUpdates.Clear();
                m_queueRemoves.Clear();
            }
        }

        private void AddItem(object item, object parent)
        {
            if (m_dictNodes.ContainsKey(item))
                return;

            TreeListView.Node nodeParent = null;
            if (parent != null)
                m_dictNodes.TryGetValue(parent, out nodeParent);

            TreeListView.Node node =
                CreateNodeForObject(
                    item,
                    m_itemView,
                    m_treeListView.ImageList,
                    m_treeListView.StateImageList,
                    m_dictNodes);

            m_dictNodes.Add(item, node);

            // Recursively add any children
            AddChildrenToItemRecursively(
                item,
                node,
                m_view,
                m_itemView,
                m_treeListView.ImageList,
                m_treeListView.StateImageList,
                m_dictNodes);

            TreeListView.NodeCollection collection =
                nodeParent == null
                    ? m_treeListView.Nodes
                    : nodeParent.Nodes;

            // Make sure collection can support having children
            if (collection.IsReadOnly && (collection.Owner != null))
                collection.Owner.IsLeaf = false;
            
            if (collection.IsReadOnly)
                return;

            // Add to tree
            collection.Add(node);

            if (nodeParent != null)
                nodeParent.Expanded = true;
        }

        private void UpdateItem(object item)
        {
            TreeListView.Node node;
            if (!m_dictNodes.TryGetValue(item, out node))
                return;

            ItemInfo info =
                GetItemInfo(
                    item,
                    m_itemView,
                    m_treeListView.ImageList,
                    m_treeListView.StateImageList);

            UpdateNodeFromItemInfo(node, item, info);

            m_treeListView.Invalidate(node);
        }

        private void RemoveItem(object item)
        {
            TreeListView.Node node;
            if (!m_dictNodes.TryGetValue(item, out node))
                return;

            m_dictNodes.Remove(item);

            // Need to recursively remove all child nodes from the dictionary
            if (node.HasChildren)
            {
                foreach (TreeListView.Node child in GatherNodes(node.Nodes))
                {
                    if (m_dictNodes.ContainsKey(child.Tag))
                        m_dictNodes.Remove(child.Tag);
                }
            }

            TreeListView.NodeCollection collection =
                node.Parent != null
                    ? node.Parent.Nodes
                    : m_treeListView.Nodes;

            if (collection.IsReadOnly)
                return;

            collection.Remove(node);
        }

        private void HandleSelectionUpdates()
        {
            if (m_selectionContext == null)
                return;

            object hitItem = LastHit;
            if ((hitItem == null) || (hitItem == this))
                return;

            TreeListView.Node hitNode;
            if (!m_dictNodes.TryGetValue(hitItem, out hitNode))
                return;

            HashSet<object> oldSelection = null;
            HashSet<object> newSelection = null;

            var handler = ItemSelected;
            if (handler != null)
                oldSelection = new HashSet<object>(m_selectionContext.Selection);

            switch (Control.ModifierKeys)
            {
                case Keys.Shift:
                {
                    if (m_selectionStartItem != null)
                    {
                        int idx1 = m_treeListView.GetNodeIndex(m_selectionStartItem);
                        int idx2 = m_treeListView.GetNodeIndex(hitNode);

                        int startIndex = Math.Min(idx1, idx2);
                        int endIndex = Math.Max(idx1, idx2);

                        newSelection = new HashSet<object>();
                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            TreeListView.Node item = m_treeListView.GetNodeAtIndex(i);
                            newSelection.Add(item.Tag);
                        }
                        m_selectionContext.SetRange(newSelection);
                    }
                    else
                    {
                        m_selectionContext.Set(hitItem);
                        m_selectionStartItem = hitNode;
                    }
                }
                break;

                case Keys.Control:
                {
                    if (m_selectionContext.SelectionContains(hitItem))
                        m_selectionContext.Remove(hitItem);
                    else
                        m_selectionContext.Add(hitItem);

                    m_selectionStartItem = hitNode;
                }
                break;

                default:
                {
                    m_selectionContext.Set(hitItem);
                    m_selectionStartItem = hitNode;
                }
                break;
            }

            if (handler == null)
                return;

            if (newSelection == null)
                newSelection = new HashSet<object>(m_selectionContext.Selection);

            foreach (object removed in oldSelection.Except(newSelection))
            {
                TreeListView.Node node;
                if (!m_dictNodes.TryGetValue(removed, out node))
                    continue;

                ItemSelected.Raise(this, new NodeAdapterEventArgs(removed, node));
            }

            foreach (object added in newSelection.Except(oldSelection))
            {
                TreeListView.Node node;
                if (!m_dictNodes.TryGetValue(added, out node))
                    continue;

                ItemSelected.Raise(this, new NodeAdapterEventArgs(added, node));
            }
        }

        private static void AddChildrenToItemRecursively(
            object item,
            TreeListView.Node node,
            ITreeListView view,
            IItemView itemView,
            ImageList imageList,
            ImageList stateImageList,
            IDictionary<object, TreeListView.Node> dictNodes)
        {
            if (view == null)
                return;

            IEnumerable<object> children = view.GetChildren(item);
            foreach (object child in children)
            {
                TreeListView.Node nodeChild = CreateNodeForObject(child, itemView, imageList, stateImageList, dictNodes);

                if (dictNodes != null)
                    dictNodes.Add(child, nodeChild);

                node.Nodes.Add(nodeChild);

                AddChildrenToItemRecursively(child, nodeChild, view, itemView, imageList, stateImageList, dictNodes);
            }
        }

        private static TreeListView.Node CreateNodeForObject(
            object item,
            IItemView itemView,
            ImageList imageList,
            ImageList stateImageList,
            IDictionary<object, TreeListView.Node> dictNodes)
        {
            // Check for existing
            TreeListView.Node node;
            if (dictNodes.TryGetValue(item, out node))
                return node;

            // Create new
            node = new TreeListView.Node();

            ItemInfo info =
                GetItemInfo(
                    item,
                    itemView,
                    imageList,
                    stateImageList);

            UpdateNodeFromItemInfo(node, item, info);

            return node;
        }

        private static ItemInfo GetItemInfo(object item, IItemView itemView, ImageList imageList, ImageList stateImageList)
        {
            var info = new WinFormsItemInfo(imageList, stateImageList);

            if (itemView == null)
            {
                info.Label = GetObjectString(item);
                info.Properties = new object[0];
                info.ImageIndex = TreeListView.InvalidImageIndex;
                info.StateImageIndex = TreeListView.InvalidImageIndex;
                info.CheckState = CheckState.Unchecked;
                info.FontStyle = FontStyle.Regular;
                info.IsLeaf = true;
                info.IsExpandedInView = false;
                info.HoverText = string.Empty;
            }
            else
            {
                itemView.GetInfo(item, info);
            }

            return info;
        }

        private static void UpdateNodeFromItemInfo(TreeListView.Node node, object item, ItemInfo info)
        {
            node.Label = info.Label;
            node.Properties = info.Properties;
            node.ImageIndex = info.ImageIndex;
            node.StateImageIndex = info.StateImageIndex;
            node.CheckState = info.GetCheckState();
            node.Tag = item;
            node.IsLeaf = info.IsLeaf;
            node.Expanded = info.IsExpandedInView;
            node.FontStyle = info.FontStyle;
            node.HoverText = info.HoverText;
        }

        private static string GetObjectString(object value)
        {
            var formattable = value as IFormattable;
            return
                formattable != null
                    ? formattable.ToString(null, null)
                    : value.ToString();
        }

        private static IEnumerable<TreeListView.Node> GatherNodes(IEnumerable<TreeListView.Node> collection)
        {
            foreach (TreeListView.Node node in collection)
            {
                yield return node;

                if (!node.HasChildren)
                    continue;

                foreach (TreeListView.Node child in GatherNodes(node.Nodes))
                    yield return child;
            }
        }

        private IItemView m_itemView;
        private ITreeListView m_view;
        private ISelectionContext m_selectionContext;
        private IObservableContext m_observableContext;
        private IValidationContext m_validationContext;

        private bool m_inTransaction;
        private TreeListView.Node m_selectionStartItem;

        private readonly TreeListView m_treeListView;

        private readonly List<ItemInsertedEventArgs<object>> m_queueInserts =
            new List<ItemInsertedEventArgs<object>>();

        private readonly List<ItemChangedEventArgs<object>> m_queueUpdates =
            new List<ItemChangedEventArgs<object>>();

        private readonly List<ItemRemovedEventArgs<object>> m_queueRemoves =
            new List<ItemRemovedEventArgs<object>>();

        private readonly Dictionary<object, TreeListView.Node> m_dictNodes =
            new Dictionary<object, TreeListView.Node>();
    }
}