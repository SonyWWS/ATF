//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Source control status values</summary>
    public enum SourceControlStatus
    {
        /// <summary>
        /// The file does not exist</summary>
        FileDoesNotExist,

        /// <summary>
        /// Not under source control</summary>
        NotControlled,

        /// <summary>
        /// Checked in</summary>
        CheckedIn,

        /// <summary>
        /// Checked out</summary>
        CheckedOut,

        /// <summary>
        /// Added</summary>
        Added,

        /// <summary>
        /// Deleted</summary>
        Deleted,

        /// <summary>
        /// The status has not yet been determined.</summary>
        Unknown,

    }
}
