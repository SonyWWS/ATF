//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Collection representing active items. Maintains an ActiveItem, and enumerates
    /// items in least-recently-active order. Provides change notification, and views
    /// filtered by item type. Uses adaptation on items implementing IAdaptable to
    /// convert them to other types.</summary>
    /// <typeparam name="T">Type of items in collection</typeparam>
    public class AdaptableActiveCollection<T> : ActiveCollection<T>
        where T : class
    {
        /// <summary>
        /// Constructor for a collection that can contain an effectively unlimited number
        /// of items (int.MaxValue)</summary>
        public AdaptableActiveCollection()
        {
        }

        /// <summary>
        /// Constructor for a collection with a specified maximum size</summary>
        /// <param name="maximumCount">Maximum number of items that this collection can contain</param>
        public AdaptableActiveCollection(int maximumCount)
            : base(maximumCount)
        {
        }

        /// <summary>
        /// Converts from the collection type to another type</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <param name="item">Item in collection. If it implements IAdaptable, then the
        /// IAdaptable will be used if the normal 'as' operator fails.</param>
        /// <returns>Item, converted to given type, or null</returns>
        protected override U Convert<U>(T item)
        {
            U u = item.As<U>();
            return u;
        }
    }
}

