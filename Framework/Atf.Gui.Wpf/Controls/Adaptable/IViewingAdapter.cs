//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for control adapters that perform picking</summary>
    public interface IViewingAdapter
    {
        /// <summary>
        /// Tells the adapter to recalculate the bounds</summary>
        void Invalidate();

        /// <summary>
        /// Returns whether the items can be framed in the current view</summary>
        /// <param name="items">Items to frame</param>
        /// <returns><c>True</c> if the items can be framed in the current view</returns>
        bool CanFrame(IEnumerable<object> items);

        /// <summary>
        /// Frames the items in the current view, i.e., sets scroll and zoom so that 
        /// the item's bounding rectangle almost fills the client area</summary>
        /// <param name="items">Items to frame</param>
        void Frame(IEnumerable<object> items);

        /// <summary>
        /// Returns whether the items can be made visible in the current view;
        /// they may not be centered as in the Frame method</summary>
        /// <param name="items">Items to show</param>
        /// <returns><c>True</c> if the items can be made visible in the current view</returns>
        bool CanEnsureVisible(IEnumerable<object> items);

        /// <summary>
        /// Ensures that the items are visible in the current view</summary>
        /// <param name="items">Items to show</param>
        void EnsureVisible(IEnumerable<object> items);
    }
}
