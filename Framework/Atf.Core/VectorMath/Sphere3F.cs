//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A 3D sphere</summary>
    public struct Sphere3F : IFormattable
    {
        /// <summary>
        /// Sphere center</summary>
        public Vec3F Center;

        /// <summary>
        /// Sphere radius</summary>
        public float Radius;

        /// <summary>
        /// Cosntructor using center and radius</summary>
        /// <param name="center">Center point</param>
        /// <param name="radius">Radius</param>
        public Sphere3F(Vec3F center, float radius)
        {
            Center = center;
            Radius = radius;
            m_initialized = true;
        }

        /// <summary>
        /// Constructs a sphere from another sphere</summary>
        /// <param name="sphere">Other sphere</param>
        public Sphere3F(Sphere3F sphere)
        {
            Center = sphere.Center;
            Radius = sphere.Radius;
            m_initialized = true;
        }

        /// <summary>
        /// Gets an empty (zero radius) sphere, centered at the origin</summary>
        public static Sphere3F Empty
        {
            get
            {
                Vec3F center = new Vec3F(0, 0, 0);
                Sphere3F sphere = new Sphere3F(center, 0);
                sphere.m_initialized = false;
                return sphere;
            }
        }

        /// <summary>
        /// Gets bool indicating if the sphere is valid (initialized to some center
        /// and radius)</summary>
        public bool IsValid
        {
            get { return m_initialized; }
        }

        /// <summary>
        /// Sets sphere to given center and radius</summary>
        /// <param name="center">Center point</param>
        /// <param name="radius">Radius</param>
        public void Set(Vec3F center, float radius)
        {
            Center = center;
            Radius = radius;
            m_initialized = true;
        }

        /// <summary>
        /// Extends sphere to enclose another sphere</summary>
        /// <param name="sphere">Sphere to enclose</param>
        /// <returns>Extended sphere</returns>
        public Sphere3F Extend(Sphere3F sphere)
        {
            if (!m_initialized)
            {
                Center = sphere.Center;
                Radius = sphere.Radius;
                m_initialized = true;
            }
            else if (!Contains(sphere))
            {
                if (Center == sphere.Center)
                {
                    Radius = sphere.Radius;
                }
                else
                {
                    Vec3F normal = Vec3F.Normalize(sphere.Center - Center);
                    Vec3F p1 = sphere.Center + (normal * sphere.Radius);
                    Vec3F p2 = Center - (normal * Radius);
                    Radius = (p2 - p1).Length / 2.0f;
                    Center = (p1 + p2) / 2.0f;
                }
            }

            return this;
        }

        /// <summary>
        /// Extends sphere to enclose point</summary>
        /// <param name="pt">Point to enclose</param>
        /// <returns>Extended sphere</returns>
        public Sphere3F Extend(Vec3F pt)
        {
            if (!m_initialized)
            {
                Center = pt;
                Radius = 0;
                m_initialized = true;
            }
            else if (!Contains(pt))
            {
                Vec3F normal = Vec3F.Normalize(pt - Center);
                Vec3F p = Center - (normal * Radius);
                Radius = (pt - p).Length / 2.0f;
                Center = pt + p / 2.0f;
            }

            return this;
        }

        /// <summary>
        /// Extends sphere to enclose box</summary>
        /// <param name="box">Axis-aligned box to enclose</param>
        /// <returns>Extended sphere</returns>
        public Sphere3F Extend(Box box)
        {
            Sphere3F tmp = new Sphere3F((box.Max + box.Min) / 2.0f,
                (box.Max - box.Min).Length / 2.0f);

            Extend(tmp);

            return this;
        }

        /// <summary>
        /// Tests if other sphere is inside this sphere</summary>
        /// <param name="sphere">Other sphere</param>
        /// <returns>True iff other sphere is inside this sphere</returns>
        public bool Contains(Sphere3F sphere)
        {
            float length = (sphere.Center - Center).Length + sphere.Radius;
            return (length < Radius);
        }

        /// <summary>
        /// Tests if point is inside this sphere</summary>
        /// <param name="pt">Point</param>
        /// <returns>True iff pt is inside this sphere</returns>
        public bool Contains(Vec3F pt)
        {
            return ((pt - Center).Length < Radius);
        }

        /// <summary>
        /// Transforms a sphere by the given matrix</summary>
        /// <param name="m">Matrix</param>
        /// <returns>Transformed sphere</returns>
        public Sphere3F Transform(Matrix4F m)
        {
            m.Transform(Center, out Center);

            // Calculate the scale
            float l1 = m.XAxis.Length;
            float l2 = m.YAxis.Length;
            float l3 = m.ZAxis.Length;

            float scale = l1;
            if (scale < l2)
            {
                scale = l2;
            }
            if (scale < l3)
            {
                scale = l3;
            }

            Radius *= scale;

            return this;
        }

        /// <summary>
        /// Tests if a specified ray intersects this sphere</summary>
        /// <param name="ray">The ray, with an origin and unit-length direction. Only intersections in
        /// front of the ray count.</param>
        /// <param name="x">The intersection point, if there was an intersection</param>
        /// <returns>True iff ray intersects this sphere</returns>
        /// <remarks>Algorithm is from _Real-Time Rendering_, p. 299</remarks>
        public bool Intersects(Ray3F ray, out Vec3F x)
        {
            // vector from ray's origin to sphere's center
            Vec3F oToC = Center - ray.Origin;

            // 'd' is the distance from the ray's origin to the nearest point on the ray to Center.
            // Assumes that Direction has unit length.
            float d = Vec3F.Dot(oToC, ray.Direction);

            // distToCenterSquared is the distance from ray's origin to Center, squared.
            float distToCenterSquared = oToC.LengthSquared;
            float radiusSquared = Radius * Radius;

            // if the sphere is behind the ray and the ray's origin is outside the sphere...
            if (d < 0 && distToCenterSquared > radiusSquared)
            {
                x = new Vec3F();
                return false;
            }

            // 'm' is the distance from the ray to the center. Pythagorean's theorem.
            float mSquared = distToCenterSquared - d * d;
            if (mSquared > radiusSquared)
            {
                x = new Vec3F();
                return false;
            }

            // 'q' is the distance from the first intersection point to the nearest point
            //  on the ray to Center. Pythagorean's theorem.
            float q = (float)Math.Sqrt(radiusSquared - mSquared);

            // 't' is the distance along 'ray' to the point of first intersection.
            float t;
            if (distToCenterSquared > radiusSquared)
            {
                // ray's origin is outside the sphere.
                t = d - q;
            }
            else
            {
                // ray's origin is inside the sphere.
                t = d + q;
            }

            // the point of intersection is ray's origin plus distance along ray's direction
            x = ray.Origin + t * ray.Direction;
            return true;
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
                Center.X.ToString(format, formatProvider),
                Center.Y.ToString(format, formatProvider),
                Center.Z.ToString(format, formatProvider),
                Radius.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

        private bool m_initialized;
    }
}
