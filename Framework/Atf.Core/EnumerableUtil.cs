//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Class with static methods for iterating through IEnumerable collections</summary>
    public static class EnumerableUtil
    {
        /// <summary>
        /// Execute an action for each item and a sequence, passing in the 
        /// index of that item to the action procedure</summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="that">The sequence</param>
        /// <param name="action">A function that accepts a sequence item and its
        /// index in the sequence</param>
        public static void ForEachWithIndex<T>(this IEnumerable<T> that, Action<T, int> action)
        {
            var index = 0;
            foreach (var item in that)
            {
                action(item, index);
                index++;
            }
        }

        /// <summary>
        /// Execute an action for each item and a sequence</summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="that">The sequence</param>
        /// <param name="action">A function that accepts a sequence item</param>
        public static void ForEach<T>(this IEnumerable<T> that, Action<T> action)
        {
            foreach (var item in that)
            {
                action(item);
            }
        }

        /// <summary>
        /// Execute an action for each item in a sequence until an action returns false</summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="that">The sequence</param>
        /// <param name="action">A function that accepts a sequence item</param>
        public static void ForEachWhileTrue<T>(this IEnumerable<T> that, Func<T, bool> action)
        {
            foreach (var item in that)
            {
                if (!action(item)) break;
            }
        }

        /// <summary>
        /// Find the index of a particular item in the enumerable source</summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="source">The sequence</param>
        /// <param name="value">The value to search for</param>
        /// <returns>Index of the element, or -1 if the element was not found</returns>
        public static int IndexOf<T>(this IEnumerable<T> source, T value)
        {
            return source.IndexOf(value, null);
        }

        /// <summary>
        /// Find the index of a particular item in the enumerable source</summary>
        /// <typeparam name="T">The type of the sequence</typeparam>
        /// <param name="source">The sequence</param>
        /// <param name="value">The value to search for</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>Index of the element, or -1 if the element was not found</returns>
        public static int IndexOf<T>(this IEnumerable<T> source, T value, IEqualityComparer<T> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");

            var list = source as IList<T>;
            if (list != null)
                return list.IndexOf(value);

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            int index = 0;
            foreach (var element in source)
            {
                if (comparer.Equals(element, value))
                    return index;
                ++index;
            }

            return -1;
        }
    }
}
