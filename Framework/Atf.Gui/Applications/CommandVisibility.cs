//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Enum for where command is visible, as on menus, toolbars, etc.</summary>
    [Flags]
    public enum CommandVisibility
    {
        /// <summary>
        /// Not in menus or toolbars; user may add it to toolbar</summary>
        None = 0,

        /// <summary>
        /// In application menus</summary>
        ApplicationMenu = 1,

        /// <summary>
        /// In context menus</summary>
        ContextMenu = 2,

        /// <summary>
        /// In toolbars, if icon is available</summary>
        Toolbar = 4,

        /// <summary>
        /// In both types of menus (application and context)</summary>
        Menu = ApplicationMenu | ContextMenu,

        /// <summary>
        /// In menus, and if an icon is available, in toolbars</summary>
        Default = Menu | Toolbar,

        /// <summary>
        /// In menus and toolbars</summary>
        All = Menu | Toolbar,
    }
}
