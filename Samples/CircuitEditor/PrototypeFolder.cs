//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    class PrototypeFolder : Sce.Atf.Controls.Adaptable.Graphs.PrototypeFolder
    {
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.prototypeFolderType.nameAttribute; }
        }

        protected override ChildInfo PrototypeChildInfo
        {
            get { return Schema.prototypeFolderType.prototypeChild; }
        }

        protected override ChildInfo PrototypeFolderChildInfo
        {
            get { return Schema.prototypeFolderType.prototypeFolderChild; }
        }
    }
}
