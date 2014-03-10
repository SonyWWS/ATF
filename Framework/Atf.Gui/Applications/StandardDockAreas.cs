//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Standard docking areas, which determine where controls can be docked</summary>
    [Flags]
    public enum StandardDockAreas
    {
        /// <summary>
        /// Floating control</summary>
        Float = 1,

        /// <summary>
        /// Left docking area</summary>
        DockLeft = 2,

        /// <summary>
        /// Right docking area</summary>
        DockRight = 4,

        /// <summary>
        /// Top docking area</summary>
        DockTop = 8,

        /// <summary>
        /// Bottom docking area</summary>
        DockBottom = 16,

        /// <summary>
        /// Tabbed document docking area</summary>
        Document = 32,

        /// <summary>
        /// Default docking area</summary>
        Default = Float | DockLeft | DockRight | DockTop | DockBottom | Document
    }
}
