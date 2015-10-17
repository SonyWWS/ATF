//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Xml;

using Sce.Atf.Dom;

namespace Sce.Atf.Collada
{
    /// <summary>
    /// COLLADA XML file reader support</summary>
    class ColladaXmlPersister : DomXmlReader
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="loader">Type loader to translate element names to DOM node types</param>
        public ColladaXmlPersister(XmlSchemaTypeLoader loader)
            : base(loader)
        {

        }

        /// <summary>
        /// Gets the root element metadata for the reader's current XML node</summary>
        /// <param name="reader">XML reader</param>
        /// <param name="rootUri">URI of XML data</param>
        /// <returns>Root element metadata for the reader's current XML node</returns>
        protected override ChildInfo CreateRootElement(XmlReader reader, Uri rootUri)
        {
            ColladaSchemaTypeLoader colladaSchemaTypeLoader = TypeLoader as ColladaSchemaTypeLoader;

            if (colladaSchemaTypeLoader == null)
                return base.CreateRootElement(reader, rootUri);

            XmlQualifiedName rootElementName =
                new XmlQualifiedName(reader.LocalName, colladaSchemaTypeLoader.Namespace);

            return TypeLoader.GetRootElement(rootElementName.ToString());
        }

        /// <summary>
        /// Determines if attribute is a reference</summary>
        /// <param name="attributeInfo">Attribute</param>
        /// <returns><c>True</c> if attribute is reference</returns>
        protected override bool IsReferenceAttribute(AttributeInfo attributeInfo)
        {
            return base.IsReferenceAttribute(attributeInfo) || attributeInfo.Type.Name.EndsWith("anyURI");
        }
    }
}
