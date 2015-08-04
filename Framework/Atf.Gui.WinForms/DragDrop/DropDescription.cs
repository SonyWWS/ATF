//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Runtime.InteropServices;

namespace Sce.Atf
{
    /// <summary>
    /// This is the managed equivalent to DROPDESCRIPTION in the Win32 ShlObj.h.</summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Size = 1044)]
    public struct DropDescription
    {
        public DropImageType type;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szMessage;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szInsert;
    }
}