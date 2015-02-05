//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// OBSOLETE. Please use D2dStatechartRenderer instead.
    /// Class to handle rendering and hit testing Statecharts.</summary>
    /// <typeparam name="TNode">Node</typeparam>
    /// <typeparam name="TEdge">Edge</typeparam>
    public class StatechartRenderer<TNode, TEdge> : GraphRenderer<TNode, TEdge, BoundaryRoute>, IDisposable
        where TNode : class, IState
        where TEdge : class, IGraphEdge<TNode, BoundaryRoute>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Theme for rendering statechart</param>
        public StatechartRenderer(DiagramTheme theme)
        {
            Theme = theme;

            UpdateToTheme();
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
        /// Disposes resources</summary>
        /// <param name="disposing">If true, then Dispose() called this method and managed resources should
        /// be released in addition to unmanaged resources. If false, then the finalizer called this method
        /// and no managed objects should be called and only unmanaged resources should be released.</param>
        protected override void Dispose(bool disposing)
        {
            Theme = null;
            base.Dispose(disposing);
        }


        // Radius of rounded corners on states and pseudostates
        private const int CornerRadius = 12;

        /// <summary>
        /// Size of pseudostates</summary>
        public static readonly Size PseudostateSize = new Size(CornerRadius * 2, CornerRadius * 2);

        /// <summary>
        /// Draws a graph node</summary>
        /// <param name="node">Node to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(TNode node, DiagramDrawingStyle style, Graphics g)
        {
            switch (style)
            {
                case DiagramDrawingStyle.Normal:
                    Draw(node, g);
                    break;
                case DiagramDrawingStyle.Selected:
                case DiagramDrawingStyle.LastSelected:
                case DiagramDrawingStyle.Hot:
                case DiagramDrawingStyle.Error:
                    Draw(node, g);
                    DrawOutline(node, m_theme.GetPen(style), g);
                    break;
                case DiagramDrawingStyle.Ghosted:
                case DiagramDrawingStyle.Hidden:
                default:
                    DrawGhost(node, g);
                    break;
            }
        }

        /// <summary>
        /// Draws a graph edge</summary>
        /// <param name="edge">Edge to draw</param>
        /// <param name="style">Diagram drawing style</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(TEdge edge, DiagramDrawingStyle style, Graphics g)
        {
            Draw(edge, m_theme.GetPen(style), g);
        }

        /// <summary>
        /// Draws a partially defined graph edge</summary>
        /// <param name="fromNode">Source node, or null</param>
        /// <param name="fromRoute">Source route, or null</param>
        /// <param name="toNode">Destination node, or null</param>
        /// <param name="toRoute">Destination route, or null</param>
        /// <param name="label">Edge label</param>
        /// <param name="endPoint">Endpoint to substitute for source or destination, if either is null</param>
        /// <param name="g">Graphics object</param>
        public override void Draw(
            TNode fromNode,
            BoundaryRoute fromRoute,
            TNode toNode,
            BoundaryRoute toRoute,
            string label,
            Point endPoint,
            Graphics g)
        {
            // put endpoint into statechart space
            endPoint = GdiUtil.InverseTransform(g.Transform, endPoint);

            Point p1;
            Point normal1;
            if (fromNode != null)
            {
                p1 = ParameterToPoint(fromNode.Bounds, fromRoute.Position, out normal1);
            }
            else
            {
                p1 = endPoint;
                normal1 = new Point();
            }

            Point p4;
            Point normal2;
            if (toNode != null)
            {
                p4 = ParameterToPoint(toNode.Bounds, toRoute.Position, out normal2);
            }
            else
            {
                p4 = endPoint;
                normal2 = new Point();
            }

            Point p2, p3;
            float d = GetTransitionPoints(p1, normal1, p4, normal2, out p2, out p3);
            DrawEdgeSpline(p1, p2, p3, p4, d, m_theme.OutlinePen, g);

            BezierCurve2F curve = new BezierCurve2F(p1, p2, p3, p4);
            Vec2F midpoint = curve.Evaluate(0.5f);
            midpoint.X += 2;
            g.DrawString(label, m_theme.Font, m_theme.TextBrush, new PointF(midpoint.X, midpoint.Y));
        }

        /// <summary>
        /// Gets the bounding rectangle of a node</summary>
        /// <param name="node">Graph node</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Rectangle completely bounding the node</returns>
        public override Rectangle GetBounds(TNode node, Graphics g)
        {
            return GdiUtil.Transform(g.Transform, node.Bounds);
        }

        /// <summary>
        /// Finds node and/or edge hit by the given point</summary>
        /// <param name="graph">Graph to test</param>
        /// <param name="priorityEdge">Graph edge to test before others</param>
        /// <param name="p">Point to test</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Hit record containing node and/or edge hit by the given point</returns>
        public override GraphHitRecord<TNode, TEdge, BoundaryRoute> Pick(
            IGraph<TNode, TEdge, BoundaryRoute> graph,
            TEdge priorityEdge,
            Point p,
            Graphics g)
        {
            int tolerance = m_theme.PickTolerance;

            TNode pickedNode = null;
            TEdge pickedEdge = null;
            BoundaryRoute fromRoute = null;
            BoundaryRoute toRoute = null;

            Vec2F v = new Vec2F(p.X, p.Y);

            if (priorityEdge != null &&
                Pick(priorityEdge, v))
            {
                pickedEdge = priorityEdge;
            }
            else
            {
                foreach (TEdge edge in graph.Edges.Reverse())
                {
                    if (Pick(edge, v))
                    {
                        pickedEdge = edge;
                        break;
                    }
                }
            }

            foreach (TNode state in graph.Nodes.Reverse())
            {
                Rectangle bounds = state.Bounds;
                bounds.Inflate(tolerance, tolerance);

                if (bounds.Contains(p))
                {
                    pickedNode = state;

                    float position = PointToParameter(bounds, p);

                    bounds.Inflate(-2 * tolerance, -2 * tolerance);
                    bool onEdge = !bounds.Contains(p);

                    if (pickedEdge == null)
                    {
                        if (onEdge)
                        {
                            // edge of node can be source or destination
                            fromRoute = new BoundaryRoute(position);
                            toRoute = new BoundaryRoute(position);
                        }
                    }
                    else // hit on edge and node
                    {
                        if (onEdge)
                        {
                            if (pickedEdge.FromNode == pickedNode)
                                fromRoute = new BoundaryRoute(position);
                            else if (pickedEdge.ToNode == pickedNode)
                                toRoute = new BoundaryRoute(position);
                        }
                    }
                    break;
                }
            }

            IComplexState<TNode, TEdge> complexState = pickedNode as IComplexState<TNode, TEdge>;
            if (complexState != null && pickedEdge == null)
            {
                Rectangle bounds = pickedNode.Bounds;
                Rectangle labelBounds = new Rectangle(
                    bounds.X + CornerRadius,
                    bounds.Y + Margin,
                    bounds.Width - 2 * CornerRadius,
                    m_theme.Font.Height);

                if (labelBounds.Contains(p))
                {
                    DiagramLabel label = new DiagramLabel(labelBounds, TextFormatFlags.SingleLine);
                    return
                        new GraphHitRecord<TNode,TEdge, BoundaryRoute>(pickedNode, label);
                }
            }

            return
                new GraphHitRecord<TNode, TEdge, BoundaryRoute>(pickedNode, pickedEdge, fromRoute, toRoute);
        }

        private void theme_Redraw(object sender, EventArgs e)
        {
            UpdateToTheme();
            OnRedraw();
        }

        private void UpdateToTheme()
        {
            if (m_dividerPen != null)
                m_dividerPen.Dispose();
            m_dividerPen = new Pen(m_theme.OutlinePen.Color);
            m_fontHeight = m_theme.Font.Height;
        }

        private bool Pick(TEdge edge, Vec2F v)
        {
            int tolerance = m_theme.PickTolerance;

            Point p1, p2, p3, p4;
            GetTransitionPoints(edge, out p1, out p2, out p3, out p4);

            // check for hit on transition, away from endpoints
            BezierCurve2F curve = new BezierCurve2F(p1, p2, p3, p4);
            Vec2F hitPoint = new Vec2F();
            return BezierCurve2F.Pick(curve, v, m_theme.PickTolerance, ref hitPoint);
        }

        private void Draw(TNode state, Graphics g)
        {
            Rectangle bounds = state.Bounds;
            if (state.Type != StateType.Normal)
            {
                DrawPseudostate(bounds.Location, state.Type, m_theme.OutlinePen, g);
            }
            else
            {
                IComplexState<TNode, TEdge> complexState = state as IComplexState<TNode, TEdge>;

                // Prevent an exception being thrown by LinearGradientBrush.
                if (bounds.Width <= 0)
                    bounds.Width = 1;
                if (bounds.Height <= 0)
                    bounds.Height = 1;

                StateIndicators indicators = state.Indicators;
                if ((indicators & StateIndicators.Active) != 0)
                {
                    g.FillEllipse(
                        Brushes.SpringGreen,
                        bounds.X - CornerRadius,
                        bounds.Y - CornerRadius,
                        2 * CornerRadius,
                        2 * CornerRadius);
                }

                // draw state lozenge
                using (GraphicsPath gp = GetStatePath(bounds))
                {
                    if (!IsPrinting)
                    {
                        using (LinearGradientBrush interiorBrush =
                            new LinearGradientBrush(
                                bounds,
                                Color.WhiteSmoke,
                                Color.LightGray,
                                LinearGradientMode.ForwardDiagonal))
                        {
                            g.FillPath(interiorBrush, gp);
                        }
                    }
                    else
                    {
                        g.FillPath(Brushes.White, gp);
                    }
                    g.DrawPath(m_theme.OutlinePen, gp);
                }

                g.DrawString(
                    complexState.Name,
                    m_theme.Font,
                    m_theme.TextBrush,
                    bounds.X + CornerRadius,
                    bounds.Y + Margin);

                g.DrawLine(
                    m_theme.OutlinePen,
                    bounds.Left, bounds.Top + m_fontHeight + Margin,
                    bounds.Right, bounds.Top + m_fontHeight + Margin);

                RectangleF textBounds = new RectangleF(
                    (float)(bounds.Left + 4),
                    (float)(bounds.Top + m_fontHeight + 2),
                    (float)(bounds.Width - 5),
                    (float)(bounds.Height - m_fontHeight - 4));

                g.DrawString(complexState.Text, m_theme.Font, m_theme.TextBrush, textBounds, s_stateTextFormat);

                //IList<int> partitionWidths = complexState.PartitionSizes;
                //if (partitionWidths.Count > 0)
                //{
                //    // draw AND-state dividers
                //    int lastDivider = bounds.Left;
                //    foreach (int width in partitionWidths)
                //    {
                //        g.DrawLine(
                //            m_dividerPen,
                //            lastDivider, bounds.Y + m_fontHeight + Margin,
                //            lastDivider, bounds.Y + bounds.Height);

                //        lastDivider += width;
                //    }
                //}
            }
        }

        private void DrawGhost(TNode state, Graphics g)
        {
            Rectangle bounds = state.Bounds;
            if (state.Type != StateType.Normal)
            {
                DrawPseudostate(bounds.Location, state.Type, m_theme.GhostPen, g);
            }
            else
            {
                using (GraphicsPath gp = GetStatePath(bounds))
                {
                    g.FillPath(m_theme.GhostBrush, gp);
                    g.DrawPath(m_theme.GhostPen, gp);
                }
            }
        }

        private void DrawPseudostate(Point p, StateType type, Pen pen, Graphics g)
        {
            Point c = new Point(p.X + CornerRadius, p.Y + CornerRadius);
            int size = CornerRadius * 2;

            Brush brush = IsPrinting ? Brushes.White : Brushes.WhiteSmoke;

            g.FillEllipse(brush, p.X, p.Y, size, size);

            switch (type)
            {
                case StateType.Start:
                    g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, size, size);
                    g.FillEllipse(m_theme.TextBrush, c.X - 4, c.Y - 4, 8, 8);
                    break;

                case StateType.Final:
                    using (Pen borderPen = new Pen(pen.Color, 3))
                    {
                        g.DrawEllipse(borderPen, p.X, p.Y, size, size);
                    }
                    g.FillEllipse(m_theme.TextBrush, c.X - 4, c.Y - 4, 8, 8);
                    break;

                case StateType.ShallowHistory:
                    g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, size, size);
                    g.DrawString("H", m_theme.Font, m_theme.TextBrush, c.X - 7, c.Y - 8);
                    break;

                case StateType.DeepHistory:
                    g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, size, size);
                    g.DrawString("H*", m_theme.Font, m_theme.TextBrush, c.X - 8, c.Y - 8);
                    break;

                case StateType.Conditional:
                    g.DrawEllipse(m_theme.OutlinePen, p.X, p.Y, size, size);
                    g.DrawString("C", m_theme.Font, m_theme.TextBrush, c.X - 7, c.Y - 8);
                    break;
            }
        }

        private void DrawOutline(TNode state, Pen pen, Graphics g)
        {
            int tolerance = m_theme.PickTolerance;
            Rectangle bounds = state.Bounds;
            if (state.Type != StateType.Normal)
            {
                g.DrawEllipse(pen, bounds);
            }
            else
            {
                using (GraphicsPath gp = GetStatePath(bounds))
                {
                    g.DrawPath(pen, gp);
                }
            }
        }

        private GraphicsPath GetStatePath(Rectangle bounds)
        {
            GraphicsPath gp = new GraphicsPath();
            const int d = 2 * CornerRadius;
            gp.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            gp.AddArc(bounds.X + bounds.Width - d, bounds.Y, d, d, 270, 90);
            gp.AddArc(bounds.X + bounds.Width - d, bounds.Y + bounds.Height - d, d, d, 0, 90);
            gp.AddArc(bounds.X, bounds.Y + bounds.Height - d, d, d, 90, 90);
            gp.AddLine(bounds.X, bounds.Y + bounds.Height - d, bounds.X, bounds.Y + d / 2);
            return gp;
        }

        private void Draw(TEdge edge, Pen pen, Graphics g)
        {
            Point p1, p2, p3, p4;
            float d = GetTransitionPoints(edge, out p1, out p2, out p3, out p4);

            DrawEdgeSpline(p1, p2, p3, p4, d, pen, g);

            BezierCurve2F curve = new BezierCurve2F(p1, p2, p3, p4);
            Vec2F midpoint = curve.Evaluate(0.5f);
            midpoint.X += 2;
            g.DrawString(edge.Label, m_theme.Font, m_theme.TextBrush, new PointF(midpoint.X, midpoint.Y));
        }

        private static void DrawEdgeSpline(Point p1, Point p2, Point p3, Point p4, float d, Pen pen, Graphics g)
        {
            try
            {
                g.DrawBezier(pen, p1, p2, p3, p4);
            }
            catch (System.OutOfMemoryException)
            {
                // catch erroneous exception, only missing some pixels
                // see http://msdn2.microsoft.com/en-us/library/system.drawing.graphics.drawarc.aspx
            }

            float arrowScale = (float)ArrowSize / d;
            float dx = (p3.X - p4.X) * arrowScale;
            float dy = (p3.Y - p4.Y) * arrowScale;

            DrawArrow(g, pen, p4, dx, dy);
        }

        private float GetTransitionPoints(
            TEdge edge,
            out Point p1,
            out Point p2,
            out Point p3,
            out Point p4)
        {
            TNode fromState = edge.FromNode;
            float fromPosition = edge.FromRoute.Position;
            Point normal1;
            p1 = ParameterToPoint(fromState.Bounds, fromPosition, out normal1);

            TNode toState = edge.ToNode;
            float toPosition = edge.ToRoute.Position;
            Point normal2;
            p4 = ParameterToPoint(toState.Bounds, toPosition, out normal2);

            return GetTransitionPoints(p1, normal1, p4, normal2, out p2, out p3);
        }

        private float GetTransitionPoints(
            Point p1,
            Point normal1,
            Point p4,
            Point normal2,
            out Point p2,
            out Point p3)
        {
            float dx = (float)(p4.X - p1.X);
            float dy = (float)(p4.Y - p1.Y);
            int d = (int)Math.Sqrt(dx * dx + dy * dy) / 2;
            d = Math.Max(1, d);
            d = Math.Min(d, 64);
            p2 = new Point(p1.X + normal1.X * d, p1.Y + normal1.Y * d);
            p3 = new Point(p4.X + normal2.X * d, p4.Y + normal2.Y * d);

            return d;
        }

        private float PointToParameter(Rectangle bounds, Point p)
        {
            // translate problem to one with origin at center of rect
            float cx = bounds.X + bounds.Width * 0.5f;
            float cy = bounds.Y + bounds.Height * 0.5f;

            float px = p.X - cx;
            float py = p.Y - cy;

            // rotate problem into quadrant 0
            //  use "PerpDot" product to determine relative orientation
            //  (Graphics Gems IV, page 138)
            float dx = bounds.Width * 0.5f;
            float dy = bounds.Height * 0.5f;
            float result, temp;
            if (dy * px + dx * py > 0) // quadrant 0 or 1
            {
                if (-dy * px + dx * py < 0) // quadrant 0
                {
                    result = 0;
                }
                else // quadrant 1
                {
                    result = 1;
                    temp = px; px = -py; py = temp;
                    temp = dx; dx = dy; dy = temp;
                }
            }
            else // quadrant 2 or 3
            {
                if (dy * px + -dx * py < 0) // quadrant 2
                {
                    result = 2;
                    px = -px; py = -py;
                }
                else // quadrant 3
                {
                    result = 3;
                    temp = py; py = -px; px = temp;
                    temp = dx; dx = dy; dy = temp;
                }
            }

            float y = dx * py / px;
            result += (y + dy) / (dy * 2);

            return result;
        }

        private Point ParameterToPoint(Rectangle bounds, float t, out Point normal)
        {
            float left = bounds.X;
            float top = bounds.Y;
            float width = bounds.Width;
            float height = bounds.Height;
            float right = left + width;
            float bottom = top + height;

            float x0, y0, dx = 0, dy = 0;
            if (t < 2) // + side
            {
                if (t < 1) // right
                {
                    x0 = right;
                    y0 = top;
                    dy = height;

                    normal = new Point(1, 0);
                }
                else // bottom
                {
                    y0 = bottom;
                    x0 = right;
                    dx = -width;
                    t -= 1;

                    normal = new Point(0, 1);
                }
            }
            else // - side
            {
                if (t < 3) // left
                {
                    x0 = left;
                    y0 = bottom;
                    dy = -height;
                    t -= 2;

                    normal = new Point(-1, 0);
                }
                else // top
                {
                    y0 = top;
                    x0 = left;
                    dx = width;
                    t -= 3;

                    normal = new Point(0, -1);
                }
            }

            return new Point((int)(x0 + dx * t), (int)(y0 + dy * t));
        }

        private Point Project(Rectangle bounds, Point p)
        {
            float t = PointToParameter(bounds, p);
            Point normal;
            Point result = ParameterToPoint(bounds, t, out normal);
            return result;
        }

        private float Distance(Rectangle bounds, Point p)
        {
            Point proj = Project(bounds, p);
            float dx = p.X - proj.X;
            float dy = p.Y - proj.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private Size ExpandSize(Size oldSize, Size newSize)
        {
            return new Size(
                Math.Max(oldSize.Width, newSize.Width),
                Math.Max(oldSize.Height, newSize.Height));
        }

        private static void DrawArrow(Graphics g, Pen pen, Point p, float dx, float dy)
        {
            const double cos = 0.866;
            const double sin = 0.500;
            PointF end1 = new PointF(
                (float)(p.X + (dx * cos + dy * -sin)),
                (float)(p.Y + (dx * sin + dy * cos)));
            PointF end2 = new PointF(
                (float)(p.X + (dx * cos + dy * sin)),
                (float)(p.Y + (dx * -sin + dy * cos)));
            g.DrawLine(pen, p, end1);
            g.DrawLine(pen, p, end2);
        }

        static StatechartRenderer()
        {
            s_stateTextFormat = new StringFormat(StringFormatFlags.NoWrap);
            s_stateTextFormat.Alignment = StringAlignment.Near;
            s_stateTextFormat.LineAlignment = StringAlignment.Far;
        }

        private DiagramTheme m_theme;
        private Pen m_dividerPen;
        private int m_fontHeight;
        private const int StateMargin = 32;
        private const int ArrowSize = 8;
        private const int Margin = 4; // minimum spacing for various purposes
        private const int Levels = 8;

        private static readonly StringFormat s_stateTextFormat;
    }
}
