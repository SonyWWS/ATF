//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for PrimitiveSet</summary>
    public interface IPrimitiveSet : IAdaptable, INameable, IVisible
    {
        /// <summary>
        /// Gets the binding count</summary>
        int BindingCount
        {
            get;
        }

        /// <summary>
        /// Finds the index of the given binding name</summary>
        /// <param name="binding">Binding name</param>
        /// <returns>Index of the given binding name</returns>
        int FindBinding(string binding);

        /// <summary>
        /// Gets the bindings list</summary>
        IList<IBinding> Bindings
        {
            get;
        }

        /// <summary>
        /// Gets and sets the primitive type</summary>
        string PrimitiveType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the DOM object whose value is the primitive sizes array</summary>
        int[] PrimitiveSizes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the DOM object whose value is the primitive indices array</summary>
        int[] PrimitiveIndices
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the shader</summary>
        IShader Shader
        {
            get;
            set;
        }
    }
}
