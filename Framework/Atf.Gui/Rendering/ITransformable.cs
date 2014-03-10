//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom
{
    /// <summary>
    /// Interface for objects that maintain 3D transformation information</summary>
    /// <remarks>See TransformationTypes.</remarks>
    public interface ITransformable : IBoundable, IVisible
    {
        /// <summary>
        /// Gets and sets the local transformation matrix. This is derived from the various
        /// components below. Setting Transform does not update the components. See
        /// transform constraints.</summary>
        Matrix4F Transform
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the node translation. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        Vec3F Translation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the node rotation. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        Vec3F Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the node scale. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return new Vec3F{1,1,1}.</remarks>
        Vec3F Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the translation to origin of scaling. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        Vec3F ScalePivot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the translation after scaling. Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        Vec3F ScalePivotTranslation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the translation to origin of rotation. Setting the RotatePivot
        /// typically resets RotatePivotTranslation. See transform constraints.
        /// Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        Vec3F RotatePivot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the translation after rotation. Is used to keep local coordinates
        /// pinned to the same world coordinate when the RotatePivot is adjusted.
        /// Check TransformationType before using 'set'.</summary>
        /// <remarks>If unimplemented, 'get' should return Vec3F.ZeroVector.</remarks>
        Vec3F RotatePivotTranslation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets what type of transformation this object can support. All the transformation properties
        /// have a valid 'get', but check the appropriate flag before setting the property.</summary>
        TransformationTypes TransformationType
        {
            get;
            set;
        }
    }
}
