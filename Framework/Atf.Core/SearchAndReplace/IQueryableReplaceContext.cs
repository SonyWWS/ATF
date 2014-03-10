//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Interface for classes in which containing objects may be replaced</summary>
    public interface IQueryableReplaceContext
    {
        /// <summary>
        /// Applies a replacement on the results of the last Query</summary>
        /// <param name="replaceInfo">Replacement information</param>
        /// <returns>The list of objects on which we just performed a replacement</returns>
        IEnumerable<object> Replace(object replaceInfo);
    }
}
