//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;

namespace WinGuiCommon
{
    /// <summary>
    /// DomNode adapter for event data</summary>
    public class Event : DomNodeAdapter
    {
        /// <summary>
        /// Gets event name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.eventType.nameAttribute); }
            set { SetAttribute(Schema.eventType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets event time</summary>
        public int Time
        {
            get { return GetAttribute<int>(Schema.eventType.timeAttribute); }
            set { SetAttribute(Schema.eventType.timeAttribute, value); }
        }

        /// <summary>
        /// Gets event duration</summary>
        public int Duration
        {
            get { return GetAttribute<int>(Schema.eventType.durationAttribute); }
            set { SetAttribute(Schema.eventType.durationAttribute, value); }
        }

        /// <summary>
        /// Gets event resources</summary>
        public IList<Resource> Resources
        {
            get { return GetChildList<Resource>(Schema.eventType.resourceChild); }
        }
    }
}