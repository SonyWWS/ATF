//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Visual indicators for state transition diagrams</summary>
    [Flags]
    public enum StateIndicators
    {
        /// <summary>
        /// Displays a breakpoint indicator on state</summary>
        Breakpoint,

        /// <summary>
        /// Displays an "active" indicator on state</summary>
        Active,
    }
}
