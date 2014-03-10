//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel;

using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Context in which properties can be edited, holding the selected items whose
    /// properties are to be edited. The context provides machinery to get the descriptors
    /// for the properties being edited. The context performs property edits as logical
    /// commands for the undo/redo system.</summary>
    public class PropertyEditingContext : IPropertyEditingContext
    {
        /// <summary>
        /// Constructor with selected objects</summary>
        /// <param name="selection">Selected objects, in order of selection</param>
        public PropertyEditingContext(object[] selection)
            : this(selection, null)
        {
        }

        /// <summary>
        /// Constructor with selected objects and inner context</summary>
        /// <param name="selection">Selected objects, in order of selection</param>
        /// <param name="innerContext">Inner context containing this context</param>
        public PropertyEditingContext(object[] selection, PropertyEditingContext innerContext)
        {
            m_selection = selection;
            m_innerContext = innerContext;
        }

        /// <summary>
        /// Gets all selected objects</summary>
        public object[] Selection
        {
            get { return m_selection; }
        }

        /// <summary>
        /// Gets the last selected object</summary>
        public object LastSelected
        {
            get { return m_selection[m_selection.Length - 1]; }
        }

        /// <summary>
        /// Returns whether or not the given values are equal</summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <returns>True iff the given values are equal</returns>
        /// <remarks>Default is to do limited deep equality testing for array types, and
        /// allow small errors with floating point types. Override to customize this behavior.</remarks>
        public virtual bool AreEqual(object value1, object value2)
        {
            return PropertyUtils.AreEqual(value1, value2);
        }

        /// <summary>
        /// Returns property descriptors for the object</summary>
        /// <param name="owner">Object with properties</param>
        /// <returns>Array of property descriptors for the object</returns>
        public virtual PropertyDescriptor[] GetPropertyDescriptors(object owner)
        {
            if (m_innerContext != null)
                return m_innerContext.GetPropertyDescriptors(owner);

            return PropertyUtils.GetDefaultProperties2(owner);
        }

        /// <summary>
        /// Gets an array of property descriptors for a property editing context. This array
        /// represents the properties common to all items in the context's selection.</summary>
        /// <param name="context">Property editing context</param>
        /// <returns>Array of property descriptors common to all items in the context's selection</returns>
        public static PropertyDescriptor[] GetPropertyDescriptors(PropertyEditingContext context)
        {
            if (context == null || context.m_selection.Length == 0)
                return new PropertyDescriptor[0];

            if (context.m_propertyDescriptors != null)
                return context.m_propertyDescriptors;

            PropertyDescriptor[] result = context.CreatePropertyDescriptors();

            context.m_propertyDescriptors = result;
            return result;
        }

        /// <summary>
        /// Tests if the property values for the current selection can be reset</summary>
        /// <param name="descriptor">Property descriptor representing property</param>
        /// <returns>True iff the property values for the current selection can be reset</returns>
        public bool CanResetValue(PropertyDescriptor descriptor)
        {
            foreach (object selected in m_selection)
                if (!descriptor.CanResetValue(selected))
                    return false;

            return true;
        }

        /// <summary>
        /// Resets the property values for the current selection</summary>
        /// <param name="descriptor">Property descriptor representing property</param>
        public void ResetValue(PropertyDescriptor descriptor)
        {
            foreach (object selected in m_selection)
                descriptor.ResetValue(selected);
        }

        #region IPropertyEditingContext Members

        /// <summary>
        /// Gets an enumeration of the items with properties</summary>
        IEnumerable<object> IPropertyEditingContext.Items
        {
            get { return m_selection; }
        }

        /// <summary>
        /// Gets an enumeration of the property descriptors for the items</summary>
        IEnumerable<PropertyDescriptor> IPropertyEditingContext.PropertyDescriptors
        {
            get { return GetPropertyDescriptors(this); }
        }

        #endregion

        /// <summary>
        /// Creates an array of property descriptors for the current selection. This array
        /// represents the properties common to all items in the context's selection. The current
        /// selection must have at least one object in it.</summary>
        /// <returns>Array of property descriptors common to all items in the context's selection</returns>
        /// <remarks>The cache is not checked or updated. Is protected, so that callers must use
        /// the static method to get caching.</remarks>
        protected virtual PropertyDescriptor[] CreatePropertyDescriptors()
        {
            List<PropertyDescriptor> result =
                new List<PropertyDescriptor>(GetPropertyDescriptors(m_selection[0]));

            for (int i = 1; i < m_selection.Length; i++)
            {
                HashSet<PropertyDescriptor> next =
                    new HashSet<PropertyDescriptor>(GetPropertyDescriptors(m_selection[i]));
                for (int j = 0; j < result.Count; )
                {
                    if (!next.Contains(result[j]))
                        result.RemoveAt(j);
                    else
                        j++;
                }
            }

            return result.ToArray();
        }

        private readonly object[] m_selection;
        private PropertyDescriptor[] m_propertyDescriptors;
        private readonly PropertyEditingContext m_innerContext;
    }
}
