//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Args for "item inserted" event</summary>
    public class ItemInsertedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor using index, inserted item and parent</summary>
        /// <param name="index">Index of insertion</param>
        /// <param name="item">Inserted item</param>
        /// <param name="parent">Parent item</param>
        public ItemInsertedEventArgs(int index, T item, T parent)
        {
            Index = index;
            Item = item;
            Parent = parent;
        }

        /// <summary>
        /// Constructor using index and inserted item</summary>
        /// <param name="index">Index after insertion</param>
        /// <param name="item">Inserted item</param>
        public ItemInsertedEventArgs(int index, T item)
            : this(index, item, default(T))
        {
        }

        /// <summary>
        /// Index after insertion</summary>
        public readonly int Index;

        /// <summary>
        /// Inserted item</summary>
        public readonly T Item;

        /// <summary>
        /// Parent item</summary>
        public readonly T Parent;
    }
}
