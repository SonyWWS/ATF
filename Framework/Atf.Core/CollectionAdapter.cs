//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// This class wraps an ICollection of one type to implement ICollection of another type</summary>
    /// <typeparam name="T">Underlying list type</typeparam>
    /// <typeparam name="U">Adapted list type</typeparam>
    /// <remarks>This adapter class can be used to simulate interface covariance, where
    /// an ICollection of Type1 can be made to implement an ICollection of Type2, as long as Type1
    /// implements or can be adapted to Type2. To observe changes to the underlying collection,
    /// consider passing in an ObservableCollection of type T.</remarks>
    public class CollectionAdapter<T, U> : ICollection<U>
        where T : class
        where U : class
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="list">List to adapt, must be non-null</param>
        public CollectionAdapter(ICollection<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            m_collection = list;
        }

        #region ICollection<U> Members

        /// <summary>
        /// Adds an item to the collection</summary>
        /// <param name="item">Object to add to the collection</param>
        public void Add(U item)
        {
            if (m_collection.IsReadOnly)
                throw new InvalidOperationException("Collection is read only");
            T t = Convert(item);
            m_collection.Add(t);
        }

        /// <summary>
        /// Removes all items from the collection</summary>
        public void Clear()
        {
            if (m_collection.IsReadOnly)
                throw new InvalidOperationException("Collection is read only");
            m_collection.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains a specific value</summary>
        /// <param name="item">The object to locate in the collection</param>
        /// <returns><c>True</c> if the item is found</returns>
        public bool Contains(U item)
        {
            T t = Convert(item);
            return m_collection.Contains(t);
        }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular array index</summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. 
        /// The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in the array at which copying begins</param>
        public void CopyTo(U[] array, int arrayIndex)
        {
            int i = 0;
            foreach (T t in m_collection)
            {
                array[arrayIndex + i] = Convert(t);
                i++;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the collection</summary>
        /// <returns>Number of elements contained in the collection</returns>
        public int Count
        {
            get { return m_collection.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the collection is read-only</summary>
        /// <returns><c>True</c> if the collection is read-only</returns>
        public bool IsReadOnly
        {
            get { return m_collection.IsReadOnly; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection</summary>
        /// <param name="item">The object to remove from the collection</param>
        /// <returns><c>True</c> if item was successfully removed from the collection</returns>
        public bool Remove(U item)
        {
            if (m_collection.IsReadOnly)
                throw new InvalidOperationException("Collection is read only");
            T t = Convert(item);
            return m_collection.Remove(t);
        }

        #endregion

        #region IEnumerable<U> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection</summary>
        /// <returns>An enumerator that can be used to iterate through the collection</returns>
        public IEnumerator<U> GetEnumerator()
        {
            foreach (T t in m_collection)
                yield return Convert(t);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection</summary>
        /// <returns>An enumerator that can be used to iterate through the collection</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Converts the item to the adapted list type, throwing an InvalidOperationException
        /// if the item can't be converted</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Item, converted to the adapted list type, or null if exception occurred</returns>
        protected virtual T Convert(U item)
        {
            T t = item as T;
            if (t == null && item != null)
                throw new InvalidOperationException("Item of wrong type for adapted collection");

            return t;
        }

        /// <summary>
        /// Converts the item from the adapted list type; returns null if the item can't be
        /// converted</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Item, converted to the adapted list type, or null if the item can't be converted</returns>
        protected virtual U Convert(T item)
        {
            U u = item as U;
            return u;
        }

        private readonly ICollection<T> m_collection;
    }
}
