//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Abstract base class for DOM PropertyDescriptors</summary>
    public abstract class PropertyDescriptor : System.ComponentModel.PropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Property name</param>
        /// <param name="type">Type of property</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        public PropertyDescriptor(
            string name,
            Type type,
            string category,
            string description,
            bool isReadOnly)

            : this(name, type, category, description, isReadOnly, null, null)
        {}

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Property name</param>
        /// <param name="type">Type of property</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        public PropertyDescriptor(
            string name,
            Type type,
            string category,
            string description,
            bool isReadOnly,
            object editor)

            : this(name, type, category, description, isReadOnly, editor, null)
        {}

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Property name</param>
        /// <param name="type">Type of property</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public PropertyDescriptor(
            string name,
            Type type,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)

            : this(name, type, category, description, isReadOnly, editor, typeConverter, null)
        {}

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Property name</param>
        /// <param name="type">Type of property</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        /// <param name="attributes">An array of property attributes</param>
        public PropertyDescriptor(
            string name,
            Type type,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter,
            Attribute[] attributes)

            : base(name, attributes)
        {
            m_type = type;
            m_category = category;
            m_description = description;
            m_isReadOnly = isReadOnly;
            m_editor = editor;
            m_typeConverter = typeConverter;
        }

        /// <summary>
        /// Gets the path from the root node to the node that actually holds the property</summary>
        public virtual IEnumerable<ChildInfo> Path
        {
            get { return EmptyEnumerable<ChildInfo>.Instance; }
        }

        #region Overrides

        /// <summary>
        /// Gets the name of the category to which the member belongs, as specified in the <see cref="T:System.ComponentModel.CategoryAttribute"></see></summary>
        public override string Category
        {
            get
            {
                if (m_category != null)
                    return m_category;

                return base.Category;
            }
        }

        /// <summary>
        /// Gets the description of the member, as specified in the <see cref="T:System.ComponentModel.DescriptionAttribute"></see></summary>
        public override string Description
        {
            get { return m_description; }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether this property is read-only</summary>
        public override bool IsReadOnly
        {
            get { return m_isReadOnly; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the property</summary>
        public override Type PropertyType
        {
            get { return m_type; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the type of the component this property is bound to</summary>
        public override Type ComponentType
        {
            get { return typeof(DomNode); }
        }

        /// <summary>
        /// Gets the type converter for this property</summary>
        public override TypeConverter Converter
        {
            get
            {
                if (m_typeConverter != null)
                    return m_typeConverter;

                return base.Converter;
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns whether the value
        /// of this property needs to be persisted</summary>
        /// <param name="component">The component with the property to be examined for persistence</param>
        /// <returns><c>True</c> if the property should be persisted</returns>
        public override bool ShouldSerializeValue(object component)
        {
            return true;           
        }

        /// <summary>
        /// Gets an editor of the specified type</summary>
        /// <param name="editorBaseType">The base type of editor, which is used to differentiate between
        /// multiple editors that a property supports</param>
        /// <returns>An instance of the requested editor type, or null if an editor cannot be found</returns>
        public override object GetEditor(Type editorBaseType)
        {
            if (m_editor != null &&
                editorBaseType.IsAssignableFrom(m_editor.GetType()))
            {
                return m_editor;
            }

            return base.GetEditor(editorBaseType);
        }

        /// <summary>
        /// Test equality of property descriptors</summary>
        /// <param name="obj">Property descriptor to compare to</param>
        /// <returns><c>True</c> if property descriptors are equal</returns>
        /// <remarks>The .NET property descriptor only takes into account the Name and PropertyType.
        /// ATF needs to use Name, Category, and PropertyType. Overriding Equals() and GetHashCode()
        /// lets us use this Sce.Atf.Dom.PropertyDescriptor as a key in a Dictionary. The downside
        /// is that a System PropertyDescriptor in that Dictionary will never match ours even
        /// though it probably should. For details, see
        /// http://tracker.ship.scea.com/jira/browse/CORETEXTEDITOR-401 </remarks>
        public override bool Equals(object obj)
        {
            PropertyDescriptor other = obj as PropertyDescriptor;
            if (other != null)
                return PropertyUtils.PropertyDescriptorsEqual(this, other);

            // It's possible that 'obj' is a System.ComponentModel.PropertyDescriptor in which case we
            //  might want to consider if it is equivalent, but then we'd have a situation where the
            //  hash code was not equal, but Equals returned true, and that seems bad!
            return false;
        }

        /// <summary>
        /// Returns hash code</summary>
        /// <returns>Hash code</returns>
        /// <remarks>The .NET property descriptor only takes into account the Name and PropertyType.
        /// ATF needs to use Name, Category, and PropertyType. For details, see
        /// http://tracker.ship.scea.com/jira/browse/CORETEXTEDITOR-401 </remarks>
        public override int GetHashCode()
        {
            return this.GetPropertyDescriptorHash();
        }

        #endregion

        /// <summary>
        /// Gets node from component</summary>
        /// <param name="component">Component being edited</param>
        /// <returns>Node from component</returns>
        public virtual DomNode GetNode(object component)
        {
            return component.As<DomNode>();
        }

        /// <summary>
        /// Parses XML schema annotation to create PropertyDescriptors for the node type</summary>
        /// <param name="type">Node type</param>
        /// <param name="annotations">XML schema annotations</param>
        /// <returns>Collection of PropertyDescriptors for the node type</returns>
        public static PropertyDescriptorCollection ParseXml(DomNodeType type, IEnumerable<XmlNode> annotations)
        {
            PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(EmptyArray<PropertyDescriptor>.Instance);
            foreach (XmlNode annotation in annotations)
            {
                try
                {
                    // Get name, try to parse it as a path
                    XmlAttribute nameAttr = annotation.Attributes["name"];
                    string name = null;
                    string[] segments = null;
                    if (nameAttr != null)
                    {
                        name = nameAttr.Value;
                        segments = name.Split(PathDelimiters, StringSplitOptions.RemoveEmptyEntries);
                    }

                    PropertyDescriptor descriptor = GetDescriptor(type, annotation, name, segments);
                    if (descriptor != null)
                        descriptors.Add(descriptor);
                }
                catch (AnnotationException ex)
                {
                    Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                }
            }

            return descriptors;
        }

        private static PropertyDescriptor GetDescriptor(
            DomNodeType type,
            XmlNode annotation,
            string name,
            string[] segments)
        {
            PropertyDescriptor desc = null;
            // Get mandatory display name
            XmlAttribute displayNameAttr = annotation.Attributes["displayName"];
            if (displayNameAttr != null)
            {
                if (string.IsNullOrEmpty(name))
                    throw new AnnotationException(string.Format(
                        "Required name attribute is null or empty.\r\nType: {0}\r\nElement: {1}",
                        type.Name, annotation.Name));
                
                string displayName = displayNameAttr.Value;
                if (string.IsNullOrEmpty(displayName))
                    displayName = name;

                // Get optional annotations
                string category = GetAnnotation(annotation, "category");
                string description = GetAnnotation(annotation, "description");
                bool readOnly = GetAnnotation(annotation, "readOnly") == "true";
                object editor = CreateObject<object>(type, annotation, "editor");
                TypeConverter typeConverter = CreateObject<TypeConverter>(type, annotation, "converter");

                if (annotation.Name == "scea.dom.editors.attribute")
                {
                    // Attribute annotation
                    if (segments == null)
                        throw new AnnotationException("Unnamed attribute");

                    if (segments.Length == 1) // local attribute
                    {
                        AttributeInfo metaAttr = type.GetAttributeInfo(name);
                        if (metaAttr == null)
                            throw new AnnotationException("Type doesn't have this attribute");

                        desc = new AttributePropertyDescriptor(
                            displayName, metaAttr,
                            category, description, readOnly, editor, typeConverter);
                    }
                    else // descendant attribute
                    {
                        ChildInfo[] metaElements = GetPath(type, segments, segments.Length - 1);
                        DomNodeType childType = metaElements[segments.Length - 2].Type;
                        AttributeInfo metaAttr = childType.GetAttributeInfo(segments[segments.Length - 1]);
                        if (metaAttr == null)
                            throw new AnnotationException("Descendant type doesn't have this attribute");

                        desc = new ChildAttributePropertyDescriptor(
                            displayName, metaAttr, metaElements,
                            category, description, readOnly, editor, typeConverter);
                    }
                }
                else if (annotation.Name == "scea.dom.editors.child")
                {
                    // Child value annotation
                    ChildInfo element = type.GetChildInfo(name);
                    if (element == null)
                        throw new AnnotationException("Type doesn't have this element");

                    desc = new ChildPropertyDescriptor(
                        displayName, element,
                        category, description, readOnly, editor, typeConverter);

                }
            }
            return desc;
        }

        private static T CreateObject<T>(DomNodeType domNodeType, XmlNode annotation, string attribute)
            where T : class
        {
            string typeName = GetAnnotation(annotation, attribute);
            string paramString = string.Empty;
            if (typeName != null)
            {
                // check for params
                int colonIndex = typeName.IndexOf(':');
                if (colonIndex >= 0)
                {
                    int paramsIndex = colonIndex + 1;
                    paramString = typeName.Substring(paramsIndex, typeName.Length - paramsIndex);
                    typeName = typeName.Substring(0, colonIndex);
                }

                // create object from type name
                Type objectType = System.Type.GetType(typeName);
                if (objectType == null)
                    throw new AnnotationException("Couldn't find type " + typeName);

                // initialize with params
                object obj = Activator.CreateInstance(objectType);
                IAnnotatedParams annotatedObj = obj as IAnnotatedParams;
                if (annotatedObj != null)
                {
                    string[] parameters;

                    if (!string.IsNullOrEmpty(paramString))
                        parameters = paramString.Split(',');
                    else
                        parameters = TryGetEnumeration(domNodeType, annotation);
                    if (parameters != null)
                        annotatedObj.Initialize(parameters);
                }

                if (!(obj is T))
                    throw new AnnotationException("Object must be " + typeof(T));
                return (T)obj;
            }
            else
            {
                return null;
            }
        }

        // try to obtain enumeration values from xml annotations
        // Legacy ATF 2.8 example:
        //  <xs:simpleType name="EmotionType">
        //    <xs:annotation>
        //        <xs:appinfo>
        //            <scea.dom.editors.enumeration name="Adequate" displayName="Adequate, good enough"/>
        //            <scea.dom.editors.enumeration name="Capable" displayName="Capable, you betcha!"/>
        //            <scea.dom.editors.enumeration name="Enthusiastic" />
        //        </xs:appinfo>
        //    </xs:annotation>
        //    <xs:restriction base="xs:string">
        //        <xs:enumeration value="Adequate"/>
        //        <xs:enumeration value="Capable"/>
        //        <xs:enumeration value="Enthusiastic"/>
        //     </xs:restriction>
        //  </xs:simpleType> 
        private static string[] TryGetEnumeration(DomNodeType domNodeType, XmlNode annotation)
        {
            string[] enumeration = null;
            XmlAttribute targetDomAttribute = annotation.Attributes[AnnotationsNameAttribute];
            if (targetDomAttribute != null)
            {
                var domObjectAttribute = domNodeType.GetAttributeInfo(targetDomAttribute.Value);
                if (domObjectAttribute != null)
                {
                    var attributeType = domObjectAttribute.Type;
                    var xmlAnnotation = attributeType.GetTag<IEnumerable<XmlNode>>();
                    if (xmlAnnotation != null)
                    {

                        List<string> enumerationList = new List<string>();
                        foreach (XmlNode enumAnnotation in xmlAnnotation)
                        {
                            if (enumAnnotation.Name == AnnotationsLegacyEnumeration)
                            {
                                string name = enumAnnotation.Attributes[AnnotationsNameAttribute].Value;
                                XmlNode displayNode =
                                    enumAnnotation.Attributes.GetNamedItem(AnnotationsDisplayNameAttribute);
                                if (displayNode != null)
                                    enumerationList.Add(name + "==" + displayNode.Value);
                                else
                                    enumerationList.Add(name);
                            }
                            enumeration = enumerationList.ToArray();
                        }
                    }
                }
            }
           
            return enumeration;
        }


        /// <summary>
        /// Gets annotation value for the given attribute name</summary>
        /// <param name="annotation">Annotation</param>
        /// <param name="attributeName">Attribute name</param>
        /// <returns>Annotation value</returns>
        private static string GetAnnotation(XmlNode annotation, string attributeName)
        {
            string result = null;

            if (annotation != null)
            {
                XmlAttribute attribute = annotation.Attributes[attributeName];
                if (attribute != null)
                    result = attribute.Value;
            }

            return result;
        }

        private static ChildInfo[] GetPath(DomNodeType type, string[] segments, int length)
        {
            ChildInfo[] result = new ChildInfo[length];
            for (int i = 0; i < length; i++)
            {
                ChildInfo metaElement = type.GetChildInfo(segments[i]);
                if (metaElement == null)
                    throw new AnnotationException("Invalid path");

                result[i] = metaElement;

                type = metaElement.Type;
            }

            return result;
        }

        private readonly Type m_type;
        private readonly string m_category;
        private readonly string m_description;
        private readonly object m_editor;
        private readonly TypeConverter m_typeConverter;
        private readonly bool m_isReadOnly;

        private static readonly char[] PathDelimiters = new[] { '/', '\\', '.' };

        /// <summary>
        /// Name attribute</summary>
        private const string AnnotationsNameAttribute = "name";
        private const string AnnotationsDisplayNameAttribute = "displayName";
        private const string AnnotationsLegacyEnumeration = "scea.dom.editors.enumeration";
    }
}
