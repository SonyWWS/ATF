//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A simple 2D bounding box</summary>
    public class Box2F : IFormattable
    {
        /// <summary>
        /// Minima of extents</summary>
        public Vec2F Min;

        /// <summary>
        /// Maxima of extents</summary>
        public Vec2F Max;

        /// <summary>
        /// Constructor</summary>
        public Box2F()
        {
            m_empty = true;
        }

        /// <summary>
        /// Constructor with min and max</summary>
        /// <param name="min">Minima of extents</param>
        /// <param name="max">Maxima of extents</param>
        public Box2F(Vec2F min, Vec2F max)
        {
            Min = min;
            Max = max;
            m_empty = false;
        }

        /// <summary>
        /// Gets a value indicating if this box is empty</summary>
        public bool IsEmpty
        {
            get { return (Min == Max); }
        }

        /// <summary>
        /// Gets the centroid (geometrical center) of the box</summary>
        /// <remarks>Returns the origin for an empty box</remarks>
        public Vec2F Centroid
        {
            get { return Vec2F.Mul(Min + Max, 0.5f); }
        }

        /// <summary>
        /// Extends box to contain the given point</summary>
        /// <param name="p">Point</param>
        /// <returns>Extended box</returns>
        public Box2F Extend(Vec2F p)
        {
            if (m_empty)
            {
                Min = Max = p;
                m_empty = false;
            }
            else
            {
                Min.X = Math.Min(Min.X, p.X);
                Min.Y = Math.Min(Min.Y, p.Y);

                Max.X = Math.Max(Max.X, p.X);
                Max.Y = Math.Max(Max.Y, p.Y);
            }

            return this;
        }

        /// <summary>
        /// Extends box to contain given array, interpreted as 3D points</summary>
        /// <param name="v">Floats representing 3D points</param>
        /// <returns>Extended box</returns>
        public Box2F Extend(float[] v)
        {
            if (v.Length >= 2)
            {
                if (m_empty)
                {
                    Max.X = Min.X = v[0];
                    Max.Y = Min.Y = v[1];
                }

                for (int i = 0; i < v.Length; i += 3)
                {
                    Min.X = Math.Min(Min.X, v[i]);
                    Min.Y = Math.Min(Min.Y, v[i + 1]);

                    Max.X = Math.Max(Max.X, v[i]);
                    Max.Y = Math.Max(Max.Y, v[i + 1]);
                }
            }

            return this;
        }

        /// <summary>
        /// Extends box to contain circle</summary>
        /// <param name="circle">Input circle</param>
        /// <returns>Extended box</returns>
        public Box2F Extend(CircleF circle)
        {
            float r = circle.Radius;
            Extend(circle.Center + new Vec2F(r, r));
            Extend(circle.Center - new Vec2F(r, r));
            return this;
        }

        /// <summary>
        /// Extends the box to contain the given box</summary>
        /// <param name="other">Given box</param>
        /// <returns>Extended box</returns>
        public Box2F Extend(Box2F other)
        {
            if (!other.IsEmpty)
            {
                Extend(other.Min);
                Extend(other.Max);
            }
            return this;
        }

        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.Box2F structure</summary>
        /// <returns>A <see cref="T:System.String"></see> representing the 2D bounding box</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.Box2F structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 2D bounding box</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return Min.X.ToString("R") + ", " + Min.Y.ToString("R") + ", " + Max.X.ToString("R") + ", " + Max.Y.ToString("R");
            else
                return String.Format
                (
                     "({0}, {1}, {2}, {3})",
                     ((double)Min.X).ToString(format, formatProvider),
                     ((double)Min.Y).ToString(format, formatProvider),
                     ((double)Max.X).ToString(format, formatProvider),
                     ((double)Max.Y).ToString(format, formatProvider)
                 );

        }

        private bool m_empty = true;
    }
}
