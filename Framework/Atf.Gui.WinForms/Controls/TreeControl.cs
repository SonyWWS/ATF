//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// A Tree Control to display hierarchical data. Handles user input, including editing. Decides the
    /// placement of the images, check box, and label of each item. The TreeItemRenderer decides how to
    /// draw the items. The TreeControlAdapter adapts this tree control to the ITreeView interface.</summary>
    public class TreeControl : Control
    {
        /// <summary>
        /// Tree style</summary>
        public enum Style
        {
            /// <summary>
            /// Visual Studio-like tree control, with hierarchy shown by indentation and connections to parent</summary>
            Tree,

            /// <summary>
            /// Windows Explorer-like tree control, with hierarchy shown by indentation</summary>
            SimpleTree,

            /// <summary>
            /// Windows Office-like categorized palette</summary>
            CategorizedPalette,
        }

        /// <summary>
        /// Constructor of a Visual Studio-like tree control, with default rendering using a single font
        /// for the entire control</summary>
        public TreeControl()
            : this(Style.Tree, null)
        {
        }

        /// <summary>
        /// Constructor of a tree control with the given style and default rendering using a single font
        /// for the entire control</summary>
        /// <param name="style">Tree style</param>
        public TreeControl(Style style)
            : this(style, null)
        {
        }

        /// <summary>
        /// Constructor of a tree control with a particular style and renderer</summary>
        /// <param name="style">Tree style</param>
        /// <param name="itemRenderer">Renderer of a node in the tree. If null, then a new
        /// TreeItemRenderer is created and used</param>
        public TreeControl(Style style, TreeItemRenderer itemRenderer)
        {
            m_style = style;
            if (m_style == Style.CategorizedPalette)
                m_showRoot = false;

            m_root = new Node(this, null, null);

            m_dragHoverExpandTimer = new Timer();
            m_dragHoverExpandTimer.Interval = 1000; // 1 second delay for drag hover expand
            m_dragHoverExpandTimer.Tick += DragHoverTimerTick;

            m_autoScrollTimer = new Timer();
            m_autoScrollTimer.Interval = 200; // 5 Hz auto scroll rate
            m_autoScrollTimer.Tick += AutoScrollTimerTick;

            m_editLabelTimer = new Timer();
            m_editLabelTimer.Tick += EditLabelTimerTick;

            m_averageRowHeight = FontHeight + Margin.Top;

            SuspendLayout();

            m_textBox = new TextBox();
            m_textBox.Visible = false;
            m_textBox.BorderStyle = BorderStyle.None;

            m_textBox.KeyDown += TextBoxKeyDown;
            m_textBox.KeyPress += TextBoxKeyPress;
            m_textBox.LostFocus += TextBoxLostFocus;

            m_vScrollBar = new VScrollBar();
            m_vScrollBar.Dock = DockStyle.Right;
            m_vScrollBar.SmallChange = m_averageRowHeight;
            m_vScrollBar.ValueChanged += VerticalScrollBarValueChanged;

            m_hScrollBar = new HScrollBar();
            m_hScrollBar.Dock = DockStyle.Bottom;
            m_vScrollBar.SmallChange = 8;
            m_hScrollBar.ValueChanged += HorizontalScrollBarValueChanged;

            Controls.Add(m_vScrollBar);
            Controls.Add(m_hScrollBar);
            Controls.Add(m_textBox);

            ResumeLayout();

            BackColor = SystemColors.Window;

            base.DoubleBuffered = true;

            SetStyle(ControlStyles.ResizeRedraw, true);

            if (itemRenderer == null)
                itemRenderer = new TreeItemRenderer();
            ItemRenderer = itemRenderer;

            m_filterImage = ResourceUtil.GetImage16(Resources.FilterImage) as Bitmap;
            m_toolTip = new ToolTip();
        }

        /// <summary>
        /// Disposes of unmanaged resources</summary>
        /// <param name="disposing">Whether or not managed resources should be disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_dragHoverExpandTimer.Dispose();
                m_autoScrollTimer.Dispose();
                m_editLabelTimer.Dispose();
                m_toolTip.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the root node of the tree</summary>
        public Node Root
        {
            get { return m_root; }
        }

        /// <summary>
        /// Gets or sets whether the root node should be displayed</summary>
        public bool ShowRoot
        {
            get { return m_showRoot; }
            set
            {
                if (m_style != Style.CategorizedPalette &&
                    m_showRoot != value)
                {
                    m_showRoot = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the renderer responsible for drawing the items that are in the tree control.
        /// This allows for advanced customization, such as using multiple fonts and colors. Note
        /// that if this TreeControl is a TreeListControl, then the TreeItemRenderer must be a
        /// TreeListItemRenderer.</summary>
        public TreeItemRenderer ItemRenderer
        {
            get { return m_itemRenderer; }
            set
            {
                bool changed = m_itemRenderer != value;
                m_itemRenderer = value;
                m_itemRenderer.Owner = this;
                Indent = Indent;// make sure that the Indent is sufficient, given a new expander size
                Invalidate();
                if (changed)
                    OnItemRendererChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the number of pixels that the control should indent a
        /// Node in the hierarchy, relative to its parent</summary>
        public int Indent
        {
            get { return m_indent; }
            set
            {
                if (m_style != Style.CategorizedPalette)
                    value = Math.Max(m_itemRenderer.ExpanderSize.Width + Margin.Left, value);

                if (m_indent != value)
                {
                    m_indent = value;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the ImageList used for Node images</summary>
        public ImageList ImageList
        {
            get { return m_imageList; }
            set { m_imageList = value; }
        }

        /// <summary>
        /// Gets or sets the ImageList used for Node state images</summary>
        public ImageList StateImageList
        {
            get { return m_stateImageList; }
            set { m_stateImageList = value; }
        }

        /// <summary>
        /// Gets or sets the selection mode; SelectionMode.MultiSimple is not supported</summary>
        public SelectionMode SelectionMode
        {
            get { return m_selectionMode; }
            set
            {
                if (m_selectionMode != value)
                {
                    m_selectionMode = value;
                    ClearSelection();
                }
            }
        }

        /// <summary>
        /// Gets the height and width of the client area of the control actual available for drawing,
        /// excluding horizontal and vertical scrollbars </summary>
        internal Size ActualClientSize
        {
            get { return m_clientSize; }
        }

        /// <summary>
        /// Gets or sets whether Nodes are automatically expanded when
        /// the user hovers the mouse over them during drag and drop</summary>
        public bool DragHoverExpand
        {
            get { return m_dragHoverExpand; }
            set { m_dragHoverExpand = value; }
        }

        /// <summary>
        /// Gets or sets the delay for a node to auto-expand when the user hovers over
        /// it during a drag and drop, in milliseconds</summary>
        public int AutoExpandDelay
        {
            get { return m_dragHoverExpandTimer.Interval; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("delay must be > 0");
                m_dragHoverExpandTimer.Interval = value;
            }
        }

        /// <summary>
        /// Gets or sets the auto scrolling speed, in pixels. Auto scroll updates at
        /// a rate of 5 Hz.</summary>
        public int AutoScrollSpeed
        {
            get { return m_autoScrollSpeed; }
            set { m_autoScrollSpeed = value; }
        }

        /// <summary>
        /// Gets or sets whether or not the tree should scroll automatically when a node is expanded
        /// to show as many children as possible, like Windows Explorer and Visual Studio's
        /// Solution Explorer. True by default.</summary>
        public bool AutoScrollOnExpand
        {
            get { return m_autoScrollOnExpand; }
            set { m_autoScrollOnExpand = value; }
        }

        /// <summary>
        /// Gets and sets whether or not a node should be
        /// expanded or collapsed when the user double-clicks on the node, based
        /// on the current expand/collapse state of the node</summary>   
        public bool ToggleOnDoubleClick
        {
            get { return m_toggleOnDoubleClick; }
            set { m_toggleOnDoubleClick = value; }
        }

        /// <summary>
        /// Gets and sets whether or not a node should be
        /// expanded upon a single click, like Windows Explorer</summary>   
        public bool ExpandOnSingleClick
        {
            get { return m_expandOnSingleClick; }
            set { m_expandOnSingleClick = value; }
        }

        /// <summary>
        /// Gets whether a dragged object is between the adjacent nodes of the tree</summary>   
        public bool DragBetween
        {
            get { return m_dragBetween; }
        }

        /// <summary>
        /// Gets or sets whether the control should display drag-between cue</summary>   
        public bool ShowDragBetweenCue
        {
            get { return m_showDragBetweenCue; }
            set { m_showDragBetweenCue = value; }
        }

        /// <summary>
        /// Gets or sets whether a navigation key is changing the current selection</summary>
        public bool NavigationKeyChangingSelection { get; set; }

        /// <summary>
        /// Keyboard shortcuts style for tree control navigation</summary>
        [Flags]
        public enum KeyboardShortcuts
        {
            /// <summary>
            /// Up and Down Arrow goes up or down the hierarchy</summary>
            UpDownNav = 0x01,
            /// <summary>
            /// Right Arrow expands the current selection if it is not expanded, otherwise goes to the first child.
            /// Left Arrow collapses the current selection if it is expanded, otherwise goes to the parent.</summary>
            LeftRightExpand = 0x02,
            /// <summary>
            /// Home key jumps to top; End key jumps to bottom</summary>
            HomeEnd = 0x04,
            /// <summary>
            /// Page Up jumps up one windows distance; Page Down jumps down one page</summary>
            PageUpDown = 0x08,
            /// <summary>
            /// Numeric keypad '*' expands everything under the current selection</summary>
            StarExpandSubTree = 0x10,
            /// <summary>
            /// With a node selected, typing alpha keys jumps to the next visible node with a matching first character</summary>
            FirstLetterMatching = 0x20,

            /// <summary>
            /// Only Up Arrow and Down Arrow shortcuts are supported (goes up or down the hierarchy)</summary>
            Default = UpDownNav,

            /// <summary>
            /// Windows Explorer-like tree control keyboard navigation:
            ///     Right Arrow expands the current selection if it is not expanded, otherwise goes to the first child.
            ///     Left Arrow collapses the current selection if it is expanded, otherwise goes to the parent.
            ///     Up and Down Arrow goes up or down the hierarchy.
            ///     Home key jumps to top; End key jumps to bottom.
            ///     Page Up jumps up one windows distance; Page Down jumps down one page.
            ///     Numeric keypad '*' expands everything under the current selection.
            ///     With a node selected, typing alpha keys jumps to the next visible node with a matching first character.</summary>
            WindowsExplorer = UpDownNav | LeftRightExpand | HomeEnd | PageUpDown | StarExpandSubTree | FirstLetterMatching,
        }

        /// <summary>
        /// Gets and sets whether or not the tree control keyboard navigation
        /// follows the Windows Explorer model: 
        ///     A single click expands a folder, but does not collapse it. 
        ///     Right Arrow expands the current selection if it is not expanded, otherwise goes to the first child.
        ///     Left Arrow collapses the current selection if it is expanded, otherwise goes to the parent.
        ///     Up and Down Arrow goes up or down the hierarchy.
        ///     Home key jumps to top; End key jumps to bottom.
        ///     Page Up jumps up one windows distance; Page Down jumps down one page.
        ///     Numeric keypad '*' expands everything under the current selection.
        ///     With a node selected, typing alpha keys jumps to the next visible node with a matching first character.</summary>
        public KeyboardShortcuts NavigationKeyBehavior
        {
            get { return m_navigationKeyBehavior; }
            set { m_navigationKeyBehavior = value; }
        }

        /// <summary>
        /// Specifies how a user starts label editing in the TreeControl</summary>
        [Flags]
        public enum LabelEditModes
        {
            /// <summary>
            /// Click the selected node to start editing</summary>
            EditOnClick = 0x01,
            /// <summary>
            /// Press F2 to enter the editing mode, ESC to exit the editing mode</summary>
            EditOnF2 = 0x02,
            /// <summary>
            /// Default is EditOnClick</summary>
            Default = EditOnClick,
        }

        /// <summary>
        /// Gets or sets a LabelEditModes indicating how to begin editing a label</summary>   
        public LabelEditModes LabelEditMode
        {
            get { return m_labelEditMode; }
            set { m_labelEditMode = value; }
        }

        /// <summary>
        /// Gets all nodes, in pre-order</summary>
        public IEnumerable<Node> Nodes
        {
            get
            {
                Stack<Node> nodes = new Stack<Node>();
                nodes.Push(m_root);

                while (nodes.Count > 0)
                {
                    Node node = nodes.Pop();
                    yield return node;

                    if (node.InnerList != null)
                    {
                        for (int i = node.InnerList.Count - 1; i >= 0; i--)
                            nodes.Push(node.InnerList[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all nodes that are visible due to their parent's Expanded
        /// property being true, in pre-order. The root node is included if
        /// ShowRoot is true.</summary>
        public IEnumerable<Node> VisibleNodes
        {
            get
            {
                Stack<Node> nodes = new Stack<Node>();
                if (m_showRoot)
                {
                    nodes.Push(m_root);
                }
                else if (m_root.InnerList != null)
                {
                    for (int i = m_root.InnerList.Count - 1; i >= 0; i--)
                        nodes.Push(m_root.InnerList[i]);
                }

                while (nodes.Count > 0)
                {
                    Node node = nodes.Pop();
                    yield return node;

                    if (node.Expanded && node.InnerList != null)
                    {
                        for (int i = node.InnerList.Count - 1; i >= 0; i--)
                            nodes.Push(node.InnerList[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets all selected nodes, in pre-order</summary>
        public IEnumerable<Node> SelectedNodes
        {
            get
            {
                Stack<Node> nodes = new Stack<Node>();
                nodes.Push(m_root);

                while (nodes.Count > 0)
                {
                    Node node = nodes.Pop();
                    if (node.Selected)
                        yield return node;

                    if (node.InnerList != null)
                    {
                        for (int i = node.InnerList.Count - 1; i >= 0; i--)
                            nodes.Push(node.InnerList[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the node, if any, under the given client point</summary>
        /// <param name="clientPoint">Point, in client space</param>
        /// <returns>Node under client point, or null if none</returns>
        public Node GetNodeAt(Point clientPoint)
        {
            UpdateNodeMeasurements();

            int y = clientPoint.Y + m_vScroll - ContentVerticalOffset;
            foreach (Node node in VisibleNodes)
            {
                int rowHeight = GetRowHeight(node);
                if (rowHeight > 0 && y <= rowHeight)
                    return node;
                y -= rowHeight;
            }

            return null;
        }

        /// <summary>
        /// Gets the nodes that indicate where in the hierarchy inserted nodes should be placed.</summary>
        /// <param name="clientPoint">Point, in client space. If using coordinates from a
        /// DragEventArgs, for example, convert to client space using PointToClient().</param>
        /// <param name="parent">The Node that will be the parent of inserted node(s).
        /// Can be null to indicate that the root (if any) will be replaced.</param>
        /// <param name="before">The Node that is closest to being before (higher on the screen)
        /// than clientPoint. Can be null to indicate that the insertion point will make
        /// the inserted nodes become the first child/children of 'parent'.</param>
        /// <returns>True if clientPoint represents a valid insertion point</returns>
        public bool GetInsertionNodes(Point clientPoint, out Node parent, out Node before)
        {
            UpdateNodeMeasurements();

            // First, find the node that is immediately before the target point. We don't care
            //  yet if 'before' is a parent or a sibling of the target point.
            before = null;
            int y = clientPoint.Y + m_vScroll - ContentVerticalOffset;
            foreach (Node node in VisibleNodes)
            {
                int rowHeight = GetRowHeight(node);
                if (rowHeight > 0 && y <= rowHeight)
                    break;

                before = node;
                y -= rowHeight;
            }

            // Now find the parent. 'before' may actually be the parent.
            parent = null;
            if (before != null)
            {
                if (before.Expanded)
                {
                    parent = before;
                    before = null;
                }
                else
                {
                    parent = before.Parent;
                }
            }

            // For now, this method will always succeed. Perhaps in the future we can consider
            //  certain areas of the TreeControl out-of-bounds.
            return true;
        }

        /// <summary>
        /// Expands all nodes in the hierarchy</summary>
        public void ExpandAll()
        {
            foreach (Node node in Nodes)
                if (!node.IsLeaf)
                    node.Expanded = true;
        }

        /// <summary>
        /// Collapses all nodes in the hierarchy</summary>
        public void CollapseAll()
        {
            foreach (Node node in Nodes)
            {
                if (node != m_root)
                    node.Expanded = false;
            }
        }

        /// <summary>
        /// Expands the tree to the first leaf node and ensures that it is visible</summary>
        /// <returns>First leaf node</returns>
        public Node ExpandToFirstLeaf()
        {
            Node node = m_root;
            Node last;
            do
            {
                last = node;
                foreach (Node child in node.Children)
                {
                    node = child;
                    break;
                }
            }
            while (last != node);

            if (node != null)
                EnsureVisible(node);

            return node;
        }

        /// <summary>
        /// Expands all ancestors of the node</summary>
        /// <param name="node">Node to show</param>
        public void Show(Node node)
        {
            Node ancestor = node.Parent;
            while (ancestor != null)
            {
                ancestor.Expanded = true;
                ancestor = ancestor.Parent;
            }
        }

        /// <summary>
        /// Ensures the node is visible, by expanding all of its ancestor Nodes and scrolling
        /// it into view</summary>
        /// <param name="node">Node to make visible</param>
        public void EnsureVisible(Node node)
        {
            Show(node);
            ScrollIntoView(node);
        }

        /// <summary>
        /// Scrolls the node into view if it is currently outside the visible area</summary>
        /// <param name="node">Node to scroll into view</param>
        public void ScrollIntoView(Node node)
        {
            ScrollIntoView(node, false);//don't include children
        }

        /// <summary>
        /// Ensures that this node is visible while showing as many of its children as possible,
        /// scrolling downwards if necessary</summary>
        /// <param name="node">Parent node that is expanded and whose children are scrolled into view</param>
        public void ScrollChildrenIntoView(Node node)
        {
            ScrollIntoView(node, true);//yes, include children
        }

        private void ScrollIntoView(Node node, bool scrollChildren)
        {
            UpdateNodeMeasurements();

            // find the y value, in this control's space, of the top of this node
            int y = -m_vScroll;
            foreach (Node n in VisibleNodes)
            {
                if (n == node)
                    break;
                int rowHeight = GetRowHeight(n);
                y += rowHeight;
            }

            int newVScroll = m_vScroll;

            if (y < 0) // above top of control
            {
                // position node at 0 in control space
                newVScroll += y;
            }
            else
            {
                // calculate needed height and check if any part is below the bottom of the control
                int childrenHeight = 0;
                if (scrollChildren)
                    childrenHeight = GetChildrenHeight(node, m_clientSize.Height);

                int totalNeededHeight = GetRowHeight(node) + childrenHeight;
                if (totalNeededHeight > m_clientSize.Height)
                    totalNeededHeight = m_clientSize.Height;

                int yBottom = y + totalNeededHeight;
                if (yBottom > m_clientSize.Height)
                    newVScroll += yBottom - m_clientSize.Height;
            }

            SetVerticalScroll(newVScroll);
        }

        /// <summary>
        /// Gets whether the control has initiated a drag and drop</summary>
        public bool IsDragging
        {
            get { return m_dragging; }
        }

        /// <summary>
        /// Begins a label edit on the given node, if it is visible and editable.
        /// Otherwise, does nothing.</summary>
        /// <param name="node">Visible node whose label is to be edited</param>
        public void BeginLabelEdit(Node node)
        {
            EndLabelEdit();

            if (!node.AllowLabelEdit)
                return;

            m_labelEditNode = node;

            foreach (NodeLayoutInfo info in NodeLayout)
            {
                if (info.Node == m_labelEditNode)
                {
                    m_textBox.Bounds = GetLabelEditBounds(info);

                    m_textBox.Text = m_labelEditNode.Label;
                    m_textBox.SelectAll();
                    m_textBox.Show();
                    m_textBox.Focus();
                    Invalidate();
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the bounding rectangle for label editing.</summary>
        /// <param name="info">Node layout information</param>
        /// <returns>Bounding rectangle for label editing</returns>
        protected virtual Rectangle GetLabelEditBounds(NodeLayoutInfo info)
        {
            return new Rectangle(
                        info.LabelLeft,
                        info.Y,
                        m_clientSize.Width - info.LabelLeft + 1,
                        FontHeight + Margin.Top + 1);
        }

        /// <summary>
        /// Sets the tree selection to the given node, deselecting all other nodes</summary>
        /// <param name="selected">Node to be selected</param>
        public void SetSelection(Node selected)
        {
            foreach (Node node in SelectedNodes)
                node.Selected = false;

            if (selected != null)
                selected.Selected = true;

            m_extendSelectionBaseNode = selected;
            m_currentKeyedNode = selected;
        }

        /// <summary>
        /// Clears the tree selection by deselecting all nodes</summary>
        public void ClearSelection()
        {
            SetSelection(null);
        }

        /// <summary>
        /// Event that is raised after the value of ItemRenderer property changes</summary>
        public event EventHandler ItemRendererChanged;

        /// <summary>
        /// Raises the ItemRendererChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnItemRendererChanged(EventArgs e)
        {
            ItemRendererChanged.Raise(this, e);
        }

        /// <summary>
        /// Event that is raised before the selection is changed by the user</summary>
        public event EventHandler SelectionChanging;

        /// <summary>
        /// Raises the SelectionChanging event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnSelectionChanging(EventArgs e)
        {
            SelectionChanging.Raise(this, e);
        }

        /// <summary>
        /// Event that is raised after the selection is changed by the user</summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Raises the SelectionChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            SelectionChanged.Raise(this, e);
        }

        /// <summary>
        /// Event that is raised after a node's Selected property changes</summary>
        public event EventHandler<NodeEventArgs> NodeSelectedChanged;

        /// <summary>
        /// Raises the NodeSelectedChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnNodeSelectedChanged(NodeEventArgs e)
        {
            NodeSelectedChanged.Raise(this, e);
        }

        /// <summary>
        /// Event that is raised before changing a node's Expanded property</summary>
        public event EventHandler<CancelNodeEventArgs> NodeExpandedChanging;

        /// <summary>
        /// Raises the NodeExpandedChanging event</summary>
        /// <param name="e">Event args</param>
        /// <returns>True iff the event was cancelled</returns>
        protected virtual bool OnNodeExpandedChanging(CancelNodeEventArgs e)
        {
            return NodeExpandedChanging.RaiseCancellable(this, e);
        }

        /// <summary>
        /// Event that is raised after a node's Expanded property changes</summary>
        public event EventHandler<NodeEventArgs> NodeExpandedChanged;

        /// <summary>
        /// Raises the NodeExpandedChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnNodeExpandedChanged(NodeEventArgs e)
        {
            NodeExpandedChanged.Raise(this, e);
        }

        /// <summary>
        /// Event that is raised after a node's CheckState property is edited by the user</summary>
        public event EventHandler<NodeEventArgs> NodeCheckStateEdited;

        /// <summary>
        /// Raises the NodeCheckStateEdited event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnNodeCheckStateEdited(NodeEventArgs e)
        {
            NodeCheckStateEdited.Raise(this, e);
        }

        /// <summary>
        /// Event that is raised after a node's Label property is edited by the user</summary>
        public event EventHandler<NodeEventArgs> NodeLabelEdited;

        /// <summary>
        /// Raises the NodeLabelEdited event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnNodeLabelEdited(NodeEventArgs e)
        {
            NodeLabelEdited.Raise(this, e);
        }

        /// <summary>
        /// Virtual function to limit selection of node based on derived class's logic</summary>
        /// <param name="node">Node to test</param>
        /// <returns>True if this node can be added to the current selection set of nodes, or false otherwise.</returns>
        protected virtual bool IsNodeMultiSelectable(Node node)
        {
            return true;
        }

        /// <summary>
        /// Gets the previous visible node</summary>
        /// <param name="node">Current node</param>
        /// <returns>Previous visible node</returns>
        protected Node GetPreviousNode(Node node)
        {
            Node prev = null;
            foreach (Node n in VisibleNodes)
            {
                if (n == node)
                    return prev;
                prev = n;
            }

            return null;
        }

        /// <summary>
        /// Gets the next visible node</summary>
        /// <param name="node">Current node</param>
        /// <returns>Next visible node</returns>
        protected Node GetNextNode(Node node)
        {
            Node prev = null;
            foreach (Node n in VisibleNodes)
            {
                if (prev != null)
                    return n;
                if (n == node)
                    prev = n;
            }

            return null;
        }

        /// <summary>
        /// Gets the first visible node</summary>
        /// <returns>First visible node</returns>
        protected Node GetFirstNode()
        {
            foreach (Node node in VisibleNodes)
                return node;

            return null;
        }

        /// <summary>
        /// Raises the Resize event</summary>
        /// <param name="e">Event args</param>
        protected override void OnResize(EventArgs e)
        {
            EndLabelEdit();

            base.OnResize(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"/> event and performs configured mouse down processing</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            EndLabelEdit();

            Focus();

            m_mouseDownPoint = new Point(e.X, e.Y);

            HitRecord hitRecord = Pick(m_mouseDownPoint);
            Node node = hitRecord.Node;
            switch (hitRecord.Type)
            {
                case HitType.Expander:
                    ToggleExpand(node);
                    break;

                case HitType.CheckBox:
                    CheckState checkState = (node.CheckState == CheckState.Checked) ? CheckState.Unchecked : CheckState.Checked;
                    if (node.CheckState != checkState)
                    {
                        node.CheckState = checkState;
                        OnNodeCheckStateEdited(new NodeEventArgs(node));
                    }
                    break;

                case HitType.Item:
                case HitType.Label:
                    // Selection logic table, mostly based on Visual Studio tree control:
                    // Mouse Button    Modifier Keys   Result
                    // -------------   -------------   -----------------------------
                    // left            none            Not selected? SetSelection(node). Else, m_leftClickedSelectedNode = node.
                    // left            alt             No selection change or label editing. Different than VS.
                    // left            ctrl            toggle node's selected state. Shift key is ignored if it's pressed.
                    // left            shift           ExtendSelection(node)
                    // right           none,ctrl,shift Node is already selected? Then don't change selection set.
                    //                                 Node is not selected? Then SetSelection(node).
                    // right           alt             No selection change or label editing. Different than VS.
                    // middle, etc.    any             No change.
                    m_selecting = false;
                    Keys modifiers = FilterModifiers();
                    if ((modifiers & Keys.Alt) == 0)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            m_selecting = true;
                            OnSelectionChanging(EventArgs.Empty);

                            if ((modifiers & Keys.Control) != 0)
                            {
                                if (IsNodeMultiSelectable(node))
                                {
                                    node.Selected = !node.Selected;
                                }
                            }
                            else if ((modifiers & Keys.Shift) != 0)
                            {
                                ExtendSelection(node);
                            }
                            else
                            {
                                if (node.Selected)
                                {
                                    // even though selection set isn't changing, MouseUp expects m_selecting to be true.
                                    m_leftClickedSelectedNode = node;
                                }
                                else
                                {
                                    SetSelection(node);
                                }

                                if (ExpandOnSingleClick)
                                {
                                    if (!node.IsLeaf && !node.Expanded)
                                        node.Expanded = true;
                                }
                            }
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            if (!node.Selected)
                            {
                                m_selecting = true;
                                OnSelectionChanging(EventArgs.Empty);
                                SetSelection(node);
                            }
                        }
                    }
                    break;
            }
            m_lastMouseDownWasDoubleClick = (e.Clicks == 2);

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"/> event and performs configured mouse move processing</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // test for dragging
            if (e.Button == MouseButtons.Left &&
                !m_dragging &&
                AllowDrop)
            {
                if (m_selecting)
                {
                    Size dragSize = SystemInformation.DragSize;
                    if (Math.Abs(e.X - m_mouseDownPoint.X) > dragSize.Width ||
                        Math.Abs(e.Y - m_mouseDownPoint.Y) > dragSize.Height)
                    {
                        m_dragging = true;
                    }
                }
                else
                    m_dragging = false;
            }

            // To re-enable the firing of the hover event, without the mouse leaving the Control, we have to call
            //  the Win32 TrackMouseEvent. In .NET, we can call this Control's ResetMouseEventArgs().
            if (m_resetHoverEvent)
                ResetMouseEventArgs();

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"/> event and performs configured mouse up processing</summary>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            HitRecord hitRecord = Pick(new Point(e.X, e.Y));

            if (m_selecting)
            {
                m_selecting = false;

                // if this was a left-click on an already selected node
                if (m_leftClickedSelectedNode != null && e.Button == MouseButtons.Left)
                {
                    SetSelection(m_leftClickedSelectedNode); // click on selected node deselects all others

                    // if the mouse up was a left click over the clicked node's label, and label editing
                    //  is allowed, start label editing after a brief pause (to allow for a double-click).
                    if (LabelEditModeContains(LabelEditModes.EditOnClick) &&
                        m_lastMouseDownWasDoubleClick == false &&
                        e.Button == MouseButtons.Left &&
                        hitRecord.Node == m_leftClickedSelectedNode &&
                        hitRecord.Type == HitType.Label &&
                        hitRecord.Node.AllowLabelEdit)
                    {
                        m_editLabelTimer.Interval = SystemInformation.DoubleClickTime;
                        m_editLabelTimer.Tag = m_leftClickedSelectedNode;
                        m_editLabelTimer.Enabled = true;
                    }

                    m_leftClickedSelectedNode = null;
                }
                OnSelectionChanged(EventArgs.Empty);
            }

            m_dragging = false;

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave"/> event and performs configured mouse leave processing</summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            if (m_selecting)
            {
                m_selecting = false;
                m_leftClickedSelectedNode = null;

                OnSelectionChanged(EventArgs.Empty);
            }

            m_dragging = false;

            base.OnMouseLeave(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDoubleClick"/> event and performs configured mouse double click processing</summary>
        /// <param name="e">The MouseEventArgs instance containing the event data</param>
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            EndLabelEdit();

            HitRecord hitRecord = Pick(new Point(e.X, e.Y));
            Node node = hitRecord.Node;
            switch (hitRecord.Type)
            {
                case HitType.Item:
                case HitType.Label:
                    if (ToggleOnDoubleClick)
                        ToggleExpand(node);
                    break;
            }

            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"/> event</summary>
        /// <param name="e">The MouseEventArgs instance containing the event data</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int value = m_vScrollBar.Value - e.Delta / 2;
            SetVerticalScroll(value);

            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Raises base MouseHover event and handles it by creating HitRecord and showing tool tip</summary>
        /// <param name="e">Event arguments</param>
        /// <remarks>This doesn't work well. We only get called when the mouse leaves the Control and reenters.
        /// We want to be called whenever the mouse moves and then rests.</remarks>
        protected override void OnMouseHover(EventArgs e)
        {
            var clientMousePos = PointToClient(MousePosition);
            if (clientMousePos != m_lastHoverPosition)
            {
                m_lastHoverPosition = clientMousePos;

                HitRecord hitRecord = Pick(clientMousePos);
                Node node = hitRecord.Node;
                if (node != null &&
                    !string.IsNullOrEmpty(node.HoverText) &&
                    !IsDragging) //does it really matter about dragging? What's the convention?
                {
                    m_toolTip.SetToolTip(this, node.HoverText);
                }
                else
                {
                    m_toolTip.SetToolTip(this, string.Empty);
                }

                m_resetHoverEvent = true;
            }

            base.OnMouseHover(e);
        }
        
        /// <summary>
        /// Processes a dialog key</summary>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process</param>
        /// <returns>True iff the key was processed by the control</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            Node nextNode = null;
            bool handled = false;

            switch (keyData & Keys.KeyCode)
            {
                case Keys.Down:
                    nextNode = GetNextNode(m_currentKeyedNode);
                    if (m_style == Style.CategorizedPalette && NavigationKeyBehaviorContains(KeyboardShortcuts.UpDownNav))
                    {
                        while (nextNode != null && nextNode.Parent == Root)
                            nextNode = GetNextNode(nextNode);
                    }
                    handled = true;
                    break;
                case Keys.Up:
                    nextNode = GetPreviousNode(m_currentKeyedNode);
                    if (m_style == Style.CategorizedPalette && NavigationKeyBehaviorContains(KeyboardShortcuts.UpDownNav))
                    {
                        while (nextNode != null && nextNode.Parent == Root)
                            nextNode = GetPreviousNode(nextNode);
                    }
                    handled = true;
                    break;
                case Keys.Left:
                    // if node is expanded, collapse it; otherwise go to its parent
                    if (NavigationKeyBehaviorContains(KeyboardShortcuts.LeftRightExpand) && m_currentKeyedNode != null)
                    {
                        if (m_currentKeyedNode.Expanded)
                            ToggleExpand(m_currentKeyedNode);
                        else
                            nextNode = m_currentKeyedNode.Parent;
                        handled = true;
                    }
                    break;
                case Keys.Right:
                    if (NavigationKeyBehaviorContains(KeyboardShortcuts.LeftRightExpand) && m_currentKeyedNode != null)
                    {
                        // if node is expanded, go to 1st child; otherwise expand it
                        if (m_currentKeyedNode.Expanded)
                        {
                            foreach (Node child in m_currentKeyedNode.Children)
                            {
                                nextNode = child;
                                break;
                            }
                        }
                        else if (!m_currentKeyedNode.IsLeaf)
                            m_currentKeyedNode.Expanded = true;
                        handled = true;
                    }
                    break;
                case Keys.Home:
                    if (NavigationKeyBehaviorContains(KeyboardShortcuts.HomeEnd))
                    {
                        if (ShowRoot)
                            nextNode = Root;
                        else
                        {
                            foreach (Node child in Root.Children)
                            {
                                nextNode = child;
                                break;
                            }
                        }
                        handled = true;
                    }
                    break;
                case Keys.End:
                    if (NavigationKeyBehaviorContains(KeyboardShortcuts.HomeEnd))
                    {
                        foreach (Node node in VisibleNodes)
                            nextNode = node;
                        handled = true;
                    }
                    break;
                case Keys.PageDown:
                    if (NavigationKeyBehaviorContains(KeyboardShortcuts.PageUpDown))
                    {
                        int visibleHeight = Height - Margin.Top - Margin.Bottom;
                        SetVerticalScroll(m_vScroll + visibleHeight - FontHeight);
                        handled = true;
                    }
                    break;
                case Keys.PageUp:
                    if (NavigationKeyBehaviorContains(KeyboardShortcuts.PageUpDown))
                    {
                        int visibleHeight = Height - Margin.Top - Margin.Bottom;
                        SetVerticalScroll(m_vScroll - visibleHeight - FontHeight);
                        handled = true;
                    }
                    break;
            }



            if (nextNode != null)
            {
                EndLabelEdit();
                ScrollIntoView(nextNode);

                try
                {
                    NavigationKeyChangingSelection = true;
                    OnSelectionChanging(EventArgs.Empty);

                    m_currentKeyedNode = nextNode;
                    if ((keyData & Keys.Shift) != 0 && m_selectionMode == SelectionMode.MultiExtended)
                    {
                        ExtendSelection(nextNode);
                    }
                    else
                    {
                        SetSelection(nextNode);
                    }
                }
                finally
                {
                    OnSelectionChanged(EventArgs.Empty);
                    NavigationKeyChangingSelection = false;
                }
            }

            if (handled)
                return true;

            return base.ProcessDialogKey(keyData);
        }

        /// <summary>
        /// Raises the KeyDown event</summary>
        /// <remarks>Selects next node that starts with the input alpha character and scroll into view if necessary.</remarks>
        /// <param name="e">Event Args</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // If somebody else took care of this key press already (ie: be listening for a KeyDown event), we don't want to do anything more
            if (e.Handled)
                return;

            // If we don't have a selected node, then none of our keypress logic will have anything to do
            if (m_currentKeyedNode == null)
                return;

            if (e.KeyData == Keys.F2 && LabelEditModeContains(LabelEditModes.EditOnF2))
            {
                BeginLabelEdit(m_currentKeyedNode);
                return;
            }
            
            if (e.KeyData == Keys.Multiply) // numeric keypad *:
            {
                // expands everything under the current selection
                Stack<Node> nodes = new Stack<Node>();
                if (!m_currentKeyedNode.IsLeaf)
                    nodes.Push(m_currentKeyedNode);
                while (nodes.Count > 0)
                {
                    Node curNode = nodes.Pop();
                    curNode.Expanded = true;
                    if (curNode.InnerList != null)
                    {
                        for (int i = curNode.InnerList.Count - 1; i >= 0; i--)
                        {
                            if (!curNode.InnerList[i].IsLeaf)
                                nodes.Push(curNode.InnerList[i]);
                        }
                    }
                }
                return;
            }

            // Select the next visible node that starts with the typed letter, but only if no modifiers are active
            if (e.Modifiers == 0)
            {
                char character = Convert.ToChar(e.KeyValue);
                if (!char.IsLetterOrDigit(character))
                    return;

                string charString = char.ToString(character);
                Node node = m_currentKeyedNode;

                // find next visible node whose label starts with the char
                do
                {
                    // wrap aroud
                    node = GetNextNode(node) ?? GetFirstNode();

                    if (node == m_currentKeyedNode) // quit the search if we end up back at the starting node
                        break;

                    if (node.Label != null &&
                        node.Label.StartsWith(charString, StringComparison.CurrentCultureIgnoreCase))
                    {
                        SetSelection(node);
                        ScrollIntoView(node);
                        break;
                    }

                }
                while (node != m_currentKeyedNode);
            }
        }

        /// <summary>
        /// Raises the KeyUp event</summary>
        /// <param name="e">Key event arguments</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (m_handleKeyUp)
            {
                m_handleKeyUp = false;
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }

        /// <summary>
        /// Raises the DragOver event and performs configured drag processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnDragOver(DragEventArgs e)
        {
            Point clientPoint = PointToClient(new Point(e.X, e.Y));
            // is the point in the auto scroll area?
            if (clientPoint.Y < m_averageRowHeight)
            {
                m_autoScrollUp = true;
                m_autoScrollTimer.Enabled = true;
            }
            else if (clientPoint.Y >= m_clientSize.Height - m_averageRowHeight)
            {
                m_autoScrollUp = false;
                m_autoScrollTimer.Enabled = true;
            }
            else
            {
                m_autoScrollTimer.Enabled = false;

                Node hitNode = GetNodeAt(clientPoint);
                if (hitNode != m_dragHoverNode)
                {
                    m_dragHoverExpandTimer.Stop();
                    m_dragHoverExpandTimer.Enabled = (hitNode != null);
                    m_dragHoverNode = hitNode;
                }

                int y = clientPoint.Y + m_vScroll;
                foreach (Node node in VisibleNodes)
                {
                    int rowHeight = GetRowHeight(node);
                    if (y <= rowHeight)
                    {
                        break;
                    }
                    y -= rowHeight;
                }

                m_dragBetween = y < 5;
            }

            base.OnDragOver(e);
        }

        /// <summary>
        /// Raises the DragDrop event</summary>
        /// <param name="e">Event args</param>
        protected override void OnDragDrop(DragEventArgs e)
        {
            StopDragTimers();

            base.OnDragDrop(e);
            m_dragBetween = false;
        }

        /// <summary>
        /// Raises the DragLeave event</summary>
        /// <param name="e">Event args</param>
        protected override void OnDragLeave(EventArgs e)
        {
            StopDragTimers();

            base.OnDragLeave(e);
        }

        private void StopDragTimers()
        {
            m_dragHoverExpandTimer.Enabled = false;
            m_dragHoverNode = null;

            m_autoScrollTimer.Enabled = false;
        }

        private delegate void WndProcCallback(ref Message m);

        /// <summary>
        /// Intercepts paint messages to update scrollbars before drawing starts, to avoid
        /// annoying background flash when scrollbars are hidden</summary>
        /// <param name="m">Window message</param>
        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;
            if (m.Msg == WM_PAINT)
            {
                if (InvokeRequired)
                {
                    BeginInvoke(new WndProcCallback(WndProc), m);
                    return;
                }
                using (Graphics g = CreateGraphics())
                {
                    UpdateNodeMeasurements(g);

                    int width = 0;
                    int height = 0;
                    foreach (NodeLayoutInfo info in NodeLayout)
                    {
                        Node node = info.Node;
                        width = Math.Max(width, info.LabelLeft + node.LabelWidth);
                        height = info.Y;
                    }

                    width += m_hScroll;
                    height += m_vScroll + FontHeight + Margin.Top;

                    m_hScrollBar.Value = m_hScroll;
                    m_vScrollBar.Value = m_vScroll;

                    int visibleWidth = Width;
                    int visibleHeight = Height;

                    WinFormsUtil.UpdateScrollbars(
                        m_vScrollBar,
                        m_hScrollBar,
                        new Size(visibleWidth, visibleHeight),
                        new Size(width, height));

                    if (m_vScrollBar.Visible)
                        visibleWidth -= m_vScrollBar.Width;
                    if (m_hScrollBar.Visible)
                        visibleHeight -= m_hScrollBar.Height;

                    m_clientSize = new Size(visibleWidth, visibleHeight);
                }
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// Raises the GotFocus event</summary>
        /// <param name="e">Event args</param>
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        /// <summary>
        /// Raises the LostFocus event</summary>
        /// <param name="e">Event args</param>
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        /// <summary>
        /// Recalculates node label sizes</summary>
        /// <param name="e">Event args</param>
        protected override void OnFontChanged(EventArgs e)
        {
            foreach (Node node in Nodes)
                m_nodesToMeasure.Add(node);

            base.OnFontChanged(e);
        }

        /// <summary>
        /// Raises the Paint event and performs a variety of operations on the tree</summary>
        /// <param name="e">Event args</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            UpdateNodeMeasurements(g);

            int numVisibleNodes = 0;
            int visibleNodesHeight = 0;
            int height = m_clientSize.Height;
            int yPadding = Margin.Top;

            //bool drawHierarchyLines = (m_style == Style.Tree);
            bool drawHierarchyLines = false;
            bool drawExpanders = DrawExpanders;
            Size expanderSize = m_itemRenderer.ExpanderSize;
            int halfExpanderWidth = expanderSize.Width / 2;
            int halfExpanderHeight = expanderSize.Height / 2;
            int halfCheckBoxHeight = m_itemRenderer.CheckBoxSize.Height / 2;

            int right = m_clientSize.Width;

            // dragNode is used to draw drop-between cue
            Node dragNode = null;
            if (ShowDragBetweenCue && DragBetween )
            {
                Point clientPoint = PointToClient(Cursor.Position);
                int y = clientPoint.Y + m_vScroll;
                foreach (Node node in VisibleNodes)
                {
                    int rowHeight = GetRowHeight(node);
                    if (y <= rowHeight)
                    {
                        dragNode = node;
                        break;
                    }
                    y -= rowHeight;
                }
            }

            List<int> segmentYs = new List<int>(16);

            int depth = 0;
            foreach (NodeLayoutInfo info in NodeLayout)
            {
                Node node = info.Node;
                int rowHeight = GetRowHeight(node);
                int rowCenterY = info.Y + rowHeight / 2;

                if (segmentYs.Count == 0)
                    segmentYs.Add(rowHeight - yPadding);

                while (info.Depth != depth)
                {
                    if (info.Depth > depth)
                    {
                        segmentYs.Add(info.Y + rowHeight - yPadding);
                        depth = info.Depth;
                        break;
                    }
                    else
                    {
                        segmentYs.RemoveAt(depth); // remove last
                        depth--;
                    }
                }

                bool visible = (rowHeight > 0) && (info.Y + rowHeight > 0) && (info.Y < height);

                segmentYs[depth] = info.Y + rowHeight - yPadding;

                // draw expanders and hierarchy lines
                if (depth > 0)
                {
                    int yMin = segmentYs[depth - 1];
                    int yMax, yNext;
                    if (node.IsLeaf)
                    {
                        yMax = rowCenterY;
                        yNext = yMax + 1;
                    }
                    else
                    {
                        yMax = Math.Max(rowCenterY - halfExpanderHeight, yMin);
                        yNext = rowCenterY + halfExpanderHeight + 1;
                    }
                    segmentYs[depth - 1] = yNext;

                    if (visible)
                    {
                        m_itemRenderer.DrawBackground(node, g, info.X, info.Y);
                    }

                    if (yMax >= 0 && yMin <= height && drawHierarchyLines)
                    {
                        m_itemRenderer.DrawHierarchyLine(g,
                            new Point(info.X - Indent + halfExpanderWidth, yMin),
                            new Point(info.X - Indent + halfExpanderWidth, yMax));
                    }

                    if (visible)
                    {
                        int stemX = info.X - Indent + halfExpanderWidth;
                        if (drawExpanders)
                        {
                            if (!node.IsLeaf)
                            {
                                m_itemRenderer.DrawExpander(node, g, info.X - Indent, rowCenterY - halfExpanderHeight);
                                stemX += halfExpanderWidth + 1;
                            }
                        }
                        else
                        {
                            if (node.Parent == m_root) // category?
                            {
                                Rectangle r = new Rectangle(0, info.Y, right, rowHeight);
                                m_itemRenderer.DrawCategory(node, g, r);
                            }
                        }

                        if (drawHierarchyLines)
                        {
                            m_itemRenderer.DrawHierarchyLine(g,
                                new Point(stemX, rowCenterY),
                                new Point(info.X, rowCenterY));
                        }
                    }
                }

                if (visible)
                {
                    numVisibleNodes++;
                    visibleNodesHeight += rowHeight;

                    // draw optional check box
                    if (node.HasCheck)
                    {
                        m_itemRenderer.DrawCheckBox(node, g, info.X, rowCenterY - halfCheckBoxHeight);
                    }

                    // draw optional indicator and image
                    if (m_stateImageList != null &&
                        node.StateImageIndex >= 0 &&
                        node.StateImageIndex < m_stateImageList.Images.Count)
                    {
                        m_itemRenderer.DrawImage(m_stateImageList, g, info.StateImageLeft, rowCenterY - m_stateImageList.ImageSize.Height / 2, node.StateImageIndex);
                    }

                    if (m_imageList != null &&
                        node.ImageIndex >= 0 &&
                        node.ImageIndex < m_imageList.Images.Count)
                    {
                        m_itemRenderer.DrawImage(m_imageList, g, info.ImageLeft, rowCenterY - m_imageList.ImageSize.Height / 2, node.ImageIndex);
                    }

                    int filterOffset = 0;
                    if (node.PartiallyExpanded)
                    {
                        filterOffset += m_filterImage.Width;
                        g.DrawImage(m_filterImage, info.ImageLeft + m_filterImage.Width, rowCenterY - (m_filterImage.Height + yPadding) / 2);
                    }

                    // draw label if it's not being edited
                    if (node != m_labelEditNode)
                    {
                        m_itemRenderer.DrawLabel(node, g, info.LabelLeft + filterOffset, rowCenterY - node.LabelHeight / 2);
                    }

                    m_itemRenderer.DrawData(node, g, info.LabelLeft + filterOffset, rowCenterY - node.LabelHeight / 2);
                    if (node == dragNode)
                    { 
                        g.DrawLine(Pens.Red,
                            new Point(info.X - Indent + halfExpanderWidth, info.Y -2),
                            new Point(info.X - Indent + halfExpanderWidth + 100, info.Y - 2));
                    }
                }
            }

            if (numVisibleNodes > 0)
            {
                int newAverageRowHeight = (int)Math.Ceiling((double)visibleNodesHeight / (double)numVisibleNodes);
                if (newAverageRowHeight != m_averageRowHeight)
                {
                    m_averageRowHeight = newAverageRowHeight;
                    m_vScrollBar.SmallChange = newAverageRowHeight;
                }
            }
            else
            {
                g.DrawString(Text, Font, ItemRenderer.TextBrush, ClientRectangle);
            }
        }

        #region Event Handlers

        private void TextBoxLostFocus(object sender, EventArgs e)
        {
            EndLabelEdit();
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                EndLabelEdit();
                e.Handled = true;
                m_handleKeyUp = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                AbortLabelEdit();
                e.Handled = true;
                m_handleKeyUp = true;
            }
        }

        private void TextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (m_labelEditNode == null); // this suppresses the error sound from Keys.Enter, handled above
        }

        private void VerticalScrollBarValueChanged(object sender, EventArgs e)
        {
            m_vScroll = m_vScrollBar.Value;

            EndLabelEdit();

            Refresh();
        }

        private void HorizontalScrollBarValueChanged(object sender, EventArgs e)
        {
            m_hScroll = m_hScrollBar.Value;

            EndLabelEdit();

            Refresh();
        }

        private void DragHoverTimerTick(object sender, EventArgs e)
        {
            if (m_dragHoverNode != null)
            {
                if (m_dragHoverNode.Expanded == false &&
                    m_dragHoverNode.IsLeaf == false)
                {
                    m_dragHoverNode.Expanded = true;
                    if (m_autoScrollOnExpand)
                        ScrollChildrenIntoView(m_dragHoverNode);
                }

                m_dragHoverNode = null;
            }
        }

        private void AutoScrollTimerTick(object sender, EventArgs e)
        {
            if (m_autoScrollUp)
            {
                SetVerticalScroll(m_vScroll - m_autoScrollSpeed);
            }
            else
            {
                SetVerticalScroll(m_vScroll + m_autoScrollSpeed);
            }
        }

        private void EditLabelTimerTick(object sender, EventArgs e)
        {
            BeginLabelEdit((Node)m_editLabelTimer.Tag);
        }

        #endregion

        #region Private Methods

        private void ExtendSelection(Node clickedNode)
        {
            if (m_extendSelectionBaseNode != null)
            {
                bool selecting = false;
                foreach (Node n in VisibleNodes)
                {
                    if (!IsNodeMultiSelectable(n))
                    {
                        continue;
                    }

                    n.Selected = selecting;

                    if (n == m_extendSelectionBaseNode ||
                        n == clickedNode)
                    {
                        if (m_extendSelectionBaseNode != clickedNode)
                            selecting = !selecting;
                        n.Selected = true;
                    }
                }
            }
        }

        private void ToggleExpand(Node node)
        {
            if (node == m_root)
                return;

            bool expanded = !node.IsLeaf && !node.Expanded;
            node.Expanded = expanded;
            if (expanded && m_autoScrollOnExpand)
                ScrollChildrenIntoView(node);
        }

        private Keys FilterModifiers()
        {
            Keys modifiers = ModifierKeys;
            if (m_selectionMode != SelectionMode.MultiExtended)
                modifiers = Keys.None;
            return modifiers;
        }

        private void EndLabelEdit()
        {
            if (m_labelEditNode != null)
            {
                string newLabel = m_textBox.Text;
                if (m_labelEditNode.Label != newLabel)
                {
                    m_labelEditNode.Label = newLabel;
                    OnNodeLabelEdited(new NodeEventArgs(m_labelEditNode));
                }

                m_labelEditNode = null;
            }
            m_textBox.Hide();
            m_editLabelTimer.Enabled = false;
            m_editLabelTimer.Tag = null;
        }

        private void AbortLabelEdit()
        {
            m_labelEditNode = null;
            m_textBox.Hide();
            m_editLabelTimer.Enabled = false;
            m_editLabelTimer.Tag = null;
        }

        private void SetVerticalScroll(int vScroll)
        {
            vScroll = Math.Max(m_vScrollBar.Minimum, Math.Min(m_vScrollBar.Maximum, vScroll));
            if (m_vScroll != vScroll)
            {
                m_vScroll = vScroll;
                Invalidate();
            }
        }
        
        /// <summary>
        /// Updates node measurements. To support auto expand and auto scroll, we should measure all necessary nodes after
        /// they are created and before the measurements are used. This uses a Graphics object
        /// and so must be called during the current window's message.</summary>
        private void UpdateNodeMeasurements()
        {
            if (m_nodesToMeasure.Count > 0)
            {
                using (Graphics g = CreateGraphics())
                {
                    UpdateNodeMeasurements(g);
                }
            }
        }

        private void UpdateNodeMeasurements(Graphics g)
        {
            foreach (Node node in m_nodesToMeasure)
            {
                Size labelSize = m_itemRenderer.MeasureLabel(node, g);
                node.LabelWidth = labelSize.Width;
                node.LabelHeight = labelSize.Height;
            }
            m_nodesToMeasure.Clear();
        }

        private bool IsNodeInTree(Node node)
        {
            while (node != null && node != m_root)
            {
                node = node.Parent;
            }

            return node == m_root;
        }

        private void CleanUpSpecialNodes()
        {
            if (!IsNodeInTree(m_currentKeyedNode))
                m_currentKeyedNode = null;
            if (!IsNodeInTree(m_extendSelectionBaseNode))
                m_extendSelectionBaseNode = null;
            if (!IsNodeInTree(m_leftClickedSelectedNode))
                m_leftClickedSelectedNode = null;
            if (!IsNodeInTree(m_labelEditNode))
                m_labelEditNode = null;
        }

        #endregion

        /// <summary>
        /// Class to encapsulate node layout information.</summary>
        protected class NodeLayoutInfo
        {
            /// <summary>
            /// The node with information.</summary>
            public Node Node;
            /// <summary>
            /// The x-coordinate of the upper-left corner of the node.</summary>
            public int X;
            /// <summary>
            /// The y-coordinate of the upper-left corner of the node.</summary>
            public int Y;
            /// <summary>
            /// The level of the node in the tree.</summary>
            public int Depth;
            /// <summary>
            /// The x-coordinate of the upper-left corner of the node image that indicates its state.</summary>
            public int StateImageLeft;
            /// <summary>
            /// The x-coordinate of the upper-left corner of the node icon image.</summary>
            public int ImageLeft;
            /// <summary>
            /// The x-coordinate of the upper-left corner of the drawn label text.</summary>
            public int LabelLeft;
        }

        /// <summary>
        /// Get or sets the vertical offset to be added to draw all tree nodes</summary>
        public int ContentVerticalOffset
        {
            get { return m_contentVerticalOffset; }
            set { m_contentVerticalOffset = value; }
        }

        /// <summary>
        /// Gets layout info of the visible nodes.</summary>
        /// <value>
        /// Enumeration of NodeLayoutInfo objects</value>
        protected IEnumerable<NodeLayoutInfo> NodeLayout
        {
            get
            {
                NodeLayoutInfo nodeInfo = new NodeLayoutInfo();

                int xPadding = Margin.Left;
                int yPadding = Margin.Top;

                int x = -m_hScroll + xPadding;
                int y = -m_vScroll + yPadding + ContentVerticalOffset;

                bool drawExpanders = DrawExpanders;

                Node lastNode = m_root; // whether or not it's visible, this must be first
                int depth = 0;
                foreach (Node node in VisibleNodes)
                {
                    // adjust when changing levels in tree
                    while (node.Parent != lastNode.Parent)
                    {
                        if (node.Parent == lastNode)
                        {
                            depth++;
                            x += Indent;
                            break;
                        }
                        else
                        {
                            depth--;
                            x -= Indent;
                            lastNode = lastNode.Parent;
                        }
                    }

                    nodeInfo.Node = node;
                    nodeInfo.Depth = depth;
                    nodeInfo.X = x;
                    nodeInfo.Y = y;

                    int left = x;
                    if (!drawExpanders)
                        left -= Indent;

                    if (node.HasCheck)
                    {
                        left += m_itemRenderer.CheckBoxSize.Width + xPadding;
                    }

                    nodeInfo.StateImageLeft = left;
                    if (m_stateImageList != null && node.StateImageIndex >= 0)
                    {
                        left += m_stateImageList.ImageSize.Width + 1; // no padding
                    }

                    nodeInfo.ImageLeft = left;
                    if (m_imageList != null && node.ImageIndex >= 0)
                    {
                        left += m_imageList.ImageSize.Width + xPadding;
                    }

                    nodeInfo.LabelLeft = left;

                    yield return nodeInfo;

                    y += GetRowHeight(node);
                    lastNode = node;
                }
            }
        }

        /// <summary>
        /// Type of object hit.</summary>
        protected enum HitType
        {
            None,
            Expander,
            CheckBox,
            Item,
            Label,
        }

        /// <summary>
        /// Structure to encapsulate hit information.</summary>
        protected struct HitRecord
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="HitRecord"/> struct.</summary>
            /// <param name="type">The type of object hit</param>
            /// <param name="node">The node hit</param>
            public HitRecord(HitType type, Node node)
            {
                Type = type;
                Node = node;
            }
            /// <summary>
            /// The type of object hit.</summary>
            public readonly HitType Type;
            /// <summary>
            /// The node hit.</summary>
            public Node Node;
        }

        /// <summary>
        /// Obtain hit information for a given point.</summary>
        /// <param name="p">The point to obtain info for</param>
        /// <returns>HitRecord for given point</returns>
        protected HitRecord Pick(Point p)
        {
            UpdateNodeMeasurements();
            int xPadding = Margin.Left;

            bool drawExpanders = (m_style != Style.CategorizedPalette);
            int right = m_clientSize.Width;

            foreach (NodeLayoutInfo info in NodeLayout)
            {
                Node node = info.Node;
                int rowHeight = GetRowHeight(node);
                
                if (rowHeight == 0 ||
                    p.Y >= info.Y + rowHeight)
                    continue;

                if (node != m_root && !node.IsLeaf)
                {
                    if (drawExpanders)
                    {
                        if (p.X >= info.X - Indent &&
                            p.X <= info.X - Indent + m_itemRenderer.ExpanderSize.Width)
                        {
                            return new HitRecord(HitType.Expander, node);
                        }
                    }
                    else
                    {
                        if ((node.Parent == m_root) || //single click to expand/collapse for 1st level of categorized palette 
                            (p.X > right - GdiUtil.OfficeExpanderSize - xPadding))
                            return new HitRecord(HitType.Expander, node);
                    }
                }

                if (node.HasCheck)
                {
                    if (p.X >= info.X &&
                        p.X <= info.X + m_itemRenderer.CheckBoxSize.Width)
                    {
                        return new HitRecord(HitType.CheckBox, node);
                    }
                }

                return p.X > info.LabelLeft ? new HitRecord(HitType.Label, node) : new HitRecord(HitType.Item, node);
            }

            return new HitRecord();
        }

        private bool DrawExpanders
        {
            get { return (m_style != Style.CategorizedPalette); }
        }

        /// <summary>
        /// Calculates the total height, in pixels, needed by this Node, including a margin.
        /// Checks everything--label, expander, checkbox, source control state image, regular image.</summary>
        /// <param name="node">Node whose height is calculated</param>
        /// <returns>Total height, in pixels, needed by this Node, including a margin</returns>
        public int GetRowHeight(Node node)
        {
            int rowHeight = node.LabelHeight;

            if (DrawExpanders && !node.IsLeaf)
            {
                rowHeight = Math.Max(
                    rowHeight,
                    m_itemRenderer.ExpanderSize.Height);
            }

            if (node.HasCheck)
            {
                rowHeight = Math.Max(
                    rowHeight,
                    m_itemRenderer.CheckBoxSize.Height);
            }

            if (m_stateImageList != null && node.StateImageIndex >= 0)
            {
                rowHeight = Math.Max(
                    rowHeight,
                    m_stateImageList.ImageSize.Height);
            }

            if (m_imageList != null && node.ImageIndex >= 0)
            {
                rowHeight = Math.Max(
                    rowHeight,
                    m_imageList.ImageSize.Height);
            }

            // Categories don't have to have labels. A category with no label should not have the
            //  category row drawn, but it's children should still be drawn. http://sf.ship.scea.com/sf/go/artf37516
            if (rowHeight == 0 &&
                m_style == Style.CategorizedPalette &&
                node.Parent == m_root)
            {
                return 0;
            }

            return rowHeight + Margin.Top;
        }

        /// <summary>
        /// Gets the total height of all of the rows of all the visible descendants of a node.
        /// Stops counting once the height surpasses a given 'maxHeight', in which case 'maxHeight'
        /// is returned. Goes in depth-first order, which is the order listed in the tree view.</summary>
        /// <param name="node">Node whose descendants's height is calculated</param>
        /// <param name="maxHeight">Maximum height allowed</param>
        /// <returns>Total height of all of the rows of all the visible descendants of a node</returns>
        private int GetChildrenHeight(Node node, int maxHeight)
        {
            UpdateNodeMeasurements();
            int height = 0;
            foreach (Node child in node.Children)
            {
                height += GetRowHeight(child);
                if (height > maxHeight)
                    return maxHeight;
                if (child.Expanded && !child.IsLeaf)
                    height += GetChildrenHeight(child, maxHeight - height);
            }
            return height;
        }

        private bool LabelEditModeContains(LabelEditModes flag)
        {
            return (m_labelEditMode & flag) != 0;
        }

        private bool NavigationKeyBehaviorContains(KeyboardShortcuts shortcut)
        {
            return (m_navigationKeyBehavior & shortcut) == shortcut;
        }

        /// <summary>
        /// Event data for Nodes</summary>
        public class NodeEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node that changed</param>
            public NodeEventArgs(Node node)
            {
                Node = node;
            }
            /// <summary>
            /// Node that changed</summary>
            public readonly Node Node;
        }

        /// <summary>
        /// Cancel event data for Nodes</summary>
        public class CancelNodeEventArgs : CancelEventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node that changed</param>
            public CancelNodeEventArgs(Node node)
            {
                Node = node;
            }
            /// <summary>
            /// Node that changed</summary>
            public readonly Node Node;
        }

        /// <summary>
        /// Node in tree control's hierarchy</summary>
        public class Node
        {
            internal Node(TreeControl owner, Node parent, object tag)
            {
                m_owner = owner;
                m_parent = parent;
                m_tag = tag;

                if (parent == null) // root node can't collapse
                    SetFlag(Flags.Expanded, true);

                //calculate LabelWidth and LabelHeight
                m_owner.m_nodesToMeasure.Add(this);
            }

            /// <summary>
            /// Gets the node's parent. Is null if this is the root node or if this
            /// node was recently removed from its parent. This property is always kept in
            /// sync with the Children property.</summary>
            public Node Parent
            {
                get { return m_parent; }
            }

            /// <summary>
            /// Gets the node's control.</summary>
            public TreeControl TreeControl
            {
                get { return m_owner; }
            }

            /// <summary>
            /// Gets or sets whether the node is expanded</summary>
            public bool Expanded
            {
                get { return GetFlag(Flags.Expanded); }
                set
                {
                    bool changing = GetFlag(Flags.Expanded) ^ value;
                    if (changing)
                    {
                        CancelNodeEventArgs eventArgs = new CancelNodeEventArgs(this);
                        m_owner.OnNodeExpandedChanging(eventArgs);
                        changing = (eventArgs.Cancel == false);
                    }

                    if (changing)
                    {
                        SetFlag(Flags.Expanded, value);
                        m_owner.OnNodeExpandedChanged(new NodeEventArgs(this));
                        m_owner.Invalidate();
                    }
                }
            }

            /// <summary>
            /// Gets or sets whether the node is partially expanded</summary>
            public bool PartiallyExpanded
            {
                get { return GetFlag(Flags.PartiallyExpanded); }
                set { SetFlag(Flags.PartiallyExpanded, value); }
            }

            /// <summary>
            /// Gets or sets whether the node can be selected</summary>
            public bool AllowSelect
            {
                get { return !GetFlag(Flags.NoSelect); }
                set { SetFlag(Flags.NoSelect, !value); }
            }

            /// <summary>
            /// Gets or sets whether the node is selected</summary>
            public bool Selected
            {
                get { return GetFlag(Flags.Selected); }
                set
                {
                    // prevent programmatic selection from violating selection mode
                    SelectionMode mode = m_owner.m_selectionMode;
                    if (mode == SelectionMode.None)
                        value = false;

                    bool selected = (m_flags & Flags.Selected) != 0;
                    if (selected != value)
                    {
                        if (!value || !GetFlag(Flags.NoSelect))
                        {
                            // make sure programmatic selection doesn't violate selection mode
                            if (mode == SelectionMode.One && value) // avoid infinite loop
                                m_owner.ClearSelection();

                            SetFlag(Flags.Selected, value);

                            // treat selection differently than label, checkstate, or other
                            //  properties, by reporting all changes, not just those caused by the user
                            m_owner.OnNodeSelectedChanged(new NodeEventArgs(this));
                            m_owner.Invalidate();
                        }
                    }
                }
            }

            /// <summary>
            /// Gets the node's child nodes. This property is always kept in sync with the Parent
            /// property.</summary>
            public IEnumerable<Node> Children
            {
                get { return m_children != null ? m_children.ToArray() : s_emptyArray; }
            }

            internal List<Node> InnerList
            {
                get { return m_children; }
            }

            /// <summary>
            /// Gets or sets whether the node is a leaf and shouldn't display an expander</summary>
            public bool IsLeaf
            {
                get { return GetFlag(Flags.IsLeaf); }
                set
                {
                    if (SetFlag(Flags.IsLeaf, value) != value)
                        m_owner.Invalidate();
                }
            }

            /// <summary>
            /// Adds a node with the given object tag to the node's children as the last child of the node</summary>
            /// <param name="tag">Tag object of new node</param>
            /// <returns>New child node</returns>
            public Node Add(object tag)
            {
                int index = 0;
                if (m_children != null)
                    index = m_children.Count;
                return Insert(index, tag);
            }

            /// <summary>
            /// Inserts a node with the given object tag into the node's children at a given index in the existing children</summary>
            /// <param name="index">Index of insertion point in the existing children</param>
            /// <param name="tag">Tag object of new node</param>
            /// <returns>New child node</returns>
            public Node Insert(int index, object tag)
            {
                if (m_children == null)
                    m_children = new List<Node>();

                Node result = new Node(m_owner, this, tag);
                m_children.Insert(index, result);
                IsLeaf = false; // make sure node can now be expanded
                m_owner.Invalidate();
                return result;
            }

            /// <summary>
            /// Removes the node with the given object tag from the node's children</summary>
            /// <param name="tag">Tag object</param>
            /// <returns>True iff the child node was removed</returns>
            public bool Remove(object tag)
            {
                if (m_children != null)
                {
                    for (int i = 0; i < m_children.Count; i++)
                    {
                        if (m_children[i].Tag.Equals(tag))
                        {
                            m_children[i].m_parent = null;
                            m_children.RemoveAt(i);

                            m_owner.UpdateNodeMeasurements();
                            m_owner.CleanUpSpecialNodes();
                            m_owner.Invalidate();
                            if (m_children.Count == 0)
                                m_children = null;

                            return true;
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// Clears the node by removing all child nodes</summary>
            public void Clear()
            {
                if (m_children != null)
                {
                    foreach (Node child in m_children)
                        child.m_parent = null;
                    m_children = null;

                    m_owner.UpdateNodeMeasurements();
                    m_owner.CleanUpSpecialNodes();
                    m_owner.Invalidate();
                }
            }

            /// <summary>
            /// Gets or sets the tag object</summary>
            public object Tag
            {
                get { return m_tag; }
                set { m_tag = value; }
            }

            /// <summary>
            /// Gets or sets the node's label text</summary>
            public string Label
            {
                get { return m_label; }
                set
                {
                    if (m_label != value)
                    {
                        m_label = value;
                        InvalidateLabelSize();
                        m_owner.Invalidate();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the font style used to render the node's label text. Is intended to be set
            /// by the TreeControlAdapter from an ItemInfo's FontStyle, which allows for the ITreeAdapter
            /// to indicate things like bold-faced text for properties whose values are no longer the default.
            /// The TreeItemRenderer takes this font style as a hint.</summary>
            /// <remarks>Default is FontStyle.Regular</remarks>
            public FontStyle FontStyle
            {
                get { return m_fontStyle; }
                set
                {
                    if (m_fontStyle != value)
                    {
                        m_fontStyle = value;
                        m_owner.Invalidate();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the mouse hover over text (also known as tool tip text)</summary>
            public string HoverText
            {
                get { return m_hoverText; }
                set
                {
                    if (string.Compare(value, m_hoverText) == 0)
                        return;

                    m_hoverText = value ?? string.Empty;
                }
            }

            /// <summary>
            /// Gets or sets whether the label can be edited by the user</summary>
            public bool AllowLabelEdit
            {
                get { return GetFlag(Flags.AllowLabelEdit); }
                set { SetFlag(Flags.AllowLabelEdit, value); }
            }

            /// <summary>
            /// Gets or sets whether the node should display a check box</summary>
            public bool HasCheck
            {
                get { return GetFlag(Flags.HasCheck); }
                set
                {
                    if (SetFlag(Flags.HasCheck, value))
                    {
                        m_owner.Invalidate();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the state of the node's check box</summary>
            public CheckState CheckState
            {
                get { return m_checkState; }
                set
                {
                    if (m_checkState != value)
                    {
                        m_checkState = value;
                        m_owner.Invalidate();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the index of the node's image in the parent control's ImageList</summary>
            public int ImageIndex
            {
                get { return m_imageIndex; }
                set
                {
                    if (m_imageIndex != value)
                    {
                        m_imageIndex = value;
                        m_owner.Invalidate();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the index of the node's state image in the parent control's StateImageList</summary>
            public int StateImageIndex
            {
                get { return m_stateImageIndex; }
                set
                {
                    if (m_stateImageIndex != value)
                    {
                        m_stateImageIndex = value;
                        m_owner.Invalidate();
                    }
                }
            }

            /// <summary>
            /// Gets or sets the node's label width</summary>
            public int LabelWidth
            {
                get
                {
                    if (m_labelWidth == -1)
                        throw new InvalidOperationException("Was not initialized by a call to UpdateNodeMeasurements");
                    return m_labelWidth;
                }
                set
                {
                    m_labelWidth = value;
                }
            }

            /// <summary>
            /// Gets or sets the node's label height</summary>
            public int LabelHeight
            {
                get
                {
                    if (m_labelHeight == -1)
                        throw new InvalidOperationException("Was not initialized by a call to UpdateNodeMeasurements");
                    return m_labelHeight;
                }
                set
                {
                    m_labelHeight = value;
                }
            }

            private void InvalidateLabelSize()
            {
                m_labelWidth = -1;
                m_labelHeight = -1;
                if (!m_owner.m_nodesToMeasure.Contains(this))
                    m_owner.m_nodesToMeasure.Add(this);
            }

            private readonly TreeControl m_owner;
            private Node m_parent;
            private object m_tag;
            private string m_label;
            private FontStyle m_fontStyle = FontStyle.Regular;
            private string m_hoverText = string.Empty;
            private List<Node> m_children;
            private int m_imageIndex = -1;
            private int m_stateImageIndex = -1;
            private int m_labelWidth = -1; //set by WndProc, using a GraphicsObject, after this is added to m_nodesToMeasure
            private int m_labelHeight = -1; //must be updated when LabelWidth is updated

            [Flags]
            private enum Flags
            {
                None = 0x0,
                IsLeaf = 0x1,
                Expanded = 0x2,
                Selected = 0x4,
                NoSelect = 0x8,
                HasCheck = 0x10,
                AllowLabelEdit = 0x20,
                PartiallyExpanded = 0x40, // Some, but not all, child nodes are displayed ( filtered tree view)
            }

            private bool GetFlag(Flags flag)
            {
                return (m_flags & flag) != 0;
            }

            private bool SetFlag(Flags flag, bool value)
            {
                Flags oldFlags = m_flags;
                if (value)
                    m_flags |= flag;
                else
                    m_flags &= ~flag;
                return oldFlags != m_flags;
            }

            private Flags m_flags;
            private CheckState m_checkState;

            private static readonly Node[] s_emptyArray =
                EmptyArray<Node>.Instance;
        }

        private readonly Style m_style;
        private TreeItemRenderer m_itemRenderer;
        private readonly Node m_root;
        private readonly HashSet<Node> m_nodesToMeasure = new HashSet<Node>();
        private Size m_clientSize; //The area that the items are drawn in. Doesn't include scrollbars.
        private int m_averageRowHeight;

        private readonly VScrollBar m_vScrollBar;
        private int m_vScroll;
        private readonly HScrollBar m_hScrollBar;
        private int m_hScroll;
        private readonly Timer m_autoScrollTimer;
        private int m_autoScrollSpeed = 50;
        private bool m_autoScrollOnExpand = true;
        private bool m_autoScrollUp;

        private int m_indent = 16;
        private ImageList m_imageList;
        private ImageList m_stateImageList;
        private Image m_filterImage;
        private ToolTip m_toolTip;
        private Point m_lastHoverPosition = new Point(Int32.MinValue, Int32.MinValue); //in this Control's coordinate system
        private bool m_resetHoverEvent;

        private SelectionMode m_selectionMode = SelectionMode.MultiExtended;
        private Node m_extendSelectionBaseNode;
        private Node m_currentKeyedNode;
        private Node m_leftClickedSelectedNode;
        private Node m_labelEditNode;
        private Point m_mouseDownPoint;

        private readonly TextBox m_textBox;

        private readonly Timer m_dragHoverExpandTimer;
        private Node m_dragHoverNode;

        private readonly Timer m_editLabelTimer;

        private bool m_showRoot = true;
        private bool m_dragHoverExpand;
        private bool m_selecting;
        private bool m_dragging;
        private bool m_lastMouseDownWasDoubleClick;
        private bool m_toggleOnDoubleClick = true;
        private bool m_expandOnSingleClick;
        private bool m_dragBetween;
        private bool m_showDragBetweenCue;
        /// <summary>
        /// Whether key up event was handled or not.</summary>
        protected bool m_handleKeyUp;
        private int m_contentVerticalOffset;

        private KeyboardShortcuts m_navigationKeyBehavior;
        private LabelEditModes m_labelEditMode = LabelEditModes.Default;
    }
}