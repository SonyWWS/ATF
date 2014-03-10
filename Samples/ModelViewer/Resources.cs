//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace ModelViewerSample
{
    /// <summary>
    /// Standard game icons</summary>
    public static class Resources
    {      
        /// <summary>
        /// Smooth icon name</summary>
        [ImageResource("SmoothShading16.png", "SmoothShading24.png", "SmoothShading32.png")]
        public static readonly string SmoothImage;

        /// <summary>
        /// Wireframe icon name</summary>
        [ImageResource("Wireframe16.png", "Wireframe24.png", "Wireframe32.png")]
        public static readonly string WireframeImage;

        /// <summary>
        /// Outlined icon name</summary>
        [ImageResource("Outlined16.png", "Outlined24.png", "Outlined32.png")]
        public static readonly string OutlinedImage;

        /// <summary>
        /// Textured icon name</summary>
        [ImageResource("Texture16.png", "Texture24.png", "Texture32.png")]
        public static readonly string TexturedImage;

        /// <summary>
        /// Light icon name</summary>
        [ImageResource("Lighting16.png", "Lighting24.png", "Lighting32.png")]
        public static readonly string LightImage;

        /// <summary>
        /// Backface icon name</summary>
        [ImageResource("Backface16.png", "Backface24.png", "Backface32.png")]
        public static readonly string BackfaceImage;


        private const string ResourcePath = "ModelViewerSample.Resources.";

        static Resources()
        {
            ResourceUtil.Register(typeof(Resources), ResourcePath);
        }
    }
}
