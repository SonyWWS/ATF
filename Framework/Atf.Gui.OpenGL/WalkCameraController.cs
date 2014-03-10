//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Walk camera controller, which uses WASD keys to move, and mouse to rotate. This
    /// controller doesn't change the camera eye position's y-coordinate.</summary>
    public class WalkCameraController : CameraController
    {
        /// <summary>
        /// Handles key-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>True if controller handled the event</returns>
        public override bool KeyDown(object sender, KeyEventArgs e)
        {
            m_keyMap[e.KeyValue] = true;

            // W A S D for forward, strafe left, backward, strafe right, is the default
            Vec3F dir = new Vec3F();
            if (m_keyMap[(int)CanvasControl3D.ControlScheme.Left1] ||
                m_keyMap[(int)CanvasControl3D.ControlScheme.Left2])
                dir = dir - Camera.Right;
            if (m_keyMap[(int)CanvasControl3D.ControlScheme.Right1] ||
                m_keyMap[(int)CanvasControl3D.ControlScheme.Right2]) 
                dir = dir + Camera.Right;
            if (m_keyMap[(int)CanvasControl3D.ControlScheme.Forward1] ||
                m_keyMap[(int)CanvasControl3D.ControlScheme.Forward2])
                dir = dir + Camera.LookAt;
            if (m_keyMap[(int)CanvasControl3D.ControlScheme.Back1] ||
                m_keyMap[(int)CanvasControl3D.ControlScheme.Back2])
                dir = dir - Camera.LookAt;

            bool handled = CanvasControl3D.ControlScheme.IsControllingCamera(Control.ModifierKeys, e);

            if (handled)
            {
                dir.Normalize();
                Vec3F p = Camera.Eye;
                float y = p.Y;
                p += dir * m_scale;
                p.Y = y;
                Camera.Set(p);
            }

            return handled;
        }

        /// <summary>
        /// Clears keymap entry corresponding to the key up code</summary>
        public override bool KeyUp(object sender, KeyEventArgs e)
        {
            m_keyMap[e.KeyValue] = false;
            return true;
        }

        /// <summary>
        /// Handles mouse wheel events. By the standard of Microsoft
        /// and AutoCAD and others, scrolling the mouse wheel away from the user zooms in
        /// and scrolling the mouse wheel towards the user zooms out.</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>True if controller handled the event</returns>
        public override bool MouseWheel(object sender, MouseEventArgs e)
        {
            m_scale += (e.Delta > 0) ? 0.1f : -0.1f;
            if (m_scale <= 0.1f)
                m_scale = 0.01f;

            return true;
        }

        /// <summary>
        /// Handles mouse-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>True if controller handled the event</returns>
        public override bool MouseDown(object sender, MouseEventArgs e)
        {
            if (CanvasControl3D.ControlScheme.IsControllingCamera(Control.ModifierKeys, e))
            {
                m_lastMousePoint = e.Location;
                m_dragging = true;
                return true;
            }

            return base.MouseDown(sender, e);
        }

        /// <summary>
        /// Handles mouse-move events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>True if controller handled the event</returns>
        public override bool MouseMove(object sender, MouseEventArgs e)
        {
            if (m_dragging &&
                CanvasControl3D.ControlScheme.IsControllingCamera(Control.ModifierKeys, e))
            {
                float dx = (float)(e.X - m_lastMousePoint.X) / 150.0f;
                float dy = (float)(e.Y - m_lastMousePoint.Y) / 150.0f;

                if (CanvasControl3D.ControlScheme.IsElevating(Control.ModifierKeys, e))
                {
                    // move camera up/down
                    Vec3F p = Camera.Eye;
                    p.Y += (dy < 0) ? m_scale : -m_scale;
                    Camera.Set(p);
                }
                else if (CanvasControl3D.ControlScheme.IsTurning(Control.ModifierKeys, e))
                {
                    // pitch and yaw camera
                    Matrix4F mat = Matrix4F.RotAxisRH(Camera.Right, -dy); // pitch along camera right
                    Matrix4F yaw = new Matrix4F();
                    yaw.RotY(-dx);
                    mat.Mul(yaw, mat);

                    Vec3F lookAt = Camera.LookAt;
                    Vec3F up = Camera.Up;
                    mat.Transform(ref lookAt);
                    mat.Transform(ref up);

                    Vec3F position = Camera.Eye;
                    float d = Camera.DistanceFromLookAt;
                    Camera.Set(position, position + lookAt * d, up);
                }

                m_lastMousePoint = e.Location;

                return true;
            }

            return base.MouseMove(sender, e);
        }

        /// <summary>
        /// Handles mouse-up events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>True if controller handled the event</returns>
        public override bool MouseUp(object sender, MouseEventArgs e)
        {
            if (m_dragging)
            {
                m_dragging = false;
                return true;
            }
            return base.MouseUp(sender, e);
        }

        /// <summary>
        /// Gets whether the controller handles the WASD keys</summary>
        public override bool HandlesWASD
        {
            get { return true; }
        }

        /// <summary>
        /// Gets whether this camera can handle the given camera</summary>
        /// <param name="camera">Camera</param>
        /// <returns>True iff this camera can handle the given camera</returns>
        public override bool CanHandleCamera(Camera camera)
        {
            return camera.ProjectionType != ProjectionType.Orthographic;
        }

        /// <summary>
        /// Performs any camera initialization required by the controller</summary>
        /// <param name="camera">Camera</param>
        protected override void Setup(Camera camera)
        {
            camera.PerspectiveNearZ = 0.01f;
        }

        private float m_scale = 0.5f;
        private Point m_lastMousePoint = Point.Empty;
        private readonly bool[] m_keyMap = new bool[256];
        private bool m_dragging;
    }
}
