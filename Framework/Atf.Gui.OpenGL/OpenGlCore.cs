//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;


using OpenTK;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// Useful OpenGL Utilities and Extension manager</summary>
    public static class OpenGlCore
    {
        /// <summary>
        /// Gets an array of supported extension names</summary>
        public static string[] Extensions
        {
            get { return s_extensions; }
        }

        /// <summary>
        /// Gets a value indicating if vertex buffers are supported</summary>
        public static bool VertexBuffersSupported
        {
            get { return s_vertexBuffersSupported; }
        }

        /// <summary>
        /// Gets a value indicating if DDS textures are supported</summary>
        public static bool DdsTexturesSupported
        {
            get { return s_ddsTexturesSupported; }
        }

        /// <summary>
        /// Gets a value indicating if FBO is supported</summary>
        public static bool FrameBufferObjectSupported
        {
            get { return s_frameBufferObjectSupported; }
        }

        /// <summary>
        /// Indicates the OpenGL display list ID of the first font bitmap. Use Util3D.DrawText() to
        /// actually draw strings, so that the mapping of Unicode code points to display list IDs
        /// is correct.</summary>
        public const int TEXT_DISPLAY_LIST_BASE = 1000;

        /// <summary>
        /// Initializes an OpenGL context with the associated Win32 device context</summary>
        /// <param name="hdc">HDC from the window to which the new OpenGL context is bound</param>
        /// <param name="hglrc">Handle to the new OpenGL context</param>
        public static void InitOpenGl(GLControl control, out IntPtr hglrc)
        {
            control.MakeCurrent();

            // Attempt To Get The Rendering Context
            hglrc = control.WindowInfo.Handle;
            if (hglrc == IntPtr.Zero)
                throw new InvalidOperationException("Can't create GL rendering context");

            if (s_sharedHglrc == IntPtr.Zero)
                s_sharedHglrc = hglrc;
            else
            {
                throw new Exception("s_sharedHglrc is not IntPtr.Zero");
            }

            if (!control.Context.IsCurrent)
                throw new InvalidOperationException("Can't make the OpenGL rendering context to be the current context.");

            // Load all extensions for mainline rendering
            if (!s_initialized)
            {
                LoadAllExtensions();
                s_initialized = true;
            }

            // Init fonts
            //using (Font defaultFont = SystemFonts.DefaultFont)
            //{
            //    Gdi.SelectObject(hdc, defaultFont.ToHfont());
            //}
            //foreach (IntSet.Range range in s_fontMap.Ranges)
            //{
            //    int baseDisplayListId = range.PreviousItemsCount + TEXT_DISPLAY_LIST_BASE;

            //    if (!wglUseFontBitmaps(hdc, range.Min, range.Count, (uint)baseDisplayListId))
            //        throw new InvalidOperationException("Font bitmaps were unable to be created.");
            //}
            s_fontMap.Lock();

            Util3D.ReportErrors();
        }

        /// <summary>
        /// Gets and sets a set of integers to define the ranges of Unicode code points,
        /// also known as Unicode scalar values, which need to be used with OpenGL.
        /// These IDs are mapped to [0, # of OpenGL font bitmaps minus 1].
        /// Before OpenGL is initialized: 'get' and 'set' can be called and the object can be modified.
        /// After OpenGL is initialized: only 'get' can be called and the object cannnot be modified.</summary>
        public static IntSet UnicodeMapper
        {
            get { return s_fontMap; }
            set
            {
                if (s_initialized)
                    throw new InvalidOperationException("OpenGl has already been initialized");
                s_fontMap = value;
            }
        }

        /// <summary>
        /// The character to display when the DrawText method finds an unsupported character that
        /// it can't draw because it wasn't added to UnicodeMapper</summary>
        public static char UnknownChar = '?';

        /// <summary>
        /// Low-level text drawing routine. Caller must set up OpenGL transforms, colors, etc.
        /// Draws the text, using the private knowledge about how to map Unicode code points
        /// to particular OpenGL display list IDs. Consider using the more convenient
        /// Util3D.DrawText(). Newline characters appear as spaces. Unknown characters appear as '?'
        /// by default. See UnknownChar. See also the UnicodeMapper property.</summary>
        /// <param name="text">Text string to draw</param>
        public static void DrawText(string text)
        {
            OpenTK.Graphics.OpenGL.GL.ListBase(TEXT_DISPLAY_LIST_BASE);

            IList<int> codePoints = StringUtil.GetUnicodeCodePoints(text);
            ushort[] aMsg = new ushort[codePoints.Count];
            for (int i = 0; i < codePoints.Count; i++)
            {
                // First try to get the code point, then try for the UnknownChar. If both those fail,
                //  then aMsg[i] will be zero and so the first font display list will be drawn.
                int displayListIdOffset;
                if (s_fontMap.Contains(codePoints[i], out displayListIdOffset) ||
                    s_fontMap.Contains((int)UnknownChar, out displayListIdOffset))
                {
                    aMsg[i] = (ushort)displayListIdOffset;
                }
            }
            OpenTK.Graphics.OpenGL.GL.CallLists(aMsg.Length, OpenTK.Graphics.OpenGL.ListNameType.UnsignedShort, aMsg);
        }

        /// <summary>
        /// Destroys the provided OpenGL context</summary>
        /// <param name="hglrc">OpenGL render context that was initialized in InitOpenGl</param>
        public static void ShutdownOpenGl(GLControl control,ref IntPtr hglrc)
        {
            //if (hglrc != IntPtr.Zero)
            //{
            //    Wgl.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
            //    Wgl.wglDeleteContext(hglrc);
            //    hglrc = IntPtr.Zero;
            //}

            if(hglrc != IntPtr.Zero)
            {
                control.Context.MakeCurrent(null);
                control.Dispose();
                hglrc = IntPtr.Zero;
            }
        }
        
        //private static void PopulatePixelFormatDescriptor(ref Gdi.PIXELFORMATDESCRIPTOR pfd)
        //{
        //    pfd.nSize = (short)Marshal.SizeOf(pfd);
        //    pfd.nVersion = 1;
        //    pfd.dwFlags = Gdi.PFD_DRAW_TO_WINDOW |
        //        Gdi.PFD_SUPPORT_OPENGL |
        //        Gdi.PFD_DOUBLEBUFFER;
        //    pfd.iPixelType = (byte)Gdi.PFD_TYPE_RGBA;
        //    pfd.cColorBits = (byte)32;
        //    pfd.cRedBits = 0;
        //    pfd.cRedShift = 0;
        //    pfd.cGreenBits = 0;
        //    pfd.cGreenShift = 0;
        //    pfd.cBlueBits = 0;
        //    pfd.cBlueShift = 0;
        //    pfd.cAlphaBits = 0;
        //    pfd.cAlphaShift = 0;
        //    pfd.cAccumBits = 0;
        //    pfd.cAccumRedBits = 0;
        //    pfd.cAccumGreenBits = 0;
        //    pfd.cAccumBlueBits = 0;
        //    pfd.cAccumAlphaBits = 0;
        //    pfd.cDepthBits = 32;
        //    pfd.cStencilBits = 0;
        //    pfd.cAuxBuffers = 0;
        //    pfd.iLayerType = (byte)Gdi.PFD_MAIN_PLANE;
        //    pfd.bReserved = 0;
        //    pfd.dwLayerMask = 0;
        //    pfd.dwVisibleMask = 0;
        //    pfd.dwDamageMask = 0;
        //}

        private static void LoadAllExtensions()
        {
            string extensionsString = OpenTK.Graphics.OpenGL.GL.GetString(OpenTK.Graphics.OpenGL.StringName.Extensions);
            string[] extensions = extensionsString.Split(' ');
            s_extensions = extensions;

            Array.Sort(extensions);

            s_vertexBuffersSupported = (Array.BinarySearch(s_extensions, "GL_ARB_vertex_buffer_object") >= 0);
            s_ddsTexturesSupported = (Array.BinarySearch(s_extensions, "GL_EXT_texture_compression_s3tc") >= 0);
            s_frameBufferObjectSupported = (Array.BinarySearch(s_extensions, "GL_EXT_framebuffer_object") >= 0);
        }

        /// <summary>
        /// Constructor</summary>
        /// <remarks>As a convenience for client code, preset the supported Unicode characters according
        /// to the computer's current culture setting. This can be overridden by the
        /// UnicodeMapper property.
        /// For 2-character language codes in ISO 639-1: http://www.loc.gov/standards/iso639-2/php/code_list.php .
        /// For unicode ranges: http://en.wikipedia.org/wiki/Summary_of_Unicode_character_assignments .
        /// The maximum possible OpenGl display list ID is ushort.MaxValue.</remarks>
        static OpenGlCore()
        {
            s_fontMap = new IntSet();

            //add ASCII, Basic Latin, and Latin-1 Supplement
            s_fontMap.AddRange(0x0000, 0x00FF);

            //add additional glyphs for some of our client languages
            string languageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            switch (languageCode)
            {
                case "ja"://Japan
                    s_fontMap.AddRange(0x30A0, 0x30FF);//Katakana
                    break;
                case "nl"://Netherlands (Dutch, Flemis)
                    s_fontMap.AddRange(0x0100, 0x024F);//Latin Extended-A, Latin Extended-B
                    s_fontMap.AddRange(0x1E00, 0x1EFF);//Latin Extended Additional
                    break;
                default:
                    break;
            }
        }

        private static IntPtr s_sharedHglrc = IntPtr.Zero;
        private static bool s_initialized = false;
        private static string[] s_extensions;
        private static bool s_vertexBuffersSupported;
        private static bool s_ddsTexturesSupported;
        private static bool s_frameBufferObjectSupported;
        private static IntSet s_fontMap;
    }
}
