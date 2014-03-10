//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
	/// <summary>
	/// Interface for managing a query pattern used in a search &amp; replace process</summary>
	public interface IReplacingQueryPattern
	{
		/// <summary>
		/// Tests whether the search data of the specified candidate matches this pattern</summary>
		bool Matches(IQueryMatch itemToMatch);

		/// <summary>
		/// Using this search pattern, replaces the search data of the specified candidate 
        /// with the data from the specified object</summary>
		void Replace(IQueryMatch itemToReplace, object replaceWith);
	}
}
