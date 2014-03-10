//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

using AtfDragEventArgs = Sce.Atf.Input.DragEventArgs;
using AtfDragDropEffects = Sce.Atf.Input.DragDropEffects;

using WfDragEventArgs = System.Windows.Forms.DragEventArgs;
using WfDragDropEffects = System.Windows.Forms.DragDropEffects;

namespace Sce.Atf
{
    /// <summary>
    /// Converts drag and drop event arguments between ATF and Windows to support interoperability for events between Windows and ATF.
    /// These drag and drop event arguments provide data for the System.Windows.Forms.Control.DragDrop, System.Windows.Forms.Control.DragEnter, or 
    /// System.Windows.Forms.Control.DragOver events.</summary>
    public static class DragEventArgsInterop
    {
        /// <summary>
        /// Converts WfDragEventArgs to AtfDragEventArgs</summary>
        /// <param name="arg">WfDragEventArgs</param>
        /// <returns>AtfDragEventArgs</returns>
        public static AtfDragEventArgs ToAtf(WfDragEventArgs arg)
        {
            return new AtfDragEventArgs(arg.Data, arg.KeyState, arg.X, arg.Y, (AtfDragDropEffects)arg.AllowedEffect, (AtfDragDropEffects)arg.Effect);
        }

        /// <summary>
        /// Converts AtfDragEventArgs to WfDragEventArgs</summary>
        /// <param name="arg">AtfDragEventArgs</param>
        /// <returns>WfDragEventArgs</returns>
        public static WfDragEventArgs ToWf(AtfDragEventArgs arg)
        {
            return new WfDragEventArgs((IDataObject)arg.Data, arg.KeyState, arg.X, arg.Y, (WfDragDropEffects)arg.AllowedEffect, (WfDragDropEffects)arg.Effect);
        }
    }
}
