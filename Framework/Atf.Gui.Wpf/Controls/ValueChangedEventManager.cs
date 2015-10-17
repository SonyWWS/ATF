//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Sce.Atf.Wpf.Controls
{

    /// <summary>
    /// The "value changed" pattern doesn't use events. To listen for changes
    /// in a property, a client first obtains the PropertyDescriptor for that
    /// property, then calls the AddValueChanged method to register a callback.
    /// The arguments to the callback don't say which property has changed(!).
    ///
    /// The standard manager implementation doesn't work for this. Hence this
    /// manager overrides and/or ignores the base class methods.
    ///
    /// This manager keeps a table of records, indexed by PropertyDescriptor.
    /// Each record holds the following information:
    ///  *  PropertyDescriptor
    ///  *  Callback method
    ///  *  ListenerList
    ///     
    /// In short, there's a separate callback method for each property. That
    /// method knows which property has changed, and can ask the manager to
    /// deliver the "event" to the listeners that are interested in that property.
    /// Manager for the object.ValueChanged event.</summary>
    public class ValueChangedEventManager : WeakEventManager
    {
        /// <summary>
        /// Add a listener to the given source's event</summary>
        /// <param name="source">Source of event</param>
        /// <param name="listener">Event listener</param>
        /// <param name="pd">Property descriptor for the value</param>
        public static void AddListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateAddListener(source, listener, pd);
        }

        /// <summary>
        /// Remove a listener from the given source's event</summary>
        /// <param name="source">Source of event</param>
        /// <param name="listener">Event listener</param>
        /// <param name="pd">Property descriptor for the value</param>
        public static void RemoveListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.PrivateRemoveListener(source, listener, pd);
        }

        // The next two methods need to be defined, but they're never called.

        /// <summary>
        /// Listen to the given source for the event</summary>
        /// <param name="source">Event source</param>
        protected override void StartListening(object source)
        {
        }

        /// <summary>
        /// Stop listening to the given source for the event</summary>
        /// <param name="source">Event source</param>
        protected override void StopListening(object source)
        {
        }

        /// <summary>
        /// Remove dead entries from the data for the given event source</summary>
        /// <param name="source">Event source</param>
        /// <param name="data">Data from which to remove entries</param>
        /// <param name="purgeAll">If true, purge all entries</param>
        /// <returns><c>True</c> if some entries were actually removed</returns>
        protected override bool Purge(object source, object data, bool purgeAll)
        {
            bool foundDirt = false;

            HybridDictionary dict = (HybridDictionary)data;

            // copy the keys into a separate array, so that later on
            // we can change the dictionary while iterating over the keys
            ICollection ic = dict.Keys;
            PropertyDescriptor[] keys = new PropertyDescriptor[ic.Count];
            ic.CopyTo(keys, 0);

            for (int i = keys.Length - 1; i >= 0; --i)
            {
                // for each key, remove dead entries in its list
                bool removeList = purgeAll || source == null;

                ValueChangedRecord record = (ValueChangedRecord)dict[keys[i]];

                if (!removeList)
                {
                    if (record.Purge())
                        foundDirt = true;

                    removeList = record.IsEmpty;
                }

                // if there are no more entries, remove the key
                if (removeList)
                {
                    record.StopListening();
                    if (!purgeAll)
                    {
                        dict.Remove(keys[i]);
                    }
                }
            }

            // if there are no more listeners at all, remove the entry from
            // the main table
            if (dict.Count == 0)
            {
                foundDirt = true;
                if (source != null)     // source may have been GC'd
                {
                    this.Remove(source);
                }
            }

            return foundDirt;
        }

        private ValueChangedEventManager()
        {
        }

        /// <summary>
        /// Gets the event manager for the current thread.</summary>
        private static ValueChangedEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(ValueChangedEventManager);
                ValueChangedEventManager manager = (ValueChangedEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new ValueChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }

        /// <summary>
        /// Add a listener to the given property</summary>
        /// <param name="source">Source of the event</param>
        /// <param name="listener">The listener to add</param>
        /// <param name="pd">Property descriptor for the value</param>
        private void PrivateAddListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
        {
            Debug.Assert(listener != null && source != null && pd != null,
                "Listener, source, and pd of event cannot be null");

            using (WriteLock)
            {
                HybridDictionary dict = (HybridDictionary)this[source];

                if (dict == null)
                {
                    // no entry in the hashtable - add a new one
                    dict = new HybridDictionary();

                    this[source] = dict;
                }

                ValueChangedRecord record = (ValueChangedRecord)dict[pd];

                if (record == null)
                {
                    // no entry in the dictionary - add a new one
                    record = new ValueChangedRecord(this, source, pd);

                    dict[pd] = record;
                }

                // add a listener to the list
                record.Add(listener);

                // schedule a cleanup pass
                ScheduleCleanup();
            }
        }

        /// <summary>
        /// Remove a listener from the named property (empty means "any property")</summary>
        /// <param name="source">Source of the event</param>
        /// <param name="listener">The listener to remove</param>
        /// <param name="pd">Property descriptor for the value</param>
        private void PrivateRemoveListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
        {
            Debug.Assert(listener != null && source != null && pd != null,
                "Listener, source, and pd of event cannot be null");

            using (WriteLock)
            {
                HybridDictionary dict = (HybridDictionary)this[source];

                if (dict != null)
                {
                    ValueChangedRecord record = (ValueChangedRecord)dict[pd];

                    if (record != null)
                    {
                        // remove a listener from the list
                        record.Remove(listener);

                        // when the last listener goes away, remove the list
                        if (record.IsEmpty)
                        {
                            dict.Remove(pd);
                        }
                    }

                    if (dict.Count == 0)
                    {
                        Remove(source);
                    }
                }
            }
        }

        private class ValueChangedRecord
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="manager">The value changed event manager</param>
            /// <param name="source">The source of the value changed event</param>
            /// <param name="pd">The property descriptor of the value that changed</param>
            public ValueChangedRecord(ValueChangedEventManager manager, object source, PropertyDescriptor pd)
            {
                // keep a strong reference to the source.  Normally we avoid this, but
                // it's OK here since its scope is exactly the same as the strong reference
                // held by the PD:  begins with pd.AddValueChanged, ends with
                // pd.RemoveValueChanged.   This ensures that we _can_ call RemoveValueChanged
                // even in cases where the source implements value-semantics (which
                // confuses the PD - see 795205).
                m_manager = manager;
                m_source = new WeakReference(source);
                m_pd = pd;
                m_eventArgs = new ValueChangedEventArgs(pd);

                pd.AddValueChanged(source, OnValueChanged);
            }

            /// <summary>
            /// Gets whether the list of listeners is empty</summary>
            public bool IsEmpty
            {
                get { return m_listeners.IsEmpty; }
            }

            /// <summary>
            /// Add a listener</summary>
            /// <param name="listener">The listener to add</param>
            public void Add(IWeakEventListener listener)
            {
                // make sure list is ready for writing
                ListenerList.PrepareForWriting(ref m_listeners);

                m_listeners.Add(listener);
            }

            /// <summary>
            /// Remove a listener</summary>
            /// <param name="listener">The listener to remove</param>
            public void Remove(IWeakEventListener listener)
            {
                // make sure list is ready for writing
                ListenerList.PrepareForWriting(ref m_listeners);

                m_listeners.Remove(listener);

                // when the last listener goes away, remove the callback
                if (m_listeners.IsEmpty)
                {
                    StopListening();
                }
            }

            /// <summary>
            /// Purge dead entries</summary>
            /// <returns><c>True</c> if any entries were purged, otherwise false</returns>
            public bool Purge()
            {
                ListenerList.PrepareForWriting(ref m_listeners);
                return m_listeners.Purge();
            }

            /// <summary>
            /// Remove the callback from the PropertyDescriptor</summary>
            public void StopListening()
            {
                if (m_source != null && m_source.IsAlive)
                {
                    m_pd.RemoveValueChanged(m_source.Target, OnValueChanged);
                }
                
                m_source = null;
            }

            /// <summary>
            /// Forward the ValueChanged event to the listeners</summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void OnValueChanged(object sender, EventArgs e)
            {
                // mark the list of listeners "in use"
                using (m_manager.ReadLock)
                {
                    m_listeners.BeginUse();
                }

                // deliver the event, being sure to undo the effect of BeginUse().
                try
                {
                    m_manager.DeliverEventToList(sender, m_eventArgs, m_listeners);
                }
                finally
                {
                    m_listeners.EndUse();
                }
            }

            PropertyDescriptor m_pd;
            ValueChangedEventManager m_manager;
            WeakReference m_source;
            ListenerList m_listeners = new ListenerList();
            ValueChangedEventArgs m_eventArgs;
        }
    }

    /// <summary>
    /// Event arguments for the ValueChanged event</summary>
    public class ValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor with PropertyDescriptor</summary>
        /// <param name="pd">Property descriptor for the value that changed</param>
        public ValueChangedEventArgs(PropertyDescriptor pd)
        {
            m_pd = pd;
        }

        /// <summary>
        /// Gets the PropertyDescriptor for the value that changed</summary>
        public PropertyDescriptor PropertyDescriptor
        {
            get { return m_pd; }
        }

        private PropertyDescriptor m_pd;
    }
}
