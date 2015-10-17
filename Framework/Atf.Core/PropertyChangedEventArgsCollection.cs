//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf
{
    /// <summary>
    /// Special collection for PropertyChangedEventArgs - ensures no matching args sets are added</summary>
    public class PropertyChangedEventArgsCollection : ICollection<PropertyChangedEventArgs>
    {
        #region ICollection<PropertyChangedEventArgs> Members

        /// <summary>
        /// Add an object to the end of collection</summary>
        /// <param name="item">Item to add</param>
        public void Add(PropertyChangedEventArgs item)
        {
            if (!Contains(item))
            {
                m_innerList.Add(item);
            }
        }

        /// <summary>
        /// Remove all elements from collection</summary>
        public void Clear()
        {
            m_innerList.Clear();
        }

        /// <summary>
        /// Determine whether collection contains specified element using PropertyChangedEventArgsComparer</summary>
        /// <param name="item">Item to check</param>
        /// <returns><c>True</c> if collection contains item</returns>
        public bool Contains(PropertyChangedEventArgs item)
        {
            return m_innerList.Contains<PropertyChangedEventArgs>(item, s_comparer);
        }

        /// <summary>
        /// Copies entire collection to one-dimensional PropertyChangedEventArgs
        /// array, starting at specified index in array</summary>
        /// <param name="array">Array to copy to</param>
        /// <param name="arrayIndex">Index in target array</param>
        public void CopyTo(PropertyChangedEventArgs[] array, int arrayIndex)
        {
            m_innerList.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Get number of items in collection</summary>
        public int Count
        {
            get { return m_innerList.Count; }
        }

        /// <summary>
        /// Get whether collection is read only</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove first occurrence of item from collection</summary>
        /// <param name="item">Item to remove</param>
        /// <returns><c>True</c> if item successfully removed</returns>
        public bool Remove(PropertyChangedEventArgs item)
        {
            return m_innerList.Remove(item);
        }

        #endregion

        #region IEnumerable<PropertyChangedEventArgs> Members

        /// <summary>
        /// Return enumerator that iterates through collection</summary>
        /// <returns>Enumerator that iterates through collection</returns>
        public IEnumerator<PropertyChangedEventArgs> GetEnumerator()
        {
            return m_innerList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_innerList.GetEnumerator();
        }

        #endregion

        private readonly List<PropertyChangedEventArgs> m_innerList = new List<PropertyChangedEventArgs>();

        private static readonly PropertyChangedEventArgsComparer s_comparer = new PropertyChangedEventArgsComparer();

        private class PropertyChangedEventArgsComparer : IEqualityComparer<PropertyChangedEventArgs>
        {
            #region IEqualityComparer<PropertyChangedEventArgs> Members

            public bool Equals(PropertyChangedEventArgs x, PropertyChangedEventArgs y)
            {
                return x.PropertyName.Equals(y.PropertyName);
            }

            public int GetHashCode(PropertyChangedEventArgs obj)
            {
                return obj.PropertyName.GetHashCode();
            }

            #endregion
        }
    }
}
