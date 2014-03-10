//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Media;

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
            Matrix transform = new Matrix();
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
            Matrix transform = new Matrix();
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
            s_tempPts[0] = p;
            matrix.Transform(s_tempPts);
            return s_tempPts[0];
        }

        /// <summary>
        /// Transforms x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, as from system A to system B</param>
        /// <param name="x">X-coordinate in coordinate system A</param>
        /// <returns>X-coordinate in coordinate system B</returns>
        public static double Transform(Matrix matrix, double x)
        {
            s_tempPts[0].X = x;
            s_tempPts[0].Y = 0.0f;
            matrix.Transform(s_tempPts);
            return s_tempPts[0].X;
        }

        /// <summary>
        /// Transforms vector's x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, as from system A to system B</param>
        /// <param name="x">X-coordinate of a vector in coordinate system A</param>
        /// <returns>X-coordinate of a vector in coordinate system B</returns>
        public static double TransformVector(Matrix matrix, double x)
        {
            s_tempVcs[0].X = x;
            s_tempVcs[0].Y = 0.0f;
            matrix.Transform(s_tempVcs);
            return s_tempVcs[0].X;
        }

        /// <summary>
        /// Transforms point by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="p">Point</param>
        /// <returns>Inverse transformed point</returns>
        public static Point InverseTransform(Matrix matrix, Point p)
        {
            matrix.Invert();
            s_tempPts[0] = p;
            matrix.Transform(s_tempPts);
            return s_tempPts[0];
        }

        /// <summary>
        /// Transforms rectangle</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Transformed rectangle</returns>
        public static Rect Transform(Matrix matrix, Rect r)
        {
            s_tempPts[0] = new Point(r.Left, r.Top);
            s_tempPts[1] = new Point(r.Right, r.Bottom);
            matrix.Transform(s_tempPts);
            return new Rect(
                s_tempPts[0].X, s_tempPts[0].Y,
                s_tempPts[1].X - s_tempPts[0].X, s_tempPts[1].Y - s_tempPts[0].Y);
        }

        /// <summary>
        /// Transforms rectangle by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Inverse transformed rectangle</returns>
        public static Rect InverseTransform(Matrix matrix, Rect r)
        {
            matrix.Invert();
            s_tempPts[0] = new Point(r.Left, r.Top);
            s_tempPts[1] = new Point(r.Right, r.Bottom);
            matrix.Transform(s_tempPts);
            return new Rect(
                s_tempPts[0].X, s_tempPts[0].Y,
                s_tempPts[1].X - s_tempPts[0].X, s_tempPts[1].Y - s_tempPts[0].Y);
        }

        private static Point[] s_tempPts = new Point[2];
        private static Vector[] s_tempVcs = new Vector[2];
    }
}
