//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// PropertyDescriptor for an attribute of a node</summary>
    public class AttributePropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Attribute's display name</param>
        /// <param name="attribute">Attribute metadata</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        public AttributePropertyDescriptor(
            string name,
            AttributeInfo attribute,
            string category,
            string description,
            bool isReadOnly)

            : this(name, attribute, category, description, isReadOnly, null, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Attribute's display name</param>
        /// <param name="attribute">Attribute metadata</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor for editing this property</param>
        public AttributePropertyDescriptor(
            string name,
            AttributeInfo attribute,
            string category,
            string description,
            bool isReadOnly,
            object editor)

            : this(name, attribute, category, description, isReadOnly, editor, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Attribute's display name</param>
        /// <param name="attribute">Attribute metadata</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor for editing this property</param>
        /// <param name="typeConverter">TypeConverter for this property</param>
        public AttributePropertyDescriptor(
            string name,
            AttributeInfo attribute,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter)

            : this(name, attribute, category, description, isReadOnly, editor, typeConverter, null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="name">Attribute's display name</param>
        /// <param name="attribute">Attribute metadata</param>
        /// <param name="category">Category of property</param>
        /// <param name="description">Description of property</param>
        /// <param name="isReadOnly">Whether or not property is read-only</param>
        /// <param name="editor">The editor for editing this property</param>
        /// <param name="typeConverter">TypeConverter for this property</param>
        /// <param name="attributes">An array of attributes for this property</param>
        public AttributePropertyDescriptor(
            string name,
            AttributeInfo attribute,
            string category,
            string description,
            bool isReadOnly,
            object editor,
            TypeConverter typeConverter,
            Attribute[] attributes)

            : base(name, attribute.Type.ClrType, category, description, isReadOnly, editor, typeConverter, attributes)
        {
            m_attributeInfo = attribute;
        }

        /// <summary>
        /// Gets the metadata for the attribute</summary>
        public AttributeInfo AttributeInfo
        {
            get { return m_attributeInfo; }
        }

        #region Overrides

        /// <summary>
        /// When overridden in a derived class, returns whether the value
        /// of this property needs to be persisted</summary>
        /// <param name="component">The component with the property to be examined for persistence</param>
        /// <returns>True iff the property should be persisted</returns>
        public override bool ShouldSerializeValue(object component)
        {
            object value = GetValue(component);
            if (value == null) return false;
            return  !value.Equals(m_attributeInfo.DefaultValue);            
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns>True iff resetting the component changes its value</returns>
        public override bool CanResetValue(object component)
        {
            if (IsReadOnly) return false;

            // Don't reset Id attribute.
            DomNode node = GetNode(component);
            if (node != null && m_attributeInfo.Equivalent(node.Type.IdAttribute))
                return false;

            object value = GetValue(component);
            return (value != null && !value.Equals(m_attributeInfo.DefaultValue))
                || (value == null && m_attributeInfo.DefaultValue != null);
        }

        /// <summary>
        /// When overridden in a derived class, resets the value for this property of the component to the default value</summary>
        /// <param name="component">The component with the property value that is to be reset to the default value</param>
        public override void ResetValue(object component)
        {
            SetValue(component, m_attributeInfo.DefaultValue);
        }

        /// <summary>
        /// When overridden in a derived class, gets the result value of the property on a component</summary>
        /// <param name="component">The component with the property for which to retrieve the value</param>
        /// <returns>The value of a property for a given component.</returns>
        public override object GetValue(object component)
        {
            object value = null;
            DomNode node = GetNode(component);
            if (node != null)
                value = node.GetAttribute(m_attributeInfo);

            return value;
        }

        /// <summary>
        /// When overridden in a derived class, sets the value of the component to a different value</summary>
        /// <param name="component">The component with the property value that is to be set</param>
        /// <param name="value">The new value</param>
        public override void SetValue(object component, object value)
        {
            DomNode node = GetNode(component);
            if (node != null)
                node.SetAttribute(m_attributeInfo, value);
        }

        /// <summary>
        /// Tests equality of property descriptor with object</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>True iff property descriptors are identical</returns>
        /// <remarks>Implements Equals() for organizing descriptors in grid controls</remarks>
        public override bool Equals(object obj)
        {
            var other = obj as AttributePropertyDescriptor;

            // If true is returned, then GetNode() must also succeed when it calls
            //  DomNode.Type.IsValid(m_attributeInfo), otherwise this AttributePropertyDescriptor
            //  will be considered identical in a dictionary, but its GetValue() will fail.
            return
                other != null &&
                m_attributeInfo.Equivalent(other.m_attributeInfo);
        }

        /// <summary>
        /// Gets hash code for property descriptor</summary>
        /// <returns>Hash code</returns>
        /// <remarks>Implements GetHashCode() for organizing descriptors in grid controls</remarks>
        public override int GetHashCode()
        {
            return m_attributeInfo.GetEquivalentHashCode();
        }

        /// <summary>
        /// Gets node from component</summary>
        /// <param name="component">Component being edited</param>
        /// <returns>DomNode</returns>
        public override DomNode GetNode(object component)
        {
            var node = component.As<DomNode>();
            if (node != null)
            {
                // Check that this attribute exists on this Node
                if (!node.Type.IsValid(m_attributeInfo))
                    return null;
            }

            return node;
        }

        #endregion

        private readonly AttributeInfo m_attributeInfo;
    }
}
