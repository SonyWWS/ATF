//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for pose elements</summary>
    public interface IPoseElement
    {
        /// <summary>
        /// Gets or sets the pose target</summary>
        object Target
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets or sets the pose translation</summary>
        Vec3F Translation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pose rotation</summary>
        EulerAngles3F Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the pose scale</summary>
        Vec3F Scale
        {
            get;
            set;
        }
    }
}
