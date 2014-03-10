//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for a binding (Material, PrimitiveSet or Shader)</summary>
    public interface IBinding : IAdaptable
    {
        /// <summary>
        /// Gets and sets binding type</summary>
        string BindingType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets source DOM object</summary>
        object Source
        {
            get;
            set;
        }
    }
}
