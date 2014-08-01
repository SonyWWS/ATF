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


        [StructLayout(LayoutKind.Sequential)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = 64;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys; //The amount of actual physical memory, in bytes.
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(MEMORYSTATUSEX lpBuffer);

        public static int GetPhysicalMemoryMB()
        {
            // Available from Windows 2000 and onward; i.e., Environment.OSVersion.Version.Major >= 5 .
            var memoryStatus = new MEMORYSTATUSEX();
            GlobalMemoryStatusEx(memoryStatus);
            return (int)(memoryStatus.ullTotalPhys / (1024 * 1024));// convert bytes to megabytes
        }
    }
}

