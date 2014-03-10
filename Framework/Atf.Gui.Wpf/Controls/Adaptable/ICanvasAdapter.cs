//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for adapters that define a rectangular canvas, with a
    /// rectangular window for viewing it</summary>
    public interface ICanvasAdapter
    {
        /// <summary>
        /// Gets or sets the canvas bounds</summary>
        Rect Bounds
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised after the canvas bounds change</summary>
        event EventHandler BoundsChanged;

        /// <summary>
        /// Gets or sets the window's bounding rectangle, which defines the visible
        /// area of the canvas</summary>
        Rect WindowBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised after the window bounds change</summary>
        event EventHandler WindowBoundsChanged;
    }
}
