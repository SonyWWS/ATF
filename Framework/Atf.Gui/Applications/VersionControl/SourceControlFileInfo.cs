//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Contains source control information for a file</summary>
    public class SourceControlFileInfo
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="uri">File URI</param>
        /// <param name="status">Source control status</param>
        public SourceControlFileInfo(Uri uri, SourceControlStatus status)
        {
            Uri = uri;
            Status = status;
            OtherUsers = EmptyEnumerable<string>.Instance;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="uri">File URI</param>
        /// <param name="status">Source control status</param>
        /// <param name="headRevision">Head revision number</param>
        /// <param name="revision">Revision number</param>
        /// <param name="isLocked">True iff locked</param>
        /// <param name="otherUsers">Other file users</param>
        public SourceControlFileInfo(Uri uri, SourceControlStatus status,
            int headRevision, int revision, bool isLocked, IEnumerable<string> otherUsers)
        {
            Uri = uri;
            Status = status;
            HeadRevision = headRevision;
            Revision = revision;
            IsLocked = isLocked;
            OtherUsers = otherUsers;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="uri">File URI</param>
        /// <param name="status">Source control status</param>
        /// <param name="headRevision">Head revision number</param>
        /// <param name="revision">Revision number</param>
        /// <param name="isLocked">True iff locked</param>
        /// <param name="otherUsers">Other file users</param>
        /// <param name="otherLock">Another user has the file locked</param>
        public SourceControlFileInfo(Uri uri, SourceControlStatus status,
            int headRevision, int revision, bool isLocked, IEnumerable<string> otherUsers, string otherLock)
        {
            Uri = uri;
            Status = status;
            HeadRevision = headRevision;
            Revision = revision;
            IsLocked = isLocked;
            OtherUsers = otherUsers;
            OtherLock = otherLock;
        }
        
        public readonly Uri Uri;
        public readonly SourceControlStatus Status;
        public readonly int HeadRevision;
        public readonly int Revision;
        public readonly bool IsLocked;
        public readonly IEnumerable<string> OtherUsers; // other users have the file checked out
        public readonly string OtherLock;// if another user has the file locked
    }
}
