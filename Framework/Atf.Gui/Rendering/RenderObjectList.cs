//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// A list of IRenderObjects</summary>
    internal class RenderObjectList : ICollection<IRenderObject>
    {
        #region IEnumerable<IRenderObject> Members

        public IEnumerator<IRenderObject> GetEnumerator()
        {
            return m_list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (m_list as System.Collections.IEnumerable).GetEnumerator();
        }

        #endregion

        #region ICollection<IRenderObject> Members

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Count
        {
            get { return m_list.Count; }
        }

        public void Add(IRenderObject toAdd)
        {
            Type[] dependencies = toAdd.GetDependencies();

            if (dependencies.Length == 0)
            {
                m_list.AddFirst(toAdd);
            }
            else
            {
                // Step through from end to beginning to see if we depend on anything.
                //  The first one we find, because the list is sorted and we started from the end,
                //  is the dependency that is highest.
                LinkedListNode<IRenderObject> dependency = m_list.Last;
                for (; dependency != null; dependency = dependency.Previous)
                {
                    if (DependsOn(toAdd, dependencies, dependency.Value))
                    {
                        m_list.AddAfter(dependency, toAdd);
                        break;
                    }
                }

                // if we are not dependent on anything already in our list, place at beginning
                //  since other render objects may be dependent on us.
                if (dependency == null)
                {
                    m_list.AddFirst(toAdd);
                    return;
                }

                // now check the ones before us to see if they are dependent on us. And any that
                // we move become part of the "to check against" set that goes from firstAdded to
                //  lastAdded.
                LinkedListNode<IRenderObject> firstAdded, lastAdded;
                firstAdded = lastAdded = dependency.Next;
                LinkedListNode<IRenderObject> test = m_list.First;
                while (test != firstAdded)
                {
                    LinkedListNode<IRenderObject> nextTest = test.Next;

                    // test against all types we've already moved
                    LinkedListNode<IRenderObject> moved = firstAdded;
                    while (true)
                    {
                        if (DependsOn(test.Value, test.Value.GetDependencies(), moved.Value))
                        {
                            LinkedListNode<IRenderObject> dependent = test;
                            m_list.Remove(dependent);
                            m_list.AddAfter(lastAdded, dependent); // lastAdded < dependent
                            lastAdded = dependent;
                            break;
                        }

                        if (moved == lastAdded)
                            break;
                        moved = moved.Next;
                    }

                    test = nextTest;
                }
            }
        }

        public void Clear()
        {
            m_list.Clear();
        }

        public bool Contains(IRenderObject obj)
        {
            return m_list.Contains(obj);
        }

        public void CopyTo(IRenderObject[] array, int arrayIndex)
        {
            m_list.CopyTo(array, arrayIndex);
        }

        public bool Remove(IRenderObject obj)
        {
            return m_list.Remove(obj);
        }

        #endregion

        /// <summary>
        /// Gets the internal list of IRenderObjects</summary>
        internal LinkedList<IRenderObject> InternalList
        {
            get { return m_list; }
        }

        // Returns true if 'a' depends on 'b', in the sense that 'b' derives from or implements
        //  some class or interface in 'dependencies'.
        private bool DependsOn(IRenderObject a, Type[] dependencies, IRenderObject b)
        {
            // for example, if 'a' depends on Mammals and 'b' is a Bear, then return true.
            for (int i = 0; i < dependencies.Length; i++)
                if (dependencies[i].IsAssignableFrom(b.GetType()))
                    return true;
            return false;
        }

        private readonly LinkedList<IRenderObject> m_list = new LinkedList<IRenderObject>();
    }
}
