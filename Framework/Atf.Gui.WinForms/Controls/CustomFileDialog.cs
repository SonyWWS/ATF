//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Sce.Atf
{
    /// <summary>
    /// Base class for custom SaveFileDialog and OpenFileDialog</summary>
    public abstract class CustomFileDialog
    {
        /// <summary>
        /// Gets or sets whether to add a file extension</summary>
        public bool AddExtension
        {
            get { return m_addExt; }
            set { m_addExt = value; }
        }

        /// <summary>
        /// Gets or sets whether the user can type only names of existing files in the File Name entry
        /// field. If this flag is specified and the user enters an invalid name, the dialog box
        /// procedure displays a warning in a message box. If this flag is specified, the
        /// OFN_PATHMUSTEXIST flag is also used. This flag can be used in an Open dialog box. It
        /// cannot be used with a Save As dialog box.</summary>
        public bool CheckFileExists
        {
            get { return GetFlag(OFN_FILEMUSTEXIST); }
            set { SetFlag(OFN_FILEMUSTEXIST, value); }
        }

        /// <summary>
        /// Gets or sets whether the user can type only valid paths and file names. If this flag is used
        /// and the user types an invalid path and file name in the File Name entry field, the
        /// dialog box function displays a warning in a message box.</summary>
        public bool CheckPathExists
        {
            get { return GetFlag(OFN_PATHMUSTEXIST); }
            set { SetFlag(OFN_PATHMUSTEXIST, value); }
        }

        /// <summary>
        /// Gets or sets the default extension. GetOpenFileName and GetSaveFileName append this
        /// extension to the file name if the user fails to type an extension. This string can be
        /// any length, but only the first three characters are appended. The string should not
        /// contain a period (.). Is never null. If set to null, then the empty string is
        /// used instead.</summary>
        public string DefaultExt
        {
            get { return m_defaultExt; }
            set
            {
                if (value != null)
                    m_defaultExt = value;
                else
                    m_defaultExt = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets whether the dialog box returns the path and file name of the selected shortcut
        /// (.LNK) file. If this value is not specified, the dialog box returns the path and file
        /// name of the file referenced by the shortcut.</summary>
        public bool DereferenceLinks
        {
            get { return !GetFlag(OFN_NODEREFERENCELINKS); }
            set { SetFlag(OFN_NODEREFERENCELINKS, !value); }
        }

        /// <summary>
        /// Gets or sets the initial file name displayed to the user in the file name edit box when saving a file.
        /// If a path is included, that path is the initial directory (unless this is the very
        /// first time this application has run).</summary>
        public string FileName
        {
            get { return m_fileName; }
            set
            {
                if (value != null)
                    m_fileName = value;
                else
                    m_fileName = string.Empty;
            }
        }

        /// <summary>
        /// Gets the resulting selected file name(s)</summary>
        public string[] FileNames
        {
            get { return m_fileNames; }
        }

        /// <summary>
        /// Gets or sets a filter string whose format is pairs of strings. The first string of each
        /// pair is a user-readable description, which by convention includes the extension filter. The
        /// second string of each pair is the extension filter; if there are multiple strings, each is
        /// separated by ';'. The first and second string in each pair is separated by '|'. Each pair is
        /// separated by '|'.
        /// For example:
        /// "Setting file(*.xml)|*.xml" or
        /// "Code files (*.txt;*.cs;*.lua;*.nut;*.py;*.xml;*.dae;*.cg)|*.txt;*.cs;*.lua;*.nut;*.py;*.xml;*.dae;*.cg" or
        /// "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</summary>
        public string Filter
        {
            get { return m_filter; }
            set
            {
                if (value != null)
                    m_filter = value;
                else
                    m_filter = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the index of the currently selected filter</summary>
        public int FilterIndex
        {
            get { return m_filterIndex; }
            set { m_filterIndex = value; }
        }

        /// <summary>
        /// Gets or sets the initial directory to be used in the Open/Save dialog box. If it's the
        /// default empty string, the last directory the user navigated to is used
        /// as the initial directory. Setting to null is the same as setting to the empty string.</summary>
        public string ForcedInitialDirectory
        {
            get { return m_forcedInitialDir; }
            set
            {
                if (value != null)
                    m_forcedInitialDir = value;
                else
                    m_forcedInitialDir = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets whether the current directory is restored to its original value if the user changed
        /// the directory while searching for files. On Windows XP, this flag is ignored when opening
        /// files. See OFN_NOCHANGEDIR: http://msdn.microsoft.com/en-us/library/ms646839(VS.85).aspx. </summary>
        public bool RestoreDirectory
        {
            get { return GetFlag(OFN_NOCHANGEDIR); }
            set { SetFlag(OFN_NOCHANGEDIR, value); }
        }

        /// <summary>
        /// Gets or sets the string to appear in the title bar of the dialog box</summary>
        public string Title
        {
            get { return m_title; }
            set
            {
                if (value != null)
                    m_title = value;
                else
                    m_title = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets whether or not the returned filenames are checked for invalid characters</summary>
        public bool ValidateNames
        {
            get { return !GetFlag(OFN_NOVALIDATE); }
            set { SetFlag(OFN_NOVALIDATE, !value); }
        }

        /// <summary>
        /// Shows the dialog</summary>
        /// <returns>Result of user dialog interaction</returns>
        public DialogResult ShowDialog()
        {
            return ShowDialog(null);
        }

        /// <summary>
        /// Shows the dialog</summary>
        /// <param name="owner">Dialog owner</param>
        /// <returns>DialogResult from user</returns>
        public DialogResult ShowDialog(IWin32Window owner)
        {
            return ShowNonCustomDialog(owner);
        }

        /// <summary>
        /// Shows the built in WinForms file dialog instead of the custom dialog. This is because of an
        /// incompatibility in Windows 8.</summary>
        /// <param name="dialog">The instance of the dialog to show</param>
        /// <param name="owner">Dialog owner</param>
        /// <returns>The DialogResult from the file dialog</returns>
        internal protected virtual DialogResult ShowNonCustomDialogInternal(FileDialog dialog, IWin32Window owner)
        {
            // Initialize the dialog settings
            dialog.Filter = m_filter;
            dialog.FilterIndex = m_filterIndex;

            if (!string.IsNullOrEmpty(m_forcedInitialDir))
            {
                dialog.InitialDirectory = m_forcedInitialDir;
            }
            else
            {
                dialog.InitialDirectory = GetLastDirForFilter(m_filter);
            }
            dialog.FileName = m_fileName;
            dialog.Title = m_title;
            
            dialog.CheckFileExists = CheckFileExists;
            dialog.CheckPathExists = CheckPathExists;
            dialog.DereferenceLinks = DereferenceLinks;
            dialog.RestoreDirectory = RestoreDirectory;
            dialog.ValidateNames = ValidateNames;

            if (m_addExt)
            {
                dialog.DefaultExt = m_defaultExt;
            }

            DialogResult result = dialog.ShowDialog(owner);

            if (result == DialogResult.OK)
            {
                // Copy the results back from the dialog.
                m_filterIndex = dialog.FilterIndex;
                if (dialog.FileNames.Length > 1)
                {
                    m_fileNames = new string[dialog.FileNames.Length];
                    for (int i = 0; i < dialog.FileNames.Length; i++)
                    {
                        m_fileNames[i] = dialog.FileNames[i];
                    }
                    m_fileName = m_fileNames[0];
                }
                else
                {
                    m_fileName = dialog.FileName;
                    m_fileNames = new string[] { m_fileName };
                }

                SetLastDirForFilter(m_filter, m_fileName);
            }
            return result;
        }

        internal static IDictionary<string, string> FilterToLastUsedDirectory
        {
            get { return s_filterToLastDir; }
        }

        ////////////////////////////////////////

        /// <summary>
        /// Set dialog open flags</summary>
        /// <param name="mask">Flag mask</param>
        /// <param name="value">Flag value</param>
        protected void SetFlag(int mask, bool value)
        {
            if (value)
                m_flags |= mask;
            else
                m_flags &= ~mask;
        }

        /// <summary>
        /// Get dialog open flags</summary>
        /// <param name="mask">Flag mask</param>
        /// <returns>Masked flag value</returns>
        protected bool GetFlag(int mask)
        {
            return (m_flags & mask) != 0;
        }

        /// <summary>
        /// For sample implementation, see CustomSaveFileDialog.</summary>
        /// <param name="owner">ignored</param>
        /// <returns>DialogResult.Cancel</returns>
        protected internal virtual DialogResult ShowNonCustomDialog(IWin32Window owner)
        {
            //implement ShowNonCustomDialog in your derived class
            return DialogResult.Cancel;
        }


        protected delegate int WndProcDelegate(IntPtr hWnd, uint msg, int wParam, int lParam);

        protected const int OFN_ENABLEHOOK = 0x00000020;
        protected const int OFN_EXPLORER = 0x00080000;
        protected const int OFN_FILEMUSTEXIST = 0x00001000;
        protected const int OFN_HIDEREADONLY = 0x00000004;
        protected const int OFN_CREATEPROMPT = 0x00002000;
        protected const int OFN_NOTESTFILECREATE = 0x00010000;
        protected const int OFN_OVERWRITEPROMPT = 0x00000002;
        protected const int OFN_PATHMUSTEXIST = 0x00000800;
        protected const int OFN_NODEREFERENCELINKS = 0x00100000;
        protected const int OFN_NOCHANGEDIR = 0x00000008;
        protected const int OFN_NOVALIDATE = 0x00000100;
        protected const int OFN_ENABLESIZING = 0x00800000;
        protected const int OFN_READONLY = 0x00000001;
        protected const int OFN_ALLOWMULTISELECT = 0x00000200;


        private const int GWL_WNDPROC = -4;

        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOZORDER = 0x0004;

        /// <summary>
        /// Windows WINDOWINFO struct</summary>
        /// <remarks>See http://msdn.microsoft.com/en-us/library/ms632610(VS.85).aspx .</remarks>
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWINFO
        {
            public uint cbSize;
            public User32.RECT rcWindow;
            public User32.RECT rcClient;
            public uint dwStyle;
            public uint dwExStyle;
            public uint dwWindowStatus;
            public uint cxWindowBorders;
            public uint cyWindowBorders;
            public ushort atomWindowType;
            public ushort wCreatorVersion;
        }

        /// <summary>
        /// Windows OPENFILENAME struct</summary>
        /// <remarks>See http://msdn.microsoft.com/en-us/library/ms646839(VS.85).aspx .</remarks>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        protected struct OPENFILENAME
        {
            public int lStructSize;
            public IntPtr hwndOwner;
            public int hInstance;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpstrFilter;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpstrCustomFilter;
            public int nMaxCustFilter;
            public int nFilterIndex;
            public IntPtr lpstrFile;  // must be marshaled manually; see ShowDialog()
            public int nMaxFile; // specifies the # of TCHARs (== # of Unicode chars for us?)
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpstrFileTitle;
            public int nMaxFileTitle; // specifies the # of TCHARs (== # of Unicode chars for us?)
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpstrInitialDir;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpstrTitle;
            public int Flags;
            public short nFileOffset;
            public short nFileExtension;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpstrDefExt;
            public int lCustData;
            public WndProcDelegate lpfnHook;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpTemplateName;
            // only if on WINNT 5.0 or higher:
            public int pvReserved;
            public int dwReserved;
            public int FlagsEx;
        }

        /// <summary>
        /// Sets the directory that the user last navigated to, for a given file filter string</summary>
        /// <param name="filter">The file Open/Save dialog filter string, as in the Filter property</param>
        /// <param name="path">The path of the file that the user last navigated to. It should
        /// include the file name (e.g., "C:\MyDir\MyFile.txt") or end with a directory separator
        /// (e.g., "C:\MyDir\").</param>
        private static void SetLastDirForFilter(string filter, string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    s_filterToLastDir[filter] = dir;
            }
        }

        /// <summary>
        /// Gets the last directory that the user navigated to for the given file filter, or null</summary>
        /// <param name="filter">The file Open/Save dialog filter string, as in the Filter property</param>
        /// <returns>"C:\MyDir", for example</returns>
        private static string GetLastDirForFilter(string filter)
        {
            string dir;
            s_filterToLastDir.TryGetValue(filter, out dir);
            return dir;
        }

        private static Dictionary<string, string> s_filterToLastDir = new Dictionary<string, string>();

        private string m_filter = string.Empty;
        private int m_filterIndex;
        private string m_fileName = string.Empty;
        private string[] m_fileNames = new string[0];
        private string m_initialDir = string.Empty; //not used
        private string m_forcedInitialDir = string.Empty;
        private string m_title = string.Empty;
        private string m_defaultExt = string.Empty;
        private bool m_addExt = true;
        private int m_flags =
            OFN_EXPLORER |
            OFN_PATHMUSTEXIST |
            OFN_OVERWRITEPROMPT |
            OFN_ENABLESIZING |
            OFN_HIDEREADONLY |
            OFN_ENABLEHOOK;
    }
}

