//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Input
{
    /// <summary>
    /// Preview key down event arguments</summary>
    public class PreviewKeyDownEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor with specified key</summary>
        /// <param name="keyData">One of System.Windows.Forms.Keys values</param>
        public PreviewKeyDownEventArgs(Keys keyData)
        {
            KeyData = keyData;
        }

        /// <summary>
        /// Gets whether Alt key pressed</summary>
        public bool Alt
        {
            get { return (KeyData & Keys.Alt) == Keys.Alt; }
        }

        /// <summary>
        /// Gets whether Control key pressed</summary>
        public bool Control
        {
            get { return (KeyData & Keys.Control) == Keys.Control; }
        }

        /// <summary>
        /// Gets whether Shift key pressed</summary>
        public bool Shift
        {
            get { return (KeyData & Keys.Shift) == Keys.Shift; }
        }

        /// <summary>
        /// Gets keyboard code for System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp event</summary>
        public Keys KeyCode
        {
            get
            {
                var keyGenerated = KeyData & Keys.KeyCode;
                return !Enum.IsDefined(typeof(Keys), (int)keyGenerated) ? Keys.None : keyGenerated;
            }
        }

        /// <summary>
        /// Gets keyboard value for a System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp event</summary>
        public int KeyValue
        {
            get { return (int)(KeyData & Keys.KeyCode); }
        }

        /// <summary>
        /// Gets modifier flags for a System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp event</summary>
        public Keys Modifiers
        {
            get { return KeyData & Keys.Modifiers; }
        }

        /// <summary>
        /// key data for a System.Windows.Forms.Control.KeyDown or System.Windows.Forms.Control.KeyUp event</summary>
        public readonly Keys KeyData;
        /// <summary>
        /// Whether regular input key</summary>
        public bool IsInputKey;
    }
}
