//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Class for timeline path, which is a sequence of objects in timelines, e.g., groups, tracks, events</summary>
    public class TimelinePath : AdaptablePath<ITimelineObject>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="last">Single object making up the path</param>
        public TimelinePath(ITimelineObject last)
            : base(last)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="path">Path, as sequence of objects</param>
        public TimelinePath(IEnumerable<ITimelineObject> path)
            : base(path)
        {
        }

        /// <summary>
        /// Concatenates object with path</summary>
        /// <param name="lhs">Prefix object</param>
        /// <param name="rhs">Optional path</param>
        /// <returns>Concatenated path, with lhs as first object</returns>
        public static TimelinePath operator +(ITimelineObject lhs, TimelinePath rhs)
        {
            if (rhs == null)
                return new TimelinePath(lhs);
            ITimelineObject[] path = new ITimelineObject[1 + rhs.Count];
            path[0] = lhs;
            rhs.CopyTo(path, 1);
            return new TimelinePath(path);
        }

        /// <summary>
        /// Concatenates path with object</summary>
        /// <param name="lhs">Optional path</param>
        /// <param name="rhs">Suffix object</param>
        /// <returns>Concatenated path, with rhs as last object</returns>
        public static TimelinePath operator +(TimelinePath lhs, ITimelineObject rhs)
        {
            if (lhs == null)
                return new TimelinePath(rhs);
            ITimelineObject[] path = new ITimelineObject[lhs.Count + 1];
            lhs.CopyTo(path, 0);
            path[lhs.Count] = rhs;
            return new TimelinePath(path);
        }

        /// <summary>
        /// Concatenates two paths</summary>
        /// <param name="lhs">First path. Can be null.</param>
        /// <param name="rhs">Second path. Can be null.</param>
        /// <returns>Concatenated path, with rhs as prefix and lhs as suffix. Is null if both lhs and rhs are null.</returns>
        public static TimelinePath operator +(TimelinePath lhs, TimelinePath rhs)
        {
            if (lhs == null)
                return rhs;
            if (rhs == null)
                return lhs;
            ITimelineObject[] path = new ITimelineObject[lhs.Count + rhs.Count];
            lhs.CopyTo(path, 0);
            rhs.CopyTo(path, lhs.Count);
            return new TimelinePath(path);
        }
    }
}
