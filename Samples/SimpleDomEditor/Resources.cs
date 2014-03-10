//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// Filenames for custom embedded image resources. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Animation image resource resource filename</summary>
        [ImageResource("animation.png")]
        public static readonly string AnimationImage;

        /// <summary>
        /// Event image resource resource filename</summary>
        [ImageResource("event.png")]
        public static readonly string EventImage;

        /// <summary>
        /// Geometry image resource resource filename</summary>
        [ImageResource("geometry.png")]
        public static readonly string GeometryImage;

        /// <summary>
        /// Resource image resource resource filename</summary>
        [ImageResource("resource.png")]
        public static readonly string ResourceImage;

        /// <summary>
        /// DOM repository image resource resource filename</summary>
        [ImageResource("dom_repository.png")]
        public static readonly string DocumentRegistryImage;

        /// <summary>
        /// DOM collection image resource resource filename</summary>
        [ImageResource("dom_collection.png")]
        public static readonly string DomDocumentImage;

        /// <summary>
        /// DOM object image resource resource filename</summary>
        [ImageResource("dom_object.png")]
        public static readonly string DomNodeImage;

        /// <summary>
        /// Static constructor</summary>
        static Resources()
        {
            ResourceUtil.Register(typeof(Resources));
        }
    }
}
