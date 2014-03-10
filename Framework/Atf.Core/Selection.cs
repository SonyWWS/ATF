//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf
{
    /// <summary>
    /// Collection representing a selection. Maintains a LastSelected item, and enumerates
    /// items in least-recently-selected order. Provides change notification, and views
    /// filtered by item type. Uses simple casting to convert to other types.</summary>
    public class Selection<T> : IList<T>
    {
        /// <summary>
        /// Event that is raised before the selection changes</summary>
        public event EventHandler Changing;

        /// <summary>
        /// Event that is raised after the selection changes</summary>
        public event EventHandler Changed;

        /// <summary>
        /// Event that is raised after the selection changes, containing information
        /// about which items were added and removed</summary>
        public event EventHandler<ItemsChangedEventArgs<T>> ItemsChanged;

        /// <summary>
        /// Gets the last selected item, or default</summary>
        public T LastSelected
        {
            get { return m_list.Count == 0 ? default(T) : m_list[m_list.Count - 1]; }
        }

        /// <summary>
        /// Gets the items in the collection in most-recently selected order; the normal
        /// enumerator returns the items in the order in which they were selected</summary>
        public IEnumerable<T> MostRecentOrder
        {
            get
            {
                for (int i = m_list.Count - 1; i >= 0; i--)
                    yield return m_list[i];
            }
        }

        /// <summary>
        /// Gets a snapshot of the selected items</summary>
        /// <returns>Snapshot of the selected items</returns>
        public T[] GetSnapshot()
        {
            return m_list.ToArray();
        }

        /// <summary>
        /// Returns an enumerator for the items that can be converted to the specified type, in least-recently-selected order</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <returns>Enumerator for items that can be converted to the specified type</returns>
        public IEnumerable<U> AsIEnumerable<U>()
            where U : class
        {
            foreach (T t in this)
            {
                U u = Convert<U>(t);
                if (u != null)
                    yield return u;
            }
        }

        /// <summary>
        /// Gets the last selected item of the specified type, or null</summary>
        /// <typeparam name="U">Type implemented by desired item</typeparam>
        /// <returns>Last selected item of the specified type</returns>
        public U GetLastSelected<U>()
            where U : class
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                U u = Convert<U>(this[i]);
                if (u != null)
                    return u;
            }

            return null;
        }

        /// <summary>
        /// Gets a snapshot of the currently selected items of type U</summary>
        /// <typeparam name="U">Type implemented by desired items</typeparam>
        /// <returns>Snapshot of the currently selected items of type U</returns>
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
        /// Converts from the selection type to another given type</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <param name="item">Item in collection</param>
        /// <returns>Item, converted to given type, or null</returns>
        protected virtual U Convert<U>(T item)
            where U : class
        {
            U u = item as U;
            return u;
        }

        /// <summary>
        /// Sets the selection to a single item</summary>
        /// <param name="item">Item to select</param>
        public void Set(T item)
        {
            SetRange(new[] { item });
        }

        /// <summary>
        /// Sets the selection to multiple items</summary>
        /// <param name="items">Items to select</param>
        public void SetRange(IEnumerable<T> items)
        {
            // verify that selection changed
            if (!Equals(items))
            {
                RaiseChanging();

                // check if there are subscribers for detailed change information before doing the work
                List<T> itemsAdded = null;
                List<T> itemsRemoved = null;
                EventHandler<ItemsChangedEventArgs<T>> handler = ItemsChanged;
                if (handler != null)
                {
                    itemsAdded = new List<T>();
                    itemsRemoved = new List<T>();

                    HashSet<T> newItemSet = new HashSet<T>(items);
                    foreach (T item in m_list)
                        if (!newItemSet.Contains(item))
                            itemsRemoved.Add(item);
                    foreach (T item in items)
                        if (!m_set.Contains(item))
                            itemsAdded.Add(item);
                }

                m_list.Clear();
                m_set.Clear();

                foreach (T item in items)
                {
                    m_list.Add(item);
                    m_set.Add(item);
                }

                if (handler != null)
                {
                    handler(
                        this,
                        new ItemsChangedEventArgs<T>(itemsAdded, itemsRemoved, EmptyEnumerable<T>.Instance));
                }

                RaiseChanged();
            }
        }

        /// <summary>
        /// Adds an item to the selection</summary>
        /// <param name="item">Item to add</param>
        public void Add(T item)
        {
            AddRange(new[] { item });
        }

        /// <summary>
        /// Adds multiple items to the selection</summary>
        /// <param name="items">Items to add</param>
        public void AddRange(IEnumerable<T> items)
        {
            List<T> newSelection = new List<T>();

            // get items in selection that aren't being added
            HashSet<T> itemSet = new HashSet<T>(items);
            foreach (T item in m_list)
            {
                if (!itemSet.Contains(item))
                    newSelection.Add(item);
            }

            // append the added items
            newSelection.AddRange(items);

            SetRange(newSelection);
        }

        /// <summary>
        /// Removes an item from the selection</summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True if the item was removed</returns>
        public bool Remove(T item)
        {
            return RemoveRange(new[] { item });
        }

        /// <summary>
        /// Removes multiple items from the selection</summary>
        /// <param name="items">Items to remove</param>
        /// <returns>True if at least one item was removed</returns>
        public bool RemoveRange(IEnumerable<T> items)
        {
            HashSet<T> removed = new HashSet<T>();
            foreach (T item in items)
            {
                removed.Add(item);
            }

            bool removedOne = false;
            List<T> newSelection = new List<T>();
            foreach (T item in m_list)
            {
                if (!removed.Contains(item))
                    newSelection.Add(item);
                else
                    removedOne = true;
            }

            SetRange(newSelection);

            return removedOne;
        }

        /// <summary>
        /// Toggles an item in the selection</summary>
        /// <param name="item">Item to toggle</param>
        public void Toggle(T item)
        {
            ToggleRange(new T[] { item });
        }

        /// <summary>
        /// Toggles multiple items in the selection</summary>
        /// <param name="items">Items to toggle</param>
        public void ToggleRange(IEnumerable<T> items)
        {
            HashSet<T> toggled = new HashSet<T>();
            foreach (T item in items)
            {
                toggled.Add(item);
            }

            List<T> newSelection = new List<T>();
            // keep already selected items that aren't to be toggled
            foreach (T item in m_list)
                if (!toggled.Contains(item))
                    newSelection.Add(item);

            // add toggled items that aren't in selection
            foreach (T item in items)
            {
                if (!Contains(item))
                    newSelection.Add(item);
            }

            SetRange(newSelection);
        }

        /// <summary>
        /// Checks if this selection has the same contents in the same order as the given collection</summary>
        /// <param name="collection">Collection to compare</param>
        /// <returns>True iff the contents are the same and in the same order</returns>
        public bool Equals(ICollection<T> collection)
        {
            // Same object?
            if (object.ReferenceEquals(this, collection))
                return true;

            if (m_list.Count != collection.Count)
                return false;

            return this.SequenceEqual(collection);
        }

        /// <summary>
        /// Checks if objects have the same contents in the same order.
        /// Comparison against ICollection or IEnumerable of type T is valid.</summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True iff the contents are the same and in the same order</returns>
        public override bool Equals(object obj)
        {
            // Note that the Linq IEnumerable test is not smart about testing if ICollection is implemented.
            // We have use-cases of comparing against arrays.
            // http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=361633
            ICollection<T> otherCollection = obj as ICollection<T>;
            if (otherCollection != null)
                return Equals(otherCollection);

            IEnumerable<T> otherEnumerable = obj as IEnumerable<T>;
            if (otherEnumerable != null)
                return this.SequenceEqual(otherEnumerable);

            return false;
        }

        /// <summary>
        /// Gets the hash code for the object</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            int result = 0;
            foreach (T item in m_list)
                result ^= item.GetHashCode();

            return result;
        }

        /// <summary>
        /// Begins an update to the selection, with notification events turned off. Raises the
        /// Changing event immediately, even if the selection set does not change.</summary>
        /// <remarks>Consider using a try-finally block to ensure that EndUpdate is called</remarks>
        public void BeginUpdate()
        {
            if (m_updating)
                throw new InvalidOperationException("Can't nest updates");

            RaiseChanging();
            m_updating = true;
        }

        /// <summary>
        /// Ends an update to the selection and turns on notification events. If the
        /// selection has changed since the call to BeginUpdate, the SelectionChanged
        /// event is immediately raised.</summary>
        /// <remarks>Consider using a try-finally block to ensure that EndUpdate is called</remarks>
        public void EndUpdate()
        {
            if (!m_updating)
                throw new InvalidOperationException("Not updating");

            m_updating = false;
            RaiseChanged();
        }

        #region IList<T> Members

        /// <summary>
        /// Gets the index of the item</summary>
        /// <param name="item">Item to get index of</param>
        /// <returns>Index of the item, or -1 if not in selection</returns>
        public int IndexOf(T item)
        {
            if (!m_set.Contains(item))
                return -1;

            return m_list.IndexOf(item);
        }

        /// <summary>
        /// Inserts the item in the selection</summary>
        /// <param name="index">Desired index of item</param>
        /// <param name="item">Item to insert</param>
        /// <remarks>Note that if the item is already in the selection, it is first removed and
        /// then added at the end. To reduce change events, consider bracketing this method with BeginUpdate/EndUpdate calls
        /// if multiple items are inserted.</remarks>
        public void Insert(int index, T item)
        {
            RaiseChanging();

            if (m_set.Contains(item))
            {
                int oldIndex = m_list.IndexOf(item);
                m_list.RemoveAt(oldIndex);
                if (index > oldIndex)
                    index--;
            }

            m_list.Insert(index, item);
            m_set.Add(item);

            RaiseChanged();
        }

        /// <summary>
        /// Removes the item at the given index</summary>
        /// <param name="index">Index of item to remove</param>
        /// <remarks>To reduce change events, consider bracketing this method with BeginUpdate/EndUpdate calls if multiple
        /// items are removed.</remarks>
        public void RemoveAt(int index)
        {
            T item = m_list[index];

            RaiseChanging();

            m_list.RemoveAt(index);
            m_set.Remove(item);

            RaiseChanged();
        }

        /// <summary>
        /// Gets or sets the item in the selection at the given index</summary>
        /// <param name="index">Index of item</param>
        /// <returns>Item at the given index</returns>
        public T this[int index]
        {
            get
            {
                return m_list[index];
            }
            set
            {
                T item = m_list[index];
                if (!(item.Equals(value)))
                {
                    RaiseChanging();

                    m_list[index] = value; // add new item to list

                    m_set.Remove(item); // remove old item

                    // if value was already in selection, remove it at its old position; otherwise,
                    //  add it to set
                    if (m_set.Contains(value))
                        m_list.Remove(value);
                    else
                        m_set.Add(value);
                    
                    RaiseChanged();
                }
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Gets the number of items in the selection</summary>
        public int Count
        {
            get { return m_list.Count; }
        }

        /// <summary>
        /// Checks if selection contains an item</summary>
        /// <param name="item">Item to search for</param>
        /// <returns>True if item is in selection</returns>
        public bool Contains(T item)
        {
            return m_set.Contains(item);
        }

        /// <summary>
        /// Clears the selection</summary>
        public void Clear()
        {
            SetRange(EmptyEnumerable<T>.Instance);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only</summary>
        /// <returns>Always false</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Copies to an array at the index position</summary>
        /// <param name="array">Array</param>
        /// <param name="index">Index</param>
        public void CopyTo(T[] array, int index)
        {
            m_list.CopyTo(array, index);
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns enumerator for selection</summary>
        /// <returns>Enumerator for selection</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Raises the Changing event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnChanging(EventArgs e)
        {
            Changing.Raise(this, e);
        }

        private void RaiseChanging()
        {
            if (!m_updating)
                OnChanging(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the Changed event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnChanged(EventArgs e)
        {
            Changed.Raise(this, e);
        }

        private void RaiseChanged()
        {
            if (!m_updating)
                OnChanged(EventArgs.Empty);
        }

        private readonly List<T> m_list = new List<T>();
        private readonly HashSet<T> m_set = new HashSet<T>();
        private bool m_updating;
    }
}

