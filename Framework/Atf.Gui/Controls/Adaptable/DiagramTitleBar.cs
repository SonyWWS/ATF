//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Specifies a hit on an item's titlebar part</summary>
    public class DiagramTitleBar
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="item">Item with titlebar</param>
        public DiagramTitleBar(object item)
        {
            Item = item;
        }

        /// <summary>
        /// Item with titlebar</summary>
        public readonly object Item;
    }
}
