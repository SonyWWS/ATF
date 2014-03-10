//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for ITimeline objects, which can contain sub-timelines, and support
    /// inserting, removing, and counting those sub-timelines via an IList interface</summary>
    public interface IHierarchicalTimelineList
    {
        /// <summary>
        /// Gets an IList that allows for adding, removing, counting, clearing, etc. the list
        /// of ITimelineReferences</summary>
        IList<ITimelineReference> References
        {
            get;
        }
    }
}
