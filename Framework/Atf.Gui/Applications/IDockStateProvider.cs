//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface to access the dock panel state</summary>
    public interface IDockStateProvider
    {
        /// <summary>
        /// Gets or sets the dock panel state</summary>
        object DockState { get; set; }

        /// <summary>
        /// Event raised when the dock state changes</summary>
        event EventHandler DockStateChanged;
    }
}