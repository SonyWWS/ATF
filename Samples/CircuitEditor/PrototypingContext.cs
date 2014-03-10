//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class PrototypingContext : Sce.Atf.Controls.Adaptable.Graphs.PrototypingContext
    {
        protected override ChildInfo PrototypeFolderChildInfo
        {
            get { return Schema.circuitDocumentType.prototypeFolderChild; }
        }

        protected override DomNodeType PrototypeType
        {
            get { return Schema.prototypeType.Type; }
        }
    }
}
