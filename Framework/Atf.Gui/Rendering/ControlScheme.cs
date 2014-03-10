//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Input;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Provides a way of mapping the mouse and keyboard keys to a set of commands that
    /// are activated in a context-sensitive manner, such as controlling a camera or
    /// selecting objects in the Design View. This class is useful in cases where
    /// the use of left mouse button, middle mouse button, Shift key, Ctrl key, etc.,
    /// should be customizable for modifying the behavior of various logical commands.</summary>
    public abstract class ControlScheme
    {
        /// <summary>
        /// Should the camera rotate around the look-at point?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns>True if the user wants to rotate the camera</returns>
        public abstract bool IsRotating(Keys modifierKeys, MouseEventArgs e);

        /// <summary>
        /// Should the camera zoom in some way?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns>True if the user wants to zoom the camera</returns>
        public abstract bool IsZooming(Keys modifierKeys, MouseEventArgs e);

        /// <summary>
        /// Should the camera do a panning (strafing) motion?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns>True if the user wants to pan the camera</returns>
        /// <remarks>This is used by the arcball and trackball camera controllers.</remarks>
        public abstract bool IsPanning(Keys modifierKeys, MouseEventArgs e);

        /// <summary>
        /// Should the camera turn around in-place? This means that the look-at direction
        /// changes, but the eye point remains stationary.</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns>True if the user wants to turn the camera around in place</returns>
        /// <remarks>This is used by the fly and walk camera controllers.</remarks>
        public abstract bool IsTurning(Keys modifierKeys, MouseEventArgs e);

        /// <summary>
        /// Should the camera move up or down?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns>True if the user wants to move the camera up or down</returns>
        /// <remarks>This is used by the fly and walk camera controllers.</remarks>
        public abstract bool IsElevating(Keys modifierKeys, MouseEventArgs e);

        /// <summary>
        /// Does the user want to control the camera in some way?
        /// (As opposed to selecting something in the Design View, for example.)</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns>True if the user wants to adjust the camera</returns>
        public virtual bool IsControllingCamera(Keys modifierKeys, MouseEventArgs e)
        {
            return
                IsRotating(modifierKeys, e) ||
                IsZooming(modifierKeys, e) ||
                IsPanning(modifierKeys, e) ||
                IsTurning(modifierKeys, e) ||
                IsElevating(modifierKeys, e);
        }

        /// <summary>
        /// For keyboard input to camera controllers that allow for "strafing" (panning) left and right
        /// and for moving forward and backward in some way, these properties return the keys
        /// that should make the motion.</summary>
        public virtual Keys Left1   { get { return Keys.A; } }
        public virtual Keys Left2   { get { return Keys.Left; } }
        public virtual Keys Right1  { get { return Keys.D; } }
        public virtual Keys Right2  { get { return Keys.Right; } }
        public virtual Keys Forward1{ get { return Keys.W; } }
        public virtual Keys Forward2{ get { return Keys.Up; } }
        public virtual Keys Back1   { get { return Keys.S; } }
        public virtual Keys Back2   { get { return Keys.Down; } }

        /// <summary>
        /// Is the keyboard being used to move the camera? If the camera controller allows
        /// for this kind of control, it can check in the OnKeyDown event handler if
        /// the user is intending to move the camera.</summary>
        /// <param name="modifierKeys">The control's ModifierKeys property</param>
        /// <param name="e">The key event from the KeyDown event handler, for example</param>
        /// <returns>True if the user is trying to move the camera using the keyboard</returns>
        public virtual bool IsControllingCamera(Keys modifierKeys, KeyEventArgs e)
        {
            return IsInputKey(e.KeyCode);
        }

        /// <summary>
        /// Is key an input key for camera motion?</summary>
        /// <param name="key">Key to test</param>
        /// <returns>True iff key is input key for camera motion</returns>
        public virtual bool IsInputKey(Keys key)
        {
            return
                key == Left1 ||
                key == Left2 ||
                key == Right1 ||
                key == Right2 ||
                key == Forward1 ||
                key == Forward2 ||
                key == Back1 ||
                key == Back2;
        }

        /// <summary>
        /// Gets the modifier key or keys used to add picked objects to a selection.
        /// If the object is already in the selection, nothing should happen.</summary>
        public virtual Keys AddSelection { get { return Keys.Shift; } }

        /// <summary>
        /// Gets the modifier key or keys used to toggle picked objects in a selection.
        /// An object that is already in the selection set is removed and an
        /// object that is not in the selection set is added.</summary>
        public virtual Keys ToggleSelection { get { return Keys.Control; } }

        /// <summary>
        /// Gets the modifer key or keys used to remove picked objects from a selection.
        /// If the object is not already in the selection, nothing should happen.
        /// Getting a value of Keys.None means that this command is not support in this control scheme.</summary>
        public virtual Keys RemoveSelection { get { return Keys.None; } }
    }
}
