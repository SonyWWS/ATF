//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// 3 x 3 matrix</summary>
    public class Matrix3F : IEquatable<Matrix3F>, IFormattable
    {
        /// <summary>
        /// The 9 elements of the matrix. The translation component is [M31, M32, M33].</summary>
        public float M11, M12, M13;
        /// <summary>Second row of matrix</summary>
        public float M21, M22, M23;
        /// <summary>Third row of matrix</summary>
        public float M31, M32, M33;

        /// <summary>
        /// Constructor, creates Identity matrix</summary>
        public Matrix3F()
        {
            M11 = 1; M12 = 0; M13 = 0;
            M21 = 0; M22 = 1; M23 = 0;
            M31 = 0; M32 = 0; M33 = 1;
        }

        /// <summary>
        /// Constructor from float components array</summary>
        /// <param name="m">Array of 9 floats</param>
        public Matrix3F(float[] m)
        {
            M11 = m[0]; M12 = m[1]; M13 = m[2];
            M21 = m[3]; M22 = m[4]; M23 = m[5];
            M31 = m[6]; M32 = m[7]; M33 = m[8];
        }

        /// <summary>
        /// Constructor from another matrix</summary>
        /// <param name="m">matrix to copy</param>
        public Matrix3F(Matrix3F m)
        {
            M11 = m.M11; M12 = m.M12; M13 = m.M13;
            M21 = m.M21; M22 = m.M22; M23 = m.M23;
            M31 = m.M31; M32 = m.M32; M33 = m.M33;
        }

        /// <summary>
        /// Constructor from float components</summary>
        public Matrix3F(
            float m00, float m01, float m02,
            float m10, float m11, float m12,
            float m20, float m21, float m22)
        {
            M11 = m00; M12 = m01; M13 = m02;
            M21 = m10; M22 = m11; M23 = m12;
            M31 = m20; M32 = m21; M33 = m22;
        }

        /// <summary>
        /// Gets the determinant of the matrix</summary>
        public double Determinant
        {
            get
            {
                return
                    M11 * (M22 * M33 - M23 * M32) +
                    M12 * (M23 * M31 - M21 * M33) +
                    M13 * (M21 * M32 - M22 * M31);
            }
        }

        /// <summary>
        /// Copies the 9 elements of this matrix to the given array</summary>
        /// <param name="array">Target array</param>
        /// <param name="index">First index in array, which receives M11</param>
        public void CopyTo(float[] array, int index)
        {
            array[index++] = M11;
            array[index++] = M12;
            array[index++] = M13;
            array[index++] = M21;
            array[index++] = M22;
            array[index++] = M23;
            array[index++] = M31;
            array[index++] = M32;
            array[index++] = M33;
        }

        /// <summary>
        /// Converts matrix to array of floats</summary>
        /// <returns>Array of 9 floats</returns>
        public float[] ToArray()
        {
            return new float[] {
                M11, M12, M13,
                M21, M22, M23,
                M31, M32, M33
            };
        }

        /// <summary>
        /// Gets or sets the i-th component of the matrix</summary>
        /// <param name="i">Component index, in [0..8]</param>
        /// <returns>i-th component of the matrix</returns>
        /// <remarks>This property is slow. Use the matrix elements directly, if possible.</remarks>
        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return M11;
                    case 1: return M12;
                    case 2: return M13;
                    case 3: return M21;
                    case 4: return M22;
                    case 5: return M23;
                    case 6: return M31;
                    case 7: return M32;
                    case 8: return M33;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0: M11 = value; break;
                    case 1: M12 = value; break;
                    case 2: M13 = value; break;
                    case 3: M21 = value; break;
                    case 4: M22 = value; break;
                    case 5: M23 = value; break;
                    case 6: M31 = value; break;
                    case 7: M32 = value; break;
                    case 8: M33 = value; break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Gets the ij-th component of the matrix</summary>
        /// <param name="i">Row index in [0..2]</param>
        /// <param name="j">Column index in [0..2]</param>
        /// <returns>ij-th component of the matrix</returns>
        /// <remarks>This property is slow. Use the matrix elements directly, if possible.</remarks>
        public float this[int i, int j]
        {
            get { return this[i * 3 + j]; }
            set { this[i * 3 + j] = value; }
        }

        /// <summary>
        /// Obtains the identity matrix</summary>
        public static readonly Matrix3F Identity = new Matrix3F();

        /// <summary>
        /// Obtains the zero matrix (all zeroes)</summary>
        public static readonly Matrix3F Zero = new Matrix3F(0, 0, 0, 0, 0, 0, 0, 0, 0);

        /// <summary>
        /// Sets the matrix to the given matrix</summary>
        /// <param name="m">Other matrix</param>
        public void Set(Matrix3F m)
        {
            M11 = m.M11; M12 = m.M12; M13 = m.M13;
            M21 = m.M21; M22 = m.M22; M23 = m.M23;
            M31 = m.M31; M32 = m.M32; M33 = m.M33;
        }

        /// <summary>
        /// Sets the matrix to the rotation specified by the given quaternion</summary>
        /// <param name="q">Quaternion representation of rotation</param>
        public void Set(QuatF q)
        {
            M11 = (float)(1.0 - 2.0 * q.Y * q.Y - 2.0 * q.Z * q.Z);
            M21 = (float)(2.0 * (q.X * q.Y + q.W * q.Z));
            M31 = (float)(2.0 * (q.X * q.Z - q.W * q.Y));

            M12 = (float)(2.0 * (q.X * q.Y - q.W * q.Z));
            M22 = (float)(1.0 - 2.0 * q.X * q.X - 2.0 * q.Z * q.Z);
            M32 = (float)(2.0 * (q.Y * q.Z + q.W * q.X));

            M13 = (float)(2.0 * (q.X * q.Z + q.W * q.Y));
            M23 = (float)(2.0 * (q.Y * q.Z - q.W * q.X));
            M33 = (float)(1.0 - 2.0 * q.X * q.X - 2.0 * q.Y * q.Y);
        }

        /// <summary>
        /// Sets the matrix to the rotation specified by the angle-axis</summary>
        /// <param name="a">Angle-axis representation of rotation</param>
        public void Set(AngleAxisF a)
        {
            double mag = Math.Sqrt(a.Axis.X * a.Axis.X + a.Axis.Y * a.Axis.Y + a.Axis.Z * a.Axis.Z);

            if (mag < EPS)
            {
                Set(Identity);
            }
            else
            {
                double ooMag = 1.0 / mag;
                double x = a.Axis.X * ooMag;
                double y = a.Axis.Y * ooMag;
                double z = a.Axis.Z * ooMag;

                double sin = Math.Sin(a.Angle);
                double cos = Math.Cos(a.Angle);
                double t = 1.0 - cos;

                double xz = a.Axis.X * a.Axis.Z;
                double xy = a.Axis.X * a.Axis.Y;
                double yz = a.Axis.Y * a.Axis.Z;

                M11 = (float)(t * x * x + cos);
                M12 = (float)(t * xy - sin * z);
                M13 = (float)(t * xz + sin * y);

                M21 = (float)(t * xy + sin * z);
                M22 = (float)(t * y * y + cos);
                M23 = (float)(t * yz - sin * x);

                M31 = (float)(t * xz - sin * y);
                M32 = (float)(t * yz + sin * x);
                M33 = (float)(t * z * z + cos);
            }
        }

        #region Matrix Building

        /// <summary>
        /// Sets this matrix to a uniform scale</summary>
        /// <param name="scale">Scale factor</param>
        public void UniformScale(double scale)
        {
            M11 = (float)scale;
            M12 = 0;
            M13 = 0;

            M21 = 0;
            M22 = (float)scale;
            M23 = 0;

            M31 = 0;
            M32 = 0;
            M33 = (float)scale;
        }

        /// <summary>
        /// Sets this matrix to a non-uniform scale by the given vector</summary>
        /// <param name="scale">Vector representing x, y, and z scales</param>
        public void Scale(Vec3F scale)
        {
            M12 = M13 =
            M21 = M23 =
            M31 = M32 = 0;

            M11 = scale.X;
            M22 = scale.Y;
            M33 = scale.Z;
        }

        /// <summary>
        /// Obtains the scale component of the matrix</summary>
        /// <returns>Scale component of the matrix</returns>
        public Vec3F GetScale()
        {
            return new Vec3F(
                XAxis.Length,
                YAxis.Length,
                ZAxis.Length);
        }

        /// <summary>
        /// Gets and sets the first row or basis vector, which can be considered the local x-axis
        /// that has been transformed to the destination coordinate system</summary>
        public Vec3F XAxis
        {
            get { return new Vec3F(M11, M12, M13); }
            set { M11 = value.X; M12 = value.Y; M13 = value.Z; }
        }

        /// <summary>
        /// Gets and sets the second row or basis vector, which can be considered the local y-axis
        /// that has been transformed to the destination coordinate system</summary>
        public Vec3F YAxis
        {
            get { return new Vec3F(M21, M22, M23); }
            set { M21 = value.X; M22 = value.Y; M23 = value.Z; }
        }

        /// <summary>
        /// Gets and sets the third row or basis vector, which can be considered the local z-axis
        /// that has been transformed to the destination coordinate system</summary>
        public Vec3F ZAxis
        {
            get { return new Vec3F(M31, M32, M33); }
            set { M31 = value.X; M32 = value.Y; M33 = value.Z; }
        }

        /// <summary>
        /// Sets this matrix to the product of the x, y, and z rotations specified by
        /// the given vector</summary>
        /// <param name="rotation">Vector representing x, y, and z rotations</param>
        public void Rotation(Vec3F rotation)
        {
            RotX(rotation.X);
            s_rotationTemp.RotY(rotation.Y);
            Mul(this, s_rotationTemp);
            s_rotationTemp.RotZ(rotation.Z);
            Mul(this, s_rotationTemp);
        }
        private static readonly Matrix3F s_rotationTemp = new Matrix3F();//only for Rotation(Vec3F)

        /// <summary>
        /// Builds rotation about x-axis</summary>
        /// <param name="angle">Angle in radians</param>
        public void RotX(double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            M11 = 1;
            M12 = 0;
            M13 = 0;

            M21 = 0;
            M22 = (float)cos;
            M23 = (float)sin;

            M31 = 0;
            M32 = (float)-sin;
            M33 = (float)cos;
        }

        /// <summary>
        /// Builds rotation about y-axis</summary>
        /// <param name="angle">Angle in radians</param>
        public void RotY(double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            M11 = (float)cos;
            M12 = 0;
            M13 = (float)-sin;

            M21 = 0;
            M22 = 1;
            M23 = 0;

            M31 = (float)sin;
            M32 = 0;
            M33 = (float)cos;
        }

        /// <summary>
        /// Builds rotation about z-axis</summary>
        /// <param name="angle">Angle in radians</param>
        public void RotZ(double angle)
        {
            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            M11 = (float)cos;
            M12 = (float)sin;
            M13 = 0;

            M21 = (float)-sin;
            M22 = (float)cos;
            M23 = 0;

            M31 = 0;
            M32 = 0;
            M33 = 1;
        }

        #endregion

        #region Matrix Arithmetic

        /// <summary>
        /// Sets this matrix to the given matrix, scaled by the given value</summary>
        /// <param name="m">Other matrix</param>
        /// <param name="scale">Scale factor</param>
        public void Mul(Matrix3F m, float scale)
        {
            M11 = scale * m.M11;
            M12 = scale * m.M12;
            M13 = scale * m.M13;

            M21 = scale * m.M21;
            M22 = scale * m.M22;
            M23 = scale * m.M23;

            M31 = scale * m.M31;
            M32 = scale * m.M32;
            M33 = scale * m.M33;
        }

        /// <summary>
        /// Multiplies the two given matrices and sets this to the product</summary>
        /// <param name="m1">Left matrix</param>
        /// <param name="m2">Right matrix</param>
        /// <remarks>It's safe to pass 'this' as one of the arguments</remarks>
        public void Mul(Matrix3F m1, Matrix3F m2)
        {
            float m00 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31;
            float m01 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32;
            float m02 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33;

            float m10 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31;
            float m11 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32;
            float m12 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33;

            float m20 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31;
            float m21 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32;
            float m22 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33;

            M11 = m00; M12 = m01; M13 = m02;
            M21 = m10; M22 = m11; M23 = m12;
            M31 = m20; M32 = m21; M33 = m22;
        }

        /// <summary>
        /// Sets this matrix to the inverse of the given matrix</summary>
        /// <param name="m">Other matrix</param>
        /// <remarks>If you know that this matrix only contains rotations, the
        /// inverse is simply the transpose.</remarks>
        public void Invert(Matrix3F m)
        {
            float m11 = -m.M23 * m.M32 + m.M22 * m.M33;
            float m12 = m.M13 * m.M32 - m.M12 * m.M33;
            float m13 = -m.M13 * m.M22 + m.M12 * m.M23;
            float m21 = m.M23 * m.M31 - m.M21 * m.M33;
            float m22 = -m.M13 * m.M31 + m.M11 * m.M33;
            float m23 = m.M13 * m.M21 - m.M11 * m.M23;
            float m31 = -m.M22 * m.M31 + m.M21 * m.M32;
            float m32 = m.M12 * m.M31 - m.M11 * m.M32;
            float m33 = -m.M12 * m.M21 + m.M11 * m.M22;

            // in case 'm' is 'this', compute determinant now
            float oneOverDeterminant = (float)(1.0 / m.Determinant);

            M11 = m11; M12 = m12; M13 = m13;
            M21 = m21; M22 = m22; M23 = m23;
            M31 = m31; M32 = m32; M33 = m33;

            Mul(this, oneOverDeterminant);
        }

        /// <summary>
        /// Sets this matrix to the transpose of the given matrix</summary>
        /// <param name="m">Other matrix</param>
        public void Transpose(Matrix3F m)
        {
            // use a temporary in case m == this
            float temp;

            temp = m.M21; M21 = m.M12; M12 = temp;
            temp = m.M31; M31 = m.M13; M13 = temp;
            temp = m.M32; M32 = m.M23; M23 = temp;
        }

        /// <summary>
        /// Sets this matrix to the given matrix after normalizing it</summary>
        /// <param name="m">Un-normalized matrix</param>
        public void Normalize(Matrix3F m)
        {
            double mag = 1.0 / Math.Sqrt(m.M11 * m.M11 + m.M21 * m.M21 + m.M31 * m.M31);
            M11 = (float)(m.M11 * mag);
            M21 = (float)(m.M21 * mag);
            M31 = (float)(m.M31 * mag);

            mag = 1.0 / Math.Sqrt(m.M12 * m.M12 + m.M22 * m.M22 + m.M32 * m.M32);
            M12 = (float)(m.M12 * mag);
            M22 = (float)(m.M22 * mag);
            M32 = (float)(m.M32 * mag);

            M13 = M21 * M32 - M22 * M31;
            M23 = M12 * M31 - M11 * M32;
            M33 = M11 * M22 - M12 * M21;
        }

        /// <summary>
        /// Sets this matrix to the negation of the given matrix</summary>
        /// <param name="m">Other matrix</param>
        public void Negate(Matrix3F m)
        {
            M11 = -m.M11;
            M12 = -m.M12;
            M13 = -m.M13;

            M21 = -m.M21;
            M22 = -m.M22;
            M23 = -m.M23;

            M31 = -m.M31;
            M32 = -m.M32;
            M33 = -m.M33;
        }

        /// <summary>
        /// Transforms the given vector</summary>
        /// <param name="v">Input vector</param>
        /// <param name="result">Output vector</param>
        public void Transform(Vec3F v, out Vec3F result)
        {
            float x = M11 * v.X + M12 * v.Y + M13 * v.Z;
            float y = M21 * v.X + M22 * v.Y + M23 * v.Z;
            float z = M31 * v.X + M32 * v.Y + M33 * v.Z;
            result.X = x;
            result.Y = y;
            result.Z = z;
        }

        #endregion

        /// <summary>
        /// Gets the Euler angle representation of the matrix</summary>
        /// <param name="x">X rotation</param>
        /// <param name="y">Y rotation</param>
        /// <param name="z">Z rotation</param>
        public void GetEulerAngles(out float x, out float y, out float z)
        {
            Vec3F wk = new Vec3F(M11 * M11, M12 * M12, M13 * M13);
            float cY = (float)Math.Sqrt(wk.X + wk.Y);

            if (cY > 1e-6f)
            {
                x = (float)Math.Atan2(M23, M33);
                y = (float)Math.Atan2(-M13, cY);
                z = (float)Math.Atan2(M12, M11);
            }
            else
            {
                x = (float)Math.Atan2(-M33, M22);
                y = (float)Math.Atan2(-M13, cY);
                z = 0.0f;
            }
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
                "{0}{9} {1}{9} {2}{9} {3}{9} {4}{9} {5}{9} {6}{9} {7}{9} {8}",
                M11.ToString(format, formatProvider),
                M12.ToString(format, formatProvider),
                M13.ToString(format, formatProvider),
                M21.ToString(format, formatProvider),
                M22.ToString(format, formatProvider),
                M23.ToString(format, formatProvider),
                M31.ToString(format, formatProvider),
                M32.ToString(format, formatProvider),
                M33.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

        /// <summary>
        /// Tests exact equality with the given matrix</summary>
        /// <param name="m">Other matrix</param>
        /// <returns>True iff matrices are exactly equal</returns>
        public bool Equals(Matrix3F m)
        {
            return
                M11 == m.M11 && M12 == m.M12 && M13 == m.M13 &&
                M21 == m.M21 && M22 == m.M22 && M23 == m.M23 &&
                M31 == m.M31 && M32 == m.M32 && M33 == m.M33;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see></summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see></param>
        /// <returns>True iff the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see></returns>
        public override bool Equals(object obj)
        {
            if (obj is Matrix3F)
            {
                Matrix3F other = (Matrix3F)obj;
                return Equals(other);
            }
            return false;
        }

        /// <summary>
        /// Tests equality to another matrix within a given epsilon</summary>
        /// <param name="m">Other matrix</param>
        /// <param name="eps">Epsilon, or margin for error</param>
        /// <returns>True iff all components of the two matrices are within epsilon</returns>
        public bool EpsilonEquals(Matrix3F m, double eps)
        {
            return
                Math.Abs(M11 - m.M11) < eps &&
                Math.Abs(M12 - m.M12) < eps &&
                Math.Abs(M13 - m.M13) < eps &&
                Math.Abs(M21 - m.M21) < eps &&
                Math.Abs(M22 - m.M22) < eps &&
                Math.Abs(M23 - m.M23) < eps &&
                Math.Abs(M31 - m.M31) < eps &&
                Math.Abs(M32 - m.M32) < eps &&
                Math.Abs(M33 - m.M33) < eps;
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table</summary>
        /// <returns>A hash code for the current <see cref="T:System.Object"></see></returns>
        public override int GetHashCode()
        {
            long bits = 1;
            bits = 31 * bits + M11.GetHashCode();
            bits = 31 * bits + M12.GetHashCode();
            bits = 31 * bits + M13.GetHashCode();
            bits = 31 * bits + M21.GetHashCode();
            bits = 31 * bits + M22.GetHashCode();
            bits = 31 * bits + M23.GetHashCode();
            bits = 31 * bits + M31.GetHashCode();
            bits = 31 * bits + M32.GetHashCode();
            bits = 31 * bits + M33.GetHashCode();
            return (int)(bits ^ (bits >> 32));
        }

        private const double EPS = 0.000001;
    }
}
