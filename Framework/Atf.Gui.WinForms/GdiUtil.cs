//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.VectorMath;

namespace Sce.Atf
{
    /// <summary>
    /// GDI+ utilities to perform drawing and related operations</summary>
    public static class GdiUtil
    {
        /// <summary>
        /// Gets the dots-per-inch (DPI) factor, which is the ratio of actual DPI to
        /// the default 96 dpi of Windows</summary>
        public static float DpiFactor
        {
            get
            {
                if (s_dpiYFactor == 0.0f)
                {
                    using (var frm = new Form())
                    {
                        using (Graphics gr = frm.CreateGraphics())
                        {
                            s_dpiYFactor = gr.DpiY / 96.0f;
                        }
                    }
                }
                return s_dpiYFactor;
            }
        }

        /// <summary>
        /// Creates a GDI transform representing a uniform scale and translation</summary>
        /// <param name="translation">Translation</param>
        /// <param name="scale">Scale</param>
        /// <returns>GDI transform representing a uniform scale and translation</returns>
        public static Matrix GetTransform(Point translation, float scale)
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
        public static Matrix GetTransform(Point translation, float xScale, float yScale)
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
            matrix.TransformPoints(s_tempPts);
            return s_tempPts[0];
        }

        /// <summary>
        /// Transforms point</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="p">Point</param>
        /// <returns>Transformed point</returns>
        public static PointF Transform(Matrix matrix, PointF p)
        {
            s_tempPtF[0] = p;
            matrix.TransformPoints(s_tempPtF);
            return s_tempPtF[0];
        }

        /// <summary>
        /// Transforms vector</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="v">Vector</param>
        /// <returns>Transformed vector</returns>
        public static PointF TransformVector(Matrix matrix, PointF v)
        {
            s_tempPtF[0] = v;
            matrix.TransformVectors(s_tempPtF);
            return s_tempPtF[0];
        }

        /// <summary>
        /// Transforms x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, from system A to system B</param>
        /// <param name="x">X-coordinate in coordinate system A</param>
        /// <returns>X-coordinate in coordinate system B</returns>
        public static float Transform(Matrix matrix, float x)
        {
            s_tempPtF[0].X = x;
            s_tempPtF[0].Y = 0.0f;
            matrix.TransformPoints(s_tempPtF);
            return s_tempPtF[0].X;
        }

        /// <summary>
        /// Transforms vector's x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, from system A to system B</param>
        /// <param name="x">X-coordinate of vector in coordinate system A</param>
        /// <returns>X-coordinate of vector in coordinate system B</returns>
        public static float TransformVector(Matrix matrix, float x)
        {
            s_tempPtF[0].X = x;
            s_tempPtF[0].Y = 0.0f;
            matrix.TransformVectors(s_tempPtF);
            return s_tempPtF[0].X;
        }

        /// <summary>
        /// Transforms point by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="p">Point</param>
        /// <returns>Inverse transformed point</returns>
        public static Point InverseTransform(Matrix matrix, Point p)
        {
            using (Matrix inverse = matrix.Clone())
            {
                inverse.Invert();
                s_tempPts[0] = p;
                inverse.TransformPoints(s_tempPts);
            }
            return s_tempPts[0];
        }

        /// <summary>
        /// Transforms point by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="p">Point</param>
        /// <returns>Inverse transformed point</returns>
        public static PointF InverseTransform(Matrix matrix, PointF p)
        {
            using (Matrix inverse = matrix.Clone())
            {
                inverse.Invert();
                s_tempPtF[0] = p;
                inverse.TransformPoints(s_tempPtF);
            }
            return s_tempPtF[0];
        }

        /// <summary>
        /// Transforms vector by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="v">Vector</param>
        /// <returns>Inverse transformed vector</returns>
        public static PointF InverseTransformVector(Matrix matrix, PointF v)
        {
            using (Matrix inverse = matrix.Clone())
            {
                inverse.Invert();
                s_tempPtF[0] = v;
                inverse.TransformVectors(s_tempPtF);
            }
            return s_tempPtF[0];
        }

        /// <summary>
        /// Does inverse transform of a point's x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, from system A to system B</param>
        /// <param name="x">X-coordinate of a point in coordinate system B</param>
        /// <returns>X-coordinate of a point in coordinate system A</returns>
        public static float InverseTransform(Matrix matrix, float x)
        {
            using (Matrix inverse = matrix.Clone())
            {
                inverse.Invert();
                s_tempPtF[0].X = x;
                s_tempPtF[0].Y = 0.0f;
                inverse.TransformPoints(s_tempPtF);
            }
            return s_tempPtF[0].X;
        }

