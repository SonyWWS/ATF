//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;

using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// Binding that adds validation rules to begin and end transactions</summary>
    public class TransactionBinding : Binding
    {
        /// <summary>
        /// Constructor</summary>
        public TransactionBinding()
        {
            m_core = new TransactionBindingCore(ValidationRules);
            UpdateSourceExceptionFilter = UpdateSourceExceptionFilterCallback;
            ValidatesOnDataErrors = true;
            ValidatesOnExceptions = true;
            Mode = BindingMode.TwoWay;
        }

        /// <summary>
        /// Constructor with an initial path to the binding source property</summary>
        /// <param name="path">Initial path to the binding source property</param>
        public TransactionBinding(string path)
            : base(path)
        {
            m_core = new TransactionBindingCore(ValidationRules);
            UpdateSourceExceptionFilter = UpdateSourceExceptionFilterCallback;
            ValidatesOnDataErrors = true;
            ValidatesOnExceptions = true;
            Mode = BindingMode.TwoWay;
        }

        /// <summary>
        /// Gets or sets the name of the transaction e.g. "Rename"</summary>
        public string Transaction
        {
            get { return m_core.Transaction; }
            set { m_core.Transaction = value; }
        }

        // On exceptions - ensure transaction is cancelled before forwarding the exception
        private object UpdateSourceExceptionFilterCallback(object bindExpression, Exception exception)
        {
            m_core.CancelTransaction();
            return exception;
        }

        private readonly TransactionBindingCore m_core;
    }

    /// <summary>
    /// Multibinding that adds validation rules to begin and end transactions</summary>
    public class TransactionMultiBinding : MultiBinding
    {
        /// <summary>
        /// Constructor that creates TransactionBindingCore object</summary>
        public TransactionMultiBinding()
        {
            m_core = new TransactionBindingCore(ValidationRules);
        }

        /// <summary>
        /// Gets or sets the name of the transaction e.g. "Rename"</summary>
        public string Transaction
        {
            get { return m_core.Transaction; }
            set { m_core.Transaction = value; }
        }

        private readonly TransactionBindingCore m_core;
    }

    /// <summary>
    /// Core class used in order to share functionality between TransactionMultiBinding 
    /// and TransactionBinding</summary>
    internal class TransactionBindingCore
    {
        private static IContextRegistry s_cachedContextRegistry;
        private ITransactionContext m_currentTransactionContext;

        /// <summary>
        /// Constructor</summary>
        /// <param name="rules">Validation rules collection</param>
        public TransactionBindingCore(ICollection<ValidationRule> rules)
        {
            rules.Add(new TransactionBeginEdit(this));
            rules.Add(new TransactionEndEdit(this));
        }

        /// <summary>
        /// Gets or sets the name of the transaction e.g. "Rename"</summary>
        public string Transaction { get; set; }

        /// <summary>
        /// Cancels transaction</summary>
        public void CancelTransaction()
        {
            if (m_currentTransactionContext != null)
            {
                m_currentTransactionContext.Cancel();
                m_currentTransactionContext = null;
            }
        }

        private void BeginTransaction()
        {
            var context = GetCurrentTransactionContext();
            if (context != null && !context.InTransaction)
            {
                m_currentTransactionContext = context;
                context.Begin(Transaction);
            }
        }

        private void EndTransaction()
        {
            if (m_currentTransactionContext != null)
            {
                if (m_currentTransactionContext.InTransaction)
                    m_currentTransactionContext.End();
                m_currentTransactionContext = null;
            }
        }

        /// <summary>
        /// Uses static access to the Composer to try and get the application context registry.
        /// If this succeeds, it caches it (assumes that the IContextRegistry never changes).</summary>
        /// <returns>The active ITransactionContext or null</returns>
        private static ITransactionContext GetCurrentTransactionContext()
        {
            // It is assumed that the IContextRegistry never changes
            if (s_cachedContextRegistry == null)
            {
                var composer = Composer.Current;
                if (composer != null)
                {
                    s_cachedContextRegistry = composer.Container.GetExportedValueOrDefault<IContextRegistry>();
                }
            }

            if (s_cachedContextRegistry != null)
            {
                return s_cachedContextRegistry.GetActiveContext<ITransactionContext>();
            }

            return null;
        }

        private class TransactionBeginEdit : ValidationRule
        {
            public TransactionBeginEdit(TransactionBindingCore owner)
                : base(ValidationStep.RawProposedValue, false)
            {
                m_owner = owner;
            }

            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
            {
                m_owner.BeginTransaction();
                return ValidationResult.ValidResult;
            }

            private readonly TransactionBindingCore m_owner;

        }

        private class TransactionEndEdit : ValidationRule
        {
            public TransactionEndEdit(TransactionBindingCore owner)
                : base(ValidationStep.UpdatedValue, false)
            {
                m_owner = owner;
            }

            public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
            {
                m_owner.EndTransaction();
                return ValidationResult.ValidResult;
            }

            private readonly TransactionBindingCore m_owner;
        }
    }
}
