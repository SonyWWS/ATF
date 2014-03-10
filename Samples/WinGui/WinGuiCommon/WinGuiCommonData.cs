//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

using Sce.Atf.Dom;

namespace WinGuiCommon
{
    /// <summary>
    /// DomNode adapter for event sequence data</summary>
    public class WinGuiCommonData : DomNodeAdapter
    {
        /// <summary>
        /// Gets list of events</summary>
        public IList<Event> Events
        {
            get { return GetChildList<Event>(Schema.winGuiCommonDataType.eventChild); }
        }
    }
}
