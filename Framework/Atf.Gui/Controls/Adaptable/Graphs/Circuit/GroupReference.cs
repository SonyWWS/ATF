//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapter for a reference instance of a group template. 
    /// Since a group template can be shared by multiple group references,
    /// it duplicates group pins from target group, and has its own Info and Expanded properties.</summary>
    abstract public class GroupReference : Group, IReference<Group>, IReference<DomNode>
    {
        /// <summary>
        /// Gets the unique identifier reference attribute.</summary>
        /// <value>
        /// The unique identifier reference attribute</value>
        protected abstract AttributeInfo GuidRefAttribute { get; }

        /// <summary>
        /// Performs initialization when the adapter is connected to the group's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing:
        /// creates a DomNodeListAdapter for various circuit elements.</summary>
        protected override void OnNodeSet()
        {         
            m_info = new CircuitGroupInfo();
            m_inputs = new List<GroupPin>();
            m_outputs = new List<GroupPin>();    
            Refresh();
        }

        /// <summary>Gets and sets  a globally unique identifier (GUID) that represents this template</summary>
        public virtual Template Template
        {
            get
            {
                return GetReference<Template>(GuidRefAttribute);
               
            }
            set
            {
                SetReference(GuidRefAttribute, value.DomNode);
                Refresh();
            }
        }

        /// <summary>
        /// Gets owner's parent as IGraph</summary>
        public override IGraph<Element, Wire, ICircuitPin> ParentGraph
        {
            get { return DomNode.Parent.As<IGraph<Element, Wire, ICircuitPin>>(); }
        }


        /// <summary>
        /// Gets a list of the visible input pins in target group</summary>
        public override IList<ICircuitPin> Inputs
        {
            get
            {
                if (m_targetGroup == null) //using MissingElementType
                {
                    var missingElement = Template.Target.As<Element>();
                    return missingElement.Type.Inputs;
                }

                var inputs = m_inputs.OrderBy(n => n.Index).Where(n => n.Visible).ToArray();
                return inputs;
            }
        }

        /// <summary>
        /// Gets a list of the visible output pins in target group</summary>
        public override IList<ICircuitPin> Outputs
        {
            get
            {
                if (m_targetGroup == null) //using MissingElementType
                {
                    var missingElement = Template.Target.As<Element>();
                    return missingElement.Type.Outputs;
                }

                var outputs = m_outputs.OrderBy(n => n.Index).Where(n => n.Visible).ToArray();
                return outputs;
            }
        }

        /// <summary>
        /// Tests if group has a given input pin</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns><c>True</c> if group contains the given input pin</returns>
        public override bool HasInputPin(ICircuitPin pin)
        {
            if (m_targetGroup == null) 
                return false; // disallow connecting to a missing type
            if (InputGroupPins.Contains(pin)) // check group pins in the proxy
                return true;
            return m_targetGroup.HasInputPin(pin); // check group pins in the original group target
        }

        /// <summary>
        /// Tests if group has a given output pin.</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns><c>True</c> if group contains the given output pin</returns>
        public override bool HasOutputPin(ICircuitPin pin)
        {
            if (m_targetGroup == null) // disallow connecting to a missing type
                return false;
            if (OutputGroupPins.Contains(pin)) // check group pins in the proxy
                return true;
            return m_targetGroup.HasOutputPin(pin); // check group pins in the original group target
        }

        /// <summary>
        /// Gets input pin for given pin index.</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Input pin for given pin index</returns>
        public override ICircuitPin InputPin(int pinIndex)
        {
            if (m_targetGroup == null) //using MissingElementType
            {
                var missingElement = Template.Target.As<Element>();
                return missingElement.Type.Inputs[pinIndex];
            }
            var pin = m_inputs.FirstOrDefault(x => x.Index == pinIndex);
            return pin;
        }

        /// <summary>
        /// Gets output pin for given pin index.</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Output pin for given pin index</returns>
        public override ICircuitPin OutputPin(int pinIndex)
        {
            if (m_targetGroup == null)//using MissingElementType
            {
                var missingElement = Template.Target.As<Element>();
                return missingElement.Type.Outputs[pinIndex];
            }
            var pin = m_outputs.FirstOrDefault(x => x.Index == pinIndex);
            return pin;
        }

        /// <summary>
        /// Gets whether the group (subgraph) is expanded</summary>
        /// <remarks>Overrides should call this base property for the setter, so that Changed event is raised.</remarks>
        public override bool Expanded
        {
            get
            {
                IReference<Group> group = this;
                if (group.Target.Type is MissingElementType)
                    return false;
                return m_expanded;
            }
            set
            {
                IReference<Group> group = this;
                if (value != m_expanded)
                {
                    if (!(group.Target.Type is MissingElementType))
                    {
                        m_expanded = value;
                        OnChanged(EventArgs.Empty);
                    }
                }
            }
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

        /// <summary>
        /// Finds the element and pin that matched the pin target for this circuit container</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            if (pinTarget == null || pinTarget.InstancingNode != DomNode)
                return new Pair<Element, ICircuitPin>(); // look no farthur

            var result = m_targetGroup.MatchPinTarget(pinTarget, inputSide);
            if (result.First != null)
                result.First = this;
            return result;
        }


        #region IReference<Group>  memebers
        bool IReference<Group>.CanReference(Group item)
        {
            return true;
        }

        Group IReference<Group>.Target
        {
            get { return Template.Target.As<Group>(); }
            set { throw new InvalidOperationException("The group template determines the target"); }          
        }

        #endregion

        #region IReference<DomNode>  memebers

        bool IReference<DomNode>.CanReference(DomNode item)
        {
            return item.Is<Group>();
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

        /// <summary>
        /// Gets the referenced group</summary>
        public Group Group
        {
            get { return m_targetGroup; }
        }

        /// <summary>
        /// Refresh group</summary>
        public void Refresh()
        {        
            m_inputs.Clear();
            m_outputs.Clear();

            if (Template == null)
                return;

            var targetGroup = Template.Target.As<Group>();
            if (m_targetGroup != targetGroup)
            {
                if (m_targetGroup != null)
                     m_targetGroup.Changed -= TargetGroupChanged;
                m_targetGroup = targetGroup;
                if (m_targetGroup != null)
                    m_targetGroup.Changed += TargetGroupChanged;
            }

            if (m_targetGroup == null)
                return;

            var templateInputPins = m_targetGroup.InputGroupPins.ToArray();
            var templateOutputPins = m_targetGroup.OutputGroupPins.ToArray();
            var grpPinCopies = DomNode.Copy(templateInputPins.AsIEnumerable<DomNode>());

            for (int i = 0; i < grpPinCopies.Length; ++i)
            {
                var grpPinNode = grpPinCopies[i];
                var grpin = grpPinNode.Cast<GroupPin>();
                grpin.SetPinTarget(true);
                grpin.PinTarget.InstancingNode = DomNode;
                // share the template group pin info
                grpin.Info = templateInputPins[i].Info;
                m_inputs.Add(grpin);
            }

            var grpPinCopies2 = DomNode.Copy(templateOutputPins.AsIEnumerable<DomNode>());
            for (int i = 0; i < grpPinCopies2.Length; ++i)
            {
                var grpPinNode = grpPinCopies2[i];
                var grpin = grpPinNode.Cast<GroupPin>();
                grpin.SetPinTarget(false);
                grpin.PinTarget.InstancingNode = DomNode;
                // share the template group pin info
                grpin.Info = templateOutputPins[i].Info;
                m_outputs.Add(grpin);
            }

            Info.Offset = m_targetGroup.Info.Offset;
        }

        private void TargetGroupChanged(object sender, EventArgs eventArgs)
        {
            Refresh();
            OnChanged(eventArgs);
        }


        /// <summary>
        /// Gets a list of the input pins in this group</summary>
        public override IEnumerable<GroupPin> InputGroupPins
        {
            get
            {
                var targetGroup = Template.Target.As<Group>();
                if (targetGroup == null)
                     return EmptyEnumerable<GroupPin>.Instance;
                return m_inputs;
            }
        }

        /// <summary>
        /// Gets a list of the output pins in this group</summary>
        public override IEnumerable<GroupPin> OutputGroupPins
        {
            get
            {
                var targetGroup = Template.Target.As<Group>();
                if (targetGroup == null)
                    return EmptyEnumerable<GroupPin>.Instance;
                return m_outputs;
            }
        }

        private Group m_targetGroup;
        private List<GroupPin> m_inputs;
        private List<GroupPin> m_outputs;
        private CircuitGroupInfo m_info;
        private bool m_expanded;
    }
}
