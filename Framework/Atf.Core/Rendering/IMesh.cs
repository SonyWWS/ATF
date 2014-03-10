//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for Mesh</summary>
    public interface IMesh : IAdaptable, INameable
    {
        /// <summary>
        /// Gets the DataSet list</summary>
        IEnumerable<IDataSet> DataSets
        {
            get;
        }

        /// <summary>
        /// Gets the PrimitiveSets list</summary>
        IEnumerable<IPrimitiveSet> PrimitiveSets
        {
            get;
        }

        /// <summary>
        /// Gets the bounding box</summary>
        Box BoundingBox
        {
            get;
        }
    }
}
