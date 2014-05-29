//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Extension methods for getting adapters for basic types</summary>
    public static class Adapters
    {
        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter</summary>
        /// <param name="reference">Reference to convert</param>
        /// <param name="type">Desired type, should be ref type</param>
        /// <returns>Converted reference for the given object or null</returns>
        public static object As(this object reference, Type type)
        {
            if (reference == null)
                return null;

            if (type == null)
                throw new ArgumentNullException("type");

            // is the adapted object compatible?
            if (type.IsAssignableFrom(reference.GetType()))
                return reference;

            // try to get an adapter
            var adaptable = reference as IAdaptable;
            if (adaptable != null)
            {
                object adapter = adaptable.GetAdapter(type);
                if (adapter != null)
                    return adapter;
            }

            return null;
        }

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter</summary>
        /// <typeparam name="T">Desired type, must be ref type</typeparam>
        /// <param name="reference">Reference to convert</param>
        /// <returns>Converted reference for the given object or null</returns>
        public static T As<T>(this object reference)
            where T : class
        {
            if (reference == null)
                return null;

            // try a normal cast
            var converted = reference as T;

            // if that fails, try to get an adapter
            if (converted == null)
            {
                var adaptable = reference as IAdaptable;
                if (adaptable != null)
                    converted = adaptable.GetAdapter(typeof(T)) as T;
            }

            return converted;
        }

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter</summary>
        /// <typeparam name="T">Desired type, must be ref type</typeparam>
        /// <param name="adaptable">Adaptable object</param>
        /// <returns>Converted reference for the given object or null</returns>
        public static T As<T>(this IAdaptable adaptable)
            where T : class
        {
            if (adaptable == null)
                return null;

            // try a normal cast
            var converted = adaptable as T;

            // if that fails, try to get an adapter
            if (converted == null)
                converted = adaptable.GetAdapter(typeof(T)) as T;

            return converted;
        }

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter; if none is available, throws an AdaptationException</summary>
        /// <param name="reference">Reference to convert</param>
        /// <param name="type">Desired type, should be ref type</param>
        /// <returns>Converted reference for the given object</returns>
        public static object Cast(this object reference, Type type)
        {
            object converted = As(reference, type);
            if (converted == null)
                throw new AdaptationException(type.Name + " adapter required");
            return converted;
        }

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter; if none is available, throws an AdaptationException</summary>
        /// <typeparam name="T">Desired type, must be ref type</typeparam>
        /// <param name="reference">Reference to convert</param>
        /// <returns>Converted reference for the given object</returns>
        public static T Cast<T>(this object reference)
            where T : class
        {
            T converted = As<T>(reference);
            if (converted == null)
                throw new AdaptationException(typeof(T).Name + " adapter required");
            return converted;
        }

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter; if none is available, throws an AdaptationException</summary>
        /// <typeparam name="T">Desired type, must be ref type</typeparam>
        /// <param name="adaptable">Adaptable object</param>
        /// <returns>Converted reference for the given object</returns>
        public static T Cast<T>(this IAdaptable adaptable)
            where T : class
        {
            T converted = As<T>(adaptable);
            if (converted == null)
                throw new AdaptationException(typeof(T).Name + " adapter required");
            return converted;
        }

        /// <summary>
        /// Returns a value indicating if the given reference can be converted to one of
        /// the desired type</summary>
        /// <param name="reference">Reference to test</param>
        /// <param name="type">Desired type, should be ref type</param>
        /// <returns>True iff the given object can be converted</returns>
        public static bool Is(this object reference, Type type)
        {
            return As(reference, type) != null;
        }

        /// <summary>
        /// Returns a value indicating if the given reference can be converted to one of
        /// the desired type</summary>
        /// <typeparam name="T">Adapter type, must be ref type</typeparam>
        /// <param name="reference">Reference to test</param>
        /// <returns>True iff the given object can be converted</returns>
        public static bool Is<T>(this object reference)
            where T : class
        {
            return As<T>(reference) != null;
        }

        /// <summary>
        /// Returns a value indicating if the given reference can be converted to one of
        /// the desired type</summary>
        /// <typeparam name="T">Adapter type, must be ref type</typeparam>
        /// <param name="adaptable">Adaptable object</param>
        /// <returns>True iff the given object can be converted</returns>
        public static bool Is<T>(this IAdaptable adaptable)
            where T : class
        {
            return As<T>(adaptable) != null;
        }

        /// <summary>
        /// Gets all decorators that can convert a reference to the given type</summary>
        /// <param name="reference">Reference to convert</param>
        /// <param name="type">Decorator type, should be ref type</param>
        /// <returns>Enumerable returning all decorators of the given type.
        /// Decorators are never null. The enumeration may be empty.</returns>
        public static IEnumerable<object> AsAll(this object reference, Type type)
        {
            if (reference != null)
            {
                // if IDecoratable, use that to get decorators
                var decoratable = reference as IDecoratable;
                if (decoratable != null)
                    return decoratable.GetDecorators(type);

                // is the decorated object compatible?
                if (type.IsAssignableFrom(reference.GetType()))
                    return new object[] { reference };
            }

            return EmptyEnumerable<object>.Instance;
        }

        /// <summary>
        /// Gets all decorators that can convert a reference to the given type</summary>
        /// <typeparam name="T">Decorator type, must be ref type</typeparam>
        /// <param name="reference">Reference to convert</param>
        /// <returns>Enumerable returning all decorators of the given type.
        /// Decorators are never null. The enumeration may be empty.</returns>
        public static IEnumerable<T> AsAll<T>(this object reference)
            where T : class
        {
            if (reference != null)
            {
                // if IDecoratable, use that to get decorators
                var decoratable = reference as IDecoratable;
                if (decoratable != null)
                    return AsAll<T>(decoratable);

                // otherwise, cast
                var t = reference as T;
                if (t != null)
                    return new T[] { t };
            }

            return EmptyEnumerable<T>.Instance;
        }

        /// <summary>
        /// Gets an enumeration of all decorators that can convert a reference to the given type</summary>
        /// <typeparam name="T">Decorator type, must be ref type</typeparam>
        /// <param name="decoratable">Decoratable object</param>
        /// <returns>Enumerable returning all decorators of the given type</returns>
        public static IEnumerable<T> AsAll<T>(this IDecoratable decoratable)
            where T : class
        {
            if (decoratable != null)
            {
                foreach (object decorator in decoratable.GetDecorators(typeof(T)))
                    yield return decorator as T;
            }
        }

        /// <summary>
        /// Gets an adapter that converts an enumerable to an enumerable of another type</summary>
        /// <param name="enumerable">Enumerable to adapt</param>
        /// <param name="type">Adapter type, should be ref type</param>
        /// <returns>Enumerable returning adapted items</returns>
        public static IEnumerable<object> AsIEnumerable(this IEnumerable enumerable, Type type)
        {
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                {
                    object adapter = As(item, type);
                    if (adapter != null)
                        yield return adapter;
                }
            }
        }

        /// <summary>
        /// Returns an enumeration for an adapter that converts an enumerable to an enumerable of another type</summary>
        /// <typeparam name="T">Adapter type, must be ref type</typeparam>
        /// <param name="enumerable">Enumerable to adapt</param>
        /// <returns>Enumerable returning adapted items</returns>
        public static IEnumerable<T> AsIEnumerable<T>(this IEnumerable enumerable)
            where T : class
        {
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                {
                    T adapter = As<T>(item);
                    if (adapter != null)
                        yield return adapter;
                }
            }
        }

        /// <summary>
        /// Returns a value indicating if any of the items in the enumerable are
        /// adaptable to the given type</summary>
        /// <param name="enumerable">Enumerable to adapt</param>
        /// <param name="type">Adapter type, should be ref type</param>
        /// <returns>True iff any of the items in the enumerable are adaptable to
        /// the given type</returns>
        public static bool Any(this IEnumerable enumerable, Type type)
        {
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                {
                    object adapter = As(item, type);
                    if (adapter != null)
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns a value indicating if any of the items in the enumerable are
        /// adaptable to the given type</summary>
        /// <typeparam name="T">Adapter type, must be ref type</typeparam>
        /// <param name="enumerable">Enumerable to adapt</param>
        /// <returns>True iff any of the items in the enumerable are adaptable to
        /// the given type</returns>
        public static bool Any<T>(this IEnumerable enumerable)
            where T : class
        {
            return Any(enumerable, typeof(T));
        }

        /// <summary>
        /// Returns a value indicating if all of the items in the enumerable are
        /// adaptable to the given type</summary>
        /// <param name="enumerable">Enumerable to adapt</param>
        /// <param name="type">Adapter type, should be ref type</param>
        /// <returns>True iff all of the items in the enumerable are adaptable to
        /// the given type</returns>
        public static bool All(this IEnumerable enumerable, Type type)
        {
            if (enumerable != null)
            {
                foreach (object item in enumerable)
                {
                    object adapter = As(item, type);
                    if (adapter == null)
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a value indicating if all of the items in the enumerable are
        /// adaptable to the given type</summary>
        /// <typeparam name="T">Adapter type, must be ref type</typeparam>
        /// <param name="enumerable">Enumerable to adapt</param>
        /// <returns>True iff all of the items in the enumerable are adaptable to
        /// the given type</returns>
        public static bool All<T>(this IEnumerable enumerable)
            where T : class
        {
            return All(enumerable, typeof(T));
        }
    }
}
