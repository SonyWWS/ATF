//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Base class for binding adapter objects</summary>
    /// <remarks>
    /// This class is a CustomTypeDescriptor object that adapts an IAdaptable
    /// object and provides a customised set of property descriptors for WPF binding.</remarks>
    internal abstract class BindingAdapterObjectBase : CustomTypeDescriptor,
        IAdaptable,
        INotifyPropertyChanged
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="adaptee">Object that is adapted</param>
        public BindingAdapterObjectBase(object adaptee)
        {
            Adaptee = adaptee;
        }

        /// <summary>
        /// Gets object that is adapted</summary>
        public object Adaptee { get; private set; }

        /// <summary>
        /// Returns the customised set of property descriptors for WPF binding</summary>
        /// <returns>Cached property descriptors</returns>
        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        /// <summary>
        /// Returns the customised set of property descriptors for WPF binding, generating them if necessary</summary>
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

        // This is unused but INotifyPropertyChanged is implemented 
        // to speed up WPF binding to this object
#pragma warning disable 0067
        /// <summary>
        /// Event that is raised after a property changed</summary>
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 0067

        #endregion

        #region IAdaptable Members

        /// <summary>
        /// Gets an adapter of the specified type or null</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type or null if no adapter available</returns>
        public object GetAdapter(Type type)
        {
            if (type.IsAssignableFrom(GetType()))
                return this;
            return Adaptee.As(type);
        }

        #endregion

        /// <summary>
        /// Generates a customised set of property descriptors for WPF binding</summary>
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
    /// This BindingAdapterObject generates propertydescriptors based 
    /// on adapting the adaptee to all possible types and returning 
    /// descriptors for each adapter</summary>
    internal class BindingAdapterObject : BindingAdapterObjectBase
    {
        public BindingAdapterObject(object adaptee)
            : base(adaptee)
        {
        }

        protected override PropertyDescriptorCollection GenerateDescriptors()
        {
            var result = new List<System.ComponentModel.PropertyDescriptor>();

            foreach (var adapter in Adaptee.AsAll<object>())
            {
                Type adapterType = adapter.GetType();
                BindingAdapterPropertyDescriptor[] descriptors = GetDescriptorsFromBaseTypes(adapterType);
                MergeDescriptors(result, descriptors);
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
                // Don't want to lock during this operation as it is slow.

                List<Type> baseTypes = new List<Type>(adapterType.GetInterfaces());

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
                    if (!descriptorsList.Any<BindingAdapterPropertyDescriptor>(x => x.Name == baseType.Name))
                    {
                        descriptorsList.Add(new BindingAdapterPropertyDescriptor(baseType));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Warning: Ignoring multiple base types with the same simple name: " + baseType.Name);
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
        private static Dictionary<Type, BindingAdapterPropertyDescriptor[]> s_baseTypesLookup
            = new Dictionary<Type, BindingAdapterPropertyDescriptor[]>();

        /// <summary>
        /// Custom property descriptor class for BindingAdapter</summary>
        private class BindingAdapterPropertyDescriptor : System.ComponentModel.PropertyDescriptor
        {
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
            private Type m_type;

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
}
