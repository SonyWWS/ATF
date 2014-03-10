//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
	/// <summary>
	/// Interface for classes in which objects may be searched</summary>
	public interface IQueryableContext
    {
        /// <summary>
        /// Compiles an enumeration of objects that satisfy the conditions of the search predicate
        /// </summary>
        /// <param name="predicate">Search predicate</param>
        /// <returns>Enumeration of objects satisfying the search predicate conditions</returns>
        IEnumerable<object> Query(IQueryPredicate predicate);
	}
}
