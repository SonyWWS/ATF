//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Extension methods for working with System.Lazy objects</summary>
    public static class Lazies
    {
        /// <summary>
        /// Converts an enumeration of System.Lazy objects into an enumeration of the generic
        /// type. If this is the first time that these Lazy objects have had their Value property
        /// accessed, an instance of the generic type is created.</summary>
        /// <typeparam name="T">The type of object that is being lazily initialized</typeparam>
        /// <param name="lazies">Enumeration of lazy objects</param>
        /// <returns>Enumeration of type T</returns>
        public static IEnumerable<T> GetValues<T>(this IEnumerable<Lazy<T>> lazies)
        {
            foreach (Lazy<T> lazy in lazies)
                yield return lazy.Value;
        }
    }
}
