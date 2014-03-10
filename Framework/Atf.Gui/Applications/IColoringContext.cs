//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Applications
{

    public enum ColoringTypes
    {
        ForeColor, // foreground color
        BackColor, // background color
    }

    /// <summary>
    /// Interface for contexts where items can be colored</summary>
    public interface IColoringContext
    {
        /// <summary>
        /// Gets the item's specified color in the context</summary>
        /// <param name="type">Coloring type</param>
        /// <param name="item">Item</param>
        Color GetColor(ColoringTypes type, object item);

        /// <summary>
        /// Returns whether the item can be colored</summary>
        /// <param name="type">Coloring type</param>
        /// <param name="item">Item to color</param>
        /// <returns>True iff the item can be colored</returns>
        bool CanSetColor(ColoringTypes type, object item);

        /// <summary>
        /// Sets the item's color</summary>
        /// <param name="type">Coloring type</param>
        /// <param name="item">Item to name</param>
        /// <param name="newValue">Item new color</param>
        void SetColor(ColoringTypes type, object item, Color newValue);
    }
}
