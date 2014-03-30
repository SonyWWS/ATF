//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct2D1;

using Sce.Atf.Adaptation;
using Sce.Atf.VectorMath;
using Sce.Atf.Controls.Adaptable.Graphs;

using Graphics = System.Drawing.Graphics;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;
using PointF = System.Drawing.PointF;
using RectangleF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;
using GdiPixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Sce.Atf.Direct2D
{
    /// <summary>    
    /// Represents an object that can receive drawing commands.</summary>
    /// <remarks>
    /// Use D2dFactory to create an instance of D2dGraphics, or use ATF's derived classes.
    /// New abstract methods may be added to this class in the future.
    /// 
    /// Your application should create a D2dGraphics object once per control and hold
    /// onto it for the life of the control.</remarks>
    public abstract class D2dGraphics : D2dResource
    {
        /// <summary>
        /// Gets and sets the current transform of the D2dGraphics</summary>
        public Matrix3x2F Transform
        {
            get { return m_xform; }
            set
            {
                if (!m_xform.Equals(value))
                {
                    m_xform = value;
                    Matrix3x2 xform;
                    xform.M11 = m_xform.M11;
                    xform.M12 = m_xform.M12;
                    xform.M21 = m_xform.M21;
                    xform.M22 = m_xform.M22;
                    xform.M31 = m_xform.DX;
                    xform.M32 = m_xform.DY;
                    m_renderTarget.Transform = xform;

                    var x_axis = new Vec2F(xform.M11, xform.M12);
                    var y_axis = new Vec2F(xform.M21, xform.M22);
                    m_scale = new PointF(x_axis.Length, y_axis.Length);
                    // apply same transform to gdi+ device.
                    if (m_graphics != null)
                        ApplyXformToGdi();
                }
            }
        }

        /// <summary>
        /// Changes the origin of the coordinate system by prepending the specified translation
        /// to the Transform property</summary>
        /// <param name="dx">X translation</param>
        /// <param name="dy">Y translation</param>
        /// <remarks>Modeled after GDI version http://msdn.microsoft.com/en-us/library/6a1d65f4.aspx </remarks>
        public void TranslateTransform(float dx, float dy)
        {
            var trans = Matrix3x2F.CreateTranslation(dx, dy);
            var xform = Transform;
            Transform = (trans * xform);
        }

        /// <summary>
        /// Changes the scale of the coordinate system by prepending the specified scale
        /// to the Transform property</summary>
        /// <param name="sx">Amount to scale by in the x-axis direction</param>
        /// <param name="sy">Amount to scale by in the y-axis direction</param>
        public void ScaleTransform(float sx, float sy)
        {
            var trans = Matrix3x2F.CreateScale(sx, sy);
            var xform = Transform;
            Transform = (trans * xform);
        }

        /// <summary>
        /// Changes the rotation of the coordinate system by prepending the specified rotation
        /// to the Transform property</summary>
        /// <param name="angle">Angle of rotation, in degrees</param>
        public void RotateTransform(float angle)
        {
            var trans = Matrix3x2F.CreateRotation(angle);
            var xform = Transform;
            Transform = (trans * xform);
        }

        /// <summary>
        /// Gets and sets the current antialiasing mode for nontext drawing operations</summary>
        public D2dAntialiasMode AntialiasMode
        {
            get { return (D2dAntialiasMode)m_renderTarget.AntialiasMode; }
            set { m_renderTarget.AntialiasMode = (AntialiasMode)value; }
        }

        /// <summary>
        /// Gets the pixel size of the D2dGraphics in device pixels</summary>
        public Size PixelSize
        {
            get { return new Size(m_renderTarget.PixelSize.Width, m_renderTarget.PixelSize.Height); }
        }

        /// <summary>
        /// Gets the size of the D2dGraphics in device-independent pixels (DIPs)</summary>
        public SizeF Size
        {
            get { return new SizeF(m_renderTarget.Size.Width, m_renderTarget.Size.Height); }
        }

        /// <summary>
        /// Gets the current antialiasing mode for text and glyph drawing operations</summary>
        public D2dTextAntialiasMode TextAntialiasMode
        {
            get { return (D2dTextAntialiasMode)m_renderTarget.TextAntialiasMode; }
            set { m_renderTarget.TextAntialiasMode = (TextAntialiasMode)value; }
        }

        /// <summary>
        ///  Get or sets the dots per inch (DPI) of the D2dGraphics</summary>
        /// <remarks>
        ///  This method specifies the mapping from pixel space to device-independent
        ///  space for the D2dGraphics. If both dpiX and dpiY are 0, the factory-read
        ///  system DPI is chosen. If one parameter is zero and the other unspecified,
        ///  the DPI is not changed. For D2dHwndGraphics, the DPI
        ///  defaults to the most recently factory-read system DPI. The default value
        ///  for all other D2dGraphics is 96 DPI.</remarks>
        public SizeF DotsPerInch
        {
            get { return new SizeF(m_renderTarget.DotsPerInch.Width, m_renderTarget.DotsPerInch.Height); }
            set { m_renderTarget.DotsPerInch = value.ToSharpDX(); }
        }

        /// <summary>
        /// Clears the drawing area to the specified color</summary>
        /// <param name="color">The color to which the drawing area is cleared</param>
        public void Clear(Color color)
        {
            m_renderTarget.Clear(color.ToColor4());
        }

        /// <summary>
        /// Initiates drawing on this D2dGraphics</summary>
        public void BeginDraw()
        {
            D2dFactory.CheckForRecreateTarget();
            m_renderTarget.BeginDraw();
        }

        /// <summary>
        /// Ends drawing operations on the D2dGraphics and indicates the current error state</summary>
        /// <returns>
        /// If the method succeeds, it returns D2dResult.OK. Otherwise, it throws an exception.
        /// Also it may raise RecreateResources event.</returns>
        public D2dResult EndDraw()
        {
            try
            {
                m_renderTarget.EndDraw();
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode == D2DERR_RECREATE_TARGET
                 || ex.ResultCode == D2DERR_WRONG_RESOURCE_DOMAIN)
                {
                    D2dFactory.CheckForRecreateTarget();
                    RecreateTargetAndResources();

                }
                else
                    throw;
            }
            return D2dResult.Ok;
        }

        /// <summary>
        /// Draws an arc representing a portion of an ellipse specified by a D2dEllipse</summary>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">The brush used to paint the arc's outline</param>
        /// <param name="startAngle">Starting angle in degrees measured clockwise from the x-axis 
        /// to the starting point of the arc</param>
        /// <param name="sweepAngle">Sweep angle in degrees measured clockwise from the startAngle 
        /// parameter to ending point of the arc</param>
        /// <param name="strokeWidth">The thickness of the ellipse's stroke. The stroke is centered 
        /// on the ellipse's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the arc's outline or null to draw a solid line</param>
        public void DrawArc(D2dEllipse ellipse, D2dBrush brush, float startAngle, float sweepAngle, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            // compute steps
            float step = Tessellation / m_scale.X;
            float angle1 = startAngle * ToRadian;
            float angle2 = (startAngle + sweepAngle) * ToRadian;
            if (angle1 > angle2)
            {
                float temp = angle1;
                angle1 = angle2;
                angle2 = temp;
            }

            float cx = ellipse.Center.X;
            float cy = ellipse.Center.Y;

            var v1 = new Vec2F();
            v1.X = ellipse.RadiusX * (float)Math.Cos(angle1);
            v1.Y = ellipse.RadiusY * (float)Math.Sin(angle1);

            var v2 = new Vec2F();
            v2.X = ellipse.RadiusX * (float)Math.Cos(angle2);
            v2.Y = ellipse.RadiusY * (float)Math.Sin(angle2);

            float arcLen = (v2 - v1).Length; // approx arc len.
            float numSegs = arcLen / step;
            float dtheta = (angle2 - angle1) / numSegs;

            m_tempPoints.Clear();
            for (float theta = angle1; theta < angle2; theta += dtheta)
            {
                var pt = new PointF();
                pt.X = cx + ellipse.RadiusX * (float)Math.Cos(theta);
                pt.Y = cy + ellipse.RadiusY * (float)Math.Sin(theta);
                m_tempPoints.Add(pt);
            }
            DrawLines(m_tempPoints, brush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws an arc representing a portion of an ellipse specified by a D2dEllipse</summary>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="color">The color used to paint the arc's outline</param>
        /// <param name="startAngle">Starting angle in degrees measured clockwise from the x-axis 
        /// to the starting point of the arc</param>
        /// <param name="sweepAngle">Sweep angle in degrees measured clockwise from the startAngle 
        /// parameter to ending point of the arc</param>
        /// <param name="strokeWidth">The thickness of the ellipse's stroke. The stroke is centered 
        /// on the ellipse's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the arc's outline or null to draw a solid line</param>
        public void DrawArc(D2dEllipse ellipse, Color color, float startAngle, float sweepAngle, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawArc(ellipse, m_solidColorBrush, startAngle, sweepAngle, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws a Bézier spline defined by four System.Drawing.PointF structures</summary>
        /// <param name="pt1">Represents the starting point of the curve</param>
        /// <param name="pt2">Represents the first control point for the curve</param>
        /// <param name="pt3">Represents the second control point for the curve</param>
        /// <param name="pt4">Represents the ending point of the curve</param>
        /// <param name="brush">The brush used to paint the curve's stroke</param>
        /// <param name="strokeWidth">The thickness of the geometry's stroke. The stroke is centered on the geometry's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the geometry's outline or null to draw a solid line</param>
        public void DrawBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            using (var geom = new PathGeometry(D2dFactory.NativeFactory))
            {
                var sink = geom.Open();
                sink.BeginFigure(pt1.ToSharpDX(), FigureBegin.Hollow);
                var seg = new BezierSegment
                {
                    Point1 = pt2.ToSharpDX(),
                    Point2 = pt3.ToSharpDX(),
                    Point3 = pt4.ToSharpDX()
                };
                sink.AddBezier(seg);
                sink.EndFigure(FigureEnd.Open);
                sink.Close();
                sink.Dispose();

                var stroke = strokeStyle == null ? null : strokeStyle.NativeStrokeStyle;
                m_renderTarget.DrawGeometry(geom, brush.NativeBrush, strokeWidth, stroke);
            }
        }

        /// <summary>
        /// Draws a Bézier spline defined by four System.Drawing.PointF structures</summary>
        /// <param name="pt1">Represents the starting point of the curve</param>
        /// <param name="pt2">Represents the first control point for the curve</param>
        /// <param name="pt3">Represents the second control point for the curve</param>
        /// <param name="pt4">Represents the ending point of the curve</param>
        /// <param name="color">The color used to paint the curve's stroke</param>
        /// <param name="strokeWidth">The thickness of the geometry's stroke. The stroke is centered on the geometry's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the geometry's outline or null to draw a solid line</param>
        public void DrawBezier(PointF pt1, PointF pt2, PointF pt3, PointF pt4, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawBezier(pt1, pt2, pt3, pt4, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws the specified bitmap at a specified point using the
        /// D2dGraphic's DPI instead of the bitmap's DPI</summary>
        /// <param name="bmp">The bitmap to render</param>
        /// <param name="point">Point structure that represents the location of the upper-left
        /// corner of the drawn bitmap</param>
        /// <param name="opacity">A value between 0.0f and 1.0f, inclusive, that specifies an opacity value
        /// to apply to the bitmap. This value is multiplied against the alpha values
        /// of the bitmap's contents.</param>
        public void DrawBitmap(D2dBitmap bmp, PointF point, float opacity = 1.0f)
        {
            SizeF bmpsize = bmp.PixelSize;
            var rect = new SharpDX.RectangleF(point.X, point.Y, point.X + bmpsize.Width, point.Y + bmpsize.Height);
            m_renderTarget.DrawBitmap(bmp.NativeBitmap, rect, opacity, BitmapInterpolationMode.Linear, null);
        }

        /// <summary>
        /// Draws the specified bitmap after scaling it to the size of the specifiedrectangle</summary>
        /// <param name="bmp">The bitmap to render</param>
        /// <param name="destRect">The size and position, in pixels, in the D2dGraphics's coordinate
        /// space, of the area in which the bitmap is drawn. If the rectangle is not well-ordered,
        /// nothing is drawn, but the D2dGraphics does not enter an error state.</param>
        /// <param name="opacity">A value between 0.0f and 1.0f, inclusive, that specifies an opacity value        
        /// to apply to the bitmap. This value is multiplied against the alpha values
        /// of the bitmap's contents.</param>
        /// <param name="interpolationMode">The interpolation mode to use if the bitmap is scaled or rotated 
        /// by the drawing operation</param>
        public void DrawBitmap(D2dBitmap bmp, RectangleF destRect, float opacity = 1.0f, D2dBitmapInterpolationMode interpolationMode = D2dBitmapInterpolationMode.Linear)
        {
            m_renderTarget.DrawBitmap(bmp.NativeBitmap, destRect.ToSharpDX(), opacity, (BitmapInterpolationMode)interpolationMode, null);
        }

        /// <summary>
        /// Draws the specified bitmap after scaling it to the size of the specified rectangle</summary>
        /// <param name="bmp">The bitmap to render</param>
        /// <param name="destRect">The size and position, in pixels, in the D2dGraphics's coordinate
        /// space, of the area in which the bitmap is drawn. If the rectangle is not well-ordered,
        /// nothing is drawn, but the D2dGraphics does not enter an error state.</param>
        /// <param name="opacity">A value between 0.0f and 1.0f, inclusive, that specifies an opacity value        
        /// to apply to the bitmap. This value is multiplied against the alpha values
        /// of the bitmap's contents.</param>
        /// <param name="interpolationMode">The interpolation mode to use if the bitmap is scaled or rotated 
        /// by the drawing operation</param>
        /// <param name="sourceRect">The size and position, in pixels in the bitmap's coordinate space, of the area
        /// within the bitmap to draw</param>
        public void DrawBitmap(D2dBitmap bmp, RectangleF destRect, float opacity,
            D2dBitmapInterpolationMode interpolationMode, RectangleF sourceRect)
        {
            m_renderTarget.DrawBitmap(bmp.NativeBitmap, destRect.ToSharpDX(), opacity,
                (BitmapInterpolationMode)interpolationMode, sourceRect.ToSharpDX());
        }

        /// <summary>
        /// Draws the outline of the specified ellipse</summary>
        /// <param name="rect">The rectangle, in pixels, that encloses the ellipse to paint</param>
        /// <param name="brush">The brush used to paint the ellipse's outline</param>
        /// <param name="strokeWidth">The thickness of the ellipse's stroke. The stroke is centered on the ellipse's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the ellipse's outline or null to draw a solid line</param>
        public void DrawEllipse(RectangleF rect, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            var tmpEllipse = new Ellipse();
            tmpEllipse.RadiusX = rect.Width * 0.5f;
            tmpEllipse.RadiusY = rect.Height * 0.5f;
            tmpEllipse.Point = new DrawingPointF(rect.X + tmpEllipse.RadiusX, rect.Y + tmpEllipse.RadiusY);
            m_renderTarget.DrawEllipse(tmpEllipse, brush.NativeBrush, strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }

        /// <summary>
        /// Draws the outline of the specified ellipse</summary>
        /// <param name="rect">The rectangle, in pixels, that encloses the ellipse to paint</param>
        /// <param name="color">The color used to paint the ellipse's outline</param>
        /// <param name="strokeWidth">The thickness of the ellipse's stroke. The stroke is centered on the ellipse's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the ellipse's outline or null to draw a solid line</param>
        public void DrawEllipse(RectangleF rect, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawEllipse(rect, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws the outline of the specified ellipse using the specified stroke style</summary>
        /// <param name="ellipse">Position and radius of the ellipse to draw in pixels</param>
        /// <param name="brush">The brush used to paint the ellipse's outline</param>
        /// <param name="strokeWidth">The thickness of the ellipse's stroke. The stroke is centered on the ellipse's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the ellipse's outline or null to draw a solid line</param>
        public void DrawEllipse(D2dEllipse ellipse, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_renderTarget.DrawEllipse(ellipse.ToSharpDX(), brush.NativeBrush, strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }

        /// <summary>
        /// Draws the outline of the specified ellipse</summary>
        /// <param name="ellipse">Position and radius of the ellipse to draw in pixels</param>
        /// <param name="color">The color used to paint the ellipse's outline</param>
        /// <param name="strokeWidth">The thickness of the ellipse's stroke. The stroke is centered on the ellipse's outline.</param>
        /// <param name="strokeStyle">The style of stroke to apply to the ellipse's outline or null to draw a solid line</param>
        public void DrawEllipse(D2dEllipse ellipse, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawEllipse(ellipse, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws a line between the specified points</summary>
        /// <param name="pt1X">The starting x-coordinate of the line, in pixels</param>
        /// <param name="pt1Y">The starting y-coordinate of the line, in pixels</param>
        /// <param name="pt2X">The ending x-coordinate of the line, in pixels</param>
        /// <param name="pt2Y">The ending y-coordinate of the line, in pixels</param>
        /// <param name="brush">The brush used to paint the line's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or null to paint a solid line</param>
        public void DrawLine(float pt1X, float pt1Y, float pt2X, float pt2Y, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_renderTarget.DrawLine(
                new DrawingPointF(pt1X, pt1Y),
                new DrawingPointF(pt2X, pt2Y),
                brush.NativeBrush,
                strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }

        /// <summary>
        /// Draws a line between the specified points</summary>
        /// <param name="pt1X">The starting x-coordinate of the line, in pixels</param>
        /// <param name="pt1Y">The starting y-coordinate of the line, in pixels</param>
        /// <param name="pt2X">The ending x-coordinate of the line, in pixels</param>
        /// <param name="pt2Y">The ending y-coordinate of the line, in pixels</param>
        /// <param name="color">The color used to paint the line's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or null to paint a solid line</param>
        public void DrawLine(float pt1X, float pt1Y, float pt2X, float pt2Y, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawLine(pt1X, pt1Y, pt2X, pt2Y, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws a line between the specified points</summary>
        /// <param name="pt1">The start point of the line, in pixels</param>
        /// <param name="pt2">The end point of the line, in pixels</param>
        /// <param name="brush">The brush used to paint the line's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or NULL to paint a solid line</param>
        public void DrawLine(PointF pt1, PointF pt2, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_renderTarget.DrawLine(pt1.ToSharpDX(), pt2.ToSharpDX(),
                brush.NativeBrush, strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }

        /// <summary>
        /// Draws a line between the specified points</summary>
        /// <param name="pt1">The start point of the line, in pixels</param>
        /// <param name="pt2">The end point of the line, in pixels</param>
        /// <param name="color">The color used to paint the line's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or NULL to paint a solid line</param>
        public void DrawLine(PointF pt1, PointF pt2, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawLine(pt1, pt2, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws a series of line segments that connect an array of System.Drawing.PointF</summary>
        /// <param name="points">Array of PointF that represent the points to connect</param>
        /// <param name="brush">The brush used to paint the line's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or NULL to paint a solid line</param>
        public void DrawLines(IEnumerable<PointF> points, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            var iter = points.GetEnumerator();
            if (!iter.MoveNext()) return;

            var transparent = brush.Opacity < 1.0f;
            if (!transparent)
            {
                var sbrush = brush as D2dSolidColorBrush;
                if (sbrush != null)
                    transparent = sbrush.Color.A < 255;
            }

            var nstroke = (strokeStyle ?? s_strokeStyle).NativeStrokeStyle;

            if (transparent)
            {
                using (var geom = new PathGeometry(D2dFactory.NativeFactory))
                {
                    var sink = geom.Open();
                    var pt1 = iter.Current;
                    sink.BeginFigure(pt1.ToSharpDX(), FigureBegin.Hollow);
                    while (iter.MoveNext())
                    {
                        sink.AddLine(iter.Current.ToSharpDX());
                    }
                    sink.EndFigure(FigureEnd.Open);
                    sink.Close();
                    sink.Dispose();

                    m_renderTarget.DrawGeometry(geom, brush.NativeBrush, strokeWidth, nstroke);
                }
            }
            else
            {
                var nbrush = brush.NativeBrush;
                var pt1 = iter.Current;
                while (iter.MoveNext())
                {
                    var pt2 = iter.Current;
                    m_renderTarget.DrawLine(pt1.ToSharpDX(), pt2.ToSharpDX(), nbrush,
                            strokeWidth, nstroke);
                    pt1 = pt2;
                }
            }
        }

        /// <summary>
        /// Draws a series of line segments that connect an array of System.Drawing.PointF</summary>
        /// <param name="points">Array of PointF that represent the points to connect</param>
        /// <param name="color">The color used to paint the line's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or NULL to paint a solid line</param>
        public void DrawLines(IEnumerable<PointF> points, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawLines(points, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws a polygon defined by an array of PointF structures</summary>
        /// <param name="points">Array of System.Drawing.PointF structures 
        /// that represent the vertices of the polygon</param>
        /// <param name="brush">The brush used to paint the polygon's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or NULL to paint a solid line</param>
        public void DrawPolygon(IEnumerable<PointF> points, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            var iter = points.GetEnumerator();
            if (!iter.MoveNext()) return;

            using (var geom = new PathGeometry(D2dFactory.NativeFactory))
            {
                var sink = geom.Open();
                var pt1 = iter.Current;
                sink.BeginFigure(pt1.ToSharpDX(), FigureBegin.Hollow);
                while (iter.MoveNext())
                {
                    sink.AddLine(iter.Current.ToSharpDX());
                }
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
                sink.Dispose();

                m_renderTarget.DrawGeometry(geom, brush.NativeBrush, strokeWidth,
                    strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
            }
        }

        /// <summary>
        /// Draws a polygon defined by an array of PointF structures</summary>
        /// <param name="points">Array of System.Drawing.PointF structures 
        /// that represent the vertices of the polygon</param>
        /// <param name="color">The color used to paint the line's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of stroke to paint, or NULL to paint a solid line</param>
        public void DrawPolygon(IEnumerable<PointF> points, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawPolygon(points, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws the outline of a rectangle that has the specified dimensions and stroke style</summary>
        /// <param name="rect">The dimensions of the rectangle to draw, in pixels</param>
        /// <param name="brush">The brush used to paint the rectangle's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width 
        /// of the rectangle's stroke. The stroke is centered on the rectangle's outline.</param>
        /// <param name="strokeStyle">The style of stroke to paint or null to draw a solid line</param>
        public void DrawRectangle(RectangleF rect, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_renderTarget.DrawRectangle(rect.ToSharpDX(), brush.NativeBrush, strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }

        /// <summary>
        /// Draws the outline of a rectangle that has the specified dimensions and stroke style</summary>
        /// <param name="rect">The dimensions of the rectangle to draw, in pixels</param>
        /// <param name="color">The color used to paint the rectangle's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width 
        /// of the rectangle's stroke. The stroke is centered on the rectangle's outline.</param>
        /// <param name="strokeStyle">The style of stroke to paint or null to draw a solid line</param>
        public void DrawRectangle(RectangleF rect, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawRectangle(rect, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        ///  Draws the outline of the specified rounded rectangle</summary>
        /// <param name="roundedRect">The dimensions of the rounded rectangle to draw, in pixels</param>
        /// <param name="brush">The brush used to paint the rounded rectangle's outline</param>
        /// <param name="strokeWidth">The width of the rounded rectangle's stroke. The stroke is centered on the
        /// rounded rectangle's outline.</param>
        /// <param name="strokeStyle">The style of the rounded rectangle's stroke, or null to paint a solid stroke</param>
        public void DrawRoundedRectangle(D2dRoundedRect roundedRect, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_renderTarget.DrawRoundedRectangle(roundedRect.ToSharpDX(), brush.NativeBrush, strokeWidth,
                strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }

        /// <summary>
        ///  Draws the outline of the specified rounded rectangle</summary>
        /// <param name="roundedRect">The dimensions of the rounded rectangle to draw, in pixels</param>
        /// <param name="color">The color used to paint the rounded rectangle's stroke</param>
        /// <param name="strokeWidth">The width of the rounded rectangle's stroke. The stroke is centered on the
        /// rounded rectangle's outline.</param>
        /// <param name="strokeStyle">The style of the rounded rectangle's stroke, or null to paint a solid stroke</param>
        public void DrawRoundedRectangle(D2dRoundedRect roundedRect, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawRoundedRectangle(roundedRect, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws the specified text using the format information provided at the specified location</summary>
        /// <param name="text">The text string to draw</param>
        /// <param name="textFormat">The text format object to use</param>
        /// <param name="upperLeft">Upper left corner of the text</param>
        /// <param name="brush">The brush to use to draw the text</param>
        public void DrawText(string text, D2dTextFormat textFormat, PointF upperLeft, D2dBrush brush)
        {
            var rtsize = Size;
            using (var layout
                = new SharpDX.DirectWrite.TextLayout(D2dFactory.NativeDwFactory,
                    text, textFormat.NativeTextFormat, rtsize.Width, rtsize.Height))
            {
                if (textFormat.Underlined)
                    layout.SetUnderline(true,
                                        new SharpDX.DirectWrite.TextRange(0, text.Length));
                if (textFormat.Strikeout)
                    layout.SetStrikethrough(true,
                                            new SharpDX.DirectWrite.TextRange(0, text.Length));
                m_renderTarget.DrawTextLayout(upperLeft.ToSharpDX(),
                    layout,
                    brush.NativeBrush,
                    (DrawTextOptions)textFormat.DrawTextOptions);
            }
        }

        /// <summary>
        /// Draws the specified text using the format information provided at the specified location</summary>
        /// <param name="text">The text string to draw</param>
        /// <param name="textFormat">The text format object to use</param>
        /// <param name="upperLeft">Upper left corner of the text</param>
        /// <param name="color">The color to use to draw the text</param>
        public void DrawText(string text, D2dTextFormat textFormat, PointF upperLeft, Color color)
        {
            m_solidColorBrush.Color = color;
            DrawText(text, textFormat, upperLeft, m_solidColorBrush);
        }

        /// <summary>
        /// Draws the specified text using the format information provided by an SharpDX.DirectWrite.TextFormat object</summary>
        /// <param name="text">A string to draw</param>
        /// <param name="textFormat">An object that describes formatting details of the text to draw, such as
        /// the font, the font size, and flow direction</param>
        /// <param name="layoutRect">The size and position of the area in which the text is drawn</param>
        /// <param name="brush">The brush used to paint the text</param>
        public void DrawText(string text, D2dTextFormat textFormat, RectangleF layoutRect, D2dBrush brush)
        {
            using (var layout
                = new SharpDX.DirectWrite.TextLayout(D2dFactory.NativeDwFactory
                    , text, textFormat.NativeTextFormat, layoutRect.Width, layoutRect.Height))
            {
                if (textFormat.Underlined)
                    layout.SetUnderline(true, new SharpDX.DirectWrite.TextRange(0, text.Length));
                if (textFormat.Strikeout)
                    layout.SetStrikethrough(true, new SharpDX.DirectWrite.TextRange(0, text.Length));
                m_renderTarget.DrawTextLayout(
                    layoutRect.Location.ToSharpDX(),
                    layout, brush.NativeBrush,
                    (DrawTextOptions)textFormat.DrawTextOptions);
            }
        }

        /// <summary>
        /// Draws the specified text using the format information provided by an SharpDX.DirectWrite.TextFormat object</summary>
        /// <param name="text">A string to draw</param>
        /// <param name="textFormat">An object that describes formatting details of the text to draw, such as
        /// the font, the font size, and flow direction</param>
        /// <param name="layoutRect">The size and position of the area in which the text is drawn</param>
        /// <param name="color">The color used to paint the text</param>
        public void DrawText(string text, D2dTextFormat textFormat, RectangleF layoutRect, Color color)
        {
            m_solidColorBrush.Color = color;
            DrawText(text, textFormat, layoutRect, m_solidColorBrush);
        }

        /// <summary>
        /// Draws the formatted text described by the specified D2dTextLayout</summary>
        /// <param name="origin">The point, described in pixels, at which the upper-left
        /// corner of the text described by textLayout is drawn</param>
        /// <param name="textLayout">The formatted text to draw</param>
        /// <param name="brush">The brush used to paint any text in textLayout 
        /// that does not already have a brush associated with it as a drawing effect</param>
        public void DrawTextLayout(PointF origin, D2dTextLayout textLayout, D2dBrush brush)
        {
            m_renderTarget.DrawTextLayout(
                origin.ToSharpDX(),
                textLayout.NativeTextLayout,
                brush.NativeBrush,
                (DrawTextOptions)textLayout.DrawTextOptions);
        }

        /// <summary>
        /// Draws the formatted text described by the specified D2dTextLayout</summary>
        /// <param name="origin">The point, described in pixels, at which the upper-left
        /// corner of the text described by textLayout is drawn</param>
        /// <param name="textLayout">The formatted text to draw</param>
        /// <param name="color">The color used to paint any text in textLayout 
        /// that does not already have a brush associated with it as a drawing effect</param>
        public void DrawTextLayout(PointF origin, D2dTextLayout textLayout, Color color)
        {
            m_solidColorBrush.Color = color;
            DrawTextLayout(origin, textLayout, m_solidColorBrush);
        }

        /// <summary>
        /// Measures the specified string when drawn with the specified System.Drawing.Font</summary>
        /// <param name="text">String to measure</param>
        /// <param name="format">D2dTextFormat that defines the font and format of the string</param>
        /// <returns>This method returns a System.Drawing.SizeF structure that represents the
        /// size, in DIP (Device Independent Pixels) units of the string specified by the text parameter as drawn 
        /// with the format parameter</returns>
        public SizeF MeasureText(string text, D2dTextFormat format)
        {
            return MeasureText(text, format, new SizeF(2024, 2024));
        }

        /// <summary>
        /// Measures the specified string when drawn with the specified System.Drawing.Font</summary>
        /// <param name="text">String to measure</param>
        /// <param name="format">D2dTextFormat that defines the font and format of the string</param>
        /// <param name="maxSize">The maximum x and y dimensions</param>
        /// <returns>This method returns a System.Drawing.SizeF structure that represents the
        /// size, in DIP (Device Independent Pixels) units of the string specified by the text parameter as drawn 
        /// with the format parameter</returns>
        public SizeF MeasureText(string text, D2dTextFormat format, SizeF maxSize)
        {
            using (var layout = new SharpDX.DirectWrite.TextLayout(D2dFactory.NativeDwFactory,
                    text, format.NativeTextFormat, maxSize.Width, maxSize.Height))
            {
                var metrics = layout.Metrics;
                return new SizeF(metrics.WidthIncludingTrailingWhitespace, metrics.Height);
            }
        }

        /// <summary>
        /// Paints the interior of the specified ellipse</summary>
        /// <param name="rect">The rectangle, in pixels, that encloses the ellipse to paint</param>
        /// <param name="brush">The brush used to paint the interior of the ellipse</param>
        public void FillEllipse(RectangleF rect, D2dBrush brush)
        {
            var tmpEllipse = new Ellipse 
            {
                RadiusX = rect.Width * 0.5f, 
                RadiusY = rect.Height * 0.5f
            };
            tmpEllipse.Point = new DrawingPointF(rect.X + tmpEllipse.RadiusX, rect.Y + tmpEllipse.RadiusY);
            m_renderTarget.FillEllipse(tmpEllipse, brush.NativeBrush);
        }

        /// <summary>
        /// Paints the interior of the specified ellipse</summary>
        /// <param name="rect">The rectangle, in pixels, that encloses the ellipse to paint</param>
        /// <param name="color">The color used to paint the interior of the ellipse</param>
        public void FillEllipse(RectangleF rect, Color color)
        {
            m_solidColorBrush.Color = color;
            FillEllipse(rect, m_solidColorBrush);
        }

        /// <summary>
        /// Paints the interior of the specified ellipse</summary>
        /// <param name="ellipse">The position and radius, in pixels, of the ellipse to paint</param>
        /// <param name="brush">The brush used to paint the interior of the ellipse</param>
        public void FillEllipse(D2dEllipse ellipse, D2dBrush brush)
        {
            m_renderTarget.FillEllipse(ellipse.ToSharpDX(), brush.NativeBrush);
        }

        /// <summary>
        /// Paints the interior of the specified ellipse</summary>
        /// <param name="ellipse">The position and radius, in pixels, of the ellipse to paint</param>
        /// <param name="color">The color used to paint the interior of the ellipse</param>
        public void FillEllipse(D2dEllipse ellipse, Color color)
        {
            m_solidColorBrush.Color = color;
            FillEllipse(ellipse, m_solidColorBrush);
        }

        /// <summary>
        /// Draws a solid rectangle with the specified brush while using the alpha channel of the given
        /// bitmap to control the opacity of each pixel.</summary>
        /// <param name="opacityMask">The opacity mask to apply to the brush. The alpha value of each pixel
        /// is multiplied with the alpha value of the brush after the brush has been mapped to the area
        /// defined by destinationRectangle.</param>
        /// <param name="brush">The brush used to paint</param>
        /// <param name="destRect">The region of the render target to paint, in device-independent pixels.</param>
        public void FillOpacityMask(D2dBitmap opacityMask, D2dBrush brush, RectangleF destRect)
        {
            m_renderTarget.FillOpacityMask(opacityMask.NativeBitmap, brush.NativeBrush,
                                           OpacityMaskContent.Graphics,
                                           destRect.ToSharpDX(), null);
        }

        /// <summary>
        /// Draws a solid rectangle with the specified brush while using the alpha channel of the given
        /// bitmap to control the opacity of each pixel.</summary>
        /// <param name="opacityMask">The opacity mask to apply to the brush. The alpha value of each pixel
        /// is multiplied with the alpha value of the brush after the brush has been mapped to the area
        /// defined by destinationRectangle.</param>
        /// <param name="color">The color used to paint</param>
        /// <param name="destRect">The region of the render target to paint, in device-independent pixels.</param>
        public void FillOpacityMask(D2dBitmap opacityMask, Color color, RectangleF destRect)
        {
            m_solidColorBrush.Color = color;
            FillOpacityMask(opacityMask, m_solidColorBrush, destRect);
        }

        /// <summary>
        /// Draws a solid rectangle with the specified brush while using the alpha channel of the given
        /// bitmap to control the opacity of each pixel.</summary>
        /// <param name="opacityMask">The opacity mask to apply to the brush. The alpha value of
        /// each pixel in the region specified by sourceRectangle is multiplied with the alpha value
        /// of the brush after the brush has been mapped to the area defined by destinationRectangle.</param>
        /// <param name="brush">The brush used to paint the region of the render target specified by destinationRectangle.</param>
        /// <param name="destRect">The region of the render target to paint, in device-independent pixels.</param>
        /// <param name="sourceRect">The region of the bitmap to use as the opacity mask, in device-independent pixels.</param>
        public void FillOpacityMask(D2dBitmap opacityMask, D2dBrush brush, RectangleF destRect, RectangleF sourceRect)
        {
            m_renderTarget.FillOpacityMask(opacityMask.NativeBitmap, brush.NativeBrush,
                                           OpacityMaskContent.Graphics,
                                           destRect.ToSharpDX(), sourceRect.ToSharpDX());
        }

        /// <summary>
        /// Draws a solid rectangle with the specified brush while using the alpha channel of the given
        /// bitmap to control the opacity of each pixel.</summary>
        /// <param name="opacityMask">The opacity mask to apply to the brush. The alpha value of
        /// each pixel in the region specified by sourceRectangle is multiplied with the alpha value
        /// of the brush after the brush has been mapped to the area defined by destinationRectangle.</param>
        /// <param name="color">The color used to paint the region of the render target specified by destinationRectangle.</param>
        /// <param name="destRect">The region of the render target to paint, in device-independent pixels.</param>
        /// <param name="sourceRect">The region of the bitmap to use as the opacity mask, in device-independent pixels.</param>
        public void FillOpacityMask(D2dBitmap opacityMask, Color color, RectangleF destRect, RectangleF sourceRect)
        {
            m_solidColorBrush.Color = color;
            FillOpacityMask(opacityMask, m_solidColorBrush, destRect, sourceRect);
        }

        /// <summary>
        /// Fills the interior of a polygon defined by given points</summary>
        /// <param name="points">Array of PointF structures that represent the vertices of
        /// the polygon to fill</param>
        /// <param name="brush">Brush that determines the characteristics of the fill</param>
        public void FillPolygon(IEnumerable<PointF> points, D2dBrush brush)
        {
            var iter = points.GetEnumerator();
            if (!iter.MoveNext()) return;

            using (var geom = new PathGeometry(D2dFactory.NativeFactory))
            {
                var sink = geom.Open();
                var pt1 = iter.Current;
                sink.BeginFigure(pt1.ToSharpDX(), FigureBegin.Filled);
                while (iter.MoveNext())
                {
                    sink.AddLine(iter.Current.ToSharpDX());
                }
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
                sink.Dispose();

                m_renderTarget.FillGeometry(geom, brush.NativeBrush);
            }
        }

        /// <summary>
        /// Fills the interior of a polygon defined by given points</summary>
        /// <param name="points">Array of PointF structures that represent the vertices of
        /// the polygon to fill</param>
        /// <param name="color">Color used for the fill</param>
        public void FillPolygon(IEnumerable<PointF> points, Color color)
        {
            m_solidColorBrush.Color = color;
            FillPolygon(points, m_solidColorBrush);
        }

        /// <summary>
        /// Paints the interior of the specified rectangle</summary>
        /// <param name="rect">Rectangle to paint, in pixels</param>
        /// <param name="brush">The brush used to paint the rectangle's interior</param>
        public void FillRectangle(RectangleF rect, D2dBrush brush)
        {
            m_renderTarget.FillRectangle(rect.ToSharpDX(), brush.NativeBrush);
        }

        /// <summary>
        /// Paints the interior of the specified rectangle using a single color</summary>
        /// <param name="rect">Rectangle to paint, in pixels</param>
        /// <param name="color">Color used to paint the rectangle</param>
        public void FillRectangle(RectangleF rect, Color color)
        {
            m_solidColorBrush.Color = color;
            FillRectangle(rect, m_solidColorBrush);
        }

        /// <summary>
        /// Paints the interior of the specified rectangle with a smooth color gradient</summary>
        /// <param name="rect">Rectangle to paint, in pixels</param>
        /// <param name="pt1">The point, in pixels, that color1 gets mapped to</param>
        /// <param name="pt2">The point, in pixels, that color2 gets mapped to</param>
        /// <param name="color1">The color to use at the first point</param>
        /// <param name="color2">The color to use at the second point</param>
        /// <remarks>Note that each color combination is used to create a brush that
        /// is cached, so you cannot use an unlimited number of color combinations.</remarks>
        public void FillRectangle(RectangleF rect, PointF pt1, PointF pt2, Color color1, Color color2)
        {
            var brush = GetCachedLinearGradientBrush(color1, color2);
            brush.StartPoint = pt1.ToSharpDX();
            brush.EndPoint = pt2.ToSharpDX();
            m_renderTarget.FillRectangle(rect.ToSharpDX(), brush);
        }

        /// <summary>
        /// Paints the interior of the specified rounded rectangle</summary>
        /// <param name="roundedRect">The dimensions of the rounded rectangle to paint, in pixels</param>
        /// <param name="brush">The brush used to paint the interior of the rounded rectangle</param>
        public void FillRoundedRectangle(D2dRoundedRect roundedRect, D2dBrush brush)
        {
            var tmp = roundedRect.ToSharpDX();
            m_renderTarget.FillRoundedRectangle(ref tmp, brush.NativeBrush);
        }

        /// <summary>
        /// Paints the interior of the specified rounded rectangle</summary>
        /// <param name="roundedRect">The dimensions of the rounded rectangle to paint, in pixels</param>
        /// <param name="color">The color used to paint the interior of the rounded rectangle</param>
        public void FillRoundedRectangle(D2dRoundedRect roundedRect, Color color)
        {
            m_solidColorBrush.Color = color;
            FillRoundedRectangle(roundedRect, m_solidColorBrush);
        }

        /// <summary>
        /// Draws a path defined by an enumeration of EdgeStyleData structures</summary>
        /// <param name="path">The enumeration of drawing primitives that describes the contents of the path</param>
        /// <param name="brush">The brush used to paint the path's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <remarks>Assume the end point of one primitive be coincident with the start point of the following primitive in a path</remarks>
        /// <param name="strokeStyle">The style of the rounded rectangle's stroke, or null to paint a solid stroke</param>
        public void DrawPath(IEnumerable<EdgeStyleData> path, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            using (var geom = new PathGeometry(D2dFactory.NativeFactory))
            {
                var sink = geom.Open();
                var firstPoint = true;
                foreach (var edge in path)
                {
                    if (edge.ShapeType == EdgeStyleData.EdgeShape.Line)
                    {
                        var line = edge.EdgeData.As<PointF[]>();
                        if (firstPoint)
                        {
                            sink.BeginFigure(line[0].ToSharpDX(), FigureBegin.Hollow);
                            firstPoint = false;
                        }
                        for (var i = 1; i < line.Length; ++i)
                            sink.AddLine(line[i].ToSharpDX());

                    }
                    else if (edge.ShapeType == EdgeStyleData.EdgeShape.Bezier)
                    {
                        var curve = edge.EdgeData.As<BezierCurve2F>();
                        if (firstPoint)
                        {
                            sink.BeginFigure(new DrawingPointF(curve.P1.X, curve.P1.Y), FigureBegin.Hollow);
                            firstPoint = false;
                        }
                        var seg = new BezierSegment
                        {
                            Point1 = new DrawingPointF(curve.P2.X, curve.P2.Y),
                            Point2 = new DrawingPointF(curve.P3.X, curve.P3.Y),
                            Point3 = new DrawingPointF(curve.P4.X, curve.P4.Y)
                        };
                        sink.AddBezier(seg);
                    }
                }
                sink.EndFigure(FigureEnd.Open);
                sink.Close();
                sink.Dispose();

                m_renderTarget.DrawGeometry(geom, brush.NativeBrush, strokeWidth,
                    strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
            }
        }

        /// <summary>
        /// Draws a path defined by an enumeration of EdgeStyleData structures</summary>
        /// <param name="path">The enumeration of drawing primitives that describes the contents of the path</param>
        /// <param name="color">The color used to paint the path's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <remarks>Assume the end point of one primitive be coincident with the start point of the following primitive in a path</remarks>
        /// <param name="strokeStyle">The style of the rounded rectangle's stroke, or null to paint a solid stroke</param>
        public void DrawPath(IEnumerable<EdgeStyleData> path, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawPath(path, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Fills the interior of a path defined by an enumeration of EdgeStyleData structures</summary>
        /// <param name="path">The enumeration of drawing primitives that describes the contents of the path</param>
        /// <param name="brush">The brush used to paint the path's stroke</param>
        /// <remarks>Assume the end point of one primitive be coincident with the start point of the following primitive in a path</remarks>
        public void FillPath(IEnumerable<EdgeStyleData> path, D2dBrush brush)
        {
            using (var geom = new PathGeometry(D2dFactory.NativeFactory))
            {
                var sink = geom.Open();
                var firstPoint = true;
                foreach (var edge in path)
                {
                    if (edge.ShapeType == EdgeStyleData.EdgeShape.Line)
                    {
                        var line = edge.EdgeData.As<PointF[]>();
                        if (firstPoint)
                        {
                            sink.BeginFigure(line[0].ToSharpDX(), FigureBegin.Filled);
                            firstPoint = false;
                        }
                        for (var i = 1; i < line.Length; ++i)
                            sink.AddLine(line[i].ToSharpDX());

                    }
                    else if (edge.ShapeType == EdgeStyleData.EdgeShape.Bezier)
                    {
                        var curve = edge.EdgeData.As<BezierCurve2F>();
                        if (firstPoint)
                        {
                            sink.BeginFigure(new DrawingPointF(curve.P1.X, curve.P1.Y), FigureBegin.Hollow);
                            firstPoint = false;
                        }
                        var seg = new BezierSegment
                        {
                            Point1 = new DrawingPointF(curve.P2.X, curve.P2.Y),
                            Point2 = new DrawingPointF(curve.P3.X, curve.P3.Y),
                            Point3 = new DrawingPointF(curve.P4.X, curve.P4.Y)
                        };
                        sink.AddBezier(seg);
                    }
                }
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();
                sink.Dispose();

                m_renderTarget.FillGeometry(geom, brush.NativeBrush);
            }
        }

        /// <summary>
        /// Fills the interior of a path defined by an enumeration of EdgeStyleData structures</summary>
        /// <param name="path">The enumeration of drawing primitives that describes the contents of the path</param>
        /// <param name="color">The color used to paint the path's stroke</param>
        /// <remarks>Assume the end point of one primitive be coincident with the start point of the following primitive in a path</remarks>
        public void FillPath(IEnumerable<EdgeStyleData> path, Color color)
        {
            m_solidColorBrush.Color = color;
            FillPath(path, m_solidColorBrush);
        }

        /// <summary>
        /// Draws a geometry previosly defined</summary>
        /// <param name="geometry">The geometry to draw</param>
        /// <param name="brush">The brush used to paint the geometry's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of the geometry's stroke, or null to paint a solid stroke</param>
        public void DrawGeometry(D2dGeometry geometry, D2dBrush brush, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_renderTarget.DrawGeometry(geometry.NativeGeometry, brush.NativeBrush, strokeWidth,
                    strokeStyle != null ? strokeStyle.NativeStrokeStyle : null);
        }

        /// <summary>
        /// Draws a geometry previosly defined</summary>
        /// <param name="geometry">The geometry to draw</param>
        /// <param name="color">The color used to paint the geometry's stroke</param>
        /// <param name="strokeWidth">A value greater than or equal to 0.0f that specifies the width of the stroke</param>
        /// <param name="strokeStyle">The style of the geometry's stroke, or null to paint a solid stroke</param>
        public void DrawGeometry(D2dGeometry geometry, Color color, float strokeWidth = 1.0f, D2dStrokeStyle strokeStyle = null)
        {
            m_solidColorBrush.Color = color;
            DrawGeometry(geometry, m_solidColorBrush, strokeWidth, strokeStyle);
        }

        /// <summary>
        /// Draws a geometry previosly defined</summary>
        /// <param name="geometry">The geometry to draw</param>
        /// <param name="brush">The brush used to fill.</param>
        public void FillGeometry(D2dGeometry geometry, D2dBrush brush)
        {
            m_renderTarget.FillGeometry(geometry.NativeGeometry, brush.NativeBrush);
        }

        /// <summary>
        /// Draws a geometry previosly defined</summary>
        /// <param name="geometry">The geometry to draw</param>
        /// <param name="color">The color used to fill.</param>
        public void FillGeometry(D2dGeometry geometry, Color color)
        {
            m_solidColorBrush.Color = color;
            FillGeometry(geometry, m_solidColorBrush);
        }

        /// <summary>    
        /// Specifies a rectangle to which all subsequent drawing operations are clipped.
        /// Use current antialiasing mode to draw the edges of clip rects.</summary>    
        /// <remarks>The clipRect is transformed by the current world transform set on the render target.
        /// After the transform is applied to the clipRect that is passed in, the axis-aligned 
        /// bounding box for the clipRect is computed. For efficiency, the contents are clipped 
        /// to this axis-aligned bounding box and not to the original clipRect that is passed in.</remarks>    
        /// <param name="clipRect">The size and position of the clipping area, in pixels</param>
        public void PushAxisAlignedClip(RectangleF clipRect)
        {
            PushAxisAlignedClip(clipRect, AntialiasMode);
        }

        /// <summary>    
        /// Specifies a rectangle to which all subsequent drawing operations are clipped</summary>
        /// <remarks>The clipRect is transformed by the current world transform set on the render target.
        /// After the transform is applied to the clipRect that is passed in, the axis-aligned 
        /// bounding box for the clipRect is computed. For efficiency, the contents are clipped 
        /// to this axis-aligned bounding box and not to the original clipRect that is passed in.</remarks>    
        /// <param name="clipRect">The size and position of the clipping area, in pixels</param>
        /// <param name="antialiasMode">The antialiasing mode that is used to draw the edges of clip rects that have subpixel 
        /// boundaries, and to blend the clip with the scene contents. The blending is performed 
        /// once when the PopAxisAlignedClip method is called, and does not apply to each 
        /// primitive within the layer.</param>        
        public void PushAxisAlignedClip(RectangleF clipRect, D2dAntialiasMode antialiasMode)
        {
            m_clipStack.Push(clipRect);
            var rect = new SharpDX.RectangleF(clipRect.Left, clipRect.Top, clipRect.Right, clipRect.Bottom);
            m_renderTarget.PushAxisAlignedClip(rect, (AntialiasMode)antialiasMode);
        }

        /// <summary>    
        /// Removes the last axis-aligned clip from the render target. 
        /// After this method is called, the clip is no longer applied to subsequent 
        /// drawing operations.</summary>        
        public void PopAxisAlignedClip()
        {
            m_clipStack.Pop();
            m_renderTarget.PopAxisAlignedClip();
        }

        /// <summary>
        /// Creates a new D2dSolidColorBrush that has the specified color.
        /// It is recommended to create D2dSolidBrushes and reuse them instead
        /// of creating and disposing of them for each method call.</summary>
        /// <param name="color">The red, green, blue, and alpha values of the brush's color</param>
        /// <returns>A new D2d D2dSolidColorBrush</returns>
        public D2dSolidColorBrush CreateSolidBrush(Color color)
        {
            return new D2dSolidColorBrush(this, color);
        }

        /// <summary>
        /// Creates a D2dLinearGradientBrush with the specified points and colors</summary>        
        /// <param name="gradientStops">An array of D2dGradientStop structures that describe the colors in the brush's gradient 
        /// and their locations along the gradient line</param>
        /// <returns>A new instance of D2dLinearGradientBrush</returns>
        public D2dLinearGradientBrush CreateLinearGradientBrush(params D2dGradientStop[] gradientStops)
        {
            return CreateLinearGradientBrush(
                new PointF(),
                new PointF(),
                gradientStops,
                D2dExtendMode.Clamp,
                D2dGamma.StandardRgb);
        }

        /// <summary>
        /// Creates a D2dLinearGradientBrush that contains the specified gradient stops,
        /// extend mode, and gamma interpolation. has no transform, and has a base opacity of 1.0.</summary>
        /// <param name="pt1">A System.Drawing.PointF structure that represents the starting point of the
        /// linear gradient</param>
        /// <param name="pt2">A System.Drawing.PointF structure that represents the endpoint of the linear gradient</param>
        /// <param name="gradientStops">An array of D2dGradientStop structures that describe the colors in the brush's gradient 
        /// and their locations along the gradient line</param>
        /// <param name="extendMode">The behavior of the gradient outside the [0,1] normalized range</param>
        /// <param name="gamma">The space in which color interpolation between the gradient stops is performed</param>
        /// <returns>A new instance of D2dLinearGradientBrush</returns>
        public D2dLinearGradientBrush CreateLinearGradientBrush(
            PointF pt1,
            PointF pt2,
            D2dGradientStop[] gradientStops,
            D2dExtendMode extendMode,
            D2dGamma gamma)
        {
            return new D2dLinearGradientBrush(this, pt1, pt2, gradientStops, extendMode, gamma);
        }

        /// <summary>
        /// Creates a D2dRadialGradientBrush that contains the specified gradient stops</summary>
        /// <param name="gradientStops">An array of D2dGradientStop structures that describe the
        /// colors in the brush's gradient and their locations along the gradient</param>
        /// <returns>A new instance of D2dRadialGradientBrush</returns>
        public D2dRadialGradientBrush CreateRadialGradientBrush(params D2dGradientStop[] gradientStops)
        {
            return CreateRadialGradientBrush(
                new PointF(0, 0),
                new PointF(0, 0),
                1.0f,
                1.0f,
                gradientStops);
        }

        /// <summary>
        /// Creates a D2dRadialGradientBrush that uses the specified arguments</summary>
        /// <param name="center">In the brush's coordinate space, the center of the gradient ellipse</param>
        /// <param name="gradientOriginOffset">In the brush's coordinate space, the offset of the gradient origin relative
        /// to the gradient ellipse's center</param>
        /// <param name="radiusX">In the brush's coordinate space, the x-radius of the gradient ellipse</param>
        /// <param name="radiusY">In the brush's coordinate space, the y-radius of the gradient ellipse</param>
        /// <param name="gradientStops">An array of D2dGradientStop structures that describe the
        /// colors in the brush's gradient and their locations along the gradient</param>
        /// <returns>A new instance of D2dRadialGradientBrush</returns>
        public D2dRadialGradientBrush CreateRadialGradientBrush(
            PointF center,
            PointF gradientOriginOffset,
            float radiusX,
            float radiusY,
            params D2dGradientStop[] gradientStops)
        {
            return new D2dRadialGradientBrush(this, center, gradientOriginOffset, radiusX, radiusY, gradientStops);
        }

        /// <summary>
        /// Creates a new D2dBitmapBrush from the specified bitmap</summary>
        /// <param name="bitmap">The bitmap contents of the new brush</param>
        /// <returns>A new instance of D2dBitmapBrush</returns>
        public D2dBitmapBrush CreateBitmapBrush(D2dBitmap bitmap)
        {
            return new D2dBitmapBrush(this, bitmap);
        }

        /// <summary>
        /// Creates a new instance of the D2dBitmap class from a specified resource</summary>
        /// <param name="type">The type used to extract the resource</param>
        /// <param name="resource">The name of the resource</param>
        /// <returns>A new D2dBitmap</returns>
        public D2dBitmap CreateBitmap(Type type, string resource)
        {
            using (var bmp = new System.Drawing.Bitmap(type, resource))
            {
                var d2dbmp = CreateBitmap(bmp);
                return d2dbmp;
            }
        }

        /// <summary>
        /// Creates a new instance of the D2dBitmap from the specified stream</summary>
        /// <param name="stream">The data stream used to load the image</param>
        /// <returns>A new D2dBitmap</returns>
        /// <remarks>It is safe to close the stream after the function call.</remarks>
        public D2dBitmap CreateBitmap(System.IO.Stream stream)
        {
            using (var bmp = new System.Drawing.Bitmap(stream))
            {
                var d2dbmp = CreateBitmap(bmp);
                return d2dbmp;
            }
        }

        /// <summary>
        /// Creates a new instance of the D2dBitmap from the specified file</summary>
        /// <param name="filename">The name of the bitmap file</param>
        /// <returns>A new D2dBitmap</returns>
        public D2dBitmap CreateBitmap(string filename)
        {
            using (var bmp = new System.Drawing.Bitmap(filename))
            {
                var d2dbmp = CreateBitmap(bmp);
                return d2dbmp;
            }
        }


        /// <summary>
        /// Creates a Direct2D Bitmap by copying the specified GDI image</summary>
        /// <param name="img">A GDI bitmap from which to create new D2dBitmap</param>
        /// <returns>A new D2dBitmap</returns>
        public D2dBitmap CreateBitmap(System.Drawing.Image img)
        {
            if (img == null)
                throw new ArgumentNullException("img");
            using (var bmp = new System.Drawing.Bitmap(img))
            {
                return CreateBitmap(bmp);
            }
        }


        /// <summary>
        /// Creates a Direct2D Bitmap by copying the specified GDI bitmap</summary>
        /// <param name="bmp">A GDI bitmap from which to create new D2dBitmap</param>
        /// <returns>A new D2dBitmap</returns>
        public D2dBitmap CreateBitmap(System.Drawing.Bitmap bmp)
        {
            if (bmp == null)
                throw new ArgumentNullException("bmp");
            var copy = bmp.Clone(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), GdiPixelFormat.Format32bppPArgb);
            return new D2dBitmap(this, copy);
        }



        /// <summary>
        /// Creates an empty Direct2D Bitmap from specified width and height
        /// Pixel format is set to 32 bit ARGB with premultiplied alpha</summary>      
        /// <param name="width">Width of the bitmap in pixels</param>
        /// <param name="height">Height of the bitmap in pixels</param>
        /// <returns>A new D2dBitmap</returns>
        public D2dBitmap CreateBitmap(int width, int height)
        {
            if (width < 1 || height < 1)
                throw new ArgumentOutOfRangeException("Width and height must be greater than zero");

            var bmp = new System.Drawing.Bitmap(width, height, GdiPixelFormat.Format32bppPArgb);
            return new D2dBitmap(this, bmp);
        }

        /// <summary>
        /// Creates a new D2dBitmapGraphics for use during intermediate offscreen drawing 
        /// that is compatible with the current D2dGraphics and has the same size, DPI, and pixel format
        /// as the current D2dGraphics</summary>        
        public D2dBitmapGraphics CreateCompatibleGraphics()
        {
            var rt = new BitmapRenderTarget(m_renderTarget, CompatibleRenderTargetOptions.None);
            return new D2dBitmapGraphics(rt);
        }

        /// <summary>
        /// Creates a new D2dBitmapGraphics for use during intermediate offscreen drawing 
        /// that is compatible with the current D2dGraphics and has the same size, DPI, and pixel format
        /// as the current D2dGraphics and with the specified options</summary>        
        /// <param name="options">Whether the new D2dBitmapGraphics must be compatible with GDI</param>        
        public D2dBitmapGraphics CreateCompatibleGraphics(D2dCompatibleGraphicsOptions options)
        {
            var rt = new BitmapRenderTarget(m_renderTarget, (CompatibleRenderTargetOptions)options);
            return new D2dBitmapGraphics(rt);

        }

        /// <summary>
        /// Creates a new D2dBitmapGraphics with specified pixel size for use during intermediate offscreen drawing
        /// that is compatible with the current D2dGraphics and has the same DPI, and pixel format
        /// as the current D2dGraphics and with the specified options</summary>        
        /// <param name="pixelSize">The desired size of the new D2dGraphics in pixels</param>
        /// <param name="options">Whether the new D2dBitmapGraphics must be compatible with GDI</param>        
        public D2dBitmapGraphics CreateCompatibleGraphics(Size pixelSize, D2dCompatibleGraphicsOptions options)
        {
            var dsize = new DrawingSize(pixelSize.Width, pixelSize.Height);
            var rt = new BitmapRenderTarget(m_renderTarget, (CompatibleRenderTargetOptions)options, null, dsize, null);
            return new D2dBitmapGraphics(rt);
        }

        /// <summary>
        /// Creates a new D2dBitmapGraphics with specified DIP size for use during intermediate offscreen drawing
        /// that is compatible with the current D2dGraphics and has the same DPI, and pixel format
        /// as the current D2dGraphics and with the specified options</summary>        
        /// <param name="size">The desired size of the new D2dGraphics in pixels</param>
        /// <param name="options">Whether the new D2dBitmapGraphics must be compatible with GDI</param>        
        public D2dBitmapGraphics CreateCompatibleGraphics(SizeF size, D2dCompatibleGraphicsOptions options)
        {
            var rt = new BitmapRenderTarget(m_renderTarget, (CompatibleRenderTargetOptions)options, size.ToSharpDX(), null, null);
            return new D2dBitmapGraphics(rt);
        }

        /// <summary>
        /// Begin GDI+ secion.
        /// You can use a GDI device returned from Graphics property.
        /// Call EndGDISection when you done drawing with GDI.</summary>
        /// <remarks>
        /// This GDI interop is for code migration and/or missing features.
        /// It has significant performance cost.</remarks>
        public void BeginGdiSection()
        {
            if (m_graphics != null)
                throw new InvalidOperationException("EndGDISection() call required");

            m_graphics = Graphics.FromHdc(m_gdiInterop.GetDC(DeviceContextInitializeMode.Copy));
            m_graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            ApplyXformToGdi();
        }

        /// <summary>
        /// Gets GDI+ graphics. This property is only valid when used
        /// between BeginGDISection() and EndGDISection().</summary>
        /// <remarks>
        /// This GDI interop is for code migration and/or missing features.
        /// It has significant performance cost.</remarks>
        public Graphics Graphics
        {
            get
            {
                if (m_graphics == null)
                    throw new InvalidOperationException("Graphics only valid when called between BeginGDISection() and EndGDISection()");
                return m_graphics;
            }
        }

        /// <summary>
        /// Ends GDI section</summary>
        /// <remarks>Call EndGdiSection after calling BeginGdiSection.</remarks>
        public void EndGdiSection()
        {
            if (m_graphics == null)
                throw new InvalidOperationException("BeginGDISection() call required");

            m_graphics.Dispose();
            m_graphics = null;
            m_gdiInterop.ReleaseDC();
        }

        /// <summary>
        /// Gets the current clipping rectangle, in pixels. Is analagous to
        /// System.Drawing.Graphics' ClipBounds property. Works with PushAxisAlignedClip and
        /// PopAxisAlignedClip. If no clip rectangle has been pushed, this property gets the
        /// size of the D2D surface.</summary>
        public RectangleF ClipBounds
        {
            get
            {
                if (m_clipStack.Count > 0)
                    return m_clipStack.Peek();

                return new RectangleF(new PointF(), Size);
            }
        }

        /// <summary>
        /// Event that is raised after render target is recreated.
        /// All the resources are automatically recreated except resources 
        /// of type D2dBitmapGraphics. User must recreate all the 
        /// resources of type D2dBitmapGraphics that are been created 
        /// from this D2dGraphics</summary>
        public event EventHandler RecreateResources;

        /// <summary>
        /// Gets the serial number for the underlining render target.
        /// Each time the render target is recreated the number will be incremented.
        /// Cient code can compare this number with previous one to determine if the render target has 
        /// been recreated since last time a call made to EndDraw().</summary>
        public uint RenderTargetNumber
        {
            get { return m_renderTargetNumber; }
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="renderTarget">Render target</param>
        protected D2dGraphics(RenderTarget renderTarget)
        {
            SetRenderTarget(renderTarget);
        }

        /// <summary>
        /// Sets the render target, releasing resources tied to previous render target if necessary.</summary>
        /// <param name="renderTarget"></param>
        protected void SetRenderTarget(RenderTarget renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");

            ReleaseResources(true);
            m_renderTargetNumber++;
            m_renderTarget = renderTarget;
            m_gdiInterop = m_renderTarget.QueryInterface<GdiInteropRenderTarget>();
            Transform = Matrix3x2F.Identity;

            if (s_strokeStyle == null)
            {
                var props = new D2dStrokeStyleProperties { EndCap = D2dCapStyle.Round, StartCap = D2dCapStyle.Round };
                s_strokeStyle = D2dFactory.CreateD2dStrokeStyle(props);
            }
            m_solidColorBrush = CreateSolidBrush(Color.Empty);

        }

        /// <summary>
        /// Recreates the render target, if necessary, by calling SetRenderTarget.</summary>
        protected abstract void RecreateRenderTarget();

        /// <summary>
        /// Dispose of unmanaged resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;
            ReleaseResources(disposing);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets render target</summary>
        protected internal RenderTarget D2dRenderTarget
        {
            get { return m_renderTarget; }
        }

        private void ReleaseResources(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources (objects that have finalizers).
                if (m_solidColorBrush != null)
                {
                    m_solidColorBrush.Dispose();
                    m_solidColorBrush = null;
                }
            }

            if (m_gdiInterop != null)
            {
                m_gdiInterop.Dispose();
                m_gdiInterop = null;
            }

            // Dispose of unmanaged resources.
            // Render target and LinearGradientBrush do not implement a finalizer, so it is considered an unmanaged resource.            
            if (m_renderTarget != null)
            {
                m_renderTarget.Dispose();
                m_renderTarget = null;

            }
            foreach (var brush in m_linearGradients.Values)
                brush.Dispose();
            m_linearGradients.Clear();
        }

        /// <summary>
        /// Applies same transform to GDI</summary>
        private void ApplyXformToGdi()
        {
            using (var xform = new System.Drawing.Drawing2D.Matrix(m_xform.M11, m_xform.M12, m_xform.M21, m_xform.M22
                    , m_xform.DX, m_xform.DY))
            {
                m_graphics.Transform = xform;
            }
        }

        private void RecreateTargetAndResources()
        {
            RecreateRenderTarget();
            RecreateResources.Raise(this, EventArgs.Empty);
        }

        private LinearGradientBrush GetCachedLinearGradientBrush(Color color1, Color color2)
        {
            long kh = color1.ToArgb(); //  high bits
            long kl = color2.ToArgb(); //  low bits
            var colorKey = (kh << 32) | kl;

            LinearGradientBrush brush;
            if (!m_linearGradients.TryGetValue(colorKey, out brush))
            {
                var stops = new GradientStop[2];
                stops[0].Color = color1.ToColor4();
                stops[0].Position = 0;
                stops[1].Color = color2.ToColor4();
                stops[1].Position = 1;
                using (var stopCollection = new GradientStopCollection(m_renderTarget, stops, Gamma.StandardRgb, ExtendMode.Clamp))
                {
                    var props = new LinearGradientBrushProperties();
                    brush = new LinearGradientBrush(m_renderTarget, props, stopCollection);
                }
                m_linearGradients.Add(colorKey, brush);
            }

            return brush;
        }

        private uint m_renderTargetNumber;
        private static readonly Result D2DERR_WRONG_RESOURCE_DOMAIN = new Result(0x88990015);
        private static readonly Result D2DERR_RECREATE_TARGET = new Result(0x8899000C);
        private const double DPI = 3.1415926535897931;
        private const float ToRadian = (float)(DPI / 180.0);
        // temp list of points used by DrawArc.
        private readonly List<PointF> m_tempPoints = new List<PointF>(100);

        private PointF m_scale = new PointF(1, 1);
        private const float Tessellation = 4; // in pixel.
        private GdiInteropRenderTarget m_gdiInterop;
        private Graphics m_graphics;
        private RenderTarget m_renderTarget;
        private Matrix3x2F m_xform = Matrix3x2F.Identity;
        private D2dSolidColorBrush m_solidColorBrush;
        private readonly Stack<RectangleF> m_clipStack = new Stack<RectangleF>();
        private readonly Dictionary<long, LinearGradientBrush> m_linearGradients = new Dictionary<long, LinearGradientBrush>();

        // used for drawing connected lines.
        private static D2dStrokeStyle s_strokeStyle;
    }

    /// <summary>
    /// Specifies how the edges of nontext primitives are rendered</summary>
    public enum D2dAntialiasMode
    {
        /// <summary>
        /// Edges are antialiased using the Direct2D per-primitive method of high-quality
        /// antialiasing</summary>
        PerPrimitive = 0,

        /// <summary>
        /// Objects are aliased in most cases. Objects are antialiased only when they
        /// are drawn to a D2dGraphics created by the {{CreateDxgiSurfaceRenderTarget}}
        /// method and Direct3D multisampling has been enabled on the backing DirectX
        /// Graphics Infrastructure (DXGI) surface.</summary>
        Aliased = 1,
    }

    /// <summary>
    /// Describes the antialiasing mode used for drawing text</summary>
    public enum D2dTextAntialiasMode
    {
        /// <summary>
        /// Use the system default</summary>
        Default = 0,

        /// <summary>
        /// Use ClearType antialiasing</summary>
        Cleartype = 1,

        /// <summary>
        /// Use grayscale antialiasing</summary>
        Grayscale = 2,

        /// <summary>
        /// Do not use antialiasing</summary>
        Aliased = 3,
    }

}
