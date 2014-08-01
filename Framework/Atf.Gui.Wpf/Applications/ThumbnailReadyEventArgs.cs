//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Event args for "ThumbnailReady" event</summary>
    public class ThumbnailReadyEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="resourceUri">URI of the resource that thumbnail represents</param>
        /// <param name="image">Thumbnail image</param>
        public ThumbnailReadyEventArgs(Uri resourceUri, object image)
        {
            ResourceUri = resourceUri;
            Image = image;
        }

        /// <summary>
        /// URI of the resource that the thumbnail represents</summary>
        public readonly Uri ResourceUri;

        /// <summary>
        /// Thumbnail image</summary>
        public readonly object Image;
    }
}
