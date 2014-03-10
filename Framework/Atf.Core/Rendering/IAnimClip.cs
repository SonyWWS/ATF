//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for Anim clip</summary>
    public interface IAnimClip : INameable
    {
        /// <summary>
        /// Gets the child Anim list</summary>
        IList<IAnim> Anims
        {
            get;
        }
    }
}
