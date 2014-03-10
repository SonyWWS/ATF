//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Interface for types whose instances can be decorated</summary>
    public interface IDecoratable
    {
        /// <summary>
        /// Gets all decorators of the specified type, or null</summary>
        /// <param name="type">Decorator type</param>
        /// <returns>Enumeration of decorators that are of the specified type</returns>
        IEnumerable<object> GetDecorators(Type type);
    }
}
