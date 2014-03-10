//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for markers, which are zero length events on all tracks in timeline</summary>
    public interface IMarker : IEvent
    {
        /// <summary>
        /// Gets the timeline that contains the marker</summary>
        ITimeline Timeline { get; }
    }
}


