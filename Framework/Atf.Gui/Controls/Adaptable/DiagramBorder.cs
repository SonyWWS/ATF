//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Diagram border, which specifies a hit on an item's border part</summary>
    public class DiagramBorder
    {
        public enum BorderType
        {
            None,
            Left,
            Right,
            Top,
            Bottom,
            UpperLeftCorner,
            UpperRightCorner,
            LowerLeftCorner,
            LowerRightCorner  
        }

        public DiagramBorder(object item)
        {
            Item = item;
            Border = BorderType.None;
        }

        public DiagramBorder(object item, BorderType border)
        {
            Item = item;
            Border = border;
        }

        public readonly object Item;
        public BorderType Border { get; set; }
    }
}
