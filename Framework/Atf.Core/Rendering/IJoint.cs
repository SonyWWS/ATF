//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Interface for ATGI joint</summary>
    /// <remarks>Interface borrowed from INode (without Meshes, ChildNodes)</remarks>
    public interface IJoint : INameable
    {
        /// <summary>
        /// Gets the child joint list</summary>
        IList<IJoint> ChildJoints
        {
            get;
        }

        /// <summary>
        /// Gets or sets the combined transformation matrix</summary>
        Matrix4F Transform
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint translation</summary>
        Vec3F Translation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint rotation</summary>
        Vec3F Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint axis of rotation</summary>
        Vec3F RotationAxis
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint rotation pivot</summary>
        Vec3F RotatePivot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint rotation pivot translation</summary>
        Vec3F RotatePivotTranslation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint scale</summary>
        Vec3F Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint scale pivot</summary>
        Vec3F ScalePivot
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the scale pivot translation</summary>
        Vec3F ScalePivotTranslation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rotation freedom in x</summary>
        bool RotationFreedomInX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rotation freedom in y</summary>
        bool RotationFreedomInY
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rotation freedom in z</summary>
        bool RotationFreedomInZ
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the joint has a rotation minimum in x</summary>
        bool HasRotationMinX
        {
            get;
        }

        /// <summary>
        /// Gets whether the joint has a rotation minimum in y</summary>
        bool HasRotationMinY
        {
            get;
        }

        /// <summary>
        /// Gets whether the joint has a rotation minimum in z</summary>
        bool HasRotationMinZ
        {
            get;
        }

        /// <summary>
        /// Gets or sets joint rotation minimum in x</summary>
        float RotationMinX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets joint rotation minimum in y</summary>
        float RotationMinY
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets joint rotation minimum in z</summary>
        float RotationMinZ
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether the joint has a rotation maximum in x</summary>
        bool HasRotationMaxX
        {
            get;
        }

        /// <summary>
        /// Gets whether the joint has a rotation maximum in y</summary>
        bool HasRotationMaxY
        {
            get;
        }

        /// <summary>
        /// Gets whether the joint has a rotation maximum in z</summary>
        bool HasRotationMaxZ
        {
            get;
        }

        /// <summary>
        /// Gets or sets the joint rotation maximum in x</summary>
        float RotationMaxX
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint rotation maximum in y</summary>
        float RotationMaxY
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the joint rotation maximum in z</summary>
        float RotationMaxZ
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the additional rotation applied after the normal node rotation</summary>
        EulerAngles3F JointOrientEul
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the joint should compensate for scale</summary>
        bool ScaleCompensate
        {
            get;
            set;
        }
    }
}
