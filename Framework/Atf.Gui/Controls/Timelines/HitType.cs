//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Enumeration of types of hit result, sorted from highest priority to lowest when picking</summary>
    public enum HitType
    {
        /// <summary>
        /// Hit a key</summary>
        Key,

        /// <summary>
        /// Hit a Marker</summary>
        Marker,

        /// <summary>
        /// Hit on the left edge of an interval, indicating resizing, or hit on the left scale handle</summary>
        IntervalResizeLeft,

        /// <summary>
        /// Hit on the right edge of an interval, indicating resizing, or hit on the right scale handle</summary>
        IntervalResizeRight,

        /// <summary>
        /// Hit an Interval</summary>
        Interval,

        /// <summary>
        /// Hit the right side of the header</summary>
        HeaderResize,

        /// <summary>
        /// Hit a group expander box</summary>
        GroupExpand,

        /// <summary>
        /// Hit a group move handle, which is also used for selecting a group</summary>
        GroupMove,

        /// <summary>
        /// Hit the time scale</summary>
        TimeScale,

        /// <summary>
        /// Hit a track move handle, which is also used for selecting a track</summary>
        TrackMove,

        /// <summary>
        /// Hit a track outside of its handle, like in a blank portion between events</summary>
        Track,

        /// <summary>
        /// Hit a group outside of its handle, but not in a track</summary>
        Group,

        /// <summary>
        /// A custom control or manipulator</summary>
        Custom,

        /// <summary>
        /// Hit nothing</summary>
        None,
    }
}
