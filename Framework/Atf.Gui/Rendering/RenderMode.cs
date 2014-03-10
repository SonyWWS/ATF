//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Rendering modes</summary>
    [Flags]
    public enum RenderMode
    {
        /// <summary>
        /// Smooth shading</summary>
        Smooth          = 1,

        /// <summary>
        /// Wireframe</summary>
        Wireframe       = 2,

        /// <summary>
        /// Textured</summary>
        Textured        = 4,

        /// <summary>
        /// Lit</summary>
        Lit             = 8,

        /// <summary>
        /// Solid color</summary>
        SolidColor      = 16,

        /// <summary>
        /// Wireframe color</summary>
        WireframeColor  = 32,

        /// <summary>
        /// Wireframe thickness</summary>
        WireframeThickness = 64,

        /// <summary>
        /// Disable Z-buffer test (read, compare, and write). If this is set, the
        /// DisableZBufferWrite bit is irrelevant.</summary>
        DisableZBuffer  = 128,

        /// <summary>
        /// Alpha (transparency)</summary>
        Alpha           = 256,

        /// <summary>
        /// Cull backfacing primitives</summary>
        CullBackFace    = 512,

        /// <summary>
        /// Disable Z-buffer writes. Has an effect only if the DisableZBuffer bit is not set.</summary>
        DisableZBufferWrite = 1024,

        /// <summary>
        /// The highest flag value, to help with enumerating through each bit</summary>
        Max = 1024
    }
}