        /// <summary>
        /// Does inverse transform of vector's x-coordinate with assumed 0.0 y-coordinate</summary>
        /// <param name="matrix">Matrix representing transform, from system A to system B</param>
        /// <param name="x">X-coordinate of a vector in coordinate system B</param>
        /// <returns>X-coordinate of a vector in coordinate system A</returns>
        public static float InverseTransformVector(Matrix matrix, float x)
        {
            using (Matrix inverse = matrix.Clone())
            {
                inverse.Invert();
                s_tempPtF[0].X = x;
                s_tempPtF[0].Y = 0.0f;
                inverse.TransformVectors(s_tempPtF);
            }
            return s_tempPtF[0].X;
        }

        /// <summary>
        /// Transforms rectangle</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Transformed rectangle</returns>
        public static Rectangle Transform(Matrix matrix, Rectangle r)
        {
            s_tempPts[0] = new Point(r.Left, r.Top);
            s_tempPts[1] = new Point(r.Right, r.Bottom);
            matrix.TransformPoints(s_tempPts);
            return new Rectangle(s_tempPts[0].X, s_tempPts[0].Y, s_tempPts[1].X - s_tempPts[0].X, s_tempPts[1].Y - s_tempPts[0].Y);
        }

        /// <summary>
        /// Transforms rectangle</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Transformed rectangle</returns>
        public static RectangleF Transform(Matrix matrix, RectangleF r)
        {
            s_tempPtsF[0] = new PointF(r.Left, r.Top);
            s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
            matrix.TransformPoints(s_tempPtsF);
            return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
        }

        /// <summary>
        /// Transforms rectangle by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Inverse transformed rectangle</returns>
        public static Rectangle InverseTransform(Matrix matrix, Rectangle r)
        {
            using (Matrix inverse = matrix.Clone())
            {
                inverse.Invert();
                s_tempPts[0] = new Point(r.Left, r.Top);
                s_tempPts[1] = new Point(r.Right, r.Bottom);
                inverse.TransformPoints(s_tempPts);
            }
            return new Rectangle(s_tempPts[0].X, s_tempPts[0].Y, s_tempPts[1].X - s_tempPts[0].X, s_tempPts[1].Y - s_tempPts[0].Y);
        }

        /// <summary>
        /// Transforms rectangle by inverse transform</summary>
        /// <param name="matrix">Matrix representing transform</param>
        /// <param name="r">Rectangle</param>
        /// <returns>Inverse transformed rectangle</returns>
        public static RectangleF InverseTransform(Matrix matrix, RectangleF r)
        {
            using (Matrix inverse = matrix.Clone())
            {
                inverse.Invert();
                s_tempPtsF[0] = new PointF(r.Left, r.Top);
                s_tempPtsF[1] = new PointF(r.Right, r.Bottom);
                inverse.TransformPoints(s_tempPtsF);
            }
            return MakeRectangle(s_tempPtsF[0], s_tempPtsF[1]);
        }

        /// <summary>
        /// Tests if a segment intersects a rectangle</summary>
        /// <param name="seg">The segment</param>
        /// <param name="rect">Rectangle, in Windows coordinates, such that the top y has
        /// a lower value than the bottom Y</param>
        /// <returns><c>True</c> if the segment intersects the rectangle</returns>
        public static bool Intersects(Seg2F seg, RectangleF rect)
        {
            // Quick acceptance
            if (rect.Contains(seg.P1) ||
                rect.Contains(seg.P2))
                return true;

            // Check if both segment points are on same side of rectangle.
            if (seg.P1.X < rect.Left &&
                seg.P2.X < rect.Left)
                return false;

            if (seg.P1.Y < rect.Top &&
                seg.P2.Y < rect.Top)
                return false;

            if (seg.P1.X > rect.Right &&
                seg.P2.X > rect.Right)
                return false;

            if (seg.P1.Y > rect.Bottom &&
                seg.P2.Y > rect.Bottom)
                return false;

            // Check if all four rectangle points are on the same side of the line.
            Vec2F dir = new Vec2F(seg.P2.X - seg.P1.X, seg.P2.Y - seg.P1.Y);
            if (dir.LengthSquared > Seg2F.DegenerateLength * Seg2F.DegenerateLength)
            {
                Vec2F normal = dir.Perp;
                float dot1 = Vec2F.Dot(new Vec2F(rect.Left, rect.Top) - seg.P1, normal);
                float dot2 = Vec2F.Dot(new Vec2F(rect.Right, rect.Top) - seg.P1, normal);
                if (dot1 * dot2 > 0) // both are < 0 or both are > 0
                {
                    dot2 = Vec2F.Dot(new Vec2F(rect.Left, rect.Bottom) - seg.P1, normal);
                    if (dot1 * dot2 > 0) // both are < 0 or both are > 0
                    {
                        dot2 = Vec2F.Dot(new Vec2F(rect.Right, rect.Bottom) - seg.P1, normal);
                        if (dot1 * dot2 > 0) // both are < 0 or both are > 0
                            return false;
                    }
                }
            }

            // Must intersect.
            return true;
        }

