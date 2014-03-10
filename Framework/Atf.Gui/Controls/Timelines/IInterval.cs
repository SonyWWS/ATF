//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for intervals, which are zero or greater length events on a track</summary>
    public interface IInterval : IEvent
    {
        /// <summary>
        /// Gets the track containing this interval</summary>
        ITrack Track { get; }
    }

    /// <summary>
    /// Useful static and extension methods for IInterval objects</summary>
    public static class Intervals
    {
        /// <summary>
        /// Sets the interval's track</summary>
        /// <param name="interval">Interval to move to a new track</param>
        /// <param name="newTrack">New track that becomes the owner of this interval,
        /// or null if there is no new owning track</param>
        /// <remarks>
        /// SetTrack DOES NOT by itself set the interval's Track property!
        /// This happens indirectly and only if ITrack is implemented correctly.
        /// 
        /// If the interval and new track are DomNodes AND Intervals are DOM-children of their track,
        /// you can implement the Track property like this:
        /// public ITrack Track { get { return GetParentAs&lt;ITrack>(); } }
        /// 
        /// Otherwise, use an ObservableCollection for your Intervals list,
        /// monitor changes and update the interval's Track property when
        /// intervals are being added to or removed from a Track.
        /// </remarks>
        public static void SetTrack(this IInterval interval, ITrack newTrack)
        {
            ITrack currentTrack = interval.Track;
            if (currentTrack != null)
                currentTrack.Intervals.Remove(interval);
            if (newTrack != null)
                newTrack.Intervals.Add(interval);
        }
    }
}


