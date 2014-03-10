//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for a layering context. Layering contexts control item visibility and
    /// provide a tree view of layers.</summary>
    public interface ILayeringContext : IVisibilityContext, ITreeView, IItemView
    {
        /// <summary>
        /// Sets the active item in the layering context; used by UI components to
        /// set insertion point as the user selects and edits</summary>
        /// <param name="item">Active layer or item</param>
        void SetActiveItem(object item);
    }
}

