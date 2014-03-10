//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace SimpleDomNoXmlEditorSample
{
    /// <summary>
    /// DomNode adapter for resource data</summary>
    public class Resource : DomNodeAdapter
    {
        /// <summary>
        /// Gets name associated with resource, such as a label</summary>
        public string Name
        {
            get { return GetAttribute<string>(DomTypes.resourceType.nameAttribute); }
            set { SetAttribute(DomTypes.resourceType.nameAttribute, value); }
        }

        /// <summary>
        /// Gets or sets resource size</summary>
        public int Size
        {
            get { return GetAttribute<int>(DomTypes.resourceType.sizeAttribute); }
            set { SetAttribute(DomTypes.resourceType.sizeAttribute, value); }
        }
    }
}