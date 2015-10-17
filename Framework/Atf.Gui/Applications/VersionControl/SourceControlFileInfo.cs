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
        /// <param name="isLocked"><c>True</c> if locked</param>
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
        /// <param name="isLocked"><c>True</c> if locked</param>
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
        
        /// <summary>
        /// File URI</summary>
        public readonly Uri Uri;
        /// <summary>
        /// SourceControlStatus</summary>
        public readonly SourceControlStatus Status;
        /// <summary>
        /// Head revision number</summary>
        public readonly int HeadRevision;
        /// <summary>
        /// Revision number</summary>
        public readonly int Revision;
        /// <summary>
        /// Whether file is locked</summary>
        public readonly bool IsLocked;
        /// <summary>
        /// Other users who have file checked out</summary>
        public readonly IEnumerable<string> OtherUsers;
        /// <summary>
        /// Other user who has file locked</summary>
        public readonly string OtherLock;
    }
}
