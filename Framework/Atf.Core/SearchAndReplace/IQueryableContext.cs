//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for classes in which objects may be searched</summary>
    public interface IQueryableContext
    {
        /// <summary>
        /// Compiles an enumeration of objects that satisfy the conditions of the search predicates</summary>
        /// <param name="predicate">Search predicates</param>
        /// <returns>Enumeration of objects satisfying the conditions</returns>
        IEnumerable<object> Query(IQueryPredicate predicate);
    }
}
