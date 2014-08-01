//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Dom;
using Sce.Atf.Rendering;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to circuit element, which is the base circuit element with pins.
    /// It maintains local name and bounds for faster
    /// circuit rendering during editing operations, such as dragging elements and wires.</summary>
    public abstract class Element : DomNodeAdapter, ICircuitElement, IVisible
    {
        // These protected abstract methods are conveniences, to help implement the corresponding
        //  public virtual methods, using the derived class's DOM-backed data. If a derived class
        //  stores the data differently or doesn't even use the DOM, it can have these methods
        //  return null and override the corresponding public properties to get/set the data.
        
        /// <summary>
        /// Gets the AttributeInfo for the Id property (and nothing else)</summary>
        protected abstract AttributeInfo NameAttribute { get; }

        /// <summary>
        /// Gets the AttributeInfo for the Name property (and nothing else)</summary>
        protected abstract AttributeInfo LabelAttribute { get; }

        /// <summary>
        /// Gets the AttributeInfo for the Position property (and nothing else)</summary>
        protected abstract AttributeInfo XAttribute { get; }

        /// <summary>
        /// Gets the AttributeInfo for the Position property (and nothing else)</summary>
        protected abstract AttributeInfo YAttribute { get; }

        /// <summary>
        /// Gets the AttributeInfo for the Visible property (and nothing else)</summary>
        protected abstract AttributeInfo VisibleAttribute { get; }

        /// <summary>
        /// Gets or sets the circuit element ID</summary>
        public virtual string Id
        {
            get { return GetAttribute<string>(NameAttribute); }
            set { SetAttribute(NameAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the user-visible name</summary>
        public virtual string Name
        {
            get { return GetAttribute<string>(LabelAttribute); }
            set { SetAttribute(LabelAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the position of the element</summary>
        public virtual Point Position
        {
            get
            {
                return new Point(
                    GetAttribute<int>(XAttribute),
                    GetAttribute<int>(YAttribute));
            }
            set
            {
                SetAttribute(XAttribute, value.X);
                SetAttribute(YAttribute, value.Y);
            }
        }

        /// <summary>
        /// Gets the circuit element type</summary>
        public virtual ICircuitElementType Type
        {
            get
            {
                ICircuitElementType result= null;
                if (DomNode.Is<ICircuitElement>()) // favor domNode direct support
                {
                    var circuitElement = DomNode.Cast<ICircuitElement>();
                    if (circuitElement != this) // the check is needed to prevent from self-loop
                        result = DomNode.Cast<ICircuitElement>().Type;
                }
                  
                if (result == null) // now try domNode type tag
                {
                    if (m_elementType == null)
                        m_elementType = DomNode.Type.GetTag<ICircuitElementType>();
                    result = m_elementType; 
                }
                Debug.Assert(result != null);
                return result;
            }
        }

        /// <summary>
        /// Gets level, or depth of the element </summary>
        public int Level
        {
            get { return DomNode.Ancestry.Count(); }
        }

        /// <summary>
        /// Tests if the element has a given input pin</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns>True iff the element has the given input pin</returns>
        public virtual bool HasInputPin(ICircuitPin pin)
        {
            if (this.Is<Group>())
                return this.Cast<Group>().HasInputPin(pin);
            return Type.Inputs.Contains(pin);       
        }

        /// <summary>
        /// Tests if the element has a given output pin</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns>True iff the element has the given output pin</returns>
        public virtual bool HasOutputPin(ICircuitPin pin)
        {
            if (this.Is<Group>())
                return this.Cast<Group>().HasOutputPin(pin);
            return Type.Outputs.Contains(pin);

        }

        /// <summary>
        /// Gets the input pin for the given pin index</summary>
        /// <param name="pinIndex"></param>
        /// <returns></returns>
        public virtual ICircuitPin InputPin(int pinIndex)
        {
            return Type.Inputs[pinIndex];
        }

        /// <summary>
        /// Gets the output pin for the given pin index</summary>
        /// <param name="pinIndex"></param>
        /// <returns></returns>
        public virtual ICircuitPin OutputPin(int pinIndex)
        {
            return Type.Outputs[pinIndex];
        }


        /// <summary>
        /// Gets all the input pins for this element</summary>
        public virtual IEnumerable<ICircuitPin> AllInputPins
        {
            get { return Type.Inputs; }
        }

        /// <summary>
        /// Gets all the output pins for this element</summary>
        public virtual IEnumerable<ICircuitPin> AllOutputPins
        {
            get { return Type.Outputs; }
        }

        /// <summary>
        /// Finds the element and pin that matched the pin target for this circuit container</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public virtual Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            var result = new Pair<Element, ICircuitPin>();
            if (pinTarget != null &&  pinTarget.LeafDomNode == DomNode) // an element must be a leaf node in a circut hiearchy
            {
                bool validPinIndex = inputSide
                                         ? pinTarget.LeafPinIndex < Type.Inputs.Count
                                         : pinTarget.LeafPinIndex < Type.Outputs.Count;

                if (validPinIndex)
                {
                    result.First = this;
                    result.Second = inputSide
                                        ? Type.Inputs[pinTarget.LeafPinIndex]
                                        : Type.Outputs[pinTarget.LeafPinIndex];
                }
            }
 
            return result;
        }

        /// <summary>
        /// Finds the element and pin that fully matched the pin target for this circuit container, 
        /// including the template instance node</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public virtual Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            return MatchPinTarget(pinTarget, inputSide);
        }

        #region IVisible Members

        /// <summary>
        /// Gets or sets whether the element is visible</summary>
        public virtual bool Visible
        {
            get
            {
                return VisibleAttribute == null || GetAttribute<bool>(VisibleAttribute);
            }
            set { SetAttribute(VisibleAttribute, value); }
        }

        #endregion


        /// <summary>
        /// Gets or sets the local bounds information, in world coordinates</summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return new Rectangle(Position, m_size);
            }
            set
            {
                SetAttribute(XAttribute, value.X);
                SetAttribute(YAttribute, value.Y);
                m_size = value.Size;
            }
        }

        /// <summary>
        /// Convert pin index to display order</summary>
        /// <param name="pinIndex">Pin index to convert</param>
        /// <param name="inputSide">Whether input side or not</param>
        /// <returns>Integer representing display order</returns>
        /// <remarks>Usually ICircuitPin.Index also indicates the display order. 
        /// Override this method if pin's index does not correspond to display order.</remarks>
        public virtual int PinDisplayOrder(int pinIndex, bool inputSide)
        {
            return pinIndex;
        }


        private ICircuitElementType m_elementType;
        private Size m_size;
    }
}
