//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Class to read DOM data defined by an XML schema</summary>
    public class DomXmlReader
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="typeLoader">Type loader to translate element names to DOM node types</param>
        public DomXmlReader(XmlSchemaTypeLoader typeLoader)
        {
            m_typeLoader = typeLoader;
        }

        /// <summary>
        /// Gets the type loader that defines DOM node types</summary>
        public XmlSchemaTypeLoader TypeLoader
        {
            get { return m_typeLoader; }
        }

        /// <summary>
        /// Gets the URI for the current read</summary>
        public Uri Uri
        {
            get { return m_uri; }
        }

        /// <summary>
        /// Gets the root node for the current read</summary>
        public DomNode Root
        {
            get { return m_root; }
        }

        /// <summary>
        /// Gets dictionary with keys for DomNodes</summary>
        public IDictionary<string, DomNode> NodeDictionary
        {
            get { return m_nodeDictionary; }
        }

        /// <summary>
        /// Gets an enumeration of unresolved XML node references</summary>
        public IEnumerable<XmlNodeReference> UnresolvedReferences
        {
            get { return m_nodeReferences; }

            protected set { m_nodeReferences = value.ToList(); }
        }

        /// <summary>
        /// Reads a node tree from a stream</summary>
        /// <param name="stream">Read stream</param>
        /// <param name="uri">URI of stream</param>
        /// <returns>Node tree, from stream</returns>
        public virtual DomNode Read(Stream stream, Uri uri)
        {
            m_uri = uri;
            m_root = null;
            m_nodeDictionary.Clear();
            m_nodeReferences.Clear();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            //settings.IgnoreWhitespace = true;

            using (XmlReader reader = XmlReader.Create(stream, settings))
            {
                reader.MoveToContent();

                ChildInfo rootElement = CreateRootElement(reader, m_uri);
                if (rootElement == null)
                    throw new InvalidOperationException(
                        "No root element was found in the XML document, probably " +
                        "due to a namespace mismatch with the schema file");

                m_root = ReadElement(rootElement, reader);

                ResolveReferences();
            }

            return m_root;
        }

        /// <summary>
        /// Gets the root element metadata for the reader's current XML node</summary>
        /// <param name="reader">XML reader</param>
        /// <param name="rootUri">URI of XML data</param>
        /// <returns>Root element metadata for the reader's current XML node</returns>
        protected virtual ChildInfo CreateRootElement(XmlReader reader, Uri rootUri)
        {
            string ns = reader.NamespaceURI;
            if (string.IsNullOrEmpty(ns))
            {
                // no xmlns declaration in the file, so grab the first type collection's target namespace
                foreach (XmlSchemaTypeCollection typeCollection in m_typeLoader.GetTypeCollections())
                {
                    ns = typeCollection.DefaultNamespace;
                    break;
                }
            }

            ChildInfo rootElement = m_typeLoader.GetRootElement(ns + ":" + reader.LocalName);
            return rootElement;
        }


        /// <summary>
        /// Gets NodeReferences. A subclass needs this property to process attributes.</summary>
        protected IList<XmlNodeReference> NodeReferences
        {
            get { return m_nodeReferences; }
        }
        /// <summary>
        /// Converts the give string to attribute value and set it to given node using attributeInfo</summary>
        /// <param name="node">DomNode </param>
        /// <param name="attributeInfo">attributeInfo to set</param>
        /// <param name="valueString">The string representation of the attribute value</param>
        protected virtual void ReadAttribute(DomNode node, AttributeInfo attributeInfo, string valueString)
        {            
            if (IsReferenceAttribute(attributeInfo))
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
       
        /// <summary>
        /// Reads the node specified by the child metadata</summary>
        /// <param name="nodeInfo">Child metadata for node</param>
        /// <param name="reader">XML reader</param>
        /// <returns>DomNode specified by the child metadata</returns>
        protected virtual DomNode ReadElement(ChildInfo nodeInfo, XmlReader reader)
        {
            // handle polymorphism, if necessary
            DomNodeType type = null;
            var substitutionGroupRule = nodeInfo.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault();
            if (substitutionGroupRule != null)
            {
                foreach (var sub in substitutionGroupRule.Substitutions)
                {
                    if (sub.Name == reader.LocalName)
                    {
                        type = sub.Type;
                        break;
                    }
                }

                // Fallback to non-substituted version (for example loading an old schema).
                if (type == null)
                    type = GetChildType(nodeInfo.Type, reader);

                if (type == null)
                    throw new InvalidOperationException("Could not match substitution group for child " + nodeInfo.Name);
            }
            else
            {
                type = GetChildType(nodeInfo.Type, reader);
            }

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
                        ReadAttribute(node, attributeInfo, reader.Value);
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
                        if (childInfo == null)
                        {
                            // Try and get substitution group
                            childInfo = GetSubsitutionGroup(type, reader.LocalName);
                        }

                        if (childInfo != null)
                        {
                            DomNode childNode = ReadElement(childInfo, reader);
                            if (childNode != null)
                            {
                                // childNode is fully populated sub-tree
                                if (childInfo.IsList)
                                {
                                    node.GetChildList(childInfo).Add(childNode);
                                }
                                else
                                {
                                    node.SetChild(childInfo, childNode);
                                }
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
                                            ReadAttribute(node, attributeInfo, reader.Value);                                            
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
                            ReadAttribute(node, attributeInfo, reader.Value);                                                      
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

        private ChildInfo GetSubsitutionGroup(DomNodeType type, string localName)
        {
            foreach (var childInfo in type.Children)
            {
                var substitutionGroupRule = childInfo.Rules.OfType<SubstitutionGroupChildRule>().FirstOrDefault();
                if (substitutionGroupRule != null)
                {
                    // This is a candidate group
                    foreach (var substitutechildInfo in substitutionGroupRule.Substitutions)
                    {
                        // If name of substitutechildInfo matches localName then return childInfo
                        if (substitutechildInfo.Name == localName)
                        {
                            return childInfo;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Determines if attribute is a reference</summary>
        /// <param name="attributeInfo">Attribute</param>
        /// <returns><c>True</c> if attribute is reference</returns>
        protected virtual bool IsReferenceAttribute(AttributeInfo attributeInfo)
        {
            return (attributeInfo.Type.Type == AttributeTypes.Reference);
        }

        /// <summary>
        /// Gets a derived node type, given a base type, namespace, and type name</summary>
        /// <param name="baseType">Base node type</param>
        /// <param name="ns">Type namespace</param>
        /// <param name="typeName">Type name</param>
        /// <returns>Derived node type</returns>
        protected virtual DomNodeType GetDerivedType(DomNodeType baseType, string ns, string typeName)
        {
            return m_typeLoader.GetNodeType(ns + ":" + typeName);
        }

        /// <summary>
        /// Resolves XML node references</summary>
        protected virtual void ResolveReferences()
        {
            List<XmlNodeReference> unresolved = new List<XmlNodeReference>();
            foreach (XmlNodeReference nodeReference in m_nodeReferences)
            {
                // ID fixup ported from ATF 2 DomXmlResolver.Resolve(DomUri)
                string id = nodeReference.Value.TrimStart('#');
                id = Uri.UnescapeDataString(id); // remove escape characters
                id = id.TrimStart(s_trimChars);
                
                DomNode refNode;
                if (m_nodeDictionary.TryGetValue(id, out refNode))
                {
                    nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, refNode);
                }
                else
                {
                    unresolved.Add(nodeReference);
                    object value = nodeReference.AttributeInfo.Type.Convert(nodeReference.Value);
                    nodeReference.Node.SetAttribute(nodeReference.AttributeInfo, value);
                }
            }

            m_nodeReferences = unresolved;            
        }

        /// <summary>
        /// Gets node type of child of a node type</summary>
        /// <param name="type">Node type</param>
        /// <param name="reader">XML reader</param>
        /// <returns>Child's node type</returns>
        protected DomNodeType GetChildType(DomNodeType type, XmlReader reader)
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

                result = GetDerivedType(result, ns, typeName);

                if (result == null)
                {
                    string baseTypeName = type != null ? type.Name : "<none>";
                    throw new InvalidOperationException(string.Format(
                        "No type was found with the name {0} in namespace {1} that derives from {2}", typeName, ns, baseTypeName));
                }
            }

            return result;
        }

        private static readonly char[] s_trimChars = new[] { '|' };

        private readonly XmlSchemaTypeLoader m_typeLoader;

        private DomNode m_root;
        private Uri m_uri;

        private readonly Dictionary<string, DomNode> m_nodeDictionary = new Dictionary<string,DomNode>();
        private List<XmlNodeReference> m_nodeReferences = new List<XmlNodeReference>();
    }
}
