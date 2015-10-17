//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Rendering.OpenGL;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Maya-style camera controller</summary>
    public class TrackBallCameraController : CameraController
    {
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
                    // elevation
                    m_elevation += dy;
                    if (m_elevation > PI)
                        m_elevation -= TwoPI;
                    else if (m_elevation < -PI)
                        m_elevation += TwoPI;

                    // azimuth
                    m_azimuth -= dx;
                    if (m_azimuth > PI)
                        m_azimuth -= TwoPI;
                    else if (m_azimuth < -PI)
                        m_azimuth += TwoPI;

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
                    // The fudge factor was calculated so that an object that is fitted to view will have
                    // its pivot point stay under the cursor during panning. This was tested using the proxy
                    // cube. 0.25f was a bit too high.
                    float s = CalculateZoomScale() * 0.21f;
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
        /// <returns><c>True</c> if controller handled the event</returns>
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
            m_distanceFromLookAt = Camera.DistanceFromLookAt;
            m_dollyThreshold = Camera.FocusRadius * 0.1f;

            Vec3F lookAtDir = Camera.LookAt;
            Vec3F up = Camera.Up;

            m_elevation = (float)Math.Asin(-lookAtDir.Y);
            if (up.Y < 0)
            {
                if (lookAtDir.Y > 0)
                    m_elevation = -PI - m_elevation;
                else
                    m_elevation = PI - m_elevation;
                m_azimuth = (float)Math.Atan2(lookAtDir.X, lookAtDir.Z);
            }
            else
            {
                m_azimuth = (float)Math.Atan2(-lookAtDir.X, -lookAtDir.Z);
            }

            base.CameraToController(camera);
        }

        /// <summary>
        /// Synchronizes the camera to the controller's current state</summary>
        /// <param name="camera">Camera</param>
        protected override void ControllerToCamera(Camera camera)
        {
            Vec3F lookAt = camera.LookAt;
            Vec3F right = camera.Right;
            Vec3F up = camera.Up;

            if (camera.ViewType == ViewTypes.Perspective)
            {
                Camera.PerspectiveNearZ = CalculatePerspectiveNearZ();

                // override the camera's frame of reference
                float sinPhi = (float)Math.Sin(m_elevation);
                float cosPhi = (float)Math.Cos(m_elevation);
                float sinTheta = (float)Math.Sin(m_azimuth);
                float cosTheta = (float)Math.Cos(m_azimuth);

                lookAt = new Vec3F(-cosPhi * sinTheta, -sinPhi, -cosPhi * cosTheta);
                right = new Vec3F(cosTheta, 0, -sinTheta);
                up = Vec3F.Cross(right, lookAt); // TODO compute from sin/cos values
            }

            float lookAtOffset = 0;
            if (m_distanceFromLookAt < m_dollyThreshold) // do we need to start dollying?
                lookAtOffset = m_distanceFromLookAt - m_dollyThreshold;

            float eyeOffset = m_distanceFromLookAt;

            Camera.Set(
                m_lookAtPoint - (eyeOffset * lookAt),       // eye point
                m_lookAtPoint - (lookAtOffset * lookAt),    // look-at point
                up);                                        // up vector

            base.ControllerToCamera(camera);
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
        /// It's important that it allows the camera to zoom through objects, so m_distanceFromLookAt
        /// may be negative. Calling this for every event, rather than calculating it only on mouse-down
        /// events, allows for the zooming effort to be consistent, regardless of the original zoom level.</summary>
        private float CalculateZoomScale()
        {
            return Math.Max( Math.Abs(m_distanceFromLookAt), m_dollyThreshold);
        }

        private float m_distanceFromLookAt = 16.0f; // distance from camera to lookAt point
        private float m_azimuth;                    // angle of rotation in XZ plane starting from +Z axis (radians)
        private float m_elevation;                  // angle of elevation from XZ plane (radians)
        private Vec3F m_lookAtPoint = new Vec3F();

        private float m_dollyThreshold;
        private Point m_lastMousePoint;
        private bool m_dragging;

        //private Vec3F ProjectToTrackball(Point point)
        //{
        //    float x = point.X / (m_width / 2);    // Scale so bounds map to [0,0] - [2,2]
        //    float y = point.Y / (m_height / 2);

        //    x = x - 1;                           // Translate 0,0 to the center
        //    y = 1 - y;                           // Flip so +Y is up
        //    if (x < -1) x = -1;
        //    else if (x > 1) x = 1;
        //    if (y < -1) y = -1;
        //    else if (y > 1) y = 1;

        //    float z2 = 1 - x * x - y * y;       // z^2 = 1 - x^2 - y^2
        //    float z = z2 > 0 ? (float)Math.Sqrt(z2) : 0;

        //    Vec3F p = new Vec3F(x, y, z);
        //    p.Normalize();

        //    return p;
        //}

        //private void Track(Point p1, Point p2)
        //{
        //    if (p1.X == p2.X && p1.Y == p2.Y)
        //        return;

        //    if (p1.X < 0 || p1.X > m_width)
        //        return;

        //    if (p1.Y < 0 || p1.Y > m_height)
        //        return;

        //    if (p2.X < 0 || p2.X > m_width)
        //        return;

        //    if (p2.Y < 0 || p2.Y > m_height)
        //        return;

        //    Vec3F v1 = ProjectToTrackball(p1);
        //    Vec3F v2 = ProjectToTrackball(p2);
        //    Vec3F axis = Vec3F.Cross(v2, v1);
        //    axis.Normalize();

        //    float angle = (float)Math.Acos(Vec3F.Dot(v1, v2)) * 1.5f;

        //    // you can use either quaternion or matrix multiply
        //    // I found matrix approach to be more accurate.
        //    //Quaternion delta = Quaternion.CreateFromAxisAngle(axis, -angle);
        //    //delta.Normalize();

        //    // Compose the delta with the previous orientation
        //    //prevQ = delta * prevQ;
        //    //prevQ.Normalize();
        //    Matrix4F r = new Matrix4F(new AngleAxisF(-angle, axis));
        //    m_rot = Matrix4F.Multiply(m_rot, r);
        //}

        //public override string ToString()
        //{
        //    return
        //        "Camera controller params:" + Environment.NewLine
        //        + "Look at Point: " + m_lookAtPoint + Environment.NewLine
        //        + "Rho: " + m_distanceFromOrigin + Environment.NewLine
        //        + "phi: " + m_elevation + Environment.NewLine
        //        + "theta: " + m_azimuth + Environment.NewLine + Environment.NewLine;
        //}
    }
}
