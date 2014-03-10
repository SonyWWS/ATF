//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary> 
    /// Interface for property descriptors (System.ComponentModel.PropertyDescriptor) that need to 
    /// provide their own logic for sorting objects represented by this property descriptor</summary> 
    public interface IPropertyCustomSorter
    {
        /// <summary> 
        /// Gets the IComparer object, used for sorting objects that share this property descriptor</summary> 
        /// <param name="ascending">Whether or not the sorting of the objects should be ascending</param> 
        /// <returns>The IComparer object or null, if the default object sorting should be done</returns> 
        System.Collections.IComparer GetComparer(bool ascending);
    }
}
