//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;


namespace Sce.Atf
{
    /// <summary>
    /// Attribute to mark properties, for automatically grouping items in DataBoundListView control</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class GroupAttribute : Attribute
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="groupName">Group name</param>
        public GroupAttribute(string groupName)
        {
            m_groupName = groupName;
        }

        /// <summary>
        /// Gets the group name (used for key)</summary>
        public string GroupName
        {
            get { return m_groupName; }
        }

        /// <summary>
        /// Gets or sets the header text for the group</summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the property names that are read-only</summary>
        public string ReadOnlyProperties
        {
            get
            {
                return m_readOnlyProperties;
            }
            set
            {
                m_readOnlyProperties = value;
            }
        }

        /// <summary>
        /// Gets or sets the property names that are edited only by external editors</summary>
        public string ExternalEditorProperties
        {
            get
            {
                return m_externalEditorProperties;
            }
            set
            {
                m_externalEditorProperties = value;
            }
        }



        private readonly string m_groupName;
        private string m_readOnlyProperties;
        private string m_externalEditorProperties;
    }

}
