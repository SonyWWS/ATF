//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Applications;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Timelines.Direct2D
{
    /// <summary>
    /// Abstract base class for timeline renderers. The class is designed to be instantiated
    /// once per TimelineControl.</summary>
    public abstract class D2dTimelineRenderer : IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        protected D2dTimelineRenderer()
            : this(SystemFonts.StatusFont)
        {
        }

        /// <summary>
        /// Constructor with font</summary>
        /// <param name="font">Font for rendering timeline text</param>
        protected D2dTimelineRenderer(Font font)
        {
            Font = font;
            m_textFormat = D2dFactory.CreateTextFormat(font);
        }

        /// <summary>
        /// Initializes this class with a graphics object. Must be called before using this class to render.</summary>
        /// <param name="graphics">The graphics object that this TimelineRenderer will permanently use</param>
        public virtual void Init(D2dGraphics graphics)
        {
            if (m_graphics != null)
                throw new InvalidOperationException("Only call Init() once");
            m_graphics = graphics;
            m_gridPen = m_graphics.CreateSolidBrush(Color.FromArgb(128, 128, 128, 128));
            m_headerBrush = m_graphics.CreateSolidBrush(SystemColors.Control);
            m_headerLineBrush = m_graphics.CreateSolidBrush(SystemColors.ControlDark);
            m_scaleLineBrush = m_graphics.CreateSolidBrush(SystemColors.ControlDark);
            m_expanderBrush = m_graphics.CreateSolidBrush(Color.DimGray);
            m_nameBrush = m_graphics.CreateSolidBrush(SystemColors.WindowText);
            m_scaleTextBrush = m_graphics.CreateSolidBrush(SystemColors.WindowText);
            m_generalSolidColorBrush = m_graphics.CreateSolidBrush(Color.Empty);
            
            m_trackTextFormat = D2dFactory.CreateTextFormat(Font);
            m_trackTextFormat.ParagraphAlignment = D2dParagraphAlignment.Far;
            m_trackTextFormat.TextAlignment = D2dTextAlignment.Trailing;
        }

        /// <summary>
        /// Gets the Direct2D graphics object that this TimelineRenderer was created with</summary>
        public D2dGraphics Graphics
        {
            get { return m_graphics; }
        }

        /// <summary>
        /// Gets or sets the D2dBrush used to draw the horizontal and vertical grid lines</summary>
        /// <remarks>The supplied object is considered to be owned by this D2dTimelineRenderer
        /// and is disposed of when it is no longer needed.</remarks>
        public D2dBrush GridBrush
        {
            get { return m_gridPen; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_gridPen.Dispose();
                m_gridPen = value;
            }
        }

        /// <summary>
        /// Gets or sets the D2dBrush used to draw the "header" rectangle on the left side of
        /// the timeline canvas</summary>
        /// <remarks>The supplied object is considered to be owned by this D2dTimelineRenderer
        /// and is disposed of when it is no longer needed.</remarks>
        public D2dBrush HeaderBrush
        {
            get { return m_headerBrush; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_headerBrush.Dispose();
                m_headerBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the D2dBrush used to draw the "header" lines on the left side of
        /// the timeline canvas</summary>
        /// <remarks>The supplied object is considered to be owned by this D2dTimelineRenderer
        /// and is disposed of when it is no longer needed.</remarks>
        public D2dBrush HeaderLineBrush
        {
            get { return m_headerLineBrush; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_headerLineBrush.Dispose();
                m_headerLineBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the D2dBrush used to draw the horizontal line that separates the scale from
        /// the top of the timeline canvas</summary>
        /// <remarks>The supplied object is considered to be owned by this D2dTimelineRenderer
        /// and is disposed of when it is no longer needed.</remarks>
        public D2dBrush ScaleLineBrush
        {
            get { return m_scaleLineBrush; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_scaleLineBrush.Dispose();
                m_scaleLineBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the D2dBrush used to draw the text of the timeline scale</summary>
        /// <remarks>The supplied object is considered to be owned by this D2dTimelineRenderer
        /// and is disposed of when it is no longer needed.</remarks>
        public D2dBrush ScaleTextBrush
        {
            get { return m_scaleTextBrush; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_scaleTextBrush.Dispose();
                m_scaleTextBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the D2dBrush used to draw the expander icon for groups and referenced timelines</summary>
        /// <remarks>The supplied object is considered to be owned by this D2dTimelineRenderer
        /// and is disposed of when it is no longer needed.</remarks>
        public D2dBrush ExpanderBrush
        {
            get { return m_expanderBrush; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_expanderBrush.Dispose();
                m_expanderBrush = value;
            }
        }

        /// <summary>
        /// Gets or sets the D2dBrush used to draw the names of groups, tracks, and timeline references</summary>
        /// <remarks>The supplied object is considered to be owned by this D2dTimelineRenderer
        /// and is disposed of when it is no longer needed.</remarks>
        public D2dBrush NameBrush
        {
            get { return m_nameBrush; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException();
                m_nameBrush.Dispose();
                m_nameBrush = value;
            }
        }

        /// <summary>
        /// Gets the D2D text format object that was built using the supplied font. Can be used when
        /// calling D2dGraphics.DrawText.</summary>
        public D2dTextFormat TextFormat
        {
            get { return m_textFormat; }
        }

        /// <summary>
        /// The System.Drawing.Font used when creating TextFormat</summary>
        public readonly Font Font;

        /// <summary>
        /// Finalizer</summary>
        ~D2dTimelineRenderer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes unmanaged resources</summary>
        /// <param name="disposing">If true, then Dispose() called this method and managed resources should
        /// be released in addition to unmanaged resources. If false, then the finalizer called this method
        /// and no managed objects should be called and only unmanaged resources should be released.</param>
        /// <remarks>Derived classes must call base.Dispose(disposing).</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    // Dispose of resources that don't have finalizers
                    m_gridPen.Dispose();
                    m_headerBrush.Dispose();
                    m_headerLineBrush.Dispose();
                    m_scaleLineBrush.Dispose();
                    m_expanderBrush.Dispose();
                    m_nameBrush.Dispose();
                    m_scaleTextBrush.Dispose();
                    m_generalSolidColorBrush.Dispose();
                    m_trackTextFormat.Dispose();
                    m_textFormat.Dispose();
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// Gets or sets the margin of group expander rectangle</summary>
        protected Padding ExpanderRectMargin
        {
            get { return m_expanderRectMargin; }
            set { m_expanderRectMargin = value; }
        }

        /// <summary>
        /// Gets or sets the size of group expander rectangle</summary>
        protected int ExpanderRectSize
        {
            get { return m_expanderRectSize; }
            set { m_expanderRectSize = value; }
        }

        /// <summary>
        /// Gets or sets track indentation</summary>
        protected int TrackIndent
        {
            get { return m_trackIndent; }
            set { m_trackIndent = value; }
        }

        /// <summary>
        /// Gets or sets minimum track size</summary>
        protected float MinimumTrackSize
        {
            get { return m_minimumTrackSize; }
            set { m_minimumTrackSize = value; }
        }

        /// <summary>
        /// Gets or sets the vertical margin, in pixels, that is displayed between track and interval edges</summary>
        public int Margin
        {
            get { return m_margin; }
            set
            {
                if (m_margin != value)
                {
                    m_margin = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the offset in x</summary>
        public float OffsetX
        {
            get { return m_offsetX; }
        }

        /// <summary>
        /// Gets or sets the width, in pixels, of group and track headers. Essentially the same as GlobalHeaderWidth.</summary>
        [DefaultValue(128)]
        public virtual int HeaderWidth
        {
            get { return GlobalHeaderWidth; }
            set
            {
                GlobalHeaderWidth = value;
                OnInvalidated(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the width, in pixels, of group and track headers. Essentially the same as HeaderWidth.</summary>
        [DefaultValue(128)]
        public static int GlobalHeaderWidth
        {
            get { return s_headerWidth; }
            set { s_headerWidth = value; }
        }

        /// <summary>
        /// Gets or sets the size of keys, in pixels</summary>
        [DefaultValue(12)]
        public static int GlobalKeySize
        {
            get { return s_keySize; }
            set { s_keySize = value; }
        }

        /// <summary>
        /// Gets or sets the height, in pixels, of the frame time scale</summary>
        [DefaultValue(24)]
        public int TimeScaleHeight
        {
            get { return m_timeScaleHeight; }
            set
            {
                if (m_timeScaleHeight != value)
                {
                    m_timeScaleHeight = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the major tick spacing, in pixels, of the time scale. Essentially the same as GlobalMajorTickSpacing.</summary>
        [DefaultValue(64)]
        public int MajorTickSpacing
        {
            get { return GlobalMajorTickSpacing; }
            set
            {
                if (GlobalMajorTickSpacing != value)
                {
                    GlobalMajorTickSpacing = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the major tick spacing, in pixels, of the time scale. Essentially the same as MajorTickSpacing.</summary>
        [DefaultValue(64)]
        public static int GlobalMajorTickSpacing
        {
            get { return s_majorTickSpacing; }
            set { s_majorTickSpacing = value; }
        }

        /// <summary>
        /// Gets or sets the minimum spacing, in graph coordinates, between ticks. For example,
        /// 1.0f would prevent ticks from being drawn for fractional graph coordinates. The
        /// default is 1.0f.</summary>
        [DefaultValue(1.0f)]
        public float MinimumGraphStep
        {
            get { return m_minimumGraphStep; }
            set
            {
                if (m_minimumGraphStep != value)
                {
                    m_minimumGraphStep = value;
                    OnInvalidated(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the tolerance, in pixels, for picking timeline objects. Essentially the same as GlobalPickTolerance.</summary>
        [DefaultValue(3)]
        public virtual int PickTolerance
        {
            get { return GlobalPickTolerance; }
            set { GlobalPickTolerance = value; }
        }

        /// <summary>
        /// Gets or sets the tolerance, in pixels, for picking timeline objects. Essentially the same as PickTolerance.</summary>
        [DefaultValue(3)]
        public static int GlobalPickTolerance
        {
            get { return s_pickTolerance; }
            set { s_pickTolerance = value; }
        }

        /// <summary>
        /// Gets or sets the track height, relative to a unit of time</summary>
        [DefaultValue(1)]
        public static float GlobalTrackHeight
        {
            get { return s_trackHeight; }
            set { s_trackHeight = value; }
        }

        /// <summary>
        /// Event that is raised when the renderer changes</summary>
        public event EventHandler Invalidated;

        /// <summary>
        /// Raises the Invalidated event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnInvalidated(EventArgs e)
        {
            EventHandler handler = Invalidated;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Gets bounding rectangles in Windows client space for all timeline objects, organized
        /// in a dictionary</summary>
        /// <param name="timeline">Timeline</param>
        /// <param name="transform">Transform taking timeline objects to display coordinates</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        /// <returns>Bounding rectangles for all timeline objects, organized in a dictionary of TimelinePath/RectangleF pairs</returns>
        public TimelineLayout GetLayout(
            ITimeline timeline,
            Matrix transform,
            Rectangle clientRectangle)
        {
            Context c = new Context(this, transform, clientRectangle, m_graphics);
            TimelineLayout layout = Layout(timeline, c);
            return layout;
        }

        /// <summary>
        /// Draws the timeline to the display</summary>
        /// <param name="timeline">Timeline</param>
        /// <param name="selection">Selected timeline objects</param>
        /// <param name="activeGroup">Currently active group, or null</param>
        /// <param name="activeTrack">Currently active track, or null</param>
        /// <param name="transform">Transform taking timeline objects to display coordinates</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        /// <param name="marginBounds">Page coordinate bounding rectangle for printing</param>
        public void Print(
            ITimeline timeline,
            ISelectionContext selection,
            TimelinePath activeGroup,
            TimelinePath activeTrack,
            Matrix transform,
            RectangleF clientRectangle,
            Rectangle marginBounds)
        {
            m_marginBounds = marginBounds;
            try
            {
                m_printing = true;
                Draw(timeline, selection, activeGroup, activeTrack, transform, clientRectangle);
            }
            finally
            {
                m_printing = false;
            }
        }

        /// <summary>
        /// Draws the timeline to the display</summary>
        /// <param name="timeline">Timeline</param>
        /// <param name="selection">Selected timeline objects</param>
        /// <param name="activeGroup">Currently active group, or null</param>
        /// <param name="activeTrack">Currently active track, or null</param>
        /// <param name="transform">Transform taking timeline objects to display coordinates</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        /// <returns>Bounding rectangles for all timeline objects, organized in a dictionary of TimelinePath/RectangleF pairs</returns>
        public virtual TimelineLayout Draw(
            ITimeline timeline,
            ISelectionContext selection,
            TimelinePath activeGroup,
            TimelinePath activeTrack,
            Matrix transform,
            RectangleF clientRectangle)
        {
            Context c = new Context(this, transform, clientRectangle, m_graphics);
            TimelineLayout layout = Layout(timeline, c);
            c.ClearRecursionData();

            try
            {
                if (m_printing)
                {
                    transform.Translate(m_marginBounds.Left, m_marginBounds.Top, MatrixOrder.Append);
                    m_graphics.PushAxisAlignedClip(m_marginBounds);
                }

                // Clear the header column.
                m_graphics.FillRectangle(new RectangleF(0, 0, HeaderWidth, c.ClientRectangle.Height), m_headerBrush);

                // Draw the main timeline and then any sub-timelines.
                DrawSubTimeline(null, timeline, false, true, selection, activeGroup, activeTrack, layout, c);
                foreach (TimelinePath path in D2dTimelineControl.GetHierarchy(timeline))
                    DrawSubTimeline(path, selection, activeGroup, activeTrack, layout, c);

                // Draw the dark vertical line on the header that separates the groups and tracks.
                m_graphics.DrawLine(new PointF(TrackIndent, m_timeScaleHeight), new PointF(TrackIndent, c.ClientRectangle.Height), m_headerLineBrush);

                // Draw the dark vertical line on the right-side of the header, separating it from the canvas.
                m_graphics.DrawLine(new PointF(HeaderWidth, m_timeScaleHeight), new PointF(HeaderWidth, c.ClientRectangle.Height), m_headerLineBrush);

                // draw scales, etc.
                if (m_printing)
                    c.Graphics.TranslateTransform(0, m_marginBounds.Top);
                DrawEventOverlay(c);

                // Draw the dark horizontal line underneath the scale.
                m_graphics.DrawLine(new PointF(0, m_timeScaleHeight), new PointF(HeaderWidth, m_timeScaleHeight), m_scaleLineBrush);
            }
            finally
            {
                if (m_printing)
                    m_graphics.PopAxisAlignedClip();
            }

            // Give the Markers in the main timeline precedence over the scale and canvas
            RectangleF clipBounds = m_graphics.ClipBounds;
            clipBounds.X = HeaderWidth;
            //var clipBounds = new RectangleF(HeaderWidth, 0, m_graphics.Size.Width, m_graphics.Size.Height);
            DrawMarkers(null, timeline, selection, c, layout, clipBounds);

            return layout;
        }

        private void DrawSubTimeline(
            TimelinePath path,
            ISelectionContext selection,
            TimelinePath activeGroup,
            TimelinePath activeTrack,
            TimelineLayout layout,
            Context c)
        {
            // Include the reference's offset into the Transform and InverseTransform properties.
            Matrix localToWorld = D2dTimelineControl.CalculateLocalToWorld(path);
            c.PushTransform(localToWorld, MatrixOrder.Prepend);

            // draw the row that has this timeline reference's name
            ITimelineReference reference = (ITimelineReference)path.Last;
            RectangleF clipBounds = m_graphics.ClipBounds;
            RectangleF bounds = layout.GetBounds(path);
            IHierarchicalTimeline timeline = reference.Target;
            if (bounds.IntersectsWith(clipBounds))
            {
                DrawMode drawMode = DrawMode.Normal;
                if (selection.SelectionContains(path))
                    drawMode |= DrawMode.Selected;
                DrawTimelineReference(reference, bounds, drawMode, c);
            }

            // draw the timeline document as if it were the main document
            if (timeline != null)
                DrawSubTimeline(path, timeline, true, reference.Options.Expanded, selection, activeGroup, activeTrack, layout, c);

            c.PopTransform();
        }

        private void DrawSubTimeline(
            TimelinePath path,
            ITimeline timeline,
            bool subTimeline,
            bool expandedTimeline,
            ISelectionContext selection,
            TimelinePath activeGroup,
            TimelinePath activeTrack,
            TimelineLayout layout,
            Context c)
        {
            //if (c.TestRecursion(timeline))
            //    return;

            if (!subTimeline)
                m_offsetX = c.Transform.OffsetX;

            RectangleF clipBounds = m_graphics.ClipBounds;

            DrawGroupsAndTracks(path, timeline, expandedTimeline, selection, c, layout, clipBounds);

            // draw markers over keys, intervals, tracks, and group
            if (subTimeline)
            {
                // Give the Markers in the main timeline precedence; draw on top of everything.
                clipBounds.X = HeaderWidth;
                clipBounds.Width -= HeaderWidth;
                DrawMarkers(path, timeline, selection, c, layout, clipBounds);
            }

            // Draw the group and track handles only if the owning timeline is expanded.
            if (expandedTimeline)
            {
                if (m_printing)
                    c.Graphics.TranslateTransform(m_marginBounds.Left, 0);
                RectangleF bounds;
                foreach (IGroup group in timeline.Groups)
                {
                    IList<ITrack> tracks = group.Tracks;
                    TimelinePath groupPath = path + group;
                    bounds = layout.GetBounds(groupPath);
                    bounds = GetGroupHandleRect(bounds, !group.Expanded);
                    RectangleF groupLabelBounds = new RectangleF(bounds.X, bounds.Y, HeaderWidth, bounds.Height);

                    // Draw group's move handle.
                    DrawMoveHandle(bounds, selection.SelectionContains(groupPath), groupPath == activeGroup);

                    // Draw expander?
                    if (tracks.Count > 1)
                    {
                        RectangleF expanderRect = GetExpanderRect(bounds);
                        m_graphics.DrawExpander(
                            expanderRect.X,
                            expanderRect.Y,
                            expanderRect.Width,
                            m_expanderBrush,
                            group.Expanded);

                        groupLabelBounds.X += TrackIndent;
                        groupLabelBounds.Width -= TrackIndent;
                    }

                    // Draw tracks' move handles?
                    if (group.Expanded || tracks.Count == 1)
                    {
                        foreach (ITrack track in tracks)
                        {
                            TimelinePath trackPath = path + track;
                            bounds = layout.GetBounds(trackPath);
                            bounds = GetTrackHandleRect(bounds);
                            DrawMoveHandle(bounds, selection.SelectionContains(trackPath), trackPath == activeTrack);
                            if (bounds.Width > 0f)
                                m_graphics.DrawText(track.Name, m_trackTextFormat, bounds, m_nameBrush);
                        }
                    }

                    // Draw group name.
                    if (groupLabelBounds.Width > 0)
                        m_graphics.DrawText(group.Name, c.TextFormat, groupLabelBounds, m_nameBrush);
                }
                if (m_printing)
                    c.Graphics.TranslateTransform(-m_marginBounds.Left, 0);
            }

            return;
        }

        private void DrawMarkers(TimelinePath path, ITimeline timeline, ISelectionContext selection,
            Context c, TimelineLayout layout, RectangleF clipBounds)
        {
            RectangleF bounds;
            foreach (IMarker marker in timeline.Markers)
            {
                TimelinePath markerPath = path + marker;
                if (!layout.TryGetBounds(markerPath, out bounds))
                    continue;
                if (bounds.IntersectsWith(clipBounds))
                {
                    DrawMode drawMode = DrawMode.Normal;
                    if (selection.SelectionContains(markerPath))
                        drawMode |= DrawMode.Selected;
                    Draw(marker, bounds, drawMode, c);
                }
            }
        }

        private void DrawGroupsAndTracks(TimelinePath path, ITimeline timeline, bool expandedTimeline,
            ISelectionContext selection, Context c, TimelineLayout layout, RectangleF clipBounds)
        {
            RectangleF canvasBounds = clipBounds; //clipBounds minus the left-side header
            canvasBounds.X = HeaderWidth;
            canvasBounds.Width -= HeaderWidth;

            RectangleF bounds;
            foreach (IGroup group in timeline.Groups)
            {
                TimelinePath groupPath = path + group;
                if (!layout.TryGetBounds(groupPath, out bounds))
                    continue;
                if (bounds.IntersectsWith(clipBounds))
                {
                    DrawMode drawMode = DrawMode.Normal;
                    if (selection.SelectionContains(groupPath))
                        drawMode |= DrawMode.Selected;
                    if (expandedTimeline)
                        Draw(group, bounds, drawMode, c);

                    IList<ITrack> tracks = group.Tracks;
                    bool collapsed = !expandedTimeline || (!group.Expanded && tracks.Count > 1);
                    foreach (ITrack track in tracks)
                    {
                        TimelinePath trackPath = path + track;
                        bounds = layout.GetBounds(trackPath);
                        if (bounds.IntersectsWith(clipBounds))
                        {
                            drawMode = DrawMode.Normal;
                            if (selection.SelectionContains(trackPath))
                                drawMode |= DrawMode.Selected;
                            if (collapsed)
                                drawMode = DrawMode.Collapsed;
                            Draw(track, bounds, drawMode, c);

                            foreach (IInterval interval in track.Intervals)
                            {
                                TimelinePath intervalPath = path + interval;
                                bounds = layout.GetBounds(intervalPath);
                                if (bounds.IntersectsWith(canvasBounds))
                                {
                                    drawMode = DrawMode.Normal;
                                    if (selection.SelectionContains(intervalPath))
                                        drawMode |= DrawMode.Selected;
                                    if (collapsed)
                                        drawMode = DrawMode.Collapsed;
                                    Draw(interval, bounds, drawMode, c);
                                }
                            }

                            foreach (IKey key in track.Keys)
                            {
                                TimelinePath keyPath = path + key;
                                bounds = layout.GetBounds(keyPath);
                                if (bounds.IntersectsWith(canvasBounds))
                                {
                                    drawMode = DrawMode.Normal;
                                    if (selection.SelectionContains(keyPath))
                                        drawMode |= DrawMode.Selected;
                                    if (collapsed)
                                        drawMode = DrawMode.Collapsed;
                                    Draw(key, bounds, drawMode, c);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws ghosted objects over timeline display during move and resize operations</summary>
        /// <param name="ghosts">Ghosted timeline objects</param>
        /// <param name="type">Type of ghosts to draw</param>
        /// <param name="transform">Transform taking timeline objects to display coordinates</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        public void DrawGhosts(
            ICollection<GhostInfo> ghosts,
            GhostType type,
            Matrix transform,
            Rectangle clientRectangle)
        {
            Context c = new Context(this, transform, clientRectangle, m_graphics);

            RectangleF oldClipBounds = c.Graphics.ClipBounds;
            RectangleF clipBounds = new RectangleF(
                oldClipBounds.X + HeaderWidth,
                oldClipBounds.Y,
                oldClipBounds.Width - HeaderWidth,
                oldClipBounds.Height);
            try
            {
                m_graphics.PushAxisAlignedClip(clipBounds);

                DrawMode drawModeFlags = 0;
                if (type == GhostType.ResizeLeft)
                    drawModeFlags |= DrawMode.ResizeLeft;
                if (type == GhostType.ResizeRight)
                    drawModeFlags |= DrawMode.ResizeRight;

                foreach (GhostInfo ghost in ghosts)
                {
                    DrawMode drawMode = ghost.Valid ? DrawMode.Ghost : DrawMode.Invalid;
                    drawMode |= drawModeFlags;

                    IInterval interval = ghost.Object as IInterval;
                    if (interval != null)
                    {
                        Draw(interval, ghost.Bounds, drawMode, c);
                        continue;
                    }

                    ITimelineReference reference = ghost.Object as ITimelineReference;
                    if (reference != null)
                    {
                        // until we get the local-to-world transform put in
                        DrawTimelineReference(reference, ghost.Bounds, drawMode, c);
                        continue;
                    }

                    IKey key = ghost.Object as IKey;
                    if (key != null)
                    {
                        Draw(key, ghost.Bounds, drawMode, c);
                        continue;
                    }

                    IMarker marker = ghost.Object as IMarker;
                    if (marker != null)
                    {
                        Draw(marker, ghost.Bounds, drawMode, c);
                        continue;
                    }

                    ITrack track = ghost.Object as ITrack;
                    if (track != null)
                    {
                        Draw(track, ghost.Bounds, drawMode, c);
                        continue;
                    }

                    IGroup group = ghost.Object as IGroup;
                    if (group != null)
                    {
                        Draw(group, ghost.Bounds, drawMode, c);
                        continue;
                    }
                }
            }
            finally
            {
                m_graphics.PopAxisAlignedClip();
            }
        }

        /// <summary>
        /// Finds the list of hits on timeline objects that intersect the given rectangle</summary>
        /// <param name="timeline">Timeline</param>
        /// <param name="pickRect">Picking rectangle</param>
        /// <param name="transform">Transform taking timeline objects to display coordinates</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        /// <returns>List of hits on timeline objects that intersect the given rectangle</returns>
        public virtual IList<HitRecord> Pick(
            ITimeline timeline,
            RectangleF pickRect,
            Matrix transform,
            Rectangle clientRectangle)
        {
            List<HitRecord> result = new List<HitRecord>();
            Context c = new Context(this, transform, clientRectangle, m_graphics);
            TimelineLayout layout = GetCachedLayout(timeline, c);

            PickSubTimeline(null, timeline, pickRect, c, layout, result);

            // If the pick is on the timescale, then there's no point in looking at sub-timelines.
            if (!(
                pickRect.Left > HeaderWidth &&
                pickRect.Right < clientRectangle.Width &&
                pickRect.Bottom > 0 &&
                pickRect.Bottom < TimeScaleHeight))
            {
                foreach (TimelinePath path in D2dTimelineControl.GetHierarchy(timeline))
                    PickSubTimeline(path, pickRect, c, layout, result);
            }

            PrioritizeHits(result);

            return result;
        }

        private void PickSubTimeline(
            TimelinePath path,
            RectangleF pickRect,
            Context c,
            TimelineLayout layout,
            List<HitRecord> result)
        {
            RectangleF clipBounds = c.Graphics.ClipBounds;
            ITimelineReference reference = (ITimelineReference)path.Last;
            RectangleF bounds;
            if (!layout.TryGetBounds(path, out bounds))
                return;

            IHierarchicalTimeline timeline = reference.Target;
            if (timeline != null && reference.Options.Expanded)
                PickSubTimeline(path, timeline, pickRect, c, layout, result);

            if (timeline != null && bounds.IntersectsWith(clipBounds))
            {
                IList<IGroup> groups = timeline.Groups;
                bool collapsible = groups.Count > 0;
                bool expanded = reference.Options.Expanded;
                bool collapsed = collapsible && !expanded;

                RectangleF handleRect = GetGroupHandleRect(bounds, collapsed);
                RectangleF expanderRect = GetExpanderRect(handleRect);

                if (collapsible && expanderRect.IntersectsWith(pickRect))
                {
                    result.Add(new HitRecord(HitType.GroupExpand, path));
                }
                else if (bounds.IntersectsWith(pickRect))
                {
                    result.Add(new HitRecord(HitType.Key, path));
                }
            }
            else if (bounds.IntersectsWith(pickRect))
            {
                result.Add(new HitRecord(HitType.Key, path));
            }
        }

        private void PickSubTimeline(
            TimelinePath root,
            ITimeline timeline,
            RectangleF pickRect,
            Context c,
            TimelineLayout layout,
            List<HitRecord> result)
        {
            if (timeline == null)
                return;

            RectangleF clipBounds = c.Graphics.ClipBounds;
            RectangleF clientRectangle = c.ClientRectangle;
            RectangleF bounds;
            HitType hitType;

            foreach (IMarker marker in timeline.Markers)
            {
                if (!layout.TryGetBounds(root + marker, out bounds))
                    continue;
                if (bounds.IntersectsWith(clipBounds) && pickRect.Right >= HeaderWidth)
                {
                    hitType = Pick(marker, bounds, pickRect, c);
                    if (hitType != HitType.None)
                        result.Add(new HitRecord(hitType, root + marker));
                }
            }

            // If the pick is on the timescale, then let's stop here.
            if (pickRect.Left > HeaderWidth &&
                pickRect.Right < clientRectangle.Width &&
                pickRect.Bottom > 0 &&
                pickRect.Bottom < TimeScaleHeight)
            {
                if ((result.Count == 0) && (pickRect.Height <= 2 * PickTolerance) && (pickRect.Width <= 2 * PickTolerance))
                    result.Add(new HitRecord(HitType.TimeScale, null));
                return;
            }

            IList<IGroup> groups = timeline.Groups;
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                IGroup group = groups[i];
                if (!layout.ContainsPath(root + group))
                    continue;
                IList<ITrack> tracks = group.Tracks;
                bool expanded = group.Expanded;
                bool collapsible = tracks.Count > 1;
                bool collapsed = collapsible && !expanded;
                if (!collapsed)
                {
                    for (int j = tracks.Count - 1; j >= 0; j--)
                    {
                        ITrack track = tracks[j];
                        IList<IKey> keys = track.Keys;
                        for (int k = keys.Count - 1; k >= 0; k--)
                        {
                            IKey key = keys[k];
                            if (!layout.TryGetBounds(root + key, out bounds))
                                continue;
                            if (bounds.IntersectsWith(clipBounds) && pickRect.Right >= HeaderWidth)
                            {
                                hitType = Pick(key, bounds, pickRect, c);
                                if (hitType != HitType.None)
                                    result.Add(new HitRecord(hitType, root + key));
                            }
                        }

                        IList<IInterval> intervals = track.Intervals;
                        for (int k = intervals.Count - 1; k >= 0; k--)
                        {
                            IInterval interval = intervals[k];
                            if (!layout.TryGetBounds(root + interval, out bounds))
                                continue;
                            if (bounds.IntersectsWith(clipBounds) && pickRect.Right >= HeaderWidth)
                            {
                                hitType = Pick(interval, bounds, pickRect, c);
                                if (hitType != HitType.None)
                                    result.Add(new HitRecord(hitType, root + interval));
                            }
                        }

                        if (!layout.TryGetBounds(root + track, out bounds))
                            continue;
                        if (bounds.IntersectsWith(clipBounds))
                        {
                            RectangleF handleRect = GetTrackHandleRect(bounds);
                            if (handleRect.IntersectsWith(pickRect))
                            {
                                result.Add(new HitRecord(HitType.TrackMove, root + track));
                            }
                            else if (bounds.IntersectsWith(pickRect))
                            {
                                result.Add(new HitRecord(HitType.Track, root + track));
                            }
                        }
                    }
                }

                if (!layout.TryGetBounds(root + group, out bounds))
                    continue;
                if (bounds.IntersectsWith(clipBounds))
                {
                    RectangleF handleRect = GetGroupHandleRect(bounds, collapsed);
                    RectangleF expanderRect = GetExpanderRect(handleRect);

                    if (collapsible && expanderRect.IntersectsWith(pickRect))
                    {
                        result.Add(new HitRecord(HitType.GroupExpand, root + group));
                    }
                    else if (handleRect.IntersectsWith(pickRect))
                    {
                        result.Add(new HitRecord(HitType.GroupMove, root + group));
                    }
                    else if (bounds.IntersectsWith(pickRect))
                    {
                        result.Add(new HitRecord(HitType.Group, root + group));
                    }
                }
            }

            if (pickRect.Left < HeaderWidth && HeaderWidth < pickRect.Right)
            {
                result.Add(new HitRecord(HitType.HeaderResize, null));
            }
        }

        /// <summary>
        /// Sorts multiple picked items by priority, with the most important being first</summary>
        /// <param name="hits">Zero or more unsorted and unfiltered hit records to be sorted/filtered</param>
        protected virtual void PrioritizeHits(List<HitRecord> hits)
        {
            if (hits.Count <= 1)
                return;

            hits.Sort(CompareHits);
        }

        /// <summary>
        /// Draws a group</summary>
        /// <param name="group">Group</param>
        /// <param name="bounds">Bounding rectangle, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected abstract void Draw(IGroup group, RectangleF bounds, DrawMode drawMode, Context c);

        /// <summary>
        /// Draws a track</summary>
        /// <param name="track">Track</param>
        /// <param name="bounds">Bounding rectangle, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected abstract void Draw(ITrack track, RectangleF bounds, DrawMode drawMode, Context c);

        /// <summary>
        /// Draws an interval</summary>
        /// <param name="interval">Interval</param>
        /// <param name="bounds">Bounding rectangle, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected abstract void Draw(IInterval interval, RectangleF bounds, DrawMode drawMode, Context c);

        /// <summary>
        /// Draws a key</summary>
        /// <param name="key">Key</param>
        /// <param name="bounds">Bounding rectangle, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected abstract void Draw(IKey key, RectangleF bounds, DrawMode drawMode, Context c);

        /// <summary>
        /// Draws a marker</summary>
        /// <param name="marker">Marker</param>
        /// <param name="bounds">Bounding rectangle, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected abstract void Draw(IMarker marker, RectangleF bounds, DrawMode drawMode, Context c);

        /// <summary>
        /// Draws a timeline reference</summary>
        /// <param name="reference">The timeline reference object that may or may not be resolved</param>
        /// <param name="bounds">The bounding rectangle, in screen space, that was created during the layout phase</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected virtual void DrawTimelineReference(ITimelineReference reference, RectangleF bounds, DrawMode drawMode, Context c)
        {
            DrawReferenceBackground(reference, bounds, drawMode, c);

            RectangleF labelBounds = new RectangleF(bounds.X, bounds.Y, HeaderWidth, bounds.Height);
            IHierarchicalTimeline timeline = reference.Target;
            IList<IGroup> groups;
            bool expanded = false;
            bool collapsible = false;
            bool collapsed = false;
            if (timeline != null)
            {
                groups = timeline.Groups;
                expanded = reference.Options.Expanded;
                collapsible = groups.Count > 0;
                collapsed = collapsible && !expanded;
            }

            if (collapsible)
            {
                labelBounds.X += TrackIndent;
                labelBounds.Width -= TrackIndent;
            }

            DrawReferenceLabel(reference, labelBounds, drawMode, c);

            // Draw an origin point to show how much the referenced timeline has been offset.
            RectangleF originRect = bounds;
            float screenStart = GdiUtil.Transform(c.Transform, 0) + bounds.X;
            originRect.X = screenStart - 4;
            originRect.Width = 8;
            originRect.Height = 16;
            if (originRect.X + originRect.Width >= HeaderWidth &&
                originRect.X <= c.ClientRectangle.Width)
            {
                DrawReferenceOrigin(reference, originRect, drawMode, c);
            }

            if (timeline != null)
            {
                bounds = GetGroupHandleRect(bounds, collapsed);

                if (collapsible)
                {
                    RectangleF expanderRect = GetExpanderRect(bounds);
                    m_graphics.DrawExpander(
                        expanderRect.X,
                        expanderRect.Y,
                        expanderRect.Width,
                        m_expanderBrush,
                        expanded);
                }
            }
        }

        /// <summary>
        /// Draws the background of a timeline reference</summary>
        /// <param name="reference">The timeline reference object that may or may not be resolved</param>
        /// <param name="bounds">The bounding rectangle, in screen space, that was created during the layout phase</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected virtual void DrawReferenceBackground(ITimelineReference reference, RectangleF bounds, DrawMode drawMode, Context c)
        {
            if (drawMode == DrawMode.Normal)
                m_generalSolidColorBrush.Color = reference.Color;
            else
                m_generalSolidColorBrush.Color = Color.LightCoral;
            c.Graphics.FillRectangle(bounds, m_generalSolidColorBrush);
        }

        /// <summary>
        /// Draws the text label for the timeline reference</summary>
        /// <param name="reference">The timeline reference object that may or may not be resolved</param>
        /// <param name="labelBounds">The bounding rectangle for the label, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected virtual void DrawReferenceLabel(ITimelineReference reference, RectangleF labelBounds, DrawMode drawMode, Context c)
        {
            c.Graphics.DrawText(reference.Name, c.TextFormat, labelBounds, m_nameBrush);
        }

        /// <summary>
        /// Draws the origin symbol for the timeline reference that indicates the zero-point for that timeline</summary>
        /// <param name="reference">The timeline reference object that may or may not be resolved</param>
        /// <param name="originRect">The suggested bounding rectangle for the origin symbol, in screen space</param>
        /// <param name="drawMode">Drawing mode</param>
        /// <param name="c">Drawing context</param>
        protected virtual void DrawReferenceOrigin(ITimelineReference reference, RectangleF originRect, DrawMode drawMode, Context c)
        {
            Color originColor;
            if (drawMode == DrawMode.Normal)
                originColor = Color.Black;
            else
                originColor = Color.Gray;
            c.Graphics.DrawLines(new[] {
                    new PointF(originRect.X + originRect.Width * 0.5f, originRect.Y),
                    new PointF(originRect.X, originRect.Y + originRect.Height * 0.5f),
                    new PointF(originRect.X + originRect.Width * 0.5f, originRect.Bottom),
                    new PointF(originRect.Right, originRect.Y + originRect.Height * 0.5f),
                    new PointF(originRect.X + originRect.Width * 0.5f, originRect.Y) },
                originColor, 1.0f);
        }

        /// <summary>
        /// Draws the event overlay</summary>
        /// <param name="c">Drawing context</param>
        /// <remarks>Draws a horizontal frame scale, and vertical grid lines at major spacings</remarks>
        protected virtual void DrawEventOverlay(Context c)
        {
            m_generalSolidColorBrush.Color = Color.White;
            m_graphics.FillRectangle(new RectangleF(HeaderWidth, 0, c.ClientRectangle.Width, m_timeScaleHeight), m_generalSolidColorBrush);
            RectangleF scaleBounds = c.ClientRectangle;
            scaleBounds.X += HeaderWidth;
            scaleBounds.Width -= HeaderWidth;
            scaleBounds = GdiUtil.InverseTransform(c.Transform, scaleBounds);

            c.Graphics.DrawVerticalScaleGrid(
                c.Transform,
                scaleBounds,
                MajorTickSpacing,
                m_gridPen);

            c.Graphics.DrawHorizontalScale(
                c.Transform,
                scaleBounds,
                true,
                MajorTickSpacing,
                m_minimumGraphStep,
                m_gridPen,
                m_textFormat,
                m_scaleTextBrush);
        }

        /// <summary>
        /// Draws a rectangular move handle for groups and tracks</summary>
        /// <param name="r">Bounding rectangle of handle</param>
        /// <param name="selected">Whether group or track is selected</param>
        /// <param name="active">Whether group or track is active</param>
        protected virtual void DrawMoveHandle(RectangleF r, bool selected, bool active)
        {
            Color handleColor = Color.LightGray;
            if (active)
                handleColor = Color.DarkGray;
            if (selected)
                handleColor = Color.Tomato;

            m_graphics.FillRectangle(r, handleColor);
            m_graphics.DrawLine(r.Left, r.Bottom, r.Right, r.Bottom, Color.DarkGray, 1.0f, null);
            m_graphics.DrawLine(r.Right, r.Bottom, r.Right, r.Top, Color.DarkGray, 1.0f, null);
            m_graphics.DrawLine(r.Left, r.Bottom, r.Left, r.Top, Color.WhiteSmoke, 1.0f, null);
            m_graphics.DrawLine(r.Left, r.Top, r.Right, r.Top, Color.WhiteSmoke, 1.0f, null);
        }

        /// <summary>
        /// Finds hits on an interval, given a picking rectangle</summary>
        /// <param name="interval">Interval</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="pickRect">Picking rectangle</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Type of hit</returns>
        protected virtual HitType Pick(IInterval interval, RectangleF bounds, RectangleF pickRect, Context c)
        {
            if (!bounds.IntersectsWith(pickRect))
                return HitType.None;

            if (pickRect.Left <= bounds.Left && pickRect.Right >= bounds.Left)
                return HitType.IntervalResizeLeft;

            if (pickRect.Left <= bounds.Right && pickRect.Right >= bounds.Right)
                return HitType.IntervalResizeRight;

            return HitType.Interval;
        }

        /// <summary>
        /// Finds hits on a key, given a picking rectangle</summary>
        /// <param name="key">Key</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="pickRect">Picking rectangle</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Type of hit</returns>
        protected virtual HitType Pick(IKey key, RectangleF bounds, RectangleF pickRect, Context c)
        {
            return bounds.IntersectsWith(pickRect) ? HitType.Key : HitType.None;
        }

        /// <summary>
        /// Finds hits on a marker, given a picking rectangle</summary>
        /// <param name="marker">Marker</param>
        /// <param name="bounds">Bounding rectangle, computed during layout phase</param>
        /// <param name="pickRect">Picking rectangle</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Type of hit</returns>
        protected virtual HitType Pick(IMarker marker, RectangleF bounds, RectangleF pickRect, Context c)
        {
            return bounds.IntersectsWith(pickRect) ? HitType.Marker : HitType.None;
        }

        /// <summary>
        /// Gets the bounding rectangle for an interval, in timeline coordinates</summary>
        /// <param name="interval">Interval</param>
        /// <param name="trackTop">Top of track holding interval, in timeline coordinates</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Bounding rectangle for the interval, in timeline coordinates</returns>
        protected abstract RectangleF GetBounds(IInterval interval, float trackTop, Context c);

        /// <summary>
        /// Gets the bounding rectangle for a key, in timeline coordinates</summary>
        /// <param name="key">Key</param>
        /// <param name="trackTop">Top of track holding key, in timeline coordinates</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Bounding rectangle for the key, in timeline coordinates</returns>
        protected abstract RectangleF GetBounds(IKey key, float trackTop, Context c);

        /// <summary>
        /// Gets the bounding rectangle for a marker, in timeline coordinates</summary>
        /// <param name="marker">Marker</param>
        /// <param name="c">Drawing context</param>
        /// <returns>Bounding rectangle for the marker, in timeline coordinates</returns>
        protected abstract RectangleF GetBounds(IMarker marker, Context c);

        /// <summary>
        /// Gets the bounding rectangle for an interval, in timeline coordinates</summary>
        /// <param name="interval">Interval</param>
        /// <param name="trackTop">Top of track holding interval, in timeline coordinates</param>
        /// <param name="transform">Transform, taking timeline coordinates to display</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        /// <returns>Bounding rectangle for the interval, in timeline coordinates</returns>
        public RectangleF GetBounds(IInterval interval, float trackTop, Matrix transform,
            RectangleF clientRectangle)
        {
            var c = new Context(this, transform, clientRectangle, m_graphics);
            return GetBounds(interval, trackTop, c);
        }

        /// <summary>
        /// Gets the bounding rectangle for a key, in timeline coordinates</summary>
        /// <param name="key">Key</param>
        /// <param name="trackTop">Top of track holding key, in timeline coordinates</param>
        /// <param name="transform">Transform, taking timeline coordinates to display</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        /// <returns>Bounding rectangle for the key, in timeline coordinates</returns>
        public RectangleF GetBounds(IKey key, float trackTop, Matrix transform,
            RectangleF clientRectangle)
        {
            var c = new Context(this, transform, clientRectangle, m_graphics);
            return GetBounds(key, trackTop, c);
        }

        /// <summary>
        /// Gets the bounding rectangle for a marker, in timeline coordinates</summary>
        /// <param name="marker">Marker</param>
        /// <param name="transform">Transform, taking timeline coordinates to display</param>
        /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
        /// <returns>Bounding rectangle for the marker, in timeline coordinates</returns>
        public RectangleF GetBounds(IMarker marker, Matrix transform, RectangleF clientRectangle)
        {
            var c = new Context(this, transform, clientRectangle, m_graphics);
            return GetBounds(marker, c);
        }

        /// <summary>
        /// Calculates and returns the rectangles of the objects in Windows client coordinates.
        /// Depending on collapse/expanded states, not every ITimelineObject may be in the dictionary.
        /// Consider calling GetCachedLayout() for picking purposes.</summary>
        /// <param name="timeline">The timeline to be laid out</param>
        /// <param name="c">The editing context</param>
        /// <returns>A dictionary mapping every timeline object in the timeline to its bounding box</returns>
        protected TimelineLayout Layout(ITimeline timeline, Context c)
        {
            TimelineLayout result = new TimelineLayout();
            SizeF pixelSize = c.PixelSize;
            float documentTop = pixelSize.Height * m_timeScaleHeight;

            LayoutSubTimeline(null, timeline, ref documentTop, true, c, result);

            foreach (TimelinePath path in D2dTimelineControl.GetHierarchy(timeline))
                LayoutSubTimeline(path, ref documentTop, c, result);

            m_cachedLayout.First = timeline;
            m_cachedLayout.Second = result;

            return result;
        }

        /// <summary>Gets the last layout dictionary calculated by a call to Layout. Use for picking,
        /// for example. Creates the layout dictionary if necessary.</summary>
        /// <param name="timeline">Timeline</param>
        /// <param name="c">The editing context</param>
        /// <returns>Last layout dictionary</returns>
        protected TimelineLayout GetCachedLayout(ITimeline timeline, Context c)
        {
            if (m_cachedLayout.First == timeline)
                return m_cachedLayout.Second;
            return Layout(timeline, c);
        }

        // documentTop is in timeline coordinates
        private void LayoutSubTimeline(
            TimelinePath path, ref float documentTop, Context c,
            TimelineLayout result)
        {
            SizeF pixelSize = c.PixelSize;
            float margin = pixelSize.Height * m_margin;

            // Limit Markers, etc., to being drawn within the owning timeline.
            RectangleF originalBounds = c.Bounds;
            c.Bounds.Y = documentTop;

            // Add a large bounding box for the whole TimelineReference row.
            float docRowHeight = Math.Max(margin * 2, MinimumTrackSize);
            RectangleF refBounds = new RectangleF(c.Bounds.X, documentTop, c.Bounds.Width, docRowHeight);
            documentTop += docRowHeight;

            // The whole timeline, with all groups and tracks laid out, and offset by the reference.Start.
            ITimelineReference reference = (ITimelineReference)path.Last;
            TimelineReferenceOptions options = reference.Options;
            IHierarchicalTimeline resolved = reference.Target;
            if (resolved != null)
            {
                Matrix localToWorld = D2dTimelineControl.CalculateLocalToWorld(path);
                c.PushTransform(localToWorld, MatrixOrder.Prepend);

                LayoutSubTimeline(path, resolved, ref documentTop, options.Expanded, c, result);

                c.PopTransform();
            }

            // Now that we know the height of the tallest group, we can update the collapsed rectangle.
            if (!options.Expanded)
            {
                docRowHeight = Math.Max(docRowHeight, documentTop - refBounds.Y);
                refBounds.Height = docRowHeight;
            }
            refBounds = GdiUtil.Transform(c.Transform, refBounds);
            result.Add(path, refBounds);

            // Restore the bounds so that the horizontal scale (tick marks & numbers) is correct.
            c.Bounds = originalBounds;
        }

        // documentTop is in timeline coordinates
        private void LayoutSubTimeline(
            TimelinePath path,
            ITimeline timeline, ref float documentTop, bool expandedTimeline, Context c,
            TimelineLayout result)
        {
            //if (c.TestRecursion(timeline))
            //    return;

            RectangleF bounds;
            SizeF pixelSize = c.PixelSize;
            float margin = pixelSize.Height * m_margin;

            float groupTop = documentTop;
            float documentBottom = groupTop;

            foreach (IGroup group in timeline.Groups)
            {
                bool expanded = expandedTimeline && group.Expanded;

                float groupBottom = groupTop;
                float trackTop = groupTop;
                foreach (ITrack track in group.Tracks)
                {
                    float eventTop = trackTop + margin;
                    float trackBottom = eventTop;

                    foreach (IInterval interval in track.Intervals)
                    {
                        bounds = GetBounds(interval, eventTop, c);
                        trackBottom = Math.Max(trackBottom, bounds.Bottom);
                        bounds = GdiUtil.Transform(c.Transform, bounds);
                        // add it, even if 'expandedTimeline' is false, to get the shadow effect
                        result.Add(path + interval, bounds);
                    }

                    foreach (IKey key in track.Keys)
                    {
                        bounds = GetBounds(key, eventTop, c);
                        trackBottom = Math.Max(trackBottom, bounds.Bottom);
                        bounds = GdiUtil.Transform(c.Transform, bounds);
                        // add it, even if 'expandedTimeline' is false, to get the shadow effect
                        result.Add(path + key, bounds);
                    }

                    trackBottom += margin;
                    trackBottom = Math.Max(trackBottom, trackTop + MinimumTrackSize); // need height for track, even if it's empty

                    bounds = new RectangleF(c.Bounds.X, trackTop, c.Bounds.Width, trackBottom - trackTop);
                    bounds = GdiUtil.Transform(c.Transform, bounds);
                    bounds.X = c.ClientRectangle.X;
                    // add it, even if 'expandedTimeline' is false, to get the shadow effect
                    result.Add(path + track, bounds);

                    if (expanded)
                        trackTop = trackBottom;

                    groupBottom = Math.Max(groupBottom, trackBottom);
                }

                // need height for group, even if it's empty
                groupBottom = Math.Max(groupBottom, groupTop + Math.Max(margin*2, MinimumTrackSize));

                float groupHeight = groupBottom - groupTop;
                bounds = new RectangleF(0, groupTop, c.Bounds.Width, groupHeight);
                bounds = GdiUtil.Transform(c.Transform, bounds);
                bounds.X = c.ClientRectangle.X;
                // add it, even if 'expandedTimeline' is false, to get the shadow effect
                result.Add(path + group, bounds);
                if (expandedTimeline)
                    groupTop = groupBottom;
                documentBottom = Math.Max(documentBottom, groupBottom);
            }

            if (expandedTimeline)
            {
                // Draw Markers, but limit them to be within the owning timeline.
                RectangleF originalBounds = c.Bounds;
                c.Bounds.Height = documentBottom - c.Bounds.Y;
                foreach (IMarker marker in timeline.Markers)
                {
                    bounds = GetBounds(marker, c);
                    bounds = GdiUtil.Transform(c.Transform, bounds);
                    result.Add(path + marker, bounds);
                }
                c.Bounds = originalBounds;
            }

            documentTop = documentBottom;
        }

        private RectangleF GetGroupHandleRect(RectangleF bounds, bool collapsed)
        {
            int groupHandleWidth = collapsed ? HeaderWidth : TrackIndent;
            return new RectangleF(
                0,
                bounds.Y,
                groupHandleWidth,
                bounds.Height);
        }

        private RectangleF GetTrackHandleRect(RectangleF bounds)
        {
            return new RectangleF(
                TrackIndent,
                bounds.Y,
                HeaderWidth - TrackIndent,
                bounds.Height);
        }

        private RectangleF GetExpanderRect(RectangleF bounds)
        {
            return new RectangleF(
                bounds.X + ExpanderRectMargin.Left,
                bounds.Y + ExpanderRectMargin.Top,
                ExpanderRectSize,
                ExpanderRectSize);
        }

        /// <summary>
        /// Comparer for sorting HitRecords by their HitType enum. Lower-valued enums come first.</summary>
        private int CompareHits(HitRecord x, HitRecord y)
        {
            return x.Type.CompareTo(y.Type);
        }

        /// <summary>
        /// Drawing modes</summary>
        [Flags]
        protected enum DrawMode
        {
            /// <summary>
            /// Draw normally</summary>
            Normal = 1,

            /// <summary>
            /// Draw as part of collapsed group/track</summary>
            Collapsed = 2,

            /// <summary>
            /// Draw ghosted</summary>
            Ghost = 4,

            /// <summary>
            /// Draw as invalid, during move/resize operations</summary>
            Invalid = 8,

            /// <summary>
            /// States part of DrawMode value</summary>
            States = Normal | Collapsed | Ghost | Invalid,

            /// <summary>
            /// Draw ghost for resize-left operation</summary>
            ResizeLeft = 16,

            /// <summary>
            /// Draw ghost for resize-right operation</summary>
            ResizeRight = 32,

            /// <summary>
            /// Draw normal as selected</summary>
            Selected = 64,

            /// <summary>
            /// Modifier flags part of DrawMode value</summary>
            Modifiers = ResizeLeft | ResizeRight | Selected,

            /// <summary>
            /// Rendering to printed page</summary>
            Printing = 128,
        }

        /// <summary>
        /// Class to hold useful context for laying out and drawing timeline elements</summary>
        public class Context
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="renderer">Timeline renderer</param>
            /// <param name="transform">Transform, taking timeline coordinates to display</param>
            /// <param name="clientRectangle">Bounds of displayed area of timeline, in screen space</param>
            /// <param name="g">Graphics object</param>
            public Context(
                D2dTimelineRenderer renderer,
                Matrix transform,
                RectangleF clientRectangle,
                D2dGraphics g)
            {
                Graphics = g;

                Transform = transform;

                ClientRectangle = clientRectangle;
                Bounds = GdiUtil.InverseTransform(transform, clientRectangle);

                TextFormat = renderer.m_textFormat;
                FontHeight = renderer.m_textFormat.FontHeight;

                PixelSize = new SizeF(1 / transform.Elements[0], 1 / transform.Elements[3]);
            }

            /// <summary>
            /// Graphics object</summary>
            public readonly D2dGraphics Graphics;

            /// <summary>
            /// Gets or sets transform matrix, taking timeline coordinates to display. When setting, a copy of the given
            /// Matrix object is made. InverseTransform is updated as well.</summary>
            public Matrix Transform
            {
                get { return m_transform; }
                set
                {
                    m_transform = value.Clone();
                    m_inverseTransform = value.Clone();
                    m_inverseTransform.Invert();
                }
            }

            /// <summary>
            /// Gets inverse transform, taking display coordinates to timeline</summary>
            public Matrix InverseTransform
            {
                get { return m_inverseTransform; }
            }

            /// <summary>
            /// Bounds of displayed area of timeline, in screen space</summary>
            public readonly RectangleF ClientRectangle;
            
            /// <summary>
            /// Bounds of displayed area of timeline, in timeline coordinates</summary>
            public RectangleF Bounds;
            
            /// <summary>
            /// Direct2D TextFormat object for displaying timeline text</summary>
            public readonly D2dTextFormat TextFormat;

            /// <summary>
            /// Height of timeline font</summary>
            public readonly float FontHeight;

            /// <summary>
            /// Pixel size, in timeline coordinates, for drawing constant-size elements of timeline display</summary>
            public readonly SizeF PixelSize;

            /// <summary>
            /// Pushes the current Transform and InverseTransform properties on to a stack and sets Transform to be a new
            /// Matrix object that is the multiplication of a given matrix and the previous Transform</summary>
            /// <param name="m">The matrix to multiply Transform by</param>
            /// <param name="matrixOrder">The order to do the multiplication</param>
            /// <returns>The new Transform matrix property, which is a product of the given matrix and the previous Transform property</returns>
            public Matrix PushTransform(Matrix m, MatrixOrder matrixOrder)
            {
                m_stack.Push(m_transform);
                m_transform = m_transform.Clone();
                m_transform.Multiply(m, matrixOrder);
                m_inverseTransform = m_transform.Clone();
                m_inverseTransform.Invert();
                return m_transform;
            }

            /// <summary>
            /// Removes the top of the matrix stack and replaces the Transform matrix. Updates InverseTransform.</summary>
            /// <returns>The new Transform matrix</returns>
            public Matrix PopTransform()
            {
                m_transform = m_stack.Pop();
                m_inverseTransform = m_transform.Clone();
                m_inverseTransform.Invert();
                return m_transform;
            }

            ///// <summary>
            ///// Tests if this timeline has been seen already by this method and returns true if so. This allows
            ///// the caller to prevent recursion when one timeline can refer to another timeline via
            ///// IHierarchicalTimeline. Should have been called TestDuplicate(), I think.</summary>
            ///// <param name="timeline">the timeline to test</param>
            ///// <returns>true if this timeline has been seen already by this Context object</returns>
            //public bool TestRecursion(ITimeline timeline)
            //{
            //    if (m_timelines.Contains(timeline))
            //        return true;
            //    m_timelines.Add(timeline);
            //    return false;
            //}

            /// <summary>
            /// Clears the data collected by TestRecursion</summary>
            public void ClearRecursionData()
            {
                m_timelines.Clear();
            }

            private Matrix m_transform, m_inverseTransform;
            private readonly Stack<Matrix> m_stack = new Stack<Matrix>(1);
            private readonly HashSet<ITimeline> m_timelines = new HashSet<ITimeline>();
        }

        private static int s_pickTolerance = 3;
        private static int s_headerWidth = 128;
        private static int s_keySize = 12;
        private static int s_majorTickSpacing = 64;
        private static float s_trackHeight = 1;

        private D2dGraphics m_graphics;
        private int m_margin = 8;
        private int m_timeScaleHeight = 24;
        private float m_minimumGraphStep = 1.0f;
        private bool m_disposed;
        private float m_offsetX;
        private float m_minimumTrackSize = 0.025f;
        private int m_expanderRectSize = 8;
        private Padding m_expanderRectMargin = new Padding(3, 3, 3, 3);
        private int m_trackIndent = 16;
        private bool m_printing;
        private Rectangle m_marginBounds; // for printing only

        // Objects that need disposing
        private D2dBrush m_gridPen;
        private D2dBrush m_headerBrush, m_headerLineBrush, m_scaleLineBrush, m_expanderBrush;
        private D2dBrush m_nameBrush;
        private D2dBrush m_scaleTextBrush;
        private D2dSolidColorBrush m_generalSolidColorBrush;
        private D2dTextFormat m_trackTextFormat;
        private readonly D2dTextFormat m_textFormat;

        // For caching the layout dictionary for the last ITimeline that was drawn.
        //  Used to improve picking performance.
        private Pair<ITimeline, TimelineLayout> m_cachedLayout;
    }
}
