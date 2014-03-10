using System.Windows.Forms;

using AtfKeys = Sce.Atf.Input.Keys;
using AtfKeyEventArgs = Sce.Atf.Input.KeyEventArgs;
using AtfMouseEventArgs = Sce.Atf.Input.MouseEventArgs;
using AtfMessage = Sce.Atf.Input.Message;
using AtfDragEventArgs = Sce.Atf.Input.DragEventArgs;

using WfKeys = System.Windows.Forms.Keys;
using WfKeyEventArgs = System.Windows.Forms.KeyEventArgs;
using WfMouseEventArgs = System.Windows.Forms.MouseEventArgs;
using WfMessage = System.Windows.Forms.Message;
using WfDragEventArgs = System.Windows.Forms.DragEventArgs;

namespace Sce.Atf
{
    /// <summary>
    /// Supports interoperability for events between Windows and ATF</summary>
    public class InteropControl : Control
    {
        /// <summary>
        /// Calls an ATF class event handler to handle KeyDown event in Windows class</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnKeyDown(WfKeyEventArgs e) { OnKeyDown(KeyEventArgsInterop.ToAtf(e)); }
        /// <summary>
        /// Calls a Window class event handler to handle KeyDown event in ATF class</summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnKeyDown(AtfKeyEventArgs e) { base.OnKeyDown(KeyEventArgsInterop.ToWf(e)); }

        /// <summary>
        /// Calls an ATF class event handler to handle KeyUp event in Windows class</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnKeyUp(WfKeyEventArgs e) { OnKeyUp(KeyEventArgsInterop.ToAtf(e)); }
        /// <summary>
        /// Calls a Window class event handler to handle KeyUp event in ATF class</summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnKeyUp(AtfKeyEventArgs e) { base.OnKeyUp(KeyEventArgsInterop.ToWf(e)); }

        /// <summary>
        /// Tests if Windows key is an input key by calling ATF IsInputKey with converted argument</summary>
        /// <param name="keyData">Windows key data</param>
        /// <returns>True iff key is an input key</returns>
        protected override bool IsInputKey(WfKeys keyData) { return IsInputKey(KeysInterop.ToAtf(keyData)); }
        /// <summary>
        /// Tests if ATF key is an input key by calling Windows IsInputKey with converted argument</summary>
        /// <param name="keyData">ATF key data</param>
        /// <returns>True iff key is an input key</returns>
        protected virtual bool IsInputKey(AtfKeys keyData) { return base.IsInputKey(KeysInterop.ToWf(keyData)); }

        /// <summary>
        /// Process Windows message and command key by calling ATF ProcessCmdKey with converted arguments.
        /// Returning false allows the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// Returning true means that this key press has been consumed by this method and this
        /// event is not passed on to any other methods or controls.</summary>
        /// <param name="msg">Windows message to process</param>
        /// <param name="keyData">Windows key data</param>
        /// <returns>False to allow the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// True to consume this key press, so this
        /// event is not passed on to any other methods or controls.</returns>
        protected override bool ProcessCmdKey(ref WfMessage msg, WfKeys keyData) 
        {
            AtfMessage atfMsg = MessageInterop.ToAtf(msg);
            return ProcessCmdKey(ref atfMsg, KeysInterop.ToAtf(keyData)); 
        }
        /// <summary>
        /// Process ATF message and command key by calling the base ProcessCmdKey with converted arguments, 
        /// which allows this key press to be consumed by owning
        /// controls like PropertyView and PropertyGridView and be seen by ControlHostService.
        /// Returning false allows the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// Returning true means that this key press has been consumed by this method and this
        /// event is not passed on to any other methods or controls.</summary>
        /// <param name="msg">ATF message to process</param>
        /// <param name="keyData">ATF key data</param>
        /// <returns>False to allow the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// True to consume this key press, so this
        /// event is not passed on to any other methods or controls.</returns>
        protected virtual bool ProcessCmdKey(ref AtfMessage msg, AtfKeys keyData) 
        {
            WfMessage wfMsg = MessageInterop.ToWf(msg);
            return base.ProcessCmdKey(ref wfMsg, KeysInterop.ToWf(keyData)); 
        }

        /// <summary>
        /// Calls an ATF class event handler to handle MouseWheel event in Windows class</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseWheel(WfMouseEventArgs e) { OnMouseWheel(MouseEventArgsInterop.ToAtf(e)); }
        /// <summary>
        /// Calls a Window class event handler to handle MouseWheel event in ATF class</summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnMouseWheel(AtfMouseEventArgs e) { base.OnMouseWheel(MouseEventArgsInterop.ToWf(e)); }

        /// <summary>
        /// Calls an ATF class event handler to handle MouseDown event in Windows class</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseDown(WfMouseEventArgs e) { OnMouseDown(MouseEventArgsInterop.ToAtf(e)); }
        /// <summary>
        /// Calls a Window class event handler to handle MouseDown event in ATF class</summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnMouseDown(AtfMouseEventArgs e) { base.OnMouseDown(MouseEventArgsInterop.ToWf(e)); }

        /// <summary>
        /// Calls an ATF class event handler to handle MouseMove event in Windows class</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseMove(WfMouseEventArgs e) { OnMouseMove(MouseEventArgsInterop.ToAtf(e)); }
        /// <summary>
        /// Calls a Window class event handler to handle MouseMove event in ATF class</summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnMouseMove(AtfMouseEventArgs e) { base.OnMouseMove(MouseEventArgsInterop.ToWf(e)); }

        /// <summary>
        /// Calls an ATF class event handler to handle MouseUp event in Windows class</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnMouseUp(WfMouseEventArgs e) { OnMouseUp(MouseEventArgsInterop.ToAtf(e)); }
        /// <summary>
        /// Calls a Window class event handler to handle MouseUp event in ATF class</summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnMouseUp(AtfMouseEventArgs e) { base.OnMouseUp(MouseEventArgsInterop.ToWf(e)); }

        /// <summary>
        /// Calls an ATF class event handler to handle DragEnter event in Windows class</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnDragEnter(WfDragEventArgs e) { OnDragEnter(DragEventArgsInterop.ToAtf(e)); }
        /// <summary>
        /// Calls a Window class event handler to handle DragEnter event in ATF class</summary>
        /// <param name="e">Event arguments</param>
        protected virtual void OnDragEnter(AtfDragEventArgs e) { base.OnDragEnter(DragEventArgsInterop.ToWf(e)); }
    }
}

