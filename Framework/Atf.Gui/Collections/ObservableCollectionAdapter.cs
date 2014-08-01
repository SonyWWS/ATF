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

        public ObservableCollectionAdapter(IObservableCollection<T> collection)
            : base(collection)
        {
            m_collection = collection;
            m_collection.PropertyChanged += new PropertyChangedEventHandler(collection_PropertyChanged);
            m_collection.CollectionChanged += new NotifyCollectionChangedEventHandler(collection_CollectionChanged);
        }

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

        public new System.Collections.IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion

        #region IList Members

        public int Add(object value)
        {
            base.Add(value as U);
            return base.IndexOf(value as U);
        }

        public bool Contains(object value)
        {
            return base.Contains(value as U);
        }

        public int IndexOf(object value)
        {
            return base.IndexOf(value as U);
        }

        public void Insert(int index, object value)
        {
            base.Insert(index, value as U);
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            base.Remove(value as U);
        }

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

        public void CopyTo(Array array, int index)
        {
            for (int i = index; i < Count; i++)
                array.SetValue(this[i], i);
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion
    }
}
