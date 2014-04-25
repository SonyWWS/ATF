//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Adapter for an instance of a module template</summary>
    public class ModuleInstance : Module,IReference<Module>, IReference<DomNode>
    {
        #region IReference members

        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        /// <remarks>This method should never throw any exceptions</remarks>
        public bool CanReference(Module item)
        {
            return true;
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        /// <remarks>Callers should always check CanReference before setting this property.
        /// It is up to the implementer to decide whether null is an acceptable value and whether to
        /// throw an exception if the specified value cannot be targeted.</remarks>
        public Module Target
        {
            get { return GetReference<Module>(RefAttribute); }
            set { SetReference(RefAttribute, value); }
        }

        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        /// <remarks>This method should never throw any exceptions</remarks>
        bool IReference<DomNode>.CanReference(DomNode item)
        {
            return item.Is<Module>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        /// <remarks>Callers should always check CanReference before setting this property.
        /// It is up to the implementer to decide whether null is an acceptable value and whether to
        /// throw an exception if the specified value cannot be targeted.</remarks>
        DomNode IReference<DomNode>.Target
        {
            get { return GetReference<DomNode>(RefAttribute); }
            set { SetReference(RefAttribute, value); }
        }

        #endregion

 
        /// <summary>
        /// Gets the ICircuitElementType of the module instance</summary>
        public override ICircuitElementType Type
        {
            get { return Target.Type; }
        }

        /// <summary>
        /// Gets the name attribute for module instance</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.moduleType.nameAttribute; }
        }

        /// <summary>
        /// Gets the label attribute for module instance</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets the x-coordinate position attribute for module instance</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.moduleType.xAttribute; }
        }

        /// <summary>
        /// Gets the y-coordinate position attribute for module instance</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.moduleType.yAttribute; }
        }

        /// <summary>
        /// Gets the visible attribute for module instance</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.moduleType.visibleAttribute; }
        }

        private AttributeInfo RefAttribute
        {
            get { return Schema.moduleTemplateRefType.typeRefAttribute; }
        }

        // ICircuitElementType
        /// <summary>
        /// Gets desired interior size, in pixels, of this element type</summary>
        public Size InteriorSize
        {
            get { return Target.Type.InteriorSize; }
        }

        /// <summary>
        /// Gets image to draw for this element type</summary>
        public Image Image
        {
            get { return Target.Type.Image; }
        }

        /// <summary>
        /// Gets the list of input pins for this element type; the list is considered
        /// to be read-only</summary>
        public IList<ICircuitPin> Inputs
        {
            get { return Target.Type.Inputs; }
        }

        /// <summary>
        /// Gets the list of output pins for this element type; the list is considered
        /// to be read-only</summary>
        public IList<ICircuitPin> Outputs
        {
            get { return Target.Type.Outputs; }
        }

        /// <summary>
        /// Finds the element and pin that matched the pin target for this circuit container</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            if (pinTarget.InstancingNode != DomNode)
                return new Pair<Element, ICircuitPin>(); // look no farthur
            var result = Target.Cast<Module>().MatchPinTarget(pinTarget, inputSide);
            if (result.First != null)
                result.First = this;
            return result;
        }

        /// <summary>
        /// Finds the element and pin that fully matched the pin target for this circuit container, 
        /// including the template instance node</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public override Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            return MatchPinTarget(pinTarget, inputSide);
        }
    }
}
