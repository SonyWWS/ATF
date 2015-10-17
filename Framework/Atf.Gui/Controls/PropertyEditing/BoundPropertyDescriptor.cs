//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// A specialization of System.ComponentModel.PropertyDescriptor that is bound
    /// to a specific property of an object or type. If the property's setter is private,
    /// this BoundPropertyDescriptor's IsReadOnly property is true.</summary>
    /// <remarks>Use this class to expose an object or type's property for property editing.</remarks>
    public class BoundPropertyDescriptor : PropertyDescriptor
    {
        /// <summary>
        /// Constructor for instance properties</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="expression">Lambda expression that accesses the property;
        /// e.g., () => myObject.MyProperty</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        public BoundPropertyDescriptor(
            object owner,
            Expression<Func<object>> expression,
            string displayName,
            string category,
            string description)

            : this(displayName, category, description)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(expression);
            Init(owner, null, null, propertyInfo, null, null);
        }

        /// <summary>
        /// Constructor for static properties</summary>
        /// <param name="ownerType">Type holding static property</param>
        /// <param name="expression">Lambda expression that accesses the property;
        /// e.g., () => MyClass.MyProperty</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        public BoundPropertyDescriptor(
            Type ownerType,
            Expression<Func<object>> expression,
            string displayName,
            string category,
            string description)

            : this(displayName, category, description)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(expression);
            Init(null, ownerType, null, propertyInfo, null, null);
        }

        /// <summary>
        /// Constructor for instance properties</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="expression">Lambda expression that accesses the property;
        /// e.g., () => myObject.MyProperty</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Editor for property</param>
        /// <param name="converter">TypeConverter for property</param>
        public BoundPropertyDescriptor(
            object owner,
            Expression<Func<object>> expression,
            string displayName,
            string category,
            string description,
            object editor,
            TypeConverter converter)

            : this(displayName, category, description)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(expression);
            Init(owner, null, null, propertyInfo, editor, converter);
        }

        /// <summary>
        /// Constructor for static properties</summary>
        /// <param name="ownerType">Type holding static property</param>
        /// <param name="expression">Lambda expression that accesses the property;
        /// e.g., () => MyClass.MyProperty</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Editor for property</param>
        /// <param name="converter">TypeConverter for property</param>
        public BoundPropertyDescriptor(
            Type ownerType,
            Expression<Func<object>> expression,
            string displayName,
            string category,
            string description,
            object editor,
            TypeConverter converter)

            : this(displayName, category, description)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(expression);
            Init(null, ownerType, null, propertyInfo, editor, converter);
        }

        /// <summary>
        /// Constructor for instance properties</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        public BoundPropertyDescriptor(
            object owner,
            string name,
            string displayName,
            string category,
            string description)

            : this(displayName, category, description)
        {
            Init(owner, null, name, null, null, null);
        }

        /// <summary>
        /// Constructor for instance properties</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Editor for property</param>
        public BoundPropertyDescriptor(
            object owner,
            string name,
            string displayName,
            string category,
            string description,
            object editor)

            : this(displayName, category, description)
        {
            Init(owner, null, name, null, editor, null);
        }

        /// <summary>
        /// Constructor for instance properties</summary>
        /// <param name="owner">Property owner</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Editor for property</param>
        /// <param name="converter">TypeConverter for property</param>
        public BoundPropertyDescriptor(
            object owner,
            string name,
            string displayName,
            string category,
            string description,
            object editor,
            TypeConverter converter)

            : this(displayName, category, description)
        {
            Init(owner, null, name, null, editor, converter);
        }

        /// <summary>
        /// Constructor for static properties</summary>
        /// <param name="ownerType">Type holding static property</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        public BoundPropertyDescriptor(
            Type ownerType,
            string name,
            string displayName,
            string category,
            string description)

            : this(displayName, category, description)
        {
            Init(null, ownerType, name, null, null, null);
        }

        /// <summary>
        /// Constructor for static properties</summary>
        /// <param name="ownerType">Type holding static property</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Editor for property</param>
        public BoundPropertyDescriptor(
            Type ownerType,
            string name,
            string displayName,
            string category,
            string description,
            object editor)

            : this(displayName, category, description)
        {
            Init(null, ownerType, name, null, editor, null);
        }

        /// <summary>
        /// Constructor for static properties</summary>
        /// <param name="ownerType">Type holding static property</param>
        /// <param name="name">Property name</param>
        /// <param name="displayName">Property display name</param>
        /// <param name="category">Property category</param>
        /// <param name="description">Property description</param>
        /// <param name="editor">Editor for property</param>
        /// <param name="converter">TypeConverter for property</param>
        public BoundPropertyDescriptor(
            Type ownerType,
            string name,
            string displayName,
            string category,
            string description,
            object editor,
            TypeConverter converter)

            : this(displayName, category, description)
        {
            Init(null, ownerType, name, null, editor, converter);
        }

        private void Init(
            object owner,
            Type ownerType,
            string name,
            PropertyInfo propertyInfo,
            object editor,
            TypeConverter converter)
        {
            m_owner = owner;

            // if given the property owner, ignore the ownerType parameter
            if (owner != null)
                ownerType = owner.GetType();
            m_ownerType = ownerType;

            if (string.IsNullOrEmpty(name))
            {
                if (propertyInfo == null)
                    throw new ArgumentException("either 'name' or 'propertyInfo' must be non-null");
                name = propertyInfo.Name;
            }

            // if given the property info, don't use reflection to find it
            m_propertyInfo = propertyInfo;
            if (m_propertyInfo == null)
            {
                m_propertyInfo = m_ownerType.GetProperty(
                    name,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.Static);

                if (m_propertyInfo == null)
                    throw new ArgumentException(name + ":  Property doesn't exist");
            }

            // look at "set_" method to determine if property is read-only.
            // (PropertyInfo.CanWrite will return true if there is a set
            // accessor, even if that accessor is made inaccessible using
            // asymmetric accessor accessibility.)
            MethodInfo setInfo = m_ownerType.GetMethod("set_" + name,
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.Static);

            m_readOnly = (setInfo == null);

            m_editor = editor;
            m_typeConverter = converter;
        }

        private BoundPropertyDescriptor(
            string displayName,
            string category,
            string description)

            : base(displayName,
                   new Attribute[] { new CategoryAttribute(category),
                                     new DescriptionAttribute(description), })
        {
        }

        /// <summary>
        /// When overridden in a derived class, returns whether resetting an object changes its value</summary>
        /// <param name="component">The component to test for reset capability</param>
        /// <returns><c>True</c> if resetting the component changes its value</returns>
        public override bool CanResetValue(object component)
        {
            object defaultValue;
            return GetDefaultValue(out defaultValue) && !Object.Equals(GetValue(null), defaultValue);
        }

        /// <summary>
        /// Gets the component this property is bound to. Is null, if bound to a static class's property.</summary>
        public object Owner
        {
            get { return m_owner; }
        }

        /// <summary>
        /// Gets the type of the component this property is bound to</summary>
        public override Type ComponentType
        {
            get { return m_ownerType; }
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
        /// <param name="component">The component with the property value that is to be reset to the default value</param>
        public override void ResetValue(object component)
        {
            object defaultValue;
            GetDefaultValue(out defaultValue);
            SetValue(component, defaultValue);
        }

        /// <summary>
        /// Determines whether the value of this property needs to be persisted</summary>
        /// <param name="component">The component with the property to be examined for persistence</param>
        /// <returns><c>True</c> if the property should be persisted</returns>
        public override bool ShouldSerializeValue(object component)
        {
            object val = GetValue(component);

            object defaultValue;
            if (!GetDefaultValue(out defaultValue) && val == null)
                return false;
            else
                return !val.Equals(defaultValue);
        }

        /// <summary>
        /// Returns the Owner's value (if the Owner is not null) or the component's value of the property</summary>
        /// <param name="component">Component to examine</param>
        /// <returns>The value of a property</returns>
        public override object GetValue(object component)
        {
            if (m_owner != null)
                component = m_owner;
            return m_propertyInfo.GetValue(component, null);
        }

        /// <summary>
        /// Sets the value of the Owner (if not null) or component</summary>
        /// <param name="component">Component</param>
        /// <param name="value">The new value</param>
        public override void SetValue(object component, object value)
        {
            if (m_owner != null)
                component = m_owner;
            m_propertyInfo.SetValue(component, value, null);
        }

        /// <summary>
        /// Gets the default value</summary>
        /// <param name="result">Is set to the default value or null, if it couldn't be determined</param>
        /// <returns>Whether or not the default value was determined</returns>
        /// <remarks>Uses reflection to look for a DefaultValueAttribute</remarks>
        public virtual bool GetDefaultValue(out object result)
        {
            bool foundDefault = false;
            result = null;
            object[] attributes = m_propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false);
            if (attributes.Length > 0)
            {
                foundDefault = true;
                result = (attributes[0] as DefaultValueAttribute).Value;
                if (result != null && result.GetType() != m_propertyInfo.PropertyType)
                {
                    // Default value type is not the same as the property type; convert it.
                    // This can happen if the property's type is not CLS-compliant (e.g. UInt32).
                    TypeConverter converter = TypeDescriptor.GetConverter(result);
                    if (converter.CanConvertTo(m_propertyInfo.PropertyType))
                    {
                        result = converter.ConvertTo(result, m_propertyInfo.PropertyType);
                    }
                    else
                    {
                        // Try using the converter associated with the source instead of the target
                        // (Not sure if this is useful for not, but can it hurt?)
                        converter = TypeDescriptor.GetConverter(m_propertyInfo.PropertyType);
                        if (converter.CanConvertFrom(result.GetType()))
                        {
                            result = converter.ConvertFrom(result);
                        }
                    }
                }
            }

            return foundDefault;
        }

        /// <summary>
        /// Returns an editor of the specified type</summary>
        /// <param name="editorBaseType">Base type of editor, which is used to differentiate between multiple
        /// editors that a property supports</param>
        /// <returns>An instance of the requested editor type, or null if an editor cannot be found</returns>
        public override object GetEditor(Type editorBaseType)
        {
            if (m_editor != null &&
                editorBaseType.IsInstanceOfType(m_editor))
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

        // Does not return null. Will throw an exception if 'expression' is poorly formed.
        private static PropertyInfo GetPropertyInfo(Expression<Func<object>> expression)
        {
            PropertyInfo propertyInfo = null;
            MemberExpression memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
            {
                // this is the usual case for when a property has a public getter and setter
                propertyInfo = memberExpression.Member as PropertyInfo;
            }
            else
            {
                // if the setter is private, the expression is a UnaryExpression type for some reason.
                UnaryExpression unaryExpression = expression.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                    if (memberExpression != null)
                        propertyInfo = memberExpression.Member as PropertyInfo;
                }
            }

            if (propertyInfo == null)
                throw new ArgumentException(
                    "lambda expression was not properly formed." +
                    " Should be \"() => myObject.MyProperty\" or" +
                    " \"() => MyClass.MyProperty\"");

            return propertyInfo;
        }

        object m_owner;
        Type m_ownerType;
        PropertyInfo m_propertyInfo;
        bool m_readOnly;
        private object m_editor;
        private TypeConverter m_typeConverter;
    }
}
