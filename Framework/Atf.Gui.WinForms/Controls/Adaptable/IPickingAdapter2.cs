//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for control adapters that perform picking</summary>
    public interface IPickingAdapter2
    {
        /// <summary>
        /// Performs hit test for a point, in client coordinates</summary>
        /// <param name="p">Pick point, in client coordinates</param>
        /// <returns>Hit record for a point, in client coordinates</returns>
        DiagramHitRecord Pick(Point p);

        /// <summary>
        /// Performs hit testing for rectangle bounds, in client coordinates</summary>
        /// <param name="bounds">Pick rectangle, in client coordinates</param>
        /// <returns>Items that overlap with the rectangle, in client coordinates</returns>
        IEnumerable<object> Pick(Rectangle bounds);

        /// <summary>
        /// Gets a bounding rectangle for the items, in client coordinates</summary>
        /// <param name="items">Items to bound</param>
        /// <returns>Bounding rectangle for the items, in client coordinates</returns>
        Rectangle GetBounds(IEnumerable<object> items);
    }
}
