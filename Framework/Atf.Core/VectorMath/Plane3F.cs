//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// 3D Plane, of the form Normal * p = Distance, where 'p' is a point on the plane</summary>
    public struct Plane3F : IFormattable
    {
        /// <summary>
        /// Plane normal -- a unit vector</summary>
        public Vec3F Normal;

        /// <summary>
        /// Plane constant. The distance to the origin, such that for any point 'p' on the plane,
        /// Dot( Normal, p) - Distance equals zero.</summary>
        public float Distance;

        /// <summary>
        /// Initializes the plane according to the plane equation:
        ///     Normal * p = Distance
        /// for any point 'p' on the plane.</summary>
        /// <param name="normal">Normal. Should be a unit vector.</param>
        /// <param name="distance">Distance to origin, if Normal * p = Distance formula is used</param>
        public Plane3F(Vec3F normal, float distance)
        {
            Normal = normal;
            Distance = distance;
        }

        /// <summary>
        /// Constructs a plane from normal and point on plane, such that Normal * p = Distance</summary>
        /// <param name="normal">Normal. Should be a unit vector.</param>
        /// <param name="pointOnPlane">Point on plane</param>
        public Plane3F(Vec3F normal, Vec3F pointOnPlane)
        {
            Normal = normal;
            Distance = Vec3F.Dot(normal, pointOnPlane);
        }

        /// <summary>
        /// Constructs a plane from 3 non-linear points on the plane, such that
        /// Normal * p = Distance</summary>
        /// <param name="p1">First point</param>
        /// <param name="p2">Second point</param>
        /// <param name="p3">Third point</param>
        public Plane3F(Vec3F p1, Vec3F p2, Vec3F p3)
        {
            Vec3F d12 = p2 - p1;
            Vec3F d13 = p3 - p1;
            Vec3F normal = Vec3F.Cross(d12, d13);
            Normal = Vec3F.Normalize(normal);
            Distance = Vec3F.Dot(Normal, p1);
        }

        /// <summary>
        /// Constructs a plane from another plane</summary>
        /// <param name="plane">Other plane</param>
        public Plane3F(Plane3F plane)
        {
            Normal = plane.Normal;
            Distance = plane.Distance;
        }

        /// <summary>
        /// Sets this plane to be located 'd' distance along the vector 'n' (the plane's normal).
        /// This means that the plane equation is Normal * p = Distance.</summary>
        /// <param name="n">Plane's normal, a unit vector</param>
        /// <param name="d">Distance along the normal that the plane should be located</param>
        public void Set(Vec3F n, float d)
        {
            Normal = n;
            Distance = d;
        }

        /// <summary>
        /// Returns a point that is on the plane. Assumes that Normal is a unit vector.</summary>
        /// <returns>Point on the plane</returns>
        public Vec3F PointOnPlane()
        {
            return Vec3F.Mul(Normal, Distance);
        }

        /// <summary>
        /// Returns the signed distance from this plane. This only works if the plane equation is
        /// Normal * p = Distance for any point 'p' on the plane. A positive result means that
        /// the point is on the same side of the plane that the normal is "on".</summary>
        /// <param name="point">Mathematical point</param>
        /// <returns>Signed distance from 'point' to this plane. Positive means "in front".</returns>
        public float SignedDistance(Vec3F point)
        {
            return Vec3F.Dot(Normal, point) - Distance;
        }

        /// <summary>
        /// Projects the given point onto this plane</summary>
        /// <param name="point">Mathematical point</param>
        /// <returns>The given point projected onto this plane</returns>
        public Vec3F Project(Vec3F point)
        {
            return point - SignedDistance(point) * Normal;
        }

        /// <summary>
        /// Returns a string representation of this object for GUIs. For persistence, use
        /// ToString("R", CultureInfo.InvariantCulture).</summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string listSeparator = StringUtil.GetNumberListSeparator(formatProvider);

            // For historic reasons, use "R" for round-trip support, in case this string is persisted.
            if (format == null)
                format = "R";

            return String.Format(
                "{0}{4} {1}{4} {2}{4} {3}",
                ((double)Normal.X).ToString(format, formatProvider),
                ((double)Normal.Y).ToString(format, formatProvider),
                ((double)Normal.Z).ToString(format, formatProvider),
                ((double)Distance).ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

    }
}
