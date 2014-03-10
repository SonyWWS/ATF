//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for Shader</summary>
    public interface IShader : IAdaptable, INameable
    {
        /// <summary>
        /// Gets the list of shader bindings</summary>
        IList<IBinding> Bindings
        {
            get;
        }

        /// <summary>
        /// Gets the list of custom attributes</summary>
        IList<object> CustomAttributes
        {
            get;
        }
    }
}
