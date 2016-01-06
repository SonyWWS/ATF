//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using Sce.Atf.Direct2D;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Adapter that works with D2dRectangleDragSelector, to render the selection rectangle.
    /// Two classes are needed instead of one because the selection rectangle needs to appear
    /// on top of all other circuit elements but should receive mouse events at a lower priority.</summary>
    public class D2dRectangleDragRenderer : DraggingControlAdapter
    {
        public D2dRectangleDragRenderer(D2dRectangleDragSelector selector)
        {
            Selector = selector;
        }

        public D2dRectangleDragSelector Selector { get; private set; }

        /// <summary>
        /// Gets or sets the color of the border of the selection rectangle.</summary>
        public Color SelectionBorderColor = Color.DodgerBlue;

        /// <summary>
        /// Gets or sets the color used to fill the selection rectangle.</summary>
        public Color SelectionFillColor = Color.FromArgb(64, Color.DodgerBlue);//25% opaque

        /// <summary>
        /// Binds the adapter to the adaptable control. Called in the order that the adapters
        /// were defined on the control.</summary>
        /// <param name="control">Adaptable control. Must be a D2dAdaptableControl.</param>
        protected override void Bind(AdaptableControl control)
        {
            m_transformAdapter = control.As<ITransformAdapter>();

            var d2dControl = (D2dAdaptableControl)control;
            d2dControl.DrawingD2d += control_DrawingD2d;

            base.Bind(control);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            var d2dControl = (D2dAdaptableControl)control;
            d2dControl.DrawingD2d -= control_DrawingD2d;

            base.Unbind(control);
        }

        private void control_DrawingD2d(object sender, EventArgs e)
        {
            if (Selector.IsSelecting)
            {
                var d2dControl = (D2dAdaptableControl)sender;
                D2dGraphics g = d2dControl.D2dGraphics;

                // Replace transform and anti-aliasing setting.
                Matrix3x2F xform = d2dControl.D2dGraphics.Transform;
                d2dControl.D2dGraphics.Transform = Matrix3x2F.Identity;
                D2dAntialiasMode oldAA = d2dControl.D2dGraphics.AntialiasMode;
                d2dControl.D2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;

                // Draw the selection rectangle.
                Rectangle rect = MakeSelectionRect(
                    ClientToCanvas(Selector.CurrentPoint), ClientToCanvas(Selector.FirstPoint));
                rect.Intersect(AdaptedControl.ClientRectangle);
                var rectF = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
                g.DrawRectangle(rectF, SelectionBorderColor, 1.0f, null);
                g.FillRectangle(rectF, SelectionFillColor);

                // Restore D2dGraphics settings.
                d2dControl.D2dGraphics.Transform = xform;
                d2dControl.D2dGraphics.AntialiasMode = oldAA;
            }
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

        private ITransformAdapter m_transformAdapter;
    }
}
