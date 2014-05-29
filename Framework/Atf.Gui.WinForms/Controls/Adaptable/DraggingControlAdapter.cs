//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Abstract base class for control adapters that drag. It tracks the mouse and
    /// handles the logic of determining when the mouse has moved enough to be considered
    /// a drag.</summary>
    public abstract class DraggingControlAdapter : ControlAdapter
    {
        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the reverse order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control</param>
        protected override void BindReverse(AdaptableControl control)
        {
            control.MouseDown += MouseDownHandler;
            control.MouseMove += MouseMoveHandler;
            control.MouseUp += MouseUpHandler;
            control.MouseClick += OnMouseClick;
            control.MouseDoubleClick += OnMouseDoubleClick;
            control.MouseLeave += MouseLeaveHandler;
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.MouseDown -= MouseDownHandler;
            control.MouseMove -= MouseMoveHandler;
            control.MouseUp -= MouseUpHandler;
            control.MouseClick -= OnMouseClick;
            control.MouseDoubleClick -= OnMouseDoubleClick;
            control.MouseLeave -= MouseLeaveHandler;
        }

        /// <summary>
        /// Gets the point of the last mouse down event</summary>
        public Point FirstPoint
        {
            get { return m_firstPoint; }
        }
        private Point m_firstPoint;

        /// <summary>
        /// Gets the current mouse position</summary>
        public Point CurrentPoint
        {
            get { return m_currentPoint; }
        }
        private Point m_currentPoint;

        /// <summary>
        /// Gets the difference between CurrentPoint and FirstPoint</summary>
        public Point Delta
        {
            get
            {
                return new Point(
                    m_currentPoint.X - m_firstPoint.X,
                    m_currentPoint.Y - m_firstPoint.Y);
            }
        }

        /// <summary>
        /// Gets whether mouse is being dragged by the user</summary>
        public bool IsDragging
        {
            get { return m_isDragging; }
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseDown events</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void OnMouseDown(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseMove events</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void OnMouseMove(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs one time action when drag starts</summary>
        /// <param name="e">MouseEventArgs describing event</param>
        protected virtual void OnBeginDrag(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse move event args</param>
        protected virtual void OnDragging(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs one time action when drag ends</summary>
        /// <param name="e">MouseEventArgs describing event</param>
        protected virtual void OnEndDrag(MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseUp events. On a double-click, this
        /// method gets called twice.</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void OnMouseUp(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseClick events. On a double-click, this
        /// method is only called once.</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void OnMouseClick(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseDoubleClick events</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseLeave events</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected virtual void OnMouseLeave(object sender, EventArgs e)
        {
        }

        private void MouseDownHandler(object sender, MouseEventArgs e)
        {
            m_firstPoint = m_currentPoint = new Point(e.X, e.Y);
            m_mouseDown = true;
            OnMouseDown(sender, e);
        }

        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            m_currentPoint = new Point(e.X, e.Y);

            if (m_mouseDown && !m_isDragging)
            {
                Size dragThreshold = SystemInformation.DragSize;
                if (Math.Abs(m_firstPoint.X - m_currentPoint.X) >= dragThreshold.Width ||
                    Math.Abs(m_firstPoint.Y - m_currentPoint.Y) >= dragThreshold.Height)
                {
                    OnBeginDrag(e);
                    m_isDragging = true;
                }
            }

            if (m_isDragging)
                OnDragging(e);

            OnMouseMove(sender, e);
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            m_mouseDown = false;

            if (m_isDragging)
                OnEndDrag(e);

            m_isDragging = false;
            
            OnMouseUp(sender, e);
        }

        private void MouseLeaveHandler(object sender, EventArgs e)
        {
            m_mouseDown = false;
            m_isDragging = false;
            OnMouseLeave(sender, e);
        }

        private bool m_mouseDown;
        private bool m_isDragging;
    }
}
