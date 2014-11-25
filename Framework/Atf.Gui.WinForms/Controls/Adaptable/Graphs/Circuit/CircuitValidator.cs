//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

using Sce.Atf.Adaptation;

using Sce.Atf.Dom;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Adapter that tracks changes to transitions and updates their routing during validation.
    /// Update transitions on Ending event are part of the transactions themselves, 
    /// then validate all sub-graphs in the current document on Ended event. Requires
    /// Sce.Atf.Dom.ReferenceValidator to be available on the adapted DomNode.</summary>
    public abstract class CircuitValidator : Validator
    {

        /// <summary>
        /// Gets module label attribute</summary>
        protected abstract AttributeInfo ElementLabelAttribute { get; }
        /// <summary>
        /// Gets pin name attribute</summary>
        protected abstract AttributeInfo PinNameAttributeAttribute { get; }


        /// <summary>
        /// Performs initialization when the adapter's node is set</summary>
        protected override void OnNodeSet()
        {
            m_subGraphs = new HashSet<Group>();
            m_circuits = new HashSet<Circuit>();
            m_historyContexts = new HashSet<HistoryContext>();
            foreach (DomNode node in DomNode.Subtree)
            {
                if (CircuitUtil.IsGroupTemplateInstance(node))
                {
                    var template = CircuitUtil.GetGroupTemplate(node);
                    if (template != null)
                    {
                        m_templateInstances.Add(template.DomNode, node);
                        m_subGraphs.Add(template);
                    }
                }
                else if (node.Is<Group>())
                {
                    m_subGraphs.Add(node.Cast<Group>());
                }
                else if (node.Is<Circuit>())
                {
                    m_circuits.Add(node.Cast<Circuit>());
                }

            }

            base.OnNodeSet();

            // Since templates may be externally referenced & edited, better to validate and fix the dangling wires 
            // that were connected to already deleted sub-nodes of a template
            if (m_templateInstances.Keys.Any())
            {
                UpdateWires(m_subGraphs);
                UpdateWires(m_circuits);
            }
        }

        /// <summary>
        /// Gets or sets whether the validation is suspended</summary>
        internal bool Suspended { get; set; }

        /// <summary>
        /// Gets or sets whether an editing operation of moving nodes across containers is occurring</summary>
        internal bool MovingCrossContainer { get; set; }

        internal HistoryContext ActiveHistoryContext { get; set; }

        /// <summary>
        /// Performs custom actions for a node that has been added to the DOM node subtree</summary>
        /// <param name="node">Added node</param>
        protected override void AddNode(DomNode node)
        {
            foreach (HistoryContext historyContext in node.AsAll<HistoryContext>())
            {
                // Disable automatically combining attribute setting operations, as operations such as grouping pin index changes better run its course   
                historyContext.PendingSetOperationLifetime = TimeSpan.Zero;
                m_historyContexts.Add(historyContext);
            }

            base.AddNode(node);
        }

        /// <summary>
        /// Performs custom actions for a node that has been removed from the DOM node subtree</summary>
        /// <param name="node">Removed node</param>
        protected override void RemoveNode(DomNode node)
        {
            foreach (HistoryContext historyContext in node.AsAll<HistoryContext>())
                m_historyContexts.Remove(historyContext);
            base.RemoveNode(node);        
        }


        /// <summary>
        /// Performs custom actions on validation Beginning events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnBeginning(object sender, EventArgs e)
        {
            ActiveHistoryContext = sender.Cast<HistoryContext>();
            m_undoingOrRedoing = m_historyContexts.Any(h => h.UndoingOrRedoing);
            m_nodesInserted.Clear();
            var referenceValidator = DomNode.As<ReferenceValidator>();
            if (referenceValidator != null)
                referenceValidator.Suspended = m_undoingOrRedoing;

            MovingCrossContainer = false;        
        }

        /// <summary>
        /// Performs custom actions after a child is inserted into the DOM node subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildInserted(object sender, ChildEventArgs e)
        {
            AddSubtree(e.Child);
            if (!m_undoingOrRedoing && e.Child.Is<Wire>())
                UpdateGroupPinConnectivity(e.Child.Cast<Wire>());        
        }

        /// <summary>
        /// Performs custom actions after a child is removed from the DOM node subtree</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Child event args</param>
        protected override void OnChildRemoved(object sender, ChildEventArgs e)
        {
            RemoveSubtree(e.Child);
            if (!m_undoingOrRedoing && e.Child.Is<Wire>())
                UpdateGroupPinConnectivity(e.Child.Cast<Wire>());      
        }

       

        /// <summary>
        /// Performs custom actions after an attribute in the DOM node subtree changed</summary>
        /// <param name="sender">Sender (root DOM node)</param>
        /// <param name="e">Attribute change event args</param>
        protected override void OnAttributeChanged(object sender, AttributeEventArgs e)
        {
            if (Validating && !m_undoingOrRedoing)
            {
                if (e.DomNode.Parent.Is<Group>() && e.AttributeInfo == ElementLabelAttribute)
                {
                    var subGraph = e.DomNode.Parent.Cast<Group>();
                    if (e.DomNode.Is<Element>())
                    {
                        // the name of a sub-node has changed, needs to update group pin names that are not manually set
                        SyncGroupPinNamesFromModuleName(subGraph, e.DomNode);
                    }
                }
                else if (e.DomNode.Is<GroupPin>() && e.AttributeInfo == PinNameAttributeAttribute)
                {

                    if (e.DomNode.Parent != null)
                    {
                        var subGraph = e.DomNode.Parent.Cast<Group>();
                        // ensure group pin names are unique at local level
                        UniqueNamer uniqueName = new UniqueNamer();
                        GroupPin childGrpPin = e.DomNode.Cast<GroupPin>();
  
                        foreach (var grpPin in subGraph.InputGroupPins)
                        {
                            if (grpPin != childGrpPin)
                                uniqueName.Name(grpPin.Name);
                        }

                        foreach (var grpPin in subGraph.OutputGroupPins)
                        {
                            if (grpPin != childGrpPin)
                                uniqueName.Name(grpPin.Name);
                        }


                        string unique = uniqueName.Name(childGrpPin.Name);
                        if (unique != childGrpPin.Name)
                            childGrpPin.Name = unique;

                        // try to reset IsDefaultName
                        childGrpPin.IsDefaultName = childGrpPin.Name == childGrpPin.DefaultName(childGrpPin.IsInputSide);
                        UpdateParentGroupPinName(childGrpPin.IsInputSide, childGrpPin);

                    }
                }
            }

            base.OnAttributeChanged(sender,e);
        }

        /// <summary>
        /// Propagate up group pin name changes to the parent pin (only need to take care of non-default name,
        /// the default names are auto-updated in Group.UpdateGroupPins())</summary>
        private void UpdateParentGroupPinName(bool inputSide, GroupPin childPin)
        {
            var parentGrpPin = childPin.GetAncestry(inputSide).FirstOrDefault();
            if (parentGrpPin != null)
            {
                if (!childPin.IsDefaultName && parentGrpPin.IsDefaultName)
                {

                    parentGrpPin.Name = childPin.Name;
                    parentGrpPin.IsDefaultName = false;
                }
            }
        }

        /// <summary>
        /// Update group pin names after the name change of the connected node</summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        private void SyncGroupPinNamesFromModuleName(Group group, DomNode node)
        {
            foreach (GroupPin grpPin in group.Inputs)
            {
                if (grpPin.InternalElement.DomNode != node) continue;
                if (grpPin.IsDefaultName)
                    grpPin.Name = grpPin.InternalElement.Name + ":" + grpPin.InternalElement.AllInputPins.ElementAt(grpPin.InternalPinIndex).Name;
            }

            foreach (GroupPin grpPin in group.Outputs)
            {
                if (grpPin.InternalElement.DomNode != node) continue;
                if (grpPin.IsDefaultName)
                    grpPin.Name = grpPin.InternalElement.Name + ":" + grpPin.InternalElement.AllOutputPins.ElementAt(grpPin.InternalPinIndex).Name;

            }
        }

        /// <summary>
        /// Performs custom actions on validation Ending events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnding(object sender, EventArgs e)
        {
            var referenceValidator = DomNode.As<ReferenceValidator>();
            if (referenceValidator != null)
                referenceValidator.Suspended = false;


            if (m_undoingOrRedoing)
            {
                foreach (var subgraph in m_subGraphs.Where(x => x.Dirty).OrderByDescending(s => s.Level))
                {
                    subgraph.Dirty = false; // just reset Dirty flag
                    foreach (var wire in subgraph.Wires)
                    {
                        // reset pin target
                        wire.InputPinTarget = null;
                        wire.OutputPinTarget = null;
                    }
                   
                
                    subgraph.UpdateGroupPinInfo();    
                    subgraph.OnChanged(EventArgs.Empty); // but notify the change
                }
                    
                foreach (var circuit in m_circuits)
                {
                    foreach (var wire in circuit.Wires)
                    {
                        // reset pin target
                        wire.InputPinTarget = null;
                        wire.OutputPinTarget = null;
                    }
                    circuit.Dirty = false;
                }
                    
                return;
            }

            var containersToCheck = new List<ICircuitContainer>();
            containersToCheck.AddRange(m_subGraphs.Where(g => g.Dirty).AsIEnumerable<ICircuitContainer>());
            containersToCheck.AddRange(m_circuits.Where(g => g.Dirty).AsIEnumerable<ICircuitContainer>());

            while (m_subGraphs.Any(n => n.Dirty) || m_circuits.Any(n => n.Dirty))
            {
                // inner subgraphs updated first
                foreach (var subgraph in m_subGraphs.OrderByDescending(s => s.Level))
                {
                    subgraph.IgnoreFanInOut = MovingCrossContainer;
                    subgraph.Update();
                    if (subgraph.IgnoreFanInOut)
                    {
                        subgraph.IgnoreFanInOut = false;
                        subgraph.Dirty = true;
                    }
                }
                UpdateWires(containersToCheck);
                MovingCrossContainer = false;

                foreach (var circuit in m_circuits)
                {
                    circuit.Update();
                }
            }

            // update group pin connectivity and other info from bottom up, for display purpose only
            foreach (var group in containersToCheck.AsIEnumerable<Group>().OrderByDescending(s => s.Level))
            {
                group.UpdateGroupPinInfo();
            }
   
            foreach (var subgraph in m_nodesInserted.Keys)
            {
                IEnumerable<Element> nodes = m_nodesInserted[subgraph];
                if (nodes.Any())
                {
                    var viewingContext = subgraph.Cast<ViewingContext>();
                    if (viewingContext.Control != null)
                    {
                        var subGraphPinAdapter = viewingContext.Control.As<GroupPinEditor>();
                        if (subGraphPinAdapter != null)
                            subGraphPinAdapter.AdjustLayout(nodes, EmptyEnumerable<GroupPin>.Instance, new Point(0, 0));
                    }
                }
            } 
        }

        // update edge routes
        private void UpdateWires(IEnumerable<ICircuitContainer> containers )
        {
            foreach (var container in containers)
            {
                foreach (var wire  in container.Wires.ToArray())
                {
                    //string edgeName = CircuitUtil.GetDomNodeName(wire.DomNode);
                    bool input = container.Elements.Contains(wire.InputElement);
                    bool output = container.Elements.Contains(wire.OutputElement);

                    if (input && output) // still an internal edge of the container (not a container-crossing link)
                    {
                        if (wire.InputElement.Type is MissingElementType || wire.OutputElement.Type is MissingElementType)
                            continue; // skip wires connected to missing types
                        
                        var matchedInput = new Pair<Element, ICircuitPin>();
                        foreach (var module in container.Elements)
                        {
                            matchedInput = module.FullyMatchPinTarget(wire.InputPinTarget, true);                          
                            if (matchedInput.First != null)
                                break;                                                     
                        }

                        var matchedOutput = new Pair<Element, ICircuitPin>();
                        foreach (var module in container.Elements)
                        {
                            matchedOutput = module.FullyMatchPinTarget(wire.OutputPinTarget, false);
                            if (matchedOutput.First != null)
                                break;
                        }

                        if (matchedInput.First != null && matchedOutput.First != null)
                        {
                            wire.InputElement = matchedInput.First;
                            wire.InputPin = matchedInput.Second;

                            Debug.Assert(matchedOutput.First != null);
                            wire.OutputElement = matchedOutput.First;
                            wire.OutputPin = matchedOutput.Second;
                            continue;
                        }
                    }

                    if (MovingCrossContainer)
                    {
                        var leafNodeIn = wire.InputPinTarget.InstancingNode ?? wire.InputPinTarget.LeafDomNode;
                        var leafNodeOut = wire.OutputPinTarget.InstancingNode ?? wire.OutputPinTarget.LeafDomNode;
                        var newParent = DomNode.GetLowestCommonAncestor(leafNodeIn,leafNodeOut);
                        if (newParent == null)
                            continue;
                        var newParentContainer = newParent.Cast<ICircuitContainer>();

                        var matchedInput = newParentContainer.FullyMatchPinTarget(wire.InputPinTarget, true);
                        Debug.Assert(matchedInput.First != null);
                        var matchedOutput = newParentContainer.FullyMatchPinTarget(wire.OutputPinTarget, false);
                        Debug.Assert(matchedOutput.First != null); 

                        if (newParentContainer.Is<Group>()) // the edge should connect to child nodes of the parent container
                        {
                            var grpPin = matchedInput.Second.Cast<GroupPin>();
                            wire.InputElement = grpPin.InternalElement;
                            wire.InputPin = grpPin.InternalElement.AllInputPins.ElementAt(grpPin.InternalPinIndex);
                        }
                        else
                        {
                            wire.InputElement = matchedInput.First;
                            wire.InputPin = matchedInput.Second;
                        }

                        if (newParentContainer.Is<Group>())
                        {
                            var grpPin = matchedOutput.Second.Cast<GroupPin>();
                            wire.OutputElement = grpPin.InternalElement;
                            wire.OutputPin = grpPin.InternalElement.AllOutputPins.ElementAt(grpPin.InternalPinIndex);
                        }
                        else
                        {
                            wire.OutputElement = matchedOutput.First;
                            wire.OutputPin = matchedOutput.Second;
                        }

                        if (container != newParentContainer)
                        {
                            container.Wires.Remove(wire);
                            newParentContainer.Wires.Add(wire);
                        }
                    }
                    else
                    {
                        // the edge has invalid route, must be caused by nodes deletion down the container hierarchy
                        container.Wires.Remove(wire);
                    }
                                       
                }
            }       
        }

        /// <summary>
        /// Performs custom actions on validation Ended events</summary>
        /// <param name="sender">Validation context</param>
        /// <param name="e">Event args</param>
        protected override void OnEnded(object sender, EventArgs e)
        {
            ActiveHistoryContext = null;

#if DEBUG
            foreach (var subgraph in m_subGraphs.OrderByDescending(s => s.Level))
            {
                subgraph.Validate();
                ValidateEdges(subgraph);
            }

            foreach (var circuit in m_circuits)
                ValidateEdges(circuit);
#endif
            m_undoingOrRedoing = false;
        }
         

        /// <summary>
        /// Validate edges of the graph</summary>
        /// <param name="graph">Graph with edges to be validated</param>
        private static void ValidateEdges(IGraph<Element, Wire, ICircuitPin> graph)
        {
            foreach (var edge in graph.Edges)
            {
                if (edge.InputElement.Type is MissingElementType || edge.OutputElement.Type is MissingElementType)
                    continue; // skip wires connected to missing types

                // an edge should connect two nodes at the same level
                //Debug.Assert(edge.InputElement.Level == edge.OutputElement.Level,
                //    string.Format(CircuitUtil.GetDomNodeName(edge.DomNode)) + "does not connect two nodes at the same level");

                // if the link connects to a group pin, verify they targets to the same leaf node and pin index
                if (edge.InputElement.Is<Group>())
                {
                    Group nestedSubGraph;
                    if (edge.InputElement.Is<IReference<Group>>())
                    {
                        var reference = edge.InputElement.As<IReference<Group>>();
                        nestedSubGraph = reference.Target;
                    }
                    else
                        nestedSubGraph = edge.InputElement.Cast<Group>();         
                  
                    var grpPin = nestedSubGraph.InputGroupPins.First(x => x.Index == edge.InputPin.Index);
                    bool sameLeafNode = grpPin.PinTarget == edge.InputPinTarget;
                    if (!sameLeafNode) // could this be a valid case for (deep)copy group nodes
                    {
                        var name1 = grpPin.PinTarget.LeafDomNode.Cast<Element>().Name;
                        var name2 = edge.InputPinTarget.LeafDomNode.Cast<Element>().Name;
                        sameLeafNode = name1 == name2;
                    }

                    Debug.Assert(sameLeafNode, "Group pin references the same input pin target ");
                }
                else
                {
                    if (edge.InputPinTarget != null) // occurs undo grouping
                    {
                        if (edge.InputPinTarget.InstancingNode == null)
                            Debug.Assert(edge.InputElement.DomNode == edge.InputPinTarget.LeafDomNode,
                                           "Top level graph edge should reference Dom node directly ");
                        else
                        {
                            //TODO
                        }

                        Debug.Assert(edge.InputPin.Index == edge.InputPinTarget.LeafPinIndex,
                                           "Top level graph edge should reference node pin index directly ");
                    }
                }


                if (edge.OutputElement.Is<Group>())
                {
                    Group nestedSubGraph;
                    if (edge.OutputElement.Is<IReference<Group>>())
                    {
                        var reference = edge.OutputElement.As<IReference<Group>>();
                        nestedSubGraph = reference.Target;
                    }
                    else
                        nestedSubGraph = edge.OutputElement.Cast<Group>();                  
                   
                    var grpPin = nestedSubGraph.OutputGroupPins.First(x => x.Index == edge.OutputPin.Index);
                    bool sameLeafNode = grpPin.PinTarget == edge.OutputPinTarget;
                    if (!sameLeafNode) // could this be a valid case for (deep)copy group nodes
                    {
                        var name1 = grpPin.PinTarget.LeafDomNode.Cast<Element>().Name;
                        var name2 = edge.OutputPinTarget.LeafDomNode.Cast<Element>().Name;
                        sameLeafNode = name1 == name2;
                    }
                    Debug.Assert(sameLeafNode, "Group pin and edge pin reference the same output pin target");
                    
                }
                else
                {
                    if (edge.OutputPinTarget != null)
                    {
                        if (edge.OutputPinTarget.InstancingNode == null)
                            Debug.Assert(edge.OutputElement.DomNode == edge.OutputPinTarget.LeafDomNode,
                                "Top level graph edge should reference Dom node directly ");
                        else
                        {
                            //TODO
                        }
                        Debug.Assert(edge.OutputPin.Index == edge.OutputPinTarget.LeafPinIndex,
                          "Top level graph edge should reference node pin index directly ");

                    }
                }
            }

        }

        private void AddSubtree(DomNode root)
        {
            foreach (DomNode node in root.Subtree)
            {
                if (CircuitUtil.IsGroupTemplateInstance(node))
                {
                    var template = CircuitUtil.GetGroupTemplate(node);
                    if (template != null) // if the template is not missing
                    {
                        m_templateInstances.Add(template.DomNode, node);
                        m_subGraphs.Add(template);
                    }
                }
                else if (node.Is<Group>())
                    m_subGraphs.Add(node.Cast<Group>());
                else if (node.Is<Circuit>())
                    m_circuits.Add(node.Cast<Circuit>());
            }
        }

        private void RemoveSubtree(DomNode root)
        {
            foreach (DomNode node in root.Subtree)
            {
                if (CircuitUtil.IsGroupTemplateInstance(node))
                {
                    var template = CircuitUtil.GetGroupTemplate(node);
                    if (template != null)// if the template is not missing
                        m_templateInstances.Remove(template.DomNode, node);                 
                }
                else if (node.Is<Group>())
                    m_subGraphs.Remove(node.Cast<Group>());
                else if (node.Is<Circuit>())
                    m_circuits.Remove(node.Cast<Circuit>());
            }
        }

        /// <summary>
        /// Updates the template info (to be reflected in UI)</summary>
        /// <param name="template">Group template</param>
        /// <remarks>Currently only update group pin connectivity for the group template</remarks>
        public void UpdateTemplateInfo(Group template)
        {
            // for template editing, the  group pin connectivity should be updated 
            // by scanning  all wires of all graph containers that share it by referencing
            var containersToCheck = new List<ICircuitContainer>();
            foreach (var templateInstance in m_templateInstances[template.DomNode])
            {
                var parentContainer = templateInstance.Parent.As<ICircuitContainer>();
                if (parentContainer != null)
                    containersToCheck.Add(parentContainer);
            }

            foreach (var grpPin in template.InputGroupPins)
            {
                grpPin.Info.ExternalConnected = false;
                foreach (var subgraph in containersToCheck)
                {
                    if (grpPin.Info.ExternalConnected)
                        break;
                    foreach (var wire in subgraph.Wires)
                    {
                        if (wire.InputPinTarget == grpPin.PinTarget)
                        {
                            grpPin.Info.ExternalConnected = true;
                            grpPin.Visible = true;
                            break;
                        }
                    }
                }
                
              
            }

            foreach (var grpPin in template.OutputGroupPins)
            {
                grpPin.Info.ExternalConnected = false;
                foreach (var subgraph in containersToCheck)
                {
                    if (grpPin.Info.ExternalConnected)
                        break;
                    foreach (var wire in subgraph.Wires)
                    {
                        if (wire.OutputPinTarget == grpPin.PinTarget)
                        {
                            grpPin.Info.ExternalConnected = true;
                            grpPin.Visible = true;
                            break;
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Updates the pin external connectivity of the connecting group</summary>
        /// <param name="wire">Wire that has been added or removed in the DOM node tree</param>
        private void UpdateGroupPinConnectivity(Wire wire)
        {
            if (wire.InputElement == null || wire.OutputElement == null)
                return;

            var updatedNodes = new List<DomNode>();
            // need to update the pin external connectivity of the connecting group       
            if (CircuitUtil.IsGroupTemplateInstance(wire.InputElement.DomNode))
            {
                var template = CircuitUtil.GetGroupTemplate(wire.InputElement.DomNode);
                if (template != null) // if the template is not missing
                {
                    updatedNodes.Add(template.DomNode);
                    UpdateTemplateInfo(template);
                }              
            }
            else if (wire.InputElement.DomNode.Is<Group>())
            {
                updatedNodes.Add(wire.InputElement.DomNode);
                wire.InputElement.DomNode.Cast<Group>().UpdateGroupPinInfo();
            }

            if (CircuitUtil.IsGroupTemplateInstance(wire.OutputElement.DomNode))
            {
                var template = CircuitUtil.GetGroupTemplate(wire.OutputElement.DomNode);
                if (template != null) // if the template is not missing
                {
                    updatedNodes.Add(template.DomNode);
                    UpdateTemplateInfo(template);
                }              
            }
            else if (wire.OutputElement.DomNode.Is<Group>())
            {
                updatedNodes.Add(wire.OutputElement.DomNode);
                wire.OutputElement.DomNode.Cast<Group>().UpdateGroupPinInfo();
            }

            // let's assume all the updated groups have changed connectivity, and notify the graph adaptors
            foreach (var group in updatedNodes)
            {
                var editingContext = group.As<CircuitEditingContext>();
                if (editingContext != null)
                    editingContext.NotifyObjectChanged(group);
            }
        }

        private bool m_undoingOrRedoing;
        private HashSet<Group> m_subGraphs;  // all subgraphs in the document
        private HashSet<Circuit> m_circuits;  // all circuits in the document
        private HashSet<HistoryContext> m_historyContexts;
        private Multimap<Group, Element> m_nodesInserted = new Multimap<Group, Element>();
        // key is template DomNode, values are the instances of the template
        private Multimap<DomNode, DomNode> m_templateInstances = new Multimap<DomNode, DomNode>();
  
    }
}
