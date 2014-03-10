//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;

namespace SimpleDomNoXmlEditorSample
{
    /// <summary>
    /// DomNode adapter for event data</summary>
    public class Event : DomNodeAdapter
    {
        /// <summary>
        /// Gets or sets name associated with event, such as a label</summary>
        public string Name
        {
            get { return GetAttribute<string>(DomTypes.eventType.nameAttribute); }
            set { SetAttribute(DomTypes.eventType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets or sets time associated with event</summary>
        public int Time
        {
            get { return GetAttribute<int>(DomTypes.eventType.timeAttribute); }
            set { SetAttribute(DomTypes.eventType.timeAttribute, value); }
        }

        /// <summary>
        /// Gets or sets event duration</summary>
        public int Duration
        {
            get { return GetAttribute<int>(DomTypes.eventType.durationAttribute); }
            set { SetAttribute(DomTypes.eventType.durationAttribute, value); }
        }

        /// <summary>
        /// Gets event Resources</summary>
        public IList<Resource> Resources
        {
            get { return GetChildList<Resource>(DomTypes.eventType.resourceChild); }
        }
    }
}