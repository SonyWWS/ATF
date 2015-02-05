//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.VectorMath;
using Sce.Atf.Direct2D;



namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Class to handle rendering and hit testing state charts</summary>
    /// <typeparam name="TNode">IState node</typeparam>
    /// <typeparam name="TEdge">IGraphEdge edge</typeparam>
    public class D2dStatechartRenderer<TNode, TEdge> : D2dGraphRenderer<TNode, TEdge, BoundaryRoute>, IDisposable
        where TNode : class, IState
        where TEdge : class, IGraphEdge<TNode, BoundaryRoute>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="theme">Theme for rendering state chart</param>
        public D2dStatechartRenderer(D2dDiagramTheme theme)
        {
            m_theme = theme;
            UpdateToTheme();                        
            m_theme.Redraw += theme_Redraw;
            m_stateRect.RadiusX = CornerRadius;
            m_stateRect.RadiusY = CornerRadius;                            
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            m_theme.Redraw -= theme_Redraw;
            if (disposing)
            {
                m_centerText.Dispose();
            }
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
        public override void Draw(TNode node, DiagramDrawingStyle style, D2dGraphics g)
        {
            switch (style)
            {
                case DiagramDrawingStyle.Normal:
                    Draw(node, g, true);
                    break;
                case DiagramDrawingStyle.Selected:
                case DiagramDrawingStyle.LastSelected:
                case DiagramDrawingStyle.Hot:
                case DiagramDrawingStyle.Error:
                    Draw(node, g, false);
                    DrawOutline(node, m_theme.GetOutLineBrush(style), g);
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
        public override void Draw(TEdge edge, DiagramDrawingStyle style, D2dGraphics g)
        {
            Draw(edge, m_theme.GetOutLineBrush(style), g);
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
            D2dGraphics g)
        {
            // put endpoint into statechart space            
            var inverse = g.Transform;
            inverse.Invert();
            PointF end = Matrix3x2F.TransformPoint(inverse, endPoint);

            PointF p1;
            PointF normal1;
            if (fromNode != null)
            {
                p1 = ParameterToPoint(fromNode.Bounds, fromRoute.Position, out normal1);
            }
            else
            {
                p1 = end;
                normal1 = new Point();
            }

            PointF p4;
            PointF normal2;
            if (toNode != null)
            {
                p4 = ParameterToPoint(toNode.Bounds, toRoute.Position, out normal2);
            }
            else
            {
                p4 = end;
                normal2 = new Point();
            }

            PointF p2, p3;
            float d = GetTransitionPoints(p1, normal1, p4, normal2, out p2, out p3);
            DrawEdgeSpline(p1, p2, p3, p4, d, m_theme.OutlineBrush, g);

            if (!string.IsNullOrEmpty(label))
            {
                BezierCurve2F curve = new BezierCurve2F(p1, p2, p3, p4);
                Vec2F midpoint = curve.Evaluate(0.5f);                
                g.DrawText(label, m_centerText, new PointF(midpoint.X, midpoint.Y), m_theme.TextBrush);
            }
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
            PointF p,
            D2dGraphics g)
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
                RectangleF bounds = state.Bounds;
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
                RectangleF bounds = pickedNode.Bounds;
                RectangleF labelBounds = new RectangleF(
                    bounds.X + CornerRadius,
                    bounds.Y + Margin,
                    bounds.Width - 2 * CornerRadius,
                    m_theme.TextFormat.FontHeight);

                if (labelBounds.Contains(p))
                {
                    DiagramLabel label = new DiagramLabel(Rectangle.Truncate(labelBounds), TextFormatFlags.SingleLine);
                    return new GraphHitRecord<TNode, TEdge, BoundaryRoute>(pickedNode, label);
                }
            }

            return new GraphHitRecord<TNode, TEdge, BoundaryRoute>(pickedNode, pickedEdge, fromRoute, toRoute);
        }

        private void theme_Redraw(object sender, EventArgs e)
        {
            UpdateToTheme();
            OnRedraw();
        }

      
        private bool Pick(TEdge edge, Vec2F v)
        {
            int tolerance = m_theme.PickTolerance;

            PointF p1, p2, p3, p4;
            GetTransitionPoints(edge, out p1, out p2, out p3, out p4);

            // check for hit on transition, away from endpoints
            BezierCurve2F curve = new BezierCurve2F(p1, p2, p3, p4);
            Vec2F hitPoint = new Vec2F();
            return BezierCurve2F.Pick(curve, v, m_theme.PickTolerance, ref hitPoint);
        }

        private void Draw(TNode state, D2dGraphics g, bool outline)
        {            
            RectangleF bounds = state.Bounds;
            if (state.Type != StateType.Normal)
            {
                DrawPseudostate(state, g, outline);
            }
            else
            {
                float scaleX = g.Transform.M11; // assume no rotation.
                float radInPixel = scaleX * CornerRadius;

                IComplexState<TNode, TEdge> complexState = state as IComplexState<TNode, TEdge>;
               
                StateIndicators indicators = state.Indicators;
                if ((indicators & StateIndicators.Active) != 0)
                {
                    if (radInPixel > MinRadiusInPixel)
                    {
                        D2dEllipse ellipse = new D2dEllipse();
                        ellipse.RadiusX = CornerRadius;
                        ellipse.RadiusY = CornerRadius;
                        ellipse.Center = bounds.Location;
                        g.FillEllipse(ellipse, Color.SpringGreen);
                    }
                }

                
                if (radInPixel > MinRadiusInPixel)
                {
                    m_stateRect.Rect = bounds;
                    D2dLinearGradientBrush gradbrush = m_theme.FillGradientBrush;
                    gradbrush.StartPoint = bounds.Location;
                    gradbrush.EndPoint = new PointF(bounds.Right, bounds.Bottom);

                    g.FillRoundedRectangle(m_stateRect, gradbrush);
                    if (outline)
                    {
                        g.DrawRoundedRectangle(m_stateRect, m_theme.OutlineBrush);
                    }
                }
                else
                {
                    g.FillRectangle(bounds, m_theme.FillBrush);
                    if (outline)
                    {
                        g.DrawRectangle(bounds, m_theme.OutlineBrush);
                    }

                }
                g.DrawLine(bounds.Left, bounds.Top + m_fontHeight + Margin,
                    bounds.Right, bounds.Top + m_fontHeight + Margin, m_theme.OutlineBrush);

                if ((scaleX * m_fontHeight) > MinFontHeightInPixel)
                {
                    g.DrawText(complexState.Name, m_theme.TextFormat,
                        new PointF(bounds.X + CornerRadius, bounds.Y + Margin), m_theme.TextBrush);
                }

                

                //RectangleF textBounds = new RectangleF(
                //    (float)(bounds.Left + 4),
                //    (float)(bounds.Top + m_fontHeight + 2),
                //    (float)(bounds.Width - 5),
                //    (float)(bounds.Height - m_fontHeight - 4));

                //g.DrawString(complexState.Text, m_theme.Font, m_theme.TextBrush, textBounds, s_stateTextFormat);

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

        private void DrawGhost(TNode state, D2dGraphics g)
        {
            RectangleF bounds = state.Bounds;
            if (state.Type != StateType.Normal)
            {
                g.FillEllipse((D2dEllipse)bounds, m_theme.GhostBrush);
            }
            else
            {
                m_stateRect.Rect = bounds;
                g.FillRoundedRectangle(m_stateRect, m_theme.GhostBrush);                
            }
        }

        private void DrawPseudostate( TNode state, D2dGraphics g, bool outline)
        {
            RectangleF bounds = state.Bounds;
            D2dEllipse ellipse = (D2dEllipse)bounds;
            D2dEllipse innerEllipse = ellipse;
            innerEllipse.RadiusX = 4;
            innerEllipse.RadiusY = 4;

            g.FillEllipse(ellipse, m_theme.FillBrush);

            switch (state.Type)
            {
                case StateType.Start:                    
                    g.FillEllipse(innerEllipse, m_theme.TextBrush);
                    break;
                case StateType.Final:
                    g.DrawEllipse(ellipse, m_theme.OutlineBrush, 3.0f);
                    g.FillEllipse(innerEllipse, m_theme.TextBrush);                    
                    break;
                case StateType.ShallowHistory:
                    g.DrawText("H", m_centerText, bounds, m_theme.TextBrush);
                    break;
                case StateType.DeepHistory:                
                    g.DrawText("H*", m_centerText, bounds, m_theme.TextBrush);
                    break;
                case StateType.Conditional:                    
                    g.DrawText("C", m_centerText, bounds, m_theme.TextBrush);                    
                    break;
            }

            if(outline && state.Type != StateType.Final)
                g.DrawEllipse(ellipse,m_theme.OutlineBrush);
        }

        private void DrawOutline(TNode state, D2dBrush brush, D2dGraphics g)
        {            
            RectangleF bounds = state.Bounds;
            if (state.Type != StateType.Normal)
            {
                g.DrawEllipse((D2dEllipse)bounds, brush, m_theme.StrokeWidth);
            }
            else
            {
                float scaleX = g.Transform.M11; // assume no rotation.
                float radInPixel = scaleX * CornerRadius;
                if (radInPixel > MinRadiusInPixel)
                {
                    m_stateRect.Rect = bounds;
                    g.DrawRoundedRectangle(m_stateRect, brush, m_theme.StrokeWidth);
                }
                else
                {
                    g.DrawRectangle(bounds, brush, m_theme.StrokeWidth);
                }
                
            }
        }
             
        private void Draw(TEdge edge, D2dBrush brush, D2dGraphics g)
        {
            PointF p1, p2, p3, p4;
            float d = GetTransitionPoints(edge, out p1, out p2, out p3, out p4);

            DrawEdgeSpline(p1, p2, p3, p4, d, brush, g);
            GdiUtil.MakeRectangle(p1, p2);

            BezierCurve2F curve = new BezierCurve2F(p1, p2, p3, p4);
            Vec2F midpoint = curve.Evaluate(0.5f);
            midpoint.X += 2;
            string label = edge.Label;
            if(!string.IsNullOrEmpty(label))
                g.DrawText(edge.Label, m_theme.TextFormat, new PointF(midpoint.X, midpoint.Y), m_theme.TextBrush);
        }

        private void DrawEdgeSpline(PointF p1, PointF p2, PointF p3, PointF p4, float d, D2dBrush brush, D2dGraphics g)
        {
            g.DrawBezier(p1, p2, p3, p4, brush, m_theme.StrokeWidth);
            float arrowScale = (float)ArrowSize / d;
            float dx = (p3.X - p4.X) * arrowScale;
            float dy = (p3.Y - p4.Y) * arrowScale;
            DrawArrow(g, brush, p4, dx, dy);
        }

        private float GetTransitionPoints(
            TEdge edge,
            out PointF p1,
            out PointF p2,
            out PointF p3,
            out PointF p4)
        {
            TNode fromState = edge.FromNode;
            float fromPosition = edge.FromRoute.Position;
            PointF normal1;
            p1 = ParameterToPoint(fromState.Bounds, fromPosition, out normal1);

            TNode toState = edge.ToNode;
            float toPosition = edge.ToRoute.Position;
            PointF normal2;
            p4 = ParameterToPoint(toState.Bounds, toPosition, out normal2);

            return GetTransitionPoints(p1, normal1, p4, normal2, out p2, out p3);
        }

        private float GetTransitionPoints(
            PointF p1,
            PointF normal1,
            PointF p4,
            PointF normal2,
            out PointF p2,
            out PointF p3)
        {
            float dx = (float)(p4.X - p1.X);
            float dy = (float)(p4.Y - p1.Y);
            int d = (int)Math.Sqrt(dx * dx + dy * dy) / 2;
            d = Math.Max(1, d);
            d = Math.Min(d, 64);
            p2 = new PointF(p1.X + normal1.X * d, p1.Y + normal1.Y * d);
            p3 = new PointF(p4.X + normal2.X * d, p4.Y + normal2.Y * d);

            return d;
        }

        private float PointToParameter(RectangleF bounds, PointF p)
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

        private PointF ParameterToPoint(RectangleF bounds, float t, out PointF normal)
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

            return new PointF(x0 + dx * t, y0 + dy * t);
        }

        private PointF Project(RectangleF bounds, PointF p)
        {
            float t = PointToParameter(bounds, p);
            PointF normal;
            PointF result = ParameterToPoint(bounds, t, out normal);
            return result;
        }

        private float Distance(RectangleF bounds, Point p)
        {
            PointF proj = Project(bounds, p);
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

        private void DrawArrow(D2dGraphics g, D2dBrush brush, PointF p, float dx, float dy)
        {
            const float cos = 0.866f;
            const float sin = 0.500f;
            PointF end1 = new PointF(
                (float)(p.X + (dx * cos + dy * -sin)),
                (float)(p.Y + (dx * sin + dy * cos)));
            PointF end2 = new PointF(
                (float)(p.X + (dx * cos + dy * sin)),
                (float)(p.Y + (dx * -sin + dy * cos)));
            g.DrawLine(p, end1, brush, m_theme.StrokeWidth);
            g.DrawLine(p, end2, brush, m_theme.StrokeWidth);
        }

        private void UpdateToTheme()
        {
            D2dTextFormat textFormat = m_theme.TextFormat;
            if (m_centerText == null
                || textFormat.FontFamilyName != m_centerText.FontFamilyName
                || textFormat.FontSize != m_centerText.FontSize)
            {
                if(m_centerText != null)
                    m_centerText.Dispose();

                m_centerText = D2dFactory.CreateTextFormat(textFormat.FontFamilyName,
                    D2dFontWeight.Bold, D2dFontStyle.Normal, m_theme.TextFormat.FontHeight);
                m_centerText.TextAlignment = D2dTextAlignment.Center;
                m_centerText.ParagraphAlignment = D2dParagraphAlignment.Center;
                m_fontHeight = m_theme.TextFormat.FontHeight;
            }
        }
      
        // Level of detail control variables.

        private const float MinRadiusInPixel = 3.0f;
        private const float MinFontHeightInPixel = 5.0f;


        private D2dDiagramTheme m_theme;
        private D2dTextFormat m_centerText;
        private float m_fontHeight;
        private const int StateMargin = 32;
        private const int ArrowSize = 8;
        private const int Margin = 4; // minimum spacing for various purposes        
        private D2dRoundedRect m_stateRect = new D2dRoundedRect();
    }
}
