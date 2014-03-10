//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for a module reference, used within layer folders to represent
    /// circuit modules that belong to that layer.
    /// </summary>
    public class ModuleRef : ElementRef
    {
        /// <summary>
        /// Gets the AttributeInfo that represents the MinHeight property</summary>
        protected override AttributeInfo RefAttribute
        {
            get { return Schema.moduleRefType.refAttribute; }
        }
    }
}
