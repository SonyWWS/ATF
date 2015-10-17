//Copyright � 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Class representing a 2D Bezier curve</summary>
    public class BezierCurve2F : IFormattable
    {
        /// <summary>
        /// First control point (endpoint)</summary>
        public Vec2F P1;

        /// <summary>
        /// Second control point (tangent)</summary>
        public Vec2F P2;

        /// <summary>
        /// Third control point (tangent)</summary>
        public Vec2F P3;

        /// <summary>
        /// Fourth control point (endpoint)</summary>
        public Vec2F P4;

        /// <summary>
        /// Constructs curve from 4 control points</summary>
        /// <param name="p1">First control point (endpoint)</param>
        /// <param name="p2">Second control point (tangent)</param>
        /// <param name="p3">Third control point (tangent)</param>
        /// <param name="p4">Fourth control point (endpoint)</param>
        public BezierCurve2F(PointF p1, PointF p2, PointF p3, PointF p4)
            : this(
                new Vec2F(p1.X, p1.Y),
                new Vec2F(p2.X, p2.Y),
                new Vec2F(p3.X, p3.Y),
                new Vec2F(p4.X, p4.Y))
        {
        }

        /// <summary>
        /// Constructs curve from 4 control points as vectors</summary>
        /// <param name="p1">First control point (endpoint)</param>
        /// <param name="p2">Second control point (tangent)</param>
        /// <param name="p3">Third control point (tangent)</param>
        /// <param name="p4">Fourth control point (endpoint)</param>
        public BezierCurve2F(Vec2F p1, Vec2F p2, Vec2F p3, Vec2F p4)
        {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            P4 = p4;
        }

        /// <summary>
        /// Gets the flatness, or maximum distance of the control points to
        /// the segment connecting the curve endpoints</summary>
        public float Flatness
        {
            get
            {
                Seg2F seg = new Seg2F(P1, P4);
                float d1 = Seg2F.DistanceToSegment(seg, P2);
                float d2 = Seg2F.DistanceToSegment(seg, P3);
                return Math.Max(d1, d2);
            }
        }

        /// <summary>
        /// Evaluates the curve on parameter t [0,1]</summary>
        /// <param name="t">Parameter</param>
        /// <returns>Point on the curve at the given parameter</returns>
        public Vec2F Evaluate(float t)
        {
            // use de Casteljau constuction

            float oneMinusT = 1.0f - t;

            Vec2F s11 = P1 * oneMinusT + P2 * t;
            Vec2F s12 = P2 * oneMinusT + P3 * t;
            Vec2F s13 = P3 * oneMinusT + P4 * t;

            Vec2F s21 = s11 * oneMinusT + s12 * t;
            Vec2F s22 = s12 * oneMinusT + s13 * t;

            Vec2F s31 = s21 * oneMinusT + s22 * t;

            return s31;
        }

        /// <summary>
        /// Subdivides the Bezier curve into two equivalent Bezier curves</summary>
        /// <param name="t">Parameter of subdivision point</param>
        /// <param name="left">Left or "less than or equal to t" parameter side</param>
        /// <param name="right">Right or "greater than or equal to t" parameter side</param>
        public void Subdivide(float t, out BezierCurve2F left, out BezierCurve2F right)
        {
            // use de Casteljau constuction

            float oneMinusT = 1.0f - t;

            Vec2F s11 = P1 * oneMinusT + P2 * t;
            Vec2F s12 = P2 * oneMinusT + P3 * t;
            Vec2F s13 = P3 * oneMinusT + P4 * t;

            Vec2F s21 = s11 * oneMinusT + s12 * t;
            Vec2F s22 = s12 * oneMinusT + s13 * t;

            Vec2F s31 = s21 * oneMinusT + s22 * t;

            left = new BezierCurve2F(P1, s11, s21, s31);
            right = new BezierCurve2F(s31, s22, s13, P4);
        }

        /// <summary>
        /// Picks the specified curve</summary>
        /// <param name="curve">Curve</param>
        /// <param name="p">Picking point</param>
        /// <param name="tolerance">Pick tolerance</param>
        /// <param name="hitPoint">Hit point</param>
        /// <returns><c>True</c> if curve found; false otherwise</returns>
        public static bool Pick(BezierCurve2F curve, Vec2F p, float tolerance, ref Vec2F hitPoint)
        {
            Queue<BezierCurve2F> curves = new Queue<BezierCurve2F>();
            curves.Enqueue(curve);

            float dMin = float.MaxValue;
            Vec2F closestPoint = new Vec2F();
            while (curves.Count > 0)
            {
                BezierCurve2F current = curves.Dequeue();

                // project p onto segment connecting curve endpoints
                Seg2F seg = new Seg2F(current.P1, current.P4);
                Vec2F projection = Seg2F.Project(seg, p);
                float d = Vec2F.Distance(p, projection);

                // reject - point not near enough to segment, expanded by curve "thickness"
                float flatness = current.Flatness;
                if (d - flatness > tolerance)
                    continue;

                // accept - point within tolerance of curve
                if (flatness <= tolerance)
                {
                    if (d < dMin)
                    {
                        dMin = d;
                        closestPoint = projection;
                    }
                }
                else
                {
                    BezierCurve2F left, right;
                    current.Subdivide(0.5f, out left, out right);
                    curves.Enqueue(left);
                    curves.Enqueue(right);
                }
            }

            if (dMin < tolerance)
            {
                hitPoint = closestPoint;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a string representation of this object for GUIs. For persistence, use
        /// ToString("R", CultureInfo.InvariantCulture).</summary>
        /// <returns>String representation of object</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        #region IFormattable
        /// <summary>
        /// Returns the string representation of this object</summary>
        /// <param name="format">Optional standard numeric format string for a floating point number.
        /// If null, "R" is used for round-trip support in case the string is persisted.
        /// http://msdn.microsoft.com/en-us/library/vstudio/dwhawy9k(v=vs.100).aspx </param>
        /// <param name="formatProvider">Optional culture-specific formatting provider. This is usually
        /// a CultureInfo object or NumberFormatInfo object. If null, the current culture is used.
        /// Use CultureInfo.InvariantCulture for persistence.</param>
        /// <returns>String representation of object</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string listSeparator = StringUtil.GetNumberListSeparator(formatProvider);

            // For historic reasons, use "R" for round-trip support, in case this string is persisted.
            if (format == null)
                format = "R";

            return String.Format(
                "{0}{8} {1}{8} {2}{8} {3}{8} {4}{8} {5}{8} {6}{8} {7}",
                P1.X.ToString(format, formatProvider),
                P1.Y.ToString(format, formatProvider),
                P2.X.ToString(format, formatProvider),
                P2.Y.ToString(format, formatProvider),
                P3.X.ToString(format, formatProvider),
                P3.Y.ToString(format, formatProvider),
                P4.X.ToString(format, formatProvider),
                P4.Y.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion
    }
}
