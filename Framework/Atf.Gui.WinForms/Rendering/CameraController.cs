//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Base class for camera controllers</summary>
    public abstract class CameraController : Controller, IDisposable
    {
        /// <summary>
        /// Detaches from camera's event</summary>
        public void Dispose()
        {
            Camera = null;
        }

        /// <summary>
        /// Gets and sets the camera</summary>
        public Camera Camera
        {
            get { return m_camera; }
            set
            {
                if (m_camera != null)
                    m_camera.CameraChanged -= CameraChanged;

                m_camera = value;

                if (m_camera != null)
                {
                    Setup(m_camera);
                    CameraToController(m_camera);
                    ControllerToCamera(m_camera); // this removes transforms from the camera which the controller can't handle
                    m_camera.CameraChanged += CameraChanged;
                }
            }
        }

        /// <summary>
        /// Gets whether the controller handles the WASD keys</summary>
        public virtual bool HandlesWASD
        {
            get { return false; }
        }

        /// <summary>
        /// Gets whether this camera can handle the given camera</summary>
        /// <param name="camera">Camera</param>
        /// <returns><c>True</c> if this camera can handle the given camera</returns>
        public virtual bool CanHandleCamera(Camera camera)
        {
            return true;
        }

        /// <summary>
        /// Gets or sets whether or not the orthographic views (top, left, side) are locked against
        /// rotation. If true and the user attempts to rotate the camera, nothing happens.
        /// If false and the user rotates the camera, the projection mode changes
        /// automatically to perspective view.</summary>
        public static bool LockOrthographic
        {
            get { return s_lockOrthographic; }
            set { s_lockOrthographic = value; }
        }

        /// <summary>
        /// Synchronizes the camera to the controller</summary>
        protected void ControllerToCamera()
        {
            try
            {
                m_updating = true;
                ControllerToCamera(m_camera);
            }
            finally
            {
                m_updating = false;
            }
        }

        /// <summary>
        /// Performs any camera initialization required by the controller</summary>
        /// <param name="camera">Camera</param>
        protected virtual void Setup(Camera camera)
        {
        }

        /// <summary>
        /// Synchronizes the controller to the camera's current state</summary>
        /// <param name="camera">Camera</param>
        /// <remarks>Both CameraToController and ControllerToCamera should be
        /// overridden for controllers that shadow camera state</remarks>
        protected virtual void CameraToController(Camera camera)
        {
        }

        /// <summary>
        /// Synchronizes the camera to the controller's current state</summary>
        /// <param name="camera">Camera</param>
        /// <remarks>Both CameraToController and ControllerToCamera should be
        /// overridden for controllers that shadow camera state</remarks>
        protected virtual void ControllerToCamera(Camera camera)
        {
        }

        // handle CameraChanged event
        private void CameraChanged(object sender, EventArgs e)
        {
            if (!m_updating)
                CameraToController(m_camera);
        }

        private Camera m_camera;
        private bool m_updating;

        // Represents a global setting (probably user-defined) for whether or not an orthographic
        //  camera view can be rotated to a perspective camera view.
        private static bool s_lockOrthographic;
    }
}
