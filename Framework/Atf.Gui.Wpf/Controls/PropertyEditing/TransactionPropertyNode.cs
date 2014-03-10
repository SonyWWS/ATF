//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using System.Windows;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// PropertyNode where setting the property value is done as a transaction</summary>
    public class TransactionPropertyNode : PropertyNode
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="instance">Object or collection of objects that share a property</param>
        /// <param name="descriptor">PropertyDescriptor of shared property</param>
        /// <param name="isEnumerable">Whether the object is enumerable</param>
        /// <param name="owner">Object(s) owner</param>
        public TransactionPropertyNode(object instance, PropertyDescriptor descriptor, bool isEnumerable, FrameworkElement owner)
           : base(instance, descriptor, isEnumerable, owner)
        {
        }

        /// <summary>
        /// Sets the value of the property for all object(s) in the TransactionPropertyNode, performing the set as a transaction</summary>
        /// <param name="value">New value for property</param>
        protected override void SetValue(object value)
        {
            // Due to use of anonymous lamda, we can't call directly into the base SetValue
            Owner.DataContext.As<ITransactionContext>()
                .DoTransaction(() => SetValueInternal(value), "Edit Property".Localize());
        }

        private void SetValueInternal(object value)
        {
            base.SetValue(value);
        }
    }
}
