//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that allows users to drag-select rectangular regions on the
    /// adapted control to modify the selection. The AdaptableControl must be
    /// a D2dAdaptableControl. Use with a D2dRectangleDragRenderer.</summary>
    public class D2dRectangleDragSelector : DraggingControlAdapter, IDragSelector
    {
        /// <summary>
        /// Gets or sets the modifier keys that indicate a drag operation</summary>
        public Keys ModifierKeys
        {
            get { return m_modifierKeys; }
            set { m_modifierKeys = value; }
        }
        private Keys m_modifierKeys = Keys.Shift | Keys.Control;

        /// <summary>
        /// Event that is raised after a selection is made</summary>
        public event EventHandler<DragSelectionEventArgs> Selected;

        /// <summary>
        /// Gets or sets the color of the border of the selection rectangle.</summary>
        public Color SelectionBorderColor = Color.DodgerBlue;

        /// <summary>
        /// Gets or sets the color used to fill the selection rectangle.</summary>
        public Color SelectionFillColor = Color.FromArgb(64, Color.DodgerBlue);//25% opaque

        /// <summary>
        /// Gets whether the user is actively creating a selection rectangle by clicking and
        /// dragging on the canvas.</summary>
        public bool IsSelecting { get; private set; }

        /// <summary>
        /// Performs custom operations when a selection is made</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnSelected(DragSelectionEventArgs e)
        {
        }

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control. Must be a D2dAdaptableControl.</param>
        protected override void Bind(AdaptableControl control)
        {
            m_transformAdapter = control.As<ITransformAdapter>();
            m_autoTranslateAdapter = control.As<IAutoTranslateAdapter>();

            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            base.Unbind(control);
        }

        /// <summary>
        /// Performs custom actions when performing a mouse dragging operation</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnDragging(MouseEventArgs e)
        {
            m_modifiers = Control.ModifierKeys;
            if (e.Button == MouseButtons.Left &&
                ((m_modifiers & ~m_modifierKeys) == 0))
            {
                // make sure we can capture the mouse
                if (!AdaptedControl.Capture)
                {
                    m_firstCanvasPoint = m_currentCanvasPoint = ClientToCanvas(FirstPoint);

                    IsSelecting = true;

                    AdaptedControl.Capture = true;
                    m_saveCursor = AdaptedControl.Cursor;
                    AdaptedControl.Cursor = Cursors.Cross;

                    if (m_autoTranslateAdapter != null)
                        m_autoTranslateAdapter.Enabled = true;
                }
            }

            if (IsSelecting)
            {
                AdaptedControl.Invalidate();
                m_currentCanvasPoint = ClientToCanvas(CurrentPoint);
            }
        }

        /// <summary>
        /// Performs custom actions on adaptable control MouseUp events; base method should
        /// be called first</summary>
        /// <param name="sender">Adaptable control</param>
        /// <param name="e">Event args</param>
        protected override void OnMouseUp(object sender, MouseEventArgs e)
        {
            base.OnMouseUp(sender, e);

            if (IsSelecting)
            {
                Rectangle rect = MakeSelectionRect(m_currentCanvasPoint, m_firstCanvasPoint);
                RaiseSelected(rect, m_modifiers);

                IsSelecting = false;
                AdaptedControl.Cursor = m_saveCursor;
                AdaptedControl.Capture = false;

                if (m_autoTranslateAdapter != null)
                    m_autoTranslateAdapter.Enabled = false;

                // In case nothing was selected, we need to make sure that the rectangle is cleared.
                AdaptedControl.Invalidate();
            }
        }

        private void RaiseSelected(Rectangle bounds, Keys modifiers)
        {
            var e = new DragSelectionEventArgs(bounds, modifiers);
            OnSelected(e);
            Selected.Raise(this, e);
        }

        // Makes a rectangle in canvas (world) coordinates, given points in Windows Client coordinates.
        private Rectangle MakeSelectionRect(PointF p1, PointF p2)
        {
            Rectangle rect = GdiUtil.MakeRectangle(
                new Point((int)p1.X, (int)p1.Y),
                new Point((int)p2.X, (int)p2.Y));

            if (m_transformAdapter != null)
                rect = GdiUtil.Transform(m_transformAdapter.Transform, rect);

            return rect;
        }

        private PointF ClientToCanvas(PointF p)
        {
            if (m_transformAdapter != null)
                p = GdiUtil.InverseTransform(m_transformAdapter.Transform, p);
            return p;
        }

        private IAutoTranslateAdapter m_autoTranslateAdapter;
        private ITransformAdapter m_transformAdapter;
        private Cursor m_saveCursor;
        private Keys m_modifiers;
        private PointF m_firstCanvasPoint;
        private PointF m_currentCanvasPoint;
    }
}
