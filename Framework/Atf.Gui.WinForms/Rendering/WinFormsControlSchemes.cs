//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using WfKeys = System.Windows.Forms.Keys;
using WfMouseEventArgs = System.Windows.Forms.MouseEventArgs;
using WfKeyEventArgs = System.Windows.Forms.KeyEventArgs;

using AtfKeyEventArgs = Sce.Atf.Input.KeyEventArgs;
using AtfMouseEventArgs = Sce.Atf.Input.MouseEventArgs;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Extension methods for class ControlScheme to allow interoperability with WinForms input classes</summary>
    public static class WinFormsControlSchemes
    {
        /// <summary>
        /// Should the camera rotate around the look-at point?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's AtfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to rotate the camera</returns>
        public static bool IsRotating(this ControlScheme controlScheme, WfKeys modifierKeys, AtfMouseEventArgs e)
        {
            return controlScheme.IsRotating(KeysInterop.ToAtf(modifierKeys), e);
        }

        /// <summary>
        /// Should the camera rotate around the look-at point?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's WfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to rotate the camera</returns>
        public static bool IsRotating(this ControlScheme controlScheme, WfKeys modifierKeys, WfMouseEventArgs e)
        {
            return controlScheme.IsRotating(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
        }

        /// <summary>
        /// Should the camera zoom in some way?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's AtfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to zoom the camera</returns>
        public static bool IsZooming(this ControlScheme controlScheme, WfKeys modifierKeys, AtfMouseEventArgs e)
        {
            return controlScheme.IsZooming(KeysInterop.ToAtf(modifierKeys), e);
        }

        /// <summary>
        /// Should the camera zoom in some way?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's WfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to zoom the camera</returns>
        public static bool IsZooming(this ControlScheme controlScheme, WfKeys modifierKeys, WfMouseEventArgs e)
        {
            return controlScheme.IsZooming(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
        }

        /// <summary>
        /// Should the camera do a panning (strafing) motion?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's AtfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to pan the camera</returns>
        /// <remarks>This is used by the arcball and trackball camera controllers.</remarks>
        public static bool IsPanning(this ControlScheme controlScheme, WfKeys modifierKeys, AtfMouseEventArgs e)
        {
            return controlScheme.IsPanning(KeysInterop.ToAtf(modifierKeys), e);
        }

        /// <summary>
        /// Should the camera do a panning (strafing) motion?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's WfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to pan the camera</returns>
        /// <remarks>This is used by the arcball and trackball camera controllers.</remarks>
        public static bool IsPanning(this ControlScheme controlScheme, WfKeys modifierKeys, WfMouseEventArgs e)
        {
            return controlScheme.IsPanning(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
        }

        /// <summary>
        /// Should the camera turn around in-place? This means that the look-at direction
        /// changes, but the eye point remains stationary.</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's AtfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to turn the camera around in place</returns>
        /// <remarks>This is used by the fly and walk camera controllers.</remarks>
        public static bool IsTurning(this ControlScheme controlScheme, WfKeys modifierKeys, AtfMouseEventArgs e)
        {
            return controlScheme.IsTurning(KeysInterop.ToAtf(modifierKeys), e);
        }

        /// <summary>
        /// Should the camera turn around in-place? This means that the look-at direction
        /// changes, but the eye point remains stationary.</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's WfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to turn the camera around in place</returns>
        /// <remarks>This is used by the fly and walk camera controllers.</remarks>
        public static bool IsTurning(this ControlScheme controlScheme, WfKeys modifierKeys, WfMouseEventArgs e)
        {
            return controlScheme.IsTurning(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
        }

        /// <summary>
        /// Should the camera move up or down?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's AtfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to move the camera up or down</returns>
        /// <remarks>This is used by the fly and walk camera controllers.</remarks>
        public static bool IsElevating(this ControlScheme controlScheme, WfKeys modifierKeys, AtfMouseEventArgs e)
        {
            return controlScheme.IsElevating(KeysInterop.ToAtf(modifierKeys), e);
        }

        /// <summary>
        /// Should the camera move up or down?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's WfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to move the camera up or down</returns>
        /// <remarks>This is used by the fly and walk camera controllers.</remarks>
        public static bool IsElevating(this ControlScheme controlScheme, WfKeys modifierKeys, WfMouseEventArgs e)
        {
            return controlScheme.IsElevating(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
        }

        /// <summary>
        /// Does the user want to control the camera in some way?
        /// (As opposed to selecting something in the Design View, for example.)</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's AtfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to adjust the camera</returns>
        public static bool IsControllingCamera(this ControlScheme controlScheme, WfKeys modifierKeys, AtfMouseEventArgs e)
        {
            return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), e);
        }

        /// <summary>
        /// Does the user want to control the camera in some way?
        /// (As opposed to selecting something in the Design View, for example.)</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The camera control's ModifierKeys property</param>
        /// <param name="e">The camera control's event handler's WfMouseEventArgs</param>
        /// <returns><c>True</c> if the user wants to adjust the camera</returns>
        public static bool IsControllingCamera(this ControlScheme controlScheme, WfKeys modifierKeys, WfMouseEventArgs e)
        {
            return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), MouseEventArgsInterop.ToAtf(e));
        }

        /// <summary>
        /// Is the keyboard being used to move the camera? If the camera controller allows
        /// for this kind of control, then it can check in the OnKeyDown event handler if
        /// the user is intending to move the camera.</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The Control's ModifierKeys property</param>
        /// <param name="e">The key event from the KeyDown event handler, for example</param>
        /// <returns><c>True</c> if the user is trying to move the camera using the keyboard</returns>
        public static bool IsControllingCamera(this ControlScheme controlScheme, WfKeys modifierKeys, AtfKeyEventArgs e)
        {
            return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), e);
        }

        /// <summary>
        /// Is the keyboard being used to move the camera? If the camera controller allows
        /// for this kind of control, then it can check in the OnKeyDown event handler if
        /// the user is intending to move the camera.</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="modifierKeys">The Control's ModifierKeys property</param>
        /// <param name="e">The key event from the KeyDown event handler, for example</param>
        /// <returns><c>True</c> if the user is trying to move the camera using the keyboard</returns>
        public static bool IsControllingCamera(this ControlScheme controlScheme, WfKeys modifierKeys, WfKeyEventArgs e)
        {
            return controlScheme.IsControllingCamera(KeysInterop.ToAtf(modifierKeys), KeyEventArgsInterop.ToAtf(e));
        }

        /// <summary>
        /// Is key an input key for camera motion?</summary>
        /// <param name="controlScheme">The control scheme instance to extend</param>
        /// <param name="key">Key to test</param>
        /// <returns><c>True</c> if key is input key for camera motion</returns>
        public static bool IsInputKey(this ControlScheme controlScheme, WfKeys key)
        {
            return controlScheme.IsInputKey(KeysInterop.ToAtf(key));
        }
    }
}
