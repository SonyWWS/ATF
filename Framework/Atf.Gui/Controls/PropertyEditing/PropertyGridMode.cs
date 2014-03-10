//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Property grid modes</summary>
    [Flags]
    public enum PropertyGridMode
    {
        /// <summary>
        /// Allows the user to select a property organization</summary>
        PropertySorting = 1,

        /// <summary>
        /// Allows the user to navigate in and out of composite properties</summary>
        AllowEditingComposites = 2,

        /// <summary>
        /// Displays property descriptions in tooltips when the mouse hovers
        /// over the property name</summary>
        DisplayTooltips = 4,

        /// <summary>
        /// Displays property descriptions in the bottom description area</summary>
        DisplayDescriptions = 8,

        /// <summary>
        /// Disables controls for searching the contents of the property grid</summary>
        DisableSearchControls = 16,

        /// <summary>
        /// Allows the user to show and hide properties (columns) in the grid</summary>
        ShowHideProperties = 32,

        /// <summary>
        /// Disable allowing user to reorder columns by dragging and dropping column headers</summary>
        DisableDragDropColumnHeaders = 64,
    }
}
