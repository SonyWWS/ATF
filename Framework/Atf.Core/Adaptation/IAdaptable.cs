//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Interface for types that can provide adapters to other types</summary>
    public interface IAdaptable
    {
        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null if no adapter available</returns>
        object GetAdapter(Type type);
    }
}
