//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using Sce.Atf.VectorMath;
using SharpDX;
using SharpDX.Direct2D1;
using RectangleF = System.Drawing.RectangleF;

namespace Sce.Atf.Direct2D
{
    internal static class D2dToSharpDX
    {
        /// <summary>
        /// convert PointF to SharpDX.Vector2.</summary>
        internal static Vector2 ToSharpDX(this PointF point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// convert Vec2F to SharpDX.Vector2.</summary>
        internal static Vector2 ToSharpDX(this Vec2F point)
        {
            return new Vector2(point.X, point.Y);
        }

        /// <summary>
        /// convert SizeF to DrawingSizeF.</summary>
        internal static Size2F ToSharpDX(this SizeF point)
        {
            return new Size2F(point.Width, point.Height);
        }

        /// <summary>
        /// convert RectangleF to SharpDX.RectangleF.</summary>
        internal static SharpDX.RectangleF ToSharpDX(this RectangleF rect)
        {
            return new SharpDX.RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// convert D2dRoundedRect to SharpDX.RoundedRectangle.</summary>
        internal static RoundedRectangle ToSharpDX(this D2dRoundedRect rec)
        {
            return new RoundedRectangle
            {
                RadiusX = rec.RadiusX,
                RadiusY = rec.RadiusY,
                Rect = rec.Rect.ToSharpDX()
            };
        }
        /// <summary>
        /// Converts D2dEllipse to SharpDX.Ellipse</summary>
        internal static Ellipse ToSharpDX(this D2dEllipse ellipse)
        {
            return new Ellipse
            {
                Point = ellipse.Center.ToSharpDX(),
                RadiusX = ellipse.RadiusX,
                RadiusY = ellipse.RadiusY
            };
        }

        /// <summary>
        /// Converts D2dBezierSegment to SharpDX.BezierSegment</summary>
        internal static BezierSegment ToSharpDX(this D2dBezierSegment seg)
        {
            return new BezierSegment
            {
                Point1 = seg.Point1.ToSharpDX(),
                Point2 = seg.Point2.ToSharpDX(),
                Point3 = seg.Point3.ToSharpDX(),
            };
        }

        /// <summary>
        /// convert D2dArcSegment to SharpDX.ArcSegment.</summary>
        internal static ArcSegment ToSharpDX(this D2dArcSegment arc)
        {
            return new ArcSegment
            {
                Point = arc.Point.ToSharpDX(),
                Size = arc.Size.ToSharpDX(),
                RotationAngle = arc.RotationAngle,
                ArcSize = (ArcSize)arc.ArcSize,
                SweepDirection = (SweepDirection)arc.SweepDirection
            };
        }
    }
}
