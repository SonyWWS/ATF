//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


namespace Sce.Atf.Controls.Adaptable
{
    public enum Orientation
    {
        Vertical,    // Control or layout should be vertically oriented
        Horizontal,  // Control or layout should be horizontally oriented        
    }

    /// <summary>
    /// Diagram scrollbar, which specifies a hit on an item's scrollbar part</summary>
    public class DiagramScrollBar
    {
        public DiagramScrollBar(object item, Orientation orientation)
        {
            Item = item;
            BarOrientation = orientation;
        }

        /// <summary>
        /// Gets or sets whether thes crollBar is displayed horizontally or vertically</summary>
        public Orientation BarOrientation { get; set; }

        public readonly object Item;
    }
}
