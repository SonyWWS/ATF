//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for ITimeline objects, which can contain sub-timelines</summary>
    /// <remarks>The resulting data graph is a tree where each timeline has only one parent
    /// (or "owner") within a timeline document. However, with multiple documents open, there
    /// are, in effect, multiple parents with no way for this interface to find just one.</remarks>
    public interface IHierarchicalTimeline : ITimeline
    {
        /// <summary>
        /// Gets the references owned by this timeline. This is not a recursive enumeration.</summary>
        IEnumerable<ITimelineReference> References { get; }
    }
}
