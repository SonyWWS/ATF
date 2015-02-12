//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter that tracks changes to transitions and updates their routing during validation.
    /// Update transitions on Ending event are part of the transactions themselves, 
    /// then validate all sub-graphs in the current document on Ended event. Requires
    /// Sce.Atf.Dom.ReferenceValidator to be available on the adapted DomNode.</summary>
    class CircuitValidator : Sce.Atf.Controls.Adaptable.Graphs.CircuitValidator
    {

        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            ValidateNodes();
        }


        /// <summary>
        /// Gets module label attribute</summary>
        protected override AttributeInfo ElementLabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets pin name attribute</summary>
        protected override AttributeInfo PinNameAttributeAttribute
        {
            get { return Schema.pinType.nameAttribute; }
        }

        /// <summary>
        /// Performs custom actions on validation Ended events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnded(object sender, EventArgs e)
        {
            base.OnEnded(sender, e);
            ValidateNodes();
        }

        private void ValidateNodes()
        {
            //// pseudo code to update element's IsValid 
            //foreach (var node in DomNode.Subtree)
            //{
            //    var element = node.As<Element>();
            //    if (element != null)
            //        element.ElementInfo.IsValid = !string.IsNullOrEmpty(element.Name);
            //}
        }
    }
}
