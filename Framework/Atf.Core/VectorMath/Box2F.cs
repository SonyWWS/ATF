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
                "{0}{4} {1}{4} {2}{4} {3}",
                Min.X.ToString(format, formatProvider),
                Min.Y.ToString(format, formatProvider),
                Max.X.ToString(format, formatProvider),
                Max.Y.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

        private bool m_empty = true;
    }
}
