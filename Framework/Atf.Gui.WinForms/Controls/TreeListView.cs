//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Partial implementation of the TreeListView class, which provides a tree ListView</summary>
    public sealed partial class TreeListView : IDisposable, IAdaptable
    {
        /// <summary>
        /// TreeListView Styles</summary>
        public enum Style
        {
            /// <summary>
            /// A list</summary>
            List,

            /// <summary>
            /// A list with check boxes next to items</summary>
            CheckedList,

            /// <summary>
            /// A list capable of supporting thousands of items</summary> 
            VirtualList,

            /// <summary>
            /// A tree with columns (the default)</summary>
            TreeList,

            /// <summary>
            /// A tree list with check-boxes next to the items</summary>
            CheckedTreeList,
        }

        /// <summary>
        /// Constructor</summary>
        public TreeListView()
            : this(Style.TreeList)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="style">The style of the TreeListView</param>
        public TreeListView(Style style)
        {
            m_nodes = new NodeCollection(null);
            m_nodes.NodeAdding += NodeCollectionNodeAdding;
            m_nodes.NodeAdded += NodeCollectionNodeAdded;
            m_nodes.NodeRemoving += NodeCollectionNodeRemoving;
            m_nodes.NodesRemoving += NodeCollectionNodesRemoving;

            m_columns = new ColumnCollection();
            m_columns.ColumnAdding += ColumnsColumnAdding;
            m_columns.ColumnRemoving += ColumnsColumnRemoving;
            m_columns.ColumnClearAll += ColumnsColumnClearAll;

            m_control =
                new TheTreeListView(style, this)
                    {
                        View = View.Details,
                        FullRowSelect = true,
                        GridLines = true,
                        AllowColumnReorder = false,
                        LabelEdit = false,
                        SmallImageList = ResourceUtil.GetImageList16(),
                        StateImageList = ResourceUtil.GetImageList16(),
                        CheckBoxes = false
                    };

            m_control.MouseDown += ControlMouseDown;
            m_control.MouseUp += ControlMouseUp;
            m_control.ColumnWidthChanged += ControlColumnWidthChanged;
            m_control.MouseClick += ControlMouseClick;
            m_control.MouseDoubleClick += ControlMouseDoubleClick;
            m_control.Scroll += ControlScroll;
            
            if (style == Style.VirtualList)
            {
                m_control.VirtualMode = true;
                m_control.RetrieveVirtualItem += ControlRetrieveVirtualItem;
                m_control.SelectedIndexChanged += ControlSelectedIndexChanged;
            }
            else
            {
                m_control.ColumnClick += ControlColumnClick;
                m_control.ItemSelectionChanged += ControlItemSelectionChanged;

                m_sortTimer = new Timer {Interval = 200};
                m_sortTimer.Tick += SortTimerTick;
                m_sortTimer.Start();
            }

            m_editBox = new TextBox { Parent = m_control };
            m_editBox.LostFocus += EditBoxLostFocus;
            m_editBox.KeyPress += EditBoxKeyPress;
            m_editBox.Hide();

            m_listViewItemSorter = new ListViewItemSorter();
            m_control.ListViewItemSorter = m_listViewItemSorter;
        }

        /// <summary>
        /// Implicitly convert the TreeListView to Control</summary>
        /// <param name="treeListView">TreeListView to convert</param>
        /// <returns>TreeListView as Control</returns>
        public static implicit operator Control(TreeListView treeListView)
        {
            return treeListView.m_control;
        }

        /// <summary>
        /// Gets or sets the name</summary>
        public string Name
        {
            get { return m_control.Name; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new InvalidOperationException("empty or null name not permitted");

                m_control.Name = value;
            }
        }

        /// <summary>
        /// Gets the TreeListView as a Control</summary>
        public Control Control
        {
            get { return m_control; }
        }

        /// <summary>
        /// Gets the column collection</summary>
        public ColumnCollection Columns
        {
            get { return m_columns; }
        }

        /// <summary>
        /// Gets the TreeListView.Style of the TreeListView</summary>
        public Style TheStyle
        {
            get { return m_control.TheStyle; }
        }

        /// <summary>
        /// Gets or sets the header style</summary>
        public ColumnHeaderStyle HeaderStyle
        {
            get { return m_control.HeaderStyle; }
            set { m_control.HeaderStyle = value; }
        }

        /// <summary>
        /// Gets or sets whether gridlines are shown or not</summary>
        public bool GridLines
        {
            get { return m_control.GridLines; }
            set { m_control.GridLines = value; }
        }

        /// <summary>
        /// Gets the root level node collection</summary>
        public NodeCollection Nodes
        {
            get { return m_nodes; }
        }

        /// <summary>
        /// Gets or sets the comparer to use when sorting nodes</summary>
        public IComparer<Node> NodeSorter
        {
            get { return m_sorter ?? m_defaultSorter; }
            set { m_sorter = value; }
        }

        /// <summary>
        /// Gets or sets the column to sort</summary>
        public int SortColumn
        {
            get
            {
                if (m_control.TheStyle == Style.VirtualList)
                    throw new InvalidOperationException(ExceptionTextSortingNotAllowedInVirtualList);

                return m_sortColumn;
            }

            set
            {
                if (m_control.TheStyle == Style.VirtualList)
                    throw new InvalidOperationException(ExceptionTextSortingNotAllowedInVirtualList);

                if (value == m_sortColumn)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");

                if (value > m_control.Columns.Count)
                    throw new ArgumentOutOfRangeException("value");

                m_sortColumn = value;
                Sort();
            }
        }

        /// <summary>
        /// Gets or sets the sort order</summary>
        public SortOrder SortOrder
        {
            get
            {
                if (m_control.TheStyle == Style.VirtualList)
                    throw new InvalidOperationException(ExceptionTextSortingNotAllowedInVirtualList);

                return m_sortOrder;
            }

            set
            {
                if (m_control.TheStyle == Style.VirtualList)
                    throw new InvalidOperationException(ExceptionTextSortingNotAllowedInVirtualList);

                m_sortOrder = value;
                Sort();
            }
        }

        /// <summary>
        /// Sort the contents of the TreeListView</summary>
        public void Sort()
        {
            if (m_sorting)
                return;

            // Don't sort inside a BeginUpdate/EndUpdate chunk
            if (m_updateCount != 0)
            {
                m_needSorting = true;
                return;
            }

            try
            {
                m_sorting = true;

                if (m_control.TheStyle == Style.VirtualList)
                    return;

                UInt64 number = 0;
                SetupHierarchy(Nodes, ref number, NodeSorter);

                m_control.Sort();
            }
            finally
            {
                m_sorting = false;
                m_needSorting = false;
            }
        }

        /// <summary>
        /// Gets or sets whether the user can edit the labels of items in the control</summary>
        public bool AllowLabelEdit { get; set; }

        /// <summary>
        /// If set allows editing of properties in all columns</summary>
        public bool AllowPropertyEdit { get; set; }

        /// <summary>
        /// Gets or sets whether to show node hover over text</summary>
        public bool ShowNodeHoverText
        {
            get { return m_control.ShowItemToolTips; }
            set { m_control.ShowItemToolTips = value; }
        }

        /// <summary>
        /// Places the node text in edit mode</summary>
        /// <param name="node">Node to edit</param>
        public void BeginLabelEdit(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            ListViewItem lstItem;
            if (!AllowLabelEdit || !m_itemMap.TryGetValue(node, out lstItem))
                return;

            var args = new CanLabelEditEventArgs(node);
            CanLabelEdit.Raise(this, args);
            if (!args.CanEdit)
                return;

            m_editBox.Bounds = node.LabelHitRect;
            m_currentEditNode = node;
            m_currentEditColumnIndex = 0;

            m_editBox.Text = node.Label;
            m_editBox.Show();
            m_editBox.Focus();
        }

        /// <summary>
        /// Gets or sets the ImageList to use for the underlying ListView</summary>
        public ImageList ImageList
        {
            get { return m_control.SmallImageList; }
            set { m_control.SmallImageList = value; }
        }

        /// <summary>
        /// Gets or sets the StateImageList to use for the underlying ListView</summary>
        public ImageList StateImageList
        {
            get { return m_control.StateImageList; }
            set { m_control.StateImageList = value; }
        }

        /// <summary>
        /// Gets or sets whether to apply check state changes to descendants</summary>
        public bool RecursiveCheckBoxes
        {
            get { return m_recursiveCheckBoxes; }
            set { m_recursiveCheckBoxes = value; }
        }

        /// <summary>
        /// Obtains a Node at a particular point</summary>
        /// <param name="clientPoint">Point at which to obtain Node</param>
        /// <returns>Node or null if no Node at the given point</returns>
        public Node GetNodeAt(Point clientPoint)
        {
            ListViewItem lstItem =
                m_control.GetItemAt(
                    clientPoint.X,
                    clientPoint.Y);

            return
                lstItem == null
                    ? null
                    : lstItem.Tag as Node;
        }

        /// <summary>
        /// Gets the column index of a Node at a given point</summary>
        /// <param name="clientPoint">Point at which to obtain Node's column index</param>
        /// <returns>Column index or -1 if no Node at given point</returns>
        public int GetNodeColumnIndexAt(Point clientPoint)
        {
            ListViewItem lstItem =
                m_control.GetItemAt(
                    clientPoint.X,
                    clientPoint.Y);

            if (lstItem == null)
                return -1;

            ListViewItem.ListViewSubItem subItem =
                lstItem.GetSubItemAt(
                    clientPoint.X,
                    clientPoint.Y);

            if (subItem == null)
                return -1;

            int column = 0;
            if (lstItem.Bounds != subItem.Bounds)
                column = lstItem.SubItems.IndexOf(subItem);

            return column;
        }

        /// <summary>
        /// Gets a node's index in the internal list control</summary>
        /// <param name="node">Node</param>
        /// <returns>Node's index</returns>
        public int GetNodeIndex(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            ListViewItem lstItem;
            return m_itemMap.TryGetValue(node, out lstItem) ? lstItem.Index : -1;
        }

        /// <summary>
        /// Gets a node at a particular index</summary>
        /// <param name="index">Index</param>
        /// <returns>Node</returns>
        public Node GetNodeAtIndex(int index)
        {
            ListViewItem lstItem;
            
            if (m_control.TheStyle != Style.VirtualList)
            {
                // Non-virtual list
                if ((index < 0) || (index >= m_control.Items.Count))
                    throw new ArgumentOutOfRangeException("index");

                lstItem = m_control.Items[index];
            }
            else
            {
                // Virtual list
                lstItem = GetVirtualListItemAtIndexOrLookup(index);
            }

            return (Node)lstItem.Tag;
        }

        /// <summary>
        /// Gets or sets all of the currently selected nodes</summary>
        public IEnumerable<Node> SelectedNodes
        {
            get
            {
                //
                // Virtual lists can't use ListView.SelectedItems;
                // they can only use ListView.SelectedIndices
                //

                if (m_control.TheStyle != Style.VirtualList)
                {
                    foreach (ListViewItem lstItem in m_control.SelectedItems)
                    {
                        if (lstItem.Tag == null)
                            continue;

                        if (!(lstItem.Tag is Node))
                            continue;

                        yield return (Node)lstItem.Tag;
                    }
                }
                else
                {
                    foreach (int index in m_control.SelectedIndices)
                    {
                        ListViewItem lstItem = GetVirtualListItemAtIndexOrLookup(index);
                        if (lstItem.Tag == null)
                            continue;

                        if (!(lstItem.Tag is Node))
                            continue;

                        yield return (Node)lstItem.Tag;
                    }
                }
            }

            set
            {
                if (m_control.TheStyle != Style.VirtualList)
                {
                    // Clear current selection
                    var selectedNodes = SelectedNodes.ToList();
                    foreach (var node in selectedNodes)
                    {
                        ListViewItem lstItem;
                        if (!m_itemMap.TryGetValue(node, out lstItem))
                            continue;

                        lstItem.Selected = false;
                    }

                    if (value == null)
                        return;

                    // Set new selection
                    foreach (Node node in value)
                    {
                        ListViewItem lstItem;
                        if (!m_itemMap.TryGetValue(node, out lstItem))
                            continue;

                        lstItem.Selected = true;
                    }
                }
                else
                {
                    throw new NotImplementedException("not added... yet");
                    //foreach (Node node in value)
                    //{
                    //    ListViewItem lstItem;
                    //    if (!m_itemMap.TryGetValue(node, out lstItem))
                    //        continue;

                    //    lstItem.Selected = true;
                    //    node.Selected = true;
                    //}
                }
            }
        }

        /// <summary>
        /// Expands all nodes</summary>
        /// <remarks>Triggers lazy loads if needed</remarks>
        public void ExpandAll()
        {
            try
            {
                m_expandingCollapsing = true;
                BeginUpdate();

                var nodes = new List<Node>(Nodes);
                foreach (Node node in nodes)
                    ExpandAll(node);
            }
            finally
            {
                m_expandingCollapsing = false;
                EndUpdate();
            }
        }

        /// <summary>
        /// Collapses all nodes</summary>
        public void CollapseAll()
        {
            try
            {
                m_expandingCollapsing = true;
                BeginUpdate();

                var nodes = new List<Node>(Nodes);
                foreach (Node node in nodes)
                    CollapseAll(node);
            }
            finally
            {
                m_expandingCollapsing = false;
                EndUpdate();
            }
        }

        /// <summary>
        /// Removes all items</summary>
        public void ClearAll()
        {
            try
            {
                BeginUpdate();

                m_insertQueue.Clear();

                m_control.Columns.Clear();
                m_control.ResetWorkaroundList();
                m_control.Items.Clear();

                m_columnMap.Clear();
                m_itemMap.Clear();
                m_virtualItemMap.Clear();
                m_virtualListOldSelectedIndices.Clear();

                Columns.Clear();
                Nodes.Clear();
            }
            finally
            {
                EnsureEditingTerminated();
                EndUpdate();
            }
        }

        /// <summary>
        /// Shows a specific node</summary>
        /// <param name="node">Node to show</param>
        public void Show(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            Node ancestor = node.Parent;
            while (ancestor != null)
            {
                ancestor.Expanded = true;
                ExpandNode(ancestor);

                ancestor = ancestor.Parent;
            }
        }

        /// <summary>
        /// Ensures that a specific node is visible</summary>
        /// <param name="node">Node to be made visible</param>
        public void EnsureVisible(Node node)
        {
            Show(node);

            ListViewItem lstItem;
            if (!m_itemMap.TryGetValue(node, out lstItem))
                return;

            m_control.EnsureVisible(lstItem.Index);
        }

        /// <summary>
        /// Scrolls a specific node into view</summary>
        /// <param name="node">Node to scroll into view</param>
        public void ScrollIntoView(Node node)
        {
            EnsureVisible(node);
        }

        /// <summary>
        /// Gets or sets the size of the underlying ListView</summary>
        /// <remarks>This property can only be used when the TreeListView is in Virtual mode.</remarks>
        public int VirtualListSize
        {
            get
            {
                if (m_control.TheStyle != Style.VirtualList)
                    throw new InvalidOperationException(ExceptionTextOnlyAvailableOnVirtualList);

                return m_control.VirtualListSize;
            }

            set
            {
                if (m_control.TheStyle != Style.VirtualList)
                    throw new InvalidOperationException(ExceptionTextOnlyAvailableOnVirtualList);

                m_control.VirtualListSize = value;
            }
        }

        /// <summary>
        /// Gets the last hit item</summary>
        public object LastHit
        {
            get { return m_lastHit; }
            private set
            {
                // If value not changing then bail
                if (ReferenceEquals(value, m_lastHit))
                    return;

                m_lastHit = value;
                LastHitChanged.Raise(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the Node renderer to use</summary>
        public NodeRenderer Renderer
        {
            get { return m_control.Renderer; }
            set { m_control.Renderer = value; }
        }

        /// <summary>
        /// Gets or sets the top Node</summary>
        public Node TopItem
        {
            get
            {
                ListViewItem lstItem = m_control.TopItem;
                return lstItem == null ? null : lstItem.Tag as Node;
            }
            set
            {
                ListViewItem lstItem;
                if (!m_itemMap.TryGetValue(value, out lstItem))
                    return;

                // Sometimes setting the TopItem doesn't
                // actually make it work so you have to do
                // it twice (common issue found when Googling)

                m_control.TopItem = lstItem;
                if (m_control.TopItem != lstItem)
                    m_control.TopItem = lstItem;
            }
        }

        /// <summary>
        /// Calls BeginUpdate() on the underlying control</summary>
        public void BeginUpdate()
        {
            if (m_updateCount == 0)
                m_control.BeginUpdate();

            m_updateCount++;
        }

        /// <summary>
        /// Calls EndUpdate() on the underlying control</summary>
        public void EndUpdate()
        {
            m_updateCount--;

            // Clamp
            if (m_updateCount < 0)
                m_updateCount = 0;

            if (m_updateCount == 0)
            {
                Sort();
                m_control.EndUpdate();
            }
        }

        /// <summary>
        /// Gets or sets whether the control can accept data that the user drags onto it</summary>
        public bool AllowDrop
        {
            get { return m_control.AllowDrop; }
            set
            {
                if (m_control.AllowDrop == value)
                    return;

                if (!value && m_control.AllowDrop)
                {
                    m_control.DragEnter -= ControlDragEnter;
                    m_control.DragOver -= ControlDragOver;
                    m_control.DragLeave -= ControlDragLeave;
                    m_control.DragDrop -= ControlDragDrop;
                    m_control.ItemDrag -= ControlItemDrag;
                }

                m_control.AllowDrop = value;

                if (value)
                {
                    m_control.DragEnter += ControlDragEnter;
                    m_control.DragOver += ControlDragOver;
                    m_control.DragLeave += ControlDragLeave;
                    m_control.DragDrop += ControlDragDrop;
                    m_control.ItemDrag += ControlItemDrag;
                }
            }
        }

        /// <summary>
        /// Invalidates a node, forcing it to be redrawn</summary>
        /// <param name="node">Node</param>
        public void Invalidate(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            ListViewItem lstItem;
            if (!m_itemMap.TryGetValue(node, out lstItem))
                return;

            m_control.Invalidate(lstItem.Bounds);
        }

        /// <summary>
        /// Begins a drag-and-drop operation</summary>
        /// <param name="data">Data</param>
        /// <param name="allowedEffects">Allowed drag-and-drop effects</param>
        public void DoDragDrop(object data, DragDropEffects allowedEffects)
        {
            if (!AllowDrop)
                throw new InvalidOperationException("drag and drop not allowed");

            m_control.DoDragDrop(data, allowedEffects);
        }

        /// <summary>
        /// Gets or sets whether to use the insert queue</summary>
        internal bool UseInsertQueue { get; set; }

        /// <summary>
        /// Flushes the insert queue by adding all the pending items in the insert queue to the TreeListView</summary>
        internal void FlushInsertQueue()
        {
            if (m_insertQueue.Count <= 0)
                return;

            try
            {
                m_checkChanging = true;
                m_control.Items.AddRange(m_insertQueue.ToArray());
            }
            finally
            {
                m_checkChanging = false;
                m_insertQueue.Clear();
            }
        }

        /// <summary>
        /// Event fired when a node's expander icon is clicked
        /// and the node is not a leaf but has no children</summary>
        public event EventHandler<NodeEventArgs> NodeLazyLoad;

        /// <summary>
        /// Event fired when the last hit item changed</summary>
        public event EventHandler LastHitChanged;

        /// <summary>
        /// Event fired when a Node is selected</summary>
        public event EventHandler<ItemSelectedEventArgs<Node>> NodeSelected;

        /// <summary>
        /// Event fired when a Node is checked or unchecked</summary>
        public event EventHandler<NodeCheckedEventArgs> NodeChecked;

        /// <summary>
        /// Event fired when the underlying ListView is requesting data for a particular index</summary>
        public event EventHandler<RetrieveVirtualNodeEventArgs> RetrieveVirtualNode;

        /// <summary>
        /// Event fired when the Expanded property has changed</summary>
        public event EventHandler<NodeEventArgs> NodeExpandedChanged;

        /// <summary>
        /// Event fired when an object is dragged into the control's bounds</summary>
        public event EventHandler<DragEventArgs> DragEnter;

        /// <summary>
        /// Event fired when an object is dragged over the control's bounds</summary>
        public event EventHandler<DragEventArgs> DragOver;

        /// <summary>
        /// Event fired when an object is dragged out of the control's bounds</summary>
        public event EventHandler DragLeave;

        /// <summary>
        /// Event fired when a drag-and-drop operation is completed</summary>
        public event EventHandler<DragEventArgs> DragDrop;

        /// <summary>
        /// Event fired when a node is dragged</summary>
        public event EventHandler<NodeDragEventArgs> NodeDrag;

        /// <summary>
        /// Event fired that asks whether a node's label can be edited in-place</summary>
        /// <remarks>The event is fired only if the tree or the column the node is in allows label editing</remarks>
        public event EventHandler<CanLabelEditEventArgs> CanLabelEdit;

        /// <summary>
        /// Event fired after a node's label is edited</summary>
        public event EventHandler<NodeLabelEditEventArgs> AfterNodeLabelEdit;

        /// <summary>
        /// Event fired that asks whether a node's property can be changed</summary>
        /// <remarks>The event is fired only if the tree or the column the node is in allows property editing</remarks>
        public event EventHandler<CanPropertyChangeEventArgs> CanPropertyChange;

        /// <summary>
        /// Event fired after a node's property has changed</summary>
        public event EventHandler<PropertyChangedEventArgs> PropertyChanged;

        /// <summary>
        /// Gets or sets persisting column widths. Used with ISettingsService.</summary>
        public string PersistedSettings
        {
            get
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("Columns");
                xmlDoc.AppendChild(root);

                foreach (var kv in m_columnWidths)
                {
                    XmlElement columnElement = xmlDoc.CreateElement("Column");
                    root.AppendChild(columnElement);

                    columnElement.SetAttribute("Name", kv.Key);
                    columnElement.SetAttribute("Width", kv.Value.ToString());
                }

                return xmlDoc.InnerXml;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if ((root == null) || (root.Name != "Columns"))
                    throw new ArgumentException("invalid TreeListView settings");

                XmlNodeList columns = root.SelectNodes("Column");
                if (columns == null)
                    return;

                foreach (XmlElement columnElement in columns)
                {
                    string name = columnElement.GetAttribute("Name");
                    string widthString = columnElement.GetAttribute("Width");
                    int width;
                    if (!string.IsNullOrEmpty(widthString) && int.TryParse(widthString, out width))
                        m_columnWidths[name] = width;
                }

                try
                {
                    m_control.SuspendLayout();

                    foreach (Column column in Columns)
                        SetColumnWidth(column);
                }
                finally
                {
                    m_control.ResumeLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color TextColor
        {
            get { return m_control.TextColor; }
            set { m_control.TextColor = value; }
        }

        /// <summary>
        /// Gets or sets the modifiable text color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color ModifiableTextColor
        {
            get { return m_control.ModifiableTextColor; }
            set { m_control.ModifiableTextColor = value; ; }
        }

        /// <summary>
        ///  Gets or sets the highlight text color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color HighlightTextColor
        {
            get { return m_control.HighlightTextColor; }
            set { m_control.HighlightTextColor = value; }
        }

        /// <summary>
        /// Gets or sets the modifiable highlight text color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color ModifiableHighlightTextColor
        {
            get { return m_control.ModifiableHighlightTextColor; }
            set { m_control.ModifiableHighlightTextColor = value; }
        }

        /// <summary>
        /// Gets or sets the disabled text color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color DisabledTextColor
        {
            get { return m_control.DisabledTextColor; }
            set { m_control.DisabledTextColor = value; }
        }

        /// <summary>
        /// Gets or sets the background color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color BackColor
        {
            get { return m_control.BackColor; }
            set { m_control.BackColor = value; }
        }

        /// <summary>
        /// Gets or sets the highlight background color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color HighlightBackColor
        {
            get { return m_control.HighlightBackColor; }
            set { m_control.HighlightBackColor = value; }
        }

        /// <summary>
        /// Gets or sets the disabled background color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color DisabledBackColor
        {
            get { return m_control.DisabledBackColor; }
            set { m_control.DisabledBackColor = value; }
        }

        /// <summary>
        /// Gets or sets the grid lines color</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Color GridLinesColor
        {
            get { return m_control.GridLinesColor; }
            set { m_control.GridLinesColor = value; }
        }

        /// <summary>
        /// Gets or sets the expander gradient</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public ControlGradient ExpanderGradient
        {
            get { return m_control.ExpanderGradient; }
            set { m_control.ExpanderGradient = value; }
        }

        /// <summary>
        /// Gets or sets the expander and collapser pen</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Pen ExpanderPen
        {
            get { return m_control.ExpanderPen; }
            set { m_control.ExpanderPen = value; }
        }

        /// <summary>
        /// Gets or sets the hierarchy line pen</summary>
        /// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        public Pen HierarchyLinePen
        {
            get { return m_control.HierarchyLinePen; }
            set { m_control.HierarchyLinePen = value; }
        }

        ///// <summary>
        ///// Gets or sets the column header gradient</summary>
        ///// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        //public ControlGradient ColumnHeaderGradient
        //{
        //    get { return m_control.ColumnHeaderGradient; }
        //    set { m_control.ColumnHeaderGradient = value; }
        //}

        ///// <summary>
        ///// Gets or sets the column header separator color</summary>
        ///// <remarks>Wrapper to allow clients access to the internal colors in the ListView from the <see cref="Sce.Atf.Controls.TreeListView.NodeRenderer"/> class</remarks>
        //public Color ColumnHeaderSeparatorColor
        //{
        //    get { return m_control.ColumnHeaderSeparatorColor; }
        //    set { m_control.ColumnHeaderSeparatorColor = value; }
        //}

        #region IDisposable Interface

        /// <summary>
        /// Disposes of resources</summary>
        public void Dispose()
        {
            if (m_disposed)
                return;

            try
            {
                m_control.Dispose();

                m_sortTimer.Stop();
                m_sortTimer.Dispose();
            }
            finally
            {
                m_disposed = true;
            }
        }

        #endregion

        #region IAdaptable Interface

        /// <summary>
        /// Adapts the TreeListView to other types</summary>
        /// <param name="type">Type</param>
        /// <returns>Adapted type or null if no adaption possible</returns>
        public object GetAdapter(Type type)
        {
            return type.Equals(typeof(Control)) ? Control : null;
        }

        #endregion

        #region ListView Events

        private void ControlMouseDown(object sender, MouseEventArgs e)
        {
            EnsureEditingTerminated();
            
            var clientPoint = new Point(e.X, e.Y);
            SetLastHit(clientPoint);
        }

        private void ControlMouseUp(object sender, MouseEventArgs e)
        {
            var clientPoint = new Point(e.X, e.Y);
            SetLastHit(clientPoint);
        }

        private void ControlColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (m_addingColumn)
                return;

            ColumnHeader columnHeader = m_control.Columns[e.ColumnIndex];
            m_columnWidths[columnHeader.Text] = columnHeader.Width;
            ((Column)columnHeader.Tag).Width = columnHeader.Width;
        }

        private void ControlRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = GetVirtualListItemAtIndexOrLookup(e.ItemIndex);
        }

        private void ControlColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (m_sortColumn == e.Column)
            {
                m_sortOrder =
                    m_sortOrder == SortOrder.Ascending
                        ? SortOrder.Descending
                        : SortOrder.Ascending;
            }

            m_sortColumn = e.Column;
            m_defaultSorter.SortOrder = m_sortOrder;

            Sort();
        }

        private void ControlItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (m_selectionChanging)
                return;

            try
            {
                m_selectionChanging = true;

                var node = e.Item.Tag.As<Node>();
                if (node == null)
                    return;

                node.Selected = e.Item.Selected;

                NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node, e.IsSelected));
            }
            finally
            {
                m_selectionChanging = false;
            }
        }

        private void ControlSelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_virtualListAvoidSelectionRecursion)
                return;

            if (m_control.TheStyle != Style.VirtualList)
                throw new InvalidOperationException(ExceptionTextOnlyAvailableOnVirtualList);

            try
            {
                m_virtualListAvoidSelectionRecursion = true;
                var clearPreviousSelection = true;
                var nodeLastHit = LastHit.As<Node>();

                var keys = Control.ModifierKeys;
                if (keys == Keys.Control)
                    clearPreviousSelection = false;

                if (clearPreviousSelection)
                {
                    foreach (int index in m_virtualListOldSelectedIndices)
                    {
                        ListViewItem lstItem = GetVirtualListItemAtIndexOrLookup(index);
                        lstItem.Selected = false;

                        var node = (Node)lstItem.Tag;
                        node.Selected = false;

                        NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node, node.Selected));
                    }
                }

                foreach (int index in m_control.SelectedIndices)
                {
                    ListViewItem lstItem = GetVirtualListItemAtIndexOrLookup(index);

                    var node = (Node)lstItem.Tag;
                    node.Selected = true;

                    NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node, node.Selected));
                }

                if (keys == Keys.Shift)
                {
                    if ((m_virtualListSelectionLastHit != null) && (nodeLastHit != null))
                    {
                        int idx1 = m_virtualListSelectionLastHit.Index;
                        int idx2 = GetNodeIndex(nodeLastHit);

                        int startIndex = Math.Min(idx1, idx2);
                        int endIndex = Math.Max(idx1, idx2);

                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            ListViewItem lstItem = GetVirtualListItemAtIndexOrLookup(i);
                            lstItem.Selected = true;

                            var node = (Node)lstItem.Tag;
                            node.Selected = true;

                            NodeSelected.Raise(this, new ItemSelectedEventArgs<Node>(node, node.Selected));
                        }
                    }
                }
                else
                {
                    // Update last hit
                    if (nodeLastHit != null)
                        m_itemMap.TryGetValue(nodeLastHit, out m_virtualListSelectionLastHit);
                }

                m_virtualListOldSelectedIndices.Clear();
                m_virtualListOldSelectedIndices.AddRange(m_control.SelectedIndices.Cast<int>());
            }
            finally
            {
                m_virtualListAvoidSelectionRecursion = false;
            }
        }

        private void ControlDragEnter(object sender, DragEventArgs e)
        {
            DragEnter.Raise(this, e);
        }

        private void ControlDragOver(object sender, DragEventArgs e)
        {
            Point clientPoint = m_control.PointToClient(new Point(e.X, e.Y));
            SetLastHit(clientPoint);

            DragOver.Raise(this, e);
        }

        private void ControlDragLeave(object sender, EventArgs e)
        {
            DragLeave.Raise(this, e);
        }

        private void ControlDragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = m_control.PointToClient(new Point(e.X, e.Y));
            SetLastHit(clientPoint);

            DragDrop.Raise(this, e);
        }

        private void ControlItemDrag(object sender, ItemDragEventArgs e)
        {
            var item = e.Item.As<ListViewItem>();
            if (item == null)
                return;

            var node = item.Tag.As<Node>();
            if (node == null)
                return;

            NodeDrag.Raise(this, new NodeDragEventArgs(node, e.Button));
        }

        private void ControlMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                HandleLeftMouseClick(e);
        }

        private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var node = GetNodeAt(e.Location);
            if (node == null)
            {
                return;
            }

            int columnIndex = GetNodeColumnIndexAt(e.Location);
            if (columnIndex < 0)
            {
                return;
            }

            var hitinfo = m_control.HitTest(e.X, e.Y);
            if (columnIndex == 0)
            {
                if (!AllowLabelEdit || !node.LabelHitRect.Contains(e.Location))
                {
                    return;
                }

                BeginLabelEdit(node);
            }
            else
            {
                if (!(AllowPropertyEdit || m_columns.ElementAt(columnIndex).AllowPropertyEdit))
                {
                    return;
                }

                // ask if the node's property can be changed
                var args = new CanPropertyChangeEventArgs(node, columnIndex - 1);
                CanPropertyChange.Raise(this, args);
                if (!args.CanChange)
                    return;
                
                m_editBox.Bounds = hitinfo.SubItem.Bounds;
                m_currentEditNode = node;
                m_currentEditColumnIndex = columnIndex;

                m_editBox.Text = hitinfo.SubItem.Text;
                m_editBox.Show();
                m_editBox.Focus();
            }
        }

        private void ControlScroll(object sender, ScrollEventArgs e)
        {
            EnsureEditingTerminated();
        }

        private void EditBoxLostFocus(object sender, EventArgs e)
        {
            EnsureEditingTerminated();
        }

        private void EditBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != (char)Keys.Enter)
                return;

            try
            {
                EnsureEditingTerminated();
            }
            finally
            {
                e.Handled = true;
            }
        }

        #endregion

        /// <summary>
        /// Internal/default Node sorter to use on the TreeListView</summary>
        private class DefaultSorter : IComparer<Node>
        {
            /// <summary>
            /// Compares two Nodes</summary>
            /// <param name="x">Node x</param>
            /// <param name="y">Node y</param>
            /// <returns>0 if Nodes identical, -1 if Node x before Node y, or 1 if Node x after Node y</returns>
            public int Compare(Node x, Node y)
            {
                int result = string.Compare(x.Label, y.Label);

                // Try some tie breakers
                if (result == 0)
                    result = CompareProperties(x.Properties, y.Properties);

                if (SortOrder == SortOrder.Descending)
                    result *= -1;

                return result;
            }

            /// <summary>
            /// Gets or sets the sort order</summary>
            public SortOrder SortOrder { private get; set; }

            private static int CompareProperties(object[] props1, object[] props2)
            {
                if ((props1 == null) && (props2 == null))
                    return 0;

                if (props1 == null)
                    return 1;

                if (props2 == null)
                    return -1;

                if (props1.Length != props2.Length)
                    return props1.Length < props2.Length ? -1 : 1;

                int result = 0;

                for (int i = 0; i < props1.Length; i++)
                {
                    result = string.Compare(props1[i].ToString(), props2[i].ToString());
                    if (result != 0)
                        break;
                }

                return result;
            }
        }

        private void ColumnsColumnAdding(object sender, CancelColumnEventArgs e)
        {
            e.Cancel = !AddColumn(e.Column);
        }

        private void ColumnsColumnRemoving(object sender, ColumnEventArgs e)
        {
            RemoveColumn(e.Column);
        }

        private void ColumnsColumnClearAll(object sender, EventArgs e)
        {
            ClearColumns();
        }

        private bool AddColumn(Column column)
        {
            try
            {
                m_addingColumn = true;

                if (m_columnMap.ContainsKey(column))
                    return false;

                var columnHeader =
                    new ColumnHeader
                        {
                            Text = column.Label,
                            Width = column.Width,
                            Tag = column
                        };

                m_control.Columns.Add(columnHeader);
                m_columnMap.Add(column, columnHeader);

                if (m_columnWidths.ContainsKey(column.Label))
                {
                    column.Width = m_columnWidths[column.Label];
                    columnHeader.Width = column.Width;
                }
                else
                {
                    m_columnWidths[column.Label] = column.Width;
                }

                if (m_dictColumnAllowEditCache.ContainsKey(column.Label))
                {
                    column.AllowPropertyEdit = m_dictColumnAllowEditCache[column.Label];
                }
                else
                {
                    m_dictColumnAllowEditCache[column.Label] = column.AllowPropertyEdit;
                }

                column.LabelChanged += ColumnLabelChanged;
                column.WidthChanged += ColumnWidthChanged;
                column.AllowPropertyEditChanged += ColumnAllowPropertyEditChanged;

                return true;
            }
            finally
            {
                m_addingColumn = false;
            }
        }

        private void ColumnLabelChanged(object sender, EventArgs e)
        {
            var column = (Column)sender;

            ColumnHeader columnHeader;
            if (!m_columnMap.TryGetValue(column, out columnHeader))
                return;

            columnHeader.Text = column.Label;
        }

        private void ColumnWidthChanged(object sender, EventArgs e)
        {
            var column = (Column)sender;

            ColumnHeader columnHeader;
            if (!m_columnMap.TryGetValue(column, out columnHeader))
                return;

            columnHeader.Width = column.Width;
        }

        private void ColumnAllowPropertyEditChanged(object sender, EventArgs e)
        {
            // TODO: if column label changes we don't update the dictionary to
            // TODO: remove the old label entry...
            var column = (Column)sender;
            m_dictColumnAllowEditCache[column.Label] = column.AllowPropertyEdit;
        }

        private void RemoveColumn(Column column)
        {
            ColumnHeader columnHeader;
            if (!m_columnMap.TryGetValue(column, out columnHeader))
                return;

            column.LabelChanged -= ColumnLabelChanged;
            column.WidthChanged -= ColumnWidthChanged;

            m_control.Columns.Remove(columnHeader);
            m_columnMap.Remove(column);
        }

        private void ClearColumns()
        {
            m_control.Columns.Clear();
            m_columnMap.Clear();
        }

        private void NodeCollectionNodeAdding(object sender, CancelNodeEventArgs e)
        {
            e.Cancel = !AddNode(e.Node);
        }

        private void NodeCollectionNodeAdded(object sender, NodeEventArgs e)
        {
            Sort();
        }

        private void NodeCollectionNodeRemoving(object sender, NodeEventArgs e)
        {
            RemoveNode(e.Node);
        }

        private void NodeCollectionNodesRemoving(object sender, NodesRemovingEventArgs e)
        {
            RemoveNodes(e.Owner, e.Nodes);
        }

        private bool AddNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (m_control.TheStyle == Style.VirtualList)
                throw new InvalidOperationException(ExceptionTextAddingNotAllowedInVirtualList);

            node.Visible = true;

            // No duplicates... for now.
            if (m_itemMap.ContainsKey(node))
                return false;

            // Don't add to GUI but everything was fine
            if (!node.Visible)
                return true;

            ListViewItem lstItem =
                CreateListViewItem(
                    node,
                    m_control.Columns.Count);

            if (UseInsertQueue)
                m_insertQueue.Add(lstItem);
            else
                m_control.Items.Add(lstItem);

            m_needSorting = true;
            m_itemMap.Add(node, lstItem);

            // Watch Node changes
            {
                node.LabelChanged += NodePropertyLabelChanged;
                node.ExpandedChanged += NodePropertyExpandedChanged;
                node.CheckStateChanged += NodePropertyCheckStateChanged;
                node.PropertiesChanged += NodePropertyPropertiesChanged;
                node.ImageIndexChanged += NodePropertyImageIndexChanged;
                node.StateImageIndexChanged += NodePropertyStateImageIndexChanged;
                node.SelectedChanged += NodePropertySelectedChanged;
                node.FontStyleChanged += NodePropertyFontStyleChanged;
                node.HoverTextChanged += NodePropertyHoverTextChanged;

                node.Nodes.NodeAdding += NodeCollectionNodeAdding;
                node.Nodes.NodeAdded += NodeCollectionNodeAdded;
                node.Nodes.NodeRemoving += NodeCollectionNodeRemoving;
                node.Nodes.NodesRemoving += NodeCollectionNodesRemoving;
            }

            if (node.Expanded)
            {
                // Recursively add children
                foreach (Node subNode in node.Nodes)
                    AddNode(subNode);
            }

            return true;
        }

        private void UpdateNode(Node node, NodeChangeTypes changeTypes)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            ListViewItem lstItem;
            if (!m_itemMap.TryGetValue(node, out lstItem))
                return;

            var shouldDoBeginEndUpdate = !m_selectionChanging;

            try
            {
                if (shouldDoBeginEndUpdate)
                    BeginUpdate();

                if (IsSet(NodeChangeTypes.Label, changeTypes))
                    lstItem.Text = node.Label;

                if (IsSet(NodeChangeTypes.Expanded, changeTypes))
                    ExpandOrCollapse(node);

                if (!m_checkChanging && IsSet(NodeChangeTypes.CheckState, changeTypes))
                    lstItem.Checked = node.CheckState == CheckState.Checked;

                if (IsSet(NodeChangeTypes.Properties, changeTypes))
                    UpdateProperties(node, lstItem, Columns.Count);
                
                if (IsSet(NodeChangeTypes.ImageIndex, changeTypes))
                    lstItem.ImageIndex = node.ImageIndex;

                // ListViewItem.StateImageIndex can only be 0-14. Weird.
                // Lets work around that by simply using what's stored in Node.
                // So, keep this code commented out or ArgumentOutOfRangeException's
                // get thrown.
                //if (IsSet(NodeChangeTypes.StateImageIndex, changeTypes))
                //    lstItem.StateImageIndex = node.StateImageIndex;

                if (!m_selectionChanging && IsSet(NodeChangeTypes.Selected, changeTypes))
                    lstItem.Selected = node.Selected;

                if (IsSet(NodeChangeTypes.FontStyle, changeTypes))
                    Invalidate(node);

                if (IsSet(NodeChangeTypes.HoverText, changeTypes))
                {
                    lstItem.ToolTipText = node.HoverText;
                    Invalidate(node);
                }
            }
            finally
            {
                if (shouldDoBeginEndUpdate)
                    EndUpdate();
            }
        }

        private void RemoveNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (m_control.TheStyle == Style.VirtualList)
                throw new InvalidOperationException(ExceptionTextRemovingNotAllowedInVirtualList);

            ListViewItem lstItem;
            if (!m_itemMap.TryGetValue(node, out lstItem))
                return;

            // Remove watched Node changes
            {
                node.Nodes.NodeAdding -= NodeCollectionNodeAdding;
                node.Nodes.NodeAdded -= NodeCollectionNodeAdded;
                node.Nodes.NodeRemoving -= NodeCollectionNodeRemoving;
                node.Nodes.NodesRemoving -= NodeCollectionNodesRemoving;

                node.LabelChanged -= NodePropertyLabelChanged;
                node.ExpandedChanged -= NodePropertyExpandedChanged;
                node.CheckStateChanged -= NodePropertyCheckStateChanged;
                node.PropertiesChanged -= NodePropertyPropertiesChanged;
                node.ImageIndexChanged -= NodePropertyImageIndexChanged;
                node.StateImageIndexChanged -= NodePropertyStateImageIndexChanged;
                node.SelectedChanged -= NodePropertySelectedChanged;
                node.FontStyleChanged -= NodePropertyFontStyleChanged;
                node.HoverTextChanged -= NodePropertyHoverTextChanged;

                // clear selection now that handlers are removed
                try
                {
                    m_selectionChanging = true;
                    node.Selected = false;
                    lstItem.Selected = false;
                }
                finally
                {
                    m_selectionChanging = false;
                }
            }

            m_control.Items.Remove(lstItem);
            m_itemMap.Remove(node);

            m_needSorting = true;
        }

        private void RemoveNodes(Node node, IEnumerable<Node> nodes)
        {
            bool startValue = m_expandingCollapsing;

            try
            {
                m_needSorting = true;
                m_expandingCollapsing = true;

                try
                {
                    BeginUpdate();

                    foreach (Node child in nodes)
                        RemoveNode(child);

                    if ((node != null) && node.Expanded)
                    {
                        node.Expanded = false;
                        InvalidateNode(node);
                    }
                }
                finally
                {
                    EndUpdate();
                }
            }
            finally
            {
                m_expandingCollapsing = startValue;
            }
        }

        private void NodePropertyLabelChanged(object sender, EventArgs e)
        {
            UpdateNode((Node)sender, NodeChangeTypes.Label);
        }

        private void NodePropertyExpandedChanged(object sender, EventArgs e)
        {
            var node = (Node)sender;

            NodeExpandedChanged.Raise(this, new NodeEventArgs(node));

            if (m_expandingCollapsing)
                return;

            if (!LazyLoadNode(node))
            {
                if (!node.Expandable && node.IsLeaf)
                    return;

                ExpandOrCollapse(node);
            }
        }

        private void NodePropertyCheckStateChanged(object sender, EventArgs e)
        {
            var node = (Node)sender;
            UpdateNode(node, NodeChangeTypes.CheckState);
            NodeChecked.Raise(this, new NodeCheckedEventArgs(node));

            if (m_recursiveCheckBoxes && TheStyle == Style.CheckedTreeList && !m_doingRecursiveCheckStateChange)
            {
                m_doingRecursiveCheckStateChange = true;
                SetCheckStateRecursive(node.Nodes, node.CheckState);
                m_doingRecursiveCheckStateChange = false;
            }
        }

        private void SetCheckStateRecursive(IEnumerable<Node> nodes, CheckState checkState)
        {
            foreach (var node in new List<Node>(nodes))
            {
                node.CheckState = checkState;
                NodeChecked.Raise(this, new NodeCheckedEventArgs(node));
                SetCheckStateRecursive(node.Nodes, checkState);
            }
        }

        private void NodePropertyPropertiesChanged(object sender, EventArgs e)
        {
            UpdateNode((Node)sender, NodeChangeTypes.Properties);
        }

        private void NodePropertyImageIndexChanged(object sender, EventArgs e)
        {
            UpdateNode((Node)sender, NodeChangeTypes.ImageIndex);
        }

        private void NodePropertyStateImageIndexChanged(object sender, EventArgs e)
        {
            UpdateNode((Node)sender, NodeChangeTypes.StateImageIndex);
        }

        private void NodePropertySelectedChanged(object sender, EventArgs e)
        {
            UpdateNode((Node)sender, NodeChangeTypes.Selected);
        }

        private void NodePropertyFontStyleChanged(object sender, EventArgs e)
        {
            UpdateNode((Node)sender, NodeChangeTypes.FontStyle);
        }

        private void NodePropertyHoverTextChanged(object sender, EventArgs e)
        {
            UpdateNode((Node)sender, NodeChangeTypes.HoverText);
        }

        private static void SetupHierarchy(NodeCollection nodes, ref UInt64 number, IComparer<Node> comparer)
        {
            nodes.Sort(comparer);

            // Set up proper visual ordering
            foreach (Node child in nodes)
            {
                child.VisualPosition = ++number;
                SetupNodeHierarchy(child, ref number, comparer);
            }
        }

        private static void SetupNodeHierarchy(Node node, ref UInt64 number, IComparer<Node> comparer)
        {
            if (node == null)
                return;

            if (node.Parent != null)
            {
                node.Level = node.Parent.Level + 1;

                if (!node.Parent.Visible)
                    node.Visible = false;
            }
            else
            {
                // Node level's start @ 1

                node.Level = 1;
            }

            if (!node.HasChildren)
                return;

            // Sort children and arrange in proper visual order
            node.Nodes.Sort(comparer);

            Node lastNode = null;
            foreach (Node child in node.Nodes)
            {
                child.Previous = lastNode;
                child.Next = null;

                if (lastNode != null)
                    lastNode.Next = child;

                lastNode = child;

                child.VisualPosition = ++number;
                SetupNodeHierarchy(child, ref number, comparer);
            }
        }

        private void SetLastHit(Point clientPoint)
        {
            bool bSetLastHit = false;

            try
            {
                object lastHit =
                    m_control.GetItemAt(
                        clientPoint.X,
                        clientPoint.Y);

                if (lastHit == null)
                    return;

                if (!(lastHit is ListViewItem))
                    return;

                bSetLastHit = true;
                SetLastHit((ListViewItem)lastHit);
            }
            finally
            {
                // If we didn't set the LastHit
                // property then finally set it
                if (!bSetLastHit)
                    LastHit = this;
            }
        }

        private void SetLastHit(ListViewItem item)
        {
            LastHit = item.Tag;
        }

        private void HandleLeftMouseClick(MouseEventArgs e)
        {
            object item = LastHit;
            if ((item == null) || (item == this))
                return;

            var node = item as Node;
            if (node == null)
                return;

            ListViewItem lstItem;
            if (!m_itemMap.TryGetValue(node, out lstItem))
                return;

            // Hit test against expander buttons
            if ((m_control.TheStyle == Style.TreeList || m_control.TheStyle == Style.CheckedTreeList) 
                && node.HitRect.Contains(e.Location))
            {
                node.Expanded = !node.Expanded;
                return;
            }

            // Hit test against check-boxes
            if ((m_control.TheStyle == Style.CheckedList || m_control.TheStyle == Style.CheckedTreeList)
                && node.CheckBoxHitRect.Contains(e.Location))
            {
                node.CheckState = node.CheckState == CheckState.Checked ? CheckState.Unchecked : CheckState.Checked;
                return;
            }
        }

        private void InvalidateNode(Node node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            ListViewItem lstItem;
            if (!m_itemMap.TryGetValue(node, out lstItem))
                return;

            m_control.Invalidate(lstItem.GetBounds(ItemBoundsPortion.Entire));
        }

        private void ExpandOrCollapse(Node node)
        {
            if (m_expandingCollapsing)
                return;

            try
            {
                m_expandingCollapsing = true;

                try
                {
                    BeginUpdate();

                    // The logic might look backwards but actually the Node's Expanded
                    // property has already been changed by the time we reach this code.

                    if (node.Expanded)
                    {
                        ExpandNode(node);
                        node.Expanded = true;

                        Sort();
                    }
                    else
                    {
                        CollapseNode(node);
                        node.Expanded = false;

                        InvalidateNode(node);
                    }
                }
                finally
                {
                    EndUpdate();
                }
            }
            finally
            {
                m_expandingCollapsing = false;
            }
        }

        private void ExpandNode(Node node)
        {
            foreach (Node n in node.Nodes)
            {
                AddNode(n);

                if (n.HasChildren && n.Expanded)
                    ExpandNode(n);
            }
        }

        private void CollapseNode(Node node)
        {
            foreach (Node n in node.Nodes)
            {
                if (!n.Visible)
                    continue;

                RemoveNode(n);

                if (!n.HasChildren)
                    continue;

                if (n.Expanded)
                    CollapseNode(n);
            }
        }

        private bool LazyLoadNode(Node node)
        {
            if (node == null)
                return false;

            if (!node.NeedsLazyLoad)
                return false;

            NodeLazyLoad.Raise(this, new NodeEventArgs(node));

            bool oldValue = m_expandingCollapsing;
            try
            {
                m_expandingCollapsing = true;
                node.Expanded = true;
            }
            finally
            {
                m_expandingCollapsing = oldValue;
            }

            // If lazy load added no children then set property
            // so expander/collapser will disappear when drawing
            if (!node.HasChildren)
            {
                node.IsLeaf = true;
                InvalidateNode(node);
            }

            return true;
        }

        private void ExpandAll(Node node)
        {
            if (node.Expandable || !node.IsLeaf || node.HasChildren)
            {
                if (!node.Expanded)
                {
                    node.Expanded = true;
                    if (!LazyLoadNode(node))
                        ExpandNode(node);
                }
            }

            var children = new List<Node>(node.Nodes);
            foreach (Node child in children)
                ExpandAll(child);
        }

        private void CollapseAll(Node node)
        {
            node.Expanded = false;
            CollapseNode(node);

            var children = new List<Node>(node.Nodes);
            foreach (Node child in children)
                CollapseAll(child);
        }

        private void SetColumnWidth(Column column)
        {
            if (column == null)
                return;

            ColumnHeader columnHeader;
            if (!m_columnMap.TryGetValue(column, out columnHeader))
                return;

            int width;
            if (m_columnWidths.TryGetValue(column.Label, out width))
            {
                column.Width = width;
                columnHeader.Width = width;
            }
            else
                m_columnWidths[columnHeader.Text] = column.Width;
        }

        private static ListViewItem CreateListViewItem(Node node, int columnCount)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            // ListViewItem.StateImageIndex can only be 0-14. Weird.
            // Lets work around that by simply using what's store in Node.

            var lstItem =
                new ListViewItem(node.Label)
                    {
                        Tag = node,
                        ImageIndex = node.ImageIndex,
                        Checked = node.CheckState == CheckState.Checked,
                        ToolTipText = node.HoverText
                    };

            // Grab column-item details
            UpdateProperties(node, lstItem, columnCount);

            return lstItem;
        }

        private static void UpdateProperties(Node node, ListViewItem lstItem, int columns)
        {
            if (lstItem == null)
                return;

            if (node.Properties == null)
                return;

            int count = Math.Min(columns, node.Properties.Length);
            for (int i = 0; i < count; i++)
            {
                bool subItemExists = lstItem.SubItems.Count > (i + 1);
                object value = node.Properties[i];

                if (subItemExists)
                {
                    ListViewItem.ListViewSubItem subItem =
                        lstItem.SubItems[i + 1];

                    subItem.Tag = node;
                    subItem.Text = GetObjectString(value);
                }
                else
                {
                    var subItem =
                        new ListViewItem.ListViewSubItem(
                            lstItem,
                            GetObjectString(value)) {Tag = node};

                    lstItem.SubItems.Add(subItem);
                }
            }
        }

        private static string GetObjectString(object value)
        {
            var formattable = value as IFormattable;
            return
                formattable != null
                    ? formattable.ToString(null, null)
                    : value.ToString();
        }

        private static bool IsSet(NodeChangeTypes type, NodeChangeTypes changes)
        {
            return (changes & type) == type;
        }

        private void SortTimerTick(object sender, EventArgs e)
        {
            if (m_needSorting)
                Sort();
        }

        private ListViewItem GetVirtualListItemAtIndexOrLookup(int index)
        {
            if (m_control.TheStyle != Style.VirtualList)
                throw new InvalidOperationException(ExceptionTextOnlyAvailableOnVirtualList);

            if ((index < 0) || (index >= VirtualListSize))
                throw new ArgumentOutOfRangeException("index");

            ListViewItem listItem;
            if (m_virtualItemMap.TryGetValue(index, out listItem))
                return listItem;

            var ea = new RetrieveVirtualNodeEventArgs(index);
            RetrieveVirtualNode.Raise(this, ea);
            if (ea.Node == null)
                throw new NullReferenceException("client code returned null");

            // Convert user data
            listItem = CreateListViewItem(ea.Node, m_columns.Count);

            m_itemMap.Add(ea.Node, listItem);
            m_virtualItemMap.Add(index, listItem);

            return listItem;
        }

        private void EnsureEditingTerminated()
        {
            try
            {
                if (m_currentEditNode != null)
                {
                    try
                    {
                        string value = m_editBox.Text;
                        if (m_currentEditColumnIndex == 0)
                        {
                            var args = new NodeLabelEditEventArgs(m_currentEditNode, value);
                            AfterNodeLabelEdit.Raise(this, args);

                            ListViewItem lstItem;
                            if (!m_itemMap.TryGetValue(m_currentEditNode, out lstItem))
                                return;

                            if (!args.CancelEdit)
                            {
                                lstItem.Text = value;
                                m_currentEditNode.Label = value;
                            }
                        }
                        else
                        {
                            TrySetNodeProperty(m_currentEditNode, m_currentEditColumnIndex - 1, value);
                        }
                    }
                    finally
                    {
                        // Hide() triggers EnsureEditingTerminated to get
                        // called again so clear the current edit node first
                        m_currentEditNode = null;
                        m_editBox.Hide();
                    }
                }
            }
            finally
            {
                m_currentEditNode = null;
                m_currentEditColumnIndex = -1;
            }
        }

        private void TrySetNodeProperty(Node node, int propertyIndex, string value)
        {
            object oldValue = node.Properties[propertyIndex];
            node.SetProperty(propertyIndex, value);

            if (PropertyChanged != null)
            {
                var args = new PropertyChangedEventArgs(node, propertyIndex, value);
                PropertyChanged(this, args);
                if (args.CancelChange)
                {
                    node.SetProperty(propertyIndex, oldValue);
                }
            }
        }

        private bool m_disposed;
        private object m_lastHit;

        private int m_sortColumn;
        private int m_updateCount;
        private bool m_addingColumn;
        private bool m_sorting;
        private bool m_needSorting;
        private bool m_checkChanging;
        private bool m_expandingCollapsing;
        private bool m_selectionChanging;
        private SortOrder m_sortOrder;
        private IComparer<Node> m_sorter;
        private bool m_virtualListAvoidSelectionRecursion;
        private ListViewItem m_virtualListSelectionLastHit;
        private Node m_currentEditNode;
        private int m_currentEditColumnIndex;
        private bool m_recursiveCheckBoxes;
        private bool m_doingRecursiveCheckStateChange;

        private readonly Timer m_sortTimer;
        private readonly NodeCollection m_nodes;
        private readonly TheTreeListView m_control;
        private readonly ColumnCollection m_columns;
        private readonly ListViewItemSorter m_listViewItemSorter;
        private readonly TextBox m_editBox;
        
        private readonly List<ListViewItem> m_insertQueue =
            new List<ListViewItem>();

        private readonly DefaultSorter m_defaultSorter =
            new DefaultSorter();

        private readonly Dictionary<string, int> m_columnWidths =
            new Dictionary<string, int>();

        private readonly Dictionary<Node, ListViewItem> m_itemMap =
            new Dictionary<Node, ListViewItem>();

        private readonly List<int> m_virtualListOldSelectedIndices =
            new List<int>();

        private readonly Dictionary<int, ListViewItem> m_virtualItemMap =
            new Dictionary<int, ListViewItem>();

        private readonly Dictionary<Column, ColumnHeader> m_columnMap =
            new Dictionary<Column, ColumnHeader>();

        private readonly Dictionary<string, bool> m_dictColumnAllowEditCache =
            new Dictionary<string, bool>();

        private const string ExceptionTextOnlyAvailableOnVirtualList = "only available in virtual list mode";
        private const string ExceptionTextSortingNotAllowedInVirtualList = "sorting not allowed in virtual list mode";
        private const string ExceptionTextAddingNotAllowedInVirtualList = "adding not allowed in virtual list mode";
        private const string ExceptionTextRemovingNotAllowedInVirtualList = "removing not allowed in virtual list mode";

        /// <summary>
        /// Invalid image index value</summary>
        public const int InvalidImageIndex = -1;
    }
}