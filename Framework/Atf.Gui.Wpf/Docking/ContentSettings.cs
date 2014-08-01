//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Enum of possible states of content</summary>
    internal enum DockState
    {
        Docked,
        Floating,
        Collapsed
    }

    /// <summary>
    /// Content settings collection class</summary>
    internal class ContentSettings
    {
        /// <summary>
        /// Get the default dock side of the content</summary>
        public DockTo DefaultDock { get; private set; }

        /// <summary>
        /// Get or set the size of the content</summary>
        public Size Size { get; set; }

        /// <summary>
        /// Get or set the dock state for content</summary>
        public DockState DockState { get; set; }

        /// <summary>
        /// Get or set the location of the content</summary>
        public Point Location { get; set; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="defaultDock">Default dock side</param>
        public ContentSettings(DockTo defaultDock)
        {
            DefaultDock = defaultDock;
            Size = new Size(0, 0);
            DockState = DockState.Docked;
        }
    }
}
