//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    class LayerFolder : Sce.Atf.Controls.Adaptable.Graphs.LayerFolder
    {
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.layerFolderType.nameAttribute; }
        }

        protected override ChildInfo LayerFolderChild
        {
            get { return Schema.layerFolderType.layerFolderChild; }
        }

        protected override ChildInfo ElementRefChildInfo
        {
            get { return Schema.layerFolderType.moduleRefChild; }
        }
    }
}
