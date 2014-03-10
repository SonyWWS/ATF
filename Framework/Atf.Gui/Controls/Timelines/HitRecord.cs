//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Holds the results of a hit testing operation</summary>
    public class HitRecord
    {
        /// <summary>
        /// Constructor for hitting a custom object in the timeline control</summary>
        /// <param name="type">Hit type</param>
        /// <param name="hitObject">Hit object. Should be the same object over multiple mouse
        /// move events so that tooltips don't flicker.</param>
        public HitRecord(HitType type, object hitObject)
        {
            Type = type;
            HitObject = hitObject;
            HitPath = null;
            HitTimelineObject = null;
        }

        /// <summary>
        /// Constructor for a HitRecord representing a hit on an ITimelineObject. If this
        /// ITimelineObject is in the main document, then the path will have just one element
        /// in it. Otherwise, the elements of the path should be ITimelineReference objects
        /// plus some other ITimelineObject (like IInterval, for example) as the last element.</summary>
        /// <param name="type">Hit type</param>
        /// <param name="path">Full path of the hit timeline object</param>
        public HitRecord(HitType type, TimelinePath path)
        {
            Type = type;
            HitPath = path;
            HitTimelineObject = path != null ? path.Last : null;
            HitObject = HitTimelineObject;
        }

        /// <summary>
        /// Hit type</summary>
        public readonly HitType Type;

        /// <summary>
        /// Hit object. Is not a path, but is always the last element of the path, if applicable.</summary>
        public readonly object HitObject;

        /// <summary>
        /// Hit timeline object or null if the hit object isn't of type ITimelineObject</summary>
        public readonly ITimelineObject HitTimelineObject;

        /// <summary>
        /// The path of the ITimelineObject. Will have just one element (the HitTimelineObject) if
        /// the hit occurred in the main document. Otherwise, the first elements are ITimelineReference
        /// objects.</summary>
        public readonly TimelinePath HitPath;
    }
}
