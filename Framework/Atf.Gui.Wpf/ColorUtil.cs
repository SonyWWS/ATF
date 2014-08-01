//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Media;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Utility functions for working with colors</summary>
    public class ColorUtil
    {
        /// <summary>
        /// Gets a color from a base color, lightened or darkened by the given amount</summary>
        /// <param name="color">Base color</param>
        /// <param name="amount">Amount to darken or lighten, should be > 0</param>
        /// <param name="alpha"></param>
        /// <returns>Lightened or darkened color</returns>
        public static Color GetShade(Color color, float amount, byte alpha)
        {
            amount = Math.Max(0.0f, amount);
            byte r = (byte)Math.Min(255, color.R * amount);
            byte g = (byte)Math.Min(255, color.G * amount);
            byte b = (byte)Math.Min(255, color.B * amount);
            return Color.FromArgb(alpha, r, g, b);
        }

        /// <summary>
        /// Converts value from a string to a System.Windows.Media.Color</summary>
        /// <param name="color">Color string in hex format, e.g. "#FFFFFF"</param>
        /// <returns>The corresponding System.Windows.Media.Color</returns>
        public static Color ConvertFromString(string color)
        {
            var convertFromString = ColorConverter.ConvertFromString(color);
            if (convertFromString != null) return (Color)convertFromString;
            return Colors.White;
        }
    }
}