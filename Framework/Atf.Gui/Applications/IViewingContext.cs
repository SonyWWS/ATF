//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for contexts where items can be viewed</summary>
    public interface IViewingContext
    {
        /// <summary>
        /// Returns whether the items can be framed in the current view</summary>
        /// <param name="items">Items to frame</param>
        /// <returns>True iff the items can be framed in the current view</returns>
        bool CanFrame(IEnumerable<object> items);

        /// <summary>
        /// Frames the items in the current view</summary>
        /// <param name="items">Items to frame</param>
        void Frame(IEnumerable<object> items);

        /// <summary>
        /// Returns whether the items can be made visible in the current view;
        /// they may not be centered as in the Frame method</summary>
        /// <param name="items">Items to show</param>
        /// <returns>True iff the items can be made visible in the current view</returns>
        bool CanEnsureVisible(IEnumerable<object> items);

        /// <summary>
        /// Ensures that the items are visible in the current view</summary>
        /// <param name="items">Items to show</param>
        void EnsureVisible(IEnumerable<object> items);
    }
}
