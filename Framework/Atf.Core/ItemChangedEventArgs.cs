//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// Arguments for "item changed" event.</summary>
    /// <typeparam name="T">Type of changed item</typeparam>
    public class ItemChangedEventArgs<T> : EventArgs
        //where T : class
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="item">Changed item</param>
        /// <param name="reloaded">Whether contents of changed item was completely reloaded</param>
        public ItemChangedEventArgs(T item, bool reloaded=false)
        {
            Item = item;
            Reloaded = reloaded;
        }

        /// <summary>
        /// Changed item</summary>
        public readonly T Item;

        /// <summary>
        /// Whether contents of changed item was completely reloaded.</summary>
        public readonly bool Reloaded;
    }
}
