//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Context for embedded property editing controls that provides a safe,
    /// convenient interface for getting and setting values without worrying
    /// about the intricacies of property descriptors and transaction contexts.
    /// Is only constructed by PropertyView.</summary>
    public class PropertyEditorControlContext : ITypeDescriptorContext
    {
        /// <summary>
        /// Constructor, used only by PropertyView</summary>
        /// <param name="editingControlOwner">Interface for property editing control owners</param>
        /// <param name="descriptor">Property descriptor for property being edited</param>
        /// <param name="transactionContext">Interface for transaction contexts</param>
        public PropertyEditorControlContext(
            IPropertyEditingControlOwner editingControlOwner,
            PropertyDescriptor descriptor,
            ITransactionContext transactionContext)
        {
            m_editingControlOwner = editingControlOwner;
            m_descriptor = descriptor;
            m_transactionContext = transactionContext;

            // MultiPropertyDescriptors need the SelectedObjects delegate
            // to be able to retrieve the selection. They need the delegate
            // rather than just the current selection because of control caching.
            var desc = descriptor as Sce.Atf.Dom.MultiPropertyDescriptor;
            if (desc != null)
                desc.GetSelectionFunc = () => SelectedObjects;
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="editingControlOwner">Interface for property editing control owners</param>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="transactionContext">Interface for transaction contexts</param>
        /// <param name="contextRegistry">Context registry</param>
        public PropertyEditorControlContext(
            IPropertyEditingControlOwner editingControlOwner,
            PropertyDescriptor descriptor,
            ITransactionContext transactionContext,
            IContextRegistry contextRegistry)
            : this(editingControlOwner, descriptor, transactionContext)
        {
            m_contextRegistry = contextRegistry;
        }

        /// <summary>
        /// Gets PropertyDescriptor</summary>
        public PropertyDescriptor Descriptor
        {
            get { return m_descriptor; }
        }

        /// <summary>
        /// Gets the interface for property editing control owners, e.g., GridView
        /// (spreadsheet-style property editor) or PropertyGridView (2-column property editor)</summary>
        public IPropertyEditingControlOwner EditingControlOwner
        {
            get { return m_editingControlOwner; }
        }

        /// <summary>
        /// Cache current selection</summary>
        public void CacheSelection()
        {
            object[] selection = m_editingControlOwner.SelectedObjects;
            m_cachedSelection = new List<object>(selection.Length);
            m_cachedSelection.AddRange(selection);
        }

        /// <summary>
        /// Clear cached selection</summary>
        public void ClearCachedSelection()
        {
            m_cachedSelection = null;
        }

        /// <summary>
        /// Gets cached selection if any, otherwise gets current selection</summary>
        public IEnumerable<object> SelectedObjects
        {
            get { return (IEnumerable<object>)m_cachedSelection ?? m_editingControlOwner.SelectedObjects; }
        }

        /// <summary>
        /// Gets last selected object from cached selection if any, otherwise gets last selected object from current selection</summary>
        public object LastSelectedObject
        {
            get
            {
                if (m_cachedSelection != null)
                    return m_cachedSelection.Count > 0 ? m_cachedSelection[m_cachedSelection.Count - 1] : null;

                object[] selectedObjects = m_editingControlOwner.SelectedObjects;
                return selectedObjects.Length > 0 ? selectedObjects[selectedObjects.Length - 1] : null;
            }
        }

        /// <summary>
        /// Gets the property value on the last selected object</summary>
        /// <returns>Property value on the last selected object</returns>
        public virtual object GetValue()
        {
            return m_descriptor.GetValue(LastSelectedObject);
        }

        /// <summary>
        /// Gets an array of property values for all selected objects</summary>
        /// <returns>Array of property values for all selected objects</returns>
        public object[] GetValues()
        {
            List<object> result = new List<object>();
            foreach (object selectedObject in SelectedObjects)
                result.Add(m_descriptor.GetValue(selectedObject));
            return result.ToArray();
        }

        /// <summary>
        /// Resets the property value on all selected objects.</summary>
        public virtual void ResetValue()
        {
            m_transactionContext.DoTransaction(delegate
            {
                PropertyUtils.ResetProperty(SelectedObjects, m_descriptor);                
            },
               string.Format("Reset: {0}".Localize(), m_descriptor.DisplayName));
        }

        /// <summary>
        /// Sets the property value on all selected objects</summary>
        /// <param name="newValue">New property value</param>
        public virtual void SetValue(object newValue)
        {
            m_transactionContext.DoTransaction(delegate
            {
                foreach (object selectedObject in SelectedObjects)
                    PropertyUtils.SetProperty(selectedObject, m_descriptor, newValue);
            },
               string.Format("Edit: {0}".Localize(), m_descriptor.DisplayName));
        }

        /// <summary>
        /// Sets the Array property value on all selected objects</summary>
        /// <param name="refArray">Reference array that the user sees as the old value</param>
        /// <param name="newArray">New array that the user edited</param>
        public void SetValue(Array refArray, Array newArray)
        {
            bool[] equalComponents = GetEqualComponents(refArray, newArray);

            m_transactionContext.DoTransaction(delegate
            {
                foreach (object selectedObject in SelectedObjects)
                {
                    Array oldArray = (Array)m_descriptor.GetValue(selectedObject);
                    Array mergedArray = GetMergedArray(selectedObject, oldArray, newArray, equalComponents);
                    //if (!PropertyUtils.AreEqual(oldArray, mergedArray)) TODO make command combining more sophisticated
                    {
                        PropertyUtils.SetProperty(selectedObject, m_descriptor, mergedArray);
                    }
                }
            },
               string.Format("Edit: {0}".Localize(), m_descriptor.DisplayName));
        }

        /// <summary>
        /// Gets the transaction context</summary>
        public ITransactionContext TransactionContext
        {
            get { return m_transactionContext; }
            set { m_transactionContext = value; }
        }

        /// <summary>
        /// Gets the context registry</summary>
        public IContextRegistry ContextRegistry
        {
            get { return m_contextRegistry; }
        }

        private static bool[] GetEqualComponents(Array refArray, Array newArray)
        {
            bool[] result = new bool[newArray.Length];
            if (refArray != null)
            {
                for (int i = 0; i < newArray.Length; i++)
                    result[i] = PropertyUtils.AreEqual(newArray.GetValue(i), refArray.GetValue(i));
            }

            return result;
        }

        private static Array GetMergedArray(object selected, Array oldArray, Array newArray, bool[] equalComponents)
        {
            Array result = newArray;
            if (oldArray != null)
            {
                result = (Array)oldArray.Clone();
                for (int i = 0; i < newArray.Length; i++)
                    if (!equalComponents[i])
                        result.SetValue(newArray.GetValue(i), i);
            }

            return result;
        }

        /// <summary>
        /// Gets whether the last selected object has the default property value</summary>
        public bool IsDefaultValue
        {
            get { return !m_descriptor.CanResetValue(LastSelectedObject); }
        }

        /// <summary>
        /// Gets whether all selected objects have the default property value</summary>
        public bool IsDefaultValueForAll
        {
            get
            {
                foreach (object selectedObject in SelectedObjects)
                    if (m_descriptor.CanResetValue(selectedObject))
                        return false;

                return true;
            }
        }

        /// <summary>
        /// Gets whether this property is read-only</summary>
        public bool IsReadOnly
        {
            get { return m_descriptor.IsReadOnly; }
        }

        /// <summary>
        /// Gets a UITypeEditor for editing the property</summary>
        /// <returns>UITypeEditor for editing the property</returns>
        public UITypeEditor GetUITypeEditor()
        {
            UITypeEditor editor = WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);
            return editor;
        }

        /// <summary>
        /// Gets property text for this property, possibly using a TypeConverter</summary>
        /// <returns>Property text for this property</returns>
        public string GetPropertyText()
        {
            string text = PropertyUtils.GetPropertyText(LastSelectedObject, m_descriptor);
            return text;
        }

        #region IServiceProvider Members

        /// <summary>
        /// Gets the service object of the specified type</summary>
        /// <param name="serviceType">An object that specifies the type of service object to get</param>
        /// <returns>A service object of type serviceType, or null if there is no service object of type serviceType</returns>
        object IServiceProvider.GetService(Type serviceType)
        {
            if (serviceType.IsAssignableFrom(GetType()))
            {
                return this;
            }

            return null;
        }

        #endregion

        #region ITypeDescriptorContext Members

        /// <summary>
        /// Gets the container representing this System.ComponentModel.TypeDescriptor request</summary>
        IContainer ITypeDescriptorContext.Container
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the object that is connected with this type descriptor request</summary>
        /// <returns>The object that invokes the method on the <see cref="T:System.ComponentModel.TypeDescriptor"></see>; 
        /// otherwise, null if there is no object responsible for the call</returns>
        object ITypeDescriptorContext.Instance
        {
            get { return LastSelectedObject; }
        }

        /// <summary>
        /// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanged"></see> event</summary>
        void ITypeDescriptorContext.OnComponentChanged()
        {
        }

        /// <summary>
        /// Raises the <see cref="E:System.ComponentModel.Design.IComponentChangeService.ComponentChanging"></see> event</summary>
        /// <returns>True iff this object can be changed</returns>
        bool ITypeDescriptorContext.OnComponentChanging()
        {
            return true;
        }

        /// <summary>
        /// Gets the <see cref="T:System.ComponentModel.PropertyDescriptor"></see> that is associated with the given context item</summary>
        /// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor"></see> that describes the given context item; 
        /// otherwise, null if there is no <see cref="T:System.ComponentModel.PropertyDescriptor"></see> responsible for the call</returns>
        PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor
        {
            get { return m_descriptor; }
        }

        #endregion

        private readonly IPropertyEditingControlOwner m_editingControlOwner;
        private readonly PropertyDescriptor m_descriptor;
        private readonly IContextRegistry m_contextRegistry;
        private List<object> m_cachedSelection;
        private ITransactionContext m_transactionContext;
    }
}