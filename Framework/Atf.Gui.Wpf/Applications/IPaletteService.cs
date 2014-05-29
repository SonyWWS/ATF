//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for a palette service, which presents palette items
    /// that can be dragged and dropped into other controls</summary>
    public interface IPaletteService : Sce.Atf.Applications.IPaletteService
    {
        /// <summary>
        /// Converts from palette items to actual items</summary>
        /// <param name="items">Items to convert</param>
        /// <returns>Enumeration of actual items, converted from palette items</returns>
        IEnumerable<object> Convert(IEnumerable<object> items);
    }
}
