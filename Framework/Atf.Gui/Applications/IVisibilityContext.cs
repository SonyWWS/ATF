//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for contexts where items can be shown and hidden</summary>
    public interface IVisibilityContext
    {
        /// <summary>
        /// Returns whether the item is visible</summary>
        /// <param name="item">Item</param>
        /// <returns>True iff the item is visible</returns>
        bool IsVisible(object item);

        /// <summary>
        /// Returns whether the item can be made visible and invisible</summary>
        /// <param name="item">Item</param>
        /// <returns>True iff the item can be made visible and invisible</returns>
        bool CanSetVisible(object item);

        /// <summary>
        /// Sets the visibility state of the item to the value</summary>
        /// <param name="item">Item to show or hide</param>
        /// <param name="value">True to show, false to hide</param>
        void SetVisible(object item, bool value);
    }
}
