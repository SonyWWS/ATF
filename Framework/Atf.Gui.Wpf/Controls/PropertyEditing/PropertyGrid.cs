//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Collections;
using Sce.Atf.Controls.PropertyEditing;

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
            DependencyProperty.Register("Instances", 
                typeof(IEnumerable), typeof(PropertyGrid), new FrameworkPropertyMetadata(InstancesPropertyChanged));

        private static void InstancesPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                var b = grid.GetBindingExpression(CustomPropertyDescriptorsProperty);
                if (b != null)
                    b.UpdateTarget();

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
            DependencyProperty.Register("CustomPropertyDescriptors", 
                typeof(IEnumerable<PropertyDescriptor>), typeof(PropertyGrid), new FrameworkPropertyMetadata(CustomPropertyDescriptorsPropertyChanged));

        private static void CustomPropertyDescriptorsPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                // Update instances too
                var b = grid.GetBindingExpression(InstancesProperty);
                b.UpdateTarget();

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
            DependencyProperty.Register("Properties", 
                typeof(IEnumerable<PropertyNode>), typeof(PropertyGrid), new FrameworkPropertyMetadata(PropertiesPropertyChanged));

        private static void PropertiesPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var grid = s as PropertyGrid;
            if (grid != null)
            {
                grid.SelectedProperty = null;
                grid.SetGrouping();
                grid.SetSorting();
            }
        }

        #endregion

        #region HeaderProperty Property

        /// <summary>
        /// Get or set grid header DependencyProperty value</summary>
        public PropertyNode HeaderProperty
        {
            get { return (PropertyNode)GetValue(HeaderPropertyProperty); }
            set { SetValue(HeaderPropertyProperty, value); }
        }

        /// <summary>
        /// Grid header DependencyProperty</summary>
        public static readonly DependencyProperty HeaderPropertyProperty =
            DependencyProperty.Register("HeaderProperty", typeof(PropertyNode), typeof(PropertyGrid), new UIPropertyMetadata(null));

        #endregion

        #region TransactionContext Property

        /// <summary>
        /// Transaction context DependencyProperty</summary>
        public static readonly DependencyProperty TransactionContextProperty =
           DependencyProperty.Register("TransactionContext", typeof(object), typeof(PropertyGrid), new PropertyMetadata(default(object), TransactionContextPropertyChanged));

        /// <summary>
        /// Get or set transaction context DependencyProperty value</summary>
        public object TransactionContext
        {
            get { return (object)GetValue(TransactionContextProperty); }
            set { SetValue(TransactionContextProperty, value); }
        }

        private static void TransactionContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
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
            DependencyProperty.Register("Sorting",
                typeof(IComparer), typeof(PropertyGrid), new FrameworkPropertyMetadata(SortingPropertyChanged));

        private static void SortingPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
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
            DependencyProperty.Register("Grouping", 
                typeof(GroupDescription), typeof(PropertyGrid), new FrameworkPropertyMetadata(GroupingPropertyChanged));

        private static void GroupingPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
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

        #region ListBoxItemsPanel Property

        /// <summary>
        /// Gets or sets PropertyGrid's ListBoxItemsPanel dependency property</summary>
        public ItemsPanelTemplate ListBoxItemsPanel
        {
            get { return (ItemsPanelTemplate)GetValue(ListBoxItemsPanelProperty); }
            set { SetValue(ListBoxItemsPanelProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's ListBoxItemsPanel dependency property</summary>
        public static readonly DependencyProperty ListBoxItemsPanelProperty =
            DependencyProperty.Register("ListBoxItemsPanel", typeof(ItemsPanelTemplate), typeof(PropertyGrid));

        #endregion

        #region ListBoxItemContainerStyle Property

        /// <summary>
        /// Gets or sets PropertyGrid's ListBoxItemContainerStyle dependency property</summary>
        public Style ListBoxItemContainerStyle
        {
            get { return (Style)GetValue(ListBoxItemContainerStyleProperty); }
            set { SetValue(ListBoxItemContainerStyleProperty, value); }
        }

        /// <summary>
        /// PropertyGrid's ListBoxItemContainerStyle dependency property</summary>
        public static readonly DependencyProperty ListBoxItemContainerStyleProperty =
            DependencyProperty.Register("ListBoxItemContainerStyle", typeof(Style), typeof(PropertyGrid));

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
        /// Get standard values editor template resource key used in XAML files</summary>
        public static ComponentResourceKey StandardValuesEditorTemplateKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "StandardValuesEditorTemplate"); }
        }

        /// <summary>
        /// Get Boolean editor style resource key used in XAML files</summary>
        public static ComponentResourceKey BoolEditorStyleKey
        {
            get { return new ComponentResourceKey(typeof(PropertyGrid), "BoolEditorStyle"); }
        }

        /// <summary>
        /// Get resource key used in XAML files for the Boolean editor template</summary>
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

        #region Persistent Settings

        /// <summary>
        /// Gets or sets the persistent state for the control</summary>
        public string Settings
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("PropertyView");
                //root.SetAttribute("PropertySorting", m_propertySorting.ToString());
                xmlDoc.AppendChild(root);

                foreach (var pair in m_categoryExpanded)
                {
                    XmlElement columnElement = xmlDoc.CreateElement("Category");
                    root.AppendChild(columnElement);

                    columnElement.SetAttribute("Name", pair.Key);
                    columnElement.SetAttribute("Expanded", pair.Value.ToString());
                }

                WriteSettings(root);

                return xmlDoc.InnerXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if (root == null || root.Name != "PropertyView")
                    throw new Exception("Invalid PropertyView settings");

                string s;
                //s = root.GetAttribute("PropertySorting");
                //if (s != null)
                //    m_propertySorting = (PropertySorting)Enum.Parse(typeof(PropertySorting), s);

                XmlNodeList columns = root.SelectNodes("Category");
                if (columns != null)
                {
                    foreach (XmlElement columnElement in columns)
                    {
                        string name = columnElement.GetAttribute("Name");
                        s = columnElement.GetAttribute("Expanded");
                        bool expanded;
                        if (s != null && bool.TryParse(s, out expanded))
                        {
                            m_categoryExpanded[name] = expanded;
                        }
                    }
                }

                ReadSettings(root);
            }
        }

        /// <summary>
        /// Reads persisted control setting information from the given XML element</summary>
        /// <param name="root">Root element of XML settings</param>
        protected virtual void ReadSettings(XmlElement root)
        {
        }

        /// <summary>
        /// Writes persisted control setting information to the given XML element</summary>
        /// <param name="root">Root element of XML settings</param>
        protected virtual void WriteSettings(XmlElement root)
        {
        }

        #endregion

        /// <summary>
        /// Get or set ObservableCollection of ValueEditors</summary>
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

            // Set up 50ms binding guard timer to prevent rapid updates from slowing
            // down the property grid
            m_bindingUpdateTimer = new DispatcherTimer();
            m_bindingUpdateTimer.Tick += BindingUpdateTimer_Tick;
            m_bindingUpdateTimer.Interval = TimeSpan.FromMilliseconds(50);

            Grouping = DefaultPropertyGrouping.ByCategory;
            Loaded += PropertyGrid_Loaded;
            Unloaded += PropertyGrid_Unloaded;
        }

        /// <summary>
        /// Method called whenever ApplyTemplate, which builds the current template's visual tree, is called</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            DependencyObject part = GetTemplateChild("PART_Selector");
            var view = part as TreeView;
            if (view != null)
            {
                view.SelectedItemChanged += view_SelectedItemChanged;
            }
            else
            {
                var selector = part as Selector;
                if (selector != null)
                {
                    selector.SelectionChanged += selector_SelectionChanged;
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

                if (HeaderProperty != null)
                    HeaderProperty.Refresh();
            }
        }

        private void DestroyPropertyNode(PropertyNode node)
        {
            node.ValueSet -= node_ValueSet;
            node.ValueError -= node_ValueError;
            node.Dispose();
        }

        /// <summary>
        /// Rebuild property nodes implementation function</summary>
        protected virtual void RebuildPropertyNodesImpl()
        {
            DestroyPropertyNodes();

            object[] instances = Instances == null ? EmptyArray<object>.Instance
                                     : Instances.Cast<object>().ToArray();

            // TODO: cache and reuse PropertyNodes where possible to prevent having to 
            // rebuild all of the data templates
            IEnumerable<PropertyDescriptor> descriptors = null;

            if (CustomPropertyDescriptors != null)
            {
                descriptors = CustomPropertyDescriptors;
            }
            else if (Instances != null)
            {
                descriptors = PropertyUtils.GetSharedPropertiesOriginal(instances);
            }

            PropertyNode headerPropertyNode = null;
            var propertyNodes = new ObservableCollection<PropertyNode>();

            if (descriptors != null)
            {
                var context = TransactionContext.As<ITransactionContext>();
                if (context == null)
                    context = DataContext.As<ITransactionContext>();

                foreach (var descriptor in descriptors)
                {
                    if (descriptor.IsBrowsable)
                    {
                        PropertyNode node;
                        if (PropertyFactory != null)
                        {
                            node = PropertyFactory.CreateProperty(instances, descriptor, true, context);
                        }
                        else
                        {
                            node = new PropertyNode();
                            node.Initialize(instances, descriptor, true);
                        }

                        node.ValueSet += node_ValueSet;
                        node.ValueError += node_ValueError;

                        if (node.Category != null)
                        {
                            bool expansionState;
                            if (m_categoryExpanded.TryGetValue(node.Category, out expansionState))
                                node.IsExpanded = expansionState;
                        }

                        if (headerPropertyNode == null && descriptor.Attributes[typeof(HeaderPropertyAttribute)] != null)
                            headerPropertyNode = node;
                        else
                            propertyNodes.Add(node);
                    }
                }
            }

            // Listen for expansion state changes so that we can persist through different objects.
            m_listener = ChangeListener.Create(propertyNodes, "IsExpanded");
            m_listener.PropertyChanged += ChildExpandedPropertyChanged;

            Properties = propertyNodes;
            HeaderProperty = headerPropertyNode;
        }

        /// <summary>
        /// Destroy all property nodes</summary>
        protected virtual void DestroyPropertyNodes()
        {
            if (m_listener != null)
            {
                m_listener.PropertyChanged -= ChildExpandedPropertyChanged;
                m_listener.Dispose();
                m_listener = null;
            }

            // Destroy and unsubscribe from old properties
            if (Properties != null)
            {
                foreach (PropertyNode node in Properties)
                    DestroyPropertyNode(node);

                Properties = null;
            }

            if (HeaderProperty != null)
            {
                DestroyPropertyNode(HeaderProperty);
                HeaderProperty = null;
            }
        }

        private void RebuildPropertyNodes()
        {
            if (IsLoaded)
            {
                // Guard updates with a short timer for better performance
                m_bindingUpdateTimer.Stop();
                m_bindingUpdateTimer.Start();
            }
        }

        void ChildExpandedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var node = sender as PropertyNode;
            if (node != null && !string.IsNullOrEmpty(node.Category))
            {
                m_categoryExpanded[node.Category] = node.IsExpanded;
            }
        }

        private void BindingUpdateTimer_Tick(object sender, EventArgs e)
        {
            m_bindingUpdateTimer.Stop();
            RebuildPropertyNodesImpl();
        }

        private void PropertyGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RebuildPropertyNodes();
        }

        private void PropertyGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            DestroyPropertyNodes();
            SelectedProperty = null;
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
            var node = ((Selector)sender).SelectedItem as PropertyNode;
            if (node != SelectedProperty)
                SelectedProperty = node;
        }

        private void view_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = e.NewValue as PropertyNode;
            if (node != SelectedProperty)
                SelectedProperty = node;
        }

        private void InstancesOrPropertiesChanged(IEnumerable oldInstances)
        {
            var oldCollection = oldInstances as INotifyCollectionChanged;
            if (oldCollection != null)
                oldCollection.CollectionChanged -= Instances_CollectionChanged;

            var collection = Instances as INotifyCollectionChanged;
            if (collection != null)
                collection.CollectionChanged += Instances_CollectionChanged;

            RebuildPropertyNodes();
        }

        private void Instances_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RebuildPropertyNodes();
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
                            node.ValueSet -= node_ValueSet;
                            node.ValueError -= node_ValueError;
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
        private ChangeListener m_listener;
        private readonly DispatcherTimer m_bindingUpdateTimer;
        private readonly Dictionary<string, bool> m_categoryExpanded = new Dictionary<string, bool>();

        #endregion
    }
}
