//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Arcball camera controller, with Maya style zooming and panning</summary>
    public class ArcBallCameraController : CameraController
    {
        /// <summary>
        /// Performs any camera initialization required by the controller</summary>
        /// <param name="camera">Camera</param>
        protected override void Setup(Camera camera)
        {
            camera.PerspectiveNearZ = 0.01f;
        }

        /// <summary>
        /// Handles mouse-down events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public override bool MouseDown(object sender, MouseEventArgs e)
        {
            if (CanvasControl3D.ControlScheme.IsControllingCamera(Control.ModifierKeys, e))
            {
                m_lastMousePoint = e.Location;
                m_dragging = true;

                Control c = sender as Control;
                m_width = c.Width;
                m_height = c.Height;

                // get initial rotation
                m_firstPoint = ProjectToArcball(m_lastMousePoint);

                return true;
            }

            return base.MouseDown(sender, e);
        }

        /// <summary>
        /// Handles mouse-move events</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns><c>True</c> if controller handled the event</returns>
        public override bool MouseMove(object sender, MouseEventArgs e)
        {
            if (m_dragging &&
                CanvasControl3D.ControlScheme.IsControllingCamera(Control.ModifierKeys, e))
            {
                Control c = sender as Control;
                float dx = (e.X - m_lastMousePoint.X) * (4.0f / c.Width);
                float dy = (e.Y - m_lastMousePoint.Y) * (4.0f / c.Height);

                if (CanvasControl3D.ControlScheme.IsRotating(Control.ModifierKeys, e) &&
                    (Camera.ViewType == ViewTypes.Perspective || LockOrthographic == false))
                {
                    Track(new Point(e.X, e.Y));

                    if (Camera.ViewType != ViewTypes.Perspective)
                        Camera.ViewType = ViewTypes.Perspective;
                    ControllerToCamera();
                }
                else if (CanvasControl3D.ControlScheme.IsZooming(Control.ModifierKeys, e))
                {
                    float zoom = (-dy - dx);
                    const float ZoomScale = 0.7f; // fudge factor to get the right amount of zoom
                    m_distanceFromLookAt += zoom * CalculateZoomScale() * ZoomScale;
                    ControllerToCamera();
                }
                else if (CanvasControl3D.ControlScheme.IsPanning(Control.ModifierKeys, e))
                {
                    float s = Math.Abs(m_distanceFromLookAt) * 0.25f;
                    if (s > 0)
                    {
                        m_lookAtPoint += Camera.Up * dy * s;
                        m_lookAtPoint += Camera.Right * -dx * s * Camera.Aspect;
                    }
                    else
                    {
                        m_lookAtPoint -= Camera.Up * dy * s;
                        m_lookAtPoint -= Camera.Right * -dx * s * Camera.Aspect;
                    }

                    ControllerToCamera();
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
        /// <returns><c>True</c> if controller handled the event</returns>
        public override bool MouseUp(object sender, MouseEventArgs e)
        {
            if (m_dragging)
            {
                m_dragging = false;
                m_rotation = QuatF.Normalize(m_rotation * m_currentRotation);
                m_currentRotation = QuatF.Identity;

                return true;
            }
            return base.MouseUp(sender, e);
        }

        /// <summary>
        /// Handles mouse wheel events for zooming in and out. By the standard of Microsoft
        /// and AutoCAD and others, scrolling the mouse wheel away from the user zooms in
        /// and scrolling the mouse wheel towards the user zooms out.</summary>
        /// <param name="sender">Control that raised original event</param>
        /// <param name="e">Event args</param>
        /// <returns>True</returns>
        public override bool MouseWheel(object sender, MouseEventArgs e)
        {
            if (!CanvasControl3D.ControlScheme.IsZooming(Control.ModifierKeys, e))
                return true;

            // on a Logitech mouse, scrolling back by one "notch" made e.Delta be -120.
            float delta = -e.Delta * CalculateZoomScale() * 0.002f;

            // account for the fact that zooming in starts with a larger look-at distance
            if (e.Delta > 0)
            {
                float origLookAtDist = m_distanceFromLookAt;
                m_distanceFromLookAt += delta;
                delta = -e.Delta * CalculateZoomScale() * 0.002f;
                m_distanceFromLookAt = origLookAtDist;
            }

            //minimum distance to travel in world space with one wheel "notch". If this is too
            // small, zooming can feel too slow as we get close to the look-at-point.
            const float min_wheel_delta = 1.5f;
            if (delta > -min_wheel_delta && delta < min_wheel_delta)
            {
                if (delta < 0.0f)
                    delta = -min_wheel_delta;
                else
                    delta = min_wheel_delta;
            }

            m_distanceFromLookAt += delta;
            ControllerToCamera();
            return true;
        }

        /// <summary>
        /// Synchronizes the controller to the camera's current state</summary>
        /// <param name="camera">Camera</param>
        protected override void CameraToController(Camera camera)
        {
            m_lookAtPoint = Camera.LookAtPoint;
            Vec3F lookAtVector = Camera.Eye - m_lookAtPoint;
            m_distanceFromLookAt = lookAtVector.Length;

            m_dollyThreshold = Camera.FocusRadius * 0.1f;

            m_rotation.Set(Camera.ViewMatrix);

            base.CameraToController(camera);
        }

        /// <summary>
        /// Synchronizes the camera to the controller's current state</summary>
        /// <param name="camera">Camera</param>
        protected override void ControllerToCamera(Camera camera)
        {
            Vec3F lookAt = Camera.LookAt;
            Vec3F up = Camera.Up;
            if (camera.ViewType == ViewTypes.Perspective)
            {
                Camera.PerspectiveNearZ = CalculatePerspectiveNearZ();

                QuatF rotation = m_rotation * m_currentRotation;
                rotation = rotation.Inverse;
                Matrix4F transform = new Matrix4F(rotation);

                lookAt = new Vec3F(0, 0, -1);
                up = new Vec3F(0, 1, 0);
                transform.Transform(ref lookAt);
                transform.Transform(ref up);
            }

            float eyeOffset = m_distanceFromLookAt;
            float lookAtOffset = 0;
            if (m_distanceFromLookAt < m_dollyThreshold) // do we need to start dollying?
            {
                eyeOffset = m_distanceFromLookAt;
                lookAtOffset = m_distanceFromLookAt - m_dollyThreshold;
            }

            Camera.Set(
                m_lookAtPoint - (eyeOffset * lookAt),       // eye
                m_lookAtPoint - (lookAtOffset * lookAt),    // lookAt
                up);                                        // up

            base.ControllerToCamera(camera);
        }

        private Vec3F ProjectToArcball(Point point)
        {
            float x = (float)point.X / (m_width / 2);    // Scale so bounds map to [0,0] - [2,2]
            float y = (float)point.Y / (m_height / 2);

            x = x - 1;                           // Translate 0,0 to the center
            y = 1 - y;                           // Flip so +Y is up
            if (x < -1)
                x = -1;
            else if (x > 1)
                x = 1;
            if (y < -1)
                y = -1;
            else if (y > 1)
                y = 1;

            float z2 = 1 - x * x - y * y;       // z^2 = 1 - x^2 - y^2
            float z = z2 > 0 ? (float)Math.Sqrt(z2) : 0;

            Vec3F p = new Vec3F(x, y, z);
            p.Normalize();

            return p;
        }

        private void Track(Point current)
        {
            if (current.X < 0 || current.X > m_width)
                return;

            if (current.Y < 0 || current.Y > m_height)
                return;

            Vec3F currentPoint = ProjectToArcball(current);
            Vec3F i = Vec3F.Cross(currentPoint, m_firstPoint);
            float r = Vec3F.Dot(m_firstPoint, currentPoint);
            m_currentRotation = new QuatF(i.X, i.Y, i.Z, r);
        }

        private float CalculatePerspectiveNearZ()
        {
            // make nearZ smaller for doing close-up work. but don't let it get large.
            float nearZ = Math.Abs(m_distanceFromLookAt) * 0.1f;
            nearZ = Math.Max(nearZ, 0.001f);
            nearZ = Math.Min(nearZ, Camera.FarZ * 0.001f);
            return nearZ;
        }

        /// <summary>
        /// Calculates an appropriate zoom scale factor based on distance from the look-at point.
        /// It's important that it allows the camera to zoom through objects, so the m_distanceFromLookAt field
        /// may be negative. Calling this for every event, rather than calculating it only on mouse-down
        /// events, allows for the zooming effort to be consistent, regardless of the original zoom level.</summary>
        private float CalculateZoomScale()
        {
            //Console.WriteLine("look at:" + m_distanceFromLookAt + ", threshold:" + m_dollyThreshold);
            return Math.Max(Math.Abs(m_distanceFromLookAt), m_dollyThreshold);
        }

        private float m_distanceFromLookAt = 16.0f; // distance from camera to lookAt point
        private Vec3F m_firstPoint;
        private Vec3F m_lookAtPoint;
        private QuatF m_rotation = QuatF.Identity;
        private QuatF m_currentRotation = QuatF.Identity;

        private int m_width;
        private int m_height;
        private float m_dollyThreshold;
        private Point m_lastMousePoint;
        private bool m_dragging;
    }
}
