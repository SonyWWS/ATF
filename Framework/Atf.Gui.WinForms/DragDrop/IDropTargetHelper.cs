//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Sce.Atf.DragDrop
{
    [ComVisible(true)]
    [ComImport]
    [Guid("4657278B-411B-11D2-839A-00C04FD918D0")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDropTargetHelper
    {
        void DragEnter(
            [In] IntPtr hwndTarget,
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
            [In] ref Point pt,
            [In] int effect);

        void DragLeave();

        void DragOver(
            [In] ref Point pt,
            [In] int effect);

        void Drop(
            [In, MarshalAs(UnmanagedType.Interface)] IDataObject dataObject,
            [In] ref Point pt,
            [In] int effect);

        void Show(
            [In] bool show);
    }
}