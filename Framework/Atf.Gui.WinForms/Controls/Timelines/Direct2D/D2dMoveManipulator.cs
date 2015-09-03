//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Sce.Atf.Controls.Timelines.Direct2D
{
    /// <summary>
    /// A timeline manipulator for moving IEvent objects, such as markers, keys, and intervals</summary>
    public class D2dMoveManipulator
    {
        /// <summary>
        /// Constructor that permanently attaches to the given TimelineControl by subscribing to its
        /// events</summary>
        /// <param name="owner">The timeline control that we are permanently attached to</param>
        public D2dMoveManipulator(D2dTimelineControl owner)
        {
            m_owner = owner;
            m_owner.MouseDownPicked += owner_MouseDownPicked;
            m_owner.MouseMovePicked += owner_MouseMovePicked;
            m_owner.MouseMove += owner_MouseMove;
            m_owner.MouseDown += owner_MouseDown;
            m_owner.MouseUp += owner_MouseUp;
            m_owner.DrawingD2d += owner_DrawingD2d;
            m_owner.KeyDown += owner_KeyDown;
            m_owner.DragOver += owner_DragOver;
            m_owner.DragEnter += owner_DragEnter;
            m_owner.DragLeave += owner_DragLeave;
            m_owner.DragDrop += owner_DragDrop;
        }

        /// <summary>
        /// Gets the timeline control that we are permanently attached to</summary>
        protected D2dTimelineControl Owner
        {
            get { return m_owner; }
        }

        /// <summary>
        /// Gets the layout information for showing timeline objects that are currently being dragged.
        /// Is null if the IsMovingSelection property is false.</summary>
        protected GhostInfo[] Ghosts
        {
            get { return m_ghosts; }
        }

        /// <summary>
        /// Gets whether or not the user has dragged a selection set past a small threshold and thus
        /// ghost objects should be drawn</summary>
        protected bool IsMovingSelection
        {
            get
            {
                if (m_isDragDropping)
                    return true;
                return
                    m_mouseMoveHitRecord != null && 
                    DragOverThreshold &&
                    !m_owner.IsUsingMouse;
            }
        }

        /// <summary>
        /// Moves the objects represented by the current ghosts to be at the position of
        /// the ghosts</summary>
        protected virtual void MoveSelection()
        {
            // To avoid setting the dirty bit in custom applications.
            // http://sf.ship.scea.com/sf/go/artf22506
            PointF dragOffset = GetDragOffset();
            if (!m_isDragDropping &&
                (dragOffset.X == 0.0f) &&
                (dragOffset.Y == 0.0f))
                return;

            // If we're dragging up, then don't create new tracks
            if (dragOffset.Y < 0)
                foreach (GhostInfo ghost in m_ghosts)
                    if (!ghost.Valid)
                        return;

            if (m_isDragDropping)
            {
                // No transaction for drag-drop; this is the responsibility of the timeline context, for example.
                // Also, the timeline context will create tracks and groups as necessary.
                // All we really need to do is to update the position of the selected objects so that
                //  snapping is correct.
                _MoveSelection(false, false);
            }
            else
            {
                // If Control key is being held down, then we're in copy mode.
                bool tryToCopy = (Control.ModifierKeys == Keys.Control);

                m_owner.TransactionContext.DoTransaction(
                    () => _MoveSelection(tryToCopy, true),
                    "Move Events".Localize("Move Manipulator's undo / redo description for moving timeline events"));
            }
        }

        private void _MoveSelection(bool tryToCopy, bool createTracksAndGroups)
        {
            ITimeline timeline = m_owner.TimelineDocument.Timeline;
            Dictionary<ITrack, ITrack> newTrackMap = new Dictionary<ITrack, ITrack>();
            List<Sce.Atf.Pair<ITrack, IEvent>> toAdd = new List<Sce.Atf.Pair<ITrack, IEvent>>();

            for (int i = 0; i < m_ghosts.Length; i++)
            {
                GhostInfo ghost = m_ghosts[i];

                ITimelineObject ghostCopy = null;
                if (tryToCopy)
                {
                    ICloneable cloneable = ghost.Object as ICloneable;
                    if (cloneable != null)
                        ghostCopy = cloneable.Clone() as ITimelineObject;
                }

                ITimelineReference reference = ghost.Object as ITimelineReference;
                if (reference != null)
                {
                    if (ghostCopy != null)
                        reference = (ITimelineReference)ghostCopy;
                    reference.Start = ghost.Start;
                    if (ghostCopy != null && timeline is IHierarchicalTimelineList)
                        ((IHierarchicalTimelineList)timeline).References.Add(reference);
                    continue;
                }

                IInterval interval = ghost.Object as IInterval;
                if (interval != null)
                {
                    if (ghostCopy != null)
                        interval = (IInterval)ghostCopy;
                    interval.Start = ghost.Start;
                    interval.Length = ghost.Length;
                    ITrack target = (ITrack)ghost.Target;
                    if (target != interval.Track && createTracksAndGroups)
                    {
                        if (target == null)
                            target = CreateTargetTrack(interval.Track, newTrackMap);
                        if (ghostCopy == null)
                            interval.Track.Intervals.Remove(interval);
                        toAdd.Add(new Sce.Atf.Pair<ITrack, IEvent>(target, interval));
                    }
                    continue;
                }

                IKey key = ghost.Object as IKey;
                if (key != null)
                {
                    if (ghostCopy != null)
                        key = (IKey)ghostCopy;
                    key.Start = ghost.Start;
                    ITrack target = (ITrack)ghost.Target;
                    if (target != key.Track && createTracksAndGroups)
                    {
                        if (target == null)
                            target = CreateTargetTrack(key.Track, newTrackMap);
                        if (ghostCopy == null)
                            key.Track.Keys.Remove(key);
                        toAdd.Add(new Sce.Atf.Pair<ITrack, IEvent>(target, key));
                    }
                    continue;
                }

                IMarker marker = ghost.Object as IMarker;
                if (marker != null)
                {
                    if (ghost.Valid &&
                        marker.Start != ghost.Start)
                    {
                        if (ghostCopy != null)
                        {
                            IMarker markerCopy = (IMarker)ghostCopy;
                            markerCopy.Start = ghost.Start;
                            marker.Timeline.Markers.Add(markerCopy);
                        }
                        else
                            marker.Start = ghost.Start;
                    }
                    continue;
                }

                ITrack track = ghost.Object as ITrack;
                if (track != null)
                {
                    var targetTrack = ghost.Target as ITrack;
                    if (targetTrack != null &&
                        targetTrack != track)
                    {
                        if (ghostCopy != null)
                            track = (ITrack)ghostCopy;
                        int index = targetTrack.Group.Tracks.IndexOf(targetTrack);
                        if (ghostCopy == null)
                            track.Group.Tracks.Remove(track);
                        targetTrack.Group.Tracks.Insert(index, track);
                    }
                    else
                    {
                        var targetGroup = ghost.Target as IGroup;
                        if (targetGroup != null)
                        {
                            // the track was dragged to an empty group. http://tracker.ship.scea.com/jira/browse/WWSATF-1371
                            if (ghostCopy != null)
                                track = (ITrack)ghostCopy;
                            else
                                track.Group.Tracks.Remove(track);
                            targetGroup.Tracks.Insert(0, track);
                        }
                    }
                    continue;
                }

                IGroup group = ghost.Object as IGroup;
                if (group != null)
                {
                    IGroup target = (IGroup)ghost.Target;
                    if (target != null &&
                        target != group)
                    {
                        if (ghostCopy != null)
                            group = (IGroup)ghostCopy;
                        int index = m_owner.TimelineDocument.Timeline.Groups.IndexOf(target);
                        if (ghostCopy == null)
                            m_owner.TimelineDocument.Timeline.Groups.Remove(group);
                        m_owner.TimelineDocument.Timeline.Groups.Insert(index, group);
                    }
                    continue;
                }
            }

            // So that when multiple intervals from multiple tracks are relocated to a different
            //  set of tracks, we need to remove them all and then add them all. If the remove and
            //  adds are done in pairs, one by one, then the events can step on each other. artf32260
            foreach (Sce.Atf.Pair<ITrack, IEvent> trackEventPair in toAdd)
            {
                if (trackEventPair.Second is IInterval)
                    trackEventPair.First.Intervals.Add((IInterval)trackEventPair.Second);
                else
                    trackEventPair.First.Keys.Add((IKey)trackEventPair.Second);
            }
        }

        // get start and y offsets in timeline space
        private PointF GetDragOffset()
        {
            if (!m_isDragDropping)
                return m_owner.GetDragOffset();
            PointF dragOffset = GdiUtil.InverseTransformVector(m_owner.Transform, DragDelta);
            dragOffset.X = m_owner.ConstrainFrameOffset(dragOffset.X);
            return dragOffset;
        }

        /// <summary>
        /// Gets the delta between current and drag points</summary>
        private Point DragDelta
        {
            get
            {
                if (!m_isDragDropping)
                    return m_owner.DragDelta;
                Point dragPoint = DragPoint;
                return new Point(dragPoint.X - m_firstPoint.X, dragPoint.Y - m_firstPoint.Y);
            }
        }

        /// <summary>
        /// Gets whether or not mouse movement passed the DragThreshold in x or y</summary>
        private bool DragOverThreshold
        {
            get
            {
                if (!m_isDragDropping)
                    return m_owner.DragOverThreshold;
                return m_isDragging && m_dragOverThreshold;
            }
        }

        /// <summary>
        /// Gets the first mouse point, or current mouse point if drag has passed the
        /// drag threshold</summary>
        private Point DragPoint
        {
            get { return m_dragOverThreshold ? m_currentPoint : m_firstPoint; }
        }

        /// <summary>
        /// Creates a target track if it doesn't already exist as a value in the 'newTrackMap' dictionary. New
        /// tracks are added to 'newTrackMap', with 'original' as the key.</summary>
        /// <param name="original">The key in the 'newTrackMap' dictionary</param>
        /// <param name="newTrackMap">Dictionary that maps original tracks to new tracks</param>
        /// <returns>The new track, equal to newTrackMap[original]</returns>
        private ITrack CreateTargetTrack(ITrack original, Dictionary<ITrack, ITrack> newTrackMap)
        {
            ITrack target;
            if (!newTrackMap.TryGetValue(original, out target))
            {
                ITimeline timeline = m_owner.TimelineDocument.Timeline;
                IGroup newGroup = m_owner.Create(original.Group);
                timeline.Groups.Add(newGroup);
                target = m_owner.Create(original);
                newGroup.Tracks.Add(target);
                newTrackMap.Add(original, target);
            }
            return target;
        }

        private void owner_MouseDownPicked(object sender, HitEventArgs e)
        {
            if (e.MouseEvent.Button == MouseButtons.Left &&
                !m_owner.IsUsingMouse)
            {
                m_mouseMoveHitRecord = null;

                HitRecord hitRecord = e.HitRecord;
                if (m_owner.IsEditable(hitRecord.HitPath))
                {
                    switch (e.HitRecord.Type)
                    {
                        case HitType.GroupMove:
                        case HitType.TrackMove:
                        case HitType.Interval:
                        case HitType.Key:
                        case HitType.Marker:
                            m_mouseMoveHitRecord = hitRecord;
                            break;
                    }
                }

                m_owner.IsMovingSelection = m_mouseMoveHitRecord != null; //obsolete
            }
        }

        private void owner_MouseMovePicked(object sender, HitEventArgs e)
        {
            if (e.MouseEvent.Button == MouseButtons.None &&
                !m_owner.IsUsingMouse)
            {
                HitRecord hitRecord = e.HitRecord;
                if (m_owner.IsEditable(hitRecord.HitPath))
                {
                    switch (hitRecord.Type)
                    {
                        case HitType.Interval:
                        case HitType.Key:
                        case HitType.Marker:
                            m_owner.Cursor = Cursors.SizeAll;
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        // mouse move events aren't issued during a drag-over
        private void owner_DragOver(object sender, DragEventArgs e)
        {
            //MouseMove(e.X, e.Y);
            IEnumerable<ITimelineObject> dragDropObjects = m_owner.DragDropObjects;
            if (dragDropObjects == null)
                return;

            m_firstPoint.X = m_currentPoint.X = e.X;
            m_firstPoint.Y = m_currentPoint.Y = e.Y;
            float clientWindowX = m_owner.PointToClient(new Point(e.X, e.Y)).X;
            float worldX = GdiUtil.InverseTransform(m_owner.Transform, clientWindowX);
            foreach (ITimelineObject obj in dragDropObjects)
            {
                var _event = obj as IEvent;
                if (_event != null)
                    _event.Start = worldX;
            }
            MouseMove(e.X, e.Y);
        }

        private void owner_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove(e.X, e.Y);
        }

        private void MouseMove(int x, int y)
        {
            m_currentPoint = new Point(x, y);
            int xDelta = Math.Abs(m_currentPoint.X - m_firstPoint.X);
            int yDelta = Math.Abs(m_currentPoint.Y - m_firstPoint.Y);
            if (m_isDragging &&
                !m_dragOverThreshold &&
                (xDelta > m_dragThreshold || yDelta > m_dragThreshold))
            {
                m_dragOverThreshold = true;
            }

            if (IsMovingSelection)
            {
                m_ghosts = GetMoveGhostInfo(m_owner.Transform, m_owner.GetLayout());
                if (Control.ModifierKeys == Keys.Control)
                    m_owner.Cursor = Cursors.PanNW; //to indicate copy operation
                else
                    m_owner.Cursor = Cursors.SizeAll;
            }
        }

        // mouse move events aren't issued during a drag-over
        void owner_DragEnter(object sender, DragEventArgs e)
        {
            MouseDown(e.X, e.Y);
        }

        private void owner_MouseDown(object sender, MouseEventArgs e)
        {
            MouseDown(e.X, e.Y);
        }

        private void MouseDown(int x, int y)
        {
            m_firstPoint = m_currentPoint = new Point(x, y);
            m_isDragging = true;
            m_isDragDropping = (m_owner.DragDropObjects != null);
        }

        void owner_DragLeave(object sender, EventArgs e)
        {
            CancelDrag();
        }

        /// <summary>
        /// Method called when drag and drop ends</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void owner_DragDrop(object sender, DragEventArgs e)
        {
            owner_MouseUp(sender, null);
        }

        private void owner_MouseUp(object sender, MouseEventArgs e)
        {
            if (IsMovingSelection)
            {
                MoveSelection();
                m_owner.Invalidate();
            }
            CancelDrag();
        }

        // draw dragged events
        private void owner_DrawingD2d(object sender, EventArgs e)
        {
            Rectangle clientRectangle = m_owner.ClientRectangle;
            Matrix transform = m_owner.Transform;
            
            if (IsMovingSelection && m_ghosts != null)
            {
                m_owner.Renderer.DrawGhosts(
                    m_ghosts,
                    GhostType.Move,
                    transform,
                    clientRectangle);

                // raise EventMoving events
                foreach (GhostInfo ghost in m_ghosts)
                {
                    IEvent _event = ghost.Object as IEvent;
                    if (_event != null)
                    {
                        m_owner.MoveEvent(
                            new EventMovedEventArgs(_event, ghost.Start, ghost.Target as ITrack));
                    }
                }
            }
        }

        private void owner_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == m_owner.CancelDragKey)
                CancelDrag();
        }

        private void CancelDrag()
        {
            m_isDragging = false;
            m_dragOverThreshold = false;
            m_isDragDropping = false;

            if (m_mouseMoveHitRecord != null)
            {
                m_mouseMoveHitRecord = null;
                m_ghosts = null;
                m_owner.IsMovingSelection = false;
                m_owner.Cursor = Cursors.Arrow;
                m_owner.Invalidate();
            }
        }

        // gets move information, for drawing ghosts and for performing actual move operation
        private GhostInfo[] GetMoveGhostInfo(Matrix worldToView, TimelineLayout layout)
        {
            // get start and y offsets in timeline space
            PointF dragOffset = GetDragOffset();

            // Get snapping points along the timeline (in world coordinates).
            List<float> movingPoints = new List<float>(2);
            TimelinePath snapperPath;
            if (m_mouseMoveHitRecord != null)
                snapperPath = m_mouseMoveHitRecord.HitPath;//use the last clicked event (interval, key or marker)
            else
                snapperPath = m_owner.Selection.LastSelected as TimelinePath;//moving a group or track, for example
            IEvent snapperEvent = snapperPath != null ? snapperPath.Last as IEvent : null;

            if (snapperEvent != null)
            {
                using (Matrix localToWorld = D2dTimelineControl.CalculateLocalToWorld(snapperPath))
                {
                    float worldStart = GdiUtil.Transform(localToWorld, snapperEvent.Start + dragOffset.X);
                    movingPoints.Add(worldStart);
                    if (snapperEvent.Length > 0.0f)
                        movingPoints.Add(GdiUtil.Transform(localToWorld,
                            snapperEvent.Start + dragOffset.X + snapperEvent.Length));
                }
            }

            // Get the offset from one of the world snap points to the closest non-selected object.
            float snapOffset;
            try
            {
                s_snapOptions.FilterContext = snapperEvent;
                s_snapOptions.Filter = MoveSnapFilter;
                snapOffset = m_owner.GetSnapOffset(movingPoints, s_snapOptions);
            }
            finally
            {
                s_snapOptions.FilterContext = null;
                s_snapOptions.Filter = null;
            }

            // adjust dragOffset to "snap-to" nearest event
            dragOffset.X += snapOffset;

            // get offsets in client space
            float xOffset = dragOffset.X * worldToView.Elements[0];
            float yOffset = dragOffset.Y * worldToView.Elements[3];

            TimelinePath[] targets = GetMoveTargets(layout);

            // Pretend that drag-drop objects are in the TimelineLayout object
            if (m_owner.DragDropObjects != null)
            {
                foreach (ITimelineObject dragDrop in m_owner.DragDropObjects)
                    layout[dragDrop] = GetDragDropBounds(dragDrop);
            }

            GhostInfo[] ghosts = new GhostInfo[targets.Length];
            int i = -1;
            foreach(TimelinePath path in m_owner.Selection.Selection)
            {
                i++;

                ITimelineObject timelineObject = path.Last;
                RectangleF bounds = layout[path];

                TimelinePath targetPath = targets[i];
                ITimelineObject target = targetPath != null ? targetPath.Last : null;

                float start = 0;
                float length = 0;
                bool valid = true;

                IInterval interval = timelineObject as IInterval;
                IKey key = timelineObject as IKey;
                IMarker marker = timelineObject as IMarker;
                ITrack track = timelineObject as ITrack;
                IGroup group = timelineObject as IGroup;
                ITimelineReference reference = timelineObject as ITimelineReference;

                if (interval != null)
                {
                    ITrack targetTrack = target as ITrack;
                    start = interval.Start + dragOffset.X;
                    length = interval.Length;
                    valid = targetTrack != null;
                    valid &= m_owner.Constraints.IsStartValid(interval, ref start);
                    valid &= m_owner.Constraints.IsLengthValid(interval, ref length);

                    if (valid)
                    {
                        ITrack intervalTrack = interval.Track;
                        if (intervalTrack != null)
                            yOffset = layout[target].Y - layout[interval.Track].Y;
                        else
                            yOffset = layout[target].Y - bounds.Y;

                        TimelinePath testPath = new TimelinePath(targetPath);
                        foreach (IInterval other in targetTrack.Intervals)
                        {
                            // skip selected intervals, since they are moving too
                            testPath.Last = other;
                            if (m_owner.Selection.SelectionContains(testPath))
                                continue;

                            if (!m_owner.Constraints.IsIntervalValid(interval, ref start, ref length, other))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }
                }
                else if (reference != null)
                {
                    // don't allow for vertical repositioning yet
                    start = reference.Start + dragOffset.X;
                    valid = true;
                }
                else if (key != null)
                {
                    start = key.Start + dragOffset.X;
                    ITrack targetTrack = target as ITrack;
                    valid = targetTrack != null;
                    valid &= m_owner.Constraints.IsStartValid(key, ref start);

                    if (valid)
                    {
                        ITrack keyTrack = key.Track;
                        if (keyTrack != null)
                            yOffset = layout[targetTrack].Y - layout[key.Track].Y;
                        else
                            yOffset = layout[targetTrack].Y - bounds.Y;
                    }
                }
                else if (marker != null)
                {
                    start = marker.Start + dragOffset.X;
                    yOffset = 0;
                    valid = m_owner.Constraints.IsStartValid(marker, ref start);
                }
                else if (track != null)
                {
                    xOffset = 0;
                    if (target == null)
                    {
                        target =
                            (DragDelta.Y < 0) ? GetLastTrack() : GetFirstTrack();
                    }
                }
                else if (group != null)
                {
                    xOffset = 0;
                    if (target == null)
                    {
                        IList<IGroup> groups = m_owner.TimelineDocument.Timeline.Groups;
                        target = (DragDelta.Y < 0) ? groups[0] : groups[groups.Count - 1];
                    }
                }

                bounds.Offset(xOffset, yOffset);

                ghosts[i] = new GhostInfo(timelineObject, target, start, length, bounds, valid);
            }
            return ghosts;
        }

        // Gets the bounding box in client window coordinates
        private RectangleF GetDragDropBounds(ITimelineObject dragDrop)
        {
            var bounds = new RectangleF();

            var interval = dragDrop as IInterval;
            if (interval != null)
                bounds = m_owner.Renderer.GetBounds(interval, 0, m_owner.Transform, m_owner.ClientRectangle);

            var key = dragDrop as IKey;
            if (key != null)
                bounds = m_owner.Renderer.GetBounds(key, 0, m_owner.Transform, m_owner.ClientRectangle);

            bounds = GdiUtil.Transform(m_owner.Transform, bounds);

            return bounds;
        }

        private ITrack GetFirstTrack()
        {
            foreach (IGroup group in m_owner.TimelineDocument.Timeline.Groups)
                foreach (ITrack track in group.Tracks)
                    return track;

            return null;
        }

        private ITrack GetLastTrack()
        {
            IList<IGroup> groups = m_owner.TimelineDocument.Timeline.Groups;
            for (int i = groups.Count - 1; i >= 0; --i)
            {
                IGroup group = groups[i];
                IList<ITrack> tracks = group.Tracks;
                for (int j = tracks.Count - 1; j >= 0; ) // loop decrement is unreachable code!
                    return tracks[j];
            }

            return null;
        }

        // get track that contains y value (in client window coordinates, aka, screen space)
        private TimelinePath GetTargetTrack(
            float y,
            TimelineLayout layout,
            IList<ITrack> tracks)
        {
            TimelinePath testPath = new TimelinePath((ITimelineObject)null);
            foreach (ITrack track in tracks)
            {
                testPath.Last = track;
                RectangleF targetBounds = layout[testPath];
                if (targetBounds.Top <= y && targetBounds.Bottom >= y)
                    return testPath;
            }

            return null;
        }

        // gets objects of appropriate type that are underneath the moving selected objects
        private TimelinePath[] GetMoveTargets(TimelineLayout layout)
        {
            // get groups and visible tracks
            List<IGroup> groups = new List<IGroup>();
            List<ITrack> visibleTracks = new List<ITrack>();
            foreach (IGroup group in m_owner.TimelineDocument.Timeline.Groups)
            {
                groups.Add(group);

                IList<ITrack> tracks = group.Tracks;
                bool expanded = group.Expanded;
                bool collapsible = tracks.Count > 1;
                bool collapsed = collapsible && !expanded;
                if (!collapsed)
                    foreach (ITrack track in tracks)
                        visibleTracks.Add(track);
            }

            TimelinePath[] targets = new TimelinePath[m_owner.Selection.SelectionCount];
            RectangleF bounds;
            TimelinePath testPath = new TimelinePath((ITimelineObject)null);

            int i = -1;
            foreach(TimelinePath path in m_owner.Selection.Selection)
            {
                i++;

                IGroup group = path.Last as IGroup;
                if (group != null)
                {
                    foreach (IGroup targetGroup in groups)
                    {
                        Point p = DragPoint;
                        testPath.Last = targetGroup;
                        RectangleF targetBounds = layout[testPath];
                        if (targetBounds.Top <= p.Y && targetBounds.Bottom >= p.Y)
                        {
                            targets[i] = new TimelinePath(testPath);
                            break;
                        }
                    }
                    continue;
                }

                ITrack track = path.Last as ITrack;
                if (track != null)
                {
                    foreach (ITrack targetTrack in visibleTracks)
                    {
                        Point p = m_owner.DragPoint;
                        testPath.Last = targetTrack;
                        RectangleF targetBounds = layout[testPath];
                        if (targetBounds.Top <= p.Y && targetBounds.Bottom >= p.Y)
                        {
                            targets[i] = new TimelinePath(testPath);
                            break;
                        }
                    }
                    if (targets[i] == null)
                    {
                        foreach (IGroup targetGroup in groups)
                        {
                            Point p = DragPoint;
                            testPath.Last = targetGroup;
                            RectangleF targetBounds = layout[testPath];
                            if (targetBounds.Top <= p.Y && targetBounds.Bottom >= p.Y)
                            {
                                targets[i] = new TimelinePath(testPath);
                                break;
                            }
                        }
                    }
                    continue;
                }

                IInterval interval = path.Last as IInterval;
                if (interval != null)
                {
                    track = interval.Track;
                    if (track != null)
                    {
                        testPath.Last = track;
                        bounds = layout[testPath];
                        float y = bounds.Top + bounds.Height * 0.5f + DragDelta.Y;
                        targets[i] = GetTargetTrack(y, layout, visibleTracks);
                        continue;
                    }
                    else
                    {
                        float y = m_owner.RectangleToClient(new Rectangle(Cursor.Position, new Size())).Y;
                        targets[i] = GetTargetTrack(y, layout, visibleTracks);
                    }
                }

                IKey key = path.Last as IKey;
                if (key != null)
                {
                    track = key.Track;
                    if (track != null)
                    {
                        testPath.Last = track;
                        bounds = layout[testPath];
                        float y = bounds.Top + bounds.Height * 0.5f + DragDelta.Y;
                        targets[i] = GetTargetTrack(y, layout, visibleTracks);
                        continue;
                    }
                    else
                    {
                        //float y = m_owner.ClientToCanvas(m_owner.DragPoint).Y;
                        float y = m_owner.RectangleToClient(new Rectangle(Cursor.Position, new Size())).Y;
                        targets[i] = GetTargetTrack(y, layout, visibleTracks);
                    }
                }

                // ignore Markers, as they don't hit targets
            }

            return targets;
        }

        private bool MoveSnapFilter(IEvent testEvent, D2dTimelineControl.SnapOptions options)
        {
            ITimelineReference movingReference = options.FilterContext as ITimelineReference;
            if (movingReference == null)
                return true;

            IHierarchicalTimeline movingTimeline = movingReference.Target;
            if (movingTimeline == null)
                return true;

            ITimeline owningTimeline = null;
            if (testEvent is ITimelineReference)
                owningTimeline = ((ITimelineReference)testEvent).Parent;
            else if (testEvent is IInterval)
                owningTimeline = ((IInterval)testEvent).Track.Group.Timeline;
            else if (testEvent is IKey)
                owningTimeline = ((IKey)testEvent).Track.Group.Timeline;
            else if (testEvent is IMarker)
                owningTimeline = ((IMarker)testEvent).Timeline;

            // Is the reference being compared to an object that it owns? Never snap!
            if (owningTimeline == movingTimeline)
                return false;

            // Is the reference being compared against a sibling object? Snap away.
            if (owningTimeline == movingReference.Parent)
                return true;

            // to-do: support a true hierarchy. We can't support directed acyclic graphs because
            //  the Layout implementation uses a Dictionary of ITimelineObjects and the same object
            //  can't have multiple layout rectangles. We could support a tree hierarchy, though.
            //// The test object may be a grandchild of the moving reference. Look for a parent.
            //owningTimeline = owningTimeline.Parent;

            return true;
        }

        private static readonly D2dTimelineControl.SnapOptions s_snapOptions = new D2dTimelineControl.SnapOptions();

        private readonly D2dTimelineControl m_owner;
        private GhostInfo[] m_ghosts;
        private HitRecord m_mouseMoveHitRecord;
        private Point m_firstPoint;
        private Point m_currentPoint;
        private int m_dragThreshold = 3;
        private bool m_dragOverThreshold;
        private bool m_isDragging;
        private bool m_isDragDropping;
    }
}
