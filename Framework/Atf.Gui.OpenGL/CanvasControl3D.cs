//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.OpenGL
{
    /// <summary>
    /// 3D OpenGL canvas control</summary>
    public class CanvasControl3D : Panel3D
    {
        /// <summary>
        /// Constructor</summary>
        public CanvasControl3D()
            : this((float)(Math.PI / 4), 1.0f, 2048.0f)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="yFOV">Y field of view, in radians</param>
        /// <param name="nearZ">Near z plane constant</param>
        /// <param name="farZ">Far z plane constant</param>
        public CanvasControl3D(float yFOV, float nearZ, float farZ)
        {
            m_camera = new Camera();
            m_camera.SetPerspective(yFOV, 1.0f, nearZ, farZ);

            m_cameraController = new TrackBallCameraController();
            m_cameraController.Camera = m_camera;

            m_camera.CameraChanged += new EventHandler(CameraChanged);
            Sphere3F sphere = new Sphere3F(new Vec3F(0, 0, 0), 25.0f);
            m_camera.ZoomOnSphere(sphere);
        }

        /// <summary>
        /// Gets the camera object. There is only one camera object.</summary>
        public Camera Camera
        {
             get{return m_camera;}
        }

        /// <summary>
        /// Gets or sets current camera controller</summary>
        public CameraController CameraController
        {
            get { return m_cameraController; }
            set
            {
                CameraController camCtrl = value as CameraController;
                if (camCtrl == null)
                    throw new ArgumentException();
                if (m_cameraController != null)
                    m_cameraController.Camera = null;
                m_cameraController = camCtrl;
                m_cameraController.Camera = m_camera;
            }
        }

        /// <summary>
        /// Gets or sets ViewTypes</summary>
        public ViewTypes ViewType
        {
            get { return m_camera.ViewType; }
            set { m_camera.ViewType = value; }
        }

        /// <summary>
        /// Gets and sets the threshold, in pixels, before mouse movement is
        /// considered a "drag"</summary>
        public int DragThreshold
        {
            get { return m_dragThreshold; }
            set { m_dragThreshold = value; }
        }

        /// <summary>
        /// Gets and sets the tolerance, in pixels, for picking</summary>
        public int PickTolerance
        {
            get { return m_pickTolerance; }
            set { m_pickTolerance = value; }
        }

        /// <summary>
        /// Event that is raised when the transform changes</summary>
        public event EventHandler TransformChanged;

        /// <summary>
        /// Gets whether mouse movement passed the drag threshold in x or y</summary>
        public bool DragOverThreshold
        {
            get { return m_dragOverThreshold; }
        }

        /// <summary>
        /// Gets a value indicating if the user is picking</summary>
        public bool IsPicking
        {
            get { return m_isPicking; }
        }

        /// <summary>
        /// Gets or sets ControlScheme used to map the input devices (mouse, keyboard) to particular camera
        /// commands like zooming, panning, and rotating</summary>
        /// <remarks>The camera controllers that derive from CameraController should use this
        /// ControlScheme property rather than reading keyboard and mouse events directly.</remarks>
        public static ControlScheme ControlScheme
        {
            get { return s_controlScheme; }
            set { s_controlScheme = value; }
        }

        /// <summary>
        /// Raises the TransformChanged event</summary>
        protected virtual void OnTransformChanged()
        {
            if (TransformChanged != null)
                TransformChanged(this, EventArgs.Empty);

            Invalidate();
        }

        /// <summary>
        /// Begins drawing the canvas contents</summary>
        protected override void BeginPaint()
        {
            BeginPaint(false);
        }

        /// <summary>
        /// Begins drawing the canvas contents to perform picking</summary>
        /// <param name="picking">True iff perform picking</param>
        protected virtual void BeginPaint(bool picking)
        {
            base.BeginPaint();
        }

        /// <summary>
        /// Ends drawing the canvas contents</summary>
        protected override void EndPaint()
        {
            if (m_isPicking)
                Util3D.DrawHatchedFrame(Size, FirstMousePoint, CurrentMousePoint, Color.Black);

            base.EndPaint();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Resize"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            m_cameraController.KeyDown(this, e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyUp"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            e.Handled = m_cameraController.KeyUp(this, e);
        }

        /// <summary>
        /// Tests if key is an input key. Catching the arrow keys is important here. If we didn't return true, OnKeyDown and OnKeyUp would not get
        /// called for the arrow keys.</summary>
        /// <param name="keyData">Key</param>
        /// <returns>True iff key is input key for camera motion</returns>
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == KeysInterop.ToWf(s_controlScheme.Left1) ||
                keyData == KeysInterop.ToWf(s_controlScheme.Left2) ||
                keyData == KeysInterop.ToWf(s_controlScheme.Right1) ||
                keyData == KeysInterop.ToWf(s_controlScheme.Right2) ||
                keyData == KeysInterop.ToWf(s_controlScheme.Forward1) ||
                keyData == KeysInterop.ToWf(s_controlScheme.Forward2) ||
                keyData == KeysInterop.ToWf(s_controlScheme.Back1) ||
                keyData == KeysInterop.ToWf(s_controlScheme.Back2))
            {
                return true;
            }

            return base.IsInputKey(keyData);
        }

        /// <summary>
        /// Calling the base ProcessCmdKey allows this key press to be consumed by owning
        /// controls like PropertyView and PropertyGridView and be seen by ControlHostService.
        /// Returning false allows the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// Returning true means that this key press has been consumed by this method and this
        /// event is not passed on to any other methods or controls.</summary>
        /// <param name="msg">Windows message to process</param>
        /// <param name="keyDataWf">Key data</param>
        /// <returns>False to allow the key press to escape to IsInputKey, OnKeyDown, OnKeyUp, etc.
        /// True to consume this key press, so this
        /// event is not passed on to any other methods or controls.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyDataWf)
        {
            var keyData = KeysInterop.ToAtf(keyDataWf);
            if (m_cameraController.HandlesWASD && (keyData == s_controlScheme.Left1 ||
                keyData == s_controlScheme.Left2 ||
                keyData == s_controlScheme.Right1 ||
                keyData == s_controlScheme.Right2 ||
                keyData == s_controlScheme.Forward1 ||
                keyData == s_controlScheme.Forward2 ||
                keyData == s_controlScheme.Back1 ||
                keyData == s_controlScheme.Back2))
            {
                return false;
            }
            return base.ProcessCmdKey(ref msg, KeysInterop.ToWf(keyData));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter"></see> event</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            m_cameraController.MouseWheel(this, e);
            base.OnMouseWheel(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            FirstMousePoint = CurrentMousePoint = new Point(e.X, e.Y);
            m_dragOverThreshold = false;

            bool handled = m_cameraController.MouseDown(this, e);
            m_isPicking = (!handled && e.Button == MouseButtons.Left);
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            CurrentMousePoint = new Point(e.X, e.Y);

            m_cameraController.MouseMove(this, e);
                
            int dx = CurrentMousePoint.X - FirstMousePoint.X;
            int dy = CurrentMousePoint.Y - FirstMousePoint.Y;
            if (!m_dragOverThreshold)
            {
                if (Math.Abs(dx) > m_dragThreshold || Math.Abs(dy) > m_dragThreshold)
                {
                    m_dragOverThreshold = true;
                }
            }
            if (m_isPicking)
            {
                // show selection rectangle
                Invalidate();
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            m_cameraController.MouseUp(this, e);
            if (m_isPicking)
            {
                m_isPicking = false;
                Invalidate(); // hide selection rectangle
            }
            base.OnMouseUp(e);
        }

        /// <summary>
        /// Gets the starting mouse coordinates</summary>
        protected Point FirstMousePoint;

        /// <summary>
        /// The current mouse coordinates</summary>
        protected Point CurrentMousePoint;

        private void CameraChanged(object sender, EventArgs e)
        {
            OnTransformChanged();
        }

        private bool m_dragOverThreshold;
        private int m_dragThreshold = 3;
        private int m_pickTolerance = 3;
        private bool m_isPicking;

        private readonly Camera m_camera;  // only camera object.
        private CameraController m_cameraController;
        private static ControlScheme s_controlScheme = new MayaControlScheme();
    }
}
