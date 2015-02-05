//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Sce.Atf.Collections
{
    /// <summary>
    /// Class that listens for specific property changes for all items within a collection</summary>
    public class CollectionChangeListener : ChangeListener
    {
        #region Private Fields

        private readonly INotifyCollectionChanged m_value;
        private readonly Dictionary<INotifyPropertyChanged, ChangeListener> m_collectionListeners = new Dictionary<INotifyPropertyChanged, ChangeListener>();

        #endregion

        #region Ctors

        /// <summary>
        /// Constructor with collection and property name</summary>
        /// <param name="collection">Collection whose changes are listened to</param>
        /// <param name="propertyName">Property whose value change is listened to</param>
        public CollectionChangeListener(INotifyCollectionChanged collection, string propertyName)
        {
            m_value = collection;
            PropertyName = propertyName;

            Subscribe();
        }

        #endregion

        #region Private Methods

        private void Subscribe()
        {
            m_value.CollectionChanged += ValueCollectionChanged;

            foreach (INotifyPropertyChanged item in (IEnumerable)m_value)
            {
                ResetChildListener(item);
            }
        }

        private void ResetChildListener(INotifyPropertyChanged item)
        {
            Requires.NotNull(item, "item");

            RemoveItem(item);

            ChangeListener listener = null;

            if (item is INotifyCollectionChanged)
                listener = new CollectionChangeListener(item as INotifyCollectionChanged, PropertyName);
            else
                listener = new ChildChangeListener(item);

            listener.PropertyChanged += ListenerPropertyChanged;
            m_collectionListeners.Add(item, listener);
        }

        private void RemoveItem(INotifyPropertyChanged item)
        {
            if (m_collectionListeners.ContainsKey(item))
            {
                m_collectionListeners[item].PropertyChanged -= ListenerPropertyChanged;

                m_collectionListeners[item].Dispose();
                m_collectionListeners.Remove(item);
            }
        }


        private void ClearCollection()
        {
            foreach (var key in m_collectionListeners.Keys)
            {
                m_collectionListeners[key].Dispose();
            }

            m_collectionListeners.Clear();
        }

        #endregion

        #region Event Handlers

        void ValueCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearCollection();
            }
            else
            {
                // Don't care about e.Action, if there are old items, Remove them...
                if (e.OldItems != null)
                {
                    foreach (INotifyPropertyChanged item in (IEnumerable)e.OldItems)
                        RemoveItem(item);
                }

                // ...add new items as well
                if (e.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in (IEnumerable)e.NewItems)
                        ResetChildListener(item);
                }
            }
        }

        void ListenerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(sender, string.Format("{0}{1}{2}", PropertyName, PropertyName != null ? "[]." : null, e.PropertyName));
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Releases all collection item handlers and self handler
        /// </summary>
        protected override void Unsubscribe()
        {
            ClearCollection();

            m_value.CollectionChanged -= ValueCollectionChanged;
        }
        
        #endregion
    }

    /// <summary>
    /// Abstract class to handle listening for changes on a view model</summary>
    public abstract class ChangeListener : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Name of property whose change is listened to</summary>
        protected string PropertyName;

        /// <summary>
        /// Stop listening for changes</summary>
        protected abstract void Unsubscribe();

        #region INotifyPropertyChanged

        /// <summary>
        /// Property changed event</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raise property changed event</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="propertyName">Property whose value change is listened to</param>
        protected virtual void RaisePropertyChanged(object sender, string propertyName)
        {
            OnPropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handle property changed event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Property changed event arguments</param>
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(sender, e);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Dispose of resources</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unsubscribe();
            }
        }

        /// <summary>
        /// Default destructor</summary>
        ~ChangeListener()
        {
            Dispose(false);
        }

        #endregion

        /// <summary>
        /// Create a ChangeListener for item whose property value can change</summary>
        /// <param name="value">Item whose property value change is listened to</param>
        /// <returns>ChangeListener for item</returns>
        public static ChangeListener Create(INotifyPropertyChanged value)
        {
            return Create(value, null);
        }

        /// <summary>
        /// Create a ChangeListener for item whose value can change</summary>
        /// <param name="value">Item whose property value change is listened to</param>
        /// <param name="propertyName">Property whose value change is listened to</param>
        /// <returns>ChangeListener for item</returns>
        public static ChangeListener Create(INotifyPropertyChanged value, string propertyName)
        {
            if (value is INotifyCollectionChanged)
            {
                return new CollectionChangeListener(value as INotifyCollectionChanged, propertyName);
            }

            return new ChildChangeListener(value, propertyName);
        }
    }

    /// <summary>
    /// Class used to listen for property changes on a child view model</summary>
    public class ChildChangeListener : ChangeListener
    {
        #region Fields

        /// <summary>
        /// Type of item whose property value change is listened to</summary>
        protected static readonly Type m_inotifyType = typeof(INotifyPropertyChanged);
        private readonly INotifyPropertyChanged m_value;
        private readonly Type m_type;
        private readonly Dictionary<string, ChangeListener> m_childListeners = new Dictionary<string, ChangeListener>();

        #endregion

        #region Ctors

        /// <summary>
        /// Constructor with item</summary>
        /// <param name="instance">Item whose property value change is listened to</param>
        public ChildChangeListener(INotifyPropertyChanged instance)
        {
            Requires.NotNull(instance, "instance");

            m_value = instance;
            m_type = m_value.GetType();

            Subscribe();
        }

        /// <summary>
        /// Constructor with item and property</summary>
        /// <param name="instance">Item whose property value change is listened to</param>
        /// <param name="propertyName">Name of property whose value change is listened to</param>
        public ChildChangeListener(INotifyPropertyChanged instance, string propertyName)
            : this(instance)
        {
            PropertyName = propertyName;
        }

        #endregion

        #region Private Methods

        private void Subscribe()
        {
            m_value.PropertyChanged += ValuePropertyChanged;

            var query =
                from property
                in m_type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where m_inotifyType.IsAssignableFrom(property.PropertyType)
                select property;

            foreach (var property in query)
            {
                // Declare property as known "Child", then register it
                m_childListeners.Add(property.Name, null);
                ResetChildListener(property.Name);
            }
        }

        /// <summary>
        /// Resets known (must exist in children collection) child event handlers
        /// </summary>
        /// <param name="propertyName">Name of known child property</param>
        private void ResetChildListener(string propertyName)
        {
            if (m_childListeners.ContainsKey(propertyName))
            {
                // Unsubscribe if existing
                if (m_childListeners[propertyName] != null)
                {
                    m_childListeners[propertyName].PropertyChanged -= ChildPropertyChanged;

                    // Should unsubscribe all events
                    m_childListeners[propertyName].Dispose();
                    m_childListeners[propertyName] = null;
                }

                var property = m_type.GetProperty(propertyName);
                if (property == null)
                    throw new InvalidOperationException(string.Format("Was unable to get '{0}' property information from Type '{1}'", propertyName, m_type.Name));

                object newValue = property.GetValue(m_value, null);

                // Only recreate if there is a new value
                if (newValue != null)
                {
                    if (newValue is INotifyCollectionChanged)
                    {
                        m_childListeners[propertyName] = new CollectionChangeListener(newValue as INotifyCollectionChanged, propertyName);
                    }
                    else if (newValue is INotifyPropertyChanged)
                    {
                        m_childListeners[propertyName] = new ChildChangeListener(newValue as INotifyPropertyChanged, propertyName);
                    }

                    if (m_childListeners[propertyName] != null)
                        m_childListeners[propertyName].PropertyChanged += ChildPropertyChanged;
                }
            }
        }
        
        #endregion

        #region Event Handlers

        void ChildPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(sender, e.PropertyName);
        }

        void ValuePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ResetChildListener(e.PropertyName);
            RaisePropertyChanged(sender, e.PropertyName);
        }

        /// <summary>
        /// Raise property changed event</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="propertyName">Property changing</param>
        protected override void RaisePropertyChanged(object sender, string propertyName)
        {
            base.RaisePropertyChanged(sender, string.Format("{0}{1}{2}", PropertyName, PropertyName != null ? "." : null, propertyName));
        }
        
        #endregion

        #region Overrides

        /// <summary>
        /// Release all child handlers and self handler
        /// </summary>
        protected override void Unsubscribe()
        {
            m_value.PropertyChanged -= ValuePropertyChanged;

            foreach (var binderKey in m_childListeners.Keys)
            {
                if (m_childListeners[binderKey] != null)
                    m_childListeners[binderKey].Dispose();
            }

            m_childListeners.Clear();
        }
        
        #endregion
    }
}
