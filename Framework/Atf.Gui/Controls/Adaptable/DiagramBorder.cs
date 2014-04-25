//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Diagram border, which specifies a hit on an item's border part</summary>
    public class DiagramBorder
    {
        /// <summary>
        /// Enumeration for part of border</summary>
        public enum BorderType
        {
            /// <summary>Not on border</summary>
            None,
            /// <summary>Left side of border</summary>
            Left,
            /// <summary>Right side of border</summary>
            Right,
            /// <summary>Top of border</summary>
            Top,
            /// <summary>Bottom of border</summary>
            Bottom,
            /// <summary>Upper left corner of border</summary>
            UpperLeftCorner,
            /// <summary>Upper right corner of border</summary>
            UpperRightCorner,
            /// <summary>Lower left corner of border</summary>
            LowerLeftCorner,
            /// <summary>Lower right corner of border</summary>
            LowerRightCorner  
        }

        /// <summary>
        /// Constructor with item</summary>
        /// <param name="item">Item with border</param>
        public DiagramBorder(object item)
        {
            Item = item;
            Border = BorderType.None;
        }

        /// <summary>
        /// Constructor with item and border type</summary>
        /// <param name="item">Item with border</param>
        /// <param name="border">Border type</param>
        public DiagramBorder(object item, BorderType border)
        {
            Item = item;
            Border = border;
        }

        /// <summary>
        /// Item with border</summary>
        public readonly object Item;
        /// <summary>
        /// Gets or sets border type</summary>
        public BorderType Border { get; set; }
    }
}
