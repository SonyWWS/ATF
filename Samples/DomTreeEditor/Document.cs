//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Adapts root node to IDocument</summary>
    public class Document : DomDocument
    {        
        /// <summary>
        /// Gets the document client's file type name</summary>
        public override string Type
        {
            get { return "UI".Localize(); }
        }
    }
}
