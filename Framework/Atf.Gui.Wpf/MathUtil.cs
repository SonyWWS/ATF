//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Media;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Mathematics utilities</summary>
    public static class MathUtil
    {
        /// <summary>
        /// Creates a GDI transform representing a uniform scale and translation</summary>
        /// <param name="translation">Translation</param>
        /// <param name="scale">Scale</param>
        /// <returns>GDI transform representing a uniform scale and translation</returns>
        public static Matrix GetTransform(Point translation, double scale)
        {
            var transform = new Matrix();
            transform.Translate(translation.X, translation.Y);
            transform.Scale(scale, scale);
            return transform;
        }

        /// <summary>
        /// Creates a GDI transform representing a non-uniform scale and translation</summary>
        /// <param name="translation">Translation</param>
        /// <param name="xScale">X scale</param>
        /// <param name="yScale">Y scale</param>
        /// <returns>GDI transform representing a non-uniform scale and translation</returns>
        public static Matrix GetTransform(Point translation, double xScale, double yScale)
        {
            var transform = new Matrix();
            transform.Translate(translation.X, translation.Y);
            transform.Scale(xScale, yScale);
            return transform;
        }

        /// <summary>
        /// Transforms point</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="p">Point</param>
        /// <returns>Transformed point</returns>
        public static Point Transform(Matrix matrix, Point p)
        {
            return matrix.Transform(p);
        }

        /// <summary>
        /// Transforms x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, as from system A to system B</param>
        /// <param name="x">X-coordinate in coordinate system A</param>
        /// <returns>X-coordinate in coordinate system B</returns>
        public static double Transform(Matrix matrix, double x)
        {
            return matrix.Transform(new Point(x, 0.0)).X;
        }

        /// <summary>
        /// Transforms vector's x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, as from system A to system B</param>
        /// <param name="x">X-coordinate of a vector in coordinate system A</param>
        /// <returns>X-coordinate of a vector in coordinate system B</returns>
        public static double TransformVector(Matrix matrix, double x)
        {
            return matrix.Transform(new Vector(x, 0.0)).X;
        }

        /// <summary>
        /// Transforms point by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="p">Point</param>
        /// <returns>Inverse transformed point</returns>
        public static Point InverseTransform(Matrix matrix, Point p)
        {
            if (matrix.HasInverse)
                matrix.Invert();

            return matrix.Transform(p);
        }

        /// <summary>
        /// Transforms rectangle</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Transformed rectangle</returns>
        public static Rect Transform(Matrix matrix, Rect r)
        {
            var result = new [] { new Point(r.Left, r.Top), new Point(r.Right, r.Bottom) };
            matrix.Transform(result);
            return new Rect(result[0].X, result[0].Y, result[1].X - result[0].X, result[1].Y - result[0].Y);
        }

        /// <summary>
        /// Transforms rectangle by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Inverse transformed rectangle</returns>
        public static Rect InverseTransform(Matrix matrix, Rect r)
        {
            if (matrix.HasInverse)
                matrix.Invert();

            var result = new[] { new Point(r.Left, r.Top), new Point(r.Right, r.Bottom) };
            matrix.Transform(result);
            return new Rect(result[0].X, result[0].Y, result[1].X - result[0].X, result[1].Y - result[0].Y);
        }

        /// <summary>
        /// Round a double value to a given number of fractional digits</summary>
        /// <param name="value">Value to round</param>
        /// <param name="digits">Rounding number of fractional digits</param>
        /// <returns>Rounded double value</returns>
        public static double RoundToDoublePrecision(double value, int digits)
        {
            double d = Math.Abs(value);
            if (d >= 1.0)
            {
                digits -= (int)Math.Ceiling(Math.Log10(d));
            }
            
            if (digits >= 0)
            {
                value = Math.Round(value, digits);
            }
            
            return value;
        }

        /// <summary>
        /// Calculates the minimum distance squared between the starting rectangle and the target,
        /// or returns int.MaxValue if the target rectangle is not visible in the given direction</summary>
        /// <param name="startRect">Starting rectangle</param>
        /// <param name="arrow">The direction to look in. Must be Up, Right, Down, or Left.</param>
        /// <param name="targetRect">Destination rectangle, to be tested against</param>
        /// <returns>The distance minimum squared between the rectangles, or double.MaxValue</returns>
        public static double CalculateDistance(Rect startRect, Keys arrow, Rect targetRect)
        {
            // Transform the problem so that the appropriate two edges of the two rectangles
            //  are rotated to be parallel to the x-axis with the target having a greater y than
            //  the starting edge if it is in front of the starting edge. In all cases, 'left'
            //  will be <= 'right'. And startY <= targetY if the target is visible.
            double startLeft, startRight, startY;
            double targetLeft, targetRight, targetY, targetFarY;
            switch (arrow)
            {
                case Keys.Up:
                    startLeft = startRect.Left; startRight = startRect.Right; startY = -startRect.Top;
                    targetLeft = targetRect.Left; targetRight = targetRect.Right; targetY = -targetRect.Bottom;
                    targetFarY = -targetRect.Top;
                    break;
                case Keys.Right:
                    startLeft = startRect.Top; startRight = startRect.Bottom; startY = startRect.Right;
                    targetLeft = targetRect.Top; targetRight = targetRect.Bottom; targetY = targetRect.Left;
                    targetFarY = targetRect.Right;
                    break;
                case Keys.Down:
                    startLeft = startRect.Left; startRight = startRect.Right; startY = startRect.Bottom;
                    targetLeft = targetRect.Left; targetRight = targetRect.Right; targetY = targetRect.Top;
                    targetFarY = targetRect.Bottom;
                    break;
                case Keys.Left:
                    startLeft = startRect.Top; startRight = startRect.Bottom; startY = -startRect.Left;
                    targetLeft = targetRect.Top; targetRight = targetRect.Bottom; targetY = -targetRect.Right;
                    targetFarY = -targetRect.Right;
                    break;
                default:
                    throw new ArgumentException("'arrow' must be a single arrow key");
            }

            // Try to exclude the target from this quadrant.
            if (startY > targetFarY)
                return double.MaxValue;

            double farthestDistY = targetFarY - startY;
            if (targetRight < startLeft - farthestDistY ||
                targetLeft > startRight + farthestDistY)
                return double.MaxValue;

            // The target is definitely in this quadrant. Find the distance squared.
            double nearestDistY = targetY - startY;
            if (targetRight < startLeft)
            {
                double distX = startLeft - targetRight;
                return distX * distX + nearestDistY * nearestDistY;
            }
            
            if (targetLeft > startRight)
            {
                double distX = targetLeft - startRight;
                return distX * distX + nearestDistY * nearestDistY;
            }
            
            return nearestDistY * nearestDistY;
        }
    }
}
