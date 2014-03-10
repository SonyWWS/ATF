//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adapts a DOM node to an ITransactionContext</summary>
    public class TransactionContext : DomNodeAdapter, ITransactionContext, IValidationContext
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;

            base.OnNodeSet();
        }

        #region ITransactionContext, IValidationContext Members

        /// <summary>
        /// Begins a transaction with the given name</summary>
        /// <param name="transactionName">Transaction name</param>
        /// <remarks>Throws an InvalidOperationException if another transaction is in
        /// progress</remarks>
        public virtual void Begin(string transactionName)
        {
            // If there is a transaction in progress, throw the exception.
            if (InTransaction)
                throw new InvalidOperationException("already in transaction");

            m_transactionCancelled = false; 

            // Client code expects the transaction name to be available for listeners to the Beginning event.
            m_transactionName = transactionName;

            // Perform custom operations, such as closing an existing transaction.
            OnBeginning();
            Beginning.Raise(this, EventArgs.Empty);

            // Setting this indicates that a transaction is in progress; InTransaction will be true.
            if (!m_transactionCancelled)
                m_transactionOperations = new List<Operation>();
        }

        /// <summary>
        /// Event that is raised before a transaction begins</summary>
        public event EventHandler Beginning;

        /// <summary>
        /// Performs custom actions before a transaction begins</summary>
        protected virtual void OnBeginning()
        {
        }

        /// <summary>
        /// Cancels the current transaction</summary>
        public virtual void Cancel()
        {
            if (InTransaction)
            {
                OnCancelled();
                Cancelled.Raise(this, EventArgs.Empty);

                // rollback in reverse order
                for (int i = m_transactionOperations.Count - 1; i >= 0; i--)
                    m_transactionOperations[i].Undo();

                m_transactionOperations = null;
                m_transactionName = null;
            }
            m_transactionCancelled = true;
        }

        /// <summary>
        /// Event that is raised after a transaction has been cancelled</summary>
        public event EventHandler Cancelled;

        /// <summary>
        /// Performs custom actions after a transaction is cancelled</summary>
        protected virtual void OnCancelled()
        {
        }

        /// <summary>
        /// Ends the current transaction</summary>
        public virtual void End()
        {
            if (InTransaction)
            {
                OnEnding();
                Ending.Raise(this, EventArgs.Empty);

                OnEnded();
                Ended.Raise(this, EventArgs.Empty);

                // transaction operations are assumed to be available by the Ended listeners like HistoryContext
                m_transactionOperations = null;
                m_transactionName = null;
            }
        }

        /// <summary>
        /// Event that is raised before a transaction ends</summary>
        public event EventHandler Ending;

        /// <summary>
        /// Performs custom actions before a transaction ends</summary>
        protected virtual void OnEnding()
        {
        }

        /// <summary>
        /// Event that is raised after a transaction ends</summary>
        public event EventHandler Ended;

        /// <summary>
        /// Performs custom actions after a transaction ends</summary>
        protected virtual void OnEnded()
        {
        }

        /// <summary>
        /// Gets a value indicating if a transaction is in progress</summary>
        public virtual bool InTransaction
        {
            get { return m_transactionOperations != null; }
        }

        #endregion

        /// <summary>
        /// Gets or sets a value indicating if all data changes require a transaction</summary>
        public bool RequireTransactions
        {
            get { return m_requireTransactions; }
            set { m_requireTransactions = value; }
        }

        /// <summary>
        /// Gets operations representing the changes made by the transaction. Is valid after
        /// the Beginning event.</summary>
        public IList<Operation> TransactionOperations
        {
            get { return m_transactionOperations; }
        }

        /// <summary>
        /// Gets the name of the current transaction. Is valid during all events--Beginning,
        /// Cancelled, Ending, and Ended. Is null otherwise.</summary>
        public string TransactionName
        {
            get { return m_transactionName; }
        }

        /// <summary>
        /// Adds an operation to the list representing the changes made by the transaction</summary>
        /// <param name="operation">Operation to add</param>
        public virtual void AddOperation(Operation operation)
        {
            m_transactionOperations.Add(operation);
        }

        /// <summary>
        /// Abstract base class for operations in a transaction</summary>
        public abstract class Operation
        {
            /// <summary>
            /// Does the transaction operation</summary>
            public abstract void Do();

            /// <summary>
            /// Rolls back the transaction operation</summary>
            public abstract void Undo();
        }

        /// <summary>
        /// Operation to change an attribute value</summary>
        public class AttributeChangedOperation : Operation
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="e">Event args</param>
            public AttributeChangedOperation(AttributeEventArgs e)
            {
                m_node = e.DomNode;
                m_attributeInfo = e.AttributeInfo;
                m_oldValue = e.OldValue;
                m_newValue = e.NewValue;
            }

            /// <summary>
            /// Does the transaction operation</summary>
            public override void Do()
            {
                m_node.SetAttribute(m_attributeInfo, m_newValue);
            }

            /// <summary>
            /// Rolls back the transaction operation</summary>
            public override void Undo()
            {
                m_node.SetAttribute(m_attributeInfo, m_oldValue);
            }

            /// <summary>
            /// Gets the DomNode for the attribute being changed</summary>
            public DomNode DomNode
            {
                get { return m_node; }
            }

            /// <summary>
            /// Gets the attribute info for the attribute being changed</summary>
            public AttributeInfo AttributeInfo
            {
                get { return m_attributeInfo; }
            }

            /// <summary>
            /// Gets and sets the new value</summary>
            public object NewValue
            {
                get { return m_newValue; }
                set { m_newValue = value; }
            }

            private readonly DomNode m_node;
            private readonly AttributeInfo m_attributeInfo;
            private readonly object m_oldValue;
            private object m_newValue;
        }

        /// <summary>
        /// Operation to insert a child node</summary>
        public class ChildInsertedOperation : Operation
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="e">Event args</param>
            public ChildInsertedOperation(ChildEventArgs e)
            {
                m_parent = e.Parent;
                m_child = e.Child;
                m_childInfo = e.ChildInfo;
                m_index = e.Index;
            }

            /// <summary>
            /// Does the transaction operation</summary>
            public override void Do()
            {
                if (m_childInfo.IsList)
                {
                    IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
                    childList.Insert(m_index, m_child);
                }
                else
                {
                    m_parent.SetChild(m_childInfo, m_child);
                }
            }

            /// <summary>
            /// Rolls back the transaction operation</summary>
            public override void Undo()
            {
                if (m_childInfo.IsList)
                {
                    IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
                    childList.RemoveAt(m_index);
                }
                else
                {
                    m_parent.SetChild(m_childInfo, null);
                }
            }

            private readonly DomNode m_parent;
            private readonly ChildInfo m_childInfo;
            private readonly DomNode m_child;
            private readonly int m_index;
        }

        /// <summary>
        /// Operation to remove a child node</summary>
        public class ChildRemovedOperation : Operation
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="e">Event args</param>
            public ChildRemovedOperation(ChildEventArgs e)
            {
                m_parent = e.Parent;
                m_child = e.Child;
                m_childInfo = e.ChildInfo;
                m_index = e.Index;
            }

            /// <summary>
            /// Constructor</summary>
            /// <param name="parent"></param>
            /// <param name="child"></param>
            public ChildRemovedOperation(DomNode parent, DomNode child)
            {
                m_parent = parent;
                m_child = child;
                m_childInfo = child.ChildInfo;
                m_index = m_childInfo.IsList ? parent.GetChildList(m_childInfo).IndexOf(child) : 0;
            }

            /// <summary>
            /// Does the transaction operation</summary>
            public override void Do()
            {
                if (m_childInfo.IsList)
                {
                    IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
                    childList.RemoveAt(m_index);
                }
                else
                {
                    m_parent.SetChild(m_childInfo, null);
                }
            }

            /// <summary>
            /// Rolls back the transaction operation</summary>
            public override void Undo()
            {
                if (m_childInfo.IsList)
                {
                    IList<DomNode> childList = m_parent.GetChildList(m_childInfo);
                    childList.Insert(m_index, m_child);
                }
                else
                {
                    m_parent.SetChild(m_childInfo, m_child);
                }
            }

            private readonly DomNode m_parent;
            private readonly ChildInfo m_childInfo;
            private readonly DomNode m_child;
            private readonly int m_index;
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            if (CheckTransaction())
                AddOperation(new AttributeChangedOperation(e));
        }

        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            if (CheckTransaction())
                AddOperation(new ChildInsertedOperation(e));
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (CheckTransaction())
                AddOperation(new ChildRemovedOperation(e));
        }

        private bool CheckTransaction()
        {
            bool inTransaction = InTransaction;
            if (!inTransaction && m_requireTransactions)
            {
                throw new InvalidOperationException("data changed outside transaction");
            }
            return inTransaction;
        }

        private string m_transactionName;
        private List<Operation> m_transactionOperations;
        private bool m_requireTransactions;
        private bool m_transactionCancelled;
    }
}
