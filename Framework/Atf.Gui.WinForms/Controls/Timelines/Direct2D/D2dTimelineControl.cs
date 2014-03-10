//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.VectorMath;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D
{
    /// <summary>
    /// Control to display a timeline. In a multiple document application, there is one of these
    /// per 'tab' or document.</summary>
    public class D2dTimelineControl : CanvasControl, IPerformanceTarget
    {
        /// <summary>
        /// Constructor that uses a default timeline renderer, default timeline constraints,
        /// and default manipulators</summary>
        /// <param name="timelineDocument">The timeline document to be edited</param>
        /// <remarks>Uses the default draw style</remarks>
        public D2dTimelineControl(ITimelineDocument timelineDocument)
            : this(timelineDocument, null)
        {
        }

        /// <summary>
        /// Constructor that uses a given timeline renderer, default timeline constraints and default manipulators</summary>
        /// <param name="timelineDocument">The timeline document to be edited</param>
        /// <param name="timelineRenderer">Timeline renderer, to be used only by this TimelineControl</param>
        public D2dTimelineControl(ITimelineDocument timelineDocument, D2dTimelineRenderer timelineRenderer)
            : this(timelineDocument, timelineRenderer, new DefaultTimelineConstraints())
        {
        }

        /// <summary>
        /// Constructor that uses given timeline renderer and timeline constraints, and default manipulators</summary>
        /// <param name="timelineDocument">The timeline document to be edited</param>
        /// <param name="timelineRenderer">Timeline renderer, to be used only by this TimelineControl</param>
        /// <param name="timelineConstraints">Timeline constraints</param>
        public D2dTimelineControl(
            ITimelineDocument timelineDocument,
            D2dTimelineRenderer timelineRenderer,
            TimelineConstraints timelineConstraints)
            : this(timelineDocument, timelineRenderer, timelineConstraints, true)
        {
        }

        /// <summary>
        /// Constructor where all important options are supplied by the caller</summary>
        /// <param name="timelineDocument">The timeline document to be edited</param>
        /// <param name="timelineRenderer">Timeline renderer, to be used only by this TimelineControl.
        /// If null, then a D2dDefaultTimelineRenderer is created.</param>
        /// <param name="timelineConstraints">Timeline constraints</param>
        /// <param name="createDefaultManipulators">Whether to create default manipulators or not</param>
        public D2dTimelineControl(
            ITimelineDocument timelineDocument,
            D2dTimelineRenderer timelineRenderer,
            TimelineConstraints timelineConstraints,
            bool createDefaultManipulators)
        {
            SetStyle(
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque |
                ControlStyles.UserPaint, true);

            // D2dHwndGraphics needs to be resized on size changed. Look at OnResize.
            m_d2dGraphics = D2dFactory.CreateD2dHwndGraphics(Handle);

            base.SetZoomRange(0.1f, 50f, 10f, 100f);

            base.XZoom = 40f;
            base.YZoom = 40f;

            base.AllowDrop = (timelineDocument != null);
            base.AutoScroll = true;

            if (timelineRenderer == null)
            {
                timelineRenderer = new D2dDefaultTimelineRenderer();
                timelineRenderer.Init(D2dGraphics);
            }
            m_timelineRenderer = timelineRenderer;

            timelineRenderer.Invalidated += timelineRenderer_Invalidated;

            m_timelineConstraints = timelineConstraints;

            TimelineDocument = timelineDocument;

            m_toolTip = new ToolTip();
            m_toolTip.AutoPopDelay = m_toolTipDuration;
            m_toolTip.InitialDelay = 100; //Seems to be ignored because we show the tool tip in OnMouseHover
            // which is governed by SystemInformation.MouseHoverTime.
            m_toolTip.ReshowDelay = 100;
            m_toolTip.ShowAlways = true;

            m_allowHeaderResize = true;

            GetSnapOffset = null; //set default value for m_snapper

            if (createDefaultManipulators)
            {
                // The order here determines the order of receiving Paint events and is the reverse
                //  order of receiving picking events. For example, a custom Control that is drawn
                //  on top of everything else and that can be clicked on should come last in this
                //  list so that it is drawn last and is picked first.
                D2dSelectionManipulator selectionManipulator = new D2dSelectionManipulator(this);
                D2dMoveManipulator moveManipulator = new D2dMoveManipulator(this);
                D2dScaleManipulator scaleManipulator = new D2dScaleManipulator(this);
                D2dSplitManipulator splitManipulator = new D2dSplitManipulator(this);
                D2dSnapManipulator snapManipulator = new D2dSnapManipulator(this);
                D2dScrubberManipulator scrubberManipulator = new D2dScrubberManipulator(this);

                // Allow the snap manipulator to snap objects to the scrubber.
                snapManipulator.Scrubber = scrubberManipulator;
            }
        }

        /// <summary>
        /// Gets the Direct2D graphics device associated with this Control</summary>
        public D2dGraphics D2dGraphics
        {
            get { return m_d2dGraphics; }
        }

        /// <summary>
        /// Draws one complete 'frame' using Direct2D. Is equivalent to this Control receiving a Paint message.
        /// Raises the DrawingD2d event.</summary>
        public void DrawD2d()
        {
            OnBeginDrawD2d();
            OnDrawingD2d();
            OnEndDrawD2d();
        }

        /// <summary>
        /// Event that is raised when drawing on the Direct2D surface. Is raised after OnBeginDrawD2d,
        /// and after the timeline renderer has finished. Is raised before OnEndDrawD2d. Listeners
        /// can use the D2dGraphics property to draw.</summary>
        public event EventHandler DrawingD2d;

        /// <summary>
        /// Method called to prepare the Direct2D graphics device for drawing, including clearing the back buffer
        /// render target and initializing the Transform property of D2dGraphics</summary>
        protected virtual void OnBeginDrawD2d()
        {
            D2dGraphics.BeginDraw();
            D2dGraphics.Transform = Matrix3x2F.Identity;
            D2dGraphics.Clear(BackColor);
            D2dGraphics.AntialiasMode = D2dAntialiasMode.Aliased;
            D2dGraphics.TextAntialiasMode = D2dTextAntialiasMode.Cleartype;
        }

        /// <summary>
        /// Does the DrawingD2d drawing. Raises the DrawingD2d event.</summary>
        protected virtual void OnDrawingD2d()
        {
            Matrix transform = Transform;
            Rectangle clientRectangle = ClientRectangle;

            if (m_timeline != null)
                m_layout = m_timelineRenderer.Draw(
                    m_timeline,
                    m_selection,
                    m_activeGroup,
                    m_activeTrack,
                    transform,
                    clientRectangle);

            // Goes last to give event handlers a chance to draw on top.
            DrawingD2d.Raise(this, EventArgs.Empty);

            if (m_timeline != null)
                UpdateScrollBars(VerticalScrollBar, HorizontalScrollBar);

            // Let go of the cached result now that Paint event handlers have been called.
            m_layout = null;
        }

        /// <summary>
        /// Ends drawing the current frame to the Direct2D back buffer render target</summary>
        protected virtual void OnEndDrawD2d()
        {
            D2dGraphics.EndDraw();
        }

        /// <summary>
        /// Raises the Resize event after performing custom processing</summary>
        /// <param name="e">EventArgs that contains the event data</param>
        protected override void OnResize(EventArgs e)
        {
            // Resize backbuffer to match control's client size.
            // Resize before calling base implementation.
            m_d2dGraphics.Resize(ClientSize);

            base.OnResize(e);
        }

        /// <summary>
        /// Gets transform matrix for transforming canvas (world) coordinates to Windows
        /// client coordinates</summary>
        public override Matrix Transform
        {
            get
            {
                Matrix t = base.Transform;
                t.Translate(m_timelineRenderer.HeaderWidth, 0, MatrixOrder.Append);
                return t;
            }
        }

        /// <summary>
        /// Gets and sets whether or not to allow resizing of track headers. Defaults to true.</summary>
        public bool AllowHeaderResize
        {
            get { return m_allowHeaderResize; }
            set { m_allowHeaderResize = value; }
        }

        /// <summary>
        /// Gets the mouse drag offset in world coordinates</summary>
        /// <returns>Mouse drag offset in world coordinates</returns>
        public PointF GetDragOffset()
        {
            PointF dragOffset = GdiUtil.InverseTransformVector(Transform, base.DragDelta);
            dragOffset.X = ConstrainFrameOffset(dragOffset.X);
            return dragOffset;
        }

        /// <summary>
        /// A delegate for testing whether or not the given event should be included when snapping</summary>
        /// <param name="testEvent">Event to check</param>
        /// <param name="options">SnapOptions to get the FilterContext and other useful data</param>
        /// <returns>True iff the event should be included</returns>
        /// <remarks>Note that SnapOptions has a standard filter for testing selected items.</remarks>
        public delegate bool SnapFilter(IEvent testEvent, SnapOptions options);

        /// <summary>
        /// The options that are optionally passed to the SnapOffsetFinder delegate</summary>
        public class SnapOptions
        {
            /// <summary>
            /// Whether or not to snap to selected objects. The default is false, which means
            /// that selected objects are ignored when trying to snap.</summary>
            public bool IncludeSelected;

            /// <summary>
            /// Whether or not the scrubber (if any exists) should be tested against</summary>
            public bool IncludeScrubber = true;

            /// <summary>
            /// Whether or not to check the modifier keys. By default, Shift disables snap-to.</summary>
            public bool CheckModifierKeys = true;

            /// <summary>
            /// The delegate that tests whether or not the given event should be allowed to be snapped to.
            /// If null, then all IEvents are eligible to be snapped against.</summary>
            public SnapFilter Filter;

            /// <summary>
            /// A custom object that can be set prior to calling GetSnapOffset, so that the SnapFilter
            /// delegate can use it</summary>
            public object FilterContext;
        }

        /// <summary>
        /// A delegate for getting the offset from one of the world snap points to the closest
        /// non-selected object's edge</summary>
        /// <param name="movingPoints">The x-coordinates to snap "from", in world coordinates</param>
        /// <param name="options">The snap options to control the behavior. If null, the defaults are used.</param>
        /// <returns>The value to be added, to GetDragOffset().X, for example. Is in world coordinates.</returns>
        public delegate float SnapOffsetFinder(IEnumerable<float> movingPoints, SnapOptions options);

        /// <summary>
        /// Gets and sets the delegate for finding the offset and nearest object to snap the
        /// given points to. Is set by the SnapManipulator, for example. Never returns null.
        /// If set to null, the default implementation is used, which is to not perform
        /// any snapping so the delegate always returns 0.0f.</summary>
        public SnapOffsetFinder GetSnapOffset
        {
            get { return m_snapper; }
            set
            {
                if (value != null)
                    m_snapper = value;
                else
                    m_snapper = new SnapOffsetFinder(NoSnapOffset);
            }
        }

        /// <summary>
        /// Releases all resources used by the component</summary>
        /// <param name="disposing">Is the component disposing?</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_d2dGraphics.Dispose();

                if (m_toolTip != null)//in case Dispose() is called twice
                {
                    m_toolTip.Dispose();
                    m_toolTip = null;
                }

                TimelineDocument = null;

                m_timelineRenderer.Invalidated -= timelineRenderer_Invalidated;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the Windows client rectangle minus the area of the scrollbars and header on the left
        /// side</summary>
        public override Rectangle VisibleClientRectangle
        {
            get
            {
                Rectangle rect = base.VisibleClientRectangle;
                rect.X += m_timelineRenderer.HeaderWidth;
                rect.Width -= m_timelineRenderer.HeaderWidth;
                return rect;
            }
        }

        /// <summary>
        /// Gets and sets the timeline document being edited by this control</summary>
        public ITimelineDocument TimelineDocument
        {
            get { return m_timelineDocument; }
            set
            {
                if (m_timelineDocument != null)
                {
                    m_selection.SelectionChanged -= selection_SelectionChanged;

                    m_timelineDocument.ItemInserted -= timelineDocument_ItemInserted;
                    m_timelineDocument.ItemRemoved -= timelineDocument_ItemRemoved;
                    m_timelineDocument.ItemChanged -= timelineDocument_ItemChanged;
                    m_timelineDocument.Reloaded -= timelineDocument_Reloaded;
                }

                if (value == null)
                {
                    AllowDrop = false;
                }
                else
                {
                    AllowDrop = true;
                }

                m_timelineDocument = value;

                if (m_timelineDocument != null)
                {
                    m_timeline = m_timelineDocument.Timeline;
                    m_timelineDocument.ItemInserted += timelineDocument_ItemInserted;
                    m_timelineDocument.ItemRemoved += timelineDocument_ItemRemoved;
                    m_timelineDocument.ItemChanged += timelineDocument_ItemChanged;
                    m_timelineDocument.Reloaded += timelineDocument_Reloaded;

                    m_selection = m_timelineDocument.Cast<ISelectionContext>();
                    m_selection.SelectionChanged += selection_SelectionChanged;

                    m_transactionContext = m_timelineDocument.As<ITransactionContext>();
                }
                else
                {
                    m_timeline = null;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets the timeline</summary>
        public ITimeline Timeline
        {
            get { return TimelineDocument != null ? TimelineDocument.Timeline : null; }
        }

        /// <summary>
        /// Gets the events, tracks, and groups selected in the control</summary>
        public ISelectionContext Selection
        {
            get { return m_selection; }
        }

        /// <summary>
        /// Gets the transaction context</summary>
        public ITransactionContext TransactionContext
        {
            get { return m_transactionContext; }
        }

        /// <summary>
        /// Gets an enumeration of selected objects that also pass the IsEditable() test</summary>
        public IEnumerable<TimelinePath> EditableSelection
        {
            get
            {
                if (m_selection == null)
                    yield break;
                foreach (TimelinePath selected in m_selection.GetSelection<TimelinePath>())
                    if (IsEditable(selected))
                        yield return selected;
            }
        }

        /// <summary>
        /// Gets the control's timeline renderer</summary>
        public D2dTimelineRenderer Renderer
        {
            get { return m_timelineRenderer; }
        }

        /// <summary>
        /// Gets the track that can accept key and interval insertions from paste-like operations</summary>
        public TimelinePath TargetTrack
        {
            get { return m_activeTrack; }
        }

        /// <summary>
        /// Gets the group that can accept track insertions from paste-like operations</summary>
        public TimelinePath TargetGroup
        {
            get { return m_activeGroup; }
        }

        /// <summary>
        /// Gets and sets the mouse button used to pan the display</summary>
        public MouseButtons PanButton
        {
            get { return m_panButton; }
            set { m_panButton = value; }
        }

        /// <summary>
        /// Gets and sets the modifier key(s) used to pan the display</summary>
        public Keys PanModifierKeys
        {
            get { return m_panModifierKeys; }
            set { m_panModifierKeys = value; }
        }

        /// <summary>
        /// Gets and sets the mouse button used to zoom the display</summary>
        public MouseButtons ZoomButton
        {
            get { return m_zoomButton; }
            set { m_zoomButton = value; }
        }

        /// <summary>
        /// Gets and sets the modifier key(s) used to zoom the display</summary>
        public Keys ZoomModifierKeys
        {
            get { return m_zoomModifierKeys; }
            set { m_zoomModifierKeys = value; }
        }

        /// <summary>
        /// Gets and sets the modifier key(s) used to enter 'constrain' mode</summary>
        public Keys ConstrainModifierKeys
        {
            get { return m_constrainModifierKeys; }
            set { m_constrainModifierKeys = value; }
        }

        /// <summary>
        /// Gets and sets key used to cancel a move/resize (drag) gesture</summary>
        public Keys CancelDragKey
        {
            get { return m_cancelDragKey; }
            set { m_cancelDragKey = value; }
        }

        /// <summary>
        /// Gets whether events in the same group must be dragged together</summary>
        public bool MoveGroupEventsTogether
        {
            get { return m_moveGroupEventsTogether; }
            set { m_moveGroupEventsTogether = value; }
        }

        /// <summary>
        /// Event that is raised when an event is moved by the user</summary>
        /// <remarks>The actual move is performed on the data after the user releases the mouse button.</remarks>
        public event EventHandler<EventMovedEventArgs> EventMoved;

        /// <summary>
        /// Event that is raised when a pick or hit test is being performed. Listeners can check
        /// HitEventArgs.HitRecord. If it is null, the event has not been "handled". Assigning
        /// a new HitRecord or setting HitEventArgs.Handled to true "handles" the event so subsequent listeners
        /// won't be called, and the modified HitRecord is passed along by the MouseDownPicked or
        /// MouseMovePicked events when the Picking event was in response to those mouse events. Event handlers
        /// are called in the reverse of their subscribing order--the last listener is called first,
        /// so that the order is the opposite of the Paint event.</summary>
        public event EventHandler<HitEventArgs> Picking;

        /// <summary>
        /// Event that is raised when an object is picked due to a mouse down event. Setting the
        /// HitEventArgs.Handled to true causes subsequent listeners to not be called. Event handlers
        /// are called in the reverse of their subscribing order--the last listener is called first,
        /// so that the order is the opposite of the Paint event.</summary>
        public event EventHandler<HitEventArgs> MouseDownPicked;

        /// <summary>
        /// Event that is raised when an object is found underneath the cursor during a mouse
        /// move event. This event is raised regardless of what mouse buttons are being held down.
        /// Setting the HitEventArgs.Handled to true causes subsequent listeners to not be called.
        /// Event handlers are called in the reverse of their subscribing order--the last listener
        /// is called first, so that the order is the opposite of the Paint event.</summary>
        public event EventHandler<HitEventArgs> MouseMovePicked;

        /// <summary>
        /// The event argument for the BoundingRectUpdating event</summary>
        public class BoundingRectEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor that takes the current bounding rectangle around all events and groups
            /// and tracks, in Windows client coordinates</summary>
            /// <param name="currentClientRect">Rectangle around all events and groups and tracks</param>
            public BoundingRectEventArgs(RectangleF currentClientRect)
            {
                CurrentClientRect = currentClientRect;
                NewClientRect = currentClientRect;
            }

            /// <summary>
            /// The current bounding rectangle around all events and groups and tracks, in Windows client
            /// coordinates</summary>
            public readonly RectangleF CurrentClientRect;

            /// <summary>
            /// The new client rectangle, to be updated if necessary by the listener. This rectangle is
            /// combined with CurrentClientRect and all other listener rectangles in a Union. This means,
            /// that a listener can only expand the resulting rectangle and cannot shrink it.</summary>
            public RectangleF NewClientRect;
        }

        /// <summary>
        /// Event that is raised when the bounding box around all timeline objects is being calculated.
        /// The listener may set the NewClientRect property to add a rectangle to the union of all other
        /// rectangles. All coordinates are in Windows client coordinates.</summary>
        public event EventHandler<BoundingRectEventArgs> BoundingRectUpdating;

        /// <summary>
        /// Gets the layout of all timeline objects, in Windows client coordinates</summary>
        /// <returns>Dictionary, pairing timeline path objects with their bounding rectangles in
        /// Windows client coordinates</returns>
        public TimelineLayout GetLayout()
        {
            if (m_layout != null)
                return m_layout;

            TimelineLayout result = m_timelineRenderer.GetLayout(m_timeline, Transform, ClientRectangle);

            return result;
        }

        /// <summary>
        /// Gets a bounding rectangle, in Windows client coordinates, containing all events (intervals, keys,
        /// and markers), and groups and tracks</summary>
        public RectangleF GetBoundingRect()
        {
            return GetBoundingRect(false);
        }

        /// <summary>
        /// Gets a bounding rectangle, in Windows client coordinates, containing only selected events (intervals,
        /// keys, and markers) and groups and tracks</summary>
        public RectangleF GetSelectionBoundingRect()
        {
            return GetBoundingRect(true);
        }

        /// <summary>
        /// Pans and zooms so that all events (intervals, keys, and markers), groups and tracks are visible</summary>
        public void Frame()
        {
            Frame(GetBoundingRect());
        }

        /// <summary>
        /// Pans and zooms so that selected events (intervals, keys, and markers), groups and tracks are visible</summary>
        public void FrameSelection()
        {
            Frame(GetSelectionBoundingRect());
        }

        /// <summary>
        /// Performs a pick operation against the current timeline</summary>
        /// <param name="clientPoint">The client point, in device-independent pixels, to pick with</param>
        /// <returns>HitRecord, with the results of the pick operation</returns>
        public HitRecord Pick(PointF clientPoint)
        {
            Matrix transform = Transform;
            Rectangle clientRectangle = ClientRectangle;
            RectangleF pickRect = GetPickingRectangle(clientPoint);

            // Give picking listeners (e.g., manipulators) a first chance.
            HitEventArgs e = new HitEventArgs(null, pickRect, null);
            OnPicking(e);
            if (e.HitRecord != null)
                return e.HitRecord;

            if (m_timeline == null)
                return new HitRecord(HitType.None, null);

            // Perform the basic picking tests on ITimelineObjects.
            IList<HitRecord> hits = m_timelineRenderer.Pick(m_timeline, pickRect, transform, clientRectangle);

            if (hits.Count > 0)
            {
                // Give selected objects of the same type the highest priority. This solves
                //  the problem of trying to scale the edge of a selected interval that is
                //  adjacent to another interval.
                HitRecord topHit = hits[0];
                if (topHit.HitObject == null ||
                    m_selection.SelectionContains(topHit.HitPath))
                    return topHit;

                Type topType = topHit.HitObject.GetType();
                for (int i = 1; i < hits.Count; i++)
                {
                    HitRecord currHit = hits[i];
                    Type currHitType = null;
                    if (currHit.HitObject != null)
                        currHitType = currHit.HitObject.GetType();
                    if (topType.IsAssignableFrom(currHitType) &&
                        m_selection.SelectionContains(currHit.HitPath))
                    {
                        return currHit;
                    }
                }

                return topHit;
            }

            return new HitRecord(HitType.None, null);
        }

        /// <summary>
        /// Depending on the current modifier keys that are being held down, either the given object becomes
        /// the new selection set, it is added to the existing selection set, or it is removed from
        /// the existing selection set. The resulting selection set is filtered to include only objects of
        /// type 'T', which is one of the ITimelineObjects (groups, tracks, events).</summary>
        /// <typeparam name="T">The filtering type: group, track, or event object</typeparam>
        /// <param name="item">The object to set the selection set to or to be added or removed</param>
        /// <returns>True iff at least one ITimelineObject is still selected</returns>
        public bool Select<T>(TimelinePath item)
            where T : class, ITimelineObject
        {
            Keys modifiers = ModifierKeys;

            List<TimelinePath> filtered;
            if (KeysUtil.ClearsSelection(modifiers))
            {
                filtered = new List<TimelinePath>(1);
                filtered.Add(item);
            }
            else
            {
                filtered = new List<TimelinePath>();
                foreach (TimelinePath path in m_selection.GetSelection<TimelinePath>())
                {
                    if (path.Last is T)
                        filtered.Add(path);
                }
                if (KeysUtil.TogglesSelection(modifiers))
                {
                    if (!filtered.Remove(item))
                        filtered.Add(item);
                }
                else //adds to selection, making it the last selected item
                {
                    filtered.Remove(item);
                    filtered.Add(item);
                }
            }

            m_selection.SetRange(filtered);
            return m_selection.LastSelected != null;
        }

        /// <summary>
        /// Gets the constraints for validating potential changes to intervals and events</summary>
        public TimelineConstraints Constraints
        {
            get { return m_timelineConstraints; }
        }

        /// <summary>
        /// Constrains one world coordinate of a timeline object that might be moved or resized</summary>
        /// <param name="offset">Timeline world coordinate</param>
        /// <returns>Constrained frame offset</returns>
        /// <remarks>Default constrains offsets to integral values, forcing all move and resize
        /// operations to maintain integral start and length properties.</remarks>
        public virtual float ConstrainFrameOffset(float offset)
        {
            return (float)Math.Round(offset);
        }

        /// <summary>
        /// Creates a new IGroup that is the same object type as 'original'. Use this
        /// instead of calling ITimeline.CreateGroup().</summary>
        /// <param name="original">The original object. If null, this method
        /// attempts to create an object of the same type as 'original', 
        /// but this can fail in some client implementations.</param>
        /// <returns>New IGroup object. Does not return null.</returns>
        public IGroup Create(IGroup original)
        {
            ITimelineObjectCreator creator = original as ITimelineObjectCreator;
            if (creator != null)
                return (IGroup)creator.Create();

            return TimelineDocument.Timeline.CreateGroup();
        }

        /// <summary>
        /// Creates a new IMarker that is the same object type as 'original'. Use this
        /// instead of calling ITimeline.CreateMarker().</summary>
        /// <param name="original">The original object. If null, this method
        /// attempts to create an object of the same type as 'original', 
        /// but this can fail in some client implementations.</param>
        /// <returns>New IMarker object. Does not return null.</returns>
        public IMarker Create(IMarker original)
        {
            ITimelineObjectCreator creator = original as ITimelineObjectCreator;
            if (creator != null)
                return (IMarker)creator.Create();

            return TimelineDocument.Timeline.CreateMarker();
        }

        /// <summary>
        /// Creates a new ITrack that is the same object type as 'original'. Use this
        /// instead of calling IGroup.CreateTrack().</summary>
        /// <param name="original">The original object. If null, this method
        /// attempts to create an object of the same type as 'original', 
        /// but this can fail in some client implementations.</param>
        /// <returns>New ITrack object. Does not return null.</returns>
        public ITrack Create(ITrack original)
        {
            ITimelineObjectCreator creator = original as ITimelineObjectCreator;
            if (creator != null)
                return (ITrack)creator.Create();

            return original.Group.CreateTrack();
        }

        /// <summary>
        /// Creates a new IInterval that is the same object type as 'original'. Use this
        /// instead of calling ITrack.CreateInterval().</summary>
        /// <param name="original">The original object. If null, this method
        /// attempts to create an object of the same type as 'original', 
        /// but this can fail in some client implementations.</param>
        /// <returns>New IInterval object. Does not return null.</returns>
        public IInterval Create(IInterval original)
        {
            ITimelineObjectCreator creator = original as ITimelineObjectCreator;
            if (creator != null)
                return (IInterval)creator.Create();

            return original.Track.CreateInterval();
        }

        /// <summary>
        /// Creates a new IKey that is the same object type as 'original'. Use this
        /// instead of calling ITrack.CreateKey().</summary>
        /// <param name="original">The original object. If null, this method
        /// attempts to create an object of the same type as 'original', 
        /// but this can fail in some client implementations.</param>
        /// <returns>New IKey object. Does not return null.</returns>
        public IKey Create(IKey original)
        {
            ITimelineObjectCreator creator = original as ITimelineObjectCreator;
            if (creator != null)
                return (IKey)creator.Create();

            return original.Track.CreateKey();
        }

        /// <summary>
        /// Gets whether or not the given timeline object can be edited by the user. By default,
        /// the objects in a sub-timeline cannot be edited.</summary>
        /// <param name="path">Timeline object</param>
        /// <returns>Whether or not the given timeline object can be edited by the user</returns>
        public virtual bool IsEditable(TimelinePath path)
        {
            if (path == null)
                return false;
            ITimeline owningTimeline = GetOwningTimeline(path.Last);
            return
                owningTimeline == null ||
                owningTimeline == Timeline;
        }

        /// <summary>
        /// Gets the timeline that directly owns a given timeline object</summary>
        /// <param name="obj">Timeline object whose owner is to be determined</param>
        /// <returns>Timeline that directly owns given timeline object</returns>
        public virtual ITimeline GetOwningTimeline(ITimelineObject obj)
        {
            var interval = obj as IInterval;
            if (interval != null)
            {
                ITrack intervalTrack = interval.Track;
                if (intervalTrack != null)
                {
                    IGroup intervalGroup = intervalTrack.Group;
                    if (intervalGroup != null)
                        return intervalGroup.Timeline;
                }
                return null;
            }

            var reference = obj as ITimelineReference;
            if (reference != null)
                return reference.Parent;

            var key = obj as IKey;
            if (key != null)
            {
                ITrack keyTrack = key.Track;
                if (keyTrack != null)
                {
                    IGroup keyGroup = keyTrack.Group;
                    if (keyGroup != null)
                        return keyGroup.Timeline;
                }
                return null;
            }

            var marker = obj as IMarker;
            if (marker != null)
                return marker.Timeline;

            var track = obj as ITrack;
            if (track != null)
            {
                IGroup trackGroup = track.Group;
                if (trackGroup != null)
                    return trackGroup.Timeline;
                return null;
            }

            var group = obj as IGroup;
            if (group != null)
                return group.Timeline;

            return null;
        }

        /// <summary>
        /// Gets an enumerator that goes through all the events (intervals, keys, and markers)
        /// in the associated ITimelineDocument, including its resolved sub-documents</summary>
        public IEnumerable<TimelinePath> AllEvents
        {
            get
            {
                foreach (TimelinePath path in GetObjects<IEvent>(m_timeline))
                    yield return path;
            }
        }

        /// <summary>
        /// Gets an enumerator that goes through all the markers in the associated ITimelineDocument, including
        /// its resolved sub-documents</summary>
        public IEnumerable<TimelinePath> AllMarkers
        {
            get
            {
                foreach (TimelinePath path in GetObjects<IMarker>(m_timeline))
                    yield return path;
            }
        }

        /// <summary>
        /// Gets an enumerator that goes through all of the tracks in the associated ITimelineDocument, including
        /// its resolved sub-documents</summary>
        public IEnumerable<TimelinePath> AllTracks
        {
            get
            {
                foreach (TimelinePath path in GetObjects<ITrack>(m_timeline))
                    yield return path;
            }
        }

        /// <summary>
        /// Gets an enumerator that goes through all of the groups in the associated ITimelineDocument, including
        /// its resolved sub-documents</summary>
        public IEnumerable<TimelinePath> AllGroups
        {
            get
            {
                foreach (TimelinePath path in GetObjects<IGroup>(m_timeline))
                    yield return path;
            }
        }

        /// <summary>
        /// Gets the specified types of ITimelineObjects. Does not look at any sub-documents or check if
        /// 'timeline' is IHierarchicalTimeline.</summary>
        /// <typeparam name="T">The type of ITimelineObjects (groups, tracks, events) to get</typeparam>
        /// <param name="timeline">The one timeline to examine</param>
        /// <returns>An enumeration of the objects of type T in 'timeline'</returns>
        public static IEnumerable<T> GetObjectsInOneDocument<T>(ITimeline timeline)
            where T : ITimelineObject
        {
            if (typeof(T).IsAssignableFrom(typeof(IMarker)))
            {
                foreach (T obj in timeline.Markers)
                    yield return obj;
            }

            foreach (IGroup group in timeline.Groups)
            {
                if (typeof(T).IsAssignableFrom(typeof(IGroup)))
                    yield return (T)group;

                foreach (ITrack track in group.Tracks)
                {
                    if (typeof(T).IsAssignableFrom(typeof(ITrack)))
                        yield return (T)track;

                    if (typeof(T).IsAssignableFrom(typeof(IInterval)))
                    {
                        foreach (IInterval interval in track.Intervals)
                            yield return (T)interval;
                    }

                    if (typeof(T).IsAssignableFrom(typeof(IKey)))
                    {
                        foreach (IKey key in track.Keys)
                            yield return (T)key;
                    }
                }
            }
        }

        /// <summary>
        /// Recursively gets the specified type of ITimelineObjects of this timeline and any
        /// resolved sub-timelines</summary>
        /// <typeparam name="T">The type of ITimelineObjects (groups, tracks, events) to get</typeparam>
        /// <param name="timeline">The hierarchical timeline to begin with</param>
        /// <returns>An enumeration of ITimelineObjects of 'timeline' and any resolved sub-timelines
        /// within it, and their sub-timelines and so on</returns>
        public static IEnumerable<TimelinePath> GetObjects<T>(ITimeline timeline)
            where T : ITimelineObject
        {
            foreach (ITimelineObject item in GetObjectsInOneDocument<T>(timeline))
                yield return new TimelinePath(item);

            foreach (TimelinePath path in GetHierarchy(timeline))
            {
                IHierarchicalTimeline hierarchical = ((ITimelineReference)path.Last).Target;
                if (hierarchical != null)
                {
                    foreach (ITimelineObject item in GetObjectsInOneDocument<T>(hierarchical))
                        yield return new TimelinePath(path) + new TimelinePath(item);
                }
            }
        }

        /// <summary>
        /// Gets a depth-first hierarchy of ITimelineReference objects. The hierarchy is a tree.
        /// Any duplicate ITimelineReferences are ignored and skipped over. Equivalently, the last
        /// ITimelineReference of each path is unique.</summary>
        /// <param name="timeline">The root timeline. If this is not an IHierarchicalTimeline,
        /// an empty enumeration is returned.</param>
        /// <returns>Enumeration of paths leading to all nodes (parents and leaves) of the
        /// hierarchy of timelines, or null if the root timeline is not an IHierarchicalTimeline</returns>
        public static IEnumerable<TimelinePath> GetHierarchy(ITimeline timeline)
        {
            IHierarchicalTimeline root = timeline as IHierarchicalTimeline;
            if (root == null)
                return EmptyEnumerable<TimelinePath>.Instance;

            List<ITimelineObject> lineage = new List<ITimelineObject>();
            HashSet<IHierarchicalTimeline> all = new HashSet<IHierarchicalTimeline>();
            return GetHierarchy(root, lineage, all);
        }

        private static IEnumerable<TimelinePath> GetHierarchy(
            IHierarchicalTimeline root, List<ITimelineObject> lineage, HashSet<IHierarchicalTimeline> all)
        {
            all.Add(root);
            foreach (ITimelineReference reference in root.References)
            {
                lineage.Add(reference);//add at the end
                yield return new TimelinePath(lineage);

                IHierarchicalTimeline child = reference.Target;
                if (child != null &&
                    !all.Contains(child))
                {
                    foreach (TimelinePath childPath in GetHierarchy(child, lineage, all))
                        yield return childPath;
                }
                lineage.RemoveAt(lineage.Count - 1);//remove from the end
            }
        }

        /// <summary>
        /// Calculates the matrix for transforming objects in the last timeline of a given path to
        /// the world coordinate system</summary>
        /// <param name="path">The path of timeline references, from top-most parent to leaf</param>
        /// <returns>Matrix for transforming objects in the last timeline of the given path
        /// to the world coordinate system</returns>
        public static Matrix CalculateLocalToWorld(TimelinePath path)
        {
            Matrix localToWorld = new Matrix();
            for (int i = 0; i < path.Count; i++)
            {
                ITimelineReference reference = path[i] as ITimelineReference;
                if (reference != null)
                {
                    Matrix currentTransform = new Matrix(1, 0, 0, 1, reference.Start, 0);
                    localToWorld.Multiply(currentTransform, MatrixOrder.Prepend);
                }
            }
            return localToWorld;
        }

        /// <summary>
        /// Calculates the world minimum and maximum of this timeline object, which must be an IEvent</summary>
        /// <param name="path">The path of an IEvent to measure</param>
        /// <param name="worldStart">The minimum world position</param>
        /// <param name="worldEnd">The maximum world position</param>
        public static void CalculateRange(
            TimelinePath path,
            out float worldStart,
            out float worldEnd)
        {
            IEvent curr = (IEvent)path.Last;
            Matrix localToWorld = CalculateLocalToWorld(path);
            worldStart = GdiUtil.Transform(localToWorld, curr.Start);
            worldEnd = GdiUtil.Transform(localToWorld, curr.Start + curr.Length);
        }

        /// <summary>
        /// Calculates the world minimum and maximum of timeline objects</summary>
        /// <param name="objects">A set of timeline objects to measure</param>
        /// <param name="worldMin">The minimum world position (or float.MaxValue if objects is empty)</param>
        /// <param name="worldMax">The maximum world position (or float.MinValue if objects is empty)</param>
        /// <returns>Whether or not any IEvents were measured</returns>
        public static bool CalculateRange(
            IEnumerable<TimelinePath> objects,
            out float worldMin,
            out float worldMax)
        {
            worldMin = float.MaxValue;
            worldMax = float.MinValue;

            foreach (TimelinePath path in objects)
            {
                if (path.Last is IEvent)
                {
                    float worldStart, worldEnd;
                    CalculateRange(path, out worldStart, out worldEnd);
                    if (worldStart < worldMin)
                        worldMin = worldStart;
                    if (worldMax < worldEnd)
                        worldMax = worldEnd;
                }
            }

            return worldMax > worldMin + float.Epsilon;
        }

        /// <summary>
        /// Gets whether this Control currently owns a command that uses the mouse</summary>
        /// <remarks>This property can be useful to manipulators if they need to not act if
        /// the TimelineControl's basic navigation is being used, for example. This also saves
        /// the manipulators from having to determine what behavior the modifier keys cause in
        /// the TimelineControl.</remarks>
        public bool IsUsingMouse
        {
            get
            {
                // includes multi-selecting, scrolling, and zooming
                return IsDragging;
            }
        }

        #region Obsolete. Exists only for backwards compatibility with apps like CoreText Editor.
        /// <summary>
        /// Gets whether the user is resizing the selection</summary>
        /// <remarks>The ScaleManipulator knows about resizing objects. It updates this for now
        /// to maintain backwards compatibility.</remarks>
        //[Obsolete("The resizing / scaling functionality is in D2dScaleManipulator, by default")]
        public bool IsResizingSelection
        {
            get { return m_isResizingSelection; }
            set { m_isResizingSelection = value; }
        }

        /// <summary>
        /// Raises MoveEvent. A way of maintaining backwards compatibility. See D2dMoveManipulator.</summary>
        /// <param name="e">EventMovedEventArgs containing event data</param>
        public void MoveEvent(EventMovedEventArgs e)
        {
            OnEventMoved(e);
        }

        /// <summary>
        /// Gets or sets whether selection being moved. A way of maintaining backwards compatibility. See D2dMoveManipulator.</summary>
        public bool IsMovingSelection
        {
            get
            {
                return
                    base.DragOverThreshold &&
                    m_movingSelection;
            }
            set
            {
                m_movingSelection = value;
            }
        }

        /// <summary>
        /// Cancels drag. A way of maintaining backwards compatibility. See D2dMoveManipulator.</summary>
        public void CancelDrag()
        {
            m_movingSelection = false;
            Cursor = Cursors.Arrow;
            Invalidate();
        }

        /// <summary>
        /// Raises EventMoved. Obsolete. This functionality is in the D2dMoveManipulator by default.</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnEventMoved(EventMovedEventArgs e)
        {
            if (EventMoved != null)
                EventMoved(this, e);
        }

        #endregion

        /// <summary>
        /// Gets or sets the timeline objects that are being dragged over the timeline control
        /// from outside the control, as from the palette. Can be null.</summary>
        /// <remarks>The D2dMoveManipulator can use this for giving a preview of where dropped
        /// timeline objects will appear.</remarks>
        public IEnumerable<ITimelineObject> DragDropObjects
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the tooltip text for the given event</summary>
        /// <param name="_event">The event</param>
        /// <returns>Tooltip text for the given event</returns>
        /// <remarks>Returns null or the empty string to inhibit tooltip</remarks>
        protected virtual string GetEventTooltipText(IEvent _event)
        {
            return _event.ToString();
        }

        /// <summary>
        /// Gets the tooltip text for the given group</summary>
        /// <param name="_group">The group</param>
        /// <returns>Tooltip text for the given group</returns>
        /// <remarks>Returnd null or the empty string to inhibit tooltip</remarks>
        protected virtual string GetGroupTooltipText(IGroup _group)
        {
            return _group.ToString();
        }

        /// <summary>
        /// Gets the tooltip text for the given track</summary>
        /// <param name="_track">The track</param>
        /// <returns>Tooltip text for the given track</returns>
        /// <remarks>Returns null or the empty string to inhibit tooltip</remarks>
        protected virtual string GetTrackTooltipText(ITrack _track)
        {
            return _track.ToString();
        }

        /// <summary>
        /// Raises the MouseDownPicked event for when something has been picked due to a mouse
        /// down event</summary>
        /// <param name="e">The HitEventArgs. Set HitEventArgs.Handled to true to prevent subsequent listeners
        /// from being called.</param>
        protected virtual void OnMouseDownPicked(HitEventArgs e)
        {
            if (MouseDownPicked != null)
            {
                Delegate[] delegates = MouseDownPicked.GetInvocationList();
                for (int i = delegates.Length; --i >= 0; )
                {
                    if (e.Handled)
                        break;
                    delegates[i].DynamicInvoke(this, e);
                }
            }
        }

        /// <summary>
        /// Raises the MouseMovePicked event for when something has been "picked" (hovered over)
        /// due to a mouse move event or during a drag operation (a mouse button is down)</summary>
        /// <param name="e">The HitEventArgs. Set HitEventArgs.Handled to true to prevent subsequent listeners
        /// from being called.</param>
        protected virtual void OnMouseMovePicked(HitEventArgs e)
        {
            if (MouseMovePicked != null)
            {
                Delegate[] delegates = MouseMovePicked.GetInvocationList();
                for (int i = delegates.Length; --i >= 0; )
                {
                    if (e.Handled)
                        break;
                    delegates[i].DynamicInvoke(this, e);
                }
            }
        }

        /// <summary>
        /// Raises the Picking event for when objects are being tested for a hit or pick, either by
        /// a mouse move or a mouse down, for example</summary>
        /// <param name="e">The HitEventArgs has a null HitRecord to begin with. If a listener
        /// sets a HitRecord, this entire HitEventArgs parameter might be sent to OnMouseDownPicked
        /// or OnMouseMovePicked calls. Setting HitEventArgs.Handled to true causes subsequent listeners
        /// not to be called.</param>
        protected virtual void OnPicking(HitEventArgs e)
        {
            if (Picking != null)
            {
                Delegate[] delegates = Picking.GetInvocationList();
                for (int i = delegates.Length; --i >= 0; )
                {
                    if (e.Handled)
                        break;
                    delegates[i].DynamicInvoke(this, e);
                }
            }
        }

        /// <summary>
        /// Gets the size of the canvas, in pixels</summary>
        /// <returns>Size of the canvas, in pixels</returns>
        protected override Size GetCanvasSize()
        {
            RectangleF bounds = GetBoundingRect();
            Point offset = ScrollPosition;
            int canvasWidth = (int)Math.Ceiling(bounds.Right) - offset.X + Width / 2; // give user another half window's worth of canvas
            int canvasHeight = (int)Math.Ceiling(bounds.Bottom) - offset.Y + Height / 2;
            return new Size(canvasWidth, canvasHeight);
        }

        /// <summary>
        /// Paints the canvas using Direct2D calls. Does NOT raise the Paint event. Raises the
        /// DrawingD2d event.</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"></see> that contains the event data</param>
        /// <remarks>Listen to the DrawingD2d event instead of the Paint event.</remarks>
        protected override void OnPaint(PaintEventArgs e)
        {
            DrawD2d();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown"></see> event and performs custom actions</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            base.Focus();

            // For full support of manipulators, always get pick information.
            PointF mouseLocation = e.Location;
            HitRecord hitRecord = Pick(mouseLocation);

            // pan/zoom?
            Keys modifiers = Control.ModifierKeys;
            bool constrain = (modifiers & m_constrainModifierKeys) != 0;
            if ((e.Button == m_panButton || e.Button == MouseButtons.Middle) && (modifiers & m_panModifierKeys) != 0)
            {
                IsScrolling = true;
                Constrain = constrain;
            }
            else if (e.Button == m_zoomButton && (modifiers & m_zoomModifierKeys) != 0)
            {
                IsZooming = true;
                Constrain = constrain;
            }
            else
            {
                if (e.Button == MouseButtons.Left)
                {
                    ITimelineObject hitObject = hitRecord.HitTimelineObject;
                    ITrack hitTrack = hitObject as ITrack;
                    IGroup hitGroup = hitObject as IGroup;
                    IInterval hitInterval = hitObject as IInterval;
                    ITimelineReference hitRef = hitObject as ITimelineReference;
                    if (hitInterval != null)
                        hitTrack = hitInterval.Track;
                    IKey hitKey = hitRecord.HitObject as IKey;
                    if (hitKey != null)
                        hitTrack = hitKey.Track;
                    if (hitTrack != null)
                        hitGroup = hitTrack.Group;

                    switch (hitRecord.Type)
                    {
                        case HitType.GroupExpand:
                            if (hitGroup != null)
                                ToggleGroupExpansion(hitGroup);
                            else if (hitRef != null)
                                ToggleReferenceExpansion(hitRef);
                            Invalidate();
                            break;

                        case HitType.HeaderResize:
                            if (m_allowHeaderResize)
                            {
                                m_resizingHeader = true;
                                AutoScroll = false; //temporarily disable since we don't support scrolling the header
                            }
                            break;

                        case HitType.Group:
                        case HitType.Track:
                        case HitType.None:
                            if (ClientRectangle.Contains(e.X, e.Y))
                            {
                                IsMultiSelecting = true;
                            }
                            break;

                        default:
                            break;
                    }

                    m_activeGroup = hitGroup != null ? new TimelinePath(hitGroup) : null;
                    m_activeTrack = hitTrack != null ? new TimelinePath(hitTrack) : null;
                }
            }

            mouseLocation = e.Location;
            RectangleF pickRect = GetPickingRectangle(mouseLocation);
            OnMouseDownPicked(new HitEventArgs(hitRecord, pickRect, e));
        }

        private void InvalidateControl()
        {
            if (InvokeRequired)
            {
                // Pass the same function to BeginInvoke, but the call would come on the correct
                // thread and InvokeRequired will be false.
                // http://www.codeproject.com/csharp/begininvoke.asp
                BeginInvoke(new Action(InvalidateControl));
                return;
            }
            Invalidate();
        }

        private void TurnOffToolTip()
        {
            // The Control may have been disposed of already because the document was closed while
            //  a tooltip was active.
            if (m_toolTip == null)
                return;

            if (InvokeRequired)
            {
                // Pass the same function to BeginInvoke, but the call would come on the correct
                // thread and InvokeRequired will be false.
                // http://www.codeproject.com/csharp/begininvoke.asp
                BeginInvoke(new Action(TurnOffToolTip));
                return;
            }
            m_toolTip.SetToolTip(this, "");
            m_toolTip.Hide(this);

            // Cause this control to be invalidated, to fix a blank spot left behind by the tooltip.
            new System.Threading.Timer(
                delegate
                {
                    InvalidateControl();
                },
                m_toolTip, 20, Timeout.Infinite);
        }

        /// <summary>
        /// Raises the System.Windows.Forms.Control.MouseHover event and performs custom actions</summary>
        /// <param name="e">An event with no data</param>
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);

            if (m_lastMouseMove == null || m_lastMouseMove.Button != MouseButtons.None)
                return;

            PointF mouseLocation = m_lastMouseMove.Location;
            HitRecord hitRecord = Pick(mouseLocation);
            object hitObject = hitRecord.HitObject;

            bool alreadyDisplayingToolTip = (m_timer != null);
            bool willDisplayToolTip = (hitObject != null);

            if (alreadyDisplayingToolTip)
            {
                m_timer.Dispose();
                m_timer = null;
                Invalidate();
            }

            if (willDisplayToolTip)
            {
                //Use Show() because SetToolTip() doesn't work reliably.
                m_toolTip.Show(GetToolTipText(hitObject), this, m_lastMouseMove.X, m_lastMouseMove.Y + 20);

                //Show() can't seem to be undone. Use a timer to turn off the tooltip.
                m_timer = new System.Threading.Timer(
                    delegate
                    {
                        TurnOffToolTip();
                    },
                    m_toolTip, m_toolTipDuration, Timeout.Infinite);
            }
            else if (alreadyDisplayingToolTip)
            {
                m_toolTip.SetToolTip(this, "");
                m_toolTip.Hide(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event and performs custom actions</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // spurious mouse messages seem to be common
            if (m_lastMouseMove != null &&
                m_lastMouseMove.Location == e.Location)
                return;

            m_lastMouseMove = e;

            // The retarded way of resetting a hover. can't believe this is necessary.
            // Otherwise, our hovers would only be triggered after the first time by alt+tab'ing
            // to another app that would cause a refresh of the Control or by using the middle
            // mouse button/wheel!
            User32.TRACKMOUSEEVENT tme = new User32.TRACKMOUSEEVENT(Handle);
            User32.TrackMouseEvent(ref tme);

            // have to manually turn-off tooltips when mouse moves because we seem to have to use
            //  m_toolTip.Show() which prevents the tooltip from turning off after a delay.
            // Update: This is a big performance hit which causes the whole timeline to be redrawn.
            //TurnOffToolTip();

            PointF mouseLocation = e.Location;
            HitRecord hitRecord = Pick(mouseLocation);

            if (e.Button == MouseButtons.None)
            {
                switch (hitRecord.Type)
                {
                    case HitType.GroupMove:
                    case HitType.TrackMove:
                        Cursor = Cursors.SizeAll;
                        break;

                    case HitType.HeaderResize:
                        if (m_allowHeaderResize)
                            Cursor = Cursors.VSplit;
                        else
                            Cursor = Cursors.Default;
                        break;

                    default:
                        Cursor = Cursors.Default;
                        break;
                }
            }
            else
            {
                if (m_resizingHeader)
                    m_timelineRenderer.HeaderWidth = Math.Max(e.X, MinHeaderWidth);

                if (!base.IsMultiSelecting)
                    Invalidate();
            }

            RectangleF pickRect = GetPickingRectangle(e.Location);
            OnMouseMovePicked(new HitEventArgs(hitRecord, pickRect, e));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"></see> event and performs custom actions</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (IsMultiSelecting)
            {
                if (m_selection != null)
                {
                    List<TimelinePath> newSelection;
                    Keys modifiers = Control.ModifierKeys;

                    if (KeysUtil.ClearsSelection(modifiers))
                        newSelection = new List<TimelinePath>();
                    else
                        newSelection = new List<TimelinePath>(m_selection.GetSelection<TimelinePath>());

                    IList<HitRecord> hits =
                        m_timelineRenderer.Pick(m_timeline, SelectionRect, Transform, ClientRectangle);

                    KeysUtil.Select(newSelection, HitRecordsToEvents(hits), modifiers);

                    m_selection.Selection = newSelection.Cast<object>();
                }

                Invalidate();
            }

            Cursor = Cursors.Arrow;

            m_resizingHeader = false;
            AutoScroll = true; // in case m_resizingHeader was true.

            base.OnMouseUp(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event and performs custom actions</summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Invalidate();
            base.OnMouseWheel(e);
        }

        #region IPerformanceTarget Members

        /// <summary>
        /// Does the event whose duration is measured and then returns. Raising the
        /// EventOccurred event is optional.</summary>
        /// <example>For example, if we are monitoring the time it takes a window to redraw,
        /// this event would be the equivalent of Refresh() on a control.</example>
        void IPerformanceTarget.DoEvent()
        {
            DrawD2d();
        }

        /// <summary>
        /// Event that is raised on the object being measured to indicate that
        /// another timing sample should be taken, so that the number of events per second
        /// can be calculated.</summary>
        /// <remarks>For example, if we are monitoring the time it takes a window to redraw,
        /// this event might be raised for every Paint event on the control.</remarks>
        event EventHandler IPerformanceTarget.EventOccurred
        {
            add { DrawingD2d += value; }
            remove { DrawingD2d -= value; }
        }

        #endregion

        private void ToggleGroupExpansion(IGroup group)
        {
            bool expanded = group.Expanded;
            string command = expanded ? "Collapse Group" : "Expand Group";
            TransactionContext.DoTransaction(delegate
            {
                group.Expanded = !expanded;
            },
                command);
        }

        // idea: maybe this could be combined with ToggleGroupExpansion and work on a new IExpandable interface.
        private void ToggleReferenceExpansion(ITimelineReference reference)
        {
            bool expanded = reference.Options.Expanded;
            string command = expanded ? "Collapse Reference" : "Expand Reference";
            TransactionContext.DoTransaction(delegate
            {
                reference.Options.Expanded = !expanded;
            },
                command);
        }

        /// <summary>
        /// Gets the bounding rectangle in Windows client coordinates of either all of the timeline
        /// objects (Events, Groups, and Tracks) or just the current selection</summary>
        /// <param name="selectionOnly">Whether or not only selected objects are used. If true and
        /// nothing is selected, an empty rectangle at (0,0) is returned.</param>
        /// <returns>Either a bounding rectangle around the desired objects or (0,0)</returns>
        private RectangleF GetBoundingRect(bool selectionOnly)
        {
            TimelineLayout layout = GetLayout();

            RectangleF result = new RectangleF();

            IEnumerator<KeyValuePair<TimelinePath, RectangleF>> pairs = layout.GetEnumerator();
            if (pairs.MoveNext())
                result = pairs.Current.Value;

            while (pairs.MoveNext())
            {
                KeyValuePair<TimelinePath, RectangleF> pair = pairs.Current;
                if (!selectionOnly || m_selection.SelectionContains(pair.Key))
                {
                    if (pair.Key.Last is IInterval || pair.Key.Last is IKey)
                    {
                        // expand bounds for intervals and keys
                        result = RectangleF.Union(result, pair.Value);
                    }
                    else if (pair.Key.Last is IGroup || pair.Key.Last is ITrack || pair.Key.Last is ITimelineReference)
                    {
                        // expand only vertically for groups and tracks and sub-timelines
                        RectangleF bounds = pair.Value;
                        result.Y = Math.Min(result.Y, bounds.Y);
                        float bottom = Math.Max(result.Bottom, bounds.Bottom);
                        result.Height = bottom - result.Y;
                    }
                    else if (pair.Key.Last is IMarker)
                    {
                        // expand only horizontally for markers
                        RectangleF bounds = pair.Value;
                        result.X = Math.Min(result.X, bounds.X);
                        float right = Math.Max(result.Right, bounds.Right);
                        result.Width = right - result.Left;
                    }
                }
            }

            // Let BoundingRectUpdating listeners add themselves.
            BoundingRectEventArgs e = new BoundingRectEventArgs(result);
            if (BoundingRectUpdating != null)
            {
                foreach (Delegate listener in BoundingRectUpdating.GetInvocationList())
                {
                    listener.DynamicInvoke(this, e);
                    result = RectangleF.Union(result, e.NewClientRect);
                }
            }

            return result;
        }

        // Prepare picking rectangle with built-in tolerance.
        private RectangleF GetPickingRectangle(PointF clientPoint)
        {
            int tolerance = m_timelineRenderer.PickTolerance;
            return new RectangleF(
                clientPoint.X - tolerance,
                clientPoint.Y - tolerance,
                2 * tolerance,
                2 * tolerance);
        }

        // Gets the tooltip text for the given object. GetEventTooltipText is the customization
        // point for IEvent objects.
        private string GetToolTipText(object hitObject)
        {
            if (hitObject is IEvent)
                return GetEventTooltipText((IEvent)hitObject);
            if (hitObject is IGroup)
                return GetGroupTooltipText((IGroup)hitObject);
            if (hitObject is ITrack)
                return GetTrackTooltipText((ITrack)hitObject);

            // Don't display the string if it's just the type name.
            string tip = hitObject.ToString();
            if (tip == hitObject.GetType().ToString())
                return null;
            return tip;
        }

        // the default implementation of the GetSnapOffset property.
        private static float NoSnapOffset(IEnumerable<float> movingPoints, SnapOptions options)
        {
            return 0.0f;
        }

        private IEnumerable<TimelinePath> HitRecordsToEvents(IEnumerable<HitRecord> hits)
        {
            foreach (HitRecord hit in hits)
            {
                if (hit.HitTimelineObject is IEvent)
                    yield return hit.HitPath;
            }
        }

        private void timelineDocument_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            // run the rounding logic
            IEvent timelineEvent = e.Item as IEvent;
            if (timelineEvent != null)
            {
                timelineEvent.Start = ConstrainFrameOffset(timelineEvent.Start);
                timelineEvent.Length = ConstrainFrameOffset(timelineEvent.Length);

                IInterval interval = timelineEvent as IInterval;
                if (interval != null && interval.Track != null)
                {
                    foreach (IInterval other in interval.Track.Intervals)
                    {
                        if (other == interval)
                            continue;

                        // Try our best to fix overlaps.
                        float worldStart = interval.Start;
                        float worldLength = interval.Length;
                        if (m_timelineConstraints.IsIntervalValid(interval, ref worldStart, ref worldLength, other))
                        {
                            if (interval.Start != worldStart)
                                interval.Start = worldStart;
                            if (interval.Length != worldLength)
                                interval.Length = worldLength;
                        }
                        else
                            throw new InvalidTransactionException("Intervals must not overlap");
                    }
                }
            }

            Invalidate();
        }

        private void timelineDocument_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            if (m_activeGroup != null && m_activeGroup.Last == e.Item)
                m_activeGroup = null;
            else if (m_activeTrack != null && m_activeTrack.Last == e.Item)
                m_activeTrack = null;

            Invalidate();
        }

        private void timelineDocument_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            Invalidate();
        }

        private void timelineDocument_Reloaded(object sender, EventArgs e)
        {
            m_timeline = m_timelineDocument.Timeline;
            Invalidate();
        }

        private void selection_SelectionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void timelineRenderer_Invalidated(object sender, EventArgs e)
        {
            Invalidate();
        }

        private readonly D2dHwndGraphics m_d2dGraphics;
        private readonly D2dTimelineRenderer m_timelineRenderer;
        private readonly TimelineConstraints m_timelineConstraints;

        private ITimelineDocument m_timelineDocument;
        private ITimeline m_timeline;
        private ISelectionContext m_selection; //to do: create an EmptySelectionContext class and assign it here and then clean-up checks for null
        private ITransactionContext m_transactionContext;
        private TimelinePath m_activeTrack;
        private TimelinePath m_activeGroup;

        private MouseButtons m_panButton = MouseButtons.Left;
        private Keys m_panModifierKeys = Keys.Alt;
        private MouseButtons m_zoomButton = MouseButtons.Right;
        private Keys m_zoomModifierKeys = Keys.Alt;
        private Keys m_constrainModifierKeys = Keys.Shift;
        private Keys m_cancelDragKey = Keys.Escape;

        private ToolTip m_toolTip;
        private MouseEventArgs m_lastMouseMove;
        private int m_toolTipDuration = 3000;
        private System.Threading.Timer m_timer;

        private bool m_moveGroupEventsTogether;
        private bool m_movingSelection;
        private bool m_resizingHeader;
        private bool m_isResizingSelection;
        private TimelineLayout m_layout;

        private SnapOffsetFinder m_snapper;

        private bool m_allowHeaderResize;

        private const int MinHeaderWidth = 32;
    }
}
