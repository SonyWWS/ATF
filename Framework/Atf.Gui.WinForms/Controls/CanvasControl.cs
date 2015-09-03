//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Control for viewing and editing a 2D bounded canvas</summary>
    public abstract class CanvasControl : Control
    {
        /// <summary>
        /// Constructor</summary>
        public CanvasControl()
        {
            SuspendLayout();

            m_vScrollBar = new VScrollBar();
            m_vScrollBar.Dock = DockStyle.Right;
            m_vScrollBar.ValueChanged += vScrollBar_ValueChanged;

            m_hScrollBar = new HScrollBar();
            m_hScrollBar.Dock = DockStyle.Bottom;
            m_hScrollBar.ValueChanged += hScrollBar_ValueChanged;

            Controls.Add(m_vScrollBar);
            Controls.Add(m_hScrollBar);

            ResumeLayout();

            m_autoScrollTimer = new Timer();
            m_autoScrollTimer.Interval = 10;
            m_autoScrollTimer.Tick += autoScrollTimer_Tick;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.Control"></see> and its child controls and optionally releases the managed resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_autoScrollTimer.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets and sets threshold, in pixels, before drag starts</summary>
        public int DragThreshold
        {
            get { return m_dragThreshold; }
            set { m_dragThreshold = value; }
        }

        /// <summary>
        /// Gets transform matrix for transforming canvas (world) coordinates to Windows
        /// client coordinates</summary>
        public virtual Matrix Transform
        {
            get
            {
                m_transform.Reset();
                m_transform.Translate(m_scroll.X, m_scroll.Y);
                m_transform.Scale(m_xZoom, m_yZoom);
                return m_transform;
            }
        }
        private readonly Matrix m_transform = new Matrix();

        /// <summary>
        /// Gets whether interactive zooming is uniform on x and y axes</summary>
        public bool UniformZoom
        {
            get { return m_uniformZoom; }
        }

        /// <summary>
        /// Gets and sets zoom, for uniform zooming. Updates ScrollPosition.</summary>
        /// <remarks>Should be considered write-only if non-uniform zoom is used</remarks>
        public float Zoom
        {
            get { return m_xZoom; }
            set
            {
                ZoomAboutCenter(value, value);
            }
        }

        /// <summary>
        /// Gets and sets zoom in the x-direction. Updates ScrollPosition.</summary>
        public float XZoom
        {
            get { return m_xZoom; }
            set
            {
                if (m_xZoom != value)
                    ZoomAboutCenter(value, m_yZoom);
            }
        }

        /// <summary>
        /// Gets and sets zoom in the y-direction. Updates ScrollPosition.</summary>
        public float YZoom
        {
            get { return m_yZoom; }
            set
            {
                if (m_yZoom != value)
                    ZoomAboutCenter(m_xZoom, value);
            }
        }

        /// <summary>
        /// Gets a value indicating if mouse movement passed the DragThreshold in x or y</summary>
        public bool DragOverThreshold
        {
            get { return m_isDragging && m_dragOverThreshold; }
        }

        /// <summary>
        /// Gets the first mouse point</summary>
        public Point FirstPoint
        {
            get { return m_firstPoint; }
        }

        /// <summary>
        /// Gets the current mouse point</summary>
        public Point CurrentPoint
        {
            get { return m_currentPoint; }
        }

        /// <summary>
        /// Gets the first mouse point, or current mouse point if drag has passed the
        /// drag threshold</summary>
        public Point DragPoint
        {
            get { return m_dragOverThreshold ? m_currentPoint : m_firstPoint; }
        }

        /// <summary>
        /// Gets the client rectangle minus the area of the scrollbars, if applicable</summary>
        public virtual Rectangle VisibleClientRectangle
        {
            get
            {
                Rectangle rect = ClientRectangle;
                if (m_hScrollBar.Visible)
                {
                    rect.Height -= m_hScrollBar.Height;
                }
                if (m_vScrollBar.Visible)
                {
                    rect.Width -= m_vScrollBar.Width;
                }
                return rect;
            }
        }

        /// <summary>
        /// Gets and sets a value indicating if drag constrain is on</summary>
        public bool Constrain
        {
            get { return m_constrain; }
            set { m_constrain = value; }
        }

        /// <summary>
        /// Gets the delta between current and drag points</summary>
        public Point DragDelta
        {
            get
            {
                Point dragPoint = DragPoint;
                return new Point(dragPoint.X - m_firstPoint.X, dragPoint.Y - m_firstPoint.Y);
            }
        }

        /// <summary>
        /// Event that is raised after the control scrolls</summary>
        public event EventHandler Scrolled;

        /// <summary>
        /// Event that is raised after the control zooms</summary>
        public event EventHandler Zoomed;

        /// <summary>
        /// Sets non-uniform uniform minimum/maximum zoom ranges in x and y directions.
        /// Each zoom value is Windows client space divided by canvas space.</summary>
        /// <param name="minXZoom">Float minimum zoom in x direction</param>
        /// <param name="maxXZoom">Float maximum zoom in x direction</param>
        /// <param name="minYZoom">Float minimum zoom in y direction</param>
        /// <param name="maxYZoom">Float maximum zoom in y direction</param>
        public void SetZoomRange(float minXZoom, float maxXZoom, float minYZoom, float maxYZoom)
        {
            m_minXZoom = minXZoom;
            m_maxXZoom = maxXZoom;
            m_minYZoom = minYZoom;
            m_maxYZoom = maxYZoom;
            m_uniformZoom = false;
            SetZoom(m_xZoom, m_yZoom);
        }

        /// <summary>
        /// Sets uniform zooming minimum/maximum ranges. The zoom value is Windows client
        /// space divided by canvas space.</summary>
        /// <param name="minZoom">Float minimum zoom</param>
        /// <param name="maxZoom">Float maximum zoom</param>
        public void SetZoomRange(float minZoom, float maxZoom)
        {
            m_minXZoom = m_minYZoom= minZoom;
            m_maxXZoom = m_maxYZoom= maxZoom;
            m_uniformZoom = true;
            SetZoom(m_xZoom, m_yZoom);
        }

        /// <summary>
        /// Transforms client point to canvas coordinates</summary>
        /// <param name="x">Point, in client coordinates</param>
        /// <returns>Point, in canvas coordinates</returns>
        public Point ClientToCanvas(Point x)
        {
            return Sce.Atf.GdiUtil.InverseTransform(Transform, x);
        }

        /// <summary>
        /// Transforms client point to canvas coordinates</summary>
        /// <param name="x">Point, in client coordinates</param>
        /// <returns>Point, in canvas coordinates</returns>
        public PointF ClientToCanvas(PointF x)
        {
            return Sce.Atf.GdiUtil.InverseTransform(Transform, x);
        }

        /// <summary>
        /// Transforms client rectangle to canvas coordinates</summary>
        /// <param name="x">Rectangle, in client coordinates</param>
        /// <returns>Rectangle, in canvas coordinates</returns>
        public Rectangle ClientToCanvas(Rectangle x)
        {
            return Sce.Atf.GdiUtil.InverseTransform(Transform, x);
        }

        /// <summary>
        /// Transforms client rectangle to canvas coordinates</summary>
        /// <param name="x">Rectangle, in client coordinates</param>
        /// <returns>Rectangle, in canvas coordinates</returns>
        public RectangleF ClientToCanvas(RectangleF x)
        {
            return Sce.Atf.GdiUtil.InverseTransform(Transform, x);
        }

        /// <summary>
        /// Transforms canvas point to client coordinates</summary>
        /// <param name="x">Point, in canvas coordinates</param>
        /// <returns>Point, in client coordinates</returns>
        public Point CanvasToClient(Point x)
        {
            return Sce.Atf.GdiUtil.Transform(Transform, x);
        }

        /// <summary>
        /// Transforms canvas point to client coordinates</summary>
        /// <param name="x">Point, in canvas coordinates</param>
        /// <returns>Point, in client coordinates</returns>
        public PointF CanvasToClient(PointF x)
        {
            return Sce.Atf.GdiUtil.Transform(Transform, x);
        }

        /// <summary>
        /// Transforms canvas rectangle to client coordinates</summary>
        /// <param name="x">Rectangle, in canvas coordinates</param>
        /// <returns>Rectangle, in client coordinates</returns>
        public Rectangle CanvasToClient(Rectangle x)
        {
            return Sce.Atf.GdiUtil.Transform(Transform, x);
        }

        /// <summary>
        /// Transforms canvas rectangle to client coordinates</summary>
        /// <param name="x">Rectangle, in canvas coordinates</param>
        /// <returns>Rectangle, in client coordinates</returns>
        public RectangleF CanvasToClient(RectangleF x)
        {
            return Sce.Atf.GdiUtil.Transform(Transform, x);
        }

        /// <summary>
        /// Scrolls and zooms so that the given rectangle almost fills the client area,
        /// within the scroll limits set by SetZoomRange</summary>
        /// <param name="bounds">Rectangle to be framed, in Windows client coordinates</param>
        public void Frame(RectangleF bounds)
        {
            Frame(bounds, true);
        }

        /// <summary>
        /// Ensures that the rectangle is visible in the client area</summary>
        /// <param name="bounds">Rectangle, in Windows client coordinates, to be made visible</param>
        public void EnsureVisible(RectangleF bounds)
        {
            Frame(bounds, false);
        }

        // 'bounds' is in Windows client coordinates. This rectangle will be made visible. If
        //  fillClientWindow is true, zoom and pan will be set to maximize the rendered size of
        //  'bounds' even if the rectangle is already currently visible.
        private void Frame(RectangleF bounds, bool fillClientWindow)
        {
            // Check if rectangle is already in the visible rect.
            RectangleF clientRect = VisibleClientRectangle;
            if (fillClientWindow == false && clientRect.Contains(bounds))
            {
                return;
            }

            // Get the center of the Windows client area.
            Point clientCenter = new Point(
                (int)(clientRect.Left + clientRect.Width / 2),
                (int)(clientRect.Top + clientRect.Height / 2));

            // Figure out desired center point on the canvas (world) before adjusting the zoom.
            RectangleF targetCanvasRect = ClientToCanvas(bounds);
            Point canvasCenter = new Point(
                (int)(targetCanvasRect.Left + targetCanvasRect.Width / 2),
                (int)(targetCanvasRect.Top + targetCanvasRect.Height / 2));

            // Check if zoom needs to be adjusted.
            float newXZoom = XZoom;
            if (fillClientWindow || bounds.Width > clientRect.Width)
            {
                newXZoom = Math.Abs(clientRect.Width / targetCanvasRect.Width) * FrameScale;
            }
            float newYZoom = YZoom;
            if (fillClientWindow || bounds.Height > clientRect.Height)
            {
                newYZoom = Math.Abs(clientRect.Height / targetCanvasRect.Height) * FrameScale;
            }
            SetZoom(newXZoom, newYZoom);

            // Update the scrollbars in order to update the min/max valid values for ScrollPosition.
            UpdateScrollBars(m_vScrollBar, m_hScrollBar);

            // Set the translation to go from the canvas (world) center to client window center.
            // CenterScreen = CenterWorld * Scale + (Translation + VisibleScreenOrigin)
            // Translation = CenterScreen - CenterWorld * Scale - VisibleScreenOrigin
            ScrollPosition = new Point(
                (int)(clientCenter.X - canvasCenter.X * m_xZoom - clientRect.Left),
                (int)(clientCenter.Y - canvasCenter.Y * m_yZoom - clientRect.Top));
        }

        /// <summary>
        /// Gets or sets the scroll position, in Windows client coordinates. This is also the translation
        /// from canvas (world) coordinates to Windows client coordinates.</summary>
        public Point ScrollPosition
        {
            get { return m_scroll; }
            set
            {
                int x = -Math.Max(m_hScrollBar.Minimum, Math.Min(m_hScrollBar.Maximum - m_hScrollBar.LargeChange, -value.X));
                int y = -Math.Max(m_vScrollBar.Minimum, Math.Min(m_vScrollBar.Maximum - m_vScrollBar.LargeChange, -value.Y));

                m_scroll = new Point(x, y);
                m_hScrollBar.Value = -x;
                m_vScrollBar.Value = -y;

                OnScroll();

                Invalidate();
            }
        }

        /// <summary>
        /// Sets the zoom factors directly, without setting ScrollPosition.
        /// The zoom factors will be capped to the minimum and maximum values.</summary>
        /// <param name="xZoom">The x zoom.</param>
        /// <param name="yZoom">The y zoom.</param>
        public void SetZoom(float xZoom, float yZoom)
        {
            xZoom = Math.Max(m_minXZoom, xZoom);
            xZoom = Math.Min(m_maxXZoom, xZoom);

            yZoom = Math.Max(m_minYZoom, yZoom);
            yZoom = Math.Min(m_maxYZoom, yZoom);

            if (xZoom == m_xZoom &&
                yZoom == m_yZoom)
            {
                return;
            }

            if (UniformZoom)
            {
                m_xZoom = m_yZoom = xZoom;
            }
            else
            {
                m_xZoom = xZoom;
                m_yZoom = yZoom;
            }

            OnZoom();

            Invalidate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            m_firstPoint = m_currentPoint = m_last = new Point(e.X, e.Y);
            m_autoScrollPositionStart = ScrollPosition;
            m_isDragging = true;

            base.OnMouseDown(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            m_last = m_currentPoint;
            m_currentPoint = new Point(e.X, e.Y);

            int xDelta = Math.Abs(m_currentPoint.X - m_firstPoint.X);
            int yDelta = Math.Abs(m_currentPoint.Y - m_firstPoint.Y);

            if (m_isDragging &&
                !m_dragOverThreshold &&
                (xDelta > m_dragThreshold || yDelta > m_dragThreshold))
            {
                m_dragOverThreshold = true;

                if (m_constrain)
                    m_constrainX = (xDelta < yDelta);
            }

            if (m_constrain)
            {
                if (m_constrainX)
                    m_currentPoint.X = m_firstPoint.X;
                else
                    m_currentPoint.Y = m_firstPoint.Y;
            }

            if (m_dragOverThreshold)
            {
                Point delta = DragDelta;

                // auto scroll if mouse is outside control
                if (m_autoScroll && !VisibleClientRectangle.Contains(m_currentPoint) && !m_autoScrollTimer.Enabled)
                    m_autoScrollTimer.Start();

                if (m_isMultiSelecting)
                {
                    Rectangle rect;

                    rect = MakeSelectionRect(m_last, m_firstPoint);
                    ControlPaint.DrawReversibleFrame(rect, BackColor, FrameStyle.Dashed);

                    rect = MakeSelectionRect(m_currentPoint, m_firstPoint);
                    ControlPaint.DrawReversibleFrame(rect, BackColor, FrameStyle.Dashed);
                }
                else if (m_isScrolling)
                {
                    ScrollPosition = new Point(
                        m_autoScrollPositionStart.X + delta.X,
                        m_autoScrollPositionStart.Y + delta.Y);
                }
                else if (m_isZooming)
                {

                    float xScale = 1 + 2 * delta.X / (float)Width;
                    float yScale = 1 + 2 * delta.Y / (float)Height;

                    if (m_constrain || UniformZoom)
                        xScale = yScale = Math.Max(xScale, yScale);

                    SetZoom(m_xZoomStart * xScale, m_yZoomStart * yScale);

                    ScrollPosition = new Point((int)(m_firstPoint.X - m_zoomCenterStart.X * m_xZoom),
                        (int)(m_firstPoint.Y - m_zoomCenterStart.Y * m_yZoom));
                }
            }

            base.OnMouseMove(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // Let MouseUp event listeners have knowledge of the current action.
            base.OnMouseUp(e);

            // Now the action data can be cleared.
            CancelAction();
        }

        /// <summary>
        /// Raises the <see cref="System.Windows.Forms.Control.OnKeyDown"></see> event.
        /// Catches the Esc key so that certain user interface actions can be cancelled.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
                CancelAction();
            base.OnKeyDown(e);
        }

        /// <summary>
        /// Gets the size of the canvas, in pixels</summary>
        /// <returns>Size of the canvas, in pixels</returns>
        protected abstract Size GetCanvasSize();

        /// <summary>
        /// Returns the current mouse point, constrained so that the drag rectangle is completely on
        /// canvas</summary>
        /// <param name="offsetRect">Rectangle that must remain completely on canvas</param>
        /// <returns>Current point, constrained so that the drag rectangle is completely on canvas</returns>
        protected Point ForceOnCanvas(Rectangle offsetRect)
        {
            Point result = m_currentPoint;
            Rectangle bounds = offsetRect;
            bounds.Offset(new Point(
                m_currentPoint.X - m_scroll.X,
                m_currentPoint.Y - m_scroll.Y));
            if (bounds.Left < 0)
                result.X -= bounds.Left;
            if (bounds.Top < 0)
                result.Y -= bounds.Top;

            return result;
        }

        /// <summary>
        /// Gets the current dragged selection rectangle</summary>
        protected Rectangle SelectionRect
        {
            get { return Sce.Atf.GdiUtil.MakeRectangle(m_currentPoint, m_firstPoint); }
        }

        /// <summary>
        /// Gets and sets whether the user is multi-selecting</summary>
        public bool IsMultiSelecting
        {
            get { return m_isMultiSelecting; }
            set
            {
                if (value)
                {
                    m_isScrolling = m_isZooming = false;
                    Cursor = Cursors.Cross;
                }
                m_isMultiSelecting = value;
            }
        }

        /// <summary>
        /// Gets and sets whether the user is scrolling (panning)</summary>
        public bool IsScrolling
        {
            get { return m_isScrolling; }
            set
            {
                if (value)
                {
                    m_isZooming = m_isMultiSelecting = false;
                    Cursor = Cursors.Hand;
                }
                m_isScrolling = value;
            }
        }

        /// <summary>
        /// Gets and sets whether the user is zooming</summary>
        public bool IsZooming
        {
            get { return m_isZooming; }
            set
            {
                if (value)
                {
                    m_xZoomStart = m_xZoom;
                    m_yZoomStart = m_yZoom;
                    m_zoomCenterStart = new PointF(
                        (float)(-m_scroll.X + m_firstPoint.X) / m_xZoom,
                        (float)(-m_scroll.Y + m_firstPoint.Y) / m_yZoom);
                    m_isMultiSelecting = m_isScrolling = false;
                    Cursor = Cursors.SizeAll;
                }
                m_isZooming = value;
            }
        }

        /// <summary>
        /// Gets whether the user is dragging</summary>
        public bool IsDragging
        {
            get { return m_isMultiSelecting || m_isScrolling || m_isZooming; }
        }

        /// <summary>
        /// Gets and sets whether the control should auto-scroll when the
        /// user drags past an edge</summary>
        public bool AutoScroll
        {
            get { return m_autoScroll; }
            set { m_autoScroll = value; }
        }

        /// <summary>
        /// Override for custom actions during auto scroll</summary>
        protected virtual void OnAutoScroll()
        {
        }

        /// <summary>
        /// Gets the vertical scroll bar control</summary>
        protected VScrollBar VerticalScrollBar
        {
            get { return m_vScrollBar; }
        }

        /// <summary>
        /// Gets the horizontal scroll bar control</summary>
        protected HScrollBar HorizontalScrollBar
        {
            get { return m_hScrollBar; }
        }

        /// <summary>
        /// Updates vertical and horizontal scrollbars to correspond to the current visible and canvas
        /// dimensions</summary>
        /// <param name="vScrollBar">Vertical ScrollBar, or null if none</param>
        /// <param name="hScrollBar">Horizontal ScrollBar, or null if none</param>
        protected virtual void UpdateScrollBars(VScrollBar vScrollBar, HScrollBar hScrollBar)
        {
            Size canvasSizeInPixels = GetCanvasSize();
            RectangleF clientRect = VisibleClientRectangle;
            Size viewSizeInPixels = new Size((int)clientRect.Width, (int)clientRect.Height);

            WinFormsUtil.UpdateScrollbars(
                vScrollBar,
                hScrollBar,
                viewSizeInPixels,
                canvasSizeInPixels);
        }

        /// <summary>
        /// Raises the Scrolled event</summary>
        protected virtual void OnScroll()
        {
            EventHandler handler = Scrolled;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the Zoomed event</summary>
        protected virtual void OnZoom()
        {
            EventHandler handler = Zoomed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void autoScrollTimer_Tick(object sender, EventArgs e)
        {
            m_autoScrollTimer.Stop();
            if (!VisibleClientRectangle.Contains(m_currentPoint))
            {
                if (m_isMultiSelecting)
                {
                    Rectangle selectionRect = MakeSelectionRect(m_currentPoint, m_firstPoint);
                    ControlPaint.DrawReversibleFrame(selectionRect, BackColor, FrameStyle.Dashed);
                }

                const int autoScrollSpeed = 10;
                Rectangle visibleRect = VisibleClientRectangle;
                Point scroll = m_scroll;
                if (m_currentPoint.X < 0)
                    scroll.X += autoScrollSpeed;
                else if (m_currentPoint.X > visibleRect.Width)
                    scroll.X -= autoScrollSpeed;
                if (m_currentPoint.Y < 0)
                    scroll.Y += autoScrollSpeed;
                else if (m_currentPoint.Y > visibleRect.Height)
                    scroll.Y -= autoScrollSpeed;
                Point diff = m_scroll;

                ScrollPosition = new Point(scroll.X, scroll.Y);

                diff.X = m_scroll.X - diff.X;
                diff.Y = m_scroll.Y - diff.Y;

                // adjust mouse positions
                m_firstPoint.X += diff.X;
                m_firstPoint.Y += diff.Y;
                m_last = m_currentPoint;

                OnAutoScroll();
                base.Update(); // without this, the selection rect doesn't draw correctly

                if (m_isMultiSelecting)
                {
                    Rectangle rect = MakeSelectionRect(m_currentPoint, m_firstPoint);
                    ControlPaint.DrawReversibleFrame(rect, BackColor, FrameStyle.Dashed);
                }
                m_autoScrollTimer.Start();
            }
        }

        private void ZoomAboutCenter(float xZoom, float yZoom)
        {
            PointF zoomCenter = new PointF(
                (float)(m_scroll.X - Width / 2) / (float)m_canvasSize.Width,
                (float)(m_scroll.Y - Height / 2) / (float)m_canvasSize.Height);

            SetZoom(xZoom, yZoom);

            Invalidate();

            ScrollPosition = new Point(
                (int)(-zoomCenter.X * m_canvasSize.Width - Width / 2),
                (int)(-zoomCenter.Y * m_canvasSize.Height - Height / 2));
        }

        // Make a rectangle with positive width and height
        private Rectangle MakeSelectionRect(Point p1, Point p2)
        {
            Rectangle rect =  GdiUtil.MakeRectangle(p1, p2);
            rect.Intersect(VisibleClientRectangle);
            rect = RectangleToScreen(rect);
            return rect;
        }

        private void vScrollBar_ValueChanged(object sender, EventArgs e)
        {
            m_scroll.Y = -m_vScrollBar.Value;

            OnScroll();

            Invalidate();
        }

        private void hScrollBar_ValueChanged(object sender, EventArgs e)
        {
            m_scroll.X = -m_hScrollBar.Value;

            OnScroll();

            Invalidate();
        }

        // To be called when the user has switched focus to another app or lifted up a mouse button
        // or otherwise cancelled a user interface action in progress.
        private void CancelAction()
        {
            m_isDragging = false;
            m_isMultiSelecting = false;
            m_isScrolling = false;
            m_isZooming = false;
            m_constrain = false;
            m_dragOverThreshold = false;
            m_autoScrollTimer.Stop();
            Cursor = Cursors.Arrow;
        }

        private const float FrameScale = 0.86f;

        private float m_xZoom = 1.0f;
        private float m_yZoom = 1.0f;
        private float m_minXZoom = 0.125f;
        private float m_minYZoom = 0.125f;
        private float m_maxXZoom = 200.0f;
        private float m_maxYZoom = 200.0f;
        private Size m_canvasSize = new Size(1, 1);

        private Point m_scroll; //the translation in Transform which is also the negative scrollbar values.
        private readonly VScrollBar m_vScrollBar;
        private readonly HScrollBar m_hScrollBar;

        private bool m_autoScroll;
        private Point m_autoScrollPositionStart;
        private readonly Timer m_autoScrollTimer;
        
        private Point m_firstPoint;
        private Point m_currentPoint;
        private Point m_last;
        private int m_dragThreshold = 3;
        private float m_xZoomStart;
        private float m_yZoomStart;
        private PointF m_zoomCenterStart;

        private bool m_isDragging;
        private bool m_isMultiSelecting;
        private bool m_isScrolling;
        private bool m_dragOverThreshold;
        private bool m_constrain;
        private bool m_constrainX;
        private bool m_isZooming;
        private bool m_uniformZoom = true;
    }
}
