//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Linear gradient properties for controls</summary>
    public struct DockColors
    {
        /// <summary>
        /// Gets or sets ControlGradient for auto hide dock strip</summary>
        public ControlGradient AutoHideDockStripGradient { get; set; }
        /// <summary>
        /// Gets or sets ControlGradient for auto hide tab</summary>
        public ControlGradient AutoHideTabGradient { get; set; }

        /// <summary>
        /// Gets or sets ControlGradient for document's active tab</summary>
        public ControlGradient DocumentActiveTabGradient { get; set; }
        /// <summary>
        /// Gets or sets ControlGradient for document's inactive tab</summary>
        public ControlGradient DocumentInactiveTabGradient { get; set; }
        /// <summary>
        /// Gets or sets ControlGradient for document's dock strip</summary>
        public ControlGradient DocumentDockStripGradient { get; set; }

        /// <summary>
        /// Gets or sets ControlGradient for tool window active caption</summary>
        public ControlGradient ToolWindowActiveCaptionGradient { get; set; }
        /// <summary>
        /// Gets or sets ControlGradient for tool window inactive caption</summary>
        public ControlGradient ToolWindowInactiveCaptionGradient { get; set; }
        /// <summary>
        /// Gets or sets ControlGradient for tool window active tab</summary>
        public ControlGradient ToolWindowActiveTabGradient { get; set; }
        /// <summary>
        /// Gets or sets ControlGradient for tool window inactive tab</summary>
        public ControlGradient ToolWindowInactiveTabGradient { get; set; }
        /// <summary>
        /// Gets or sets ControlGradient for tool window dock strip</summary>
        public ControlGradient ToolWindowDockStripGradient { get; set; }
    }
}
