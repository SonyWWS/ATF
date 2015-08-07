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
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            var editingContext = DomNode.Cast<EditingContext>();
            editingContext.DirtyChanged += (sender, args) => Dirty = editingContext.Dirty;
        }

        /// <summary>
        /// Gets the document client's file type name</summary>
        public override string Type
        {
            get { return "UI".Localize(); }
        }
    }
}
