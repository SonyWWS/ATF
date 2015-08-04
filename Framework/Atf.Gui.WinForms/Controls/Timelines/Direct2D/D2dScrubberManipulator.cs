//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D
{
    /// <summary>
    /// Scrubber manipulator: a vertical bar that can slide left and right by grabbing the handle on
    /// the top</summary>
    public class D2dScrubberManipulator : IEvent
    {
        /// <summary>
        /// Constructor that permanently attaches to the given timeline control by subscribing to its
        /// events</summary>
        /// <param name="owner">The timeline control whose events we permanently listen to</param>
        public D2dScrubberManipulator(D2dTimelineControl owner)
        {
            Owner = owner;
            Owner.MouseDownPicked += owner_MouseDownPicked;
            Owner.MouseMovePicked += owner_MouseMovePicked;
            Owner.Picking += owner_Picking;
            Owner.MouseMove += MouseMoveHandler;
            Owner.MouseUp += owner_MouseUp;
            Owner.DrawingD2d += owner_DrawingD2d;
            Owner.BoundingRectUpdating += owner_BoundingRectUpdating;

            m_position = owner.TimelineStart;
        }

        /// <summary>
        /// Gets or sets the current position of the scrubber for the owning timeline control, in world coordinates</summary>
        public float Position
        {
            get { return m_position; }
            set
            {
                value = ValidatePosition(value);
                if (m_position != value)
                {
                    m_position = value;
                    Owner.Invalidate();
                    OnMoved(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets whether or not the user is actively moving the scrubber manipulator</summary>
        public bool IsMoving
        {
            get { return m_isMoving; }
        }

        /// <summary>
        /// The timeline control that this scrubber manipulator is bound to, listening to its events</summary>
        public readonly D2dTimelineControl Owner;

        /// <summary>
        /// Event that is raised after any scrubber manipulator has been moved. The 'sender' parameter
        /// is the scrubber manipulator whose Position property has changed.</summary>
        public static event EventHandler Moved;

        /// <summary>
        /// Gets or sets the color of this scrubber manipulator and handle</summary>
        [DefaultValue(typeof(Color), "Black")]
        public static Color Color
        {
            get { return s_color; }
            set { s_color = value; }
        }

        /// <summary>
        /// Validates and corrects the proposed new position of the scrubber manipulator</summary>
        /// <param name="position">The proposed new value of the Position property</param>
        /// <returns>The corrected value that the Position property becomes</returns>
        protected virtual float ValidatePosition(float position)
        {
            return Owner.ConstrainFrameOffset(position);
        }

        /// <summary>
        /// Raises the Moved event</summary>
        /// <param name="e">an empty EventArgs</param>
        protected virtual void OnMoved(EventArgs e)
        {
            if (Moved != null)
                Moved(this, e);
        }

        /// <summary>
        /// Draws the scrubber manipulator and calculates the bounding rectangle on the handle</summary>
        /// <param name="g">The graphics object to draw with</param>
        /// <param name="handleRect">The handle's bounding rectangle for pick tests, in view
        /// coordinates</param>
        protected virtual void DrawManipulator(D2dGraphics g, out RectangleF handleRect)
        {
            Matrix worldToView = Owner.Transform;
            float viewX = Sce.Atf.GdiUtil.Transform(worldToView, Position);
            Rectangle clipRectangle = Owner.VisibleClientRectangle;

            // allow only the arrow portion to be selected
            handleRect = new RectangleF(
                viewX - 5,
                clipRectangle.Top,
                10,
                7);

            g.DrawLine(viewX, clipRectangle.Top, viewX, clipRectangle.Bottom, s_color, 1.0f, null);

            Color handle_color = m_isMoving ? Color.Tomato : s_color;
            float pos_x = viewX;
            float pos_y = clipRectangle.Top + 5;
            s_arrow[0] = new PointF(pos_x - 4, pos_y - 5);
            s_arrow[1] = new PointF(pos_x - 4, pos_y);
            s_arrow[2] = new PointF(pos_x - 5, pos_y + 1);
            s_arrow[3] = new PointF(pos_x - 5, pos_y + 2);
            s_arrow[4] = new PointF(pos_x, pos_y + 7);
            s_arrow[5] = new PointF(pos_x + 5, pos_y + 2);
            s_arrow[6] = new PointF(pos_x + 5, pos_y + 1);
            s_arrow[7] = new PointF(pos_x + 4, pos_y);
            s_arrow[8] = new PointF(pos_x + 4, pos_y - 5);
            //g.FillPolygon(s_arrow, handle_color);   // Fill arrow
            // Draw arrow border with same gray Photoshop uses
            //g.DrawLines(s_arrow, Color.FromArgb(116, 114, 106), 3.0f);

            g.DrawLines(s_arrow, handle_color, 2.0f);

            string label = Position.ToString(CultureInfo.CurrentCulture);
            g.DrawText(label, Owner.Renderer.TextFormat, new PointF(pos_x + 6, clipRectangle.Top), SystemColors.WindowText);
        }

        #region IEvent Members

        /// <summary>
        /// Gets and sets the event's start time</summary>
        float IEvent.Start
        {
            get { return Position; }
            set { Position = value; }
        }

        /// <summary>
        /// Gets and sets the event's length (duration)</summary>
        float IEvent.Length
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Gets and sets the event's color</summary>
        Color IEvent.Color
        {
            get { return Color; }
            set { Color = value; }
        }

        /// <summary>
        /// Gets and sets the event's name</summary>
        string IEvent.Name
        {
            get { return "Scrubber"; }
            set { }
        }

        #endregion

        private void owner_DrawingD2d(object sender, EventArgs e)
        {
            // Tighten clipping region. The TimelineRenderer assumes that it has to shrink the current
            //  Graphics.Clip region by the header width.
            D2dGraphics g = Owner.D2dGraphics;
            Rectangle clipRectangle = Owner.VisibleClientRectangle;
            try
            {
                g.PushAxisAlignedClip(clipRectangle);
                DrawManipulator(g, out m_handleRect);
            }
            finally
            {
                g.PopAxisAlignedClip();
            }
        }

        private void owner_BoundingRectUpdating(object sender, D2dTimelineControl.BoundingRectEventArgs e)
        {
            e.NewClientRect = m_handleRect;
        }

        private void owner_MouseDownPicked(object sender, HitEventArgs e)
        {
            if (e.MouseEvent.Button == MouseButtons.Left &&
                e.HitRecord.HitObject == m_handleHitObject)
            {
                m_isMoving = true;
                Owner.Cursor = Cursors.SizeWE;
                e.Handled = true;
                Owner.Invalidate();
            }
            else if (e.HitRecord.Type == HitType.TimeScale)
            {
                m_isMoving = false;
                Matrix worldToView = Owner.Transform;
                // Setting the position also invalidates the Owner Control.
                PointF mouseLocation = e.MouseEvent.Location;
                Position = GdiUtil.InverseTransform(worldToView, mouseLocation.X);
            }
        }

        private void owner_MouseMovePicked(object sender, HitEventArgs e)
        {
            if (e.MouseEvent.Button == MouseButtons.None &&
                e.HitRecord.HitObject == m_handleHitObject)
            {
                Owner.Cursor = Cursors.SizeWE;
                e.Handled = true;
            }
        }

        private void owner_Picking(object sender, HitEventArgs e)
        {
            Rectangle clipRectangle = Owner.VisibleClientRectangle;

            if (m_handleRect.IntersectsWith(clipRectangle) &&
                e.PickRectangle.IntersectsWith(m_handleRect))
            {
                // sets Handled to true
                e.HitRecord = new HitRecord(HitType.Custom, m_handleHitObject);
            }
        }

        /// <summary>
        /// Event handler for a mouse move event on the owning timeline control</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected virtual void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            if (IsMoving)
            {
                Matrix worldToView = Owner.Transform;
                float newPosition = Sce.Atf.GdiUtil.InverseTransform(worldToView, e.Location.X);

                if (Control.ModifierKeys == Keys.Shift)
                {
                    // Get snap-from point in world coordinates.
                    float[] movingPoints = new[] { newPosition };
                    D2dTimelineControl.SnapOptions options = new D2dTimelineControl.SnapOptions();
                    options.IncludeScrubber = false;
                    options.CheckModifierKeys = false;
                    float snapOffset = Owner.GetSnapOffset(movingPoints, options);
                    newPosition += snapOffset;
                }

                Position = newPosition;
            }
        }

        private void owner_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_isMoving)
            {
                m_isMoving = false;
                Owner.Invalidate();
            }
        }

        // An object to be returned in a HitRecord when the manipulator is hit.
        private class HitRecordObject
        {
            public HitRecordObject()
            {
            }

            // for tooltip text
            public override string ToString()
            {
                return "drag left or right to reposition Scrubber";
            }
        }

        private readonly HitRecordObject m_handleHitObject = new HitRecordObject();//allocated once so tooltips behave
        private RectangleF m_handleRect; //Area of the manipulator handle, in view coordinates.
        private bool m_isMoving;
        private float m_position;

        private static Color s_color = Color.Black;
        private static readonly PointF[] s_arrow = new PointF[9];
    }
}
