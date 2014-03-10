//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for Events, base interface for IInterval, IKey and IMarker</summary>
    public interface IEvent : ITimelineObject
    {
        /// <summary>
        /// Gets and sets the event's start time</summary>
        float Start { get; set; }

        /// <summary>
        /// Gets and sets the event's length (duration)</summary>
        float Length { get; set; }

        /// <summary>
        /// Gets and sets the event's color</summary>
        Color Color { get; set; }

        /// <summary>
        /// Gets and sets the event's name</summary>
        string Name { get; set; }
    }
}

