//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class that supports property editing on any ISelectionContext; it implements
    /// IObservableContext by raising the Reloaded event when the selection changes. It only
    /// raises the Reloaded event (although it tries to do that efficiently using an
    /// IValidationContext interface) if the selection context can be converted to an IValidationContext interface.</summary>
    public class SelectionPropertyEditingContext : IPropertyEditingContext, IObservableContext, IAdaptable
    {
        /// <summary>
        /// Gets or sets the selection context for property editing</summary>
        public ISelectionContext SelectionContext
        {
            get { return m_selectionContext; }
            set
            {
                if (m_selectionContext != value)
                {
                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged -= selection_Changed;

                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemChanged -= observableContext_ItemChanged;
                            m_observableContext.ItemInserted -= observableContext_ItemInserted;
                            m_observableContext.ItemRemoved -= observableContext_ItemRemoved;
                        }

                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning -= validationContext_Beginning;
                            m_validationContext.Ended -= validationContext_Ended;
                            m_validationContext.Cancelled -= validationContext_Cancelled;
                        }
                    }

                    m_selectionContext = value;
                    m_observableContext = null;
                    m_validationContext = null;

                    if (m_selectionContext != null)
                    {
                        m_selectionContext.SelectionChanged += selection_Changed;

                        m_observableContext = m_selectionContext.As<IObservableContext>();
                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemChanged += observableContext_ItemChanged;
                            m_observableContext.ItemInserted += observableContext_ItemInserted;
                            m_observableContext.ItemRemoved += observableContext_ItemRemoved;
                        }

                        m_validationContext = m_selectionContext.As<IValidationContext>();
                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning += validationContext_Beginning;
                            m_validationContext.Ended += validationContext_Ended;
                            m_validationContext.Cancelled += validationContext_Cancelled;
                        }
                    }

                    OnReloaded(EventArgs.Empty);
                }
            }
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            OnReloaded(e);
        }

        #region IPropertyEditingContext Members

        /// <summary>
        /// Gets an enumeration of the items with properties</summary>
        public IEnumerable<object> Items
        {
            get
            {
                if (m_selectionContext != null)
                    return m_selectionContext.Selection;

                return EmptyEnumerable<object>.Instance;
            }
        }

        /// <summary>
        /// Gets an enumeration of the property descriptors for the items</summary>
        public IEnumerable<PropertyDescriptor> PropertyDescriptors
        {
            get { return GetPropertyDescriptors(); }
        }

        #endregion

        #region IObservableContext Members

        /// <summary>
        /// Event that is raised when an item is inserted</summary>
        public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted
        {
            add { } // not supported
            remove { }
        }

        /// <summary>
        /// Event that is raised when an item is removed</summary>
        public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved
        {
            add { } // not supported
            remove { }
        }

        /// <summary>
        /// Event that is raised when an item is changed</summary>
        public event EventHandler<ItemChangedEventArgs<object>> ItemChanged
        {
            add { } // not supported
            remove { }
        }

        /// <summary>
        /// Event that is raised when collection has been reloaded</summary>
        public event EventHandler Reloaded;

        #endregion

        #region IAdaptable Members

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null</returns>
        object IAdaptable.GetAdapter(Type type)
        {
            // prefer returning this, if compatible
            if (type.IsAssignableFrom(GetType()))
                return this;

            // otherwise, selection context may be able to adapt
            return m_selectionContext.As(type);
        }

        #endregion

        /// <summary>
        /// Raises the Reloaded event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnReloaded(EventArgs e)
        {
            Reloaded.Raise(this, e);
        }

        /// <summary>
        /// Returns a sequence of property descriptors for the selected objects</summary>
        /// <returns>Sequence of property descriptors for the selected objects</returns>
        /// <remarks>Default behavior is to return only the descriptors common to all
        /// selected objects; override to customize.</remarks>
        protected virtual IEnumerable<PropertyDescriptor> GetPropertyDescriptors()
        {
            return PropertyUtils.GetSharedProperties(Items); // only props shared by all selected objects
        }

        private void validationContext_Beginning(object sender, EventArgs e)
        {
            m_validating = true;
        }

        private void validationContext_Ended(object sender, EventArgs e)
        {
            EndValidation();
        }

        private void validationContext_Cancelled(object sender, EventArgs e)
        {
            EndValidation();
        }

        private void observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            Invalidate();
        }

        private void observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            Invalidate();
        }

        private void observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            Invalidate();
        }

        private void Invalidate()
        {
            if (m_validating)
            {
                m_invalid = true;
            }
            else
            {
                OnReloaded(EventArgs.Empty);
                m_invalid = false;
            }
        }

        private void EndValidation()
        {
            m_validating = false;
            if (m_invalid)
                OnReloaded(EventArgs.Empty);
            m_invalid = false;
        }

        private ISelectionContext m_selectionContext;
        private IObservableContext m_observableContext;
        private IValidationContext m_validationContext;

        private bool m_validating;
        private bool m_invalid;
    }
}
