//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Partial implementation of the TreeListView class, which provides a tree ListView</summary>
    public sealed partial class TreeListView
    {
        /// <summary>
        /// Represents a node item on the TreeListView</summary>
        public class Node : IAdaptable
        {
            /// <summary>
            /// Constructor</summary>
            public Node()
            {
                m_children = new NodeCollection(this);

                Visible = true;

                // Node level's start @ 1
                Level = 1;

                IsLeaf = false;
            }

            #region IAdaptable Interface

            /// <summary>
            /// Gets an adapter of the specified type or null</summary>
            /// <param name="type">Adapter type</param>
            /// <returns>Adapter of the specified type or null if no adapter available</returns>
            public object GetAdapter(Type type)
            {
                if (Tag == null)
                    return null;

                Type tagType = Tag.GetType();
                if (type.Equals(tagType))
                    return Tag;

                return type.IsAssignableFrom(tagType) ? Tag : null;
            }

            #endregion

            /// <summary>
            /// Gets the parent node or null if a root</summary>
            public Node Parent { get; internal set; }

            /// <summary>
            /// Gets or sets whether the node can have children or not</summary>
            /// <remarks>Setting to false makes the node's Nodes collection readonly!</remarks>
            public bool IsLeaf { get; set; }

            /// <summary>
            /// Gets whether the node has any children</summary>
            public bool HasChildren
            {
                get { return Nodes.Count > 0; }
            }

            /// <summary>
            /// Gets the node's children as a NodeCollection</summary>
            public NodeCollection Nodes
            {
                get { return m_children; }
            }

            /// <summary>
            /// Gets or sets user data attached to node</summary>
            public object Tag { get; set; }

            /// <summary>
            /// Gets or sets the node's label</summary>
            public string Label
            {
                get { return m_label; }
                set
                {
                    if (string.Compare(value, m_label) == 0)
                        return;

                    m_label = value;

                    LabelChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets or sets whether the node is expanded</summary>
            public bool Expanded
            {
                get { return m_expanded; }
                set
                {
                    if (value == m_expanded)
                        return;

                    if (m_expandedChanging)
                        return;

                    try
                    {
                        m_expandedChanging = true;
                        m_expanded = value;

                        ExpandedChanged.Raise(this, EventArgs.Empty);
                    }
                    finally
                    {
                        m_expandedChanging = false;
                    }
                }
            }

            /// <summary>
            /// Gets or sets the node's check state</summary>
            public CheckState CheckState
            {
                get { return m_checkState; }
                set
                {
                    if (m_checkState == value)
                        return;

                    m_checkState = value;

                    CheckStateChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets or sets the node's properties</summary>
            /// <remarks>Properties represent column data. Raises the PropertiesChanged event.</remarks>
            public object[] Properties
            {
                get { return m_properties; }
                set
                {
                    if (ReferenceEquals(m_properties, value))
                        return;

                    m_properties = value;

                    PropertiesChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets or sets the node's image index</summary>
            public int ImageIndex
            {
                get { return m_imageIndex; }
                set
                {
                    if (m_imageIndex == value)
                        return;

                    m_imageIndex = value;

                    ImageIndexChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets or sets the node's state image index</summary>
            public int StateImageIndex
            {
                get { return m_stateImageIndex; }
                set
                {
                    if (m_stateImageIndex == value)
                        return;

                    m_stateImageIndex = value;

                    StateImageIndexChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets or sets whether the Node is selected or not</summary>
            /// <remarks>Raises the SelectedChanged event</remarks>
            public bool Selected
            {
                get { return m_selected; }
                set
                {
                    if (m_selected == value)
                        return;

                    m_selected = value;

                    SelectedChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Gets or sets the node's font style</summary>
            public FontStyle FontStyle
            {
                get { return m_fontStyle; }
                set
                {
                    if (m_fontStyle == value)
                        return;

                    m_fontStyle = value;

                    FontStyleChanged.Raise(this, EventArgs.Empty);
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

                    HoverTextChanged.Raise(this, EventArgs.Empty);
                }
            }

            /// <summary>
            /// Event that is raised after the node's label changed</summary>
            public event EventHandler LabelChanged;

            /// <summary>
            /// Event that is raised after the node's Expanded property changed</summary>
            public event EventHandler ExpandedChanged;

            /// <summary>
            /// Event that is raised after the node's check state changed</summary>
            public event EventHandler CheckStateChanged;

            /// <summary>
            /// Event that is raised after the node's properties changed</summary>
            public event EventHandler PropertiesChanged;

            /// <summary>
            /// Event that is raised after the node's image index changed</summary>
            public event EventHandler ImageIndexChanged;

            /// <summary>
            /// Event that is raised after the node's state image index changed</summary>
            public event EventHandler StateImageIndexChanged;

            /// <summary>
            /// Event that is raised after the node is selected or deselected</summary>
            public event EventHandler SelectedChanged;

            /// <summary>
            /// Event that is raised after the font style is changed</summary>
            public event EventHandler FontStyleChanged;

            /// <summary>
            /// Event that is raised after the hover text property is changed</summary>
            public event EventHandler HoverTextChanged;

            /// <summary>
            /// Gets the node's root level ancestor</summary>
            /// <remarks>Used for rendering</remarks>
            internal Node RootLevelAncestor
            {
                get
                {
                    Node node = this;
                    while (node.Parent != null)
                        node = node.Parent;

                    return node;
                }
            }

            /// <summary>
            /// Gets or sets the node's next sibling</summary>
            /// <remarks>Used for rendering</remarks>
            internal Node Next { get; set; }

            /// <summary>
            /// Gets or sets the node's previous sibling</summary>
            /// <remarks>Used for rendering</remarks>
            internal Node Previous { get; set; }

            /// <summary>
            /// Gets or sets whether the node is visible</summary>
            /// <remarks>Used for rendering</remarks>
            internal bool Visible { get; set; }

            /// <summary>
            /// Gets or sets the node's indentation level</summary>
            /// <remarks>Used for rendering</remarks>
            internal int Level { get; set; }

            /// <summary>
            /// Gets or sets the node's expander/collapser clickable area</summary>
            /// <remarks>Used for rendering</remarks>
            internal Rectangle HitRect { get; set; }

            /// <summary>
            /// Gets or sets the node's checkbox clickable area</summary>
            internal Rectangle CheckBoxHitRect { get; set; }

            /// <summary>
            /// Gets or sets the node's label clickable area</summary>
            internal Rectangle LabelHitRect { get; set; }

            /// <summary>
            /// Gets whether the node can be expanded in the first place or not</summary>
            /// <remarks>Used for rendering</remarks>
            internal bool Expandable
            {
                get
                {
                    foreach (Node node in Nodes)
                    {
                        if (node.Visible)
                            return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// Gets whether the node needs to be lazy loaded or not</summary>
            /// <remarks>Used for rendering</remarks>
            internal bool NeedsLazyLoad
            {
                get { return !IsLeaf && !HasChildren; }
            }

            /// <summary>
            /// Gets or sets the node's visual sort position</summary>
            /// <remarks>Used for rendering</remarks>
            internal UInt64 VisualPosition { get; set; }

            /// <summary>
            /// Set the property</summary>
            /// <param name="index">Position</param>
            /// <param name="value">Value</param>
            internal void SetProperty(int index, object value)
            {
                if (index >= m_properties.Length)
                {
                    throw new InvalidOperationException("Property index is greater than the number of properties for this node.");
                }

                m_properties[index] = value;
                PropertiesChanged.Raise(this, EventArgs.Empty);
            }

            private object[] m_properties;

            private bool m_expanded;
            private bool m_selected;
            private bool m_expandedChanging;
            private int m_imageIndex = InvalidImageIndex;
            private int m_stateImageIndex = InvalidImageIndex;
            private string m_label = string.Empty;
            private string m_hoverText = string.Empty;
            private FontStyle m_fontStyle = FontStyle.Regular;
            private CheckState m_checkState = CheckState.Unchecked;

            private readonly NodeCollection m_children;
        }

        /// <summary>
        /// Represents a node collection</summary>
        public class NodeCollection : ICollection<Node>
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="owner">The node that owns the collection</param>
            internal NodeCollection(Node owner)
            {
                m_owner = owner;
            }

            /// <summary>
            /// Gets the node that owns the collection</summary>
            public Node Owner
            {
                get { return m_owner; }
            }

            /// <summary>
            /// Sorts the collection</summary>
            /// <param name="comparer">The comparer to use when sorting</param>
            internal void Sort(IComparer<Node> comparer)
            {
                m_nodes.Sort(comparer);
            }

            #region ICollection<Node> Interface

            /// <summary>
            /// Gets the Node enumerator</summary>
            /// <returns>Node enumerator</returns>
            public IEnumerator<Node> GetEnumerator()
            {
                return m_nodes.GetEnumerator();
            }

            /// <summary>
            /// Gets the Node enumerator</summary>
            /// <returns>Node enumerator</returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_nodes.GetEnumerator();
            }

            /// <summary>
            /// Adds a node to the collection</summary>
            /// <remarks>Can't contain duplicates!</remarks>
            /// <param name="item">The Node to add</param>
            public void Add(Node item)
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("collection is read only");

                if (Contains(item))
                    return;

                if (m_owner != null)
                    item.Parent = m_owner;

                var ea = new CancelNodeEventArgs(item);

                bool cancelled = NodeAdding.RaiseCancellable(this, ea);
                if (cancelled)
                    return;

                m_nodes.Add(item);

                NodeAdded.Raise(this, new NodeEventArgs(item));
            }

            /// <summary>
            /// Removes all nodes from the collection</summary>
            /// <remarks>Recursively removes child collection nodes, too</remarks>
            public void Clear()
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("collection is read only");

                // Store items to avoid modification while enumerating
                var children = new List<Node>(m_nodes);

                // Recursively clear
                foreach (Node child in children)
                {
                    if (child.HasChildren)
                        child.Nodes.Clear();
                }

                NodesRemoving.Raise(this, new NodesRemovingEventArgs(Owner, children));

                m_nodes.Clear();
            }

            /// <summary>
            /// Checks if a Node is in the collection</summary>
            /// <param name="item">Node to check</param>
            /// <returns><c>True</c> if Node in the collection</returns>
            public bool Contains(Node item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                return m_nodes.Contains(item);
            }

            /// <summary>
            /// Copies all Nodes in collection to an array</summary>
            /// <param name="array">Array</param>
            /// <param name="arrayIndex">Starting position in array to copy all Nodes to</param>
            public void CopyTo(Node[] array, int arrayIndex)
            {
                if (array == null)
                    throw new ArgumentNullException("array");

                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException("arrayIndex");

                for (int i = 0; i < Count; i++)
                    array[i + arrayIndex] = m_nodes[i];
            }

            /// <summary>
            /// Removes a Node from the collection</summary>
            /// <param name="item">Node</param>
            /// <returns><c>True</c> if Node removed</returns>
            public bool Remove(Node item)
            {
                if (item == null)
                    throw new ArgumentNullException("item");

                if (IsReadOnly)
                    throw new InvalidOperationException("collection is read only");

                if (item.HasChildren)
                    item.Nodes.Clear();

                NodeRemoving.Raise(this, new NodeEventArgs(item));

                return m_nodes.Remove(item);
            }

            /// <summary>
            /// Gets the number of nodes in the collection</summary>
            public int Count
            {
                get { return m_nodes.Count; }
            }

            /// <summary>
            /// Get whether the collection is read only or not</summary>
            /// <remarks>If it's read only, exceptions are thrown if Nodes are attempted to be added or removed.</remarks>
            public bool IsReadOnly
            {
                get { return (m_owner != null) && m_owner.IsLeaf; }
            }

            #endregion

            /// <summary>
            /// Event fired when a node is being added</summary>
            internal event EventHandler<CancelNodeEventArgs> NodeAdding;

            /// <summary>
            /// Event fired after a node is added</summary>
            internal event EventHandler<NodeEventArgs> NodeAdded;

            /// <summary>
            /// Event fired when a node is being removed</summary>
            internal event EventHandler<NodeEventArgs> NodeRemoving;

            /// <summary>
            /// Event fired when NodeCollection Clear() is called</summary>
            internal event EventHandler<NodesRemovingEventArgs> NodesRemoving;

            private readonly Node m_owner;
            private readonly List<Node> m_nodes = new List<Node>();
        }

        /// <summary>
        /// Node event arguments</summary>
        public class NodeEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node</param>
            public NodeEventArgs(Node node)
            {
                Node = node;
            }

            /// <summary>
            /// Gets the node</summary>
            public Node Node { get; private set; }
        }

        /// <summary>
        /// Event args that check if a node's label can be edited in-place</summary>
        public class CanLabelEditEventArgs : NodeEventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node</param>
            public CanLabelEditEventArgs(Node node)
                : base(node)
            {
                CanEdit = true;
            }

            /// <summary>
            /// Gets or sets whether or not the label can be edited in-place</summary>
            public bool CanEdit { get; set; }
        }

        /// <summary>
        /// Node label edit event arguments</summary>
        public class NodeLabelEditEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node</param>
            /// <param name="label">New label</param>
            public NodeLabelEditEventArgs(Node node, string label)
            {
                Node = node;
                Label = label;
            }

            /// <summary>
            /// Gets the node</summary>
            public Node Node { get; private set; }

            /// <summary>
            /// Gets the new label</summary>
            public string Label { get; private set; }

            /// <summary>
            /// Gets or sets whether to cancel the label change</summary>
            public bool CancelEdit { get; set; }
        }

        /// <summary>
        /// Event args that check if a node's property can be changed</summary>
        public class CanPropertyChangeEventArgs : NodeEventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node</param>
            /// <param name="propertyIndex">Property index</param>
            public CanPropertyChangeEventArgs(Node node, int propertyIndex)
                : base(node)
            {
                PropertyIndex = propertyIndex;
                CanChange = true;
            }

            /// <summary>
            /// Gets the property index</summary>
            public int PropertyIndex { get; private set; }

            /// <summary>
            /// Gets or sets whether or not the property can be changed</summary>
            public bool CanChange { get; set; }
        }

        /// <summary>
        /// Property changed event args</summary>
        public class PropertyChangedEventArgs : EventArgs
        {
            /// <summary>
            /// Initialises an instance of NodePropertyEditEventArgs </summary>
            /// <param name="node">Node that contains changed property</param>
            /// <param name="propertyIndex">Index of property that has changed</param>
            /// <param name="value">New property value</param>
            public PropertyChangedEventArgs(Node node, int propertyIndex, object value)
            {
                Node = node;
                PropertyIndex = propertyIndex;
                Value = value;
            }

            /// <summary>
            /// Gets the node</summary>
            public Node Node { get; private set; }

            /// <summary>
            /// Gets the index of the property that has changed</summary>
            public int PropertyIndex { get; private set; }

            /// <summary>
            /// The new property value</summary>
            public object Value { get; private set; }

            /// <summary>
            /// Callbacks can set this to true to cancel the change</summary>
            public bool CancelChange { get; set; }
        }

        /// <summary>
        /// Types of Node updates/changes</summary>
        [Flags]
        internal enum NodeChangeTypes
        {
            /// <summary>
            /// No change</summary>
            None                = 0x00,

            /// <summary>
            /// Label changed</summary>
            Label               = 0x001,

            /// <summary>
            /// Expanded property changed</summary>
            Expanded            = 0x002,

            /// <summary>
            /// CheckState property changed</summary>
            CheckState          = 0x004,

            /// <summary>
            /// Properties property changed</summary>
            Properties          = 0x008,

            /// <summary>
            /// ImageIndex property changed</summary> 
            ImageIndex          = 0x010,

            /// <summary>
            /// StateImageIndex property changed</summary>
            StateImageIndex     = 0x020,

            /// <summary>
            /// Selected property changed</summary>
            Selected            = 0x040,

            /// <summary>
            /// FontStyle property changed</summary>
            FontStyle           = 0x080,

            /// <summary>
            /// HoverText property changed</summary>
            HoverText           = 0x100,
        }

        /// <summary>
        /// Nodes removing event arguments</summary>
        internal class NodesRemovingEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="owner">Node that items are being removed from</param>
            /// <param name="nodes">Nodes to remove</param>
            public NodesRemovingEventArgs(Node owner, IEnumerable<Node> nodes)
            {
                Owner = owner;
                Nodes = nodes;
            }

            /// <summary>
            /// Gets node owner</summary>
            public Node Owner { get; private set; }

            /// <summary>
            /// Gets nodes to remove</summary>
            public IEnumerable<Node> Nodes { get; private set; }
        }

        /// <summary>
        /// Cancelable node event arguments</summary>
        internal class CancelNodeEventArgs : CancelEventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node</param>
            public CancelNodeEventArgs(Node node)
            {
                Node = node;
            }

            /// <summary>
            /// Gets the node</summary>
            public Node Node { get; private set; }
        }

        /// <summary>
        /// Class used for rendering Nodes on the TreeListView</summary>
        public class NodeRenderer
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="owner">TreeListView that owns the renderer</param>
            public NodeRenderer(TreeListView owner)
            {
                Owner = owner;
            }

            /// <summary>
            /// Draws the background</summary>
            /// <param name="gfx">GDI+ graphics object</param>
            /// <param name="bounds">Rectangle bounding area to draw</param>
            public virtual void DrawBackground(Graphics gfx, Rectangle bounds)
            {
                if (Owner.Control.Enabled)
                {
                    using (Brush b = new SolidBrush(Owner.BackColor))
                        gfx.FillRectangle(b, bounds);
                }
                else
                {
                    using (Brush b = new SolidBrush(Owner.DisabledBackColor))
                        gfx.FillRectangle(b, bounds);
                }
            }

            /// <summary>
            /// Draws the background</summary>
            /// /// <param name="node">Node whose background is drawn</param>
            /// <param name="gfx">GDI+ graphics object</param>
            /// <param name="bounds">Rectangle bounding area to draw</param>
            public virtual void DrawBackground(Node node, Graphics gfx, Rectangle bounds)
            {
                if (Owner.Control.Enabled)
                {
                    using (Brush b = new SolidBrush(Owner.BackColor))
                        gfx.FillRectangle(b, bounds);
                }
                else
                {
                    using (Brush b = new SolidBrush(Owner.DisabledBackColor))
                        gfx.FillRectangle(b, bounds);
                }
            }

            /// <summary>
            /// Draws the label</summary>
            /// <param name="node">Node whose label is drawn</param>
            /// <param name="gfx">GDI+ graphics object</param>
            /// <param name="bounds">Rectangle bounding area to draw</param>
            /// <param name="column">Column to draw. Is zero for node label, &gt; 0 if one of Properties</param>
            public virtual void DrawLabel(Node node, Graphics gfx, Rectangle bounds, int column)
            {
                string text =
                    column == 0
                        ? node.Label
                        : ((node.Properties != null) &&
                           (node.Properties.Length >= column))
                            ? GetObjectString(node.Properties[column - 1])
                            : null;

                if (string.IsNullOrEmpty(text))
                    text = string.Empty;

                TextFormatFlags flags = TextFormatFlags.VerticalCenter;

                // Add ellipsis if needed
                {
                    Size textSize = TextRenderer.MeasureText(gfx, text, Owner.Control.Font);

                    if (textSize.Width > bounds.Width)
                        flags |= TextFormatFlags.EndEllipsis;
                }

                if (node.Selected && Owner.Control.Enabled)
                {
                    using (Brush b = new SolidBrush(Owner.HighlightBackColor))
                        gfx.FillRectangle(b, bounds);
                }

                Color textColor =
                    node.Selected
                        ? Owner.HighlightTextColor
                        : Owner.TextColor;

                if (!Owner.Control.Enabled)
                    textColor = Owner.DisabledTextColor;

                var disposeFont = false;
                var font = Owner.Control.Font;
                if (node.FontStyle != FontStyle.Regular)
                {
                    font = new Font(font, node.FontStyle);
                    disposeFont = true;
                }

                TextRenderer.DrawText(gfx, text, font, bounds, textColor, flags);

                if (disposeFont)
                    font.Dispose();
            }

            /// <summary>
            /// Draws the checkbox when the TreeListView is using the CheckedList style</summary>
            /// <param name="node">Node whose checkbox is drawn</param>
            /// <param name="gfx">GDI+ graphics object</param>
            /// <param name="bounds">Rectangle bounding area to draw</param>
            public virtual void DrawCheckBox(Node node, Graphics gfx, Rectangle bounds)
            {
                bool enabled = Owner.Control.Enabled;

                CheckBoxState state =
                    node.CheckState == CheckState.Checked
                        ? (enabled
                            ? CheckBoxState.CheckedNormal
                            : CheckBoxState.CheckedDisabled)
                        : (enabled
                            ? CheckBoxState.UncheckedNormal
                            : CheckBoxState.UncheckedDisabled);

                CheckBoxRenderer.DrawCheckBox(gfx, bounds.Location, state);
            }

            /// <summary>
            /// Draws the image/icon</summary>
            /// <param name="node">Node whose image is drawn</param>
            /// <param name="gfx">GDI+ graphics object</param>
            /// <param name="bounds">Rectangle bounding area to draw</param>
            public virtual void DrawImage(Node node, Graphics gfx, Rectangle bounds)
            {
                // No image to draw
                if (node.ImageIndex == InvalidImageIndex)
                    return;

                if (Owner.ImageList == null)
                    return;

                if (node.ImageIndex >= Owner.ImageList.Images.Count)
                    return;

                // intentionally not using Owner.ImageList.Draw() any more due to rogue
                // pixels that show up if the image size doesn't match the bounds size

                using (Image image = Owner.ImageList.Images[node.ImageIndex])
                {
                    if (Owner.Control.Enabled)
                        gfx.DrawImage(image, bounds);
                    else
                        ControlPaint.DrawImageDisabled(gfx, image, bounds.X, bounds.Y, Owner.DisabledBackColor);
                }
            }

            /// <summary>
            /// Draws the state image/icon</summary>
            /// <param name="node">Node whose state image is drawn</param>
            /// <param name="gfx">GDI+ graphics object</param>
            /// <param name="bounds">Rectangle bounding area to draw</param>
            public virtual void DrawStateImage(Node node, Graphics gfx, Rectangle bounds)
            {
                // No image to draw
                if (node.StateImageIndex == InvalidImageIndex)
                    return;

                if (Owner.StateImageList == null)
                    return;

                if (node.StateImageIndex >= Owner.StateImageList.Images.Count)
                    return;

                // intentionally not using Owner.StateImageList.Draw() any more due to
                // match DrawImage behavior. see comment in DrawImage.

                using (Image image = Owner.StateImageList.Images[node.StateImageIndex])
                {
                    if (Owner.Control.Enabled)
                        gfx.DrawImage(image, bounds);
                    else
                        ControlPaint.DrawImageDisabled(gfx, image, bounds.X, bounds.Y, Owner.DisabledBackColor);
                }
            }

            ///// <summary>
            ///// Draws the column header</summary>
            ///// /// <param name="column">Column</param>
            ///// <param name="gfx">GDI+ graphics object</param>
            ///// <param name="bounds">Rectangle bounding area to draw</param>
            ///// <remarks>Column will be null when drawing the area to the right of the last header</remarks>
            //public virtual void DrawColumnHeader(Column column, Graphics gfx, Rectangle bounds)
            //{
            //    { // will want to keep the chunk in these braces, always

            //        using (Brush brush = new SolidBrush(Owner.ColumnHeaderSeparatorColor))
            //        {
            //            gfx.FillRectangle(brush, bounds);
            //        }

            //        Rectangle gradRect =
            //            new Rectangle(
            //                bounds.X,
            //                bounds.Y,
            //                bounds.Width - 1,
            //                bounds.Height - 1);

            //        using (LinearGradientBrush brush =
            //            new LinearGradientBrush(
            //                gradRect,
            //                Owner.ColumnHeaderGradient.StartColor,
            //                Owner.ColumnHeaderGradient.EndColor,
            //                Owner.ColumnHeaderGradient.LinearGradientMode))
            //        {
            //            gfx.FillRectangle(brush, gradRect);
            //        }
            //    }

            //    // there's an area to the right of the valid column headers that we can't draw.
            //    // we fake this out by always drawing the filled rectangle, as the TreeListView
            //    // will give us valid bounds for the column we can't draw, but there's obviously
            //    // no valid column data for this column so we must bail. now.

            //    if (column == null)
            //        return;

            //    string text = column.Label;
            //    if (string.IsNullOrEmpty(text))
            //        return;

            //    Font font = Owner.Control.Font;
            //    Color textColor = Owner.ColumnHeaderGradient.TextColor;
            //    TextFormatFlags flags = TextFormatFlags.VerticalCenter;

            //    {
            //        Size textSize = TextRenderer.MeasureText(gfx, text, font);

            //        if (textSize.Width > bounds.Width)
            //            flags |= TextFormatFlags.EndEllipsis;
            //    }

            //    TextRenderer.DrawText(gfx, text, font, bounds, textColor, flags);
            //}

            /// <summary>
            /// Gets the TreeListView that uses the renderer</summary>
            public TreeListView Owner { get; private set; }
        }
    }
}