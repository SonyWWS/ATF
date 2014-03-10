//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// AdapterCreator that produces adapters of the given type</summary>
    /// <typeparam name="T">Adapter type, a class implementing IAdapter and with a
    /// default constructor</typeparam>
    public class AdapterCreator<T> : IAdapterCreator
        where T : class, IAdapter, new()
    {
        /// <summary>
        /// Gets a value indicating if an adapter can be created</summary>
        /// <param name="adaptee">Object to adapt</param>
        /// <param name="type">Adapter type</param>
        /// <returns>True iff an adapter can be created</returns>
        public bool CanAdapt(object adaptee, Type type)
        {
            return
                adaptee != null &&
                type != null &&
                type.IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Gets an adapter for the adaptee or null</summary>
        /// <param name="adaptee">Object to adapt</param>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter for the adaptee or null</returns>
        public object GetAdapter(object adaptee, Type type)
        {
            if (type != null &&
                type.IsAssignableFrom(typeof(T)))
            {
                T adapter = new T();
                adapter.Adaptee = adaptee;
                return adapter;
            }

            return null;
        }
    }
}
