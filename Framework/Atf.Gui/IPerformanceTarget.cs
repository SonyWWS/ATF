//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for custom situations where the performance monitor is measuring
    /// the performance of an object that is either not a control or is a control that
    /// doesn't raise the Paint event, e.g., a Direct2D control</summary>
    /// <remarks>See PerformanceMonitor and PerformanceMonitorControl</remarks>
    public interface IPerformanceTarget
    {
        /// <summary>
        /// Does the event whose duration is measured and then returns. Raising the
        /// EventOccurred event is optional.</summary>
        /// <example>For example, if we are monitoring the time it takes a window to redraw,
        /// this event would be the equivalent of Refresh() on a control.</example>
        void DoEvent();

        /// <summary>
        /// Event that is raised on the object being measured to indicate that
        /// another timing sample should be taken, so that the number of events per second
        /// can be calculated.</summary>
        /// <remarks>For example, if we are monitoring the time it takes a window to redraw,
        /// this event might be raised for every Paint event on the control.</remarks>
        event EventHandler EventOccurred;
    }
}
