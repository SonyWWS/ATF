//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// An annotation on a diagram</summary>
    public interface IAnnotation
    {
        /// <summary>
        /// Gets the annotation text</summary>
        string Text
        {
            get;
        }

        /// <summary>
        /// Gets the annotation bounds, in world coordinates</summary>
        Rectangle Bounds
        {
            get;
        }

        /// <summary>
        /// Sets the size of the annotation's text, measured using the annotation font</summary>
        /// <param name="size">Text size</param>
        void SetTextSize(Size size);
    }
}
