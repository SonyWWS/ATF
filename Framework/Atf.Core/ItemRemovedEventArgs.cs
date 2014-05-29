//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Arguments for "item removed" event</summary>
    /// <typeparam name="T">Type of removed item</typeparam>
    public class ItemRemovedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor using index and removed item</summary>
        /// <param name="index">Index of removal</param>
        /// <param name="item">Removed item</param>
        public ItemRemovedEventArgs(int index, T item)
            : this(index, item, default(T))
        {
        }

        /// <summary>
        /// Constructor using index, removed item and parent</summary>
        /// <param name="index">Index of removal</param>
        /// <param name="item">Removed item</param>
        /// <param name="parent">Item's former parent</param>
        public ItemRemovedEventArgs(int index, T item, T parent)
        {
            Index = index;
            Item = item;
            Parent = parent;
        }

        /// <summary>
        /// Index of removal</summary>
        public readonly int Index;

        /// <summary>
        /// Removed item</summary>
        public readonly T Item;

        /// <summary>
        /// Item's former parent</summary>
        public readonly T Parent;
    }
}
