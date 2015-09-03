//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Timelines.Direct2D
{
    /// <summary>
    /// A timeline manipulator for implementing the selection logic. Should probably be attached
    /// last to the timeline control. The attachment is permanent and there must be one
    /// timeline control per ITimelineDocument.</summary>
    public class D2dSelectionManipulator
    {
        /// <summary>
        /// Constructor that permanently attaches to the given timeline control by subscribing to its
        /// events.</summary>
        /// <param name="owner">The timeline control that we are permanently attached to</param>
        public D2dSelectionManipulator(D2dTimelineControl owner)
        {
            Owner = owner;
            Owner.MouseDownPicked += Owner_MouseDownPicked;
            Owner.KeyDown += Owner_KeyDown;
            Owner.MouseUp += Owner_MouseUp;
        }

        /// <summary>
        /// Event handler for the timeline control.MouseDownPicked event</summary>
        /// <param name="sender">Timeline control that we are attached to</param>
        /// <param name="e">Event args</param>
        protected virtual void Owner_MouseDownPicked(object sender, HitEventArgs e)
        {
            HitRecord hitRecord = e.HitRecord;
            TimelinePath hitObject = hitRecord.HitPath;
            if (hitObject != null)
            {
                Keys modifiers = Control.ModifierKeys;

                if (e.MouseEvent.Button == MouseButtons.Left)
                {
                    if (modifiers == Keys.None && SelectionContext != null &&
                        SelectionContext.SelectionContains(hitObject))
                    {
                        // The hit object is already selected. Wait until the mouse up to see if there was
                        //  a drag or not. If no drag, then set the selection set to be this one object.
                        m_mouseDownHitRecord = hitRecord;
                        m_mouseDownPos = e.MouseEvent.Location;
                    }
                    else if (modifiers == Keys.Control)
                    {
                        // Either this object is not already selected or Shift key is being held down.
                        //  to be consistent with the Windows interface.
                        m_mouseDownHitRecord = hitRecord;
                        m_mouseDownPos = e.MouseEvent.Location;
                    }
                    else if ((modifiers & Keys.Alt) == 0)
                    {
                        // The 'Alt' key might mean something different. If no Alt key, we can update the
                        //  selection immediately.
                        UpdateSelection(hitRecord, modifiers);
                    }
                }
                else if (e.MouseEvent.Button == MouseButtons.Right)
                {
                    if (modifiers == Keys.None && SelectionContext != null &&
                        !SelectionContext.SelectionContains(hitObject))
                    {
                        // The hit object is not already selected and a right-click landed on it. Select it.
                        UpdateSelection(hitRecord, modifiers);
                    }
                }
            }
        }

        /// <summary>
        /// MouseUp event handler</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected virtual void Owner_MouseUp(object sender, MouseEventArgs e)
        {
            // 'm_mouseDownHitRecord' is already selected. If no drag is taking place, update the selection.
            if (m_mouseDownHitRecord != null)
            {
                try
                {
                    int tolerance = Owner.Renderer.PickTolerance;
                    if (Math.Abs(e.Location.X - m_mouseDownPos.X) <= tolerance &&
                        Math.Abs(e.Location.Y - m_mouseDownPos.Y) <= tolerance)
                    {
                        UpdateSelection(m_mouseDownHitRecord, Control.ModifierKeys);
                    }
                }
                finally
                {
                    m_mouseDownHitRecord = null;
                }
            }
        }

        /// <summary>
        /// Updates the selection set, given the hit object and the modifier keys</summary>
        /// <param name="hitRecord">HitRecord</param>
        /// <param name="modifiers">Modifier keys</param>
        private void UpdateSelection(HitRecord hitRecord, Keys modifiers)
        {
            TimelinePath hitObject = hitRecord.HitPath;
            bool hitIsValidAnchor = true;

            switch (hitRecord.Type)
            {
                case HitType.GroupMove:
                    SelectGroups(hitObject);
                    break;

                case HitType.TrackMove:
                    SelectTracks(hitObject);
                    break;

                case HitType.Interval:
                case HitType.Key:
                case HitType.Marker:
                    SelectEvents(hitObject);
                    Owner.Constrain = (modifiers & Owner.ConstrainModifierKeys) != 0;
                    break;
                
                default:
                    Anchor = null;
                    hitIsValidAnchor = false;
                    break;
            }

            if (hitIsValidAnchor)
            {
                // If the Shift key is not held down or the current Anchor is null, or the user
                //  has switched between track and group, or the user has picked an event, then
                //  update the Anchor. IEvents are always additive with the shift key.
                if ((modifiers & Keys.Shift) == 0 ||
                    Anchor == null ||
                    (Anchor.Last is IGroup && hitObject.Last is ITrack) ||
                    (Anchor.Last is ITrack && hitObject.Last is IGroup) ||
                    (Anchor.Last is IEvent && hitObject.Last is IEvent))
                {
                    Anchor = hitObject;
                }
            }
        }

        /// <summary>
        /// Event handler for the timeline control KeyDown event to handle Shift+End and Shift+Home</summary>
        /// <param name="sender">Timeline control that we are attached to</param>
        /// <param name="e">Event args</param>
        protected virtual void Owner_KeyDown(object sender, KeyEventArgs e)
        {
            // Holding down Ctrl or Alt simultaneously still does a range selection in
            //  Visual Studio's Solution Explorer.
            if ((e.Modifiers & Keys.Shift) != Keys.None)
            {
                if (e.KeyCode == Keys.End)
                {
                    if (Anchor != null && Anchor.Last is IGroup)
                        SelectGroups(GetLastGroup());
                    else if (Anchor != null && Anchor.Last is ITrack)
                        SelectTracks(GetLastTrack());
                }
                else if (e.KeyCode == Keys.Home)
                {
                    if (Anchor != null && Anchor.Last is IGroup)
                        SelectGroups(GetFirstGroup());
                    else if (Anchor != null && Anchor.Last is ITrack)
                        SelectTracks(GetFirstTrack());
                }
            }
        }

        /// <summary>
        /// Adds range of events to selection, from anchor to given target.
        /// Handles selecting an event (e.g., IInterval, IKey, IMarker), taking into account
        /// selecting ranges of tracks.
        /// If no anchor, just selects target.</summary>
        /// <param name="target">Timeline path</param>
        protected virtual void SelectEvents(TimelinePath target)
        {
            // add range of events, from anchor to target?
            if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
            {
                if (Anchor != null && Anchor.Last is IEvent && SelectionContext != null)
                {
                    SelectionContext.AddRange(GetRangeOfEvents(Anchor, target));
                    return;
                }
            }

            // simply add this target track, using the current modifier keys to determine how
            Owner.Select<IEvent>(target);
        }

        /// <summary>
        /// Gets the range of events that intersect the rectangle that encloses 'begin' and 'end'</summary>
        /// <param name="begin">Beginning timeline path</param>
        /// <param name="end">Ending timeline path</param>
        /// <returns>Enumeration of timeline paths that intersect the rectangle.
        /// A timeline path is a sequence of objects in timelines, e.g., groups, tracks, events.</returns>
        protected virtual IEnumerable<TimelinePath> GetRangeOfEvents(TimelinePath begin, TimelinePath end)
        {
            // If the two paths are the same, then the rectangle is just a point.
            // This check fixes a bug where the 'rectangle' becomes the whole interval
            //  and then falsely selects an adjacent zero-length object due to Overlaps().
            if (begin.Equals(end))
                return new TimelinePath[] { begin };

            // Get the range of tracks between these two events.
            bool searchMarkers = false;
            System.Collections.IEnumerable rangeOfTracks;
            if (begin.Last is IMarker || end.Last is IMarker)
            {
                rangeOfTracks = Owner.AllTracks;
                searchMarkers = true;
            }
            else
            {
                TimelinePath beginTrack = GetOwningTrack(begin);
                TimelinePath endTrack = GetOwningTrack(end);
                rangeOfTracks = GetRangeOfTracks(beginTrack, endTrack);
            }

            // Get the range of times to look for.
            float beginStart, beginEnd;
            D2dTimelineControl.CalculateRange(begin, out beginStart, out beginEnd);
            float endStart, endEnd;
            D2dTimelineControl.CalculateRange(end, out endStart, out endEnd);
            float beginTime = Math.Min(beginStart, endStart);
            float endTime = Math.Max(beginEnd, endEnd);

            // Look through all the IEvents of these tracks.
            List<TimelinePath> range = new List<TimelinePath>();
            foreach (TimelinePath testPath in rangeOfTracks)
            {
                ITrack track = (ITrack)testPath.Last;
                foreach (IKey key in track.Keys)
                {
                    testPath.Last = key;
                    if (Overlaps(testPath, beginTime, endTime))
                        range.Add(new TimelinePath(testPath));
                }

                foreach (IInterval interval in track.Intervals)
                {
                    testPath.Last = interval;
                    if (Overlaps(testPath, beginTime, endTime))
                        range.Add(new TimelinePath(testPath));
                }
            }

            // Look for markers?
            if (searchMarkers)
            {
                foreach (TimelinePath testPath in Owner.AllMarkers)
                {
                    if (Overlaps(testPath, beginTime, endTime))
                        range.Add(testPath);
                }
            }

            return range;
        }

        /// <summary>
        /// Adds range of groups to selection, from anchor to given target.
        /// Handles selecting a group, taking into account selecting ranges of groups.
        /// If no anchor, just selects target.</summary>
        /// <param name="target">Timeline path</param>
        protected virtual void SelectGroups(TimelinePath target)
        {
            // Add range of groups, from anchor to target? Holding down Ctrl or Alt simultaneously
            //  still does a range selection in Visual Studio's Solution Explorer.
            if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
            {
                if (Anchor != null && Anchor.Last is IGroup && SelectionContext != null)
                {
                    SelectionContext.SetRange(GetRangeOfGroups(Anchor, target));
                    return;
                }
            }

            // simply add this target group, using the current modifier keys to determine how
            Owner.Select<IGroup>(target);
        }

        /// <summary>
        /// Gets the range of groups from 'begin' to 'end', inclusive, as determined by the order
        /// that they are presented by the ITimelineDocument. The result always has 'begin'
        /// as the first ITimelineObject and 'end' as the last. If 'begin' and 'end' are the same,
        /// then the result has only the one ITimelineObject.</summary>
        /// <param name="begin">Beginning timeline path</param>
        /// <param name="end">Ending timeline path</param>
        /// <returns>Enumeration of timeline paths in the given range.
        /// A timeline path is a sequence of objects in timelines, e.g., groups, tracks, events.</returns>
        protected virtual IEnumerable<TimelinePath> GetRangeOfGroups(TimelinePath begin, TimelinePath end)
        {
            TimelinePath lastGroupToFind = null;
            List<TimelinePath> range = new List<TimelinePath>();
            foreach (TimelinePath group in Owner.AllGroups)
            {
                if (lastGroupToFind == null)
                {
                    if (group == begin)
                        lastGroupToFind = end;
                    else if (group == end)
                        lastGroupToFind = begin;
                }

                if (lastGroupToFind != null)
                    range.Add(group);

                if (group == lastGroupToFind)
                    break;
            }

            if (lastGroupToFind == begin)
                range.Reverse();

            return range;
        }

        /// <summary>
        /// Adds range of tracks to selection, from anchor to given target.
        /// Handles selecting a track, taking into account selecting ranges of tracks.</summary>
        /// <param name="target">Track as TimelinePath</param>
        protected virtual void SelectTracks(TimelinePath target)
        {
            // add range of tracks, from anchor to target?
            if ((Control.ModifierKeys & Keys.Shift) != Keys.None)
            {
                if (Anchor != null && Anchor.Last is ITrack && SelectionContext != null)
                {
                    SelectionContext.SetRange(GetRangeOfTracks(Anchor, target));
                    return;
                }
            }

            // simply add this target track, using the current modifier keys to determine how
            Owner.Select<ITrack>(target);
        }

        /// <summary>
        /// Gets the range of tracks from 'begin' to 'end', inclusive, as determined by the order
        /// that they are presented by the ITimelineDocument. The result always has 'begin'
        /// as the first ITimelineObject and 'end' as the last. If 'begin' and 'end' are the same,
        /// then the result has only the one ITimelineObject.</summary>
        /// <param name="begin">Beginning timeline path</param>
        /// <param name="end">Ending timeline path</param>
        /// <returns>Enumeration of timeline paths in the given range.
        /// A timeline path is a sequence of objects in timelines, e.g., groups, tracks, events.</returns>
        protected virtual IEnumerable<TimelinePath> GetRangeOfTracks(TimelinePath begin, TimelinePath end)
        {
            TimelinePath lastTrackToFind = null;
            List<TimelinePath> range = new List<TimelinePath>();
            foreach (TimelinePath track in Owner.AllTracks)
            {
                if (lastTrackToFind == null)
                {
                    if (track == begin)
                        lastTrackToFind = end;
                    else if (track == end)
                        lastTrackToFind = begin;
                }

                if (lastTrackToFind != null)
                    range.Add(track);

                if (track == lastTrackToFind)
                    break;
            }

            if (lastTrackToFind == begin)
                range.Reverse();

            return range;
        }

        /// <summary>
        /// Gets the first group as determined by the associated ITimeline</summary>
        /// <returns>First group as TimelinePath</returns>
        protected TimelinePath GetFirstGroup()
        {
            foreach (TimelinePath group in Owner.AllGroups)
                return group;
            return null;
        }

        /// <summary>
        /// Gets the last group as determined by the associated ITimeline</summary>
        /// <returns>Last group as TimelinePath</returns>
        protected TimelinePath GetLastGroup()
        {
            TimelinePath last = null;
            foreach (TimelinePath group in Owner.AllGroups)
                last = group;
            return last;
        }

        /// <summary>
        /// Gets the first track as determined by the associated ITimeline</summary>
        /// <returns>First track as TimelinePath</returns>
        protected TimelinePath GetFirstTrack()
        {
            foreach (TimelinePath track in Owner.AllTracks)
                return track;
            return null;
        }

        /// <summary>
        /// Gets the last track as determined by the associated ITimeline</summary>
        /// <returns>Last track as TimelinePath</returns>
        protected TimelinePath GetLastTrack()
        {
            TimelinePath last = null;
            foreach (TimelinePath track in Owner.AllTracks)
                last = track;
            return last;
        }

        /// <summary>
        /// The timeline control that this manipulator is permanently attached to. There is one
        /// timeline control for each ITimelineDocument/selection set.</summary>
        protected readonly D2dTimelineControl Owner;

        /// <summary>
        /// The selection anchor object that is used to define the beginning of a ranged selection
        /// when holding down the shift key</summary>
        protected TimelinePath Anchor;

        // private because this may be moved to TimelineControl
        private bool Overlaps(TimelinePath path, float beginTime, float endTime)
        {
            IEvent e = (IEvent)path.Last;

            float start, length;
            using (Matrix localToWorld = D2dTimelineControl.CalculateLocalToWorld(path))
            {
                start = GdiUtil.Transform(localToWorld, e.Start);
                length = GdiUtil.TransformVector(localToWorld, e.Length);
            }

            // If the length is zero, then count an exact match with beginTime or endTime as
            //  being an overlap.
            if (length == 0)
                return !(
                    start > endTime ||
                    start + length < beginTime);

            // Otherwise, don't count an exact match.
            return !(
                start >= endTime ||
                start + length <= beginTime);
        }

        // private because this may be moved to TimelineControl
        private TimelinePath GetOwningTrack(TimelinePath begin)
        {
            if (begin.Last is IKey)
            {
                TimelinePath trackPath = new TimelinePath(begin);
                trackPath.Last = ((IKey)begin.Last).Track;
                return trackPath;
            }
            if (begin.Last is IInterval)
            {
                TimelinePath trackPath = new TimelinePath(begin);
                trackPath.Last = ((IInterval)begin.Last).Track;
                return trackPath;
            }
            if (begin.Last is ITrack)
                return begin;
            return null;
        }

        private ISelectionContext SelectionContext
        {
            get
            {
                if (Owner != null && Owner.TimelineDocument != null)
                    return Owner.TimelineDocument.As<ISelectionContext>();
                return null;
            }
        }

        // The hit record during a mouse-down. Need to wait for the mouse-up to use it.
        private HitRecord m_mouseDownHitRecord;
        private Point m_mouseDownPos;
    }
}
