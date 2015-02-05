//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// OBSOLETE. Please use D2dCircuitRenderer instead.
    /// Graph renderer that draws graph nodes as circuit elements, and edges as wires.
    /// Elements have 0 or more output pins, where wires originate, and 0 or more input
    /// pins, where wires end. Output pins are on the right and input pins are on the left
    /// side of elements.</summary>
    /// <typeparam name="TElement">Element type, must implement IElementType, and IGraphNode</typeparam>
    /// <typeparam name="TWire">Wire type, must implement IGraphEdge</typeparam>
    /// <typeparam name="TPin">Pin type, must implement ICircuitPin, IEdgeRoute</typeparam>
    public class CircuitRenderer<TElement, TWire, TPin> : GraphRenderer<TElement, TWire, TPin>, IDisposable
        where TElement : class, ICircuitElement
        where TWire : class, IGraphEdge<TElement, TPin>
        where TPin : class, ICircuitPin
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Diagram theme for rendering graph</param>
        public CircuitRenderer(DiagramTheme theme)
        {
            Theme = theme;

            SetPinSpacing();
        }

        /// <summary>
        /// Disposes resources</summary>
        /// <param name="disposing">If true, then Dispose() called this method and managed resources should
        /// be released in addition to unmanaged resources. If false, then the finalizer called this method
        /// and no managed objects should be called and only unmanaged resources should be released.</param>
        protected override void Dispose(bool disposing)
        {
            DisposeElementInfo();
            Theme = null;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets or sets the diagram theme</summary>
        public DiagramTheme Theme
        {
            get { return m_theme; }
            set
            {
                if (m_theme != value)
                {
                    if (m_theme != null)
                        m_theme.Redraw -= theme_Redraw;

                    m_theme = value;

                    if (m_theme != null)
                        m_theme.Redraw += theme_Redraw;

                    base.OnRedraw();
                }
            }
        }

        /// <summary>
        /// Invalidates cached info for element type name</summary>
        /// <param name="elementType">Element type to invalidate</param>
        public void Invalidate(ICircuitElementType elementType)
        {
            m_elementTypeCache.Remove(elementType);
            OnRedraw();
        }

        /// <summary>
        /// Draws a graph node</summary>
        /// <param name="element">Element to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(TElement element, DiagramDrawingStyle style, Graphics g)
        {
            switch (style)
            {
                case DiagramDrawingStyle.Normal:
                    Draw(element, g);
                    break;
                case DiagramDrawingStyle.Selected:
                case DiagramDrawingStyle.LastSelected:
                case DiagramDrawingStyle.Hot:
                default:
                    Draw(element, g);
                    DrawOutline(element, m_theme.GetPen(style), g);
                    break;
                case DiagramDrawingStyle.Ghosted:
                case DiagramDrawingStyle.Hidden:
                    DrawGhost(element, g);
                    break;
            }
        }

        /// <summary>
        /// Draws a graph edge</summary>
        /// <param name="edge">Edge to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(TWire edge, DiagramDrawingStyle style, Graphics g)
        {
            TPin inputPin = edge.ToRoute;
            TPin outputPin = edge.FromRoute;
            Pen pen = (style == DiagramDrawingStyle.Normal) ? GetPen(inputPin) : m_theme.GetPen(style);
            DrawWire(edge.FromNode, outputPin, edge.ToNode, inputPin, g, pen);
        }

        /// <summary>
        /// Draws a partially defined graph edge</summary>
        /// <param name="outputElement">Source element, or null</param>
        /// <param name="outputPin">Source pin, or null</param>
        /// <param name="inputElement">Destination element, or null</param>
        /// <param name="inputPin">Destination pin, or null</param>
        /// <param name="label">Edge label</param>
        /// <param name="endPoint">Endpoint to substitute for source or destination, if null</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(
            TElement outputElement,
            TPin outputPin,
            TElement inputElement,
            TPin inputPin,
            string label,
            Point endPoint,
            Graphics g)
        {
            if (inputElement == null)
            {
                // dragging from output to endPoint
                DrawWire(outputElement, outputPin, endPoint, true, g);
            }
            else if (outputElement == null)
            {
                // dragging from input to endPoint
                DrawWire(inputElement, inputPin, endPoint, false, g);
            }
            else
            {
                // routed edge
                Pen pen = GetPen(outputPin);
                DrawWire(outputElement, outputPin, inputElement, inputPin, g, pen);
            }
        }

        /// <summary>
        /// Gets the bounding rect of a circuit element</summary>
        /// <param name="element">Element to get the bounds for.</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Rectangle completely bounding the node</returns>
        public override Rectangle GetBounds(TElement element, Graphics g)
        {
            Rectangle result = GetElementBounds(element, g);
            result.Height += PinMargin + m_rowSpacing; // label at bottom
            return GdiUtil.Transform(g.Transform, result);
        }

        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public override GraphHitRecord<TElement, TWire, TPin> Pick(
            IGraph<TElement, TWire, TPin> graph, TWire priorityEdge, Point p, Graphics g)
        {
            TElement pickedElement = null;
            TPin pickedInput = null;
            TPin pickedOutput = null;
            TWire pickedWire = null;

            if (priorityEdge != null &&
                PickEdge(priorityEdge, p, g))
            {
                pickedWire = priorityEdge;
            }
            else
            {
                foreach (TWire edge in graph.Edges)
                {
                    if (PickEdge(edge, p, g))
                    {
                        pickedWire = edge;
                        break;
                    }
                }
            }

            foreach (TElement element in graph.Nodes.Reverse())
            {
                if (Pick(element, g, p))
                {
                    pickedElement = element;
                    pickedInput = PickInput(element, g, p);
                    pickedOutput = PickOutput(element, g, p);
                    break;
                }
            }

            if (pickedElement != null && pickedWire == null)
            {
                Rectangle bounds = GetElementBounds(pickedElement, g);
                Rectangle labelBounds = new Rectangle(
                    bounds.Left, bounds.Bottom + PinMargin, bounds.Width, m_rowSpacing);
                labelBounds = GdiUtil.Transform(g.Transform, labelBounds);
                if (labelBounds.Contains(p))
                {
                    DiagramLabel label = new DiagramLabel(
                        labelBounds,
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.HorizontalCenter);

                    return new GraphHitRecord<TElement, TWire, TPin>(pickedElement, label);
                }
            }

            return new GraphHitRecord<TElement, TWire, TPin>(pickedElement, pickedWire, pickedOutput, pickedInput);
        }

        private bool PickEdge(TWire edge, Point p, Graphics g)
        {
            ElementTypeInfo fromInfo = GetElementTypeInfo(edge.FromNode, g);
            TPin inputPin = edge.ToRoute;
            TPin outputPin = edge.FromRoute;

            Point p1 = edge.FromNode.Bounds.Location;
            int x1 = p1.X + fromInfo.Size.Width;
            int y1 = p1.Y + GetPinOffset(outputPin.Index);

            Point p2 = edge.ToNode.Bounds.Location;
            int x2 = p2.X;
            int y2 = p2.Y + GetPinOffset(inputPin.Index);

            int tanLen = GetTangentLength(x1, x2);

            BezierCurve2F curve = new BezierCurve2F(
                new Vec2F(x1, y1),
                new Vec2F(x1 + tanLen, y1),
                new Vec2F(x2 - tanLen, y2),
                new Vec2F(x2, y2));

            Vec2F hitPoint = new Vec2F();
            return BezierCurve2F.Pick(curve, new Vec2F(p.X, p.Y), m_theme.PickTolerance, ref hitPoint);
        }

        /// <summary>
        /// Raises the Redraw event</summary>
        protected override void OnRedraw()
        {
            DisposeElementInfo();

            m_elementTypeCache.Clear(); // invalidate cached info

            base.OnRedraw();
        }

        private void theme_Redraw(object sender, EventArgs e)
        {
            OnRedraw();
        }

        private void Draw(TElement element, Graphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            Point p = element.Bounds.Location;
            Rectangle bounds = new Rectangle(p, info.Size);

            // clip to window
            if (g.ClipBounds.IntersectsWith(bounds))
            {
                ICircuitElementType type = element.Type;

                if (info.Path == null)
                    BuildGraphics(type, info, g);

                s_pathTransform.Translate(p.X, p.Y);
                info.Path.Transform(s_pathTransform);

                // try to use custom brush if registered
                Brush fillBrush = m_theme.GetCustomBrush(type.Name);
                if (fillBrush != null)
                {
                    g.FillPath(fillBrush, info.Path);
                }
                else
                {
                    // use a default brush
                    using (LinearGradientBrush lgb = new LinearGradientBrush(
                        bounds,
                        Color.White,
                        Color.LightSteelBlue,
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(lgb, info.Path);
                    }
                }
                g.DrawPath(m_theme.OutlinePen, info.Path);

                int titleHeight = m_rowSpacing + PinMargin;
                g.DrawLine(m_theme.OutlinePen, p.X, p.Y + titleHeight, p.X + info.Size.Width, p.Y + titleHeight);
                g.DrawString(type.Name, m_theme.Font, m_theme.TextBrush, p.X + PinMargin + 1, p.Y + PinMargin + 1);

                int pinY = p.Y + titleHeight + PinMargin;
                foreach (TPin inputPin in type.Inputs)
                {
                    Pen pen = GetPen(inputPin);
                    if (pen != null)
                        g.DrawRectangle(pen, p.X + 1, pinY + m_pinOffset, m_pinSize, m_pinSize);
                    g.DrawString(inputPin.Name, m_theme.Font, m_theme.TextBrush, p.X + m_pinSize + PinMargin, pinY);
                    pinY += m_rowSpacing;
                }

                pinY = p.Y + titleHeight + PinMargin;
                int i = 0;
                foreach (TPin outputPin in type.Outputs)
                {
                    Pen pen = GetPen(outputPin);
                    if (pen != null)
                        g.DrawRectangle(pen, p.X + info.Size.Width - m_pinSize, pinY + m_pinOffset, m_pinSize, m_pinSize);
                    g.DrawString(outputPin.Name, m_theme.Font, m_theme.TextBrush, p.X + info.OutputLeftX[i], pinY);
                    pinY += m_rowSpacing;
                    i++;
                }

                Image image = type.Image;
                if (image != null)
                    g.DrawImage(image, p.X + info.Interior.X, p.Y + info.Interior.Y, info.Interior.Width, info.Interior.Height);

                s_pathTransform.Translate(-2 * p.X, -2 * p.Y);
                info.Path.Transform(s_pathTransform);
                s_pathTransform.Reset();

                string name = element.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    RectangleF alignRect = new RectangleF(
                        bounds.Left - MaxNameOverhang, bounds.Bottom + PinMargin, bounds.Width + 2 * MaxNameOverhang, m_rowSpacing);
                    g.DrawString(name, m_theme.Font, m_theme.TextBrush, alignRect, m_theme.CenterStringFormat);
                }
            }
        }

        private void DrawGhost(TElement element, Graphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            Point p = element.Bounds.Location;
            Rectangle bounds = new Rectangle(p, info.Size);

            // clip to window to speed up drawing when zoomed in
            if (g.ClipBounds.IntersectsWith(bounds))
            {
                if (info.Path == null)
                {
                    ICircuitElementType type = element.Type;
                    BuildGraphics(type, info, g);
                }

                s_pathTransform.Translate(p.X, p.Y);
                info.Path.Transform(s_pathTransform);

                g.FillPath(m_theme.GhostBrush, info.Path);
                g.DrawPath(m_theme.GhostPen, info.Path);

                s_pathTransform.Translate(-2 * p.X, -2 * p.Y);
                info.Path.Transform(s_pathTransform);
                s_pathTransform.Reset();
            }
        }

        private void DrawOutline(TElement element, Pen pen, Graphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            if (info.Path == null)
            {
                ICircuitElementType type = element.Type;
                BuildGraphics(type, info, g);
            }

            Point p = element.Bounds.Location;
            s_pathTransform.Translate(p.X, p.Y);
            info.Path.Transform(s_pathTransform);

            g.DrawPath(pen, info.Path);

            s_pathTransform.Translate(2 * -p.X, 2 * -p.Y);
            info.Path.Transform(s_pathTransform);
            s_pathTransform.Reset();
        }

        private bool Pick(TElement element, Graphics g, Point p)
        {
            Rectangle bounds = GetBounds(element, g);
            int pickTolerance = m_theme.PickTolerance;
            bounds.Inflate(pickTolerance, pickTolerance);
            return bounds.Contains(p.X, p.Y);
        }

        /// <summary>
        /// Find input pin, if any, at given point</summary>
        /// <param name="element">Element</param>
        /// <param name="g">Graphics object</param>
        /// <param name="p">Point to test</param>
        /// <returns>Input pin hit by p; null otherwise</returns>
        private TPin PickInput(TElement element, Graphics g, Point p)
        {
            Point ep = element.Bounds.Location;
            int x = ep.X;
            int y = ep.Y + m_rowSpacing + 2 * PinMargin + m_pinOffset;
            int width = m_pinSize;
            //if (IsDraggingWire)
            //{
            //    ElementTypeInfo info = GetElementTypeInfo(element, g);
            //    width = info.Width / 2 - m_pickTolerance;
            //}

            Rectangle bounds = new Rectangle(x, y, width, m_pinSize);
            int pickTolerance = m_theme.PickTolerance;
            bounds.Inflate(pickTolerance, pickTolerance);
            ICircuitElementType type = element.Type;
            foreach (TPin input in type.Inputs)
            {
                if (bounds.Contains(p.X, p.Y))
                    return input;
                bounds.Y += m_rowSpacing;
            }
            return null;
        }

        /// <summary>
        /// Find output pin, if any, at given point</summary>
        /// <param name="element">Element</param>
        /// <param name="g">Graphics object</param>
        /// <param name="p">Point to test</param>
        /// <returns>Output pin hit by p; null otherwise</returns>
        private TPin PickOutput(TElement element, Graphics g, Point p)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);

            Point ep = element.Bounds.Location;
            int x = ep.X + info.Size.Width - m_pinSize;
            int y = ep.Y + m_rowSpacing + 2 * PinMargin + m_pinOffset;
            int width = m_pinSize;

            Rectangle bounds = new Rectangle(x, y, width, m_pinSize);
            int pickTolerance = m_theme.PickTolerance;
            bounds.Inflate(pickTolerance, pickTolerance);
            ICircuitElementType type = element.Type;
            foreach (TPin output in type.Outputs)
            {
                if (bounds.Contains(p.X, p.Y))
                    return output;
                bounds.Y += m_rowSpacing;
            }
            return null;
        }

        private ElementTypeInfo GetElementTypeInfo(TElement element, Graphics g)
        {
            // look it up in the cache
            ICircuitElementType type = element.Type;
            ElementTypeInfo cachedInfo;
            if (m_elementTypeCache.TryGetValue(type, out cachedInfo))
                return cachedInfo;

            // not in cache, recompute
            ElementSizeInfo sizeInfo = GetElementSizeInfo(type, g);
            ElementTypeInfo info = new ElementTypeInfo
            {
                Size = sizeInfo.Size,
                Interior = sizeInfo.Interior,
                OutputLeftX = sizeInfo.OutputLeftX.ToArray()
            };

            m_elementTypeCache.Add(type, info);

            return info;
        }

        /// <summary>
        /// Computes interior and exterior size as well as the X positions of output pins</summary>
        /// <param name="type">Circuit element type</param>
        /// <param name="g">Graphics that can be used for measuring strings</param>
        /// <returns>Element size info, must not be null</returns>
        /// <remarks>Clients using customized rendering should override this method
        /// to adjust sizes accordingly. These sizes will be used by drag-fram picking.</remarks>
        protected virtual ElementSizeInfo GetElementSizeInfo(ICircuitElementType type, Graphics g)
        {
            SizeF typeNameSize = g.MeasureString(type.Name, m_theme.Font);
            int width = (int)typeNameSize.Width + 2 * PinMargin;

            IList<ICircuitPin> inputPins = type.Inputs;
            IList<ICircuitPin> outputPins = type.Outputs;
            int inputCount = inputPins.Count;
            int outputCount = outputPins.Count;
            int minRows = Math.Min(inputCount, outputCount);
            int maxRows = Math.Max(inputCount, outputCount);

            int[] outputLeftX = new int[outputCount];

            int height = m_rowSpacing + 2 * PinMargin;
            height += Math.Max(
                maxRows * m_rowSpacing,
                minRows * m_rowSpacing + type.InteriorSize.Height - PinMargin);

            bool imageRight = true;
            for (int i = 0; i < maxRows; i++)
            {
                double rowWidth = 2 * PinMargin;
                if (inputCount > i)
                {
                    SizeF labelSize = g.MeasureString(inputPins[i].Name, m_theme.Font);
                    rowWidth += labelSize.Width + m_pinSize + PinMargin;
                }
                else
                {
                    rowWidth += type.InteriorSize.Width;
                    imageRight = false;
                }
                if (outputCount > i)
                {
                    SizeF labelSize = g.MeasureString(outputPins[i].Name, m_theme.Font);
                    outputLeftX[i] = (int)labelSize.Width;
                    rowWidth += labelSize.Width + m_pinSize + PinMargin;
                }
                else
                {
                    rowWidth += type.InteriorSize.Width;
                }
                width = Math.Max(width, (int)rowWidth);
            }

            if (inputCount == outputCount)
                width = Math.Max(width, type.InteriorSize.Width + 2);

            width = Math.Max(width, MinElementWidth);
            height = Math.Max(height, MinElementHeight);

            Size size = new Size(width, height);
            Rectangle interior = new Rectangle(
                imageRight ? width - type.InteriorSize.Width : 1,
                height - type.InteriorSize.Height,
                type.InteriorSize.Width,
                type.InteriorSize.Height);

            for (int i = 0; i < outputLeftX.Length; i++)
                outputLeftX[i] = width - PinMargin - m_pinSize - outputLeftX[i];

            return new ElementSizeInfo(size, interior, outputLeftX);
        }

        private void DisposeElementInfo()
        {
            foreach (ElementTypeInfo info in m_elementTypeCache.Values)
            {
                if (info.Path != null)
                    info.Path.Dispose();
            }
        }

        private void BuildGraphics(ICircuitElementType elementType, ElementTypeInfo info, Graphics g)
        {
            int width = info.Size.Width;
            int height = info.Size.Height;

            // create rounded corner rect
            GraphicsPath gp = new GraphicsPath();
            const float r = 6;
            const float d = 2 * r;
            gp.AddLine(r, 0, width - d, 0);
            gp.AddArc(width - d, 0, d, d, 270, 90);
            gp.AddLine(width, r, width, height - d);
            gp.AddArc(width - d, height - d, d, d, 0, 90);
            gp.AddLine(width - d, height, r, height);
            gp.AddArc(0, height - d, d, d, 90, 90);
            gp.AddLine(0, height - d, 0, r);
            gp.AddArc(0, 0, d, d, 180, 90);

            info.Path = gp;
        }

        private void DrawWire(
            TElement outputElement,
            TPin outputPin,
            TElement inputElement,
            TPin inputPin,
            Graphics g,
            Pen pen)
        {
            ElementTypeInfo info = GetElementTypeInfo(outputElement, g);

            Point op = outputElement.Bounds.Location;
            int x1 = op.X + info.Size.Width;
            int y1 = op.Y + GetPinOffset(outputPin.Index);
            Point ip = inputElement.Bounds.Location;
            int x2 = ip.X;
            int y2 = ip.Y + GetPinOffset(inputPin.Index);

            DrawWire(g, pen, x1, y1, x2, y2);
        }

        private void DrawWire(
            TElement element,
            TPin pin,
            Point p,
            bool fromOutput,
            Graphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);

            Point ep = element.Bounds.Location;
            int x = ep.X;
            int y = ep.Y + GetPinOffset(pin.Index);
            if (fromOutput)
                x += info.Size.Width;

            Matrix inverse = g.Transform;
            inverse.Invert();
            Point end = GdiUtil.Transform(inverse, p);

            Pen pen = GetPen(pin);
            if (fromOutput)
                DrawWire(g, pen, x, y, end.X, end.Y);
            else
                DrawWire(g, pen, end.X, end.Y, x, y);
        }

        private void DrawWire(Graphics g, Pen pen, int x1, int y1, int x2, int y2)
        {
            int tanLen = GetTangentLength(x1, x2);
            try
            {
                g.DrawBezier(
                    pen,
                    new Point(x1, y1),
                    new Point(x1 + tanLen, y1),
                    new Point(x2 - tanLen, y2),
                    new Point(x2, y2));
            }
            catch (System.OutOfMemoryException ex)
            {
                // let it go, only missing some pixels
                // see http://msdn2.microsoft.com/en-us/library/Graphics.drawarc.aspx
                Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
            }
        }

        private int GetTangentLength(int x1, int x2)
        {
            const int minTanLen = 32;
            int tanLen = Math.Abs(x1 - x2) / 2;
            tanLen = Math.Max(tanLen, minTanLen);
            return tanLen;
        }

        private Pen GetPen(TPin pin)
        {
            Pen pen = m_theme.GetCustomPen(pin.TypeName);
            if (pen == null)
                pen = m_theme.GhostPen;

            return pen;
        }

        private int GetPinOffset(int index)
        {
            return m_rowSpacing + 2 * PinMargin + index * m_rowSpacing + m_pinOffset + m_pinSize / 2;
        }

        private void SetPinSpacing()
        {
            int fontHeight = m_theme.Font.Height;
            m_rowSpacing = fontHeight + PinMargin;
            m_pinOffset = (fontHeight - m_pinSize) / 2;
        }

        private Rectangle GetElementBounds(TElement element, Graphics g)
        {
            ElementTypeInfo info = GetElementTypeInfo(element, g);
            return new Rectangle(element.Bounds.Location, info.Size);
        }

        #region Private Classes

        /// <summary>
        /// Class to hold cached element type layout</summary>
        private class ElementTypeInfo
        {
            public Size Size;
            public Rectangle Interior;
            public int[] OutputLeftX;
            public GraphicsPath Path; // built by DrawElement
        }

        /// <summary>
        /// Size info for a CircuitElement Type</summary>
        public class ElementSizeInfo
        {
            /// <summary>
            /// Constructor with parameters</summary>
            /// <param name="size">Size</param>
            /// <param name="interior">Interior size</param>
            /// <param name="outputLeftX">Horizontal offset of output pins in pixels</param>
            public ElementSizeInfo(Size size, Rectangle interior, IEnumerable<int> outputLeftX)
            {
                m_size = size;
                m_interior = interior;
                m_outputLeftX = outputLeftX;
            }

            /// <summary>
            /// Gets the size in pixels</summary>
            public Size Size { get { return m_size; } }

            /// <summary>
            /// Gets the interior rectangle</summary>
            public Rectangle Interior { get { return m_interior; } }

            /// <summary>
            /// Gets the horizontal offset of output pins in pixels</summary>
            public IEnumerable<int> OutputLeftX { get { return m_outputLeftX; } }

            private readonly Size m_size;
            private readonly Rectangle m_interior;
            private readonly IEnumerable<int> m_outputLeftX;
        }

        #endregion

        private DiagramTheme m_theme;

        private int m_rowSpacing;
        private int m_pinSize = 8;
        private int m_pinOffset;

        private readonly Dictionary<ICircuitElementType, ElementTypeInfo> m_elementTypeCache =
            new Dictionary<ICircuitElementType, ElementTypeInfo>();

        private const int PinMargin = 2;
        private const int MinElementWidth = 4;
        private const int MinElementHeight = 4;
        private const int HighlightingWidth = 3;
        private const int MaxNameOverhang = 64;

        private static readonly Matrix s_pathTransform = new Matrix();
    }
}