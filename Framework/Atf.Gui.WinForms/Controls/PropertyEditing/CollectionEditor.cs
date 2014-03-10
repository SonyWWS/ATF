//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Editor that provides collection editing controls</summary>
    public class CollectionEditor : IPropertyEditor
    {
        #region IPropertyEditor Members

        /// <summary>
        /// Obtains a control to edit a given property. Changes to the selection set
        /// cause this method to be called again (and passed a new 'context'),
        /// unless ICacheablePropertyControl is implemented on the control. For
        /// performance reasons, it is highly recommended that the control implement
        /// the ICacheablePropertyControl interface.</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public Control GetEditingControl(PropertyEditorControlContext context)
        {
            return new CollectionEditingControl(context);
        }

        #endregion
    }
}
