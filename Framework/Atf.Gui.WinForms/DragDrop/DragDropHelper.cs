//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Sce.Atf
{
    /// <summary>
    /// Provides a wrapper fror the CLSID_DragDropHelper class found in shell32.dll
    /// for internal use only.
    /// </summary>
    internal static class DragDropHelper
    {
        [ComImport]
        [Guid("4657278A-411B-11d2-839A-00C04FD918D0")]
        private class ComHelper { }

        private static readonly ComHelper s_helper = new ComHelper();

        #region -- IDragSourceHelper2 -----------------------------------------

        public static void InitializeFromBitmap(IDataObject dataObject, Bitmap image, Point offset)
        {
            var hbmp = image.GetHbitmap();

            var shdi = new ShDragImage
            {
                sizeDragImage = image.Size,
                ptOffset = offset,
                crColorKey = Color.Magenta.ToArgb(),
                hbmpDragImage = hbmp
            };

            try
            {
                var ds = (IDragSourceHelper2)s_helper;
                ds.InitializeFromBitmap(ref shdi, dataObject);
            }
            catch
            {
                DeleteObject(hbmp);
            }
        }

        public static void InitializeFromWindow(IntPtr hwnd, ref Point pt, IDataObject dataObject)
        {
            var ds = (IDragSourceHelper2)s_helper;
            ds.InitializeFromWindow(hwnd, ref pt, dataObject);
        }

        public static void SetFlags(int dwFlags)
        {
            var ds = (IDragSourceHelper2)s_helper;
            ds.SetFlags(dwFlags);
        }

        [DllImport("gdiplus.dll")]
        private static extern bool DeleteObject(IntPtr hgdi);

        #endregion

        #region -- IDropTargetHelper ------------------------------------------

        public static void DragEnter(IntPtr hwndTarget, IDataObject dataObject, ref Point pt, int effect)
        {
            var dt = (IDropTargetHelper)s_helper;
            dt.DragEnter(hwndTarget, dataObject, ref pt, effect);
        }

        public static void DragLeave()
        {
            var dt = (IDropTargetHelper)s_helper;
            dt.DragLeave();
        }

        public static void DragOver(ref Point pt, int effect)
        {
            var dt = (IDropTargetHelper)s_helper;
            dt.DragOver(ref pt, effect);
        }

        public static void Drop(IDataObject dataObject, ref Point pt, int effect)
        {
            var dt = (IDropTargetHelper)s_helper;
            dt.Drop(dataObject, ref pt, effect);
        }

        public static void Show(bool show)
        {
            var dt = (IDropTargetHelper)s_helper;
            dt.Show(show);
        }

        #endregion
    }
}