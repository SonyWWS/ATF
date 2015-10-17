using System;
using System.Collections;
using System.ComponentModel;

using Sce.Atf.Adaptation;

using DomPropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// PropertyDescriptor for child or descendant collections of attributes of a DOM object</summary>
    public class ChildAttributeCollectionPropertyDescriptor : DomPropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfos">An array of meta attributes in the collection</param>
        /// <param name="childInfo">Meta element identifying child that owns the attributes</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        public ChildAttributeCollectionPropertyDescriptor(
            string name,
            AttributeInfo[] attributeInfos,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly)

            : this(name, attributeInfos, childInfo, category, description, isReadOnly, null, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfos">An array of meta attributes in the collection</param>
        /// <param name="childInfo">Meta element identifying child that owns the attributes</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        public ChildAttributeCollectionPropertyDescriptor(
            string name,
            AttributeInfo[] attributeInfos,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor)

            : this(name, attributeInfos, childInfo, category, description, isReadOnly, editor, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfos">An array of meta attributes in the collection</param>
        /// <param name="childInfo">Meta element identifying child that owns the attributes</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public ChildAttributeCollectionPropertyDescriptor(
            string name,
            AttributeInfo[] attributeInfos,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)
            : this(name, attributeInfos, childInfo, category, description, isReadOnly, editor, typeConverter, null)
        {
        }


        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfos">An array of meta attributes in the collection</param>
        /// <param name="childInfo">Meta element identifying child that owns the attributes</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        /// <param name="defaultValues">An array of default values for each attribute, must be null (ignored) or same length as attributeInfos array</param>
        public ChildAttributeCollectionPropertyDescriptor(
            string name,
            AttributeInfo[] attributeInfos,
            ChildInfo childInfo,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter,
            object[] defaultValues)
            : base(name, typeof(IList), category, description, isReadOnly, editor, typeConverter)
        {
            m_attributeInfos = attributeInfos;
            m_childInfos = new[] { childInfo };
            m_defaultValues = defaultValues;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfos">An array of meta attributes in the collection</param>
        /// <param name="childInfos">An array of meta elements describing path to descendant element that owns the attributes</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        public ChildAttributeCollectionPropertyDescriptor(
            string name,
            AttributeInfo[] attributeInfos,
            ChildInfo[] childInfos,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)
            : this(name, attributeInfos, childInfos, category, description, isReadOnly, editor, typeConverter, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Value's display name</param>
        /// <param name="attributeInfos">An array of meta attributes in the collection</param>
        /// <param name="childInfos">An array of meta elements describing path to descendant element that owns the attributes</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor used to edit the property</param>
        /// <param name="typeConverter">The type converter used for this property</param>
        /// <param name="defaultValues">An array of default values for each attribute, must be null (ignored) or same length as attributeInfos array</param>
        public ChildAttributeCollectionPropertyDescriptor(
            string name,
            AttributeInfo[] attributeInfos,
            ChildInfo[] childInfos,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter,
            object[] defaultValues)

            : base(name, typeof(IList), category, description, isReadOnly, editor, typeConverter)
        {
            m_attributeInfos = attributeInfos;
            m_childInfos = childInfos;
            m_defaultValues = defaultValues;
        }

        /// <summary>
        /// Tests equality of property descriptor with object</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns><c>True</c> if property descriptors are identical</returns>
        /// <remarks>Implements Equals() for organizing descriptors in grid controls</remarks>
        public override bool Equals(object obj)
        {
            ChildAttributeCollectionPropertyDescriptor other = obj as ChildAttributeCollectionPropertyDescriptor;

            if (!base.Equals(other))
                return false;

            if (m_attributeInfos.Length != other.m_attributeInfos.Length)
                return false;

            for (int i = 0; i < m_attributeInfos.Length; i++)
                if (m_attributeInfos[i] != other.m_attributeInfos[i])
                    return false;

            for (int i = 0; i < m_childInfos.Length; i++)
                if (m_childInfos[i] != other.m_childInfos[i])
                    return false;

            return true;
        }

        /// <summary>
        /// Gets hash code for property descriptor</summary>
        /// <returns>Hash code</returns>
        /// <remarks>Implements GetHashCode() for organizing descriptors in grid controls</remarks>
        public override int GetHashCode()
        {
            int result = base.GetHashCode();
            foreach (AttributeInfo attributeInfo in m_attributeInfos)
                result ^= attributeInfo.GetHashCode();
            return result;
        }

        /// <summary>
        /// Gets DOM object from component</summary>
        /// <param name="component">Component being edited</param>
        /// <returns>DomNode</returns>
        public override DomNode GetNode(object component)
        {
            DomNode domNode = component.As<DomNode>();
            if (domNode != null)
            {
                // Check that the meta element path exists on the parent DOM object
                foreach (ChildInfo childInfo in m_childInfos)
                {
                    ChildInfo classMeta = domNode.Type.GetChildInfo(childInfo.Name);

                    if (classMeta != null)
                        return domNode.GetChild(classMeta);
                }
            }

            return domNode;
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns><c>True</c> if resetting the component changes its value</returns>
        public override bool CanResetValue(object component)
        {
            if (IsReadOnly)
                return false;

            DomNode domNode = GetNode(component);
            if (domNode != null)
            {
                for (int i = 0; i < m_attributeInfos.Length; ++i)
                {
                    AttributeInfo attributeInfo = m_attributeInfos[i];
                    object attributeValue = domNode.GetAttribute(attributeInfo);
                    if (m_defaultValues != null && m_defaultValues.Length > i)
                    {
                        if (attributeValue != m_defaultValues[i])
                            return true;
                    }
                    else if (attributeValue != attributeInfo.DefaultValue)
                        return true;
                }
            }

            return false;

        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value</summary>
        /// <param name="component">The component with the property value that is to be reset to the default value</param>
        public override void ResetValue(object component)
        {
            DomNode domNode = GetNode(component);
            if (domNode != null)
            {
                for (int i = 0; i < m_attributeInfos.Length; ++i)
                {
                    AttributeInfo attributeInfo = m_attributeInfos[i];
                    object attributeValue = domNode.GetAttribute(attributeInfo);
                    if (m_defaultValues != null && m_defaultValues.Length > i)
                        domNode.SetAttribute(attributeInfo, m_defaultValues[i]);
                    else if (attributeValue != attributeInfo.DefaultValue)
                        domNode.SetAttribute(attributeInfo, attributeInfo.DefaultValue);
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the result value of the property on a component</summary>
        /// <param name="component">The component with the property for which to retrieve the value</param>
        /// <returns>The value of a property for a given component</returns>
        public override object GetValue(object component)
        {
            object[] attributeValues = new object[m_attributeInfos.Length];

            DomNode domNode = GetNode(component);
            if (domNode != null)
                for (int i = 0; i < attributeValues.Length; ++i)
                    attributeValues[i] = domNode.GetAttribute(m_attributeInfos[i]);

            return attributeValues;
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value</summary>
        /// <param name="component">The component with the property value that is to be set</param>
        /// <param name="value">The new value</param>
        public override void SetValue(object component, object value)
        {
            // value should be an array of values
            DomNode domNode = GetNode(component);
            if (domNode == null)
                throw new InvalidOperationException("Attempted to set value of an invalid or null object.");

            object[] attributeValues = value as object[];
            if (attributeValues.Length != m_attributeInfos.Length)
                throw new InvalidOperationException("Array of values has incorrect dimension.");

            for (int i = 0; i < m_attributeInfos.Length; ++i)
                domNode.SetAttribute(m_attributeInfos[i], ((Array)value).GetValue(i));
        }


        private readonly AttributeInfo[] m_attributeInfos;
        private readonly ChildInfo[] m_childInfos;
        private readonly object[] m_defaultValues;
    }

}