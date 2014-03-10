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
        /// <param name="node">DomNode whose children will be adapted by this</param>
        /// <param name="childInfo">Metadata indicating which child list to wrap</param>
        public DomNodeListAdapter(DomNode node, ChildInfo childInfo)
        {
            m_nodes = node.GetChildList(childInfo);
        }

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return m_nodes.IndexOf(GetNode(item));
        }

        public void Insert(int index, T item)
        {
            m_nodes.Insert(index, GetNode(item));
        }

        public void RemoveAt(int index)
        {
            m_nodes.RemoveAt(index);
        }

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

        public void Add(T item)
        {
            m_nodes.Add(GetNode(item));
        }

        public void Clear()
        {
            m_nodes.Clear();
        }

        public bool Contains(T item)
        {
            return m_nodes.Contains(GetNode(item));
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = arrayIndex; i < m_nodes.Count; i++)
                array[i] = m_nodes[i].As<T>();
        }

        public int Count
        {
            get { return m_nodes.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return m_nodes.Remove(GetNode(item));
        }

        #endregion

        #region IEnumerable<T> Members

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
