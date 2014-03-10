//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for worlds</summary>
    public interface IWorld
    {
        /// <summary>
        /// Gets or sets the scene</summary>
        IScene Scene
        {
            get;
            set;
        }
    }
}
