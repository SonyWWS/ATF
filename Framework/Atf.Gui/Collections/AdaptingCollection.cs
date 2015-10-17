//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// Collection that filters the items by the specified filter</summary>
    /// <typeparam name="T">Type of the objects in the collection</typeparam>
    /// <typeparam name="M">Metadata about the type T</typeparam>
    public class FilteringCollection<T, M> : AdaptingCollection<T, M>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="filter">Filter function to apply to items in the collection</param>
        public FilteringCollection(Func<Lazy<T, M>, bool> filter)
            : base(e => e.Where(filter))
        {
        }
    }

    /// <summary>
    /// Collection that orders the items by the specified order</summary>
    /// <typeparam name="T">Type of the objects in the collection</typeparam>
    /// <typeparam name="M">Metadata about the type T</typeparam>
    public class OrderingCollection<T, M> : AdaptingCollection<T, M>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="keySelector">Key selector function</param>
        /// <param name="descending">True to sort in descending order</param>
        public OrderingCollection(Func<Lazy<T, M>, object> keySelector, bool descending = false)
            : base(e => descending ? e.OrderByDescending(keySelector) : e.OrderBy(keySelector))
        {
        }
    }

    /// <summary>
    /// Base class for collections that can sort or filter their contents</summary>
    /// <typeparam name="T">Type of items in the collection</typeparam>
    public class AdaptingCollection<T> : AdaptingCollection<T, IDictionary<string, object>>
    {
        /// <summary>
        /// Constructor with adapter function</summary>
        /// <param name="adaptor">Function to apply to items in the collection</param>
        public AdaptingCollection(Func<IEnumerable<Lazy<T, IDictionary<string, object>>>,
                                       IEnumerable<Lazy<T, IDictionary<string, object>>>> adaptor)
            : base(adaptor)
        {
        }
    }

    /// <summary>
    /// Base class for collections that can sort or filter their contents</summary>
    /// <typeparam name="T">Type of items in the collection</typeparam>
    /// <typeparam name="M">Metadata about type T</typeparam>
    public class AdaptingCollection<T, M> : ICollection<Lazy<T, M>>, INotifyCollectionChanged
    {
        /// <summary>
        /// Constructor</summary>
        public AdaptingCollection()
            : this(null)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="adaptor">Function to apply to items in the collection</param>
        public AdaptingCollection(Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> adaptor)
        {
            m_adaptor = adaptor;
        }

        /// <summary>
        /// CollectionChanged event for INotifyCollectionChanged</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Force the adaptor function to be run again</summary>
        public void ReapplyAdaptor()
        {
            if (m_adaptedItems != null)
            {
                m_adaptedItems = null;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        #region ICollection Implementation

        /// <summary>
        /// Returns whether the item is present in the collection</summary>
        /// <remarks>Accessors work directly against adapted collection</remarks>
        /// <param name="item">Item to look for</param>
        /// <returns><c>True</c> if the item is in the collection</returns>
        public bool Contains(Lazy<T, M> item)
        {
            return AdaptedItems.Contains(item);
        }

        /// <summary>
        /// Copies the entire list to a one-dimensional array, starting at the specified index of the target array</summary>
        /// <remarks>Accessors work directly against adapted collection</remarks>
        /// <param name="array">The target array</param>
        /// <param name="arrayIndex">The starting index</param>
        public void CopyTo(Lazy<T, M>[] array, int arrayIndex)
        {
            AdaptedItems.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of items in the collection</summary>
        /// <remarks>Accessors work directly against adapted collection</remarks>
        public int Count
        {
            get { return AdaptedItems.Count; }
        }

        /// <summary>
        /// Gets whether the collection is read only.</summary>
        /// <remarks>Accessors work directly against adapted collection</remarks>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an enumerator for the collection</summary>
        /// <remarks>Accessors work directly against adapted collection</remarks>
        /// <returns>The IEnumerator</returns>
        public IEnumerator<Lazy<T, M>> GetEnumerator()
        {
            return AdaptedItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Add an item to the collection</summary>
        /// <remarks>Mutation methods work against complete collection and then force
        /// a reset of the adapted collection</remarks>
        /// <param name="item">The item to add</param>
        public void Add(Lazy<T, M> item)
        {
            m_allItems.Add(item);
            ReapplyAdaptor();
        }

        /// <summary>
        /// Clear all items from the collection</summary>
        /// <remarks>Mutation methods work against complete collection and then force
        /// a reset of the adapted collection</remarks>
        public void Clear()
        {
            m_allItems.Clear();
            ReapplyAdaptor();
        }

        /// <summary>
        /// Remove an item from the collection</summary>
        /// <remarks>Mutation methods work against complete collection and then force
        /// a reset of the adapted collection</remarks>
        /// <param name="item">The item to remove</param>
        /// <returns><c>True</c> if the item was found, otherwise false</returns>
        public bool Remove(Lazy<T, M> item)
        {
            bool removed = m_allItems.Remove(item);
            ReapplyAdaptor();
            return removed;
        }

        #endregion

        /// <summary>
        /// Invoke the adaptor function on the collection</summary>
        /// <param name="collection">The collection to adapt</param>
        /// <returns>The adapted collection</returns>
        protected virtual IEnumerable<Lazy<T, M>> Adapt(IEnumerable<Lazy<T, M>> collection)
        {
            if (m_adaptor != null)
            {
                return m_adaptor.Invoke(collection);
            }

            return collection;
        }

        /// <summary>
        /// Fire the CollectionChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var collectionChanged = CollectionChanged;
            if (collectionChanged != null)
            {
                collectionChanged.Invoke(this, e);
            }
        }

        private List<Lazy<T, M>> AdaptedItems
        {
            get { return m_adaptedItems ?? (m_adaptedItems = Adapt(m_allItems).ToList()); }
        }

        private readonly List<Lazy<T, M>> m_allItems = new List<Lazy<T, M>>();
        private readonly Func<IEnumerable<Lazy<T, M>>, IEnumerable<Lazy<T, M>>> m_adaptor = null;
        private List<Lazy<T, M>> m_adaptedItems = null;

    }
}