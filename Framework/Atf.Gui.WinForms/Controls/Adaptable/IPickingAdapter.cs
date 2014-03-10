//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Obsolete interface for control adapters that perform picking</summary>
    /// <remarks>Please consider implementing and using IPickingAdapter2, which doesn't use
    /// the Region type.</remarks>
    public interface IPickingAdapter
    {
        /// <summary>
        /// Performs hit test for a point, in client coordinates</summary>
        /// <param name="p">Pick point, in client coordinates</param>
        /// <returns>Hit record for a point, in client coordinates</returns>
        DiagramHitRecord Pick(Point p);

        /// <summary>
        /// Performs hit testing for a region, in client coordinates</summary>
        /// <param name="region">Pick region, in client coordinates</param>
        /// <returns>Items that overlap with the region, in client coordinates</returns>
        IEnumerable<object> Pick(Region region);

        /// <summary>
        /// Gets a bounding rectangle for the items, in client coordinates</summary>
        /// <param name="items">Items to bound</param>
        /// <returns>Bounding rectangle for the items, in client coordinates</returns>
        Rectangle GetBounds(IEnumerable<object> items);
    }
}
