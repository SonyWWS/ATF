//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Interface for property editors, which supply controls that are embedded in
    /// complex property editing controls</summary>
    /// <remarks>There is one IPropertyEditor for each PropertyDescriptor. See various property descriptor's
    /// (such as UnboundPropertyDescriptor) GetEditor() methods. Each PropertyDescriptor (and therefore each IPropertyEditor)
    /// is re-created when the selection changes. See the static PropertyEditingContext's
    /// GetPropertyDescriptors(). So, the PropertyDescriptor is created once for each annotated
    /// property, but then it's shared by all of the property editors that use that
    /// PropertyEditingContext.
    /// If the Control is cacheable (ICacheablePropertyControl.Cacheable is true), the
    /// GetEditingControl method is only called once per annotated property per PropertyView
    /// (the base class for the 2-column property editor and the grid-style property editor).</remarks>
    public interface IPropertyEditor
    {
        /// <summary>
        /// Obtains a control to edit a given property. Changes to the selection set
        /// cause this method to be called again (and passed a new 'context'),
        /// unless ICacheablePropertyControl is implemented on the control. For
        /// performance reasons, it is highly recommended that the control implement
        /// the ICacheablePropertyControl interface.</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        Control GetEditingControl(PropertyEditorControlContext context);
    }
}
