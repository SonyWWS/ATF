//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Class to represent a path in a tree or graph with the capability of adapting the
    /// last element of the path (the Last property) to a requested type. For example,
    /// the DOM property descriptors (like AttributePropertyDescriptor) try to adapt
    /// an AdaptablePath object to a DomNode and this succeeds if the last element of
    /// the path can be adapted to a DomNode.</summary>
    /// <typeparam name="T">Type of items in path</typeparam>
    public class AdaptablePath<T> : Path<T>, IAdaptable, IDecoratable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="last">Single object making up the path</param>
        public AdaptablePath(T last)
            : base(last)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="path">Path as sequence of objects</param>
        public AdaptablePath(IEnumerable<T> path)
            : base(path)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="path">Path as collection of objects</param>
        public AdaptablePath(ICollection<T> path)
            : base(path)
        {
        }

        #region IAdaptable, IDecoratable, and Related Methods

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null</returns>
        public object GetAdapter(Type type)
        {
            object adapter = Last.As(type);
            if (adapter != null)
                return adapter;

            if (type.IsAssignableFrom(GetType()))
                return this;

            return null;
        }

        /// <summary>
        /// Gets all decorators of the specified type</summary>
        /// <param name="type">Decorator type</param>
        /// <returns>Enumeration of non-null decorators that are of the specified type. The enumeration may be empty.</returns>
        public IEnumerable<object> GetDecorators(Type type)
        {
            foreach (object obj in Last.AsAll(type))
                yield return obj;
        }

        // implement the following members as a convenience when extension methods aren't available

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter</summary>
        /// <typeparam name="U">Desired type, must be ref type</typeparam>
        /// <returns>Converted reference for the given object or null</returns>
        public U As<U>()
            where U : class
        {
            return Adapters.As<U>(this);
        }

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter; if none is available, throws an AdaptationException</summary>
        /// <typeparam name="U">Desired type, must be ref type</typeparam>
        /// <returns>Converted reference for the given object</returns>
        public U Cast<U>() where U : class
        {
            return Adapters.Cast<U>(this);
        }

        /// <summary>
        /// Returns whether the given reference can be converted to one of
        /// the desired type</summary>
        /// <typeparam name="U">Adapter type, must be ref type</typeparam>
        /// <returns>True iff the given object can be converted</returns>
        public bool Is<U>()
            where U : class
        {
            return Adapters.Is<U>(this);
        }

        /// <summary>
        /// Returns an enumeration of all decorators that can convert a reference to the given type</summary>
        /// <typeparam name="U">Decorator type, must be ref type</typeparam>
        /// <returns>Enumerable returning all decorators of the given type</returns>
        public IEnumerable<U> AsAll<U>()
            where U : class
        {
            return Adapters.AsAll<U>(this);
        }

        #endregion

        /// <summary>
        /// Concatenates object with path</summary>
        /// <param name="lhs">Prefix object</param>
        /// <param name="rhs">Optional path</param>
        /// <returns>Concatenated path, with lhs as first object</returns>
        public static AdaptablePath<T> operator +(T lhs, AdaptablePath<T> rhs)
        {
            if (rhs == null)
                return new AdaptablePath<T>(lhs);
            T[] path = new T[1 + rhs.Count];
            path[0] = lhs;
            rhs.CopyTo(path, 1);
            return new AdaptablePath<T>(path);
        }

        /// <summary>
        /// Concatenates path with object</summary>
        /// <param name="lhs">Optional path</param>
        /// <param name="rhs">Suffix object</param>
        /// <returns>Concatenated path, with rhs as last object</returns>
        public static AdaptablePath<T> operator +(AdaptablePath<T> lhs, T rhs)
        {
            if (lhs == null)
                return new AdaptablePath<T>(rhs);
            T[] path = new T[lhs.Count + 1];
            lhs.CopyTo(path, 0);
            path[lhs.Count] = rhs;
            return new AdaptablePath<T>(path);
        }

        /// <summary>
        /// Concatenates 2 paths</summary>
        /// <param name="lhs">First path. Can be null.</param>
        /// <param name="rhs">Second path. Can be null.</param>
        /// <returns>Concatenated path, with rhs as prefix and lhs as suffix. Is null if both lhs and rhs are null.</returns>
        public static AdaptablePath<T> operator +(AdaptablePath<T> lhs, AdaptablePath<T> rhs)
        {
            if (lhs == null)
                return rhs;
            if (rhs == null)
                return lhs;
            T[] path = new T[lhs.Count + rhs.Count];
            lhs.CopyTo(path, 0);
            rhs.CopyTo(path, lhs.Count);
            return new AdaptablePath<T>(path);
        }

        /// <summary>
        /// Converts from the path type to another type</summary>
        /// <typeparam name="U">Desired type</typeparam>
        /// <param name="item">Item to convert</param>
        /// <returns>Item converted to given type or null</returns>
        protected override U Convert<U>(T item)
        {
            U u = item.As<U>();
            return u;
        }
    }
}
