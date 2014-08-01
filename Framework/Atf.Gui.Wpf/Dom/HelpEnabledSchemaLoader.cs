//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Wpf.Dom
{
    /// <summary>
    /// Class for working with objects that provide context sensitive help
    /// </summary>
    public static class HelpAnnotations
    {
        /// <summary>
        /// Parses the atf.wpf.help.key annotation
        /// </summary>
        /// <param name="schemaSet">Schema to parse</param>
        /// <param name="annotations">List of the help annotations found in the schema set</param>
        public static void ParseAnnotations(XmlSchemaSet schemaSet, IDictionary<NamedMetadata, IList<XmlNode>> annotations)
        {
            foreach (var pair in annotations)
            {
                var keys = new List<string>();

                foreach (var annotation in pair.Value)
                {
                    if (annotation.Name == "atf.wpf.help.key")
                    {
                        keys.Add(annotation.Attributes["key"].Value);
                    }
                }

                if (keys.Count > 0)
                {
                    pair.Key.SetTag<IHelpContext>(new HelpContext(keys.ToArray()));
                }
            }
        }

        private class HelpContext : IHelpContext
        {
            public HelpContext(string[] keys)
            {
                m_keys = keys;
            }

            #region IHelpContext Members

            public string[] GetHelpKeys()
            {
                return m_keys;
            }

            #endregion

            private string[] m_keys;
        }
    }

}
