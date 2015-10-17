//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Data;
using Sce.Atf.Wpf.Markup;
using System.Globalization;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Provides a TreeView ListView</summary>
    public class TreeListView : TreeView
    {
        /// <summary>
        /// Static constructor</summary>
        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
        }

        /// <summary>
        /// Gets a collection of the GridViewColumn in the ListView</summary>
        public GridViewColumnCollection Columns
        {
            get
            {
                if (m_columns == null)
                {
                    m_columns = new GridViewColumnCollection();
                }

                return m_columns;
            }
        }

        /// <summary>
        /// Selects the given item</summary>
        /// <param name="item">Item to be selected</param>
        /// <returns><c>True</c> if item was selected</returns>
        public bool SetSelectedItem(object item)
        {
            if (item == null) return false;

            TreeListViewItem container = (TreeListViewItem)ItemContainerGenerator.ContainerFromItem(item);
            if (container != null)
            {
                container.IsSelected = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets TreeListViewItem DependencyObject to serve as an item container in a TreeListView</summary>
        /// <returns>TreeListViewItem to serve as item container in a TreeListView</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        /// <summary>
        /// Determines whether an item can serve as an item container in a TreeListView</summary>
        /// <param name="item">Item to test if can be container</param>
        /// <returns><c>True</c> if item can serve as an item container in a TreeListView</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        private GridViewColumnCollection m_columns;
    }

    /// <summary>
    /// Describes binding of tree view items with a type and ancestor level for type</summary>
    /// <typeparam name="T">Item with dependency property</typeparam>
    public class AncestorTypeBinding<T> : Binding
        where T : class
    {
        private AncestorTypeBinding() { }

        /// <summary>
        /// Constructor with path</summary>
        /// <param name="path">Path describing binding</param>
        public AncestorTypeBinding(string path)
            : this(path, 1) { }

        /// <summary>
        /// Constructor with path and ancestor level</summary>
        /// <param name="path">Path describing binding</param>
        /// <param name="ancestorLevel">Ordinal position of desired ancestor among all ancestors of type</param>
        public AncestorTypeBinding(string path, int ancestorLevel)
            : base(path)
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(T), ancestorLevel);
        }
    }

    /// <summary>
    /// Selectable item in a TreeListView</summary>
    public class TreeListViewItem : TreeViewItem
    {
        /// <summary>
        /// Static constructor</summary>
        static TreeListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewItem), new FrameworkPropertyMetadata(typeof(TreeListViewItem)));
        }

        /// <summary>
        /// Gets level of item in TreeListView</summary>
        public int Level
        {
            get
            {
                if (m_level == -1)
                {
                    TreeListViewItem parent = ItemsControl.ItemsControlFromItemContainer(this) as TreeListViewItem;
                    m_level = (parent != null) ? parent.Level + 1 : 0;
                }
                return m_level;
            }
        }

        /// <summary>
        /// Executes operations required after initialization</summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        /// <summary>
        /// Gets TreeListViewItem DependencyObject to serve as an item container in a TreeListView</summary>
        /// <returns>TreeListViewItem to serve as item container in a TreeListView</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeListViewItem();
        }

        /// <summary>
        /// Determines whether an item can serve as an item container in a TreeListView</summary>
        /// <param name="item">Item to test if can be container</param>
        /// <returns><c>True</c> if item can serve as an item container in a TreeListView</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        /// <summary>
        /// Event handler for when the ItemContainerGenerator changes</summary>
        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                return;

            BindingOperations.SetBinding(this,
                HorizontalContentAlignmentProperty,
                new AncestorTypeBinding<ItemsControl>("HorizontalContentAlignment"));

            BindingOperations.SetBinding(this,
                VerticalContentAlignmentProperty,
                new AncestorTypeBinding<ItemsControl>("VerticalContentAlignment"));
        }

        private int m_level = -1;
    }

    /// <summary>
    /// Represents an object that specifies the layout of a row of data in a TreeView ListView</summary>
    public class TreeGridViewRowPresenter : GridViewRowPresenter
    {
        /// <summary>
        /// Row first column indent dependency property</summary>
        public static DependencyProperty FirstColumnIndentProperty =
            DependencyProperty.Register(
                "FirstColumnIndent",
                typeof(Double),
                typeof(TreeGridViewRowPresenter),
                new PropertyMetadata(0d));

        /// <summary>
        /// Row expander dependency property</summary>
        public static DependencyProperty ExpanderProperty =
            DependencyProperty.Register(
                "Expander",
                typeof(UIElement),
                typeof(TreeGridViewRowPresenter),
                new FrameworkPropertyMetadata(null, OnExpanderChanged));

        /// <summary>
        /// Gets or sets first column indent dependency property</summary>
        public Double FirstColumnIndent
        {
            get { return (Double)this.GetValue(FirstColumnIndentProperty); }
            set { this.SetValue(FirstColumnIndentProperty, value); }
        }

        /// <summary>
        /// Gets or sets row expander dependency property</summary>
        public UIElement Expander
        {
            get { return (UIElement)this.GetValue(ExpanderProperty); }
            set { this.SetValue(ExpanderProperty, value); }
        }

        /// <summary>
        /// Constructor</summary>
        public TreeGridViewRowPresenter()
        {
            m_children = new UIElementCollection(this, this);
        }

        /// <summary>
        /// Arranges (positions and determines size) of row of data in a TreeView ListView</summary>
        /// <param name="arrangeSize">Row's final size</param>
        /// <returns>Row's final size</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Size s = base.ArrangeOverride(arrangeSize);

            if (this.Columns == null || this.Columns.Count == 0)
                return s;

            UIElement expander = this.Expander;
            double current = 0;
            double max = arrangeSize.Width;
            for (int x = 0; x < this.Columns.Count; x++)
            {
                GridViewColumn column = this.Columns[x];

                // Actual index needed for column reorder
                UIElement uiColumn = (UIElement)base.GetVisualChild((int)ActualIndexProperty.GetValue(column, null));

                // Compute column width
                double w = Math.Min(max, (Double.IsNaN(column.Width)) ? (double)DesiredWidthProperty.GetValue(column, null) : column.Width);

                // First column indent
                if (x == 0 && expander != null)
                {
                    double indent = FirstColumnIndent + expander.DesiredSize.Width;
                    uiColumn.Arrange(new Rect(current + indent, 0, Math.Max(0, w - indent), arrangeSize.Height));
                }
                else
                {
                    uiColumn.Arrange(new Rect(current, 0, w, arrangeSize.Height));
                }

                max -= w;
                current += w;
            }

            // Show expander
            if (expander != null)
            {
                expander.Arrange(new Rect(this.FirstColumnIndent, 0, expander.DesiredSize.Width, expander.DesiredSize.Height));
            }

            return s;
        }

        /// <summary>
        /// Sets desired size of row of data in a TreeView ListView</summary>
        /// <param name="constraint">Desired size</param>
        /// <returns>Actual size of row of data</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size s = base.MeasureOverride(constraint);

            // Measure expander
            UIElement expander = this.Expander;
            if (expander != null)
            {
                // Compute max measure
                expander.Measure(constraint);
                s.Width = Math.Max(s.Width, expander.DesiredSize.Width);
                s.Height = Math.Max(s.Height, expander.DesiredSize.Height);
            }

            return s;
        }

        /// <summary>
        /// Obtains the Visual object at the index</summary>
        /// <param name="index">Index of Visual object to obtain</param>
        /// <returns>Visual object at index</returns>
        protected override Visual GetVisualChild(int index)
        {
            if (index < base.VisualChildrenCount)
                return base.GetVisualChild(index);

            return this.Expander;
        }

        /// <summary>
        /// Obtains count of Visual objects</summary>
        /// <returns>Count of Visual objects</returns>
        protected override int VisualChildrenCount
        {
            get
            {
                if (this.Expander != null)
                    return base.VisualChildrenCount + 1;

                return base.VisualChildrenCount;
            }
        }

        private static void OnExpanderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Use a second UIElementCollection so base methods work as original
            var p = (TreeGridViewRowPresenter)d;

            if (e.OldValue != null)
            {
                p.m_children.Remove(e.OldValue as UIElement);
            }

            if (e.NewValue != null)
            {
                p.m_children.Add((UIElement)e.NewValue);
            }
        }

        private static PropertyInfo ActualIndexProperty = typeof(GridViewColumn).GetProperty("ActualIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo DesiredWidthProperty = typeof(GridViewColumn).GetProperty("DesiredWidth", BindingFlags.NonPublic | BindingFlags.Instance);

        private UIElementCollection m_children;
    }

    /// <summary>
    /// Converter to determine the width of the last column</summary>
    public class LastColumnFillWidthConverter : MultiConverterMarkupExtension<LastColumnFillWidthConverter>
    {
        #region IMultiValueConverter Members

        /// <summary>
        /// Converter function to calculate how wide to make the final column</summary>
        /// <param name="values">values[0] is the ListView. values[1] is the total width of the control.</param>
        /// <param name="type">Not used</param>
        /// <param name="parameter">Not used</param>
        /// <param name="culture">Not used</param>
        /// <returns>The remaining width available for the final column</returns>
        public override object Convert(object[] values, Type type, object parameter, CultureInfo culture)
        {
            if (ValuesPopulated(values))
            {
                ListView l = values[0] as ListView;
                GridView g = l.View as GridView;
                double total = 0;
                for (int i = 0; i < g.Columns.Count - 1; i++)
                {
                    total += g.Columns[i].Width;
                }

                return (double)values[1] - total;
            }

            return 0.0;
        }

        static bool ValuesPopulated(object[] values)
        {
            foreach (object value in values)
            {
                if (value == null || value.Equals(DependencyProperty.UnsetValue))
                    return false;
            }
            return true;
        }

        #endregion
    }

    /// <summary>
    /// Converter to calculate the amount to indent an item based on its level in the tree</summary>
    public class LevelToIndentConverter : ConverterMarkupExtension<LevelToIndentConverter>
    {
        #region IValueConverter Members

        /// <summary>
        /// Converter function to calculate the amount to indent an item based on its level in the tree</summary>
        /// <param name="o">The level of the item (as an integer)</param>
        /// <param name="type">Not used</param>
        /// <param name="parameter">Optional: The indent size to use per level (as a string that will be parsed to a double)</param>
        /// <param name="culture">Not used</param>
        /// <returns>The size to indent by</returns>
        public override object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            double indentScale = 20.0;
            if (parameter != null)
                indentScale = Double.Parse((string)parameter);

            // return new Thickness((int)o * identScale, 0, 0, 0);
            return (int)o * indentScale;
        }

        #endregion
    }
}
