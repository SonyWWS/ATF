//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for timeline objects that can create new instances of the same kind of object
    /// that they are. Callers should test for this interface instead of using the following:
    /// ITimeline.CreateGroup()
    /// ITimeline.CreateMarker()
    /// IGroup.CreateTrack()
    /// ITrack.CreateInterval()
    /// ITrack.CreateKey()</summary>
    /// <remarks>These 5 methods do not allow client code to determine the correct object to create
    /// in the case where there are multiple implementors of each of those interfaces:
    /// IGroup, IMarker, ITrack, IInterval, and IKey.</remarks>
    public interface ITimelineObjectCreator : ITimelineObject
    {
        /// <summary>
        /// Creates a new instance of this type of timeline object. Does not add the new object
        /// to any timeline-related container.</summary>
        /// <returns>New timeline object; does not return null</returns>
        ITimelineObject Create();
    }
}
