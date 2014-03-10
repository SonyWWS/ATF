//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Windows;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// TransactionPropertyNode factory</summary>
    public class TransactionPropertyFactory : IPropertyFactory
    {
        /// <summary>
        /// Gets or sets the factory instance, creating it if necessary</summary>
        public static TransactionPropertyFactory Instance
        {
            get
            {
                if (s_instance == null)
                    s_instance = new TransactionPropertyFactory();
                return s_instance;
            }
        }

        /// <summary>
        /// Creates and initializes TransactionPropertyNode instance from parameters</summary>
        /// <param name="instance">Object or collection of objects that share a property</param>
        /// <param name="descriptor">PropertyDescriptor of shared property</param>
        /// <param name="isEnumerable">Whether the object is enumerable</param>
        /// <param name="owner">Object(s) owner</param>
        /// <returns>Initialized PropertyNode instance</returns>
        public static PropertyNode CreateTransactionProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable, FrameworkElement owner)
        {
            return new TransactionPropertyNode(instance, descriptor, isEnumerable, owner);
        }

        #region IPropertyFactory Members

        /// <summary>
        /// Creates and initializes TransactionPropertyNode instance from parameters</summary>
        /// <param name="instance">Object or collection of objects that share a property</param>
        /// <param name="descriptor">PropertyDescriptor of shared property</param>
        /// <param name="isEnumerable">Whether the object is enumerable</param>
        /// <param name="owner">Object(s) owner</param>
        /// <returns>Initialized PropertyNode instance</returns>
        public PropertyNode CreateProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable, FrameworkElement owner)
        {
            return CreateTransactionProperty(instance, descriptor, isEnumerable, owner);
        }

        #endregion

        private static TransactionPropertyFactory s_instance;
    }
}
