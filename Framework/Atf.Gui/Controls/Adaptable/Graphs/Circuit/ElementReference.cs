//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;


using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapter for a reference instance of an element template.</summary>
    abstract public class ElementReference : Element, IReference<Element>, IReference<DomNode>
    {
        /// <summary>
        /// Gets the unique identifier reference attribute.</summary>
        /// <value>
        /// The unique identifier reference attribute</value>
        protected abstract AttributeInfo GuidRefAttribute { get; }

        /// <summary>
        /// Gets and sets a globally unique identifier (GUID) that represents this template.</summary>
        /// <value>
        /// The template</value>
        public virtual Template Template
        {
            get
            {
                return GetReference<Template>(GuidRefAttribute);

            }
            set
            {
                SetReference(GuidRefAttribute, value.DomNode);
            }
        }

        #region IReference<DomNode>  memebers

        bool IReference<DomNode>.CanReference(DomNode item)
        {
            return item.Is<Element>();
        }

        DomNode IReference<DomNode>.Target
        {
            get { return Template.Target; }
            set
            {
                throw new InvalidOperationException("The group template determines the target");
            }
        }

        #endregion

        #region IReference<Element>  memebers

        bool IReference<Element>.CanReference(Element item)
        {
            return true;
        }

        Element IReference<Element>.Target
        {
            get { return Template.Target.As<Element>(); }
            set
            {
                throw new InvalidOperationException("The group template determines the target");
            }
        }

        #endregion

        /// <summary>
        /// Gets the referenced Element</summary>
        public Element Element
        {
            get { return Template.Target.As<Element>(); }
        }
        /// <summary>
        /// Gets the ICircuitElementType of the module instance</summary>
        public override ICircuitElementType Type
        {
            get { return Element.Type; }
        }


        /// <summary>
        /// Gets the list of input pins for this element type; the list is considered
        /// to be read-only.</summary>
        public IList<ICircuitPin> Inputs
        {
            get { return Element.Type.Inputs; }
        }

        /// <summary>
        /// Gets the list of output pins for this element type; the list is considered
        /// to be read-only.</summary>
        public IList<ICircuitPin> Outputs
        {
            get { return Element.Type.Outputs; }
        }

        /// <summary>
        /// Gets input pin for given pin index.</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Input pin for given pin index</returns>
        public override ICircuitPin InputPin(int pinIndex)
        {
            return Element.Type.GetInputPin(pinIndex);            
        }

        /// <summary>
        /// Gets output pin for given pin index.</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Output pin for given pin index</returns>
        public override ICircuitPin OutputPin(int pinIndex)
        {
            return Element.Type.GetOutputPin(pinIndex);            
        }

        /// <summary>
        /// Finds the element and pin that matched the pin target for this circuit container.</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            if (pinTarget.InstancingNode != DomNode)
                return new Pair<Element, ICircuitPin>(); // look no farthur
            var result = Element.MatchPinTarget(pinTarget, inputSide);
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
