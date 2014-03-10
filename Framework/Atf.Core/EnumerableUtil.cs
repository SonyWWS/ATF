//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    public static class EnumerableUtil
    {
        /// <summary>
        /// Executes an action for each item and a sequence, passing in the 
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
        /// Executes an action for each item and a sequence</summary>
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
        /// Executes an action for each item in a sequence until an action returns false</summary>
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
    }
}
