//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;

namespace Sce.Atf.Dom
{
    /// <summary>
    /// Validator that checks DOM changes against attribute and child rules contained in metadata.
    /// Checks are only made within validations, which are signaled by IValidationContexts within
    /// the DOM data. InvalidTransactionExceptions are thrown, if necessary, in the Ended event
    /// of the IValidationContext.</summary>
    public class DataValidator : Validator
    {
        /// <summary>
        /// Performs custom actions after an attribute in the DOM subtree changes</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute change event args</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (Validating)
                m_attributeChanges[e.AttributeInfo] = e.NewValue;
        }

        /// <summary>
        /// Performs custom actions after a child is inserted into the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_childChanges.Add(new Pair<Pair<DomNode, DomNode>, ChildInfo>(new Pair<DomNode, DomNode>(e.Parent, e.Child), e.ChildInfo));
        }

        /// <summary>
        /// Performs custom actions after a child is removed from the DOM subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildRemoved(object sender, ChildEventArgs e)
        {
            if (Validating)
                m_childChanges.Add(new Pair<Pair<DomNode, DomNode>, ChildInfo>(new Pair<DomNode, DomNode>(e.Parent, e.Child), e.ChildInfo));
        }

        /// <summary>
        /// Performs custom actions after validation finished</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute event args</param>
        protected override void OnEnded(object sender, System.EventArgs e)
        {
            foreach (KeyValuePair<AttributeInfo, object> keyValuePair in m_attributeChanges)
            {
                AttributeInfo info = keyValuePair.Key;
                object newValue = keyValuePair.Value;
                if (!info.Validate(newValue))
                    throw new InvalidTransactionException("invalid attribute value");
            }
            m_attributeChanges.Clear();

            foreach (Pair<Pair<DomNode, DomNode>, ChildInfo> pair in m_childChanges)
            {
                DomNode parent = pair.First.First;
                DomNode child = pair.First.Second;
                ChildInfo info = pair.Second;
                if (!info.Validate(parent, child))
                    throw new InvalidTransactionException("invalid child removal or insertion");
            }
            m_childChanges.Clear();
        }

        /// <summary>
        /// Performs custom actions when validation cancelled</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute event args</param>
        protected override void OnCancelled(object sender, System.EventArgs e)
        {
            m_childChanges.Clear();
            m_attributeChanges.Clear();
        }

        //pairs of parent and child; todo: use the Tuple in .Net 4.0
        private HashSet<Pair<Pair<DomNode, DomNode>,ChildInfo>> m_childChanges =
            new HashSet<Pair<Pair<DomNode, DomNode>,ChildInfo>>();

        //pairs of DomNode and its attribute, with the new value
        private Dictionary<AttributeInfo, object> m_attributeChanges =
            new Dictionary<AttributeInfo, object>();
    }
}
