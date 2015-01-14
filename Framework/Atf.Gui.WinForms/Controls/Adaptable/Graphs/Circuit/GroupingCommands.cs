//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;
using Sce.Atf.Input;

namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Component that defines circuit-specific commands for group and ungroup. Grouping takes
    /// modules and the connections between them and turns those into a single element that is equivalent.</summary>
    [InheritedExport(typeof(IInitializable))]
    [InheritedExport(typeof(IContextMenuCommandProvider))]
    [InheritedExport(typeof(GroupingCommands))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public abstract class GroupingCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
    {
        // required DomNodeType
        /// <summary>
        /// Gets type for Group</summary>
        protected abstract DomNodeType GroupType { get; }

        /// <summary>
        /// Gets or sets the default pin order style</summary>
        public Group.PinOrderStyle DefaultPinOrderStyle
        {
            get { return m_defaultPinOrderStyle; }
            set { m_defaultPinOrderStyle = value; }
        }

        /// <summary>
        /// Enums for behavior of group pins</summary>
        public enum GroupCreationOptions
        {
            /// <summary>
            /// Specifies that default behavior should be used</summary>
            None,
            /// <summary>
            /// Only expose connected group pins</summary>
            HideUnconnectedPins
        }
           
        /// <summary>
        /// Gets or sets group creation options</summary>
        public GroupCreationOptions CreationOptions { get; set; }

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public GroupingCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            Placement = PlacementMode.UpperLeft;
        }

        /// <summary>
        /// Populates a newly created group with circuit elements that are currently in the given graph</summary>
        /// <param name="newGroup">A new group, empty of circuit elements</param>
        /// <param name="elementsToGroup">The circuit elements to move into 'newGroup'.</param>
        /// <param name="graphContainer">The container for the circuit elements and their wires. These will
        /// be removed from 'graphContainer' and placed into 'newGroup'.</param>
        /// <remarks>This method is intended to help with persistence of circuit groups.</remarks>
        public static void CreateGroup(Group newGroup, IEnumerable<object> elementsToGroup, ICircuitContainer graphContainer)
        {
            // get the selected modules and the connections between them
            HashSet<Element> modules = new HashSet<Element>();
            List<Wire> internalConnections = new List<Wire>();
            List<Wire> externalConnections = new List<Wire>();

            CircuitUtil.GetSubGraph(graphContainer, elementsToGroup, modules, internalConnections, externalConnections, externalConnections);

            // the group must be added before transferring modules and connections to it,
            //  so that the history mechanism will capture all the changes.
            graphContainer.Elements.Add(newGroup);

            // transfer modules
            foreach (Element module in modules)
            {
                graphContainer.Elements.Remove(module);
                newGroup.Elements.Add(module);
            }

            // auto-generate sub-graph group pins to support the external connections to group
            // group pins may have multiple external connections
            newGroup.UpdateGroupPins(modules, internalConnections, externalConnections);

            // transfer internal connections (those between grouped modules)
            foreach (Wire connection in internalConnections)
            {
                graphContainer.Wires.Remove(connection);
                newGroup.Wires.Add(connection);
            }

            // initalize group pin's index and pinY
            newGroup.InitializeGroupPinIndexes(internalConnections);

            if (graphContainer.Is<Group>()) // making a group inside a group
            {
                // remap group pins in the parent group
                var parentGroup = graphContainer.Cast<Group>();

                // remap parent group pins that reference the new group's subnodes to the new group
                foreach (var grpPin in parentGroup.InputGroupPins)
                {
                    if (modules.Contains(grpPin.InternalElement))
                    {
                        // adjust the internal pin index first 
                        for (int j = 0; j < newGroup.Inputs.Count; ++j)
                        {
                            var newGrpPin = newGroup.Inputs[j] as GroupPin;
                            if (newGrpPin.InternalElement.DomNode == grpPin.InternalElement.DomNode &&
                                newGrpPin.InternalPinIndex == grpPin.InternalPinIndex)
                            {
                                grpPin.InternalPinIndex = j;
                                newGrpPin.Name = grpPin.Name;
                                //grpPin.Name = newGroup.Name + ":" + newGrpPin.Name;

                                break;
                            }
                        }
                        // now update node references for the parent group pin
                        grpPin.InternalElement = newGroup;
                    }
                }

                foreach (var grpPin in parentGroup.OutputGroupPins)
                {
                    if (modules.Contains(grpPin.InternalElement))
                    {
                        // adjust the internal pin index first 
                        for (int j = 0; j < newGroup.Outputs.Count; ++j)
                        {
                            var newGrpPin = newGroup.Outputs[j] as GroupPin;
                            if (newGrpPin.InternalElement.DomNode == grpPin.InternalElement.DomNode &&
                                newGrpPin.InternalPinIndex == grpPin.InternalPinIndex)
                            {
                                grpPin.InternalPinIndex = j;
                                newGrpPin.Name = grpPin.Name;
                                //grpPin.Name = newGroup.Name + ":" + newGrpPin.Name;

                                break;
                            }
                        }
                        // now update node references for the parent group pin
                        grpPin.InternalElement = newGroup;
                    }
                }

            }

            newGroup.OnChanged(EventArgs.Empty); // notify the change( derived class of Group may need custom actions)

            // Remap external connections from grouped modules to group.
            foreach (Wire connection in externalConnections)
            {
                var groupInputPin = newGroup.MatchedGroupPin(connection.InputElement, connection.InputPin.Index, true);
                if (groupInputPin != null)
                {
                    groupInputPin.SetPinTarget(true);

                    // reroute original edge 
                    connection.SetInput(newGroup, groupInputPin);
                    connection.InputPinTarget = groupInputPin.PinTarget;
                }

                var groupOutputPin = newGroup.MatchedGroupPin(connection.OutputElement, connection.OutputPin.Index, false);
                if (groupOutputPin != null)
                {
                    groupOutputPin.SetPinTarget(false);

                    // reroute original edge 
                    connection.SetOutput(newGroup, groupOutputPin);
                    connection.OutputPinTarget = groupOutputPin.PinTarget;
                }
            }


            // find upper-left corner of the subnodes
            Point minLocation = new Point(int.MaxValue, int.MaxValue);
            foreach (var module in newGroup.Elements)
            {
                if (minLocation.X > module.Bounds.Location.X)
                    minLocation.X = module.Bounds.Location.X;
                if (minLocation.Y > module.Bounds.Location.Y)
                    minLocation.Y = module.Bounds.Location.Y;
            }
            // offset sub-nodes location so they are relative to the parent
            foreach (var module in newGroup.Elements)
            {
                var relLoc = module.Bounds.Location;
                relLoc.Offset(-minLocation.X, -minLocation.Y);
                module.Bounds = new Rectangle(relLoc, module.Bounds.Size);
                module.Position = module.Bounds.Location;
            }
        }

        /// <summary>
        /// Ungroups the given group, causing its child elements to be moved to the given container</summary>
        /// <param name="group">The group to destroy while preserving its child elements</param>
        /// <param name="circuitContainer">The container that currently holds the group</param>
        /// <remarks>This method is intended to help with persistence of circuit groups.</remarks>
        public static void UngroupGroup(Group group, ICircuitContainer circuitContainer)
        {
            // restore external connections to modules in group
            foreach (Wire connection in circuitContainer.Wires)
            {
                if (connection.InputElement.As<Group>() == group)
                {
                    var pin = connection.InputPin.As<GroupPin>();
                    Element element = pin.InternalElement;
                    connection.SetInput(element, element.AllInputPins.ElementAt(pin.InternalPinIndex));
                }
                else if (connection.OutputElement.As<Group>() == group)
                {
                    var pin = connection.OutputPin.As<GroupPin>();
                    Element element = pin.InternalElement;
                    connection.SetOutput(element, element.AllOutputPins.ElementAt(pin.InternalPinIndex));
                }
            }

            // restore modules
            IList<Element> modules = group.Elements;
            for (int i = modules.Count - 1; i >= 0; i--)
            {
                Element element = modules[i];
                modules.RemoveAt(i);
                // location restores to parent space
                element.Bounds = new Rectangle(element.Bounds.Location.X + group.Bounds.Location.X - group.Info.Offset.X,
                                              element.Bounds.Location.Y + group.Bounds.Location.Y - group.Info.Offset.Y,
                                              element.Bounds.Width, element.Bounds.Height);

                circuitContainer.Elements.Add(element);
            }

            // restore internal connections
            IList<Wire> connections = group.Wires;
            for (int i = connections.Count - 1; i >= 0; i--)
            {
                Wire wire = connections[i];
                connections.RemoveAt(i);
                circuitContainer.Wires.Add(wire);
            }

            // remove group
            circuitContainer.Elements.Remove(group);           
        }

        /// <summary>
        /// Enum for group placement</summary>
        internal enum PlacementMode
        {
            /// <summary>
            /// Newly created group node is placed at the center of grouped nodes</summary>
            Center,
            /// <summary>
            /// Newly created group node is placed at upper-left corner of the grouped nodes</summary>
            UpperLeft,
        }

        /// <summary>
        /// Gets or sets group placement</summary>
        internal static PlacementMode Placement { get; set; }

        //public enum GroupPinAutoCreationMode
        //{
        //    /// <summary>
        //    ///  a group pin is created for a sub-node pin that is either connected to external, or empty(not link)
        //    /// </summary>
        //    ExternalOrEmpty,
        //    /// <summary>
        //    ///  a group pin is only created for a sub-node pin that is connected to an external node
        //    /// </summary>
        //    ExternalOnly,
        //}

        private enum CommandTag
        {
            HideUnconnectedPins,   // only expose connected group pins
            ShowExpandedGroupPins, // whether to draw the orange box on the left and right borders of  an expanded group that represents the group pin
            TogglePinVisibility,   // in-place group pin visibility toggling - not supported yet
            ResetGroupPinNames,
            EdgeStyleDefault,
            EdgeStyleDirectCurve,
        }

        /// <summary>
        /// Gets IContextRegistry</summary>
        protected IContextRegistry ContextRegistry { get { return m_contextRegistry; } }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering grouping commands</summary>
        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(CommandInfo.EditGroup, this);
            m_commandService.RegisterCommand(CommandInfo.EditUngroup, this);

            var showExpandedGroupPins = m_commandService.RegisterCommand(
                CommandTag.ShowExpandedGroupPins,
                StandardMenu.Edit,
                StandardCommandGroup.EditOther,
                "Show Expanded Group Pins".Localize(),
                "Show Expanded Group Pins".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);
            showExpandedGroupPins.CheckOnClick = true;

            m_commandService.RegisterCommand(
                CommandTag.ResetGroupPinNames,
                StandardMenu.Edit,
                StandardCommandGroup.EditOther,
                "Reset Group Pin Names".Localize(),
                "Reset Group Pin Names".Localize(),
                Keys.None,
                null,
                CommandVisibility.ContextMenu,
                this);

            //m_commandService.RegisterCommand(
            //   CommandTag.TogglePinVisibility,
            //   StandardMenu.Edit,
            //   StandardCommandGroup.EditOther,
            //   "Toggle Pin Visibility".Localize(),
            //   "Toggle Pin Visibility".Localize(),
            //   Keys.None,
            //   null,
            //   CommandVisibility.ContextMenu,
            //   this);

            var hideUnconnectedPins = m_commandService.RegisterCommand(
              CommandTag.HideUnconnectedPins,
              StandardMenu.Edit,
              StandardCommandGroup.EditOther,
              "Hide Unconnected Pins".Localize(),
              "Hide Unconnected Pins".Localize(),
              Keys.None,
              null,
              CommandVisibility.ContextMenu,
              this);
            hideUnconnectedPins.CheckOnClick = true;

            //m_commandService.RegisterCommand(
            //  CommandTag.EdgeStyleDefault,
            //  StandardMenu.Edit,
            //  StandardCommandGroup.EditOther,
            //  "Edge Style/Default".Localize("The '/' indicates a sub-menu"),
            //  "Default Edge Style".Localize(),
            //  Keys.None,
            //  null,
            //  CommandVisibility.ContextMenu,
            //  this);

            //m_commandService.RegisterCommand(
            //      CommandTag.EdgeStyleDirectCurve,
            //      StandardMenu.Edit,
            //      StandardCommandGroup.EditOther,
            //      "Edge Style/Direct Curve".Localize(The '/' indicates a sub-menu),
            //      "Direct Curve".Localize(),
            //      Keys.None,
            //      null,
            //      CommandVisibility.ContextMenu,
            //      this);
            if (m_scriptingService != null)
            {
                m_scriptingService.SetVariable("grpCmds", this);
            }
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public virtual bool CanDoCommand(object commandTag)
        {
            bool enabled = false;
            var context = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
            if (context != null)
            {
                var selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
                if (commandTag is StandardCommand )
                {
                    if (StandardCommand.EditGroup.Equals(commandTag))
                    {
                       var lastSelected = selectionContext.GetLastSelected<Element>();
                        enabled = (lastSelected != null) && // at least one Module selected
                                  selectionContext.Selection.All( // selected elements should have a common parent(i.e. belong to the same container) 
                                      x => x.Is<Element>() && x.Cast<DomNode>().Parent == lastSelected.DomNode.Parent);
                        if (enabled)
                            enabled = selectionContext.Selection.All(x => !CircuitUtil.IsTemplateTargetMissing(x)); 
                        if (enabled && (!context.SupportsNestedGroup))
                        {    
                            // if nested group is not supported, then neither any selected item can be a group, 
                            // nor any of their ancestry
                            enabled=  selectionContext.Selection.All(x => !x.Is<Group>());
                            if (enabled)
                                enabled = lastSelected.DomNode.Ancestry.All(x => !x.Is<Group>());
                        }
                    }
                    else if (StandardCommand.EditUngroup.Equals(commandTag))
                    {
                       
                        enabled = selectionContext.Selection.Any() &&
                            selectionContext.Selection.All(x => x.Is<Group>() &&  // selected are all are group instances
                               !(x.Is<IReference<DomNode>>())  // disallow template instances as they suppose acting atomically 
                            );                      
                    }                  
                }
                else if (commandTag is CommandTag)
                {
                    if (CommandTag.ShowExpandedGroupPins.Equals(commandTag))
                    {
                        enabled = m_targetRef != null && m_targetRef.Target.Is<Group>() &&
                            m_targetRef.Target.Cast<Group>().Expanded;
                    }
                    else if (CommandTag.ResetGroupPinNames.Equals(commandTag))
                    {
                        enabled = m_targetRef != null && m_targetRef.Target.Is<Group>() && (m_targetRef.Target is IReference<DomNode>);
                        if (enabled)
                            enabled = !CircuitUtil.IsTemplateTargetMissing(m_targetRef.Target);
                    }
                    else if (CommandTag.HideUnconnectedPins.Equals(commandTag))
                    {
                        enabled = m_targetRef != null && m_targetRef.Target.Is<Group>();
                        if (enabled)
                            enabled = !CircuitUtil.IsTemplateTargetMissing(m_targetRef.Target);
                    }
                    else if (CommandTag.TogglePinVisibility.Equals(commandTag))
                    {
                        enabled = m_targetRef != null && CanDoTogglePinVisibility(context, m_targetRef.Target);
                    }
                    else if (CommandTag.EdgeStyleDefault.Equals(commandTag))
                    {
                        enabled = true;
                    }
                    else if (CommandTag.EdgeStyleDirectCurve.Equals(commandTag))
                    {
                        enabled = true;
                    }
   
                }
            }

            return enabled;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public virtual void DoCommand(object commandTag)
        {
            var circuitEditingContext = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
            var selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
            var viewingContext = m_contextRegistry.GetActiveContext<ViewingContext>();
            var transactionContext = m_contextRegistry.GetActiveContext<ITransactionContext>();
            if (transactionContext == null)
                transactionContext = circuitEditingContext; //fallback 
            if (commandTag is StandardCommand)
            {
                if (StandardCommand.EditGroup.Equals(commandTag))
                {
                    transactionContext.DoTransaction(() => CreateGroup(selectionContext, viewingContext), "Group".Localize("a verb"));
                }
                else if (StandardCommand.EditUngroup.Equals(commandTag))
                {
                    transactionContext.DoTransaction(() => UngroupGroups(circuitEditingContext, selectionContext), "Ungroup".Localize("a verb"));
                }
            }
            else if (commandTag is CommandTag)
            {
                if (CommandTag.ShowExpandedGroupPins.Equals(commandTag))
                {
                    transactionContext.DoTransaction(() => ToggleShowExpandedGroupPins(viewingContext),
                        "Toggle Show Group Pins When Expanded".Localize());
                }
                else if (CommandTag.ResetGroupPinNames.Equals(commandTag))
                {
                    transactionContext.DoTransaction(() => GroupResetPinNames(), "Reset Group Pin Names".Localize());
                }
                else if (CommandTag.HideUnconnectedPins.Equals(commandTag))
                {
                    transactionContext.DoTransaction(() => ToggleHideUnconnectedPins(), "Hide Unconnected Pins".Localize());
                }
                //else if (CommandTag.EdgeStyleDirectCurve.Equals(commandTag))
                //{
                //    foreach (var connection in context.CircuitContainer.Wires)
                //        connection.Cast<WireStyleProvider>().EdgeStyle = EdgeStyle.DirectCurve;
                //}
                //else if (CommandTag.EdgeStyleDefault.Equals(commandTag))
                //{
                //    foreach (var connection in context.CircuitContainer.Wires)
                //        connection.Cast<WireStyleProvider>().EdgeStyle = EdgeStyle.Default;
                //}
            }         
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
            if (commandTag is CommandTag)
            {
               
                if (commandTag.Equals(CommandTag.ResetGroupPinNames))
                {
                    if (m_targetRef != null && m_targetRef.Target != null)
                    {
                        object target = m_targetRef.Target;
                        if (target.Is<Group>() && !CircuitUtil.IsTemplateTargetMissing(target))
                        {
                            var group = target.Cast<Group>();
                            state.Text = string.Format("Reset Pin Names on \"{0}\"".Localize(), group.Name);
                        }
                    }
                }
                else if (commandTag.Equals(CommandTag.ShowExpandedGroupPins))
                {
                    if (m_targetRef != null && m_targetRef.Target != null)
                    {
                        object target = m_targetRef.Target;
                        if (target.Is<Group>())
                        {
                            var group = target.Cast<Group>();
                            state.Check = group.Info.ShowExpandedGroupPins;
                            state.Text = string.Format("Show Expanded Group Pins on \"{0}\"".Localize(), group.Name);
                        }
                    }
                }
                else if (commandTag.Equals(CommandTag.HideUnconnectedPins))
                {
                    if (m_targetRef != null && m_targetRef.Target != null)
                    {
                        object target = m_targetRef.Target;
                        if (target.Is<Group>() && !CircuitUtil.IsTemplateTargetMissing(target))
                        {
                            var group = target.Cast<Group>();
                            var graphContainer = group.ParentGraph.As<ICircuitContainer>();
                            if (graphContainer != null)
                            {
                                // check if all unconnected pins are hidden
                                m_allUnconnectedHidden = true;
                                foreach (var grpPin in group.InputGroupPins)
                                {
                                    bool externalConectd =
                                        graphContainer.Wires.FirstOrDefault(
                                            x => x.InputPinTarget.FullyEquals(grpPin.PinTarget)) != null;
                                    if (!externalConectd && grpPin.Visible)
                                    {
                                        m_allUnconnectedHidden = false;
                                        break;
                                    }
                                }
                                if (m_allUnconnectedHidden)
                                {
                                    foreach (var grpPin in group.OutputGroupPins)
                                    {
                                        bool externalConectd =
                                            graphContainer.Wires.FirstOrDefault(
                                                x => x.OutputPinTarget.FullyEquals(grpPin.PinTarget)) != null;
                                        if (!externalConectd && grpPin.Visible)
                                        {
                                            m_allUnconnectedHidden = false;
                                            break;
                                        }
                                    }
                                }

                                state.Check = m_allUnconnectedHidden;
                                state.Text = string.Format("Hide Unconnected Pins on \"{0}\"".Localize(), group.Name);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region IContextMenuCommandProvider Members

        /// <summary>
        /// Gets tags for context menu (right click) commands</summary>
        /// <param name="context">Context containing target object</param>
        /// <param name="target">Right clicked object, or null if none</param>
        IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
        {
            m_targetRef = null;
            if (context.Is<CircuitEditingContext>())
            {
                m_targetRef = new WeakReference(target);

                if (CanDoCommand(StandardCommand.EditGroup))
                    yield return StandardCommand.EditGroup;

                if (CanDoCommand(StandardCommand.EditUngroup))
                    yield return StandardCommand.EditUngroup;

                if (CanToggleShowExpandedGroupPins(context, target))
                    yield return CommandTag.ShowExpandedGroupPins;
                if (target.Is<Group>())
                    yield return CommandTag.ResetGroupPinNames;
                if (CanDoTogglePinVisibility(context, target))
                    yield return CommandTag.TogglePinVisibility;

                yield return CommandTag.HideUnconnectedPins;
                yield return CommandTag.EdgeStyleDefault;
                yield return CommandTag.EdgeStyleDirectCurve;
             }    
        }

        #endregion

        private void CreateGroup(ISelectionContext selectionContext, ViewingContext viewingContext)
        {
            // build the group
            var newGroup = new DomNode(GroupType).As<Group>();
            newGroup.DefaultPinOrder = DefaultPinOrderStyle;
            newGroup.DomNode.Type.SetTag<ICircuitElementType>(newGroup);
            newGroup.Id = "Group".Localize("a noun");
            newGroup.Name = newGroup.Id;
            newGroup.ShowExpandedGroupPins = CircuitDefaultStyle.ShowExpandedGroupPins;

            var selected = selectionContext.LastSelected.As<DomNode>();
            var doc = selected.GetRoot().Cast<DomDocument>();
            var subGraphValidator = doc.Cast<CircuitValidator>();
            subGraphValidator.Suspended = true;

            var circuitEditingContext = selected.Parent.Cast<CircuitEditingContext>();

            // Place the circuit group before the elements get repositioned to the group's local coordinate system.
            if (Placement == PlacementMode.Center)
            {
                // position it at center of grouped modules
                Rectangle bounds = viewingContext.GetBounds(selectionContext.Selection.AsIEnumerable<Element>());
                circuitEditingContext.Center(new object[] { newGroup }, new Point(
                    bounds.X + bounds.Width / 2,
                    bounds.Y + bounds.Height / 2));
            }
            else
            {
                // find upper-left corner of the subnodes
                Point minLocation = new Point(int.MaxValue, int.MaxValue);
                foreach (var module in selectionContext.Selection.AsIEnumerable<Element>())
                {
                    if (minLocation.X > module.Bounds.Location.X)
                        minLocation.X = module.Bounds.Location.X;
                    if (minLocation.Y > module.Bounds.Location.Y)
                        minLocation.Y = module.Bounds.Location.Y;
                }
                // position it at upper-left conner of grouped modules
                newGroup.Bounds = new Rectangle(minLocation.X, minLocation.Y, newGroup.Bounds.Width, newGroup.Bounds.Height);
                newGroup.Position = newGroup.Bounds.Location;
            }

            CreateGroup(newGroup, selectionContext.Selection, circuitEditingContext.CircuitContainer);

            if (CreationOptions == GroupCreationOptions.HideUnconnectedPins)
            {
                newGroup.UpdateGroupPinInfo();
                foreach (var grpPin in newGroup.InputGroupPins)
                    grpPin.Visible = grpPin.Info.ExternalConnected;
                       
                foreach (var grpPin in newGroup.OutputGroupPins)
                    grpPin.Visible = grpPin.Info.ExternalConnected;  
            }

            // select the newly created group
            circuitEditingContext.Selection.Set(newGroup);
            subGraphValidator.Suspended = false;
        }

        private static void UngroupGroups(CircuitEditingContext circuitEditingContext, ISelectionContext selectionContext)
        {
            var graphContainer = circuitEditingContext.CircuitContainer;
            var newSelection = new List<object>();
            foreach (var group in selectionContext.Selection.AsIEnumerable<Group>())
            {
#if CS_4
                newSelection.AddRange(group.Elements);
#else
                newSelection.AddRange(group.Elements.AsIEnumerable<object>());
#endif
                UngroupGroup(group, graphContainer);
            }
            selectionContext.SetRange(newSelection);
        }

        //private void GroupToggleAutoSize(ViewingContext viewingContext)
        //{
        //    if (m_targetRef == null || m_targetRef.Target == null)
        //        return;

        //    var group = m_targetRef.Target.Cast<Group>();
        //    if (group.AutoSize && group.Bounds.Width == 0 && group.Bounds.Height == 0) // initialize group width & height with reasonable values
        //    {
        //        var graphAdapter = viewingContext.Control.Cast<D2dGraphAdapter<Module, Connection, ICircuitPin>>();
        //        var currentBound = graphAdapter.GetBounds(new object[] { group });
        //        group.Bounds = currentBound;
        //    }
        //    group.AutoSize = !group.AutoSize;
        //}

        private bool CanToggleShowExpandedGroupPins(object context, object target)
        {
            if (target.Is<Group>())
            {
                var viewingContext = context.Cast<CircuitEditingContext>().DomNode.Cast<ViewingContext>();
                if (viewingContext.Control != null)
                {
                    
                    var contextMenu = viewingContext.Control.As<ContextMenuAdapter>();
                    if (CommandService.ContextMenuIsTriggering && contextMenu != null)
                    {
                        foreach (var pickingAdapter in viewingContext.Control.AsAll<IPickingAdapter2>())
                        {
                            DiagramHitRecord hitRecord = pickingAdapter.Pick(contextMenu.TriggeringLocation.GetValueOrDefault());
                            if (hitRecord.SubItem.Is<Group>())
                            {
                                m_targetRef = new WeakReference(hitRecord.SubItem); // favor inner container
                                return true;
                            }
                          
                        }
                    }
                }
                return true;

            }
            return false;
        }

        private void ToggleShowExpandedGroupPins(ViewingContext viewingContext)
        {
            if (m_targetRef == null || m_targetRef.Target == null)
                return;

            var group = m_targetRef.Target.Cast<Group>();
            group.Info.ShowExpandedGroupPins = !group.Info.ShowExpandedGroupPins;
            viewingContext.Control.Invalidate();
        }

        private void GroupResetPinNames()
        {
            if (m_targetRef == null || m_targetRef.Target == null)
                return;

            var group = m_targetRef.Target.Cast<Group>();
            foreach (var grpPin in group.InputGroupPins)
            {
                grpPin.IsDefaultName = true;
                grpPin.Name = grpPin.DefaultName(true);            
            }
            foreach (var grpPin in group.OutputGroupPins)
            {
                grpPin.IsDefaultName = true;
                grpPin.Name = grpPin.DefaultName(false);        
            }
        }

        private void ToggleHideUnconnectedPins()
        {
            if (m_targetRef == null || m_targetRef.Target == null)
                return;

            var group = m_targetRef.Target.Cast<Group>();
            if (CircuitUtil.IsGroupTemplateInstance(m_targetRef.Target))             
            {
                group = CircuitUtil.GetGroupTemplate(m_targetRef.Target);
                // for template instances, force update template group pin connectivity   
                // because currently the pin connectivity  of a group template is computed on-demand
                var graphValidator = m_targetRef.Target.Cast<DomNode>().GetRoot().As<CircuitValidator>();
                if (graphValidator == null) // it is possible to hold on a templated instance that is converting to a copy instance during rapid mouse clicks, 
                                            // where the templated instance is no longer a child of root node
                    return;
                graphValidator.UpdateTemplateInfo(group);           
            }
         
            // CTE does not set group’s parent during deserialization,
            // this call ensures the group pins’ external connectivity updated 
            group.UpdateGroupPinInfo(); 
           
            if (m_allUnconnectedHidden)
            {
                foreach (var grpPin in group.InputGroupPins)
                {
                    foreach (var childGroupPin in grpPin.SinkChain(true))
                    {
                        childGroupPin.Visible = true;
                    }
                }
                foreach (var grpPin in group.OutputGroupPins)
                {
                    foreach (var childGroupPin in grpPin.SinkChain(false))
                    {
                        childGroupPin.Visible = true;
                    }
                }
                          
            }
            else
            {
                foreach (var grpPin in group.InputGroupPins)
                    grpPin.Visible = grpPin.Info.ExternalConnected;

                foreach (var grpPin in group.OutputGroupPins)
                    grpPin.Visible = grpPin.Info.ExternalConnected;
            }
          
               
        }

        private bool CanDoTogglePinVisibility(object context, object target)
        {
            if (target.Is<Group>())
            {
                if (CircuitUtil.IsTemplateTargetMissing(target))
                    return false;
                var viewingContext = context.Cast<CircuitEditingContext>().DomNode.Cast<ViewingContext>();
                if (viewingContext.Control != null)
                {
                    var contextMenu = viewingContext.Control.As<ContextMenuAdapter>();
                    if (CommandService.ContextMenuIsTriggering && contextMenu != null)
                    {
                        foreach (var pickingAdapter in viewingContext.Control.AsAll<IPickingAdapter2>())
                        {
                            DiagramHitRecord hitRecord = pickingAdapter.Pick(contextMenu.TriggeringLocation.GetValueOrDefault());
                            if (hitRecord.Part.Is<GroupPin>())
                            {
                                return true;
                            }
                            if (hitRecord.SubPart.Is<GroupPin>())
                            {
                                return true;
                            }
                            if (hitRecord.SubPart.Is<ElementType.Pin>())
                            {
                                return true;
                            }

                        }
                       
                    }
                }
            }
            return false;
        }

 
        private Group.PinOrderStyle m_defaultPinOrderStyle;
        private WeakReference m_targetRef;
        private bool m_allUnconnectedHidden;
    }
}
