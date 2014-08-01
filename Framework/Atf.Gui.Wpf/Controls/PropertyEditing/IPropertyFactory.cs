//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// PropertyNode factory</summary>
    public interface IPropertyFactory
    {
        /// <summary>
        /// Creates and initializes PropertyNode instance from parameters</summary>
        /// <param name="instance">Object or collection of objects that share a property</param>
        /// <param name="descriptor">PropertyDescriptor of shared property</param>
        /// <param name="isEnumerable">Whether the object is enumerable</param>
        /// <param name="context">Transaction context</param>
        /// <returns>Initialized PropertyNode instance</returns>
        PropertyNode CreateProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable, ITransactionContext context);
   
    }
}
