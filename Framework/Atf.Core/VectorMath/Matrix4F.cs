//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Represents a 4x4 matrix that is equivalent to OpenGl's 4x4 matrix and is laid out
    /// in memory the same way as in OpenGl. The translation components are M41, M42, and M43.</summary>
    /// <remarks>See http://www.opengl.org/resources/faq/technical/transformations.htm </remarks>
    public class Matrix4F : IEquatable<Matrix4F>, IFormattable
    {
        /// <summary>
        /// The 16 elements of the matrix. The translation component is [M41, M42, M43].</summary>
        public float M11, M12, M13, M14;
        /// <summary>Second row of matrix</summary>
        public float M21, M22, M23, M24;
        /// <summary>Third row of matrix</summary>
        public float M31, M32, M33, M34;
        /// <summary>Fourth row of matrix</summary>
        public float M41, M42, M43, M44;

        /// <summary>
        /// Constructor, creates identity matrix</summary>
        public Matrix4F()
        {
            M11 = 1; M12 = 0; M13 = 0; M14 = 0;
            M21 = 0; M22 = 1; M23 = 0; M24 = 0;
            M31 = 0; M32 = 0; M33 = 1; M34 = 0;
            M41 = 0; M42 = 0; M43 = 0; M44 = 1;
        }

        /// <summary>
        /// Constructor using array of floats</summary>
        /// <param name="m">Array of 16 floats</param>
        public Matrix4F(float[] m)
        {
            Set(m);
        }

        /// <summary>
        /// Constructor, uses 16 floats</summary>
        /// <param name="m11">First row component</param>
        /// <param name="m12">First row component</param>
        /// <param name="m13">First row component</param>
        /// <param name="m14">First row component</param>
        /// <param name="m21">Second row component</param>
        /// <param name="m22">Second row component</param>
        /// <param name="m23">Second row component</param>
        /// <param name="m24">Second row component</param>
        /// <param name="m31">Third row component</param>
        /// <param name="m32">Third row component</param>
        /// <param name="m33">Third row component</param>
        /// <param name="m34">Third row component</param>
        /// <param name="m41">Fourth row component</param>
        /// <param name="m42">Fourth row component</param>
        /// <param name="m43">Fourth row component</param>
        /// <param name="m44">Fourth row component</param>
        public Matrix4F(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        /// <summary>
        /// Constructor using a translation to embed</summary>
        /// <param name="translation">Translation to embed in 1x3 bottom-left sub-matrix</param>
        public Matrix4F(Vec3F translation)
        {
            Set(translation);
        }

        /// <summary>
        /// Constructor, given matrix to embed</summary>
        /// <param name="m">Matrix to embed in 3x3 top-left sub-matrix</param>
        public Matrix4F(Matrix3F m)
        {
            Set(m);
        }

        /// <summary>
        /// Constructor, copying another matrix</summary>
        /// <param name="m">Matrix to copy</param>
        public Matrix4F(Matrix4F m)
        {
            Set(m);
        }

        /// <summary>
        /// Constructs a matrix with the same rotation as the given quaternion</summary>
        /// <param name="q">Quaternion representing rotation</param>
        public Matrix4F(QuatF q)
        {
            Set(q);
        }

        /// <summary>
        /// Constructs a matrix with the same rotation as the given angle axis</summary>
        /// <param name="angleAxis">AngleAxis representing rotation</param>
        public Matrix4F(AngleAxisF angleAxis)
        {
            Set(angleAxis);
        }

        /// <summary>
        /// Gets a new matrix object that is the identity matrix</summary>
        public static Matrix4F Identity
        {
            get { return new Matrix4F(); }
        }

        /// <summary>
        /// Gets a new matrix that is all zeroes</summary>
        public static Matrix4F Zero
        {
            get { return new Matrix4F(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0); }
        }

        /// <summary>
        /// Sets this matrix to the translation specified by the given vector</summary>
        /// <param name="translation">Vector representing x, y, and z translations</param>
        public void Set(Vec3F translation)
        {
            M11 = M22 = M33 = M44 = 1;
            M12 = M13 = M14 = M21 = M23 = M24 = M31 = M32 = M34 = 0;
            M41 = translation.X;
            M42 = translation.Y;
            M43 = translation.Z;
        }

        /// <summary>
        /// Sets the rotation part of the matrix to the given rotation sub-matrix</summary>
        /// <param name="m">Rotation sub-matrix</param>
        public void Set(Matrix3F m)
        {
            M11 = m.M11; M12 = m.M12; M13 = m.M13; M14 = 0;
            M21 = m.M21; M22 = m.M22; M23 = m.M23; M24 = 0;
            M31 = m.M31; M32 = m.M32; M33 = m.M33; M34 = 0;
            M41 = 0; M42 = 0; M43 = 0; M44 = 1;
        }

        /// <summary>
        /// Sets the matrix to the given matrix</summary>
        /// <param name="m">Other matrix</param>
        public void Set(Matrix4F m)
        {
            M11 = m.M11; M12 = m.M12; M13 = m.M13; M14 = m.M14;
            M21 = m.M21; M22 = m.M22; M23 = m.M23; M24 = m.M24;
            M31 = m.M31; M32 = m.M32; M33 = m.M33; M34 = m.M34;
            M41 = m.M41; M42 = m.M42; M43 = m.M43; M44 = m.M44;
        }

        /// <summary>
        /// Sets the matrix to the given components</summary>
        /// <param name="m11">First row component 1</param>
        /// <param name="m12">First row component 2</param>
        /// <param name="m13">First row component 3</param>
        /// <param name="m14">First row component 4</param>
        /// <param name="m21">Second row component 1</param>
        /// <param name="m22">Second row component 2</param>
        /// <param name="m23">Second row component 3</param>
        /// <param name="m24">Second row component 4</param>
        /// <param name="m31">Third row component 1</param>
        /// <param name="m32">Third row component 2</param>
        /// <param name="m33">Third row component 3</param>
        /// <param name="m34">Third row component 4</param>
        /// <param name="m41">Fourth row component 1</param>
        /// <param name="m42">Fourth row component 2</param>
        /// <param name="m43">Fourth row component 3</param>
        /// <param name="m44">Fourth row component 4</param>
        public void Set(
            float m11, float m12, float m13, float m14,
            float m21, float m22, float m23, float m24,
            float m31, float m32, float m33, float m34,
            float m41, float m42, float m43, float m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        /// <summary>
        /// Sets the matrix from the given array</summary>
        /// <param name="m">Array of floats</param>
        public void Set(float[] m)
        {
            M11 = m[0]; M12 = m[1]; M13 = m[2]; M14 = m[3];
            M21 = m[4]; M22 = m[5]; M23 = m[6]; M24 = m[7];
            M31 = m[8]; M32 = m[9]; M33 = m[10]; M34 = m[11];
            M41 = m[12]; M42 = m[13]; M43 = m[14]; M44 = m[15];
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

            M14 = M24 = M34 = M41 = M42 = M43 = 0;
            M44 = 1;
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

                M14 = M24 = M34 = M41 = M42 = M43 = 0;
                M44 = 1;
            }
        }

        /// <summary>
        /// Copies the 16 elements of this matrix to the given array</summary>
        /// <param name="array">Target array</param>
        /// <param name="index">First index in array, which receives M11</param>
        public void CopyTo(float[] array, int index)
        {
            array[index++] = M11;
            array[index++] = M12;
            array[index++] = M13;
            array[index++] = M14;
            array[index++] = M21;
            array[index++] = M22;
            array[index++] = M23;
            array[index++] = M24;
            array[index++] = M31;
            array[index++] = M32;
            array[index++] = M33;
            array[index++] = M34;
            array[index++] = M41;
            array[index++] = M42;
            array[index++] = M43;
            array[index] = M44;
        }

        /// <summary>
        /// Converts matrix to array of floats</summary>
        /// <returns>Array of 16 floats. X, Y, and Z translation are indices 12,13, and 14.</returns>
        public float[] ToArray()
        {
            return new float[] {
                M11, M12, M13, M14,
                M21, M22, M23, M24,
                M31, M32, M33, M34,
                M41, M42, M43, M44
            };
        }

        /// <summary>
        /// Gets the i-th component of the matrix</summary>
        /// <param name="i">Component index, in [0..15]</param>
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
                    case 3: return M14;
                    case 4: return M21;
                    case 5: return M22;
                    case 6: return M23;
                    case 7: return M24;
                    case 8: return M31;
                    case 9: return M32;
                    case 10: return M33;
                    case 11: return M34;
                    case 12: return M41;
                    case 13: return M42;
                    case 14: return M43;
                    case 15: return M44;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (i)
                {
                    case 0: M11 = value; return;
                    case 1: M12 = value; return;
                    case 2: M13 = value; return;
                    case 3: M14 = value; return;
                    case 4: M21 = value; return;
                    case 5: M22 = value; return;
                    case 6: M23 = value; return;
                    case 7: M24 = value; return;
                    case 8: M31 = value; return;
                    case 9: M32 = value; return;
                    case 10: M33 = value; return;
                    case 11: M34 = value; return;
                    case 12: M41 = value; return;
                    case 13: M42 = value; return;
                    case 14: M43 = value; return;
                    case 15: M44 = value; return;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Gets the ij-th component of the matrix</summary>
        /// <param name="i">Row index, in [0..3]</param>
        /// <param name="j">Column index, in [0..3]</param>
        /// <returns>ij-th component of the matrix</returns>
        /// <remarks>This property is slow. Use the matrix elements directly, if possible.</remarks>
        public float this[int i, int j]
        {
            get { return this[i * 4 + j]; }
            set { this[i * 4 + j] = value; }
        }

        /// <summary>
        /// Creates a translation matrix</summary>
        /// <param name="translation">Translation</param>
        /// <returns>Translation matrix</returns>
        public static Matrix4F CreateTranslation(Vec3F translation)
        {
            Matrix4F temp = new Matrix4F();
            temp.Set(translation);
            return temp;
        }


        /// <summary>
        /// Sets this matrix to a non-uniform scale by the given number</summary>
        /// <param name="s">Scale</param>
        public void Scale(float s)
        {
            M12 = M13 = M14 =
            M21 = M23 = M24 =
            M31 = M32 = M34 =
            M41 = M42 = M43 = 0;
            M44 = 1.0f;

            M11 = s;
            M22 = s;
            M33 = s;
        }

        /// <summary>
        /// Sets this matrix to a non-uniform scale by the given vector</summary>
        /// <param name="x">Vector x component</param>
        /// <param name="y">Vector y component</param>
        /// <param name="z">Vector z component</param>
        public void Scale(float x, float y, float z)
        {
            M12 = M13 = M14 =
            M21 = M23 = M24 =
            M31 = M32 = M34 =
            M41 = M42 = M43 = 0;
            M44 = 1.0f;

            M11 = x;
            M22 = y;
            M33 = z;
        }

        /// <summary>
        /// Sets this matrix to a non-uniform scale by the given vector</summary>
        /// <param name="scale">Vector representing x, y, and z scales</param>
        public void Scale(Vec3F scale)
        {
            M12 = M13 = M14 =
            M21 = M23 = M24 =
            M31 = M32 = M34 =
            M41 = M42 = M43 = 0;
            M44 = 1.0f;

            M11 = scale.X;
            M22 = scale.Y;
            M33 = scale.Z;
        }

        /// <summary>
        /// Returns the scale component of the matrix</summary>
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
        /// Gets and sets the translation component of the matrix</summary>
        public Vec3F Translation
        {
            get { return new Vec3F(M41, M42, M43); }
            set { M41 = value.X; M42 = value.Y; M43 = value.Z; }
        }

        /// <summary>
        /// Gets and sets the x-axis translation component</summary>
        public float XTranslation
        {
            get { return M41; }
            set { M41 = value; }
        }

        /// <summary>
        /// Gets and sets the y-axis translation component</summary>
        public float YTranslation
        {
            get { return M42; }
            set { M42 = value; }
        }

        /// <summary>
        /// Gets and sets the z-axis translation component</summary>
        public float ZTranslation
        {
            get { return M43; }
            set { M43 = value; }
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
        private static readonly Matrix4F s_rotationTemp = new Matrix4F(); //only to be used by Rotation(Vec3F)

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

            M14 = M24 = M34 = M41 = M42 = M43 = 0;
            M44 = 1;
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

            M14 = M24 = M34 = M41 = M42 = M43 = 0;
            M44 = 1;
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

            M14 = M24 = M34 = M41 = M42 = M43 = 0;
            M44 = 1;
        }

        /// <summary>
        /// Builds rotation matrix about given axis right-hand system</summary>
        /// <param name="axis">Axis of rotation</param>
        /// <param name="angle">Angle in radians</param>
        /// <returns>Rotation matrix</returns>
        public static Matrix4F RotAxisRH(Vec3F axis, double angle)
        {
            float c = (float)Math.Cos(angle);
            float s = (float)Math.Sin(angle);
            float t = (1.0f - c);
            Matrix4F m = new Matrix4F();
            float x = axis.X;
            float y = axis.Y;
            float z = axis.Z;

            m.Set(c + t * x * x, t * x * y + s * z, t * x * z - s * y, 0,
                t * x * y - s * z, c + t * y * y, t * y * z + s * x, 0,
                t * x * z + s * y, t * y * z - s * x, c + t * z * z, 0,
                0, 0, 0, 1);

            return m;

        }

        /// <summary>
        /// Gets the 3x3 rotation sub-matrix of this 4x4 matrix</summary>
        /// <param name="result">Will be set to the 3x3 rotation sub-matrix</param>
        public void GetRotation(Matrix3F result)
        {
            result.M11 = M11; result.M12 = M12; result.M13 = M13;
            result.M21 = M21; result.M22 = M22; result.M23 = M23;
            result.M31 = M31; result.M32 = M32; result.M33 = M33;
        }

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
        /// Sets this matrix to the given matrix, scaled by the given value</summary>
        /// <param name="m">Other matrix</param>
        /// <param name="scale">Scale factor</param>
        public void Mul(Matrix4F m, float scale)
        {
            M11 = scale * m.M11;
            M12 = scale * m.M12;
            M13 = scale * m.M13;
            M14 = scale * m.M14;

            M21 = scale * m.M21;
            M22 = scale * m.M22;
            M23 = scale * m.M23;
            M24 = scale * m.M24;

            M31 = scale * m.M31;
            M32 = scale * m.M32;
            M33 = scale * m.M33;
            M34 = scale * m.M34;

            M41 = scale * m.M41;
            M42 = scale * m.M42;
            M43 = scale * m.M43;
            M44 = scale * m.M44;
        }

        /// <summary>
        /// Multiplies the two given matrices and sets this to the product. Follows the
        /// convention of the left matrix being the first operation and the right matrix
        /// being the second operation. For example, if A rotates 90 degrees and B translates
        /// by x=5, then A * B will first rotate and then translate.</summary>
        /// <param name="m1">Left matrix</param>
        /// <param name="m2">Right matrix</param>
        /// <remarks>It's safe to pass 'this' as one of the arguments</remarks>
        public void Mul(Matrix4F m1, Matrix4F m2)
        {
            // it's OK if m1 is same object as m2, but not if either of them is 'this'
            // case #1: a.Mul(a,a) ==> a.Mul(temp,temp)
            // case #2: a.Mul(a,b) ==> a.Mul(temp,b)
            // case #3: a.Mul(b,a) ==> a.Mul(b,temp)
            // case #4: a.Mul(b,c) ==> a.Mul(b,c)
            if (m1 == this)
            {
                s_mulTemp.Set(m1);
                m1 = s_mulTemp;
                if (m2 == this)
                    m2 = s_mulTemp;
            }
            else if (m2 == this)
            {
                s_mulTemp.Set(m2);
                m2 = s_mulTemp;
            }

            M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
            M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
            M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
            M14 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;

            M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
            M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
            M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
            M24 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;

            M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
            M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
            M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
            M34 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;

            M41 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
            M42 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
            M43 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
            M44 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;
        }
        private static readonly Matrix4F s_mulTemp = new Matrix4F(); //only to be used by Mul(Matrix4F, Matrix4F)

        /// <summary>
        /// Sets this matrix to the inverse of the given matrix</summary>
        /// <param name="m">Other matrix</param>
        public void Invert(Matrix4F m)
        {
            float m11 = m.M23 * m.M34 * m.M42 - m.M24 * m.M33 * m.M42 + m.M24 * m.M32 * m.M43 - m.M22 * m.M34 * m.M43 - m.M23 * m.M32 * m.M44 + m.M22 * m.M33 * m.M44;
            float m12 = m.M14 * m.M33 * m.M42 - m.M13 * m.M34 * m.M42 - m.M14 * m.M32 * m.M43 + m.M12 * m.M34 * m.M43 + m.M13 * m.M32 * m.M44 - m.M12 * m.M33 * m.M44;
            float m13 = m.M13 * m.M24 * m.M42 - m.M14 * m.M23 * m.M42 + m.M14 * m.M22 * m.M43 - m.M12 * m.M24 * m.M43 - m.M13 * m.M22 * m.M44 + m.M12 * m.M23 * m.M44;
            float m14 = m.M14 * m.M23 * m.M32 - m.M13 * m.M24 * m.M32 - m.M14 * m.M22 * m.M33 + m.M12 * m.M24 * m.M33 + m.M13 * m.M22 * m.M34 - m.M12 * m.M23 * m.M34;
            float m21 = m.M24 * m.M33 * m.M41 - m.M23 * m.M34 * m.M41 - m.M24 * m.M31 * m.M43 + m.M21 * m.M34 * m.M43 + m.M23 * m.M31 * m.M44 - m.M21 * m.M33 * m.M44;
            float m22 = m.M13 * m.M34 * m.M41 - m.M14 * m.M33 * m.M41 + m.M14 * m.M31 * m.M43 - m.M11 * m.M34 * m.M43 - m.M13 * m.M31 * m.M44 + m.M11 * m.M33 * m.M44;
            float m23 = m.M14 * m.M23 * m.M41 - m.M13 * m.M24 * m.M41 - m.M14 * m.M21 * m.M43 + m.M11 * m.M24 * m.M43 + m.M13 * m.M21 * m.M44 - m.M11 * m.M23 * m.M44;
            float m24 = m.M13 * m.M24 * m.M31 - m.M14 * m.M23 * m.M31 + m.M14 * m.M21 * m.M33 - m.M11 * m.M24 * m.M33 - m.M13 * m.M21 * m.M34 + m.M11 * m.M23 * m.M34;
            float m31 = m.M22 * m.M34 * m.M41 - m.M24 * m.M32 * m.M41 + m.M24 * m.M31 * m.M42 - m.M21 * m.M34 * m.M42 - m.M22 * m.M31 * m.M44 + m.M21 * m.M32 * m.M44;
            float m32 = m.M14 * m.M32 * m.M41 - m.M12 * m.M34 * m.M41 - m.M14 * m.M31 * m.M42 + m.M11 * m.M34 * m.M42 + m.M12 * m.M31 * m.M44 - m.M11 * m.M32 * m.M44;
            float m33 = m.M12 * m.M24 * m.M41 - m.M14 * m.M22 * m.M41 + m.M14 * m.M21 * m.M42 - m.M11 * m.M24 * m.M42 - m.M12 * m.M21 * m.M44 + m.M11 * m.M22 * m.M44;
            float m34 = m.M14 * m.M22 * m.M31 - m.M12 * m.M24 * m.M31 - m.M14 * m.M21 * m.M32 + m.M11 * m.M24 * m.M32 + m.M12 * m.M21 * m.M34 - m.M11 * m.M22 * m.M34;
            float m41 = m.M23 * m.M32 * m.M41 - m.M22 * m.M33 * m.M41 - m.M23 * m.M31 * m.M42 + m.M21 * m.M33 * m.M42 + m.M22 * m.M31 * m.M43 - m.M21 * m.M32 * m.M43;
            float m42 = m.M12 * m.M33 * m.M41 - m.M13 * m.M32 * m.M41 + m.M13 * m.M31 * m.M42 - m.M11 * m.M33 * m.M42 - m.M12 * m.M31 * m.M43 + m.M11 * m.M32 * m.M43;
            float m43 = m.M13 * m.M22 * m.M41 - m.M12 * m.M23 * m.M41 - m.M13 * m.M21 * m.M42 + m.M11 * m.M23 * m.M42 + m.M12 * m.M21 * m.M43 - m.M11 * m.M22 * m.M43;
            float m44 = m.M12 * m.M23 * m.M31 - m.M13 * m.M22 * m.M31 + m.M13 * m.M21 * m.M32 - m.M11 * m.M23 * m.M32 - m.M12 * m.M21 * m.M33 + m.M11 * m.M22 * m.M33;

            // in case 'm' is 'this', compute determinant now
            float oneOverDeterminant = (float)(1.0 / m.Determinant);

            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;

            Mul(this, oneOverDeterminant);
        }

        /// <summary>
        /// Sets this matrix to the transpose of the given matrix</summary>
        /// <param name="m">The matrix to take the transpose of</param>
        public void Transpose(Matrix4F m)
        {
            float m11 = m.M11;
            float m12 = m.M21;
            float m13 = m.M31;
            float m14 = m.M41;
            float m21 = m.M12;
            float m22 = m.M22;
            float m23 = m.M32;
            float m24 = m.M42;
            float m31 = m.M13;
            float m32 = m.M23;
            float m33 = m.M33;
            float m34 = m.M43;
            float m41 = m.M14;
            float m42 = m.M24;
            float m43 = m.M34;
            float m44 = m.M44;

            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }

        /// <summary>
        /// Gets the determinant of the matrix</summary>
        public double Determinant
        {
            get
            {
                return
                    M14 * M23 * M32 * M41 - M13 * M24 * M32 * M41 - M14 * M22 * M33 * M41 + M12 * M24 * M33 * M41 +
                    M13 * M22 * M34 * M41 - M12 * M23 * M34 * M41 - M14 * M23 * M31 * M42 + M13 * M24 * M31 * M42 +
                    M14 * M21 * M33 * M42 - M11 * M24 * M33 * M42 - M13 * M21 * M34 * M42 + M11 * M23 * M34 * M42 +
                    M14 * M22 * M31 * M43 - M12 * M24 * M31 * M43 - M14 * M21 * M32 * M43 + M11 * M24 * M32 * M43 +
                    M12 * M21 * M34 * M43 - M11 * M22 * M34 * M43 - M13 * M22 * M31 * M44 + M12 * M23 * M31 * M44 +
                    M13 * M21 * M32 * M44 - M11 * M23 * M32 * M44 - M12 * M21 * M33 * M44 + M11 * M22 * M33 * M44;
            }
        }

        /// <summary>
        /// Sets this matrix to the given matrix after normalizing it</summary>
        /// <param name="m">Un-normalized matrix</param>
        public void Normalize(Matrix4F m)
        {
            double mag = 1.0 / Math.Sqrt(m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13);
            M11 = (float)(m.M11 * mag);
            M12 = (float)(m.M12 * mag);
            M13 = (float)(m.M13 * mag);

            mag = 1.0 / Math.Sqrt(m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23);
            M21 = (float)(m.M21 * mag);
            M22 = (float)(m.M22 * mag);
            M23 = (float)(m.M23 * mag);

            mag = 1.0 / Math.Sqrt(m.M31 * m.M31 + m.M32 * m.M32 + m.M33 * m.M33);
            M31 = (float)(m.M31 * mag);
            M32 = (float)(m.M32 * mag);
            M33 = (float)(m.M33 * mag);

            M41 = m.M41;
            M42 = m.M42;
            M43 = m.M43;
        }

        /// <summary>
        /// Orthonormalizes the matrix</summary>
        /// <param name="m">The matrix to orthonormalize</param>
        public void OrthoNormalize(Matrix4F m)
        {
            Vec3F xAxis = new Vec3F(m.M11, m.M12, m.M13);
            Vec3F yAxis = new Vec3F(m.M21, m.M22, m.M23);
            Vec3F zAxis;

            zAxis = Vec3F.Cross(xAxis, yAxis);
            yAxis = Vec3F.Cross(zAxis, xAxis);

            xAxis = xAxis / xAxis.Length;
            yAxis = yAxis / yAxis.Length;
            zAxis = zAxis / zAxis.Length;

            M11 = xAxis.X; M12 = xAxis.Y; M13 = xAxis.Z;
            M21 = yAxis.X; M22 = yAxis.Y; M23 = yAxis.Z;
            M31 = zAxis.X; M32 = zAxis.Y; M33 = zAxis.Z;

            M41 = m.M41;
            M42 = m.M42;
            M43 = m.M43;
        }

        /// <summary>
        /// Transforms the given point by this matrix</summary>
        /// <param name="v">Input vector</param>
        /// <param name="result">Output vector</param>
        public void Transform(Vec3F v, out Vec3F result)
        {
            result.X = M11 * v.X + M21 * v.Y + M31 * v.Z + M41;
            result.Y = M12 * v.X + M22 * v.Y + M32 * v.Z + M42;
            result.Z = M13 * v.X + M23 * v.Y + M33 * v.Z + M43;
        }

        /// <summary>
        /// Transforms the given point by this matrix</summary>
        /// <param name="v">Input/Output vector</param>
        public void Transform(ref Vec3F v)
        {
            float x = M11 * v.X + M21 * v.Y + M31 * v.Z + M41;
            float y = M12 * v.X + M22 * v.Y + M32 * v.Z + M42;
            float z = M13 * v.X + M23 * v.Y + M33 * v.Z + M43;
            v.X = x;
            v.Y = y;
            v.Z = z;
        }

        /// <summary>
        /// Transforms the given vector by this matrix</summary>
        /// <param name="v">Input vector</param>
        /// <param name="result">Output vector</param>
        public void TransformVector(Vec3F v, out Vec3F result)
        {
            result.X = M11 * v.X + M21 * v.Y + M31 * v.Z;
            result.Y = M12 * v.X + M22 * v.Y + M32 * v.Z;
            result.Z = M13 * v.X + M23 * v.Y + M33 * v.Z;
        }

        /// <summary>
        /// Transforms by this matrix a vector that is perpendicular to some geometry or that is the component of a 3D plane.
        /// Points may be transformed using Transform(). Directional vectors like those in rays can be
        /// transformed by TransformVector(), and normals need to be transformed by TransformNormal().</summary>
        /// <param name="n">The normal vector to some geometry</param>
        /// <param name="result">The normal vector after being transformed by this matrix. Is not normalized.</param>
        public void TransformNormal(Vec3F n, out Vec3F result)
        {
            Matrix4F transposeOfInverse = new Matrix4F(this);
            transposeOfInverse.Invert(transposeOfInverse);
            transposeOfInverse.Transpose(transposeOfInverse);
            TransformNormal(n, transposeOfInverse, out result);
        }

        /// <summary>
        /// Transforms by this matrix a vector that is perpendicular to some geometry or that is the component of a 3D plane.
        /// Points may be transformed using Transform(). Directional vectors like those in rays can be
        /// transformed by TransformVector(), and normals need to be transformed by TransformNormal().</summary>
        /// <param name="n">The normal vector to some geometry</param>
        /// <param name="transposeOfInverse">The transpose of the inverse of this matrix, for performance reasons</param>
        /// <param name="result">The normal vector after being transformed by this matrix</param>
        public void TransformNormal(Vec3F n, Matrix4F transposeOfInverse, out Vec3F result)
        {
            transposeOfInverse.Transform(n, out result);
        }

        /// <summary>
        /// Transforms the given vector by this matrix</summary>
        /// <param name="v">Input vector</param>
        /// <param name="result">Output vector</param>
        public void Transform(Vec4F v, out Vec4F result)
        {
            result.X = M11 * v.X + M21 * v.Y + M31 * v.Z + M41 * v.W;
            result.Y = M12 * v.X + M22 * v.Y + M32 * v.Z + M42 * v.W;
            result.Z = M13 * v.X + M23 * v.Y + M33 * v.Z + M43 * v.W;
            result.W = M14 * v.X + M24 * v.Y + M34 * v.Z + M44 * v.W;
        }

        /// <summary>
        /// Transforms the given plane by this matrix</summary>
        /// <param name="p">Input plane</param>
        /// <param name="result">Output plane</param>
        public void Transform(Plane3F p, out Plane3F result)
        {
            Vec3F normal = p.Normal;
            Vec3F point = p.PointOnPlane();
            TransformNormal(normal, out normal);
            Transform(point, out point);
            normal.Normalize();
            result = new Plane3F(normal, point);
        }

        /// <summary>
        /// Transforms the given plane by this matrix</summary>
        /// <param name="p">Input plane</param>
        /// <param name="transposeOfInverse">Transpose of the inverse of this matrix, for performance reasons</param>
        /// <param name="result">Output plane</param>
        public void Transform(Plane3F p, Matrix4F transposeOfInverse, out Plane3F result)
        {
            Vec3F normal = p.Normal;
            Vec3F point = p.PointOnPlane();
            TransformNormal(normal, transposeOfInverse, out normal);
            Transform(point, out point);
            normal.Normalize();
            result = new Plane3F(normal, point);
        }

        /// <summary>
        /// Returns a string that represents this matrix</summary>
        /// <returns>A <see cref="T:System.String"></see> that represents this matrix</returns>
        public override string ToString()
        {
            string s = StringUtil.GetNumberListSeparator(null) + " ";

            return
                M11 + s + M12 + s + M13 + s + M14 + Environment.NewLine +
                M21 + s + M22 + s + M23 + s + M24 + Environment.NewLine +
                M31 + s + M32 + s + M33 + s + M34 + Environment.NewLine +
                M41 + s + M42 + s + M43 + s + M44 + Environment.NewLine + Environment.NewLine;
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
        /// <returns>String that represents matrix</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            string listSeparator = StringUtil.GetNumberListSeparator(formatProvider);

            // For historic reasons, use "R" for round-trip support, in case this string is persisted.
            if (format == null)
                format = "R";

            return String.Format(
                "{0}{16} {1}{16} {2}{16} {3}{16} {4}{16} {5}{16} {6}{16} {7}{16} {8}{16} {9}{16} " +
                "{10}{16} {11}{16} {12}{16} {13}{16} {14}{16} {15}",
                M11.ToString(format, formatProvider),
                M12.ToString(format, formatProvider),
                M13.ToString(format, formatProvider),
                M14.ToString(format, formatProvider),
                M21.ToString(format, formatProvider),
                M22.ToString(format, formatProvider),
                M23.ToString(format, formatProvider),
                M24.ToString(format, formatProvider),
                M31.ToString(format, formatProvider),
                M32.ToString(format, formatProvider),
                M33.ToString(format, formatProvider),
                M34.ToString(format, formatProvider),
                M41.ToString(format, formatProvider),
                M42.ToString(format, formatProvider),
                M43.ToString(format, formatProvider),
                M44.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion

        /// <summary>
        /// Tests exact equality with the given matrix</summary>
        /// <param name="m">Other matrix</param>
        /// <returns><c>True</c> if matrices are exactly equal</returns>
        public bool Equals(Matrix4F m)
        {
            return
                M11 == m.M11 && M12 == m.M12 && M13 == m.M13 && M14 == m.M14 &&
                M21 == m.M21 && M22 == m.M22 && M23 == m.M23 && M24 == m.M24 &&
                M31 == m.M31 && M32 == m.M32 && M33 == m.M33 && M34 == m.M34 &&
                M41 == m.M41 && M42 == m.M42 && M43 == m.M43 && M44 == m.M44;
        }

        /// <summary>
        /// Determines whether the specified object is a matrix that is equivalent to this matrix</summary>
        /// <param name="obj">The object to compare to</param>
        /// <returns><c>True</c> if the specified object is equal to this matrix</returns>
        public override bool Equals(object obj)
        {
            if (obj is Matrix4F)
            {
                Matrix4F other = (Matrix4F)obj;
                return Equals(other);
            }
            return false;
        }

        /// <summary>
        /// Tests equality to another matrix within a given epsilon</summary>
        /// <param name="m">Other matrix</param>
        /// <param name="eps">Epsilon, or margin for error</param>
        /// <returns><c>True</c> if all components of the two matrices are within epsilon</returns>
        public bool EpsilonEquals(Matrix4F m, double eps)
        {
            return
                Math.Abs(M11 - m.M11) < eps &&
                Math.Abs(M12 - m.M12) < eps &&
                Math.Abs(M13 - m.M13) < eps &&
                Math.Abs(M14 - m.M14) < eps &&
                Math.Abs(M21 - m.M21) < eps &&
                Math.Abs(M22 - m.M22) < eps &&
                Math.Abs(M23 - m.M23) < eps &&
                Math.Abs(M24 - m.M24) < eps &&
                Math.Abs(M31 - m.M31) < eps &&
                Math.Abs(M32 - m.M32) < eps &&
                Math.Abs(M33 - m.M33) < eps &&
                Math.Abs(M34 - m.M34) < eps &&
                Math.Abs(M41 - m.M41) < eps &&
                Math.Abs(M42 - m.M42) < eps &&
                Math.Abs(M43 - m.M43) < eps &&
                Math.Abs(M44 - m.M44) < eps;
        }

        /// <summary>
        /// Serves as a hash function for this matrix. Is suitable for use in hashing algorithms
        /// and data structures like a hash table.</summary>
        /// <returns>A hash code for this matrix</returns>
        public override int GetHashCode()
        {
            long bits = 1;
            bits = 31 * bits + M11.GetHashCode();
            bits = 31 * bits + M12.GetHashCode();
            bits = 31 * bits + M13.GetHashCode();
            bits = 31 * bits + M14.GetHashCode();
            bits = 31 * bits + M21.GetHashCode();
            bits = 31 * bits + M22.GetHashCode();
            bits = 31 * bits + M23.GetHashCode();
            bits = 31 * bits + M24.GetHashCode();
            bits = 31 * bits + M31.GetHashCode();
            bits = 31 * bits + M32.GetHashCode();
            bits = 31 * bits + M33.GetHashCode();
            bits = 31 * bits + M34.GetHashCode();
            bits = 31 * bits + M41.GetHashCode();
            bits = 31 * bits + M42.GetHashCode();
            bits = 31 * bits + M43.GetHashCode();
            bits = 31 * bits + M44.GetHashCode();
            return (int)(bits ^ (bits >> 32));
        }


        /// <summary>
        /// Multiplies the two given matrices and returns the product</summary>
        /// <param name="m1">Left matrix</param>
        /// <param name="m2">Right matrix</param>
        /// <param name="result"> Matrix product </param>        
        public static void Multiply(Matrix4F m1, Matrix4F m2, Matrix4F result)
        {           
            float f11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
            float f12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
            float f13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
            float f14 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;
            
            float f21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
            float f22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
            float f23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
            float f24 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;
            
            float f31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
            float f32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
            float f33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
            float f34 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;
            
            float f41 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
            float f42 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
            float f43 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
            float f44 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;

            result.M11 = f11;
            result.M12 = f12;
            result.M13 = f13;
            result.M14 = f14;

            result.M21 = f21;
            result.M22 = f22;
            result.M23 = f23;
            result.M24 = f24;

            result.M31 = f31;
            result.M32 = f32;
            result.M33 = f33;
            result.M34 = f34;

            result.M41 = f41;
            result.M42 = f42;
            result.M43 = f43;
            result.M44 = f44;
        }

        /// <summary>
        /// Multiplies the two given matrices and returns the product</summary>
        /// <param name="m1">Left matrix</param>
        /// <param name="m2">Right matrix</param>
        /// <returns>Matrix product</returns>
        public static Matrix4F Multiply(Matrix4F m1, Matrix4F m2)
        {
            Matrix4F result = new Matrix4F();

            result.M11 = m1.M11 * m2.M11 + m1.M12 * m2.M21 + m1.M13 * m2.M31 + m1.M14 * m2.M41;
            result.M12 = m1.M11 * m2.M12 + m1.M12 * m2.M22 + m1.M13 * m2.M32 + m1.M14 * m2.M42;
            result.M13 = m1.M11 * m2.M13 + m1.M12 * m2.M23 + m1.M13 * m2.M33 + m1.M14 * m2.M43;
            result.M14 = m1.M11 * m2.M14 + m1.M12 * m2.M24 + m1.M13 * m2.M34 + m1.M14 * m2.M44;

            result.M21 = m1.M21 * m2.M11 + m1.M22 * m2.M21 + m1.M23 * m2.M31 + m1.M24 * m2.M41;
            result.M22 = m1.M21 * m2.M12 + m1.M22 * m2.M22 + m1.M23 * m2.M32 + m1.M24 * m2.M42;
            result.M23 = m1.M21 * m2.M13 + m1.M22 * m2.M23 + m1.M23 * m2.M33 + m1.M24 * m2.M43;
            result.M24 = m1.M21 * m2.M14 + m1.M22 * m2.M24 + m1.M23 * m2.M34 + m1.M24 * m2.M44;

            result.M31 = m1.M31 * m2.M11 + m1.M32 * m2.M21 + m1.M33 * m2.M31 + m1.M34 * m2.M41;
            result.M32 = m1.M31 * m2.M12 + m1.M32 * m2.M22 + m1.M33 * m2.M32 + m1.M34 * m2.M42;
            result.M33 = m1.M31 * m2.M13 + m1.M32 * m2.M23 + m1.M33 * m2.M33 + m1.M34 * m2.M43;
            result.M34 = m1.M31 * m2.M14 + m1.M32 * m2.M24 + m1.M33 * m2.M34 + m1.M34 * m2.M44;

            result.M41 = m1.M41 * m2.M11 + m1.M42 * m2.M21 + m1.M43 * m2.M31 + m1.M44 * m2.M41;
            result.M42 = m1.M41 * m2.M12 + m1.M42 * m2.M22 + m1.M43 * m2.M32 + m1.M44 * m2.M42;
            result.M43 = m1.M41 * m2.M13 + m1.M42 * m2.M23 + m1.M43 * m2.M33 + m1.M44 * m2.M43;
            result.M44 = m1.M41 * m2.M14 + m1.M42 * m2.M24 + m1.M43 * m2.M34 + m1.M44 * m2.M44;

            return result;
        }

        /// <summary>
        /// Multiplies two matrices</summary>
        /// <param name="m1">First matrix</param>
        /// <param name="m2">Second matrix</param>
        /// <returns>Matrix product</returns>
        public static Matrix4F operator *(Matrix4F m1, Matrix4F m2)
        {
            return Matrix4F.Multiply(m1, m2);
        }

        private const double EPS = 0.000001;
    }
}
