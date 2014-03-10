//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace FsmEditorSample
{
    /// <summary>
    /// Filenames for custom embedded image resources for FSM editor. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Annotation image resource resource filename</summary>
        [ImageResource("annotation.png")]
        public static readonly string AnnotationImage;

        /// <summary>
        /// State image resource resource filename</summary>
        [ImageResource("state.png")]
        public static readonly string StateImage;

        static Resources()
        {
            ResourceUtil.Register(typeof(Resources));
        }
    }
}
