//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for texture</summary>
    public interface ITexture : INameable
    {
        /// <summary>
        /// Gets or sets the texture path name</summary>
        string PathName
        {
            get;
            set;
        }
    }
}
