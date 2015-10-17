//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.CurveEditing
{
    /// <summary>
    /// Utility class used by curve control and client applications</summary>
    public static class CurveUtils
    {
        /// <summary>
        /// Validates the curve</summary>
        /// <param name="curve">Curve to validate</param>
        /// <returns><c>True</c> if curve is valid</returns>
        public static bool IsValid(ICurve curve)
        {
            if (curve == null)
                return false;
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (points.Count < 2)
                return true;

            int count = points.Count;
            IControlPoint prev = points[0];
            for (int i = 1; i < count; i++)
            {
                IControlPoint curPoint = points[i];
                if ((curPoint.X - prev.X) < s_epsilone)
                    return false;
                prev = curPoint;
            }
            return true;
        }

        /// <summary>
        /// Creates a curve evaluator for a curve</summary>
        /// <param name="curve">Curve</param>
        /// <returns>Curve evaluator</returns>
        /// <remarks>A curve evaluator calculates y-coordinates from x-coordinates using appropriate interpolation for a curve</remarks>
        public static ICurveEvaluator CreateCurveEvaluator(ICurve curve)
        {
            ICurveEvaluator cv = null;
            if (curve.CurveInterpolation == InterpolationTypes.Linear)
                cv = new LinearCurveEvaluator(curve);
            else if (curve.CurveInterpolation == InterpolationTypes.Hermite)
                cv = new HermiteCurveEvaluator(curve);
            else
                throw new NotImplementedException("CurveEvaluator not implement for "
                    + curve.CurveInterpolation);
            return cv;
        }

        /// <summary>
        /// Offsets curve</summary>
        /// <param name="curve">Curve</param>
        /// <param name="x">X offset</param>
        /// <param name="y">Y offset</param>
        public static void OffsetCurve(ICurve curve, float x, float y)
        {
            foreach (IControlPoint cpt in curve.ControlPoints)
            {
                cpt.X += x;
                cpt.Y += y;
            }
        }

        /// <summary>
        /// Forces the distance between consecutive x values of the control points of the given curve to be at least 
        /// an epsilon margin (s_epsilone field). The first control point of the curve is not moved, 
        /// but later control points may have their x values increased in order to achieve this minimum distance. 
        /// The points are assumed to be increasing in x, from the first point to the last. 
        /// This method does nothing if the curve has less than two control points.</summary>
        /// <param name="curve">Curve</param>
        public static void ForceMinDistance(ICurve curve)
        {
            ForceMinDistance(curve, s_epsilone);
        }

        /// <summary>
        /// Forces the distance between consecutive x values of the control points of the given curve to be at least 
        /// a specified distance. The first control point of the curve is not moved, 
        /// but later control points may have their x values increased in order to achieve this minimum distance. 
        /// The points are assumed to be increasing in x, from the first point to the last. 
        /// This method does nothing if the curve has less than two control points.</summary>
        /// <param name="curve">Curve</param>
        /// <param name="dist">Distance</param>
        public static void ForceMinDistance(ICurve curve, float dist)
        {
            if (curve == null)
                return;
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (points.Count < 2)
                return;

            int lastIndex = points.Count - 1;
            for (int i = 0; i < lastIndex; i++)
            {
                if ((points[i + 1].X - points[i].X) < dist)
                    points[i + 1].X = points[i].X + dist;                 
            }
        }

        /// <summary>
        /// Compute tangents for all control points in a given curve</summary>
        /// <param name="curve">Curve</param>
        public static void ComputeTangent(ICurve curve)
        {
            if (curve == null)
                return;
            
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (points.Count == 0)
                return;
            if (points.Count == 1)
            {
                Vec2F tan = new Vec2F(1, 0);
                IControlPoint pt = points[0];
                if (pt.TangentIn != tan)
                    pt.TangentIn = tan;
                if (pt.TangentOut != tan)
                    pt.TangentOut = tan;
                return;                                
            }

            for (int i = 0; i < points.Count; i++)
            {
                ComputeTangentIn(curve, i);
                ComputeTangentOut(curve, i);
            }
        }
      
        /// <summary>
        /// Compute tangent out for the control point at an index</summary>
        /// <param name="curve">Curve object to be modified</param>
        /// <param name="index">0-based index into the curve's ControlPoints property. If the index is out of
        /// bounds, this method does nothing.</param>
        public static void ComputeTangentOut(ICurve curve, int index)
        {
            if (curve == null)
                return;
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (index < 0 || index >= points.Count)
                return;

            IControlPoint curP = points[index];
            if (curP.TangentOutType == CurveTangentTypes.Fixed)
                return;

            Vec2F tan = new Vec2F(0, 0);
            int lastIndex = points.Count - 1;
            IControlPoint prevP = (index > 0) ? points[index - 1] : null;
            IControlPoint nextP = (index < lastIndex) ? points[index + 1] : null;
                       
            if ((curP.TangentOutType == CurveTangentTypes.Clamped) && (nextP != null))
            {
                float ny = nextP.Y - curP.Y;
                if (ny < 0.0f) ny = -ny;
                float py = (prevP == null ? ny : prevP.Y - curP.Y);
                if (py < 0.0f) py = -py;
                if ((ny <= 0.05f) || (py <= 0.05f))
                {
                    curP.TangentOutType = CurveTangentTypes.Flat;                    
                }
            }

                       
            switch (curP.TangentOutType)
            {
                case CurveTangentTypes.Clamped:
                case CurveTangentTypes.Spline:
                    if (curP.TangentOutType == CurveTangentTypes.Clamped)
                        curP.TangentOutType = CurveTangentTypes.Spline;

                    /* Maya 2.0 smooth tangents */
                    if ((prevP == null) && (nextP != null))
                    {
                        tan.X = nextP.X - curP.X;
                        tan.Y = nextP.Y - curP.Y;
                    }
                    else if ((prevP != null) && (nextP == null))
                    {
                        tan.X = curP.X - prevP.X;
                        tan.Y = curP.Y - prevP.Y;
                    }
                    else if ((prevP != null) && (nextP != null))
                    {
                        float dx = nextP.X - prevP.X;
                        float dy = 0.0f;
                        if (dx < s_epsilone)
                        {
                            dy = MaxTan;
                        }
                        else
                        {
                            dy = (nextP.Y - prevP.Y) / dx;
                        }
                        tan.X = nextP.X - curP.X;
                        tan.Y = dy * tan.X;
                    }
                    else
                    {
                        tan = new Vec2F(1, 0);
                    }
                    break;
                case CurveTangentTypes.Linear:
                    if (nextP == null)
                    {
                        tan = new Vec2F(1, 0);
                    }
                    else
                    {
                        tan = new Vec2F(nextP.X - curP.X, nextP.Y - curP.Y);
                    }
                    break;                
                case CurveTangentTypes.Stepped:
                    tan = new Vec2F(0, 0);                                            
                    break;
                case CurveTangentTypes.SteppedNext:
                    tan = new Vec2F(float.MaxValue, float.MaxValue);                    
                    break;
                case CurveTangentTypes.Flat:
                    if (nextP == null)
                    {
                        float x = (prevP == null) ? 0 : ( curP.X - prevP.X);
                        tan = new Vec2F(x, 0);
                    }
                    else
                    {
                        tan = new Vec2F(nextP.X - curP.X, 0);
                    }
                    break;                
                default:
                    throw new NotImplementedException(curP.TangentOutType.ToString());
                    
            }

            tan = EnsureValidTangent(tan, false);            
            if (curP.TangentOut != tan)
                curP.TangentOut = tan;
        }

        /// <summary>
        /// Compute tangents for the given curve</summary>
        /// <param name="curve">Curve object to be modified</param>
        /// <param name="index">0-based index into the curve's ControlPoints. If the index is out of
        /// bound, this method does nothing</param>
        public static void ComputeTangentIn(ICurve curve, int index)
        {
            if (curve == null)
                return;
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (index < 0 || index >= points.Count)
                return;
            
            IControlPoint curP = points[index];
            if (curP.TangentInType == CurveTangentTypes.Fixed)
                return;

            Vec2F tan = new Vec2F(0, 0);
            int lastIndex = points.Count - 1;
            IControlPoint prevP = (index > 0) ? points[index - 1] : null;
            IControlPoint nextP = (index < lastIndex) ? points[index + 1] : null;
           
            if ((curP.TangentInType == CurveTangentTypes.Clamped) && (prevP != null))
            {
                float py = prevP.Y - curP.Y;
                if (py < 0.0f) py = -py;
                float ny = (nextP == null ? py : nextP.Y - curP.Y);
                if (ny < 0.0f) ny = -ny;
                if ((ny <= 0.05f) || (py <= 0.05f))
                {
                    curP.TangentInType = CurveTangentTypes.Flat;
                }                
            }            
            switch (curP.TangentInType)
            {
                case CurveTangentTypes.Clamped:
                case CurveTangentTypes.Spline:
                    if (curP.TangentInType == CurveTangentTypes.Clamped)
                    {                      
                        curP.TangentInType = CurveTangentTypes.Spline;
                    }
                    
                    /* Maya 2.0 smooth tangents */
                    if ((prevP == null) && (nextP != null))
                    {
                        tan.X = nextP.X - curP.X;
                        tan.Y = nextP.Y - curP.Y;
                    }
                    else if ((prevP != null) && (nextP == null))
                    {
                        tan.X = curP.X - prevP.X;
                        tan.Y = curP.Y - prevP.Y;
                    }
                    else if ((prevP != null) && (nextP != null))
                    {
                        float dx = nextP.X - prevP.X;
                        float dy = 0.0f;
                        if (dx < s_epsilone)
                        {
                            dy = MaxTan;
                        }
                        else
                        {
                            dy = (nextP.Y - prevP.Y) / dx;
                        }
                        tan.X = curP.X - prevP.X;
                        tan.Y = dy * tan.X;
                    }
                    else
                        tan = new Vec2F(1, 0);                   
                    break;
                case CurveTangentTypes.Linear:
                    if (prevP == null)
                    {
                        tan = new Vec2F(1, 0);
                    }
                    else
                    {
                        tan = new Vec2F(curP.X - prevP.X, curP.Y - prevP.Y);
                    }
                    break;                                
                case CurveTangentTypes.Stepped:
                    tan = new Vec2F(0, 0);                    
                    break;
                case CurveTangentTypes.SteppedNext:
                    tan = new Vec2F(float.MaxValue, float.MaxValue);                    
                    break;
                case CurveTangentTypes.Flat:
                    if (prevP == null)
                    {
                        float x = (nextP == null) ? 0 : (nextP.X - curP.X);
                        tan = new Vec2F(x, 0);
                    }
                    else
                    {
                        tan = new Vec2F(curP.X - prevP.X, 0);
                    }

                    break;
                case CurveTangentTypes.Fixed:
                    break;              
                default:
                    throw new NotImplementedException(curP.TangentInType.ToString());                    
            }
            
            tan = EnsureValidTangent(tan,false);            
            if (curP.TangentIn != tan)
                curP.TangentIn = tan;
        }

        /// <summary>
        /// Determines if given control point's x coordinate is in sorted order</summary>
        /// <param name="cp">Control point</param>
        /// <returns><c>True</c> if control point's x coordinate is in sorted order</returns>
        public static bool IsSorted(IControlPoint cp)
        {
            bool valid = true;            
            ICurve curve = cp.Parent;            
            int index = curve.ControlPoints.IndexOf(cp);
            if (index == -1)
                throw new ArgumentException("cp not found in parent curve");
            

            int lastIndex = curve.ControlPoints.Count - 1;
            if (index < lastIndex)
            {
                IControlPoint nextcp = curve.ControlPoints[index + 1];
                if (cp.X > nextcp.X)
                {
                    valid = false;                    
                }
            }
            if (valid && index > 0)
            {
                IControlPoint prevCp = curve.ControlPoints[index - 1];
                if (cp.X < prevCp.X )
                {
                    valid = false;
                }
            }
            return valid;
        }


        private static List<IControlPoint> s_points = new List<IControlPoint>();
        /// <summary>
        /// Sort control points along x axis</summary>       
        /// <param name="curve">Curve whose control points are sorted</param>
        public static void Sort(ICurve curve)
        {
            if (curve == null) return;

            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (points.Count < 2)
                return;

            try
            {
                s_points.Clear();
                s_points.AddRange(points);

                // note about "insertion sort"
                // this type of sort is most efficient for Substantially sorted data
                // in this case the data is Substantially sorted, only few elements need fixing.
                for (int i = 1; i < s_points.Count; i++)
                {
                    IControlPoint val = s_points[i];
                    int j = i - 1;
                    while (j >= 0 && s_points[j].X > val.X)
                    {
                        s_points[j + 1] = s_points[j];
                        j--;
                    }
                    s_points[j + 1] = val;
                }

                for (int i = 0; i < s_points.Count; i++)
                {
                    IControlPoint scpt = s_points[i];
                    IControlPoint cpt = points[i];
                    if (scpt != cpt)
                    {                        
                        curve.InsertControlPoint(i, scpt);
                    }
                }
            }
            finally
            {
                s_points.Clear();
            }            
        }

        /// <summary>
        /// Create and add new control point to the given curve at a given Vec2F gpt</summary>
        /// <param name="curve">Curve to modify</param>
        /// <param name="gpt">Location on the x-axis at which to add a control point. Whether
        /// or not the y-coordinate is used depends on 'insert' parameter.</param>
        /// <param name="insert">If true, the y-value is inserted on the existing curve, using
        /// curve.Evaluate(gpt.X). If false, the y-value comes from gpt.Y.</param>
        /// <param name="computeTangent">Whether ComputeTangent() should be called</param>
        public static void AddControlPoint(ICurve curve, Vec2F gpt, bool insert, bool computeTangent)
        {
            if (curve == null)
                throw new ArgumentNullException("curve");

            // add/insert new control point to the given curve. Using the following rules.
            // if adding then add the point at gpt and set is tangent type to spline.
            // if inserting then add the point at (gpt.x,curve.eval(gpt.x)) and compute its tangent at the point.            
            // recompute tangents for the curve.
            // find insersion index.
            int index = GetValidInsertionIndex(curve, gpt.X);
            if (index >= 0)
            {
                IControlPoint p = curve.CreateControlPoint();
                p.EditorData.SelectedRegion = PointSelectionRegions.None;
                if (insert)
                {
                    p.X = gpt.X;
                    ICurveEvaluator cv = CurveUtils.CreateCurveEvaluator(curve);
                    
                    float prevY = cv.Evaluate(gpt.X - s_epsilone);
                    float nextY = cv.Evaluate(gpt.X + s_epsilone);
                    p.Y = cv.Evaluate(gpt.X);
                    p.TangentInType = CurveTangentTypes.Fixed;
                    p.TangentOutType = CurveTangentTypes.Fixed;
                    Vec2F tanIn = new Vec2F(s_epsilone, p.Y - prevY);
                    tanIn.Normalize();
                    Vec2F tanOut = new Vec2F(s_epsilone, nextY - p.Y);
                    tanOut.Normalize();
                    p.TangentIn = tanIn;
                    p.TangentOut = tanOut;                                                            
                }
                else
                {
                    p.X = gpt.X;
                    p.Y = gpt.Y;
                    p.TangentInType = CurveTangentTypes.Spline;
                    p.TangentOutType = CurveTangentTypes.Spline;
                }               
                curve.InsertControlPoint(index, p);
                if (computeTangent)
                    ComputeTangent(curve);
            }
        }

        /// <summary>
        /// Create and add new control point to the given curve at a given Vec2F gpt</summary>        
        /// <param name="curve">Ccurve to modify</param>
        /// <param name="gpt">Location on the x-axis at which to add a control point. Whether
        /// or not the y-coordinate is used depends on 'insert' parameter.</param>
        /// <param name="insert">If true, the y-value is inserted on the existing curve, using
        /// curve.Evaluate(gpt.X). If false, the y-value comes from gpt.Y.</param>
        public static void AddControlPoint(ICurve curve, Vec2F gpt, bool insert)
        {
            AddControlPoint(curve, gpt, insert, true);
        }
      
        /// <summary>
        /// Snaps float value x to y</summary>
        /// <param name="x">X-coordinate</param>
        /// <param name="y">Y-coordinate</param>
        /// <returns>Snapped y-coordinate</returns>
        public static float SnapTo(float x, float y)
        {
            double f = Math.Round(x / y);
            return (float)(f * y);
        }

        /// <summary>
        /// Gets valid insertion index at x for curve</summary>
        /// <param name="curve">Curve</param>
        /// <param name="x">X coordinate</param>
        /// <returns>Valid insertion index at x for curve</returns>
        public static int GetValidInsertionIndex(ICurve curve, float x)
        {
            ReadOnlyCollection<IControlPoint> points = curve.ControlPoints;
            if (points.Count == 0)
            {
                return 0;
            }

            IControlPoint last = points[points.Count - 1];
            if ((x - last.X) > s_epsilone)
                return (points.Count);

            for (int i = 0; i < points.Count; i++)
            {
                IControlPoint p = points[i];
                if ((x - p.X) < -s_epsilone)
                    return i;
                if ((x - p.X) > s_epsilone)
                    continue;
                else
                    return -1;
            }
            return points.Count;
        }
      
        /// <summary>
        /// Gets or sets epsilone</summary>
        public static float Epsilone
        {
            get { return s_epsilone; }
            set
            {
                if (value < float.Epsilon)
                    throw new ArgumentOutOfRangeException("value");
                s_epsilone = value;
            }
        }

        private static Vec2F EnsureValidTangent(Vec2F tan, bool isWeighted)
        {
            Vec2F result = tan;

            if (result.X == float.MaxValue && result.Y == float.MaxValue)
                return result;

            if (result.X < 0.0f)
            {
                result.X = 0.0f;
            }

            if (isWeighted)
                return result;

            float length = result.Length;

            if (length != 0.0f)
            {
                result.X = result.X / length;
                result.Y = result.Y / length;
            }
            if ((result.X == 0.0f) && (result.Y != 0.0f))
            {
                result.X = s_epsilone;
                result.Y = ((result.Y < 0.0f) ? -1.0f : 1.0f) * (result.X * MaxTan);
            }
            return result;
        }

        private static float s_epsilone = 0.001f;
        private static float MaxTan = 1000000.0f; //5729577.9485111479f from Maya animEngine.c didn't work for Guerrilla for some reason
    }
}
