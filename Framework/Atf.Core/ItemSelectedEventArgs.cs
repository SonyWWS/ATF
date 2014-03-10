//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Event args for "Item selected" events</summary>
    /// <typeparam name="T">Type of item</typeparam>
    public class ItemSelectedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="item">Item selected or deselected</param>
        /// <param name="selected">Whether item was selected or deselected</param>
        public ItemSelectedEventArgs(T item, bool selected)
        {
            Item = item;
            Selected = selected;
        }

        /// <summary>
        /// Item selected or deselected</summary>
        public readonly T Item;

        /// <summary>
        /// Whether item was selected or deselected</summary>
        public bool Selected;
    }
}
