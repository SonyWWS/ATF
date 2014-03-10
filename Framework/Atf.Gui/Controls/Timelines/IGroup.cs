//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for groups, which contain zero or more tracks and can be expanded or collapsed
    /// in a timeline viewing control</summary>
    public interface IGroup : ITimelineObject
    {
        /// <summary>
        /// Gets and sets the group name</summary>
        string Name { get; set; }

        /// <summary>
        /// Gets and sets whether or not the group is expanded (i.e., are the tracks it contains
        /// visible?)</summary>
        bool Expanded { get; set; }

        /// <summary>
        /// Gets the timeline that contains the group</summary>
        ITimeline Timeline { get; }

        /// <summary>
        /// Creates a new track. Try to use TimelineControl.Create(ITrack) if there is a "source" ITrack.
        /// Does not add the track to this group.</summary>
        /// <returns>New unparented track</returns>
        ITrack CreateTrack();

        /// <summary>
        /// Gets a list of all tracks in the group</summary>
        IList<ITrack> Tracks { get; }
    }
}



