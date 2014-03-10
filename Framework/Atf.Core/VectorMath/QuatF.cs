//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Unit quaternion, to represent rotations</summary>
    /// <remarks>The x, y, z components describe the normalized axis of rotation, scaled by the sine of twice
    /// the angle of rotation. The w component is the cosine of twice the angle of rotation.</remarks>
    public struct QuatF : IEquatable<QuatF>, IFormattable
    {
        /// <summary>
        /// X component</summary>
        public float X;

        /// <summary>
        /// Y component</summary>
        public float Y;

        /// <summary>
        /// Z component</summary>
        public float Z;

        /// <summary>
        /// W component</summary>
        public float W;

        /// <summary>
        /// Constructor using components</summary>
        /// <param name="x">X component of imaginary part</param>
        /// <param name="y">Y component of imaginary part</param>
        /// <param name="z">Z component of imaginary part</param>
        /// <param name="w">Real part</param>
        public QuatF(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Constructor using array of components</summary>
        /// <param name="coords">Array of floats with Length >= 4</param>
        public QuatF(float[] coords)
        {
            if (coords.Length < 4)
                throw new ArgumentException("not enough coords");

            X = coords[0];
            Y = coords[1];
            Z = coords[2];
            W = coords[3];
        }



        /// <summary>
        /// Constructor using vector</summary>
        /// <param name="v">Vector to be the imaginary part; W will be set to 1.0</param>
        public QuatF(Vec3F v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = 1.0f;
        }

        /// <summary>
        /// Constructor using vector and float for real part</summary>
        /// <param name="v">Vector to be the imaginary part</param>
        /// <param name="w">the real part</param>
        public QuatF(Vec3F v, float w)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = w;
        }

        /// <summary>
        /// Constructor using Vec4F</summary>
        /// <param name="v">Vector to interpret as quaternion</param>
        public QuatF(Vec4F v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }

        /// <summary>
        /// Gets the conjugate of this quaternion</summary>
        public QuatF Conjugate
        {
            get
            {
                return new QuatF(-X, -Y, -Z, W);
            }
        }

        /// <summary>
        /// Gets the inverse of this quaternion</summary>
        public QuatF Inverse
        {
            get
            {
                double ooNorm = 1.0 / (X * X + Y * Y + Z * Z + W * W);
                return new QuatF(
                    (float)(-X * ooNorm),
                    (float)(-Y * ooNorm),
                    (float)(-Z * ooNorm),
                    (float)(W * ooNorm));
            }
        }

        /// <summary>
        /// The identity quaternion</summary>
        public static readonly QuatF Identity = new QuatF(0, 0, 0, 1);

        /// <summary>
        /// Returns the product of the given quaternions</summary>
        /// <param name="q1">Quaternion 1</param>
        /// <param name="q2">Quaternion 2</param>
        /// <returns>Product of the given quaternions</returns>
        public static QuatF Mul(QuatF q1, QuatF q2)
        {
            float x = q1.X * q2.W + q1.Y * q2.Z - q1.Z * q2.Y + q1.W * q2.X;
            float y = -q1.X * q2.Z + q1.Y * q2.W + q1.Z * q2.X + q1.W * q2.Y;
            float z = q1.X * q2.Y - q1.Y * q2.X + q1.Z * q2.W + q1.W * q2.Z;
            float w = -q1.X * q2.X - q1.Y * q2.Y - q1.Z * q2.Z + q1.W * q2.W;

            return new QuatF(x, y, z, w);
        }

        /// <summary>
        /// Returns the normal of the given quaternion</summary>
        /// <param name="q">Quaternion</param>
        /// <returns>Normal for the given quaternion</returns>
        public static QuatF Normalize(QuatF q)
        {
            double norm = q.W * q.W + q.X * q.X + q.Y * q.Y + q.Z * q.Z;
            float x, y, z, w;

            if (norm > EPS)
            {
                norm = 1.0 / Math.Sqrt(norm);
                x = (float)(norm * q.X);
                y = (float)(norm * q.Y);
                z = (float)(norm * q.Z);
                w = (float)(norm * q.W);
            }
            else
            {
                x = y = z = w = 0.0f;
            }
            return new QuatF(x, y, z, w);
        }

        /// <summary>
        /// Sets this quaternion to the rotation part of the given matrix</summary>
        /// <param name="m">Matrix</param>
        public void Set(Matrix4F m)
        {
            double ww = 0.25 * (m.M11 + m.M22 + m.M33 + 1.0f);

            if (ww >= 0)
            {
                if (ww >= EPS2)
                {
                    double wwSqrt = Math.Sqrt(ww);
                    W = (float)wwSqrt;
                    ww = 0.25 / wwSqrt;
                    X = (float)((m.M32 - m.M23) * ww);
                    Y = (float)((m.M13 - m.M31) * ww);
                    Z = (float)((m.M21 - m.M12) * ww);
                    return;
                }
            }
            else
            {
                W = 0;
                X = 0;
                Y = 0;
                Z = 1;
                return;
            }

            W = 0;
            ww = -0.5 * (m.M22 + m.M33);
            if (ww >= 0)
            {
                if (ww >= EPS2)
                {
                    double wwSqrt = Math.Sqrt(ww);
                    X = (float)wwSqrt;
                    ww = 0.5 / wwSqrt;
                    Y = (float)(m.M21 * ww);
                    Z = (float)(m.M31 * ww);
                    return;
                }
            }
            else
            {
                X = 0;
                Y = 0;
                Z = 1;
                return;
            }

            X = 0;
            ww = 0.5 * (1.0f - m.M33);
            if (ww >= EPS2)
            {
                double wwSqrt = Math.Sqrt(ww);
                Y = (float)wwSqrt;
                Z = (float)(m.M32 / (2.0 * wwSqrt));
            }

            Y = 0;
            Z = 1;
        }

        /// <summary>
        /// Creates a quaternion from an axis/angle rotation</summary>
        /// <param name="axis">Axis</param>
        /// <param name="angle">Angle</param>
        /// <returns>Quaternion equivalent to axis/angle rotation</returns>
        public static QuatF FromAxisAngle(Vec3F axis, float angle)
        {
            QuatF temp = new QuatF();
            double mag = Math.Sqrt(axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z);
            if (mag < EPS)
            {
                temp = new QuatF(0, 0, 0, 0);
            }
            else
            {
                double sin = Math.Sin(angle / 2.0);
                double scale = sin / mag;
                temp.W = (float)Math.Cos(angle / 2.0);
                temp.X = (float)(axis.X * scale);
                temp.Y = (float)(axis.Y * scale);
                temp.Z = (float)(axis.Z * scale);
            }
            return temp;
        }

        /// <summary>
        /// Sets this quaternion to the rotation of the given angle-axis</summary>
        /// <param name="a">Angle-axis</param>
        public void Set(AngleAxisF a)
        {
            double mag = Math.Sqrt(a.Axis.X * a.Axis.X + a.Axis.Y * a.Axis.Y + a.Axis.Z * a.Axis.Z);
            if (mag < EPS)
            {
                X = Y = Z = W = 0;
            }
            else
            {
                double sin = Math.Sin(a.Angle / 2.0);
                double scale = sin / mag;
                W = (float)Math.Cos(a.Angle / 2.0);
                X = (float)(a.Axis.X * scale);
                Y = (float)(a.Axis.Y * scale);
                Z = (float)(a.Axis.Z * scale);
            }
        }

        /// <summary>
        /// Obtains the spherical-linear-interpolation of the given quaternions and parameter</summary>
        /// <param name="q1">Quaternion 1</param>
        /// <param name="q2">Quaternion 2</param>
        /// <param name="t">Parameter</param>
        /// <returns>Spherical-linear-interpolation of the given quaternions and parameter</returns>
        public static QuatF Slerp(QuatF q1, QuatF q2, float t)
        {
            double dot = q2.X * q1.X + q2.Y * q1.Y + q2.Z * q1.Z + q2.W * q1.W;

            if (dot < 0)
                q1.X = -q1.X; q1.Y = -q1.Y; q1.Z = -q1.Z; q1.W = -q1.W;

            double s1, s2;
            if (1.0 - Math.Abs(dot) > EPS)
            {
                // slerp
                double omega = Math.Acos(dot);
                double sinom = Math.Sin(omega);
                s1 = Math.Sin((1.0 - t) * omega) / sinom;
                s2 = Math.Sin(t * omega) / sinom;
            }
            else
            {
                // lerp
                s1 = 1.0f - t;
                s2 = t;
            }
            return new QuatF(
                (float)(s1 * q1.X + s2 * q2.X),
                (float)(s1 * q1.Y + s2 * q2.Y),
                (float)(s1 * q1.Z + s2 * q2.Z),
                (float)(s1 * q1.W + s2 * q2.W));
        }

        /// <summary>
        /// Tests for equality to given quaternion</summary>
        /// <param name="q">Other quaternion</param>
        /// <returns>True iff quaternions are exactly equal</returns>
        public bool Equals(QuatF q)
        {
            return
                X == q.X &&
                Y == q.Y &&
                Z == q.Z &&
                W == q.W;
        }

        /// <summary>
        /// Tests for equality to another quaternion, within a given epsilon</summary>
        /// <param name="q">Other quaternion</param>
        /// <param name="eps">Epsilon, or margin for error</param>
        /// <returns>True iff all components are within epsilon</returns>
        public bool Equals(QuatF q, double eps)
        {
            return
                Math.Abs(X - q.X) < eps &&
                Math.Abs(Y - q.Y) < eps &&
                Math.Abs(Z - q.Z) < eps &&
                Math.Abs(W - q.W) < eps;
        }

        /// <summary>
        /// Converts quaternion to array of floats</summary>
        /// <returns>Array of 4 floats</returns>
        public float[] ToArray()
        {
            return new float[] { X, Y, Z, W };
        }


        #region Operator Overloads

        /// <summary>
        /// Gets the product of the given quaternions</summary>
        /// <param name="q1">Quaternion 1</param>
        /// <param name="q2">Quaternion 2</param>
        /// <returns>Product of given quaternions</returns>
        public static QuatF operator *(QuatF q1, QuatF q2)
        {
            return QuatF.Mul(q1, q2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Indicates whether this instance and a specified object are equal</summary>
        /// <param name="obj">Another object to compare to</param>
        /// <returns>True iff obj and this instance are the same type and represent the same value</returns>
        public override bool Equals(Object obj)
        {
            if (obj is QuatF)
            {
                QuatF other = (QuatF)obj;
                return Equals(this, other);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this instance</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance</returns>
        public override int GetHashCode()
        {
            long bits = 1;
            bits = 31 * bits + W.GetHashCode();
            bits = 31 * bits + X.GetHashCode();
            bits = 31 * bits + Y.GetHashCode();
            bits = 31 * bits + Z.GetHashCode();
            return (int)(bits ^ (bits >> 32));
        }

        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.QuatF structure</summary>
        /// <returns>A <see cref="T:System.String"></see> representing the quaternion</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary> 
        /// Returns the string representation of this Scea.VectorMath.QuatF structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the quaternion</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return X.ToString("R") + ", " + Y.ToString("R") + ", " + Z.ToString("R") + ", " + W.ToString("R");
            else
                return String.Format
                (
                     "({0}, {1}, {2}, {3})",
                     ((double)X).ToString(format, formatProvider),
                     ((double)Y).ToString(format, formatProvider),
                     ((double)Z).ToString(format, formatProvider),
                     ((double)W).ToString(format, formatProvider)
                 );

        }

        #endregion

        private const double EPS = 0.000001;
        private const double EPS2 = 1.0e-30;
    }
}
