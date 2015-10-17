//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf.VectorMath
{
    /// <summary>
    /// 2D vector represented with floats</summary>
    public struct Vec2F : IEquatable<Vec2F>, IFormattable
    {
        /// <summary>
        /// X component</summary>
        public float X;

        /// <summary>
        /// Y component</summary>
        public float Y;

        /// <summary>
        /// Constructor using 2 components</summary>
        /// <param name="x">X component</param>
        /// <param name="y">Y component</param>
        public Vec2F(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructor using array of floats</summary>
        /// <param name="coords">Array of floats with Length >= 2</param>
        public Vec2F(float[] coords)
        {
            if (coords.Length < 2)
                throw new ArgumentException("not enough coords");

            X = coords[0];
            Y = coords[1];
        }

        /// <summary>
        /// Constructor using point</summary>
        /// <param name="p">Point</param>
        public Vec2F(PointF p)
        {
            X = p.X;
            Y = p.Y;
        }

        /// <summary>
        /// Converts a Vec2F to a PointF</summary>
        /// <param name="v">Vec2F</param>
        /// <returns>PointF</returns>
        public static implicit operator PointF(Vec2F v)
        {
            return new PointF(v.X, v.Y);
        }

        /// <summary>
        /// Gets the length of the vector</summary>
        public float Length
        {
            get { return (float)Math.Sqrt(X * X + Y * Y); }
        }

        /// <summary>
        /// Gets the squared length of the vector</summary>
        public float LengthSquared
        {
            get { return X * X + Y * Y; }
        }

        /// <summary>
        /// Gets the vector perpendicular to this one, rotated 90 degrees counter-clockwise</summary>
        public Vec2F Perp
        {
            get { return new Vec2F(-Y, X); }
        }

        /// <summary>
        /// Indexer into components, valid index range is [0, 1]</summary>
        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return X;
                    case 1: return Y;
                }
                throw new ArgumentOutOfRangeException();
            }

            set
            {
                switch (i)
                {
                    case 0: X = value; return;
                    case 1: Y = value; return;
                }
                throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the X-axis vector</summary>
        public static readonly Vec2F XAxis = new Vec2F(1, 0);

        /// <summary>
        /// Gets the Y-axis vector</summary>
        public static readonly Vec2F YAxis = new Vec2F(0, 1);

        /// <summary>
        /// Sets vector to the given coordinates</summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void Set(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Constructor using float array</summary>
        /// <param name="coords">Array of floats with Length >= 2</param>
        public void Set(float[] coords)
        {
            if (coords.Length < 2)
                throw new ArgumentException("not enough coords");

            X = coords[0];
            Y = coords[1];
        }

        /// <summary>
        /// Sets vector to the given vector</summary>
        /// <param name="v">Vector</param>
        public void Set(Vec2F v)
        {
            X = v.X;
            Y = v.Y;
        }

        /// <summary>
        /// Returns the negation of the given vector</summary>
        /// <param name="v">Vector</param>
        /// <returns>negation of the given vector</returns>
        public static Vec2F Neg(Vec2F v)
        {
            return new Vec2F(-v.X, -v.Y);
        }

        /// <summary>
        /// Negates this vector</summary>
        public void Neg()
        {
            Set(-X, -Y);
        }

        /// <summary>
        /// Returns the sum of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector sum</returns>
        public static Vec2F Add(Vec2F v1, Vec2F v2)
        {
            return new Vec2F(
                v1.X + v2.X,
                v1.Y + v2.Y);
        }

        /// <summary>
        /// Returns the difference of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 - Vector 2</returns>
        public static Vec2F Sub(Vec2F v1, Vec2F v2)
        {
            return new Vec2F(
                v1.X - v2.X,
                v1.Y - v2.Y);
        }

        /// <summary>
        /// Gets the component-wise product of the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector component-wise product</returns>
        public static Vec2F Mul(Vec2F v1, Vec2F v2)
        {
            return new Vec2F(
                v1.X * v2.X,
                v1.Y * v2.Y);
        }

        /// <summary>
        /// Gets the component-wise product of the vector and scalar</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scalar</param>
        /// <returns>Component-wise product</returns>
        public static Vec2F Mul(Vec2F v, float s)
        {
            return new Vec2F(
                v.X * s,
                v.Y * s);
        }

        /// <summary>
        /// Lerp (linear interpolate) from v1 to v2, result = u(1-t) + v(t)</summary>
        /// <param name="v1">First vector</param>
        /// <param name="v2">Second vector</param>
        /// <param name="t">Parameter</param>
        /// <returns>Lerp vector</returns>
        public static Vec2F Lerp(Vec2F v1, Vec2F v2, float t)
        {
            return new Vec2F(
                v1.X + (v2.X - v1.X) * t,
                v1.Y + (v2.Y - v1.Y) * t);
        }

        /// <summary>
        /// Clamps vector components to the given minimum and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="min">Component minimum</param>
        /// <returns>Clamped vector</returns>
        public static Vec2F ClampMin(Vec2F v, float min)
        {
            return new Vec2F(
                Math.Max(v.X, min),
                Math.Max(v.Y, min));
        }

        /// <summary>
        /// Clamps vector components to the given maximum and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="max">Component maximum</param>
        /// <returns>Clamped vector</returns>
        public static Vec2F ClampMax(Vec2F v, float max)
        {
            return new Vec2F(
                Math.Min(v.X, max),
                Math.Min(v.Y, max));
        }

        /// <summary>
        /// Clamps vector components to the given range and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <param name="min">Component minimum</param>
        /// <param name="max">Component maximum</param>
        /// <returns>Clamped vector</returns>
        public static Vec2F Clamp(Vec2F v, float min, float max)
        {
            return new Vec2F(
                Math.Min(Math.Max(v.X, min), max),
                Math.Min(Math.Max(v.Y, min), max));
        }

        /// <summary>
        /// Gets the component-wise absolute value of the given vector and returns new vector. 
        /// Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>Component-wise absolute value of the given vector</returns>
        public static Vec2F Abs(Vec2F v)
        {
            return new Vec2F(
                Math.Abs(v.X),
                Math.Abs(v.Y));
        }

        /// <summary>
        /// Gets the normal for the given vector and returns new vector. Original vector is unchanged.</summary>
        /// <param name="v">Vector</param>
        /// <returns>Normal for the given vector</returns>
        public static Vec2F Normalize(Vec2F v)
        {
            float len = v.Length;
            if (len < 0.000001f)
                return v;

            float ooLen = 1.0f / len;
            return new Vec2F(v.X * ooLen, v.Y * ooLen);
        }

        /// <summary>
        /// Normalizes this vector</summary>
        public void Normalize()
        {
            Set(Vec2F.Normalize(this));
        }

        /// <summary>
        /// Calculates the "Perp Dot" product of 2 vectors u and v, Dot(u.Perp, v)</summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>"Perp Dot" product of 2 vectors u and v, Dot(u.Perp, v)</returns>
        /// <remarks>See Graphics Gems IV, p. 138, for a good article on the applications of this function</remarks>
        public static float PerpDot(Vec2F u, Vec2F v)
        {
            return -u.Y * v.X + u.X * v.Y;
        }

        /// <summary>
        /// Calculates the dot product of two vectors</summary>
        /// <param name="u">First vector</param>
        /// <param name="v">Second vector</param>
        /// <returns>Dot product of vectors</returns>
        public static float Dot(Vec2F u, Vec2F v)
        {
            return u.X * v.X + u.Y * v.Y;
        }

        /// <summary>
        /// Calculates the distance between given points</summary>
        /// <param name="v1">Point 1</param>
        /// <param name="v2">Point 2</param>
        /// <returns>Distance between the given points</returns>
        public static float Distance(Vec2F v1, Vec2F v2)
        {
            Vec2F d = Sub(v1, v2);
            return d.Length;
        }

        /// <summary>
        /// Gets the angle between the given vectors</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Angle between the given vectors</returns>
        public static float Angle(Vec2F v1, Vec2F v2)
        {
            double vDot = Dot(v1, v2) / Math.Sqrt(Dot(v1, v1) * Dot(v2, v2));

            vDot = Math.Max(vDot, -1.0);
            vDot = Math.Min(vDot, 1.0);

            return (float)Math.Acos(vDot);
        }

        /// <summary>
        /// Tests for exact equality to given vector</summary>
        /// <param name="v">Other vector</param>
        /// <returns><c>True</c> if vectors are exactly equal</returns>
        public bool Equals(Vec2F v)
        {
            return
                X == v.X &&
                Y == v.Y;
        }

        /// <summary>
        /// Tests for equality to another vector, within a given epsilon</summary>
        /// <param name="v">Other vector</param>
        /// <param name="eps">Epsilon, or margin for error</param>
        /// <returns><c>True</c> if all components are within epsilon</returns>
        public bool Equals(Vec2F v, double eps)
        {
            return
                Math.Abs(X - v.X) < eps &&
                Math.Abs(Y - v.Y) < eps;
        }
      
        /// <summary>
        /// Converts vector to array of floats</summary>
        /// <returns>Array of 2 floats</returns>
        public float[] ToArray()
        {
            return new float[] {X,Y};            
        }

        #region Operator Overloads

        /// <summary>
        /// Negation operator</summary>
        /// <param name="v">Vector</param>
        /// <returns>Additive inverse of vector</returns>
        public static Vec2F operator -(Vec2F v)
        {
            return Vec2F.Neg(v);
        }

        /// <summary>
        /// Addition operator</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector sum</returns>
        public static Vec2F operator +(Vec2F v1, Vec2F v2)
        {
            return Vec2F.Add(v1, v2);
        }

        /// <summary>
        /// Subtraction operator</summary>
        /// <param name="v1">Vector 1</param>
        /// <param name="v2">Vector 2</param>
        /// <returns>Vector 1 - Vector 2</returns>
        public static Vec2F operator -(Vec2F v1, Vec2F v2)
        {
            return Vec2F.Sub(v1, v2);
        }

        /// <summary>
        /// Multiply by constant operator</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Scale factor</param>
        /// <returns>Scaled vector</returns>
        public static Vec2F operator *(Vec2F v, float s)
        {
            return Vec2F.Mul(v, s);
        }

        /// <summary>
        /// Multiply by constant operator</summary>
        /// <param name="s">Scale factor</param>
        /// <param name="v">Vector</param>
        /// <returns>Scaled vector</returns>
        public static Vec2F operator *(float s, Vec2F v)
        {
            return Vec2F.Mul(v, s);
        }

        /// <summary>
        /// Divide by constant operator</summary>
        /// <param name="v">Vector</param>
        /// <param name="s">Divisor</param>
        /// <returns>Scaled vector</returns>
        public static Vec2F operator /(Vec2F v, float s)
        {
            return Vec2F.Mul(v, 1.0f / s);
        }

        /// <summary>
        /// Equality operator</summary>
        /// <param name="v1">Left hand vector</param>
        /// <param name="v2">Right hand vector</param>
        /// <returns><c>True</c> if vectors are exactly equal</returns>
        public static bool operator ==(Vec2F v1, Vec2F v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>
        /// Inequality operator</summary>
        /// <param name="v1">Left hand vector</param>
        /// <param name="v2">Right hand vector</param>
        /// <returns><c>True</c> if vectors are not exactly equal</returns>
        public static bool operator !=(Vec2F v1, Vec2F v2)
        {
            return !v1.Equals(v2);
        }

        #endregion

        /// <summary>
        /// Indicates whether this instance and a specified object are exactly equal</summary>
        /// <param name="obj">Another object to compare to</param>
        /// <returns><c>True</c> if object and this instance are the same type and represent the same value</returns>
        public override bool Equals(object obj)
        {
            if (obj is Vec2F)
            {
                Vec2F other = (Vec2F)obj;
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
            return (int)(bits ^ (bits >> 32));
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
                "{0}{2} {1}",
                X.ToString(format, formatProvider),
                Y.ToString(format, formatProvider),
                listSeparator);
        }
        #endregion
    }
}
