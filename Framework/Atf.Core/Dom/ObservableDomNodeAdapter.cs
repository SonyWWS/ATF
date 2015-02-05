//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// DomNodeAdapter that facilitates observing property changes in DomNode children</summary>
    public abstract class ObservableDomNodeAdapter : DomNodeAdapter, INotifyPropertyChanged
    {
        /// <summary>
        /// Perform initialization when the adapter's node is set.
        /// Check all property descriptors of all registered adapter types and when an ObservableDomPropertyAttribute
        /// is present on the descriptor, finds the AttributeInfo and ChildInfo with matching name on the DomNodeType,
        /// and sets PropertyChangedEventArgs tags to match.
        /// This method is called each time the adapter is connected to its underlying node.
        /// Typically overridden by creators of DOM adapters.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            Type adapterType = GetType();

            lock (s_registeredTypes)
            {
                if (!s_registeredTypes.Find(adapterType).Contains(DomNode.Type))
                {
                    AddAttributeTags(adapterType, DomNode.Type);
                    AddChildTags(adapterType, DomNode.Type);
                    s_registeredTypes.Add(adapterType, DomNode.Type);
                }
            }

            m_hasTags = HasTags();
        }

        /// <summary>
        /// Gets the children of our adapted DomNode</summary>
        /// <typeparam name="T">The type to adapt each child to</typeparam>
        /// <param name="childInfo">Metadata to indicate the child list</param>
        /// <returns>Wrapper that adapts a node child list to a list of T items</returns>
        public IObservableCollection<T> GetObservableChildList<T>(ChildInfo childInfo)
            where T : class
        {
            IObservableCollection list;

            if (m_childListsCache == null)
                m_childListsCache = new Dictionary<ChildInfo, Dictionary<Type, IObservableCollection>>();

            lock (m_childListsCache)
            {
                Dictionary<Type, IObservableCollection> typeLookup;
                if (!m_childListsCache.TryGetValue(childInfo, out typeLookup))
                {
                    // Unlike the normal DomNodeAdapter, we cache child lists
                    // this is so that any controls bound to the list will share the same
                    // ICollectionView
                    typeLookup = new Dictionary<Type, IObservableCollection>();
                    m_childListsCache.Add(childInfo, typeLookup);
                }

                if (!typeLookup.TryGetValue(typeof(T), out list))
                {
                    list = new ObservableDomNodeListAdapter<T>(DomNode, childInfo);
                    typeLookup.Add(typeof(T), list);
                }
            }

            return (IObservableCollection<T>)list;
        }

        private bool HasTags()
        {
            return DomNode.Type.Attributes
                .Any(attributeInfo => attributeInfo.GetTag<PropertyChangedEventArgsCollection>() != null);
        }

        /// <summary>
        /// Checks all property descriptors of adapter type and when an ObservableDomPropertyAttribute
        /// is present on the descriptor, finds the AttributeInfo with matching name on the DomNodeType,
        /// and sets PropertyChangedEventArgs tags to match</summary>
        /// <param name="adapterType">Type of adapter</param>
        /// <param name="nodeType">DomNode type</param>
        private static void AddAttributeTags(Type adapterType, DomNodeType nodeType)
        {
            foreach (var property in adapterType.GetProperties())
            {
                object[] attributes = property.GetCustomAttributes(typeof(ObservableDomPropertyAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    var eventArgs = new PropertyChangedEventArgs(property.Name);

                    foreach (var attribute in attributes)
                    {
                        var attributeName = ((ObservableDomPropertyAttribute)attribute).AttributeName;
                        var currentType = nodeType;
                        var attributeInfo = currentType.GetAttributeInfo(attributeName);
                       
                        Requires.NotNull(attributeInfo, "Unrecognized attribute name in ObservableDomPropertyAttribute");

                        // Each base type may have duplicate attributeInfo if it is an inherited attribute
                        // We need to set the tag on each base attributeInfo so that the PropertyChanged event
                        // will be raised even if the attribute is set on a base type
                        while (currentType != DomNodeType.BaseOfAllTypes && attributeInfo != null)
                        {
                            var args = attributeInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
                            if (args == null)
                            {
                                args = new PropertyChangedEventArgsCollection();
                                attributeInfo.SetTag(args);
                            }
                            
                            args.Add(eventArgs);

                            currentType = currentType.BaseType;
                            attributeInfo = currentType.GetAttributeInfo(attributeName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks all property descriptors of adapter type and when an ObservableDomPropertyAttribute
        /// is present on the descriptor, finds the ChildInfo with matching name on the DomNodeType,
        /// and sets PropertyChangedEventArgs tags to match</summary>
        /// <param name="adapterType">Type of adapter</param>
        /// <param name="nodeType">DomNode type</param>
        private static void AddChildTags(Type adapterType, DomNodeType nodeType)
        {
            foreach (var property in adapterType.GetProperties())
            {
                object[] attributes = property.GetCustomAttributes(typeof(ObservableDomChildAttribute), true);
                if (attributes.Length > 0)
                {
                    var eventArgs = new PropertyChangedEventArgs(property.Name);

                    foreach (var attribute in attributes)
                    {
                        var childName = ((ObservableDomChildAttribute)attribute).ChildName;
                        var currentType = nodeType;
                        var childInfo = currentType.GetChildInfo(childName);

                        Requires.NotNull(childInfo, "Unrecognized childInfo in ObservableDomPropertyAttribute");

                        // Each base type may have duplicate childInfo if it is an inherited childInfo
                        // We need to set the tag on each base childInfo so that the PropertyChanged event
                        // will be raised even if the child is set on a base type
                        while (currentType != DomNodeType.BaseOfAllTypes && childInfo != null)
                        {
                            var args = childInfo.GetTag<PropertyChangedEventArgsCollection>();
                            if (args == null)
                            {
                                args = new PropertyChangedEventArgsCollection();
                                childInfo.SetTag(args);
                            }
                            
                            args.Add(eventArgs);

                            currentType = currentType.BaseType;
                            childInfo = currentType.GetChildInfo(childName);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// On attribute changed attempt to get PropertyChangedEventArgs tags from 
        /// AttributeInfo and raise events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Attribute event arguments</param>
        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (e.DomNode != DomNode)
                return;
            
            var args = e.AttributeInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
            if (args != null)
            {
                foreach (var arg in args)
                    OnPropertyChanged(arg);
            }
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (e.Parent != DomNode)
                return;
            
            var args = e.ChildInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
            if (args != null)
            {
                foreach (var arg in args)
                    OnPropertyChanged(arg);
            }
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (e.Parent != DomNode)
                return;
            
            var args = e.ChildInfo.GetTagLocal<PropertyChangedEventArgsCollection>();
            if (args != null)
            {
                foreach (var arg in args)
                    OnPropertyChanged(arg);
            }
        }

        private void Subscibe()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;
        }

        private void Unsubscibe()
        {
            DomNode.AttributeChanged -= DomNode_AttributeChanged;
            DomNode.ChildInserted -= DomNode_ChildInserted;
            DomNode.ChildRemoved -= DomNode_ChildRemoved;
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Property changed event</summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                if (m_propertyChanged == null && m_hasTags)
                    Subscibe();

                m_propertyChanged += value;
            }
            remove
            {
                m_propertyChanged -= value;

                if(m_propertyChanged == null)
                    Unsubscibe();
            }
        }

        #endregion

        /// <summary>
        /// Raise PropertyChanged event</summary>
        /// <param name="propertyName">Name of changed property</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handle PropertyChanged event</summary>
        /// <param name="e">PropertyChanged event arguments</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = m_propertyChanged;
            if (handler != null)
                handler(this, e);
        }

        private static readonly Multimap<Type, DomNodeType> s_registeredTypes = new Multimap<Type, DomNodeType>();
        private Dictionary<ChildInfo, Dictionary<Type, IObservableCollection>> m_childListsCache;
        private bool m_hasTags;
        private event PropertyChangedEventHandler m_propertyChanged;
    }
}
