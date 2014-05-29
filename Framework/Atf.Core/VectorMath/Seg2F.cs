//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// 2D line segment</summary>
    public class Seg2F : IFormattable
    {
        /// <summary>
        /// First endpoint</summary>
        public Vec2F P1;
        /// <summary>
        /// Second endpoint</summary>
        public Vec2F P2;

        /// <summary>
        /// Constructor using end points</summary>
        /// <param name="p1">First endpoint</param>
        /// <param name="p2">Second endpoint</param>
        public Seg2F(Vec2F p1, Vec2F p2)
        {
            P1 = p1;
            P2 = p2;
        }

        /// <summary>
        /// Projects a point onto a segment</summary>
        /// <param name="seg">Segment</param>
        /// <param name="p">Point to project</param>
        /// <returns>Point on the segment nearest to the point</returns>
        public static Vec2F Project(Seg2F seg, Vec2F p)
        {
            Vec2F result = new Vec2F();
            Vec2F dir = seg.P2 - seg.P1;
            Vec2F vec = p - seg.P1;
            float lengthSquared = Vec2F.Dot(dir, dir);
            if (lengthSquared < DegenerateLength * DegenerateLength)    // degenerate segment?
            {
                result = seg.P1;
            }
            else
            {
                float projection = Vec2F.Dot(dir, vec);
                if (projection < 0)
                {
                    result = seg.P1;
                }
                else if (projection > lengthSquared)
                {
                    result = seg.P2;
                }
                else
                {
                    double scale = projection / lengthSquared;
                    result.X = (float)(seg.P1.X + scale * dir.X);
                    result.Y = (float)(seg.P1.Y + scale * dir.Y);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets the distance from a point p to its projection on a segment</summary>
        /// <param name="seg">Segment</param>
        /// <param name="p">Point to project</param>
        /// <returns>Distance from point p to its projection on segment</returns>
        public static float DistanceToSegment(Seg2F seg, Vec2F p)
        {
            Vec2F projection = Project(seg, p);
            float d = Vec2F.Distance(p, projection);
            return d;
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
                "{0}{4} {1}{4} {2}{4} {3}",
                P1.X.ToString(format, formatProvider),
                P1.Y.ToString(format, formatProvider),
                P2.X.ToString(format, formatProvider),
                P2.Y.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

        /// <summary>
        /// The minimum length of a segment, below which, the segment is considered to have zero length</summary>
        public const float DegenerateLength = 0.00001f;
    }
}
