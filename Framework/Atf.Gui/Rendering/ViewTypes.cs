//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Rendering
{
    /// <summary>
    /// Viewport types. These values should not be changed, since this enum is persisted as an integer.
    /// So, be sure to add new enums at the end</summary>
    public enum ViewTypes
    {
        /// <summary>
        /// Front view, orthographic projection</summary>
        Front,

        /// <summary>
        /// Back view, orthographic projection</summary>
        Back,

        /// <summary>
        /// Top view, orthographic projection</summary>
        Top,

        /// <summary>
        /// Bottom view, orthographic projection</summary>
        Bottom,

        /// <summary>
        /// Left view, orthographic projection</summary>
        Left,

        /// <summary>
        /// Right view, orthographic projection</summary>
        Right,

        /// <summary>
        /// Perspective view</summary>
        Perspective,
    }
}
