//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Standard Pair struct</summary>
    /// <typeparam name="T1">First type</typeparam>
    /// <typeparam name="T2">Second type</typeparam>
    public struct Pair<T1, T2> : IComparable, IComparable<Pair<T1, T2>>
    {
        /// <summary>
        /// Constructor using two values</summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        /// <summary>
        /// Constructor using KeyValuePair</summary>
        /// <param name="keyValuePair">Key value pair</param>
        public Pair(KeyValuePair<T1, T2> keyValuePair)
        {
            First = keyValuePair.Key;
            Second = keyValuePair.Value;
        }

        /// <summary>
        /// The first item</summary>
        public T1 First;

        /// <summary>
        /// The second item</summary>
        public T2 Second;

        /// <summary>
        /// Indicates whether this instance and a specified object are equal</summary>
        /// <param name="obj">Another object to compare to</param>
        /// <returns><c>True</c> if object and this instance are the same type and represent the same value</returns>
        public override bool Equals(object obj)
        {
            if (obj is Pair<T1, T2>)
            {
                Pair<T1, T2> other = (Pair<T1, T2>)obj;
                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Tests for equality to a specified pair</summary>
        /// <param name="other">The other pair</param>
        /// <returns><c>True</c> if pairs equal</returns>
        public bool Equals(Pair<T1, T2> other)
        {
            return
                s_equalityComparer1.Equals(First, other.First) &&
                s_equalityComparer2.Equals(Second, other.Second);
        }

        /// <summary>
        /// Returns the hash code for this instance</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance</returns>
        public override int GetHashCode()
        {
            int hash1 = (First == null) ? 0x6778318E : First.GetHashCode();
            int hash2 = (Second == null) ? 0x7F5A9535 : Second.GetHashCode();
            return hash1 ^ hash2;
        }

        /// <summary>
        /// Returns the fully qualified type name of this instance</summary>
        /// <returns>A <see cref="T:System.String"></see> containing a fully qualified type name</returns>
        public override string ToString()
        {
            return
                string.Format("{0} {1}",
                    (First == null) ? "null" : First.ToString(),
                    (Second == null) ? "null" : Second.ToString());
        }

        /// <summary>
        /// Compares this to another pair, in lexical (first, then second) order</summary>
        /// <param name="other">Other pair</param>
        /// <returns>-1, 0 or 1 depending on compare</returns>
        /// <exception cref="NotSupportedException">if other is not the same type</exception>
        public int CompareTo(Pair<T1, T2> other)
        {
            try
            {
                int firstCompare = s_comparer1.Compare(First, other.First);
                if (firstCompare != 0)
                    return firstCompare;
                else
                    return s_comparer2.Compare(Second, other.Second);
            }
            catch (ArgumentException)
            {
                throw new NotSupportedException("Can't compare types");
            }
        }

        /// <summary>
        /// Compares this to another object</summary>
        /// <param name="obj">Other object</param>
        /// <returns>-1, 0 or 1 depending on compare</returns>
        /// <exception cref="ArgumentException">if object is not of the same type</exception>
        int IComparable.CompareTo(object obj)
        {
            if (obj is Pair<T1, T2>)
                return CompareTo((Pair<T1, T2>)obj);
            else
                throw new ArgumentException("obj is wrong type");
        }

        /// <summary>
        /// Tests two pairs for equality</summary>
        /// <param name="pair1">First pair</param>
        /// <param name="pair2">Second pair</param>
        /// <returns><c>True</c> if pairs are equal</returns>
        public static bool operator ==(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
        {
            return
                s_equalityComparer1.Equals(pair1.First, pair2.First) &&
                s_equalityComparer2.Equals(pair1.Second, pair2.Second);
        }

        /// <summary>
        /// Tests two pairs for inequality</summary>
        /// <param name="pair1">First pair</param>
        /// <param name="pair2">Second pair</param>
        /// <returns><c>True</c> if pairs are not equal</returns>
        public static bool operator !=(Pair<T1, T2> pair1, Pair<T1, T2> pair2)
        {
            return !(pair1 == pair2);
        }

        /// <summary>
        /// Creates a KeyValuePair from this pair</summary>
        /// <returns>KeyValuePair equivalent to this pair</returns>
        public KeyValuePair<T1, T2> ToKeyValuePair()
        {
            return new KeyValuePair<T1, T2>(First, Second);
        }

        /// <summary>
        /// Creates a KeyValuePair from a Pair</summary>
        /// <param name="pair">Pair</param>
        /// <returns>KeyValuePair equivalent to pair</returns>
        public static explicit operator KeyValuePair<T1, T2>(Pair<T1, T2> pair)
        {
            return new KeyValuePair<T1, T2>(pair.First, pair.Second);
        }

        /// <summary>
        /// Creates a Pair from a KeyValuePair</summary>
        /// <param name="keyValuePair">KeyValuePair</param>
        /// <returns>Pair equivalent to keyValuePair</returns>
        public static explicit operator Pair<T1, T2>(KeyValuePair<T1, T2> keyValuePair)
        {
            return new Pair<T1, T2>(keyValuePair);
        }

        private static readonly IComparer<T1> s_comparer1 = Comparer<T1>.Default;
        private static readonly IComparer<T2> s_comparer2 = Comparer<T2>.Default;

        private static readonly IEqualityComparer<T1> s_equalityComparer1 = EqualityComparer<T1>.Default;
        private static readonly IEqualityComparer<T2> s_equalityComparer2 = EqualityComparer<T2>.Default;
    }
}
