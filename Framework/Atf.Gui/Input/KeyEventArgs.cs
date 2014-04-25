//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Input
{
    /// <summary>
    /// Key event arguments</summary>
    public class KeyEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="keyData">A Sce.Atf.Keys value representing the key that was pressed, combined
        ///  with any modifier flags that indicate which Shift, Ctrl, and Alt keys were
        ///  pressed at the same time. Possible values are obtained be applying the bitwise
        ///  OR (|) operator to constants from the System.Windows.Forms.Keys enumeration.</param>
        public KeyEventArgs(Keys keyData) 
        {
            KeyData = keyData;
        }

        /// <summary>
        /// Gets whether the Alt key was pressed</summary>
        public virtual bool Alt { get { return ((KeyData & Keys.Alt) != 0); } }

        /// <summary>
        /// Gets whether the Ctrl key was pressed</summary>
        public virtual bool Control { get { return ((KeyData & Keys.Control) != 0); } }

        /// <summary>
        /// Gets or sets a value indicating whether the event was handled</summary>
        /// <remarks>Value is true to bypass the control's default handling; otherwise, is false to also pass
        /// the event along to the default control handler.</remarks>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the System.Windows.Forms.Keys keyboard code for a System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp event</summary>
        public Keys KeyCode { get { return KeyData & Keys.KeyCode; } }

        /// <summary>
        /// Gets the key data for a System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp event</summary>
        /// <remarks>The value is a System.Windows.Forms.Keys representing the key code for the key that was
        /// pressed, combined with modifier flags that indicate which combination of
        /// Shift, Ctrl, and Alt keys was pressed at the same time.</remarks>
        public Keys KeyData { get; private set; }

        /// <summary>
        /// Gets the keyboard value for a System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp event</summary>
        public int KeyValue
        {
            get
            {
                return (int)(KeyData & Keys.KeyCode);
            }
        }

        /// <summary>
        /// Gets or sets the modifier flags for a System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp
        /// event. The flags indicate which combination of Shift, Ctrl, and Alt keys was pressed.</summary>
        /// <remarks>The value is a System.Windows.Forms.Keys value representing one or more modifier flags.</remarks>
        public Keys Modifiers { get; set; }

        /// <summary>
        /// Gets a value indicating whether the Shift key was pressed</summary>
        public virtual bool Shift { get { return ((KeyData & Keys.Shift) != 0); } }

        /// <summary>
        ///  Gets or sets a value indicating whether the key event should be passed on
        ///  to the underlying control</summary>
        public bool SuppressKeyPress { get; set; }
    }
}
