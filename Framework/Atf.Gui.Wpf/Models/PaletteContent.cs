//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Class representing global palette of objects that can be dragged onto other controls</summary>
    public class PaletteContent : NotifyPropertyChangedBase, IDragDropConverter
    {
        private ObservableCollection<object> m_items =
            new ObservableCollection<object>();
        private IPaletteService m_paletteService;
        private ICollectionView m_dataView;

        /// <summary>
        /// Constructor</summary>
        /// <param name="service">Palette service</param>
        public PaletteContent(IPaletteService service)
        {
            m_paletteService = service;
            m_dataView = CollectionViewSource.GetDefaultView(m_items);
            m_dataView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
            // Add sort?
        }

        /// <summary>
        /// Adds an item to the palette</summary>
        /// <param name="item">Item to add</param>
        /// <param name="categoryName">Category in which to add item</param>
        public void AddItem(object item, string categoryName)
        {
            m_items.Add(item);
        }

        /// <summary>
        /// Removes an item from the palette</summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(object item)
        {
            m_items.Remove(item);
        }

        #region IPaletteService Members

        /// <summary>
        /// Converts from palette items to actual items</summary>
        /// <param name="items">Items to convert</param>
        public IEnumerable<object> Convert(IEnumerable<object> items)
        {
            return m_paletteService.Convert(items);
        }

        #endregion

        /// <summary>
        /// Gets a view for grouping, sorting, filtering, and navigating a data collection</summary>
        public ICollectionView Data { get { return m_dataView; } }
    }

    /// <summary>
    /// Item on a palette</summary>
    public class PaletteItem : NotifyPropertyChangedBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="item">Palette item</param>
        /// <param name="categoryName">Item category</param>
        public PaletteItem(object item, string categoryName)
        {
            Item = item;
            Category = categoryName;
        }

        /// <summary>
        /// Gets item</summary>
        public object Item { get; private set; }
        /// <summary>
        /// Gets item category</summary>
        public string Category { get; private set; }
    }
}
