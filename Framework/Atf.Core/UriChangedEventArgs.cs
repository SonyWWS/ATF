//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Event args for URI changed events</summary>
    public class UriChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="oldUri">Old resource URI</param>
        public UriChangedEventArgs(Uri oldUri)
        {
            OldUri = oldUri;
        }

        /// <summary>
        /// Old resource URI</summary>
        public readonly Uri OldUri;
    }
}
