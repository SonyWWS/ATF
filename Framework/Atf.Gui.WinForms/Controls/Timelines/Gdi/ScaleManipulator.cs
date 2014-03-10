//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// OBOSLETE. Please use the much faster D2dScaleManipulator version in the Sce.Atf.Controls.Timelines.Direct2D namespace.
    /// Scale manipulator: a horizontal bar on the top of the scale bar. Has handles at each end,
    ///  bracketing the selection set.
    /// Has two distinct modes:
    /// 1. Individual interval scaling occurs when the left or right border of an interval is scaled.
    /// In this case, the entire selection of intervals is scaled "in place". That is, their starting
    /// locations do not change. Only intervals are affected.
    /// 2. If the handles on the scaling manipulator are dragged, then the portion of the timeline
    /// that is bracketed by the first selected object to the last selected object is scaled in time.
    /// Everything selected is scaled -- keys, markers, and intervals.</summary>
    /// <remarks>In the next release of ATF, we are planning on marking this with the Obsolete attribute</remarks>
    public class ScaleManipulator
    {
        /// <summary>
        /// Constructor that permanently attaches to the given TimelineControl by subscribing to its
        /// events</summary>
        /// <param name="owner">The TimelineControl whose events we permanently listen to</param>
        public ScaleManipulator(TimelineControl owner)
        {
            Owner = owner;
            Owner.MouseDownPicked += owner_MouseDownPicked;
            Owner.MouseMovePicked += owner_MouseMovePicked;
            Owner.Picking += owner_Picking;
            Owner.MouseUp += owner_MouseUp;
            Owner.Paint += owner_Paint;
        }

        /// <summary>
        /// The TimelineControl that this ScaleManipulator is bound to, listening to its events</summary>
        public readonly TimelineControl Owner;

        /// <summary>
        /// The height, in display units, of the manipulator handle</summary>
        [DefaultValue(24)]
        public static int HandleHeight
        {
            get { return s_handleHeight; }
            set { s_handleHeight = value; }
        }

        /// <summary>
        /// Gets or sets the color of this manipulator and the two handles</summary>
        [DefaultValue(typeof(Color), "Black")]
        public static Color Color
        {
            get { return s_color; }
            set { s_color = value; }
        }

        /// <summary>
        /// Gets the minimum world coordinate for the currently selected events from the last paint</summary>
        protected float WorldMin
        {
            get { return m_worldMin; }
        }

        /// <summary>
        /// Gets the maximum world coordinate for the currently selected events from the last paint</summary>
        protected float WorldMax
        {
            get { return m_worldMax; }
        }

        /// <summary>
        /// Gets whether the user is currently scaling the selection by holding down
        /// the mouse button on either an interval's boundaries or on one of this manipulator's
        /// handles and dragging</summary>
        protected bool IsScaling
        {
            get
            {
                return
                    m_resizer != null &&
                    Owner.DragOverThreshold &&
                    !Owner.IsUsingMouse;
            }
        }

        /// <summary>
        /// Gets the temporary object used per scaling operation. If this is not null, then the user
        /// is currently holding down the mouse button on either an interval's boundary or on one of
        /// the manipulator's handles</summary>
        protected Resizer ScaleHelper
        {
            get { return m_resizer; }
        }

        /// <summary>
        /// Gets which side of the interval or selection set is being resized. Can only be called
        /// if IsScaling is true.</summary>
        protected GhostType DraggedSide
        {
            get
            {
                return m_resizer.DraggedSide == Side.Left ? GhostType.ResizeLeft : GhostType.ResizeRight;
            }
        }

        /// <summary>
        /// Enum for scaling mode we are in</summary>
        protected enum ScaleMode
        {
            /// <summary>Only intervals are affected and their start positions remain pinned</summary>
            InPlace,

            /// <summary>
            /// All selected IEvents (keys, markers, and intervals) are scaled in an
            /// interval bracketed by the earliest and latest selected events</summary>
            TimePeriod
        }

        /// <summary>
        /// Draws the scale manipulator and calculates the bounding rectangles on the left and right
        /// handles</summary>
        /// <param name="g">The graphics object to draw with</param>
        /// <param name="leftHandle">The left handle's bounding rectangle for pick tests, in view
        /// coordinates</param>
        /// <param name="rightHandle">The right handle's bounding rectangle for pick tests, in view
        /// coordinates</param>
        protected virtual void DrawManipulator(Graphics g, out RectangleF leftHandle, out RectangleF rightHandle)
        {
            const int penWidth = 3;

            Matrix worldToView = Owner.Transform;
            float viewMin = Sce.Atf.GdiUtil.Transform(worldToView, WorldMin);
            float viewMax = Sce.Atf.GdiUtil.Transform(worldToView, WorldMax);
            leftHandle = new RectangleF(viewMin - penWidth * 0.5f, 0.0f, penWidth, HandleHeight);
            rightHandle = new RectangleF(viewMax - penWidth * 0.5f, 0.0f, penWidth, HandleHeight);

            if (IsScaling &&
                ScaleHelper.Mode == ScaleMode.TimePeriod)
            {
                // Draw lines in red at current ghost position.
                viewMin = Sce.Atf.GdiUtil.Transform(worldToView, ScaleHelper.WorldGhostMin);
                viewMax = Sce.Atf.GdiUtil.Transform(worldToView, ScaleHelper.WorldGhostMax);
                using (Pen pen = new Pen(Color.Red, penWidth))
                {
                    g.DrawLine(pen, viewMin, 0.0f, viewMax, 0.0f);
                    g.DrawLine(pen, viewMin, 0.0f, viewMin, HandleHeight);
                    g.DrawLine(pen, viewMax, 0.0f, viewMax, HandleHeight);
                }
            }
            else
            {
                // Draw using original positions and the usual color.
                using (Pen pen = new Pen(Color, penWidth))
                {
                    g.DrawLine(pen, viewMin, 0.0f, viewMax, 0.0f);
                    g.DrawLine(pen, viewMin, 0.0f, viewMin, HandleHeight);
                    g.DrawLine(pen, viewMax, 0.0f, viewMax, HandleHeight);
                }
            }
        }

        /// <summary>
        /// Enum for which side of the manipulator or interval is being dragged</summary>
        protected enum Side
        {
            /// <summary>Left side</summary>
            Left,

            /// <summary>Right side</summary>
            Right
        }

        /// <summary>
        /// Created for the duration of a scaling event. Is responsible for tracking the 'ghosts' and for
        /// doing the final update of the selected objects.</summary>
        protected class Resizer
        {
            /// <summary>
            /// Which scaling mode are we in?</summary>
            public readonly ScaleMode Mode;

            /// <summary>
            /// What is the x-offset from the drag starting point, in world coordinates?</summary>
            public float DragOffsetWithSnap;

            /// <summary>
            /// Gets which side of the interval or manipulator is being dragged</summary>
            public Side DraggedSide
            {
                get { return m_intervalSide; }
            }

            /// <summary>
            /// Gets the minimum ghost x-coordinate in world coordinates</summary>
            public float WorldGhostMin
            {
                get { return m_worldGhostMin; }
            }

            /// <summary>
            /// Gets the maximum ghost x-coordinate in world coordinates</summary>
            public float WorldGhostMax
            {
                get { return m_worldGhostMax; }
            }

            /// <summary>
            /// Constructor for a single scaling or resizing operation</summary>
            /// <param name="side">Which side of the interval or manipulator is being dragged?</param>
            /// <param name="mode">Is an Interval being dragged or a manipulator handle?</param>
            /// <param name="boundary">World coordinate starting position for the drag operation</param>
            /// <param name="owner">The TimelineControl that this manipulator is attached to</param>
            internal Resizer(Side side, ScaleMode mode, float boundary, TimelineControl owner)
            {
                m_intervalSide = side;
                Mode = mode;
                m_originalBoundary = boundary;
                m_owner = owner;
                if (Mode == ScaleMode.InPlace)
                    m_selection = new HashSet<TimelinePath>(GetEditableEvents<IInterval>());
                else
                    m_selection = new HashSet<TimelinePath>(GetEditableEvents<IEvent>());
                if (m_selection.Count == 0)
                    throw new InvalidTransactionException("only IEvents can be resized");

                m_initialMin = float.MaxValue;
                m_initialMax = float.MinValue;

                if (Mode == ScaleMode.TimePeriod)
                {
                    foreach (TimelinePath path in m_selection)
                    {
                        IEvent curr = (IEvent)path.Last;
                        if (curr.Start < m_initialMin)
                            m_initialMin = curr.Start;
                        if (m_initialMax < curr.Start + curr.Length)
                            m_initialMax = curr.Start + curr.Length;
                    }
                }

                DragOffsetWithSnap = m_owner.GetDragOffset().X;
            }

            private IEnumerable<TimelinePath> GetEditableEvents<T>()
                where T : class, IEvent
            {
                foreach (TimelinePath path in m_owner.Selection.GetSelection<TimelinePath>())
                    if (path.Last is T)
                        if (m_owner.IsEditable(path))
                            yield return path;
            }

            /// <summary>
            /// Creates and caches resize information, for drawing ghosts and for performing
            /// actual resize operations</summary>
            /// <param name="layout">TimelineLayout</param>
            /// <param name="worldDrag">Drag offset</param>
            /// <param name="worldToView">World to view transformation matrix</param>
            /// <returns>Array of GhostInfo</returns>
            internal GhostInfo[] CreateGhostInfo(TimelineLayout layout, float worldDrag, Matrix worldToView)
            {
                // Get snap-from point in world coordinates.
                float[] movingPoints = new[] { worldDrag + m_originalBoundary };
                float snapOffset = m_owner.GetSnapOffset(movingPoints, null);

                // adjust dragOffset to snap-to nearest event
                worldDrag += snapOffset;
                DragOffsetWithSnap = worldDrag;

                GhostInfo[] ghosts = new GhostInfo[m_selection.Count];
                IEnumerator<TimelinePath> events = m_selection.GetEnumerator();
                m_worldGhostMin = float.MaxValue;
                m_worldGhostMax = float.MinValue;

                for (int i = 0; i < ghosts.Length; i++)
                {
                    events.MoveNext();
                    IEvent curr = (IEvent)events.Current.Last;
                    RectangleF bounds = layout[events.Current];
                    float viewStart = bounds.Left;
                    float viewEnd = viewStart + bounds.Width;
                    float worldStart = curr.Start;
                    float worldEnd = worldStart + curr.Length;

                    Resize(
                        worldDrag,
                        worldToView,
                        ref viewStart, ref viewEnd,
                        ref worldStart, ref worldEnd);

                    float worldLength = worldEnd - worldStart;
                    bounds = new RectangleF(viewStart, bounds.Y, viewEnd - viewStart, bounds.Height);
                    if (m_worldGhostMin > worldStart)
                        m_worldGhostMin = worldStart;
                    if (m_worldGhostMax < worldEnd)
                        m_worldGhostMax = worldEnd;

                    bool valid = true;

                    IInterval interval = curr as IInterval;
                    if (interval != null)
                    {
                        TimelinePath testPath = new TimelinePath(events.Current);
                        foreach (IInterval other in interval.Track.Intervals)
                        {
                            // Skip this interval if it's part of the selection because we have to assume
                            //  that the track began in a valid state and that all of the selected objects
                            //  will still be valid relative to each other. We only need to check that the
                            //  scaled objects are valid relative to the stationary objects.
                            testPath.Last = other;
                            if (Mode == ScaleMode.TimePeriod &&
                                m_selection.Contains(testPath))
                                continue;
                            else if (other == interval)
                                continue;

                            if (!m_owner.Constraints.IsIntervalValid(interval, ref worldStart, ref worldLength, other))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    ghosts[i] = new GhostInfo(curr, null, worldStart, worldLength, bounds, valid);
                }

                m_ghosts = ghosts;
                return ghosts;
            }

            /// <summary>
            /// CreateGhostInfo must be called first, although sometimes a MouseUp event happens
            /// without a Paint event</summary>
            internal void ResizeSelection()
            {
                if (m_ghosts == null ||
                    m_ghosts.Length == 0)
                {
                    return;
                }

                foreach (GhostInfo ghost in m_ghosts)
                    if (ghost != null && !ghost.Valid)
                        return;

                m_owner.TransactionContext.DoTransaction(delegate
                    {
                        foreach (GhostInfo ghost in m_ghosts)
                        {
                            IEvent curr = ghost.Object as IEvent;
                            if (curr != null)
                            {
                                curr.Start = ghost.Start;
                                curr.Length = ghost.Length;
                            }
                        }
                    },
                    "Resize Events".Localize("scale manipulator's undo / redo description for resizing timeline events"));
            }

            private void Resize(
                float worldDrag,
                Matrix worldToView,
                ref float viewStart, ref float viewEnd,
                ref float worldStart, ref float worldEnd)
            {
                float finalMin;
                float finalMax;

                if (Mode == ScaleMode.InPlace)
                {
                    m_initialMin = worldStart;
                    m_initialMax = worldEnd;
                }

                if (m_intervalSide == Side.Left)
                {
                    finalMin = m_owner.ConstrainFrameOffset(m_initialMin + worldDrag);
                    if (finalMin < 0)
                        finalMin = 0;
                    else if (finalMin >= m_initialMax)
                        finalMin = m_initialMin;
                    finalMax = m_initialMax;
                }
                else
                {
                    finalMax = m_owner.ConstrainFrameOffset(m_initialMax + worldDrag);
                    if (finalMax <= m_initialMin)
                        finalMax = m_initialMax;
                    finalMin = m_initialMin;
                }

                // transform the 4 points (world begin & end and view begin & end)
                ScalePoint(ref worldStart, m_initialMin, m_initialMax, finalMin, finalMax);
                worldStart = m_owner.ConstrainFrameOffset(worldStart);

                ScalePoint(ref worldEnd, m_initialMin, m_initialMax, finalMin, finalMax);
                worldEnd = m_owner.ConstrainFrameOffset(worldEnd);

                viewStart = Sce.Atf.GdiUtil.Transform(worldToView, worldStart);
                viewEnd = Sce.Atf.GdiUtil.Transform(worldToView, worldEnd);
            }

            private static void ScalePoint(ref float x, float initialMin, float initialMax, float finalMin, float finalMax)
            {
                float initialWidth = initialMax - initialMin;
                if (initialWidth < 0.00001f)
                    return;

                x =
                    (x - initialMin) *  //translate by 'origin' of original coordinate system
                        ((finalMax - finalMin) / (initialWidth)) //scale
                    + finalMin; //translate by origin of new coordinate system
            }

            private readonly TimelineControl m_owner;
            private readonly HashSet<TimelinePath> m_selection;
            private readonly Side m_intervalSide;
            private readonly float m_originalBoundary; //x-coordinate, in world coordinates, of the original edge to be moved
            private float m_initialMin, m_initialMax; // in world coordinates
            private float m_worldGhostMin, m_worldGhostMax;
            private GhostInfo[] m_ghosts;
        }

        private void owner_Paint(object sender, PaintEventArgs e)
        {
            // Test if anything is visible.
            m_visibleManipulator =
                TimelineControl.CalculateRange(Owner.EditableSelection, out m_worldMin, out m_worldMax);
            if (!m_visibleManipulator)
                return;

            Graphics g = e.Graphics;
            Matrix worldToView = Owner.Transform;

            // Make the TimelineRenderer draw the ghosts, if necessary. Must happen before
            //  the manipulator is drawn so that snap-to info is created.
            if (IsScaling)
            {
                TimelineLayout layout = Owner.GetLayout();

                GhostInfo[] ghosts = m_resizer.CreateGhostInfo(
                    layout,
                    Owner.GetDragOffset().X,
                    worldToView);

                Owner.Renderer.DrawGhosts(
                    ghosts,
                    DraggedSide,
                    worldToView,
                    Owner.ClientRectangle,
                    g);
            }

            // Tighten clipping region. Has to occur after DrawGhosts because the TimelineRenderer
            //  assumes that it has to shrink the current Graphics.Clip region by the header width.
            Region originalClip = g.Clip;
            Rectangle clipRectangle = Owner.VisibleClientRectangle;
            g.Clip = new Region(clipRectangle);

            // Draw the manipulator, giving client code a customization point.
            DrawManipulator(g, out m_leftHandle, out m_rightHandle);

            // Restore clipping region.
            g.Clip = originalClip;
        }

        private void owner_MouseDownPicked(object sender, HitEventArgs e)
        {
            if (e.MouseEvent.Button != MouseButtons.Left)
                return;

            TimelinePath hitPath = e.HitRecord.HitPath;

            bool isResizing = false;

            switch(e.HitRecord.Type)
            {
                case HitType.IntervalResizeLeft:
                    if (Owner.IsEditable(hitPath) && (
                            Owner.Selection.SelectionContains(hitPath) ||
                            Owner.Select<IEvent>(hitPath)))
                    {
                        IInterval hitInterval = e.HitRecord.HitTimelineObject as IInterval;
                        m_resizer = new Resizer(Side.Left, ScaleMode.InPlace, hitInterval.Start, Owner);
                        isResizing = true;
                    }
                    break;

                case HitType.IntervalResizeRight:
                    if (Owner.IsEditable(hitPath) && (
                            Owner.Selection.SelectionContains(hitPath) ||
                            Owner.Select<IEvent>(hitPath)))
                    {
                        IInterval hitInterval = e.HitRecord.HitTimelineObject as IInterval;
                        m_resizer = new Resizer(Side.Right, ScaleMode.InPlace, hitInterval.Start + hitInterval.Length, Owner);
                        isResizing = true;
                    }
                    break;

                case HitType.Custom:
                    {
                        HitRecordObject hitManipulator = e.HitRecord.HitObject as HitRecordObject;
                        if (hitManipulator != null)
                        {
                            if (hitManipulator.Side == Side.Left)
                                m_resizer = new Resizer(Side.Left, ScaleMode.TimePeriod, m_worldMin, Owner);
                            else
                                m_resizer = new Resizer(Side.Right, ScaleMode.TimePeriod, m_worldMax, Owner);
                            isResizing = true;
                        }
                    }
                    break;
                
                default:
                    m_resizer = null;
                    break;
            }

            Owner.IsResizingSelection = isResizing; //legacy. obsolete.
        }

        private void owner_MouseMovePicked(object sender, HitEventArgs e)
        {
            if (e.MouseEvent.Button == MouseButtons.None)
            {
                HitRecord hitRecord = e.HitRecord;
                TimelinePath hitPath = hitRecord.HitPath;

                switch (hitRecord.Type)
                {
                    case HitType.IntervalResizeLeft:
                    case HitType.IntervalResizeRight:
                        if (Owner.IsEditable(hitPath))
                            Owner.Cursor = Cursors.SizeWE;
                        break;

                        //one of the scale manipulator's handles?
                    case HitType.Custom:
                        if (hitRecord.HitObject == m_leftHitObject ||
                            hitRecord.HitObject == m_rightHitObject)
                            Owner.Cursor = Cursors.SizeWE;
                        break;

                    default:
                        break;
                }
            }
        }

        private void owner_Picking(object sender, HitEventArgs e)
        {
            if (m_visibleManipulator)
            {
                Rectangle clipRectangle = Owner.VisibleClientRectangle;

                if (m_leftHandle.IntersectsWith(clipRectangle) &&
                    e.PickRectangle.IntersectsWith(m_leftHandle))
                {
                    e.HitRecord = new HitRecord(HitType.Custom, m_leftHitObject);
                }
                else if (m_rightHandle.IntersectsWith(clipRectangle) &&
                    e.PickRectangle.IntersectsWith(m_rightHandle))
                {
                    e.HitRecord = new HitRecord(HitType.Custom, m_rightHitObject);
                }
            }
        }

        private void owner_MouseUp(object sender, MouseEventArgs e)
        {
            if (IsScaling)
            {
                m_resizer.ResizeSelection();
                m_resizer = null; //so that Invalidate doesn't cause ghosts to be drawn
                Owner.Invalidate();
            }

            m_resizer = null;
        }

        // An object to be returned in a HitRecord when the handles of the scale manipulator are hit.
        private class HitRecordObject
        {
            public HitRecordObject(Side side)
            {
                Side = side;
            }

            public readonly Side Side;

            // for tooltip text
            public override string ToString()
            {
                return "drag left or right handles to scale current selection";
            }
        }

        private bool m_visibleManipulator; //are the manipulator bar and two handles visible?
        private float m_worldMin, m_worldMax; //minimum and maximum world coordinates of selection set from last paint
        private readonly HitRecordObject m_leftHitObject = new HitRecordObject(Side.Left);//allocated once so tooltips behave
        private readonly HitRecordObject m_rightHitObject = new HitRecordObject(Side.Right);//allocated once so tooltips behave
        private Resizer m_resizer; //if not null, then we are in the middle of resizing a selection
        private RectangleF m_leftHandle; //Area of the left manipulator handle, in view coordinates.
        private RectangleF m_rightHandle; //Area of the right manipulator handle, in view coordinates.

        private static int s_handleHeight = 24;
        private static Color s_color = Color.Black;
    }
}
