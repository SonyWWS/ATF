//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for adapters that define a rectangular canvas, with a
    /// rectangular window for viewing it</summary>
    public interface ICanvasAdapter
    {
        /// <summary>
        /// Gets or sets the canvas bounds in world coordinates</summary>
        Rectangle Bounds
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised after the canvas bounds change</summary>
        event EventHandler BoundsChanged;

        /// <summary>
        /// Gets or sets the window's bounding rectangle, which defines the visible
        /// area of the canvas, in the window's client coordinates</summary>
        Rectangle WindowBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised after the window bounds change</summary>
        event EventHandler WindowBoundsChanged;
    }
}
