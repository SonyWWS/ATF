//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf
{
    /// <summary>
    /// Interface for classes that are passed parameters from schema annotations</summary>
    public interface IAnnotatedParams
    {
        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        void Initialize(string[] parameters);
    }
}
