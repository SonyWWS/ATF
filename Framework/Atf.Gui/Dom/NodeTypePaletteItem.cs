//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Read-only data used to describe how a particular DomNodeType should appear in a
    /// palette of items that the user can drag and drop into a document. Usage example:
    /// MyDomNodeType.SetTag( new NodeTypePaletteItem(...)). IPaletteClient implementors
    /// can call DomNodeType.GetTag to retrieve these NodeTypePaletteItem objects.</summary>
    public class NodeTypePaletteItem
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="nodeType">DomNodeType that this object applies to</param>
        /// <param name="name">User-readable name of this DomNodeType</param>
        /// <param name="description">User-readable description of this DomNodeType</param>
        /// <param name="imageName">Image resource name, e.g., "DomTreeEditorSample.Resources.form.png"</param>
        public NodeTypePaletteItem(
            DomNodeType nodeType,
            string name,
            string description,
            string imageName)
        {
            NodeType = nodeType;
            Name = name;
            Description = description;
            ImageName = imageName;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="nodeType">DomNodeType that this object applies to</param>
        /// <param name="name">User-readable name of this DomNodeType</param>
        /// <param name="description">User-readable description of this DomNodeType</param>
        /// <param name="imageName">Image resource name, e.g., "DomTreeEditorSample.Resources.form.png"</param>
        /// <param name="category">Category in which the type is displayed in the palette control</param>
        /// <param name="menuText">Text for the context-menu command to add a new instance of this type</param>
        public NodeTypePaletteItem(
            DomNodeType nodeType,
            string name,
            string description,
            string imageName,
            string category,
            string menuText)
        {
            NodeType = nodeType;
            Name = name;
            Description = description;
            ImageName = imageName;
            Category = category;
            MenuText = menuText;
        }

        /// <summary>
        /// DomNodeType that this object applies to</summary>
        public readonly DomNodeType NodeType;

        /// <summary>
        /// User-readable name of this DomNodeType</summary>
        public readonly string Name;

        /// <summary>
        /// User-readable description of this DomNodeType</summary>
        public readonly string Description;

        /// <summary>
        /// Image resource name, e.g., "DomTreeEditorSample.Resources.form.png"</summary>
        public readonly string ImageName;

        /// <summary>
        /// User-readable category for this DomNodeType</summary>
        public readonly string Category;

        /// <summary>
        /// User-readable menu text for this DomNodeType</summary>
        public readonly string MenuText;
    }
}

