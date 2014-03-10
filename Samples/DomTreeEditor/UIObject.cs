//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Base for all UI DomNode adapters, with a name attribute</summary>
    public abstract class UIObject : DomNodeAdapter
    {
        /// <summary>
        /// Gets or sets the UI object's name</summary>
        public string Name
        {
            get { return GetAttribute<string>(UISchema.UIObjectType.nameAttribute); }
            set { SetAttribute(UISchema.UIObjectType.nameAttribute, value); }
        }
    }
}
