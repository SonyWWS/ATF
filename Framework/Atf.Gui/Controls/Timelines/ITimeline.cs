//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for timelines, which contain groups and markers</summary>
    /// <remarks>The hierarchy of timeline objects is this:
    /// Timelines contain Groups contain Tracks contain Events: Intervals, Keys (zero-length Intervals) and Markers
    /// (zero-length Events that are on all Tracks in a timeline).</remarks>
    public interface ITimeline
    {
        /// <summary>
        /// Creates a new group. Try to use TimelineControl.Create(IGroup) if there is a "source" IGroup.</summary>
        /// <returns>New group</returns>
        IGroup CreateGroup();

        /// <summary>
        /// Creates a new marker. Try to use TimelineControl.Create(IMarker) if there is a "source" IMarker.</summary>
        /// <returns>New marker</returns>
        IMarker CreateMarker();

        /// <summary>
        /// Gets the list of all groups in the timeline</summary>
        IList<IGroup> Groups { get; }

        /// <summary>
        /// Gets the list of all markers in the timeline</summary>
        IList<IMarker> Markers { get; }
    }
}

