//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A simple 3D bounding box</summary>
    public class Box : IFormattable
    {
        /// <summary>
        /// Minima of extents</summary>
        public Vec3F Min;

        /// <summary>
        /// Maxima of extents</summary>
        public Vec3F Max;

        /// <summary>
        /// Constructor</summary>
        public Box()
        {
        }

        /// <summary>
        /// Constructor with min and max</summary>
        /// <param name="min">Minima of extents</param>
        /// <param name="max">Maxima of extents</param>
        public Box(Vec3F min, Vec3F max)
        {
            Min = min;
            Max = max;
            m_initialized = true;
        }

        /// <summary>
        /// Gets a value indicating if this box has zero-volume</summary>
        public bool IsEmpty
        {
            get { return (Min == Max); }
        }

        /// <summary>
        /// Gets the centroid (geometrical center) of the box</summary>
        /// <remarks>Returns the origin for an empty box</remarks>
        public Vec3F Centroid
        {
            get { return Vec3F.Mul(Min + Max, 0.5f); }
        }

        /// <summary>
        /// Extends box to contain the given point, or if this box is uninitialized, the box is
        /// initialized from the point</summary>
        /// <param name="p">Point</param>
        /// <returns>Extended box</returns>
        public Box Extend(Vec3F p)
        {
            if (m_initialized == false)
            {
                Min = Max = p;
                m_initialized = true;
            }
            else
            {
                Min.X = Math.Min(Min.X, p.X);
                Min.Y = Math.Min(Min.Y, p.Y);
                Min.Z = Math.Min(Min.Z, p.Z);

                Max.X = Math.Max(Max.X, p.X);
                Max.Y = Math.Max(Max.Y, p.Y);
                Max.Z = Math.Max(Max.Z, p.Z);
            }

            return this;
        }

        /// <summary>
        /// Extends box to contain given array, interpreted as 3D points. 
        /// If this box is currently uninitialized, the box is initialized to the first point.</summary>
        /// <param name="v">Floats representing 3D points</param>
        /// <returns>Extended box</returns>
        public Box Extend(IList<float> v)
        {
            if (v.Count >= 3)
            {
                if (!m_initialized)
                {
                    Max.X = Min.X = v[0];
                    Max.Y = Min.Y = v[1];
                    Max.Z = Min.Z = v[2];

                    m_initialized = true;
                }

                for (int i = 0; i < v.Count; i += 3)
                {
                    Min.X = Math.Min(Min.X, v[i]);
                    Min.Y = Math.Min(Min.Y, v[i + 1]);
                    Min.Z = Math.Min(Min.Z, v[i + 2]);

                    Max.X = Math.Max(Max.X, v[i]);
                    Max.Y = Math.Max(Max.Y, v[i + 1]);
                    Max.Z = Math.Max(Max.Z, v[i + 2]);
                }
            }

            return this;
        }

        /// <summary>
        /// Extends box to contain sphere, initializing this box if it is currently uninitialized</summary>
        /// <param name="sphere">Input sphere</param>
        /// <returns>The extended box</returns>
        public Box Extend(Sphere3F sphere)
        {
            float r = sphere.Radius;
            Extend(sphere.Center + new Vec3F(r, r, r));
            Extend(sphere.Center - new Vec3F(r, r, r));
            return this;
        }

        /// <summary>
        /// Extends the box to contain the given box.
        /// If this box is currently uninitialized, sets this box to be the other box.</summary>
        /// <param name="other">The given box</param>
        /// <returns>The extended box</returns>
        public Box Extend(Box other)
        {
            if (!other.IsEmpty)
            {
                Extend(other.Min);
                Extend(other.Max);
            }
            return this;
        }

        /// <summary>
        /// Gets the object's string representation</summary>
        /// <returns>Object's string representation</returns>
        public override string ToString()
        {
            return Min + Environment.NewLine + Max;
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
                "{0}{6} {1}{6} {2}{6} {3}{6} {4}{6} {5}",
                Min.X.ToString(format, formatProvider),
                Min.Y.ToString(format, formatProvider),
                Min.Z.ToString(format, formatProvider),
                Max.X.ToString(format, formatProvider),
                Max.Y.ToString(format, formatProvider),
                Max.Z.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion


        /// <summary>
        /// Sets box to extents of original box, transformed by the given matrix</summary>
        /// <param name="M">Transformation matrix</param>
        public void Transform(Matrix4F M)
        {
            // http://www.ics.uci.edu/~arvo/code/TransformingBoxes.c
            // "Transforming Axis-Aligned Bounding Boxes",
            //  by Jim Arvo, in "Graphics Gems", Academic Press, 1990.
            float[] oldMin = new[] { Min.X, Min.Y, Min.Z };
            float[] oldMax = new[] { Max.X, Max.Y, Max.Z };

            float[] newMin = new[] { M.M41, M.M42, M.M43 };
            float[] newMax = new[] { newMin[0], newMin[1], newMin[2] };

            float[] mArray = M.ToArray();

            float a, b;

            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    a = mArray[j * 4 + i] * oldMin[j];
                    b = mArray[j * 4 + i] * oldMax[j];

                    if (a < b)
                    {
                        newMin[i] += a;
                        newMax[i] += b;
                    }
                    else
                    {
                        newMin[i] += b;
                        newMax[i] += a;
                    }
                }
            }

            Min.X = newMin[0];
            Min.Y = newMin[1];
            Min.Z = newMin[2];

            Max.X = newMax[0];
            Max.Y = newMax[1];
            Max.Z = newMax[2];

            m_initialized = true;
        }

        /// <summary>
        /// Returns an array of six floats representing this box</summary>
        /// <returns>Array of six floats representing this box</returns>
        public float[] ToArray()
        {
            float[] temp = new float[6];
            temp[0] = Min.X;
            temp[1] = Min.Y;
            temp[2] = Min.Z;

            temp[3] = Max.X;
            temp[4] = Max.Y;
            temp[5] = Max.Z;
            return temp;
        }

        /// <summary>
        /// Tests if specified ray intersects the box</summary>
        /// <param name="ray">The ray</param>
        /// <returns>True iff ray intersects box</returns>
        public bool Intersects(Ray3F ray)
        {
            // http://www.gametutorials.com/gtstore/pc-429-9-ray-and-aabb-collision.aspx
            // Compute the ray delta
            Vec3F rayDelta = ray.Direction * 100000f;

            // First we check to see if the origin of the ray is 
            // inside the AABB.  If it is, the ray intersects the AABB so 
            // we'll return true.  We start by assuming the ray origin is 
            // inside the AABB
            bool inside = true;

            // This stores the distance from either the min or max (x,y,z) to the ray's
            // origin (x,y,z) respectively, divided by the length of the ray.  The largest
            // value has the delta time of a possible intersection.
            float xt, yt, zt;

            // Test the X component of the ray's origin to see if we are inside or not
            if (ray.Origin.X < Min.X)
            {
                xt = Min.X - ray.Origin.X;

                if (xt > rayDelta.X) // If the ray is moving away from the AABB, there is no intersection 
                    return false;

                xt /= rayDelta.X;
                inside = false;
            }
            else if (ray.Origin.X > Max.X)
            {
                xt = Max.X - ray.Origin.X;

                if (xt < rayDelta.X) // If the ray is moving away from the AABB, there is no intersection 
                    return false;

                xt /= rayDelta.X;
                inside = false;
            }
            else
            {
                // Later on we use the "xt", "yt", and "zt" variables to determine which plane (either
                // xy, xz, or yz) we may collide with.  Since the x component of the ray
                // origin is in between, the AABB's left and right side (which reside in the yz plane), 
                // we know we don't have to test those sides so we set this to a negative value.
                xt = -1.0f;
            }

            // Test the X component of the ray's origin to see if we are inside or not
            if (ray.Origin.Y < Min.Y)
            {
                yt = Min.Y - ray.Origin.Y;

                if (yt > rayDelta.Y) // If the ray is moving away from the AABB, there is no intersection 
                    return false;

                yt /= rayDelta.Y;
                inside = false;
            }
            else if (ray.Origin.Y > Max.Y)
            {
                yt = Max.Y - ray.Origin.Y;

                if (yt < rayDelta.Y) // If the ray is moving away from the AABB, there is no intersection 
                    return false;

                yt /= rayDelta.Y;
                inside = false;
            }
            else
            {
                // Later on we use the "xt", "yt", and "zt" variables to determine which plane (either
                // xy, xz, or yz) we may collide with.  Since the y component of the ray
                // origin is in between, the AABB's top and bottom side (which reside in the xz plane), 
                // we know we don't have to test those sides so we set this to a negative value.
                yt = -1.0f;
            }

            if (ray.Origin.Z < Min.Z)
            {
                zt = Min.Z - ray.Origin.Z;

                if (zt > rayDelta.Z) // If the ray is moving away from the AABB, there is no intersection 
                    return false;

                zt /= rayDelta.Z;
                inside = false;
            }
            else if (ray.Origin.Z > Max.Z)
            {
                zt = Max.Z - ray.Origin.Z;

                if (zt < rayDelta.Z) // If the ray is moving away from the AABB, there is no intersection 
                    return false;

                zt /= rayDelta.Z;
                inside = false;
            }
            else
            {
                // Later on we use the "xt", "yt", and "zt" variables to determine which plane (either
                // xy, xz, or yz) we may collide with.  Since the z component of the ray
                // origin is in between, the AABB's front and back side (which reside in the xy plane), 
                // we know we don't have to test those sides so we set this to a negative value.
                zt = -1.0f;
            }

            // If the origin inside the AABB
            if (inside)
                return true; // The ray intersects the AABB

            // Otherwise we have some checking to do...

            // We want to test the AABB planes with largest value out of xt, yt, and zt.  So
            // first we determine which value is the largest.

            float t = xt;

            if (yt > t)
                t = yt;

            if (zt > t)
                t = zt;

            // **NOTE** Normally comparing two floating point numbers won't necessarily work, however,
            //            since we set to explicitly to equal either xt, yt, or zt above, the equality test
            //            will pass

            if (t == xt) // If the ray intersects with the AABB's YZ plane
            {
                // Compute intersection values
                float y = ray.Origin.Y + rayDelta.Y * t;
                float z = ray.Origin.Z + rayDelta.Z * t;

                // Test to see if collision takes place within the bounds of the AABB
                if (y < Min.Y || y > Max.Y)
                    return false;
                else if (z < Min.Z || z > Max.Z)
                    return false;
            }
            else if (t == yt) // Intersects with the XZ plane
            {
                // Compute intersection values
                float x = ray.Origin.X + rayDelta.X * t;
                float z = ray.Origin.Z + rayDelta.Z * t;

                // Test to see if collision takes place within the bounds of the AABB
                if (x < Min.X || x > Max.X)
                    return false;
                else if (z < Min.Z || z > Max.Z)
                    return false;
            }
            else // Intersects with the XY plane
            {
                // Compute intersection values
                float x = ray.Origin.X + rayDelta.X * t;
                float y = ray.Origin.Y + rayDelta.Y * t;

                // Test to see if collision takes place within the bounds of the AABB
                if (x < Min.X || x > Max.X)
                    return false;
                else if (y < Min.Y || y > Max.Y)
                    return false;
            }

            // The ray intersects the AABB
            return true;
        }

        private bool m_initialized; //has been set with valid data; the box may still have zero-volume.
    }
}