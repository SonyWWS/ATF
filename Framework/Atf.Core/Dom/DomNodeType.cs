//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Defines a DOM type in a type hierarchy. Each derived type has only one base type. This is
    /// similar to XML complex types. Each type can define attributes (simple .NET types or arrays
    /// of those types), children, and extensions (typically adapters that implement IAdaptable).
    /// Each DomNode is considered to be an instance of one immutable DomNodeType.</summary>
    public class DomNodeType : NamedMetadata
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Name of node type</param>
        public DomNodeType(string name)
            : base(name)
        {
            m_baseType = s_baseOfAllTypes;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Name of node type</param>
        /// <param name="baseType">Base node type. 'null' equates to DomNodeType.BaseOfAllTypes</param>
        /// <param name="attributes">Attributes for node type, or null</param>
        /// <param name="children">Children for node type, or null</param>
        /// <param name="extensions">Extensions for node type, or null</param>
        public DomNodeType(
            string name,
            DomNodeType baseType,
            IEnumerable<AttributeInfo> attributes,
            IEnumerable<ChildInfo> children,
            IEnumerable<ExtensionInfo> extensions)
            : base(name)
        {
            SetBaseType(baseType);

            if (attributes != null)
                m_definitions.Attributes.AddRange(attributes);
            if (children != null)
                m_definitions.Children.AddRange(children);
            if (extensions != null)
                m_definitions.Extensions.AddRange(extensions);
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Name of node type</param>
        /// <param name="baseType">Base node type. 'null' equates to DomNodeType.BaseOfAllTypes</param>
        /// <param name="metadata">Zero or more AttributeInfo, ChildInfo, or ExtensionInfo objects</param>
        public DomNodeType(
            string name,
            DomNodeType baseType,
            params FieldMetadata[] metadata)
            : base(name)
        {
            SetBaseType(baseType);
            foreach (FieldMetadata info in metadata)
            {
                if (info is AttributeInfo)
                    m_definitions.Attributes.Add((AttributeInfo)info);
                else if (info is ChildInfo)
                    m_definitions.Children.Add((ChildInfo)info);
                else if (info is ExtensionInfo)
                    m_definitions.Extensions.Add((ExtensionInfo)info);
            }
        }

        /// <summary>
        /// Defines an attribute for the type. Attributes are primitive values, like int or float,
        /// or arrays of primitives.</summary>
        /// <param name="attributeInfo">Information about the attribute</param>
        public void Define(AttributeInfo attributeInfo)
        {
            if (m_attributes != null)
                throw new InvalidOperationException("Attributes frozen");
            m_definitions.Attributes.Add(attributeInfo);
        }

        /// <summary>
        /// Defines a child node for the type; children can be singletons or lists</summary>
        /// <param name="childInfo">Information about the child</param>
        public void Define(ChildInfo childInfo)
        {
            if (m_children != null)
                throw new InvalidOperationException("Children frozen");
            m_definitions.Children.Add(childInfo);
        }

        /// <summary>
        /// Defines an extension for the type; extensions can be any type, and should
        /// be used when any data needs to be in 1-1 correspondence to instances of the
        /// node type. Extension types that implement IAdapter can be retrieved through
        /// the IAdaptable interface of DomNode.</summary>
        /// <param name="extensionInfo">Information about the extension</param>
        public void Define(ExtensionInfo extensionInfo)
        {
            if (m_extensions != null)
                throw new InvalidOperationException("Extensions frozen");
            m_definitions.Extensions.Add(extensionInfo);
        }

        /// <summary>
        /// Adds an adapter creator for the type. Adapter creators extend the adaptability
        /// of instances of the node type, and are queried by the implementation of IAdaptable.</summary>
        /// <param name="creator">Adapter creator</param>
        public void AddAdapterCreator(IAdapterCreator creator)
        {
            if (m_extensions == null)
                FreezeExtensions();

            AddCreator(creator);
        }

        /// <summary>
        /// Gets or sets base NodeType. The base type can't be changed once any of
        /// the attribute, child, or extension metadata has been frozen.</summary>
        public DomNodeType BaseType
        {
            get { return m_baseType; }
            set
            {
                if (m_attributes != null || m_children != null || m_extensions != null)
                    throw new InvalidOperationException("Can't change base type once any fields are frozen");

                SetBaseType(value);
            }
        }

        private void SetBaseType(DomNodeType baseType)
        {
            if (baseType == null)
                baseType = s_baseOfAllTypes;

            m_baseType = baseType;
        }

        /// <summary>
        /// Gets the node type from which all node types ultimately derive.
        /// This type is useful to register general-purpose DOM adapters for all nodes.</summary>
        public static DomNodeType BaseOfAllTypes
        {
            get { return s_baseOfAllTypes; }
        }

        /// <summary>
        /// Gets all node types in the lineage of this type, starting with this type</summary>
        public IEnumerable<DomNodeType> Lineage
        {
            get
            {
                DomNodeType type = this;
                while (type != null)
                {
                    yield return type;

                    type = type.BaseType;
                }
            }
        }

        /// <summary>
        /// Gets metadata for attributes</summary>
        public IEnumerable<AttributeInfo> Attributes
        {
            get
            {
                if (m_attributes == null)
                    FreezeAttributes();

                return m_attributes;
            }
        }

        internal AttributeInfo GetAttributeInfo(int index)
        {
            if (m_attributes == null)
                FreezeAttributes();

            if (index < m_attributes.Length)
                return m_attributes[index];

            return null;
        }

        /// <summary>
        /// Gets the id attribute for this type</summary>
        /// <remarks>This attribute should be of a type that can serve as a unique id,
        /// such as string, int, or Uri. If no id attribute is defined on the type, the lineage
        /// is searched for the closest ancestor with a defined id attribute.</remarks>
        public AttributeInfo IdAttribute
        {
            get
            {
                for (DomNodeType type = this; type != null; type = type.m_baseType)
                    if (type.m_idAttribute != null)
                        return type.m_idAttribute;

                return null;
            }
        }

        /// <summary>
        /// Sets the id attribute by name</summary>
        /// <param name="name">name of id attribute</param>
        public void SetIdAttribute(string name)
        {
            m_idAttribute = GetAttributeInfo(name);
        }

        /// <summary>
        /// Sets the id attribute</summary>
        /// <param name="idAttribute">Attribute (as AttributeInfo)</param>
        public void SetIdAttribute(AttributeInfo idAttribute)
        {
            if (!IsValid(idAttribute))
                throw new InvalidOperationException("invalid attribute info");

            m_idAttribute = idAttribute;
        }

        /// <summary>
        /// Gets metadata for children</summary>
        public IEnumerable<ChildInfo> Children
        {
            get
            {
                if (m_children == null)
                    FreezeChildren();

                return m_children;
            }
        }

        internal ChildInfo GetChildInfo(int index)
        {
            if (m_children == null)
                FreezeChildren();

            if (index < m_children.Length)
                return m_children[index];

            return null;
        }

        /// <summary>
        /// Gets metadata for extensions</summary>
        public IEnumerable<ExtensionInfo> Extensions
        {
            get
            {
                if (m_extensions == null)
                    FreezeExtensions();

                return m_extensions;
            }
        }

        internal ExtensionInfo GetExtensionInfo(int index)
        {
            if (m_extensions == null)
                FreezeExtensions();

            if (index < m_extensions.Length)
                return m_extensions[index];

            return null;
        }

        /// <summary>
        /// Gets a value indicating if this type is abstract (can't be instantiated)</summary>
        public bool IsAbstract
        {
            get { return m_isAbstract; }
            set { m_isAbstract = value; }
        }

        /// <summary>
        /// Tests if this node type is a base class of the given node type</summary>
        /// <param name="type">Other node type</param>
        /// <returns>True iff this node type is a base class of the given node type</returns>
        public bool IsAssignableFrom(DomNodeType type)
        {
            for (DomNodeType descendant = type; descendant != null; descendant = descendant.BaseType)
                if (this == descendant)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets metadata for an attribute</summary>
        /// <param name="name">Simple attribute name</param>
        /// <returns>Metadata for the simple attribute</returns>
        public AttributeInfo GetAttributeInfo(string name)
        {
            if (m_attributes == null)
                FreezeAttributes();

            int index = m_attributeIndex.FindIndex(name);
            return (index >= 0) ? m_attributes[index] : null;
        }

        /// <summary>
        /// Gets metadata for a child</summary>
        /// <param name="name">Child name</param>
        /// <returns>Metadata for the child</returns>
        public ChildInfo GetChildInfo(string name)
        {
            if (m_children == null)
                FreezeChildren();

            int index = m_childIndex.FindIndex(name);
            return (index >= 0) ? m_children[index] : null;
        }

        /// <summary>
        /// Gets the descendant metadata corresponding to the given path</summary>
        /// <param name="path">Path to descendant, with ':' as separator</param>
        /// <returns>Descendant metadata corresponding to the given path</returns>
        public ChildInfo GetDescendantInfo(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            ChildInfo result = null;
            DomNodeType type = this;
            string[] segments = path.Split(':');
            foreach (string segment in segments)
            {
                result = type.GetChildInfo(segment);
                if (result == null)
                    break;
                type = result.Type;
            }

            return result;
        }

        /// <summary>
        /// Gets metadata for an extension</summary>
        /// <param name="name">Extension name</param>
        /// <returns>Metadata for the extension</returns>
        public ExtensionInfo GetExtensionInfo(string name)
        {
            if (m_extensions == null)
                FreezeExtensions();

            int index = m_extensionIndex.FindIndex(name);
            return (index >= 0) ? m_extensions[index] : null;
        }

        /// <summary>
        /// Converts the type to its string representation</summary>
        /// <returns>Type's string representation</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Returns a value indicating if the attribute is defined on this type</summary>
        /// <param name="attributeInfo">Information about attribute</param>
        /// <returns>True iff the attribute is defined on this type</returns>
        public bool IsValid(AttributeInfo attributeInfo)
        {
            if (m_attributes == null)
                FreezeAttributes();

            return TryGetDataIndex(attributeInfo) >= 0;
        }

        internal int GetDataIndex(AttributeInfo attributeInfo)
        {
            int index = TryGetDataIndex(attributeInfo);
            if (index < 0)
                throw new InvalidOperationException("attributeInfo doesn't belong to node type");

            return index;
        }

        private int TryGetDataIndex(AttributeInfo attributeInfo)
        {
            if (attributeInfo == null)
                throw new ArgumentNullException("attributeInfo");

            // info is valid if it has the same index and was defined on the same type as local metadata.
            // Keep this test in sync with FieldMetaData.Equivalent().
            int index = attributeInfo.Index;
            if (index < m_attributes.Length &&
                m_attributes[index].DefiningType == attributeInfo.DefiningType)
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// Returns a value indicating if the child is defined on this type</summary>
        /// <param name="childInfo">Information about child</param>
        /// <returns>True iff the child is defined on this type</returns>
        public bool IsValid(ChildInfo childInfo)
        {
            if (m_children == null)
                FreezeChildren();

            return TryGetDataIndex(childInfo) >= 0;
        }

        internal int GetDataIndex(ChildInfo childInfo)
        {
            int index = TryGetDataIndex(childInfo);
            if (index < 0)
                throw new InvalidOperationException("childInfo doesn't belong to node type");

            return index;
        }

        internal int TryGetDataIndex(ChildInfo childInfo)
        {
            if (childInfo == null)
                throw new ArgumentNullException("childInfo");

            // info is valid if it has the same index and was defined on the same type as local metadata
            int index = childInfo.Index;
            if (index < m_children.Length &&
                m_children[index].DefiningType == childInfo.DefiningType)
            {
                return index + FirstChildIndex;
            }

            return -1;
        }

        /// <summary>
        /// Returns a value indicating if the extension is defined on this type</summary>
        /// <param name="extensionInfo">Information about extension</param>
        /// <returns>True iff the extension is defined on this type</returns>
        public bool IsValid(ExtensionInfo extensionInfo)
        {
            if (m_extensions == null)
                FreezeExtensions();

            return TryGetDataIndex(extensionInfo) >= 0;
        }

        internal int GetDataIndex(ExtensionInfo extensionInfo)
        {
            int index = TryGetDataIndex(extensionInfo);
            if (index < 0)
                throw new InvalidOperationException("extensionInfo doesn't belong to node type");

            return index;
        }

        internal int TryGetDataIndex(ExtensionInfo extensionInfo)
        {
            if (extensionInfo == null)
                throw new ArgumentNullException("extensionInfo");

            // info is valid if it has the same index and was defined on the same type as local metadata
            int index = extensionInfo.Index;
            if (index < m_extensions.Length &&
                m_extensions[index].DefiningType == extensionInfo.DefiningType)
            {
                return index + FirstExtensionIndex;
            }

            return -1;
        }

        /// <summary>
        /// Gets the parent of this metadata, in this case, the base node type</summary>
        /// <returns>Parent of this metadata</returns>
        protected override NamedMetadata GetParent()
        {
            return m_baseType;
        }

        // These help make DomNode a bit more efficient

        internal int FieldCount;
        internal int FirstChildIndex;
        internal int FirstExtensionIndex;
        internal bool IsFrozen;

        internal void Freeze()
        {
            if (m_attributes == null)
                FreezeAttributes();
            if (m_children == null)
                FreezeChildren();
            if (m_extensions == null)
                FreezeExtensions();

            IsFrozen = true;
            m_definitions = null;

            FirstChildIndex = m_attributes.Length;
            FirstExtensionIndex = FirstChildIndex + m_children.Length;
            FieldCount = FirstExtensionIndex + m_extensions.Length;
        }

        private void FreezeAttributes()
        {
            List<AttributeInfo> mergedAttributes = new List<AttributeInfo>();
            if (m_baseType != null)
                mergedAttributes.AddRange(m_baseType.Attributes);

            foreach (AttributeInfo info in m_definitions.Attributes)
            {
                info.OwningType = this;

                AttributeInfo baseInfo = m_baseType.GetAttributeInfo(info.Name);
                if (baseInfo != null)
                {
                    // field is also present in base; make field compatible
                    info.Index = baseInfo.Index;
                    info.DefiningType = baseInfo.DefiningType;
                    mergedAttributes[baseInfo.Index] = info;
                }
                else
                {
                    // this is a new field
                    info.Index = mergedAttributes.Count;
                    info.DefiningType = this;
                    mergedAttributes.Add(info);
                }
            }

            if (mergedAttributes.Count > 0)
            {
                m_attributes = mergedAttributes.ToArray();
                m_attributeIndex = new StringIndex(m_attributes);
            }
            else
            {
                m_attributes = EmptyArray<AttributeInfo>.Instance;
                m_attributeIndex = StringIndex.Empty;
            }
        }

        private void FreezeChildren()
        {
            List<ChildInfo> mergedChildren = new List<ChildInfo>();
            if (m_baseType != null)
                mergedChildren.AddRange(m_baseType.Children);

            HashSet<string> childrenSet = new HashSet<string> ();
            foreach (ChildInfo info in m_definitions.Children)
            {
                info.OwningType = this;

                ChildInfo baseInfo = m_baseType.GetChildInfo(info.Name);
                if (baseInfo != null)
                {
                    // field is also present in base; make field compatible
                    info.Index = baseInfo.Index;
                    info.DefiningType = baseInfo.DefiningType;
                    mergedChildren[baseInfo.Index] = info;
                }
                else
                {
                    // this is a new field
                    if (childrenSet.Add(info.Name)) // avoid duplicated definitions from sequence and choice compositors
                    {
                        info.Index = mergedChildren.Count;
                        info.DefiningType = this;
                        mergedChildren.Add(info);
                        childrenSet.Add(info.Name);
                    }
                }
            }

            if (mergedChildren.Count > 0)
            {
                m_children = mergedChildren.ToArray();
                m_childIndex = new StringIndex(m_children);
            }
            else
            {
                m_children = EmptyArray<ChildInfo>.Instance;
                m_childIndex = StringIndex.Empty;
            }
        }

        private void FreezeExtensions()
        {
            List<ExtensionInfo> mergedExtensions = new List<ExtensionInfo>();
            if (m_baseType != null)
                mergedExtensions.AddRange(m_baseType.Extensions);

            foreach (ExtensionInfo info in m_definitions.Extensions)
            {
                info.OwningType = this;

                ExtensionInfo baseInfo = null;
                if (m_baseType != null)
                    baseInfo = m_baseType.GetExtensionInfo(info.Name);

                if (baseInfo != null)
                {
                    // field is also present in base; make field compatible
                    info.Index = baseInfo.Index;
                    info.DefiningType = baseInfo.DefiningType;
                    mergedExtensions[baseInfo.Index] = info;
                }
                else
                {
                    // this is a new field
                    info.Index = mergedExtensions.Count;
                    info.DefiningType = this;
                    mergedExtensions.Add(info);
                }
            }

            if (mergedExtensions.Count > 0)
            {
                m_extensions = mergedExtensions.ToArray();
                m_extensionIndex = new StringIndex(m_extensions);
            }
            else
            {
                m_extensions = EmptyArray<ExtensionInfo>.Instance;
                m_extensionIndex = StringIndex.Empty;
            }

            foreach (ExtensionInfo extensionInfo in m_extensions)
            {
                // if this NodeType defines the extension, add an interface creator
                if (extensionInfo.DefiningType == this &&
                    typeof(IAdapter).IsAssignableFrom(extensionInfo.Type))
                {
                    AddCreator(new ExtensionAdapterCreator(extensionInfo));
                }
            }
        }

        private void AddCreator(IAdapterCreator creator)
        {
            if (m_adapterCreators == null)
                m_adapterCreators = new List<IAdapterCreator>();

            m_adapterCreators.Add(creator);
        }

        internal static object GetAdapter(DomNode node, Type type)
        {
            IEnumerable<IAdapterCreator> adapterCreators = GetAdapterCreators(node, type);
            foreach (IAdapterCreator creator in adapterCreators)
                return creator.GetAdapter(node, type);

            return null;
        }

        internal static IEnumerable<object> GetAdapters(DomNode node, Type type)
        {
            IEnumerable<IAdapterCreator> adapterCreators = GetAdapterCreators(node, type);
            foreach (IAdapterCreator adapterCreator in adapterCreators)
            {
                object adapter = adapterCreator.GetAdapter(node, type);
                yield return adapter;
            }
        }

        private static IEnumerable<IAdapterCreator> GetAdapterCreators(DomNode node, Type type)
        {
            DomNodeType nodeType = node.Type;
            if (nodeType.m_adapterCreatorCache == null)
                nodeType.m_adapterCreatorCache = new Dictionary<Type, IEnumerable<IAdapterCreator>>();

            IEnumerable<IAdapterCreator> adapterCreators;
            if (!nodeType.m_adapterCreatorCache.TryGetValue(type, out adapterCreators))
            {
                // build an array of adapter creators that can adapt the node
                List<IAdapterCreator> creators = new List<IAdapterCreator>();
                while (nodeType != null)
                {
                    if (nodeType.m_adapterCreators != null)
                    {
                        foreach (IAdapterCreator creator in nodeType.m_adapterCreators)
                        {
                            if (creator.CanAdapt(node, type))
                                creators.Add(creator);
                        }
                    }

                    nodeType = nodeType.BaseType;
                }

                // for empty arrays, use global instance
                adapterCreators = (creators.Count > 0) ? creators.ToArray() : EmptyEnumerable<IAdapterCreator>.Instance;

                // cache the result for subsequent searches
                node.Type.m_adapterCreatorCache.Add(type, adapterCreators);
            }

            return adapterCreators;
        }

        // Class constructor
        static DomNodeType()
        {
            // define the base of all types
            s_baseOfAllTypes = new DomNodeType("Sce.Atf.Dom.Object");
            // constructor sets all base types to s_baseOfAllTypes, so override that
            s_baseOfAllTypes.m_baseType = null;
            //s_baseOfAllTypes.Freeze();
        }

        // class to hold field and extension definitions until the type is frozen
        private class Definitions
        {
            public readonly List<AttributeInfo> Attributes = new List<AttributeInfo>();
            public readonly List<ChildInfo> Children = new List<ChildInfo>();
            public readonly List<ExtensionInfo> Extensions = new List<ExtensionInfo>();
        }

        // Class that allows extensions implementing IAdapter to be initialized
        private class ExtensionAdapterCreator : IAdapterCreator
        {
            public ExtensionAdapterCreator(ExtensionInfo extensionInfo)
            {
                m_extensionInfo = extensionInfo;
            }

            public bool CanAdapt(object adaptee, Type type)
            {
                DomNode node = adaptee as DomNode;
                return
                    node != null &&
                    type != null &&
                    type.IsAssignableFrom(m_extensionInfo.Type);
            }

            public object GetAdapter(object adaptee, Type type)
            {
                DomNode node = adaptee as DomNode;
                if (node != null &&
                    type.IsAssignableFrom(m_extensionInfo.Type))
                {
                    IAdapter adapter = (IAdapter)node.GetExtension(m_extensionInfo);

                    // this check is required, as shown by our unit tests
                    if (adapter.Adaptee == null)
                        adapter.Adaptee = node;

                    return adapter;
                }

                return null;
            }

            private readonly ExtensionInfo m_extensionInfo;
        }

        // Class to speed up search for field names using binary search
        private class StringIndex
        {
            public StringIndex(IList<FieldMetadata> fields)
            {
                m_strings = new StringInfo[fields.Count];
                int i = 0;
                foreach (FieldMetadata field in fields)
                {
                    m_strings[i] = new StringInfo(field.Name, i);
                    i++;
                }
                Array.Sort(m_strings);

                // Verify uniqueness. If multiple fields have the same name, then the duplicates
                //  will no longer be able to be retrieved by the FieldMetaData's Name.
                if (m_strings.Length >= 2)
                {
                    string prev = m_strings[0].String;
                    for (i = 1; i < m_strings.Length; i++)
                    {
                        string curr = m_strings[i].String;
                        if (curr.Equals(prev))
                            m_strings[i].Duplicate = true;

                        prev = curr;
                    }
                }
            }

            public int FindIndex(string searchString)
            {
                int lo = 0;
                int hi = m_strings.Length;
                while (lo != hi)
                {
                    int mid = (lo + hi) / 2;
                    int compare = m_strings[mid].String.CompareTo(searchString);
                    if (compare < 0)
                    {
                        lo = mid + 1;
                    }
                    else if (compare > 0)
                    {
                        hi = mid;
                    }
                    else
                    {
                        // Duplicate FieldMetaData names are acceptable when searching using the method
                        //  DomNode.GetExtension(ExtensionInfo), but not when searching by the name.
                        // See this tracker item: http://sf.ship.scea.com/sf/go/artf38078
                        if (m_strings[mid].Duplicate)
                            throw new InvalidOperationException(
                                "FieldMetaData named '" + searchString + "' was not unique on its DomNodeType");

                        return m_strings[mid].Index;
                    }
                }
                return -1;
            }
            
            public static readonly StringIndex Empty = new StringIndex(EmptyArray<FieldMetadata>.Instance);

            private struct StringInfo : IComparable<StringInfo>
            {
                public StringInfo(string s, int index)
                {
                    String = s;
                    Index = index;
                    Duplicate = false;
                }

                public readonly string String;
                public readonly int Index;
                public bool Duplicate; //Another StringInfo has the same String

                public int CompareTo(StringInfo other)
                {
                    return String.CompareTo(other.String);
                }
            }

            private readonly StringInfo[] m_strings;
        }

        private DomNodeType m_baseType;
        private AttributeInfo[] m_attributes;
        private StringIndex m_attributeIndex;
        private ChildInfo[] m_children;
        private StringIndex m_childIndex;
        private ExtensionInfo[] m_extensions;
        private StringIndex m_extensionIndex;
        private AttributeInfo m_idAttribute;

        private Definitions m_definitions = new Definitions();

        private List<IAdapterCreator> m_adapterCreators;
        private Dictionary<Type, IEnumerable<IAdapterCreator>> m_adapterCreatorCache;
        private bool m_isAbstract;

        private static readonly DomNodeType s_baseOfAllTypes;
    }
}
