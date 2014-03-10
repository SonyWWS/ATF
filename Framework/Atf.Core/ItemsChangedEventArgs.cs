//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf
{
    /// <summary>
    /// Class for notifying event subscribers when items are added, removed,
    /// or changed in some context or collection</summary>
    /// <typeparam name="T">Item type</typeparam>
    public class ItemsChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="addedItems">Items that were added</param>
        /// <param name="removedItems">Items that were removed</param>
        /// <param name="changedItems">Items that were changed</param>
        public ItemsChangedEventArgs(
            IEnumerable<T> addedItems,
            IEnumerable<T> removedItems,
            IEnumerable<T> changedItems)
        {
            AddedItems = addedItems;
            RemovedItems = removedItems;
            ChangedItems = changedItems;
        }

        /// <summary>
        /// The items that were added</summary>
        public readonly IEnumerable<T> AddedItems;

        /// <summary>
        /// The items that were removed</summary>
        public readonly IEnumerable<T> RemovedItems;

        /// <summary>
        /// The items that were changed</summary>
        public readonly IEnumerable<T> ChangedItems;
    }
}

