//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// Extension methods for LinkedList to find next and previous elements with a given value</summary>
    public static class LinkedListExtensions
    {
        /// <summary>
        /// Find the next element in a linked list whose value is equal to the value argument.</summary>
        /// <typeparam name="T">Type of element in the list</typeparam>
        /// <param name="list">The list</param>
        /// <param name="node">The node to start from</param>
        /// <param name="value">The value to match</param>
        /// <returns>The next node in the list whose value is equal to the value argument</returns>
        public static LinkedListNode<T> FindNext<T>(this LinkedList<T> list, LinkedListNode<T> node, T value)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (node == null)
            {
                return list.Find(value);
            }
            if (list != node.List)
            {
                throw new ArgumentException("The list does not contain the given node.");
            }
            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            for (node = node.Next; node != null; node = node.Next)
            {
                if (value != null)
                {
                    if (@default.Equals(node.Value, value))
                    {
                        return node;
                    }
                }
                else
                {
                    if (node.Value == null)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find the previous element in a linked list whose value is equal to the value argument.</summary>
        /// <typeparam name="T">Type of element in the list</typeparam>
        /// <param name="list">The list</param>
        /// <param name="node">The node to start from</param>
        /// <param name="value">The value to match</param>
        /// <returns>The previous node in the list whose value is equal to the value argument</returns>
        public static LinkedListNode<T> FindPrevious<T>(this LinkedList<T> list, LinkedListNode<T> node, T value)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }
            if (node == null)
            {
                return list.FindLast(value);
            }
            if (list != node.List)
            {
                throw new ArgumentException("The list does not contain the given node.");
            }
            EqualityComparer<T> @default = EqualityComparer<T>.Default;
            for (node = node.Previous; node != null; node = node.Previous)
            {
                if (value != null)
                {
                    if (@default.Equals(node.Value, value))
                    {
                        return node;
                    }
                }
                else
                {
                    if (node.Value == null)
                    {
                        return node;
                    }
                }
            }
            return null;
        }
    }
}