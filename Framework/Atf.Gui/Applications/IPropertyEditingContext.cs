//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for contexts in which properties can be edited by controls,
    /// such as the PropertyGrid or GridControl</summary>
    public interface IPropertyEditingContext
    {
        /// <summary>
        /// Gets an enumeration of the items with properties</summary>
        IEnumerable<object> Items
        {
            get;
        }

        /// <summary>
        /// Gets an enumeration of the property descriptors for the items</summary>
        IEnumerable<PropertyDescriptor> PropertyDescriptors
        {
            get;
        }
    }
}
