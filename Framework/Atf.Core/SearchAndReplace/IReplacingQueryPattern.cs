//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for managing a query pattern used in a search &amp; replace process</summary>
    public interface IReplacingQueryPattern
    {
        /// <summary>
        /// Tests whether the search data of the specified candidate matches this pattern</summary>
        /// <param name="itemToMatch">Match item</param>
        /// <returns>True iff match</returns>
        bool Matches(IQueryMatch itemToMatch);

        /// <summary>
        /// Using this search pattern, replaces the search data of the specified candidate with the data from the specified object</summary>
        /// <param name="itemToReplace">Item to replace</param>
        /// <param name="replaceWith">Replacement item</param>
        void Replace(IQueryMatch itemToReplace, object replaceWith);
    }
}
