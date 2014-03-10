//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Interface for drag-selecting adapters</summary>
    public interface IDragSelector
    {
        /// <summary>
        /// Event that is raised after user drag-selects items</summary>
        event EventHandler<DragSelectionEventArgs> Selected;
    }
}
