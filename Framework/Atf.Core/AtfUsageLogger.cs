//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Management;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Sce.Atf
{
    /// <summary>
    /// Sends usage information to an ATF logging server, including the full computer name, user name,
    /// and operating system information.</summary>
    /// <remarks>The purpose of logging this information is to help the ATF team know who is
    /// using ATF and on what kind of hardware. The ATF team was asked to gather this data
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
        // Clients can turn on "Enable Just My Code" in the debugger options to avoid seeing handled
        //  exceptions in the .NET sockets code if the server can't be found.
        private static void _SendAtfUsageInfo(object unusedState)
        {
            // GetHostName() and GetHostEntry() can throw an exception. We don't want to cause any problems for clients.
            try
            {
                var recapUri = new Uri("http://sd-cdump-dev002.share.scea.com:8080");
                
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
                var loadedAssemblies = new StringBuilder();
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

        private static bool s_atfUsageInfoLogged;
#endif
    }
}
