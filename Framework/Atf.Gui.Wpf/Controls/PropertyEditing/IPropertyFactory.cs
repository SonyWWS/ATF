//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Windows;

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
        /// <param name="owner">Object(s) owner</param>
        /// <returns>Initialized PropertyNode instance</returns>
        PropertyNode CreateProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable, FrameworkElement owner);
    }
}
