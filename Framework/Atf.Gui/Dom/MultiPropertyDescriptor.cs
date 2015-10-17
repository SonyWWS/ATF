//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Controls.PropertyEditing;

using SysPropertyDescriptor = System.ComponentModel.PropertyDescriptor;
using DomPropertyDescriptor = Sce.Atf.Dom.PropertyDescriptor;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// This is a property descriptor that can create a single user-editable property
    /// that applies to multiple different types of objects that otherwise could not
    /// all use the same property descriptor. The separate property descriptors of
    /// each selected object are combinable into this MultiPropertyDescriptor if
    /// they have the same Category, Name, and Type.
    /// See PropertyUtils.GetPropertyDescriptorKey().</summary>
    public class MultiPropertyDescriptor : DomPropertyDescriptor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="masterDescriptor">Master descriptor to serve as template.
        /// Name, PropertyType, Category, Description, ReadOnly, Editor, Converter 
        /// and Attributes of this descriptor are all used.</param>
        public MultiPropertyDescriptor(SysPropertyDescriptor masterDescriptor)
            : base(
                masterDescriptor.Name,
                masterDescriptor.PropertyType,
                masterDescriptor.Category,
                masterDescriptor.Description,
                masterDescriptor.IsReadOnly,
                masterDescriptor.GetEditor(typeof(object)),
                masterDescriptor.Converter,
                masterDescriptor.Attributes.Cast<Attribute>().ToArray())
        {
            m_key = masterDescriptor.GetPropertyDescriptorKey();
        }

        /// <summary>
        /// Sets the current selection.
        /// PropertyEditorControlContext should own the descriptor</summary>
        public Func<IEnumerable<object>> GetSelectionFunc { private get; set; }

        /// <summary>
        /// Tests if the value can be reset for the specified component</summary>
        /// <param name="component">Component for which the value is to be reset</param>
        /// <returns><c>True</c> if the value can be reset for the specified component</returns>
        public override bool CanResetValue(object component)
        {
            SysPropertyDescriptor descriptor = FindDescriptor(component);
            return descriptor != null && descriptor.CanResetValue(component);
        }

        /// <summary>
        /// Tests if the value can be reset for ANY of the selected items</summary>
        /// <returns><c>True</c> if the value can be reset for ANY of the selected items</returns>
        public bool CanResetValues()
        {
            foreach (object component in GetSelectionFunc())
                if (CanResetValue(component))
                    return true;
            return false;
        }

        /// <summary>
        /// Resets the values of the specified component</summary>
        /// <param name="component">Component for which the value is to be reset</param>
        public override void ResetValue(object component)
        {
            SysPropertyDescriptor descriptor = FindDescriptor(component);
            if (descriptor != null)
                descriptor.ResetValue(component);
        }

        /// <summary>
        /// Resets the values of all selected components with matching descriptors, if possible</summary>
        public void ResetValues()
        {
            foreach (object component in GetSelectionFunc())
                if (CanResetValue(component))
                    ResetValue(component);
        }

        /// <summary>
        /// Sets the value of the specified component to the specified value,
        /// if it has a matching descriptor</summary>
        /// <param name="component">Component for which to set the value</param>
        /// <param name="value">New value, must be compatible with value type of the descriptor</param>
        public override void SetValue(object component, object value)
        {
            SysPropertyDescriptor descriptor = FindDescriptor(component);
            if (descriptor != null)
                descriptor.SetValue(component, value);
        }

        /// <summary>
        /// Sets the value of all selected components to the specified value,
        /// if they have matching descriptors</summary>
        /// <param name="value">New value, must be compatible with value type of descriptors</param>
        public void SetValues(object value)
        {
            foreach (object component in GetSelectionFunc())
                SetValue(component, value);
        }

        /// <summary>
        /// Gets the value of the specified component</summary>
        /// <param name="component">Component for which to get the value</param>
        /// <returns>Value of specified component, or null if no value</returns>
        public override object GetValue(object component)
        {
            SysPropertyDescriptor descriptor = FindDescriptor(component);
            if (descriptor != null)
                return descriptor.GetValue(component);
            return null;
        }

        /// <summary>
        /// Gets the values of all selected items using their corresponding descriptors</summary>
        /// <returns>Enumeration of selected item values, or empty enumeration if selected items have no values</returns>
        public IEnumerable<object> GetValues()
        {
            foreach (object component in GetSelectionFunc())
                yield return GetValue(component);
        }

        /// <summary>
        /// Gets all of the property descriptors contained within</summary>
        /// <returns></returns>
        public IEnumerable<SysPropertyDescriptor> GetDescriptors()
        {
            return m_descriptorMap.Values;
        }

        /// <summary>
        /// Returns a property descriptor of the specified component 
        /// that matches the m_key member, or null if none found</summary>
        /// <param name="component">Component for which to find a matching descriptor</param>
        /// <returns>A property descriptor of the specified component 
        /// that matches the key member, or null if none found</returns>
        public SysPropertyDescriptor FindDescriptor(object component)
        {
            if (component == null)
                return null;

            SysPropertyDescriptor descriptor;

            // Try to get cached descriptor for the component
            if (m_descriptorMap.TryGetValue(component, out descriptor))
                return descriptor;

            // Try to find a descriptor matching the 'key' and cache it
            descriptor = PropertyUtils.FindPropertyDescriptor(component, m_key);
            m_descriptorMap.Add(component, descriptor);

            return descriptor;
        }

        private readonly string m_key;
        private readonly Dictionary<object, SysPropertyDescriptor> m_descriptorMap = new Dictionary<object, SysPropertyDescriptor>();
    }
}