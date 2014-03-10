//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Class to write DOM data defined by an XML schema</summary>
    public class DomXmlWriter
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="typeCollection">Type collection to translate element names to NodeTypes</param>
        public DomXmlWriter(XmlSchemaTypeCollection typeCollection)
        {
            m_typeCollection = typeCollection;
        }

        /// <summary>
        /// Gets the collection that defines DOM node types</summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }

        /// <summary>
        /// Gets the URI for the current write</summary>
        public Uri Uri
        {
            get { return m_uri; }
        }

        /// <summary>
        /// Gets the root node for the current write</summary>
        public DomNode Root
        {
            get { return m_root; }
        }

        /// <summary>
        /// Gets or sets whether XML elements that are a simple-type and that can occur only once
        /// in a sequence will be preserved as XML elements when the DOM is written to the XML file.
        /// If 'false', these XML elements will be persisted as XML attributes. This is the default
        /// behavior since ATF 3.0. If 'true', these XML elements that were loaded will be
        /// persisted as XML elements when saved. The default is 'false'.</summary>
        public bool PreserveSimpleElements
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether default XML attributes are persisted. Normally, persisting an
        /// attribute that is at its default value is not necessary, because the schema will provide
        /// the default value. But if other apps need to consume the XML file without the schema,
        /// you may need to write out these attributes. The default value is 'false'.</summary>
        public bool PersistDefaultAttributes
        {
            get;
            set;
        }

        /// <summary>
        /// Writes the node tree to a stream, using default settings for the XmlWriter</summary>
        /// <param name="root">Node tree to write</param>
        /// <param name="stream">Write stream</param>
        /// <param name="uri">URI of stream</param>
        public virtual void Write(DomNode root, Stream stream, Uri uri)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "\t";
            settings.NewLineHandling = NewLineHandling.Replace;
            settings.NewLineChars = "\r\n";
            Write(root, stream, uri, settings);
        }

        /// <summary>
        /// Writes the node tree to a stream</summary>
        /// <param name="root">Node tree to write</param>
        /// <param name="stream">Write stream</param>
        /// <param name="uri">URI of stream</param>
        /// <param name="settings">Settings for creating an XmlWriter. Specifies encoding, indent, etc.</param>
        public virtual void Write(DomNode root, Stream stream, Uri uri, XmlWriterSettings settings)
        {
            try
            {
                m_uri = uri;
                m_root = root;
                m_inlinePrefixes = new Dictionary<string, string>();

                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    writer.WriteStartDocument();

                    WriteElement(root, writer);

                    writer.WriteEndDocument();
                }
            }
            finally
            {
                m_uri = null;
                m_root = null;
                m_inlinePrefixes = null;
            }
        }

        /// <summary>
        /// Writes the element corresponding to the node</summary>
        /// <param name="node">DomNode to write</param>
        /// <param name="writer">The XML writer. See <see cref="T:System.Xml.XmlWriter"/></param>
        protected virtual void WriteElement(DomNode node, XmlWriter writer)
        {
            WriteStartElement(node, writer);
            WriteAttributes(node, writer);
            WriteChildElementsRecursive(node, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Starts writing an element for the given DomNode</summary>
        /// <param name="node">DomNode to write</param>
        /// <param name="writer">The XML writer. See <see cref="T:System.Xml.XmlWriter"/></param>
        protected void WriteStartElement(DomNode node, XmlWriter writer)
        {
            // It's possible to create DomNodes with no ChildInfo, and if the DomNode is never
            //  parented, then its ChildInfo property will still be null. We could try to search
            //  for a compatible root element in the m_typeCollection, but that's more code.
            if (node.ChildInfo == null)
                throw new InvalidOperationException(
                    "Please check your document's creation method to ensure that the root DomNode's" +
                    " constructor was given a ChildInfo.");
            // writes the start of an element
            m_elementNS = m_typeCollection.TargetNamespace;
            int index = node.ChildInfo.Type.Name.LastIndexOf(':');
            if (index >= 0)
                m_elementNS = node.ChildInfo.Type.Name.Substring(0, index);

            m_elementPrefix = string.Empty;

            // is this the root DomNode (the one passed to Write)?
            if (IsRootNode(node))
            {
                m_elementPrefix = m_typeCollection.GetPrefix(m_elementNS);
                if (m_elementPrefix == null)
                    m_elementPrefix = GeneratePrefix(m_elementNS);

                writer.WriteStartElement(m_elementPrefix, node.ChildInfo.Name, m_elementNS);

                // define the xsi namespace
                writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);

                // define schema namespaces
                foreach (XmlQualifiedName name in m_typeCollection.Namespaces)
                    if (name.Name != m_elementPrefix) // don't redefine the element namespace
                        writer.WriteAttributeString("xmlns", name.Name, null, name.Namespace);
            }
            else
            {
                ChildInfo actualChildInfo = node.ChildInfo;

                var substitutionGroupRule = node.ChildInfo.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault();
                if (substitutionGroupRule != null)
                {
                    var substituteChildInfo = substitutionGroupRule.Substitutions.FirstOrDefault(x => x.Type == node.Type);
                    if (substituteChildInfo == null)
                    {
                        throw new InvalidOperationException("No suitable Substitution Group found for node " + node);
                    }

                    actualChildInfo = substituteChildInfo;
                    m_elementNS = m_typeCollection.TargetNamespace;

                    index = substituteChildInfo.Type.Name.LastIndexOf(':');
                    if (index >= 0)
                        m_elementNS = substituteChildInfo.Type.Name.Substring(0, index);

                    // It is possible that an element of this namspace has not
                    // yet been written.  If the lookup fails then get the prefix from
                    // the type collection
                    m_elementPrefix = writer.LookupPrefix(m_elementNS);
                    if (m_elementPrefix == null)
                    {
                        m_elementPrefix = m_typeCollection.GetPrefix(m_elementNS);
                    }

                }
                else
                {
                    // not the root, so all schema namespaces have been defined
                    m_elementPrefix = writer.LookupPrefix(m_elementNS);
                }

                if (m_elementPrefix == null)
                    m_elementPrefix = GeneratePrefix(m_elementNS);

                writer.WriteStartElement(m_elementPrefix, actualChildInfo.Name, m_elementNS);
            }
        }

        /// <summary>
        /// Converts attribute to string.
        /// WriteAttributes(..) call this method to convert dom attribute to string before writing.</summary>
        /// <param name="node">DomNode that owns the attribute to be converted</param>
        /// <param name="attributeInfo">The attribute that need to be converted</param>
        /// <returns>the string value of the attribute</returns>
        protected virtual string Convert(DomNode node, AttributeInfo attributeInfo)
        {
            string valueString = null;
            object value = node.GetAttribute(attributeInfo);
            if (attributeInfo.Type.Type == AttributeTypes.Reference)
            {
                // if reference is a valid node, convert to string
                DomNode refNode = value as DomNode;
                if (refNode != null)
                    valueString = GetNodeReferenceString(refNode, m_root, m_uri);
            }
            if (valueString == null)
                valueString = attributeInfo.Type.Convert(value);

            return valueString;
        }
        /// <summary>
        /// Writes the attributes corresponding to the node</summary>
        /// <param name="node">DomNode to write</param>
        /// <param name="writer">The XML writer. See <see cref="T:System.Xml.XmlWriter"/></param>
        protected virtual void WriteAttributes(DomNode node, XmlWriter writer)
        {
            // write type name if this is a polymorphic type
            // if this node is substitution group element then ignore type name
            DomNodeType type = node.Type;
            if (node.ChildInfo.Type != type && node.ChildInfo.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault() == null)
            {
                string name = type.Name;
                int index = name.LastIndexOf(':');
                if (index >= 0)
                {
                    string typeName = name.Substring(index + 1, type.Name.Length - index - 1);
                    string typeNS = name.Substring(0, index);
                    string typePrefix = writer.LookupPrefix(typeNS);
                    if (typePrefix == null)
                    {
                        typePrefix = GeneratePrefix(typeNS);
                        writer.WriteAttributeString("xmlns", typePrefix, null, typeNS);
                    }

                    name = typeName;
                    if (typePrefix != string.Empty)
                        name = typePrefix + ":" + typeName;
                }

                writer.WriteAttributeString("xsi", "type", XmlSchema.InstanceNamespace, name);
            }

            // write attributes
            AttributeInfo valueAttribute = null;
            var attributesAsElements = new List<AttributeInfo>();
            foreach (AttributeInfo attributeInfo in type.Attributes)
            {
                // if attribute is not the default, write it
                if (ShouldWriteAttribute(node, attributeInfo))
                {
                    if (attributeInfo.Name == string.Empty)
                    {
                        valueAttribute = attributeInfo;
                    }
                    else
                    {
                        if (PreserveSimpleElements)
                        {
                            var xmlAttributeInfo = attributeInfo as XmlAttributeInfo;
                            if (xmlAttributeInfo != null &&
                                xmlAttributeInfo.IsElement)
                            {
                                attributesAsElements.Add(xmlAttributeInfo);
                                continue;
                            }
                        }

                        WriteXmlAttribute(node, attributeInfo, writer);
                    }
                }
            }

            // write value if not the default
            if (valueAttribute != null)
            {
                string valueString = Convert(node, valueAttribute);
                writer.WriteString(valueString);
            }

            // write DOM attributes that were originally XML elements of a simple type
            foreach (AttributeInfo info in attributesAsElements)
            {
                writer.WriteStartElement(m_elementPrefix, info.Name, m_elementNS);                
                string valueString = Convert(node, info);
                writer.WriteString(valueString);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Writes the given DomNode's attribute as an XML attribute</summary>
        /// <param name="node">DomNode whose attribute should be written</param>
        /// <param name="attributeInfo">Attribute info on the DomNode</param>
        /// <param name="writer">XML writer</param>
        /// <remarks>The default is to call Convert() to get a string representation of the DomNode's attribute
        /// and then to write that out using the AttributeInfo's Name as the XML local name.</remarks>
        protected virtual void WriteXmlAttribute(DomNode node, AttributeInfo attributeInfo, XmlWriter writer)
        {
            string valueString = Convert(node, attributeInfo);
            writer.WriteAttributeString(attributeInfo.Name, valueString);
        }

        /// <summary>
        /// Writes the child elements recursively</summary>
        /// <param name="node">DomNode to write</param>
        /// <param name="writer">The XML writer. See <see cref="T:System.Xml.XmlWriter"/>.</param>
        protected virtual void WriteChildElementsRecursive(DomNode node, XmlWriter writer)
        {
            // write child elements
            foreach (ChildInfo childInfo in node.Type.Children)
            {
                if (childInfo.IsList)
                {
                    foreach (DomNode child in node.GetChildList(childInfo))
                        WriteElement(child, writer);
                }
                else
                {
                    DomNode child = node.GetChild(childInfo);
                    if (child != null)
                        WriteElement(child, writer);
                }
            }
        }
        /// <summary>
        /// Converts a node to a string reference when writing</summary>
        /// <param name="refNode">Node that is referenced</param>
        /// <param name="root">Root node of data that is being written</param>
        /// <param name="uri">URI of data that is being written</param>
        /// <returns>String encoding the reference to the node</returns>
        protected virtual string GetNodeReferenceString(DomNode refNode, DomNode root, Uri uri)
        {
            string id = refNode.GetId();

            // if referenced node is in another resource, prepend URI
            if (!refNode.IsDescendantOf(root))
            {
                DomNode nodeRoot = refNode.GetRoot();
                IResource resource = nodeRoot.As<IResource>();
                if (resource != null)
                {
                    Uri relativeUri = uri.MakeRelativeUri(resource.Uri);
                    id = relativeUri + "#" + id;
                }
            }

            return id;
        }

        /// <summary>
        /// Gets whether or not the given DomNode should be considered the root node for this XML document</summary>
        /// <param name="node">DomNode to test</param>
        /// <returns>True iff given DomNode should be considered the root node</returns>
        protected virtual bool IsRootNode(DomNode node)
        {
            return node == m_root;
        }

        /// <summary>
        /// Determines whether the attribute's value should be persisted</summary>
        /// <returns>true if the attribute's value should be persisted; otherwise, false.</returns>
        protected virtual bool ShouldWriteAttribute(DomNode node, AttributeInfo attributeInfo)
        {
            return PersistDefaultAttributes || !node.IsAttributeDefault(attributeInfo);
        }

        /// <summary>
        /// Gets a prefix suitable for passing to XmlWriter.WriteStartElement(). Is only used if the namespace
        /// is not the default namespace and if the namespace is not known to the Sce.Atf.Dom.XmlSchemaTypeCollection.</summary>
        /// <param name="ns">Namespace name, e.g., "timeline"</param>
        /// <returns>Prefix, e.g., "_p2"</returns>
        protected string GeneratePrefix(string ns)
        {
            string prefix = null;
            if (!string.IsNullOrEmpty(ns))
            {
                if (!m_inlinePrefixes.TryGetValue(ns, out prefix))
                {
                    int suffix = m_inlinePrefixes.Count;
                    prefix = "_p" + suffix;
                    m_inlinePrefixes.Add(ns, prefix);
                }
            }

            return prefix;
        }

        private readonly XmlSchemaTypeCollection m_typeCollection;

        private DomNode m_root;
        private Uri m_uri;
        private Dictionary<string, string> m_inlinePrefixes;
        private string m_elementNS;
        private string m_elementPrefix;

    }
}
