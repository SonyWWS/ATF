//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to diagram annotation</summary>
    public abstract class Annotation : DomNodeAdapter, IAnnotation
    {
        // required  DOM attributes info
        protected abstract AttributeInfo TextAttribute { get; }
        protected abstract AttributeInfo XAttribute { get; }
        protected abstract AttributeInfo YAttribute { get; }
        protected abstract AttributeInfo WidthAttribute { get; }
        protected abstract AttributeInfo HeightAttribute { get; }
        protected abstract AttributeInfo BackColorAttribute { get; }
 
        /// <summary>
        /// Gets or sets the comment text</summary>
        public string Text
        {
            get { return (string)DomNode.GetAttribute(TextAttribute); }
            set { DomNode.SetAttribute(TextAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the comment location's center point(backing DOM data)</summary>
        public virtual Point Location
        {
            get
            {
                return new Point(
                    GetAttribute<int>(XAttribute),
                    GetAttribute<int>(YAttribute));
            }
            set
            {
                SetAttribute(XAttribute, value.X);
                SetAttribute(YAttribute, value.Y);
            }
        }

        /// <summary>
        /// Gets or sets the size of the comment</summary>
        public virtual Size Size
        {
            get
            {
                return new Size(
                    GetAttribute<int>(WidthAttribute),
                    GetAttribute<int>(HeightAttribute));
            }
            set
            {
                SetAttribute(WidthAttribute, value.Width);
                SetAttribute(HeightAttribute, value.Height);
            }
        }

        /// <summary>
        /// Gets or sets the local bounds information</summary>
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle(Location, Size);
            }
            set
            {
                Location = value.Location;
                Size = value.Size;
            }
        }

        /// <summary>
        /// Gets and sets the background color of the annotation</summary>
        public Color BackColor 
        {
            get
            {
                 return BackColorAttribute == null ? SystemColors.Info : 
                    Color.FromArgb((int)DomNode.GetAttribute(BackColorAttribute));
            }
            set { DomNode.SetAttribute(BackColorAttribute, value.ToArgb()); }
        }


        /// <summary>
        /// Sets the size (as a rectangle) of the annotation's text, 
        /// as measured in the current graphics context</summary>
        /// <param name="size">Text size</param>
        public void SetTextSize(Size size)
        {
            Size = size;
        }
    }
}
