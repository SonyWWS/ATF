//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Sce.Atf.Dom;

namespace Sce.Atf.Adaptation
{
    /// <summary>
    /// Base class for binding adapter objects</summary>
    /// <remarks>
    /// This class is a CustomTypeDescriptor object that adapts an IAdaptable
    /// object and provides a customized set of property descriptors for binding.</remarks>
    public abstract class BindingAdapterObjectBase : CustomTypeDescriptor,
        IAdaptable,
        INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="adaptee">Object that is adapted</param>
        protected BindingAdapterObjectBase(object adaptee)
        {
            Adaptee = adaptee;
            if (PropertyChanged != null) return;
        }

        /// <summary>
        /// Gets object that is adapted</summary>
        public object Adaptee { get; private set; }

        /// <summary>
        /// Returns the customized set of property descriptors for binding</summary>
        /// <returns>Cached property descriptors</returns>
        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        /// <summary>
        /// Returns the customized set of property descriptors for binding, generating them if necessary</summary>
        /// <param name="attributes">Attribute array (unused)</param>
        /// <returns>Cached property descriptors</returns>
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            if (m_cachedPropertyDescriptors == null && Adaptee != null)
            {
                m_cachedPropertyDescriptors = GenerateDescriptors();
            }
            
            return m_cachedPropertyDescriptors;
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Event that is raised after a property changed</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IAdaptable Members

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null if no adapter available</returns>
        public object GetAdapter(Type type)
        {
            return type.IsAssignableFrom(GetType()) ? this : Adaptee.As(type);
        }

        #endregion

        /// <summary>
        /// Generates a customized set of property descriptors for binding</summary>
        /// <returns>Cached property descriptors</returns>
        protected abstract PropertyDescriptorCollection GenerateDescriptors();

