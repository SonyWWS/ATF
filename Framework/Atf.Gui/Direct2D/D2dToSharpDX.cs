//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using SharpDX;
using SharpDX.Direct2D1;
using RectangleF = System.Drawing.RectangleF;

namespace Sce.Atf.Direct2D
{
    internal static class D2dToSharpDX
    {
        /// <summary>
        /// convert PointF to DrawingPointF.</summary>
        internal static DrawingPointF ToSharpDX(this PointF point)
        {
            return new DrawingPointF(point.X, point.Y);
        }

        /// <summary>
        /// convert SizeF to DrawingSizeF.</summary>
        internal static DrawingSizeF ToSharpDX(this SizeF point)
        {
            return new DrawingSizeF(point.Width, point.Height);
        }

        /// <summary>
        /// convert RectangleF to SharpDX.RectangleF.</summary>
        internal static SharpDX.RectangleF ToSharpDX(this RectangleF point)
        {
            return new SharpDX.RectangleF(point.Left, point.Top, point.Right, point.Bottom);
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
        /// convert D2dEllipse to SharpDX.Ellipse.</summary>
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
        /// convert D2dBezierSegment to SharpDX.BezierSegment.</summary>
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
