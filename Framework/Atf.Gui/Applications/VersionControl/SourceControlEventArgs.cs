//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Arguments for source control events</summary>
    public class SourceControlEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="uri">URI for source controlled resource</param>
        /// <param name="status">Source control status of resource</param>
        public SourceControlEventArgs(Uri uri, SourceControlStatus status)
        {
            Uri = uri;
            Status = status;
        }

        /// <summary>
        /// URI for source controlled resource</summary>
        public readonly Uri Uri;

        /// <summary>
        /// Source control status of resource</summary>
        public readonly SourceControlStatus Status;
    }
}
