//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>Adapter for a reference instance of a group template</summary>
    /// <remarks>TODO: rename to GroupReference</remarks>
    public class GroupInstance : Module, ICircuitGroupType<Module, Connection, ICircuitPin>,
                                  IReference<Module>,
                                  IReference<DomNode>,
                                  IReference<Sce.Atf.Controls.Adaptable.Graphs.Group> // for circuit render
    {
        /// <summary>
        /// Constructor</summary>
        public GroupInstance()
        {
            m_info = new CircuitGroupInfo();
            m_info.PropertyChanged += groupInfoChanged;
        }

        /// <summary>Gets and sets  a globally unique identifier (GUID) that represents this template</summary>
        public Template Template
        {
            get
            {
                var template = GetReference<Template>(Schema.groupTemplateRefType.guidRefAttribute);
                if (template == null) // in case reading older circuit documents before ATF3.8
                {
                    var target = GetReference<DomNode>(Schema.groupTemplateRefType.typeRefAttribute);
                    if (target != null)
                    {
                        template = target.Parent.As<Template>();
                        if (template != null) // replace obsolete "typeRef" attribute with  guidRef
                        {
                            SetReference(Schema.groupTemplateRefType.guidRefAttribute, template.DomNode);
                            SetAttribute(Schema.groupTemplateRefType.typeRefAttribute, null);

                            Guid guid;
                            if (Guid.TryParse(Id, out guid)) // avoid using GUID as ID too, because we want to resolve guid reference specially
                                Id = "GroupInstance" + Id;                                    
                        }
                    }
                }
                return template;
            }
            set
            {
                SetReference(Schema.groupTemplateRefType.guidRefAttribute, value.DomNode);
                OnTargetSet(Target);
            }
        }


        /// <summary>
        /// Gets an adapter of the specified type, or null if there is none</summary>
        /// <param name="type">Adapter type</param>
        /// <returns>Adapter of the specified type, or null if there is none</returns>
        public override object GetAdapter(Type type)
        {
            // if type is assignable to group
            if (typeof(Sce.Atf.Controls.Adaptable.Graphs.Group).IsAssignableFrom(type))
                return ProxyGroup;

            return base.GetAdapter(type);
        }

        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            if (Template != null)
                OnTargetSet(Target);
            Info.ShowExpandedGroupPins = ShowExpandedGroupPins;
        }


        #region IReference memebers
        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        /// <remarks>This method should never throw any exceptions</remarks>
        public bool CanReference(Module item)
        {
            return item.Is<Group>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        /// <remarks>Callers should always check CanReference before setting this property.
        /// It is up to the implementer to decide whether null is an acceptable value and whether to
        /// throw an exception if the specified value cannot be targeted.</remarks>
        public Module Target
        {
            get { return Template.Target.As<Module>(); }
            set
            {
                throw new InvalidOperationException("The group template determines the target");
            }
        }

        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        /// <remarks>This method should never throw any exceptions</remarks>
        bool IReference<Sce.Atf.Controls.Adaptable.Graphs.Group>.CanReference(Sce.Atf.Controls.Adaptable.Graphs.Group item)
        {
            return item.Is<Group>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        /// <remarks>Callers should always check CanReference before setting this property.
        /// It is up to the implementer to decide whether null is an acceptable value and whether to
        /// throw an exception if the specified value cannot be targeted.</remarks>
        Sce.Atf.Controls.Adaptable.Graphs.Group IReference<Sce.Atf.Controls.Adaptable.Graphs.Group>.Target
        {
            get { return Template.Target.As<Sce.Atf.Controls.Adaptable.Graphs.Group>(); }
            set
            {
                throw new InvalidOperationException("The group template determines the target");
            }
        }

        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        /// <remarks>This method should never throw any exceptions</remarks>
        bool IReference<DomNode>.CanReference(DomNode item)
        {
            return item.Is<Group>();
        }

        /// <summary>
        /// Gets or sets the referenced element</summary>
        /// <remarks>Callers should always check CanReference before setting this property.
        /// It is up to the implementer to decide whether null is an acceptable value and whether to
        /// throw an exception if the specified value cannot be targeted.</remarks>
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
        /// Tests if group has a given input pin</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns>True iff group contains the given input pin</returns>
        public override bool HasInputPin(ICircuitPin pin)
        {
            if (Target.Type is MissingElementType)
                return false; // disallow connecting to a missing type
            if (ProxyGroup.InputGroupPins.Contains(pin)) // check group pins in the proxy
                return true;
            return Target.HasInputPin(pin); // check group pins in the original group target
        }

        /// <summary>
        /// Tests if group has a given output pin</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns>True iff group contains the given output pin</returns>
        public override bool HasOutputPin(ICircuitPin pin)
        {
            if (Target.Type is MissingElementType) // disallow connecting to a missing type
                return false;
            if (ProxyGroup.OutputGroupPins.Contains(pin)) // check group pins in the proxy
                return true;
            return Target.HasOutputPin(pin); // check group pins in the original group target
        }

        /// <summary>
        /// Gets the output pin for the given pin index</summary>
        /// <param name="pinIndex"></param>
        /// <returns></returns>
        public override ICircuitPin OutputPin(int pinIndex)
        {
            if (Target.Type is MissingElementType)
                return Target.Type.Outputs[pinIndex];
            var pin = ProxyGroup.OutputPin(pinIndex);
            return pin;
        }

        /// <summary>
        /// Gets the input pin for the given pin index</summary>
        /// <param name="pinIndex"></param>
        /// <returns></returns>
        public override ICircuitPin InputPin(int pinIndex)
        {
            if (Target.Type is MissingElementType)
                return Target.Type.Inputs[pinIndex];
            return ProxyGroup.InputPin(pinIndex);
        }

        /// <summary>
        /// Gets all the input pins for this group, including hidden ones</summary>
        public override IEnumerable<ICircuitPin> AllInputPins
        {
            get { return ProxyGroup.InputGroupPins; }
        }

        /// <summary>
        /// Gets all the output pins for this group, including hidden ones</summary>
        public override IEnumerable<ICircuitPin> AllOutputPins
        {
            get { return ProxyGroup.OutputGroupPins; }
        }


        /// <summary>
        /// Gets a ProxyGroup, which provides a surrogate that delegates communications
        /// with the targeted group on behalf of the group referencing instance</summary>
        public ProxyGroup ProxyGroup
        {
            get
            {
                if (m_proxyGroup == null)
                {
                    m_proxyGroup = new ProxyGroup(this, Target.As<Group>());
                    m_proxyGroup.Refresh();
                }
                return m_proxyGroup;
            }
            set { m_proxyGroup = value; }
        }

        /// <summary>
        /// Gets ICircuitElementType type</summary>
        public override ICircuitElementType Type
        {
            get
            {
                if (m_group == null)
                    OnTargetSet(Target);
                return Target.Type;
            }
        }

        /// <summary>
        /// Gets name attribute for group instance</summary>
        protected override AttributeInfo NameAttribute
        {
            get { return Schema.groupType.nameAttribute; }
        }

        /// <summary>
        /// Gets label attribute for group instance</summary>
        protected override AttributeInfo LabelAttribute
        {
            get { return Schema.moduleType.labelAttribute; }
        }

        /// <summary>
        /// Gets x-coordinate position attribute for group instance</summary>
        protected override AttributeInfo XAttribute
        {
            get { return Schema.groupType.xAttribute; }
        }

        /// <summary>
        /// Gets y-coordinate position attribute for group instance</summary>
        protected override AttributeInfo YAttribute
        {
            get { return Schema.groupType.yAttribute; }
        }

        /// <summary>
        /// Gets visibility attribute for group instance</summary>
        protected override AttributeInfo VisibleAttribute
        {
            get { return Schema.groupType.visibleAttribute; }
        }

        /// <summary>
        /// Finds the element and pin that matched the pin target for this circuit container</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            if (pinTarget == null ||  pinTarget.InstancingNode != DomNode)
                return new Pair<Element, ICircuitPin>(); // look no farthur

            var result = Target.Cast<Group>().MatchPinTarget(pinTarget, inputSide);
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

        //private AttributeInfo RefAttribute
        //{
        //    get { return Schema.groupTemplateRefType.typeRefAttribute; }
        //}


        /// <summary>
        /// Gets an enumeration of subnodes in the group instance</summary>
        IEnumerable<Module> IHierarchicalGraphNode<Module, Connection, ICircuitPin>.SubNodes
        {
            get { return m_group != null ? m_group.SubNodes : EmptyEnumerable<Module>.Instance; }
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
            get { return ProxyGroup.Type.Inputs; }
        }

        /// <summary>
        /// Gets the list of output pins for this element type; the list is considered
        /// to be read-only</summary>
        public IList<ICircuitPin> Outputs
        {
            get { return ProxyGroup.Type.Outputs; }
        }

        /// <summary>
        /// Gets whether the group (subgraph) is expanded</summary>
        /// <remarks>Overrides should call this base property for the setter, so that Changed event is raised.</remarks>
        public virtual bool Expanded
        {
            get
            {
                if (Target.Type is MissingElementType)
                    return false;
                return m_expanded;
            }
            set
            {
                if (value != m_expanded)
                {
                    if (!(Target.Type is MissingElementType))
                    {
                        m_expanded = value;
                        OnChanged(EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show the group pins when the group is expanded</summary>
        public bool ShowExpandedGroupPins
        {
            get { return GetAttribute<bool>(Schema.groupTemplateRefType.refShowExpandedGroupPinsAttribute); }
            set { SetAttribute(Schema.groupTemplateRefType.refShowExpandedGroupPinsAttribute, value); }
        }

        /// <summary>
        /// Gets or sets whether the group container is automatically resized to display its entire contents</summary>
        public bool AutoSize
        {
            get { return m_group != null ? m_group.AutoSize : false; }
            set { if (m_group != null) m_group.AutoSize = value; }
        }

        /// <summary>
        /// Gets the group's (subgraph's) internal edges</summary>
        IEnumerable<Connection> ICircuitGroupType<Module, Connection, ICircuitPin>.SubEdges
        {
            get { return m_group != null ? m_group.SubEdges : EmptyEnumerable<Connection>.Instance; }
        }

        /// <summary>
        /// Gets or sets CircuitGroupInfo object which controls various options on this circuit group</summary>
        public CircuitGroupInfo Info
        {
            get { return m_info; }
        }

        /// <summary>
        /// Event that is raised when this group reference has changed</summary>
        public event EventHandler Changed;

        /// <summary>
        /// Raises the Changed event, so that listeners can be alerted that the state of this group has changed</summary>
        /// <param name="e">Event args</param>
        public virtual void OnChanged(EventArgs e)
        {
            Changed.Raise(this, e);

            foreach (var domNode in DomNode.Ancestry)
            {
                var circuitEditingContext = domNode.As<CircuitEditingContext>();
                if (circuitEditingContext != null)
                    circuitEditingContext.NotifyObjectChanged(this);
            }
        }

        private void Target_AttributeChanged(object sender, AttributeEventArgs e)
        {
            OnChanged(EventArgs.Empty);
        }

        private void Target_ChildInserted(object sender, ChildEventArgs e)
        {

            OnChanged(EventArgs.Empty);
        }

        private void Target_ChildRemoved(object sender, ChildEventArgs e)
        {
            OnChanged(EventArgs.Empty);

        }


        private void OnTargetSet(Module target)
        {
            if (target != null)
            {
                m_group = Target.DomNode.As<ICircuitGroupType<Module, Connection, ICircuitPin>>();
                target.DomNode.AttributeChanged += Target_AttributeChanged;
                target.DomNode.ChildInserted += Target_ChildInserted;
                target.DomNode.ChildRemoved += Target_ChildRemoved;
            }
        }

        private void groupInfoChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ShowExpandedGroupPins = Info.ShowExpandedGroupPins;
        }

        private ICircuitGroupType<Module, Connection, ICircuitPin> m_group;
        private ProxyGroup m_proxyGroup;
        private CircuitGroupInfo m_info;
        private bool m_expanded;
    }
}
