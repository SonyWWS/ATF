//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for ATGI Scene</summary>
    public interface IScene : INameable
    {
        /// <summary>
        /// Gets the list of scene nodes</summary>
        IList<INode> Nodes
        {
            get;
        }
    }
}
