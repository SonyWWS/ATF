//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// XML schema loader that creates DomNodeTypes, parses annotations, and creates attribute rules</summary>
    public class XmlSchemaTypeLoader
    {
        /// <summary>
        /// Gets and sets the schema resolver</summary>
        /// <remarks>If the resolver is not set, an XmlUrlResolver is used</remarks>
        public XmlResolver SchemaResolver
        {
            get { return m_schemaResolver; }
            set { m_schemaResolver = value; }
        }

        /// <summary>Loads and registers a schema, given a schema file name. Searches culture-specific
        /// subdirectories first.</summary>
        /// <param name="schemaFileName">Schema file name</param>
        /// <returns>XmlSchema read from the schema file</returns>
        public XmlSchema Load(string schemaFileName)
        {
            XmlSchema schema = null;

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.XmlResolver = m_schemaResolver;
            using (XmlReader xmlReader = XmlReader.Create(schemaFileName, xmlReaderSettings))
            {
                schema = XmlSchema.Read(xmlReader, null);
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                schemaSet.XmlResolver = m_schemaResolver; // so imported/included schemas resolve correctly
                schemaSet.Add(schema);
                Load(schemaSet);
            }

            return schema;
        }

        /// <summary>
        /// Converts schemas to NodeTypes, AttributeTypes, and root elements</summary>
        /// <param name="schemaSet">Schemas to register</param>
        public void Load(XmlSchemaSet schemaSet)
        {
            if (!schemaSet.IsCompiled)
                schemaSet.Compile();

            System.Collections.ICollection schemas = schemaSet.Schemas();
            foreach (XmlSchema schema in schemas)
            {
                string targetNamespace = schema.TargetNamespace;
                if (string.IsNullOrEmpty(targetNamespace))
                    throw new InvalidOperationException("Schema has no target namespace");

                // only register the schema once; targetNamespaces must be globally unique
                if (!m_typeCollections.ContainsKey(targetNamespace))
                {
                    XmlQualifiedName[] nameSpaces = schema.Namespaces.ToArray();
                    XmlSchemaTypeCollection typeCollection = new XmlSchemaTypeCollection(nameSpaces, targetNamespace, this);
                    m_typeCollections.Add(targetNamespace, typeCollection);
                }
            }

            try
            {
                m_annotations = new Dictionary<NamedMetadata, IList<XmlNode>>();
                m_typeNameSet = new HashSet<string>();
                m_localElementSet = new Dictionary<XmlSchemaElement, XmlQualifiedName>();
                // collect global element & type names so we do not generate local type names that collides with those
                foreach (XmlSchemaElement element in schemaSet.GlobalElements.Values)
                    m_typeNameSet.Add(element.QualifiedName.Name);

                foreach (XmlSchemaType type in schemaSet.GlobalTypes.Values)
                {
                    if (type is XmlSchemaComplexType)
                    {
                        m_typeNameSet.Add(type.Name);
                    }
                }

                var substitutionGroups = new Multimap<XmlQualifiedName, ChildInfo>();

                // Get types reachable from global elements
                foreach (XmlSchemaElement element in schemaSet.GlobalElements.Values)
                {
                    XmlSchemaType type = element.ElementSchemaType;
                    DomNodeType nodeType = GetNodeType(type, element);
                    ChildInfo childInfo = new ChildInfo(GetFieldName(element.QualifiedName), nodeType);
                    m_annotations.Add(childInfo, GetAnnotation(element));

                    // Keep list of substitution groups
                    if (!element.SubstitutionGroup.IsEmpty)
                    {
                        substitutionGroups.Add(element.SubstitutionGroup, childInfo);
                    }

                    // only add root elements once; root element names must be globally unique
                    string name = element.QualifiedName.ToString();
                    if (!m_rootElements.ContainsKey(name))
                    {
                        m_rootElements[name] = childInfo;
                    }
                }

                // Get global complex type definitions
                foreach (XmlSchemaType type in schemaSet.GlobalTypes.Values)
                {
                    if (type is XmlSchemaComplexType)
                    {
                        GetNodeType(type, null);
                    }
                }

                // Parse substitution groups
                foreach (var kvp in m_refElements)
                {
                    XmlQualifiedName refName = kvp.Value;
                    ChildInfo childInfo = kvp.Key;

                    var substitutions = CreateSubstitutions(substitutionGroups, refName).ToArray();
                    if (substitutions.Length > 0)
                    {
                        childInfo.AddRule(new SubstitutionGroupChildRule(substitutions));
                    }
                }

                // Preserve annotation from any types that were redefined
                foreach (XmlSchema schema in schemas)
                {
                    foreach (XmlSchemaObject schemaInclude in schema.Includes)
                    {
                        XmlSchemaRedefine schemaRedefine = schemaInclude as XmlSchemaRedefine;
                        if (schemaRedefine != null)
                            MergeRedefinedTypeAnnotations(schemaRedefine);
                    }
                }

                // Sort DomNodeTypes, so that base types are always before derived types
                // Bucket sort by depth in the inheritance tree 
                // Time: O(n * d) with n = number of DomNodeTypes, d = depth of inheritance tree
                var sortedTypes = new List<List<DomNodeType>>();
                foreach (DomNodeType type in GetNodeTypes())
                {
                    // Get inheritance depth of current type
                    int depth = 0;
                    DomNodeType curType = type;
                    while (curType != null && curType != DomNodeType.BaseOfAllTypes)
                    {
                        depth++;
                        curType = curType.BaseType;
                    }

                    // We don't need to merge annotations for BaseAllTypes (level 0)
                    // and its immediate child types (level 1)
                    int idx = depth - 2;
                    if (idx >= 0)
                    {
                        while (sortedTypes.Count <= idx)
                            sortedTypes.Add(new List<DomNodeType>());
                        sortedTypes[idx].Add(type);
                    }
                }

                // Merge type annotations with base type annotations
                foreach (var list in sortedTypes)
                {
                    foreach (DomNodeType type in list)
                    {
                        if (type.BaseType != null && type.BaseType != DomNodeType.BaseOfAllTypes)
                        {
                            IList<XmlNode> baseAnnotations;
                            IList<XmlNode> annotations;
                            if (m_annotations.TryGetValue(type.BaseType, out baseAnnotations)
                                && m_annotations.TryGetValue(type, out annotations))
                            {
                                // Call protected virtual merge method - allowing clients to define if & how annotations are being merged
                                IEnumerable<XmlNode> mergedAnnotations = MergeInheritedTypeAnnotations(baseAnnotations, annotations);
                                m_annotations[type] = mergedAnnotations as IList<XmlNode> ?? mergedAnnotations.ToList();
                            }
                        }
                    }
                }

                // Call before the DomNodeTypes are frozen. Note that iterating through Attributes or
                //  calling 'SetIdAttribute' freezes the attributes on DomNodeType.
                OnSchemaSetLoaded(schemaSet);

                // Set up ID attributes where xs:ID has been specified
                foreach (DomNodeType nodeType in GetNodeTypes())
                {
                    foreach (var attribute in nodeType.Attributes.OfType<XmlAttributeInfo>())
                    {
                        if (((XmlAttributeType)attribute.Type).XmlTypeCode == XmlTypeCode.Id)
                            nodeType.SetIdAttribute(attribute.Name);
                    }
                }

                // Attach annotation as metadata to the associated type so that other classes can find it
                foreach (var keyValuePair in m_annotations)
                {
                    if (keyValuePair.Value.Count > 0)
                    {
                        keyValuePair.Key.SetTag<IEnumerable<XmlNode>>(keyValuePair.Value); 
                    }
                }
                ParseAnnotations(schemaSet, m_annotations);

                // Call this after the ID attributes have been set and after the DomNodeTypes are frozen.
                OnDomNodeTypesFrozen(schemaSet);
            }
            finally
            {
                m_annotations = null;
                m_typeNameSet = null;
                m_localElementSet = null;
            }
        }

        /// <summary>
        /// Searches XML nodes for an element</summary>
        /// <param name="xmlNodes">XML nodes to search</param>
        /// <param name="elementName">Element name to match</param>
        /// <returns>First XML node with the given name or null if not found</returns>
        public static XmlNode FindElement(IEnumerable<XmlNode> xmlNodes, string elementName)
        {
            foreach (XmlNode xmlNode in xmlNodes)
                if (xmlNode.LocalName == elementName)
                    return xmlNode;
            return null;
        }

        /// <summary>
        /// Searches XML nodes for an attribute on an element</summary>
        /// <param name="xmlNodes">XML nodes to search</param>
        /// <param name="elementName">Element name to match</param>
        /// <param name="attributeName">Attribute name to match</param>
        /// <returns>First attribute value on matching attribute/element or null if not found</returns>
        public static string FindAttribute(IEnumerable<XmlNode> xmlNodes, string elementName, string attributeName)
        {
            XmlNode xmlNode = FindElement(xmlNodes, elementName);
            if (xmlNode != null)
                return FindAttribute(xmlNode, attributeName);

            return null;
        }

        /// <summary>
        /// Searches for an attribute on an element</summary>
        /// <param name="xmlNode">XML node to search</param>
        /// <param name="attributeName">Attribute name to match</param>
        /// <returns>Attribute value or null</returns>
        public static string FindAttribute(XmlNode xmlNode, string attributeName)
        {
            XmlAttribute attribute = xmlNode.Attributes[attributeName];
            return attribute != null ? attribute.Value : null;
        }

        /// <summary>
        /// Gets the schema registered under the given target namespace</summary>
        /// <param name="targetNamespace">Target namespace</param>
        /// <returns>Schema registered under the given target namespace or null</returns>
        public XmlSchemaTypeCollection GetTypeCollection(string targetNamespace)
        {
            XmlSchemaTypeCollection schema;
            m_typeCollections.TryGetValue(targetNamespace, out schema);
            return schema;
        }

        /// <summary>
        /// Gets all schema type collections</summary>
        /// <returns>Enumeration of DOM metadata collections for schema types</returns>
        public IEnumerable<XmlSchemaTypeCollection> GetTypeCollections()
        {
            return m_typeCollections.Values;
        }

        /// <summary>
        /// Gets an attribute type</summary>
        /// <param name="name">Name of attribute type</param>
        /// <returns>Attribute type or null if unknown name</returns>
        public XmlAttributeType GetAttributeType(string name)
        {
            XmlAttributeType attributeType;
            m_attributeTypes.TryGetValue(name, out attributeType);
            return attributeType;
        }

        /// <summary>
        /// Gets attribute types for the given namespace</summary>
        /// <param name="ns">Namespace</param>
        /// <returns>Enumeration of all attribute types in the given namespace</returns>
        public IEnumerable<AttributeType> GetAttributeTypes(string ns)
        {
            ns += ":";
            foreach (KeyValuePair<string, XmlAttributeType> kvp in m_attributeTypes)
                if (kvp.Key.StartsWith(ns))
                    yield return kvp.Value;
        }

        /// <summary>
        /// Gets all attribute types</summary>
        /// <returns>Enumeration of all attribute types</returns>
        public IEnumerable<AttributeType> GetAttributeTypes()
        {
            foreach (XmlAttributeType attributeType in m_attributeTypes.Values)
                yield return attributeType;
        }

        /// <summary>
        /// Gets a node type</summary>
        /// <param name="name">Qualified name of node type</param>
        /// <returns>Node type or null if unknown name</returns>
        public DomNodeType GetNodeType(string name)
        {
            DomNodeType nodeType;
            m_nodeTypes.TryGetValue(name, out nodeType);
            return nodeType;
        }

        /// <summary>
        /// Gets node types for the given namespace</summary>
        /// <param name="ns">Namespace</param>
        /// <returns>Enumeration of all node types in the given namespace</returns>
        public IEnumerable<DomNodeType> GetNodeTypes(string ns)
        {
            ns += ":";
            foreach (KeyValuePair<string, DomNodeType> kvp in m_nodeTypes)
                if (kvp.Key.StartsWith(ns))
                    yield return kvp.Value;
        }

        /// <summary>
        /// Gets all node types derived from base type</summary>
        /// <param name="baseType">Base type</param>
        /// <remarks>Base type is not returned in the array.</remarks>
        /// <returns>Enumeration of all node types derived from baseType</returns>
        public IEnumerable<DomNodeType> GetNodeTypes(DomNodeType baseType)
        {
            if (baseType == null)
                throw new ArgumentNullException("baseType");

            foreach (DomNodeType type in m_nodeTypes.Values)
                if (type != baseType && baseType.IsAssignableFrom(type))
                    yield return type;
        }

        /// <summary>
        /// Gets all node types</summary>
        /// <returns>All node types</returns>
        public IEnumerable<DomNodeType> GetNodeTypes()
        {
            return m_nodeTypes.Values;
        }

        /// <summary>
        /// Adds a node type to the ones defined by the schema</summary>
        /// <remarks>If the node type is already defined, it is overwritten</remarks>
        /// <param name="name">Name of node type</param>
        /// <param name="type">New node type</param>
        public void AddNodeType(string name, DomNodeType type)
        {
            m_nodeTypes[name] = type;
        }

        /// <summary>
        /// Removes the node type with the specified name </summary>
        /// <param name="name">Name of node type</param>
        /// <returns>
        /// true if the node typ is successfully found and removed; otherwise, false.  
        /// This method returns false if name is not found in the defined node type.
        /// </returns>
        public bool RemoveNodeType(string name)
        {
           return m_nodeTypes.Remove(name);
        }

        /// <summary>
        /// Gets the root element with the given name</summary>
        /// <param name="name">Name of element</param>
        /// <returns>Child info for root element or null if unknown name</returns>
        public ChildInfo GetRootElement(string name)
        {
            ChildInfo childInfo;
            m_rootElements.TryGetValue(name, out childInfo);
            return childInfo;
        }

        /// <summary>
        /// Gets root elements for the given namespace</summary>
        /// <param name="ns">Namespace to match</param>
        /// <returns>Enumeration of child info for root elements in the given namespace</returns>
        public IEnumerable<ChildInfo> GetRootElements(string ns)
        {
            ns += ":";
            foreach (KeyValuePair<string, ChildInfo> kvp in m_rootElements)
                if (kvp.Key.StartsWith(ns))
                    yield return kvp.Value;
        }

        /// <summary>
        /// Gets all root elements</summary>
        /// <returns>Enumeration of child info for all root elements</returns>
        public IEnumerable<ChildInfo> GetRootElements()
        {
            return m_rootElements.Values;
        }

        /// <summary>
        /// Method called after the schema set has been loaded and the DomNodeTypes have been created, but
        /// before the DomNodeTypes have been frozen. This means that DomNodeType.SetIdAttribute, for example, has
        /// not been called on the DomNodeTypes. Is called shortly before OnDomNodeTypesFrozen.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected virtual void OnSchemaSetLoaded(XmlSchemaSet schemaSet)
        {
        }

        /// <summary>
        /// Is called after the schema set has been loaded and the DomNodeTypes have been frozen with
        /// their ID attributes set. Is called shortly after OnSchemaSetLoaded.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        protected virtual void OnDomNodeTypesFrozen(XmlSchemaSet schemaSet)
        {
        }

        /// <summary>
        /// Parses annotations in schema sets. Override this to handle custom annotations.</summary>
        /// <param name="schemaSet">XML schema sets being loaded</param>
        /// <param name="annotations">Dictionary of annotations in schema</param>
        protected virtual void ParseAnnotations(
            XmlSchemaSet schemaSet,
            IDictionary<NamedMetadata, IList<XmlNode>> annotations)
        {
            // Inspect root types for the legacy annotation specifying id attribute
            // Get types reachable from global elements
            foreach (XmlSchemaElement element in schemaSet.GlobalElements.Values)
            {
                ChildInfo childInfo = GetRootElement(element.QualifiedName.ToString());
                IList<XmlNode> xmlNodes;
                if (annotations.TryGetValue(childInfo.Type, out xmlNodes))
                {
                    string idAttribute = FindAttribute(xmlNodes, "idAttribute", "name");
                    if (idAttribute != null)
                    {
                        foreach (DomNodeType type in GetNodeTypes(element.QualifiedName.Namespace))
                            type.SetIdAttribute(idAttribute);
                    }
                }
            }
        }

        /// <summary>
        /// Converts a qualified name into a locally unique (on the type) field name</summary>
        /// <param name="qualifiedName">XML qualified name</param>
        /// <returns>Unique field name</returns>
        protected virtual string GetFieldName(XmlQualifiedName qualifiedName)
        {
            XmlSchemaTypeCollection typeCollection;
            if (m_typeCollections.TryGetValue(qualifiedName.Namespace, out typeCollection))
            {
                if (qualifiedName.Namespace == typeCollection.TargetNamespace)
                    return qualifiedName.Name;
            }

            return qualifiedName.ToString();
        }

        /// <summary>
        /// Gets the dictionary of annotations that are available during the call to
        /// Load(XmlSchemaSet).</summary>
        protected IDictionary<NamedMetadata, IList<XmlNode>> Annotations
        {
            get { return m_annotations; }
        }

        private IEnumerable<ChildInfo> CreateSubstitutions(Multimap<XmlQualifiedName, ChildInfo> substitutionGroups, XmlQualifiedName refName)
        {
            foreach (var group in substitutionGroups.Keys)
            {
                if (group == refName)
                {
                    var childInfos = substitutionGroups[group];
                    foreach (var childInfo in childInfos)
                    {
                        yield return childInfo;

                        var ns = string.Empty;
                        int index = childInfo.Type.Name.LastIndexOf(':');
                        if (index >= 0)
                            ns = childInfo.Type.Name.Substring(0, index);

                        var qualifiedName = new XmlQualifiedName(childInfo.Name, ns);

                        foreach (var sub in CreateSubstitutions(substitutionGroups, qualifiedName))
                            yield return sub;
                    }
                }
            }
        }


        #region Private Schema-to-Type Methods

        private XmlAttributeType GetAttributeType(XmlSchemaSimpleType simpleType)
        {
            XmlAttributeType attributeType;
            if (!m_attributeTypes.TryGetValue(simpleType.QualifiedName.ToString(), out attributeType))
            {
                bool simpleList = simpleType.Content is XmlSchemaSimpleTypeList;
                int length = 1;
                if (simpleList)
                    length = Int32.MaxValue; // unbounded, until restricted

                List<AttributeRule> rules = null;
                XmlSchemaSimpleTypeRestriction restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
                if (restriction != null)
                {
                    if (restriction.BaseTypeName != null)
                    {
                        XmlAttributeType baseType;
                        m_attributeTypes.TryGetValue(restriction.BaseTypeName.ToString(), out baseType);
                        if (baseType != null)
                            length = baseType.Length;
                    }

                    foreach (XmlSchemaFacet facet in restriction.Facets)
                    {
                        XmlSchemaLengthFacet lengthFacet = facet as XmlSchemaLengthFacet;
                        if (lengthFacet != null)
                        {
                            Int32.TryParse(lengthFacet.Value, out length);
                            continue;
                        }
                        // also handle minLength, maxLength facets in a limited fashion, ie. either one
                        //  will specify the array length
                        XmlSchemaMinLengthFacet minLengthFacet = facet as XmlSchemaMinLengthFacet;
                        if (minLengthFacet != null)
                        {
                            int minLength;
                            if (Int32.TryParse(minLengthFacet.Value, out minLength))
                                length = Math.Max(length, minLength);
                            continue;
                        }
                        XmlSchemaMaxLengthFacet maxLengthFacet = facet as XmlSchemaMaxLengthFacet;
                        if (maxLengthFacet != null)
                        {
                            int maxLength;
                            if (Int32.TryParse(maxLengthFacet.Value, out maxLength))
                                length = Math.Max(length, maxLength);
                            continue;
                        }
                    }

                    rules = GetRules(restriction);
                }

                string typeName = simpleType.QualifiedName.ToString();

                // if xs:IDREF, then the attribute type should be DomNode as this is a reference
                Type valueType = simpleType.Datatype.ValueType;
                XmlTypeCode xmlTypeCode = simpleType.Datatype.TypeCode;
                if (xmlTypeCode == XmlTypeCode.Idref)
                {
                    if (valueType.IsArray)
                        valueType = typeof(string[]);
                    else
                        valueType = typeof(DomNode);
                }

                // map xs:integer to xs:int (ATGI schema uses xs:integer, which we don't want to map to System.Decimal)
                if (xmlTypeCode == XmlTypeCode.Integer)
                {
                    if (valueType.IsArray)
                        valueType = typeof(Int32[]);
                    else
                        valueType = typeof(Int32);
                    xmlTypeCode = XmlTypeCode.Int;
                }
                else if (xmlTypeCode == XmlTypeCode.NonNegativeInteger)
                {
                    if (valueType.IsArray)
                        valueType = typeof(UInt32[]);
                    else
                        valueType = typeof(UInt32);
                    xmlTypeCode = XmlTypeCode.UnsignedInt;
                }

                // create our extended attribute type
                attributeType = new XmlAttributeType(typeName, valueType, length, xmlTypeCode);

                m_annotations.Add(attributeType, GetAnnotation(simpleType));

                if (rules != null)
                {
                    foreach (AttributeRule rule in rules)
                        attributeType.AddRule(rule);
                }

                if (!string.IsNullOrEmpty(typeName))
                    m_attributeTypes.Add(typeName, attributeType);
            }

            return attributeType;
        }

        private DomNodeType GetNodeType(
            XmlSchemaType type,
            XmlSchemaElement element)
        {
            XmlSchemaComplexType complexType = type as XmlSchemaComplexType;
            DomNodeType nodeType = null;
            if (complexType != null)
            {
                nodeType = GetNodeType(complexType, element);
            }
            else
            {
                // must be simple type
                XmlSchemaSimpleType simpleType = type as XmlSchemaSimpleType;
                bool firstTime;
                nodeType = WrapSimpleType(simpleType, out firstTime);
            }
            return nodeType;
        }

        private DomNodeType GetNodeType(XmlSchemaComplexType complexType, XmlSchemaElement element)
        {
            // get type name
            XmlQualifiedName name = complexType.QualifiedName;
            if (name.IsEmpty) // local type
                name = GetLocalTypeName(element);

            string typeName = name.ToString();
            DomNodeType nodeType;
            if (!m_nodeTypes.TryGetValue(typeName, out nodeType))
            {
                // build a new complex type and add it to the dictionary
                nodeType = new DomNodeType(typeName);
                m_nodeTypes.Add(typeName, nodeType);

                m_annotations.Add(nodeType, GetAnnotation(complexType));

                DomNodeType baseType = null;
                XmlAttributeType valueType = null;

                XmlSchemaComplexType complexBaseType = GetBaseType(complexType);
                if (complexBaseType != null)
                {
                    baseType = GetNodeType(complexBaseType, null);
                }

                XmlSchemaSimpleType simpleBase = complexType.BaseXmlSchemaType as XmlSchemaSimpleType;
                if (simpleBase != null)
                {
                    valueType = GetAttributeType(simpleBase);
                }
                else if (complexType.IsMixed)
                {
                    valueType = s_mixedTextFieldSimpleType;
                }

                WalkParticle(complexType.ContentTypeParticle, nodeType);

                if (valueType != null)
                {
                    XmlAttributeInfo attributeInfo = new XmlAttributeInfo(string.Empty, valueType);
                    nodeType.Define(attributeInfo);
                }

                // get XML attributes
                System.Collections.ICollection attributeUses = complexType.AttributeUses.Values;
                foreach (XmlSchemaAttribute attribute in attributeUses)
                {
                    XmlAttributeType attributeType = GetAttributeType(attribute.AttributeSchemaType);
                    string fieldName = GetFieldName(attribute.QualifiedName);
                    XmlAttributeInfo attributeInfo = new XmlAttributeInfo(fieldName, attributeType);

                    if (attribute.DefaultValue != null)
                        attributeInfo.DefaultValue = attributeType.Convert(attribute.DefaultValue);

                    m_annotations.Add(attributeInfo, GetAnnotation(attribute));

                    nodeType.Define(attributeInfo);
                }

                if (baseType != null)
                    nodeType.BaseType = baseType;

                nodeType.IsAbstract = complexType.IsAbstract;
            }

            return nodeType;
        }

        private void WalkParticle(XmlSchemaParticle particle, DomNodeType nodeType)
        {
            XmlSchemaElement element = particle as XmlSchemaElement;
            if (element != null)
            {
                XmlSchemaSimpleType simpleType = element.ElementSchemaType as XmlSchemaSimpleType;
                if (simpleType != null &&
                    element.MaxOccurs == 1)
                {
                    XmlAttributeType attributeType = GetAttributeType(simpleType);
                    string fieldName = GetFieldName(element.QualifiedName);
                    XmlAttributeInfo attributeInfo = new XmlAttributeInfo(fieldName, attributeType);

                    nodeType.Define(attributeInfo);
                    m_annotations.Add(attributeInfo, GetAnnotation(element));

                    attributeInfo.IsElement = true;

                    if (element.DefaultValue != null)
                    {
                        if (element.FixedValue != null)
                            throw new InvalidOperationException(string.Format("Schema element {0} cannot have both a default value and a fixed value", element.QualifiedName));
                        attributeInfo.DefaultValue = attributeType.Convert(element.DefaultValue);
                    }
                    else if (element.FixedValue != null)
                    {
                        attributeInfo.DefaultValue = attributeType.Convert(element.FixedValue);
                    }
                }
                else
                {
                    DomNodeType childNodeType = null;
                    if (simpleType != null)
                    {
                        bool firstTime;
                        childNodeType = WrapSimpleType(simpleType, out firstTime);

                        // The collada.xsd's ListOfUInts element breaks the generated Schema.cs file otherwise.
                        if (firstTime)
                        {
                            // Add the value attribute
                            XmlAttributeType valueAttributeType = GetAttributeType(simpleType);
                            var valueAttributeInfo = new XmlAttributeInfo(string.Empty, valueAttributeType);
                            childNodeType.Define(valueAttributeInfo);
                        }
                    }
                    else
                    {
                        XmlSchemaComplexType complexType = element.ElementSchemaType as XmlSchemaComplexType;
                        if (complexType != null)
                            childNodeType = GetNodeType(complexType, element);
                    }

                    if (childNodeType != null)
                    {
                        int minOccurs;
                        int maxOccurs;

                        // If <xs:choice> is within a <xs:sequence>, choose the most relaxed constraints.
                        if (particle.Parent is XmlSchemaChoice)
                        {
                            var parent = (XmlSchemaChoice)particle.Parent;
                            minOccurs = (int)Math.Min(Math.Min(element.MinOccurs, parent.MinOccurs), Int32.MaxValue);
                            maxOccurs = (int)Math.Min(Math.Max(element.MaxOccurs, parent.MaxOccurs), Int32.MaxValue);
                        }
                        else if (particle.Parent is XmlSchemaSequence)
                        {
                            var parent = (XmlSchemaSequence)particle.Parent;
                            minOccurs = (int)Math.Min(Math.Min(element.MinOccurs, parent.MinOccurs), Int32.MaxValue);
                            maxOccurs = (int)Math.Min(Math.Max(element.MaxOccurs, parent.MaxOccurs), Int32.MaxValue);
                        }
                        else
                        {
                            minOccurs = (int)Math.Min(element.MinOccurs, Int32.MaxValue);
                            maxOccurs = (int)Math.Min(element.MaxOccurs, Int32.MaxValue);
                        }

                        ChildInfo childInfo = new ChildInfo(GetFieldName(element.QualifiedName), childNodeType, maxOccurs > 1);

                        if (minOccurs > 0 || maxOccurs < Int32.MaxValue)
                        {
                            childInfo.AddRule(new ChildCountRule(minOccurs, maxOccurs));
                        }

                        // Check for substitution groups
                        if (!element.RefName.IsEmpty)
                        {
                            m_refElements.Add(childInfo, element.RefName);
                        }

                        nodeType.Define(childInfo);
                        m_annotations.Add(childInfo, GetAnnotation(element));
                    }
                }
            }
            else
            {
                // if sequence, continue collecting elements
                XmlSchemaSequence sequence = particle as XmlSchemaSequence;
                if (sequence != null)
                {
                    foreach (XmlSchemaParticle subParticle in sequence.Items)
                        WalkParticle(subParticle, nodeType);
                }
                else
                {
                    XmlSchemaChoice choice = particle as XmlSchemaChoice;
                    if (choice != null)
                    {
                        // for now, treat choice as if it were a sequence
                        foreach (XmlSchemaParticle subParticle in choice.Items)
                            WalkParticle(subParticle, nodeType);
                    }
                }
            }
        }

        // Wrap a simple type if it's a global or root element
        private DomNodeType WrapSimpleType(XmlSchemaSimpleType simpleType, out bool firstTime)
        {
            string typeName = simpleType.QualifiedName.ToString();
            DomNodeType nodeType;
            firstTime = false;
            if (!m_nodeTypes.TryGetValue(typeName, out nodeType))
            {
                firstTime = true;
                nodeType = new DomNodeType(typeName);
                m_nodeTypes.Add(typeName, nodeType);

                m_annotations.Add(nodeType, GetAnnotation(simpleType));
            }

            return nodeType;
        }

        private XmlSchemaComplexType GetBaseType(XmlSchemaComplexType type)
        {
            XmlSchemaComplexType baseType = type;
            while (
                baseType != null &&
                baseType.QualifiedName == type.QualifiedName)
            {
                baseType = baseType.BaseXmlSchemaType as XmlSchemaComplexType;
            }

            if (baseType != null &&
                baseType.QualifiedName == s_anyTypeName)
            {
                baseType = null;
            }

            return baseType;
        }

        private XmlQualifiedName GetLocalTypeName(XmlSchemaElement element)
        {
            XmlQualifiedName localTypeName;
            if (m_localElementSet.TryGetValue(element, out localTypeName))
                return localTypeName;

            if (element.QualifiedName == null ||
                element.RefName.Name == string.Empty)
            {
                XmlSchemaObject parent = element.Parent;
                string typeName = null;
                while (parent != null)
                {
                    XmlSchemaComplexType complexType = parent as XmlSchemaComplexType;
                    if (complexType != null)
                    {
                        string name = complexType.Name;
                        if (name == null)
                        {
                            XmlSchemaElement parentElement = (XmlSchemaElement)complexType.Parent;
                            name = parentElement.Name;
                        }

                        if (typeName == null)
                            typeName = element.Name;
                        typeName = name + "_" + typeName;

                        if (!m_typeNameSet.Contains(typeName))
                        {
                            localTypeName = new XmlQualifiedName(typeName, element.QualifiedName.Namespace);
                            m_localElementSet.Add(element, localTypeName);
                            m_typeNameSet.Add(typeName);
                            return localTypeName;
                        }
                    }

                    parent = parent.Parent;
                }
            }

            return element.QualifiedName;
        }

        private List<AttributeRule> GetRules(XmlSchemaSimpleTypeRestriction restriction)
        {
            List<AttributeRule> rules = new List<AttributeRule>();

            List<string> enumValues = null;
            foreach (XmlSchemaFacet facet in restriction.Facets)
            {
                XmlSchemaEnumerationFacet enumFacet = facet as XmlSchemaEnumerationFacet;
                if (enumFacet != null)
                {
                    if (enumValues == null)
                        enumValues = new List<string>();
                    enumValues.Add(enumFacet.Value);
                    continue;
                }

                XmlSchemaMinExclusiveFacet minExclusiveFacet = facet as XmlSchemaMinExclusiveFacet;
                if (minExclusiveFacet != null)
                {
                    double minExclusive;
                    if (Double.TryParse(minExclusiveFacet.Value, out minExclusive))
                        rules.Add(new NumericMinRule(minExclusive, false));
                    continue;
                }

                XmlSchemaMinInclusiveFacet minInclusiveFacet = facet as XmlSchemaMinInclusiveFacet;
                if (minInclusiveFacet != null)
                {
                    double minInclusive;
                    if (Double.TryParse(minInclusiveFacet.Value, out minInclusive))
                        rules.Add(new NumericMinRule(minInclusive, true));
                    continue;
                }

                XmlSchemaMaxExclusiveFacet maxExclusiveFacet = facet as XmlSchemaMaxExclusiveFacet;
                if (maxExclusiveFacet != null)
                {
                    double maxExclusive;
                    if (Double.TryParse(maxExclusiveFacet.Value, out maxExclusive))
                        rules.Add(new NumericMaxRule(maxExclusive, false));
                    continue;
                }

                XmlSchemaMaxInclusiveFacet maxInclusiveFacet = facet as XmlSchemaMaxInclusiveFacet;
                if (maxInclusiveFacet != null)
                {
                    double maxInclusive;
                    if (Double.TryParse(maxInclusiveFacet.Value, out maxInclusive))
                        rules.Add(new NumericMaxRule(maxInclusive, true));
                    continue;
                }
            }

            if (enumValues != null && enumValues.Count > 0)
                rules.Add(new StringEnumRule(enumValues.ToArray()));

            return rules;
        }

        private List<XmlNode> GetAnnotation(XmlSchemaAnnotated annotated)
        {
            List<XmlNode> nodes = new List<XmlNode>();
            XmlSchemaAnnotation annotation = annotated.Annotation;
            if (annotation != null)
            {
                // find the first <xs:appinfo> element
                foreach (XmlSchemaObject schemaObj in annotation.Items)
                {
                    XmlSchemaAppInfo appInfo = schemaObj as XmlSchemaAppInfo;
                    if (appInfo != null)
                    {
                        // copy annotation, removing comments
                        foreach (XmlNode node in appInfo.Markup)
                            if (node.NodeType != XmlNodeType.Comment)
                                nodes.Add(node);
                    }
                }
            }

            return nodes;
        }

        /// <summary>
        /// Merges annotations between redefined and redefining schemas</summary>
        /// <param name="schemaRedefine">XmlSchemeRefine</param>
        private void MergeRedefinedTypeAnnotations(XmlSchemaRedefine schemaRedefine)
        {
            // get index to original complex types
            Dictionary<XmlQualifiedName, XmlSchemaComplexType> originalTypes =
                new Dictionary<XmlQualifiedName, XmlSchemaComplexType>();
            foreach (XmlSchemaObject schemaItem in schemaRedefine.Schema.Items)
            {
                XmlSchemaComplexType complexType = schemaItem as XmlSchemaComplexType;
                if (complexType != null)
                    originalTypes.Add(complexType.QualifiedName, complexType);
            }

            // merge annotation for redefined complex types
            foreach (XmlSchemaObject schemaItem in schemaRedefine.Items)
            {
                XmlSchemaComplexType complexType = schemaItem as XmlSchemaComplexType;
                if (complexType != null)
                {
                    XmlQualifiedName name = complexType.QualifiedName;
                    XmlSchemaComplexType originalType = originalTypes[name];

                    if (originalType.IsAbstract != complexType.IsAbstract)
                        throw new InvalidOperationException(
                            "Type redefinition changes abstractness. (" + originalType.Name + ")");

                    string typeName = name.ToString();
                    DomNodeType nodeType;
                    if (m_nodeTypes.TryGetValue(typeName, out nodeType))
                    {
                        IList<XmlNode> annotations;
                        if (m_annotations.TryGetValue(nodeType, out annotations))
                        {
                            // Call protected virtual merge method - allowing clients to define if & how annotations are being merged
                            IEnumerable<XmlNode> mergedAnnotations = MergeRedefinedTypeAnnotations(
                                GetAnnotation(originalType),
                                annotations);
                            m_annotations[nodeType] = mergedAnnotations as IList<XmlNode> ?? mergedAnnotations.ToList();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Merge type annotation of a redefined type with the annotation of the type it is redefining</summary>
        /// <param name="originalTypeAnnotations">Annotations defined for the original type</param>
        /// <param name="redefineTypeAnnotations">Annotations defined for the redefining type</param>
        /// <returns>Merged annotations</returns>
        protected virtual IEnumerable<XmlNode> MergeRedefinedTypeAnnotations(
            IEnumerable<XmlNode> originalTypeAnnotations,
            IEnumerable<XmlNode> redefineTypeAnnotations)
        {
            // By default: just append base annotations to derived annotations
            List<XmlNode> mergedAnnotations = new List<XmlNode>(redefineTypeAnnotations);
            mergedAnnotations.AddRange(originalTypeAnnotations);
            return mergedAnnotations;
        }

        /// <summary>
        /// Merge type annotations with annotations defined for their base type</summary>
        /// <param name="baseAnnotations">Annotations defined for the base type</param>
        /// <param name="derivedAnnotations">Annotations defined for the derived type</param>
        /// <returns>Merged annotations</returns>
        protected virtual IEnumerable<XmlNode> MergeInheritedTypeAnnotations(
            IEnumerable<XmlNode> baseAnnotations,
            IEnumerable<XmlNode> derivedAnnotations)
        {
            // By default: don't merge with base type annotations
            return derivedAnnotations;
        }

        #endregion

        private readonly Dictionary<string, XmlSchemaTypeCollection> m_typeCollections =
            new Dictionary<string, XmlSchemaTypeCollection>();

        private readonly Dictionary<string, XmlAttributeType> m_attributeTypes =
            new Dictionary<string, XmlAttributeType>();

        private readonly Dictionary<string, DomNodeType> m_nodeTypes =
            new Dictionary<string, DomNodeType>();

        private readonly Dictionary<string, ChildInfo> m_rootElements =
            new Dictionary<string, ChildInfo>();

        private readonly Dictionary<ChildInfo, XmlQualifiedName> m_refElements =
            new Dictionary<ChildInfo, XmlQualifiedName>();

        private IDictionary<NamedMetadata, IList<XmlNode>> m_annotations;

        private HashSet<string> m_typeNameSet;//ensure type names generated be unique
        private Dictionary<XmlSchemaElement, XmlQualifiedName> m_localElementSet;

        private XmlResolver m_schemaResolver = new XmlUrlResolver();

        private static readonly XmlAttributeType s_mixedTextFieldSimpleType =
            new XmlAttributeType("mixed_text_field", typeof(string), 1, XmlTypeCode.String);

        private static readonly XmlQualifiedName s_anyTypeName =
            new XmlQualifiedName("anyType", XmlSchema.Namespace);
    }

    /// <summary>
    /// A rule to allow the use of the schema 'substitutionGroup'.
    /// http://www.w3schools.com/schema/schema_complex_subst.asp  </summary>
    internal class SubstitutionGroupChildRule : ChildRule
    {
        private readonly ChildInfo[] m_substitutions;

        public SubstitutionGroupChildRule(ChildInfo[] substitutions)
        {
            m_substitutions = substitutions;
        }

        public IEnumerable<ChildInfo> Substitutions
        {
            get { return m_substitutions; }
        }

        public override bool Validate(DomNode parent, DomNode child, ChildInfo childInfo)
        {
            return m_substitutions.Any(x => x.Type == child.Type);
        }
    }

}
