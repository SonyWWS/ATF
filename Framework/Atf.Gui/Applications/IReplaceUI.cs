//Sony Computer Entertainment Confidential

using System;
using Sce.Atf;

namespace Sce.Atf.Applications
{
	/// <summary>
	/// Interface for client-defined UI that specifically allows the user to apply replacement data on a 
	/// set of search results</summary>
	public interface IReplaceUI : ISearchableContextUI
	{
		/// <summary>
		/// Assigns a set of search results on which replacements can be made, given data entered into this UI</summary>
		/// <param name="replaceableContext">Interface query search results, whose data is to be replaced</param>
		void Bind(IQueryableReplaceContext replaceableContext);
	}
}