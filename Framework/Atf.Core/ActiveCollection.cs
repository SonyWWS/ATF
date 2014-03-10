//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Collection representing active items. Maintains an ActiveItem, and enumerates
    /// items in least-recently-active order. Provides change notification, and views
    /// filtered by item type.</summary>
    /// <typeparam name="T">Type of item in collection, must be reference type</typeparam>
    public class ActiveCollection<T> : ICollection<T>
        where T : class
    {
        /// <summary>
        /// Constructor</summary>
        public ActiveCollection()
            : this(int.MaxValue)
        {
        }

        /// <summary>
        /// Constructor using maximum items in collection</summary>
        /// <param name="maximumCount">Maximum number of items in collection. When collection
        /// is full, the least recently active item is dropped.</param>
        public ActiveCollection(int maximumCount)
        {
            if (maximumCount <= 0)
                throw new ArgumentException("maximumCount must be > 0");

            m_maximumCount = maximumCount;
        }

        /// <summary>
        /// Gets or sets the maximum number of items in collection. When collection
        /// is full, the least recently active item is dropped.</summary>
        public int MaximumCount
        {
            get { return m_maximumCount; }
            set
            {
                // remove excess elements, beginning with least recently used
                while (m_list.Count > value)
                    Remove(m_list[0]);

                m_maximumCount = value;
            }
        }

        /// <summary>
        /// Gets or sets the active item. If the item is not in the collection,
        /// it is added</summary>
        public virtual T ActiveItem
        {
            get
            {
                return (m_list.Count > 0) ? m_list[m_list.Count - 1] : null;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                T oldItem = ActiveItem;
                if (value != oldItem)
                {
                    OnActiveItemChanging(EventArgs.Empty);

                    int index = m_list.IndexOf(value);
                    if (index >= 0)
                    {
                        // already in collection, make it the most recent item
                        m_list.RemoveAt(index);
                        m_list.Add(value);
                    }
                    else
                    {
                        // if maximum will be exceeded, remove least recent item
                        if (m_list.Count == m_maximumCount)
                            Remove(m_list[0]);

                        // add new item to collection
                        m_list.Add(value);
                        OnItemAdded(new ItemInsertedEventArgs<T>(0, value));
                    }

                    OnActiveItemChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets an enumeration of the items in the collection in most-recently active order; the normal
        /// enumerator returns the items in the order in which they were activated</summary>
        public IEnumerable<T> MostRecentOrder
        {
            get
            {
                for (int i = m_list.Count - 1; i >= 0; i--)
                    yield return m_list[i];
            }
        }

        /// <summary>
        /// Gets a snapshot of the current collection items</summary>
        /// <returns>Snapshot of the current collection items in array</returns>
        public T[] GetSnapshot()
        {
            return m_list.ToArray();
        }

        /// <summary>
        /// Gets the most recently active item that can be converted to the specified type</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <returns>The most recently active item that can be converted to the specified type</returns>
        public U GetActiveItem<U>()
            where U : class
        {
            for (int i = m_list.Count - 1; i >= 0; i--)
            {
                U u = Convert<U>(m_list[i]);
                if (u != null)
                    return u;
            }

            return null;
        }

        /// <summary>
        /// Gets an enumeration of the items that can be converted to the specified type, in least-recently-active order</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <returns>Enumeration of items that can be converted to the specified type</returns>
        public IEnumerable<U> AsIEnumerable<U>()
            where U : class
        {
            foreach (T item in this)
            {
                U u = Convert<U>(item);
                if (u != null)
                    yield return u;
            }
        }

        /// <summary>
        /// Gets a snapshot of the current collection items of type U</summary>
        /// <typeparam name="U">Type implemented by desired items</typeparam>
        /// <returns>Snapshot of the current collection items of type U in array</returns>
        public U[] GetSnapshot<U>()
            where U : class
        {
            List<U> list = new List<U>();
            foreach (T item in this)
            {
                U u = Convert<U>(item);
                if (u != null)
                    list.Add(u);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Converts from the collection type to another type</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <param name="item">Item to convert</param>
        /// <returns>Item, converted to given type, or null</returns>
        protected virtual U Convert<U>(T item)
            where U : class
        {
            U u = item as U;
            return u;
        }

        /// <summary>
        /// Event that is raised before the active item changes</summary>
        public event EventHandler ActiveItemChanging;

        /// <summary>
        /// Event that is raised after the active item changes</summary>
        public event EventHandler ActiveItemChanged;

        /// <summary>
        /// Raises the ActiveItemChanging event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnActiveItemChanging(EventArgs e)
        {
            ActiveItemChanging.Raise(this, e);
        }

        /// <summary>
        /// Raises the ActiveItemChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnActiveItemChanged(EventArgs e)
        {
            ActiveItemChanged.Raise(this, e);
        }

        /// <summary>
        /// Event that is raised when an item is added</summary>
        public event EventHandler<ItemInsertedEventArgs<T>> ItemAdded;

        /// <summary>
        /// Raises the ItemAdded event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnItemAdded(ItemInsertedEventArgs<T> e)
        {
            ItemAdded.Raise(this, e);
        }

        /// <summary>
        /// Clears the collection and sets it to the new items</summary>
        /// <param name="items">Active items, ordered from least to most recently active</param>
        public void Set(IEnumerable<T> items)
        {
            object oldActive = ActiveItem;

            ClearInternal();
            int i = 0;
            foreach (T item in items)
            {
                m_list.Add(item);
                OnItemAdded(new ItemInsertedEventArgs<T>(i++, item));
            }

            if (oldActive != ActiveItem)
                OnActiveItemChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Removes an item from the collection</summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True iff item was removed from collection</returns>
        public bool Remove(T item)
        {
            object oldActive = ActiveItem;
            int index = m_list.IndexOf(item);
            if (index >= 0)
            {
                m_list.RemoveAt(index);
                OnItemRemoved(new ItemRemovedEventArgs<T>(index, item));

                if (oldActive != ActiveItem)
                    OnActiveItemChanged(EventArgs.Empty);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Event that is raised when an item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<T>> ItemRemoved;

        /// <summary>
        /// Raises the ItemRemoved event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnItemRemoved(ItemRemovedEventArgs<T> e)
        {
            ItemRemoved.Raise(this, e);
        }

        /// <summary>
        /// Clears the collection</summary>
        public void Clear()
        {
            object oldActive = ActiveItem;

            ClearInternal();

            if (oldActive != ActiveItem)
                OnActiveItemChanged(EventArgs.Empty);
        }

        private void ClearInternal()
        {
            for (int i = m_list.Count - 1; i >= 0; i--)
            {
                T item = m_list[i];
                m_list.RemoveAt(i);
                OnItemRemoved(new ItemRemovedEventArgs<T>(i, item));
            }
        }

        #region ICollection<T> Members

        /// <summary>
        /// Add item to collection and make it the active item</summary>
        /// <param name="item">Added item</param>
        public void Add(T item)
        {
            ActiveItem = item;
        }

        /// <summary>
        /// Test if collection contains item</summary>
        /// <param name="item">Item tested</param>
        /// <returns>True iff item in collection</returns>
        public bool Contains(T item)
        {
            return m_list.Contains(item);
        }

        /// <summary>
        /// Copy collection to array starting at index</summary>
        /// <param name="array">Array to copy collection items to</param>
        /// <param name="arrayIndex">Starting index</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets number of items actually in collection</summary>
        public int Count
        {
            get { return m_list.Count; }
        }

        /// <summary>
        /// Gets indicator that collection is read only</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns collection enumerator</summary>
        /// <returns>Collection enumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Gets the raw list used to store the items in this collection. Changes to this list
        /// will not raise events.</summary>
        internal protected IList<T> RawList
        {
            get { return m_list; }
        }

        private readonly List<T> m_list = new List<T>();
        private int m_maximumCount;
    }
}

