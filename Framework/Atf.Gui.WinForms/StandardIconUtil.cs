//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf
{
    /// <summary>
    /// Static methods to load standard system icons</summary>
    public static class StandardIconUtil
    {
        /// <summary>
        /// Enumeration of standard icon types</summary>
        public enum IconType
        {
            /// <summary>
            /// Standard application icon</summary>
            Application = 32512,

            /// <summary>
            /// Standard error icon</summary>
            Error = 32513,

            /// <summary>
            /// Standard question icon</summary>
            Question = 32514,

            /// <summary>
            /// Standard exclamation icon</summary>
            Exclamation = 32515,

            /// <summary>
            /// Standard warning icon</summary>
            Warning = 32515,

            /// <summary>
            /// Standard information icon</summary>
            Information = 32516,
        }

        /// <summary>
        /// Gets a standard icon</summary>
        /// <param name="type">Standard icon type</param>
        /// <returns>Standard icon</returns>
        public static Icon GetStandardIcon(IconType type)
        {
            IntPtr handle = User32.LoadIcon((IntPtr) null, (IntPtr) type);

            // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
            Icon icon = (Icon)Icon.FromHandle(handle).Clone();
            User32.DestroyIcon(handle);    // Cleanup
            return icon;
        }
    }
}


