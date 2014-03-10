//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to a layer folder</summary>
    public abstract class LayerFolder : DomNodeAdapter
    {

        protected abstract AttributeInfo NameAttribute { get; }

        // required  child info
        protected abstract ChildInfo LayerFolderChild { get; }
        protected abstract ChildInfo ElementRefChildInfo { get; }

  
        /// <summary>
        /// Gets or sets layer folder name</summary>
        public string Name
        {
            get { return (string)DomNode.GetAttribute(NameAttribute); }
            set { DomNode.SetAttribute(NameAttribute, value); }
        }

        /// <summary>
        /// Gets the list of layer folders</summary>
        public IList<LayerFolder> Folders
        {
            get { return GetChildList<LayerFolder>(LayerFolderChild); }
        }

        /// <summary>
        /// Gets the list of module references to circuit modules that belong to this layer</summary>
        public IList<ElementRef> ElementRefs
        {
            get { return GetChildList<ElementRef>(ElementRefChildInfo); }
        }

        /// <summary>
        /// Gets the list of circuit modules that belong to this layer</summary>
        public IEnumerable<Element> GetElements()
        {
            foreach (ElementRef reference in ElementRefs)
                yield return reference.Element;
        }

        /// <summary>
        /// Tests if a module is in this layer</summary>
        /// <param name="element">Module to test</param>
        /// <returns>True iff module is in this layer</returns>
        public bool Contains(Element element)
        {
            foreach (ElementRef reference in ElementRefs)
                if (reference.Element == element)
                    return true;
            return false;
        }

     
    }
}
