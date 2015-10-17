//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Wrapper that adapts a node child list to a list of T items</summary>
    /// <typeparam name="T">Adapted list item type that adapts a DomNode or is a DomNode.
    /// Should implement IAdaptable. Examples include DomNodeAdapter, DomNode, and IAdapter.</typeparam>
    public class DomNodeListAdapter<T> : IList<T>
        where T : class
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="node">DomNode whose children are adapted by this</param>
        /// <param name="childInfo">Metadata indicating which child list to wrap</param>
        public DomNodeListAdapter(DomNode node, ChildInfo childInfo)
        {
            m_nodes = node.GetChildList(childInfo);
        }

        #region IList<T> Members

        /// <summary>
        /// Returns index of given DomNode in adapted list</summary>
        /// <param name="item">DomNode whose index is found</param>
        /// <returns>Zero-based index of given DomNode</returns>
        public int IndexOf(T item)
        {
            return m_nodes.IndexOf(GetNode(item));
        }

        /// <summary>
        /// Insert DomNode at index in adapted list</summary>
        /// <param name="index">Zero-based index to insert at</param>
        /// <param name="item">DomNode inserted</param>
        public void Insert(int index, T item)
        {
            m_nodes.Insert(index, GetNode(item));
        }

        /// <summary>
        /// Removes DomNode at index from adapted list</summary>
        /// <param name="index">Zero-based index at which to remove DomNode</param>
        public void RemoveAt(int index)
        {
            m_nodes.RemoveAt(index);
        }

        /// <summary>
        /// Gets DomNode at given index</summary>
        /// <param name="index">Zero-based index at which to retrieve DomNode</param>
        /// <returns>Retrieved DomNode</returns>
        public T this[int index]
        {
            get
            {
                return m_nodes[index].As<T>();
            }
            set
            {
                m_nodes[index] = GetNode(value);
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        ///  Adds a DomNode to the adapted list</summary>
        /// <param name="item">DomNode to add</param>
        public void Add(T item)
        {
            m_nodes.Add(GetNode(item));
        }

        /// <summary>
        /// Removes all DomNodes from the adapted list</summary>
        public void Clear()
        {
            m_nodes.Clear();
        }

        /// <summary>
        /// Determines whether the adapted list contains a specific DomNode</summary>
        /// <param name="item">DomNode to find</param>
        /// <returns><c>True</c> if the adapted list contains the specified DomNode</returns>
        public bool Contains(T item)
        {
            return m_nodes.Contains(GetNode(item));
        }

        /// <summary>
        /// Copies the elements of the given array to the adapted list,
        /// starting at a given index</summary>
        /// <param name="array">Array to copy into the adapted list</param>
        /// <param name="arrayIndex">Zero-based index at which to start copy</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < m_nodes.Count; i++)
                array[i] = m_nodes[i].As<T>();
        }

        /// <summary>
        /// Gets the number of elements contained in the adapted list</summary>
        public int Count
        {
            get { return m_nodes.Count; }
        }

        /// <summary>
        /// Gets whether the adapted list is read only</summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of the specified DomNode from the adapted list</summary>
        /// <param name="item">DomNode to remove</param>
        /// <returns><c>True</c> if DomNode found and removed</returns>
        public bool Remove(T item)
        {
            return m_nodes.Remove(GetNode(item));
        }

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the adapted list</summary>
        /// <returns>Enumerator that iterates through the adapted list</returns>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (DomNode node in m_nodes)
            {
                T adapter = node.As<T>();
                yield return adapter;
            }
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the adapted list</summary>
        /// <returns>Enumerator that iterates through the adapted list</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private DomNode GetNode(T item)
        {
            DomNode adaptedNode = item.As<DomNode>();
            if (adaptedNode == null)
                throw new InvalidOperationException("item must be adaptable to a DomNode");

            return adaptedNode;
        }

        private readonly IList<DomNode> m_nodes;
    }
}