        /// <summary>
        /// Tests whether or not the rectangle intersects a Bezier curve</summary>
        /// <param name="curve">Bezier curve</param>
        /// <param name="rect">Rectangle, in Windows coordinates, such that the top y has
        /// a lower value than the bottom y</param>
        /// <param name="tolerance">The picking tolerance</param>
        /// <returns><c>True</c> if the rectangle intersects the Bezier curve</returns>
        public static bool Intersects(BezierCurve2F curve, RectangleF rect, float tolerance)
        {
            //Note that the convex hull around the 4 control points always contains the curve.
            // Subdivide until we have line segments.
            if (curve.Flatness <= tolerance)
            {
                // Test as a line segment from P1 to P4 for simplicity's sake. This could have
                //  some false negatives if the tangent points are far away and colinear.
                Seg2F segment = new Seg2F(curve.P1, curve.P4);
                if (Intersects(segment, rect))
                    return true;
            }
            else
            {
                // Subdivide and try again.
                BezierCurve2F left, right;
                curve.Subdivide(0.5f, out left, out right);
                if (Intersects(left, rect, tolerance))
                    return true;
                if (Intersects(right, rect, tolerance))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Resizes the image to the given size</summary>
        /// <param name="image">Image to resize</param>
        /// <param name="size">New size</param>
        /// <returns>Resized version of the image, with the given size</returns>
        public static Image ResizeImage(Image image, int size)
        {
            return ResizeImage(image, size, size);
        }

        /// <summary>
        /// Resizes the image to the given width and height</summary>
        /// <param name="image">Image to resize</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <returns>Resized version of the image, with the given dimensions</returns>
        public static Image ResizeImage(Image image, int width, int height)
        {
            if (image == null)
                throw new ArgumentNullException();
            if (width <= 0 || height <= 0)
                throw new ArgumentOutOfRangeException();
            if (image.Width == width && image.Height == height)
                return new Bitmap(image);
            Bitmap bitmap = new Bitmap(width, height, image.PixelFormat);
            bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
            Graphics g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, new Rectangle(0, 0, width, height));
            g.Dispose();
            return bitmap;
        }

        /// <summary>
        /// Gets an image from the calling assembly's resources</summary>
        /// <param name="imageName">Full name of image resource</param>
        /// <returns>Image</returns>
        public static Image GetImage(string imageName)
        {
            return GetImage(Assembly.GetCallingAssembly(), imageName);
        }

        /// <summary>
        /// Gets an image from the calling assembly's resources</summary>
        /// <param name="path">Path prefix of image name, e.g., "Scea.Controls"</param>
        /// <param name="imageName">Name of image resource, e.g., "folder"</param>
        /// <returns>Image</returns>
        /// <remarks>The full name will be "path.imageName"</remarks>
        public static Image GetImage(string path, string imageName)
        {
            return GetImage(Assembly.GetCallingAssembly(), path + "." + imageName);
        }

        /// <summary>
        /// Gets an image from the given assembly's resources</summary>
        /// <param name="assembly">Assembly holding resource</param>
        /// <param name="imageName">Full name of image resource</param>
        /// <returns>Image</returns>
        public static Image GetImage(Assembly assembly, string imageName)
        {
            Image image = null;
            Stream strm = null;
            try
            {
                strm = assembly.GetManifestResourceStream(imageName);
                if (strm != null)
                {
                    image = new Bitmap(strm);
                    image = new Bitmap(image); // without this, we may get weird OutOfMemoryException
                }
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                Outputs.WriteLine(OutputMessageType.Error, ex.StackTrace);
            }
            finally
            {
                if (strm != null)
                    strm.Close();
            }
            return image;
        }

        /// <summary>
        /// Gets an icon from the calling assembly's resources</summary>
        /// <param name="iconName">Full name of icon resource</param>
        /// <returns>Icon</returns>
        public static Icon GetIcon(string iconName)
        {
            return GetIcon(Assembly.GetCallingAssembly(), iconName);
        }

        /// <summary>
        /// Gets an icon from the calling assembly's resources</summary>
        /// <param name="path">Path prefix of icon name, e.g., "Scea.Controls"</param>
        /// <param name="iconName">Name of icon resource, e.g., "folder"</param>
        /// <returns>Icon</returns>
        /// <remarks>The full name will be "path.iconName"</remarks>
        public static Icon GetIcon(string path, string iconName)
        {
            return GetIcon(Assembly.GetCallingAssembly(), path + "." + iconName);
        }

        /// <summary>
        /// Gets an icon from the given assembly's resources</summary>
        /// <param name="assembly">Assembly holding resource</param>
        /// <param name="iconName">Full name of icon resource</param>
        /// <returns>Icon</returns>
        public static Icon GetIcon(Assembly assembly, string iconName)
        {
            Icon icon = null;
            Stream strm = null;

            try
            {
                strm = assembly.GetManifestResourceStream(iconName);
                icon = new Icon(strm);
                //icon = new Icon(icon); // necessary, or we can't use icon once stream is closed
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
            }
            finally
            {
                if (strm != null)
                    strm.Close();
            }
            return icon;
        }

        /// <summary>
        /// Converts an image into an icon</summary>
        /// <param name="image">Source image</param>
        /// <returns>Icon</returns>
        public static Icon CreateIcon(Image image)
        {
            int size = Math.Max(image.Width, image.Height);
            return CreateIcon(image, size, false);
        }

        /// <summary>
        /// Converts an image into an icon</summary>
        /// <param name="image">Source image</param>
        /// <param name="size">The width and height of the icon. Standard
        /// sizes are 16x16, 32x32, 48x48, 64x64</param>
        /// <param name="keepAspectRatio">Whether the image should be squashed into a
        /// square or whether whitespace should be put around it</param>
        /// <returns>Icon</returns>
        public static Icon CreateIcon(Image image, int size, bool keepAspectRatio)
        {
            Bitmap square = new Bitmap(size, size); // create new bitmap
            using (Graphics g = Graphics.FromImage(square))
            {
                int x, y, w, h; // dimensions for new image

                if (!keepAspectRatio || image.Height == image.Width)
                {
                    // just fill the square
                    x = y = 0; // set x and y to 0
                    w = h = size; // set width and height to size
                }
                else
                {
                    // work out the aspect ratio
                    float r = (float)image.Width / (float)image.Height;

                    // set dimensions accordingly to fit inside size^2 square
                    if (r > 1)
                    { // w is bigger, so divide h by r
                        w = size;
                        h = (int)((float)size / r);
                        x = 0; y = (size - h) / 2; // center the image
                    }
                    else
                    { // h is bigger, so multiply w by r
                        w = (int)((float)size * r);
                        h = size;
                        y = 0; x = (size - w) / 2; // center the image
                    }
                }

                // make the image shrink nicely by using HighQualityBicubic mode
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, x, y, w, h); // draw image with specified dimensions
            }

            // following line would work directly on any image, but then
            // it wouldn't look as nice.
            return Icon.FromHandle(square.GetHicon());
        }

