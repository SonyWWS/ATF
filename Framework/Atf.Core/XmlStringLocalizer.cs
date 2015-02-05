//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Xml;

namespace Sce.Atf
{
    /// <summary>
    /// An abstract base class that provides support for reading compatible XML documents that contain
    /// pairs of IDs and translated strings. Derives from StringLocalizer to provide support for
    /// translating strings. See the tool LocalizableStringExtractor for building these XML files.
    /// Derived classes provide a way of finding the XML documents. See EmbeddedResourceStringLocalizer
    /// and SatelliteAssemblyStringLocalizer.</summary>
    public abstract class XmlStringLocalizer : StringLocalizer
    {
        /// <summary>
        /// Returns a localized version of the user-readable string 's'</summary>
        /// <param name="s">The string to be localized</param>
        /// <param name="context">The context that the string is used in, to help guide a human translator
        /// or to disambiguate a word that might be translated differently in different contexts</param>
        /// <returns>The localized string or the input string 's' if no localized string could be found</returns>
        public override string Localize(string s, string context)
        {
            // look up a list of pairs of contexts and translations
            List<Tuple<string, string>> translations;
            if (m_translations.TryGetValue(s, out translations))
            {
                // Ignore context if there is only one translation
                if (translations.Count == 1)
                    return translations[0].Item2;

                // Look for an exact context match.
                int i = translations.FindIndex(pair => pair.Item1.Equals(context));
                if (i >= 0)
                    return translations[i].Item2;

                // The context in the database may be out of sync from the code. Any translation is better than none.
                return translations[0].Item2;
            }

            return s;
        }

        /// <summary>
        /// Adds the IDs and localized strings from the given XML document</summary>
        /// <param name="xmlDoc">A compatible XML document containing pairs of IDs and translated strings</param>
        /// <exception cref="InvalidOperationException">If 'xmlDoc' is not compatible</exception>
        protected void AddLocalizedStrings(XmlDocument xmlDoc)
        {
            XmlElement root = xmlDoc.DocumentElement;
            if (root == null || root.Name != "StringLocalizationTable")
                throw new InvalidOperationException("invalid localization file: " + xmlDoc.BaseURI);

            var duplicates = new List<string>();
            foreach (XmlElement element in root.GetElementsByTagName("StringItem"))
            {
                string id = element.GetAttribute("id");
                string context = element.GetAttribute("context");
                string translation = element.GetAttribute("translation");

                // Empty translation means do not translate. ('translation' will never be null here.)
                if (String.IsNullOrEmpty(translation))
                    continue;

                // An asterisk substitutes to the id.
                if (translation == "*")
                    translation = id;

                List<Tuple<string, string>> translations;
                if (!m_translations.TryGetValue(id, out translations))
                    m_translations.Add(id, new List<Tuple<string, string>> {new Tuple<string, string>(context, translation)});
                else
                {
                    int i = translations.FindIndex(pair => pair.Item1.Equals(context));
                    if (i < 0)
                    {
                        // duplicate id, but with a new context, so add the pair.
                        translations.Add(new Tuple<string, string>(context, translation));
                    }
                    else if (translations[i].Item2 != translation)
                    {
                        // duplicate id and duplicate context, but not a duplicate translation. Error!
                        duplicates.Add(string.Format("1. \"{0}\", context: \"{1}\" => \"{2}\"", id, context, translations[i].Item2));
                        duplicates.Add(string.Format("2. \"{0}\", context: \"{1}\" => \"{2}\"", id, context, translation));
                    }
                }
            }

            if (duplicates.Count > 0)
                throw new InvalidOperationException("Conflicting translations in a localized XML file: \n\t" + string.Join("\n\t", duplicates));
        }

        // Each original (English) string is associated with a list of pairs of contexts and translations,
        //  so that the same original string can be translated differently depending on the context.
        private readonly Dictionary<string, List<Tuple<string, string>>> m_translations =
            new Dictionary<string, List<Tuple<string, string>>>();
    }
}
