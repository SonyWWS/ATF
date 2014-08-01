//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf
{
    /// <summary>
    /// Special collection for PropertyChangedEventArgs - ensures no matching args sets are added
    /// </summary>
    public class PropertyChangedEventArgsCollection : ICollection<PropertyChangedEventArgs>
    {
        #region ICollection<PropertyChangedEventArgs> Members

        public void Add(PropertyChangedEventArgs item)
        {
            if (!Contains(item))
            {
                m_innerList.Add(item);
            }
        }

        public void Clear()
        {
            m_innerList.Clear();
        }

        public bool Contains(PropertyChangedEventArgs item)
        {
            return m_innerList.Contains<PropertyChangedEventArgs>(item, s_comparer);
        }

        public void CopyTo(PropertyChangedEventArgs[] array, int arrayIndex)
        {
            m_innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return m_innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(PropertyChangedEventArgs item)
        {
            return m_innerList.Remove(item);
        }

        #endregion

        #region IEnumerable<PropertyChangedEventArgs> Members

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
