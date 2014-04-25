//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sce.Atf.DragDrop
{
    using IComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

    /// <summary>
    /// Provides helper methods for working with the Shell drag image manager.
    /// </summary>
    internal static class DropDescriptionHelper
    {
        /// <summary>
        /// Internally used to track information about the current drop description.
        /// </summary>
        [Flags]
        private enum DropDescriptionFlags
        {
            None = 0,
            IsDefault = 1,
            InvalidateRequired = 2
        }

        /// <summary>
        /// Checks if the IsDefault drop description flag is set for the associated DataObject.
        /// </summary>
        /// <param name="dataObject">The associated DataObject.</param>
        /// <returns>True if the IsDefault flag is set, otherwise False.</returns>
        public static bool IsDropDescriptionDefault(IComDataObject dataObject)
        {
            var current = GetDropDescriptionFlag(dataObject);
            return (current & DropDescriptionFlags.IsDefault) == DropDescriptionFlags.IsDefault;
        }

        /// <summary>
        /// Checks if the InvalidateRequired drop description flag is set for the associated DataObject.
        /// </summary>
        /// <param name="dataObject">The associated DataObject.</param>
        /// <returns>True if the InvalidateRequired flag is set, otherwise False.</returns>
        public static bool InvalidateRequired(IComDataObject dataObject)
        {
            var current = GetDropDescriptionFlag(dataObject);
            return (current & DropDescriptionFlags.InvalidateRequired) == DropDescriptionFlags.InvalidateRequired;
        }

        /// <summary>
        /// Sets the IsDefault drop description flag for the associated DataObject.
        /// </summary>
        /// <param name="dataObject">The associdated DataObject.</param>
        /// <param name="isDefault">True to set the flag, False to unset it.</param>
        public static void SetDropDescriptionIsDefault(IComDataObject dataObject, bool isDefault)
        {
            SetDropDescriptionFlag(dataObject, DropDescriptionFlags.IsDefault, isDefault);
        }

        /// <summary>
        /// Sets the InvalidatedRequired drop description flag for the associated DataObject.
        /// </summary>
        /// <param name="dataObject">The associdated DataObject.</param>
        /// <param name="required">True to set the flag, False to unset it.</param>
        public static void SetInvalidateRequired(IComDataObject dataObject, bool required)
        {
            SetDropDescriptionFlag(dataObject, DropDescriptionFlags.InvalidateRequired, required);
        }

        /// <summary>
        /// Sets a drop description flag.
        /// </summary>
        /// <param name="dataObject">The associated DataObject.</param>
        /// <param name="flag">The drop description flag to set.</param>
        /// <param name="enabled">True to set the flag, False to unset it.</param>
        private static void SetDropDescriptionFlag(IComDataObject dataObject, DropDescriptionFlags flag, bool enabled)
        {
            var current = GetDropDescriptionFlag(dataObject);
            var next = enabled ? (current | flag) : (current | flag) ^ flag;
            if (current != next)
            {
                dataObject.SetData("DropDescriptionFlags", (int)next);
            }
        }

        /// <summary>
        /// Gets a drop description flag.
        /// </summary>
        private static DropDescriptionFlags GetDropDescriptionFlag(IComDataObject dataObject)
        {
            int flag;
            if (dataObject.TryGetData("DropDescriptionFlags", out flag))
            {
                return (DropDescriptionFlags)flag;
            }
            return DropDescriptionFlags.None;
        }

        /// <summary>
        /// Sets a drop description.
        /// </summary>
        public static void SetDropDescription(IComDataObject data, DropImageType dragDropEffects, string format, string insert)
        {
            if (format != null && format.Length > 259)
                throw new ArgumentException("Format string exceeds the maximum allowed length of 259.", "format");
            if (insert != null && insert.Length > 259)
                throw new ArgumentException("Insert string exceeds the maximum allowed length of 259.", "insert");

            // Fill the structure
            DropDescription dd;
            dd.type = dragDropEffects;
            dd.szMessage = format;
            dd.szInsert = insert;

            data.SetData("DropDescription", dd);
        }

        /// <summary>
        /// Gets a drop description.
        /// </summary>
        public static object GetDropDescription(IComDataObject dataObject)
        {
            // CFSTR_DROPDESCRIPTION
            DropDescription desc;
            if (dataObject.TryGetData("DropDescription", out desc))
            {
                return desc;
            }
            return null;
        }

        /// <summary>
        /// Gets the image type from a drop description.
        /// </summary>
        public static DropImageType GetDropImageType(IComDataObject dataObject)
        {
            var data = GetDropDescription(dataObject);
            return (data is DropDescription) ? ((DropDescription)data).type : DropImageType.Invalid;
        }

        /// <summary>
        /// Check if the description is valid.
        /// </summary>
        public static bool IsDropDescriptionValid(IComDataObject dataObject)
        {
            return GetDropImageType(dataObject) != DropImageType.Invalid;
        }

        /// <summary>
        /// Default feedback handler.
        /// </summary>
        public static void DefaultGiveFeedback(IComDataObject data, GiveFeedbackEventArgs e)
        {
            // For drop targets that don't set the drop description, we'll
            // set a default one. Drop targets that do set drop descriptions
            // should set an invalid drop description during DragLeave.
            var setDefaultDropDesc = false;
            var isDefaultDropDesc = IsDropDescriptionDefault(data);
            var currentType = DropImageType.Invalid;
            if (!IsDropDescriptionValid(data) || isDefaultDropDesc)
            {
                currentType = GetDropImageType(data);
                setDefaultDropDesc = true;
            }

            if (IsShowingLayered(data))
            {
                // The default drag source implementation uses drop descriptions,
                // so we won't use default cursors.
                e.UseDefaultCursors = false;
                Cursor.Current = Cursors.Default;
            }
            else
            {
                e.UseDefaultCursors = true;
            }

            // We need to invalidate the drag image to refresh the drop description.
            // This is tricky to implement correctly, but we try to mimic the Windows
            // Explorer behavior. We internally use a flag to tell us to invalidate
            // the drag image, so if that is set, we'll invalidate. Otherwise, we
            // always invalidate if the drop description was set by the drop target,
            // *or* if the current drop image is not None. So if we set a default
            // drop description to anything but None, we'll always invalidate.
            if (InvalidateRequired(data) || !isDefaultDropDesc || currentType != DropImageType.None)
            {
                IntPtr hwnd;
                if (data.TryGetData("DragWindow", out hwnd))
                {
                    const uint WM_INVALIDATEDRAGIMAGE = 0x403;
                    PostMessage(hwnd, WM_INVALIDATEDRAGIMAGE, IntPtr.Zero, IntPtr.Zero);
                }

                // The invalidate required flag only lasts for one invalidation
                SetInvalidateRequired(data, false);
            }

            // If the drop description is currently invalid, or if it is a default
            // drop description already, we should check about re-setting it.
            if (setDefaultDropDesc && (DropImageType)e.Effect != currentType)
            {
                if (e.Effect != DragDropEffects.None)
                {
                    SetDropDescription(data, (DropImageType)e.Effect, e.Effect.ToString(), null);
                }
                else
                {
                    SetDropDescription(data, (DropImageType)e.Effect, e.Effect.ToString(), null);
                }

                SetDropDescriptionIsDefault(data, true);

                // We can't invalidate now, because the drag image manager won't
                // pick it up... so we set this flag to invalidate on the next
                // GiveFeedback event.
                SetInvalidateRequired(data, true);
            }
        }

        // The drag image manager sets this flag to indicate if the current
        // drop target supports drag images.
        private static bool IsShowingLayered(IComDataObject dataObject)
        {
            int value;
            if (dataObject.TryGetData("IsShowingLayered", out value))
            {
                return (value != 0);
            }
            return false;
        }

        #region -- DLL imports ------------------------------------------------

        [DllImport("user32.dll")]
        private static extern void PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        #endregion
    }
}