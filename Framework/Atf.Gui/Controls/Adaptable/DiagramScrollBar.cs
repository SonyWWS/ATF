//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// DiagramScrollBar orientation</summary>
    public enum Orientation
    {
        /// <summary>
        /// Control or layout should be vertically oriented</summary>
        Vertical,
        /// <summary>
        /// Control or layout should be horizontally oriented</summary>
        Horizontal,
    }

    /// <summary>
    /// Diagram scrollbar, which specifies a hit on an item's scrollbar part</summary>
    public class DiagramScrollBar
    {
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="item">Object with scrollbar</param>
        /// <param name="orientation">Scrollbar orientation</param>
        public DiagramScrollBar(object item, Orientation orientation)
        {
            Item = item;
            BarOrientation = orientation;
        }

        /// <summary>
        /// Gets or sets whether the scrollbar is displayed horizontally or vertically</summary>
        public Orientation BarOrientation { get; set; }

        /// <summary>
        /// Object with scrollbar</summary>
        public readonly object Item;
    }
}
