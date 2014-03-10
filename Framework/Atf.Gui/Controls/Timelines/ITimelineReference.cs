//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for objects that hold a reference to a timeline document</summary>
    public interface ITimelineReference : IEvent
    {
        /// <summary>
        /// Gets the referenced ITimeline object or null if it doesn't exist or if the physical file
        /// could not be found, etc.</summary>
        IHierarchicalTimeline Target
        {
            get;
        }

        /// <summary>
        /// Gets the timeline that contains this reference</summary>
        IHierarchicalTimeline Parent
        {
            get;
        }

        /// <summary>
        /// Gets an object that contains properties that affect the user interface and behavior of this
        /// timeline reference</summary>
        /// <remarks>There is no 'set' because changes to the object persist, and so that client code
        /// can provide a derived class.</remarks>
        TimelineReferenceOptions Options
        {
            get;
        }
    }
}
