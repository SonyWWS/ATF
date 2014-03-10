//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for client-defined UI that specifically displays the results of a generated search</summary>
    public interface IResultsUI : ISearchableContextUI
    {
        /// <summary>
        /// Assigns the search result content which the user-defined UI displays</summary>
        /// <param name="queryResultContext">Interface to the search results of a database query</param>
        void Bind(IQueryableResultContext queryResultContext);
    }
}