//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Interface for property editing control owners, which contain
    /// property editing controls</summary>
    public interface IPropertyEditingControlOwner
    {
        /// <summary>
        /// Gets the selected objects for a property edit</summary>
        object[] SelectedObjects
        {
            get;
        }
    }
}
