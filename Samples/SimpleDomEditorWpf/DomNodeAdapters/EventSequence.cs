//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;

namespace SimpleDomEditorWpfSample
{
    /// <summary>
    /// DomNode adapter for event sequence data</summary>
    public class EventSequence : DomNodeAdapter
    {
        /// <summary>
        /// Gets list of Events in sequence</summary>
        public IList<Event> Events
        {
            get { return GetChildList<Event>(Schema.eventSequenceType.eventChild); }
        }
    }
}
