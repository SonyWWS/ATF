//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Adds history to a TransactionContext. If the adapter can adapt the node to
    /// an ISelectionContext, the history includes selection changes.</summary>
    public class HistoryContext : TransactionContext, IHistoryContext
    {
        /// <summary>
        /// Constructor</summary>
        public HistoryContext()
        {
            History = new CommandHistory();
        }

        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            // The TransactionContext should subscribe to DOM events as soon as possible,
            //  before we get the ISelectionContext, because that could create other adapters
            //  which will get their OnNodeSet() called.
            base.OnNodeSet();

            m_selectionContext = this.As<ISelectionContext>(); // optional ISelectionContext
        }

        /// <summary>
        /// Gets or sets the context's history. Set the context's history to route
        /// history from this context onto the history of another, more global context.</summary>
        public CommandHistory History
        {
            get { return m_history; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (m_history != value)
                {
                    if (m_history != null)
                    {
                        m_history.CommandUndone -= history_CommandUndone;
                        m_history.DirtyChanged -= history_DirtyChanged;
                    }

                    m_history = value;

                    if (m_history != null)
                    {
                        m_history.CommandUndone += history_CommandUndone;
                        m_history.DirtyChanged += history_DirtyChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether transactions should be saved in the history</summary>
        public bool Recording
        {
            get { return m_recording; }
            set { m_recording = value; }
        }

        /// <summary>
        /// Gets whether the context is currently undoing or redoing operations</summary>
        public bool UndoingOrRedoing
        {
            get { return m_undoingOrRedoing; }
            internal set { m_undoingOrRedoing = value; }
        }

        /// <summary>
        /// Gets or sets the time interval during which operations are automatically combined into one undo/redo entry</summary>
        /// <remarks>The default value is half a second. Set this property to TimeSpan.Zero to disable this feature.</remarks>
        public TimeSpan PendingSetOperationLifetime
        {
            get { return m_pendingSetOperationLifetime; }
            set { m_pendingSetOperationLifetime = value; }
        }

        #region IHistoryContext Members

        /// <summary>
        /// Tests if there is a command that can be undone</summary>
        public virtual bool CanUndo
        {
            get { return m_history.CanUndo; }
        }

        /// <summary>
        /// Tests if there is a command that can be redone</summary>
        public virtual bool CanRedo
        {
            get { return m_history.CanRedo; }
        }

        /// <summary>
        /// Gets the description of the next undoable command</summary>
        public virtual string UndoDescription
        {
            get { return m_history.UndoDescription; }
        }

        /// <summary>
        /// Gets the name of the next redoable command</summary>
        public virtual string RedoDescription
        {
            get { return m_history.RedoDescription; }
        }

        /// <summary>
        /// Undoes the last "done" command</summary>
        public virtual void Undo()
        {
            var globalHistoryContext = DomNode.Lineage.FirstOrDefault(x => x.Is<GlobalHistoryContext>());
            try
            {
                m_undoingOrRedoing = true;
                // When sharing a global history, need to synchronize undoing/redoing status for participating historycontexts 
                // to prevent commands executed in undo/redo accidentally recorded again into the global history during undo/redo
                if (globalHistoryContext != null)
                    globalHistoryContext.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
                m_history.Undo();
            }
            finally
            {
                m_undoingOrRedoing = false;
                if (globalHistoryContext != null)
                    globalHistoryContext.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
            }
        }

        /// <summary>
        /// Redoes the last "undone" command</summary>
        public virtual void Redo()
        {
            var globalHistoryContext = DomNode.Lineage.FirstOrDefault(x => x.Is<GlobalHistoryContext>());
            try
            {
                m_undoingOrRedoing = true;
                if (globalHistoryContext != null)
                    globalHistoryContext.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
                m_history.Redo();
            }
            finally
            {
                m_undoingOrRedoing = false;
                if (globalHistoryContext != null)
                    globalHistoryContext.Cast<GlobalHistoryContext>().SynchronizeUndoRedoStatus(m_undoingOrRedoing);
            }
        }

        /// <summary>
        /// Gets and sets a value indicating if the command history is at its "clean" point.</summary>
        /// <remarks>Setting this property to false will set the "clean" point.  Setting
        /// it to true will force a "dirty" state until the next time Dirty is set to false.</remarks>
        public virtual bool Dirty
        {
            get { return m_history.Dirty; }
            set { m_history.Dirty = value; }
        }

        /// <summary>
        /// Event that is raised when the Dirty state changes</summary>
        public event EventHandler DirtyChanged;

        #endregion

        /// <summary>
        /// Performs custom actions before a transaction begins</summary>
        protected override void OnBeginning()
        {
            m_lastSelection = SnapshotSelection();

            base.OnBeginning();
        }

        /// <summary>
        /// Performs custom actions after a transaction ends</summary>
        protected override void  OnEnded()
        {
            if (m_undoingOrRedoing)
                return;
            if (!m_recording)
                return;

            DateTime now = DateTime.UtcNow;
            TimeSpan elapsed = now.Subtract(m_lastSetOperationTime);
            m_lastSetOperationTime = now;

            // remove pending set operations if too much time has elapsed
            if (elapsed > PendingSetOperationLifetime)
            {
                m_pendingChanges.Clear();
            }

            // if operations can be combined with pending set operations, combine and remove them
            IList<Operation> operations = TransactionOperations;
            int i = 0;
            while (i < operations.Count)
            {
                Operation operation = operations[i];
                var setOp = operation as AttributeChangedOperation;
                if (setOp != null)
                {
                    var id = new Pair<DomNode, AttributeInfo>(setOp.DomNode, setOp.AttributeInfo);
                    AttributeChangedOperation pendingSetOp;
                    if (m_pendingChanges.TryGetValue(id, out pendingSetOp))
                    {
                        pendingSetOp.NewValue = setOp.NewValue;
                        operations.RemoveAt(i);
                        continue; // don't increment i
                    }
                    else
                    {
                        m_pendingChanges.Add(id, setOp);
                    }
                }

                i++;
            }

            if (operations.Count > 0)
            {
                SetSelectionCommand setSelectionCommand = null;
                if (m_selectionContext != null)
                {
                    setSelectionCommand = new SetSelectionCommand(
                        m_selectionContext,
                        m_lastSelection,
                        SnapshotSelection());
                }

                m_history.Add(new TransactionCommand(this, TransactionName, operations.ToArray(), setSelectionCommand));
            }

            m_lastSelection = null;

            base.OnEnded();
        }

        private void history_CommandUndone(object sender, EventArgs e)
        {
            // clear any pending set operations
            m_pendingChanges.Clear();
        }

        private void history_DirtyChanged(object sender, EventArgs e)
        {
            DirtyChanged.Raise(this, e);
        }

        private object[] SnapshotSelection()
        {
            if (m_selectionContext != null)
                return
                    m_selectionContext.GetSelection<object>().ToArray();

            return EmptyArray<object>.Instance;
        }

        private class TransactionCommand : Command
        {
            public TransactionCommand(
                HistoryContext context,
                string name,
                Operation[] operations,
                SetSelectionCommand setSelectionCommand)

                : base(name)
            {
                m_context = context;
                m_operations = operations;
                m_setSelectionCommand = setSelectionCommand;
            }

            public override void Do()
            {

                m_context.DoTransaction(
                    delegate
                    {
                        foreach (Operation operation in m_operations)
                            operation.Do();
                    }, Description);

                if (m_setSelectionCommand != null)
                    m_setSelectionCommand.Do();
            }

            public override void Undo()
            {
                m_context.DoTransaction(
                    delegate
                    {
                        // undo in reverse order
                        for (int i = m_operations.Length - 1; i >= 0; i--)
                            m_operations[i].Undo();
                    }, Description);

                if (m_setSelectionCommand != null)
                    m_setSelectionCommand.Undo();
            }

            private readonly HistoryContext m_context;
            private readonly Operation[] m_operations;
            private readonly SetSelectionCommand m_setSelectionCommand;
        }

        // Dictionary holding pending set-attribute operations, so that rapid sequences of such
        //  operations can be combined into a single logical command for undo/redo
        private readonly Dictionary<Pair<DomNode, AttributeInfo>, AttributeChangedOperation> m_pendingChanges =
            new Dictionary<Pair<DomNode, AttributeInfo>, AttributeChangedOperation>();

        // Rather than use a timer, just record the time of attribute-set operations and combine
        //  if new set operations with any pending set operations. There isn't a leak problem with
        //  holding on to these set operations in a dictionary, since they are referenced anyway by
        //  some command on the command history; if the user undoes the command, we can clear the
        //  dictionary.
        private DateTime m_lastSetOperationTime;
        private TimeSpan m_pendingSetOperationLifetime = new TimeSpan(0, 0, 0, 0, 500); // 500 msec

        private CommandHistory m_history;
        private ISelectionContext m_selectionContext;

        private object[] m_lastSelection;
        private bool m_undoingOrRedoing;
        private bool m_recording = true;
    }
}
