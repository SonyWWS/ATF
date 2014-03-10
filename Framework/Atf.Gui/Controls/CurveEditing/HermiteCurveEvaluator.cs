using System;
using System.Collections.ObjectModel;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Evaluator for hermite curve.
    /// A curve evaluator calculates y-coordinates from x-coordinates using appropriate interpolation for a curve</summary>
    public class HermiteCurveEvaluator : ICurveEvaluator
    {
        #region ctor
        /// <summary>
        /// Default constructor</summary>
        /// <param name="curve">Curve for which evaluator created</param>
        public HermiteCurveEvaluator(ICurve curve)
        {
            if (curve == null)
                throw new ArgumentNullException("curve");
            m_curve = curve;            
            Reset();
        }
        #endregion

        #region public methods
        /// <summary>        
        /// Resets the curve. Call this whenever curve changes.</summary>
        public void Reset()
        {
            m_lastP1 = null;
            m_points = m_curve.ControlPoints;
            
        }

        /// <summary>
        /// Evaluates point on curve</summary>
        /// <param name="x">X-coordinate for which y-coordinate is calculated</param>
        /// <remarks>Calculates y-coordinate from x-coordinate using appropriate interpolation for a curve</remarks>
        public float Evaluate(float x)
        {
            if (m_points.Count == 0)
                return 0.0f;

            if (m_points.Count == 1)
                return m_points[0].Y;

            IControlPoint firstPt = m_points[0];
            IControlPoint lastPt = m_points[m_points.Count - 1];
            float offset = 0.0f;
            if (x < firstPt.X) // pre-infiniy.
            {
                CurveLoopTypes loop = m_curve.PreInfinity;
                if (loop == CurveLoopTypes.Constant)
                {
                    return firstPt.Y;
                }
                if (loop == CurveLoopTypes.Linear)
                {
                    return (firstPt.Y - ((firstPt.TangentIn.Y / firstPt.TangentIn.X) * (firstPt.X - x)));
                }                
                float firstX = firstPt.X;
                float lastX = lastPt.X;
                float rangeX = lastX - firstX;
                float numcycle = (int)((firstX - x) / rangeX);
                float nx =  ((firstX - x) - numcycle * rangeX);
                if (loop == CurveLoopTypes.Cycle)
                {
                    x = lastX-nx;
                }
                else if(loop == CurveLoopTypes.CycleWithOffset)
                {
                    x = lastX-nx;
                    offset = -((numcycle+1) * (lastPt.Y - firstPt.Y));
                    
                }
                else if (loop == CurveLoopTypes.Oscillate)
                {
                    x = (((int)numcycle & 1) == 0) ? firstX + nx : lastX - nx;
                }
               
            }
            else if (x > lastPt.X) // post-infinity.
            {
                CurveLoopTypes loop = m_curve.PostInfinity;
                if (loop == CurveLoopTypes.Constant)
                {
                    return lastPt.Y;
                }
                if (loop == CurveLoopTypes.Linear)
                {
                    return (lastPt.Y + ((lastPt.TangentOut.Y / lastPt.TangentOut.X) * ( x-lastPt.X)));
                }

                float firstX = firstPt.X;
                float lastX = lastPt.X;
                float rangeX = lastX - firstX;
                float numcycle = (int)((x-lastX) / rangeX);
                float nx =  (x - lastX) - numcycle * rangeX;
                if (loop == CurveLoopTypes.Cycle)
                {
                    x = firstX + nx;
                }
                else if(loop == CurveLoopTypes.CycleWithOffset)
                {
                    x = firstX + nx;
                    offset = (numcycle+1) * (lastPt.Y - firstPt.Y);
                }
                else if (loop == CurveLoopTypes.Oscillate)
                {
                    x = (((int)numcycle & 1)==0)? lastX - nx : firstX + nx;
                }
            }
                              
            bool exactMatch;
            int index = FindIndex(x, out exactMatch);
            if (exactMatch)
                return m_points[index].Y + offset;

            IControlPoint p1 = m_points[index];
            IControlPoint p2 = m_points[index + 1];

            if (p1.TangentOut.X == 0 && p1.TangentOut.Y == 0)
            {// step
                return p1.Y + offset;
            }
            else if (p1.TangentOut.X == float.MaxValue && p1.TangentOut.Y == float.MaxValue)
            {// step-next
                return p2.Y + offset;
            }
            else
            {// hermite eval.                
                if (m_lastP1 != p1)
                {
                    // compute coeff.
                    ComputeHermiteCoeff(p1, p2, m_coeff);
                    m_lastP1 = p1;
                }
                return CubicPolyEval(p1.X, x, m_coeff) + offset;
            }                       
        }
        #endregion

        #region private helper methods
        /// <summary>
        /// Finds control points at x coordinate or prior to it</summary>        
        private int FindIndex(float x, out bool exactMatch)
        {
            exactMatch = false;
            int low = 0;
            int high = m_points.Count - 1;            
            do
            {
                int mid = (low + high) >> 1;
                if (x < m_points[mid].X)
                {
                    high = mid - 1;
                }
                else if (x > m_points[mid].X)
                {
                    low = mid + 1;
                }
                else
                {
                    exactMatch = true;
                    return mid;
                }                
            } while (low <= high);            
            return high;
        }

        private float CubicPolyEval(float x0, float x, float[] Coeff)
        {
            float t = x - x0; // usually t = (x-x0)/(x1-x0), but coeff has extra 1/(x1-x0) factor.
            return (t * (t * (t * Coeff[0] + Coeff[1]) + Coeff[2]) + Coeff[3]);
        }

        private void ComputeHermiteCoeff(IControlPoint p1, IControlPoint p2, float[] Coeff)
        {
            Vec2F t1 = new Vec2F(p1.TangentOut.X, p1.TangentOut.Y);
            Vec2F t2 = new Vec2F(p2.TangentIn.X, p2.TangentIn.Y);

            float m1 = 0.0f;
            if (t1.X != 0.0f)
            {
                m1 = t1.Y / t1.X;
            }

            float m2 = 0.0f;
            if (t2.X != 0.0f)
            {
                m2 = t2.Y / t2.X;
            }

            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float length = 1.0f / (dx * dx);
            float d1 = dx * m1;
            float d2 = dx * m2;

            Coeff[0] = (d1 + d2 - dy - dy) * length / dx;
            Coeff[1] = (dy + dy + dy - d1 - d1 - d2) * length;
            Coeff[2] = m1;
            Coeff[3] = p1.Y;
        }
             
        #endregion

        #region private fields
        private readonly ICurve m_curve;
        private readonly float[] m_coeff = new float[4];       
        private ReadOnlyCollection<IControlPoint> m_points;
        private IControlPoint m_lastP1;
        #endregion
    }
}
