//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Information for drawing ghosted timeline objects</summary>
    public class GhostInfo
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="obj">Ghosted object</param>
        /// <param name="target">Target to snap ghosted object to</param>
        /// <param name="start">Start of ghosted object</param>
        /// <param name="length">Length of ghosted object</param>
        /// <param name="bounds">Bounds of ghosted object</param>
        /// <param name="valid">Whether ghosted object is valid</param>
        public GhostInfo(
            ITimelineObject obj,
            ITimelineObject target,
            float start,
            float length,
            RectangleF bounds,
            bool valid)
        {
            Object = obj;
            Target = target;
            Start = start;
            Length = length;
            Bounds = bounds;
            Valid = valid;
        }

        /// <summary>
        /// Ghosted object</summary>
        public readonly ITimelineObject Object;

        /// <summary>
        /// Target to snap ghosted object to</summary>
        public readonly ITimelineObject Target;
        /// <summary>
        /// Start of ghosted object</summary>
        public readonly float Start;

        /// <summary>
        /// Length of ghosted object</summary>
        public readonly float Length;

        /// <summary>
        /// Bounds of ghosted object</summary>
        public readonly RectangleF Bounds;

        /// <summary>
        /// Whether ghosted object is valid</summary>
        public readonly bool Valid;
    }
}
