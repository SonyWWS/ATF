//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A circle (sphere in 2D) represented with floats</summary>
    public struct CircleF : IFormattable
    {
        /// <summary>
        /// Circle center</summary>
        public Vec2F Center;

        /// <summary>
        /// Circle radius</summary>
        public float Radius;

        /// <summary>
        /// Constructor with center and radius</summary>
        /// <param name="center">Center point</param>
        /// <param name="radius">Radius</param>
        public CircleF(Vec2F center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Constructs the circle containing 3 points</summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <param name="p3">Point 3</param>
        public CircleF(Vec2F p1, Vec2F p2, Vec2F p3)
        {
            Vec2F o1 = Vec2F.Add(p1, p2); o1 *= 0.5f;
            Vec2F o2 = Vec2F.Add(p3, p2); o2 *= 0.5f;
            Vec2F d1 = Vec2F.Sub(p2, p1); d1 = d1.Perp;
            Vec2F d2 = Vec2F.Sub(p3, p2); d2 = d2.Perp;
            double t1 = 0;
            double t2 = 0;
            if (Ray2F.Intersect(new Ray2F(o1, d1), new Ray2F(o2, d2), ref t1, ref t2))
            {
                Center = o1 + d1 * (float)t1;
                Radius = Vec2F.Distance(Center, p1);
            }
            else
            {
                Center = new Vec2F(float.PositiveInfinity, float.PositiveInfinity);
                Radius = float.PositiveInfinity;
            }
        }

        /// <summary>
        /// Determines if point is inside circle</summary>
        /// <param name="p">Point</param>
        /// <returns>True iff point is inside circle</returns>
        public bool Contains(Vec2F p)
        {
            Vec2F d = Vec2F.Sub(p, Center);
            return d.LengthSquared < (Radius * Radius);
        }

        /// <summary>
        /// Projects a point onto a circle</summary>
        /// <param name="p">Point to project</param>
        /// <param name="c">Circle to project onto</param>
        /// <param name="projection">Projected point</param>
        /// <returns>True iff projection is well defined</returns>
        public static bool Project(Vec2F p, CircleF c, ref Vec2F projection)
        {
            Vec2F d = Vec2F.Sub(p, c.Center);
            float length = d.Length;
            bool wellDefined = false;
            if (length > 0.000001f * c.Radius)
            {
                wellDefined = true;
                float scale = c.Radius / length;
                projection = Vec2F.Add(c.Center, Vec2F.Mul(d, scale));
            }
            return wellDefined;
        }

        /// <summary>
        /// Calculates the points of intersection between 2 CircleF objects</summary>
        /// <param name="c1">First CircleF</param>
        /// <param name="c2">Second CircleF</param>
        /// <param name="p1">First intersection point</param>
        /// <param name="p2">Second intersection point</param>
        /// <returns>True iff there are 1 or 2 intersection points; false if there are none or an infinite number</returns>
        public static bool Intersect(CircleF c1, CircleF c2, ref Vec2F p1, ref Vec2F p2)
        {
            Vec2F v1 = c2.Center - c1.Center;
            double d = v1.Length;
            const double EPS = 1.0e-6;
            if (d < EPS ||
                d > c1.Radius + c2.Radius)
                return false;

            v1 *= (float)(1 / d);
            Vec2F v2 = v1.Perp;

            double cos = (d * d + c1.Radius * c1.Radius - c2.Radius * c2.Radius) / (2 * c1.Radius * d);
            double sin = Math.Sqrt(1 - cos * cos);

            Vec2F t1 = Vec2F.Mul(v1, (float)(c1.Radius * cos));
            Vec2F t2 = Vec2F.Mul(v2, (float)(c1.Radius * sin));
            p1 = c1.Center + t1 + t2;
            p2 = c1.Center + t1 - t2;

            return true;

            // From http://mathforum.org/library/drmath/view/51710.html
            // First, let C1 and C2 be the centers of the Circlefs with radii r1 and 
            // r2, and let d be the distance between C1 and C2.
            // 
            // Now let V1 be the unit vector from C1 to C2, and let V2 be the unit 
            // vector perpendicular to V1.
            // 
            // Also let V3 be the vector from C1 to one of the intersection points.
            // 
            // Finally, let A be the angle between V1 and V3.
            // 
            // From the law of cosines we know that
            // 
            //         r2^2 = r1^2 + d^2 - 2*r1*d*cos(A)
            // 
            // With this equation we can solve for 'A'.
            // 
            // The intersection points will be
            // 
            //         C1 + [r1*cos(A)]*V1 + [r1*sin(A)]*V2
            // 
            //         C1 + [r1*cos(A)]*V1 - [r1*sin(A)]*V2

            // a simple unit test
            //            CircleF test1 = new CircleF(new Vec2F(-0.5f, 0), 1);
            //            CircleF test2 = new CircleF(new Vec2F(0.5f, 0), 1);
            //            Vec2F result1 = new Vec2F();
            //            Vec2F result2 = new Vec2F();
            //            CircleF.Intersect(test1, test2, ref result1, ref result2);

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
                "{0}{3} {1}{3} {2}",
                ((double)Center.X).ToString(format, formatProvider),
                ((double)Center.Y).ToString(format, formatProvider),
                ((double)Radius).ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

    }
}
