//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls.Adaptable
{
    /// <summary>
    /// Event arguments for drag selection event</summary>
    public class DragSelectionEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="region">Drag region</param>
        /// <param name="modifiers">Key modifiers</param>
        public DragSelectionEventArgs(Rect region, ModifierKeys modifiers)
        {
            Region = region;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Drag region</summary>
        public readonly Rect Region;

        /// <summary>
        /// Key modifiers</summary>
        public readonly ModifierKeys Modifiers;
    }
}
