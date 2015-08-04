//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using STATSTG = System.Runtime.InteropServices.ComTypes.STATSTG;

namespace Sce.Atf
{
    internal static class OleConverter
    {
        private static readonly byte[] s_serializedObjectId = new Guid("FD9EA796-3B13-4370-A679-56106BB288FB").ToByteArray();
        public const TYMED SupportedTymed = TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTREAM | TYMED.TYMED_GDI;

        #region -- Unmanaged functions ----------------------------------------

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GlobalLock(HandleRef handle);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern bool GlobalUnlock(HandleRef handle);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern int GlobalSize(HandleRef handle);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);

        [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr GlobalFree(HandleRef handle);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int DragQueryFile(HandleRef hDrop, int iFile, StringBuilder lpszFile, int cch);

        #endregion

        public static object Convert(string format, ref STGMEDIUM medium)
        {
            if (medium.unionmember != IntPtr.Zero)
            {
                if (medium.tymed == TYMED.TYMED_HGLOBAL)
                {
                    return ConvertHandle(format, new HandleRef(format, medium.unionmember));
                }
                if (medium.tymed == TYMED.TYMED_ISTREAM)
                {
                    return ConvertStream(format, new HandleRef(format, medium.unionmember));
                }
                if (medium.tymed == TYMED.TYMED_GDI)
                {
                    return ConvertBitmap(new HandleRef(format, medium.unionmember));
                }
            }
            return null;
        }

        public static FORMATETC CreateFormat(string format)
        {
            return new FORMATETC
            {
                tymed = SupportedTymed,
                cfFormat = unchecked((short)(ushort)(DataFormats.GetFormat(format).Id)),
                dwAspect = DVASPECT.DVASPECT_CONTENT,
                lindex = -1,
                ptd = IntPtr.Zero
            };
        }

        private static object ConvertHandle(string format, HandleRef data)
        {
            var ptr = GlobalLock(data);
            try
            {
                // filedrop requires special API for conversion.
                if (format.Equals(DataFormats.FileDrop))
                {
                    return ReadFileListFromHandle(data);
                }

                // the rest we simply get from a byte array.
                var size = GlobalSize(data);
                var bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);

                if (IsSerializedObject(bytes))
                {
                    int count = s_serializedObjectId.Length;
                    using (var stream = new MemoryStream(bytes, count, bytes.Length-count))
                    {
                        var formatter = new BinaryFormatter { AssemblyFormat = FormatterAssemblyStyle.Simple };
                        return formatter.Deserialize(stream);
                    }
                }
                if (format.Equals(DataFormats.Text)
                        || format.Equals(DataFormats.Rtf)
                        || format.Equals(DataFormats.OemText)
                        || format.Equals("FileName"))
                {
                    return Encoding.ASCII.GetString(bytes);
                }
                if (format.Equals(DataFormats.UnicodeText)
                        || format.Equals("FileNameW"))
                {
                    return Encoding.Unicode.GetString(bytes);
                }
                if (format.Equals(DataFormats.Html))
                {
                    return Encoding.UTF8.GetString(bytes);
                }

                return new MemoryStream(bytes);
            }
            finally
            {
                GlobalUnlock(data);
                GlobalFree(data);
            }
        }

        private static string[] ReadFileListFromHandle(HandleRef handle)
        {
            var sb = new StringBuilder();

            var count = DragQueryFile(handle, unchecked((int)0xFFFFFFFF), null, 0);
            if (count <= 0)
            {
                return null;
            }

            var files = new string[count];
            for (var i = 0; i < count; i++)
            {
                var charlen = DragQueryFile(handle, i, sb, sb.Capacity);
                var s = sb.ToString();
                if (s.Length > charlen)
                {
                    s = s.Substring(0, charlen);
                }

                files[i] = s;
            }
            return files;
        }

        private static object ConvertStream(string format, HandleRef data)
        {
            var pStream = (IStream)Marshal.GetObjectForIUnknown(data.Handle);
            Marshal.Release(data.Handle);

            STATSTG sstg;
            pStream.Stat(out sstg, 0);
            var size = (int)sstg.cbSize;

            var hglobal = GlobalAlloc(0x2042, size);
            var handle = new HandleRef(format, hglobal);
            var ptr = GlobalLock(handle);
            pStream.Read(ptr, size);
            GlobalUnlock(handle);

            return ConvertHandle(format, handle);
        }

        private static object ConvertBitmap(HandleRef data)
        {
            using (Image clipboardImage = Image.FromHbitmap(data.Handle))
            {
                return clipboardImage.Clone();
            }
        }

        private static bool IsSerializedObject(byte[] bytes)
        {
            if (bytes.Length <= s_serializedObjectId.Length)
                return false;
            for (var i = 0; i < s_serializedObjectId.Length; i++)
            {
                if (s_serializedObjectId[i] != bytes[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
