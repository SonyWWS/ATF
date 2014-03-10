using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using Sce.Atf.Dom;

namespace DomGen
{
    /// <summary>
    /// Schema loader capturing sce.domgen annotations</summary>
    public class SchemaLoader : XmlSchemaTypeLoader
    {
        /// <summary>
        /// Parses schema set annotations and captures the sce.domgen annotation XmlNodes
        /// to a dictionary indext by type name</summary>
        /// <param name="schemaSet">Xml schema set</param>
        /// <param name="annotations">Local dictionary to write annotations to.
        /// This is NOT the same as the DomGenAnnotations dictionary.</param>
        protected override void ParseAnnotations(XmlSchemaSet schemaSet, IDictionary<NamedMetadata, IList<XmlNode>> annotations)
        {
            base.ParseAnnotations(schemaSet, annotations);

            DomGenAnnotations = new Dictionary<string, XmlNode>();
            foreach (var annotation in annotations)
            {
                string typeName = annotation.Key.Name;
                foreach (XmlNode xmlNode in annotation.Value)
                {
                    if (xmlNode.Name == "sce.domgen")
                        DomGenAnnotations.Add(typeName, xmlNode);
                }
            }
        }

        /// <summary>
        /// Gets a dictionary of DomGen annotations</summary>
        /// <remarks>The key is the typeName (DomNodeType.Name) and value 
        /// is the sce.domgen annotation Xml node for this type if one exists,
        /// null otherwise. Types without sce.domgen annotation are not
        /// included in this dictionary.</remarks>
        public IDictionary<string, XmlNode> DomGenAnnotations { get; private set; }
    }
}
