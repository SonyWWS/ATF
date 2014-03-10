//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;

namespace Sce.Atf
{
    /// <summary>
    /// This class is used for getting ATF version for a given assembly.
    /// ATF version is different than Assembly version.
    /// With each new ATF release, all ATF assemblies will have ATFVersion in 
    /// addition to default assembly version and file version.
    /// Client code can use ATF version to enforce compatibility.</summary>
    public class AtfVersion
    {
        /// <summary>
        /// Returns the ATF version</summary>
        /// <returns>ATF version or null if it can't be found</returns>
        public static Version GetVersion()
        {
            return GetVersion(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Returns the ATF version for a given assembly</summary>
        /// <param name="assem">Assembly for which the ATF version is found</param>
        /// <returns>ATF version of the given assembly or null if not found</returns>
        public static Version GetVersion(Assembly assem)
        {
            //dynamically generated assemblies don't implement GetManifestResourceXxx()
            if (assem is AssemblyBuilder)
                return null;

            // First try with the wws_atf.component file. This was introduced to ATF 3.4.
            string strAtfResx = null;
            string[] resxs = assem.GetManifestResourceNames();
            foreach (string resx in resxs)
            {
                if (resx.EndsWith("wws_atf.component", StringComparison.OrdinalIgnoreCase))
                    strAtfResx = resx;
            }

            if (strAtfResx != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(assem.GetManifestResourceStream(strAtfResx));
                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("xsi", "http://www.ship.scea.com/wws_sdk/component/v1");
                XmlNode node = xmlDoc.SelectSingleNode("xsi:component/xsi:version", nsmgr);
                if (node != null)
                    return new Version(node.InnerText);
            }

            // For backwards compatibility with ATF 3.3 and earlier, look for AtfVersion.xml.
            // For example, AtfRefactor may be examining an older assembly.
            strAtfResx = null;
            foreach (string resx in resxs)
            {
                if (resx.EndsWith("AtfVersion.xml", StringComparison.OrdinalIgnoreCase))
                    strAtfResx = resx;
            }

            if (strAtfResx != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(assem.GetManifestResourceStream(strAtfResx));
                XmlNode node = xmlDoc.SelectSingleNode("root/AtfVersion");
                if (node != null)
                    return new Version(node.InnerText);
            }

            return null;
        }

        /// <summary>
        /// Gets the assembly version for the main assembly, which is the assembly that contains
        /// the Main() function</summary>
        /// <returns>Version of the main (entry) assembly</returns>
        public static Version GetEntryAssemblyVersion()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            
            // Can be null if called from unmanaged code, like in UnitTests.
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();

            return assembly.GetName().Version;
        }
    }
}
