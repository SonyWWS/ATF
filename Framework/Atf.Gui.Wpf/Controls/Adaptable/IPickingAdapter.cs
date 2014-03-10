//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Windows;

using Sce.Atf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for control adapters that perform picking</summary>
    public interface IPickingAdapter
    {
        /// <summary>
        /// Performs hit test for a point, in client coordinates, returning DiagramHitRecord</summary>
        /// <param name="p">Pick point, in client coordinates</param>
        /// <returns>Hit record for a point, in client coordinates</returns>
        DiagramHitRecord Pick(Point p);

        /// <summary>
        /// Performs hit testing for a region, in client coordinates, returning enumeration of hit items</summary>
        /// <param name="region">Pick region, in client coordinates</param>
        /// <returns>Enumeration of items that overlap with the region, in client coordinates</returns>
        IEnumerable<object> Pick(Rect region);

        /// <summary>
        /// Gets a bounding rectangle for hit items, in client coordinates</summary>
        /// <param name="items">Items to bound</param>
        /// <returns>Bounding rectangle for the items, in client coordinates</returns>
        Rect GetBounds(IEnumerable<object> items);
    }
}
