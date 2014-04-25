//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Input 
{
    /// <summary>
    /// Enumeration for drag and drop effects</summary>
    [Flags]
    public enum DragDropEffects
    {
        /// <summary>Scrolling drag and drop</summary>
        Scroll = -2147483648,
        /// <summary>All drag and drop effects</summary>
        All = -2147483645,
        /// <summary>No drag and drop effects</summary>
        None = 0,
        /// <summary>Copy drag and drop</summary>
        Copy = 1,
        /// <summary>Move drag and drop</summary>
        Move = 2,
        /// <summary>Link drag and drop</summary>
        Link = 4,
    }

    /// <summary>
    /// Provides data for the System.Windows.Forms.Control.DragDrop, System.Windows.Forms.Control.DragEnter, or 
    /// System.Windows.Forms.Control.DragOver event</summary>
    public class DragEventArgs : EventArgs 
    { 
        /// <summary>
        /// The data associated with this event</summary>
        private readonly object data;

        /// <summary>
        /// The current state of the Shift, Ctrl, and Alt keys</summary>
        private readonly int keyState;

        /// <summary>
        /// The mouse x location</summary>
        private readonly int x; 

        /// <summary>
        /// The mouse y location</summary>
        private readonly int y; 

        /// <summary>
        /// The effect that should be applied to the mouse cursor</summary>
        private readonly DragDropEffects allowedEffect;
        /// <summary>
        /// Initializes a new instance of the System.Windows.Forms.DragEventArgs class</summary>
        private DragDropEffects effect;
 
        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="data">System.Windows.Forms.IDataObject that contains the data associated with this event</param>
        /// <param name="keyState">Current state of the Shift, Ctrl, and Alt keys</param>
        /// <param name="x">X-coordinate of mouse pointer</param>
        /// <param name="y">Y-coordinate of mouse pointer</param>
        /// <param name="allowedEffect">>Drag and drop operations allowed by the originator (or source) of the drag and drop event</param>
        /// <param name="effect">Drag and drop operations allowed by the target of the drag and drop event</param>
        public DragEventArgs(object data, int keyState, int x, int y, DragDropEffects allowedEffect, DragDropEffects effect) { 
            this.data = data;
            this.keyState = keyState; 
            this.x = x; 
            this.y = y;
            this.allowedEffect = allowedEffect; 
            this.effect = effect;
        }


        /// <summary> 
        /// Gets the System.Windows.Forms.IDataObject that contains the data associated with this event</summary> 
        public object Data {
            get {
                return data; 
            }
        } 

        /// <summary> 
        /// Gets the current state of the Shift, Ctrl, and Alt keys</summary> 
        public int KeyState { 
            get { 
                return keyState;
            } 
        }

        /// <summary> 
        ///  Gets the x-coordinate of the mouse pointer</summary>
        public int X {
            get {
                return x;
            } 
        }

        /// <summary>
        /// Gets the y-coordinate of the mouse pointer</summary>
        public int Y {
            get { 
                return y; 
            }
        } 

        /// <summary>
        /// Gets which drag-and-drop operations are allowed by the originator (or source) of the drag event</summary> 
        public DragDropEffects AllowedEffect {
            get { 
                return allowedEffect;
            }
        }

        /// <summary> 
        /// Gets or sets which drag and drop operations are allowed by the target of the drag event</summary>
        public DragDropEffects Effect {
            get {
                return effect;
            } 
            set {
                effect = value; 
            } 
        }
    } 
}
