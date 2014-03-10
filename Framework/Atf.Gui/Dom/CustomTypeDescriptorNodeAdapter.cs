//Sony Computer Entertainment Confidential

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
                        // filter out duplicate names, so derived type data overrides base type data
                        if (!names.Contains(propertyDescriptor.Name))
                        {
                            names.Add(propertyDescriptor.Name);
                            result.Add(propertyDescriptor);
                        }
                    }
                }
                nodeType = nodeType.BaseType;
            }

            return result.ToArray();
        }

        #region ICustomTypeDescriptor Members

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return new PropertyDescriptorCollection(GetPropertyDescriptors());
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            return new PropertyDescriptorCollection(GetPropertyDescriptors());
        }

        String ICustomTypeDescriptor.GetClassName()
        {
            return DomNode.Type.Name;
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        String ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        System.ComponentModel.PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(System.ComponentModel.PropertyDescriptor pd)
        {
            return DomNode;
        }

        #endregion
    }
}
