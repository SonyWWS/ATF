//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to a module reference, which is used within layer folders to represent
    /// circuit modules that belong to that layer</summary>
    public abstract class ElementRef : DomNodeAdapter
    {
        /// <summary>
        /// Gets the AttributeInfo for a module reference</summary>
        protected abstract AttributeInfo RefAttribute { get; }
        
        /// <summary>
        /// Gets or sets the referenced module</summary>
        public Element Element
        {
            get { return GetReference<Element>(RefAttribute); }
            set { SetReference(RefAttribute, value); }
        }

      
    }
}
