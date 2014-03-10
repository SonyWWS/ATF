//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// Encapsulates an object or collection of objects and a property common to them all</summary>
    public class PropertyNode : NotifyPropertyChangedBase, IDataErrorInfo, IDisposable, IComparable<PropertyNode>,
                                IComparable
    {
        // This class is similar to ATF PropertyEditorControlContext
        // perhaps look at merging base behavior?

        private static readonly PropertyChangedEventArgs s_isSelectedArgs
            = ObservableUtil.CreateArgs<PropertyNode>(x => x.IsSelected);

        private static readonly PropertyChangedEventArgs s_valueArgs
            = ObservableUtil.CreateArgs<PropertyNode>(x => x.Value);

        private readonly PropertyDescriptor m_descriptor;
        private readonly object m_instance;
        private bool m_disposed;

        private bool m_isSelected;

        /// <summary>
        /// Constructor</summary>
        /// <param name="instance">Object or collection of objects that share a property</param>
        /// <param name="descriptor">PropertyDescriptor of shared property</param>
        /// <param name="isEnumerable">Whether the object is enumerable</param>
        /// <param name="owner">Object(s) owner</param>
        public PropertyNode(object instance, PropertyDescriptor descriptor, bool isEnumerable, FrameworkElement owner)
        {
            m_instance = instance;
            m_descriptor = descriptor;
            IsEnumerable = isEnumerable;
            Owner = owner;
            SubscribeValueChanged();

            if (m_descriptor.Converter != null)
            {
                var tdcontext = new TypeDescriptorContext(FirstInstance, m_descriptor, null);
                if (m_descriptor.Converter.GetStandardValuesExclusive(tdcontext))
                {
                    StandardValues = m_descriptor.Converter.GetStandardValues().Cast<object>().ToArray();
                }
            }
        }

        /// <summary>
        /// Gets objects' owner</summary>
        public FrameworkElement Owner { get; private set; }

        /// <summary>
        /// Gets the object(s) (may be enumerable)</summary>
        public object Instance
        {
            get { return m_instance; }
        }

        /// <summary>
        /// Gets the first instance of the objects if the instance is enumerable, otherwise returns the object</summary>
        /// <value>The first instance</value>
        public object FirstInstance
        {
            get
            {
                if (IsEnumerable)
                    return ((IEnumerable) m_instance).Cast<object>().FirstOrDefault();
                return m_instance;
            }
        }

        /// <summary>
        /// Gets the object or a single instance as an enumerable if the PropertyNode has a collection of objects</summary>
        /// <value>The instances</value>
        public IEnumerable Instances
        {
            get
            {
                if (IsEnumerable)
                {
                    foreach (object o in (IEnumerable) m_instance)
                        yield return o;
                }
                else
                {
                    yield return m_instance;
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
        /// Gets the common value of the property for all objects in the PropertyNode
        /// or null if there is no common value for the property</summary>
        public object Value
        {
            get
            {
                if (IsEnumerable)
                    return GetValueFromEnumerable();
                return GetValue(m_instance);
            }
            set { SetValue(value); }
        }

        /// <summary>
        /// Gets the last Value before it was last changed</summary>
        public object OldValue { get; private set; }

        /// <summary>
        /// Gets whether the object(s) in the PropertyNode is/are enumerable</summary>
        public bool IsEnumerable { get; private set; }

        /// <summary>
        /// Gets the context for an editor for this PropertyNode</summary>
        public object EditorContext { get; internal set; }

        /// <summary>
        /// Gets a collection of standard values for the property's data type that
        /// its type converter is designed for when provided with a format context.
        /// Is null if the data type does not support a standard set of values.</summary>
        public object[] StandardValues { get; private set; }

        /// <summary>
        /// Gets or sets whether one of the instances is selected</summary>
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
                if (y != null)
                    return x.CompareTo(y);
                return 1;
            }

            if (y != null)
                return -1;
            return 0;
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
            return m_descriptor.GetEditor(typeof(ValueEditor)) as ValueEditor;
        }

        /// <summary>
        /// Stops handling property value changed events</summary>
        public virtual void UnBind()
        {
            UnsubscribeValueChanged();
        }

        /// <summary>
        /// Refreshes property values, notifying listeners as if they had changed</summary>
        public virtual void Refresh()
        {
            OnPropertyChanged(s_valueArgs);
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

        /// <summary>
        /// Gets common value of the property for all objects in the PropertyNode or null if there is no common value for the property</summary>
        /// <returns>Common value of the property for all objects in the PropertyNode or null</returns>
        private object GetValueFromEnumerable()
        {
            object value = null;

            foreach (object component in (IEnumerable) m_instance)
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

        private void SubscribeValueChanged()
        {
            foreach (object instance in Instances)
                m_descriptor.AddValueChanged(instance, Instance_PropertyValueChanged);
        }

        private void UnsubscribeValueChanged()
        {
            foreach (object instance in Instances)
                m_descriptor.RemoveValueChanged(instance, Instance_PropertyValueChanged);
        }

        private void Instance_PropertyValueChanged(object sender, EventArgs e)
        {
            OnValueChanged();
            OnPropertyChanged(s_valueArgs);
        }

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
                    UnBind();
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
        ///     <c>true</c> if this instance is writeable; otherwise, <c>false</c>.
        /// </value>
        public bool IsWriteable
        {
            get { return !IsReadOnly; }
        }

        /// <summary>
        /// Gets whether this instance's property is read only</summary>
        /// <value>
        ///     <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get { return Descriptor.IsReadOnly; }
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
}