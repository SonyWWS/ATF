//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Controls.PropertyEditing
{
    /// <summary>
    /// PropertyNode where setting the property value is done as a transaction.
    /// Holds a weak reference to the TX context</summary>
    public class TransactionPropertyNode : DynamicPropertyNode
    {
        /// <summary>
        /// Constructor with ITransactionContext</summary>
        /// <param name="context">ITransactionContext</param>
        public TransactionPropertyNode(ITransactionContext context)
        {
            if(context != null)
                m_contextRef = new WeakReference(context);
            else
                IsReadOnly = true;
        }

        /// <summary>
        /// Gets the transaction context from the owner
        /// Does not hold a strong reference to the context</summary>
        public ITransactionContext TransactionContext
        {
            get
            {
                if (m_contextRef != null && m_contextRef.IsAlive)
                    return m_contextRef.Target as ITransactionContext;
                return null;
            }
        }

        /// <summary>
        /// Stops handling property changed events</summary>
        public override void UnBind()
        {
            base.UnBind();
            if (m_contextRef != null)
                m_contextRef.Target = null;
        }

        /// <summary>
        /// Reset the property to its default value</summary>
        public override void ResetValue()
        {
            var ctxt = TransactionContext;
            if (ctxt == null)
                throw new InvalidOperationException("No transaction context available for TransactionPropertyNode");

            ctxt.DoTransaction(ResetValueInternal, "Reset Property".Localize());
        }

        /// <summary>
        /// Sets the property to the given value</summary>
        /// <param name="value">Value to set</param>
        protected override void SetValue(object value)
        {
            var ctxt = TransactionContext;
            if (ctxt == null)
                throw new InvalidOperationException("No transaction context available for TransactionPropertyNode");

            // Due to use of anonymous lambda we can't call directly into the base SetValue
            ctxt.DoTransaction(() => SetValueInternal(value), "Edit Property".Localize());
        }

        private void SetValueInternal(object value)
        {
            base.SetValue(value);
        }

        private void ResetValueInternal()
        {
            base.ResetValue();
        }

        private readonly WeakReference m_contextRef;
    }
}
