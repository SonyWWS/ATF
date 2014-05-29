//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Collection representing a selection. Maintains a LastSelected item, and enumerates
    /// items in least-recently-selected order. Provides change notification, and views
    /// filtered by item type. Uses adaptation to convert to other types.</summary>
    /// <typeparam name="T">Type of items in selection</typeparam>
    public class AdaptableSelection<T> : Selection<T>
        where T : class
    {
        /// <summary>
        /// Converts from the selection type to another given type</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <param name="item">Item in collection</param>
        /// <returns>Item, converted to given type, or null</returns>
        protected override U Convert<U>(T item)
        {
            U u = item.As<U>();
            return u;
        }
    }
}

