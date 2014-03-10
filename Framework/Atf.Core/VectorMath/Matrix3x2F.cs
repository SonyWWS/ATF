//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// Defines 2D transformation matrix</summary>    
    public struct Matrix3x2F : IEquatable<Matrix3x2F>
    {
        /// <summary>
        /// Value at row 1 column 1 of the matrix</summary>
        public float M11;

        /// <summary>
        /// Value at row 1 column 2 of the matrix</summary>
        public float M12;

        /// <summary>
        /// Value at row 2 column 1 of the matrix</summary>
        public float M21;

        /// <summary>
        /// Value at row 2 column 2 of the matrix</summary>
        public float M22;

        /// <summary>
        /// Value at row 3 column 1 of the matrix</summary>
        public float DX;

        /// <summary>
        ///  Value at row 3 column 2 of the matrix</summary>
        public float DY;

        /// <summary>
        /// Gets an instance of the identity matrix</summary>
        public static Matrix3x2F Identity
        {
            get
            {
                Matrix3x2F result = new Matrix3x2F();
                result.M11 = 1f;
                result.M22 = 1f;
                return result;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this matrix is the identity matrix.
        /// This property is true iff this matrix is identity.</summary>
        public bool IsIdentity
        {
            get
            {                
                return (this.M11 == 1f)
                    && (this.M12 == 0f)
                    && (this.M21 == 0f)
                    && (this.M22 == 1f)
                    && (this.DX == 0f)
                    && (this.DY == 0f);
            }
        }

        /// <summary>
        /// Multiplies the given matrices</summary>
        /// <param name="left">Left matrix</param>
        /// <param name="right">Right matrix</param>
        /// <param name="result">Multiplication result</param>
        public static void Multiply(ref Matrix3x2F left, ref Matrix3x2F right, out Matrix3x2F result)
        {
            Matrix3x2F r = new Matrix3x2F();
            float num10 = left.M12;
            float num4 = right.M21;
            float num9 = left.M11;
            float num3 = right.M11;
            r.M11 = (num3 * num9) + (num4 * num10);
            float num2 = right.M22;
            float num = right.M12;
            r.M12 = (num * num9) + (num2 * num10);
            float num8 = left.M22;
            float num7 = left.M21;
            r.M21 = (num8 * num4) + (num7 * num3);
            r.M22 = (num7 * num) + (num8 * num2);
            float num6 = left.DY;
            float num5 = left.DX;
            r.DX = right.DX + ((num5 * num3) + (num6 * num4));
            r.DY = right.DY + ((num5 * num) + (num6 * num2));
            result = r;
        }

        /// <summary>
        /// Multiplies the given matrices</summary>
        /// <param name="left">Left matrix</param>
        /// <param name="right">Right matrix</param>
        /// <returns>Multiplication result</returns>
        public static Matrix3x2F Multiply(Matrix3x2F left, Matrix3x2F right)
        {
            Matrix3x2F r = new Matrix3x2F();
            float num4 = right.M21;
            float num10 = left.M12;
            float num9 = left.M11;
            float num3 = right.M11;
            r.M11 = (num3 * num9) + (num10 * num4);
            float num2 = right.M22;
            float num = right.M12;
            r.M12 = (num * num9) + (num2 * num10);
            float num8 = left.M22;
            float num7 = left.M21;
            r.M21 = (num8 * num4) + (num7 * num3);
            r.M22 = (num7 * num) + (num8 * num2);
            float num6 = left.DY;
            float num5 = left.DX;
            r.DX = right.DX + ((num5 * num3) + (num6 * num4));
            r.DY = right.DY + ((num5 * num) + (num6 * num2));
            return r;
        }

        /// <summary>
        /// Creates a matrix that can be used to rotate a set of points clockwise
        /// around the origin (zero x and y coordinates) by the amount specified 
        /// in the angle parameter</summary>
        /// <param name="angle">Angle of rotation, in degrees</param>
        /// <param name="result">Rotation matrix</param>
        public static void CreateRotation(float angle, out Matrix3x2F result)
        {
            Matrix3x2F r = new Matrix3x2F();
            double radians = (angle * 3.1415926535897931) / 180.0;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            r.M11 = cos;
            r.M12 = sin;
            r.M21 = -sin;
            r.M22 = cos;
            //r.DX = 0f;
            //r.DY = 0f;
            result = r;
        }

        /// <summary>
        /// Creates a matrix that can be used to rotate a set of points clockwise
        /// around the origin (zero x and y coordinates) by the amount specified 
        /// in the angle parameter</summary>        
        /// <param name="angle">Angle of rotation, in degrees</param>
        /// <returns>Rotation matrix</returns>
        public static Matrix3x2F CreateRotation(float angle)
        {
            Matrix3x2F result = new Matrix3x2F();
            double radians = (angle * 3.1415926535897931) / 180.0;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);
            result.M11 = cos;
            result.M12 = sin;
            result.M21 = -sin;
            result.M22 = cos;
            //result.DX = 0f;
            //result.DY = 0f;
            return result;
        }

        /// <summary>
        /// Creates a scaling matrix</summary>
        /// <param name="x">Amount to scale by in the x-axis direction</param>
        /// <param name="y">Amount to scale by in the x-axis direction</param>
        /// <param name="result">The scaling matrix</param>
        public static void CreateScale(float x, float y, out Matrix3x2F result)
        {
            Matrix3x2F r = new Matrix3x2F();
            r.M11 = x;
            //r.M12 = 0f;
            //r.M21 = 0f;
            r.M22 = y;
            //r.DX = 0f;
            //r.DY = 0f;
            result = r;
        }

        /// <summary>
        /// Creates scaling matrix</summary>
        /// <param name="x">Amount to scale by in the x-axis direction</param>
        /// <param name="y">Amount to scale by in the y-axis direction</param>
        /// <returns>The scaling matrix</returns>
        public static Matrix3x2F CreateScale(float x, float y)
        {
            Matrix3x2F r = new Matrix3x2F();
            r.M11 = x;
            //r.M12 = 0f;
            //r.M21 = 0f;
            r.M22 = y;
            //r.DX = 0f;
            //r.DY = 0f;
            return r;
        }
      
        /// <summary>
        /// Creates a translation matrix</summary>
        /// <param name="x">Distance to translate along the x-axis</param>
        /// <param name="y">Distance to translate along the y-axis</param>
        /// <param name="result">The translation matrix</param>
        public static void CreateTranslation(float x, float y, out Matrix3x2F result)
        {
            Matrix3x2F r = new Matrix3x2F();
            r.M11 = 1f;
            //r.M12 = 0f;
            //r.M21 = 0f;
            r.M22 = 1f;
            r.DX = x;
            r.DY = y;
            result = r;
        }

        /// <summary>
        /// Creates a translation matrix</summary>
        /// <param name="x">Distance to translate along the x-axis</param>
        /// <param name="y">Distance to translate along the y-axis</param>
        /// <returns>New translation matrix</returns>
        public static Matrix3x2F CreateTranslation(float x, float y)
        {
            Matrix3x2F r = new Matrix3x2F();
            r.M11 = 1f;
            //r.M12 = 0f;
            //r.M21 = 0f;
            r.M22 = 1f;
            r.DX = x;
            r.DY = y;
            return r;
        }

        /// <summary>
        /// Transforms rectangle</summary>     
        /// <param name="mat">Matrix representing transform</param>
        /// <param name="r">Rectangle to transform</param>
        /// <returns>Transformed rectangle</returns>
        public static RectangleF Transform(Matrix3x2F mat, RectangleF r)
        {
            RectangleF result = new RectangleF();
            result.X = r.X * mat.M11 + r.Y * mat.M21 + mat.DX;
            result.Y = r.X * mat.M12 + r.Y * mat.M22 + mat.DY;
            result.Width =  r.Width * mat.M11 + r.Height * mat.M21;
            result.Height = r.Width * mat.M12 + r.Height * mat.M22;
            return result;
        }
            
        /// <summary>
        /// Transforms the given point</summary>
        /// <param name="mat">Transformation matrix</param>
        /// <param name="point">Point to be transformed</param>
        /// <param name="result">Transformed point</param>
        public static void TransformPoint(ref Matrix3x2F mat, ref PointF point, ref PointF result)
        {
            result.X = point.X * mat.M11 + point.Y * mat.M21 + mat.DX;
            result.Y = point.X * mat.M12 + point.Y * mat.M22 + mat.DY;                        
        }


        /// <summary>
        /// Transforms the given point</summary>
        /// <param name="mat">Transformation matrix</param>
        /// <param name="point">Point to transform</param>
        /// <returns>Transformed point</returns>
        public static PointF TransformPoint(Matrix3x2F mat, PointF point)
        {
            PointF r = new PointF();
            r.X = point.X * mat.M11 + point.Y * mat.M21 + mat.DX;
            r.Y = point.X * mat.M12 + point.Y * mat.M22 + mat.DY;
            return r;
        }

        /// <summary>
        /// Transforms the given point by using only scale and rotate components
        /// of the given matrix</summary>
        /// <param name="mat">Transformation matrix</param>
        /// <param name="point">Point to transform</param>
        /// <returns>Transformed point</returns>
        public static PointF TransformVector(Matrix3x2F mat, PointF point)
        {
            PointF r = new PointF();
            r.X = point.X * mat.M11 + point.Y * mat.M21;
            r.Y = point.X * mat.M12 + point.Y * mat.M22;
            return r;
        }

        /// <summary>
        /// Computes determinant of matrix</summary>        
        public float Determinant()
        {
            return ((this.M22 * this.M11) - (this.M21 * this.M12));
        }
    
        /// <summary>
        /// Inverts matrix</summary>
        public void Invert()
        {
            float m11 = M22;
            float m12 = -M12;
            float m21 = -M21;
            float m22 = M11;
            float dx = M21 * DY - M22 * DX;
            float dy = M12 * DX - M11 * DY;

            float invdet = 1.0f / Determinant();
            
            M11 = m11 * invdet;
            M12 = m12 * invdet;
            M21 = m21 * invdet;
            M22 = m22 * invdet;
            DX = dx * invdet;
            DY = dy * invdet;
        }
       

        /// <summary>
        /// Calculates the inverse of specified matrix</summary>
        /// <param name="m">Matrix whose inverse is to be calculated</param>
        /// <returns>Inverse of specified matrix</returns>
        public static Matrix3x2F Invert(Matrix3x2F m)
        {
            float m11 =  m.M22;
            float m12 = -m.M12;            
            float m21 = -m.M21;
            float m22 = m.M11;
            float dx = m.M21 * m.DY  - m.M22 * m.DX;
            float dy = m.M12 * m.DX - m.M11 * m.DY;
            
            float invdet = 1.0f / m.Determinant();
            Matrix3x2F result = new Matrix3x2F();
            result.M11 = m11 * invdet;
            result.M12 = m12 * invdet;
            result.M21 = m21 * invdet;
            result.M22 = m22 * invdet;
            result.DX = dx * invdet;
            result.DY = dy * invdet;
            return result;                       
        }


        /// <summary>
        /// Multiplies the given matrices</summary>
        /// <param name="left">Left matrix</param>
        /// <param name="right">Right matrix</param>
        /// <returns>Result of multiplication</returns>
        public static Matrix3x2F operator *(Matrix3x2F left, Matrix3x2F right)
        {
            Matrix3x2F r = new Matrix3x2F();
            float num4 = right.M21;
            float num10 = left.M12;
            float num9 = left.M11;
            float num3 = right.M11;
            r.M11 = (num3 * num9) + (num10 * num4);
            float num2 = right.M22;
            float num = right.M12;
            r.M12 = (num * num9) + (num2 * num10);
            float num8 = left.M22;
            float num7 = left.M21;
            r.M21 = (num8 * num4) + (num7 * num3);
            r.M22 = (num7 * num) + (num8 * num2);
            float num6 = left.DY;
            float num5 = left.DX;
            r.DX = right.DX + ((num5 * num3) + (num6 * num4));
            r.DY = right.DY + ((num5 * num) + (num6 * num2));
            return r;
        }


        /// <summary>
        /// Performs an implicit conversion from System.Drawing.Drawing2D.Matrix to Matrix3x2F</summary>
        /// <param name="matrix">Matrix to convert</param>
        /// <returns>Converted matrix</returns>
        public static implicit operator Matrix3x2F(System.Drawing.Drawing2D.Matrix matrix)
        {
            Matrix3x2F r = new Matrix3x2F();
            float[] elms = matrix.Elements;
            r.M11 = elms[0];
            r.M12 = elms[1];
            r.M21 = elms[2];
            r.M22 = elms[3];
            r.DX = elms[4];
            r.DY = elms[5];
            return r;
        }

        /// <summary>
        /// Compute hash code</summary>        
        public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode()
                + M21.GetHashCode() + M22.GetHashCode()
                + DX.GetHashCode() + DY.GetHashCode();
        }

        /// <summary>
        /// Test if two matrices are exactly equal</summary>
        /// <param name="value1">First matrix</param>
        /// <param name="value2">Second matrix</param>
        /// <returns>True iff matrices are exactly equal</returns>
        public static bool Equals(ref Matrix3x2F value1, ref Matrix3x2F value2)
        {
            return (value1.M11 == value2.M11)
                && (value1.M12 == value2.M12)
                && (value1.M21 == value2.M21)
                && (value1.M22 == value2.M22)
                && (value1.DX == value2.DX)
                && (value1.DY == value2.DY);
        }

        /// <summary>
        /// Test if matrix is exactly equal to another matrix</summary>
        /// <param name="other">Matrix to compare to</param>
        /// <returns>True iff matrices are exactly equal</returns>
        public bool Equals(Matrix3x2F other)
        {
            return (this.M11 == other.M11)
                && (this.M12 == other.M12)
                && (this.M21 == other.M21)
                && (this.M22 == other.M22)
                && (this.DX == other.DX)
                && (this.DY == other.DY);

        }

        /// <summary>
        /// Tests if matrix is exactly equal to another object</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True iff object and this instance are the same type and represent exactly the same value</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj.GetType() != base.GetType())
            {
                return false;
            }
            return this.Equals((Matrix3x2F)obj);
        }
    }


}
