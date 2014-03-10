//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for ATGI LodGroup (Level Of Detail Group)</summary>
    public interface ILodGroup
    {
        /// <summary>
        /// Gets distance thresholds for the LODs (Level Of Detail)</summary>
        IList<float> Thresholds
        {
            get;
        }

        /// <summary>
        /// Gets the bounding box in local space</summary>
        Box BoundingBox
        {
            get;
        }
    }
}
