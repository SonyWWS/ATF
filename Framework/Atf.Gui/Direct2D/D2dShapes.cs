//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Contains the dimensions and corner radii of a rounded rectangle</summary>
    public struct D2dRoundedRect
    {
        /// <summary>
        /// The x-radius for the quarter ellipse that is drawn to replace every corner
        /// of the rectangle</summary>
        public float RadiusX;

        /// <summary>
        /// The y-radius for the quarter ellipse that is drawn to replace every corner
        /// of the rectangle</summary>
        public float RadiusY;

        /// <summary>
        /// The coordinates of the rectangle</summary>
        public RectangleF Rect;
    }

    /// <summary>
    /// Contains the center point, x-radius, and y-radius of an ellipse</summary>
    public struct D2dEllipse
    {
        /// <summary>
        /// The center point of the ellipse</summary>
        public PointF Center;

        /// <summary>
        /// The x-radius of the ellipse</summary>
        public float RadiusX;

        /// <summary>
        /// The y-radius of the ellipse</summary>
        public float RadiusY;

        /// <summary>
        /// Constructs a new instance of the D2dEllipse struct</summary>
        /// <param name="center">The center</param>
        /// <param name="radiusX">The x radius</param>
        /// <param name="radiusY">The y radius</param>
        public D2dEllipse(PointF center, float radiusX, float radiusY)
        {
            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        /// <summary>
        /// Explicit cast for D2dEllipse</summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>Cast value</returns>
        public static explicit operator D2dEllipse(Rectangle rect)
        {
            D2dEllipse temp = new D2dEllipse();
            float radx = (float)rect.Width / 2.0f;
            float rady = (float)rect.Height / 2.0f;
            temp.Center.X = rect.X + radx;
            temp.Center.Y = rect.Y + rady;
            temp.RadiusX = radx;
            temp.RadiusY = rady;
            return temp;
        }

        /// <summary>
        /// Explicit cast for D2dEllipse</summary>
        /// <param name="rect">Rectangle</param>
        /// <returns>Cast value</returns>
        public static explicit operator D2dEllipse(RectangleF rect)
        {
            D2dEllipse temp = new D2dEllipse();
            float radx = rect.Width / 2.0f;
            float rady = rect.Height / 2.0f;
            temp.Center.X = rect.X + radx;
            temp.Center.Y = rect.Y + rady;
            temp.RadiusX = radx;
            temp.RadiusY = rady;
            return temp;
        }
    }

    public struct D2dBezierSegment
    {
        public PointF Point1;
        public PointF Point2;
        public PointF Point3;
    }

    public enum D2dSweepDirection : int
    {
        /// <summary>Arcs are drawn in a counterclockwise (negative-angle) direction.</summary>
        CounterClockwise = 0,
        /// <summary>Arcs are drawn in a clockwise (positive-angle) direction.</summary>
        Clockwise = 1,
    }

    public enum D2dArcSize
    {
        /// <summary>An arc's sweep should be 180 degrees or less.</summary>
        Small = 0,
        /// <summary>An arc's sweep should be 180 degrees or greater.</summary>
        Large = 1,
    }

    public struct D2dArcSegment
    {
        public PointF Point;
        public SizeF Size;
        public float RotationAngle;
        public D2dSweepDirection SweepDirection;
        public D2dArcSize ArcSize;
    }
}
