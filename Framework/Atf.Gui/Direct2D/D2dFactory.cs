//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using Sce.Atf.VectorMath;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;

using SizeF = System.Drawing.SizeF;
using PointF = System.Drawing.PointF;
using Color = System.Drawing.Color;
using FontStyle = SharpDX.DirectWrite.FontStyle;
using GdiFontStyle = System.Drawing.FontStyle;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace Sce.Atf.Direct2D
{
    /// <summary>
    /// Represents the Direct2D factory. Creates Direct2D resources.
    /// Any resource created by this factory can be shared between D2dGraphics.</summary>
    public static class D2dFactory
    {
        /// <summary>
        /// Creates D2dHwndGraphics for given handle</summary>
        /// <param name="hwnd">Window handle</param>
        /// <returns>D2dHwndGraphics object for given handle</returns>
        /// <remarks>Important: This factory forces 1 DIP (device independent pixel) = 1 pixel
        /// regardless of current DPI settings.</remarks>                
        public static D2dHwndGraphics CreateD2dHwndGraphics(IntPtr hwnd)
        {
            CheckForRecreateTarget();
            HwndRenderTargetProperties hwnProps
                = new HwndRenderTargetProperties();
            hwnProps.Hwnd = hwnd;
            hwnProps.PixelSize = new SharpDX.Size2(16, 16);
            hwnProps.PresentOptions = PresentOptions.Immediately;

            WindowRenderTarget rt = null;

            if (s_rtprops.DpiX == 0)
            {
                // force  1 dip = 1 pixel
                s_rtprops.DpiX = 96.0f;
                s_rtprops.DpiY = 96.0f;
                s_rtprops.PixelFormat = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
                s_rtprops.Usage = RenderTargetUsage.GdiCompatible;
                s_rtprops.MinLevel = FeatureLevel.Level_DEFAULT;

                //create gdi compatible direct 2d render target.
                try
                {
                    s_rtprops.Type = RenderTargetType.Hardware;
                    rt = new WindowRenderTarget(s_d2dFactory, s_rtprops, hwnProps);
                }
                catch
                {
                    // try to create software target.
                    s_rtprops.Type = RenderTargetType.Software;
                    rt = new WindowRenderTarget(s_d2dFactory, s_rtprops, hwnProps);
                }
            }
            else// use the property of the first created render target.
            {
                rt = new WindowRenderTarget(s_d2dFactory, s_rtprops, hwnProps);
            }
            return new D2dHwndGraphics(rt);
        }

        /// <summary>
        /// Creates an instance of D2dGraphics 
        /// that is used for creating shared resources</summary>
        /// <param name="hwnd">The handle of the control or Form
        /// that is used for creating D2dGraphics</param>
        public static void EnableResourceSharing(IntPtr hwnd)
        {
            if (s_gfx == null)
            {
                s_gfx = CreateD2dHwndGraphics(hwnd);
            }
        }

        /// <summary>
        /// Creates an instance of D2dWicGraphics that can be used for rendering to off screen surfaces
        /// and can be copied to main memory</summary>
        /// <param name="width">Width of the off screen surface.</param>
        /// <param name="height">Height of the off screen surface.</param>
        /// <returns>D2dWicGraphics that can be used for rendering to off screen surfaces</returns>
        public static D2dWicGraphics CreateWicGraphics(int width, int height)
        {
            var wicBitmap = new SharpDX.WIC.Bitmap(s_wicFactory, width, height, SharpDX.WIC.PixelFormat.Format32bppPBGRA,
                SharpDX.WIC.BitmapCreateCacheOption.CacheOnLoad);

            var rtprops = new RenderTargetProperties 
            {
                Type = RenderTargetType.Default,
                DpiX = 96.0f, 
                DpiY = 96.0f,
                PixelFormat = new PixelFormat(Format.Unknown, AlphaMode.Unknown), 
                Usage = RenderTargetUsage.None,
                MinLevel = FeatureLevel.Level_DEFAULT
            };

            var rt = new WicRenderTarget(s_d2dFactory, wicBitmap, rtprops);
            return new D2dWicGraphics(rt, wicBitmap);
        }


        /// <summary>
        /// Creates a D2dStrokeStyle that describes start cap, dash pattern,
        /// and other features of a stroke</summary>
        /// <param name="props">A structure that describes the stroke's line cap, 
        /// dash offset, and other details of a stroke</param>    
        /// <returns>D2dStrokeStyle that describes stroke features</returns>
        public static D2dStrokeStyle CreateD2dStrokeStyle(D2dStrokeStyleProperties props)
        {
            return CreateD2dStrokeStyle(props, new float[0]);
        }

        /// <summary>
        /// Creates a D2dStrokeStyle that describes start cap, dash pattern,
        /// and other features of a stroke</summary>
        /// <param name="props">A structure that describes the  stroke's line cap, 
        /// dash offset, and other details of a stroke.</param>    
        /// <param name="dashes">An array whose elements are set to the length of each dash and space in the
        /// dash pattern. The first element sets the length of a dash, the second element
        /// sets the length of a space, the third element sets the length of a dash,
        /// and so on. The length of each dash and space in the dash pattern is the product
        /// of the element value in the array and the stroke width.</param>
        /// <returns>D2dStrokeStyle that describes stroke features</returns>
        public static D2dStrokeStyle CreateD2dStrokeStyle(D2dStrokeStyleProperties props, float[] dashes)
        {
            var nativeProps = new StrokeStyleProperties();
            nativeProps.DashCap = (CapStyle)props.DashCap;
            nativeProps.DashOffset = props.DashOffset;
            nativeProps.DashStyle = (DashStyle)props.DashStyle;
            nativeProps.EndCap = (CapStyle)props.EndCap;
            nativeProps.LineJoin = (LineJoin)props.LineJoin;
            nativeProps.MiterLimit = props.MiterLimit;
            nativeProps.StartCap = (CapStyle)props.StartCap;

            var strokeStyle = new StrokeStyle(s_d2dFactory, nativeProps, dashes);
            return new D2dStrokeStyle(strokeStyle);
        }

        /// <summary>
        /// Creates a text format object used for text layout with normal weight, style
        /// and stretch</summary>
        /// <param name="fontFamilyName">The name of the font family</param>
        /// <param name="fontSize">The logical size of the font in pixel units.</param>
        /// <returns>New instance of D2dTextFormat</returns>
        public static D2dTextFormat CreateTextFormat(string fontFamilyName, float fontSize)
        {
            TextFormat txtFormat = new TextFormat(s_dwFactory, fontFamilyName, fontSize);
            return new D2dTextFormat(txtFormat);
        }

        /// <summary>
        /// Creates a text format with the specified parameter</summary>
        /// <param name="fontFamilyName">The name of the font family</param>
        /// <param name="fontWeight">A value that indicates the font weight</param>
        /// <param name="fontStyle">A value that indicates the font style</param>        
        /// <param name="fontSize">The logical size of the font in pixels</param>        
        /// <returns>A new instance of D2dTextFormat</returns>
        public static D2dTextFormat CreateTextFormat(
            string fontFamilyName,
            D2dFontWeight fontWeight,
            D2dFontStyle fontStyle,
            float fontSize)
        {
            TextFormat txtFormat = new TextFormat(
                s_dwFactory,
                fontFamilyName,
                null,
                (FontWeight)fontWeight,
                (FontStyle)fontStyle,
                FontStretch.Normal,
                fontSize,
                "");
            return new D2dTextFormat(txtFormat);
        }

        /// <summary>
        /// Creates a text format with the specified parameter</summary>
        /// <param name="fontFamilyName">The name of the font family</param>
        /// <param name="fontWeight">A value that indicates the font weight</param>
        /// <param name="fontStyle">A value that indicates the font style</param>
        /// <param name="fontStretch">Font stretch compared to normal aspect ratio (not used)</param>
        /// <param name="fontSize">The logical size of the font in pixels</param>
        /// <param name="localeName">Locale name</param>
        /// <returns>A new instance of D2dTextFormat</returns>
        public static D2dTextFormat CreateTextFormat(
            string fontFamilyName,
            D2dFontWeight fontWeight,
            D2dFontStyle fontStyle,
            D2dFontStretch fontStretch,
            float fontSize,
            string localeName)
        {
            TextFormat txtFormat = new TextFormat(
                s_dwFactory,
                fontFamilyName,
                null,
                (FontWeight)fontWeight,
                (FontStyle)fontStyle,
                FontStretch.Normal,
                fontSize,
                localeName);
            return new D2dTextFormat(txtFormat);
        }

        /// <summary>
        /// Creates a text format object from GDI font and StringFormat.
        /// Text format is used for text layout with normal weight, style
        /// and stretch.</summary>        
        /// <param name="font">System.Drawing.Font used for creating TexFormat.</param>        
        /// <returns>A new instance of D2dTextFormat</returns>
        public static D2dTextFormat CreateTextFormat(System.Drawing.Font font)
        {

            float fontsize = (font.SizeInPoints * 96.0f) / 72.0f;

            FontWeight weight = FontWeight.Regular;
            if ((font.Style & GdiFontStyle.Bold) == GdiFontStyle.Bold)
            {
                weight = FontWeight.Bold;
            }

            FontStyle style = FontStyle.Normal;
            if ((font.Style & GdiFontStyle.Italic) == GdiFontStyle.Italic)
            {
                style = FontStyle.Italic;
            }

            TextFormat txtFormat = new TextFormat(s_dwFactory,
                font.FontFamily.Name,
                weight,
                style,
                fontsize);

            return new D2dTextFormat(txtFormat);

            // (font.SizeInPoints * 96.0f) / 72.0f )  convert font size from Point unit to DIP unit.

            // Relation between pixel, dip ,dpi, logical inch, and  point
            //  DIP  is (device independent pixel).
            //  DPI  is (dot per inch) in Windows is (pixel per logical inch).
            //   1 point = 1/72 of logical inch. 
            //   1  DIP   = 1/96 of logical inch.
            //   a logical inch can larger or smaller than physical inch.
            //   The length of a logical inch depends on pixel density of output device (screen).
            //   1 logical inch == 1 physical inch  if and only if  72 pixels == 1 physical inch.
        }


        /// <summary>
        /// Creates a TextLayout with the specified parameter</summary>
        /// <param name="text">The string to create a new D2dTextLayout object from</param>
        /// <param name="textFormat">The text format to apply to the string</param>
        /// <returns>A new instance of D2dTextLayout</returns>
        public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat)
        {
            return CreateTextLayout(text, textFormat, 2048, 2048);
        }

        /// <summary>
        /// Creates a TextLayout with the specified parameter</summary>
        /// <param name="text">The string to create a new D2dTextLayout object from</param>
        /// <param name="textFormat">The text format to apply to the string</param>
        /// <param name="layoutWidth">Text layout width</param>
        /// <param name="layoutHeight">Text layout height</param>
        /// <returns>A new instance of D2dTextLayout</returns>
        public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat, float layoutWidth, float layoutHeight)
        {
            var textlayout = new TextLayout(s_dwFactory, text, textFormat.NativeTextFormat, 
                layoutWidth, layoutHeight);
            return new D2dTextLayout(text, textlayout);
        }

        /// <summary>
        /// Creates a TextLayout with the specified parameter</summary>
        /// <param name="text">The string to create a new D2dTextLayout object from</param>
        /// <param name="textFormat">The text format to apply to the string</param>
        /// <param name="transform">2D Matrix used to transform the text</param>
        /// <returns>A new instance of D2dTextLayout</returns>
        public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat, Matrix3x2F transform)
        {
            return CreateTextLayout(text, textFormat, 2048, 2048, transform);
        }

        /// <summary>
        /// Creates a TextLayout with the specified parameter</summary>
        /// <param name="text">The string to create a new D2dTextLayout object from</param>
        /// <param name="textFormat">The text format to apply to the string</param>
        /// <param name="layoutWidth">Text layout width</param>
        /// <param name="layoutHeight">Text layout height</param>
        /// <param name="transform">2D Matrix used to transform the text</param>
        /// <returns>A new instance of D2dTextLayout</returns>
        public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat, float layoutWidth, float layoutHeight, Matrix3x2F transform)
        {
            var matrix = new Matrix3x2
            {
                M11 = transform.M11,
                M12 = transform.M12,
                M21 = transform.M21,
                M22 = transform.M22,
                M31 = transform.DX,
                M32 = transform.DY
            };

            var textlayout = new TextLayout(s_dwFactory, text, textFormat.NativeTextFormat, 
                layoutWidth, layoutHeight, 1, matrix, true);
            return new D2dTextLayout(text, textlayout);
        }

        /// <summary>
        /// Measures the specified string when drawn with the specified System.Drawing.Font</summary>
        /// <param name="text">String to measure</param>
        /// <param name="textFormat">D2dTextFormat that defines the font and format of the string</param>
        /// <returns>This method returns a System.Drawing.SizeF structure that represents the
        /// size, in DIP (Device Independent Pixels) units of the string specified by the text parameter as drawn 
        /// with the format parameter</returns>
        public static SizeF MeasureText(string text, D2dTextFormat textFormat)
        {
            if (string.IsNullOrEmpty(text))
                return SizeF.Empty;
            return s_gfx.MeasureText(text, textFormat);
        }

        /// <summary>
        /// Creates a new D2dSolidColorBrush that has the specified color.
        /// It is recommended to create D2dSolidBrushes and reuse them instead
        /// of creating and disposing them for each method call.</summary>
        /// <param name="color">The red, green, blue, and alpha values of the brush's color</param>
        /// <returns>A new D2d D2dSolidColorBrush</returns>
        public static D2dSolidColorBrush CreateSolidBrush(Color color)
        {
            return s_gfx.CreateSolidBrush(color);
        }

        /// <summary>
        /// Creates a D2dLinearGradientBrush with the specified points and colors</summary>        
        /// <param name="gradientStops">An array of D2dGradientStop structures that describe the colors in the brush's gradient 
        /// and their locations along the gradient line</param>
        /// <returns>A new instance of D2dLinearGradientBrush</returns>
        public static D2dLinearGradientBrush CreateLinearGradientBrush(params D2dGradientStop[] gradientStops)
        {
            return s_gfx.CreateLinearGradientBrush(gradientStops);
        }

        /// <summary>
        /// Creates a D2dLinearGradientBrush that contains the specified gradient stops,
        /// extend mode, and gamma interpolation. It has no transform and has a base opacity of 1.0.</summary>
        /// <param name="pt1">A System.Drawing.PointF structure that represents the starting point of the linear gradient</param>
        /// <param name="pt2">A System.Drawing.PointF structure that represents the endpoint of the linear gradient</param>
        /// <param name="gradientStops">An array of D2dGradientStop structures that describe the colors in the brush's gradient 
        /// and their locations along the gradient line</param>
        /// <param name="extendMode">The behavior of the gradient outside the [0,1] normalized range</param>
        /// <param name="gamma">The space in which color interpolation between the gradient stops is performed</param>
        /// <returns>A new instance of D2dLinearGradientBrush</returns>
        public static D2dLinearGradientBrush CreateLinearGradientBrush(
            PointF pt1,
            PointF pt2,
            D2dGradientStop[] gradientStops,
            D2dExtendMode extendMode,
            D2dGamma gamma)
        {
            return s_gfx.CreateLinearGradientBrush(pt1, pt2, gradientStops, extendMode, gamma);
        }

        /// <summary>
        /// Creates a D2dRadialGradientBrush that contains the specified gradient stops</summary>
        /// <param name="gradientStops">A array of D2dGradientStop structures that describe the
        /// colors in the brush's gradient and their locations along the gradient</param>
        /// <returns>A new instance of D2dRadialGradientBrush</returns>
        public static D2dRadialGradientBrush CreateRadialGradientBrush(params D2dGradientStop[] gradientStops)
        {
            return s_gfx.CreateRadialGradientBrush(
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
        public static D2dRadialGradientBrush CreateRadialGradientBrush(
            PointF center,
            PointF gradientOriginOffset,
            float radiusX,
            float radiusY,
            params D2dGradientStop[] gradientStops)
        {
            return s_gfx.CreateRadialGradientBrush(center, gradientOriginOffset, radiusX, radiusY, gradientStops);
        }

        /// <summary>
        /// Creates a new D2dBitmapBrush from the specified bitmap</summary>
        /// <param name="bitmap">The bitmap contents of the new brush</param>
        /// <returns>A new instance of D2dBitmapBrush</returns>
        public static D2dBitmapBrush CreateBitmapBrush(D2dBitmap bitmap)
        {
            return s_gfx.CreateBitmapBrush(bitmap);
        }

        /// <summary>
        /// Creates a new instance of the D2dBitmap class from a specified resource</summary>
        /// <param name="type">The type used to extract the resource</param>
        /// <param name="resource">The name of the resource</param>
        /// <returns>A new D2dBitmap</returns>
        public static D2dBitmap CreateBitmap(Type type, string resource)
        {
            return s_gfx.CreateBitmap(type, resource);
        }

        /// <summary>
        /// Creates a new instance of the D2dBitmap from the specified stream</summary>
        /// <param name="stream">The data stream used to load the image</param>
        /// <returns>A new D2dBitmap</returns>
        /// <remarks>It is safe to close the stream after the function call.</remarks>
        public static D2dBitmap CreateBitmap(System.IO.Stream stream)
        {
            return s_gfx.CreateBitmap(stream);
        }

        /// <summary>
        /// Creates a new instance of the D2dBitmap from the specified file</summary>
        /// <param name="filename">The name of the bitmap file</param>
        /// <returns>A new D2dBitmap</returns>
        public static D2dBitmap CreateBitmap(string filename)
        {
            return s_gfx.CreateBitmap(filename);
        }


        /// <summary>
        /// Creates a Direct2D Bitmap by copying the specified GDI Image</summary>
        /// <param name="img">A GDI bitmap from which to create new D2dBitmap</param>
        /// <returns>A new D2dBitmap</returns>
        public static D2dBitmap CreateBitmap(System.Drawing.Image img)
        {
            return s_gfx.CreateBitmap(img);
        }

        /// <summary>
        /// Creates a Direct2D Bitmap by copying the specified GDI bitmap</summary>
        /// <param name="bmp">A GDI bitmap from which to create new D2dBitmap</param>
        /// <returns>A new D2dBitmap</returns>
        public static D2dBitmap CreateBitmap(System.Drawing.Bitmap bmp)
        {
            return s_gfx.CreateBitmap(bmp);
        }

        /// <summary>
        /// Creates an empty Direct2D Bitmap from specified width and height
        /// Pixel format is set to 32 bit ARGB with premultiplied alpha</summary>      
        /// <param name="width">Width of the bitmap in pixels</param>
        /// <param name="height">Height of the bitmap in pixels</param>
        /// <returns>A new D2dBitmap</returns>
        public static D2dBitmap CreateBitmap(int width, int height)
        {
            return s_gfx.CreateBitmap(width, height);
        }

        /// <summary>
        /// Retrieves the current desktop dots per inch (DPI). To refresh this value,
        /// Call ReloadSystemMetrics.</summary>
        /// <remarks>
        /// Use this method to obtain the system DPI when setting physical pixel values,
        /// such as when you specify the size of a window.</remarks>
        public static SizeF DesktopDpi
        {
            get { return new SizeF(s_d2dFactory.DesktopDpi.Width, s_d2dFactory.DesktopDpi.Height); }
        }

        /// <summary>
        /// Converts value in points to pixels</summary>
        /// <param name="point">Points value</param>
        /// <returns>Value in pixels</returns>
        public static float FontSizeToPixel(float point)
        {
            // convert point to logical inch.
            float lg = point / 72.0f;

            // convert logical inch to pixel
            float pixel = lg * s_d2dFactory.DesktopDpi.Width;
            return pixel;
        }

        /// <summary>
        /// Forces the factory to refresh any system defaults that it might have changed
        /// since factory creation</summary>
        /// <remarks>
        /// You should call this method before calling the {{GetDesktopDpi}} method
        /// to ensure that the system DPI is current.</remarks>
        public static void ReloadSystemMetrics()
        {
            s_d2dFactory.ReloadSystemMetrics();
        }

        /// <summary>
        /// Gets the render target number for the underlining D2dGraphics.</summary>
        public static uint RenderTargetNumber
        {
            get { return s_gfx != null ? s_gfx.RenderTargetNumber : 0; }
        }

        private static bool m_checking;
        internal static void CheckForRecreateTarget()
        {
            if (m_checking) return;
            try
            {
                m_checking = true;
                if (s_gfx != null)
                {
                    s_gfx.BeginDraw();
                    s_gfx.EndDraw();
                }
            }
            finally
            {
                m_checking = false;
            }
        }


        internal static SharpDX.DirectWrite.Factory NativeDwFactory
        {
            get { return s_dwFactory; }
        }

        internal static SharpDX.Direct2D1.Factory NativeFactory
        {
            get { return s_d2dFactory; }
        }

        internal static SharpDX.WIC.ImagingFactory NativeWicFactory
        {
            get { return s_wicFactory; }
        }

        internal static RenderTargetProperties RenderTargetProperties
        {
            get { return s_rtprops; }
        }

        static D2dFactory()
        {
            if (Environment.OSVersion.Version.Major < 6)
                throw new Exception("Direct2D requires Windows Vista or newer");
            s_d2dFactory = new SharpDX.Direct2D1.Factory();
            s_dwFactory = new SharpDX.DirectWrite.Factory();
            s_wicFactory = new SharpDX.WIC.ImagingFactory();
        }

        private static readonly SharpDX.Direct2D1.Factory s_d2dFactory;
        private static readonly SharpDX.DirectWrite.Factory s_dwFactory;
        private static readonly SharpDX.WIC.ImagingFactory s_wicFactory;
        private static D2dHwndGraphics s_gfx;

        // the first render target property
        private static RenderTargetProperties s_rtprops = new RenderTargetProperties();
    }

    /// <summary>
    /// Result of Direct2D drawing operation</summary>
    public struct D2dResult : IEquatable<D2dResult>
    {
        /// <summary>Operation succeeded</summary>
        public readonly static D2dResult Ok;
        /// <summary>Operation was aborted</summary>
        public readonly static D2dResult Abord;
        /// <summary>Operation was denied access</summary>
        public readonly static D2dResult AccessDenied;
        /// <summary>Operation failed</summary>
        public readonly static D2dResult Fail;
        /// <summary>Operation result is handle</summary>
        public readonly static D2dResult Handle;
        /// <summary>Operation had invalid argument</summary>
        public static D2dResult InvalidArg;
        /// <summary>Operation has no interface</summary>
        public static D2dResult NoInterface;
        /// <summary>Operation is not implemented</summary>
        public static D2dResult NotImplemented;
        /// <summary>Operation ran out of memory</summary>
        public static D2dResult OutOfMemory;
        /// <summary>Operation had invalid pointer</summary>
        public static D2dResult InvalidPointer;
        /// <summary>Operation had an unexpected failure</summary>
        public static D2dResult UnexpectedFailure;

        internal D2dResult(int code)
        {
            m_code = code;
        }

        internal D2dResult(uint code)
        {
            m_code = (int)code;
        }

        /// <summary>
        /// Gets results code</summary>
        public int Code
        {
            get { return m_code; }
        }

        /// <summary>
        /// Gets whether factory succeeded in creating object</summary>
        public bool IsSuccess
        {
            get { return (this.Code >= 0); }
        }

        /// <summary>
        /// Gets whether factory failed in creating object</summary>
        public bool IsFailure
        {
            get { return (this.Code < 0); }
        }

        /// <summary>
        /// Gets whether last result code is same as input</summary>
        /// <param name="other">Other result code</param>
        /// <returns>True iff result codes are identical</returns>
        public bool Equals(D2dResult other)
        {
            return (this.Code == other.Code);
        }

        /// <summary>
        /// Gets whether last result code is same as input object</summary>
        /// <param name="obj">Other result code object</param>
        /// <returns>True iff result codes are identical</returns>
        public override bool Equals(object obj)
        {
            return ((obj is D2dResult) && this.Equals((D2dResult)obj));
        }

        /// <summary>
        /// Gets this objects hash code</summary>
        /// <returns>Hash code for object</returns>
        public override int GetHashCode()
        {
            return this.Code;
        }

        /// <summary>
        /// Result code equality operator</summary>
        /// <param name="left">Result code 1</param>
        /// <param name="right">Result code 2</param>
        /// <returns>True iff result codes are identical</returns>
        public static bool operator ==(D2dResult left, D2dResult right)
        {
            return (left.Code == right.Code);
        }

        /// <summary>
        /// Result code inequality operator</summary>
        /// <param name="left">Result code 1</param>
        /// <param name="right">Result code 2</param>
        /// <returns>True iff result codes are not identical</returns>
        public static bool operator !=(D2dResult left, D2dResult right)
        {
            return (left.Code != right.Code);
        }

        /// <summary>
        /// Provides string representation of result code</summary>
        /// <returns>String representation of result code</returns>
        public override string ToString()
        {
            // With SharpDX 2.3.1, I can't find this method anywhere. The docs say that everything
            //  in the SharpDX.Diagnostics dll got moved to SharpDX.dll, but it's not there. --Ron
            //return SharpDX.Diagnostics.ErrorManager.GetErrorMessage(Code);

            //return string.Format("result = 0x{0:X}", m_code);

            switch (m_code)
            {
                case 0: return "OK";
                case -2147467260: return "Abord? Is that supposed to be Abort?";
                case -2147024891: return "Access denied";
                case -2147467259: return "Fail";
                case -2147024890: return "Handle";
                case -2147024809: return "Invalid argument";
                case -2147467262: return "No interface";
                case -2147467263: return "Not implemented";
                case -2147024882: return "Out of memory";
                case -2147467261: return "Invalid pointer";
                case -2147418113: return "Unexpected failure";
            }

            return "Unknown error code".Localize();
        }

        static D2dResult()
        {
            Ok = new D2dResult(0);
            Abord = new D2dResult(-2147467260);
            AccessDenied = new D2dResult(-2147024891);
            Fail = new D2dResult(-2147467259);
            Handle = new D2dResult(-2147024890);
            InvalidArg = new D2dResult(-2147024809);
            NoInterface = new D2dResult(-2147467262);
            NotImplemented = new D2dResult(-2147467263);
            OutOfMemory = new D2dResult(-2147024882);
            InvalidPointer = new D2dResult(-2147467261);
            UnexpectedFailure = new D2dResult(-2147418113);
        }

        private readonly int m_code;
    }

}
