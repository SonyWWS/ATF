//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Renders the items of a tree control and provides derived classes an opportunity to customize
    /// the appearance of items in the tree control. Is responsible for rendering such things as
    /// the label, check box, and expander. However, the TreeControl decides the positions.</summary>
    /// <remarks>
    /// The ITreeAdapter plays a small roll in the render style, too, because it provides "hints" about
    /// which font style to use that gets passed to this TreeItemRenderer via the TreeControl.Node's
    /// FontStyle property.
    /// 
    /// This class is intended to be used with only one TreeControl, because it caches the fonts
    /// (by default) according to the font style. It is possible to override that behavior.</remarks>
    public class TreeItemRenderer
    {
        /// <summary>
        /// Gets or sets the brush for the background of highlighted elements</summary>      
        public Brush HighlightBrush
        {
            get { return m_highlightBrush; }
            set { m_highlightBrush = value; }
        }

        /// <summary>
        /// Gets or sets the brush for the text of highlighted elements</summary>    
        public Brush HighlightTextBrush
        {
            get { return m_highlightTextBrush; }
            set { m_highlightTextBrush = value; }
        }

        /// <summary>
        /// Gets or sets the brush for the background of highlighted elements when the tree control does not have the input focus</summary>      
        public Brush DeactiveHighlightBrush
        {
            get { return m_deactiveHighlightBrush; }
            set { m_deactiveHighlightBrush = value; }
        }

        /// <summary>
        /// Gets or sets the brush for the text of highlighted elements when the tree control does not have the input focus</summary>   
        public Brush DeactiveHighlightTextBrush
        {
            get { return m_deactiveHighlightTextBrush; }
            set { m_deactiveHighlightTextBrush = value; }
        }

        /// <summary>
        /// Gets or sets the text brush for elements</summary>
        public Brush TextBrush
        {
            get { return m_textBrush; }
            set { m_textBrush = value; }
        }

        /// <summary>
        /// Gets or sets the pen for drawing the expander</summary>
        public Pen ExpanderPen
        {
            get { return m_expanderPen; }
            set { m_expanderPen = value; }
        }

        /// <summary>
        /// Gets or sets the pen for drawing hierarchy lines</summary>
        public Pen HierarchyLinePen
        {
            get { return m_hierarchyLinePen; }
            set { m_hierarchyLinePen = value; }
        }

        /// <summary>
        /// Gets or sets the starting color of a category expander icon</summary>
        public Color CategoryStartColor
        {
            get { return m_categoryStartColor; }
            set { m_categoryStartColor = value; }
        }

        /// <summary>
        /// Gets or sets the ending color of a category expander icon</summary>
        public Color CategoryEndColor
        {
            get { return m_categoryEndColor; }
            set { m_categoryEndColor = value; }
        }

        /// <summary>
        /// Node style used during filtering mode</summary>
        [Flags]
        public enum NodeFilteringStatus
        {
            /// <summary>
            /// Regular, non-filtered, plain display</summary>
            Normal = 0,

            /// <summary>
            /// Node is partially expanded (not all of its children are displayed)</summary>
            PartiallyExpanded = 0x01,

            /// <summary>
            /// Node should be visible if itself passes the filter, or has descendants that pass the filter</summary>
            Visible = 0x02,

            /// <summary>
            /// Any of node's direct children are matched</summary>
            ChildVisible = 0x04
        }

        /// <summary>
        /// Gets or sets the search (filtering) pattern</summary>
        public string FilteringPattern { get; set; }

        /// <summary>
        /// Callback function to get current node filtering status</summary>
        public Func<TreeControl.Node, NodeFilteringStatus> FilteringStatus;

        /// <summary>
        /// Measures the dimensions of the label in pixels. Must be in sync with DrawLabel.</summary>
        /// <param name="node">The tree node whose label is to be measured</param>
        /// <param name="g">The current GDI+ Graphics object</param>
        /// <returns>The width and height of a tight rectangle around the label in pixels. The
        /// TreeControl provides the paddding in between items. Technically, the units of measure
        /// are specified by Graphics.PageUnit.</returns>
        public virtual Size MeasureLabel(TreeControl.Node node, Graphics g)
        {
            Font font = GetDefaultFont(node, g);
            return Size.Ceiling(g.MeasureString(node.Label, font));
        }

        /// <summary>
        /// Measures the dimensions of text in pixels</summary>
        /// <param name="graphics">GDI+ Graphics object</param>
        /// <param name="text">Text to measure</param>
        /// <param name="font">Text font</param>
        /// <returns>The width and height of a tight rectangle around the text in pixels</returns>
        static SizeF MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            if (string.IsNullOrEmpty(text))
                return new SizeF(0.0f, 0.0f);

            StringFormat format = new StringFormat();
            RectangleF rect = new RectangleF(0, 0, 1600, 1600);
            CharacterRange[] ranges = { new CharacterRange(0, text.Length) };

            format.SetMeasurableCharacterRanges(ranges);

            Region[] regions = graphics.MeasureCharacterRanges(text, font, rect, format);
            rect = regions[0].GetBounds(graphics);
            rect.Width += 1.0f;

            return new SizeF(rect.Width, rect.Height);
        }

        /// <summary>
        /// Draws the text of a tree node's label at the specified location and the same size
        /// that MeasureLabel() would calculate for this node</summary>
        /// <param name="node">The tree control's node whose label is to be drawn</param>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the drawn text</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the drawn text</param>
        public virtual void DrawLabel(TreeControl.Node node, Graphics g, int x, int y)
        {
            Rectangle textRect = new Rectangle(x, y, node.LabelWidth, node.LabelHeight);
            Brush textBrush = m_textBrush;
            
            Font font = GetDefaultFont(node, g);

            if (!string.IsNullOrEmpty(FilteringPattern))
            {
                //                 bool matched = false;
                int regularStart = 0;
                int matchStart;
                PointF textLoc = new PointF(textRect.X, textRect.Y);

                do
                {
                    // highlight the backdground of matched text 
                    matchStart = node.Label.IndexOf(FilteringPattern, regularStart, StringComparison.CurrentCultureIgnoreCase);
                    if (matchStart >= 0)
                    {
                        //                        matched = true;
                        // non-matched substring 
                        string regularString = node.Label.Substring(regularStart, matchStart - regularStart);
                        SizeF regularSize = MeasureDisplayStringWidth(g, regularString, font);
                        textLoc.X += regularSize.Width;
                        regularStart = matchStart + FilteringPattern.Length; // advance string offset

                        // matched substring 
                        string matchedString = node.Label.Substring(matchStart, FilteringPattern.Length);
                        SizeF matchedSize = MeasureDisplayStringWidth(g, matchedString, font);
                        RectangleF matchedRect = new RectangleF(textLoc, matchedSize);

                        // offset a couple of pixels to avoid obvious overlap with preceding char
                        matchedRect.X += 2;
                        matchedRect.Width -= 2;
                        g.FillRectangle(m_brushMatchedHighLight, matchedRect);
                        textLoc.X += matchedSize.Width;
                    }
                } while (matchStart >= 0);
            }

            if (node.Selected)
            {
                Brush highlightBrush = Owner.ContainsFocus ? HighlightBrush : DeactiveHighlightBrush;
                Brush highlightTextBrush = Owner.ContainsFocus ? HighlightTextBrush : DeactiveHighlightTextBrush;

                g.FillRectangle(highlightBrush, textRect);
                textBrush = highlightTextBrush;
            }

            g.DrawString(node.Label, font, textBrush, textRect);
        }

        /// <summary>
        /// Draws the node background at the given location</summary>
        /// <param name="node">The node to be drawn</param>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the node</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the node</param>
        public virtual void DrawBackground(TreeControl.Node node, Graphics g, int x, int y)
        {
            if (NeedGrayBackground(node))
            {
                Rectangle bgRect = new Rectangle(Owner.Margin.Left, Owner.Margin.Top + y,
                    Owner.Width - Owner.Margin.Left - Owner.Margin.Right, node.LabelHeight + Owner.Margin.Top + Owner.Margin.Bottom);
                bgRect.Y -= 3;
                g.FillRectangle(m_brushNonMatchedBg, bgRect);
            }
        }

        /// <summary>
        /// Gets or sets dimensions of the check box in pixels</summary>
        public Size CheckBoxSize
        {
            get { return m_checkBoxSize; }
            set
            {
                if (m_checkBoxSize != value)
                {
                    m_checkBoxSize = value;
                    if (Owner != null)  //allow setting by derived class's constructor
                        Owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Draws the check box at the given location and of the size specified by CheckBoxSize</summary>
        /// <param name="node">The node that the check box is to be drawn for. The HasCheck property
        /// is assumed to be true.</param>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the check box</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the check box</param>
        public virtual void DrawCheckBox(TreeControl.Node node, Graphics g, int x, int y)
        {
            ButtonState buttonState = ButtonState.Flat;
            if (node.CheckState == CheckState.Checked)
                buttonState |= ButtonState.Checked;
            else if (node.CheckState == CheckState.Indeterminate)
                buttonState |= ButtonState.Inactive;
            Rectangle bounds = new Rectangle(x, y, CheckBoxSize.Width, CheckBoxSize.Height);
            ControlPaint.DrawCheckBox(g, bounds, buttonState);
        }

        /// <summary>
        /// Gets or sets the dimensions of the expander icon in pixels</summary>
        public Size ExpanderSize
        {
            get { return m_expanderSize; }
            set
            {
                if (m_expanderSize != value)
                {
                    m_expanderSize = value;
                    if (Owner != null)  //allow setting by derived class's constructor
                        Owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Draws the expander icon at the given location, in the state given by
        /// the node's Expanded property and the size specified by ExpanderSize</summary>
        /// <param name="node">The node to draw an expander icon for</param>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the expander icon</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the expander icon</param>
        public virtual void DrawExpander(TreeControl.Node node, Graphics g, int x, int y)
        {
            node.PartiallyExpanded = false; // reset            
            GdiUtil.DrawExpander(x, y, ExpanderSize.Height, m_expanderPen, node.Expanded, g);
        }


        /// <summary>
        /// Draws the category expander icon for the Microsoft Office-like categorized palette</summary>
        /// <param name="node">The node to draw a category expander icon for</param>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <param name="r">The bounds of the category expander icon</param>
        public virtual void DrawCategory(TreeControl.Node node, Graphics g, Rectangle r)
        {
            var color1 = (CategoryStartColor.A == 0) ?
                ColorUtil.GetShade(Owner.BackColor, 0.97f) :
                CategoryStartColor;

            var color2 = (CategoryEndColor.A == 0) ?
                ColorUtil.GetShade(CategoryStartColor, 0.9f) :
                CategoryEndColor;

            using (LinearGradientBrush brush =
                new LinearGradientBrush(r, color1, color2, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, r);
            }

            Padding margin = Owner.Margin;
            int xPadding = margin.Left;
            int yPadding = margin.Top;

            GdiUtil.DrawOfficeExpander(
                r.Width - ExpanderSize.Width - xPadding,
                r.Y + yPadding,
                ExpanderPen,
                !node.Expanded,
                g);
        }

        /// <summary>
        /// Draws hierarchy lines for tree control styles that visually show parent-child relationships with
        /// vertical and horizontal lines</summary>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <param name="a">The desired starting point of the line, in pixels</param>
        /// <param name="b">The desired ending point of the line, in pixels</param>
        public virtual void DrawHierarchyLine(Graphics g, Point a, Point b)
        {
            g.DrawLine(HierarchyLinePen, a.X, a.Y, b.X, b.Y);
        }

        /// <summary>
        /// Draws an image</summary>
        /// <param name="imageList">Image list to use</param>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <param name="x">The x-coordinate of the upper-left corner of the image</param>
        /// <param name="y">The y-coordinate of the upper-left corner of the image</param>
        /// <param name="index">The image's index in the image list</param>
        public virtual void DrawImage(ImageList imageList, Graphics g, int x, int y, int index)
        {
            if ((Owner == null) || (Owner.Enabled))
            {
                using (var image = imageList.Images[index])
                    g.DrawImage(image, x, y);
            }
            else
            {
                using (var image = imageList.Images[index])
                    ControlPaint.DrawImageDisabled(g, image, x, y, Owner.BackColor);
            }
        }

        /// <summary>
        /// Gets the Control used to determine such things as the default font and the background color
        /// and whether or not the control has focus (which affects the appearance of currently selected
        /// items). Typically, Owner is the owning TreeControl. Is set by the TreeControl.</summary>
        public Control Owner
        {
            get { return m_owner; }
            internal set
            {
                m_owner = value;
                m_fonts.Clear();
            }
        }

        /// <summary>
        /// Gets the default font appropriate for this node. Is cached for better performance.
        /// This is a convenience method for MeasureLabel and DrawLabel, who are free to choose one or
        /// more fonts for a particular node any way they please.</summary>
        /// <param name="node">The node that this font is for, e.g., by checking the FontStyle property</param>
        /// <param name="g">The current GDI+ graphics object</param>
        /// <returns>A font appropriate for rendering the node's label, for example</returns>
        protected virtual Font GetDefaultFont(TreeControl.Node node, Graphics g)
        {
            Font font = Owner.Font;
            if (node.FontStyle != FontStyle.Regular)
            {
                int index = (int)node.FontStyle;
                if (!m_fonts.TryGetValue(index, out font))
                {
                    font = new Font(Owner.Font, node.FontStyle);
                    m_fonts.Add(index, font);
                }
            }

            return font;
        }

        // return false if the node itself, or any of its chilldren label matches the searching pattern
        private bool NeedGrayBackground(TreeControl.Node node)
        {
            if (string.IsNullOrEmpty(FilteringPattern))
                return false;

            return FilteringStatus == null || (FilteringStatus(node) & NodeFilteringStatus.Visible) == 0;
        }

        private Control m_owner;
        private readonly Dictionary<int, Font> m_fonts = new Dictionary<int, Font>();
        private Size m_checkBoxSize = new Size(16, 16);
        private Size m_expanderSize = new Size(GdiUtil.ExpanderSize,GdiUtil.ExpanderSize);

        private Brush m_highlightBrush = SystemBrushes.Highlight;
        private Brush m_highlightTextBrush = SystemBrushes.HighlightText;
        private Brush m_deactiveHighlightBrush = SystemBrushes.Control;
        private Brush m_deactiveHighlightTextBrush = SystemBrushes.WindowText;

        private Brush m_textBrush = SystemBrushes.WindowText;
        private Pen m_expanderPen = SystemPens.ControlDarkDark;
        private Pen m_hierarchyLinePen = SystemPens.InactiveBorder;

        private SolidBrush m_brushMatchedHighLight = new SolidBrush(Color.FromArgb(239, 203, 5));
        private SolidBrush m_brushNonMatchedBg = new SolidBrush(Color.FromArgb(230, 230, 230));
        private SolidBrush m_brushPartialExpander = new SolidBrush(Color.FromArgb(180, 180, 180));

        private Color m_categoryStartColor = Color.FromArgb(0, 0, 0, 0);
        private Color m_categoryEndColor = Color.FromArgb(0, 0, 0, 0);
    }
    
    // test case to have wildly colorful alternating letters of the label and extra-big expanders.
    [Obsolete("Is a temporary example. Will be removed or relocated.")]
    public class ChristmasTreeRenderer : TreeItemRenderer
    {
        public ChristmasTreeRenderer()
        {
            //ExpanderSize = new Size(16,32); //huge! drammatic!
            ExpanderSize = new Size(16, 8); //wide and short
            CheckBoxSize = new Size(40, 32);
        }
        public override Size MeasureLabel(TreeControl.Node node, Graphics g)
        {
            Size result = base.MeasureLabel(node, g);
            result.Width += node.Label.Length; //throw in an extra pixel per char
            return result;
        }
        public override void DrawLabel(TreeControl.Node node, Graphics g, int x, int y)
        {
            RectangleF textRect = new RectangleF(x, y, node.LabelWidth, node.LabelHeight);
            Brush textBrush;
            Font font = GetDefaultFont(node, g);

            if (node.Selected)
            {
                Brush highlightBrush = SystemBrushes.Highlight;
                if (!Owner.ContainsFocus)
                {
                    highlightBrush = SystemBrushes.ButtonHighlight;
                }

                g.FillRectangle(highlightBrush, textRect);
            }

            // Compute the rectangles around each character of the label.
            CharacterRange[] ranges = new CharacterRange[node.Label.Length];
            for (int i = 0; i < node.Label.Length; i++)
                ranges[i] = new CharacterRange(i, 1);
            StringFormat format = new StringFormat();
            format.Trimming = StringTrimming.None;
            format.FormatFlags |= StringFormatFlags.NoClip;
            format.SetMeasurableCharacterRanges(ranges);
            Region[] regions = g.MeasureCharacterRanges(node.Label, font, textRect, format);

            // Draw each character, cycling through the colored pens.
            int currColorIndex = 0;
            for (int i = 0; i < node.Label.Length; i++)
            {
                RectangleF oneCharRect = regions[i].GetBounds(g);
                oneCharRect.Width += 1;
                oneCharRect.X += i; //throw in an extra pixel per char to prevent overlap
                string oneLetter = new string(node.Label[i], 1);

                textBrush = s_coloredBrushes[currColorIndex++];
                if (currColorIndex == s_coloredBrushes.Length)
                    currColorIndex = 0;

                g.DrawString(oneLetter, font, textBrush, oneCharRect, format);
            }
        }
        public override void DrawExpander(TreeControl.Node node, Graphics g, int x, int y)
        {
            g.DrawRectangle(s_expanderPen, x, y, ExpanderSize.Width, ExpanderSize.Height);

            Size lineLength = new Size(ExpanderSize.Width - 2, ExpanderSize.Height - 2);
            Point center = new Point(ExpanderSize.Width / 2, ExpanderSize.Height / 2);
            g.DrawLine(
                s_expanderPen,
                x + 1,
                y + center.Y,
                x + 1 + lineLength.Width,
                y + center.Y);
            if (!node.Expanded)
                g.DrawLine(
                    s_expanderPen,
                    x + center.X,
                    y + 1,
                    x + center.X,
                    y + 1 + lineLength.Height);
        }
        public override void DrawHierarchyLine(Graphics g, Point a, Point b)
        {
            g.DrawLine(s_hierarchyPen, a.X, a.Y, b.X, b.Y);
        }
        private static readonly Brush[] s_coloredBrushes = new[]
                {
                    Brushes.Red,
                    Brushes.Green,
                    Brushes.Blue,
                };
        private static readonly Pen s_hierarchyPen = Pens.Green;
        private static readonly Pen s_expanderPen = Pens.Red;
    }
}
