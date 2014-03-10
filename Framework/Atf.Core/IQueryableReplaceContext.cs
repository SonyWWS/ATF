//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
	/// <summary>
	/// Interface for classes in which containing objects may be replaced</summary>
	public interface IQueryableReplaceContext
    {
        /// <summary>
		/// Apply a replacement on the results of the last Query</summary>
		/// <returns>The list of objects on which we just performed a replacement</returns>
		IEnumerable<object> Replace(object replaceInfo);
	}
}
