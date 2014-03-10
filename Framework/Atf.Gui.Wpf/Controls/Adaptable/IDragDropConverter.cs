//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for adapters that convert dragged and dropped items, such as from a palette</summary>
    public interface IDragDropConverter
    {
        /// <summary>
        /// Converts from dragged and dropped items to actual items</summary>
        /// <param name="items">Items to convert</param>
        IEnumerable<object> Convert(IEnumerable<object> items);
    }
}
