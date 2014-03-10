//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// GridView sort behavior</summary>
    public static class GridViewSortBehavior
    {
        /// <summary>
        /// Attached property to indicate user can sort GridView columns</summary>
        public static readonly DependencyProperty CanUserSortColumnsProperty =
            DependencyProperty.RegisterAttached(
                "CanUserSortColumns",
                typeof(bool),
                typeof(GridViewSortBehavior),
                new FrameworkPropertyMetadata(OnCanUserSortColumnsChanged));

        /// <summary>
        /// Attached property to indicate whether can sort GridViewColumn</summary>
        public static readonly DependencyProperty CanUseSortProperty =
            DependencyProperty.RegisterAttached(
                "CanUseSort",
                typeof(bool),
                typeof(GridViewSortBehavior),
                new FrameworkPropertyMetadata(true));

        /// <summary>
        /// Attached property to indicate sort direction</summary>
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.RegisterAttached(
                "SortDirection",
                typeof(ListSortDirection?),
                typeof(GridViewSortBehavior));

        /// <summary>
        /// Attached property to indicate sort expression</summary>
        public static readonly DependencyProperty SortExpressionProperty =
            DependencyProperty.RegisterAttached(
                "SortExpression",
                typeof(string),
                typeof(GridViewSortBehavior));

        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static bool GetCanUserSortColumns(ListView element)
        {
            return (bool)element.GetValue(CanUserSortColumnsProperty);
        }

        /// <summary>
        /// Sets whether user can sort ListView columns</summary>
        /// <param name="element">ListView to set property for</param>
        /// <param name="value">Whether user can sort ListView columns</param>
        [AttachedPropertyBrowsableForType(typeof(ListView))]
        public static void SetCanUserSortColumns(ListView element, bool value)
        {
            element.SetValue(CanUserSortColumnsProperty, value);
        }

        /// <summary>
        /// Gets whether whether can sort GridViewColumn</summary>
        /// <param name="element">GridViewColumn to obtain property for</param>
        /// <returns>True iff can sort GridViewColumn</returns>
        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static bool GetCanUseSort(GridViewColumn element)
        {
            return (bool)element.GetValue(CanUseSortProperty);
        }

        /// <summary>
        /// Sets whether can sort GridViewColumn</summary>
        /// <param name="element">GridViewColumn to set property for</param>
        /// <param name="value">Whether can sort GridViewColumn</param>
        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static void SetCanUseSort(GridViewColumn element, bool value)
        {
            element.SetValue(CanUseSortProperty, value);
        }

        /// <summary>
        /// Gets GridViewColumn sort direction</summary>
        /// <param name="element">GridViewColumn to obtain property for</param>
        /// <returns>GridViewColumn sort direction or null</returns>
        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static ListSortDirection? GetSortDirection(GridViewColumn element)
        {
            return (ListSortDirection?)element.GetValue(SortDirectionProperty);
        }

        /// <summary>
        /// Sets GridViewColumn sort direction</summary>
        /// <param name="element">GridViewColumn to set property for</param>
        /// <param name="value">GridViewColumn sort direction or null</param>
        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static void SetSortDirection(GridViewColumn element, ListSortDirection? value)
        {
            element.SetValue(SortDirectionProperty, value);
        }

        /// <summary>
        /// Gets GridViewColumn sort expression</summary>
        /// <param name="element">GridViewColumn to obtain property for</param>
        /// <returns>GridViewColumn sort expression</returns>
        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static string GetSortExpression(GridViewColumn element)
        {
            return (string)element.GetValue(SortExpressionProperty);
        }

        /// <summary>
        /// Sets GridViewColumn sort expression</summary>
        /// <param name="element">GridViewColumn to set property for</param>
        /// <param name="value">GridViewColumn sort expression</param>
        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        public static void SetSortExpression(GridViewColumn element, string value)
        {
            element.SetValue(SortExpressionProperty, value);
        }

        private static void OnCanUserSortColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listView = (ListView)d;
            if ((bool)e.NewValue)
            {
                listView.AddHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)OnColumnHeaderClick);
                if (listView.IsLoaded)
                {
                    DoInitialSort(listView);
                }
                else
                {
                    listView.Loaded += OnLoaded;
                }
            }
            else
            {
                listView.RemoveHandler(GridViewColumnHeader.ClickEvent, (RoutedEventHandler)OnColumnHeaderClick);
            }
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var listView = (ListView)e.Source;
            listView.Loaded -= OnLoaded;
            DoInitialSort(listView);
        }

        private static void DoInitialSort(ListView listView)
        {
            var gridView = (GridView)listView.View;
            var column = gridView.Columns.FirstOrDefault(c => GetSortDirection(c) != null);
            if (column != null)
            {
                DoSort(listView, column);
            }
        }

        private static void OnColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            var columnHeader = e.OriginalSource as GridViewColumnHeader;
            if (columnHeader != null && columnHeader.Column != null && GetCanUseSort(columnHeader.Column))
            {
                DoSort((ListView)e.Source, columnHeader.Column);
            }
        }

        private static void DoSort(ListView listView, GridViewColumn newColumn)
        {
            var sortDescriptions = listView.Items.SortDescriptions;
            var newDirection = ListSortDirection.Ascending;

            var propertyPath = ResolveSortExpression(newColumn);
            if (propertyPath != null)
            {
                if (sortDescriptions.Count > 0)
                {
                    if (sortDescriptions[0].PropertyName == propertyPath)
                    {
                        newDirection = GetSortDirection(newColumn) == ListSortDirection.Ascending ?
                            ListSortDirection.Descending :
                            ListSortDirection.Ascending;
                    }
                    else
                    {
                        var gridView = (GridView)listView.View;
                        foreach (var column in gridView.Columns.Where(c => GetSortDirection(c) != null))
                        {
                            SetSortDirection(column, null);
                        }
                    }

                    sortDescriptions.Clear();
                }

                sortDescriptions.Add(new SortDescription(propertyPath, newDirection));
                SetSortDirection(newColumn, newDirection);
            }
        }

        private static string ResolveSortExpression(GridViewColumn column)
        {
            var propertyPath = GetSortExpression(column);
            if (propertyPath == null)
            {
                var binding = column.DisplayMemberBinding as Binding;
                return binding != null ? binding.Path.Path : null;
            }

            return propertyPath;
        }
    }
}
