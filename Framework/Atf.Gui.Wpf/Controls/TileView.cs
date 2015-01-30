//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Class representing a tiled layout view</summary>
    public class TileView : ViewBase
    {
        /// <summary>
        /// Gets and sets the width of the tiled items</summary>
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        /// <summary>
        /// Dependency property for item width</summary>
        public static DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", 
                typeof(double), typeof(TileView), new PropertyMetadata(1.0, ScalePropertyChanged));

        /// <summary>
        /// Gets and sets the height of the tiled items</summary>
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        /// <summary>
        /// Dependency property for item height</summary>
        public static DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", 
                typeof(double), typeof(TileView), new PropertyMetadata(1.0, ScalePropertyChanged));

        /// <summary>
        /// Event fired when the item width or height changes</summary>
        /// <param name="obj">unused</param>
        /// <param name="args">unused</param>
        public static void ScalePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
        }

        /// <summary>
        /// Gets and sets the template for the appearance of the tiled items</summary>
        public DataTemplate ItemTemplate
        {
            get { return itemTemplate; }
            set { itemTemplate = value; }
        }

        /// <summary>
        /// Gets and sets the background color for selected items</summary>
        public Brush SelectedBackground
        {
            get { return selectedBackground; }
            set { selectedBackground = value; }
        }

        /// <summary>
        /// Gets and sets the border color for selected items</summary>
        public Brush SelectedBorderBrush
        {
            get { return selectedBorderBrush; }
            set { selectedBorderBrush = value; }
        }

        /// <summary>
        /// Gets the object that is associated with the style for the view mode</summary>
        protected override object DefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "TileView"); }
        }

        /// <summary>
        /// Gets the style to use for the items in the view mode</summary>
        protected override object ItemContainerDefaultStyleKey
        {
            get { return new ComponentResourceKey(GetType(), "TileViewItem"); }
        }
        
        private DataTemplate itemTemplate;
        private Brush selectedBackground = Brushes.Transparent;
        private Brush selectedBorderBrush = Brushes.Black;
    }
}
