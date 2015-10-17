//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Input;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Implements a camera control scheme, mapping certain mouse and keyboard events
    /// to camera commands, similar to Maya camera controls</summary>
    public class MayaControlScheme : ControlScheme
    {
        /// <summary>
        /// Should the camera rotate around the look-at point?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to rotate the camera</returns>
        public override bool IsRotating(Keys modifierKeys, MouseEventArgs e)
        {
            return (e.Button == MouseButtons.Left && (modifierKeys & Keys.Alt) == Keys.Alt);
        }

        /// <summary>
        /// Should the camera zoom in some way?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to zoom the camera</returns>
        public override bool IsZooming(Keys modifierKeys, MouseEventArgs e)
        {
            return
                (e.Button == MouseButtons.Right && (modifierKeys & Keys.Alt) == Keys.Alt) ||
                e.Delta != 0; //mouse wheel moved?
        }

        /// <summary>
        /// Should the camera do a panning (strafing) motion?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to pan the camera</returns>
        public override bool IsPanning(Keys modifierKeys, MouseEventArgs e)
        {
            return e.Button == MouseButtons.Middle;
        }

        /// <summary>
        /// Should the camera turn around in-place? This means that the look-at direction
        /// changes, but the eye point remains stationary.</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to turn the camera around in place</returns>
        public override bool IsTurning(Keys modifierKeys, MouseEventArgs e)
        {
            return e.Button == MouseButtons.Middle && (modifierKeys & Keys.Alt) == Keys.None;
        }

        /// <summary>
        /// Should the camera move up or down?</summary>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's MouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to move the camera up or down</returns>
        /// <remarks>This is used by the fly and walk camera controllers.</remarks>
        public override bool IsElevating(Keys modifierKeys, MouseEventArgs e)
        {
            return e.Button == MouseButtons.Middle && (modifierKeys & Keys.Alt) == Keys.Alt;
        }
    }
}
