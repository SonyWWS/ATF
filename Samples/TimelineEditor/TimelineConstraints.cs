//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Timelines;

namespace TimelineEditorSample
{
    /// <summary>
    /// Timeline constraints, such as ensuring that intervals don't overlap by 
    /// either clipping the new interval or repositioning it to the right.
    /// Client code can override methods in the base class for custom contraints on events'
    /// start times and interval placements. The default is to always fix overlaps when
    /// new intervals are added to the timeline.</summary>
    public class TimelineConstraints : DefaultTimelineConstraints
    {
    }
}
