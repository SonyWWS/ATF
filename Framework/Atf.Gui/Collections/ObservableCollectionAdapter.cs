//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// This class wraps an IObservableCollection of one type to implement IObservableCollection of another type</summary>
    /// <typeparam name="T">Underlying list type</typeparam>
    /// <typeparam name="U">Adapted list type</typeparam>
    /// <remarks>This adapter class can be used to simulate interface covariance, where
    /// an IObservableCollection of Type1 can be made to implement an IObservableCollection of Type2, as long as Type1
    /// implements or can be adapted to Type2.</remarks>
    public class ObservableCollectionAdapter<T, U> : ListAdapter<T, U>, IObservableCollection<U>
        where T : class
        where U : class
    {
        private readonly IObservableCollection<T> m_collection;

        /// <summary>
        /// Constructor with collection</summary>
        /// <param name="collection">Observable collection</param>
        public ObservableCollectionAdapter(IObservableCollection<T> collection)
            : base(collection)
        {
            m_collection = collection;
            m_collection.PropertyChanged += collection_PropertyChanged;
            m_collection.CollectionChanged += collection_CollectionChanged;
        }

        /// <summary>
        /// Handle collection changed event</summary>
        /// <param name="e">Collection changed event arguments</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
        }

        private void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var args = ConvertArgs(e);
            var h = CollectionChanged;
            if (h != null)
            {
                h(this, args);
            }
        }

        private void collection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        private NotifyCollectionChangedEventArgs ConvertArgs(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs result = null;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    result = new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Add, 
                        ConvertList(e.NewItems), 
                        e.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Move:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Remove:
                    result = new NotifyCollectionChangedEventArgs(
                       NotifyCollectionChangedAction.Remove,
                       ConvertList(e.OldItems),
                       e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    throw new NotSupportedException();
                case NotifyCollectionChangedAction.Reset:
                    result = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return result;
        }

        private IList ConvertList(IList list)
        {
            var newItemsConverted = new List<object>();
            foreach (var item in list)
                newItemsConverted.Add(Convert(item as T));
            return newItemsConverted;
        }

        #region IEnumerable Members

        /// <summary>
        /// Return enumerator that iterates through the collection</summary>
        /// <returns>Enumerator for collection</returns>
        public new System.Collections.IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion

        #region IList Members

        /// <summary>
        /// Add an item to the collection</summary>
        /// <param name="value">Object to add to the collection</param>
        /// <returns>Index of added item</returns>
        public int Add(object value)
        {
            base.Add(value as U);
            return base.IndexOf(value as U);
        }

        /// <summary>
        /// Determine whether the collection contains a specific value</summary>
        /// <param name="value">The object to locate in the collection</param>
        /// <returns><c>True</c> if the item is found</returns>
        public bool Contains(object value)
        {
            return base.Contains(value as U);
        }

        /// <summary>
        /// Determine the index of a specific item in the list</summary>
        /// <param name="value">The object to locate in the list</param>
        /// <returns>The index of item if found in the list; otherwise -1</returns>
        public int IndexOf(object value)
        {
            return base.IndexOf(value as U);
        }

        /// <summary>
        /// Insert an item into the list at the specified index</summary>
        /// <param name="index">The zero-based index at which the item should be inserted</param>
        /// <param name="value">The object to insert into the list</param>
        public void Insert(int index, object value)
        {
            base.Insert(index, value as U);
        }

        /// <summary>
        /// Get whether collection's size is fixed</summary>
        public bool IsFixedSize
        {
            get { return false; }
        }

        /// <summary>
        /// Remove the first occurrence of a specific object from the collection</summary>
        /// <param name="value">The object to remove from the collection</param>
        /// <returns><c>True</c> if item was successfully removed from the collection</returns>
        public void Remove(object value)
        {
            base.Remove(value as U);
        }

        /// <summary>
        /// Get or set value at index</summary>
        /// <param name="index">The zero-based index at which the item</param>
        /// <returns>Value at index</returns>
        public new object this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value as U;
            }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Copy collection elements to an array, starting at an index</summary>
        /// <param name="array">Array to copy to</param>
        /// <param name="index">Index to start copying in both collection and array</param>
        public void CopyTo(Array array, int index)
        {
            for (int i = index; i < Count; i++)
                array.SetValue(this[i], i);
        }

        /// <summary>
        /// Get whether access to the collection is synchronized (thread safe)</summary>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Get an object that can be used to synchronize access to the collection</summary>
        public object SyncRoot
        {
            get { return null; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Property changed event</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyCollectionChanged Members

        /// <summary>
        /// Collection changed event</summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }
}
