//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapts DomNode to circuit annotation, that is, note on the circuit</summary>
    public class Annotation : Sce.Atf.Controls.Adaptable.Graphs.Annotation
    {
        /// <summary>
        /// Gets annotation text attribute</summary>
        protected override AttributeInfo TextAttribute
        {
            get { return Schema.annotationType.textAttribute; }
        }

        /// <summary>
        /// Gets annotation x-coordinate position attribute</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.annotationType.xAttribute; }
        }

        /// <summary>
        /// Gets annotation y-coordinate position attribute</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.annotationType.yAttribute; }
        }

        /// <summary>
        /// Gets annotation width attribute</summary>
        protected override AttributeInfo WidthAttribute
        {
            get { return Schema.annotationType.widthAttribute; }
        }

        /// <summary>
        /// Gets annotation height attribute</summary>
        protected override AttributeInfo HeightAttribute
        {
            get { return Schema.annotationType.heightAttribute; }
        }

        /// <summary>
        /// Gets annotation background color attribute</summary>
        protected override AttributeInfo BackColorAttribute
        {
            get { return Schema.annotationType.backcolorAttribute; }
        }

        /// <summary>
        /// Gets annotation foreColorAttribute color attribute</summary>
        protected override AttributeInfo ForeColorAttribute
        {
            get { return Schema.annotationType.foreColorAttribute; }
        }
    }
}
