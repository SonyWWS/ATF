//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Class that encapsulates angle/axis representation of rotation</summary>
    public struct AngleAxisF : IEquatable<AngleAxisF>, IFormattable
    {
        /// <summary>
        /// Angle of rotation, in radians, clockwise around the vector. Is the opposite of
        /// the rotation in the Matrix4F, Matrix3F, and Quat4F classes.</summary>
        /// <remarks>
        /// The following 3 ways of creating a Matrix4F are equivalent:
        ///Matrix4F.RotX(angleToRotate);
        ///Matrix4F.RotAxisRH(new Vec3F(1, 0, 0), angleToRotate);
        ///new Matrix4F(new AngleAxisF(-angleToRotate, new Vec3F(1, 0, 0)));</remarks>
        public float Angle;

        /// <summary>
        /// Axis of rotation, normally of unit length</summary>
        public Vec3F Axis;

        /// <summary>
        /// Constructor</summary>
        /// <param name="angle">Angle of rotation</param>
        /// <param name="axis">Axis of rotation. The resulting axis has length of 1.</param>
        public AngleAxisF(float angle, Vec3F axis)
        {
            Angle = angle;
            Axis.X = axis.X;
            Axis.Y = axis.Y;
            Axis.Z = axis.Z;
            Axis.Normalize();
        }

        /// <summary>
        /// Sets this angle axis from the rotation part of a matrix</summary>
        /// <param name="m">Matrix</param>
        public void Set(Matrix3F m)
        {
            double x = m.M32 - m.M23;
            double y = m.M13 - m.M31;
            double z = m.M21 - m.M12;

            double mag = x * x + y * y + z * z;

            if (mag > EPS)
            {
                mag = Math.Sqrt(mag);

                double sin = 0.5 * mag;
                double cos = 0.5 * (m.M11 + m.M22 + m.M33 - 1.0);
                Angle = (float)Math.Atan2(sin, cos);

                double ooMag = 1.0 / mag;
                Axis.X = (float)(x * ooMag);
                Axis.Y = (float)(y * ooMag);
                Axis.Z = (float)(z * ooMag);
            }
            else
            {
                Axis.X = 0;
                Axis.Y = 1;
                Axis.Z = 0;
                Angle = 0;
            }
        }

        /// <summary>
        /// Sets this angle axis from a quaternion</summary>
        /// <param name="q">Quaternion</param>
        public void Set(QuatF q)
        {
            double mag = q.X * q.X + q.Y * q.Y + q.Z * q.Z;

            if (mag > EPS)
            {
                mag = Math.Sqrt(mag);
                double ooMag = 1.0 / mag;

                Axis.X = (float)(q.X * ooMag);
                Axis.Y = (float)(q.Y * ooMag);
                Axis.Z = (float)(q.Z * ooMag);
                Angle = (float)(2.0 * Math.Atan2(mag, q.W));
            }
            else
            {
                Axis.X = 0;
                Axis.Y = 1;
                Axis.Z = 0;
                Angle = 0;
            }
        }

        /// <summary>
        /// Tests for equality to given angle-axis</summary>
        /// <param name="a">Other angle-axis</param>
        /// <returns>True iff rotations are exactly equal</returns>
        public bool Equals(AngleAxisF a)
        {
            return
                Angle == a.Angle &&
                Axis.Equals(a.Axis);
        }

        /// <summary>
        /// Tests for equality to another angle-axis, within a given epsilon</summary>
        /// <param name="a">Other angle-axis</param>
        /// <param name="eps">Epsilon or margin for error</param>
        /// <returns>True iff all components are within epsilon</returns>
        /// <remarks>2 axes that are in the exact opposite direction and with opposite angles
        /// of rotation also count as being equal.</remarks>
        public bool Equals(AngleAxisF a, double eps)
        {
            return
                (Math.Abs(Angle - a.Angle) < eps && Axis.Equals(a.Axis, eps)) ||
                (Math.Abs(Angle + a.Angle) < eps && Axis.Equals(a.Axis * -1, eps));
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal</summary>
        /// <param name="obj">Another object to compare to</param>
        /// <returns>True iff obj and this instance are the same type and represent
        /// the same value</returns>
        public override bool Equals(object obj)
        {
            if (obj is AngleAxisF)
            {
                AngleAxisF other = (AngleAxisF)obj;
                return Equals(other);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance</returns>
        public override int GetHashCode()
        {
            long bits = 1;
            bits = 31 * bits + Angle.GetHashCode();
            bits = 31 * bits + Axis.GetHashCode();
            return (int)(bits ^ (bits >> 32));
        }

        /// <summary>
        /// Gets the fully qualified type name of this instance</summary>
        /// <returns>A string containing a fully qualified type name</returns>
        public override string ToString()
        {
            return Angle + ", " + Axis.X + ", " + Axis.Y + ", " + Axis.Z;
        }

        /// <summary>Gets the string representation of this Scea.VectorMath.AngleAxisF structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the Angle/axis representation</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return Angle.ToString("R") + ", " + Axis.X.ToString("R") + ", " + Axis.Y.ToString("R") + ", " + Axis.Z.ToString("R");
            else
                return String.Format
                (
                     "({0}, {1}, {2}, {3})",
                     ((double)Angle).ToString(format, formatProvider),
                     ((double)Axis.X).ToString(format, formatProvider),
                     ((double)Axis.Y).ToString(format, formatProvider),
                     ((double)Axis.Z).ToString(format, formatProvider)
                 );

        }

        private static double EPS = 0.000001;
    }
}
