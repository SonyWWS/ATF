//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Event arguments describing event that moved</summary>
    public class EventMovedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="_event">Event that moved</param>
        /// <param name="newStart">New start time of event</param>
        /// <param name="newTrack">New track that holds event, or null</param>
        public EventMovedEventArgs(IEvent _event, float newStart, ITrack newTrack)
        {
            Event = _event;
            NewStart = newStart;
            NewTrack = newTrack;
        }

        /// <summary>
        /// Event that moved</summary>
        public readonly IEvent Event;
        /// <summary>
        /// New start time of event</summary>
        public readonly float NewStart;
        /// <summary>
        /// New track that holds event, or null</summary>
        public readonly ITrack NewTrack;
    }
}
