//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Runtime.InteropServices;

namespace Sce.Atf
{
    /// <summary>
    /// Supports interoperability for structures and functions in kernel32.dll</summary>
    public static class Kernel32
    {
        /// <summary>
        /// Retrieves information about MS-DOS device names using Windows kernel</summary>
        /// <param name="lpDeviceName">An MS-DOS device name string specifying the target of the query</param>
        /// <param name="lpTargetPath">Pointer to a buffer that receives the result of the query</param>
        /// <param name="ucchMax">Maximum number of TCHARs that can be stored into the buffer pointed to by lpTargetPath</param>
        /// <returns>If the function succeeds, the return value is the number of TCHARs stored into the buffer pointed to by lpTargetPath.
        /// If the function fails, the return value is zero.
        /// If the buffer is too small, the function fails and the last error code is ERROR_INSUFFICIENT_BUFFER.</returns>
        /// <remarks>For more information, see the MSDN article at http://msdn.microsoft.com/en-us/library/windows/desktop/aa365461(v=vs.85).aspx </remarks>
        [DllImport("kernel32.dll", EntryPoint = "QueryDosDeviceW")]
        public static extern uint QueryDosDeviceW(
            [In] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpDeviceName,
            [Out] [MarshalAsAttribute(UnmanagedType.LPWStr)] System.Text.StringBuilder lpTargetPath,
            uint ucchMax);
    }
}

