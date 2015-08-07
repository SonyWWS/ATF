//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;

using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// Adapts DomNode to a State in FSMs</summary>
    public class State : DomNodeAdapter, IGraphNode
    {
        /// <summary>
        /// Gets or sets the user-visible name</summary>
        public string Name
        {
            get { return GetAttribute<string>(Schema.stateType.labelAttribute); }
            set { SetAttribute(Schema.stateType.labelAttribute, value); }
        }

        /// <summary>
        /// Gets or sets whether State is hidden. This property is just an example and is not
        /// actually used to control the visibility of the State.</summary>
        public bool Hidden
        {
            get { return GetAttribute<bool>(Schema.stateType.hiddenAttribute); }
            set { SetAttribute(Schema.stateType.hiddenAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the position of the module (backing DOM data)</summary>
        /// <remarks>See the Bounds property.</remarks>
        public Point Position
        {
            get
            {
                return new Point(
                    GetAttribute<int>(Schema.stateType.xAttribute),
                    GetAttribute<int>(Schema.stateType.yAttribute));
            }
            set
            {
                SetAttribute(Schema.stateType.xAttribute, value.X);
                SetAttribute(Schema.stateType.yAttribute, value.Y);
            }
        }

        /// <summary>
        /// Gets or sets the size. Since states are rendered as circles, this is the
        /// circle diameter.</summary>
        public int Size
        {
            get { return GetAttribute<int>(Schema.stateType.sizeAttribute); }
            set { SetAttribute(Schema.stateType.sizeAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the local bounds information</summary>
        public Rectangle Bounds
        {
            get
            {
                int diameter = Math.Max(Size, MinimumSize);
                return new Rectangle(Position, new Size(diameter, diameter));
            }
            set
            {
                Position = value.Location;
                Size = Math.Max(value.Width, value.Height);
            }
        }

        private const int MinimumSize = 64;
    }
}