        /// <summary>
        /// Merges given set of property descriptors with this instance's property descriptors</summary>
        /// <param name="result">Merged property descriptors</param>
        /// <param name="descriptors">Property descriptors to merge</param>
        protected static void MergeDescriptors(List<System.ComponentModel.PropertyDescriptor> result, System.ComponentModel.PropertyDescriptor[] descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (!result.Any(x => x.Name == descriptor.Name))
                    result.Add(descriptor);
            }
        }

        // Local PropertyDescriptorCollection
        private PropertyDescriptorCollection m_cachedPropertyDescriptors;
    }

    /// <summary>
    /// This BindingAdapterObject generates PropertyDescriptors based 
    /// on adapting the adaptee to all possible types and returning 
    /// descriptors for each adapter</summary>
    public class BindingAdapterObject : BindingAdapterObjectBase
    {
        /// <summary>
        /// Constructor with adaptee</summary>
        /// <param name="adaptee">Adaptee object</param>
        public BindingAdapterObject(object adaptee)
            : base(adaptee)
        {
        }

        /// <summary>
        /// Generate a PropertyDescriptorCollection for adapters adapting adaptee</summary>
        /// <returns>PropertyDescriptorCollection for adapters</returns>
        protected override PropertyDescriptorCollection GenerateDescriptors()
        {
            var result = new List<PropertyDescriptor>();

            foreach (var adapter in Adaptee.AsAll<object>())
            {
                var adapterType = adapter.GetType();
                MergeDescriptors(result, GetDescriptorsFromBaseTypes(adapterType));
            }

            return new PropertyDescriptorCollection(result.ToArray());
        }

        private static BindingAdapterPropertyDescriptor[] GetDescriptorsFromBaseTypes(Type adapterType)
        {
            // First try to get baseTypes from static lookup
            BindingAdapterPropertyDescriptor[] descriptors;

            if (!s_baseTypesLookup.TryGetValue(adapterType, out descriptors))
            {
                // If this fails then use reflection to get all base types and interfaces
                // from this adapter type.  
                // Dont want to lock during this opertation as it is slow

                var baseTypes = new List<Type>(adapterType.GetInterfaces());

                // Get all base types
                Type type = adapterType;
                while (type != typeof(object))
                {
                    baseTypes.Add(type);
                    type = type.BaseType;
                }

                // Generate descriptors from types
                var descriptorsList = new List<BindingAdapterPropertyDescriptor>();
                foreach (var baseType in baseTypes)
                {
                    // Check for name clashes - for now just ignore multiple types with the same
                    // simple type name
                    if (!descriptorsList.Any(x => x.Name == baseType.Name))
                    {
                        descriptorsList.Add(new BindingAdapterPropertyDescriptor(baseType));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Binding Adapter Warning: Ignoring multiple base types with the same simple name: " + baseType.Name);
                    }
                }

                descriptors = descriptorsList.ToArray();

                // Now lock and recheck before adding to the satic lookup
                lock (s_baseTypesLookup)
                {
                    if (!s_baseTypesLookup.ContainsKey(adapterType))
                    {
                        s_baseTypesLookup.Add(adapterType, descriptors);
                    }
                }
            }

            return s_baseTypesLookup[adapterType];
        }

        // Static map of types to an array of PropertyDescriptors generated from their
        // interfaces and base types
        private static readonly Dictionary<Type, BindingAdapterPropertyDescriptor[]> s_baseTypesLookup
            = new Dictionary<Type, BindingAdapterPropertyDescriptor[]>();

        /// <summary>
        /// Custom property descriptor class for BindingAdapter</summary>
        private class BindingAdapterPropertyDescriptor : PropertyDescriptor
        {
            private readonly Type m_type;

            public BindingAdapterPropertyDescriptor(Type type)
                : base(type.Name, null)
            {
                m_type = type;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override void ResetValue(object component)
            {
                throw new NotImplementedException();
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(BindingAdapterObject); }
            }

            public override Type PropertyType
            {
                get { return m_type; }
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            public override object GetValue(object component)
            {
                return ((BindingAdapterObject)component).Adaptee.As(m_type);
            }

            public override void SetValue(object component, object value)
            {
                throw new NotSupportedException();
            }
        }
    }

    /// <summary>
    /// DOM specific version of BindingAdapterObject which adds an optimization option
    /// which will cache property descriptors by DomNodeType</summary>
    public class DomBindingAdapterObject : BindingAdapterObject
    {
        /// <summary>
        /// Constructor with adaptee and optimization flag</summary>
        /// <param name="adaptee">Adaptee DomNode</param>
        /// <param name="enableNodeTypeExtensionOptimisation">True to enable static caching of
        /// property descriptors by node type</param>
        public DomBindingAdapterObject(DomNode adaptee, bool enableNodeTypeExtensionOptimisation)
            : base(adaptee)
        {
            EnableNodeTypeExtensionOptimisation = enableNodeTypeExtensionOptimisation;
        }

        /// <summary>
        /// Get whether to enable optimization by statically caching PropertyDescriptorCollections by DomNodeType</summary>
        public bool EnableNodeTypeExtensionOptimisation { get; private set; }

        /// <summary>
        /// Generate a PropertyDescriptorCollection for adapters adapting adaptee</summary>
        /// <returns>PropertyDescriptorCollection for adapters</returns>
        protected override PropertyDescriptorCollection GenerateDescriptors()
        {
            PropertyDescriptorCollection result = null;

            if (EnableNodeTypeExtensionOptimisation)
            {
                var node = Adaptee.Cast<DomNode>();

                lock (s_cachedPropertyDescriptors)
                {
                    // Performance can be improved here by assuming that all DomNodes
                    // of a given DomNodeType will always return the same set of adapter types
                    // we can then create a static lookup
                    if (!s_cachedPropertyDescriptors.TryGetValue(node.Type, out result))
                    {
                        result = base.GenerateDescriptors();

                        if (!s_cachedPropertyDescriptors.ContainsKey(node.Type))
                        {
                            s_cachedPropertyDescriptors.Add(node.Type, result);
                        }
                    }
                }
            }
            else
            {
                result = base.GenerateDescriptors();
            }

            return result;
        }

        // Static map of DomNodeTypes to their cached descriptor collections
        // Only used when EnableNodeTypeExtensionOptimisation is true
        private static readonly Dictionary<DomNodeType, PropertyDescriptorCollection> s_cachedPropertyDescriptors
            = new Dictionary<DomNodeType, PropertyDescriptorCollection>();
    }
}
