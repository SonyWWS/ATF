//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Xml;

using Sce.Atf.Dom;

namespace Sce.Atf.Atgi
{
    /// <summary>
    /// ATGI XML file reader support.
    /// In order to pretend to match the version of the ATGI universal schema file (atgi.xsd)
    /// to any ATGI XML file, we need to override the Read method of the DomXmlPersister.
    /// By doing this, the universal ATGI plug-in can read any ATGI file using the atgi.xsd
    /// schema, even if the namespaces do not match.</summary>
    public class AtgiXmlPersister : DomXmlReader
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="loader">Type loader to translate element names to DOM node types</param>
        public AtgiXmlPersister(XmlSchemaTypeLoader loader)
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
            // ignore the ATGI version in the document, and use the loaded ATGI schema instead
            AtgiSchemaTypeLoader atgiSchemaTypeLoader = TypeLoader as AtgiSchemaTypeLoader;
            if (atgiSchemaTypeLoader != null)
            {
                XmlQualifiedName rootElementName =
                    new XmlQualifiedName(reader.LocalName, atgiSchemaTypeLoader.Namespace);
                ChildInfo rootElement = TypeLoader.GetRootElement(rootElementName.ToString());
                // ID passed to TypeLoader.GetRootElement must be same format as in XmlSchemaTypeLoader.Load(XmlSchemaSet)
                // In XmlSchemaTypeLoader.cs look for "string name = element.QualifiedName.ToString();"
                return rootElement;
            }
            else
            {
                return base.CreateRootElement(reader, rootUri);
            }
        }

        /// <summary>
        /// Determines if attribute is a reference</summary>
        /// <param name="attributeInfo">Attribute</param>
        /// <returns>True iff attribute is reference</returns>
        protected override bool IsReferenceAttribute(Sce.Atf.Dom.AttributeInfo attributeInfo)
        {
            return base.IsReferenceAttribute(attributeInfo) || attributeInfo.Type.Name.EndsWith("aifPathType");
        }

        //protected override string ConvertToLocalForm(AttributeInfo attributeInfo, string uriString, Uri rootUri)
        //{
        //    if (m_relativeFilePaths)
        //    {
        //        if (attributeInfo == Schema.textureType.uriAttribute)
        //        {
        //            string prefix = Path.GetDirectoryName(rootUri.LocalPath);
        //            return prefix + "/" + uriString;
        //        }
        //    }
        //    return uriString;
        //}

        //protected override string ConvertToPersistentForm(AttributeInfo attributeInfo, string uriString, Uri rootUri)
        //{
        //    if (m_relativeFilePaths)
        //    {
        //        if (attributeInfo == Schema.textureType.uriAttribute)
        //        {
        //            string prefix = Path.GetDirectoryName(rootUri.LocalPath);
        //            return uriString.Remove(0, prefix.Length + 1);
        //        }
        //    }
        //    return uriString;
        //}

        ///// <summary>
        ///// Fix up node tree after reading</summary>
        ///// <param name="root">Root node</param>
        ///// <param name="uri">URI of XML</param>
        //protected override void OnAfterRead(DomNode root, Uri uri)
        //{
        //    // ATGI uses strings for some internal references, rather than xs:anyURI

        //    // build the id-to-node map
        //    Multimap<string, DomNode> idToNodeMap = new Multimap<string, DomNode>();
        //    foreach (DomNode node in root.Subtree)
        //    {
        //        // add node to map if it has an id
        //        if (node.Type.IdAttribute != null)
        //        {
        //            string id = node.GetId();
        //            if (!string.IsNullOrEmpty(id))
        //                idToNodeMap.Add(id, node); // binding ids aren't necessarily unique
        //        }
        //    }

        //    // attempt to resolve all references
        //    List<Pair<DomNode, AttributeInfo>> references = new List<Pair<DomNode, AttributeInfo>>();
        //    foreach (DomNode node in root.Subtree)
        //    {
        //        if (node.Type == Schema.primitives_binding.Type)
        //        {
        //            // primitive set bindings to datasets are local to the parent mesh
        //            DomNode meshNode = node.Parent.Parent; // Mesh->Primitives->Binding
        //            string sourceId = node.GetAttribute(Schema.primitives_binding.sourceAttribute).ToString();
        //            foreach (DomNode sourceNode in idToNodeMap.Find(sourceId))
        //            {
        //                if (sourceNode.Parent == meshNode)
        //                {
        //                    node.SetAttribute(Schema.primitives_binding.sourceAttribute, sourceNode);
        //                    break;
        //                }
        //            }
        //        }
        //        else if (node.Type == Schema.shaderType_binding.Type)
        //        {
        //            Resolve(node, Schema.shaderType_binding.sourceAttribute, idToNodeMap);
        //        }
        //        else if (node.Type == Schema.materialType_binding.Type)
        //        {
        //            Resolve(node, Schema.materialType_binding.sourceAttribute, idToNodeMap);
        //        }
        //        else if (node.Type == Schema.vertexArray_primitives.Type)
        //        {
        //            Resolve(node, Schema.vertexArray_primitives.shaderAttribute, idToNodeMap);
        //        }
        //    }
        //}

        //private bool Resolve(DomNode node, AttributeInfo refAttributeInfo, Multimap<string, DomNode> idToNodeMap)
        //{
        //    string sourceId = node.GetAttribute(refAttributeInfo).ToString();
        //    DomNode sourceNode = idToNodeMap.FindFirst(sourceId);
        //    if (sourceNode != null)
        //    {
        //        node.SetAttribute(refAttributeInfo, sourceNode);
        //        return true;
        //    }
        //    return false;
        //}
    }
}
