//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Interface for property editing controls that can be cached indefinitely
    /// in spite of selection changes. This means that these controls should not
    /// hold on to selected objects outside of their PropertyEditingControlContext. Being
    /// cacheable greatly improves performance (20x to 35x in tests). Intended to
    /// be implemented on controls created by IPropertyEditor.</summary>
    public interface ICacheablePropertyControl
    {
        /// <summary>
        /// Gets <c>True</c> if this control can be used indefinitely, regardless of whether the associated
        /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
        /// This property must be constant for the life of this control.</summary>
        bool Cacheable
        {
            get;
        }
    }
}
