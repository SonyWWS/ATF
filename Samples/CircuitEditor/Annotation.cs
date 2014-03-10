//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    public class Annotation : Sce.Atf.Controls.Adaptable.Graphs.Annotation
    {
        protected override AttributeInfo TextAttribute
        {
            get { return Schema.annotationType.textAttribute; }
        }

        protected override AttributeInfo XAttribute
        {
            get { return Schema.annotationType.xAttribute; }
        }

        protected override AttributeInfo YAttribute
        {
            get { return Schema.annotationType.yAttribute; }
        }

        protected override AttributeInfo WidthAttribute
        {
            get { return Schema.annotationType.widthAttribute; }
        }

        protected override AttributeInfo HeightAttribute
        {
            get { return Schema.annotationType.heightAttribute; }
        }

        protected override AttributeInfo BackColorAttribute
        {
            get { return Schema.annotationType.backcolorAttribute; }
        }
    }
}
