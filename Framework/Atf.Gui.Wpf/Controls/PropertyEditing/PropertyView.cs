//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Base class for complex property editing controls, providing formats, fonts,
    /// data binding, persistent settings, and category/property information</summary>
    public abstract class PropertyView : UserControl, IPropertyEditingControlOwner
    {
        /// <summary>
        /// Constructor</summary>
        public PropertyView()
        {
            base.AllowDrop = true; // otherwise, embedded child controls can't accept drops

            FilterPattern = "";
        }

        /// <summary>
        /// Indicates whether sub-categories are enabled. The default is true. If set
        /// to true, the names of the categories are parsed, looking for '\' as
        /// a separator between a parent category and child category.</summary>
        public static bool EnableSubCategories = true;

        #region Data Binding

        #region IPropertyEditingControlOwner implementation
        
        /// <summary>
        /// Gets the list of selected objects for the current editing context</summary>
        public object[] SelectedObjects
        {
            get { return m_selectedObjects; }
        }

        #endregion

        /// <summary>
        /// Gets the last selected object for the current editing context, or null if none</summary>
        public object LastSelectedObject
        {
            get { return m_selectedObjects.Length > 0 ? m_selectedObjects[m_selectedObjects.Length - 1] : null; }
        }

        /// <summary>
        /// Event that is raised after the editing context changes</summary>
        public event EventHandler EditingContextChanged;

        /// <summary>
        /// Gets or sets the current property editing context</summary>
        public IPropertyEditingContext EditingContext
        {
            get { return m_editingContext; }
            set
            {
                if (value != m_editingContext)
                {
                    if (m_editingContext != null)
                    {
                        IObservableContext observableContext = m_editingContext.As<IObservableContext>();
                        if (observableContext != null)
                        {
                            observableContext.ItemInserted -= observableContext_ItemInserted;
                            observableContext.ItemRemoved -= observableContext_ItemRemoved;
                            observableContext.ItemChanged -= observableContext_ItemChanged;
                            observableContext.Reloaded -= observableContext_Reloaded;
                        }

                        ISubSelectionContext selectionContext = m_editingContext.As<ISubSelectionContext>();
                        ISelectionContext subSelectionContext = (selectionContext != null) ? selectionContext.SubSelectionContext : null;
                        if (subSelectionContext != null)
                            subSelectionContext.SelectionChanged -= subSelectionContext_SelectionChanged;
                    }

                    OnEditingContextChanging();

                    m_editingContext = value;
                    m_selectedObjects = EmptyArray<object>.Instance;
                    m_propertyDescriptors = EmptyArray<PropertyDescriptor>.Instance;

                    if (m_editingContext != null)
                    {
                        m_selectedObjects = m_editingContext.Items.ToArray();
                        m_propertyDescriptors = m_editingContext.PropertyDescriptors.ToArray();

                        IObservableContext observableContext = m_editingContext.As<IObservableContext>();
                        if (observableContext != null)
                        {
                            observableContext.ItemInserted += observableContext_ItemInserted;
                            observableContext.ItemRemoved += observableContext_ItemRemoved;
                            observableContext.ItemChanged += observableContext_ItemChanged;
                            observableContext.Reloaded += observableContext_Reloaded;
                        }

                        ISubSelectionContext selectionContext = m_editingContext.As<ISubSelectionContext>();
                        ISelectionContext subSelectionContext = (selectionContext != null) ? selectionContext.SubSelectionContext : null;
                        if (subSelectionContext != null)
                            subSelectionContext.SelectionChanged += subSelectionContext_SelectionChanged;
                    }

                    UpdateEditingContext();

                    OnEditingContextChanged();

                    EditingContextChanged.Raise(this, EventArgs.Empty);                    
                }
            }
        }

        /// <summary>
        /// Gets or sets the ContextRegistry, which is passed to
        /// custom controls through the PropertyEditorControlContext</summary>
        /// <remarks>The editor containing this control should set this property, if possible.</remarks>
        public IContextRegistry ContextRegistry { get; set; }

        /// <summary>
        /// Handles the Reloaded event of the observableContext, and updates the EditingContext with
        /// the new info.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void observableContext_Reloaded(object sender, EventArgs e)
        {
            // context was reloaded; check if objects or descriptors have changed
            IPropertyEditingContext context = m_editingContext;
            if (context != null)
            {
                if (!m_selectedObjects.SequenceEqual(m_editingContext.Items) ||
                    !m_propertyDescriptors.SequenceEqual(m_editingContext.PropertyDescriptors))
                {
                    EditingContext = null;
                    EditingContext = context;
                }
                else
                {
                    InvalidateVisual();
                    Dispatcher.InvokeIfRequired(() => { RefreshEditingControls(); });
                }

                OnSubSelectionChanged();
            }
        }

        private void subSelectionContext_SelectionChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
            RefreshEditingControls();
            OnSubSelectionChanged();
        }

        private void UpdateEditingContext()
        {
            using (var d = this.Dispatcher.DisableProcessing())
            {
                // make sure this call is from the same thread that created this control,
                //  which should be the GUI thread.
                this.Dispatcher.InvokeIfRequired(() =>
                {

                    SelectedProperty = null;
                    ClearCurrentProperties();

                    if (Visibility == System.Windows.Visibility.Visible &&
                        m_editingContext != null)
                    {
                        BuildProperties();
                    }

                    UpdatePropertySorting();

                    // some property control need to be refreshed
                    // when assigning new editing-context.
                    RefreshEditingControls();
                    InvalidateVisual();

                });
            }
        }

        private void observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            UpdateEditingContext();
        }

        private void observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            UpdateEditingContext();
        }

        private void OnSubSelectionChanged()
        {
            ISubSelectionContext selectionContext = m_editingContext.As<ISubSelectionContext>();
            ISelectionContext subSelectionContext = (selectionContext != null) ? selectionContext.SubSelectionContext : null;
            if (subSelectionContext != null && subSelectionContext.SelectionCount > 0)
            {
                Object[] objectArray = subSelectionContext.Selection.ToArray();
                PropertyDescriptor descriptor = objectArray[0].As<PropertyDescriptor>();
                foreach (Property property in m_activeProperties)
                {
                    if (PropertyUtils.PropertyDescriptorsEqual(property.Descriptor, descriptor))
                        SelectedProperty = property;
                }
            }
            // We don't want to set the selected property to null here because OnSubSelectionChanged()
            //  gets called every time a property is edited and setting SelectedProperty to null
            //  breaks the Tab and arrow key navigation feature.
            //else
                //SelectedProperty = null;
        }

        private void observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            InvalidateVisual();
            RefreshEditingControls();
        }

        /// <summary>
        /// Performs custom actions before editing context has changed</summary>
        protected virtual void OnEditingContextChanging()
        {
        }

        /// <summary>
        /// Performs custom actions after editing context has changed</summary>
        protected virtual void OnEditingContextChanged()
        {
        }

        /// <summary>
        /// Performs custom actions after properties are created</summary>
        protected virtual void OnCreateProperties()
        {
        }

        #endregion

        private string m_filterPattern;
        /// <summary>
        /// Gets or sets the filter pattern</summary>
        public string FilterPattern
        {
            get { return m_filterPattern; }
            set
            {
                m_filterPattern = value;
                UpdateEditingContext();
            }
        }

        #region Reset Current and All Properties

        /// <summary>
        /// Gets whether the current property edit can be reset</summary>
        public bool CanResetCurrent
        {
            get
            {
                return
                    PropertyUtils.CanResetProperty(SelectedObjects, SelectedPropertyDescriptor);
            }
        }

        /// <summary>
        /// Resets the current property being edited to its default value</summary>
        public void ResetCurrent()
        {
            if (CanResetCurrent)
            {
                PropertyUtils.ResetProperty(SelectedObjects, SelectedPropertyDescriptor);
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Resets all properties to their default values</summary>
        public void ResetAll()
        {
            if (m_editingContext != null)
            {
                //CancelEditingControl();

                foreach (Property p in Properties)
                {
                    if (PropertyUtils.CanResetProperty(SelectedObjects, p.Descriptor))
                        PropertyUtils.ResetProperty(SelectedObjects, p.Descriptor);
                }

                if (!EditingContext.Is<IObservableContext>())
                    RefreshEditingControls();
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Refreshes all property representations by invalidating these controls and immediately
        /// redrawing them</summary>
        public void RefreshProperties()
        {
            InvalidateVisual();
        }

        /// <summary>
        /// Refreshes all editing controls, invalidating them and immediately redrawing them</summary>
        protected virtual void RefreshEditingControls()
        {
            foreach (Property p in Properties)
            {
                Control control = p.Control;
                if (control != null)
                {
                    control.InvalidateVisual();
                }
            }
        }

        #endregion

        #region Selected Property

        /// <summary>
        /// Event that is raised after the selected property changes</summary>
        public event EventHandler SelectedPropertyChanged;

        /// <summary>
        /// Gets a PropertyDescriptor for the currently selected property</summary>
        public PropertyDescriptor SelectedPropertyDescriptor
        {
            get
            {
                return (m_selectedProperty != null) ? m_selectedProperty.Descriptor : null;
            }
        }

        /// <summary>
        /// Gets or sets the currently selected property</summary>
        protected Property SelectedProperty
        {
            get { return m_selectedProperty; }
            set
            {
                if (m_selectedProperty != value)
                {
                    m_selectedProperty = value;
                    OnSelectedPropertyChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Raises the SelectedPropertyChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnSelectedPropertyChanged(EventArgs e)
        {
            EventHandler handler = SelectedPropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Clears the selected property</summary>
        public void ClearSelectedProperty()
        {
            if (SelectedProperty != null)
            {
                SelectedProperty = null; // Using property instead of member to trigger event
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Sets a custom sort order for the properties</summary>
        /// <param name="customSortOrder">A list of property names in the desired sort order</param>
        public virtual void SetCustomPropertySortOrder(List<string> customSortOrder)
        {
            PropertySorting = PropertySorting.Custom;
            m_customSortOrder = customSortOrder;

            // update with new sort order
            UpdateEditingContext();
        }

        /// <summary>
        /// Sorts the properties in the order they appear in the given list</summary>
        /// <param name="propertyNames">A list of property names use to define the sort order</param>
        protected void SortPropertiesFromPropertyNamesList(List<string> propertyNames)
        {
            // Make a temp list of all the properties
            List<Property> workingList = new List<Property>();

            // add all of the properties to the working list
            foreach (Property p in m_activeProperties)
            {
                workingList.Add(p);
            }

            // removed them from the real list
            foreach (Property p in workingList)
            {
                RemoveProperty(p);
            }

            // add each property back to the real list in the order they appear in the incoming list
            foreach (string porpertyName in propertyNames)
            {
                // try to find it in the working list
                Property found = null;
                foreach (Property p in workingList)
                {
                    if (p.Descriptor.Name.Equals(porpertyName))
                    {
                        // add the property back to the real list
                        found = p;
                        AddProperty(p);
                        break;
                    }
                }

                // remove it from the working list
                if (found != null)
                {
                    workingList.Remove(found);
                }
            }

            // if there are properties left in the working list, just add them to the end of the list
            foreach (Property p in workingList)
            {
                AddProperty(p);
            }
        }

        /// <summary>
        /// Selected property</summary>
        protected Property m_selectedProperty;

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
                root.SetAttribute("PropertySorting", m_propertySorting.ToString());
                xmlDoc.AppendChild(root);

                foreach (KeyValuePair<string, bool> pair in m_categoryExpanded)
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

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if (root == null || root.Name != "PropertyView")
                    throw new Exception("Invalid PropertyView settings");

                string s;
                s = root.GetAttribute("PropertySorting");
                m_propertySorting = (PropertySorting)Enum.Parse(typeof(PropertySorting), s);

                XmlNodeList columns = root.SelectNodes("Category");
                foreach (XmlElement columnElement in columns)
                {
                    string name = columnElement.GetAttribute("Name");
                    s = columnElement.GetAttribute("Width");
                    bool expanded;
                    if (bool.TryParse(s, out expanded))
                    {
                        m_categoryExpanded[name] = expanded;
                    }
                }

                ReadSettings(root);
                InvalidateVisual();
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

        #region Categories, Properties, and PropertySorting

        /// <summary>
        /// Gets or sets the control's property display sort order</summary>
        public PropertySorting PropertySorting
        {
            get { return m_propertySorting; }
            set
            {
                if (m_propertySorting != value)
                {
                    m_propertySorting = value;

                    UpdatePropertySorting();
                    OnPropertySortingChanged();
                }
            }
        }

        /// <summary>
        /// Performs custom actions after property sort order has changed</summary>
        /// <remarks>When overriding, call the base method to force a re-layout</remarks>
        protected virtual void OnPropertySortingChanged()
        {
            InvalidateVisual();
        }

        private void BuildProperties()
        {
            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>(m_editingContext.PropertyDescriptors);

            // Try to re-use old properties. Construct new ones as necessary.
            //  Fill in set of active properties.
            m_activeProperties.Clear();
            m_processedDescriptors.Clear();
            int index = 0;
            for (int i = 0; i < descriptors.Count; i++)
            {
                PropertyDescriptor descriptor = descriptors[i];
                if (!descriptor.IsBrowsable) 
                    continue;
                if (FilterPattern.Length == 0 || descriptor.Name.ToLower().Contains(FilterPattern.ToLower()))
                {
                    Property property = BuildProperty(descriptor, index++);

                    bool descriptorReflected = descriptor.GetType().Name == "ReflectPropertyDescriptor";
                    AddChildProperty(property, descriptorReflected, ref index);
                }
            }

            // Mark old controls as no longer being visible.
            foreach (KeyValuePair<PropertyDescriptor, Property> oldPair in m_cacheableProperties)
            {
                Control control = oldPair.Value.Control;
                if (control != null &&
                    !m_activeProperties.Contains(oldPair.Value))
                {
                    control.Visibility = System.Windows.Visibility.Hidden;
                }
            }
        }

        private Property BuildProperty(PropertyDescriptor descriptor, int index)
        {
            ITransactionContext transactionContext = m_editingContext.As<ITransactionContext>();

            Property property;
            if (m_cacheableProperties.TryGetValue(descriptor, out property))
                descriptor = property.Descriptor;
            else
                property = new Property(this);

            property.Descriptor = descriptor;
            property.DescriptorIndex = index;

            if (property.Context != null)
            {
                property.Context.TransactionContext = transactionContext;
            }
            else
            {
                property.Context = new PropertyEditorControlContext(
                    this, descriptor, transactionContext, ContextRegistry);
            }

            CustomizeAttribute customizeAttribute = null;

            // customize attributes set through api take precedence over attributes on class type
            CustomizeAttribute[] attributes = CustomizeAttributes;
            if (attributes == null)
            {
                attributes = (CustomizeAttribute[])descriptor.ComponentType.GetCustomAttributes(typeof(CustomizeAttribute), true);
            }

            if (attributes != null)
            {
                // find the attribute by name
                foreach (CustomizeAttribute attr in attributes)
                {
                    if (attr != null)
                    {
                        if (attr.PropertyName.Equals(descriptor.Name))
                        {
                            // remember this so we can set attributes for the property and the control if one is built
                            customizeAttribute = attr;

                            // if there are multiple only the first one will get used
                            break;
                        }
                    }
                }
            }

            // set the property customize attributes
            if (customizeAttribute != null)
            {
                property.DisableSort = customizeAttribute.DisableSort;
                property.DisableDragging = customizeAttribute.DisableDragging;
                property.DisableResize = customizeAttribute.DisableResize;
                property.DisableEditing = customizeAttribute.DisableEditing;
                property.DefaultWidth = customizeAttribute.ColumnWidth;
                property.HideDisplayName = customizeAttribute.HideDisplayName;
                property.HorizontalEditorOffset = customizeAttribute.HorizontalEditorOffset;
                property.NameHasWholeRow = customizeAttribute.NameHasWholeRow;
            }

            Control control = property.Control;

            if (control != null)
            {
                // Not visible by default because toggling of property columns sets visibility.
                control.Visibility = System.Windows.Visibility.Hidden;               

                if (customizeAttribute != null)
                {
                    control.Width = customizeAttribute.ColumnWidth;
                }

            }

            m_activeProperties.Add(property);
            m_processedDescriptors.Add(descriptor);
            return property;
        }
        
        private void AddChildProperty(Property property, bool reflected, ref int index)
        {
            List<PropertyDescriptor> childProperties = new List<PropertyDescriptor>();
            foreach (PropertyDescriptor childPd in property.Descriptor.GetChildProperties())
            {
                if (reflected && m_processedDescriptors.Contains(childPd))
                    continue; // detect cycles in reflected properties
                
                bool childPdReflected = childPd.GetType().Name == "ReflectPropertyDescriptor";
                if (childPdReflected && (!reflected))
                    continue; // do not mix with the reflected 

                Property childProperty = BuildProperty(childPd, index++);
                childProperties.Add(childPd);
                childProperty.Parent = property;

                AddChildProperty(childProperty, reflected, ref index);
            }
            if (childProperties.Count > 0)
                property.ChildProperties = new PropertyDescriptorCollection(childProperties.ToArray());
        }

        // Clears and cleans up the visible properties, caching those Controls that can be.
        // Lets BuildProperties() set the visibility flag in order not to toggle it uselessly.
        private void ClearCurrentProperties()
        {
            foreach (Property p in m_activeProperties)
            {
                p.Context.TransactionContext = null;
                p.Context.ClearCachedSelection();

                Control control = p.Control;
                if (control != null &&
                    !p.Cacheable)
                {
                    // This is very expensive to remove, dispose, create, and add the Controls.
                    //  ICacheablePropertyControl should be implemented as much as possible.
                    this.RemoveVisualChild(control);
                }
            }
            m_activeProperties.Clear();
            m_processedDescriptors.Clear();
            m_categories = null;
            //Console.WriteLine("{0}: cacheable:{1}, children:{2}", this, m_cacheableProperties.Count, Controls.Count);
        }

        private void UpdatePropertySorting()
        {
            if ((PropertySorting & PropertySorting.Custom) == PropertySorting.Custom)
            {
                if (m_customSortOrder != null)
                {
                    SortPropertiesFromPropertyNamesList(m_customSortOrder);
                }
            }
            else if ((PropertySorting & PropertySorting.Alphabetical) == 0)
            {
                m_activeProperties.Sort(delegate(Property x, Property y)
                {
                    return x.DescriptorIndex.CompareTo(y.DescriptorIndex);
                });
            }
            else
                m_activeProperties.Sort(MultiLevelSort);

            foreach (Property p in m_activeProperties)
            {
                p.Category = null;
                p.FirstInCategory = false;
            }

            if ((m_propertySorting & PropertySorting.Categorized) != 0)
                BuildCategories();
        }

        private Property GetLowestCommonAncestor(Property node1, Property node2)
        {
            if (node1 == null)
                return node2;
            if (node2 == null)
                return node1;

            HashSet<Property> path1 = new HashSet<Property>(node1.Lineage);
            foreach (Property node in node2.Lineage)
                if (path1.Contains(node))
                    return node;

            return null;
        }

        private int MultiLevelSort(Property a, Property b)
        {
            if (a.Parent == null && b.Parent == null)
                return string.Compare(a.Descriptor.Name, b.Descriptor.Name);

            Property ancestor = GetLowestCommonAncestor(a, b); // comparison starts from this ancestor and down


            List<Property> pathA = new List<Property>();
            Property property = a;
            do
            {
                pathA.Add(property);
                if (property == ancestor)
                    break;
                property = property.Parent;
            } while (property != null);

            List<Property> pathB = new List<Property>();
            property = b;
            do
            {
                pathB.Add(property);
                if (property == ancestor)
                    break;
                property = property.Parent;
            } while (property != null);

            if (ancestor != null && pathA.Count == 1 && pathB.Count > 1) // B is a descendent of A
                return -1;
            else if (ancestor != null && pathB.Count == 1 && pathA.Count > 1) // A is a descendent of B
                return 1;

            // A and B has no relations
            int lowestCommonDepth = Math.Min(pathA.Count, pathB.Count);
            int result = 0;

            for (int depth = 0; depth < lowestCommonDepth; ++depth)
            {
                result = string.Compare(pathA[pathA.Count - depth - 1].Descriptor.Name, pathB[pathB.Count - depth - 1].Descriptor.Name);
                if (result != 0)
                    break;
            }
            return result;

        }

        private void BuildCategories()
        {
            List<string> names = new List<string>();
            List<List<Property>> propertyLists = new List<List<Property>>();

            foreach (Property p in m_activeProperties)
            {
                string name = PropertyUtils.GetCategoryName(p.Descriptor);
                if (name != null)
                {
                    // search for existing category
                    List<Property> propertyList = null;
                    for (int i = 0; i < names.Count; i++)
                    {
                        if (name.Equals(names[i]))
                        {
                            propertyList = propertyLists[i];
                            break;
                        }
                    }

                    // if category not found, add name and new property list
                    if (propertyList == null)
                    {
                        names.Add(name);
                        propertyList = new List<Property>();
                        propertyLists.Add(propertyList);
                    }
                    propertyList.Add(p);
                }
            }

            // build categories
            m_categories = new Category[names.Count];
            for (int i = 0; i < names.Count; i++)
                m_categories[i] = BuildCategory(propertyLists[i], names[i]);

            // sort categories
            if ((PropertySorting & PropertySorting.CategoryAlphabetical) != 0)
                Array.Sort(m_categories, delegate(Category x, Category y)
                {
                    return x.Name.CompareTo(y.Name);
                });

            // support sub-categories
            if (EnableSubCategories)
            {
                // Take advantange of the existing sorting. Convert to new array, like this:
                //  "A","A\B","C\D"
                // ==>
                //  "A","B","C","D" while making "A" a parent of "B" and "C" a parent of "D"
                List<Category> final = new List<Category>(m_categories.Length);
                foreach(Category category in m_categories)
                {
                    string parentName, childName;
                    if (Category.IsSubCategoryName(category.Name, out parentName, out childName))
                    {
                        Category parent = null;
                        for(int i = final.Count; --i >= 0; )
                        {
                            if (final[i].Name.Equals(parentName))
                            {
                                parent = final[i];
                                break;
                            }
                        }
                        if (parent == null)
                        {
                            parent = BuildCategory(new List<Property>(), parentName);
                            final.Add(parent);
                        }

                        category.Name = childName;
                        category.Parent = parent;
                    }
                    final.Add(category);
                }
                m_categories = final.ToArray();
            }

            // update m_categoryExpanded
            foreach(Category category in m_categories)
                if (!m_categoryExpanded.ContainsKey(category.Name))
                    m_categoryExpanded.Add(category.Name, true);
        }

        private Category BuildCategory(List<Property> properties, string currentName)
        {
            Category category = new Category(this);
            category.Name = currentName;
            category.Properties = properties.ToArray();
            foreach (Property property in properties)
                property.Category = category;

            if (properties.Count > 0)
            {
                properties[0].FirstInCategory = true;
            }

            return category;
        }

        /// <summary>
        /// Gets all properties, in the current sorting order, whether visible or not</summary>
        public IEnumerable<Property> Properties
        {
            get
            {
                if ((m_propertySorting & PropertySorting.Categorized) != 0)
                {
                    if (m_categories != null)
                    {
                        foreach(Category category in m_categories)
                            foreach(Property property in category.Properties)
                                yield return property;
                    }
                }
                else // uncategorized
                {
                    foreach (Property property in m_activeProperties)
                        yield return property;
                }
            }
        }

        /// <summary>
        /// Gets all properties and categories, in the current sorting order, whether visible or not</summary>
        protected IEnumerable<object> Items
        {
            get
            {
                if ((m_propertySorting & PropertySorting.Categorized) != 0)
                {
                    if (m_categories != null)
                    {
                        foreach (Category category in m_categories)
                        {
                            yield return category;
                            foreach (Property property in category.Properties)
                                yield return property;
                        }
                    }
                }
                else // uncategorized
                {
                    foreach (Property property in m_activeProperties)
                        yield return property;
                }
            }
        }

        /// <summary>
        /// Gets all visible category and property objects, in the current sorting order</summary>
        protected IEnumerable<object> VisibleItems
        {
            get
            {
                foreach(object obj in Items)
                {
                    Property p = obj as Property;
                    if (p != null)
                    {
                        if (p.Visible)
                            yield return obj;
                    }
                    else
                    {
                        Category c = (Category)obj;
                        if (c.Visible)
                            yield return obj;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the previous visible property of the given property in the current sort order</summary>
        /// <param name="property">Current property</param>
        /// <returns>Previous visible property in the current sort order</returns>
        protected Property GetPreviousProperty(Property property)
        {
            Property prev = null;
            foreach (Property p in Properties)
            {
                if (!p.Visible)
                    continue;
                if (p == property)
                    return prev;
                prev = p;
            }

            return null;
        }

        /// <summary>
        /// Gets the next visible property of the given property in the current sort order</summary>
        /// <param name="property">Current property</param>
        /// <returns>Next visible property in the current sort order</returns>
        protected Property GetNextProperty(Property property)
        {
            Property next = null;
            foreach (Property p in Properties)
            {
                if (!p.Visible)
                    continue;
                if (next != null)
                    return p;
                if (p == property)
                    next = p;
            }

            return null;
        }

        /// <summary>
        /// Returns the previous visible and editable property of the given property in the current sort order</summary>
        /// <param name="property">Current property</param>
        /// <returns>Previous visible and editable property in the current sort order, null if given property is at beginning of list</returns>
        protected Property GetPreviousEditableProperty(Property property)
        {
            Property prev = GetPreviousProperty(property);
            while ((prev != null) && prev.DisableEditing)
                prev = GetPreviousProperty(prev);
            return prev;
        }

        /// <summary>
        /// Finds the next visible and editable property of the given property in the current sort order</summary>
        /// <param name="property">Current property</param>
        /// <returns>Next visible and editable property in the current sort order, null if given property is at end of list</returns>
        protected Property GetNextEditableProperty(Property property)
        {
            Property next = GetNextProperty(property);
            while ((next != null) && next.DisableEditing)
                next = GetNextProperty(next);
            return next;
        }

        /// <summary>
        /// Finds the first visible property</summary>
        /// <returns>First visible property</returns>
        protected Property GetFirstProperty()
        {
            foreach (Property p in Properties)
            {
                if (!p.Visible)
                    continue;
                return p;
            }

            return null;
        }

        /// <summary>
        /// Finds the last visible property</summary>
        /// <returns>Last visible property</returns>
        protected Property GetLastProperty()
        {
            foreach (Property p in Properties.Reverse())
            {
                if (!p.Visible)
                    continue;
                return p;
            }

            return null;
        }

        /// <summary>
        /// Adds a property to the properties list</summary>
        /// <param name="p">The property to add</param>
        protected void AddProperty(Property p)
        {
            m_activeProperties.Add(p);
        }

        /// <summary>
        /// Removes a property from the list</summary>
        /// <param name="p">The property to remove</param>
        /// <returns><c>True</c> if successful</returns>
        protected bool RemoveProperty(Property p)
        {
            return m_activeProperties.Remove(p);
        }

        /// <summary>
        /// Inserts a property</summary>
        /// <param name="index">Index for insertion</param>
        /// <param name="p">The property to insert</param>
        protected void InsertProperty(int index, Property p)
        {
            m_activeProperties.Insert(index, p);
        }

        /// <summary>
        /// Class to hold information associated with each property</summary>
        public class Property
        {
            /// <summary>
            /// PropertyDescriptor</summary>
            public PropertyDescriptor Descriptor;
            
            /// <summary>
            /// Property category</summary>
            public Category Category;
            
            /// <summary>
            /// PropertyEditorControlContext</summary>
            public PropertyEditorControlContext Context;
            
            /// <summary>
            /// Index in list of properties in property editor</summary>
            public int DescriptorIndex;
            
            /// <summary>
            /// Whether listed first in its category</summary>
            public bool FirstInCategory;
            
            /// <summary>
            /// Whether sorting disabled for this property</summary>
            public bool DisableSort;
            
            /// <summary>
            /// Whether to disable dragging for this property</summary>
            public bool DisableDragging;
            
            /// <summary>
            /// Whether to disable resizing for this property</summary>
            public bool DisableResize;
            
            /// <summary>
            /// Whether to disable editing for this property</summary>
            public bool DisableEditing;
            
            /// <summary>
            /// Whether to hide UI label for this property</summary>
            public bool HideDisplayName;

            /// <summary>
            /// Default width of the property editor or value. If 0, then the global default is used.
            /// Determines the width of the column in the spreadsheet-style property editor.</summary>
            public int DefaultWidth;

            /// <summary>
            /// The number of pixels that the editing control or value
            /// is shifted to the right of the start of the row, in the 2-column property editor. A
            /// negative number means "use the default" which is to use the user-adjustable
            /// splitter between the name and the value columns. Default is -1.</summary>
            public int HorizontalEditorOffset = -1;

            /// <summary>
            /// Whether or not the name of the property is given the whole row in the 2-column
            /// property editor. Default is false.</summary>
            /// <remarks>If HorizontalEditorOffset is a small positive number, then it may be
            /// useful to set this property to 'true' so that the property name can be fully
            /// displayed. If 'true', the 2-column property editor will take more vertical space
            /// but can save a lot of horizontal space when HorizontalEditorOffset is used.</remarks>
            public bool NameHasWholeRow;

            // for child properties
            /// <summary>
            /// Collection of PropertyDescriptors for child properties</summary>
            public PropertyDescriptorCollection ChildProperties;
            /// <summary>
            /// Parent property, may be null</summary>
            public Property Parent;
            /// <summary>
            /// Whether child properties expanded in property editor</summary>
            public bool ChildrenExpanded;

            /// <summary>
            /// Gets or sets the property editing Control associated with the property descriptor
            /// and editing context</summary>
            public Control Control
            {
                get { return m_control; }
                set
                {
                    if (m_cacheable)
                        m_owner.m_cacheableProperties.Remove(Descriptor);

                    m_control = value;

                    ICacheablePropertyControl cacheable = m_control as ICacheablePropertyControl;
                    m_cacheable =
                        cacheable != null &&
                        cacheable.Cacheable;

                    if (m_cacheable)
                    {
                        m_owner.m_cacheableProperties.Remove(Descriptor); // in case the same Descriptor was added by a different Property
                        m_owner.m_cacheableProperties.Add(Descriptor, this);
                    }
                }
            }

            /// <summary>
            /// Gets whether this property can be cached indefinitely which means that the Control will not maintain
            /// selection state outside of the Context</summary>
            public bool Cacheable
            {
                get { return m_cacheable; }
            }

            /// <summary>
            /// Gets whether the property is visible</summary>
            public bool Visible
            {
                get
                {
                    Property property = this;
                    while (property.Parent != null)
                    {
                        if (!property.Parent.ChildrenExpanded)
                            return false;
                        property = property.Parent;
                    }

                    if (property.Parent == null)
                    return
                        Category == null ||
                            (
                            Category.Expanded &&
                            (Category.Parent == null || Category.Parent.Expanded));
                    else
                        return property.ChildrenExpanded;
                }
            }

            internal Property(PropertyView owner)
            {
                m_owner = owner;
            }

            /// <summary>
            /// Gets the lineage of this property, starting with itself</summary>
            public IEnumerable<Property> Lineage
            {
                get
                {
                    Property property = this;
                    while (property != null)
                    {
                        yield return property;
                        property = property.Parent;
                    }
                }
            }

            private Control m_control;
            private bool m_cacheable;
            private readonly PropertyView m_owner;
        }

        /// <summary>
        /// Class to hold information associated with each property category</summary>
        public class Category
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="owner">PropertyView of category owner</param>
            public Category(PropertyView owner)
            {
                m_owner = owner;
            }
            /// <summary>
            /// Category name</summary>
            public string Name;
            /// <summary>
            /// Gets or set whether category expanded</summary>
            public bool Expanded
            {
                get { return m_owner.m_categoryExpanded[Name]; }
                set { m_owner.m_categoryExpanded[Name] = value; }
            }
            /// <summary>
            /// Gets or set whether category is visible</summary>
            public bool Visible
            {
                get { return Parent == null || Parent.Expanded; }
            }
            /// <summary>
            /// Array of properties in this category</summary>
            public Property[] Properties;
            /// <summary>
            /// Parent category</summary>
            public Category Parent;

            /// <summary>
            /// Owner's PropertyView</summary>
            private PropertyView m_owner;

            internal static bool IsSubCategoryName(string fullName, out string parentName, out string childName)
            {
                parentName = null;
                childName = null;
                
                int firstSlash = fullName.IndexOf('\\');
                if (firstSlash < 0)
                    return false;

                parentName = fullName.Substring(0, firstSlash);
                childName = fullName.Substring(firstSlash + 1);
                return true;
            }
        }

        /// <summary>
        /// Gets or sets array of settings for specifying how properties should be displayed or used
        /// in the spreadsheet-style or 2-column property editors.</summary>
        public CustomizeAttribute[] CustomizeAttributes { get; set; }

        private IPropertyEditingContext m_editingContext;

        private object[] m_selectedObjects = EmptyArray<object>.Instance;
        private PropertyDescriptor[] m_propertyDescriptors = EmptyArray<PropertyDescriptor>.Instance;

        private Category[] m_categories = EmptyArray<Category>.Instance;
        private readonly List<Property> m_activeProperties = new List<Property>(); //sorted
        private readonly Dictionary<PropertyDescriptor, Property> m_cacheableProperties =
            new Dictionary<PropertyDescriptor, Property>(); //includes those in m_activeProperties
        private PropertySorting m_propertySorting = PropertySorting.ByCategory;
        private readonly Dictionary<string, bool> m_categoryExpanded = new Dictionary<string, bool>();
        private readonly HashSet<PropertyDescriptor> m_processedDescriptors 
            = new HashSet<PropertyDescriptor>(); // used for parent-child cycle detection only
        private List<string> m_customSortOrder;

        #endregion

        /// <summary>
        /// Drag threshold size</summary>
        protected static Size SystemDragSize = new Size(SystemParameters.MinimumHorizontalDragDistance, SystemParameters.MinimumVerticalDragDistance);

        /// <summary>
        /// Size of category expanders, in pixels</summary>
        protected const int ExpanderSize = Sce.Atf.GdiUtil.ExpanderSize;

        /// <summary>
        /// Attribute used to set default column attributes for property editing</summary>
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
        public class CustomizeAttribute : Sce.Atf.Controls.PropertyEditing.CustomizeAttribute
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="propertyName">Property name</param>
            /// <param name="columnWidth">Column width. 0 means use the global default.</param>
            /// <param name="disableSort">Whether or not to disable column sorting</param>
            /// <param name="disableDragging">Whether or not to disable column dragging</param>
            /// <param name="disableResize">Whether or not to disable column resizing</param>
            /// <param name="disableEditing">Whether or not to disable column editing</param>
            /// <param name="hideDisplayName">Whether or not to hide the column name</param>
            /// <param name="horizontalEditorOffset">The number of pixels that the editing control or value
            /// is shifted to the right of the start of the row, in the 2-column property editor. A
            /// negative number means "use the default" which is to use the user-adjustable
            /// splitter between the name and the value columns.</param>
            /// <param name="nameHasWholeRow">Whether or not the name of the property is given the
            /// whole row in the 2-column property editor</param>
            public CustomizeAttribute(string propertyName, int columnWidth=0, bool disableSort=false, bool disableDragging=false,
                bool disableResize = false, bool disableEditing = false, bool hideDisplayName = false,
                int horizontalEditorOffset = -1, bool nameHasWholeRow = false) 
                : base(propertyName, columnWidth, disableSort, disableDragging, disableResize, disableEditing,
                    hideDisplayName, horizontalEditorOffset, nameHasWholeRow)
            {
            }
        }
    }
}
