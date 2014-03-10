//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A 3D Bezier Point</summary>
    public struct BezierPoint : IFormattable
    {
        /// <summary>
        /// Position of Bezier control point</summary>
        public Vec3F Position;

        /// <summary>
        /// Incoming tangent at control point</summary>
        public Vec3F Tangent1;

        /// <summary>
        /// Outgoing tangent at control point</summary>
        public Vec3F Tangent2;

        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.BezierPoint structure</summary>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D Bezier point</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary> Returns the string representation of this Scea.VectorMath.BezierPoint structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D Bezier point</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return Position.X.ToString("R") + ", " + Position.Y.ToString("R") + ", " + Position.Z.ToString("R") + ", " +
                    Tangent1.X.ToString("R") + ", " + Tangent1.Y.ToString("R") + ", " + Tangent1.Z.ToString("R") + ", " +
                    Tangent2.X.ToString("R") + ", " + Tangent2.Y.ToString("R") + ", " + Tangent2.Z.ToString("R");

            return String.Format
           (
                "({0}, {1}, {2}, {3},{4}, {5}, {6}, {7}, {8})",
                ((double)Position.X).ToString(format, formatProvider),
                ((double)Position.Y).ToString(format, formatProvider),
                ((double)Position.Z).ToString(format, formatProvider),
                ((double)Tangent1.X).ToString(format, formatProvider),
                ((double)Tangent1.Y).ToString(format, formatProvider),
                ((double)Tangent1.Z).ToString(format, formatProvider),
                ((double)Tangent2.X).ToString(format, formatProvider),
                ((double)Tangent2.Y).ToString(format, formatProvider),
                ((double)Tangent2.Z).ToString(format, formatProvider)
            );

        }

    }

    /// <summary>
    /// A 3D Bezier spline, made up of Bezier points</summary>
    public class BezierSpline : Collection<BezierPoint>, IFormattable
    {
        /// <summary>
        /// Gets the collection of piecewise Bezier curves</summary>
        public IList<BezierCurve> Curves
        {
            get
            {
                if (m_dirty)
                {
                    BuildCurves();
                    m_dirty = false;
                }

                return m_curves;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this spline is closed</summary>
        /// <value><c>True</c> iff this instance is closed</value>
        public bool IsClosed
        {
            get { return m_isClosed; }
            set { m_isClosed = value; }
        }

        #region Overrides

        /// <summary>
        /// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"></see></summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            m_dirty = true;
        }

        /// <summary>
        /// Inserts the BezierPoint</summary>
        /// <param name="index">Index</param>
        /// <param name="pt">Bezier point</param>
        protected override void InsertItem(int index, BezierPoint pt)
        {
            base.InsertItem(index, pt);
            m_dirty = true;
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"></see></summary>
        /// <param name="index">The zero-based index of the element to remove</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Index is less than zero.-or-index is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"></see></exception>
        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            m_dirty = true;
        }

        /// <summary>
        /// Sets the BezierPoint</summary>
        /// <param name="index">Index</param>
        /// <param name="pt">Bezier point</param>
        protected override void SetItem(int index, BezierPoint pt)
        {
            base.SetItem(index, pt);
            m_dirty = true;
        }

        #endregion

        private void BuildCurves()
        {
            m_curves.Clear();

            if (Count > 1)
            {
                if (Count == 2)
                    BuildInitialCurveFrom2Points();
                else
                {
                    Vec3F zeroVec = new Vec3F(0, 0, 0);
                    Vec3F[] tangents = CalcPointTangents();

                    for (int i = 1; i < Count; i++)
                    {
                        Vec3F chord = this[i].Position - this[i - 1].Position;
                        float segLen = chord.Length * 0.333f;

                        Vec3F[] points = new Vec3F[4];
                        points[0] = this[i - 1].Position;
                        points[3] = this[i].Position;

                        // calc points[1]
                        if (this[i - 1].Tangent2 != zeroVec)
                        {
                            points[1] = this[i - 1].Position + this[i - 1].Tangent2;
                        }
                        else
                        {
                            Vec3F tangent = tangents[i - 1];
                            if (Vec3F.Dot(chord, tangent) < 0)
                                tangent = -tangent;

                            points[1] = this[i - 1].Position + (tangent * segLen);
                        }

                        // calc points[2]
                        if (this[i].Tangent1 != zeroVec)
                        {
                            points[2] = this[i].Position + this[i].Tangent1;
                        }
                        else
                        {
                            Vec3F tangent = tangents[i];
                            if (Vec3F.Dot(-chord, tangent) < 0)
                                tangent = -tangent;

                            points[2] = this[i].Position + (tangent * segLen);
                        }

                        BezierCurve curve = new BezierCurve(points);
                        m_curves.Add(curve);
                    }

                    // Calculate last curve if is closed
                    if (m_isClosed)
                    {
                        Vec3F[] points = new Vec3F[4];
                        points[0] = this[Count - 1].Position;
                        points[3] = this[0].Position;
                        float tanLen = (points[3] - points[0]).Length / 3.0f;

                        Vec3F v = m_curves[m_curves.Count - 1].ControlPoints[2] - points[0];
                        v = v / v.Length;
                        points[1] = points[0] - (v * tanLen);

                        v = m_curves[0].ControlPoints[1] - points[3];
                        v = v / v.Length;
                        points[2] = points[3] - (v * tanLen);

                        BezierCurve curve = new BezierCurve(points);
                        m_curves.Add(curve);
                    }
                }
            }
        }

        private void BuildInitialCurveFrom2Points()
        {
            Vec3F[] points = new Vec3F[4];
            points[0] = this[0].Position;
            points[3] = this[1].Position;
            points[1] = points[0] + (points[3] - points[0]) * 0.333f;
            points[2] = points[0] + (points[3] - points[0]) * 0.666f;

            m_curves.Add(new BezierCurve(points));
        }

        private Vec3F CalcTangent(Vec3F p1, Vec3F p2, Vec3F p3)
        {
            Vec3F v1 = (p1 - p2) / (p1 - p2).Length;
            Vec3F v2 = (p3 - p2) / (p3 - p2).Length;
            Vec3F bisector = v1 + v2;
            Vec3F normal = Vec3F.Cross(v1, v2);
            if (normal.Length < 0.1e-5f) // 3 points  colinear
            {
                return v2;
            }
            Vec3F tangent = Vec3F.Cross(normal, bisector);
            tangent = tangent / tangent.Length;
            return tangent;
        }

        private Vec3F CalcEndTangents(Vec3F p1, Vec3F p2, Vec3F p2Tangent)
        {
            Vec3F v1 = p1 - p2;
            float v1Len = v1.Length;
            Vec3F u1 = v1 / v1Len;

            // Make sure that v1 and p2Tangent are facing the same way
            if (Vec3F.Dot(v1, p2Tangent) < 0)
                p2Tangent = -p2Tangent;

            // calc t for which the tangent vector t*p2Tangent is projected on 1/2 of v1
            float t = (0.5f * v1Len) / Vec3F.Dot(p2Tangent, u1);
            Vec3F p3 = p2 + (p2Tangent * t);
            Vec3F tangent = (p3 - p1) / (p3 - p1).Length;
            return tangent;
        }

        private Vec3F[] CalcPointTangents()
        {
            int n = Count;
            Vec3F[] tangents = new Vec3F[n];

            for (int i = 1; i < n - 1; i++)
            {
                tangents[i] = CalcTangent(this[i - 1].Position, this[i].Position, this[i + 1].Position);
            }

            if (m_isClosed)
            {
                tangents[0] = CalcTangent(this[n - 1].Position, this[0].Position, this[1].Position);
                tangents[n - 1] = CalcTangent(this[n - 2].Position, this[n - 1].Position, this[0].Position);
            }
            else
            {
                tangents[0] = CalcEndTangents(this[0].Position, this[1].Position, tangents[1]);
                tangents[n - 1] = CalcEndTangents(this[n - 2].Position, this[n - 1].Position, tangents[n - 2]);
            }

            return tangents;
        }


        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.BezierSpline structure</summary>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D Bezier spline</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary> Returns the string representation of this Scea.VectorMath.BezierSpline structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D Bezier spline</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Count.ToString("D", formatProvider));
            for (int i = 0; i < Count; ++i)
            {
                sb.Append(", ");
                sb.Append(this[i].ToString(format, formatProvider));//Convert each BezierPoint 
            }
            return sb.ToString();

        }


        private readonly IList<BezierCurve> m_curves = new List<BezierCurve>();
        private bool m_isClosed = true;
        private bool m_dirty = true;
    }
}
