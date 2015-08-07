//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf
{
    /// <summary>
    /// Provides static methods to read system icons for both folders and files</summary>
    public static class FileIconUtil
    {
        /// <summary>
        /// Options to specify the size of icons to return</summary>
        public enum IconSize
        {
            /// <summary>
            /// Specifies large icon - 32 pixels by 32 pixels</summary>
            Large = 0,
            /// <summary>
            /// Specifies small icon - 16 pixels by 16 pixels</summary>
            Small = 1
        }

        /// <summary>
        /// Options to specify whether folders should be in the open or closed state</summary>
        public enum FolderType
        {
            /// <summary>
            /// Specifies open folder</summary>
            Open = 0,
            /// <summary>
            /// Specifies closed folder</summary>
            Closed = 1
        }

        /// <summary>
        /// Returns an icon for a given file - indicated by the name parameter</summary>
        /// <param name="name">Pathname for file</param>
        /// <param name="size">Large or small icon</param>
        /// <param name="linkOverlay">Whether to include the link icon</param>
        /// <returns>Icon</returns>
        public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay)
        {
            Shell32.SHFILEINFO shfi = new Shell32.SHFILEINFO();
            uint flags = Shell32.SHGFI_ICON | Shell32.SHGFI_USEFILEATTRIBUTES;

            if (linkOverlay) flags += Shell32.SHGFI_LINKOVERLAY;

            // Check the size specified for return.
            if (IconSize.Small == size)
            {
                flags += Shell32.SHGFI_SMALLICON;
            }
            else
            {
                flags += Shell32.SHGFI_LARGEICON;
            }

            Shell32.SHGetFileInfo(name,
                Shell32.FILE_ATTRIBUTE_NORMAL,
                ref shfi,
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                flags);

            // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
            Icon icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
            User32.DestroyIcon(shfi.hIcon);        // Cleanup
            return icon;
        }

        /// <summary>
        /// Obtains system folder icon</summary>
        /// <param name="size">Specifies large or small icons</param>
        /// <param name="folderType">Specifies open or closed FolderType</param>
        /// <returns>System folder icon</returns>
        public static Icon GetFolderIcon(IconSize size, FolderType folderType)
        {
            // Need to add size check, although errors generated at present!
            uint flags = Shell32.SHGFI_ICON | Shell32.SHGFI_USEFILEATTRIBUTES;

            if (FolderType.Open == folderType)
            {
                flags += Shell32.SHGFI_OPENICON;
            }

            if (IconSize.Small == size)
            {
                flags += Shell32.SHGFI_SMALLICON;
            }
            else
            {
                flags += Shell32.SHGFI_LARGEICON;
            }

            // Get the folder icon
            Shell32.SHFILEINFO shfi = new Shell32.SHFILEINFO();

            // Flag SHGFI_USEFILEATTRIBUTES prevents SHGetFileInfo() from attempting
            // to access the path specified by the first argument.  However, passing 
            // some sort of non-null string is still required, or the call will fail.
            Shell32.SHGetFileInfo("C:\\Windows",
                Shell32.FILE_ATTRIBUTE_DIRECTORY,
                ref shfi,
                (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                flags);

            Icon.FromHandle(shfi.hIcon);    // Load the icon from an HICON handle

            // Now clone the icon, so that it can be successfully stored in an ImageList
            Icon icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

            User32.DestroyIcon(shfi.hIcon);        // Cleanup
            return icon;
        }
    }
}

