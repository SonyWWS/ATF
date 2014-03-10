//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Dom;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Read-only data used to describe how a particular DomNodeType should appear in a
    /// palette of items that the user can drag and drop into a document. For example,
    /// MyDomNodeType.SetTag( new NodeTypePaletteItem(...)). IPaletteClient implementors
    /// can call DomNodeType.GetTag to retrieve these NodeTypePaletteItem objects.</summary>
    public class NodeTypePaletteItem
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="nodeType">DomNodeType that this object applies to</param>
        /// <param name="name">User-readable name of this DomNodeType</param>
        /// <param name="description">User-readable description of this DomNodeType</param>
        /// <param name="category">Category of DomNodeType</param>
        /// <param name="imageKey">Image resource name, e.g., "DomTreeEditorSample.Resources.form.png"</param>
        public NodeTypePaletteItem(
            DomNodeType nodeType,
            string name,
            string description,
            string category,
            object imageKey)
            : this(nodeType, name, description, category, imageKey, new DomNode(nodeType))
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="nodeType">DomNodeType that this object applies to</param>
        /// <param name="name">User-readable name of this DomNodeType</param>
        /// <param name="description">User-readable description of this DomNodeType</param>
        /// <param name="category">Category of DomNodeType</param>
        /// <param name="imageKey">Image resource name, e.g., "DomTreeEditorSample.Resources.form.png"</param>
        /// <param name="protoType">Prototype DomNode</param>
        public NodeTypePaletteItem(
            DomNodeType nodeType,
            string name,
            string description,
            string category,
            object imageKey,
            DomNode protoType)
        {
            NodeType = nodeType;
            Name = name;
            Category = category;
            Description = description;
            ImageKey = imageKey;
            Prototype = protoType;
        }

        /// <summary>
        /// Gets the DomNodeType that this object applies to</summary>
        public DomNodeType NodeType { get; private set; }

        /// <summary>
        /// Gets the User-readable name of this DomNodeType</summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the User-readable description of this DomNodeType</summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the category of this DomNodeType</summary>
        public string Category { get; private set; }

        /// <summary>
        /// Gets the image resource name, e.g., "DomTreeEditorSample.Resources.form.png"</summary>
        public object ImageKey { get; private set; }

        /// <summary>
        /// Gets a prototype DomNode of the required type</summary>
        public DomNode Prototype { get; private set; }
    }
}
