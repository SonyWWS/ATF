//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Xml;

namespace Sce.Atf
{
    /// <summary>
    /// Localizes strings by looking for *.Localization.xml files as embedded resources. These XML files
    /// must be of a compatible format and embedded in their assembly in the following namespace:
    /// {Project Namespace}.Resources.{language identifier}.{Custom name}.Localization.xml
    /// where {language identifier} is either a System.Globalization.CultureInfo.TwoLetterISOLanguageName
    /// or Name (e.g., "ja" or "ja-JP"). See the tool LocalizableStringExtractor for building these XML
    /// files.</summary>
    public class EmbeddedResourceStringLocalizer : XmlStringLocalizer
    {
        /// <summary>
        /// Constructor that searches for *.Localization.xml files in the language-specific
        /// sub-directory of the Resources sub-directory</summary>
        public EmbeddedResourceStringLocalizer()
        {
            // Get our current language and culture identifiers for the directory names.
            string language = Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName; //"en" or "ja"
            string culture = Thread.CurrentThread.CurrentUICulture.Name; //"en-US" or "ja-JP"

            // In the satellite assembly model, the English version is in the root directory plus our
            //  source code is written in English. No use doing a lot of searching for English translations.
            // Yes, this doesn't work if we port ATF to English-Australia. Unlikely!
            if (language == "en")
                return;

            // To speed things up, only check the GAC if this assembly has been installed in the GAC.
            // This allows us to skip mscorlib, etc.
            bool searchGAC = Assembly.GetExecutingAssembly().GlobalAssemblyCache;

            // Resources are named like "Some.Random.Namespace.Resources.ja.Another.Random.Name.Localization.xml".
            // Let's search for ".Resources.ja" to make sure we're locating the correct file.
            string resourceDirectory1 = ".Resources." + language + ".";
            string resourceDirectory2 = ".Resources." + culture + ".";

            foreach (Assembly assembly in AssemblyUtil.GetLoadedAssemblies())
            {
                //dynamically generated assemblies don't implement GetManifestResourceXxx()
                if (assembly is AssemblyBuilder)
                    continue;

                if (searchGAC ||
                    !assembly.GlobalAssemblyCache)
                {
                    foreach (string resourceName in assembly.GetManifestResourceNames())
                    {
                        if (IsEmbeddedResource(resourceName, resourceDirectory1, resourceDirectory2))
                        {
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.Load(assembly.GetManifestResourceStream(resourceName));
                            AddLocalizedStrings(xmlDoc);
                        }
                    }
                }
            }
        }

        // Searches a resource name that came from an assembly's manifest to see if it represents an
        //  embedded localization file and that it is in within one of the namespaces.
        private bool IsEmbeddedResource(string resourceName, string namespace1, string namespace2)
        {
            return
                resourceName.EndsWith(".Localization.xml") &&
                (
                    resourceName.Contains(namespace1) ||
                    resourceName.Contains(namespace2)
                );
        }
    }
}
