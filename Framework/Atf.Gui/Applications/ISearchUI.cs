//Sony Computer Entertainment Confidential

using System;
using Sce.Atf;

namespace Sce.Atf.Applications
{
	/// <summary>
	/// Interface for client-defined UI that specifically provide user controls for specifying 
	/// search criteria and for triggering a search on that criteria</summary>
	public interface ISearchUI : ISearchableContextUI
	{
		/// <summary>
		/// Assigns the queryable content on which the user defined search is made</summary>
		/// <param name="queryableContext">Interface to data set on which queries can be made</param>
		void Bind(IQueryableContext queryableContext);
	}
}