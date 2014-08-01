//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Observes and collects the DOM changes in the attached DOM tree during transactions and
    /// then raises the final "cleaned-up" events after the transaction finishes. If a DOM change
    /// occurs outside a transaction, then the change is reported immediately. Cleaning-up means:
    /// 1) Multiple changes to the same DomNode attribute are collapsed into one event. 2) Changes
    /// to a DomNode after it is inserted or before it is removed are ignored.</summary>
    public class TransactionReporter : Validator
    {
        /// <summary>
        /// Event that is raised after an attribute is changed to a new value on this DomNode
        /// or any DomNode of the sub-tree, after the transaction has finished. If there is no
        /// transaction, then this event is raised immediately. Multiple changes to the same
        /// attribute during a transaction are combined to only show the last one.</summary>
        /// <remarks>Listeners should not change the DOM in response to this event, because that
        /// change will be outside of a transaction.</remarks>
        public event EventHandler<AttributeEventArgs> TransactionFinishedAttributeChanged;

        /// <summary>
        /// Event that is raised after a transaction has finished and if a DomNode was inserted
        /// during that transaction. If there is no transaction, then this event is raised immediately.</summary>
        /// <remarks>Listeners should not change the DOM in response to this event, because that
        /// change will be outside of a transaction.</remarks>
        public event EventHandler<ChildEventArgs> TransactionFinishedChildInserted;

        /// <summary>
        /// Event that is raised after a transaction has finished and if a DomNode was removed
        /// during that transaction. If there is no transaction, then this event is raised immediately.</summary>
        /// <remarks>Listeners should not change the DOM in response to this event, because that
        /// change will be outside of a transaction.</remarks>
        public event EventHandler<ChildEventArgs> TransactionFinishedChildRemoved;

        /// <summary>
        /// Raises the TransactionFinishedAttributeChanged event</summary>
        /// <param name="attributeEventArgs"></param>
        protected virtual void OnTransactionFinishedAttributeChanged(AttributeEventArgs attributeEventArgs)
        {
            TransactionFinishedAttributeChanged.Raise(this, attributeEventArgs);
        }

        /// <summary>
        /// Raises the TransactionFinishedChildInserted event</summary>
        /// <param name="childEventArgs"></param>
        protected virtual void OnTransactionFinishedChildInserted(ChildEventArgs childEventArgs)
        {
            TransactionFinishedChildInserted.Raise(this, childEventArgs);
        }

        /// <summary>
        /// Raises the TransactionFinishedChildRemoved event</summary>
        /// <param name="childEventArgs"></param>
        protected virtual void OnTransactionFinishedChildRemoved(ChildEventArgs childEventArgs)
        {
            TransactionFinishedChildRemoved.Raise(this, childEventArgs);
        }

        #region sealed methods; use the above methods and events instead

        protected sealed override void OnAttributeChanged(object sender, AttributeEventArgs attributeEventArgs)
        {
            if (m_inTransaction)
            {
                // First check if this attribute has changed on an inserted DomNode.
                foreach(DomNode ancestor in attributeEventArgs.DomNode.Lineage)
                    if (m_inserted.ContainsKey(ancestor))
                        return;

                // Now check if this attribute has already been changed.
                var domNodeAndAttributeInfo = new Pair<DomNode, AttributeInfo>(attributeEventArgs.DomNode, attributeEventArgs.AttributeInfo);
                int index;
                if (m_attributeChanges.TryGetValue(domNodeAndAttributeInfo, out index))
                {
                    // A previous event was raised for the same attribute on the same DomNode. Combine.
                    var oldEvent = (AttributeEventArgs)m_events[index].Second;
                    var newEvent = new AttributeEventArgs(
                        oldEvent.DomNode,
                        oldEvent.AttributeInfo,
                        oldEvent.OldValue,
                        attributeEventArgs.NewValue);
                    m_events[index] = new Pair<EventType, EventArgs>(EventType.AttributeChanged, newEvent);
                }
                else
                {
                    // This is the first time we've seen this attribute change on this DomNode.
                    m_attributeChanges.Add(domNodeAndAttributeInfo, m_events.Count);
                    // Record this attribute changed event.
                    m_events.Add(new Pair<EventType, EventArgs>(EventType.AttributeChanged, attributeEventArgs));
                }
            }
            else
            {
                OnTransactionFinishedAttributeChanged(attributeEventArgs);
            }
        }

        protected sealed override void OnChildInserted(object sender, ChildEventArgs childEventArgs)
        {
            if (m_inTransaction)
            {
                // Ignore if any ancestor was previously inserted.
                foreach(DomNode ancestor in childEventArgs.Parent.Lineage)
                    if (m_inserted.ContainsKey(ancestor))
                        return;

                m_inserted[childEventArgs.Child] = m_events.Count;
                // Record this child inserted event.
                m_events.Add(new Pair<EventType, EventArgs>(EventType.ChildInserted, childEventArgs));
            }
            else
                OnTransactionFinishedChildInserted(childEventArgs);
        }

        protected sealed override void OnChildRemoved(object sender, ChildEventArgs childEventArgs)
        {
            if (m_inTransaction)
            {
                // Clear out any attribute-changed events previously set for this DomNode (or any of its children).
                var dictionaryCleanup = new List<Pair<DomNode, AttributeInfo>>();
                foreach (DomNode removedNode in childEventArgs.Child.Subtree)
                {
                    foreach (var changedAttributePair in m_attributeChanges) //this search might be slow
                    {
                        if (changedAttributePair.Key.First == removedNode)
                        {
                            dictionaryCleanup.Add(changedAttributePair.Key);
                            m_events[changedAttributePair.Value] = new Pair<EventType, EventArgs>(EventType.None, null);
                        }
                    }
                    if (dictionaryCleanup.Count > 0)
                    {
                        foreach (var changedAttributePair in dictionaryCleanup)
                            m_attributeChanges.Remove(changedAttributePair);
                        dictionaryCleanup.Clear();
                    }
                }

                // Check if this DomNode was previously inserted. If so, clear the previous insertion
                //  event and ignore this new ChildRemoved event.
                int eventIndex;
                if (m_inserted.TryGetValue(childEventArgs.Child, out eventIndex))
                {
                    m_events[eventIndex] = new Pair<EventType, EventArgs>(EventType.None, null);
                    m_inserted.Remove(childEventArgs.Child);
                    return;
                }
                foreach (DomNode ancestor in childEventArgs.Parent.Lineage)
                    if (m_inserted.ContainsKey(ancestor))
                        return;

                // Record this child removed event.
                m_events.Add(new Pair<EventType, EventArgs>(EventType.ChildRemoved, childEventArgs));
            }
            else
                OnTransactionFinishedChildRemoved(childEventArgs);
        }

        protected sealed override void OnBeginning(object sender, EventArgs e)
        {
            ClearRecording();
            m_inTransaction = true;
        }

        protected sealed override void OnCancelled(object sender, EventArgs e)
        {
            ClearRecording();
            m_inTransaction = false;
        }

        protected sealed override void OnEnded(object sender, EventArgs e)
        {
            // Stop recording changes now, since the transaction has ended and we're about to report the changes.
            m_inTransaction = false;

            foreach (Pair<EventType, EventArgs> pair in m_events)
            {
                switch (pair.First)
                {
                    case EventType.AttributeChanged:
                        var attrArgs = (AttributeEventArgs)pair.Second;
                        if (!attrArgs.AttributeInfo.Type.AreEqual(attrArgs.OldValue, attrArgs.NewValue))
                            OnTransactionFinishedAttributeChanged(attrArgs);
                        break;
                    case EventType.ChildInserted:
                        OnTransactionFinishedChildInserted((ChildEventArgs)pair.Second);
                        break;
                    case EventType.ChildRemoved:
                        OnTransactionFinishedChildRemoved((ChildEventArgs)pair.Second);
                        break;
                    case EventType.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ClearRecording();
        }

        #endregion

        private void ClearRecording()
        {
            m_events.Clear();
            m_attributeChanges.Clear();
            m_inserted.Clear();
        }

        private enum EventType
        {
            AttributeChanged,
            ChildInserted,
            ChildRemoved,
            None //for when a DomNode is inserted and then removed
        }

        private class AttributeComparer : IEqualityComparer<Pair<DomNode, AttributeInfo>>
        {
            public bool Equals(Pair<DomNode, AttributeInfo> x, Pair<DomNode, AttributeInfo> y)
            {
                return
                    x.First.Equals(y.First) &&
                    x.Second.Equivalent(y.Second);
            }

            public int GetHashCode(Pair<DomNode, AttributeInfo> obj)
            {
                return
                    obj.First.GetHashCode() ^
                    obj.Second.GetEquivalentHashCode();
            }
        }

        //'m_inTransaction' is different than base.Validating because Validating is false after OnEnding() is called, but
        //  other validators (like the unique namers) will change the DOM in their OnEnding() methods. So, we
        //  need to keep track of when OnEnded() gets called.
        private bool m_inTransaction;

        // The recording of final events to be raised after the transaction finishes.
        private readonly List<Pair<EventType, EventArgs>> m_events = new List<Pair<EventType, EventArgs>>();
    
        // Map the DomNode and its AttributeInfo to the index of 'm_events' that contains the attribute changed event.
        private readonly Dictionary<Pair<DomNode, AttributeInfo>, int> m_attributeChanges =
            new Dictionary<Pair<DomNode, AttributeInfo>, int>(s_comparer);

        // Map newly inserted DomNodes (but not their children) to the index of m_events that contains the insertion event.
        private readonly Dictionary<DomNode,int> m_inserted = new Dictionary<DomNode, int>();

        private static readonly IEqualityComparer<Pair<DomNode, AttributeInfo>> s_comparer = new AttributeComparer();
    }
}
