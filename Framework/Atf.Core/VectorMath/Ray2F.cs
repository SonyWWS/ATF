//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// A 2D ray</summary>
    public struct Ray2F : IFormattable
    {
        /// <summary>
        /// Ray origin, or starting point</summary>
        public Vec2F Origin;

        /// <summary>
        /// Ray direction</summary>
        public Vec2F Direction;

        /// <summary>
        /// Constructor</summary>
        /// <param name="origin">Ray origin</param>
        /// <param name="direction">Ray direction</param>
        public Ray2F(Vec2F origin, Vec2F direction)
        {
            Origin = origin;
            Direction = direction;
        }

        /// <summary>
        /// Determines if 2 rays intersect, and if so, returns the ray parameters of the intersection point</summary>
        /// <param name="r1">Ray 1</param>
        /// <param name="r2">Ray 2</param>
        /// <param name="t1">Returned ray parameter 1</param>
        /// <param name="t2">Returned ray parameter 2</param>
        /// <returns>True iff the rays intersect (are not parallel)</returns>
        public static bool Intersect(Ray2F r1, Ray2F r2, ref double t1, ref double t2)
        {
            double denom = (r2.Direction.Y * r1.Direction.X - r2.Direction.X * r1.Direction.Y);
            if (Math.Abs(denom) < 0.000001f)
                return false;

            Vec2F d = Vec2F.Sub(r1.Origin, r2.Origin);
            t1 = (r2.Direction.X * d.Y - r2.Direction.Y * d.X) / denom;
            t2 = (r1.Direction.X * d.Y - r1.Direction.Y * d.X) / denom;
            return true;
        }

        /// <summary>
        /// Gets the first intersection of a ray with a box (closest intersection to the
        /// ray origin)</summary>
        /// <param name="ray">Ray</param>
        /// <param name="box">Box</param>
        /// <param name="intersection">Intersection point</param>
        /// <returns>True iff ray hits box</returns>
        public static bool Intersect(Ray2F ray, Box2F box, ref Vec2F intersection)
        {
            // do X slab
            float tmin, tmax;
            SlabIntersect(
                ray.Direction.X, ray.Origin.X, box.Min.X, box.Max.X, out tmin, out tmax);

            // do Y slab
            float tminTemp, tmaxTemp;
            SlabIntersect(
                ray.Direction.Y, ray.Origin.Y, box.Min.Y, box.Max.Y, out tminTemp, out tmaxTemp);

            if ((tmin > tmaxTemp) || (tminTemp > tmax))
                return false;

            tmin = Math.Max(tmin, tminTemp);
            tmax = Math.Min(tmax, tmaxTemp);

            // intersection at tmin, the maximal-minimal value
            if (tmin > 0)
            {
                intersection = ray.Origin + ray.Direction * tmin;
                return true;
            }
            return false;
        }

        private static void SlabIntersect(
            float direction, float origin, float min, float max, out float tmin, out float tmax)
        {
            float recip = 1 / direction;
            if (recip >= 0)
            {

                tmin = (min - origin) * recip;
                tmax = (max - origin) * recip;
            }
            else
            {
                tmin = (max - origin) * recip;
                tmax = (min - origin) * recip;
            }
        }
        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.Ray2F structure</summary>
        /// <returns>A <see cref="T:System.String"></see> representing the 2D ray</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary> Returns the string representation of this Scea.VectorMath.Ray2F structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 2D ray</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return Origin.X.ToString("R") + ", " + Origin.Y.ToString("R") + ", " + Direction.X.ToString("R") + ", " + Direction.Y.ToString("R");
            else
                return String.Format
                (
                     "({0}, {1}, {2}, {3})",
                     ((double)Origin.X).ToString(format, formatProvider),
                     ((double)Origin.Y).ToString(format, formatProvider),
                     ((double)Direction.X).ToString(format, formatProvider),
                     ((double)Direction.Y).ToString(format, formatProvider)
                  );

        }

    }
}
