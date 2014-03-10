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
        /// Returns the string representation of this Scea.VectorMath.Triangle3F structure</summary>
        /// <returns> A <see cref="T:System.String"></see> representing the 3D triangle</returns>        
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary> 
        /// Returns the string representation of this Scea.VectorMath.Triangle3F structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D triangle</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return V0.X.ToString("R") + ",  " + V0.Y.ToString("R") + ",  " + V0.Z.ToString("R") + ",  " +
                       V1.X.ToString("R") + ",  " + V1.Y.ToString("R") + ",  " + V1.Z.ToString("R") + ",  " +
                       V2.X.ToString("R") + ",  " + V2.Y.ToString("R") + ",  " + V2.Z.ToString("R");

            else
                return String.Format
                (
                     "({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                     ((double)V0.X).ToString(format, formatProvider),
                     ((double)V0.Y).ToString(format, formatProvider),
                     ((double)V0.Z).ToString(format, formatProvider),
                     ((double)V1.X).ToString(format, formatProvider),
                     ((double)V1.Y).ToString(format, formatProvider),
                     ((double)V1.Z).ToString(format, formatProvider),
                     ((double)V2.X).ToString(format, formatProvider),
                     ((double)V2.Y).ToString(format, formatProvider),
                     ((double)V2.Z).ToString(format, formatProvider)
                 );

        }
    }
}
