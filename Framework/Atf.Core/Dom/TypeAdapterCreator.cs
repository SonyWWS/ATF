//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// AdapterCreator that finds the adapter as a property on the DomNode's Type</summary>
    /// <typeparam name="T">Adapter type, must be reference type</typeparam>
    /// <remarks>The adapter instance is shared by all DomNode instances on
    /// which it is registered. Therefore, the adapter should be an immutable object.</remarks>
    public class TypeAdapterCreator<T> : IAdapterCreator
        where T : class
    {
        /// <summary>
        /// Indicates if an adapter can be created</summary>
        /// <param name="adaptee">Object to adapt</param>
        /// <param name="type">Adapter type</param>
        /// <returns>true iff an adapter can be created</returns>
        public bool CanAdapt(object adaptee, Type type)
        {
            DomNode node = adaptee as DomNode;
            return
                node != null &&
                type != null &&
                type.IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Gets an adapter for the adaptee, or null</summary>
        /// <param name="adaptee">Object to adapt</param>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter for the adaptee, or null</returns>
        public object GetAdapter(object adaptee, Type type)
        {
            DomNode node = adaptee as DomNode;
            if (node != null &&
                type != null &&
                type.IsAssignableFrom(typeof(T)))
            {
                T adapter = node.Type.GetTag<T>();
                return adapter;
            }

            return null;
        }
    }
}
