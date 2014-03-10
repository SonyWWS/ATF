//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Partial implementation of the TreeListView class, which provides a tree ListView</summary>
    public sealed partial class TreeListView
    {
        /// <summary>
        /// Private class for the TreeListView wrapper. This is marked internal so SkinService can set some of its values.</summary>
        internal class TheTreeListView : ListView, IAdaptable
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="style">Style</param>
            /// <param name="owner">Owner</param>
            internal TheTreeListView(Style style, TreeListView owner)
            {
                m_style = style;
                m_owner = owner;

                OwnerDraw = true;

                base.DoubleBuffered = true;
                SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

                MouseMove += ListTreeViewMouseMove; // for a workaround

                DrawColumnHeader += ListTreeViewDrawColumnHeader;
                DrawItem += ListTreeViewDrawItem;
                DrawSubItem += ListTreeViewDrawSubItem;

                // default colors & such
                BorderStyle = BorderStyle.Fixed3D;
                m_textColor = SystemColors.ControlText;
                m_modifiableTextColor = Color.Red;
                m_highlightTextColor = SystemColors.HighlightText;
                m_modifiableHighlightTextColor = SystemColors.ControlText;
                BackColor = SystemColors.ControlLightLight;
                m_disabledTextColor = SystemColors.GrayText;
                m_highlightBackColor = ((SolidBrush)SystemBrushes.Highlight).Color;
                m_disabledBackColor = ((SolidBrush)SystemBrushes.Control).Color;
                m_gridLinesColor = DefaultBackColor;
                m_expanderGradient =
                    new ControlGradient
                        {
                            StartColor = Color.White,
                            EndColor = Color.LightGray,
                            LinearGradientMode = LinearGradientMode.Vertical
                        };
                m_expanderPen = Pens.Black;
                m_hierarchyLinePen = Pens.DarkGray;
                //m_columnHeaderGradient =
                //    new ControlGradient
                //        {
                //            StartColor = Color.White,
                //            EndColor = Color.FromArgb(255, 242, 243, 245),
                //            LinearGradientMode = LinearGradientMode.Vertical,
                //            TextColor = SystemColors.ControlText
                //        };
                //m_columnHeaderSeparatorColor = Color.FromArgb(255, 213, 213, 213);
            }

            /// <summary>
            /// Event fired when control is scrolled</summary>
            public event ScrollEventHandler Scroll;

            /// <summary>
            /// Gets or sets the text color</summary>
            public Color TextColor
            {
                get { return m_textColor; }
                set { m_textColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the modifiable text color</summary>
            public Color ModifiableTextColor
            {
                get { return m_modifiableTextColor; }
                set { m_modifiableTextColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the highlight text color</summary>
            public Color HighlightTextColor
            {
                get { return m_highlightTextColor; }
                set { m_highlightTextColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the modifiable highlight text color</summary>
            public Color ModifiableHighlightTextColor
            {
                get { return m_modifiableHighlightTextColor; }
                set { m_modifiableHighlightTextColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the disabled text color</summary>
            public Color DisabledTextColor
            {
                get { return m_disabledTextColor; }
                set { m_disabledTextColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the highlight background color</summary>
            public Color HighlightBackColor
            {
                get { return m_highlightBackColor; }
                set { m_highlightBackColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the disabled background color</summary>
            public Color DisabledBackColor
            {
                get { return m_disabledBackColor; }
                set { m_disabledBackColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the grid lines color</summary>
            public Color GridLinesColor
            {
                get { return m_gridLinesColor; }
                set { m_gridLinesColor = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets whether grid lines are visible</summary>
            public new bool GridLines
            {
                get { return m_gridLines; }
                set { m_gridLines = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the expander gradient</summary>
            public ControlGradient ExpanderGradient
            {
                get { return m_expanderGradient; }
                set { m_expanderGradient = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the expander and collapser pen</summary>
            public Pen ExpanderPen
            {
                get { return m_expanderPen; }
                set { m_expanderPen = value; Invalidate(); }
            }

            /// <summary>
            /// Gets or sets the hierarchy line pen</summary>
            public Pen HierarchyLinePen
            {
                get { return m_hierarchyLinePen; }
                set { m_hierarchyLinePen = value; Invalidate(); }
            }

            ///// <summary>
            ///// Gets or sets the column header gradient</summary>
            //public ControlGradient ColumnHeaderGradient
            //{
            //    get { return m_columnHeaderGradient; }
            //    set { m_columnHeaderGradient = value; Invalidate(); }
            //}

            ///// <summary>
            ///// Gets or sets the column header separator color</summary>
            //public Color ColumnHeaderSeparatorColor
            //{
            //    get { return m_columnHeaderSeparatorColor; }
            //    set { m_columnHeaderSeparatorColor = value; Invalidate(); }
            //}

            #region IAdaptable Interface

            /// <summary>
            /// Gets an adapter of the specified type or null</summary>
            /// <param name="type">Adapter type</param>
            /// <returns>Adapter of the specified type or null if no adapter available</returns>
            public object GetAdapter(Type type)
            {
                return type == typeof(TreeListView) ? m_owner : null;
            }

            #endregion

            internal Style TheStyle
            {
                get { return m_style; }
            }

            internal NodeRenderer Renderer
            {
                get { return m_renderer ?? (m_renderer = new NodeRenderer(m_owner)); }
                set { m_renderer = value ?? new NodeRenderer(m_owner); }
            }

            internal void ResetWorkaroundList()
            {
                m_lstWorkaround.Clear();
            }

            /// <summary>
            /// WndProc</summary>
            /// <param name="m">Message</param>
            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case User32.WM_PAINT:
                    {
                        try
                        {
                            m_painting = true;
                            base.WndProc(ref m);

                            bool drawBackgroundHere =
                                (
                                    ((TheStyle == Style.VirtualList) && (VirtualListSize == 0)) ||
                                    ((TheStyle != Style.VirtualList) && (Items.Count == 0))
                                );

                            // The Draw[Sub]Item code is never called if there are no items, so we have to
                            // do some custom stuff here to ensure the background is somewhat correct. This
                            // is really only noticeable when using a custom skin.

                            if (drawBackgroundHere)
                            {
                                using (Graphics gfx = CreateGraphics())
                                    Renderer.DrawBackground(gfx, Bounds);
                            }
                        }
                        finally
                        {
                            m_painting = false;
                        }
                    }
                    break;

                    case User32.WM_VSCROLL:
                    case User32.WM_MOUSEWHEEL:
                    {
                        base.WndProc(ref m);

                        if (Scroll != null)
                            Scroll(this, new ScrollEventArgs(ScrollEventType.EndScroll, User32.GetScrollPos(Handle, SB_VERT)));
                    }
                    break;

                    case User32.WM_HSCROLL:
                    {
                        base.WndProc(ref m);

                        if (Scroll != null)
                            Scroll(this, new ScrollEventArgs(ScrollEventType.EndScroll, User32.GetScrollPos(Handle, SB_HORZ)));
                    }
                    break;

                    case User32.WM_KEYDOWN:
                    {
                        base.WndProc(ref m);

                        if (Scroll != null)
                        {
                            switch (m.WParam.ToInt32())
                            {
                                case (int)Keys.Down:
                                    Scroll(this, new ScrollEventArgs(ScrollEventType.SmallIncrement, User32.GetScrollPos(Handle, SB_VERT)));
                                    break;

                                case (int)Keys.Up:
                                    Scroll(this, new ScrollEventArgs(ScrollEventType.SmallDecrement, User32.GetScrollPos(Handle, SB_VERT)));
                                    break;

                                case (int)Keys.PageDown:
                                    Scroll(this, new ScrollEventArgs(ScrollEventType.LargeIncrement, User32.GetScrollPos(Handle, SB_VERT)));
                                    break;

                                case (int)Keys.PageUp:
                                    Scroll(this, new ScrollEventArgs(ScrollEventType.LargeDecrement, User32.GetScrollPos(Handle, SB_VERT)));
                                    break;

                                case (int)Keys.Home:
                                    Scroll(this, new ScrollEventArgs(ScrollEventType.First, User32.GetScrollPos(Handle, SB_VERT)));
                                    break;

                                case (int)Keys.End:
                                    Scroll(this, new ScrollEventArgs(ScrollEventType.Last, User32.GetScrollPos(Handle, SB_VERT)));
                                    break;
                            }
                        }
                    }
                    break;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }

            //private void DrawHeaderHack()
            //{
            //    if (Disposing || IsDisposed)
            //        return;

            //    // We can't draw the right most faux column in the draw-column-header-override,
            //    // so try and do it here!

            //    Rectangle bounds;
            //    {
            //        int pos = 0;
            //        for (int i = 0; i < Columns.Count; ++i)
            //            pos += Columns[i].Width;

            //        bounds = new Rectangle(pos, 0, ClientRectangle.Width, m_columnHeight);
            //    }

            //    using (var shdc = new ScopedHdc(this))
            //    {
            //        using (Graphics gfx = Graphics.FromHdc(shdc.DC))
            //        {
            //            Renderer.DrawColumnHeader(null, gfx, bounds);
            //        }
            //    }
            //}
            
            private void ListTreeViewMouseMove(object sender, MouseEventArgs e)
            {
                // Taken from MSDN for an issue with the ListView control
                // http://msdn.microsoft.com/en-us/library/system.windows.forms.listview.ownerdraw(v=VS.85).aspx

                ListViewItem lstItem = GetItemAt(e.X, e.Y);
                if (lstItem == null)
                    return;

                if (m_lstWorkaround.Contains(lstItem))
                    return;

                m_lstWorkaround.Add(lstItem);
                Invalidate(lstItem.Bounds);
            }

            private void ListTreeViewDrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
            {
                e.DrawDefault = true;
                //m_columnHeight = e.Bounds.Height;

                //ColumnHeader columnHeader = Columns[e.ColumnIndex];
                //Column column = columnHeader.Tag.As<Column>();

                //Renderer.DrawColumnHeader(column, e.Graphics, e.Bounds);
            }

            private void ListTreeViewDrawItem(object sender, DrawListViewItemEventArgs e)
            {
                // All drawing done via ListTreeViewDrawSubItem
            }

            private void ListTreeViewDrawSubItem(object sender, DrawListViewSubItemEventArgs e)
            {
                if (!m_painting)
                    return;

                int offset = 0;
                Node node = (Node)e.Item.Tag;
                bool isLastVisibleNode = (TheStyle != Style.VirtualList) && (e.ItemIndex == (Items.Count - 1));
                bool isLastColumn = (e.ColumnIndex == (Columns.Count - 1));

                {
                    Renderer.DrawBackground(node, e.Graphics, e.Bounds);

                    // draw background below the last item in case a custom skin is being used
                    if (isLastVisibleNode)
                    {
                        var rect =
                            new Rectangle(
                                e.Bounds.X,
                                e.Bounds.Y + e.Bounds.Height,
                                e.Bounds.Width,
                                Bounds.Bottom - e.Bounds.Bottom);

                        Renderer.DrawBackground(node, e.Graphics, rect);
                    }

                    if (GridLines)
                        DrawGridLines(e.Graphics, e.Bounds, GridLinesColor);

                    // special stuff when drawing last column... we draw past the column so that:
                    // 1) if the control gets disabled, then things don't look awful
                    // 2) if gridlines are enabled, then fake them out to infinity
                    if (isLastColumn)
                    {
                        var extraneousFauxNonClientRect =
                            new Rectangle(
                                e.Bounds.Right,
                                e.Bounds.Top,
                                Bounds.Right - e.Bounds.Right,
                                e.Bounds.Height);

                        Renderer.DrawBackground(node, e.Graphics, extraneousFauxNonClientRect);

                        if (isLastVisibleNode)
                        {
                            var rect =
                                new Rectangle(
                                    e.Bounds.Right,
                                    e.Bounds.Y + e.Bounds.Height,
                                    Bounds.Right - e.Bounds.Right,
                                    Bounds.Bottom - e.Bounds.Bottom);

                            Renderer.DrawBackground(node, e.Graphics, rect);
                        }

                        // continue grid lines out to 'infinity' horizontally
                        if (GridLines)
                        {
                            var extraneousFauxNonClientGridLinesRect = extraneousFauxNonClientRect;
                            extraneousFauxNonClientGridLinesRect.Inflate(1, 0);

                            DrawGridLines(e.Graphics, extraneousFauxNonClientGridLinesRect, GridLinesColor);
                        }
                    }
                }

                // Column 0 has some extras
                if (e.ColumnIndex == 0)
                {
                    {
                        // Limit area
                        Rectangle bounds =
                            new Rectangle(
                                e.Bounds.X,
                                e.Bounds.Y,
                                ExtraneousItemWidth,
                                e.Bounds.Height);

                        switch (m_style)
                        {
                            case Style.CheckedList:
                                Renderer.DrawCheckBox(node, e.Graphics, bounds);
                                node.CheckBoxHitRect = bounds;
                                offset += ExtraneousItemWidth;
                                break;

                            case Style.CheckedTreeList:
                                DrawExtraneousStuff(e, ref offset, ExpanderGradient, ExpanderPen, HierarchyLinePen);
                                offset += 2;
                                bounds.Offset(offset, 2);
                                Renderer.DrawCheckBox(node, e.Graphics, bounds);
                                node.CheckBoxHitRect = bounds;
                                offset += ExtraneousItemWidth;
                                break;

                            case Style.TreeList:
                                DrawExtraneousStuff(e, ref offset, ExpanderGradient, ExpanderPen, HierarchyLinePen);
                                break;

                            default:
                                offset += ExtraneousItemWidth;
                                break;
                        }
                    }

                    // Move over slightly and draw state image (if applicable)
                    {
                        if ((StateImageList != null) &&
                            (node.StateImageIndex != InvalidImageIndex))
                        {
                            Rectangle bounds =
                                new Rectangle(
                                    e.Bounds.X + offset,
                                    e.Bounds.Y,
                                    ExtraneousItemWidth,
                                    e.Bounds.Height);

                            Renderer.DrawStateImage(node, e.Graphics, bounds);
                            offset += ExtraneousItemWidth;
                        }
                    }

                    // Move over slightly and draw image
                    {
                        Rectangle bounds =
                            new Rectangle(
                                e.Bounds.X + offset,
                                e.Bounds.Y,
                                ExtraneousItemWidth,
                                e.Bounds.Height);

                        Renderer.DrawImage(node, e.Graphics, bounds);
                        offset += ExtraneousItemWidth;
                    }
                }

                // Calculate new bounds based on any offset by
                // collapser/expander image and/or hierarchy lines
                {
                    Rectangle labelBounds =
                        new Rectangle(
                            e.Bounds.X + offset,
                            e.Bounds.Y,
                            e.Bounds.Width - offset,
                            e.Bounds.Height);

                    Renderer.DrawLabel(node, e.Graphics, labelBounds, e.ColumnIndex);
                    if (e.ColumnIndex == 0)
                    {
                        node.LabelHitRect = labelBounds;
                    }
                }
            }

            private static void DrawExtraneousStuff(DrawListViewSubItemEventArgs e, ref int iOffset, ControlGradient expanderGradient, Pen expanderPen, Pen hierarchyLinePen)
            {
                if (e.Item.Tag == null)
                    return;

                // Not an item that requires expander/collapser
                // or hierarchy lines so bail
                if (!e.Item.Tag.Is<Node>())
                    return;

                // This node
                Node nodeThis = e.Item.Tag.As<Node>();

                // Early out for special 'root' level items
                if (nodeThis.IsLeaf && (nodeThis.Level == 1))
                {
                    iOffset += ExtraneousItemWidth;
                    return;
                }

                // Find out if there's a visible sibling node
                Node nodeSibling;
                FindSibling(nodeThis, out nodeSibling);
                bool bHasSibling = nodeSibling != null;

                // Find out if there's a visible child node
                Node nodeChild;
                FindChild(nodeThis, out nodeChild);
                bool bHasChild = nodeChild != null;

                //
                // Node levels start @ 1 not 0
                //

                // From left to right start drawing any hierarchy
                // lines and/or collapser/expander images
                for (int i = 0; i < nodeThis.Level; i++)
                {
                    //
                    // Set stuff up for the current iteration
                    //

                    Point posCur = new Point(e.Item.Bounds.X + iOffset, e.Item.Bounds.Y);
                    Size sizeCur = new Size(ExtraneousItemWidth, e.Item.Bounds.Height);
                    Rectangle rectBounds = new Rectangle(posCur, sizeCur);

                    //
                    // Try to do some drawing
                    //

                    // Try and find an expanded item above us (tells us
                    // what type of line we need to draw - if any)
                    Node nodeRelative;
                    FindExpandedRelativeAboveAtLevel(nodeThis, i + 1, out nodeRelative);

                    if (nodeRelative != null)
                    {
                        if (nodeRelative == nodeThis.Parent)
                            DrawElbow(e.Graphics, rectBounds, hierarchyLinePen, bHasSibling);
                    }

                    // Check for drawing vertical line(s)
                    {
                        bool bDrawVerticalLine = false;

                        // Find ancestor that's one level above the "current"
                        // (ie. i + 1) level and see if they have a sibling
                        Node nodeTemp;
                        FindExpandedRelativeAboveAtLevel(nodeThis, i + 2, out nodeTemp);

                        if (nodeTemp != null)
                        {
                            Node nodeTempSibling;
                            FindSibling(nodeTemp, out nodeTempSibling);

                            if (nodeTempSibling != null)
                                bDrawVerticalLine = true;
                        }

                        if (bDrawVerticalLine)
                            DrawVerticalLine(e.Graphics, rectBounds, hierarchyLinePen);
                    }

                    // Check if need an expander/collapser
                    bool bDrawExpanderCollapser =
                        (i == (nodeThis.Level - 1)) &&
                        (nodeThis.HasChildren || !nodeThis.IsLeaf);

                    // Draw expander/collapser
                    if (bDrawExpanderCollapser)
                    {
                        if (nodeThis.Expanded)
                            DrawCollapser(e.Graphics, rectBounds, expanderGradient, expanderPen, hierarchyLinePen, bHasChild);
                        else
                            DrawExpander(e.Graphics, rectBounds, expanderGradient, expanderPen, hierarchyLinePen);

                        // Set the are that can be clicked
                        nodeThis.HitRect = rectBounds;
                    }

                    // Check if horizontal line needed
                    bool bDrawHorizontalLine =
                        (i == (nodeThis.Level - 1)) &&
                        (i > 0) &&
                        !bDrawExpanderCollapser;

                    if (bDrawHorizontalLine)
                        DrawHorizontalLine(e.Graphics, rectBounds, hierarchyLinePen);

                    // Keep indenting
                    iOffset += ExtraneousItemWidth;
                }
            }

            private static void DrawExpanderButton(Graphics gfx, Rectangle bounds, ControlGradient expanderGradient, Pen hierarchyLinePen)
            {
                using (Brush brush = new LinearGradientBrush(
                   bounds,
                   expanderGradient.StartColor,
                   expanderGradient.EndColor,
                   expanderGradient.LinearGradientMode))
                {
                    gfx.FillRectangle(brush, bounds);
                    gfx.DrawRectangle(hierarchyLinePen, bounds);
                }
            }

            private static void DrawExpander(Graphics gfx, Rectangle bounds, ControlGradient expanderGradient, Pen expanderPen, Pen hierarchyLinePen)
            {
                const int iRectOffset = 5;
                const int iTwoTimesRectOffset = iRectOffset * 2;
                const int iLineOffset = 3;

                Point center =
                    new Point(
                        bounds.X + (bounds.Width / 2),
                        bounds.Y + (bounds.Height / 2));

                // Rectangle around the "+"
                DrawExpanderButton(
                    gfx,
                    new Rectangle(
                        center.X - iRectOffset,
                        center.Y - iRectOffset,
                        iTwoTimesRectOffset,
                        iTwoTimesRectOffset),
                    expanderGradient,
                    hierarchyLinePen);

                // Horizontal line of the "+"
                gfx.DrawLine(
                    expanderPen,
                    center.X - iLineOffset,
                    center.Y,
                    center.X + iLineOffset,
                    center.Y);

                // Vertical line of the "+"
                gfx.DrawLine(
                    expanderPen,
                    center.X,
                    center.Y - iLineOffset,
                    center.X,
                    center.Y + iLineOffset);
            }

            private static void DrawCollapser(Graphics gfx, Rectangle bounds, ControlGradient expanderGradient, Pen expanderPen, Pen hierarchyLinePen, bool bItemBelow)
            {
                const int iRectOffset = 5;
                const int iTwoTimesRectOffset = iRectOffset * 2;
                const int iLineOffset = 3;

                Point center =
                    new Point(
                        bounds.X + (bounds.Width / 2),
                        bounds.Y + (bounds.Height / 2));

                // Rectangle around the "-"
                DrawExpanderButton(
                    gfx,
                    new Rectangle(
                        center.X - iRectOffset,
                        center.Y - iRectOffset,
                        iTwoTimesRectOffset,
                        iTwoTimesRectOffset),
                    expanderGradient,
                    hierarchyLinePen);

                // Horizontal line of the "-"
                gfx.DrawLine(
                    expanderPen,
                    center.X - iLineOffset,
                    center.Y,
                    center.X + iLineOffset,
                    center.Y);

                if (!bItemBelow)
                    return;

                // Vertical line below the rectangle
                // that connects to the item below
                gfx.DrawLine(
                    hierarchyLinePen,
                    center.X,
                    center.Y + iRectOffset,
                    center.X,
                    bounds.Bottom);
            }

            private static void DrawVerticalLine(Graphics gfx, Rectangle bounds, Pen hierarchyLinePen)
            {
                Point center =
                    new Point(
                        bounds.X + (bounds.Width / 2),
                        bounds.Y + (bounds.Height / 2));

                // Vertical line
                gfx.DrawLine(
                    hierarchyLinePen,
                    center.X,
                    bounds.Top,
                    center.X,
                    bounds.Bottom);
            }

            private static void DrawElbow(Graphics gfx, Rectangle bounds, Pen hierarchyLinePen, bool bObjectBelow)
            {
                Point center =
                    new Point(
                        bounds.X + (bounds.Width / 2),
                        bounds.Y + (bounds.Height / 2));

                // Vertical part of elbow
                gfx.DrawLine(
                    hierarchyLinePen,
                    center.X,
                    bounds.Top,
                    center.X,
                    center.Y);

                // Horizontal part of elbow
                gfx.DrawLine(
                    hierarchyLinePen,
                    center.X,
                    center.Y,
                    bounds.Right,
                    center.Y);

                if (!bObjectBelow)
                    return;

                // Vertical line below the elbow
                // that connects to the item below
                gfx.DrawLine(
                    hierarchyLinePen,
                    center.X,
                    center.Y,
                    center.X,
                    bounds.Bottom);
            }

            private static void DrawHorizontalLine(Graphics gfx, Rectangle bounds, Pen hierarchyLinePen)
            {
                Point center =
                    new Point(
                        bounds.X + (bounds.Width / 2),
                        bounds.Y + (bounds.Height / 2));

                // Horizontal line
                gfx.DrawLine(
                    hierarchyLinePen,
                    bounds.Left,
                    center.Y,
                    bounds.Right,
                    center.Y);
            }

            private static void DrawGridLines(Graphics gfx, Rectangle bounds, Color color)
            {
                using (Pen p = new Pen(color))
                {
                    gfx.DrawRectangle(p, bounds);
                }
            }

            private static void FindSibling(Node node, out Node sibling)
            {
                sibling = null;

                if (node == null)
                    return;

                Node iter = node.Next;
                while (iter != null)
                {
                    if (iter.Visible)
                    {
                        sibling = iter;
                        return;
                    }

                    iter = iter.Next;
                }
            }

            private static void FindChild(Node node, out Node child)
            {
                child = null;

                if (node == null)
                    return;

                foreach (Node nodeTemp in node.Nodes)
                {
                    if (!nodeTemp.Visible)
                        continue;

                    child = nodeTemp;
                    return;
                }
            }

            private static void FindExpandedRelativeAboveAtLevel(Node node, int level, out Node relative)
            {
                // Level starts @ 1 not 0

                relative = null;

                if (node == null)
                    return;

                if (level <= 0)
                    return;

                Node iter = node.Parent;
                while (iter != null)
                {
                    // Don't need to check for iter.NodeExpanded == true
                    // since we wouldn't be here if an item above us wasn't
                    // expanded.

                    if (iter.Visible && (iter.Level == level))
                    {
                        relative = iter;
                        return;
                    }

                    iter = iter.Parent;
                }
            }

            //[DllImport("user32.dll")]
            //private static extern IntPtr GetDC(IntPtr hwnd);

            //[DllImport("user32.dll")]
            //private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

            //[DllImport("user32.dll", CharSet = CharSet.Auto)]
            //private static extern IntPtr SendMessage(IntPtr handle, int msg, int wparam, int lparam);

            //private static IntPtr GetHeaderControl(ListView list)
            //{
            //    const int LVM_GETHEADER = 0x1000 + 31;
            //    return SendMessage(list.Handle, LVM_GETHEADER, 0, 0);
            //}

            //private class ScopedHdc : IDisposable
            //{
            //    public ScopedHdc(ListView listView)
            //    {
            //        m_hwnd = GetHeaderControl(listView);
            //        DC = GetDC(m_hwnd);
            //    }

            //    public void Dispose()
            //    {
            //        ReleaseDC(m_hwnd, DC);

            //        m_hwnd = IntPtr.Zero;
            //        DC = IntPtr.Zero;
            //    }

            //    public IntPtr DC { get; private set; }

            //    private IntPtr m_hwnd;
            //}

            private bool m_painting;
            private bool m_gridLines;
            private NodeRenderer m_renderer;
            //private int m_columnHeight = 32;

            private Color m_textColor;
            private Color m_modifiableTextColor;
            private Color m_highlightTextColor;
            private Color m_modifiableHighlightTextColor;
            private Color m_disabledTextColor;
            private Color m_highlightBackColor;
            private Color m_disabledBackColor;
            private Color m_gridLinesColor;
            private ControlGradient m_expanderGradient;
            private Pen m_expanderPen;
            private Pen m_hierarchyLinePen;
            //private ControlGradient m_columnHeaderGradient;
            //private Color m_columnHeaderSeparatorColor;

            private readonly Style m_style;
            //private readonly Timer m_headerTimer;
            private readonly TreeListView m_owner;

            private readonly List<ListViewItem> m_lstWorkaround =
                new List<ListViewItem>();

            private const int ExtraneousItemWidth = 16;
            
            private const int SB_HORZ = 0;
            private const int SB_VERT = 1;
        }
        
        private class ListViewItemSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                if ((x == null) && (y == null))
                    return 0;

                if (x == null)
                    return 1;

                if (y == null)
                    return -1;

                if (ReferenceEquals(x, y))
                    return 0;

                ListViewItem lstItemX = (ListViewItem)x;
                ListViewItem lstItemY = (ListViewItem)y;

                Node lhs = (Node)lstItemX.Tag;
                Node rhs = (Node)lstItemY.Tag;

                return lhs.VisualPosition < rhs.VisualPosition ? -1 : 1;
            }
        }

        /// <summary>
        /// RetrieveVirtualNodeEventArgs class event arguments</summary>
        public class RetrieveVirtualNodeEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="index">Node index</param>
            public RetrieveVirtualNodeEventArgs(int index)
            {
                m_index = index;
            }

            /// <summary>
            /// Gets or sets the Node retrieved from the cache</summary>
            public Node Node { get; set; }

            /// <summary>
            /// Gets the index of the item to retrieve from the cache</summary>
            public int NodeIndex
            {
                get { return m_index; }
            }

            private readonly int m_index;
        }

        /// <summary>
        /// NodeCheckedEventArgs class event arguments</summary>
        public class NodeCheckedEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node</param>
            public NodeCheckedEventArgs(Node node)
            {
                Node = node;
            }

            /// <summary>
            /// Gets the Node that was checked or unchecked</summary>
            public Node Node { get; private set; }
        }

        /// <summary>
        /// NodeDragEventArgs class event arguments</summary>
        public class NodeDragEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="node">Node</param>
            /// <param name="button">Mouse buttons pressed</param>
            public NodeDragEventArgs(Node node, MouseButtons button)
            {
                Node = node;
                Button = button;
            }

            /// <summary>
            /// Gets the Node that is being dragged</summary>
            public Node Node { get; private set; }

            /// <summary>
            /// Gets which mouse buttons were pressed during the drag operation</summary>
            public MouseButtons Button { get; private set; }
        }
    }
}