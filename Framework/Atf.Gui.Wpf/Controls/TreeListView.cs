//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using Sce.Atf.Wpf.Markup;
using System.Globalization;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Provides a TreeView ListView</summary>
    public class TreeListView : TreeView
    {
        #region Ctors

        /// <summary>
        /// Static constructor</summary>
        static TreeListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListView), new FrameworkPropertyMetadata(typeof(TreeListView)));
        }

        /// <summary>
        /// Constructor</summary>
        public TreeListView()
        {
        }

        #endregion

        #region Public Properties

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
        private GridViewColumnCollection m_columns;

        /// <summary>
        /// Selects the given item</summary>
        /// <param name="item">Item to be selected</param>
        /// <returns>True iff item was selected</returns>
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

        #endregion

        #region Overrides

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
        /// <returns>True iff item can serve as an item container in a TreeListView</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        #endregion
    }

    /// <summary>
    /// Selectable item in a TreeListView</summary>
    public class TreeListViewItem : TreeViewItem
    {
        #region Ctors

        /// <summary>
        /// Static constructor</summary>
        static TreeListViewItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeListViewItem), new FrameworkPropertyMetadata(typeof(TreeListViewItem)));
        }

        /// <summary>
        /// Constructor</summary>
        public TreeListViewItem()
        {
        }

        #endregion

        #region Public Properties

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

        #endregion

        #region Overrides

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
        /// <returns>True iff item can serve as an item container in a TreeListView</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeListViewItem;
        }

        #endregion

        #region Private Fields

        private int m_level = -1;

        #endregion
    }

    /// <summary>
    /// Represents an object that specifies the layout of a row of data in a TreeView ListView</summary>
    public class TreeGridViewRowPresenter : GridViewRowPresenter
    {
        #region Dependency Properties

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
                new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnExpanderChanged)));

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

        private static PropertyInfo ActualIndexProperty = typeof(GridViewColumn).GetProperty("ActualIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo DesiredWidthProperty = typeof(GridViewColumn).GetProperty("DesiredWidth", BindingFlags.NonPublic | BindingFlags.Instance);

        #endregion

        #region Ctors

        /// <summary>
        /// Constructor</summary>
        public TreeGridViewRowPresenter()
        {
            m_children = new UIElementCollection(this, this);
        }

        #endregion

        #region Overrides

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

        #endregion

        #region Private Methods

        private static void OnExpanderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Use a second UIElementCollection so base methods work as original
            TreeGridViewRowPresenter p = (TreeGridViewRowPresenter)d;

            p.m_children.Remove(e.OldValue as UIElement);
            p.m_children.Add((UIElement)e.NewValue);
        }

        #endregion

        #region Private Fields

        private UIElementCollection m_children;

        #endregion
    }
    
    /// <summary>
    /// Converts level to indentation for control</summary>
    public class LevelToIndentConverter : ConverterMarkupExtension<LevelToIndentConverter>
    {
        /// <summary>
        /// Covmerts level to indentation for control</summary>
        /// <param name="o">Level</param>
        /// <param name="type">Type of level object (unused)</param>
        /// <param name="parameter">String representation of a indentation scale, i.e., indention for one level</param>
        /// <param name="culture">Local information (unused)</param>
        /// <returns>Indentation for control</returns>
        public override object Convert(object o, Type type, object parameter, CultureInfo culture)
        {
            double identScale = 20.0;
            if (parameter != null)
                identScale = Double.Parse((string)parameter);

            return (int)o * identScale;
        }
    }
}
