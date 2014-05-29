//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Sce.Atf
{
    /// <summary>
    /// Class to represent a path in a tree or graph. Path supports a limited form
    /// of IList, not allowing add, remove, insert, or clear.</summary>
    /// <typeparam name="T">Type of items in path</typeparam>
    public class Path<T> : IList<T>, IEquatable<Path<T>>
    {
        /// <summary>
        /// Constructor using single object</summary>
        /// <param name="last">Single object making up the path</param>
        public Path(T last)
        {
            m_path = new T[1];
            m_path[0] = last;
        }

        /// <summary>
        /// Constructor using sequence of objects</summary>
        /// <param name="path">Path, as sequence of objects</param>
        public Path(IEnumerable<T> path)
        {
            m_path = path.ToArray();
        }

        /// <summary>
        /// Constructor using collection of objects</summary>
        /// <param name="path">Path, as collection of objects</param>
        public Path(ICollection<T> path)
        {
            m_path = new T[path.Count];
            path.CopyTo(m_path, 0);
        }

        /// <summary>
        /// Gets or sets the first object in the path</summary>
        public T First
        {
            get { return m_path[0]; }
            set { m_path[0] = value; }
        }

        /// <summary>
        /// Gets or sets the last object in the path</summary>
        public T Last
        {
            get { return m_path[m_path.Length - 1]; }
            set { m_path[m_path.Length - 1] = value; }
        }

        /// <summary>
        /// Obtains a prefix with the specified length</summary>
        /// <param name="length">Prefix length</param>
        /// <returns>Prefix with the specified length</returns>
        public Path<T> Prefix(int length)
        {
            CheckSubPathLength(length);

            T[] path = new T[length];
            Array.Copy(m_path, 0, path, 0, length);

            return new Path<T>(path);
        }

        /// <summary>
        /// Obtains a suffix with the specified length</summary>
        /// <param name="length">Suffix length</param>
        /// <returns>Suffix with the specified length</returns>
        public Path<T> Suffix(int length)
        {
            CheckSubPathLength(length);

            T[] path = new T[length];
            int offset = m_path.Length - length;
            Array.Copy(m_path, offset, path, 0, length);

            return new Path<T>(path);
        }

        /// <summary>
        /// Converts path to a path of another type</summary>
        /// <typeparam name="U">Path type to convert to</typeparam>
        /// <returns>Path of new type</returns>
        public Path<U> Convert<U>()
            where U : class
        {
            U[] converted = new U[m_path.Length];
            for (int i = 0; i < m_path.Length; i++)
                converted[i] = Convert<U>(m_path[i]);
            return new Path<U>(converted);
        }

        /// <summary>
        /// Converts from the path type to another type</summary>
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
        /// Tests path for equality</summary>
        /// <param name="other">Other path</param>
        /// <returns>True iff this path is equivalent to other</returns>
        public bool Equals(Path<T> other)
        {
            if (object.Equals(other, null))
                return false;

            if (m_path.Length != other.m_path.Length)
                return false;

            for (int i = 0; i < m_path.Length; i++)
                if (!m_path[i].Equals(other.m_path[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Tests object for equality</summary>
        /// <param name="obj">Other object</param>
        /// <returns>True iff this path is equivalent to other object</returns>
        public override bool Equals(object obj)
        {
            Path<T> path = obj as Path<T>;
            if (path != null)
                return Equals(path);

            return false;
        }

        /// <summary>
        /// Obtains hash code</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            int hash = 0;
            foreach (T obj in m_path)
                hash ^= obj.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Tests paths for equality</summary>
        /// <param name="o1">First path</param>
        /// <param name="o2">Second path</param>
        /// <returns>True iff paths are equivalent</returns>
        public static bool operator ==(Path<T> o1, Path<T> o2)
        {
            if (object.Equals(o1, null))
                return object.Equals(o2, null);
            else
                return o1.Equals(o2);
        }

        /// <summary>
        /// Tests paths for inequality</summary>
        /// <param name="o1">First path</param>
        /// <param name="o2">Second path</param>
        /// <returns>True iff paths are not equivalent</returns>
        public static bool operator !=(Path<T> o1, Path<T> o2)
        {
            if (object.Equals(o1, null))
                return !object.Equals(o2, null);
            else
                return !o1.Equals(o2);
        }

        /// <summary>
        /// Concatenates object with path</summary>
        /// <param name="lhs">Prefix object</param>
        /// <param name="rhs">Optional path, may be null</param>
        /// <returns>Concatenated path, with lhs as first object</returns>
        public static Path<T> operator +(T lhs, Path<T> rhs)
        {
            if (rhs == null)
                return new Path<T>(lhs);
            T[] path = new T[1 + rhs.Count];
            path[0] = lhs;
            Array.Copy(rhs.m_path, 0, path, 1, rhs.Count);
            return new Path<T>(path);
        }

        /// <summary>
        /// Concatenates path with object</summary>
        /// <param name="lhs">Optional path, may be null</param>
        /// <param name="rhs">Suffix object</param>
        /// <returns>Concatenated path, with rhs as last object</returns>
        public static Path<T> operator +(Path<T> lhs, T rhs)
        {
            if (lhs == null)
                return new Path<T>(rhs);
            T[] path = new T[lhs.Count + 1];
            Array.Copy(lhs.m_path, 0, path, 0, lhs.Count);
            path[path.Length - 1] = rhs;
            return new Path<T>(path);
        }

        /// <summary>
        /// Concatenates two paths</summary>
        /// <param name="lhs">First path. Can be null.</param>
        /// <param name="rhs">Second path. Can be null.</param>
        /// <returns>Concatenated path, with rhs as prefix and lhs as suffix. Is null if both lhs and rhs are null.</returns>
        public static Path<T> operator +(Path<T> lhs, Path<T> rhs)
        {
            if (lhs == null)
                return rhs;
            if (rhs == null)
                return lhs;
            T[] path = new T[lhs.Count + rhs.Count];
            Array.Copy(lhs.m_path, 0, path, 0, lhs.Count);
            Array.Copy(rhs.m_path, 0, path, lhs.Count, rhs.Count);
            return new Path<T>(path);
        }

        /// <summary>
        /// Gets the enumeration of each path's last item; i.e., the property 'Last'</summary>
        /// <param name="paths">Enumeration of Path objects, whose Last property is returned</param>
        /// <returns>Last property of each Path in 'paths', in the same order as 'paths'</returns>
        public static IEnumerable<T> GetLastItems(IEnumerable<Path<T>> paths)
        {
            foreach (Path<T> path in paths)
                yield return path.Last;
        }

        #region IList<T> Members

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see></summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see></param>
        /// <returns>
        /// The index of item if found in the list; otherwise -1
        /// </returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < m_path.Length; i++)
                if (m_path[i].Equals(item))
                    return i;

            return -1;
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index</summary>
        /// <param name="index">The zero-based index at which item should be inserted</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see></param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see></exception>
        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index</summary>
        /// <param name="index">The zero-based index of the item to remove</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see></exception>
        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets or sets the item at the specified index</summary>
        /// <value>Index at which to set value</value>
        public T this[int index]
        {
            get { return m_path[index]; }
            set { m_path[index] = value; }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see></summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see></param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// is read-only</exception>
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see></summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> 
        /// is read-only</exception>
        public void Clear()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value</summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see></param>
        /// <returns>
        /// True iff item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>
        /// </returns>
        public bool Contains(T item)
        {
            foreach (T obj in m_path)
                if (obj.Equals(item))
                    return true;

            return false;
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, 
        /// starting at a particular <see cref="T:System.Array"></see> index</summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements 
        /// copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. 
        /// The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">ArrayIndex is less than 0</exception>
        /// <exception cref="T:System.ArgumentNullException">Array is null</exception>
        /// <exception cref="T:System.ArgumentException">Array is multidimensional.-or-
        /// arrayIndex is equal to or greater than the length of array.-or-
        /// The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than 
        /// the available space from arrayIndex to the end of the destination array.-or-
        /// Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_path.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see></summary>
        public int Count
        {
            get { return m_path.Length; }
        }

        /// <summary>
        /// Gets whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see></summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see></param>
        /// <returns>
        /// True iff item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>. 
        /// This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only</exception>
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection</summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)m_path).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_path.GetEnumerator();
        }

        #endregion

        /// <summary>
        /// Private constructor</summary>
        private Path(T[] path)
        {
            m_path = path;
        }

        private void CheckSubPathLength(int length)
        {
            if (length < 1)
                throw new InvalidOperationException("Length must be > 0");
            if (length > m_path.Length)
                throw new InvalidOperationException("Length greater than path length");
        }

        private readonly T[] m_path;
    }
}
