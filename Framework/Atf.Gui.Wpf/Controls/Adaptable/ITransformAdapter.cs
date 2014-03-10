//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for adapters that define a transform for the adapted control
    /// consisting of a scale, followed by a translation</summary>
    public interface ITransformAdapter
    {
        /// <summary>
        /// Gets the current transformation matrix</summary>
        Matrix Transform
        {
            get;
        }

        /// <summary>
        /// Gets or sets the translation</summary>
        Point Translation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum values for x and y translation</summary>
        Point MinTranslation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum values for x and y translation</summary>
        Point MaxTranslation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the scale</summary>
        Point Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the minimum values for x and y scale</summary>
        Point MinScale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum values for x and y scale</summary>
        Point MaxScale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the transform scale is constrained to be uniform
        /// (same in x and y direction)</summary>
        bool UniformScale
        {
            get;
        }

        /// <summary>
        /// Sets the transform scale and translation</summary>
        /// <param name="xScale">X scale</param>
        /// <param name="yScale">Y scale</param>
        /// <param name="xTranslation">X translation</param>
        /// <param name="yTranslation">Y translation</param>
        void SetTransform(double xScale, double yScale, double xTranslation, double yTranslation);

        /// <summary>
        /// Event that is raised after the transform changed</summary>
        event EventHandler TransformChanged;
    }

    /// <summary>
    /// Useful static/extension methods on ITransformAdapter</summary>
    public static class TransformAdapters
    {
        /// <summary>
        /// Sets the transform to the given matrix; rotation and skew are ignored</summary>
        /// <param name="transformAdapter">Adapter managing transform</param>
        /// <param name="transform">Transformation matrix</param>
        public static void SetTransform(this ITransformAdapter transformAdapter, Matrix transform)
        {
            transformAdapter.SetTransform(transform.M11, transform.M22, transform.OffsetX, transform.OffsetY);
        }

        /// <summary>
        /// Transforms client rectangle to transform coordinates</summary>
        /// <param name="adapter">Adapter managing transform</param>
        /// <param name="x">Rectangle, in client coordinates</param>
        /// <returns>Rectangle in transform coordinates</returns>
        public static Rect ClientToTransform(this ITransformAdapter adapter, Rect x)
        {
            return MathUtil.InverseTransform(adapter.Transform, x);
        }

        /// <summary>
        /// Transforms transform rectangle to client coordinates</summary>
        /// <param name="adapter">Adapter managing transform</param>
        /// <param name="x">Rectangle, in transform coordinates</param>
        /// <returns>Rectangle in client coordinates</returns>
        public static Rect TransformToClient(this ITransformAdapter adapter, Rect x)
        {
            return MathUtil.Transform(adapter.Transform, x);
        }

        /// <summary>
        /// Transforms client point to transform coordinates</summary>
        /// <param name="adapter">Adapter managing transform</param>
        /// <param name="x">Point, in client coordinates</param>
        /// <returns>Point in transform coordinates</returns>
        public static Point ClientToTransform(this ITransformAdapter adapter, Point x)
        {
            return MathUtil.InverseTransform(adapter.Transform, x);
        }

        /// <summary>
        /// Transforms transform point to client coordinates</summary>
        /// <param name="adapter">Adapter managing transform</param>
        /// <param name="x">Point in transform coordinates</param>
        /// <returns>Point in client coordinates</returns>
        public static Point TransformToClient(this ITransformAdapter adapter, Point x)
        {
            return MathUtil.Transform(adapter.Transform, x);
        }

        /// <summary>
        /// Constrains translation to the adapter's MinTranslation and MaxTranslation property
        /// constraints</summary>
        /// <param name="adapter">Transform adapter</param>
        /// <param name="translation">Desired translation</param>
        /// <returns>Translation constrained to the adapter's limits</returns>
        public static Point ConstrainTranslation(this ITransformAdapter adapter, Point translation)
        {
            Point minTranslation = adapter.MinTranslation;
            Point maxTranslation = adapter.MaxTranslation;
            return new Point(
                Math.Max(minTranslation.X, Math.Min(maxTranslation.X, translation.X)),
                Math.Max(minTranslation.Y, Math.Min(maxTranslation.Y, translation.Y)));
        }

        /// <summary>
        /// Constrains scale to the adapter's MinScale, MaxScale, and UniformScale property
        /// constraints</summary>
        /// <param name="adapter">Transform adapter</param>
        /// <param name="scale">Desired scale</param>
        /// <returns>Scale, constrained to the adapter's limits</returns>
        public static Point ConstrainScale(this ITransformAdapter adapter, Point scale)
        {
            Point minScale = adapter.MinScale;
            Point maxScale = adapter.MaxScale;

            if (adapter.UniformScale)
            {
                scale.X = scale.Y = Math.Max(scale.X, scale.Y);
            }

            return new Point(
                Math.Max(minScale.X, Math.Min(maxScale.X, scale.X)),
                Math.Max(minScale.Y, Math.Min(maxScale.Y, scale.Y)));
        }

        /// <summary>
        /// Sets scroll and zoom so that the largest axis of the given rectangle almost
        /// fills the client area</summary>
        /// <param name="adapter">Adapter managing transform</param>
        /// <param name="itemBounds">Rectangle to frame, in client coordinates</param>
        /// <param name="clientBounds">Client area</param>
        public static void Frame(this ITransformAdapter adapter, Rect itemBounds, Rect clientBounds)
        {
            if (adapter == null || itemBounds.IsEmpty || !adapter.Transform.HasInverse)
                return;

            Rect worldBounds = MathUtil.InverseTransform(adapter.Transform, itemBounds);

            // calculate scale so bounding rectangle (in world coordinates) fills client
            // rectangle with some margin around the edges
            const double MarginScale = 0.86;
            Point scale = new Point(
                Math.Abs(clientBounds.Width / worldBounds.Width) * MarginScale,
                Math.Abs(clientBounds.Height / worldBounds.Height) * MarginScale);

            if (adapter.UniformScale)
                scale.X = scale.Y = Math.Min(scale.X, scale.Y);

            scale = TransformAdapters.ConstrainScale(adapter, scale);

            // calculate translation needed to put bounds center at center of view
            Point worldBoundsCenter = new Point(
                worldBounds.X + worldBounds.Width / 2,
                worldBounds.Y + worldBounds.Height / 2);

            Point translation = new Point(
                clientBounds.Width / 2 - worldBoundsCenter.X * scale.X,
                clientBounds.Height / 2 - worldBoundsCenter.Y * scale.Y);

            adapter.SetTransform(scale.X, scale.Y, translation.X, translation.Y);
        }

        /// <summary>
        /// If the given rectangle isn't visible, sets scroll and scale so that the largest axis
        /// of the rectangle almost fills the client area</summary>
        /// <param name="adapter">Adapter managing transform</param>
        /// <param name="itemBounds">Rectangle to ensure visible</param>
        /// <param name="clientBounds">Client area</param>
        public static void EnsureVisible(this ITransformAdapter adapter, Rect itemBounds, Rect clientBounds)
        {
            // check rectangle is already in the visible rect
            if (clientBounds.Contains(itemBounds))
            {
                // already visible
                return;
            }

            TransformAdapters.Frame(adapter, itemBounds, clientBounds);
        }

        /// <summary>
        /// Sets scroll and zoom to center the given rectangle in the client area with a given scale</summary>
        /// <param name="adapter">Adapter managing transform</param>
        /// <param name="scale">Scale</param>
        /// <param name="itemBounds">Rectangle to frame, in client coordinates</param>
        /// <param name="clientBounds">Client area</param>
        public static void ZoomAboutCenter(this ITransformAdapter adapter, Point scale, Rect itemBounds, Rect clientBounds)
        {
            if (adapter == null || !adapter.Transform.HasInverse)
                return;

            Rect worldBounds = MathUtil.InverseTransform(adapter.Transform, itemBounds);

            if (adapter.UniformScale)
                scale.X = scale.Y = Math.Min(scale.X, scale.Y);

            scale = TransformAdapters.ConstrainScale(adapter, scale);

            // calculate translation needed to put bounds center at center of view
            Point worldBoundsCenter = new Point(
                worldBounds.X + worldBounds.Width / 2,
                worldBounds.Y + worldBounds.Height / 2);

            Point translation = new Point(
                clientBounds.Width / 2 - worldBoundsCenter.X * scale.X,
                clientBounds.Height / 2 - worldBoundsCenter.Y * scale.Y);

            adapter.SetTransform(scale.X, scale.Y, translation.X, translation.Y);
        }
    }
}
