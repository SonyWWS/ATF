//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// PropertyGrid ToolBar</summary>
    public class PropertyGridToolBar : Control
    {
        /// <summary>
        /// ShowCategorized ICommand routed through element tree</summary>
        public static readonly ICommand ShowCategorized = new RoutedCommand("ShowCategorized", typeof(PropertyGridToolBar));

        /// <summary>
        /// ShowAlphaSorted ICommand routed through element tree</summary>
        public static readonly ICommand ShowAlphaSorted = new RoutedCommand("ShowAlphaSorted", typeof(PropertyGridToolBar));

        #region Properties Property

        public bool IsCategorized
        {
            get { return (bool)GetValue(IsCategorizedProperty); }
            set { SetValue(IsCategorizedProperty, value); }
        }

        /// <summary>
        /// PropertyGridToolBar's Properties dependency property</summary>
        public static readonly DependencyProperty IsCategorizedProperty =
            DependencyProperty.Register("IsCategorized", typeof(bool), typeof(PropertyGridToolBar));

        /// <summary>
        /// Gets or sets PropertyGridToolBar's Properties dependency property</summary>
        public IEnumerable Properties
        {
            get { return (IEnumerable)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        /// <summary>
        /// PropertyGridToolBar's Properties dependency property</summary>
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(IEnumerable), typeof(PropertyGridToolBar),
            new FrameworkPropertyMetadata(PropertiesProperty_Changed));

        private static void PropertiesProperty_Changed(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var tb = o as PropertyGridToolBar;
            if (tb != null && tb.Properties != null)
            {
                if (tb.IsCategorized)
                {
                    if (tb.GetCollectionView().CanSort)
                    {
                        tb.ExecuteSort(null, null);
                    }
                }
                else if (tb.GetCollectionView().CanGroup)
                {
                    tb.ExecuteGroup(null, null);
                }
            }
        }

        #endregion

        /// <summary>
        /// Static constructor</summary>
        static PropertyGridToolBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGridToolBar), new FrameworkPropertyMetadata(typeof(PropertyGridToolBar)));
        }

        /// <summary>
        /// Constructor</summary>
        public PropertyGridToolBar()
        {
            CommandBindings.Add(new CommandBinding(ShowAlphaSorted, ExecuteSort, CanExecuteSort));
            CommandBindings.Add(new CommandBinding(ShowCategorized, ExecuteGroup, CanExecuteGroup));
        }

        private void CanExecuteSort(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Properties != null && GetCollectionView().CanSort;
        }

        private void ExecuteSort(object sender, ExecutedRoutedEventArgs e)
        {
            Clear();
            var cv = GetCollectionView();
            cv.SortDescriptions.Add(s_alphaSort);
            IsCategorized = true;
        }

        private void CanExecuteGroup(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Properties != null && GetCollectionView().CanGroup;
        }

        private void ExecuteGroup(object sender, ExecutedRoutedEventArgs e)
        {
            Clear();
            var cv = GetCollectionView();
            cv.GroupDescriptions.Add(DefaultPropertyGrouping.ByCategory);
            IsCategorized = false;
        }

        private void Clear()
        {
            if (Properties != null)
            {
                var cv = GetCollectionView();
                cv.SortDescriptions.Clear();
                cv.GroupDescriptions.Clear();
            }
        }

        private ICollectionView GetCollectionView()
        {
            return CollectionViewSource.GetDefaultView(Properties);
        }

        private static SortDescription s_alphaSort = new SortDescription("Descriptor.DisplayName", ListSortDirection.Ascending);
    }
}
