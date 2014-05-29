using System;
using System.Collections.ObjectModel;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Curve evaluator methods for linear curves</summary>
    public class LinearCurveEvaluator : ICurveEvaluator
    {
        #region ctor
        /// <summary>
        /// Default constructor</summary>
        /// <param name="curve">Curve for which evaluator created</param>       
        public LinearCurveEvaluator(ICurve curve)
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
        /// <returns>Y-coordinate on curve corresponding to given x-coordinate</returns>
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
                    IControlPoint secondPt = m_points[1];
                    float slope = (secondPt.Y - firstPt.Y) / (secondPt.X - firstPt.X);
                    return (firstPt.Y - slope * (firstPt.X - x));
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
                    IControlPoint beforeLastPt = m_points[m_points.Count - 2];
                    float slope = (lastPt.Y - beforeLastPt.Y) / (lastPt.X - beforeLastPt.X);
                    return (lastPt.Y + slope * ( x-lastPt.X));
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

            if (m_lastP1 != p1)
            {
                // compute slope and y intercept
                m = (p2.Y - p1.Y) / (p2.X - p1.X);
                b = p1.Y - m * p1.X;
                m_lastP1 = p1;
            }
            // y = mx + b  
            // offset is computed from post and pre infinities.
            return (m * x + b + offset);
                                   
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

       
             
        #endregion

        #region private fields

        // m and b in Slope–intercept form 
        // y = mx + b
        private float m; // slope 
        private float b; // y-intercept
        private readonly ICurve m_curve;        
        private ReadOnlyCollection<IControlPoint> m_points;
        private IControlPoint m_lastP1;       
        #endregion
    }
}
