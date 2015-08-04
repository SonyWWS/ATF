//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Runtime.InteropServices;

namespace Sce.Atf
{
    /// <summary>
    /// Supports interoperability for structures and functions in Shell32.dll</summary>
    public static class Shell32
    {
        /// <summary>
        /// Maximum path character length</summary>
        public const int MAX_PATH = 260;

        /// <summary>
        /// SHITEMID structure to interoperate with Shell32.dll</summary>
        [Obsolete("Throws exception: 'cannot be marshaled as an unmanaged structure; no meaningful size or offset can be computed.'")]
        [StructLayout(LayoutKind.Sequential)]
        public struct SHITEMID
        {
            public ushort cb;
            [MarshalAs(UnmanagedType.LPArray)]
            public byte[] abID;
        }

        /// <summary>
        /// ITEMIDLIST structure to interoperate with Shell32.dll</summary>
        [Obsolete("Throws exception: 'cannot be marshaled as an unmanaged structure; no meaningful size or offset can be computed.'")]
        [StructLayout(LayoutKind.Sequential)]
        public struct ITEMIDLIST
        {
            public SHITEMID mkid;
        }

        /// <summary>
        /// BROWSEINFO structure to interoperate with Shell32.dll</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszTitle;
            public uint ulFlags;
            public IntPtr lpfn;
            public int lParam;
            public IntPtr iImage;
        }

        // Browsing for directory.
        public const uint BIF_RETURNONLYFSDIRS = 0x0001;
        public const uint BIF_DONTGOBELOWDOMAIN = 0x0002;
        public const uint BIF_STATUSTEXT = 0x0004;
        public const uint BIF_RETURNFSANCESTORS = 0x0008;
        public const uint BIF_EDITBOX = 0x0010;
        public const uint BIF_VALIDATE = 0x0020;
        public const uint BIF_NEWDIALOGSTYLE = 0x0040;
        public const uint BIF_USENEWUI = (BIF_NEWDIALOGSTYLE | BIF_EDITBOX);
        public const uint BIF_BROWSEINCLUDEURLS = 0x0080;
        public const uint BIF_BROWSEFORCOMPUTER = 0x1000;
        public const uint BIF_BROWSEFORPRINTER = 0x2000;
        public const uint BIF_BROWSEINCLUDEFILES = 0x4000;
        public const uint BIF_SHAREABLE = 0x8000;

        /* From WinNT.h, handles are the same size as pointers (see the "*name"):
        #define DECLARE_HANDLE(name) struct name##__{int unused;}; typedef struct name##__ *name
        
         * From WinDef.h:
        DECLARE_HANDLE(HICON);

         * From ShellAPI.h:
        typedef struct _SHFILEINFOA
        {
                HICON       hIcon;                      // out: icon
                int         iIcon;                      // out: icon index
                DWORD       dwAttributes;               // out: SFGAO_ flags
                CHAR        szDisplayName[MAX_PATH];    // out: display name (or path)
                CHAR        szTypeName[80];             // out: type name
        } SHFILEINFOA;
        typedef struct _SHFILEINFOW
        {
                HICON       hIcon;                      // out: icon
                int         iIcon;                      // out: icon index
                DWORD       dwAttributes;               // out: SFGAO_ flags
                WCHAR       szDisplayName[MAX_PATH];    // out: display name (or path)
                WCHAR       szTypeName[80];             // out: type name
        } SHFILEINFOW; */
        /// <summary>
        /// SHFILEINFO structure to interoperate with Shell32.dll</summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHFILEINFO
        {
            public const int NAMESIZE = 80;
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = NAMESIZE)]
            public string szTypeName;
        };

        // For SHGetFileInfo's uFlags parameter:
        public const uint SHGFI_ICON = 0x000000100;     // get icon
        public const uint SHGFI_DISPLAYNAME = 0x000000200;     // get display name
        public const uint SHGFI_TYPENAME = 0x000000400;     // get type name
        public const uint SHGFI_ATTRIBUTES = 0x000000800;     // get attributes
        public const uint SHGFI_ICONLOCATION = 0x000001000;     // get icon location
        public const uint SHGFI_EXETYPE = 0x000002000;     // return exe type
        public const uint SHGFI_SYSICONINDEX = 0x000004000;     // get system icon index
        public const uint SHGFI_LINKOVERLAY = 0x000008000;     // put a link overlay on icon
        public const uint SHGFI_SELECTED = 0x000010000;     // show icon in selected state
        public const uint SHGFI_ATTR_SPECIFIED = 0x000020000;     // get only specified attributes
        public const uint SHGFI_LARGEICON = 0x000000000;     // get large icon
        public const uint SHGFI_SMALLICON = 0x000000001;     // get small icon
        public const uint SHGFI_OPENICON = 0x000000002;     // get open icon
        public const uint SHGFI_SHELLICONSIZE = 0x000000004;     // get shell size icon
        public const uint SHGFI_PIDL = 0x000000008;     // pszPath is a pidl
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;     // use passed dwFileAttribute
        public const uint SHGFI_ADDOVERLAYS = 0x000000020;     // apply the appropriate overlays
        public const uint SHGFI_OVERLAYINDEX = 0x000000040;     // Get the index of the overlay

        // For SHGetFileInfo's dwAttributes parameter:
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;
        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        /* From ShellAPI.h:
        SHSTDAPI_(DWORD_PTR) SHGetFileInfoA(LPCSTR pszPath, DWORD dwFileAttributes, __inout_bcount_opt(cbFileInfo) SHFILEINFOA *psfi,
            UINT cbFileInfo, UINT uFlags);
        SHSTDAPI_(DWORD_PTR) SHGetFileInfoW(LPCWSTR pszPath, DWORD dwFileAttributes, __inout_bcount_opt(cbFileInfo) SHFILEINFOW *psfi,
            UINT cbFileInfo, UINT uFlags);
        #ifdef UNICODE
        #define SHGetFileInfo  SHGetFileInfoW
        #else
        #define SHGetFileInfo  SHGetFileInfoA
        #endif // !UNICODE
         */

        /// <summary>
        /// Gets file information. https://msdn.microsoft.com/en-us/library/windows/desktop/bb762179%28v=vs.85%29.aspx
        /// </summary>
        /// <param name="pszPath">The relative or absolute path of a file, directory, or drive</param>
        /// <param name="dwFileAttributes">File attributes</param>
        /// <param name="psfi">SHFILEINFO to be filled out</param>
        /// <param name="cbFileInfo">Size of the SHFILEINFO, in bytes. Use Marshal.SizeOf().</param>
        /// <param name="uFlags">Flags to specify what information to retrieve</param>
        /// <returns></returns>
        [DllImport("Shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags
            );
    }
}

