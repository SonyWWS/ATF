//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for contexts that provide search and replace capabilities, through a client-defined UI</summary>
    public interface ISearchableContext
    {
        /// <summary>
        /// Gets the UI for displaying and choosing the search parameters to be used by the Search
        /// method</summary>
        ISearchUI SearchUI { get; }

        /// <summary>
        /// Gets the UI for specifying the replacement pattern to be used by the Replace method</summary>
        IReplaceUI ReplaceUI { get; }

        /// <summary>
        /// Gets the UI for displaying and choosing the results of the search</summary>
        IResultsUI ResultsUI { get; }
    }
}
