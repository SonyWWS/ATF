//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Encapsulates an object or collection of objects and a property common to them all</summary>
    public class PropertyNode : NotifyPropertyChangedBase, 
        IDataErrorInfo, 
        IDisposable, 
        IComparable<PropertyNode>,
        IComparable, 
        IWeakEventListener
    {
        private static readonly PropertyChangedEventArgs s_isExpandedArgs
            = ObservableUtil.CreateArgs<PropertyNode>(x => x.IsExpanded);

        private static readonly PropertyChangedEventArgs s_isSelectedArgs
            = ObservableUtil.CreateArgs<PropertyNode>(x => x.IsSelected);

        private static readonly PropertyChangedEventArgs s_readOnlyArgs
            = ObservableUtil.CreateArgs<PropertyNode>(x => x.IsReadOnly);

        private static readonly PropertyChangedEventArgs s_isWriteableArgs
            = ObservableUtil.CreateArgs<PropertyNode>(x => x.IsWriteable);

        private PropertyDescriptor m_descriptor;
        private object m_instance;
        private bool m_isEnumerable;
        private bool m_synchronizing;
        private bool m_disposed;
        private bool m_isExpanded = true;
        private bool m_isSelected;

        /// <summary>
        /// Initialize the node</summary>
        /// <param name="instance">Instance or enumerable of instances</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="isEnumerable"><c>True</c> if instance parameter holds an IEnumerable of instances</param>
        public void Initialize(object instance, PropertyDescriptor descriptor, bool isEnumerable)
        {
            Requires.NotNull(instance, "instance");
            Requires.NotNull(descriptor, "descriptor");

            m_instance = instance;
            m_descriptor = descriptor;
            m_isEnumerable = isEnumerable;
            
            InitializeInternal();
            
            SubscribeValueChanged();
        }

        /// <summary>
        /// Internal initialization function</summary>
        protected virtual void InitializeInternal()
        {
        }

        /// <summary>
        /// Gets the instance (may be an Enumerable)</summary>
        public object Instance
        {
            get
            {
                var customTypeDescriptor = m_instance as ICustomTypeDescriptor;
                return (customTypeDescriptor != null) ? customTypeDescriptor.GetPropertyOwner(m_descriptor) : m_instance;
            }
        }

        /// <summary>
        /// Gets whether node encapsulates multiple instances</summary>
        public bool IsMultipleInstance
        {
            get
            {
                if (IsEnumerable)
                {
                    int count = 0;
                    foreach (object item in ((IEnumerable)Instance).Cast<object>())
                    {
                        if (count++ > 0)
                            return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Gets the first instance if the Instance is an Enumerable, otherwise gets the Instance</summary>
        /// <value>The first instance</value>
        public object FirstInstance
        {
            get { return IsEnumerable ? Instances.Cast<object>().FirstOrDefault() : Instance; }
        }

        /// <summary>
        /// Gets the instances or a single Instance as an Enumerable</summary>
        /// <value>The instances</value>
        public IEnumerable Instances
        {
            get
            {
                if (IsEnumerable)
                {
                    foreach (object instance in (IEnumerable)Instance)
                    {
                        var customTypeDescriptor = instance as ICustomTypeDescriptor;
                        yield return (customTypeDescriptor != null) ? customTypeDescriptor.GetPropertyOwner(m_descriptor) : instance;
                    }
                }
                else
                {
                    yield return Instance;
                }
            }
        }

        /// <summary>
        /// Gets the PropertyDescriptor</summary>
        public PropertyDescriptor Descriptor
        {
            get { return m_descriptor; }
        }

        /// <summary>
        /// Gets or sets the instance value(s)</summary>
        public object Value
        {
            get { return IsEnumerable ? GetValueFromEnumerable() : GetValue(Instance); }
            set
            {
                if (!m_synchronizing)
                {
                    try
                    {
                        m_synchronizing = true;
                        SetValue(value);
                    }
                    finally
                    {
                        m_synchronizing = false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the last Value before it was last changed</summary>
        public object OldValue { get; private set; }

        /// <summary>
        /// Gets whether the object(s) in the PropertyNode is/are enumerable</summary>
        public bool IsEnumerable
        {
            get { return m_isEnumerable; }
        }

        /// <summary>
        /// Gets whether the value of the PropertyNode can be reset</summary>
        public bool CanResetValue
        {
            get { return Instances.Cast<object>().All(x => m_descriptor.CanResetValue(x)); }
        }

        /// <summary>
        /// Resets the value of the PropertyNode to default</summary>
        public virtual void ResetValue()
        {
            foreach (object instance in Instances)
            {
                if (m_descriptor.CanResetValue(instance))
                    m_descriptor.ResetValue(instance);
            }
        }

        /// <summary>
        /// Gets the context for an editor for this PropertyNode</summary>
        public object EditorContext { get; set; }

        /// <summary>
        /// Gets an array of standard values for the instance(s).
        /// Gets null if no standard values or standard values for each instance
        /// are not identical.</summary>
        public object[] StandardValues
        {
            get
            {
                object[] result = null;
                if (!IsEnumerable)
                {
                    result = GetStandardValues(FirstInstance);
                }
                else
                {
                    foreach (var instance in Instances)
                    {
                        var values = GetStandardValues(instance);
                        if (result == null)
                        {
                            result = values;
                        }
                        else if (values == null || !result.SequenceEqual(values))
                        {
                            result = null;
                            break;
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Gets or sets whether PropertyNode expanded</summary>
        public bool IsExpanded
        {
            get { return m_isExpanded; }
            set
            {
                m_isExpanded = value;
                OnPropertyChanged(s_isExpandedArgs);
            }
        }

        /// <summary>
        /// Gets or sets whether PropertyNode selected</summary>
        public bool IsSelected
        {
            get { return m_isSelected; }
            set
            {
                m_isSelected = value;
                OnPropertyChanged(s_isSelectedArgs);
            }
        }

        /// <summary>
        /// Gets the last exception that occurred when the property value was set</summary>
        public Exception PropertyValueError { get; private set; }

        #region IComparable Members

        /// <summary>
        /// Compares the instance's common property value to another object's common property value
        /// and returns a value indicating whether one is less than, equal to, or greater than the other</summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>Zero if the given object can't be cast as a PropertyNode.
        /// Otherwise, a signed integer that indicates the relative values of the objects:
        /// -1: PropertyNode intance's common property is less than given PropertyNode's common property.
        /// Zero: PropertyNode instance's common property equals given PropertyNode's common property. 
        /// 1: PropertyNode instance's common property is greater than given PropertyNode's common property.</returns>
        public int CompareTo(object obj)
        {
            var other = obj as PropertyNode;
            if (other == null)
                return 0;
            return CompareTo(other);
        }

        #endregion

        #region IComparable<PropertyNode> Members

        /// <summary>
        /// Compares the instance's common property value to another object's common property value
        /// and returns a value indicating whether one is less than, equal to, or greater than the other</summary>
        /// <param name="other">Object to compare to</param>
        /// <returns>A signed integer that indicates the relative values of the objects:
        /// -1: PropertyNode intance's common property is less than given PropertyNode's common property.
        /// Zero: PropertyNode instance's common property equals given PropertyNode's common property. 
        /// 1: PropertyNode instance's common property is greater than given PropertyNode's common property.</returns>
        public int CompareTo(PropertyNode other)
        {
            var x = Value as IComparable;
            var y = other.Value as IComparable;

            if (x != null)
            {
                return y != null ? x.CompareTo(y) : 1;
            }

            return (y != null) ? -1 : 0;
        }

        #endregion

        #region IDataErrorInfo Members

        /// <summary>
        /// Gets the error message for the property with the given name</summary>
        /// <param name="columnName">The name of the property whose error message to get</param>
        /// <returns>Error message for the property with the given name</returns>
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var dei = Instance as IDataErrorInfo;
                if (dei != null)
                    return dei[Descriptor.Name];
                return null;
            }
        }

        /// <summary>
        /// Gets error message indicating what is wrong with this object.
        /// The default is an empty string ("").</summary>
        string IDataErrorInfo.Error
        {
            get
            {
                var dei = Instance as IDataErrorInfo;
                if (dei != null)
                    return dei.Error;
                return null;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources</summary>
        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        /// <summary>
        /// Returns an editor for the ValueEditor type</summary>
        /// <returns>Editor for the ValueEditor type</returns>
        public ValueEditor GetCustomEditor()
        {
            if (mValueEditor != null)
                return mValueEditor;
            return m_descriptor.GetEditor(typeof(ValueEditor)) as ValueEditor;
        }

        private ValueEditor mValueEditor;
        public void SetCustomEditor(ValueEditor valueEditor)
        {
            mValueEditor = valueEditor;
        }

        /// <summary>
        /// Stop listening to property value changed events</summary>
        public virtual void UnBind()
        {
            UnsubscribeValueChanged();
        }

        /// <summary>
        /// Refreshes property values, notifying listeners as if they had changed</summary>
        public virtual void Refresh()
        {
            OnPropertyChanged(ObservableUtil.AllChangedEventArgs);
        }

        /// <summary>
        /// Sets the value of the property for all object(s) in the PropertyNode</summary>
        /// <param name="value">New value for property</param>
        protected virtual void SetValue(object value)
        {
            PropertyValueError = null;
            OldValue = Value;

            OnValueSetting();

            try
            {
                foreach (object instance in Instances)
                {
                    PropertyUtils.SetProperty(instance, m_descriptor, value);
                }
            }
            catch (Exception ex)
            {
                PropertyValueError = ex;
                OnValueError();
                throw;
            }

            OnValueSet();
        }

        /// <summary>
        /// Gets the current value of the property for a given instance</summary>
        /// <param name="instance">Object instance</param>
        /// <returns>Current value of the property for a given instance</returns>
        protected virtual object GetValue(object instance)
        {
            return m_descriptor.GetValue(instance);
        }

        private object GetValueFromEnumerable()
        {
            object value = null;

            foreach (object component in Instances)
            {
                object v = GetValue(component);
                if (value == null)
                    value = v;

                if (value != null && v == null)
                    return null;

                if (v != null && !v.Equals(value))
                    return null;
            }

            return value;
        }

        private object[] GetStandardValues(object instance)
        {
            if (m_descriptor.Converter != null)
            {
                var tdcontext = new TypeDescriptorContext(instance, m_descriptor, null);
                if (m_descriptor.Converter.GetStandardValuesExclusive(tdcontext))
                {
                    var values = m_descriptor.Converter.GetStandardValues(tdcontext);
                    if (values != null)
                    {
                        return values.Cast<object>().ToArray();
                    }
                }
            }
            return null;
        }

        private void SubscribeValueChanged()
        {
            foreach (object instance in Instances)
            {
                SubscribeValueChanged(instance);
            }
        }

        /// <summary>
        /// Start listening for value changes</summary>
        /// <param name="instance">Instance to listen for changes on</param>
        protected virtual void SubscribeValueChanged(object instance)
        {
            ValueChangedEventManager.AddListener(instance, this, m_descriptor);
        }

        private void UnsubscribeValueChanged()
        {
            foreach (object instance in Instances)
            {
                UnsubscribeValueChanged(instance);
            }
        }

        /// <summary>
        /// Stop listening for value changes</summary>
        /// <param name="instance">Instance to stop listening for changes on</param>
        protected virtual void UnsubscribeValueChanged(object instance)
        {
            ValueChangedEventManager.RemoveListener(instance, this, m_descriptor);
        }

        #region IWeakEventListener Members

        /// <summary>
        /// Receives events from event manager</summary>
        /// <param name="managerType">Event manager type</param>
        /// <param name="sender">Event originator</param>
        /// <param name="e">Event arguments</param>
        /// <returns><c>True</c> if listener handled event</returns>
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return OnReceiveWeakEvent(managerType, sender, e);
        }

        /// <summary>
        /// Handle receiving weak event</summary>
        /// <param name="managerType">Event manager type</param>
        /// <param name="sender">Event originator</param>
        /// <param name="e">Event arguments</param>
        /// <returns><c>True</c> if handled event</returns>
        protected virtual bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(ValueChangedEventManager))
            {
                var args = (ValueChangedEventArgs)e;
                if (args.PropertyDescriptor == m_descriptor)
                {
                    OnInstancePropertyValueChanged();
                    return true;
                }
            }
            return false;
        }

        #endregion

        private void OnInstancePropertyValueChanged()
        {
            OnValueChanged();
            Refresh();
        }

        /// <summary>
        /// Dispose of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    UnBind();

                    var context = EditorContext as IDisposable;
                    EditorContext = null;

                    if(context != null)
                    {
                        context.Dispose();
                    }

                    m_descriptor = null;
                    m_instance = null;

                    ValueSetting = null;
                    ValueSet = null;
                    ValueChanged = null;
                    ValueError = null;
                }

                m_disposed = true;
            }
        }

        #region Events

        /// <summary>
        /// Event that is raised before the objects' value changes</summary>
        public event EventHandler ValueSetting;

        /// <summary>
        /// Event that is raised as the objects' value changes</summary>
        public event EventHandler ValueSet;

        /// <summary>
        /// Event that is raised after the objects' value changes</summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Event that is raised when there is an error when changing objects' value</summary>
        public event EventHandler ValueError;

        /// <summary>
        /// Raises the ValueSetting event and performs custom processing</summary>
        protected virtual void OnValueSetting()
        {
            ValueSetting.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the ValueSet event and performs custom processing</summary>
        protected virtual void OnValueSet()
        {
            ValueSet.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the ValueError event and performs custom processing</summary>
        protected virtual void OnValueError()
        {
            ValueError.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the ValueChanged event and performs custom processing</summary>
        protected virtual void OnValueChanged()
        {
            ValueChanged.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region Descriptor properties

        /// <summary>
        /// Gets the name of the instance's property</summary>
        /// <value>The name</value>
        public string Name
        {
            get { return Descriptor.Name; }
        }

        /// <summary>
        /// Gets whether this instance's property is writeable</summary>
        /// <value>
        ///     <c>true</c> if this instance is writeable, otherwise <c>false</c>.
        /// </value>
        public bool IsWriteable
        {
            get { return !IsReadOnly; }
        }

        /// <summary>
        /// Gets whether this instance's property is read only</summary>
        /// <value>
        ///     <c>true</c> if this instance is read only, otherwise <c>false</c>.
        /// </value>
        public virtual bool IsReadOnly
        {
            get { return Descriptor.IsReadOnly || m_overrideReadOnly; }
            set
            {
                m_overrideReadOnly = value;
                OnReadOnlyStateChanged();
            }
        }
        private bool m_overrideReadOnly;

        /// <summary>
        /// Handle ReadOnlyStateChanged event</summary>
        protected virtual void OnReadOnlyStateChanged()
        {
            OnPropertyChanged(s_readOnlyArgs);
            OnPropertyChanged(s_isWriteableArgs);
        }

        /// <summary>
        /// Gets the type of the instance's property</summary>
        /// <value>The type of the property</value>
        public Type PropertyType
        {
            get { return Descriptor.PropertyType; }
        }

        /// <summary>
        /// Gets the name of the instance's property</summary>
        /// <value>The name of the property</value>
        public string PropertyName
        {
            get { return Descriptor.Name; }
        }

        /// <summary>
        /// Gets the display name of the instance's property</summary>
        /// <value>The display name</value>
        public string DisplayName
        {
            get { return Descriptor.DisplayName; }
        }

        /// <summary>
        /// Gets the instance's property's category</summary>
        /// <value>The category</value>
        public string Category
        {
            get { return Descriptor.Category; }
        }

        /// <summary>
        /// Gets the instance's property's description</summary>
        /// <value>The description</value>
        public string Description
        {
            get { return Descriptor.Description; }
        }

        #endregion
    }


    /// <summary>
    /// Property node that adds support for dynamic dependencies with other property descriptors</summary>
    /// <remarks>Use the GroupEnabledAttribute to enable/disable this node based on the current state
    /// of another property on the instances. Only works in single instance mode.
    /// Use the DependencyAttribute to force update of this node when another property on the instances
    /// changes.</remarks>
    public class DynamicPropertyNode : PropertyNode
    {
        private PropertyDescriptor[] m_masterGroups = EmptyArray<PropertyDescriptor>.Instance;
        private GroupEnables[] m_groupEnableAttributes = EmptyArray<GroupEnables>.Instance;
        private PropertyDescriptor[] m_dependencyGroups = EmptyArray<PropertyDescriptor>.Instance;

        /// <summary>
        /// Perform internal initialization</summary>
        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            // Note: This is attempting to add support for items which become disabled when other
            // items are set to specific values. This will need extending to support listening to multiple items.
            PropertyDescriptorCollection descriptors = null;

            var groupEnableAttribute = Descriptor.Attributes.OfType<GroupEnabledAttribute>().FirstOrDefault();
            if (groupEnableAttribute != null)
            {
                descriptors = new PropertyDescriptorCollection(PropertyUtils.GetProperties(Instances.Cast<object>()).ToArray());

                m_groupEnableAttributes = groupEnableAttribute.GroupEnables;

                var groupEnables = new List<GroupEnables>();
                var masters = new List<PropertyDescriptor>();
                foreach (var groupEnable in groupEnableAttribute.GroupEnables)
                {
                    var desc = descriptors[groupEnable.GroupName];
                    if (desc != null)
                    {
                        groupEnables.Add(groupEnable);
                        masters.Add(desc);
                    }
                    else
                    {
                        Debug.WriteLine("PropertyNode: Descriptor not found: " + groupEnable.GroupName);
                    }
                }

                m_groupEnableAttributes = groupEnables.ToArray();
                m_masterGroups = masters.ToArray();

                SetGroupEnabledState();
            }

            var dependencyAttribute = Descriptor.Attributes.OfType<DependencyAttribute>().FirstOrDefault();
            if (dependencyAttribute != null)
            {
                if (descriptors == null)
                    descriptors = new PropertyDescriptorCollection(PropertyUtils.GetProperties(Instances.Cast<object>()).ToArray());

                var groups = new List<PropertyDescriptor>();
                foreach (var descriptorName in dependencyAttribute.DependencyDescriptors)
                {
                    var desc = descriptors[descriptorName];
                    if (desc != null)
                    {
                        groups.Add(desc);
                    }
                    else
                    {
                        Debug.WriteLine("PropertyNode: Descriptor not found: " + descriptorName);
                    }
                }

                m_dependencyGroups = groups.ToArray();
            }
        }

        /// <summary>
        /// Get array of master group PropertyDescriptors</summary>
        public PropertyDescriptor[] MasterGroups
        {
            get
            {
                var copy = new PropertyDescriptor[m_masterGroups.Length];
                m_masterGroups.CopyTo(copy, 0);
                return copy;
            }
        }

        /// <summary>
        /// Get array of dependency group PropertyDescriptors</summary>
        public PropertyDescriptor[] DependencyGroups
        {
            get
            {
                var copy = new PropertyDescriptor[m_dependencyGroups.Length];
                m_dependencyGroups.CopyTo(copy, 0);
                return copy;
            }
        }

        /// <summary>
        /// Get or set whether DynamicPropertyNode is read only</summary>
        public override bool IsReadOnly
        {
            get { return base.IsReadOnly || m_groupDisable; }
            set { base.IsReadOnly = value; }
        }
        private bool m_groupDisable;

        /// <summary>
        /// Start listening for value changes</summary>
        /// <param name="instance">Instance to listen for changes on</param>
        protected override void SubscribeValueChanged(object instance)
        {
            base.SubscribeValueChanged(instance);

            // Subscribe to all master group descriptors values changing
            foreach (var masterGroup in m_masterGroups)
            {
                ValueChangedEventManager.AddListener(instance, this, masterGroup);
            }

            foreach (var dependencyGroup in m_dependencyGroups)
            {
                ValueChangedEventManager.AddListener(instance, this, dependencyGroup);
            }
        }

        /// <summary>
        /// Stop listening to property value changed events</summary>
        /// <param name="instance">Instance to stop listening for changes on</param>
        protected override void UnsubscribeValueChanged(object instance)
        {
            base.UnsubscribeValueChanged(instance);

            foreach (var masterGroup in m_masterGroups)
            {
                ValueChangedEventManager.RemoveListener(instance, this, masterGroup);
            }

            foreach (var dependencyGroup in m_dependencyGroups)
            {
                ValueChangedEventManager.RemoveListener(instance, this, dependencyGroup);
            }
        }

        /// <summary>
        /// Handle receiving weak event</summary>
        /// <param name="managerType">Event manager type</param>
        /// <param name="sender">Event originator</param>
        /// <param name="e">Event arguments</param>
        /// <returns><c>True</c> if handled event</returns>
        protected override bool OnReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            bool handled = base.OnReceiveWeakEvent(managerType, sender, e);
            if (!handled)
            {
                if (managerType == typeof(ValueChangedEventManager))
                {
                    var args = (ValueChangedEventArgs)e;
                    if (m_masterGroups.Contains(args.PropertyDescriptor))
                        OnMasterGroupPropertyValueChanged();
                    else if (m_dependencyGroups.Contains(args.PropertyDescriptor))
                        OnDependecyGroupPropertyValueChanged();
                    
                    handled = true;
                }
            }
            return handled;
        }

        private void OnMasterGroupPropertyValueChanged()
        {
            SetGroupEnabledState();
            OnMasterGroupChanged();
            Refresh();
        }

        private void OnDependecyGroupPropertyValueChanged()
        {
            OnDependencyGroupChanged();
            Refresh();
        }

        #region Events

        /// <summary>
        /// Master group changed event</summary>
        public event EventHandler MasterGroupChanged;

        /// <summary>
        /// Dependency group changed event</summary>
        public event EventHandler DependencyGroupChanged;

        /// <summary>
        /// Raise MasterGroupChanged event</summary>
        protected virtual void OnMasterGroupChanged()
        {
            MasterGroupChanged.Raise(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Raise DependencyGroupChanged event and perform custom actions</summary>
        protected virtual void OnDependencyGroupChanged()
        {
            DependencyGroupChanged.Raise(this, EventArgs.Empty);
        }

        #endregion

        private void SetGroupEnabledState()
        {
            // Update readonly flag based on value of master groups
            // If any group is disabled then disable this node
            if (!base.IsReadOnly)
            {
                // Defaults to writeable mode unless ANY group is not enabled
                bool isReadOnly = false;

                for (int index = 0; index < m_groupEnableAttributes.Length; index++)
                {
                    var attribute = m_groupEnableAttributes[index];
                    var masterGroup = m_masterGroups[index];

                    // master group could be null if no matching property descriptor was found
                    if (masterGroup != null)
                    {
                        // Get the value from instance/s
                        // This will be null if multiple instances have different values
                        object groupPropertyValue = GetValueFromDescriptor(masterGroup);
                        if (groupPropertyValue == null)
                        {
                            isReadOnly = true;
                            break;
                        }

                        // Check the value against the list of valid values
                        bool groupEnabled = GetGroupValues(masterGroup, attribute).Contains(groupPropertyValue);
                        if (!groupEnabled)
                        {
                            isReadOnly = true;
                            break;
                        }
                    }
                }

                if (m_groupDisable != isReadOnly)
                {
                    m_groupDisable = isReadOnly;
                    OnReadOnlyStateChanged();
                }
            }
        }

        private IEnumerable<object> GetGroupValues(PropertyDescriptor masterGroup, GroupEnables attribute)
        {
            // Ensure init of Typed values - currently these are cached
            // back into the GroupEnabledAttribute
            if (attribute.Values == null)
            {
                var converter = masterGroup.Converter;
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    attribute.Values = attribute.StringValues.Select(converter.ConvertFromString).ToArray();
                }
                else
                {
                    attribute.Values = attribute.StringValues;
                }
            }
            return attribute.Values;
        }

        private object GetValueFromDescriptor(PropertyDescriptor descriptor)
        {
            object value = null;

            foreach (object component in Instances)
            {
                object v = descriptor.GetValue(component);
                if (value == null)
                    value = v;

                if (value != null && v == null)
                    return null;

                if (v != null && !v.Equals(value))
                    return null;
            }

            return value;
        }
    }
}