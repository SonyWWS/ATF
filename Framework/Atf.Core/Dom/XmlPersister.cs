//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Class to read and write DOM data defined by an XML schema. Use this persister
    /// for single documents without external node references.
    /// </summary>
    public class XmlPersister
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="typeCollection">Type collection to translate element names to NodeTypes</param>
        public XmlPersister(XmlSchemaTypeCollection typeCollection)
        {
            m_typeCollection = typeCollection;
        }

        /// <summary>
        /// Gets the type collection used to translate from element names to DomNodeTypes</summary>
        public XmlSchemaTypeCollection TypeCollection
        {
            get { return m_typeCollection; }
        }

        /// <summary>
        /// Reads a node tree from a stream</summary>
        /// <param name="stream">Read stream</param>
        /// <param name="uri">URI of stream</param>
        /// <returns>Node tree read from stream</returns>
        public virtual DomNode Read(Stream stream, Uri uri)
        {
            DomNode root = null;
            XmlReader reader = null;

            try
            {
                m_uri = uri;

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreComments = true;
                settings.IgnoreProcessingInstructions = true;
                //settings.IgnoreWhitespace = true;

                reader = XmlReader.Create(stream, settings);

                reader.MoveToContent();

                ChildInfo rootElement = CreateRootElement(reader, m_uri);
                if (rootElement == null)
                    throw new InvalidOperationException("Unknown root element");

                m_nodeDictionary = new Dictionary<string, DomNode>();
                m_nodeReferences = new List<XmlNodeReference>();
                root = ReadElement(rootElement, reader);

                m_nodeDictionaries.Add(uri, m_nodeDictionary);
                ResolveInternalReferences(m_uri, root, m_nodeDictionary, m_nodeReferences);
            }
            finally
            {
                m_uri = null;
                m_nodeDictionary = null;
                m_nodeReferences = null;

                if (reader != null)
                    ((IDisposable)reader).Dispose();
            }

            return root;
        }

        /// <summary>
        /// Writes the node tree to a stream</summary>
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

            XmlWriter writer = null;
            try
            {
                m_uri = uri;

                writer = XmlWriter.Create(stream, settings);
                writer.WriteStartDocument();

                m_root = root;

                WriteElement(root, writer);

                writer.WriteEndDocument();
            }
            finally
            {
                if (writer != null)
                    ((IDisposable)writer).Dispose();

                m_uri = null;
                m_root = null;
            }
        }

        /// <summary>
        /// Gets the root element metadata for the reader's current XML node</summary>
        /// <param name="reader">XML reader</param>
        /// <param name="rootUri">URI of XML data</param>
        /// <returns>Root element metadata for the reader's current XML node</returns>
        protected virtual ChildInfo CreateRootElement(XmlReader reader, Uri rootUri)
        {
            ChildInfo rootElement = m_typeCollection.GetRootElement(reader.LocalName);
            return rootElement;
        }

        /// <summary>
        /// Reads the node specified by the child metadata</summary>
        /// <param name="nodeInfo">Child metadata for node</param>
        /// <param name="reader">XML reader</param>
        /// <returns>DomNode specified by the child metadata</returns>
        protected virtual DomNode ReadElement(ChildInfo nodeInfo, XmlReader reader)
        {
            // handle polymorphism, if necessary
            DomNodeType type = GetChildType(nodeInfo.Type, reader);
            int index = type.Name.LastIndexOf(':');
            string typeNS = type.Name.Substring(0, index);

            DomNode node = new DomNode(type, nodeInfo);

            // read attributes
            while (reader.MoveToNextAttribute())
            {
                if (reader.Prefix == string.Empty ||
                    reader.LookupNamespace(reader.Prefix) == typeNS)
                {
                    AttributeInfo attributeInfo = type.GetAttributeInfo(reader.LocalName);
                    if (attributeInfo != null)
                    {
                        string valueString = reader.Value;
                        if (attributeInfo.Type.Type == AttributeTypes.Reference)
                        {
                            // save reference so it can be resolved after all nodes have been read
                            m_nodeReferences.Add(new XmlNodeReference(node, attributeInfo, valueString));
                        }
                        else
                        {
                            object value = attributeInfo.Type.Convert(valueString);
                            node.SetAttribute(attributeInfo, value);
                        }
                    }
                }
            }

            // add node to map if it has an id
            if (node.Type.IdAttribute != null)
            {
                string id = node.GetId();
                if (!string.IsNullOrEmpty(id))
                    m_nodeDictionary[id] = node; // don't Add, in case there are multiple DomNodes with the same id
            }

            reader.MoveToElement();

            if (!reader.IsEmptyElement)
            {
                // read child elements
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // look up metadata for this element
                        ChildInfo childInfo = type.GetChildInfo(reader.LocalName);
                        if (childInfo != null)
                        {
                            DomNode childObject = ReadElement(childInfo, reader);
                            // at this point, child is a fully populated sub-tree

                            if (childInfo.IsList)
                            {
                                node.GetChildList(childInfo).Add(childObject);
                            }
                            else
                            {
                                node.SetChild(childInfo, childObject);
                            }
                        }
                        else
                        {
                            // try reading as an attribute
                            AttributeInfo attributeInfo = type.GetAttributeInfo(reader.LocalName);
                            if (attributeInfo != null)
                            {
                                reader.MoveToElement();

                                if (!reader.IsEmptyElement)
                                {
                                    // read element text
                                    while (reader.Read())
                                    {
                                        if (reader.NodeType == XmlNodeType.Text)
                                        {
                                            object value = attributeInfo.Type.Convert(reader.Value);
                                            node.SetAttribute(attributeInfo, value);
                                            // skip child elements, as this is an attribute value
                                            reader.Skip();
                                            break;
                                        }
                                        if (reader.NodeType == XmlNodeType.EndElement)
                                        {
                                            break;
                                        }
                                    }

                                    reader.MoveToContent();
                                }
                            }
                            else
                            {
                                // skip unrecognized element
                                reader.Skip();
                                // if that takes us to the end of the enclosing element, break
                                if (reader.NodeType == XmlNodeType.EndElement)
                                    break;
                            }
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        AttributeInfo attributeInfo = type.GetAttributeInfo(string.Empty);
                        if (attributeInfo != null)
                        {
                            object value = attributeInfo.Type.Convert(reader.Value);
                            node.SetAttribute(attributeInfo, value);
                        }
                    }
                    else if (reader.NodeType == XmlNodeType.EndElement)
                    {
                        break;
                    }
                }
            }

            reader.MoveToContent();

            return node;
        }

        /// <summary>
        /// Writes the element corresponding to the DomNode</summary>
        /// <param name="node">DomNode to write</param>
        protected virtual void WriteElement(DomNode node, XmlWriter writer)
        {
            string elementNS = m_typeCollection.TargetNamespace;
            int index = node.ChildInfo.Name.LastIndexOf(':');
            if (index >= 0)
                elementNS = node.ChildInfo.Name.Substring(0, index);

            string elementPrefix = string.Empty;

            // is this the root DomNode?
            if (node.Parent == null)
            {
                elementPrefix = m_typeCollection.GetPrefix(elementNS);
                if (elementPrefix == null)
                    elementPrefix = GeneratePrefix(elementNS);

                writer.WriteStartElement(elementPrefix, node.ChildInfo.Name, elementNS);

                // define the xsi namespace
                writer.WriteAttributeString("xmlns", "xsi", null, XmlSchema.InstanceNamespace);

                // define schema namespaces
                foreach (XmlQualifiedName name in m_typeCollection.Namespaces)
                    if (name.Name != elementPrefix) // don't redefine the element namespace
                        writer.WriteAttributeString("xmlns", name.Name, null, name.Namespace);
            }
            else
            {
                // not the root, so all schema namespaces have been defined
                elementPrefix = writer.LookupPrefix(elementNS);
                if (elementPrefix == null)
                    elementPrefix = GeneratePrefix(elementNS);

                writer.WriteStartElement(elementPrefix, node.ChildInfo.Name, elementNS);
            }

            // write type name if this is a polymorphic type
            DomNodeType type = node.Type;
            if (node.ChildInfo.Type != type)
            {
                string name = type.Name;
                index = name.LastIndexOf(':');
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
            foreach (AttributeInfo attributeInfo in type.Attributes)
            {
                // if attribute is required, or not the default, write it
                if (/*attributeInfo.Required ||*/ !node.IsAttributeDefault(attributeInfo))
                {
                    if (attributeInfo.Name == string.Empty)
                    {
                        valueAttribute = attributeInfo;
                    }
                    else
                    {
                        object value = node.GetAttribute(attributeInfo);
                        string valueString = null;
                        if (attributeInfo.Type.Type == AttributeTypes.Reference)
                        {
                            // if reference is a valid node, convert to string
                            DomNode refNode = value as DomNode;
                            if (refNode != null)
                                valueString = GetNodeReferenceString(refNode, m_root, m_uri);
                        }
                        if (valueString == null)
                            valueString = attributeInfo.Type.Convert(value);

                        writer.WriteAttributeString(attributeInfo.Name, valueString);
                    }
                }
            }

            // write value if not the default
            if (valueAttribute != null)
            {
                object value = node.GetAttribute(valueAttribute);
                writer.WriteString(valueAttribute.Type.Convert(value));
            }

            // write child elements
            foreach (ChildInfo childInfo in type.Children)
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

            writer.WriteEndElement();
        }

        /// <summary>
        /// Converts references from strings to DOM node references</summary>
        /// <param name="uri">URI that was read</param>
        /// <param name="root">Root node that was read</param>
        /// <param name="nodeDictionary">Dictionary that maps string ids to DOM nodes, in the DOM data that was read</param>
        /// <param name="nodeReferences">References to be converted</param>
        protected virtual void ResolveInternalReferences(
            Uri uri,
            DomNode root,
            IDictionary<string, DomNode> nodeDictionary,
            IEnumerable<XmlNodeReference> nodeReferences)
        {
            foreach (XmlNodeReference nodeReference in nodeReferences)
            {
                DomNode refNode;
                if (m_nodeDictionary.TryGetValue(nodeReference.Value, out refNode))
                {
                    nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, refNode);
                }
                else
                {
                    m_externalReferences.Add(nodeReference);
                }
            }
        }

        /// <summary>
        /// Resolves external references among references that couldn't be resolved locally</summary>
        public void ResolveExternalReferences()
        {
            foreach (XmlNodeReference nodeReference in m_externalReferences)
            {
                DomNode refNode = null;

                string value = nodeReference.Value;
                int fragmentIndex = value.LastIndexOf('#');
                if (fragmentIndex >= 0)
                {
                    string uriString = value.Substring(0, fragmentIndex);
                    value = value.Substring(fragmentIndex + 1);
                    Uri uri;
                    if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out uri))
                    {
                        IDictionary<string, DomNode> nodeDictionary;
                        if (m_nodeDictionaries.TryGetValue(uri, out nodeDictionary))
                        {
                            if (nodeDictionary.TryGetValue(value, out refNode))
                            {
                                nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, refNode);
                            }
                        }
                    }
                }

                if (refNode == null)
                    OnUnresolvedReference(nodeReference);
            }

            m_nodeDictionaries.Clear();
            m_externalReferences.Clear();
        }

        /// <summary>
        /// Handles unresolved references</summary>
        /// <param name="nodeReference">Node reference that can't be resolved to a DOM node</param>
        protected virtual void OnUnresolvedReference(XmlNodeReference nodeReference)
        {
        }

        /// <summary>
        /// Gets a derived node type given a base type, namespace, and type name</summary>
        /// <param name="baseType">Base node type</param>
        /// <param name="ns">Type namespace</param>
        /// <param name="typeName">Type name</param>
        /// <returns>Derived DomNodeType</returns>
        protected virtual DomNodeType GetDerivedType(DomNodeType baseType, string ns, string typeName)
        {
            return m_typeCollection.GetNodeType(typeName);
        }

        /// <summary>
        /// Converts a node to a string reference when writing DomNode</summary>
        /// <param name="refNode">Node that is referenced</param>
        /// <param name="root">Root node of data that is being written</param>
        /// <param name="uri">URI of data that is being written</param>
        /// <returns>String encoding the reference to the node</returns>
        protected virtual string GetNodeReferenceString(DomNode refNode, DomNode root, Uri uri)
        {
            string id = refNode.GetId();

            // if referenced node is in another resource, prepend URI
            DomNode nodeRoot = refNode.GetRoot();
            if (nodeRoot != root)
            {
                IResource resource = nodeRoot.As<IResource>();
                if (resource != null)
                    id = resource.Uri.LocalPath + "#" + id;
            }

            return id;
        }

        private string GeneratePrefix(string ns)
        {
            string prefix = null;
            if (!string.IsNullOrEmpty(ns))
            {
                if (!m_inlinePrefixes.TryGetValue(ns, out prefix))
                {
                    int suffix = m_inlinePrefixes.Count;
                    prefix = "_p" + suffix.ToString();
                    m_inlinePrefixes.Add(ns, prefix);
                }
            }

            return prefix;
        }

        private DomNodeType GetChildType(DomNodeType type, XmlReader reader)
        {
            DomNodeType result = type;

            // check for xsi:type attribute, for polymorphic elements
            string typeName = reader.GetAttribute("xsi:type");
            if (typeName != null)
            {
                // check for qualified type name
                string prefix = string.Empty;
                int index = typeName.IndexOf(':');
                if (index >= 0)
                {
                    prefix = typeName.Substring(0, index);
                    index++;
                    typeName = typeName.Substring(index, typeName.Length - index);
                }
                string ns = reader.LookupNamespace(prefix);

                result = GetDerivedType(result, prefix, typeName);

                if (result == null)
                    throw new InvalidOperationException("Unknown derived type");
            }

            return result;
        }

        private XmlSchemaTypeCollection m_typeCollection;

        private DomNode m_root;
        private Uri m_uri;

        private Dictionary<string, string> m_inlinePrefixes =
            new Dictionary<string, string>();

        private Dictionary<string, DomNode> m_nodeDictionary;
        private List<XmlNodeReference> m_nodeReferences;

        private Dictionary<Uri, IDictionary<string, DomNode>> m_nodeDictionaries =
            new Dictionary<Uri, IDictionary<string, DomNode>>();

        private List<XmlNodeReference> m_externalReferences =
            new List<XmlNodeReference>();
    }
}
