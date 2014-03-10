//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;
using System.Collections.Generic;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// This interface is used for objects that can own forms. It's useful for property editing controls that want to use
    /// forms along with controls, but do not want to add the forms to the Controls list.
    /// This provides a way for users of IFormsOwner controls to consider the forms as part of the control, even though those
    /// forms may not be added to the Controls list.</summary>
    interface IFormsOwner
    {
        /// <summary>
        /// Gets the forms owned by this object</summary>
        IEnumerable<Form> Forms { get; }
    }
}
