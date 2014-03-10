//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace CircuitEditorSample
{
    /// <summary>
    /// Filenames for custom embedded image resources. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Annotation image resource resource filename</summary>
        [ImageResource("annotation.png")]
        public static readonly string AnnotationImage;

        /// <summary>
        /// Button image resource filename</summary>
        [ImageResource("button_16.png", null, "button_32.png")]
        public static readonly string ButtonImage;

        /// <summary>
        /// And image resource filename</summary>
        [ImageResource("and_16.png", null, "and_32.png")]
        public static readonly string AndImage;

        /// <summary>
        /// Or image resource filename</summary>
        [ImageResource("or_16.png", null, "or_32.png")]
        public static readonly string OrImage;

        /// <summary>
        /// Sound image resource filename</summary>
        [ImageResource("sound_16.png", null, "sound_32.png")]
        public static readonly string SoundImage;

        /// <summary>
        /// Light image resource filename</summary>
        [ImageResource("light_16.png", null, "light_32.png")]
        public static readonly string LightImage;

        /// <summary>
        /// Speaker image resource filename</summary>
        [ImageResource("speaker_16.png", null, "speaker_32.png")]
        public static readonly string SpeakerImage;

        /// <summary>
        /// Static constructor</summary>
        static Resources()
        {
            ResourceUtil.Register(typeof(Resources));
        }
    }
}
