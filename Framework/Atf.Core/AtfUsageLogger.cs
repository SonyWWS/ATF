//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace Sce.Atf
{
    /// <summary>
    /// Sends usage information to an ATF logging server, including the full computer name, user name,
    /// and operating system information</summary>
    /// <remarks>The purpose of logging this information is to help the ATF team know who is
    /// using the ATF and on what kind of hardware. The ATF team was asked to gather this data
    /// by upper management. Usage data can be viewed here:
    /// https://sd-cdump-dev002.share.scea.com/recap/ </remarks>
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AtfUsageLogger : IInitializable
    {
        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by sending usage information to an ATF logging server</summary>
        public void Initialize()
        {
            SendAtfUsageInfo();
        }

        #endregion

        /// <summary>
        /// Sends usage information to an ATF logging server by using a separate thread. Calling this
        /// multiple times only sends usage information once.</summary>
        public static void SendAtfUsageInfo()
        {
#if !PUBLIC
            if (s_atfUsageInfoLogged)
                return;
            s_atfUsageInfoLogged = true;

            ThreadPool.QueueUserWorkItem(_SendAtfUsageInfo);
#endif
        }

#if !PUBLIC
        // Prepares and sends the usage info. Run this on a separate thread in case it is time-consuming.
        private static void _SendAtfUsageInfo(object unusedState)
        {
            // GetHostName() and GetHostEntry() can throw an exception. We don't want to cause any problems for clients.
            try
            {
                Uri recapUri = new Uri("http://sd-cdump-dev002.share.scea.com:8080");
                
                // If we didn't check this first, RecapConnect.post() could cause exceptions to
                //  be thrown within Dns.GetHostEntry(), etc. That's just the semantics of System.Net.Dns
                //  that it communicates normal results via exceptions being thrown which is annoying to
                //  clients if they don't have a working internet connection and they're using the
                //  debugger with the "break on exception thrown" setting turned on.
                if (!CanResolve(recapUri))
                    return;

                var logger = new ServerLogger();
                logger.ServerName = recapUri;
                logger.ApplicationName = "ATF_" + logger.ApplicationName;

                string atfVersionString;
                Version atfVersion = AtfVersion.GetVersion();
                atfVersionString = atfVersion != null ? atfVersion.ToString() : "n/a";

                string computerName = Dns.GetHostName();
                IPHostEntry hostEntry = Dns.GetHostEntry(computerName);
                string fullComputerName = hostEntry.HostName;
                string userName = System.Environment.UserName;
                bool appIs64Bit = (IntPtr.Size == 8);
                string osVersion = GetOSFullName();
                string clrVersion = Environment.Version.ToString();
                int physicalMb = Kernel32.GetPhysicalMemoryMB();

                // Analyze all loaded assemblies
                StringBuilder loadedAssemblies = new StringBuilder();
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string name, version;
                    ParseAssemblyName(assembly.FullName, out name, out version);
                    if (loadedAssemblies.Length > 0)
                        loadedAssemblies.Append(", ");
                    loadedAssemblies.AppendFormat("{0} {1}", name, version);
                }

                string installedVisualStudioVersions = GetInstalledVisualStudioVersions();

                string phoneHomeMessage =
                    "ATF:" + atfVersionString + Environment.NewLine +
                    "Computer:" + fullComputerName + Environment.NewLine +
                    "User:" + userName + Environment.NewLine +
                    "InstalledVS:" + installedVisualStudioVersions + Environment.NewLine +
                    "64-bit app:" + appIs64Bit + Environment.NewLine +
                    "RAM(MB):" + physicalMb + Environment.NewLine +
                    "O.S.:" + osVersion + Environment.NewLine +
                    "CLR:" + clrVersion + Environment.NewLine +
                    "assemblies:" + loadedAssemblies
                    ;

                logger.SendLogData(phoneHomeMessage);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Get OS full name</summary>        
        private static string GetOSFullName()
        {
            string fullName = null;
            string osname = null;
            string arch = null;

            ManagementClass mgcls = new ManagementClass("Win32_OperatingSystem");
            foreach (ManagementObject mobj in mgcls.GetInstances())
            {
                foreach (PropertyData pdata in mobj.Properties)
                {
                    if (osname == null && pdata.Name.Trim() == "Caption")
                    {
                        osname = pdata.Value as string;
                    }
                    if (arch == null && pdata.Name.Trim() == "OSArchitecture")
                    {
                        arch = pdata.Value as string;
                    }
                }
            }

            if (!string.IsNullOrEmpty(osname))
            {
                fullName = osname;
                if (!string.IsNullOrEmpty(arch))
                    fullName += " " + arch;
            }

            // fallback
            if (string.IsNullOrEmpty(fullName))
                fullName = System.Environment.OSVersion.ToString();

            return fullName;
        }
       

        // Parses a full assembly name into its parts. For example:
        // "Microsoft.VisualStudio.HostingProcess.Utilities.Sync, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
        // gives a name of "Microsoft.VisualStudio.HostingProcess.Utilities.Sync" and a version of "9.0.0.0".
        private static void ParseAssemblyName(string fullName, out string name, out string version)
        {
            name = string.Empty;
            version = string.Empty;
            string[] parts = fullName.Split(',');
            if (parts.Length > 0)
                name = parts[0];
            if (parts.Length > 1)
                version = parts[1].Trim();
        }

        private static string GetInstalledVisualStudioVersions()
        {
            var versionSet = new HashSet<string>();

            // Let's use a try-catch in case there's a permissions problem.
            try
            {
                // We need to check different registry keys depending on the O.S. and version of Visual Studio.
                string[] registryKeys = {
                    @"SOFTWARE\Wow6432Node\Microsoft\DevDiv\VS\Servicing\", //seems to include all versions on 64-bit Windows
                    @"SOFTWARE\Microsoft\DevDiv\VS\Servicing\", //doesn't include VS2010 on 64-bit Windows, for example
                };
                foreach (string key in registryKeys)
                {
                    RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(key);
                    if (baseKey != null)
                    {
                        foreach (string subKeyName in baseKey.GetSubKeyNames())
                            versionSet.Add(subKeyName);
                    }
                }
            }
            catch
            {
            }

            var result = new StringBuilder();
            foreach (string version in versionSet)
            {
                if (result.Length > 0)
                    result.Append(',');
                result.Append(version);
            }
            
            if (result.Length > 0)
                return result.ToString();
            return "n/a";
        }

        // Tests if the domain name can be resolved without throwing any exceptions. This gets
        //  around the very annoying Dns.GetHostEntry().
        private static bool CanResolve(Uri server)
        {
            bool result = false;
            SafeFreeAddrInfo root = null;
            try
            {
                WSAData wsaData;
                short wsaVersion = (2 << 8) | 2; //version 2.2, good since Windows 95 OSR2!
                WSAStartup(wsaVersion, out wsaData);

                AddressInfo hints = new AddressInfo();
                int errorCode = getaddrinfo(server.Host, server.Port.ToString(), ref hints, out root);

                result = (errorCode == 0); //0 means success
            }
            catch
            {
                if (root != null)
                    root.Close();
            }
            finally
            {
                WSACleanup();
            }
            return result;
        }

        // copied from System.Net.WSAData using .NET Reflector
        [StructLayout(LayoutKind.Sequential)]
        private struct WSAData
        {
            public short wVersion;
            public short wHighVersion;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x101)]
            public string szDescription;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x81)]
            public string szSystemStatus;
            public short iMaxSockets;
            public short iMaxUdpDg;
            public IntPtr lpVendorInfo;
        }

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int WSAStartup([In] short wVersionRequested, out WSAData lpWSAData);

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int WSACleanup();

        // copied from System.Net.SafeFreeAddrInfo using .NET Reflector
        [SuppressUnmanagedCodeSecurity]
        private sealed class SafeFreeAddrInfo : SafeHandleZeroOrMinusOneIsInvalid
        {
            // Methods
            public SafeFreeAddrInfo()
                : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                freeaddrinfo(base.handle);
                return true;
            }
        }

        [Flags]
        private enum AddressInfoHints
        {
            AI_CANONNAME = 2,
            AI_NUMERICHOST = 4,
            AI_PASSIVE = 1
        }

        // copied from System.Net.AddressInfo using .NET Reflector
        [StructLayout(LayoutKind.Sequential)]
        private struct AddressInfo
        {
            public AddressInfoHints ai_flags;
            public AddressFamily ai_family;
            public SocketType ai_socktype;
            public ProtocolFamily ai_protocol;
            public int ai_addrlen;
            IntPtr ai_canonname;
            IntPtr ai_addr;
            IntPtr ai_next;
        }

        [DllImport("ws2_32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        private static extern int getaddrinfo([In] string nodename,[In] string servicename,[In] ref AddressInfo hints,out SafeFreeAddrInfo handle);

        [DllImport("ws2_32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern void freeaddrinfo([In] IntPtr info);

        private static bool s_atfUsageInfoLogged;
#endif
    }
}
