//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Event args for drag-selection events</summary>
    public class DragSelectionEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="bounds">Drag region</param>
        /// <param name="modifiers">Key modifiers</param>
        public DragSelectionEventArgs(Rectangle bounds, Keys modifiers)
        {
            Bounds = bounds;
            Modifiers = modifiers;
        }

        /// <summary>
        /// Drag region</summary>
        public readonly Rectangle Bounds;

        /// <summary>
        /// Key modifiers</summary>
        public readonly Keys Modifiers;
    }
}
