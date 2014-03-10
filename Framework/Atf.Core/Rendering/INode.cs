//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for Node</summary>
    public interface INode : INameable
    {
        /// <summary>
        /// Gets the mesh list</summary>
        IList<IMesh> Meshes
        {
            get;
        }

        /// <summary>
        /// Gets the child nodes list</summary>
        IList<INode> ChildNodes
        {
            get;
        }

        /// <summary>
        /// Gets the child joints list</summary>
        IList<IJoint> ChildJoints
        {
            get;
        }

        /// <summary>
        /// Gets and sets the transformation matrix</summary>
        Matrix4F Transform
        {
            get;
            set;
        }
    }
}
