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
    /// OBSOLETE. Please use D2dDigraphRenderer instead.
    /// Standard directed graph renderer that renders nodes as disks and edges as lines or
    /// arcs. For this graph renderer, edge routes are integer indices, indicating which
    /// line or arc to draw for the edge. This allows multiple edges between a pair of states
    /// to be distinguished.</summary>
    /// <typeparam name="TNode">IGraphNode node</typeparam>
    /// <typeparam name="TEdge">IGraphEdge edge</typeparam>
    public class DigraphRenderer<TNode, TEdge> :
        GraphRenderer<TNode, TEdge, NumberedRoute>,
        IDisposable
        where TNode : class, IGraphNode
        where TEdge : class, IGraphEdge<TNode, NumberedRoute>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Diagram theme for node</param>
        public DigraphRenderer(DiagramTheme theme)
        {
            Theme = theme;
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
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
        /// Gets and sets amount, in pixels, that edges are offset when routing</summary>
        public int RouteOffset
        {
            get { return m_routeOffset; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Route offset must be greater than 0");

                if (m_routeOffset != value)
                {
                    m_routeOffset = value;
                    OnRedraw();
                }
            }
        }

        /// <summary>
        /// Gets and sets size, in pixels, of arrows at ends of edge</summary>
        public int ArrowSize
        {
            get { return m_arrowSize; }
            set
            {
                if (value < 1)
                    throw new ArgumentException("Arrow size must be greater than 0");

                if (m_arrowSize != value)
                {
                    m_arrowSize = value;
                    OnRedraw();
                }
            }
        }

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
                    Draw(node, g);
                    DrawOutline(node, m_theme.GetPen(style), g);
                    break;
                case DiagramDrawingStyle.Ghosted:
                case DiagramDrawingStyle.Hidden:
                case DiagramDrawingStyle.Error:
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
            NumberedRoute fromRoute,
            TNode toNode,
            NumberedRoute toRoute,
            string label,
            Point endPoint,
            Graphics g)
        {
            Matrix inverse = g.Transform;
            inverse.Invert();
            PointF end = GdiUtil.Transform(inverse, endPoint);

            TNode node = (fromNode != null) ? fromNode : toNode;
            CircleF boundary = GetBoundary(node);
            Vec2F proj = new Vec2F();
            if (CircleF.Project(new Vec2F(end), boundary, ref proj))
            {
                PointF start = new PointF(proj.X, proj.Y);
                g.DrawLine(m_theme.OutlinePen, start, end);

                if (fromNode == null)
                {
                    PointF temp = end;
                    end = start;
                    start = temp;
                }
                Vec2F endTangent = new Vec2F(end.X - start.X, end.Y - start.Y);
                Vec2F arrowPosition = new Vec2F(end);
                DrawArrow(arrowPosition, endTangent, m_theme.OutlinePen, g);

                if (!string.IsNullOrEmpty(label))
                {
                    PointF textPoint = new PointF((end.X + start.X) * 0.5f, (end.Y + start.Y) * 0.5f);
                    RectangleF textBox = new RectangleF(textPoint.X - 512, textPoint.Y, 1024, m_theme.Font.Height);
                    g.DrawString(label, m_theme.Font, m_theme.TextBrush, textBox, m_theme.CenterStringFormat);
                }

            }
        }

        /// <summary>
        /// Gets the bounding rect of a node</summary>
        /// <param name="node">Node</param>
        /// <param name="g">Graphics object</param>
        /// <returns>Minimum rectangle completely bounding the node</returns>
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
        public override GraphHitRecord<TNode, TEdge, NumberedRoute> Pick(
            IGraph<TNode, TEdge, NumberedRoute> graph,
            TEdge priorityEdge,
            Point p,
            Graphics g)
        {
            TNode pickedNode = null;
            TEdge pickedEdge = null;
            NumberedRoute fromRoute = null;
            NumberedRoute toRoute = null;

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

            foreach (TNode node in graph.Nodes.Reverse())
            {
                if (Pick(node, p))
                {
                    pickedNode = node;

                    CircleF boundary = GetBoundary(node);
                    boundary.Radius -= m_theme.PickTolerance;
                    bool onEdge = !boundary.Contains(v);

                    if (pickedEdge == null)
                    {
                        if (onEdge)
                        {
                            // edge of node can be source or destination
                            fromRoute = new NumberedRoute();
                            toRoute = new NumberedRoute();
                        }
                    }
                    else // hit on edge and node
                    {
                        if (onEdge)
                        {
                            if (pickedEdge.FromNode == pickedNode)
                                fromRoute = new NumberedRoute();
                            else if (pickedEdge.ToNode == pickedNode)
                                toRoute = new NumberedRoute();
                        }
                    }
                    break;
                }
            }

            GraphHitRecord<TNode, TEdge, NumberedRoute> result =
                new GraphHitRecord<TNode, TEdge, NumberedRoute>(pickedNode, pickedEdge, fromRoute, toRoute);

            if (pickedNode != null && pickedEdge == null)
            {
                // label is centered in entire node
                Rectangle labelBounds = GetBounds(pickedNode, g);
                int dHeight = labelBounds.Height - m_theme.Font.Height;
                labelBounds = new Rectangle(
                    labelBounds.X, labelBounds.Y + dHeight / 2, labelBounds.Width, labelBounds.Height - dHeight);

                if (labelBounds.Contains(p))
                {
                    DiagramLabel label = new DiagramLabel(
                        labelBounds,
                        TextFormatFlags.SingleLine |
                        TextFormatFlags.HorizontalCenter);

                    return
                        new GraphHitRecord<TNode, TEdge, NumberedRoute>(pickedNode, label);
                }

            }
            else if (pickedEdge != null)
            {
                Rectangle labelBounds = GetLabelBounds(pickedEdge);
                DiagramLabel label = new DiagramLabel(
                    labelBounds,
                    TextFormatFlags.SingleLine |
                    TextFormatFlags.HorizontalCenter);

                if (labelBounds.Contains(p))
                {
                    return
                        new GraphHitRecord<TNode, TEdge, NumberedRoute>(pickedEdge, label);
                }
            }

            return result;
        }

        private void theme_Redraw(object sender, EventArgs e)
        {
            OnRedraw();
        }

        private void Draw(TNode node, Graphics g)
        {
            RectangleF bounds = node.Bounds;

            // clip to window to speed up drawing when zoomed in
            if (g.ClipBounds.IntersectsWith(bounds))
            {
                using (LinearGradientBrush lgb = new LinearGradientBrush(
                    bounds,
                    Color.White,
                    Color.LightSteelBlue,
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillEllipse(lgb, bounds);
                    g.DrawEllipse(m_theme.OutlinePen, bounds);
                }

                g.DrawString(node.Name, m_theme.Font, m_theme.TextBrush, bounds, m_theme.CenterStringFormat);
            }
        }

        private void DrawGhost(TNode node, Graphics g)
        {
            RectangleF bounds = node.Bounds;

            // clip to window to speed up drawing when zoomed in
            if (g.ClipBounds.IntersectsWith(bounds))
            {
                g.FillEllipse(Brushes.White, bounds);
                g.DrawEllipse(m_theme.GhostPen, bounds);
            }
        }

        private void DrawOutline(TNode node, Pen pen, Graphics g)
        {
            g.DrawEllipse(pen, node.Bounds);
        }

        private bool Pick(TNode node, Point p)
        {
            CircleF boundary = GetBoundary(node);
            boundary.Radius += m_theme.PickTolerance;
            return boundary.Contains(new Vec2F(p.X, p.Y));
        }

        private bool Pick(TEdge edge, Vec2F p)
        {
            bool result = false;
            Vec2F nearest = new Vec2F();

            Vec2F startPoint = new Vec2F();
            Vec2F endPoint = new Vec2F();
            CircleF c = new CircleF();
            bool moreThan180 = false;
            int route = edge.FromRoute.Index;
            if (GetEdgeGeometry(edge, route, ref startPoint, ref endPoint, ref c, ref moreThan180))
            {
                Seg2F seg = new Seg2F(startPoint, endPoint);
                nearest = Seg2F.Project(seg, p);
                if (Vec2F.Distance(nearest, p) < m_theme.PickTolerance)
                    result = true;
            }
            else
            {
                if (CircleF.Project(p, c, ref nearest))
                {
                    if (Vec2F.Distance(nearest, p) < m_theme.PickTolerance)
                    {
                        Vec2F startToEnd = endPoint - startPoint;
                        Vec2F startToCenter = c.Center - startPoint;
                        Vec2F startToP = p - startPoint;
                        float pdp1 = Vec2F.PerpDot(startToCenter, startToEnd);
                        float pdp2 = Vec2F.PerpDot(startToP, startToEnd);
                        bool side = pdp1 * pdp2 < 0;
                        result = moreThan180 ^ side;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets geometric information for a edge and a route</summary>
        /// <param name="edge">Edge</param>
        /// <param name="route">Edge's route</param>
        /// <param name="startPoint">Calculated starting point for edge</param>
        /// <param name="endPoint">Calculated ending point for edge</param>
        /// <param name="circle">Calculated embedding circle for edge</param>
        /// <param name="moreThan180">Calculated sweep angle hint</param>
        /// <returns>True iff edge is straight line</returns>
        private bool GetEdgeGeometry(
            TEdge edge,
            int route,
            ref Vec2F startPoint,
            ref Vec2F endPoint,
            ref CircleF circle,
            ref bool moreThan180)
        {
            bool straightLine = false;

            if (edge.FromNode == edge.ToNode)
            {
                CircleF c = GetBoundary(edge.FromNode);
                circle = c;
                circle.Center.X -= circle.Radius;
                circle.Radius *= 0.85f;
                float offset = route * m_routeOffset / 2;
                circle.Center.X -= offset;
                circle.Radius += offset;
                CircleF.Intersect(circle, c, ref startPoint, ref endPoint);
                moreThan180 = true;
            }
            else
            {
                CircleF c1 = GetBoundary(edge.FromNode);
                CircleF c2 = GetBoundary(edge.ToNode);
                Vec2F d = Vec2F.Sub(c2.Center, c1.Center);
                float length = d.Length;
                if (length < c1.Radius + c2.Radius)
                {
                    // overlapping nodes, don't try to draw arcs
                    d = Vec2F.XAxis;
                    straightLine = true;
                }
                else if (route == 0)
                {
                    d *= 1.0f / length;
                    straightLine = true;
                }
                else
                {
                    Vec2F routePoint = c1.Center + d * 0.5f;
                    float dLength = d.Length;
                    Vec2F offset = d.Perp * (1.0f / dLength);
                    routePoint -= offset * m_routeOffset * route;
                    circle = new CircleF(c1.Center, routePoint, c2.Center);

                    Vec2F dummy = new Vec2F();
                    CircleF.Intersect(circle, c1, ref startPoint, ref dummy);
                    CircleF.Intersect(circle, c2, ref dummy, ref endPoint);

                    Vec2F startToEnd = startPoint - endPoint;
                    Vec2F centerToEnd = circle.Center - endPoint;
                    moreThan180 = Vec2F.PerpDot(centerToEnd, startToEnd) < 0;
                }

                if (straightLine)
                {
                    startPoint = Vec2F.Add(c1.Center, Vec2F.Mul(d, c1.Radius));
                    endPoint = Vec2F.Sub(c2.Center, Vec2F.Mul(d, c2.Radius));
                }
            }

            return straightLine;
        }

        private void Draw(TEdge edge, Pen pen, Graphics g)
        {
            Vec2F startPoint = new Vec2F();
            Vec2F endPoint = new Vec2F();
            CircleF c = new CircleF();
            bool moreThan180 = false;
            Vec2F endTangent;
            Vec2F textPoint;
            int route = edge.FromRoute.Index;
            if (GetEdgeGeometry(edge, route, ref startPoint, ref endPoint, ref c, ref moreThan180))
            {
                g.DrawLine(pen, new PointF(startPoint.X, startPoint.Y), new PointF(endPoint.X, endPoint.Y));

                endTangent = endPoint - startPoint;
                textPoint = (endPoint + startPoint) * 0.5f;
            }
            else
            {
                // prepare to draw arc
                RectangleF rect = new RectangleF(c.Center.X - c.Radius, c.Center.Y - c.Radius, 2 * c.Radius, 2 * c.Radius);

                double angle1 = Math.Atan2(startPoint.Y - c.Center.Y, startPoint.X - c.Center.X);
                double angle2 = Math.Atan2(endPoint.Y - c.Center.Y, endPoint.X - c.Center.X);
                const double twoPi = 2 * Math.PI;

                // swap so we always go clockwise
                if (angle1 > angle2)
                {
                    double temp = angle1;
                    angle1 = angle2;
                    angle2 = temp;
                }

                double startAngle = angle1;
                double sweepAngle = angle2 - angle1;

                if (moreThan180)
                {
                    if (sweepAngle < Math.PI)
                        sweepAngle = -(twoPi - sweepAngle);
                }
                else
                {
                    if (sweepAngle > Math.PI)
                        sweepAngle = -(twoPi - sweepAngle);
                }

                const double RadiansToDegrees = 360 / twoPi;
                startAngle *= RadiansToDegrees;
                sweepAngle *= RadiansToDegrees;

                //                g.DrawArc(Pens.LightGray, rect, 0, 360);
                //                g.DrawLine(Pens.LightGray, new PointF(c.Center.X, c.Center.Y), new PointF(startPoint.X, startPoint.Y));
                //                g.DrawLine(Pens.LightGray, new PointF(c.Center.X, c.Center.Y), new PointF(endPoint.X, endPoint.Y));
                g.DrawArc(pen, rect, (float)startAngle, (float)sweepAngle);

                endTangent = endPoint - c.Center; endTangent = endTangent.Perp;
                textPoint = (endPoint + startPoint) * 0.5f;
                CircleF.Project(textPoint, c, ref textPoint);
                if (moreThan180)
                    textPoint -= 2 * (textPoint - c.Center);
            }

            DrawArrow(endPoint, endTangent, pen, g);

            string label = edge.Label;
            if (!string.IsNullOrEmpty(label))
            {
                RectangleF textBox = new RectangleF(textPoint.X - 512, textPoint.Y, 1024, m_theme.Font.Height);
                g.DrawString(label, m_theme.Font, m_theme.TextBrush, textBox, m_theme.CenterStringFormat);
            }
        }

        private Rectangle GetLabelBounds(TEdge edge)
        {
            Vec2F startPoint = new Vec2F();
            Vec2F endPoint = new Vec2F();
            CircleF c = new CircleF();
            bool moreThan180 = false;
            Vec2F textPoint;
            int route = edge.FromRoute.Index;
            if (GetEdgeGeometry(edge, route, ref startPoint, ref endPoint, ref c, ref moreThan180))
            {
                textPoint = (endPoint + startPoint) * 0.5f;
            }
            else
            {
                textPoint = (endPoint + startPoint) * 0.5f;
                CircleF.Project(textPoint, c, ref textPoint);
                if (moreThan180)
                    textPoint -= 2 * (textPoint - c.Center);
            }

            float height = m_theme.Font.Height;
            float width = 32;
            if (!string.IsNullOrEmpty(edge.Label))
            {
                SizeF size = TextRenderer.MeasureText(edge.Label, m_theme.Font);
                width = size.Width;
                height = size.Height;
            }
            return new Rectangle((int)(textPoint.X - width * 0.5f), (int)textPoint.Y, (int)width, (int)height);
        }

        private void DrawArrow(Vec2F p, Vec2F d, Pen pen, Graphics g)
        {
            d *= 1.0f / d.Length;
            // draw arrowhead
            const double cos = -0.866;
            const double sin = -0.500;
            PointF head = new PointF(p.X, p.Y);
            PointF end1 = new PointF(
                (float)(p.X + (d.X * cos + d.Y * -sin) * m_arrowSize),
                (float)(p.Y + (d.X * sin + d.Y * cos) * m_arrowSize));
            PointF end2 = new PointF(
                (float)(p.X + (d.X * cos + d.Y * sin) * m_arrowSize),
                (float)(p.Y + (d.X * -sin + d.Y * cos) * m_arrowSize));
            g.DrawLine(pen, head, end1);
            g.DrawLine(pen, head, end2);
        }
        
        private CircleF GetBoundary(TNode node)
        {
            Rectangle bounds = node.Bounds;
            float r = bounds.Width / 2;
            Vec2F c = new Vec2F(bounds.X + r, bounds.Y + r);
            return new CircleF(c, r);
        }

        private const int MinimumRadius = 32;

        private DiagramTheme m_theme;

        private int m_routeOffset = 24;
        private int m_arrowSize = 8;
    }
}
