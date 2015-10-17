//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for a prototyping context, which can present a tree view of its contents
    /// and create IDataObjects from them</summary>
    public interface ITemplatingContext : ITreeView, IItemView
    {
        /// <summary>
        /// Sets the active item in the prototyping context; used by UI components to
        /// set insertion point as the user selects and edits</summary>
        /// <param name="item">Active layer or item</param>
        void SetActiveItem(object item);

        /// <summary>
        /// Gets the IDataObject for the items being dragged from a prototype lister, for
        /// use in a drag-and-drop operation</summary>
        /// <param name="items">Objects being dragged</param>
        /// <returns>IDataObject representing the dragged items</returns>
        IDataObject GetInstances(IEnumerable<object> items);

        /// <summary>
        /// Returns <c>True</c> if the reference can reference the specified target item</summary>
        /// <param name="item">Template item to be referenced</param>
        /// <returns><c>True</c> if the reference can reference the specified target item</returns>
        bool CanReference(object item);

        /// <summary>
        /// Creates a reference instance that references the specified target item</summary>
        /// <param name="item">Item to create reference for</param>
        /// <returns>Reference instance</returns>
        object CreateReference(object item);
    }
}

