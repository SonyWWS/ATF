//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Reflection;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Standard PropertyDescriptor class for implementing ICustomTypeDescriptor</summary>
    public class UnboundPropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="type">Type of property owner</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        public UnboundPropertyDescriptor(
            Type type,
            string name,
            string displayName,
            string category,
            string description)
            : this(type, name, displayName, category, description, null, null)
        {}

        /// <summary>
        /// Constructor</summary>
        /// <param name="type">Type of property owner</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Editor for property</param>
        public UnboundPropertyDescriptor(
            Type type,
            string name,
            string displayName,
            string category,
            string description,
            object editor)
            : this(type, name, displayName, category, description, editor, null)
        {}

        /// <summary>
        /// Constructor for non-static properties</summary>
        /// <param name="type">Type of property owner</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Property editor</param>
        /// <param name="converter">Type converter</param>
        public UnboundPropertyDescriptor(
            Type type,
            string name,
            string displayName,
            string category,
            string description,
            object editor,
            TypeConverter converter)

            : base(displayName, 
                   new Attribute[] { new CategoryAttribute(category),
                                     new DescriptionAttribute(description), })
        {
            m_type = type;

            m_propertyInfo = m_type.GetProperty(
                name,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static);

            if (m_propertyInfo == null)
                throw new ArgumentException(name + ":  Property doesn't exist");

            // look at "set_" method to determine if property is read-only.
            // (PropertyInfo.CanWrite will return true if there is a set
            // accessor, even if that accessor is made inaccessible using
            // asymmetric accessor accessibility.)
            MethodInfo setInfo = m_type.GetMethod("set_" + name, 
                BindingFlags.Public |                
                BindingFlags.Instance |
                BindingFlags.Static);

            m_readOnly = (setInfo == null);

            m_editor = editor;
            m_typeConverter = converter;
        }

        #region Overrides

        /// <summary>
        /// Returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns>True iff resetting the component changes its value</returns>
        public override bool CanResetValue(object component)
        {
            object defaultValue = GetDefaultValue();
            return defaultValue != null && !Object.Equals(GetValue(component), defaultValue);
        }

        /// <summary>
        /// Gets the type of the component this property is bound to</summary>
        public override Type ComponentType
        {
            get { return m_type; }
        }

        /// <summary>
        /// Gets the type of the property</summary>
        public override Type PropertyType
        {
            get { return m_propertyInfo.PropertyType; }
        }

        /// <summary>
        /// Gets whether this property is read-only</summary>
        public override bool IsReadOnly
        {
            get { return m_readOnly; }
        }

        /// <summary>
        /// Resets the value for this property of the component to the default value</summary>
        /// <param name="component">Component whose property is set to default</param>
        public override void ResetValue(object component)
        {
            object defaultValue = GetDefaultValue();
            SetValue(component, defaultValue);
        }

        /// <summary>
        /// Determines whether the value of this property needs to be persisted for a given component</summary>
        /// <param name="component">Component whose property may need to be persisted</param>
        /// <returns>True iff property needs to be persisted for a given component</returns>
        public override bool ShouldSerializeValue(object component)
        {
            object val = GetValue(component);

            object defaultValue = GetDefaultValue();
            if (defaultValue == null && val == null)
                return false;
            else
                return !val.Equals(defaultValue);
        }

        /// <summary>
        /// Gets the current value of the property on a component</summary>
        /// <param name="component">Component</param>
        /// <returns>Current value of the property</returns>
        public override object GetValue(object component)
        {
            return m_propertyInfo.GetValue(component, null);
        }

        /// <summary>
        /// Sets the value of the component to a different value</summary>
        /// <param name="component">Component</param>
        /// <param name="value">New value</param>
        public override void SetValue(object component, object value)
        {
            m_propertyInfo.SetValue(component, value, null);
        }

        /// <summary>
        /// Gets an editor of the specified type</summary>
        /// <param name="editorBaseType">The base type of editor, which is used to differentiate between multiple
        /// editors that a property supports</param>
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

        #endregion

        private object GetDefaultValue()
        {
            object result = null;
            object[] attributes = m_propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attributes.Length > 0)
            {
                result = (attributes[0] as DefaultValueAttribute).Value;
                if (result != null && result.GetType() != m_propertyInfo.PropertyType)
                {
                    // Default value type is not the same as the property type; convert it.
                    // This can happen if the property's type is not CLS-compliant (e.g. UInt32).
                    result = TypeDescriptor.GetConverter(result).ConvertTo(result, m_propertyInfo.PropertyType);
                }
            }

            return result;
        }

        private readonly Type m_type;
        private readonly PropertyInfo m_propertyInfo;
        private readonly object m_editor;
        private readonly TypeConverter m_typeConverter;
        private readonly bool m_readOnly;
    }
}
