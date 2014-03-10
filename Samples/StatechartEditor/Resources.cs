//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;

namespace StatechartEditorSample
{
    /// <summary>
    /// Filenames for custom embedded image resources for Statechart editor. Call ResourceUtil.Register(typeof(Resources))
    /// to cause the referenced images to become globally available to other users of ResourceUtil.</summary>
    public static class Resources
    {
        /// <summary>
        /// Annotation image resource resource filename</summary>
        [ImageResource("annotation.png")]
        public static readonly string AnnotationImage;

        /// <summary>
        /// Conditional image resource resource filename</summary>
        [ImageResource("conditional.png")]
        public static readonly string ConditionalImage;

        /// <summary>
        /// Final image resource resource filename</summary>
        [ImageResource("final.png")]
        public static readonly string FinalImage;

        /// <summary>
        /// History image resource resource filename</summary>
        [ImageResource("history.png")]
        public static readonly string HistoryImage;

        /// <summary>
        /// Reaction image resource resource filename</summary>
        [ImageResource("reaction.png")]
        public static readonly string ReactionImage;

        /// <summary>
        /// Start image resource resource filename</summary>
        [ImageResource("start.png")]
        public static readonly string StartImage;

        /// <summary>
        /// State image resource resource filename</summary>
        [ImageResource("state.png")]
        public static readonly string StateImage;

        /// <summary>
        /// StatechartDoc image resource resource filename</summary>
        [ImageResource("statechartDoc.png")]
        public static readonly string StatechartDocImage;

        /// <summary>
        /// Static constructor</summary>
        static Resources()
        {
            ResourceUtil.Register(typeof(Resources));
        }
    }
}
