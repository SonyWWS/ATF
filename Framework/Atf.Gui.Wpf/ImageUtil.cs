//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Image utilities and UIElement extension methods</summary>
    public static class ImageUtil
    {
        /// <summary>
        /// Renders a visual UI element to a bitmap frame, which represents image data returned by a decoder and accepted by encoders</summary>
        /// <param name="visual">Visual UI element to be rendered</param>
        /// <param name="scale">Rendering scale</param>
        /// <returns>Bitmap frame with rendered element</returns>
        public static BitmapFrame RenderToBitmapFrame(this UIElement visual, double scale)
        {
            return BitmapFrame.Create(RenderToBitmapSource(visual, scale));
        }

        public static BitmapSource RenderToBitmapSource(this UIElement visual, double scale)
        {
            Matrix m = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;

            int renderHeight = (int)(visual.RenderSize.Height * scale);
            int renderWidth = (int)(visual.RenderSize.Width * scale);
            var renderTarget = new RenderTargetBitmap(renderWidth, renderHeight,  96,  96, PixelFormats.Pbgra32);

            var sourceBrush = new VisualBrush(visual);
            var drawingVisual = new DrawingVisual();

            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.PushTransform(new ScaleTransform(scale, scale));
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(visual.RenderSize.Width, visual.RenderSize.Height)));
            }

            renderTarget.Render(drawingVisual);
            renderTarget.Freeze();

            return renderTarget;
        }

        public static ImageSource CreateFromFile(string path)
        {
            Requires.NotNull(path, "Invalid path");

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;

            try
            {
                image.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
                image.EndInit();
                image.StreamSource.Dispose();
            }
            catch (NotSupportedException)
            {
                return null;
            }
            catch (FileFormatException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            catch (SecurityException)
            {
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }

            return image;
        }

        /// <summary>
        /// Renders a visual UI element to a stream in bitmap (BMP) format</summary>
        /// <param name="visual">Visual UI element to be rendered</param>
        /// <param name="stream">Stream data written to</param>
        /// <param name="scale">Rendering scale</param>
        public static void RenderToBmp(this UIElement visual, Stream stream, double scale)
        {
            BitmapFrame frame = visual.RenderToBitmapFrame(scale);
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(frame);
            encoder.Save(stream);
        }

        public static System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;

            // Note: Memory stream must be left open. Bitmap will close it when it is Disposed.
            var outStream = new MemoryStream();
            {
                // from System.Media.BitmapImage to System.Drawing.Bitmap 
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }

            return bitmap;
        }

        public static BitmapSource BitmapToSource(System.Drawing.Bitmap bitmap)
        {
            BitmapSource destination;
            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
            destination = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
            destination.Freeze();
            return destination;
        }

        /// <summary>
        /// Renders a visual UI element to a stream in Joint Photographics Experts Group (JPEG) format
        /// at the highest quality level</summary>
        /// <param name="visual">Visual UI element to be rendered</param>
        /// <param name="stream">Stream data written to</param>
        public static void RenderToJpeg(this UIElement visual, Stream stream)
        {
            visual.RenderToJpeg(stream, 1, 100);
        }

        /// <summary>
        /// Renders a visual UI element to a stream in Joint Photographics Experts Group (JPEG) format
        /// at a high quality level (75)</summary>
        /// <param name="visual">Visual UI element to be rendered</param>
        /// <param name="stream">Stream data written to</param>
        /// <param name="scale">Rendering scale</param>
        public static void RenderToJpeg(this UIElement visual, Stream stream, double scale)
        {
            visual.RenderToJpeg(stream, scale, 75);
        }

        /// <summary>
        /// Renders a visual UI element to a stream in Joint Photographics Experts Group (JPEG) format</summary>
        /// <param name="visual">Visual UI element to be rendered</param>
        /// <param name="stream">Stream data written to</param>
        /// <param name="scale">Rendering scale</param>
        /// <param name="quality">Quality level of the JPEG image. 
        /// The value range is 1 (lowest quality) to 100 (highest quality) inclusive.</param>
        public static void RenderToJpeg(this UIElement visual, Stream stream, double scale, int quality)
        {
            BitmapFrame frame = visual.RenderToBitmapFrame(scale);
            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = quality;
            encoder.Frames.Add(frame);
            encoder.Save(stream);
        }

        public static BitmapSource CaptureWindow(Window w)
        {
            return RenderToBitmapSource(w, 1.0);
        }

        /// <summary>
        /// Captures a window view to a bitmap</summary>
        /// <param name="w">Captured window</param>
        /// <returns>Bitmap of window</returns>
        public static System.Drawing.Bitmap Capture(Window w)
        {
            IntPtr hwnd = new WindowInteropHelper(w).Handle;

            IntPtr hDC = User32.GetDC(hwnd);
            if (hDC != IntPtr.Zero)
            {
                var rect = new User32.RECT();
                User32.GetWindowRect(hwnd, ref rect);
                int width = rect.Right - rect.Left;
                int height = rect.Bottom - rect.Top;
                var bmp = new System.Drawing.Bitmap(width, height);

                using (var destGraphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    User32.PrintWindow(hwnd, destGraphics.GetHdc(), 0);
                }

                return bmp;
            }
            return null;
        }
    }
}
