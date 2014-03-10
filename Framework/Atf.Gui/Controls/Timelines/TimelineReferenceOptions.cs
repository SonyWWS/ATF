//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls.Timelines
{
    /// <summary>
    /// Options for ITimelineReference for user interfaces, etc.</summary>
    public class TimelineReferenceOptions
    {
        /// <summary>
        /// Gets and sets whether or not the referenced timeline should have all of its groups
        /// displayed on their own rows</summary>
        public virtual bool Expanded
        {
            get { return m_expanded; }
            set { m_expanded = value; }
        }

        private bool m_expanded;
    }
}
