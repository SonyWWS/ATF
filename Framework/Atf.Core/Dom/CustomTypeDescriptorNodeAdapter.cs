//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Node adapter to get PropertyDescriptors from NodeType and other metadata</summary>
    public class CustomTypeDescriptorNodeAdapter : DomNodeAdapter, ICustomTypeDescriptor
    {
        /// <summary>
        /// Creates an array of property descriptors that are associated with the adapted DomNode's
        /// DomNodeType. No duplicates are in the array (based on the property descriptor's Name
        /// property).</summary>
        /// <returns>Array of property descriptors</returns>
        protected virtual System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
        {
            HashSet<string> names = new HashSet<string>();
            List<System.ComponentModel.PropertyDescriptor> result = new List<System.ComponentModel.PropertyDescriptor>();

            DomNodeType nodeType = DomNode.Type;
            while (nodeType != null)
            {
                PropertyDescriptorCollection propertyDescriptors = nodeType.GetTag<PropertyDescriptorCollection>();
                if (propertyDescriptors != null)
                {
                    foreach (System.ComponentModel.PropertyDescriptor propertyDescriptor in propertyDescriptors)
                    {
                        // Use combination of category and name, to allow having properties with the
                        // same display name under different categories.
                        string fullName = string.Format("{0}_{1}", propertyDescriptor.Category, propertyDescriptor.Name);
                        
                        // Filter out duplicate names, so derived type data overrides base type data)
                        if (!names.Contains(fullName))
                        {
                            names.Add(fullName);
                            result.Add(propertyDescriptor);
                        }
                    }
                }
                nodeType = nodeType.BaseType;
            }

            return result.ToArray();
        }

        /// <summary>
        /// Gets class name for the node. Default is to return the node type name.</summary>
        /// <returns>Class name</returns>
        protected virtual string GetClassName()
        {
            return DomNode.Type.Name;
        }

        #region ICustomTypeDescriptor Members

        /// <summary>
        /// Returns the properties for this instance of a component</summary>
        /// <returns>A System.ComponentModel.PropertyDescriptorCollection that represents the properties for this component instance</returns>
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return new PropertyDescriptorCollection(GetPropertyDescriptors());
        }

        /// <summary>
        /// Returns the properties for this instance of a component using the attribute array as a filter</summary>
        /// <param name="attributes">An array of type System.Attribute that is used as a filter</param>
        /// <returns>A System.ComponentModel.PropertyDescriptorCollection that represents the filtered properties for this component instance</returns>
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(GetPropertyDescriptors());
        }

        /// <summary>
        /// Returns the class name of this instance of a component</summary>
        /// <returns>The class name of the object, or null if the class does not have a name</returns>
        String ICustomTypeDescriptor.GetClassName()
        {
            return GetClassName();
        }

        /// <summary>
        /// Returns a collection of custom attributes for this instance of a component</summary>
        /// <returns>An System.ComponentModel.AttributeCollection containing the attributes for this object</returns>
        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        /// <summary>
        /// Returns the name of this instance of a component</summary>
        /// <returns>The name of the object, or null if the object does not have a name</returns>
        String ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        /// <summary>
        /// Returns a type converter for this instance of a component</summary>
        /// <returns>A System.ComponentModel.TypeConverter that is the converter for this object, 
        /// or null if there is no System.ComponentModel.TypeConverter for this object</returns>
        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        /// <summary>
        /// Returns the default event for this instance of a component</summary>
        /// <returns>An System.ComponentModel.EventDescriptor that represents the default event for this object,
        /// or null if this object does not have events</returns>
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        /// <summary>
        /// Returns the default property for this instance of a component</summary>
        /// <returns>A System.ComponentModel.PropertyDescriptor that represents the default property for this object, 
        /// or null if this object does not have properties</returns>
        System.ComponentModel.PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        /// <summary>
        /// Returns an editor of the specified type for this instance of a component</summary>
        /// <param name="editorBaseType">A System.Type that represents the editor for this object</param>
        /// <returns>An System.Object of the specified type that is the editor for this object, 
        /// or null if the editor cannot be found</returns>
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        /// <summary>
        /// Returns the events for this instance of a component using the specified attribute array as a filter</summary>
        /// <param name="attributes">An array of type System.Attribute that is used as a filter</param>
        /// <returns>An System.ComponentModel.EventDescriptorCollection that represents the filtered events for this component instance</returns>
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        /// <summary>
        /// Returns the events for this instance of a component</summary>
        /// <returns>An System.ComponentModel.EventDescriptorCollection that represents the events for this component instance</returns>
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        /// <summary>
        /// Returns an object that contains the property described by the specified property descriptor</summary>
        /// <param name="pd">A System.ComponentModel.PropertyDescriptor that represents the property whose owner is to be found</param>
        /// <returns>An System.Object that represents the owner of the specified property</returns>
        object ICustomTypeDescriptor.GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd)
        {
            return DomNode;
        }

        #endregion
    }
}
