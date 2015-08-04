//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
//#define DEBUG_VERBOSE 

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
    /// Adapts DomNode to group in a circuit</summary>
    public abstract class Group : Element, ICircuitGroupType<Element, Wire, ICircuitPin>, IGraph<Element, Wire, ICircuitPin>, IAnnotatedDiagram, ICircuitContainer
    {
        /// <summary>
        /// Constructor, creating CircuitGroupInfo object</summary>
        protected Group()
        {
            m_info = new CircuitGroupInfo();
            m_info.PropertyChanged += GroupInfoChanged;
        }

        /// <summary>
        /// Gets the attribute for the minimum width in graph (world) space that the group can be</summary>
        protected abstract AttributeInfo MinWidthAttribute { get; }

        /// <summary>
        /// Gets attribute for the minimum height in graph space that the group can be</summary>
        protected abstract AttributeInfo MinHeightAttribute { get; }

        /// <summary>
        /// Gets the attribute for the current width in graph space</summary>
        protected abstract AttributeInfo WidthAttribute { get; }

        /// <summary>
        /// Gets the attribute for the current height in graph space</summary>
        protected abstract AttributeInfo HeightAttribute { get; }

        /// <summary>
        /// Gets the attribute for the auto-size flag which indicates whether the group
        /// container is automatically resized to display its entire contents.</summary>
        protected abstract AttributeInfo AutosizeAttribute { get; }

        /// <summary>
        /// Obsolete. Returns null. If clients wants to persist the Expanded property,
        /// then override Expanded.</summary>
        protected virtual AttributeInfo ExpandedAttribute { get { return null; } }

        /// <summary>
        /// Gets the attribute for the flag which indicates if the expanded group should
        /// show the group pins when the group is expanded.</summary>
        protected abstract AttributeInfo ShowExpandedGroupPinsAttribute { get; }

        /// <summary>
        /// Gets the attribute for the flag which indicates if the group has been validated.</summary>
        protected virtual AttributeInfo ValidatedAttribute { get { return null; } }



        /// <summary>
        /// Gets the child info for the circuit elements contained within this group</summary>
        protected abstract ChildInfo ElementChildInfo { get; }

        /// <summary>
        /// Gets the child info for the wires contained within this group</summary>
        protected abstract ChildInfo WireChildInfo { get; }

        /// <summary>
        /// Gets the child info for the input pins of this group</summary>
        protected abstract ChildInfo InputChildInfo { get; }

        /// <summary>
        /// Gets the child info for the output pins of this group</summary>
        protected abstract ChildInfo OutputChildInfo { get; }

        /// <summary>
        /// Gets the child info for the annotations (called comments in the GUI) contained within
        /// this group. Return null if annotations are not supported. The default is to return null.</summary>
        protected virtual ChildInfo AnnotationChildInfo
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the DomNodeType for the group pins</summary>
        protected abstract DomNodeType GroupPinType { get; }

        /// <summary>
        /// Initial pin order style</summary>
        public enum PinOrderStyle
        {
            /// <summary>Order by y-coordinate</summary>
            NodeY,
            /// <summary>Order by depth first for nested elements, ordering by y-coordinate at same depth</summary>
            DepthFirst,
        }

        /// <summary>
        /// Gets or sets the default pin order style</summary>
        public PinOrderStyle DefaultPinOrder
        {
            get { return m_defaultPinOrder; }
            set { m_defaultPinOrder = value; }
        }

        /// <summary>
        /// Gets or sets whether the contents of the group have been changed</summary>
        public bool Dirty
        {
            get { return m_dirty; }
            set
            {
                if (value)
                {
                    m_inputPinsMap = null;
                    m_outputPinsMap = null;
                }
                m_dirty = value;
            }
        }

        /// <summary>
        /// Gets/sets a value indicating whether to observe fan-in/fan-out constraints 
        /// during group pin setup/update stage.</summary>
        /// <remarks>In-place editing requires temporarily ignoring the fan constraints 
        /// to prevent premature purge of valid group pins during moving operations.</remarks>
        public bool IgnoreFanInOut { get; set; }


        /// <summary>
        /// Performs initialization when the adapter is connected to the group's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {

            SetUpGraphData();
            base.OnNodeSet();

            DomNode.AttributeChanged += DomNode_AttributeChanged;
            DomNode.ChildInserted += DomNode_ChildInserted;
            DomNode.ChildRemoved += DomNode_ChildRemoved;
            Info.MinimumSize = MinimumSize;
            Info.ShowExpandedGroupPins = ShowExpandedGroupPins;

            // fill in group pin's leaf DomNode
            foreach (var grpPin in InputGroupPins)
            {
                grpPin.SetPinTarget(true);
                if (grpPin.InternalElement.Is<IReference<DomNode>>())
                    grpPin.PinTarget.InstancingNode = grpPin.InternalElement.DomNode;
            }

            foreach (var grpPin in OutputGroupPins)
            {
                grpPin.SetPinTarget(false);
                if (grpPin.InternalElement.Is<IReference<DomNode>>())
                    grpPin.PinTarget.InstancingNode = grpPin.InternalElement.DomNode;
            }
            foreach (var connection in Wires)
                connection.SetPinTarget();

            if (!Validated )
            {
                // normally a group node is always validated upon loading. But during version transformations, 
                // a raw group node could be created to replace old subcircuits, in which case the group is not validated before,
                // so group pins need regenerated
                var internalConnections = new List<Wire>();
                var externalConnections = new List<Wire>();
                GetSubGraphConnections(internalConnections, externalConnections, externalConnections);
                UpdateGroupPins(m_elements, internalConnections, externalConnections);
                Validated = true;
            }

            Info.HiddenInputPins = m_inputs.Where(x => !x.Visible).AsIEnumerable<ICircuitPin>();
            Info.HiddenOutputPins = m_outputs.Where(x => !x.Visible).AsIEnumerable<ICircuitPin>();

            UpdateGroupPinInfo();
            UpdateOffset();
        }

        /// <summary>
        /// Event that is raised when this Group has changed</summary>
        public event EventHandler Changed;

        /// <summary>
        /// Raises the Changed event, so that listeners can be alerted that the state of this group has changed</summary>
        /// <param name="e">Event args</param>
        public virtual void OnChanged(EventArgs e)
        {
            UpdateOffset();
            Changed.Raise(this, e);

            if (DomNode.Parent != null && DomNode.Parent.Is<Group>()) // immediate update its parent too
            {  
                DomNode.Parent.Cast<Group>().Dirty = true;
                DomNode.Parent.Cast<Group>().Update();
            }
        }

        /// <summary>
        /// Gets the element type of this group</summary>
        /// <remarks>Each group element is treated as a unique type</remarks>
        public override ICircuitElementType Type
        {
            get { return this; }
        }

        /// <summary>
        /// Tests if group has a given input pin</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns>True iff group contains the given input pin</returns>
        public override bool HasInputPin(ICircuitPin pin)
        {
            return InputGroupPins.Contains(pin);
        }

        /// <summary>
        /// Tests if group has a given output pin</summary>
        /// <param name="pin">Pin to test</param>
        /// <returns>True iff group contains the given output pin</returns>
        public override bool HasOutputPin(ICircuitPin pin)
        {
            return OutputGroupPins.Contains(pin);
        }

        /// <summary>
        /// Gets the input pin for the given pin index</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Input pin for pin index</returns>
        public override ICircuitPin InputPin(int pinIndex)
        {
            // If we're dirty, construct the map.
            if (m_inputPinsMap == null)
            {
                m_inputPinsMap = new int[m_inputs.Count];
                for(int i = m_inputs.Count; --i >= 0; )
                    m_inputPinsMap[m_inputs[i].Index] = i;
            }
            return m_inputs[m_inputPinsMap[pinIndex]];
        }

        /// <summary>
        /// Gets the output pin for the given pin index</summary>
        /// <param name="pinIndex">Pin index</param>
        /// <returns>Output pin for pin index</returns>
        public override ICircuitPin OutputPin(int pinIndex)
        {
            // If we're dirty, construct the map.
            if (m_outputPinsMap == null)
            {
                m_outputPinsMap = new int[m_outputs.Count];
                for (int i = m_outputs.Count; --i >= 0; )
                    m_outputPinsMap[m_outputs[i].Index] = i;
            }
            return m_outputs[m_outputPinsMap[pinIndex]];
        }

        /// <summary>
        /// Gets all the input pins for this group, including hidden ones</summary>
        public override IEnumerable<ICircuitPin> AllInputPins
        {
            get { return InputGroupPins; }
        }

        /// <summary>
        /// Gets all the output pins for this group, including hidden ones</summary>
        public override IEnumerable<ICircuitPin> AllOutputPins
        {
            get { return OutputGroupPins; }
        }

        /// <summary>
        /// Gets a list of the modules in this group</summary>
        public IList<Element> Elements
        {
            get { return m_elements; }
        }

        /// <summary>
        /// Gets a list of the connections in this group</summary>
        public IList<Wire> Wires
        {
            get { return m_wires; }
        }

        /// <summary>
        /// Gets an editable list of all annotations in the circuit</summary>
        public IList<Annotation> Annotations
        {
            get { return m_annotations ?? s_emptyAnnotations; }
        }

        /// <summary>
        /// Gets a list of the visible input pins in this group</summary>
        public virtual IList<ICircuitPin> Inputs
        {
            get { return m_inputs.OrderBy(n => n.Index).Where(n => n.Visible).ToArray(); }
        }

        /// <summary>
        /// Gets a list of the visible output pins in this group</summary>
        public virtual IList<ICircuitPin> Outputs
        {
            get { return m_outputs.OrderBy(n => n.Index).Where(n => n.Visible).ToArray(); }
        }

        /// <summary>
        /// Gets enumeration of all the input pins in this group</summary>
        public virtual IEnumerable<GroupPin> InputGroupPins
        {
            get { return m_inputs; }
        }

        /// <summary>
        /// Gets enumeration of all output pins in this group</summary>
        public virtual IEnumerable<GroupPin> OutputGroupPins
        {
            get { return m_outputs; }
        }


        #region ICircuitElementType Members

        /// <summary>
        /// Gets desired interior size, in pixels, of this group</summary>
        Size ICircuitElementType.InteriorSize
        {
            get { return new Size(); }
        }

        /// <summary>
        /// Gets image to draw for this this group</summary>
        Image ICircuitElementType.Image
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the list of input pins, whose Visible property is 'true', for this group;
        /// the list is considered to be read-only</summary>
        IList<ICircuitPin> ICircuitElementType.Inputs
        {
            get { return Inputs; }
        }

        /// <summary>
        /// Gets the list of output pins, whose Visible property is 'true', for this group;
        /// the list is considered to be read-only</summary>
        IList<ICircuitPin> ICircuitElementType.Outputs
        {
            get { return Outputs; }
        }

        /// <summary>
        /// Gets the group element type name</summary>
        ///<remarks>Group also inherits Element.Name which is specific for each group instance. 
        /// The type name is displayed as the element title in the top area for an element, 
        /// and the element name is displayed underneath the element. 
        /// A generic "Group" is the default type name, but derived class can customize a group's type name 
        /// by reimplementing this explicit interface member</remarks>
        string ICircuitElementType.Name
        {
            get { return "Group".Localize(); }
        }

        #endregion

        #region IGraph<Module,Connection,ICircuitPin> Members

        /// <summary>
        /// Gets the nodes in the group</summary>
        IEnumerable<Element> IGraph<Element, Wire, ICircuitPin>.Nodes
        {
            get { return m_elements; }
        }

        /// <summary>
        /// Gets the edges in the group</summary>
        IEnumerable<Wire> IGraph<Element, Wire, ICircuitPin>.Edges
        {
            get
            {
                return m_wires;
            }
        }

        #endregion

        #region IAnnotatedDiagram  Members

        /// <summary>
        /// Gets the sequence of annotations in the group</summary>
        IEnumerable<IAnnotation> IAnnotatedDiagram.Annotations
        {
            get { return m_annotations.AsIEnumerable<IAnnotation>(); }
        }


        #endregion


        /// <summary>
        /// Gets or sets the size of the group in graph space(backing DOM data)</summary>
        /// <remarks>See remarks about Bounds property.</remarks>
        public Size Size
        {
            get
            {
                return new Size(
                    GetAttribute<int>(WidthAttribute),
                    GetAttribute<int>(HeightAttribute));
            }
            set
            {
                SetAttribute(WidthAttribute, value.Width);
                SetAttribute(HeightAttribute, value.Height);
            }
        }

        /// <summary>
        /// Gets or sets the minimal size of the group in graph space (backing DOM data)</summary>
        /// <remarks>By default group node size is automatically calculated  to just have enough space 
        /// to display all its children when expanded. Set the minimal size of the group will ensure 
        /// the size calculation will observe the lower limit set by this property. </remarks>
        public Size MinimumSize
        {
            get
            {
                return new Size(
                    GetAttribute<int>(MinWidthAttribute),
                    GetAttribute<int>(MinHeightAttribute));
            }
            set
            {
                SetAttribute(MinWidthAttribute, value.Width);
                SetAttribute(MinHeightAttribute, value.Height);
            }
        }


        #region IHierarchicalGraphNode and ICircuitGroupType Members

        /// <summary>
        /// Gets the sequence of nodes that are children of this group (hierarchical graph node)</summary>
        IEnumerable<Element> IHierarchicalGraphNode<Element, Wire, ICircuitPin>.SubNodes
        {
            get
            {
                var graph = (IGraph<Element, Wire, ICircuitPin>)this;
                return graph.Nodes;
            }
        }

        /// <summary>
        /// Gets the group's (subgraph's) internal edges</summary>
        IEnumerable<Wire> ICircuitGroupType<Element, Wire, ICircuitPin>.SubEdges
        {
            get { return m_wires; }
        }

        /// <summary>
        /// Gets whether the group (subgraph) is expanded</summary>
        /// <remarks>Overrides should call this base property for the setter, so that Changed event is raised.</remarks>
        public virtual bool Expanded
        {
            get { return m_expanded; }
            set
            {
                if (value != m_expanded)
                {
                    m_expanded = value;
                    OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets CircuitGroupInfo object which controls various options on this circuit group</summary>
        public virtual CircuitGroupInfo Info
        {
            get { return m_info; }
        }

        #endregion

        /// <summary>
        /// Gets or sets whether the group container is automatically resized to display its entire contents</summary>
        public virtual bool AutoSize
        {
            get { return GetAttribute<bool>(AutosizeAttribute); }
            set { SetAttribute(AutosizeAttribute, value); }
        }

        /// <summary>
        /// Gets or sets whether to the group has been validated</summary>
        public virtual bool Validated
        {
            get
            {
                if (ValidatedAttribute!= null)
                    return GetAttribute<bool>(ValidatedAttribute);
                return true;
            }
            set
            {
                if (ValidatedAttribute != null)
                    SetAttribute(ValidatedAttribute, value);
            }
        }

        /// <summary>
        /// Gets or sets whether to show the group pins when the group is expanded</summary>
        public virtual bool ShowExpandedGroupPins
        {
            get { return GetAttribute<bool>(ShowExpandedGroupPinsAttribute); }
            set
            {
                SetAttribute(ShowExpandedGroupPinsAttribute, value);
                Info.ShowExpandedGroupPins = value;
            }
        }


        /// <summary>
        /// Gets group's parent as IGraph</summary>
        public virtual IGraph<Element, Wire, ICircuitPin> ParentGraph
        {
            get { return GetParentAs<IGraph<Element, Wire, ICircuitPin>>(); }
        }

        /// <summary>
        /// Update group pin connectivity and style for each group pin</summary>
        public void UpdateGroupPinInfo()
        {
            bool topLevelGroup = !ParentGraph.Is<Group>();// for top level group, propagate down the connectivity

            // compute group pin degrees from cross-links
            if (ParentGraph != null && ParentGraph.Edges!= null)
            {
                var inputCrossLinks = ParentGraph.Edges.Where(x => x.InputElement.DomNode == DomNode);
                foreach (var groupPin in InputGroupPins)
                {
                    groupPin.Info.ExternalConnected = inputCrossLinks.Any(e => e.InputPinTarget == groupPin.PinTarget);
                    if (groupPin.Info.ExternalConnected)
                        groupPin.Visible = true; // ensure external connected pins visible
                    if (groupPin.Info.ExternalConnected && topLevelGroup)
                    {
                        foreach (var childGroupPin in groupPin.SinkChain(true))
                        {
                            childGroupPin.Info.ExternalConnected = true;
                            childGroupPin.Visible = true; // ensure external connected pins visible
                        }
                    }
                }

                var outputCrossLinks = ParentGraph.Edges.Where(x => x.OutputElement.DomNode == DomNode);
                foreach (var groupPin in OutputGroupPins)
                {
                    groupPin.Info.ExternalConnected = outputCrossLinks.Any(e => e.OutputPinTarget == groupPin.PinTarget);
                    if (groupPin.Info.ExternalConnected)
                        groupPin.Visible = true; // ensure external connected pins visible
                    if (groupPin.Info.ExternalConnected && topLevelGroup)
                    {
                        foreach (var childGroupPin in groupPin.SinkChain(false))
                        {
                            childGroupPin.Info.ExternalConnected = true;
                            childGroupPin.Visible = true; // ensure external connected pins visible
                        }                         
                    }
                }
            }

        }


        /// <summary>
        ///  update group pin indexes and the cross-link indexes of the parent graph</summary>
        private void UpdatePinOrders()
        {
            for (int pass = 0; pass < 2; ++pass)
            {

                var sorted = (pass == 0) ? m_inputs.OrderBy(n => n.Position.Y).ToArray() :
                    m_outputs.OrderBy(n => n.Position.Y).ToArray();

                // assign group pin index in y order, but visible pins goes first 
                int index = 0;
                for (int i = 0; i < sorted.Length; ++i)
                {
                    var groupPin = sorted[i];
                    if (groupPin.Visible)
                    {
                        groupPin.Index = index;
                        ++index;
                    }
                }
                for (int i = 0; i < sorted.Length; ++i)
                {
                    var groupPin = sorted[i];
                    if (!groupPin.Visible)
                    {
                        groupPin.Index = index;
                        ++index;
                    }
                }
            }

            foreach (var groupPin in InputGroupPins)
            {
                if (groupPin.PinTarget == null)
                {
                    groupPin.SetPinTarget(true);
                    groupPin.SetPinTarget(true);
                }

                // update parent sub-graph group pin index
                if (ParentGraph.Is<Group>())
                {
                    var parentSubGraph = ParentGraph.Cast<Group>();
                    foreach (var parentGrpPin in parentSubGraph.InputGroupPins)
                    {
                        if (parentGrpPin.PinTarget == groupPin.PinTarget)
                        {
                            parentGrpPin.InternalPinIndex = groupPin.Index;
                        }
                    }
                }


            }

            foreach (var groupPin in OutputGroupPins)
            {
                if (groupPin.PinTarget == null)
                {
                    groupPin.SetPinTarget(false);
                    groupPin.SetPinTarget(false);
                }

                // update parent sub-graph group pin index
                if (ParentGraph.Is<Group>())
                {
                    var parentSubGraph = ParentGraph.Cast<Group>();
                    foreach (var parentGrpPin in parentSubGraph.OutputGroupPins)
                    {
                        if (parentGrpPin.PinTarget == groupPin.PinTarget)
                        {
                            parentGrpPin.InternalPinIndex = groupPin.Index;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update group pins, its indexes, and cross-links</summary>
        public void Update()
        {
            if (!Dirty)
                return;
#if DEBUG_VERBOSE
            Trace.TraceInformation("SubGraph {0} --  Update with {1} elements",  Id, m_elements.Count);
           
#endif
            var internalConnections = new List<Wire>();
            var externalConnections = new List<Wire>();
            GetSubGraphConnections(internalConnections, externalConnections, externalConnections);
            UpdateGroupPins(m_elements, internalConnections, externalConnections);


            //if (DomNode.Parent != null && DomNode.Parent.Is<ICircuitContainer>()) // propagate changes up for newly added or removed floating pins
            //    DomNode.Parent.Cast<ICircuitContainer>().Dirty = true;


            //ConstrainCoords();
            UpdatePinOrders(); // set pin index based on pinY and visibility         
            Dirty = false;
            OnChanged(EventArgs.Empty); // notify changes to outside
        }

        /// <summary>
        /// Update group pins based on the configuration of current sub-nodes and sub-edge.
        /// Currently, automatically expose those pins that can be connected to outside.</summary>
        /// <param name="modules">Modules in group</param>
        /// <param name="internalConnections">Internal connections of the owner group</param>
        /// <param name="externalConnections">Wires with external connections</param>
        public void UpdateGroupPins(IEnumerable<Element> modules, List<Wire> internalConnections, List<Wire> externalConnections)
        {
            // 1) expose all internal pins that have external connections(TODO: this step seems not needed)
            foreach (var connection in externalConnections)
            {
                if (modules.Contains(connection.InputElement))
                {
                    var grpPin = MatchedGroupPin(connection.InputElement, connection.InputPin.Index, true);
                    if (grpPin == null)
                    {
                        var inputPin = connection.InputElement.Type.Inputs[connection.InputPin.Index];
                        var groupPin = new DomNode(GroupPinType).As<GroupPin>();
                        groupPin.TypeName = inputPin.TypeName;
                        groupPin.InternalElement = connection.InputElement;
                        groupPin.InternalPinIndex = inputPin.Index;
                        groupPin.Index = m_inputs.Count;
                        groupPin.Position = new Point(0, (groupPin.InternalPinIndex + 1) * 16 + connection.InputElement.Bounds.Location.Y);
                        groupPin.IsDefaultName = true;
                        groupPin.Visible = true;

                        m_inputs.Add(groupPin);
                        groupPin.SetPinTarget(true);
                        if (groupPin.InternalElement.Is<IReference<DomNode>>())
                            groupPin.PinTarget.InstancingNode = groupPin.InternalElement.DomNode;
                        groupPin.Name = groupPin.DefaultName(true);//connection.InputElement.Name + ":" + inputPin.Name;
                    }
                    else
                        grpPin.Visible = true;// force external connected pin visible
                }
                else
                {
                    var grpPin = MatchedGroupPin(connection.OutputElement, connection.OutputPin.Index, false);
                    if (grpPin == null)
                    {
                        var outputPin = connection.OutputElement.Type.Outputs[connection.OutputPin.Index];
                        var groupPin = new DomNode(GroupPinType).As<GroupPin>();
                        groupPin.TypeName = outputPin.TypeName;
                        groupPin.InternalElement = connection.OutputElement;
                        groupPin.InternalPinIndex = outputPin.Index;
                        groupPin.Index = m_outputs.Count;
                        groupPin.Position = new Point(0, (groupPin.InternalPinIndex + 1) * 16 + connection.OutputElement.Bounds.Location.Y);
                        groupPin.IsDefaultName = true;
                        groupPin.Visible = true;
                        m_outputs.Add(groupPin);
                        groupPin.SetPinTarget(false);
                        if (groupPin.InternalElement.Is<IReference<DomNode>>())
                            groupPin.PinTarget.InstancingNode = groupPin.InternalElement.DomNode;
                        groupPin.Name = groupPin.DefaultName(false); //connection.OutputElement.Name + ":" + outputPin.Name;
                    }
                    else
                        grpPin.Visible = true; // force external connected pin visible
                }
            }

            // 2 ) collect all the input and output pins from all sub-nodes,
            foreach (var module in modules)
            {
                Element element = module;
                foreach (var inputPin in module.AllInputPins.OrderBy(n => element.PinDisplayOrder(n.Index, true)))
                {
                    var grpPin = MatchedGroupPin(module, inputPin.Index, true);
                    if (CanExposePin(module, inputPin, internalConnections, true))
                    {
                        if (grpPin == null)
                        {
                            var groupPin = new DomNode(GroupPinType).As<GroupPin>();
                            groupPin.TypeName = inputPin.TypeName;
                            groupPin.InternalElement = module;
                            groupPin.InternalPinIndex = inputPin.Index;
                            groupPin.Index = m_inputs.Count;
                            groupPin.Position = new Point(0, (groupPin.InternalPinIndex + 1) * 16 + module.Bounds.Location.Y);
                            groupPin.Visible = inputPin.Is<IVisible>() ? inputPin.Cast<IVisible>().Visible : ShowExpandedGroupPins;
                            groupPin.IsDefaultName = true;
                            m_inputs.Add(groupPin);
                            groupPin.SetPinTarget(true);
                            if (groupPin.InternalElement.Is<IReference<DomNode>>())
                                groupPin.PinTarget.InstancingNode = groupPin.InternalElement.DomNode;
                            groupPin.Name = groupPin.DefaultName(true); //module.Name + ":" + inputPin.Name;
                        }
                        else
                        {
                            // A move operation could move nested elements across internal containers.
                            // The group pins associated with a moved element needs to update its InternalElement to survive the pin purge.
                            grpPin.InternalElement = module;
                        }
                    }
                    else
                    {
                        if (grpPin != null)
                            m_inputs.Remove(grpPin);
                    }
                }

                foreach (var outputPin in module.AllOutputPins.OrderBy(n => element.PinDisplayOrder(n.Index, false)))
                {
                    var grpPin = MatchedGroupPin(module, outputPin.Index, false);
                    if (CanExposePin(module, outputPin, internalConnections, false))
                    {
                        if (grpPin == null)
                        {

                            var groupPin = new DomNode(GroupPinType).As<GroupPin>();
                            groupPin.TypeName = outputPin.TypeName;
                            groupPin.InternalElement = module;
                            groupPin.InternalPinIndex = outputPin.Index;
                            groupPin.Index = m_outputs.Count;
                            groupPin.Position = new Point(0, (groupPin.InternalPinIndex + 1) * 16 + module.Bounds.Location.Y);
                            groupPin.Visible = outputPin.Is<IVisible>() ? outputPin.Cast<IVisible>().Visible : ShowExpandedGroupPins;
                            groupPin.IsDefaultName = true;
                            m_outputs.Add(groupPin);
                            groupPin.SetPinTarget(false);
                            if (groupPin.InternalElement.Is<IReference<DomNode>>())
                                groupPin.PinTarget.InstancingNode = groupPin.InternalElement.DomNode;
                            groupPin.Name = groupPin.DefaultName(false); //module.Name + ":" + outputPin.Name;
                        }
                        else
                        {
                            grpPin.InternalElement = module;
                        }
                    }
                    else
                    {
                        if (grpPin != null)
                            m_outputs.Remove(grpPin);
                    }
                }
            }

            // 3) remove all dangling group pins that have no corresponding internal nodes, or matched pins 
            RemoveDanglingGroupPins();

            Info.HiddenInputPins = m_inputs.Where(x => !x.Visible).AsIEnumerable<ICircuitPin>();
            Info.HiddenOutputPins = m_outputs.Where(x => !x.Visible).AsIEnumerable<ICircuitPin>();

        }

        // remove all dangling group pins that have no corresponding internal nodes, or matched pins 
        private void RemoveDanglingGroupPins()
        {
            PinTarget pinTarget;
            foreach (var grpPin in m_inputs.ToArray())
            {
                pinTarget = grpPin.PinTarget;
                if (pinTarget != null && grpPin.InternalElement.Is<IReference<DomNode>>())
                    pinTarget.InstancingNode = grpPin.InternalElement.DomNode;
                if (pinTarget == null ||
                    !Elements.Contains(grpPin.InternalElement) ||
                    grpPin.InternalElement.MatchPinTarget(pinTarget, true).First == null)
                {
                     m_inputs.Remove(grpPin);
                }
                else
                {
                    // sync group pin names
                    if (grpPin.IsDefaultName)
                        grpPin.Name = grpPin.DefaultName(true);
                }
            }

            foreach (var grpPin in m_outputs.ToArray())
            {
                pinTarget = grpPin.PinTarget;
                if (pinTarget != null && grpPin.InternalElement.Is<IReference<DomNode>>())
                    pinTarget.InstancingNode = grpPin.InternalElement.DomNode;
                if (pinTarget == null || 
                    !Elements.Contains(grpPin.InternalElement) ||
                    grpPin.InternalElement.MatchPinTarget(pinTarget, false).First == null)
                {
                    m_outputs.Remove(grpPin);
                }
                else
                {
                    // sync group pin names
                    if (grpPin.IsDefaultName)
                        grpPin.Name = grpPin.DefaultName(false);
                }
            }
        }

        /// <summary>
        /// Initialize group pin's indexes</summary>
        /// <param name="internalConnections">Internal connections of the owner group</param>
        public void InitializeGroupPinIndexes(IEnumerable<Wire> internalConnections)
        {
            if (DefaultPinOrder == PinOrderStyle.NodeY)
            {
                // layout group pins by its sub-node Y value first, then sub-node pin display order
                var orderdInputs = m_inputs.OrderBy(n => n.InternalElement.Bounds.Location.Y).ThenBy(n => n.InternalElement.PinDisplayOrder(n.InternalPinIndex, true)).ToList();
                foreach (var grpPin in m_inputs)
                {
                    int newIndex = orderdInputs.IndexOf(grpPin);
                    grpPin.Index = newIndex;
                }

                var orderdOutputs = m_outputs.OrderBy(n => n.InternalElement.Bounds.Location.Y).ThenBy(n => n.InternalElement.PinDisplayOrder(n.InternalPinIndex, false)).ToList();
                foreach (var grpPin in m_outputs)
                {
                    int newIndex = orderdOutputs.IndexOf(grpPin);
                    grpPin.Index = newIndex;
                }


                int lastY = 0;
                int firstY = 0;
                Element lastNode = null;
                // init pinY based on pin index
                foreach (GroupPin grpPin in orderdInputs)
                {
                    if (lastNode == null)
                    {
                        firstY = grpPin.InternalElement.Bounds.Location.Y;
                        grpPin.Position = new Point(0, lastY);
                        lastNode = grpPin.InternalElement;

                    }
                    else
                    {
                        int pinY = lastY + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin;
                        if (lastNode != grpPin.InternalElement) // new element
                        {
                            pinY = Math.Max(grpPin.InternalElement.Bounds.Location.Y - firstY,
                                          lastY + +CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinElementMargin);
                            lastNode = grpPin.InternalElement;
                        }
                        grpPin.Position = new Point(0, pinY);
                        lastY = pinY;
                    }

                }

                lastY = 0;
                lastNode = null;
                foreach (GroupPin grpPin in orderdOutputs)
                {
                    if (lastNode == null)
                    {
                        firstY = grpPin.InternalElement.Bounds.Location.Y;
                        grpPin.Position = new Point(0, lastY);
                        lastNode = grpPin.InternalElement;

                    }
                    else
                    {
                        int pinY = lastY + CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin;
                        if (lastNode != grpPin.InternalElement)// new element
                        {
                            pinY = Math.Max(grpPin.InternalElement.Bounds.Location.Y - firstY,
                                           lastY + +CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinElementMargin);
                            lastNode = grpPin.InternalElement;
                        }
                        grpPin.Position = new Point(0, pinY);
                        lastY = pinY;
                    }

                }
            }
            else if (DefaultPinOrder == PinOrderStyle.DepthFirst)
            {
                // a) find all output nodes( these nodes whose outputs has no internal connections)
                // b) order output nodes by y value
                // c) for each output node from b), back traverse from output node, in depth first(pre-order),
                //    the visiting order of the pin nodes seems a natural initial floating pins order

                // a node is an output node only if it is not an output of any internal node      
                var outputNodes = Elements.Where(node => !Wires.Any(e => e.OutputElement == node))
                                           .OrderBy(x => x.Bounds.Y); // descending order for stack push that follows

                var grpPinVisited = new List<GroupPin>();
                foreach (var node in outputNodes)
                    BackDepthOrderVistor(node, internalConnections, grpPinVisited);

                foreach (var grpPin in m_inputs)
                {
                    int newIndex = grpPinVisited.IndexOf(grpPin);
                    grpPin.Index = newIndex;
                }

                // outputs just based pinY 
                var orderd = m_outputs.OrderBy(n => n.InternalElement.Bounds.Location.Y).ThenBy(n => n.InternalPinIndex).ToList();
                foreach (var grpPin in m_outputs)
                {
                    int newIndex = orderd.IndexOf(grpPin);
                    grpPin.Index = newIndex;
                }

                int i = 0;
                foreach (var grpPin in m_inputs.OrderBy(n => n.Index))
                {
                    grpPin.Position = new Point(0, i * (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
                    ++i;
                }
                i = 0;
                foreach (var grpPin in m_outputs.OrderBy(n => n.Index))
                {
                    grpPin.Position = new Point(0, i * (CircuitGroupPinInfo.FloatingPinNodeHeight + CircuitGroupPinInfo.FloatingPinNodeMargin));
                    ++i;
                }
            }

            UpdatePinOrders(); // order index by visibility 
        }

        /// <summary>
        /// Whether or not the module pin can be legally exposed as a group pin</summary>
        /// <param name="element">Module to check</param>
        /// <param name="pin">Pin on module</param>
        /// <param name="internalConnections">Internal connections of the owner group</param>
        /// <param name="inputSide">Whether or not the pin is located at the input side of the module</param>
        /// <returns>True iff module pin can be legally exposed as a group pin</returns>
        /// <remarks>An internal pin must be exposed if there are external edges connected to it; 
        /// an internal pin may be visible or hidden if it is legal to expose but no external connections.
        /// This default implementation exposes all legal candidates that can be exposed as group pins. 
        /// So there is always over-exposing problem, never under-exposing problem to start with the default.</remarks>
        virtual protected bool CanExposePin(Element element, ICircuitPin pin, IEnumerable<Wire> internalConnections, bool inputSide)
        {
            bool isElementRef = element.Is<IReference<DomNode>>();

            DomNode referencingNode = isElementRef ? element.DomNode : null;

            if (!IgnoreFanInOut)
            {
                foreach (var edge in internalConnections)
                {
                    if (inputSide)
                    {
                        var inputPinTarget = pin.Is<GroupPin>() ?
                               pin.Cast<GroupPin>().PinTarget : new PinTarget(element.DomNode, pin.Index, null);

                        //inputPinTarget.InstancingNode = referencingNode;
                        if (inputPinTarget == edge.InputPinTarget)
                            if (!pin.AllowFanIn)
                                return false;
                    }
                    else
                    {
                        var outputPinTarget = pin.Is<GroupPin>() ?
                               pin.Cast<GroupPin>().PinTarget : new PinTarget(element.DomNode, pin.Index, referencingNode);
                        //outputPinTarget.InstancingNode = referencingNode;
                        if (outputPinTarget == edge.OutputPinTarget)
                        {
                            if (!pin.AllowFanOut)
                                return false;
                        }

                    }
                }
            }
            return true;
        }


        /// <summary>
        /// Validates the state of the group</summary>
        public void Validate()
        {

            // check every pin of a subnode that does not connect to an internal subnode has a corresponding group pin
            var internalConnections = new List<Wire>();
            var externalConnections = new List<Wire>();
            GetSubGraphConnections(internalConnections, externalConnections, externalConnections);

            // --- validate visible group pins
            var inputGroupPins = m_inputs.Where(p => p.Visible).ToList();
            var outputGroupPins = m_outputs.Where(p => p.Visible).ToList();
            // no self duplications
            var duplicates = inputGroupPins.GroupBy(g => g).Where(w => w.Count() > 1);
            Debug.Assert(!duplicates.Any(), "SubGraph " + Name + " has duplicated input group pins");
            duplicates = outputGroupPins.GroupBy(g => g).Where(w => w.Count() > 1);
            Debug.Assert(!duplicates.Any(), "SubGraph " + Name + " has duplicated output group pins");
            // each group pin points to different internal module and pin 
            {
                var byInternalElements = inputGroupPins.GroupBy(g => g.InternalElement);
                foreach (var grpPins in byInternalElements)
                {
                    var duplicatedPins = grpPins.GroupBy(g => g.Index).Where(w => w.Count() > 1);
                    Debug.Assert(!duplicatedPins.Any(), "SubGraph " + Name + " module " + grpPins.Key.Name + " has duplicated output group pins");
                }

                byInternalElements = outputGroupPins.GroupBy(g => g.InternalElement);
                foreach (var grpPins in byInternalElements)
                {
                    var duplicatedPins = grpPins.GroupBy(g => g.Index).Where(w => w.Count() > 1);
                    Debug.Assert(!duplicatedPins.Any(), "SubGraph " + Name + " has duplicated output group pins");
                }
            }

            foreach (var grpPin in InputGroupPins)
            {
                Debug.Assert(grpPin.PinTarget != null, "SubGraph " + Name + " Input Group Pin" + grpPin.Name + "LeaveNode not set");
                // pins of proxy group parent not set 
                //Debug.Assert(grpPin.DomNode.Parent != null, "SubGraph " + Name + " Input Group Pin" + grpPin.Name + "Parent  not set");
                Debug.Assert(grpPin.Index < InputGroupPins.Count(), "SubGraph " + Name + " Input Group Pin" + grpPin.Name +
                    " Index invalid " + grpPin.Index + " Count=" + InputGroupPins.Count());
                //Debug.Assert(grpPin.Position.Y >= 0, "SubGraph " + Name + " Input Group Pin" + grpPin.Name + "has negative Y");
                //Debug.Assert(grpPin.Position.Y <= 4096 && grpPin.Position.Y >= -4096, "SubGraph " + Name + " Input Group Pin" + grpPin.Name + "has suspicious large Y greater than 4k");
                // validate InternalPinIndex
                Debug.Assert(grpPin.InternalPinIndex >= 0 && grpPin.InternalPinIndex < grpPin.InternalElement.Type.GetAllInputPins().Count(),
                    "SubGraph " + Name + " Input Group Pin" + grpPin.Name + "InternalPinIndex is out of range");
            }
            foreach (var grpPin in OutputGroupPins)
            {
                Debug.Assert(grpPin.PinTarget != null, "SubGraph " + Name + " Output Group Pin" + grpPin.Name + "LeaveNode not set");
                // pins of proxy group parent not set 
                //Debug.Assert(grpPin.DomNode.Parent != null, "SubGraph " + Name + " Output Group Pin" + grpPin.Name + "Parent  not set");
                Debug.Assert(grpPin.Index < OutputGroupPins.Count(), "SubGraph " + Name + " Output Group Pin" + grpPin.Name +
                    " Index invalid " + grpPin.Index + " Count=" + OutputGroupPins.Count());
                //Debug.Assert(grpPin.Position.Y >= 0, "SubGraph " + Name + " Input Group Pin" + grpPin.Name + "has negative Y");
                //Debug.Assert(grpPin.Position.Y <= 4096 && grpPin.Position.Y >= -4096, "SubGraph " + Name + " Input Group Pin" + grpPin.Name + "has suspicious large Y greater than 4k");
                // validate InternalPinIndex
                Debug.Assert(grpPin.InternalPinIndex >= 0 && grpPin.InternalPinIndex < grpPin.InternalElement.Type.GetAllOutputPins().Count(),
                    "SubGraph " + Name + " Output Group Pin" + grpPin.Name + "InternalPinIndex is out of range");
            }

            var pinIndexes = inputGroupPins.GroupBy(g => g.Index).Where(w => w.Count() > 1);
            Debug.Assert(!pinIndexes.Any(),
                               "SubGraph " + Name + " Input Group Pin Indexes Duplicated");

            pinIndexes = outputGroupPins.GroupBy(g => g.Index).Where(w => w.Count() > 1);
            Debug.Assert(!pinIndexes.Any(),
                               "SubGraph " + Name + " Output Group Pin Indexes Duplicated");

            foreach (var module in Elements)
            {
                // input group pins
                foreach (var inputPin in module.Type.Inputs)
                {
                    if (CanExposePin(module, inputPin, Wires, true))
                    {
                        var grpPin = MatchedGroupPin(module, inputPin.Index, true);
                        Debug.Assert(grpPin != null, "SubGraph " + Name + " missed an input group pin for " + module.Name + ":" + inputPin.Name);
                    }
                    else
                    {
                        var grpPin = MatchedGroupPin(module, inputPin.Index, true);
                        Debug.Assert(grpPin == null, "SubGraph " + Name + " should not expose an input group pin for " + module.Name + ":" + inputPin.Name);
                    }
                }

                // output group pins
                foreach (var outputPin in module.Type.Outputs)
                {
                    if (CanExposePin(module, outputPin, Wires, false))
                    {
                        var grpPin = MatchedGroupPin(module, outputPin.Index, false);
                        Debug.Assert(grpPin != null, "SubGraph " + Name + " missed an output group pin for " + module.Name + ":" + outputPin.Name);
                    }
                    else
                    {
                        var grpPin = MatchedGroupPin(module, outputPin.Index, false);
                        Debug.Assert(grpPin == null, "SubGraph " + Name + " should not expose an output group pin for " + module.Name + ":" + outputPin.Name);
                    }
                }

            }

            // --- validate cross links
            if (ParentGraph != null)
            {
                var inputCrossLinks = ParentGraph.Edges.Where(x => x.InputElement.DomNode == DomNode);

                // check group pin index
                foreach (var link in inputCrossLinks)
                    Debug.Assert(link.InputPin.Index < m_inputs.Count, "Pin index out of range");

                // check fan in
                foreach (var grpPin in m_inputs)
                {
                    var leafModule = grpPin.PinTarget.LeafDomNode.Cast<Element>();
                    var leafPin = leafModule.Type.Inputs[grpPin.PinTarget.LeafPinIndex];
                    if (!leafPin.AllowFanIn)
                    {
                        GroupPin pin = grpPin;
                        int incomingCrossLinks = inputCrossLinks.Count(x => (x.InputPinTarget.FullyEquals(pin.PinTarget)));
                        int incomingInternalLinks = Wires.Count(x => (x.InputPinTarget.FullyEquals(pin.PinTarget)));
                        Debug.Assert(incomingCrossLinks + incomingInternalLinks <= 1,
                                           "Multiple incoming edges not allowed for this pin");
                    }
                }

                var outputCrossLinks = ParentGraph.Edges.Where(x => x.OutputElement.DomNode == DomNode);

                // check group pin index
                foreach (var link in outputCrossLinks)
                {
                    var linkName = CircuitUtil.GetDomNodeName(link.DomNode);
                    Debug.Assert(link.OutputPin.Index < m_outputs.Count, linkName + " OutputPin index out of range");
                }


                // check fan out
                foreach (var grpPin in m_outputs)
                {
                    var leafModule = grpPin.PinTarget.LeafDomNode.Cast<Element>();
                    var leafPin = leafModule.Type.Outputs[grpPin.PinTarget.LeafPinIndex];
                    if (!leafPin.AllowFanOut)
                    {
                        GroupPin pin = grpPin;
                        int outgoingCrossLinks = outputCrossLinks.Count(x => (x.OutputPinTarget == pin.PinTarget));
                        int outgoingInternalLinks = Wires.Count(x => (x.OutputPinTarget == pin.PinTarget));
                        Debug.Assert(outgoingCrossLinks + outgoingInternalLinks <= 1,
                                           "Multiple outgoing edges not allowed for this pin");
                    }
                }
            }

        }

        // returns a list of all modules, internal connections between them, and connections to external nodes
        private void GetSubGraphConnections(
            ICollection<Wire> internalConnections,
            ICollection<Wire> incomingConnections,
            ICollection<Wire> outgoingConnections)
        {

            //  add connections to modules
            foreach (var connection in Wires)
            {
                bool output = Elements.Contains(connection.OutputElement);
                bool input = Elements.Contains(connection.InputElement);
                if (output && input)
                {
                    internalConnections.Add(connection);
                }
                else if (output)
                {
                    outgoingConnections.Add(connection);
                }
                else if (input)
                {
                    incomingConnections.Add(connection);
                }

            }
        }

        /// <summary>
        /// Finds the element and pin that matched the pin target for this circuit container</summary>
        /// <param name="pinTarget">Contains pin's element and pin index</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Return a pair of element and pin. As an element instance method, if there is a match, the element is self, 
        /// and pin is one of its pins defined in Type. If there is no match, both are null.</returns>
        public override Pair<Element, ICircuitPin> MatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            var result = new Pair<Element, ICircuitPin>();
            GroupPin grpPin = inputSide ? m_inputs.FirstOrDefault(x => x.PinTarget == pinTarget) :
                    m_outputs.FirstOrDefault(x => x.PinTarget == pinTarget);

            if (grpPin != null)
            {

                if (Elements.Contains(grpPin.InternalElement))
                {
                    result.First = this;
                    result.Second = grpPin;
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
        public override Pair<Element, ICircuitPin> FullyMatchPinTarget(PinTarget pinTarget, bool inputSide)
        {
            var result = new Pair<Element, ICircuitPin>();
            GroupPin grpPin = inputSide ? m_inputs.FirstOrDefault(x => x.PinTarget.FullyEquals(pinTarget)) :
                    m_outputs.FirstOrDefault(x => x.PinTarget.FullyEquals(pinTarget));

            if (grpPin != null)
            {

                if (Elements.Contains(grpPin.InternalElement))
                {
                    result.First = this;
                    result.Second = grpPin;
                }

            }
            return result;
        }

        /// <summary>
        /// Finds a group pin corresponding to the given node and the pin index inside the node</summary>
        /// <param name="node">Node to search</param>
        /// <param name="pinIndex">Input or output pin index of the given node</param>
        /// <param name="inputSide">True for input pin, false for output pin</param>
        /// <returns>Group pin found</returns>
        public GroupPin MatchedGroupPin(Element node, int pinIndex, bool inputSide)
        {
            if (node.Is<Group>())
            {
                var nestedSubGraph = node.Cast<Group>();
                if (inputSide)
                {
                    var nestedGrpPin = nestedSubGraph.InputGroupPins.First(x => x.Index == pinIndex);
                    return m_inputs.FirstOrDefault(x => x.PinTarget.FullyEquals(nestedGrpPin.PinTarget));
                }
                else
                {
                    var nestedGrpPin = nestedSubGraph.OutputGroupPins.First(x => x.Index == pinIndex);
                    return m_outputs.FirstOrDefault(x => x.PinTarget.FullyEquals(nestedGrpPin.PinTarget));
                }
            }


            return inputSide ? m_inputs.FirstOrDefault(x => x.InternalElement.DomNode == node.DomNode && x.InternalPinIndex == pinIndex) :
                        m_outputs.FirstOrDefault(x => x.InternalElement.DomNode == node.DomNode && x.InternalPinIndex == pinIndex);

        }

        /// <summary>
        /// Returns true iff the specified attribute is name or label</summary>
        /// <param name="attributeInfo">AttributeInfo for attribute</param>
        /// <returns>True iff the specified attribute is name or label</returns>
        public bool IsNameAttribute(AttributeInfo attributeInfo)
        {
            return (attributeInfo == NameAttribute || attributeInfo == LabelAttribute);
        }

        private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
        {
            Dirty = true;
            if (e.AttributeInfo == MinHeightAttribute || e.AttributeInfo == MinWidthAttribute)
            {
                // sync mini size info
                Info.MinimumSize = MinimumSize ;
            }
#if DEBUG_VERBOSE
            if (e.DomNode.Is<Wire>())
            {
                Trace.TraceInformation("SubGraph {0} element  {1} -- Attribute {2} changed from  {3} to {4}",
                 Name, CircuitUtil.GetDomNodeName(e.DomNode), e.AttributeInfo.Name, e.OldValue, e.NewValue);
            }
            if (e.DomNode == DomNode || e.DomNode.Parent == DomNode)
            {
                Trace.TraceInformation("SubGraph {0} element  {1} -- Attribute {2} changed from  {3} to {4}",
                  Name, CircuitUtil.GetDomNodeName(e.DomNode), e.AttributeInfo.Name, e.OldValue, e.NewValue);
            }
#endif
        }


        // Need to explicitly add Inputs and Outputs of sub-graph pins when nodes are inserted or deleted   
        private void DomNode_ChildInserted(object sender, ChildEventArgs e)
        {
            Dirty = true;

#if DEBUG_VERBOSE
            if (e.Parent == DomNode)
            {
                Trace.TraceInformation("SubGraph {0} --  Added {1} to parent {2}",
                    Id, CircuitUtil.GetDomNodeName(e.Child), CircuitUtil.GetDomNodeName(e.Parent));
            }
#endif
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            Dirty = true;

#if DEBUG_VERBOSE
            if (e.Parent == DomNode)
            {
                Trace.TraceInformation("SubGraph {0} --  Removed {1}  from parent {2}", Name,
                                        CircuitUtil.GetDomNodeName(e.Child), CircuitUtil.GetDomNodeName(e.Parent));
            }
#endif
        }



        /// <summary>
        /// build sub-graph pins and links for the graph viewer 
        /// </summary>
        private void SetUpGraphData()
        {
            m_elements = new DomNodeListAdapter<Element>(DomNode, ElementChildInfo);
            m_wires = new DomNodeListAdapter<Wire>(DomNode, WireChildInfo);
            if (AnnotationChildInfo != null)
                m_annotations = new DomNodeListAdapter<Annotation>(DomNode, AnnotationChildInfo);
            m_inputs = new DomNodeListAdapter<GroupPin>(DomNode, InputChildInfo);
            m_outputs = new DomNodeListAdapter<GroupPin>(DomNode, OutputChildInfo);
        }

        private void BackDepthOrderVistor(Element outputNode, IEnumerable<Wire> internalConnections, List<GroupPin> grpPinVisited)
        {
            // push in nodes that connects to this node's input
            int pinIndex = 0;

            foreach (var inputPin in outputNode.Type.Inputs)
            {
                if (CanExposePin(outputNode, inputPin, internalConnections, true)) // reach the group pin
                {
                    var inputPinTarget = inputPin.Is<GroupPin>() ?
                             inputPin.Cast<GroupPin>().PinTarget : new PinTarget(GetDomLeafNode(outputNode.DomNode), inputPin.Index, null);
                    var grpPin = m_inputs.First(n => n.PinTarget == inputPinTarget);
                    grpPinVisited.Add(grpPin);
                }
                else
                {
                    // the node is  internally connected, traverse the edge 
                    foreach (var edge in internalConnections)
                    {
                        if (edge.InputElement == outputNode && edge.InputPin.Index == pinIndex)
                        {
                            BackDepthOrderVistor(edge.OutputElement, internalConnections, grpPinVisited);
                            break;
                        }

                    }
                }
                ++pinIndex;
            }
        }

        private DomNode GetDomLeafNode(DomNode node)
        {
            var current = node;
            while (current.Is<IReference<DomNode>>())
            {
                current = current.Cast<IReference<DomNode>>().Target;                
            }
            return current;
        }

        // constrain sub-nodes and group pins to have non-negative coordinates
        private void ConstrainCoords()
        {
            var min = new Point();
            if (Elements.Any())
            {
                min.X = Elements.Select(x => x.Bounds.Location.X).Min();
                min.Y = Elements.Select(x => x.Bounds.Location.Y).Min();
            }
            if (InputGroupPins.Any())
            {
                min.Y = Math.Min(InputGroupPins.Select(x => x.Position.Y).Min(), min.Y);
            }

            if (OutputGroupPins.Any())
            {
                min.Y = Math.Min(OutputGroupPins.Select(x => x.Position.Y).Min(), min.Y);
            }

            Point offset = new Point();
            if (min.X < 0)
                offset.X = -min.X;
            if (min.Y < 0)
                offset.Y = -min.Y;

            if (!offset.IsEmpty)
            {
                foreach (var module in Elements)
                {
                    var relLoc = module.Bounds.Location;
                    relLoc.Offset(offset);
                    module.Position = relLoc;
                }

                foreach (var grpPin in InputGroupPins)
                {
                    var relLoc = grpPin.Bounds.Location;
                    relLoc.Offset(offset);
                    grpPin.Position = relLoc;
                }

                foreach (var grpPin in OutputGroupPins)
                {
                    var relLoc = grpPin.Bounds.Location;
                    relLoc.Offset(offset);
                    grpPin.Position = relLoc;
                }
            }
        }

        // The upper-left corner of the sub-nodes and floating pinYs defines the origin offset of sub-contents
        // relative to the containing group. This offset is used when expanding groups, 
        // so that the contained sub-nodes are drawn within the expanded space
        private void UpdateOffset()
        {
            Point offset = Point.Empty;
            if (Elements == null)
                return;

            if (Elements.Any())
            {
                int minX = Elements.Min(n => n.Bounds.Location.X);
                int minY = Elements.Min(n => n.Bounds.Location.Y);

                // compensate group pin positions
                if (Inputs.Any())
                    minY = Math.Min(minY, Inputs.Min(p => p.Cast<GroupPin>().Bounds.Location.Y));
                if (Outputs.Any())
                    minY = Math.Min(minY, Outputs.Min(p => p.Cast<GroupPin>().Bounds.Location.Y));

                offset.X = -minX;
                offset.Y = -minY;
            }
            Info.Offset = offset;
        }

        /// <summary>
        /// Handler for event that raised if any of the properties on group are changed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected virtual void GroupInfoChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MinimumSize = Info.MinimumSize;
            ShowExpandedGroupPins = Info.ShowExpandedGroupPins;
        }

        private static IList<Annotation> s_emptyAnnotations = new List<Annotation>().AsReadOnly();

        private PinOrderStyle m_defaultPinOrder = PinOrderStyle.NodeY;

        private DomNodeListAdapter<Element> m_elements;
        private DomNodeListAdapter<Wire> m_wires;
        private DomNodeListAdapter<Annotation> m_annotations;
        private DomNodeListAdapter<GroupPin> m_inputs;
        private DomNodeListAdapter<GroupPin> m_outputs;

        // Maps the index passed into InputPin() and OutputPin() to the correct array index.
        //  If they're null, that's the indicator that they were dirty and need to be rebuilt.
        private int[] m_inputPinsMap, m_outputPinsMap;

        private bool m_dirty;
        private bool m_expanded;
        private CircuitGroupInfo m_info;
    }
}
