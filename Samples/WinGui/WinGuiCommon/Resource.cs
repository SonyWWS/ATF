//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace WinGuiCommon
{
    /// <summary>
    /// DomNode adapter for resource data</summary>
    public class Resource : DomNodeAdapter
    {
        /// <summary>
        /// Gets or set resource name, such as a label name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.resourceType.nameAttribute); }
            set { SetAttribute(Schema.resourceType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets or set resource size</summary>
        public int Size
        {
            get { return GetAttribute<int>(Schema.resourceType.sizeAttribute); }
            set { SetAttribute(Schema.resourceType.sizeAttribute, value); }
        }
    }
}