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

    /// <summary>
    /// Segment in Bezier curve</summary>
    public struct D2dBezierSegment
    {
        /// <summary>
        /// Control point 1</summary>
        public PointF Point1;
        /// <summary>
        /// Control point 2</summary>
        public PointF Point2;
        /// <summary>
        /// Control point 3</summary>
        public PointF Point3;
    }

    /// <summary>
    /// Direction in which arcs are drawn</summary>
    public enum D2dSweepDirection : int
    {
        /// <summary>Arcs are drawn in a counterclockwise (negative-angle) direction</summary>
        CounterClockwise = 0,
        /// <summary>Arcs are drawn in a clockwise (positive-angle) direction</summary>
        Clockwise = 1,
    }

    /// <summary>
    /// Sweep size of arc</summary>
    public enum D2dArcSize
    {
        /// <summary>An arc's sweep should be 180 degrees or less</summary>
        Small = 0,
        /// <summary>An arc's sweep should be 180 degrees or greater</summary>
        Large = 1,
    }

    /// <summary>
    /// Arc segment
    /// </summary>
    public struct D2dArcSegment
    {
        /// <summary>
        /// End point</summary>
        public PointF Point;
        /// <summary>
        /// Arc's bounding rectangle size
        /// </summary>
        public SizeF Size;
        /// <summary>
        /// Arc rotation angle</summary>
        public float RotationAngle;
        /// <summary>
        /// Arc sweep direction</summary>
        public D2dSweepDirection SweepDirection;
        /// <summary>
        /// Arc sweep size</summary>
        public D2dArcSize ArcSize;
    }
}
