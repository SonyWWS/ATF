//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Class to wrap PropertyDescriptorCollection as a CustomTypeDescriptor</summary>
    public class PropertyCollectionWrapper : CustomTypeDescriptor, ICustomTypeDescriptor
    {
        /// <summary>
        /// Constructs a wrapper from a PropertyDescriptor array</summary>
        /// <param name="properties">PropertyDescriptor array</param>
        public PropertyCollectionWrapper(PropertyDescriptor[] properties)
            : this(properties, null) { }

        /// <summary>
        /// Constructs a wrapper from a PropertyDescriptorCollection</summary>
        /// <param name="properties">PropertyDescriptor collection</param>
        public PropertyCollectionWrapper(PropertyDescriptorCollection properties)
            : this(properties, null) { }

        /// <summary>
        /// Constructs a wrapper from a PropertyDescriptor array and an owner</summary>
        /// <param name="properties">PropertyDescriptor array</param>
        /// <param name="owner">Property owner</param>
        public PropertyCollectionWrapper(PropertyDescriptor[] properties, object owner)
            : this(new PropertyDescriptorCollection(properties), owner) { }

        /// <summary>
        /// Constructs a wrapper from a PropertyDescriptorCollection and an owner</summary>
        /// <param name="properties">PropertyDescriptor collection</param>
        /// <param name="owner">Property owner</param>
        public PropertyCollectionWrapper(PropertyDescriptorCollection properties, object owner)
        {
            m_properties = properties;
            m_propertyOwner = (owner != null) ? owner : this;
        }

        /// <summary>
        /// Returns the property owner</summary>
        /// <param name="propertyDescriptor">PropertyDescriptor</param>
        /// <returns>Property owner</returns>
        public override object GetPropertyOwner(PropertyDescriptor propertyDescriptor)
        {
            return m_propertyOwner;
        }

        /// <summary>
        /// Returns a collection of the properties for this type descriptor</summary>
        /// <param name="attributes">Attributes (ignored)</param>
        /// <returns>Collection of properties for this type</returns>
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetProperties();
        }

        /// <summary>
        /// Returns a collection of the properties for this type descriptor</summary>
        /// <returns>Collection of properties for this type</returns>
        public override PropertyDescriptorCollection GetProperties()
        {
            return m_properties;
        }

        private readonly PropertyDescriptorCollection m_properties;
        private readonly object m_propertyOwner;
    }
}
