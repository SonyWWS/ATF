//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// 3D vector</summary>
    public struct Vec3F : IEquatable<Vec3F>, IFormattable
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
        /// Constructor using 3 floats</summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public Vec3F(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Constructor using array of floats</summary>
        /// <param name="coords">Array of floats with Length >= 3</param>
        public Vec3F(float[] coords)
        {
            X = coords[0];
            Y = coords[1];
            Z = coords[2];
        }

        /// <summary>
        /// Constructor using 3D vector</summary>
        /// <param name="v">3D vector</param>
        public Vec3F(Vec3F v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        /// <summary>
        /// Constructor using 2D vector</summary>
        /// <param name="v">2D vector</param>
        public Vec3F(Vec2F v)
        {
            X = v.X;
            Y = v.Y;
            Z = 1;
        }

        /// <summary>
        /// Gets the length of the vector</summary>
        public float Length
        {
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Gets the squared length of the vector</summary>
        public float LengthSquared
        {
            get { return X * X + Y * Y + Z * Z; }
        }

        /// <summary>
        /// Indexer into components, valid index range is [0, 2]</summary>
        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                }
                throw new ArgumentOutOfRangeException();
            }

            set
            {
                switch (i)
                {
                    case 0: X = value; return;
                    case 1: Y = value; return;
                    case 2: Z = value; return;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates a X-axis vector</summary>
        public static readonly Vec3F XAxis = new Vec3F(1, 0, 0);

        /// <summary>
        /// Creates a Y-axis vector</summary>
        public static readonly Vec3F YAxis = new Vec3F(0, 1, 0);

        /// <summary>
        /// Creates a Z-axis vector</summary>
        public static readonly Vec3F ZAxis = new Vec3F(0, 0, 1);

        /// <summary>
        /// Creates a zero-length vector</summary>
        public static readonly Vec3F ZeroVector = new Vec3F(0, 0, 0);

        /// <summary>
        /// Sets vector to the given coordinates</summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public void Set(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Sets vector to array of coordinates</summary>
        /// <param name="coords">Array of floats with Length >= 3</param>
        public void Set(float[] coords)
        {
            X = coords[0];
            Y = coords[1];
            Z = coords[2];
        }

        /// <summary>
        /// Sets vector to the given vector</summary>
        /// <param name="v">Vector</param>
        public void Set(Vec3F v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        /// <summary>
        /// Gets a negation of the given vector. Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>negation of the given vector</returns>
        public static Vec3F Neg(Vec3F v)
        {
            return new Vec3F(-v.X, -v.Y, -v.Z);
        }

        /// <summary>
        /// Negates this vector</summary>
        public void Neg()
        {
            Set(-X, -Y, -Z);
        }
       
        /// <summary>
        /// Calculates the sum of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector sum</returns>
        public static Vec3F Add(Vec3F v1, Vec3F v2)
        {
            return new Vec3F(
                v1.X + v2.X,
                v1.Y + v2.Y,
                v1.Z + v2.Z);
        }

        /// <summary>
        /// Calculates the difference of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 - Vector 2</returns>
        public static Vec3F Sub(Vec3F v1, Vec3F v2)
        {
            return new Vec3F(
                v1.X - v2.X,
                v1.Y - v2.Y,
                v1.Z - v2.Z);
        }

        /// <summary>
        /// Calculates the component-wise product of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 * Vector 2</returns>
        public static Vec3F Mul(Vec3F v1, Vec3F v2)
        {
            return new Vec3F(
                v1.X * v2.X,
                v1.Y * v2.Y,
                v1.Z * v2.Z);
        }

        /// <summary>
        /// Calculates the component-wise product of the vector and scalar</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scalar</param>
        /// <returns>Vector * scalar</returns>
        public static Vec3F Mul(Vec3F v, float s)
        {
            return new Vec3F(
                v.X * s,
                v.Y * s,
                v.Z * s);
        }

        /// <summary>
        /// Calculates the component-wise quotient of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 / Vector 2</returns>
        public static Vec3F Div(Vec3F v1, Vec3F v2)
        {
            return new Vec3F(
                v1.X / v2.X,
                v1.Y / v2.Y,
                v1.Z / v2.Z);
        }

        /// <summary>
        /// Lerp (linear interpolate) from v1 to v2, result = u(1-t) + v(t)</summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <param name="t">Parameter</param>
        /// <returns>Lerp vector</returns>
        public static Vec3F Lerp(Vec3F v1, Vec3F v2, float t)
        {
            return new Vec3F(
                v1.X + (v2.X - v1.X) * t,
                v1.Y + (v2.Y - v1.Y) * t,
                v1.Z + (v2.Z - v1.Z) * t);
        }

        /// <summary>
        /// Clamps vector components to the given minimum and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="min">Component minimum</param>
        /// <returns>Clamped vector</returns>
        public static Vec3F ClampMin(Vec3F v, float min)
        {
            return new Vec3F(
                Math.Max(v.X, min),
                Math.Max(v.Y, min),
                Math.Max(v.Z, min));
        }

        /// <summary>
        /// Clamps vector components to the given maximum and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="max">Component maximum</param>
        /// <returns>Clamped vector</returns>
        public static Vec3F ClampMax(Vec3F v, float max)
        {
            return new Vec3F(
                Math.Min(v.X, max),
                Math.Min(v.Y, max),
                Math.Min(v.Z, max));
        }

        /// <summary>
        /// Clamps vector components to the given range and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="min">Component minimum</param>
        /// <param name="max">Component maximum</param>
        /// <returns>Clamped vector</returns>
        public static Vec3F Clamp(Vec3F v, float min, float max)
        {
            return new Vec3F(
                Math.Min(Math.Max(v.X, min), max),
                Math.Min(Math.Max(v.Y, min), max),
                Math.Min(Math.Max(v.Z, min), max));
        }

        /// <summary>
        /// Gets the component-wise absolute value of the given vector and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>Component-wise absolute value of the given vector</returns>
        public static Vec3F Abs(Vec3F v)
        {
            return new Vec3F(
                Math.Abs(v.X),
                Math.Abs(v.Y),
                Math.Abs(v.Z));
        }

        /// <summary>
        /// Gets the normal for the given vector and returns new vector.
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>Normal for the given vector</returns>
        public static Vec3F Normalize(Vec3F v)
        {
            float len = v.Length;
            if (len < 0.000001f)
                return v;

            float ooLen = 1.0f / len;
            return new Vec3F(v.X * ooLen, v.Y * ooLen, v.Z * ooLen);
        }

        /// <summary>
        /// Normalizes this vector</summary>
        public void Normalize()
        {
            Set(Vec3F.Normalize(this));
        }

        /// <summary>
        /// Calculates the dot product of 2 vectors</summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>Dot product of vectors</returns>
        public static float Dot(Vec3F u, Vec3F v)
        {
            return u.X * v.X + u.Y * v.Y + u.Z * v.Z;
        }

        /// <summary>
        /// Calculates the distance between given points</summary>
        /// <param name="v1">Point 1</param>
        /// <param name="v2">Point 2</param>
        /// <returns>Distance between the given points</returns>
        public static float Distance(Vec3F v1, Vec3F v2)
        {
            Vec3F d = Sub(v1, v2);
            return d.Length;
        }

        /// <summary>
        /// Gets the angle between given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Angle between the given vectors</returns>
        public static float Angle(Vec3F v1, Vec3F v2)
        {
            double vDot = Dot(v1, v2) / Math.Sqrt(Dot(v1, v1) * Dot(v2, v2));

            vDot = Math.Max(vDot, -1.0);
            vDot = Math.Min(vDot, 1.0);

            return (float)Math.Acos(vDot);
        }

        /// <summary>
        /// Gets the cross product of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 x Vector 2</returns>
        public static Vec3F Cross(Vec3F v1, Vec3F v2)
        {
            return new Vec3F(
                v1.Y * v2.Z - v1.Z * v2.Y,
                v2.X * v1.Z - v2.Z * v1.X,
                v1.X * v2.Y - v1.Y * v2.X);
        }

        /// <summary>
        /// Tests for exact equality to given vector</summary>
        /// <param name="v">Other vector</param>
        /// <returns>True iff vectors are exactly equal</returns>
        public bool Equals(Vec3F v)
        {
            return
                X == v.X &&
                Y == v.Y &&
                Z == v.Z;
        }

        /// <summary>
        /// Tests for equality to another vector, within a given epsilon</summary>
        /// <param name="v">Other vector</param>
        /// <param name="eps">Epsilon, or margin for error</param>
        /// <returns>True iff all components are within epsilon</returns>
        public bool Equals(Vec3F v, double eps)
        {
            return
                Math.Abs(X - v.X) < eps &&
                Math.Abs(Y - v.Y) < eps &&
                Math.Abs(Z - v.Z) < eps;
        }

        /// <summary>
        /// Converts vector to array of floats</summary>
        /// <returns>Array of 3 floats</returns>
        public float[] ToArray()
        {
            return new float[] { X, Y, Z };
        }

        /// <summary>
        /// Returns the Vec3F represented by the given string. Complements ToString().</summary>
        /// <param name="s">String of 3 single-precision floats, separated by commas</param>
        /// <returns>Vec3F represented by the string</returns>
        /// <remarks>
        /// Exceptions:
        ///   System.OverflowException:
        ///     Number in string represents a number less than System.Single.MinValue or greater than System.Single.MaxValue.
        ///
        ///   System.ArgumentNullException:
        ///     String is null.
        ///
        ///   System.FormatException:
        ///     Text in string is not a number in a valid format.
        /// </remarks>
        public static Vec3F Parse(string s)
        {
            string[] components = s.Split(',');
            if (components.Length != 3)
                throw new System.FormatException();

            Vec3F result = new Vec3F();
            for (int i = 0; i < 3; i++)
                result[i] = float.Parse(components[i]);

            return result;
        }

        #region Operator Overloads

        /// <summary>
        /// Negation operator</summary>
        /// <param name="v">Vector</param>
        /// <returns>Additive inverse of vector</returns>
        public static Vec3F operator -(Vec3F v)
        {
            return Vec3F.Neg(v);
        }

        /// <summary>
        /// Addition operator</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 + Vector 2</returns>
        public static Vec3F operator +(Vec3F v1, Vec3F v2)
        {
            return Vec3F.Add(v1, v2);
        }

        /// <summary>
        /// Subtraction operator</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 - Vector 2</returns>
        public static Vec3F operator -(Vec3F v1, Vec3F v2)
        {
            return Vec3F.Sub(v1, v2);
        }

        /// <summary>
        /// Multiplies vector by a constant</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scale factor</param>
        /// <returns>Scaled vector</returns>
        public static Vec3F operator *(Vec3F v, float s)
        {
            return Vec3F.Mul(v, s);
        }

        /// <summary>
        /// Multiplies vector by a constant</summary>
        /// <param name="s">Scale factor</param>
        /// <param name="v">Vector</param>
        /// <returns>Scaled vector</returns>
        public static Vec3F operator *(float s, Vec3F v)
        {
            return Vec3F.Mul(v, s);
        }

        /// <summary>
        /// Multiplies two vectors</summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <returns>Vector 1 * Vector 2</returns>
        public static Vec3F operator *(Vec3F v1, Vec3F v2)
        {
            return Vec3F.Mul(v1, v2);
        }

        /// <summary>
        /// Divides vector by a constant</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Divisor</param>
        /// <returns>Scaled vector</returns>
        public static Vec3F operator /(Vec3F v, float s)
        {
            return Vec3F.Mul(v, 1.0f / s);
        }

        /// <summary>
        /// Divides two vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector1 / Vector 2</returns>
        public static Vec3F operator /(Vec3F v1, Vec3F v2)
        {
            return Vec3F.Div(v1, v2);
        }

        /// <summary>
        /// Equality operator</summary>
        /// <param name="v1">Left hand vector</param>
        /// <param name="v2">Right hand vector</param>
        /// <returns>True iff vectors are exactly equal</returns>
        public static bool operator ==(Vec3F v1, Vec3F v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>
        /// Inequality operator</summary>
        /// <param name="v1">Left hand vector</param>
        /// <param name="v2">Right hand vector</param>
        /// <returns>True iff vectors are not exactly equal</returns>
        public static bool operator !=(Vec3F v1, Vec3F v2)
        {
            return !v1.Equals(v2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Indicates whether this instance and a specified object are exactly equal</summary>
        /// <param name="obj">Another object to compare to</param>
        /// <returns>True iff object and this instance are the same type and represent exactly the same value</returns>
        public override bool Equals(object obj)
        {
            if (obj is Vec3F)
            {
                Vec3F other = (Vec3F)obj;
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
            bits = 31 * bits + X.GetHashCode();
            bits = 31 * bits + Y.GetHashCode();
            bits = 31 * bits + Z.GetHashCode();
            return (int)(bits ^ (bits >> 32));
        }

        /// <summary>
        /// Returns the string representation of this Scea.VectorMath.Vec3F structure</summary>
        /// <returns> A <see cref="T:System.String"></see> representing the 3D vector</returns>        
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary> 
        /// Returns the string representation of this Scea.VectorMath.Vec3F structure 
        /// with the specified formatting information</summary>
        /// <param name="format">Standard numeric format string characters valid for a floating point</param>
        /// <param name="formatProvider">The culture specific formatting provider</param>
        /// <returns>A <see cref="T:System.String"></see> representing the 3D vector</returns> 
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null && formatProvider == null)
                return X.ToString("R") + ",  " + Y.ToString("R") + ",  " + Z.ToString("R");
            else
                return String.Format
                (
                     "({0}, {1}, {2})",
                     ((double)X).ToString(format, formatProvider),
                     ((double)Y).ToString(format, formatProvider),
                     ((double)Z).ToString(format, formatProvider)
                 );

        }

        #endregion
    }
}
