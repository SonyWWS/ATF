//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls
{
    /// <summary>
    /// User actions in FindFile Dialogs</summary>
    public enum FindFileAction
    {
        /// <summary>
        /// Accept suggestion for this file</summary>
        AcceptSuggestion,

        /// <summary>
        /// Accept suggestion for all files</summary>
        AcceptAllSuggestions,

        /// <summary>
        /// Search for file in a user-specified directory and its sub-directories</summary>
        SearchDirectory,

        /// <summary>
        /// Search a user-specified directory and its sub-directories for all missing files</summary>
        SearchDirectoryForAll,

        /// <summary>
        /// Let the user find file</summary>
        UserSpecify,

        /// <summary>
        /// Ignore this missing file (don't try to find it)</summary>
        Ignore,

        /// <summary>
        /// Ignore all missing files (don't try to find any)</summary>
        IgnoreAll
    }
}


