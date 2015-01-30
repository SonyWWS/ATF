//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Class used by curve control for picking and rendering</summary>
    public class CurveRenderer : IDisposable
    {
        /// <summary>
        /// Gets or sets control point size</summary>
        public float PointSize
        {
            get { return m_pointSize; }
            set
            {
                if (value < 2 || value > 64)
                    throw new ArgumentOutOfRangeException();
                m_pointSize = value;
            }
        }

        /// <summary>
        /// Picks one or more control points within given rectangle</summary>
        /// <param name="curves">Curves on which to pick points</param>
        /// <param name="pickRect">Rectangle bounding picked control points</param>
        /// <param name="points">Points picked</param>
        /// <param name="regions">PointSelectionRegions of picked points</param>
        /// <param name="singlePick">if true only single point, tangetnIn, or tangentOut will be picked</param>
        public void PickPoints(IEnumerable<ICurve> curves, RectangleF pickRect,
            List<IControlPoint> points, List<PointSelectionRegions> regions, bool singlePick = false)
        {
            points.Clear();
            regions.Clear();
            if (curves == null)
                return;

            foreach(var curve in curves)
            {            
                if (!curve.Visible)
                    continue;

                ReadOnlyCollection<IControlPoint> curvePoints = curve.ControlPoints;
                for (int i = 0; i < curvePoints.Count; i++)
                {
                    IControlPoint cpt = curvePoints[i];
                    Vec2F clientcpt = m_canvas.GraphToClient(cpt.X, cpt.Y);
                    if (pickRect.Contains(clientcpt))
                    {
                        points.Add(cpt);
                        regions.Add(PointSelectionRegions.Point);
                    }
                    else if (curve.CurveInterpolation != InterpolationTypes.Linear && cpt.EditorData.SelectedRegion != PointSelectionRegions.None)
                    {
                        bool tanPicked = false;
                        bool pickTanOut = cpt.TangentOutType != CurveTangentTypes.Stepped && cpt.TangentOutType != CurveTangentTypes.SteppedNext;
                        if (pickTanOut)
                        {
                            Vec2F tangOut = Vec2F.Normalize(m_canvas.GraphToClientTangent(cpt.TangentOut));
                            Seg2F seg = new Seg2F(clientcpt, (clientcpt + tangOut * m_tangentLength));
                            if (GdiUtil.Intersects(seg, pickRect))
                            {
                                points.Add(cpt);
                                regions.Add(PointSelectionRegions.TangentOut);
                                tanPicked = true;                                
                            }
                        }

                        bool pickTanIn = true;
                        if (i > 0)
                        {
                            IControlPoint prevCpt = curvePoints[i - 1];
                            pickTanIn = prevCpt.TangentOutType != CurveTangentTypes.Stepped
                            && prevCpt.TangentOutType != CurveTangentTypes.SteppedNext;
                        }

                        if (!tanPicked && pickTanIn)
                        {
                            //  pick tangentIn.
                            Vec2F tangIn = Vec2F.Normalize(m_canvas.GraphToClientTangent(cpt.TangentIn));
                            tangIn.X = -tangIn.X;
                            tangIn.Y = -tangIn.Y;
                            Seg2F seg = new Seg2F(clientcpt, (clientcpt + tangIn * m_tangentLength));
                            if (GdiUtil.Intersects(seg, pickRect))
                            {
                                points.Add(cpt);
                                regions.Add(PointSelectionRegions.TangentIn);
                            }
                        }
                    }
                    if (singlePick && points.Count > 0) break;
                }
                if (singlePick && points.Count > 0) break;
            } // foreach curve in curves
        }

        /// <summary>
        /// Picks one or more control points or curves within given rectangle</summary>
        /// <param name="curves">Curves on which to pick points</param>
        /// <param name="pickRect">Rectangle bounding picked control points</param>
        /// <param name="points">Points picked</param>
        /// <param name="regions">PointSelectionRegions of picked points</param>
        /// <param name="singlePick">If true only single point or single curve will be picked</param>
        public void Pick(IEnumerable<ICurve> curves, RectangleF pickRect,
            List<IControlPoint> points, List<PointSelectionRegions> regions, bool singlePick = false)
        {

            PickPoints(curves, pickRect, points, regions, singlePick);
            if (curves == null)
                return;

            if (points.Count == 0)
            {
                foreach (ICurve curve in curves)
                {
                    if (!curve.Visible)
                        continue;
                    if (HitTest(curve, pickRect))
                    {
                        foreach (IControlPoint cpt in curve.ControlPoints)
                        {
                            points.Add(cpt);
                            regions.Add(PointSelectionRegions.Point);
                        }
                        if (singlePick) break;
                    }
                }
            }
        }

        /// <summary>
        /// Hit test curve</summary>
        /// <param name="curve">Curve</param>
        /// <param name="pickRect">Rectangle</param>
        /// <returns>True iff hit</returns>
        public bool HitTest(ICurve curve, RectangleF pickRect)
        {
            if (curve == null)
                return false;

            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (!curve.Visible || points.Count == 0)
                return false;
            float step = m_tessellation / m_canvas.Zoom.X;
            ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator(curve);
            float start = m_canvas.ClientToGraph(pickRect.X);
            float end = m_canvas.ClientToGraph(pickRect.Right);
            float rangeX = end - start;
            PointF scrPt = new PointF();
            float fpx = points[0].X;
            float lpx = points[points.Count - 1].X;

            if ((start < fpx && end < fpx) || (start > lpx && end > lpx))
                return false;
            for (float x = 0; x < rangeX; x += step)
            {
                float xv = start + x;
                float y = cv.Evaluate(xv);
                scrPt = m_canvas.GraphToClient(xv, y);
                if (pickRect.Contains(scrPt))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Draws given curve</summary>
        /// <param name="curve">Curve</param>
        /// <param name="g">Graphics object</param>
        /// <param name="thickness">Curve thickness in pixels</param>
        public void DrawCurve(ICurve curve, Graphics g, float thickness = 1.0f )
        {
            // draw pre-infinity
            // draw actual
            // draw post-inifiny.
            
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (points.Count == 0)
                return;

            m_infinityPen.Width = thickness;
            m_curvePen.Width = thickness;
            m_infinityPen.Color = curve.CurveColor;
            m_curvePen.Color = curve.CurveColor;
            if (points.Count == 1)
            {
                Vec2F p = m_canvas.GraphToClient(0, points[0].Y);
                g.DrawLine(m_infinityPen, 0, p.Y, m_canvas.ClientSize.Width, p.Y);
                return;
            }

            float w = m_canvas.ClientSize.Width;
            float h = m_canvas.ClientSize.Height;
            float x0 = m_canvas.ClientToGraph(0);
            float x1 = m_canvas.ClientToGraph(w);
            IControlPoint fpt = points[0];
            IControlPoint lpt = points[points.Count - 1];
            float step = m_tessellation / m_canvas.Zoom.X;
            List<PointF> pointList = new List<PointF>(m_canvas.Width / m_tessellation);
            ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator(curve);
            PointF scrPt = new PointF();
            float bound = 500; // guard again gdi+ overflow.            
            float minY = -bound;
            float maxY = h + bound;

            // draw pre infinity
            if (fpt.X > x0)
            {
                float start = x0;
                float end = Math.Min(fpt.X, x1);
                float rangeX = end - start;
                for (float x = 0; x < rangeX; x += step)
                {
                    float xv = start + x;
                    float y = cv.Evaluate(xv);
                    scrPt = m_canvas.GraphToClient(xv, y);
                    scrPt.Y = MathUtil.Clamp(scrPt.Y, minY, maxY);
                    pointList.Add(scrPt);
                }
                scrPt = m_canvas.GraphToClient(end, cv.Evaluate(end));
                scrPt.Y = MathUtil.Clamp(scrPt.Y, minY, maxY);
                pointList.Add(scrPt);
                if (pointList.Count > 1)
                    g.DrawLines(m_infinityPen, pointList.ToArray());
            }

            // draw actual 
            if ((fpt.X > x0 || lpt.X > x0) && (fpt.X < x1 || lpt.X < x1))
            {
                int leftIndex;
                int rightIndex;
                ComputeIndices(curve, out leftIndex, out rightIndex);
                if (curve.CurveInterpolation == InterpolationTypes.Linear)
                {
                    for (int i = leftIndex; i < rightIndex; i++)
                    {
                        IControlPoint p1 = points[i];
                        IControlPoint p2 = points[i + 1];
                        PointF cp1 = m_canvas.GraphToClient(p1.X, p1.Y);
                        PointF cp2 = m_canvas.GraphToClient(p2.X, p2.Y);
                        g.DrawLine(m_curvePen, cp1.X, cp1.Y, cp2.X, cp2.Y);
                    }
                }
                else
                {
                    for (int i = leftIndex; i < rightIndex; i++)
                    {
                        IControlPoint p1 = points[i];
                        IControlPoint p2 = points[i + 1];
                        if (p1.TangentOutType == CurveTangentTypes.Stepped)
                        {
                            PointF cp1 = m_canvas.GraphToClient(p1.X, p1.Y);
                            PointF cp2 = m_canvas.GraphToClient(p2.X, p2.Y);
                            g.DrawLine(m_curvePen, cp1.X, cp1.Y, cp2.X, cp1.Y);
                            g.DrawLine(m_curvePen, cp2.X, cp1.Y, cp2.X, cp2.Y);
                        }
                        else if (p1.TangentOutType != CurveTangentTypes.SteppedNext)
                        {
                            float start = Math.Max(p1.X, x0);
                            float end = Math.Min(p2.X, x1);
                            pointList.Clear();
                            float rangeX = end - start;
                            for (float x = 0; x < rangeX; x += step)
                            {
                                float xv = start + x;
                                float y = cv.Evaluate(xv);
                                scrPt = m_canvas.GraphToClient(xv, y);
                                scrPt.Y = MathUtil.Clamp(scrPt.Y, minY, maxY);
                                pointList.Add(scrPt);
                            }
                            scrPt = m_canvas.GraphToClient(end, cv.Evaluate(end));
                            scrPt.Y = MathUtil.Clamp(scrPt.Y, minY, maxY);
                            pointList.Add(scrPt);
                            if (pointList.Count > 1)
                                g.DrawLines(m_curvePen, pointList.ToArray());
                        }
                    }// for (int i = leftIndex; i < rightIndex; i++)
                }
            }
            //draw post-infinity.
            if (lpt.X < x1)
            {
                pointList.Clear();
                float start = Math.Max(x0, lpt.X);
                float end = x1;
                float rangeX = end - start;
                for (float x = 0; x < rangeX; x += step)
                {
                    float xv = start + x;
                    float y = cv.Evaluate(xv);
                    scrPt = m_canvas.GraphToClient(xv, y);
                    scrPt.Y = MathUtil.Clamp(scrPt.Y, minY, maxY);
                    pointList.Add(scrPt);
                }
                scrPt = m_canvas.GraphToClient(end, cv.Evaluate(end));
                scrPt.Y = MathUtil.Clamp(scrPt.Y, minY, maxY);
                pointList.Add(scrPt);
                if (pointList.Count > 1)
                    g.DrawLines(m_infinityPen, pointList.ToArray());
            }
        }

        /// <summary>
        /// Draws all the control points for the given curve</summary>        
        /// <param name="curve">Curve</param>
        /// <param name="g">Graphics object</param>
        public void DrawControlPoints(ICurve curve, Graphics g)
        {
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            int leftIndex;
            int rightIndex;
            ComputeIndices(curve, out leftIndex, out rightIndex);

            if (curve.CurveInterpolation == InterpolationTypes.Linear)
            {
                for (int i = leftIndex; i <= rightIndex; i++)
                {
                    IControlPoint pt = points[i];
                    Vec2F cpt = m_canvas.GraphToClient(pt.X, pt.Y);
                    RectangleF pointRect = new RectangleF();
                    float halfPointSize = m_pointSize / 2;
                    pointRect.X = cpt.X - halfPointSize;
                    pointRect.Y = cpt.Y - halfPointSize;
                    pointRect.Width = m_pointSize;
                    pointRect.Height = m_pointSize;
                    if (pt.EditorData.SelectedRegion == PointSelectionRegions.Point)
                        g.FillRectangle(m_pointHiBrush, pointRect);
                    else
                        g.FillRectangle(m_pointBrush, pointRect);
                }
            }
            else
            {
                for (int i = leftIndex; i <= rightIndex; i++)
                {
                    CurveTangentTypes prevTanType = (i == 0) ? CurveTangentTypes.Flat :
                        points[i - 1].TangentOutType;
                    DrawControlPoint(prevTanType, points[i], g);
                }
            }
        }

        /// <summary>
        /// Sets the target surface</summary>
        /// <param name="control">Cartesian2dCanvas set</param>
        public void SetCartesian2dCanvas(Cartesian2dCanvas control)
        {
            if (m_canvas != null)
                throw new InvalidOperationException("curveControl is already been set");
            m_canvas = control;
        }

        /// <summary>
        /// Gets or sets the color for the tangent arrow</summary>
        public Color TangentColor
        {
            get { return m_tangentColor; }
            set { m_tangentColor = value; }
        }

        /// <summary>
        /// Gets or sets the brush to be used for the control points that are not selected.
        /// Can't be null. Setting the brush disposes of the current one.</summary>
        public Brush PointBrush
        {
            get { return m_pointBrush; }
            set
            {
                m_pointBrush.Dispose();
                m_pointBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the brush to be used for the selected control points.
        /// Can't be null. Setting the brush disposes of the current one.</summary>
        public Brush PointHighlightBrush
        {
            get { return m_pointHiBrush; }
            set
            {
                m_pointHiBrush.Dispose();
                m_pointHiBrush = value;
            }
        }

        /// <summary>
        /// Draws single control point</summary>
        /// <param name="prevTanType">Previous CurveTangentTypes</param>
        /// <param name="cp">Control point</param>
        /// <param name="g">Graphics object</param>
        private void DrawControlPoint(CurveTangentTypes prevTanType, IControlPoint cp, Graphics g)
        {
            Vec2F p = m_canvas.GraphToClient(cp.X, cp.Y);

            PointSelectionRegions region = cp.EditorData.SelectedRegion;

            if (region != PointSelectionRegions.None)
            {
                if (prevTanType != CurveTangentTypes.Stepped && prevTanType != CurveTangentTypes.SteppedNext)
                {
                    Vec2F tangIn = Vec2F.Normalize(m_canvas.GraphToClientTangent(cp.TangentIn));
                    tangIn.X = -tangIn.X;
                    tangIn.Y = -tangIn.Y;
                    DrawArrow(p, p + tangIn * m_tangentLength, g, m_tangentColor);
                }

                if (cp.TangentOutType != CurveTangentTypes.Stepped && cp.TangentOutType != CurveTangentTypes.SteppedNext)
                {
                    Vec2F tangOut = Vec2F.Normalize(m_canvas.GraphToClientTangent(cp.TangentOut));
                    DrawArrow(p, p + tangOut * m_tangentLength, g, m_tangentColor);
                }
            }

            RectangleF pointRect = new RectangleF();
            float halfPointSize = m_pointSize / 2;
            pointRect.X = p.X - halfPointSize;
            pointRect.Y = p.Y - halfPointSize;
            pointRect.Width = m_pointSize;
            pointRect.Height = m_pointSize;
            if (region == PointSelectionRegions.Point)
                g.FillRectangle(m_pointHiBrush, pointRect);
            else
                g.FillRectangle(m_pointBrush, pointRect);
        }

        /// <summary>
        /// Draws arrow</summary>
        /// <param name="start">Starting point</param>
        /// <param name="end">Ending point</param>
        /// <param name="g">Graphics object</param>
        /// <param name="color">Arrow color</param>
        private void DrawArrow(Vec2F start, Vec2F end, Graphics g, Color color)
        {
            m_tangentArrowHeadBrush.Color = color;
            m_tangentArrowLinePen.Color = color;
            g.DrawLine(m_tangentArrowLinePen, start, end);
            Vec2F pt = FinLength * Vec2F.Normalize(start - end);

            Vec2F leftFin = new Vec2F();
            Vec2F rightFin = new Vec2F();

            leftFin.X = (pt.X * cos - pt.Y * sin) + end.X;
            leftFin.Y = (pt.X * sin + pt.Y * cos) + end.Y;

            rightFin.X = (pt.X * cos + pt.Y * sin) + end.X;
            rightFin.Y = (pt.X * -sin + pt.Y * cos) + end.Y;

            s_arrowPts[0] = new PointF(rightFin.X, rightFin.Y);
            s_arrowPts[1] = new PointF(end.X, end.Y);
            s_arrowPts[2] = new PointF(leftFin.X, leftFin.Y);
            g.FillPolygon(m_tangentArrowHeadBrush, s_arrowPts);
        }

        /// <summary>
        /// Computes indices for pre-last and post-first points on the left and right of the
        /// viewing rectangle.
        /// Set lIndex to -1 and rIndex to -2 to indicate that curve is completely panned 
        /// either to left or right of the viewing rectangle.        
        /// This method is used by picking and rendering.</summary>
        /// <param name="curve">Curve</param>
        /// <param name="lIndex">Left index</param>
        /// <param name="rIndex">Right index</param>
        private void ComputeIndices(ICurve curve, out int lIndex, out int rIndex)
        {
            lIndex = -1;
            rIndex = -2;

            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (points.Count == 0)
                return;

            float leftgx = m_canvas.ClientToGraph(0.0f);
            float rightgx = m_canvas.ClientToGraph(m_canvas.ClientSize.Width);

            if (points[0].X >= rightgx)
                return;

            if (points[points.Count - 1].X <= leftgx)
                return;

            // find the index of the control point that 
            // comes before the first visible control-point from left.            
            for (int i = (points.Count - 1); i >= 0; i--)
            {
                IControlPoint cp = points[i];
                lIndex = i;
                if (cp.X < leftgx)
                    break;
            }

            // find the index of the control-point that comes after last visible control-point 
            // from right.            
            for (int i = lIndex; i < points.Count; i++)
            {
                IControlPoint cp = points[i];
                rIndex = i;
                if (cp.X > rightgx)
                    break;
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of resources</summary>
        public void Dispose()
        {
            m_pointBrush.Dispose();
            m_pointHiBrush.Dispose();
            m_tangentArrowLinePen.Dispose();
            m_tangentArrowHeadBrush.Dispose();
            m_infinityPen.Dispose();
            m_curvePen.Dispose();
        }

        #endregion

        private Cartesian2dCanvas m_canvas;
        private const float FinLength = 10.0f;
        private const float sin = 0.5f; // cached value             
        private const float cos = 0.8660254f; // cached value       

        private Color m_tangentColor = Color.DeepSkyBlue;
        private Brush m_pointBrush = new SolidBrush(Color.Black);
        private Brush m_pointHiBrush = new SolidBrush(Color.Red);

        private float m_pointSize = 5.0f;
        private float m_tangentLength = 40.0f;
        private int m_tessellation = 4; // in pixel

        private Pen m_tangentArrowLinePen = new Pen(Color.DarkCyan, 2);
        private SolidBrush m_tangentArrowHeadBrush = (SolidBrush)Brushes.DarkCyan;
        private Pen m_infinityPen = new Pen(Color.FromArgb(40, 40, 40)) { DashPattern = new float[] { 2, 2 } };
        private Pen m_curvePen = new Pen(Color.Black);
        private static readonly PointF[] s_arrowPts = new PointF[3];
    }
}
