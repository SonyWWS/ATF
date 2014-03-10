//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Runtime.InteropServices;

namespace Sce.Atf
{
    /// <summary>
    /// Supports interoperability for structures and functions in kernel32.dll</summary>
    public static class Kernel32
    {
        [DllImport("kernel32.dll", EntryPoint = "QueryDosDeviceW")]
        public static extern uint QueryDosDeviceW(
            [In] [MarshalAsAttribute(UnmanagedType.LPWStr)] string lpDeviceName,
            [Out] [MarshalAsAttribute(UnmanagedType.LPWStr)] System.Text.StringBuilder lpTargetPath,
            uint ucchMax);
    }
}

