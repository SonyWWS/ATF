//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    class Pin : Sce.Atf.Controls.Adaptable.Graphs.Pin
    {
        protected override AttributeInfo TypeAttribute
        {
            get { return Schema.pinType.typeAttribute; }
        }

        protected override AttributeInfo NameAttribute
        {
            get { return Schema.pinType.nameAttribute; }
        }
    }
}
