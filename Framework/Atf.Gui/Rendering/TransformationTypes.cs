//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Transformation enum types</summary>
    [Flags]
    public enum TransformationTypes
    {
        /// <summary>
        /// Translation</summary>
        Translation = 1,

        /// <summary>
        /// Scale</summary>
        Scale = 2,

        /// <summary>
        /// Rotation</summary>
        Rotation = 4,

        /// <summary>
        /// Pivot around which scale is applied</summary>
        ScalePivot = 8,

        /// <summary>
        /// Translation to compensate for scale pivot</summary>
        ScalePivotTranslation = 16,

        /// <summary>
        /// Pivot around which rotation is applied</summary>
        RotatePivot = 32,

        /// <summary>
        /// Translation to compensate for rotate pivot</summary>
        RotatePivotTranslation = 64,

        /// <summary>
        /// Uniform scale</summary>
        UniformScale = 128,
    }
}
