//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapts DomNode to a diagram annotation</summary>
    public class Annotation : DomNodeAdapter, IAnnotation
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            if (string.IsNullOrEmpty(Text))
                Text = "Comment";
        }
 
        /// <summary>
        /// Gets and sets the comment text</summary>
        public string Text
        {
            get { return (string)DomNode.GetAttribute(Schema.annotationType.textAttribute); }
            set { DomNode.SetAttribute(Schema.annotationType.textAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the comment location's center point</summary>
        public Point Location
        {
            get
            {
                return new Point(
                    (int)GetAttribute<int>(Schema.annotationType.xAttribute),
                    (int)GetAttribute<int>(Schema.annotationType.yAttribute));
            }
            set
            {
                SetAttribute(Schema.annotationType.xAttribute, value.X);
                SetAttribute(Schema.annotationType.yAttribute, value.Y);
            }
        }

        /// <summary>
        /// Gets or sets the local bounds information</summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(Location, m_size);
            }
            set
            {
                Location = value.Location;
                m_size = value.Size;
            }
        }

        /// <summary>
        /// Sets the size (as a rectangle) of the annotation's text, 
        /// as measured in the current graphics context</summary>
        /// <param name="size">Text size</param>
        public void SetTextSize(Size size)
        {
            m_size = size;
        }

        private Size m_size;
    }
}
