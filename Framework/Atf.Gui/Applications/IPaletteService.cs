//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for a palette service, which presents to users palette items
    /// that can be dragged and dropped onto other controls</summary>
    public interface IPaletteService
    {
        /// <summary>
        /// Adds an item to the palette in the given category</summary>
        /// <param name="item">Palette item</param>
        /// <param name="categoryName">Category name</param>
        /// <param name="client">Client that instantiates item during drag and drop operations</param>
        void AddItem(object item, string categoryName, IPaletteClient client);

        /// <summary>
        /// Removes an item from the palette</summary>
        /// <param name="item">Item to remove</param>
        void RemoveItem(object item);
    }
}
