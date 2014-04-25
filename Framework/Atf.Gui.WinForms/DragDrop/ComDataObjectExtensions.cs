//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Sce.Atf.DragDrop
{
    /// <summary>
    /// Provides extended functionality for the COM IDataObject interface.
    /// for internal use only.</summary>
    internal static class ComDataObjectExtensions
    {
        public static void SetData<T>(this IDataObject dataObject, string format, T data)
            where T : struct
        {
            var formatEtc = OleConverter.CreateFormat(format);
            formatEtc.tymed = TYMED.TYMED_HGLOBAL;

            // We need to set the drop description as an HGLOBAL.
            // Allocate space ...
            var unmanagedPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
            try
            {
                // ... and marshal the data
                Marshal.StructureToPtr(data, unmanagedPtr, false);

                // The medium wraps the HGLOBAL
                STGMEDIUM medium;
                medium.pUnkForRelease = null;
                medium.tymed = TYMED.TYMED_HGLOBAL;
                medium.unionmember = unmanagedPtr;

                // Set the data
                dataObject.SetData(ref formatEtc, ref medium, true);
            }
            catch
            {
                // If we failed, we need to free the HGLOBAL memory
                Marshal.FreeHGlobal(unmanagedPtr);
                throw;
            }
        }

        public static bool TryGetData<T>(this IDataObject dataObject, string format, out T data)
            where T : struct
        {
            var formatEtc = OleConverter.CreateFormat(format);
            formatEtc.tymed = TYMED.TYMED_HGLOBAL;

            if (0 == dataObject.QueryGetData(ref formatEtc))
            {
                STGMEDIUM medium;
                dataObject.GetData(ref formatEtc, out medium);
                try
                {
                    data = (T)Marshal.PtrToStructure(medium.unionmember, typeof(T));
                    return true;
                }
                finally
                {
                    ReleaseStgMedium(ref medium);
                }
            }

            data = default(T);
            return false;
        }

        [DllImport("ole32.dll")]
        private static extern void ReleaseStgMedium(ref STGMEDIUM pmedium);
    }
}