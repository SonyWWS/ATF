//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections;
using Sce.Atf.Controls.PropertyEditing;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Grid of PropertyNodes</summary>
    public class PropertyGrid : Control, IDisposable
    {
        #region Instances Property

        /// <summary>
        /// Gets or sets PropertyGrid's Instances dependency property</summary>
        public IEnumerable Instances
        {
            get { return (IEnumerable)GetValue(InstancesProperty); }
            set { SetValue(InstancesProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's Instances dependency property</summary>
        public static readonly DependencyProperty InstancesProperty =
            DependencyProperty.Register("Instances", typeof(IEnumerable), typeof(PropertyGrid), new FrameworkPropertyMetadata(new PropertyChangedCallback(InstancesProperty_Changed)));

        private static void InstancesProperty_Changed(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                grid.InstancesOrPropertiesChanged(e.OldValue as IEnumerable);
            }
        }

        #endregion

        #region CustomPropertyDescriptors Property

        /// <summary>
        /// Gets or sets PropertyGrid's CustomPropertyDescriptors dependency property</summary>
        public IEnumerable<PropertyDescriptor> CustomPropertyDescriptors
        {
            get { return (IEnumerable<PropertyDescriptor>)GetValue(CustomPropertyDescriptorsProperty); }
            set { SetValue(CustomPropertyDescriptorsProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's CustomPropertyDescriptors dependency property</summary>
        public static readonly DependencyProperty CustomPropertyDescriptorsProperty =
            DependencyProperty.Register("CustomPropertyDescriptors", typeof(IEnumerable<PropertyDescriptor>), typeof(PropertyGrid), new FrameworkPropertyMetadata(new PropertyChangedCallback(CustomPropertyDescriptorsProperty_Changed)));

        private static void CustomPropertyDescriptorsProperty_Changed(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                grid.InstancesOrPropertiesChanged(e.OldValue as IEnumerable);
            }
        }

        #endregion

        #region EditorTemplateSelector Property

        /// <summary>
        /// Gets or sets PropertyGrid's EditorTemplateSelector dependency property</summary>
        public DataTemplateSelector EditorTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(EditorTemplateSelectorProperty); }
            set { SetValue(EditorTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's EditorTemplateSelector dependency property</summary>
        public static readonly DependencyProperty EditorTemplateSelectorProperty =
            DependencyProperty.Register("EditorTemplateSelector", typeof(DataTemplateSelector), typeof(PropertyGrid), new UIPropertyMetadata(null));

        #endregion

        #region Properties Property

        /// <summary>
        /// Gets or sets PropertyGrid's Properties dependency property</summary>
        public IEnumerable<PropertyNode> Properties
        {
            get { return (IEnumerable<PropertyNode>)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's Properties dependency property</summary>
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(IEnumerable<PropertyNode>), typeof(PropertyGrid), new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertiesProperty_Changed)));

        private static void PropertiesProperty_Changed(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                grid.SetGrouping();
                grid.SetSorting();
            }
        }

        #endregion

        #region SelectedProperty Property

        /// <summary>
        /// Gets or sets PropertyGrid's SelectedProperty dependency property</summary>
        public PropertyNode SelectedProperty
        {
            get { return (PropertyNode)GetValue(SelectedPropertyProperty); }
            set { SetValue(SelectedPropertyProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's SelectedProperty dependency property</summary>
        public static readonly DependencyProperty SelectedPropertyProperty =
            DependencyProperty.Register("SelectedProperty", typeof(PropertyNode), typeof(PropertyGrid), new UIPropertyMetadata(null));

        #endregion

        #region Sorting Property

        /// <summary>
        /// Gets or sets PropertyGrid's Sorting dependency property</summary>
        public IComparer Sorting
        {
            get { return (IComparer)GetValue(SortingProperty); }
            set { SetValue(SortingProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's Sorting dependency property</summary>
        public static readonly DependencyProperty SortingProperty =
            DependencyProperty.Register("Sorting", typeof(IComparer), typeof(PropertyGrid), new FrameworkPropertyMetadata(new PropertyChangedCallback(SortingProperty_Changed)));

        private static void SortingProperty_Changed(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                grid.SetSorting();
            }
        }

        private void SetSorting()
        {
            var view = CollectionViewSource.GetDefaultView(Properties) as ListCollectionView;
            if (view != null)
                view.CustomSort = Sorting;
        }

        #endregion

        #region Grouping Property

        /// <summary>
        /// Gets or sets PropertyGrid's Grouping dependency property</summary>
        public GroupDescription Grouping
        {
            get { return (GroupDescription)GetValue(GroupingProperty); }
            set { SetValue(GroupingProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's Grouping dependency property</summary>
        public static readonly DependencyProperty GroupingProperty =
            DependencyProperty.Register("Grouping", typeof(GroupDescription), typeof(PropertyGrid), new FrameworkPropertyMetadata(new PropertyChangedCallback(GroupingProperty_Changed)));

        private static void GroupingProperty_Changed(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                grid.SetGrouping();
            }
        }

        private void SetGrouping()
        {
            if (Properties != null)
            {
                ICollectionView view = CollectionViewSource.GetDefaultView(Properties);
                view.GroupDescriptions.Clear();

                if (Grouping != null)
                    view.GroupDescriptions.Add(Grouping);
            }
        }

        #endregion

        #region ToolBarStyle Property

        /// <summary>
        /// Gets or sets PropertyGrid's ToolBarStyle dependency property</summary>
        public Style ToolBarStyle
        {
            get { return (Style)GetValue(ToolBarStyleProperty); }
            set { SetValue(ToolBarStyleProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's ToolBarStyle dependency property</summary>
        public static readonly DependencyProperty ToolBarStyleProperty =
            DependencyProperty.Register("ToolBarStyle", typeof(Style), typeof(PropertyGrid));

        #endregion

        #region PropertyDetailsVisibility Property

        /// <summary>
        /// Gets or sets PropertyGrid's PropertyDetailsVisibility dependency property</summary>
        public Visibility PropertyDetailsVisibility
        {
            get { return (Visibility)GetValue(PropertyDetailsVisibilityProperty); }
            set { SetValue(PropertyDetailsVisibilityProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's PropertyDetailsVisibility dependency property</summary>
        public static readonly DependencyProperty PropertyDetailsVisibilityProperty =
            DependencyProperty.Register("PropertyDetailsVisibility", typeof(Visibility), typeof(PropertyGrid), new UIPropertyMetadata(Visibility.Visible));
        #endregion

        #region LabelWidth

        /// <summary>
        /// Gets or sets PropertyGrid's LabelWidth dependency property</summary>
        public double LabelWidth
        {
            get { return (double)GetValue(LabelWidthProperty); }
            set { SetValue(LabelWidthProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's LabelWidth dependency property</summary>
        public static readonly DependencyProperty LabelWidthProperty =
            DependencyProperty.Register("LabelWidth", typeof(double), typeof(PropertyGrid), new UIPropertyMetadata(100D));
        
        #endregion

        #region PropertyFactory

        /// <summary>
        /// Gets or sets PropertyGrid's PropertyFactory dependency property</summary>
        public IPropertyFactory PropertyFactory
        {
            get { return (IPropertyFactory)GetValue(PropertyFactoryProperty); }
            set { SetValue(PropertyFactoryProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's PropertyFactory dependency property</summary>
        public static readonly DependencyProperty PropertyFactoryProperty =
            DependencyProperty.Register("PropertyFactory", typeof(IPropertyFactory), typeof(PropertyGrid), new UIPropertyMetadata(null));

        #endregion

        #region Resource Keys

        /// <summary>
        /// Resource key used in XAML files for the property name template</summary>
        public static ComponentResourceKey PropertyNameTemplateKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "PropertyNameTemplate"); }
        }

        /// <summary>
        /// Resource key used in XAML files for read only style</summary>
        public static ComponentResourceKey ReadOnlyStyleKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "ReadOnlyStyle"); }
        }

        /// <summary>
        /// Resource key used in XAML files for the read only template</summary>
        public static ComponentResourceKey ReadOnlyTemplateKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "ReadOnlyTemplate"); }
        }

        /// <summary>
        /// Resource key used in XAML files for the default text editor style</summary>
        public static ComponentResourceKey DefaultTextEditorStyleKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "DefaultTextEditorStyle"); }
        }

        /// <summary>
        /// Resource key used in XAML files for the xxxxx</summary>
        public static ComponentResourceKey DefaultTextEditorTemplateKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "DefaultTextEditorTemplate"); }
        }

        /// <summary>
        /// Resource key used in XAML files for the combo editor style</summary>
        public static ComponentResourceKey ComboEditorStyleKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "ComboEditorStyle"); }
        }

        /// <summary>
        /// Resource key used in XAML files for the combo editor template</summary>
        public static ComponentResourceKey ComboEditorTemplateKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "ComboEditorTemplate"); }
        }

        /// <summary>
        /// Resource key used in XAML files for the bool editor style</summary>
        public static ComponentResourceKey BoolEditorStyleKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "BoolEditorStyle"); }
        }

        /// <summary>
        /// Resource key used in XAML files for the bool editor template</summary>
        public static ComponentResourceKey BoolEditorTemplateKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "BoolEditorTemplate"); }
        }

        #endregion

        #region Events

        /// <summary>
        /// Event that is raised when a property error occurs</summary>
        public event EventHandler<PropertyErrorEventArgs> PropertyError;
        
        /// <summary>
        /// Event that is raised when a property value is edited</summary>
        public event EventHandler<PropertyEditedEventArgs> PropertyEdited;

        /// <summary>
        /// Raises the PropertyError event and performs custom processing</summary>
        /// <param name="e">PropertyErrorEventArgs containing event data</param>
        protected virtual void OnPropertyError(PropertyErrorEventArgs e)
        {
            PropertyError.Raise<PropertyErrorEventArgs>(this, e);
        }

        /// <summary>
        /// Raises the PropertyEdited event and performs custom processing</summary>
        /// <param name="e">PropertyErrorEventArgs containing event data</param>
        protected virtual void OnPropertyEdited(PropertyEditedEventArgs e)
        {
            PropertyEdited.Raise<PropertyEditedEventArgs>(this, e);
        }

        #endregion

        /// <summary>
        /// Gets or sets PropertyGrid's collection of value editors</summary>
        public ObservableCollection<ValueEditor> Editors { get; set; }

        /// <summary>
        /// Static constructor</summary>
        static PropertyGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
        }

        /// <summary>
        /// Constructor</summary>
        public PropertyGrid()
        {
            Editors = new ObservableCollection<ValueEditor>();
            EditorTemplateSelector = new EditorTemplateSelector(Editors);
            Grouping = DefaultPropertyGrouping.ByCategory;
        }

        /// <summary>
        /// Method called whenever ApplyTemplate, which builds the current template's visual tree, is called</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DependencyObject part = GetTemplateChild("PART_Selector");
            TreeView view = part as TreeView;
            if (view != null)
            {
                view.SelectedItemChanged += new RoutedPropertyChangedEventHandler<object>(view_SelectedItemChanged);
            }
            else
            {
                Selector selector = part as Selector;
                if (selector != null)
                {
                    selector.SelectionChanged += new SelectionChangedEventHandler(selector_SelectionChanged);
                }
            }
        }

        /// <summary>
        /// Refreshes all PropertyGrid's PropertyNode's property values, notifying listeners as if they had changed</summary>
        public void Refresh()
        {
            if (Properties != null)
            {
                foreach (PropertyNode node in Properties)
                {
                    node.Refresh();
                }
            }
        }

        /// <summary>
        /// Reloads all PropertyNodes in PropertyGrid, recreating all PropertyNodes</summary>
        protected virtual void Reload()
        {
            // Destroy and unsubscribe from old properties
            if (Properties != null)
            {
                foreach (PropertyNode node in Properties)
                {
                    node.UnBind();
                    node.ValueSet -= new EventHandler(node_ValueSet);
                    node.ValueError -= new EventHandler(node_ValueError);
                }
            }

            object[] instances = Instances.Cast<object>().ToArray();

            // TODO: cache and reuse PropertyNodes where possible to prevent having to 
            // rebuild all of the data templates
            IEnumerable<PropertyDescriptor> descriptors = null;

            if (CustomPropertyDescriptors != null)
            {
                descriptors = CustomPropertyDescriptors;
            }
            else if (Instances != null)
            {
                descriptors = PropertyUtils.GetSharedProperties(instances);
            }

            var propertyNodes = new ObservableCollection<PropertyNode>();

            foreach (var descriptor in descriptors)
            {
                if (descriptor.IsBrowsable)
                {
                    PropertyNode node = null;
                    if (PropertyFactory != null)
                        node = PropertyFactory.CreateProperty(instances, descriptor, true, this);
                    else
                        node = new PropertyNode(instances, descriptor, true, this);

                    node.ValueSet += new EventHandler(node_ValueSet);
                    node.ValueError += new EventHandler(node_ValueError);
                    propertyNodes.Add(node);
                }
            }

            Properties = propertyNodes;
        }

        private void node_ValueSet(object sender, EventArgs e)
        {
            var node = sender as PropertyNode;
            OnPropertyEdited(new PropertyEditedEventArgs(node.Instance, node.Descriptor, node.OldValue, node.Value));
        }

        private void node_ValueError(object sender, EventArgs e)
        {
            var node = sender as PropertyNode;
            OnPropertyError(new PropertyErrorEventArgs(node.Instance, node.Descriptor, node.PropertyValueError));
        }

        private void selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedProperty = ((Selector)sender).SelectedItem as PropertyNode;
        }

        private void view_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedProperty = e.NewValue as PropertyNode;
        }

        private void InstancesOrPropertiesChanged(IEnumerable oldInstances)
        {
            var oldCollection = oldInstances as INotifyCollectionChanged;
            if (oldCollection != null)
                oldCollection.CollectionChanged -= Instances_CollectionChanged;

            var collection = Instances as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += Instances_CollectionChanged;

            Reload();
        }

        private void Instances_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Reload();
        }

        #region IDisposable Members

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    var properties = Properties;
                    if (properties != null)
                    {
                        foreach (PropertyNode node in properties)
                        {
                            node.UnBind();
                            node.ValueSet -= new EventHandler(node_ValueSet);
                            node.ValueError -= new EventHandler(node_ValueError);
                        }
                    }
                }

                m_disposed = true;
            }
        }

        /// <summary>
        /// Disposes of resources</summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private bool m_disposed;

        #endregion
    }
}
