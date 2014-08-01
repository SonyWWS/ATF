//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    public class ObservableDomNodeListAdapter<T> : DomNodeListAdapter<T>, IObservableCollection<T>, IObservableCollection
        where T : class
    {
        public ObservableDomNodeListAdapter(DomNode node, ChildInfo childInfo)
            : base(node, childInfo)
        {
            m_node = node;
            m_childInfo = childInfo;
            node.ChildInserted += Node_ChildInserted;
            node.ChildRemoved += Node_ChildRemoved;
        }

        #region IEnumerable Members

        public new IEnumerator GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Event Handlers

        private void Node_ChildRemoved(object sender, ChildEventArgs e)
        {
            OnChildrenChanged(e, NotifyCollectionChangedAction.Remove);
        }

        private void Node_ChildInserted(object sender, ChildEventArgs e)
        {
            OnChildrenChanged(e, NotifyCollectionChangedAction.Add);
        }

        private void OnChildrenChanged(ChildEventArgs e, NotifyCollectionChangedAction action)
        {
            if (e.ChildInfo.IsEquivalent(m_childInfo) && e.Parent == m_node)
            {
                T adapter = e.Child.As<T>();
                System.Diagnostics.Debug.Assert(adapter != null);

                OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, adapter, e.Index));
                OnPropertyChanged(s_countPropertyChangedArgs);
                OnPropertyChanged(s_indexerPropertyChangedArgs);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            T item = (T)value;
            base.Add(item);
            return IndexOf(item);
        }

        bool IList.Contains(object value)
        {
            T item = value as T;
            return item != null ? base.Contains(item) : false;
        }

        int IList.IndexOf(object value)
        {
            T item = value as T;
            if (item == null)
                return -1;

            // DAN: this should be implemented on base
            if (!base.Contains(item))
                return -1;

            return base.IndexOf(item);
        }

        void IList.Insert(int index, object value)
        {
            base.Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            base.Remove((T)value);
        }

        object IList.this[int index]
        {
            get { return base[index]; }
            set { base[index] = (T)value; }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            for (int i = index; i < Count; i++)
                array.SetValue(this[i],i);
            }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        int ICollection.Count
        {
            get { return base.Count; }
        }

        object ICollection.SyncRoot
        {
            get { return m_syncRoot ?? (m_syncRoot = new object()); }
        }

        #endregion

        private object m_syncRoot;
        private readonly ChildInfo m_childInfo;
        private readonly DomNode m_node;
        private static PropertyChangedEventArgs s_countPropertyChangedArgs = new PropertyChangedEventArgs("Count");
        private static PropertyChangedEventArgs s_indexerPropertyChangedArgs = new PropertyChangedEventArgs("[]");
    }
}
