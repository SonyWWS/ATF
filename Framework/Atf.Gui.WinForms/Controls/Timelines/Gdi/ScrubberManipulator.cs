//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// OBSOLETE. Please use the much faster D2dScrubberManipulator in the Sce.Atf.Controls.Timelines.Direct2D namespace.
    /// Scrubber manipulator: a vertical bar that can slide left and right by grabbing the handle on
    /// the top.</summary>
    /// <remarks>In the next release of ATF, we are planning on marking this with the Obsolete attribute</remarks>
    public class ScrubberManipulator : IEvent
    {
        /// <summary>
        /// Constructor that permanently attaches to the given TimelineControl by subscribing to its
        /// events.</summary>
        /// <param name="owner">The TimelineControl whose events we permanently listen to</param>
        public ScrubberManipulator(TimelineControl owner)
        {
            Owner = owner;
            Owner.MouseDownPicked += owner_MouseDownPicked;
            Owner.MouseMovePicked += owner_MouseMovePicked;
            Owner.Picking += owner_Picking;
            Owner.MouseMove += MouseMoveHandler;
            Owner.MouseUp += owner_MouseUp;
            Owner.Paint += owner_Paint;
            Owner.BoundingRectUpdating += owner_BoundingRectUpdating;
        }

        /// <summary>
        /// Gets or sets the current position of the scrubber for the owning TimelineControl, in world coordinates</summary>
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
        /// The TimelineControl that this ScaleManipulator is bound to, listening to its events</summary>
        public readonly TimelineControl Owner;

        /// <summary>
        /// Event that is raised after any scrubber manipulator has been moved. The 'sender' parameter
        /// will be the ScrubberManipulator whose Position has changed.</summary>
        public static event EventHandler Moved;

        /// <summary>
        /// Gets or sets the color of this manipulator and handle</summary>
        [DefaultValue(typeof(Color), "Black")]
        public static Color Color
        {
            get { return s_color; }
            set { s_color = value; }
        }

        /// <summary>
        /// Validates and corrects the proposed new position of the scrubber manipulator</summary>
        /// <param name="position">The proposed new value of the Position property.</param>
        /// <returns>The corrected value that the Position property will become</returns>
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
        protected virtual void DrawManipulator(Graphics g, out RectangleF handleRect)
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

            using (Pen pen = new Pen(s_color))
            {
                g.DrawLine(pen, viewX, clipRectangle.Top, viewX, clipRectangle.Bottom);
            }

            Color handle_color = m_isMoving ? Color.Tomato : s_color;
            using (Brush brush = new SolidBrush(handle_color))
            {
                int pos_x = Convert.ToInt32(viewX);
                int pos_y = Convert.ToInt32(clipRectangle.Top + 5);
                s_arrow[0] = new Point(pos_x - 4, pos_y - 5);
                s_arrow[1] = new Point(pos_x - 4, pos_y);
                s_arrow[2] = new Point(pos_x - 5, pos_y + 1);
                s_arrow[3] = new Point(pos_x - 5, pos_y + 2);
                s_arrow[4] = new Point(pos_x, pos_y + 7);
                s_arrow[5] = new Point(pos_x + 5, pos_y + 2);
                s_arrow[6] = new Point(pos_x + 5, pos_y + 1);
                s_arrow[7] = new Point(pos_x + 4, pos_y);
                s_arrow[8] = new Point(pos_x + 4, pos_y - 5);
                g.FillPolygon(brush, s_arrow);   // Fill arrow
                g.DrawPolygon(s_grayPen, s_arrow);  // Draw arrow border with gray

                string label = Position.ToString(CultureInfo.CurrentCulture);
                g.DrawString(label, Owner.Font, SystemBrushes.WindowText, pos_x + 6, clipRectangle.Top);
            }
        }

        #region IEvent Members

        float IEvent.Start
        {
            get { return Position; }
            set { Position = value; }
        }

        float IEvent.Length
        {
            get { return 0; }
            set { }
        }

        Color IEvent.Color
        {
            get { return Color; }
            set { Color = value; }
        }

        string IEvent.Name
        {
            get { return "Scrubber"; }
            set { }
        }

        #endregion

        private void owner_Paint(object sender, PaintEventArgs e)
        {
            // Tighten clipping region. The TimelineRenderer assumes that it has to shrink the current
            //  Graphics.Clip region by the header width.
            Graphics g = e.Graphics;
            Region originalClip = g.Clip;
            Rectangle clipRectangle = Owner.VisibleClientRectangle;
            g.Clip = new Region(clipRectangle);

            DrawManipulator(g, out m_handleRect);

            // Restore clipping region.
            g.Clip = originalClip;
        }

        private void owner_BoundingRectUpdating(object sender, TimelineControl.BoundingRectEventArgs e)
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
                Position = Sce.Atf.GdiUtil.InverseTransform(worldToView, e.MouseEvent.Location.X);
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
        /// Event handler for a mouse move event on the owning TimelineControl</summary>
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
                    TimelineControl.SnapOptions options = new TimelineControl.SnapOptions();
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
        private static readonly Pen s_grayPen = new Pen(Color.FromArgb(116, 114, 106));  // Same gray Photoshop uses
        private static readonly Point[] s_arrow = new Point[9];
    }
}
