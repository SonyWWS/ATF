//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Interface for control adapters that perform selection</summary>
    public interface ISelectionAdapter
    {
        /// <summary>
        /// Event that is raised after the user clicks on a selected item</summary>
        event EventHandler<DiagramHitEventArgs> SelectedItemHit;
    }
}
