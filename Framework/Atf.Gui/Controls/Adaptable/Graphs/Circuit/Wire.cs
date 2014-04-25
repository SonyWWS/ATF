//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapts DomNode to connection in a circuit</summary>
    public abstract class Wire : DomNodeAdapter, IGraphEdge<Element, ICircuitPin>
    {

        /// <summary>
        /// Gets label attribute on connection</summary>
        protected abstract AttributeInfo LabelAttribute { get; }
        /// <summary>
        /// Gets input module attribute for connection</summary>
        protected abstract AttributeInfo InputElementAttribute { get; }
        /// <summary>
        /// Gets output module attribute for connection</summary>
        protected abstract AttributeInfo OutputElementAttribute { get; }
        /// <summary>
        /// Gets input pin attribute for connection</summary>
        protected abstract AttributeInfo InputPinAttribute { get; }
        /// <summary>
        /// Gets output pin attribute for connection</summary>
        protected abstract AttributeInfo OutputPinAttribute { get; }


        /// <summary>
        /// Gets or sets the element whose output pin this wire connects to</summary>
        public virtual Element OutputElement
        {
            get { return GetReference<Element>(OutputElementAttribute); }
            set { SetReference(OutputElementAttribute, value); }
        }

        /// <summary>
        /// Gets or sets the output pin, i.e., pin on element that receives connection as output</summary>
        public virtual ICircuitPin OutputPin
        {
            get
            {
                int pinIndex = GetAttribute<int>(OutputPinAttribute);
                if (pinIndex >= OutputElement.Type.Outputs.Count)
                {
                    var edge = DomNode.Cast<Wire>();
                    var edgeName = "from " + edge.OutputElement.Name + " to " + edge.InputElement.Name;              
                    string message = string.Format("Edge {0} Output pin index {1}  out of range ", edgeName, pinIndex);
                    throw new IndexOutOfRangeException(message);
                }
                return OutputElement.Type.Outputs[pinIndex];
            }
            set
            {
                DomNode.SetAttribute(OutputPinAttribute, value.Index);
            }
        }

        /// <summary>
        /// Gets or sets the element whose input pin this wire connects to</summary>
        public virtual Element InputElement
        {
            get { return GetReference<Element>(InputElementAttribute); }
            set { SetReference(InputElementAttribute, value); }
        }

        /// <summary>
        /// Gets or sets input pin, i.e., pin on element that receives connection as input</summary>
        public virtual ICircuitPin InputPin
        {
            get
            {
                int pinIndex = GetAttribute<int>(InputPinAttribute);
                if (pinIndex >= InputElement.Type.Inputs.Count)
                {
                    var edge = DomNode.Cast<Wire>();
                    var edgeName = "from " + edge.OutputElement.Name + " to " + edge.InputElement.Name;
                    string message = string.Format("Edge {0} Input pin index {1}  out of range ", edgeName, pinIndex);
                    throw new IndexOutOfRangeException(message);
                }
                return InputElement.Type.Inputs[pinIndex];
            }
            set
            {
                DomNode.SetAttribute(InputPinAttribute, value.Index);
            }
        }

        /// <summary>
        /// Sets output pin for an element</summary>
        /// <param name="outputElement">Element</param>
        /// <param name="outputPin">Output pin</param>
        public virtual void SetOutput(Element outputElement, ICircuitPin outputPin)
        {
            OutputElement = outputElement;
            OutputPin = outputPin;
        }

        /// <summary>
        /// Sets input pin for an element</summary>
        /// <param name="inputElement">Element</param>
        /// <param name="inputPin">Input pin</param>
        public virtual void SetInput(Element inputElement, ICircuitPin inputPin)
        {
            InputElement = inputElement;
            InputPin = inputPin;
        }

        /// <summary>
        /// Gets or sets label on connection</summary>
        public virtual string Label
        {
            get { return GetAttribute<string>(LabelAttribute); }
            set { SetAttribute(LabelAttribute, value); }
        }

        #region IGraphEdge Members

        /// <summary>
        /// Gets edge's source node</summary>
        Element IGraphEdge<Element>.FromNode
        {
            get { return OutputElement; }
        }

        /// <summary>
        /// Gets the route taken from the source node</summary>
        ICircuitPin IGraphEdge<Element, ICircuitPin>.FromRoute
        {
            get { return OutputPin; }
        }

        /// <summary>
        /// Gets edge's destination node</summary>
        Element IGraphEdge<Element>.ToNode
        {
            get { return InputElement; }
        }

        /// <summary>
        /// Gets the route taken to the destination node</summary>
        ICircuitPin IGraphEdge<Element, ICircuitPin>.ToRoute
        {
            get { return InputPin; }
        }

        /// <summary>
        /// Gets edge's label</summary>
        string IGraphEdge<Element>.Label
        {
            get { return Label; }
        }

        #endregion

        /// <summary>
        /// Sets input and output PinTarget for this connection</summary>
        public void SetPinTarget()
        {
            if (InputPin != null)
            {
                bool isInputElementRef = InputElement.DomNode.Is<IReference<DomNode>>();

                if (InputPin.Is<GroupPin>())
                {
                    if (isInputElementRef)
                    {
                        var pinTarget = InputPin.Cast<GroupPin>().PinTarget;
                        InputPinTarget = new PinTarget(pinTarget.LeafDomNode, pinTarget.LeafPinIndex,
                                                       InputElement.DomNode);
                    }
                    else
                        InputPinTarget = InputPin.Cast<GroupPin>().PinTarget;
                }
                else
                {
                    if (isInputElementRef)
                    {
                        var reference = InputElement.As<IReference<DomNode>>();
                        InputPinTarget = new PinTarget(reference.Target, InputPin.Index, InputElement.DomNode);
                    }
                    else
                        InputPinTarget = new PinTarget(InputElement.DomNode, InputPin.Index, null);
                }
                Debug.Assert(InputPinTarget != null, "sanity check");
            }

            if (OutputPin != null)
            {
                bool isOutputElementRef = OutputElement.DomNode.Is<IReference<DomNode>>();
                if (OutputPin.Is<GroupPin>())
                {
                    if (isOutputElementRef)
                    {
                        var pinTarget = OutputPin.Cast<GroupPin>().PinTarget;
                        OutputPinTarget = new PinTarget(pinTarget.LeafDomNode, pinTarget.LeafPinIndex,
                                                        OutputElement.DomNode);
                    }
                    else
                        OutputPinTarget = OutputPin.Cast<GroupPin>().PinTarget;
                }
                else
                {
                    if (isOutputElementRef)
                    {
                        var reference = OutputElement.Cast<IReference<DomNode>>();
                        OutputPinTarget = new PinTarget(reference.Target, OutputPin.Index, OutputElement.DomNode);
                    }
                    else
                        OutputPinTarget = new PinTarget(OutputElement.DomNode, OutputPin.Index, null);


                }
                Debug.Assert(OutputPinTarget != null, "sanity check");
            }
        }

        /// <summary>
        /// Gets or sets the input pin target</summary>
        public PinTarget InputPinTarget
        {
            get
            {
                if (m_inputPinTarget == null)
                    SetPinTarget();
                
                return m_inputPinTarget;
            }
            set { m_inputPinTarget = value; }
        }

        /// <summary>
        /// Gets or sets the output pin target</summary>
        public PinTarget OutputPinTarget
        {
            get
            {
                if (m_outputPinTarget == null)
                    SetPinTarget();
                return m_outputPinTarget;
            }
            set { m_outputPinTarget = value; }
        }

        /// <summary>
        /// For input pin, gets enumeration of group pins down the chain before the leaf level</summary>
        public IEnumerable<GroupPin> InputPinSinkChain 
        {
            get
            {
                if (InputPin.Is<GroupPin>())
                    return InputPin.Cast<GroupPin>().SinkChain(true);
                return EmptyEnumerable<GroupPin>.Instance;
            } 
        }

        /// <summary>
        /// For output pin, gets enumeration of group pins down the chain before the leaf level</summary>
        public IEnumerable<GroupPin> OutputPinSinkChain
        {
            get
            {
                if (OutputPin.Is<GroupPin>())
                    return OutputPin.Cast<GroupPin>().SinkChain(false);
                return EmptyEnumerable<GroupPin>.Instance;
            }
        }

        internal bool IsValid(out int inputPinIndex, out int outputPinIndex)
        {
            outputPinIndex = OutputPin.Index; // GetAttribute<int>(OutputPinAttribute);
            inputPinIndex = InputPin.Index;   // GetAttribute<int>(InputPinAttribute);
            return outputPinIndex < OutputElement.Type.Outputs.Count && inputPinIndex < InputElement.Type.Inputs.Count;
        }

    

        private PinTarget m_inputPinTarget;
        private PinTarget m_outputPinTarget;

    }
}
