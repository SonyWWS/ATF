//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Math utilities to perform less common operations, such as testing approximate equality, clamping and snapping</summary>
    public static class MathUtil
    {
        /// <summary>
        /// Given a bit field, efficiently calculates the log-base-2 of the highest bit.
        /// For example:
        /// 1 * 2^0 returns 0.
        /// 1 * 2^8 returns 8.
        /// 1 * 2^30 is the highest possible, because bitField is signed.</summary>
        /// <param name="bitField">Bit field, which should probably have only 1 bit set</param>
        /// <returns>Exponent n, such that 2 ^ n equals bitField</returns>
        public static int LogBase2(Int32 bitField)
        {
            int result;

            int bitFieldRight16 = bitField >> 16;
            int bitFieldRight24 = bitField >> 24;
            int bitFieldRight8 = bitField >> 8;

            if (bitFieldRight16 > 0)
            {
                if (bitFieldRight24 > 0)
                    result = 24 + s_logBase2Table[bitFieldRight24];
                else
                    result = 16 + s_logBase2Table[bitFieldRight16 & 0xFF];
            }
            else
            {
                if (bitFieldRight8 > 0)
                    result = 8 + s_logBase2Table[bitFieldRight8];
                else
                    result = s_logBase2Table[bitField];
            }

            return result;
        }

        /// <summary>
        /// Determines if just one bit of 'bitField' is set</summary>
        /// <param name="bitField">Bit field</param>
        /// <returns>True iff exactly one bit of 'bitField' is 1</returns>
        public static bool OnlyOneBitSet(Int32 bitField)
        {
            return
                bitField != 0 &&
                (bitField & (bitField - 1)) == 0;
        }

        /// <summary>
        /// Text whether all components of 2 vectors (expressed as float arrays)
        /// are within an error tolerance of each other</summary>
        /// <param name="v1">First vector (float array)</param>
        /// <param name="v2">Second vector (float array)</param>
        /// <param name="error">Error tolerance or allowable difference, expressed as a fraction
        /// of the magnitude of the largest of each component of the vectors</param>
        /// <returns>True iff all components of the vectors are within the error tolerance of each other</returns>
        /// <remarks>The vector arrays must have the same length</remarks>
        public static bool AreApproxEqual(float[] v1, float[] v2, double error)
        {
            if (v1.Length != v2.Length)
                throw new ArgumentException("Incompatible arrays");

            for (int i = 0; i < v1.Length; i++)
                if (!AreApproxEqual(v1[i], v2[i], error))
                    return false;

            return true;
        }

        /// <summary>
        /// Tests if all components of 2 vectors (expressed as double arrays)
        /// are within an error tolerance of each other</summary>
        /// <param name="v1">First vector (double array)</param>
        /// <param name="v2">Second vector (double array)</param>
        /// <param name="error">Error tolerance or allowable difference, expressed as a fraction
        /// of the magnitude of the largest of each component of the vectors</param>
        /// <returns>True iff all components of the vectors are within the error tolerance of each other</returns>
        public static bool AreApproxEqual(double[] v1, double[] v2, double error)
        {
            if (v1.Length != v2.Length)
                throw new ArgumentException("Incompatible arrays");
            
            for (int i = 0; i < v1.Length; i++)
                if (!AreApproxEqual(v1[i], v2[i], error))
                    return false;

            return true;
        }

        /// <summary>
        /// Tests if 2 numbers are within a specified error tolerance of each other</summary>
        /// <param name="x">First number</param>
        /// <param name="y">Second number</param>
        /// <param name="error">Error tolerance or the allowable difference, expressed as a fraction
        /// of the magnitude of the largest of the numbers</param>
        /// <returns>True iff x and y are within the error tolerance of each other</returns>
        public static bool AreApproxEqual(double x, double y, double error)
        {
            double difference = Math.Abs(x - y);
            double magnitude = Math.Max(Math.Abs(x), Math.Abs(y));
            return difference <= error * magnitude;
            // same as difference / magnitude < error, but safe
            // <= is important so it works when both x and y are 0 or exactly the same and error is 0
        }

        /// <summary>
        /// Snaps the given number x to the closest multiple of a step</summary>
        /// <param name="x">Number to snap</param>
        /// <param name="step">Step to snap to. Must not be negative. Zero is OK.</param>
        /// <returns>Closest multiple of step to number</returns>
        public static double Snap(double x, double step)
        {
            if (step < 0)
                throw new ArgumentException("step can't be negative");

            double result = x;

            const double MinFraction = 1.0 / (double.MaxValue / 2);
            if (Math.Abs(result) * MinFraction < step) // safe to divide?
            {
                result = result / step;
                result = Math.Floor(result + 0.5);
                result = result * step;
            }

            return result;
        }

        /// <summary>
        /// Snaps the given number x to the closest multiple of a step</summary>
        /// <param name="x">Number to snap</param>
        /// <param name="step">Step to snap to. Must not be negative. Zero is OK.</param>
        /// <returns>Closest multiple of step to number</returns>
        public static int Snap(int x, int step)
        {
            if (step < 0)
                throw new ArgumentException("step can't be negative");
            if (step == 0)
                return x; //to keep same semantics as the 'double' version

            int result;
            if (x >= 0)
                result = x + step / 2;
            else
                result = x - step / 2;
            result = result - result % step;

            return result;
        }

        /// <summary>
        /// Clamps arbitrary IComparable value to range [min, max]</summary>
        /// <remarks>Based on this Stack Overflow article: http://stackoverflow.com/questions/2683442/where-can-i-find-the-clamp-function-in-net </remarks>
        /// <typeparam name="T">Type of value or object being clamped</typeparam>
        /// <param name="value">Number to clamp</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Clamped number</returns>
        public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
        {
            if (Comparer<T>.Default.Compare(value, min) < 0)
                return min;

            if (Comparer<T>.Default.Compare(value, max) > 0)
                return max;

            return value;
        }

        /// <summary>
        /// Returns the maximum of two values.
        /// </summary>
        /// <param name="x">one of the two values to compare</param>
        /// <param name="y">second of the two values to compare</param>
        /// <returns>the maximum value</returns>
        public static T Max<T>(T x, T y) where T : IComparable<T>
        {
            return (Comparer<T>.Default.Compare(x, y) > 0) ? x : y;
        }

        /// <summary>
        /// Returns the minimum of two values.
        /// </summary>
        /// <param name="x">one of the two values to compare</param>
        /// <param name="y">second of the two values to compare</param>
        /// <returns>the minimum value</returns>
        public static T Min<T>(T x, T y) where T : IComparable<T>
        {
            return (Comparer<T>.Default.Compare(x, y) < 0) ? x : y;
        }

        /// <summary>
        /// Returns value confined to range [min, max), "wrapping around" at either end</summary>
        /// <param name="value">Number to wrap</param>
        /// <param name="min">Minimum value</param>
        /// <param name="max">Maximum value</param>
        /// <returns>Wrapped number</returns>
        public static int Wrap(int value, int min, int max)
        {
            int n;
            if (value >= min)
                n = (value - min) / (max - min);
            else
                n = (value - min + 1) / (max - min) - 1;
            value -= n * (max - min);
            return value;
        }

        /// <summary>
        /// Linearly interpolates from min to max, using a value in the range of 0 to 1</summary>
        /// <param name="value">A value in the range of 0 to 1</param>
        /// <param name="min">The low end of the range, corresponding to a value of 0</param>
        /// <param name="max">The high end of the range, corresponding to a value of 1</param>
        /// <returns>min + value * (max - min)</returns>
        /// <remarks>A better name for this method is LinearInterp or Lerp.</remarks>
        public static float Interp(float value, float min, float max)
        {
            return min + value * (max - min);
        }

        /// <summary>
        /// Takes a value in the range of [min,max] and returns a value in the range of [0,1],
        /// interpolated linearly. Is the inverse of Interp.</summary>
        /// <param name="value">A value in the range [min,max]</param>
        /// <param name="min">The minimum possible value. Must be less than 'max'.</param>
        /// <param name="max">The maximum possible value. Must be greater than 'min'.</param>
        /// <returns>(value - min) / (max - min)</returns>
        public static float ReverseInterp(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }
       
        /// <summary>
        /// Computes a Catmull-Rom interpolation 
        /// using the specified points.</summary>
        /// <param name="p1">The first  interpolation point</param>
        /// <param name="p2">The second interpolation point</param>
        /// <param name="p3">The third  interpolation point</param>
        /// <param name="p4">The fourth interpolation point</param>
        /// <param name="t">Interpolation amount on the interval [0,1]. t=0 returns p2 and t=1 returns p3.</param>
        /// <returns>The interpolated point</returns>
        public static float CatmullRomInterp(float p1, float p2, float p3, float p4, float t)
        {
            float t2 = t * t;
            float t3 = t * t2;
            float n = (2f * p2) + 
                      (p3-p1) * t + 
                      (2f * p1 - 5f * p2 + 4f * p3 - p4) * t2 +
                      (3f * p2 - p1 + p4 - 3f * p3)  * t3;
            return (n * 0.5f);
        }

        /// <summary>
        /// Gets the cubic Hermite spline interpolated value in one dimension</summary>
        /// <param name="p1">The first point</param>
        /// <param name="tan1">The tangent at the first point</param>
        /// <param name="p2">The second point</param>
        /// <param name="tan2">The tangent at the second point</param>
        /// <param name="t">Interpolation amount on the interval [0,1]. t=0 returns p1 and t=1 returns p2.</param>
        /// <returns>The interpolated point</returns>
        public static float HermiteInterp(float p1, float tan1, float p2, float tan2, float t)
        {           
            float tSquared = t * t;
            float tCubed = tSquared * t;
            return
                (2 * tCubed - 3 * tSquared + 1) * p1 +
                (tCubed - 2 * tSquared + t) * tan1 +
                (-2 * tCubed + 3 * tSquared) * p2 +
                (tCubed - tSquared) * tan2;
        }

        
        /// <summary>
        /// Computes cubic interpolation 
        /// using the specified points.</summary>
        /// <param name="p1">The first  interpolation point</param>
        /// <param name="p2">The second interpolation point</param>
        /// <param name="t">Interpolation amount on the interval [0,1]. t=0 returns p1 and t=1 returns p2.</param>
        /// <returns>The interpolated point</returns>
        public static float CubicSplineInterp(float p1, float p2, float t)
        {
            float e = (t * t) * (3f - 2f * t);
            return (p1 + e * (p2 - p1));            
        }


        /// <summary>
        /// Remaps value from one range to another</summary>
        /// <param name="value">Value to be remapped</param>
        /// <param name="min">Old minimum value</param>
        /// <param name="max">Old maximum value</param>
        /// <param name="newMin">New minimum value</param>
        /// <param name="newMax">New maximum value</param>
        /// <returns>Result in [newMin, newMax] equivalent to value's position in [min, max]</returns>
        public static float Remap(float value, float min, float max, float newMin, float newMax)
        {
            return Interp(ReverseInterp(value, min, max), newMin, newMax);
        }

        /// <summary>
        /// Determines which of two values is the closest to another value</summary>
        /// <param name="value">Value to find closest number to</param>
        /// <param name="cmp1">First number</param>
        /// <param name="cmp2">Second number</param>
        /// <returns>The closest of the two numbers (or first number, if they are equally close)</returns>
        public static float Closest(float value, float cmp1, float cmp2)
        {
            if (Math.Abs(cmp1 - value) <= Math.Abs(cmp2 - value))
                return cmp1;
            else
                return cmp2;
        }

        /// <summary>
        /// Returns the remainder of dividing x by y such that the remainder's sign matches the divisor's.
        /// This is different behavior from Math.IEEERemainder(x, y), which can return a negative number
        /// even if both x and y are positive.</summary>
        /// <param name="x">Dividend x</param>
        /// <param name="y">Divisor y, must be non-zero</param>
        /// <returns>Remainder, r, such that x = q*y + r where q is an integer and abs(r) is less than abs(y)</returns>
        /// <remarks>IEEERemainder takes the quotient (x / y) and rounds it to the nearest even integer
        /// before computing the remainder. This can produce an unexpected negative number. For example,
        /// IEEERemainder(3.0, 2.0) returns -1 because 3/2 = 1.5 and the 1.5 is rounded up to 2 so the
        /// remainder has to be -1.</remarks>
        public static double Remainder(double x, double y)
        {
            return x - (y * Math.Floor(x / y));
        }

        static MathUtil()
        {
            s_logBase2Table = new byte[256];
            s_logBase2Table[0] = 0;
            s_logBase2Table[1] = 0;

            for (int i = 2; i < 256; i++)
                s_logBase2Table[i] = (byte)(1 + s_logBase2Table[i / 2]);
        }

        private static readonly byte[] s_logBase2Table;
    }
}
