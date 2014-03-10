//Sony Computer Entertainment Confidential

using System;
using Sce.Atf;

namespace Sce.Atf.Applications
{
	/// <summary>
	/// Interface for client-defined UI that specifically displays the results of a generated search</summary>
	public interface IResultsUI : ISearchableContextUI
	{
		/// <summary>
		/// Assigns the search result content that the user-defined UI will display</summary>
		/// <param name="queryResultContext">Interface to the search results of a database query</param>
		void Bind(IQueryableResultContext queryResultContext);
	}
}