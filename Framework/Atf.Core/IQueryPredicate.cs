//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
	/// <summary>
	/// Interface for specifying test conditions for a query, and for applying value replacements
    /// on data from the query matches</summary>
	public interface IQueryPredicate
    {
		/// <summary>
		/// Test the predicate on an item</summary>
		bool Test(object item, out IList<IQueryMatch> matchList);

        /// <summary>
        /// Apply a replacement on an item</summary>
		void Replace(IList<IQueryMatch> matchList, object replaceValue);
    }
}
