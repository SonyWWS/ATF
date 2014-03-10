//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Default implementation of the abstract base class for timeline constraints. It
    /// ensures that intervals don't overlap by either clipping the new interval or
    /// repositioning it to the right.</summary>
    public class DefaultTimelineConstraints : TimelineConstraints
    {
        /// <summary>
        /// Tests if start value would be valid for the given event</summary>
        /// <param name="_event">Event</param>
        /// <param name="start">Prospective start; may be modified to make start valid</param>
        /// <returns>True iff start value would be valid for the given event</returns>
        public override bool IsStartValid(IEvent _event, ref float start)
        {
            if (start >= 0)
                return true;
            start = 0;
            return false;
        }

        /// <summary>
        /// Tests if length would be valid for the given interval</summary>
        /// <param name="interval">Interval</param>
        /// <param name="length">Prospective length; may be modified to make length valid</param>
        /// <returns>True iff length value would be valid for the given interval</returns>
        public override bool IsLengthValid(IInterval interval, ref float length)
        {
            // 0 length intervals will crash DefaultTimelineRenderer
            if (length > 0)
                return true;
            length = 1;
            return false;
        }

        /// <summary>
        /// Tests if interval would be valid if it shared a track with another interval</summary>
        /// <param name="interval">Interval that is being modified</param>
        /// <param name="start">Prospective interval start; may be modified to make start valid</param>
        /// <param name="length">Prospective interval length; may be modified to make length valid</param>
        /// <param name="other">Other interval</param>
        /// <returns>True iff interval would be valid if it shared a track with the other interval</returns>
        public override bool IsIntervalValid(IInterval interval, ref float start, ref float length, IInterval other)
        {
            // If there is no overlap or a simple trimming will fix any overlap, then
            //  we're done.
            if (ClipAgainst(interval, ref start, ref length, other))
                return true;

            // Otherwise, try to reposition to the right.
            // Note: it would be helpful if Intervals were sorted.
            start = other.Start + other.Length;
            if (interval.Track != null)
            {
                IList<IInterval> intervals = interval.Track.Intervals;
                for (int i = 0; i < intervals.Count; i++)
                {
                    other = intervals[i];
                    if (other == interval)
                        continue;

                    if (!ClipAgainst(interval, ref start, ref length, other))
                    {
                        // position to the right of 'other' and try again
                        start = other.Start + other.Length;
                        i = -1;
                    }
                }
            }
            return true;
        }

        private bool ClipAgainst(IInterval interval, ref float start, ref float length, IInterval other)
        {
            // trim interval to avoid overlaps; interval is invalid only if that forces its
            //  length to be 0 or less
            float end = start + length;
            float otherStart = other.Start;
            float otherEnd = otherStart + other.Length;

            // if disjoint, return true
            if (start >= otherEnd || end <= otherStart)
                return true;

            // if interval encloses and is larger than the other, cut as little as possible
            if (start <= otherStart && end >= otherEnd && length > other.Length)
            {
                if ((end - otherEnd) < (otherStart - start))
                    length = otherStart - start;
                else
                {
                    start = otherEnd;
                    length = end - start;
                }
                return true;
            }

            // if interval contains other's start, try to trim its length
            if (start < otherStart)
            {
                length = otherStart - start;
                return true;
            }

            // if interval contains other's end, try to move its start
            if (start < otherEnd && end > otherEnd)
            {
                length = end - otherEnd;
                start = otherEnd;
                return true;
            }

            // interval is the same as other or is inside other
            return false;
        }
    }
}
