//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for tracks, which contain zero or more events</summary>
    public interface ITrack : ITimelineObject
    {
        /// <summary>
        /// Gets and sets the track name</summary>
        string Name { get; set; }

        /// <summary>
        /// Gets the group that contains the track</summary>
        IGroup Group { get; }

        /// <summary>
        /// Creates a new interval. Try to use TimelineControl.Create(ITrack) if there is a "source" ITrack.</summary>
        /// <returns>New interval</returns>
        IInterval CreateInterval();

        /// <summary>
        /// Gets the list of all intervals in the track. Adding or removing intervals in the IList 
        /// modifies the underlying data store. For example, if the DOM is being used, the DOM is
        /// modified when the IList is modified.</summary>
        IList<IInterval> Intervals { get; }

        /// <summary>
        /// Creates a new key. Try to use TimelineControl.Create(IKey) if there is a "source" IKey.</summary>
        /// <returns>New key</returns>
        IKey CreateKey();

        /// <summary>
        /// Gets the list of all keys in the track</summary>
        IList<IKey> Keys { get; }
    }
}


