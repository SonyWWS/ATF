//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for specifying test conditions for a query and for applying value replacements
    /// on data from the query matches</summary>
    public interface IQueryPredicate
    {
        /// <summary>
        /// Tests the predicate on an item</summary>
        /// <param name="item">Item queried</param>
        /// <param name="matchList">List of query matches</param>
        /// <returns><c>True</c> if match</returns>
        bool Test(object item, out IList<IQueryMatch> matchList);

        /// <summary>
        /// Applies a replacement to an item</summary>
        /// <param name="matchList">List of query matches</param>
        /// <param name="replaceValue">Replacement value</param>
        void Replace(IList<IQueryMatch> matchList, object replaceValue);
    }
}
