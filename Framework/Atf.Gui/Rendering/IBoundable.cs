//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface for objects managing a bounding box</summary>
    public interface IBoundable
    {      
        /// <summary>
        /// Gets a bounding box in local space</summary>
        Box BoundingBox
        {
            get;
        }
    }
}
