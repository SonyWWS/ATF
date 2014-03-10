//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Static class to instantiate an empty enumerable. This is slightly more efficient
    /// than EmptyArray, as enumeration is accomplished without instantiating an enumerator
    /// object.</summary>
    /// <typeparam name="T">Enumerable element type</typeparam>
    public static class EmptyEnumerable<T>
    {
        /// <summary>
        /// Gets the single instance of the empty enumerable</summary>
        public static readonly IEnumerable<T> Instance = new Enumerable();

        private class Enumerable : IEnumerable<T>
        {
            public IEnumerator<T> GetEnumerator()
            {
                return s_enumerator;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return s_enumerator;
            }

            private class Enumerator : IEnumerator<T>
            {
                public T Current
                {
                    get { throw new InvalidOperationException("no current value"); }
                }

                public void Dispose()
                {
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return Current; }
                }

                public bool MoveNext()
                {
                    return false;
                }

                public void Reset()
                {
                }
            }

            // global enumerator
            private static readonly Enumerator s_enumerator = new Enumerator();
        }
    }
}
