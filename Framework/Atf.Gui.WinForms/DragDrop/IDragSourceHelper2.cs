//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Sce.Atf
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ShDragImage
    {
        public Size sizeDragImage;
        public Point ptOffset;
        public IntPtr hbmpDragImage;
        public int crColorKey;
    }

    [ComVisible(true)]
    [ComImport]
    [Guid("83E07D0D-0C5F-4163-BF1A-60B274051E40")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDragSourceHelper2
    {
        void InitializeFromBitmap(
            [In, MarshalAs(UnmanagedType.Struct)] ref ShDragImage dragImage,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

        void InitializeFromWindow(
            [In] IntPtr hwnd,
            [In] ref Point pt,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject);

        void SetFlags(
            [In] int dwFlags);
    }
}