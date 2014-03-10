//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Interface for keys, which are zero length events on a track</summary>
    public interface IKey : IEvent
    {
        /// <summary>
        /// Gets the track that contains the key</summary>
        ITrack Track { get; }
    }
}


