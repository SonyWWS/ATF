//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Class that wraps an object to extend its adaptability</summary>
    public class Adapter : IAdapter, IAdaptable, IDecoratable
    {
        /// <summary>
        /// Constructor</summary>
        public Adapter()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="adaptee">Object that is adapted</param>
        public Adapter(object adaptee)
        {
            Adaptee = adaptee;
        }

        /// <summary>
        /// Gets or sets the adapted object</summary>
        public object Adaptee
        {
            get { return m_adaptee; }
            set
            {
                if (m_adaptee != value)
                {
                    object oldAdaptee = m_adaptee;
                    m_adaptee = value;
                    OnAdapteeChanged(oldAdaptee);
                }
            }
        }

        #region IAdaptable, IDecoratable, and Related Methods

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null</returns>
        public object GetAdapter(Type type)
        {
            // see if this can adapt
            object adapter = Adapt(type);

            // if not, let the adaptee handle it
            if (adapter == null)
                adapter = m_adaptee.As(type);

            return adapter;
        }

        /// <summary>
        /// Gets all decorators of the specified type or null</summary>
        /// <param name="type">Decorator type</param>
        /// <returns>Enumeration of decorators that are of the specified type or null</returns>
        public IEnumerable<object> GetDecorators(Type type)
        {
            // see if this can adapt
            object adapter = Adapt(type);
            if (adapter != null)
                yield return adapter;

            foreach (object obj in m_adaptee.AsAll(type))
                yield return obj;
        }

        // implement the following members as a convenience when extension methods aren't available

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter</summary>
        /// <typeparam name="T">Desired type, must be ref type</typeparam>
        /// <returns>Converted reference for the given object or null</returns>
        public T As<T>()
            where T : class
        {
            return Adapters.As<T>(this);
        }

        /// <summary>
        /// Converts a reference to the given type by first trying a CLR cast, and then
        /// trying to get an adapter; if none is available, throws an AdaptationException</summary>
        /// <typeparam name="T">Desired type, must be ref type</typeparam>
        /// <returns>Converted reference for the given object</returns>
        public T Cast<T>() where T : class
        {
            return Adapters.Cast<T>(this);
        }

        /// <summary>
        /// Returns a value indicating if the given reference can be converted to one of
        /// the desired type</summary>
        /// <typeparam name="T">Adapter type, must be ref type</typeparam>
        /// <returns>True, iff the given object can be converted</returns>
        public bool Is<T>()
            where T : class
        {
            return Adapters.Is<T>(this);
        }

        /// <summary>
        /// Gets all decorators that can convert a reference to the given type</summary>
        /// <typeparam name="T">Decorator type, must be ref type</typeparam>
        /// <returns>Enumerable returning all decorators of the given type</returns>
        public IEnumerable<T> AsAll<T>()
            where T : class
        {
            return Adapters.AsAll<T>(this);
        }

        #endregion

        /// <summary>
        /// Performs custom actions after the adapted object has been set</summary>
        /// <param name="oldAdaptee">Previous adaptee reference</param>
        protected virtual void OnAdapteeChanged(object oldAdaptee)
        {
        }

        /// <summary>
        /// Performs the adaptation to the desired type T or returns null</summary>
        /// <param name="type">Adapter type, should be assignable to type T</param>
        /// <returns>Adaptation to the desired type T or null</returns>
        protected virtual object Adapt(Type type)
        {
            // default is to return this if compatible with requested type
            if (type.IsAssignableFrom(GetType()))
                return this;

            return null;
        }

        private object m_adaptee;
    }
}
