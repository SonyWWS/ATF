//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Interface for adapter creators, used to build adapter factories</summary>
    public interface IAdapterCreator
    {
        /// <summary>
        /// Gets a value indicating if an adapter can be created</summary>
        /// <param name="adaptee">Object to adapt</param>
        /// <param name="type">Adapter type</param>
        /// <returns>True iff an adapter can be created</returns>
        bool CanAdapt(object adaptee, Type type);

        /// <summary>
        /// Gets an adapter for the adaptee or null</summary>
        /// <param name="adaptee">Object to adapt</param>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter for the adaptee or null</returns>
        object GetAdapter(object adaptee, Type type);
    }
}
