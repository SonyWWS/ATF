//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Bitmap that snaps to its parent</summary>
    public class SnappingBitmap : FrameworkElement
    {
        /// <summary>
        /// Constructor</summary>
        public SnappingBitmap()
        {
            m_sourceDownloaded = OnSourceDownloaded;
            m_sourceFailed = OnSourceFailed;

            LayoutUpdated += OnLayoutUpdated;
        }

        /// <summary>
        /// Dependency property for the bitmap source</summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(BitmapSource),
            typeof(SnappingBitmap),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                SnappingBitmap.OnSourceChanged));

        /// <summary>
        /// Gets and sets the bitmap source</summary>
        public BitmapSource Source
        {
            get { return (BitmapSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Event fired when the bitmap fails to load</summary>
        public event EventHandler<ExceptionEventArgs> BitmapFailed;

        /// <summary>
        /// Return our measure size to be the size needed to display the bitmap pixels.</summary>
        /// <param name="availableSize">unused</param>
        /// <returns>The size of the bitmap</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            Size measureSize = new Size();

            BitmapSource bitmapSource = Source;
            if (bitmapSource != null)
            {
                PresentationSource ps = PresentationSource.FromVisual(this);
                if (ps != null)
                {
                    Matrix fromDevice = ps.CompositionTarget.TransformFromDevice;

                    Vector pixelSize = new Vector(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
                    Vector measureSizeV = fromDevice.Transform(pixelSize);
                    measureSize = new Size(measureSizeV.X, measureSizeV.Y);
                }
            }

            return measureSize;
        }

        /// <summary>
        /// Render the bitmap offset by the needed amount to align to pixels</summary>
        /// <param name="dc">The drawing context</param>
        protected override void OnRender(DrawingContext dc)
        {
            BitmapSource bitmapSource = Source;
            if (bitmapSource != null)
            {
                m_pixelOffset = GetPixelOffset();

                // Render the bitmap offset by the needed amount to align to pixels.
                Size desiredSize = new Size(
                    DesiredSize.Width - Margin.Left - Margin.Right,
                    DesiredSize.Height - Margin.Top - Margin.Bottom
                    );
                dc.DrawImage(bitmapSource, new Rect(m_pixelOffset, desiredSize));
            }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bitmap = (SnappingBitmap)d;

            var oldValue = (BitmapSource)e.OldValue;
            var newValue = (BitmapSource)e.NewValue;

            if (((oldValue != null) && (bitmap.m_sourceDownloaded != null)) && (!oldValue.IsFrozen))
            {
                oldValue.DownloadCompleted -= bitmap.m_sourceDownloaded;
                oldValue.DownloadFailed -= bitmap.m_sourceFailed;
                // ((BitmapSource)newValue).DecodeFailed -= bitmap._sourceFailed; // 3.5
            }
            if (newValue != null && !newValue.IsFrozen)
            {
                newValue.DownloadCompleted += bitmap.m_sourceDownloaded;
                newValue.DownloadFailed += bitmap.m_sourceFailed;
                // ((BitmapSource)newValue).DecodeFailed += bitmap._sourceFailed; // 3.5
            }
        }

        private void OnSourceDownloaded(object sender, EventArgs e)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }

        private void OnSourceFailed(object sender, ExceptionEventArgs e)
        {
            Source = null; // setting a local value seems sketchy...

            BitmapFailed(this, e);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            if (ActualHeight == 0 || ActualWidth == 0)
                return;

            // This event just means that layout happened somewhere.  However, this is
            // what we need since layout anywhere could affect our pixel positioning.
            Point pixelOffset = GetPixelOffset();
            if (!AreClose(pixelOffset, m_pixelOffset))
            {
                InvalidateVisual();
            }
        }

        // Gets the matrix that will convert a point from "above" the
        // coordinate space of a visual into the the coordinate space
        // "below" the visual.
        private Matrix GetVisualTransform(Visual v)
        {
            if (v != null)
            {
                Matrix m = Matrix.Identity;

                Transform transform = VisualTreeHelper.GetTransform(v);
                if (transform != null)
                {
                    Matrix cm = transform.Value;
                    m = Matrix.Multiply(m, cm);
                }

                Vector offset = VisualTreeHelper.GetOffset(v);
                m.Translate(offset.X, offset.Y);

                return m;
            }

            return Matrix.Identity;
        }

        private Point TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError, out bool success)
        {
            success = true;
            if (v != null)
            {
                Matrix visualTransform = GetVisualTransform(v);
                if (inverse)
                {
                    if (!throwOnError && !visualTransform.HasInverse)
                    {
                        success = false;
                        return new Point(0, 0);
                    }
                    visualTransform.Invert();
                }
                point = visualTransform.Transform(point);
            }
            return point;
        }

        private Point ApplyVisualTransform(Point point, Visual v, bool inverse)
        {
            bool success = true;
            return TryApplyVisualTransform(point, v, inverse, true, out success);
        }

        private Point GetPixelOffset()
        {
            Point pixelOffset = new Point();

            PresentationSource ps = PresentationSource.FromVisual(this);
            if (ps != null)
            {
                Visual rootVisual = ps.RootVisual;

                // Transform (0,0) from this element up to pixels.
                pixelOffset = TransformToAncestor(rootVisual).Transform(pixelOffset);
                pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, false);
                pixelOffset = ps.CompositionTarget.TransformToDevice.Transform(pixelOffset);

                // Round the origin to the nearest whole pixel.
                pixelOffset.X = Math.Round(pixelOffset.X);
                pixelOffset.Y = Math.Round(pixelOffset.Y);

                // Transform the whole-pixel back to this element.
                pixelOffset = ps.CompositionTarget.TransformFromDevice.Transform(pixelOffset);
                pixelOffset = ApplyVisualTransform(pixelOffset, rootVisual, true);
                pixelOffset = rootVisual.TransformToDescendant(this).Transform(pixelOffset);
            }

            return pixelOffset;
        }

        private bool AreClose(Point point1, Point point2)
        {
            return AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);
        }

        private bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            double delta = value1 - value2;
            return ((delta < 1.53E-06) && (delta > -1.53E-06));
        }

        private EventHandler m_sourceDownloaded;
        private EventHandler<ExceptionEventArgs> m_sourceFailed;
        private Point m_pixelOffset;
    }
}
