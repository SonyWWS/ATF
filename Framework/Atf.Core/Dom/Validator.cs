//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Abstract base class for validators which need to track all validation events in
    /// their sub-tree</summary>
    /// <remarks>Derived classes should throw InvalidTransactionException if the DomNode
    /// is considered to be invalid in the following methods:
    ///     OnBeginning
    ///     OnEnding
    ///     OnEnded
    /// 
    /// In these DOM-change notification methods, only do your validation logic if Validating
    /// is true, because otherwise your code might end up throwing InvalidTransactionException
    /// during the transaction unrolling stage and the 2nd InvalidTransactionException will
    /// cause the app to crash. http://forums.ship.scea.com/jive/thread.jspa?messageID=54120
    /// Also, it is more dangerous to throw InvalidTransactionException in these methods because
    /// the DOM has already been changed, but not necessarily all of the listeners have gotten
    /// the notification. For example, if TransactionContext hasn't received the ChildRemoved
    /// event yet and you throw an exception, then that event won't be undone.
    ///     AddNode
    ///     RemoveNode
    ///     OnAttributeChanged
    ///     OnChildInserted
    ///     OnChildRemoved
    ///     
    /// Considering throwing InvalidTransactionException in response to these events or in these
    /// methods:
    ///     OnNodeSet
    ///     DomNode.ChildInserting
    ///     DomNode.ChildRemoving
    ///     DomNode.AttributeChanging
    /// </remarks>
    public abstract class Validator : Observer
    {
        /// <summary>
        /// Gets a value indicating if a validation context in the subtree is validating</summary>
        /// <remarks>This almost certainly should be used to determine whether DOM change events
        /// should be checked. That is, if validation is being done in OnChildInserted or
        /// OnAttributeChanged, for example, check if Validating is true before running
        /// the validation logic. http://forums.ship.scea.com/jive/thread.jspa?messageID=54120 </remarks>
        public bool Validating
        {
            get { return m_validating; }
        }

        /// <summary>
        /// Performs custom actions on validation Beginning events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected virtual void OnBeginning(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected virtual void OnEnding(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on validation Ended events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected virtual void OnEnded(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions on validation Cancelled events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected virtual void OnCancelled(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM subtree</summary>
        /// <param name="node">Added node</param>
        /// <remarks>Method overrides must call the base method.</remarks>
        protected override void AddNode(DomNode node)
        {
            foreach (IValidationContext validationContext in node.AsAll<IValidationContext>())
            {
                validationContext.Beginning += validationContext_Beginning;
                validationContext.Ending += validationContext_Ending;
                validationContext.Ended += validationContext_Ended;
                validationContext.Cancelled += validationContext_Cancelled;
            }
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from the DOM subtree</summary>
        /// <param name="node">Removed node</param>
        /// <remarks>Method overrides must call the base method.</remarks>
        protected override void RemoveNode(DomNode node)
        {
            foreach (IValidationContext validationContext in node.AsAll<IValidationContext>())
            {
                validationContext.Beginning -= validationContext_Beginning;
                validationContext.Ending -= validationContext_Ending;
                validationContext.Ended -= validationContext_Ended;
                validationContext.Cancelled -= validationContext_Cancelled;
            }
        }

        private void validationContext_Beginning(object sender, EventArgs e)
        {
            m_validating = true;

            OnBeginning(sender, e);
        }

        private void validationContext_Ending(object sender, EventArgs e)
        {
            OnEnding(sender, e);

            m_validating = false;
        }

        private void validationContext_Ended(object sender, EventArgs e)
        {
            m_validating = false;

            OnEnded(sender, e);
        }

        private void validationContext_Cancelled(object sender, EventArgs e)
        {
            m_validating = false;

            OnCancelled(sender, e);
        }

        private bool m_validating;
    }
}

