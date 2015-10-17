//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Abstract base class for timeline constraints</summary>
    public abstract class TimelineConstraints
    {
        /// <summary>
        /// Tests if start value would be valid for the given event</summary>
        /// <param name="_event">Event</param>
        /// <param name="start">Prospective start; may be modified to make start valid</param>
        /// <returns><c>True</c> if start value would be valid for the given event</returns>
        public abstract bool IsStartValid(IEvent _event, ref float start);

        /// <summary>
        /// Tests if length would be valid for the given interval</summary>
        /// <param name="interval">Interval</param>
        /// <param name="length">Prospective length; may be modified to make length valid</param>
        /// <returns><c>True</c> if length value would be valid for the given interval</returns>
        public abstract bool IsLengthValid(IInterval interval, ref float length);

        /// <summary>
        /// Tests if the interval would be valid if it shared a track with another interval.
        /// Any modification to 'start' and 'length' should not invalidate a previous test
        /// against a different interval, during a paste operation, for example.</summary>
        /// <param name="interval">Interval that is being modified</param>
        /// <param name="start">Prospective interval start; may be modified to make start valid</param>
        /// <param name="length">Prospective interval length; may be modified to make length valid</param>
        /// <param name="other">Other interval</param>
        /// <returns><c>True</c> if interval would be valid if it shared a track with the other interval</returns>
        public abstract bool IsIntervalValid(IInterval interval, ref float start, ref float length, IInterval other);
    }
}
