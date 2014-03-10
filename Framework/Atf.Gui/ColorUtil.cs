//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf
{
    /// <summary>
    /// Static class with useful Color struct utilities</summary>
    public static class ColorUtil
    {
        /// <summary>
        /// Gets a color from a base color, lightened or darkened by the given amount</summary>
        /// <param name="color">Base color</param>
        /// <param name="amount">Amount to darken or lighten, should be > 0</param>
        /// <returns>Lightened or darkened color</returns>
        public static Color GetShade(Color color, float amount)
        {
            amount = Math.Max(0.0f, amount);
            int r = (int)Math.Min(255, color.R * amount);
            int g = (int)Math.Min(255, color.G * amount);
            int b = (int)Math.Min(255, color.B * amount);
            return Color.FromArgb(color.A, r, g, b);
        }

        /// <summary>
        /// Calculate a Color from alpha, hue, saturation, and brightness</summary>
        /// <param name="a">Alpha, in [0..255]</param>
        /// <param name="h">Hue, in [0..360]</param>
        /// <param name="s">Saturation, in [0..1]</param>
        /// <param name="b">Brightness, in [0..1]</param>
        /// <remarks>Algorithm from http://blogs.msdn.com/cjacks/archive/2006/04/12/575476.aspx </remarks>
        public static Color FromAhsb(int a, float h, float s, float b)
        {
            if (0f == s) 
            {
                int level = Convert.ToInt32(b*255);
                return Color.FromArgb(a, level, level, level);
            }

            float fMax, fMid, fMin;
            int iSextant, iMax, iMid, iMin;

            if (0.5 < b) 
            {
                fMax = b - (b * s) + s;
                fMin = b + (b * s) - s;
            } 
            else 
            {
                fMax = b + (b * s);
                fMin = b - (b * s);
            }

            iSextant = (int)Math.Floor(h / 60f);
            if (300f <= h) 
            {
                h -= 360f;
            }
            h /= 60f;
            h -= 2f * (float)Math.Floor(((iSextant + 1f) % 6f) / 2f);
            if (0 == iSextant % 2) 
            {
                fMid = h * (fMax - fMin) + fMin;
            } 
            else 
            {
                fMid = fMin - h * (fMax - fMin);
            }

            iMax = Convert.ToInt32(fMax * 255);
            iMid = Convert.ToInt32(fMid * 255);
            iMin = Convert.ToInt32(fMin * 255);

            switch (iSextant) 
            {
                case 1:
                    return Color.FromArgb(a, iMid, iMax, iMin);
                case 2:
                    return Color.FromArgb(a, iMin, iMax, iMid);
                case 3:
                    return Color.FromArgb(a, iMin, iMid, iMax);
                case 4:
                    return Color.FromArgb(a, iMid, iMin, iMax);
                case 5:
                    return Color.FromArgb(a, iMax, iMin, iMid);
                default:
                    return Color.FromArgb(a, iMax, iMid, iMin);
            }
        }
    }
}

