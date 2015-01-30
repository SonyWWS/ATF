//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Filenames for custom embedded image resources for DOM tree editor. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Control image resource resource filename</summary>
        [ImageResource("control.png")]
        public static readonly string ControlImage;

        /// <summary>
        /// Font image resource resource filename</summary>
        [ImageResource("font.png")]
        public static readonly string FontImage;

        /// <summary>
        /// Form image resource resource filename</summary>
        [ImageResource("form.png")]
        public static readonly string FormImage;

        /// <summary>
        /// Package image resource resource filename</summary>
        [ImageResource("package.png")]
        public static readonly string PackageImage;

        /// <summary>
        /// Reference image resource resource filename</summary>
        [ImageResource("ref.png")]
        public static readonly string RefImage;

        /// <summary>
        /// Empty reference image resource resource filename</summary>
        [ImageResource("refEmpty.png")]
        public static readonly string RefEmptyImage;

        /// <summary>
        /// Shader image resource resource filename</summary>
        [ImageResource("shader.png")]
        public static readonly string ShaderImage;

        /// <summary>
        /// Sprite image resource resource filename</summary>
        [ImageResource("sprite.png")]
        public static readonly string SpriteImage;

        /// <summary>
        /// Text image resource resource filename</summary>
        [ImageResource("text.png")]
        public static readonly string TextImage;

        /// <summary>
        /// Texture image resource resource filename</summary>
        [ImageResource("texture.png")]
        public static readonly string TextureImage;

        [ImageResource("curve_16.png")]
        public static readonly string CurveImage;
    }
}
