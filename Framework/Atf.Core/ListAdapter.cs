//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// This class wraps an IList of one type to implement an IList of another type</summary>
    /// <typeparam name="T">Underlying list type</typeparam>
    /// <typeparam name="U">Adapted list type</typeparam>
    /// <remarks>This adapter class can be used to simulate interface covariance, where
    /// an IList of Type1 can be made to implement an IList of Type2, as long as Type1
    /// implements or can be adapted to Type2.</remarks>
    public class ListAdapter<T, U> : CollectionAdapter<T, U>, IList<U>
        where T : class
        where U : class
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="list">List to adapt</param>
        public ListAdapter(IList<T> list)
            : base(list)
        {
            m_list = list;
        }

        #region IList<U> Members

        /// <summary>
        /// Determines the index of a specific item in the list</summary>
        /// <param name="item">The object to locate in the list</param>
        /// <returns>The index of item if found in the list; otherwise -1</returns>
        public int IndexOf(U item)
        {
            T t = Convert(item);
            return m_list.IndexOf(t);
        }

        /// <summary>
        /// Inserts an item into the list at the specified index</summary>
        /// <param name="index">The zero-based index at which the item should be inserted</param>
        /// <param name="item">The object to insert into the list</param>
        public void Insert(int index, U item)
        {
            if (m_list.IsReadOnly)
                throw new InvalidOperationException("Collection is read only");
            T t = Convert(item);
            m_list.Insert(index, t);
        }

        /// <summary>
        /// Removes the list item at the specified index</summary>
        /// <param name="index">The zero-based index of the item to remove</param>
        public void RemoveAt(int index)
        {
            if (m_list.IsReadOnly)
                throw new InvalidOperationException("Collection is read only");
            m_list.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the converted item at the specified index</summary>
        public U this[int index]
        {
            get
            {
                return Convert(m_list[index]);
            }
            set
            {
                if (m_list.IsReadOnly)
                    throw new InvalidOperationException("Collection is read only");
                T t = Convert(value);
                m_list[index] = t;
            }
        }

        #endregion

        private readonly IList<T> m_list;
    }
}
