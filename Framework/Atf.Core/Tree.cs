//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Sce.Atf
{
    /// <summary>
    /// A very simple n-ary tree</summary>
    /// <typeparam name="T">Type of value held at each tree node</typeparam>
    [Serializable]
    public class Tree<T>
    {
        /// <summary>
        /// Constructor of empty tree</summary>
        public Tree()
            : this(default(T))
        {
        }

        /// <summary>
        /// Constructor for tree with one node</summary>
        /// <param name="value">Value associated with this tree</param>
        public Tree(T value)
        {
            m_value = value;
            m_children = new ChildCollection(this);
        }

        /// <summary>
        /// Gets or sets tree's parent. Is null if this is a root node.</summary>
        public Tree<T> Parent
        {
            get { return m_parent; }
            set
            {
                if (m_parent != value)
                {
                    if (m_parent != null)
                        m_parent.Children.Remove(this);

                    m_parent = value;

                    if (m_parent != null)
                        m_parent.Children.Add(this);
                }
            }
        }

        /// <summary>
        /// Gets the list of children nodes. Is the same as 'this' because this Tree implements IList.</summary>
        public IList<Tree<T>> Children
        {
            get { return m_children; }
        }

        /// <summary>
        /// Gets or sets the value associated with the tree</summary>
        public T Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        /// <summary>
        /// Tests for equality</summary>
        /// <param name="obj">Other object</param>
        /// <returns><c>True</c> if object is a tree with the same structure and values as this tree</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            Tree<T> other = obj as Tree<T>;
            if (other == null)
                return false;

            if (!m_value.Equals(other.m_value))
                return false;

            if (m_children.Count != other.m_children.Count)
                return false;

            for (int i = 0; i < m_children.Count; i++)
                if (!(m_children[i]).Equals(other.m_children[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Tests for similarity</summary>
        /// <param name="other">Other tree</param>
        /// <returns><c>True</c> if other has the same structure as this tree</returns>
        /// <remarks>Same structure means same node structure</remarks>
        public bool Similar(Tree<T> other)
        {
            if (this == other)
                return true;

            if (other == null)
                return false;

            if (m_children.Count != other.m_children.Count)
                return false;

            for (int i = 0; i < m_children.Count; i++)
                if (!(m_children[i]).Similar(other.m_children[i]))
                    return false;

            return true;
        }

        /// <summary>
        /// Returns hash code for tree</summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            int result = 0;
            foreach (Tree<T> tree in PreOrder)
                result ^= tree.Value.GetHashCode();

            return result;
        }

        /// <summary>
        /// Converts tree to string of the form "(Value(Child1),...,(ChildN))"</summary>
        /// <returns>String representation of tree</returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('(');
            Stringify(builder);
            builder.Append(')');
            return builder.ToString();
        }

        private void Stringify(StringBuilder builder)
        {
            builder.Append(m_value.ToString());
            if (!IsLeaf)
            {
                builder.Append('(');
                bool firstTime = true;
                foreach (Tree<T> t in m_children)
                {
                    if (firstTime)
                        firstTime = false;
                    else
                        builder.Append(',');
                    t.Stringify(builder);
                }
                builder.Append(')');
            }
        }

        /// <summary>
        /// Tests if this tree is a descendant of another</summary>
        /// <param name="ancestor">Possible ancestor</param>
        /// <returns><c>True</c> if this tree is a descendant of the other</returns>
        /// <remarks>A tree is considered a descendant of itself</remarks>
        public bool IsDescendantOf(Tree<T> ancestor)
        {
            Tree<T> descendant = this;
            while (descendant != null)
            {
                if (ancestor == descendant)
                    return true;
                descendant = descendant.Parent;
            }
            return false;
        }

        /// <summary>
        /// Gets whether a tree is a leaf (no children)</summary>
        public bool IsLeaf
        {
            get { return m_children.Count == 0; }
        }

        /// <summary>
        /// Gets level, or depth, in tree</summary>
        public int Level
        {
            get
            {
                int result = 0;
                Tree<T> ancestor = m_parent;
                while (ancestor != null)
                {
                    result++;
                    ancestor = ancestor.Parent;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets number of descendants, including the tree itself</summary>
        public int DescendantCount
        {
            get
            {
                int n = 0;
                foreach (Tree<T> tree in PreOrder)
                    n++;
                return n;
            }
        }

        /// <summary>
        /// Gets an enumeration of all the nodes of the tree in pre-order (depth first).
        /// For example, the root node is first, followed by its first child (and its
        /// children and so on) and then the second child of the root (and its children
        /// and so on) etc.</summary>
        public IEnumerable<Tree<T>> PreOrder
        {
            get
            {
                Stack<Tree<T>> nodes = new Stack<Tree<T>>();
                nodes.Push(this);
                while (nodes.Count > 0)
                {
                    Tree<T> node = nodes.Pop();
                    yield return node;

                    // push children in reverse order
                    for (int i = node.m_children.Count - 1; i >= 0; i--)
                        nodes.Push(node.m_children[i]);
                }
            }
        }

        /// <summary>
        /// Gets an enumeration of all the nodes of the tree in post-order. This means that for
        /// each node, the children are visited first (starting with the first child) and then
        /// the parent node is enumerated.</summary>
        public IEnumerable<Tree<T>> PostOrder
        {
            get
            {
                // push each non-leaf node twice to represent nodes whose children haven't been visited
                //  rather than storing a bit on each node.
                Stack<Tree<T>> nodes = new Stack<Tree<T>>();
                nodes.Push(this);
                if (!IsLeaf)
                    nodes.Push(this);

                while (nodes.Count > 1)
                {
                    Tree<T> node = nodes.Pop();
                    if (node != nodes.Peek())
                    {
                        yield return node;
                    }
                    else
                    {
                        // push children in reverse order
                        for (int i = node.m_children.Count - 1; i >= 0; i--)
                        {
                            Tree<T> child = node.m_children[i];
                            nodes.Push(child);
                            if (!child.IsLeaf)
                                nodes.Push(child);
                        }
                    }
                }

                yield return nodes.Pop();
            }
        }

        /// <summary>
        /// Gets an enumeration of all the nodes in a breadth-first order. This means that the
        /// root is enumerated first (level 0), followed by all of its children (level 1),
        /// followed by all of their children (level 2), and so on.</summary>
        public IEnumerable<Tree<T>> LevelOrder
        {
            get
            {
                Queue<Tree<T>> nodes = new Queue<Tree<T>>();
                nodes.Enqueue(this);
                while (nodes.Count > 0)
                {
                    Tree<T> node = nodes.Dequeue();
                    yield return node;

                    // queue children
                    foreach (Tree<T> child in node.m_children)
                        nodes.Enqueue(child);
                }
            }
        }

        private class ChildCollection : Collection<Tree<T>>
        {
            public ChildCollection(Tree<T> parent)
            {
                m_parent = parent;
            }

            protected override void InsertItem(int index, Tree<T> item)
            {
                item.m_parent = m_parent;

                base.InsertItem(index, item);
            }

            protected override void RemoveItem(int index)
            {
                Items[index].m_parent = null;

                base.RemoveItem(index);
            }

            protected override void SetItem(int index, Tree<T> item)
            {
                Items[index].m_parent = null;
                item.m_parent = m_parent;

                base.SetItem(index, item);
            }

            protected override void ClearItems()
            {
                foreach (Tree<T> subTree in Items)
                    subTree.m_parent = null;

                base.ClearItems();
            }

            private readonly Tree<T> m_parent;
        }

        private T m_value;
        private Tree<T> m_parent;
        private readonly ChildCollection m_children;
    }
}
