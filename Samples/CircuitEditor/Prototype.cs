//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    class Prototype : Sce.Atf.Controls.Adaptable.Graphs.Prototype
    {
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.prototypeType.nameAttribute; }
        }

        protected override ChildInfo ElementChildInfo
        {
            get { return Schema.prototypeType.moduleChild; }
        }

        protected override ChildInfo WireChildInfo
        {
            get { return Schema.prototypeType.connectionChild; }
        }
    }
}
