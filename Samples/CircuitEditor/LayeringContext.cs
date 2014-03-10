//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    class LayeringContext : Sce.Atf.Controls.Adaptable.Graphs.LayeringContext
    {
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        protected override ChildInfo LayerFolderChildInfo
        {
            get { return Schema.circuitType.layerFolderChild; }
        }

        protected override DomNodeType LayerFolderType
        {
            get { return Schema.layerFolderType.Type; }
        }

        protected override DomNodeType ElementRefType
        {
            get { return Schema.moduleRefType.Type; }
        }
    }
}
