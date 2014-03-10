//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.IO;
using System.Xml;

namespace Sce.Atf
{
    /// <summary>
    /// Localizes strings by looking for *.Localization.xml files in a .NET satellite assembly
    /// directory pattern. See the tool LocalizableStringExtractor for building these XML files.</summary>
    /// <remarks>Consider embedding the XML files into your assemblies as resources and using
    /// EmbeddedResourceStringLocalizer instead.</remarks>
    public class SatelliteAssemblyStringLocalizer : XmlStringLocalizer
    {
        /// <summary>
        /// Constructor that searches for *.Localization.xml files in the language-specific
        /// sub-directory of the Resources sub-directory</summary>
        public SatelliteAssemblyStringLocalizer()
        {
            string localizedDir = AppDomain.CurrentDomain.BaseDirectory;
            localizedDir = Path.Combine(localizedDir, "Resources\\");
            localizedDir = PathUtil.GetCulturePath(localizedDir);
            try
            {
                foreach (string path in Directory.GetFiles(localizedDir, "*.Localization.xml", SearchOption.TopDirectoryOnly))
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(path);
                    AddLocalizedStrings(xmlDoc);
                }
            }
            catch(System.IO.IOException e)
            {
                Outputs.WriteLine(OutputMessageType.Warning, e.Message);
            }
        }
    }
}
