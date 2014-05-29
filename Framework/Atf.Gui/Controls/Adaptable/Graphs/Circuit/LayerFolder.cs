//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to a layer folder. A layer folder allows a hierarchy of layers to be defined. 
    /// Folders can be hidden by the user to allow layering.</summary>
    public abstract class LayerFolder : DomNodeAdapter
    {

        /// <summary>
        /// Gets name attribute for layer folder</summary>
        protected abstract AttributeInfo NameAttribute { get; }

        // required  child info
        /// <summary>
        /// Gets ChildInfo for folders in layer folder</summary>
        protected abstract ChildInfo LayerFolderChild { get; }
        /// <summary>
        /// Gets ChildInfo for references to layers in layer folder</summary>
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
        /// Gets list of circuit modules that belong to this layer</summary>
        /// <returns>List of circuit modules that belong to this layer</returns>
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
