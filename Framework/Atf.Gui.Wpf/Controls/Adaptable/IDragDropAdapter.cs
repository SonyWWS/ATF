//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for adapters that allow drag and drop</summary>
    public interface IDragDropAdapter
    {
        /// <summary>
        /// Gets or sets the mouse position. May be null</summary>
        Point? MousePosition { get; set; }
    }
}
