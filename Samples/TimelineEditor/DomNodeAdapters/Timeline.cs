//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Controls.Timelines;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

#pragma warning disable 0649 // suppress "field never set" warning

namespace TimelineEditorSample.DomNodeAdapters
{
    /// <summary>
    /// Adapts DomNode to a Timeline</summary>
    public class Timeline : DomNodeAdapter, IHierarchicalTimeline, IHierarchicalTimelineList
    {
        #region ITimeline Members

        /// <summary>
        /// Creates a new group</summary>
        /// <returns>New group</returns>
        public IGroup CreateGroup()
        {
            return new DomNode(Schema.groupType.Type).As<IGroup>();
        }

        /// <summary>
        /// Creates a new marker</summary>
        /// <returns>New marker</returns>
        public IMarker CreateMarker()
        {
            return new DomNode(Schema.markerType.Type).As<IMarker>();
        }

        /// <summary>
        /// Gets the list of all groups in the timeline</summary>
        public IList<IGroup> Groups
        {
            get { return GetChildList<IGroup>(Schema.timelineType.groupChild); }
        }

        /// <summary>
        /// Gets the list of all markers the timeline</summary>
        public IList<IMarker> Markers
        {
            get { return GetChildList<IMarker>(Schema.timelineType.markerChild); }
        }

        #endregion

        #region IHierarchicalTimeline

        /// <summary>
        /// Gets the references owned by this timeline. This is not a recursive enumeration.</summary>
        IEnumerable<ITimelineReference> IHierarchicalTimeline.References
        {
            get { return GetChildList<ITimelineReference>(Schema.timelineType.timelineRefChild); }
        }

        #endregion

        #region IHierarchicalTimelineList

        /// <summary>
        /// Gets an IList that allows for adding, removing, counting, clearing, etc., the list
        /// of ITimelineReferences</summary>
        public IList<ITimelineReference> References
        {
            get { return GetChildList<ITimelineReference>(Schema.timelineType.timelineRefChild); }
        }

        #endregion

        // Only client-specific code can create a new ITimelineReference and add it.
        internal void AddReference(ITimelineReference reference)
        {
            GetChildList<ITimelineReference>(Schema.timelineType.timelineRefChild).Add(reference);
        }
    }
}




