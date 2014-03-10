//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Various ways of specifying file revisions</summary>
    public enum SourceControlRevisionKind
    {
        /// <summary>
        /// Not under source control, file may not exist in the repository</summary>
        Unspecified,

        /// <summary>
        /// Revision given as a number</summary>
        Number,

        /// <summary>
        /// Revision given as a date</summary>
        Date,

        /// <summary>
        /// The base revision of the working copy: the
        /// revision of file last synced without local modifications</summary>
        Base,

        /// <summary>
        /// Current working copy = Base + local modifications</summary>
        Working,

        /// <summary>
        /// Youngest in repository</summary>
        Head,

        /// <summary>
        /// The revision of file immediately after changelist was submitted</summary>
        ChangeList,
    }
}
