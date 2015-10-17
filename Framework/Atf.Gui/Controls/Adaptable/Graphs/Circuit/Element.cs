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
        /// Gets the optional AttributeInfo for the original GUID of template 
        /// if this module is a copy-instance of a template(and nothing else) </summary>
        protected virtual AttributeInfo SourceGuidAttribute
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the optional AttributeInfo for storing whether or not unconnected
        /// pins should be displayed.</summary>
        protected virtual AttributeInfo ShowUnconnectedPinsAttribute
        {
            get { return null; }
        }

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
        /// Gets the CircuitElementInfo for this circuit element, which specifies additional options</summary>
        public CircuitElementInfo ElementInfo
        {
            get { return m_elementInfo; }
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
        /// <returns><c>True</c> if the element has the given input pin</returns>
        public virtual bool HasInputPin(ICircuitPin pin)
        {
            if (this.Is<Group>())
                return this.Cast<Group>().HasInputPin(pin);
            return Type.Inputs.Contains(pin);       
        }

        /// <summary>
        /// Tests if the element has a given output pin.</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns><c>True</c> if the element has the given output pin</returns>
        public virtual bool HasOutputPin(ICircuitPin pin)
        {
            if (this.Is<Group>())
                return this.Cast<Group>().HasOutputPin(pin);
            return Type.Outputs.Contains(pin);

        }

        /// <summary>
        /// Gets the input pin for the given pin index.</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Input pin for pin index</returns>
        public virtual ICircuitPin InputPin(int pinIndex)
        {
            return Type.Inputs[pinIndex];
        }

        /// <summary>
        /// Gets the output pin for the given pin index.</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Output pin for pin index</returns>
        public virtual ICircuitPin OutputPin(int pinIndex)
        {
            return Type.Outputs[pinIndex];
        }


        /// <summary>
        /// Gets a read-only list of all the input pins for this element, including hidden pins
        /// (if this is a Group).</summary>
        public virtual IEnumerable<ICircuitPin> AllInputPins
        {
            get { return Type.Inputs; }
        }

        /// <summary>
        /// Gets a read-only list of all the output pins for this element, including hidden pins
        /// (if this is a Group).</summary>
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
            if (pinTarget != null &&  pinTarget.LeafDomNode == DomNode) // an element must be a leaf node in a circuit hierarchy
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
        /// Gets or sets original GUID of template if this module is a copy-instance of a template</summary>
        public Guid SourceGuid
        {
            get
            {
                if (SourceGuidAttribute == null)
                    return Guid.Empty;
                var guidValue = DomNode.GetAttribute(SourceGuidAttribute) as string;
                if (string.IsNullOrEmpty(guidValue))
                    return Guid.Empty;
                return new Guid(guidValue);
            }
            set
            {
                if (SourceGuidAttribute != null)
                    DomNode.SetAttribute(SourceGuidAttribute, value.ToString());
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

        /// <summary>
        /// Performs one-time initialization when this adapter's DomNode property is set.
        /// The DomNode property is only ever set once for the lifetime of this adapter.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();

            m_elementInfo = CreateElementInfo();

            if (ShowUnconnectedPinsAttribute != null)
                m_elementInfo.ShowUnconnectedPins = GetAttribute<bool>(ShowUnconnectedPinsAttribute);

            m_elementInfo.PropertyChanged += (sender, args) =>
            {
                if (!m_syncingElementInfo)
                {
                    m_syncingElementInfo = true;
                    try
                    {
                        if (ShowUnconnectedPinsAttribute != null)
                            SetAttribute(ShowUnconnectedPinsAttribute, m_elementInfo.ShowUnconnectedPins);
                    }
                    finally
                    {
                        m_syncingElementInfo = false;
                    }
                }
            };

            DomNode.AttributeChanged += (sender, args) =>
            {
                if (!m_syncingElementInfo && args.DomNode == DomNode)
                {
                    m_syncingElementInfo = true;
                    try
                    {
                        if (args.AttributeInfo.Equivalent(ShowUnconnectedPinsAttribute))
                            m_elementInfo.ShowUnconnectedPins = (bool) args.NewValue;
                    }
                    finally
                    {
                        m_syncingElementInfo = false;
                    }
                }
            };
        }

        /// <summary>
        /// Creates the circuit element information object</summary>
        /// <returns></returns>
        /// <remarks>This is called just once, after the DomNode property has been set.</remarks>
        protected virtual CircuitElementInfo CreateElementInfo()
        {
            return new CircuitElementInfo();
        }

        private ICircuitElementType m_elementType;
        private Size m_size;
        private CircuitElementInfo m_elementInfo;
        private bool m_syncingElementInfo;
    }
}
