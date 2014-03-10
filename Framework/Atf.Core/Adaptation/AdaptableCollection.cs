//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// This class wraps an ICollection of one type to implement ICollection of another type</summary>
    /// <typeparam name="T">Underlying list type</typeparam>
    /// <typeparam name="U">Adapted list type</typeparam>
    /// <remarks>This adapter class can be used to simulate interface covariance, where
    /// an ICollection of Type1 can be made to implement an ICollection of Type2, as long as Type1
    /// implements or can be adapted to Type2.</remarks>
    public class AdaptableCollection<T, U> : CollectionAdapter<T, U>
        where T : class
        where U : class
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="list">List to adapt</param>
        public AdaptableCollection(ICollection<T> list)
            : base(list)
        {
        }

        /// <summary>
        /// Converts the item to the adapted list type; throws an InvalidOperationException
        /// if the item can't be converted</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Item, converted to the adapted list type</returns>
        protected override T Convert(U item)
        {
            T t = item.As<T>();
            if (t == null && item != null)
                throw new InvalidOperationException("Item of wrong type for underlying collection");
            return t;
        }

        /// <summary>
        /// Converts the item from the adapted list type; returns null if the item can't be
        /// converted</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Item, converted to the adapted list type</returns>
        protected override U Convert(T item)
        {
            U u = item.As<U>();
            return u;
        }
    }
}
