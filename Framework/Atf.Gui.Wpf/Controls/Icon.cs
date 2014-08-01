//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Icon class with options for image source, selected/deselected images, and shadows</summary>
    public class Icon : Image
    {
        /// <summary>
        /// Gets and sets the red chroma</summary>
        public SolidColorBrush RedChroma
        {
            get { return (SolidColorBrush)base.GetValue(RedChromaProperty); }
            set { base.SetValue(RedChromaProperty, value); }
        }

        /// <summary>
        /// RedChroma dependency property</summary>
        public static readonly DependencyProperty RedChromaProperty =
            DependencyProperty.RegisterAttached("RedChroma", typeof(SolidColorBrush), typeof(Icon),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the red chroma</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static SolidColorBrush GetRedChroma(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(RedChromaProperty);
        }

        /// <summary>
        /// Sets the red chroma</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetRedChroma(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(RedChromaProperty, value);
        }
            
        /// <summary>
        /// Gets and sets the blue chroma</summary>
        public SolidColorBrush BlueChroma
        {
            get { return (SolidColorBrush)base.GetValue(BlueChromaProperty); }
            set { base.SetValue(BlueChromaProperty, value); }
        }

        /// <summary>
        /// BlueChroma dependency property</summary>
        public static readonly DependencyProperty BlueChromaProperty = 
            DependencyProperty.RegisterAttached("BlueChroma", 
                typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the blue chroma</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static SolidColorBrush GetBlueChroma(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(BlueChromaProperty);
        }

        /// <summary>
        /// Sets the blue chroma</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetBlueChroma(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(BlueChromaProperty, value);
        }

        /// <summary>
        /// Gets and sets the green chroma</summary>
        public SolidColorBrush GreenChroma
        {
            get { return (SolidColorBrush)base.GetValue(GreenChromaProperty); }
            set { base.SetValue(GreenChromaProperty, value); }
        }

        /// <summary>
        /// GreenChroma dependency property</summary>
        public static readonly DependencyProperty GreenChromaProperty =
            DependencyProperty.RegisterAttached("GreenChroma", typeof(SolidColorBrush), typeof(Icon),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Sets the green chroma</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetGreenChroma(DependencyObject obj, SolidColorBrush value)
        {
            obj.SetValue(GreenChromaProperty, value);
        }

        /// <summary>
        /// Gets the green chroma</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static SolidColorBrush GetGreenChroma(DependencyObject obj)
        {
            return (SolidColorBrush)obj.GetValue(GreenChromaProperty);
        }

        /// <summary>
        /// Gets and sets the source brush</summary>
        public DrawingBrush SourceBrush
        {
            get { return (DrawingBrush)base.GetValue(SourceBrushProperty); }
            set { base.SetValue(SourceBrushProperty, value); }
        }

        /// <summary>
        /// SourceBrush dependency property</summary>
        public static readonly DependencyProperty SourceBrushProperty =
            DependencyProperty.Register("SourceBrush", typeof(DrawingBrush), typeof(Icon),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// SourceKey dependency property</summary>
        public static readonly DependencyProperty SourceKeyProperty =
             DependencyProperty.RegisterAttached("SourceKey", typeof(object), typeof(Icon),
             new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, SourceKeyPropertyChanged));

        /// <summary>
        /// Sets the source key</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetSourceKey(UIElement element, object value)
        {
            element.SetValue(SourceKeyProperty, value);
        }

        /// <summary>
        /// Gets the source key</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static object GetSourceKey(UIElement element)
        {
            return element.GetValue(SourceKeyProperty);
        }

        /// <summary>
        /// UseShadow dependency property</summary>
        public static readonly DependencyProperty UseShadowProperty =
            DependencyProperty.RegisterAttached("UseShadow", typeof(bool), typeof(Icon), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Sets whether to use a shadow</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetUseShadow(UIElement element, bool value)
        {
            element.SetValue(UseShadowProperty, value);
        }

        /// <summary>
        /// Gets whether to use a shadow</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static bool GetUseShadow(UIElement element)
        {
            return (bool)element.GetValue(UseShadowProperty);
        }

        /// <summary>
        /// DeselectedDrawingBrush dependency property</summary>
        public static readonly DependencyProperty DeselectedDrawingBrushProperty = 
            DependencyProperty.RegisterAttached("DeselectedDrawingBrush", 
                typeof(DrawingBrush), typeof(Icon), new PropertyMetadata(null));

        /// <summary>
        /// Sets the deselected drawing brush</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetDeselectedDrawingBrush(DependencyObject obj, DrawingBrush value)
        {
            obj.SetValue(DeselectedDrawingBrushProperty, value);
        }

        /// <summary>
        /// Gets the deselected drawing brush</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static DrawingBrush GetDeselectedDrawingBrush(DependencyObject obj)
        {
            return (DrawingBrush)obj.GetValue(DeselectedDrawingBrushProperty);
        }

        /// <summary>
        /// DeselectedImage dependency property</summary>
        public static readonly DependencyProperty DeselectedImageProperty = 
            DependencyProperty.RegisterAttached("DeselectedImage", typeof(ImageSource), typeof(Icon), 
                new PropertyMetadata(null));

        /// <summary>
        /// Sets the selected image</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetSelectedImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(SelectedImageProperty, value);
        }

        /// <summary>
        /// Gets the selected image</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static ImageSource GetSelectedImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(SelectedImageProperty);
        }

        /// <summary>
        /// SelectedDrawingBrush dependency property</summary>
        public static readonly DependencyProperty SelectedDrawingBrushProperty = 
            DependencyProperty.RegisterAttached("SelectedDrawingBrush", typeof(DrawingBrush), typeof(Icon), 
                new PropertyMetadata(null));

        /// <summary>
        /// Sets the selected drawing brush</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetSelectedDrawingBrush(DependencyObject obj, DrawingBrush value)
        {
            obj.SetValue(SelectedDrawingBrushProperty, value);
        }

        /// <summary>
        /// Gets the selected drawing brush</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static DrawingBrush GetSelectedDrawingBrush(DependencyObject obj)
        {
            return (DrawingBrush)obj.GetValue(SelectedDrawingBrushProperty);
        }

        /// <summary>
        /// SelectedImage dependency property</summary>
        public static readonly DependencyProperty SelectedImageProperty = 
            DependencyProperty.RegisterAttached("SelectedImage", typeof(ImageSource), typeof(Icon), 
                new PropertyMetadata(null));

        /// <summary>
        /// Sets the deselected image</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetDeselectedImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(DeselectedImageProperty, value);
        }

        /// <summary>
        /// Gets the deselected image</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static ImageSource GetDeselectedImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(DeselectedImageProperty);
        }

        /// <summary>
        /// ShowSelectedIconOnMouseOver dependency property</summary>
        public static readonly DependencyProperty ShowSelectedIconOnMouseOverProperty = 
            DependencyProperty.RegisterAttached("ShowSelectedIconOnMouseOver", typeof(bool), typeof(Icon), 
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Sets whether to show the selected icon on mouseover</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <param name="value">Value to set</param>
        public static void SetShowSelectedIconOnMouseOver(DependencyObject obj, bool value)
        {
            obj.SetValue(ShowSelectedIconOnMouseOverProperty, value);
        }

        /// <summary>
        /// Gets whether to show the selected icon on mouseover</summary>
        /// <param name="obj">Dependency object to query for the property</param>
        /// <returns>Value of the property</returns>
        public static bool GetShowSelectedIconOnMouseOver(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShowSelectedIconOnMouseOverProperty);
        }

        /// <summary>
        /// Gets and sets the overlay image source</summary>
        public ImageSource OverlayImageSource
        {
            get { return (ImageSource)base.GetValue(OverlayImageSourceProperty); }
            set { base.SetValue(OverlayImageSourceProperty, value); }
        }

        /// <summary>
        /// OverlayImageSource dependency property</summary>
        public static readonly DependencyProperty OverlayImageSourceProperty = 
            DependencyProperty.Register("OverlayImageSource", typeof(ImageSource), typeof(Icon), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets and sets the overlay bounding rectangle</summary>
        public Rect OverlayRect
        {
            get { return (Rect)base.GetValue(OverlayRectProperty); }
            set { base.SetValue(OverlayRectProperty, value); }
        }

        /// <summary>
        /// OverlayRect dependency property</summary>
        public static readonly DependencyProperty OverlayRectProperty = 
            DependencyProperty.Register("OverlayRect", typeof(Rect), typeof(Icon), 
                new FrameworkPropertyMetadata(new Rect(0.0, 0.0, 0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets and sets whether to show the overlay</summary>
        public bool ShowOverlay
        {
            get { return (bool)base.GetValue(ShowOverlayProperty); }
            set { base.SetValue(ShowOverlayProperty, value); }
        }

        /// <summary>
        /// ShowOverlay dependency property</summary>
        public static readonly DependencyProperty ShowOverlayProperty = 
            DependencyProperty.Register("ShowOverlay", typeof(bool), typeof(Icon), 
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets the offset to snap to</summary>
        /// <param name="visual">Rendering support object</param>
        /// <returns>Snapping offset</returns>
        public static Point GetPixelSnappingOffset(Visual visual)
        {
            PresentationSource source = PresentationSource.FromVisual(visual);
            if (source != null)
            {
                return GetPixelSnappingOffset(visual, source.RootVisual);
            }
            return new Point();
        }

        /// <summary>
        /// Overrides the system Arrange</summary>
        /// <param name="finalSize">Size used to arrange the control</param>
        /// <returns>Returns finalSize if the ImageSource is null, otherwise returns result of base.ArrangeOverride()</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (base.Source == null)
            {
                return finalSize;
            }
            
            return base.ArrangeOverride(finalSize);
        }

        /// <summary>
        /// Render the icon</summary>
        /// <param name="drawingContext">Context to draw to</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            var source = base.Source as BitmapSource;
            var rectangle = new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height);
            if ((this.SourceBrush == null) || (((source != null) && IsClose(source.Width, base.RenderSize.Width)) && IsClose(source.Height, base.RenderSize.Height)))
            {
                ImageSource renderSource = this.RenderSource;
                if (renderSource != null)
                {
                    drawingContext.DrawImage(renderSource, rectangle);
                }
            }
            else
            {
                if (GetUseShadow(this))
                {
                    // Test shadow
                    var shadowRectangle = new Rect(1.5, 1.5, base.RenderSize.Width, base.RenderSize.Height);
                    var shadowBrush = ColorSwapper.SwapColors(SourceBrush, GetShadowColor);
                    drawingContext.DrawRectangle(shadowBrush, null, shadowRectangle);
                }

                drawingContext.DrawRectangle(this.RenderSourceBrush, null, rectangle);
            }

            if (ShowOverlay)
            {
                var overlayImageSource = OverlayImageSource;
                if (overlayImageSource != null)
                {
                    drawingContext.DrawImage(overlayImageSource, OverlayRect);
                }
            }
        }

        static Icon()
        {
            Image.StretchProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(Stretch.None));
        }

        private static void SourceKeyPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var icon = o as Icon;
            if (icon == null)
                return;

            icon.Source = null;
            icon.SourceBrush = null;

            if (e.NewValue != null)
            {
                object resource = icon.FindResource(e.NewValue);

                var b = resource as DrawingBrush;
                if (b != null)
                {
                    icon.SourceBrush = b;
                    icon.Stretch = Stretch.None;
                }
                else
                {
                    var s = resource as ImageSource;
                    if (s != null)
                    {
                        icon.Source = s;
                        icon.Stretch = Stretch.Uniform;
                    }
                }
            }
        }

        private Color GetShadowColor(Color c)
        {
            return Color.FromRgb(0x16, 0x16, 0x16);
        }

        private static Point GetPixelSnappingOffset(Visual visual, Visual rootVisual)
        {
            var point = new Point();
            if (rootVisual != null)
            {
                var transform = visual.TransformToAncestor(rootVisual) as Transform;
                if ((transform != null) && transform.Value.HasInverse)
                {
                    point = visual.PointFromScreen(visual.PointToScreen(point));
                }
            }
            
            return point;
        }

        private Color ConvertColor(Color color)
        {
            if ((color.R != color.G) || (color.R != color.B))
            {
                if ((color.G == color.B) && (this.RedChroma != null))
                {
                    return this.ScaleColor(this.RedChroma.Color, color.R, color.G, color.A);
                }
                if ((color.R == color.B) && (this.GreenChroma != null))
                {
                    return this.ScaleColor(this.GreenChroma.Color, color.G, color.R, color.A);
                }
                if ((color.R == color.G) && (this.BlueChroma != null))
                {
                    return this.ScaleColor(this.BlueChroma.Color, color.B, color.R, color.A);
                }
            }
            return color;
        }

        private static bool IsClose(double num1, double num2)
        {
            return (num1 > (num2 * 0.9));
        }

        private Color ScaleColor(Color color, byte primary, byte white, byte alpha)
        {
            return Color.FromArgb((byte)((alpha * color.A) / 0xff), (byte)(((((double)color.R) / 255.0) * (primary - white)) + white), (byte)(((((double)color.G) / 255.0) * (primary - white)) + white), (byte)(((((double)color.B) / 255.0) * (primary - white)) + white));
        }

        private ImageSource RenderSource
        {
            get
            {
                if (base.Source == null)
                {
                    return null;
                }
                if (((this.RedChroma == null) && (this.GreenChroma == null)) && (this.BlueChroma == null))
                {
                    return base.Source;
                }
                
                return ColorSwapper.SwapColors(base.Source, new ColorCallback(this.ConvertColor));
            }
        }

        private DrawingBrush RenderSourceBrush
        {
            get
            {
                if (this.SourceBrush == null)
                {
                    return null;
                }
                if (((this.RedChroma == null) && (this.GreenChroma == null)) && (this.BlueChroma == null))
                {
                    return this.SourceBrush;
                }
                
                return (DrawingBrush)ColorSwapper.SwapColors(this.SourceBrush, new ColorCallback(this.ConvertColor));
            }
        }

    }

    /// <summary>
    /// Callback function to provide a color mapping for ColorSwapper functions</summary>
    /// <param name="color">Starting color</param>
    /// <returns>Color to use instead</returns>
    public delegate Color ColorCallback(Color color);

    /// <summary>
    /// Utility class to swap colors in various types of images</summary>
    public static class ColorSwapper
    {
        /// <summary>
        /// Swap the color of a brush based on a color mapping provided by the ColorCallback</summary>
        /// <param name="brush">Original brush</param>
        /// <param name="colorCallback">Callback to provide the color mapping</param>
        /// <returns>Color-swapped brush</returns>
        public static Brush SwapColors(Brush brush, ColorCallback colorCallback)
        {
            if (colorCallback == null)
            {
                throw new ArgumentNullException("colorCallback");
            }
            Brush brush2 = brush;
            if (brush != null)
            {
                brush2 = brush.Clone();
                SwapColorsWithoutCloning(brush2, colorCallback);
                brush2.Freeze();
            }
            return brush2;
        }

        /// <summary>
        /// Swap the colors in a drawing based on a color mapping provided by the ColorCallback</summary>
        /// <param name="drawing">Original drawing</param>
        /// <param name="colorCallback">Callback to provide the color mapping</param>
        /// <returns>Color-swapped drawing</returns>
        public static Drawing SwapColors(Drawing drawing, ColorCallback colorCallback)
        {
            if (colorCallback == null)
            {
                throw new ArgumentNullException("colorCallback");
            }
            Drawing drawing2 = drawing;
            if (drawing != null)
            {
                drawing2 = drawing.Clone();
                SwapColorsWithoutCloning(drawing2, colorCallback);
                drawing2.Freeze();
            }
            return drawing2;
        }

        /// <summary>
        /// Swap the colors in an image based on a color mapping provided by the ColorCallback</summary>
        /// <param name="imageSource">Original image</param>
        /// <param name="colorCallback">Callback to provide the color mapping</param>
        /// <returns>Color-swapped image</returns>
        public static ImageSource SwapColors(ImageSource imageSource, ColorCallback colorCallback)
        {
            if (colorCallback == null)
            {
                throw new ArgumentNullException("colorCallback");
            }
            ImageSource source = imageSource;
            if (imageSource == null)
            {
                return source;
            }
            DrawingImage image = imageSource as DrawingImage;
            if (image != null)
            {
                source = image = image.Clone();
                SwapColorsWithoutCloning(image.Drawing, colorCallback);
                source.Freeze();
                return source;
            }
            BitmapSource bitmapSource = imageSource as BitmapSource;
            if (bitmapSource == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedImageSourceType", new object[] { imageSource.GetType().Name }));
            }
            return SwapColors(bitmapSource, colorCallback);
        }

        /// <summary>
        /// Swap the colors in a bitmap based on a color mapping provided by the ColorCallback</summary>
        /// <param name="bitmapSource">Original bitmap</param>
        /// <param name="colorCallback">Callback to provide the color mapping</param>
        /// <returns>Color-swapped bitmap</returns>
        public static BitmapSource SwapColors(BitmapSource bitmapSource, ColorCallback colorCallback)
        {
            if (colorCallback == null)
            {
                throw new ArgumentNullException("colorCallback");
            }
            BitmapSource source = bitmapSource;
            if (bitmapSource != null)
            {
                PixelFormat destinationFormat = PixelFormats.Bgra32;
                BitmapPalette destinationPalette = null;
                double alphaThreshold = 0.0;
                FormatConvertedBitmap bitmap = new FormatConvertedBitmap(bitmapSource, destinationFormat, destinationPalette, alphaThreshold);
                int pixelWidth = bitmap.PixelWidth;
                int pixelHeight = bitmap.PixelHeight;
                int stride = 4 * pixelWidth;
                byte[] pixels = new byte[stride * pixelHeight];
                bitmap.CopyPixels(pixels, stride, 0);
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    Color color = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
                    Color color2 = colorCallback(color);
                    if (color2 != color)
                    {
                        pixels[i] = color2.B;
                        pixels[i + 1] = color2.G;
                        pixels[i + 2] = color2.R;
                        pixels[i + 3] = color2.A;
                    }
                }
                source = BitmapSource.Create(pixelWidth, pixelHeight, bitmap.DpiX, bitmap.DpiY, destinationFormat, destinationPalette, pixels, stride);
                source.Freeze();
            }
            return source;
        }

        private static void SwapColorsWithoutCloning(Brush brush, ColorCallback colorCallback)
        {
            if (brush != null)
            {
                SolidColorBrush brush2 = brush as SolidColorBrush;
                if (brush2 != null)
                {
                    brush2.Color = colorCallback(brush2.Color);
                }
                else
                {
                    GradientBrush brush3 = brush as GradientBrush;
                    if (brush3 == null)
                    {
                        DrawingBrush brush4 = brush as DrawingBrush;
                        if (brush4 != null)
                        {
                            SwapColorsWithoutCloning(brush4.Drawing, colorCallback);
                        }
                        else
                        {
                            ImageBrush brush5 = brush as ImageBrush;
                            if (brush5 != null)
                            {
                                brush5.ImageSource = SwapColorsWithoutCloningIfPossible(brush5.ImageSource, colorCallback);
                            }
                            else if (!(brush is VisualBrush))
                            {
                                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedBrushType", new object[] { brush.GetType().Name }));
                            }
                        }
                    }
                    else
                    {
                        foreach (GradientStop stop in brush3.GradientStops)
                        {
                            stop.Color = colorCallback(stop.Color);
                        }
                    }
                }
            }
        }

        private static void SwapColorsWithoutCloning(Drawing drawing, ColorCallback colorCallback)
        {
            if (drawing != null)
            {
                DrawingGroup group = drawing as DrawingGroup;
                if (group != null)
                {
                    for (int i = 0; i < group.Children.Count; i++)
                    {
                        SwapColorsWithoutCloning(group.Children[i], colorCallback);
                    }
                }
                else
                {
                    GeometryDrawing drawing2 = drawing as GeometryDrawing;
                    if (drawing2 != null)
                    {
                        SwapColorsWithoutCloning(drawing2.Brush, colorCallback);
                        if (drawing2.Pen != null)
                        {
                            SwapColorsWithoutCloning(drawing2.Pen.Brush, colorCallback);
                        }
                    }
                    else
                    {
                        GlyphRunDrawing drawing3 = drawing as GlyphRunDrawing;
                        if (drawing3 != null)
                        {
                            SwapColorsWithoutCloning(drawing3.ForegroundBrush, colorCallback);
                        }
                        else
                        {
                            ImageDrawing drawing4 = drawing as ImageDrawing;
                            if (drawing4 != null)
                            {
                                drawing4.ImageSource = SwapColorsWithoutCloningIfPossible(drawing4.ImageSource, colorCallback);
                            }
                            else if (!(drawing is VideoDrawing))
                            {
                                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedDrawingType", new object[] { drawing.GetType().Name }));
                            }
                        }
                    }
                }
            }
        }

        private static ImageSource SwapColorsWithoutCloningIfPossible(ImageSource imageSource, ColorCallback colorCallback)
        {
            ImageSource source = imageSource;
            if (imageSource == null)
            {
                return source;
            }
            DrawingImage image = imageSource as DrawingImage;
            if (image != null)
            {
                SwapColorsWithoutCloning(image.Drawing, colorCallback);
                return source;
            }
            BitmapSource bitmapSource = imageSource as BitmapSource;
            if (bitmapSource == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedImageSourceType", new object[] { imageSource.GetType().Name }));
            }
            return SwapColors(bitmapSource, colorCallback);
        }
    }

    class ShadowChrome : Decorator
    {
        // *** Fields ***

        private static SolidColorBrush backgroundBrush;
        private static LinearGradientBrush rightBrush;
        private static LinearGradientBrush bottomBrush;
        private static RadialGradientBrush bottomRightBrush;
        private static RadialGradientBrush topRightBrush;
        private static RadialGradientBrush bottomLeftBrush;

        // *** Constructors ***

        static ShadowChrome()
        {
            MarginProperty.OverrideMetadata(typeof(ShadowChrome), new FrameworkPropertyMetadata(new Thickness(0, 0, 1, 1)));

            CreateBrushes();
        }

        // *** Overriden base methods ***

        protected override void OnRender(DrawingContext drawingContext)
        {
            // Calculate the size of the shadow

            double shadowSize = Math.Min(Margin.Right, Margin.Bottom);

            // If there is no shadow, or it is bigger than the size of the child, then just return

            if (shadowSize <= 0 || this.ActualWidth < shadowSize * 2 || this.ActualHeight < shadowSize * 2)
                return;

            // Draw the background (this may show through rounded corners of the child object)

            Rect backgroundRect = new Rect(shadowSize, shadowSize, this.ActualWidth - shadowSize, this.ActualHeight - shadowSize);
            drawingContext.DrawRectangle(backgroundBrush, null, backgroundRect);

            // Now draw the shadow gradients

            Rect topRightRect = new Rect(this.ActualWidth, shadowSize, shadowSize, shadowSize);
            drawingContext.DrawRectangle(topRightBrush, null, topRightRect);

            Rect rightRect = new Rect(this.ActualWidth, shadowSize * 2, shadowSize, this.ActualHeight - shadowSize * 2);
            drawingContext.DrawRectangle(rightBrush, null, rightRect);

            Rect bottomRightRect = new Rect(this.ActualWidth, this.ActualHeight, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomRightBrush, null, bottomRightRect);

            Rect bottomRect = new Rect(shadowSize * 2, this.ActualHeight, this.ActualWidth - shadowSize * 2, shadowSize);
            drawingContext.DrawRectangle(bottomBrush, null, bottomRect);

            Rect bottomLeftRect = new Rect(shadowSize, this.ActualHeight, shadowSize, shadowSize);
            drawingContext.DrawRectangle(bottomLeftBrush, null, bottomLeftRect);
        }

        // *** Private static methods ***

        private static void CreateBrushes()
        {
            // Get the colors for the shadow

            Color shadowColor = Color.FromArgb(128, 0, 0, 0);
            Color transparentColor = Color.FromArgb(16, 0, 0, 0);

            // Create a GradientStopCollection from these

            GradientStopCollection gradient = new GradientStopCollection(2);
            gradient.Add(new GradientStop(shadowColor, 0.5));
            gradient.Add(new GradientStop(transparentColor, 1.0));

            // Create the background brush

            backgroundBrush = new SolidColorBrush(shadowColor);

            // Create the LinearGradientBrushes

            rightBrush = new LinearGradientBrush(gradient, new Point(0.0, 0.0), new Point(1.0, 0.0));
            bottomBrush = new LinearGradientBrush(gradient, new Point(0.0, 0.0), new Point(0.0, 1.0));

            // Create the RadialGradientBrushes

            bottomRightBrush = new RadialGradientBrush(gradient);
            bottomRightBrush.GradientOrigin = new Point(0.0, 0.0);
            bottomRightBrush.Center = new Point(0.0, 0.0);
            bottomRightBrush.RadiusX = 1.0;
            bottomRightBrush.RadiusY = 1.0;

            topRightBrush = new RadialGradientBrush(gradient);
            topRightBrush.GradientOrigin = new Point(0.0, 1.0);
            topRightBrush.Center = new Point(0.0, 1.0);
            topRightBrush.RadiusX = 1.0;
            topRightBrush.RadiusY = 1.0;

            bottomLeftBrush = new RadialGradientBrush(gradient);
            bottomLeftBrush.GradientOrigin = new Point(1.0, 0.0);
            bottomLeftBrush.Center = new Point(1.0, 0.0);
            bottomLeftBrush.RadiusX = 1.0;
            bottomLeftBrush.RadiusY = 1.0;
        }
    }
}
