//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// This class wraps an IObservableCollection of one type to implement IObservableCollection of another type</summary>
    /// <typeparam name="T">Underlying list type</typeparam>
    /// <typeparam name="U">Adapted list type</typeparam>
    /// <remarks>This adapter class can be used to simulate interface covariance, where
    /// an IObservableCollection of Type1 can be made to implement an IObservableCollection of Type2, as long as Type1
    /// implements or can be adapted to Type2.</remarks>
    public class AdaptableObservableCollection<T,U> : ObservableCollectionAdapter<T,U>
        where T : class
        where U : class
    {
        public AdaptableObservableCollection(IObservableCollection<T> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Converts the item to the adapted list type; throws an InvalidOperationException
        /// if the item can't be converted</summary>
        /// <param name="item">Item to convert</param>
        /// <returns>Item, converted to the adapted list type</returns>
        protected override T Convert(U item)
        {
            T t = Adapters.As<T>(item);
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
            U u = Adapters.As<U>(item);
            return u;
        }
    }
}
