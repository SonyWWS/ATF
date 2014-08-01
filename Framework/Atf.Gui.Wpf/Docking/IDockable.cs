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
        /// Content property, content that is to be docked</summary>
        internal IDockContent Content { get; private set; }

        /// <summary>
        /// Original mouse event arguments, with cursor position (it is necessary to calculate position in 
        /// dock panel.</summary>
        internal MouseEventArgs MouseEventArgs { get; private set; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="content">Content that is to be docked</param>
        /// <param name="mouseArgs">Mouse event args to be used to determine position</param>
        internal DockDragDropEventArgs(IDockContent content, MouseEventArgs mouseArgs)
        {
            Content = content;
            MouseEventArgs = mouseArgs;
        }
    }
    /// <summary>
    /// IDockable interface, used by every class that can accept and preview dock/drops</summary>
    interface IDockable
    {
        void DockDragEnter(object sender, DockDragDropEventArgs e);
        void DockDragOver(object sender, DockDragDropEventArgs e);
        void DockDragLeave(object sender, DockDragDropEventArgs e);
        void DockDrop(object sender, DockDragDropEventArgs e);
        DockTo? DockPreview { get; }
    }
}
