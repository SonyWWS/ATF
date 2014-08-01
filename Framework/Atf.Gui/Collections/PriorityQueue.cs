//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// Queue of items of type T, sorted by priority of type TPriority</summary>
    /// <typeparam name="T">Type of the items in the queue</typeparam>
    /// <typeparam name="TPriority">Type of the priority to sort by</typeparam>
    public class PriorityQueue<T, TPriority>
    {
        /// <summary>
        /// Gets and sets items in the queue by index</summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public TPriority this[T item]
        {
            get { return m_heap[m_indexes[item]].Value; }
            set
            {
                int index;
                if (m_indexes.TryGetValue(item, out index))
                {
                    int num = m_comparer.Compare(value, m_heap[index].Value);
                    if (num != 0)
                    {
                        if (m_invert)
                        {
                            num = ~num;
                        }
                        KeyValuePair<T, TPriority> element = new KeyValuePair<T, TPriority>(item, value);
                        if (num < 0)
                        {
                            MoveUp(element, index);
                            return;
                        }
                        MoveDown(element, index);
                        return;
                    }
                }
                else
                {
                    KeyValuePair<T, TPriority> keyValuePair = new KeyValuePair<T, TPriority>(item, value);
                    m_heap.Add(keyValuePair);
                    MoveUp(keyValuePair, Count);
                }
            }
        }

        /// <summary>
        /// Gets the number of items in the queue</summary>
        public int Count
        {
            get { return m_heap.Count - 1; }
        }

        /// <summary>
        /// Constructor</summary>
        public PriorityQueue() : this(false)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="invert">If true, invert the sort order</param>
        public PriorityQueue(bool invert) : this(Comparer<TPriority>.Default)
        {
            this.m_invert = invert;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="comparer">IComparer to use in sorting the queue</param>
        public PriorityQueue(IComparer<TPriority> comparer)
        {
            this.m_comparer = comparer;
            m_heap.Add(default(KeyValuePair<T, TPriority>));
        }

        /// <summary>
        /// Add an item to the queue</summary>
        /// <param name="item">The item to add</param>
        /// <param name="priority">The priority of the item</param>
        public void Enqueue(T item, TPriority priority)
        {
            KeyValuePair<T, TPriority> keyValuePair = new KeyValuePair<T, TPriority>(item, priority);
            m_heap.Add(keyValuePair);
            MoveUp(keyValuePair, Count);
        }

        /// <summary>
        /// Remove the next item from the queue. If the queue is empty, an InvalidOperationException will be thrown.</summary>
        /// <returns>The next item from the queue</returns>
        public KeyValuePair<T, TPriority> Dequeue()
        {
            int count = Count;
            if (count < 1)
            {
                throw new InvalidOperationException("Queue is empty.");
            }
            KeyValuePair<T, TPriority> result = m_heap[1];
            KeyValuePair<T, TPriority> element = m_heap[count];
            m_heap.RemoveAt(count);
            if (count > 1)
            {
                MoveDown(element, 1);
            }
            m_indexes.Remove(result.Key);
            return result;
        }

        /// <summary>
        /// Look at the next item in the queue without removing it.</summary>
        /// <returns>The next item in the queue</returns>
        public KeyValuePair<T, TPriority> Peek()
        {
            if (Count < 1)
            {
                throw new InvalidOperationException("Queue is empty.");
            }
            return m_heap[1];
        }

        /// <summary>
        /// Get the priority of the specified item if it is in the queue.</summary>
        /// <param name="item">The item to look up</param>
        /// <param name="priority">The priority of the item, or the default priority if the
        /// item is not in the queue</param>
        /// <returns>True if the item was found, otherwise false.</returns>
        public bool TryGetValue(T item, out TPriority priority)
        {
            int num;
            if (m_indexes.TryGetValue(item, out num))
            {
                priority = m_heap[m_indexes[item]].Value;
                return true;
            }
            priority = default(TPriority);
            return false;
        }

        private void MoveUp(KeyValuePair<T, TPriority> element, int index)
        {
            while (index > 1)
            {
                int num = index >> 1;
                if (IsPrior(m_heap[num], element))
                {
                    break;
                }
                m_heap[index] = m_heap[num];
                m_indexes[m_heap[num].Key] = index;
                index = num;
            }
            m_heap[index] = element;
            m_indexes[element.Key] = index;
        }

        private void MoveDown(KeyValuePair<T, TPriority> element, int index)
        {
            int count = m_heap.Count;
            while (index << 1 < count)
            {
                int num = index << 1;
                int num2 = num | 1;
                if (num2 < count && IsPrior(m_heap[num2], m_heap[num]))
                {
                    num = num2;
                }
                if (IsPrior(element, m_heap[num]))
                {
                    break;
                }
                m_heap[index] = m_heap[num];
                m_indexes[m_heap[num].Key] = index;
                index = num;
            }
            m_heap[index] = element;
            m_indexes[element.Key] = index;
        }

        private bool IsPrior(KeyValuePair<T, TPriority> element1, KeyValuePair<T, TPriority> element2)
        {
            int num = m_comparer.Compare(element1.Value, element2.Value);
            if (m_invert)
            {
                num = ~num;
            }
            return num < 0;
        }

        private readonly List<KeyValuePair<T, TPriority>> m_heap = new List<KeyValuePair<T, TPriority>>();
        private readonly Dictionary<T, int> m_indexes = new Dictionary<T, int>();
        private readonly IComparer<TPriority> m_comparer;
        private readonly bool m_invert;
    }
}