//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Drag and drop arguments when window is dropped to be docked to dockpanel</summary>
    public class DockDragDropEventArgs : EventArgs
    {
        /// <summary>
        /// Get content property, content that is to be docked</summary>
        internal IDockContent Content { get; private set; }

        /// <summary>
        /// Get original mouse event arguments, with cursor position (it is necessary to calculate position in 
        /// dock panel</summary>
        internal MouseEventArgs MouseEventArgs { get; private set; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="content">Content that is to be docked</param>
        /// <param name="mouseArgs">Mouse event arguments to be used to determine position</param>
        internal DockDragDropEventArgs(IDockContent content, MouseEventArgs mouseArgs)
        {
            Content = content;
            MouseEventArgs = mouseArgs;
        }
    }
    /// <summary>
    /// IDockable interface, used by every class that can accept and preview dock drops, that is, 
    /// be dragged and dropped onto a dock</summary>
    interface IDockable
    {
        /// <summary>
        /// Function called when dragged window enters already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void DockDragEnter(object sender, DockDragDropEventArgs e);
        /// <summary>
        /// Function called when dragged window is moved over already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void DockDragOver(object sender, DockDragDropEventArgs e);
        /// <summary>
        /// Function called when dragged window leaves already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void DockDragLeave(object sender, DockDragDropEventArgs e);
        /// <summary>
        /// Function called when dragged window is dropped over already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void DockDrop(object sender, DockDragDropEventArgs e);
        /// <summary>
        /// Get nullable DockTo indicating where dockable window should be dropped relative to window it's dropped onto</summary>
        DockTo? DockPreview { get; }
    }
}
