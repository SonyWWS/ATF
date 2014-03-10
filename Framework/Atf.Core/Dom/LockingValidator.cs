//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Validator that tracks locked data in the DOM. Checks are only made within validations, which
    /// are signaled by IValidationContexts within the DOM data.</summary>
    public class LockingValidator : Validator
    {
        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            m_lockingContext = this.Cast<ILockingContext>(); // required ILockingContext

            // receive notification before attribute changes, to handle changes to lock state
            DomNode.AttributeChanging += OnAttributeChanging;

            base.OnNodeSet();
        }

        /// <summary>
        /// Performs custom actions on validation Beginning events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnBeginning(object sender, EventArgs e)
        {
            m_modified = new HashSet<DomNode>();
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnding(object sender, EventArgs e)
        {
            try
            {
                HashSet<DomNode> knownUnlocked = new HashSet<DomNode>();
                List<DomNode> discoveredUnlocked = new List<DomNode>();
                foreach (DomNode modified in m_modified)
                {
                    foreach (DomNode node in modified.Lineage)
                    {
                        if (knownUnlocked.Contains(node))
                            break;
                        if (m_lockingContext.IsLocked(node))
                            throw new InvalidTransactionException("item is locked");
                        discoveredUnlocked.Add(node);
                    }

                    foreach (DomNode node in discoveredUnlocked)
                        knownUnlocked.Add(node);

                    discoveredUnlocked.Clear();
                }
            }
            finally
            {
                m_modified = null;
            }
        }

        /// <summary>
        /// Performs custom actions on validation Cancelled events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnCancelled(object sender, EventArgs e)
        {
            m_modified = null;
        }

        // we allow DOM nodes to be locked and unlocked by tracking when this changes, and
        //  not considering such changes to be modifications

        /// <summary>
        /// Performs custom actions on attribute changing events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected virtual void OnAttributeChanging(object sender, AttributeEventArgs e)
        {
            if (Validating)
            {
                m_locked = m_lockingContext.IsLocked(e.DomNode);
            }
        }
        private bool m_locked;

        /// <summary>
        /// Performs custom actions after an attribute in the DOM subtree changes</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute change event args</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (Validating)
            {
                bool locked = m_lockingContext.IsLocked(e.DomNode);
                if (m_locked == locked) // lock state hasn't changed
                {
                    m_modified.Add(e.DomNode);
                }
            }
        }

        /// <summary>
        /// Performs custom actions after a child is inserted into the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_modified.Add(e.Child);
        }

        /// <summary>
        /// Performs custom actions after a child is removed from the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildRemoved(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_modified.Add(e.Child);
        }

        private ILockingContext m_lockingContext;
        private HashSet<DomNode> m_modified;
    }
}
