//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;

namespace Sce.Atf
{
    /// <summary>
    /// BITMAP structure to interoperate with Gdi32.dll</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAP
    {
        public long bmType;
        public long bmWidth;
        public long bmHeight;
        public long bmWidthBytes;
        public short bmPlanes;
        public short bmBitsPixel;
        public IntPtr bmBits;
    }

    /// <summary>
    /// BITMAPINFO_FLAT structure to interoperate with Gdi32.dll</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BITMAPINFO_FLAT
    {
        public int biSize;
        public int biWidth;
        public int biHeight;
        public short biPlanes;
        public short biBitCount;
        public int biCompression;
        public int biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public int biClrUsed;
        public int biClrImportant;
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] bmiColors;
    }

    /// <summary>
    /// BITMAPINFOHEADER class to interoperate with Gdi32.dll</summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BITMAPINFOHEADER 
    {
        public int      biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
        public int      biWidth;
        public int      biHeight;
        public short    biPlanes;
        public short    biBitCount;
        public int      biCompression;
        public int      biSizeImage;
        public int      biXPelsPerMeter;
        public int      biYPelsPerMeter;
        public int      biClrUsed;
        public int      biClrImportant;
    }

    /// <summary>
    /// BITMAPINFO class to interoperate with Gdi32.dll</summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BITMAPINFO 
    {
        public BITMAPINFOHEADER bmiHeader = new BITMAPINFOHEADER();
        [MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=1024)]
        public byte[] bmiColors; 
    }

    /// <summary>
    /// Gdi32 PInvoke wrappers</summary>
    public static class Gdi32
    {
        [DllImport("gdi32.dll")]
        static public extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        static public extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int Width, int Heigth);
        [DllImport("gdi32.dll")]
        static public extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        [DllImport("gdi32.dll")]
        static public extern IntPtr DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        static public extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Enum for CreateDIBSection -- from WinGDI.h</summary>
        public enum DIBUsage
        {
            RGB = 0,    //DIB_RGB_COLORS = 0; // color table contains RGBs
            Palette = 1 //DIB_PAL_COLORS = 1; // color table contains palette indices
        }

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO_FLAT bmi,
            DIBUsage usage, ref int ppvBits, IntPtr hSection, int dwOffset);

        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(IntPtr hDC, IntPtr hbm, int StartScan, int ScanLines, int lpBits, BITMAPINFOHEADER bmi, int usage);
        [DllImport("gdi32.dll")]
        public static extern int GetDIBits(IntPtr hdc, IntPtr hbm, int StartScan, int ScanLines, int lpBits, ref BITMAPINFO_FLAT bmi, int usage);

        public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
            int nWidth, int nHeight, IntPtr hObjectSource,
            int nXSrc, int nYSrc, int dwRop);
    }
}