        /// <summary>
        /// Measures the size of a string using the given graphics context and font</summary>
        /// <param name="graphics">Graphics GDI+ drawing surface</param>
        /// <param name="text">String to measure</param>
        /// <param name="font">Font</param>
        /// <returns>Size of the string, using the given graphics context and font</returns>
        public static SizeF MeasureDisplayString(Graphics graphics, string text, Font font)
        {
            const int width = 32;

            using (var bitmap = new Bitmap(width, 1, graphics))
            {
                SizeF size = graphics.MeasureString(text, font);
                using (Graphics anagra = Graphics.FromImage(bitmap))
                {
                    int measured_width = (int)size.Width;

                    anagra.Clear(Color.White);
                    anagra.DrawString(text + "|", font, Brushes.Black, width - measured_width, -font.Height / 2);

                    for (int i = width - 1; i >= 0; i--)
                    {
                        measured_width--;
                        if (bitmap.GetPixel(i, 0).R == 0)
                        {
                            break;
                        }
                    }
                    return new SizeF(measured_width, size.Height);
                }
            }
        }

        /// <summary>
        /// Measures the width of a string using the given graphics context and font</summary>
        /// <param name="graphics">Graphics GDI+ drawing surface</param>
        /// <param name="text">String to measure</param>
        /// <param name="font">Font</param>
        /// <returns>Width of the string, using the given graphics context and font</returns>
        public static int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            return (int)MeasureDisplayString(graphics, text, font).Width;
        }

        /// <summary>
        /// Calculates the maximum width that can be used by each instance of the ToolStripItem type,
        /// such that the amount available is shared equally</summary>
        /// <typeparam name="T">The type of ToolStripItems to look for in 'owner'</typeparam>
        /// <param name="owner">The ToolStrip to examine</param>
        /// <returns>The available width divided by the number of instances of type T in 'owner',
        /// or the available width if there are no objects of this type found</returns>
        public static int GetPreferredWidth<T>(ToolStrip owner)
            where T : ToolStripItem
        {
            // Declare a variable to store the total available width as 
            // it is calculated, starting with the display width of the 
            // owning ToolStrip.
            Int32 width = owner.DisplayRectangle.Width;

            // Subtract the width of the overflow button if it is displayed. 
            if (owner.OverflowButton.Visible)
            {
                width = width - owner.OverflowButton.Width -
                        owner.OverflowButton.Margin.Horizontal;
            }

            // Declare a variable to maintain a count of ToolStripAutoFitTextBox 
            // items currently displayed in the owning ToolStrip. 
            Int32 springBoxCount = 0;

            foreach (ToolStripItem ownerItem in owner.Items)
            {
                // Ignore items on the overflow menu.
                if (ownerItem.IsOnOverflow) continue;

                if (ownerItem is T)
                {
                    // For each item of type T, increment the count and subtract the margin width
                    //  from the total available width.
                    springBoxCount++;
                    width -= ownerItem.Margin.Horizontal;
                }
                else
                {
                    // For all other items, subtract the full width from the total available width.
                    width = width - ownerItem.Width - ownerItem.Margin.Horizontal;
                }
            }

            // If there are multiple items of type T in the owning ToolStrip, divide the total
            //  available width between them.
            if (springBoxCount > 1) width /= springBoxCount;

            return width;
        }

        /// <summary>
        /// Creates the corner image for fast rendering of lozenges</summary>
        /// <param name="color1">First color for gradient of lozenge</param>
        /// <param name="color2">Second color for gradient of lozenge</param>
        /// <param name="cornerRadius">Radius of rounded corners</param>
        /// <returns>Corner image for fast rendering of lozenges</returns>
        public static Image CreateLozengeImage(
            Color color1,
            Color color2,
            int cornerRadius)
        {
            return CreateLozengeImage(color1, color2, Pens.Transparent, Color.FromArgb(0, 0, 0, 0), cornerRadius);
        }

        /// <summary>
        /// Creates the corner image for fast rendering of lozenges</summary>
        /// <param name="color1">First color for gradient of lozenge</param>
        /// <param name="color2">Second color for gradient of lozenge</param>
        /// <param name="outlinePen">Outline pen</param>
        /// <param name="shadowColor">Shadow color; e.g., Color.FromArgb(128, 160, 160, 160)</param>
        /// <param name="cornerRadius">Radius of rounded corners</param>
        /// <returns>Corner image for fast rendering of lozenges</returns>
        public static Image CreateLozengeImage(
            Color color1,
            Color color2,
            Pen outlinePen,
            Color shadowColor,
            int cornerRadius)
        {
            int diameter = 2 * cornerRadius;
            int size = diameter + ShadowSize;

            Bitmap image = new Bitmap(size, size);

            using (Graphics g = Graphics.FromImage(image))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // draw shadow disk
                using (SolidBrush shadowBrush = new SolidBrush(shadowColor))
                {
                    g.FillEllipse(shadowBrush, ShadowSize, ShadowSize, diameter, diameter);
                }

                // draw gradient disk
                using (LinearGradientBrush interiorBrush = new LinearGradientBrush(
                    new Rectangle(0, 0, diameter, diameter),
                    color1,
                    color2,
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillEllipse(interiorBrush, 0, 0, diameter, diameter);
                }

                // draw outline
                g.DrawEllipse(outlinePen, 0, 0, diameter, diameter);
            }

            return image;
        }

        /// <summary>
        /// Draws a lozenge, using a pre-calculated lozenge corner image</summary>
        /// <param name="lozengeImage">Pre-calculated lozenge corner image</param>
        /// <param name="bounds">Lozenge bounds, not including shadow area</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        /// <remarks>Call CreateLozengeImage to generate lozenge image</remarks>
        public static void DrawLozenge(Image lozengeImage, Rectangle bounds, Graphics g)
        {
            int cornerRadius = (lozengeImage.Width - ShadowSize) / 2;

            // top left corner
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Left, bounds.Top, cornerRadius + 1, cornerRadius + 1),
                new Rectangle(0, 0, cornerRadius, cornerRadius), GraphicsUnit.Pixel);
            // top right corner
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Right - cornerRadius, bounds.Top, cornerRadius + ShadowSize, cornerRadius + 1),
                new Rectangle(cornerRadius, 0, cornerRadius + ShadowSize, cornerRadius), GraphicsUnit.Pixel);
            // bottom right corner
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Right - cornerRadius, bounds.Bottom - cornerRadius, cornerRadius + ShadowSize, cornerRadius + ShadowSize),
                new Rectangle(cornerRadius, cornerRadius, cornerRadius + ShadowSize, cornerRadius + ShadowSize), GraphicsUnit.Pixel);
            // bottom left corner
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Left, bounds.Bottom - cornerRadius, cornerRadius + 1, cornerRadius + ShadowSize),
                new Rectangle(0, cornerRadius, cornerRadius, cornerRadius + ShadowSize), GraphicsUnit.Pixel);

            // top edge
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Left + cornerRadius, bounds.Top, bounds.Width - 2 * cornerRadius, cornerRadius),
                new Rectangle(cornerRadius, 0, 1, cornerRadius), GraphicsUnit.Pixel);
            // right edge
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Right - cornerRadius, bounds.Top + cornerRadius, cornerRadius + ShadowSize, bounds.Height - 2 * cornerRadius),
                new Rectangle(cornerRadius, cornerRadius, cornerRadius + ShadowSize, 1), GraphicsUnit.Pixel);
            // bottom edge
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Left + cornerRadius, bounds.Bottom - cornerRadius, bounds.Width - 2 * cornerRadius, cornerRadius + ShadowSize),
                new Rectangle(cornerRadius, cornerRadius, 1, cornerRadius + ShadowSize), GraphicsUnit.Pixel);
            // left edge
            g.DrawImage(lozengeImage,
                new Rectangle(bounds.Left, bounds.Top + cornerRadius, cornerRadius, bounds.Height - 2 * cornerRadius),
                new Rectangle(0, cornerRadius, cornerRadius, 1), GraphicsUnit.Pixel);

            // draw the inside
            g.DrawImage(lozengeImage,
                new Rectangle(
                    bounds.Left + cornerRadius - 1,
                    bounds.Top + cornerRadius - 1,
                    bounds.Width - 2 * cornerRadius + 1,
                    bounds.Height - 2 * cornerRadius + 1),
                new Rectangle(cornerRadius, cornerRadius, 1, 1),
                GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Width of shadow on lozenges</summary>
        public const int ShadowSize = 4;

        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window</summary>
        /// <param name="handle">The handle to the window</param>
        /// <returns>Window captured image (snapshot)</returns>
        public static Image CaptureWindow(IntPtr handle)
        {
            IntPtr hdc = User32.GetDC(handle);
            if (hdc == IntPtr.Zero)
                return null;

            User32.RECT rect = new User32.RECT();
            User32.GetWindowRect(handle, ref rect);

            int width = rect.Right - rect.Left;
            int height = rect.Bottom - rect.Top;

            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics gfx = Graphics.FromImage(bitmap))
                User32.PrintWindow(handle, gfx.GetHdc(), 0);

            return bitmap;
        }

        /// <summary>
        /// Size of standard expander</summary>
        public const int ExpanderSize = 8;


        

        ///// <summary>
        ///// Draws a tree-control style expander , which looks like a triangle</summary>
        ///// <param name="x">X-coordinate of expander top left corner</param>
        ///// <param name="y">Y-coordinate of expander top left corner</param>
        ///// <param name="expanded">Whether or not expander should appear "expanded"</param>
        ///// <param name="g">Graphics GDI+ drawing surface</param>
        //public static void DrawExpander(Graphics g, Pen pen, bool expanded, int x, int y, int expanderSize)
        //{
           

        //}
        /// <summary>
        /// Draws a tree-control style expander button, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded)</summary>
        /// <param name="x">X-coordinate of expander top left corner</param>
        /// <param name="y">Y-coordinate of expander top left corner</param>
        /// <param name="expanded">Whether or not expander should appear "expanded"</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawExpander(int x, int y, bool expanded, Graphics g)
        {
            DrawExpander(x, y, ExpanderSize, SystemPens.ControlDarkDark, expanded, g);
        }


        /// <summary>
        /// Draws a tree-control style expander button, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded)</summary>
        /// <param name="x">X-coordinate of expander top left corner</param>
        /// <param name="y">Y-coordinate of expander top left corner</param>
        /// <param name="expanded">Whether or not expander should appear "expanded"</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        /// <param name="p">Pen, should be 1 pixel wide</param>
        public static void DrawExpander(int x, int y, bool expanded, Graphics g, Pen p)
        {
            DrawExpander(x, y, ExpanderSize, p, expanded, g);
        }

        /// <summary>
        /// Draws a tree-control style expander.</summary>
        /// <param name="g"></param>
        /// <param name="x">X-coordinate of expander top left corner</param>
        /// <param name="y">Y-coordinate of expander top left corner</param>
        /// <param name="size">Size of expander, in pixels</param>
        /// <param name="expanded">Whether or not expander should appear "expanded"</param>
        /// <param name="brush">Brush used for filling background if not null.</param>
        /// <param name="pen">Pen for drawing outline if not null.</param>
        public static void DrawExpander(Graphics g,
            int x, 
            int y, 
            int size, 
            bool expanded,
            Brush brush,
            Pen   pen)
        {
            s_expanderPoints[0] = new Point(x, y + size);
            if (expanded)
            {                
                s_expanderPoints[1] = new Point(x + size, y + size);
                s_expanderPoints[2] = new Point(x + size, y);                
            }
            else
            {
                s_expanderPoints[1] = new Point(x + size, y + size / 2);
                s_expanderPoints[2] = new Point(x, y);            
            }

            if (brush != null)
                g.FillPolygon(brush, s_expanderPoints);
            if (pen != null)
                g.DrawPolygon(pen, s_expanderPoints);
        }

        /// <summary>
        /// Draws a tree-control style expander, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded)</summary>
        /// <param name="x">X-coordinate of expander top left corner</param>
        /// <param name="y">Y-coordinate of expander top left corner</param>
        /// <param name="size">Size of expander, in pixels</param>
        /// <param name="pen">Pen, should be 1 pixel wide</param>
        /// <param name="expanded">Whether or not expander should appear "expanded"</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawExpander(int x, int y, int size, Pen pen, bool expanded, Graphics g)
        {
            s_expanderPoints[0] = new Point(x, y + size);
            if (expanded)
            {
                s_expanderBrush.Color = pen.Color;
                s_expanderPoints[1] = new Point(x + size, y + size);
                s_expanderPoints[2] = new Point(x + size, y);
                g.FillPolygon(s_expanderBrush, s_expanderPoints);
            }
            else
            {
                s_expanderPoints[1] = new Point(x + size, y + size / 2);
                s_expanderPoints[2] = new Point(x, y);
                g.DrawPolygon(pen, s_expanderPoints);
            }


            // old style expander

            //g.DrawRectangle(pen, x, y, size, size);
            //int lineLength = size - 4;
            //int center = size / 2;
            //g.DrawLine(pen, x + 2, y + center, x + 2 + lineLength, y + center);
            //if (!expanded)
            //    g.DrawLine(pen, x + center, y + 2, x + center, y + 2 + lineLength);
        }

        /// <summary>
        /// Size of Office-style expander</summary>
        public const int OfficeExpanderSize = 9; // odd number

        /// <summary>
        /// Draws an Office-control style expander, which looks like a square with
        /// a dash (expanded) or a cross (unexpanded)</summary>
        /// <param name="x">X-coordinate of expander top left corner</param>
        /// <param name="y">Y-coordinate of expander top left corner</param>
        /// <param name="pen">Pen, should be 1 pixel wide</param>
        /// <param name="expanded">Whether or not expander should appear "expanded"</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawOfficeExpander(int x, int y, Pen pen, bool expanded, Graphics g)
        {
            int xMid = x + OfficeExpanderSize / 2;
            int yMid = y + OfficeExpanderSize / 2;
            int xEnd = x + OfficeExpanderSize - 1;
            int yEnd = y + OfficeExpanderSize - 1;
            if (!expanded)
            {
                int temp;
                temp = x;
                x = xEnd;
                xEnd = temp;
                temp = y;
                y = yEnd;
                yEnd = temp;
            }
            g.DrawLine(pen, x, y, xMid, yMid);
            g.DrawLine(pen, xMid, yMid, xEnd, y);
            g.DrawLine(pen, x, yMid, xMid, yEnd);
            g.DrawLine(pen, xMid, yEnd, xEnd, yMid);
        }

        /// <summary>
        /// Width of standard direction indicator</summary>
        public const int SortDirectionIndicatorWidth = 12;

        /// <summary>
        /// Height of standard direction indicator</summary>
        public const int SortDirectionIndicatorHeight = 6;

        /// <summary>
        /// Draws a sort direction indicator, used to show sort order in lists</summary>
        /// <param name="x">Horizontal position</param>
        /// <param name="y">Vertical position</param>
        /// <param name="up"><c>True</c> if indicator points up, false for down</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawSortDirectionIndicator(int x, int y, bool up, Graphics g)
        {
            DrawSortDirectionIndicator(
                x,
                y,
                SortDirectionIndicatorWidth,
                SortDirectionIndicatorHeight,
                up,
                SystemBrushes.ControlDark,
                g);
        }

        /// <summary>
        /// Draws a sort direction indicator, used to show sort order in lists</summary>
        /// <param name="x">Horizontal position</param>
        /// <param name="y">Vertical position</param>
        /// <param name="width">Indicator width</param>
        /// <param name="height">Indicator height</param>
        /// <param name="up"><c>True</c> if indicator points up, false for down</param>
        /// <param name="brush">Brush to fill interior of indicator</param>
        /// <param name="g">Graphics GDI+ drawing surface</param>
        public static void DrawSortDirectionIndicator(int x, int y, int width, int height, bool up, Brush brush, Graphics g)
        {
            if (up)
            {
                y += height - 1;
                height = -height;
            }
            s_directionIndicatorPoints[0] = new Point(x, y);
            s_directionIndicatorPoints[1] = new Point(x + width, y);
            s_directionIndicatorPoints[2] = new Point(x + width / 2, y + height);
            g.FillPolygon(brush, s_directionIndicatorPoints);
        }

        /// <summary>
        /// Makes a new instance of Rectangle that is specified by two points</summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        /// <returns>New instance of Rectangle that is specified by two points</returns> 
        public static Rectangle MakeRectangle(Point p1, Point p2)
        {
            int x = p1.X;
            int y = p1.Y;
            int width = p2.X - p1.X;
            int height = p2.Y - p1.Y;
            if (width < 0)
            {
                x += width;
                width = -width;
            }
            if (height < 0)
            {
                y += height;
                height = -height;
            }
            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Makes a new instance of Rectangle that is specified by two points</summary>
        /// <param name="p1">Start point</param>
        /// <param name="p2">End point</param>
        /// <returns>New instance of Rectangle that is specified by two points</returns>
        public static RectangleF MakeRectangle(PointF p1, PointF p2)
        {
            float x = p1.X;
            float y = p1.Y;
            float width = p2.X - p1.X;
            float height = p2.Y - p1.Y;
            if (width < 0)
            {
                x += width;
                width = -width;
            }
            if (height < 0)
            {
                y += height;
                height = -height;
            }
            return new RectangleF(x, y, width, height);
        }

        /// <summary>
        /// Make a new instance of Rectangle that represents a rectangular region that bounds this region</summary>
        /// <param name="region">Given region</param>
        /// <returns>New instance of Rectangle that bounds this region</returns>
        public static RectangleF GetRegionBounds(Region region)
        {
            RectangleF bound = new RectangleF();           
            foreach (var rect in region.GetRegionScans(new Matrix()))
            {
                if (bound.IsEmpty)
                    bound = rect;
                else
                    bound = RectangleF.Union(bound,rect);
            }
            return bound;
        }

        private static float s_dpiYFactor = 0.0f;
        private static readonly Point[] s_tempPts = new Point[2];
        private static readonly PointF[] s_tempPtsF = new PointF[2];
        private static readonly PointF[] s_tempPtF = new PointF[1];
        private static readonly Point[] s_directionIndicatorPoints = new Point[3];
        private static readonly Point[] s_expanderPoints = new Point[3];
        private static readonly SolidBrush s_expanderBrush = new SolidBrush(Color.Black);
    }
}
