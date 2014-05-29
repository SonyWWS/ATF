//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.DragDrop
{
    using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

    /// <summary>
    /// Provides extended functionality for the IDataObject interface during drag/drop actions.</summary>
    public static class DragDataHelper
    {
        /// <summary>
        /// Call this method in the OnDragEnter if you wish to accept Shell Image drag/drop actions</summary>
        /// <param name="control">The control receiving the OnDragEnter event</param>
        /// <param name="e">The DragEventArgs from the OnDragEnter event</param>
        public static void DragEnter(Control control, DragEventArgs e)
        {
            var point = new Point(e.X, e.Y);
            DragDropHelper.DragEnter(control.Handle, (IComDataObject)e.Data, ref point, (int)e.Effect);
        }

        /// <summary>
        /// Call this method in the OnDragLeave if you wish to accept Shell Image drag/drop actions</summary>
        public static void DragLeave()
        {
            DragDropHelper.DragLeave();
        }

        /// <summary>
        /// Call this method in the OnDragOver if you wish to accept Shell Image drag/drop actions</summary>
        /// <param name="e">The DragEventArgs from the OnDragOver event</param>
        public static void DragOver(DragEventArgs e)
        {
            var point = new Point(e.X, e.Y);
            DragDropHelper.DragOver(ref point, (int)e.Effect);
        }

        /// <summary>
        /// Call this method in the OnDrop if you wish to accept Shell Image drag/drop actions</summary>
        /// <param name="e">The DragEventArgs from the OnDrop event</param>
        public static void Drop(DragEventArgs e)
        {
            var point = new Point(e.X, e.Y);
            DragDropHelper.Drop((IComDataObject)e.Data, ref point, (int)e.Effect);
        }

        /// <summary>
        /// Set the description on a Shell Image drag/drop action.
        /// Some UI coloring is applied to the text in <paramref name="insert"/> if used by specifying %1 
        /// in <paramref name="message"/>. 
        /// The characters %% and %1 are the subset of FormatMessage markers that are processed here.</summary>
        /// <param name="e">The DragEventArgs from the OnDragEnter/OnDragOver event</param>
        /// <param name="message">Text such as "Move to %1"</param>
        /// <param name="insert">Text such as "Documents", inserted as specified by <paramref name="message"/></param>
        public static void SetDescription(this DragEventArgs e, string message, string insert)
        {
            DropDescriptionHelper.SetDropDescription((IComDataObject)e.Data, (DropImageType)e.Effect, message, insert);
        }

        /// <summary>
        /// Set the description on a Shell Image drag/drop action</summary>
        /// <param name="e">DragEventArgs for event</param>
        public static void ClearDescription(this DragEventArgs e)
        {
            DropDescriptionHelper.SetDropDescription((IComDataObject)e.Data, DropImageType.Invalid, null, null);
        }

        /// <summary>
        /// Tell the system that you are using a default description that can be cleared automatically</summary>
        /// <param name="e">The DragEventArgs from the OnDragEnter/OnDragOver event</param>
        /// <param name="value">A boolean indicating whether the message is set as default or not</param>
        public static void SetDescriptionIsDefault(this DragEventArgs e, bool value)
        {
            DropDescriptionHelper.SetDropDescriptionIsDefault((IComDataObject)e.Data, value);
        }

        /// <summary>
        /// Marshal a struct as raw data into the IDataObject</summary>
        /// <typeparam name="T">Type of data to marshal</typeparam>
        /// <param name="dataObject">The IDataObject to manipulate.</param>
        /// <param name="format">The format identifier to use</param>
        /// <param name="data">The actual data</param>
        public static void SetData<T>(this IDataObject dataObject, string format, T data)
            where T : struct
        {
            ((IComDataObject)dataObject).SetData(format, data);
        }

        /// <summary>
        /// Try to marshal a struct as raw data from the IDataObject</summary>
        /// <typeparam name="T">Type of data to marshal</typeparam>
        /// <param name="dataObject">The IDataObject to manipulate</param>
        /// <param name="format">The format identifier to use</param>
        /// <param name="data">Actual data</param>
        /// <returns>True iff data successfully retrieved</returns>
        public static bool TryGetData<T>(this IDataObject dataObject, string format, out T data)
            where T : struct
        {
            return ((IComDataObject)dataObject).TryGetData(format, out data);
        }
    }
}
