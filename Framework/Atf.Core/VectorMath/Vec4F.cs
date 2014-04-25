//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// 4D vector</summary>
    public struct Vec4F : IEquatable<Vec4F>, IFormattable
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
        /// Constructor using 4 components</summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        /// <param name="z">Z component</param>
        /// <param name="w">W component</param>
        public Vec4F(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Constructor using array of components</summary>
        /// <param name="coords">Array of floats with Length >= 4</param>
        public Vec4F(float[] coords)
        {
            if (coords.Length < 4)
                throw new ArgumentException("not enough coords");

            X = coords[0];
            Y = coords[1];
            Z = coords[2];
            W = coords[3];
        }

        /// <summary>
        /// Constructor using 3D vector</summary>
        /// <param name="v">3D vector</param>
        public Vec4F(Vec3F v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = 1;
        }

        /// <summary>
        /// Constructor using 4D vector</summary>
        /// <param name="v">4D vector</param>
        public Vec4F(Vec4F v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }

        /// <summary>
        /// Gets the length of the vector</summary>
        public float Length
        {
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z + W * W); }
        }

        /// <summary>
        /// Gets the squared length of the vector</summary>
        public float LengthSquared
        {
            get { return X * X + Y * Y + Z * Z + W * W; }
        }

        /// <summary>
        /// Indexer into components, valid index range is [0, 3]</summary>
        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
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
                    case 3: W = value; return;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Creates a X-axis vector</summary>
        public static readonly Vec4F XAxis = new Vec4F(1, 0, 0, 0);

        /// <summary>
        /// Creates a Y-axis vector</summary>
        public static readonly Vec4F YAxis = new Vec4F(0, 1, 0, 0);

        /// <summary>
        /// Creates a Z-axis vector</summary>
        public static readonly Vec4F ZAxis = new Vec4F(0, 0, 1, 0);

        /// <summary>
        /// Creates a W-axis vector</summary>
        public static readonly Vec4F WAxis = new Vec4F(0, 0, 0, 1);

        /// <summary>
        /// Sets vector to the given coordinates</summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <param name="w">W coordinate</param>
        public void Set(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        /// <summary>
        /// Sets vector to array of components</summary>
        /// <param name="coords">Array of floats with Length >= 4</param>
        public void Set(float[] coords)
        {
            if (coords.Length < 4)
                throw new ArgumentException("not enough coords");

            X = coords[0];
            Y = coords[1];
            Z = coords[2];
            W = coords[3];
        }

        /// <summary>
        /// Sets vector to the given vector</summary>
        /// <param name="v">Vector</param>
        public void Set(Vec4F v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
            W = v.W;
        }

        /// <summary>
        /// Gets negation of given vector and returns new vector.
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>Negation of given vector</returns>
        public static Vec4F Neg(Vec4F v)
        {
            return new Vec4F(-v.X, -v.Y, -v.Z, -v.W);
        }

        /// <summary>
        /// Negates this vector</summary>
        public void Neg()
        {
            Set(-X, -Y, -Z, -W);
        }

        /// <summary>
        /// Converts vector to array of floats</summary>
        /// <returns>Array of 4 floats</returns>
        public float[] ToArray()
        {
            return new float[] { X, Y, Z, W };
        }

        /// <summary>
        /// Calculates the sum of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 + Vector 2</returns>
        public static Vec4F Add(Vec4F v1, Vec4F v2)
        {
            return new Vec4F(
                v1.X + v2.X,
                v1.Y + v2.Y,
                v1.Z + v2.Z,
                v1.W + v2.W);
        }

        /// <summary>
        /// Calculates the difference of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 - Vector 2</returns>
        public static Vec4F Sub(Vec4F v1, Vec4F v2)
        {
            return new Vec4F(
                v1.X - v2.X,
                v1.Y - v2.Y,
                v1.Z - v2.Z,
                v1.W - v2.W);
        }

        /// <summary>
        /// Calculates the component-wise product of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 * Vector 2</returns>
        public static Vec4F Mul(Vec4F v1, Vec4F v2)
        {
            return new Vec4F(
                v1.X * v2.X,
                v1.Y * v2.Y,
                v1.Z * v2.Z,
                v1.W * v2.W);
        }

        /// <summary>
        /// Calculates the component-wise product of the vector and scalar</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scalar</param>
        /// <returns>Vector * scalar</returns>
        public static Vec4F Mul(Vec4F v, float s)
        {
            return new Vec4F(
                v.X * s,
                v.Y * s,
                v.Z * s,
                v.W * s);
        }

        /// <summary>
        /// Calculates lerp (linear interpolate) from v1 to v2, result = u(1-t) + v(t)</summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <param name="t">Parameter</param>
        /// <returns>Lerp vector</returns>
        public static Vec4F Lerp(Vec4F v1, Vec4F v2, float t)
        {
            return new Vec4F(
                v1.X + (v2.X - v1.X) * t,
                v1.Y + (v2.Y - v1.Y) * t,
                v1.Z + (v2.Z - v1.Z) * t,
                v1.W + (v2.W - v1.W) * t);
        }

        /// <summary>
        /// Clamps vector components to the given minimum and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="min">Component minimum</param>
        /// <returns>Clamped vector</returns>
        public static Vec4F ClampMin(Vec4F v, float min)
        {
            return new Vec4F(
                Math.Max(v.X, min),
                Math.Max(v.Y, min),
                Math.Max(v.Z, min),
                Math.Max(v.W, min));
        }

        /// <summary>
        /// Clamps vector components to the given maximum and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="max">Component maximum</param>
        /// <returns>Clamped vector</returns>
        public static Vec4F ClampMax(Vec4F v, float max)
        {
            return new Vec4F(
                Math.Min(v.X, max),
                Math.Min(v.Y, max),
                Math.Min(v.Z, max),
                Math.Min(v.W, max));
        }

        /// <summary>
        /// Clamps vector components to the given range and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="min">Component minimum</param>
        /// <param name="max">Component maximum</param>
        /// <returns>Clamped vector</returns>
        public static Vec4F Clamp(Vec4F v, float min, float max)
        {
            return new Vec4F(
                Math.Min(Math.Max(v.X, min), max),
                Math.Min(Math.Max(v.Y, min), max),
                Math.Min(Math.Max(v.Z, min), max),
                Math.Min(Math.Max(v.W, min), max));
        }

        /// <summary>
        /// Gets the component-wise absolute value of the given vector and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>Component-wise absolute value of the given vector</returns>
        public static Vec4F Abs(Vec4F v)
        {
            return new Vec4F(
                Math.Abs(v.X),
                Math.Abs(v.Y),
                Math.Abs(v.Z),
                Math.Abs(v.W));
        }

        /// <summary>
        /// Gets the normal for the given vector and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>Normal for given vector</returns>
        public static Vec4F Normalize(Vec4F v)
        {
            float len = v.Length;
            if (len < 0.000001f)
                return v;

            float ooLen = 1.0f / len;
            return new Vec4F(v.X * ooLen, v.Y * ooLen, v.Z * ooLen, v.W * ooLen);
        }

        /// <summary>
        /// Normalizes this vector</summary>
        public void Normalize()
        {
            Set(Vec4F.Normalize(this));
        }

        /// <summary>
        /// Calculates dot product of 2 vectors</summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>Dot product of 2 vectors</returns>
        public static float Dot(Vec4F u, Vec4F v)
        {
            return u.X * v.X + u.Y * v.Y + u.Z * v.Z + u.W * v.W;
        }

        /// <summary>
        /// Calculates the distance between the given points</summary>
        /// <param name="v1">Point 1</param>
        /// <param name="v2">Point 2</param>
        /// <returns>Distance between the given points</returns>
        public static float Distance(Vec4F v1, Vec4F v2)
        {
            Vec4F d = Sub(v1, v2);
            return d.Length;
        }

        /// <summary>
        /// Calculates the angle between the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Angle between the given vectors</returns>
        public static float Angle(Vec4F v1, Vec4F v2)
        {
            double vDot = Dot(v1, v2) / Math.Sqrt(Dot(v1, v1) * Dot(v2, v2));

            vDot = Math.Max(vDot, -1.0);
            vDot = Math.Min(vDot, 1.0);

            return (float)Math.Acos(vDot);
        }

        /// <summary>
        /// Tests for exact equality to given vector</summary>
        /// <param name="v">Other vector</param>
        /// <returns>True iff vectors are exactly equal</returns>
        public bool Equals(Vec4F v)
        {
            return
                X == v.X &&
                Y == v.Y &&
                Z == v.Z &&
                W == v.W;
        }

        /// <summary>
        /// Tests for equality to another vector, within a given epsilon</summary>
        /// <param name="v">Other vector</param>
        /// <param name="eps">Epsilon, or margin for error</param>
        /// <returns>True iff all components are within epsilon</returns>
        public bool Equals(Vec4F v, double eps)
        {
            return
                Math.Abs(X - v.X) < eps &&
                Math.Abs(Y - v.Y) < eps &&
                Math.Abs(Z - v.Z) < eps &&
                Math.Abs(W - v.W) < eps;
        }

        #region Operator Overloads

        /// <summary>
        /// Negation operator</summary>
        /// <param name="v">Vector</param>
        /// <returns>Additive inverse of vector</returns>
        public static Vec4F operator -(Vec4F v)
        {
            return Vec4F.Neg(v);
        }

        /// <summary>
        /// Addition operator</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 + Vector 2</returns>
        public static Vec4F operator +(Vec4F v1, Vec4F v2)
        {
            return Vec4F.Add(v1, v2);
        }

        /// <summary>
        /// Subtraction operator</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 - Vector 2</returns>
        public static Vec4F operator -(Vec4F v1, Vec4F v2)
        {
            return Vec4F.Sub(v1, v2);
        }

        /// <summary>
        /// Multiply by constant operator</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scale factor</param>
        /// <returns>Scaled vector</returns>
        public static Vec4F operator *(Vec4F v, float s)
        {
            return Vec4F.Mul(v, s);
        }

        /// <summary>
        /// Multiply by constant operator</summary>
        /// <param name="s">Scale factor</param>
        /// <param name="v">Vector</param>
        /// <returns>Scaled vector</returns>
        public static Vec4F operator *(float s, Vec4F v)
        {
            return Vec4F.Mul(v, s);
        }

        /// <summary>
        /// Divide by constant operator</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Divisor</param>
        /// <returns>Scaled vector</returns>
        public static Vec4F operator /(Vec4F v, float s)
        {
            return Vec4F.Mul(v, 1.0f / s);
        }

        /// <summary>
        /// Equality operator</summary>
        /// <param name="v1">Left hand vector</param>
        /// <param name="v2">Right hand vector</param>
        /// <returns>True iff vectors are exactly equal</returns>
        public static bool operator ==(Vec4F v1, Vec4F v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>
        /// Inequality operator</summary>
        /// <param name="v1">Left hand vector</param>
        /// <param name="v2">Right hand vector</param>
        /// <returns>True iff vectors are not exactly equal</returns>
        public static bool operator !=(Vec4F v1, Vec4F v2)
        {
            return !v1.Equals(v2);
        }

        #endregion

        /// <summary>
        /// Indicates whether this instance and a specified object are exactly equal</summary>
        /// <param name="obj">Another object to compare to</param>
        /// <returns>True iff object and this instance are the same type and represent exactly the same value</returns>
        public override bool Equals(object obj)
        {
            if (obj is Vec4F)
            {
                Vec4F other = (Vec4F)obj;
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
            bits = 31 * bits + W.GetHashCode();
            return (int)(bits ^ (bits >> 32));
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
                "{1}{0} {2}{0} {3}{0} {4}",
                listSeparator,
                X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider),
                Z.ToString(format, formatProvider),
                W.ToString(format, formatProvider));
        }
        #endregion
    }
}
