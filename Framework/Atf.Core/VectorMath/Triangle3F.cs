//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Class representing a 3D triangle</summary>
    public class Triangle3F : IFormattable
    {
        /// <summary>
        /// 1st vertex</summary>
        public Vec3F V0;

        /// <summary>
        /// 2nd vertex</summary>
        public Vec3F V1;

        /// <summary>
        /// 3rd vertex</summary>
        public Vec3F V2;

        /// <summary>
        /// Construct triangle from 3 vertices</summary>
        /// <param name="v0">Vertex 0</param>
        /// <param name="v1">Vertex 1</param>
        /// <param name="v2">Vertex 2</param>
        public Triangle3F(Vec3F v0, Vec3F v1, Vec3F v2)
        {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        /// <summary>
        /// Returns a string representation of this object for GUIs. For persistence, use
        /// ToString("R", CultureInfo.InvariantCulture).</summary>
        /// <returns>String representing object</returns>
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
        /// <returns>String representing object</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string listSeparator = StringUtil.GetNumberListSeparator(formatProvider);

            // For historic reasons, use "R" for round-trip support, in case this string is persisted.
            if (format == null)
                format = "R";

            return String.Format(
                "{0}{9} {1}{9} {2}{9} {3}{9} {4}{9} {5}{9} {6}{9} {7}{9} {8}",
                V0.X.ToString(format, formatProvider),
                V0.Y.ToString(format, formatProvider),
                V0.Z.ToString(format, formatProvider),
                V1.X.ToString(format, formatProvider),
                V1.Y.ToString(format, formatProvider),
                V1.Z.ToString(format, formatProvider),
                V2.X.ToString(format, formatProvider),
                V2.Y.ToString(format, formatProvider),
                V2.Z.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

    }
}
