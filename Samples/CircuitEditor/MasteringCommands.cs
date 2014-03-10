//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// This component defines circuit-specific commands for master, and "un-master". Mastering
    /// takes a set of circuit elements and creates a new type of circuit element. A master circuit
    /// element can be copied and pasted. Any changes to the contents of one master element affect
    /// all instances of it. This functionality has been replaced by circuit groups and circuit
    /// group references.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(MasteringCommands))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MasteringCommands : ICommandClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public MasteringCommands(ICommandService commandService, IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
        }

        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;


        public DomNodeType SubCircuitType
        {
            get { return Schema.subCircuitType.Type; }
        }

        public DomNodeType SubCircuitInstanceType
        {
            get { return Schema.subCircuitInstanceType.Type; }
        }

        public DomNodeType CircuitPinType
        {
            get { return Schema.pinType.Type; }
        }

        #region IInitializable Members

        private enum CommandTag
        {
            CreateMaster,

            ExpandMaster,
        }

        void IInitializable.Initialize()
        {
            m_commandService.RegisterCommand(
               new CommandInfo(
                   CommandTag.CreateMaster,
                   StandardMenu.Edit,
                   StandardCommandGroup.EditGroup,
                   "Master".Localize("this is the name of a command"),
                   "Creates a custom module type from the selection".Localize()),
               this);

            m_commandService.RegisterCommand(
                new CommandInfo(
                    CommandTag.ExpandMaster,
                    StandardMenu.Edit,
                    StandardCommandGroup.EditGroup,
                    "Unmaster".Localize("this is the name of a command"),
                    "Expands the last selected custom module".Localize()),
                this);
        }

        #endregion

        #region ICommandClient Members

        /// <summary>
        /// Can the client do the command?</summary>
        /// <param name="commandTag">Command</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            bool enabled = false;
            var context = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
            if (context != null)
            {
                ISelectionContext selectionContext = context.As<ISelectionContext>();
                if (CommandTag.CreateMaster.Equals(commandTag))
                {
                    enabled = selectionContext.GetLastSelected<Element>() != null; // at least one module selected
                }
                else if (CommandTag.ExpandMaster.Equals(commandTag))
                {
                    enabled = selectionContext.GetLastSelected<SubCircuitInstance>() != null; // at least one mastered instance selected
                }
            }

            return enabled;
        }

        /// <summary>
        /// Does a command</summary>
        /// <param name="commandTag">Command</param>
        public void DoCommand(object commandTag)
        {
            var context = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
            if (CommandTag.CreateMaster.Equals(commandTag))
            {
                SubCircuitInstance subCircuitInstance = null;

                var masterContext = context.DomNode.GetRoot().Cast<CircuitEditingContext>();
                bool success = context.DoTransaction(delegate
                {
                    // create SubCircuit, and SubCircuitInstance of it
                    subCircuitInstance = CreateMaster(context);
                    context.Insert(new DataObject(new object[] { subCircuitInstance }));
                },
                    "Master".Localize("this is the name of a command"));

                // Because multiple EditingContexts can be modifying the Document, and each EditingContext has
                //  its own undo/redo history, we can't record this change in any EditingContext. The reason we
                //  can't is because TransactionContext stores the child index and uses that to undo/redo.
                //  If transactions are undone out-of-order, these indexes will no longer be correct and can
                //  cause bad behavior or unhandled exceptions.
                // Since we still want the validators to run in order to implement unique naming, we can simply
                //  turn off the recording in the HistoryContext.
                if (success)
                {
                    masterContext.Recording = false;
                    try
                    {
                        masterContext.DoTransaction(delegate
                        {
                            CircuitDocument circuitDocument = masterContext.Cast<CircuitDocument>();
                            circuitDocument.SubCircuits.Add(subCircuitInstance.SubCircuit);
                        },
                            "no undo/redo for this part of 'Master'");
                    }
                    finally
                    {
                        masterContext.Recording = true;
                    }
                }
            }
            else if (CommandTag.ExpandMaster.Equals(commandTag))
            {
                context.DoTransaction(delegate
                {
                    // get the last selected SubCircuitInstance and expand its master in its place
                    SubCircuitInstance subCircuitInstance = context.Selection.GetLastSelected<SubCircuitInstance>();
                    object[] subCircuitContents = ExpandMaster(context, subCircuitInstance);
                    context.Insert(new DataObject(subCircuitContents));
                }, "Unmaster".Localize("this is the name of a command"));
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        private SubCircuitInstance CreateMaster(CircuitEditingContext context)
        {
            var circuit = context.CircuitContainer;

            // get the selected modules and the connections between them
            HashSet<Element> modules = new HashSet<Element>();
            List<Wire> internalConnections = new List<Wire>();
            List<Wire> externalConnections = new List<Wire>();
            GetSubCircuit(context.Selection, circuit, modules, internalConnections, externalConnections, externalConnections);

            // clone modules and connections, as the originals could be restored in a future undo operation
            List<DomNode> originals = new List<DomNode>();
            originals.AddRange(Adapters.AsIEnumerable<DomNode>(modules));
            originals.AddRange(Adapters.AsIEnumerable<DomNode>(internalConnections));
            IEnumerable<DomNode> clones = DomNode.Copy(originals);

            // translate cloned modules so the top-left most one is at 16, 16
            int left = int.MaxValue, top = int.MaxValue;
            foreach (Element module in modules)
            {
                Point p = module.Position;
                left = Math.Min(left, p.X);
                top = Math.Min(top, p.Y);
            }

            // remove modules from circuit (connections will be removed by reference validator)
            foreach (Element module in modules)
                circuit.Elements.Remove(module);

            // build the sub-circuit type
            SubCircuit subCircuit = new DomNode(SubCircuitType).As<SubCircuit>();
            subCircuit.Name = "MasterInstance";

            // add modules to sub-circuit, and create pins for "connector" modules
            IList<ICircuitPin> inputPins = subCircuit.Inputs;
            IList<ICircuitPin> outputPins = subCircuit.Outputs;
            foreach (Element module in Adapters.AsIEnumerable<Element>(clones))
            {
                Point p = module.Position;
                module.Position = new Point(p.X - left + 16, p.Y - top + 16);
                subCircuit.Elements.Add(module);
            }

            // Add connections to sub circuit, keeping track of which pins have a connection.
            var connectedInputPins = new HashSet<Pair<Element, ICircuitPin>>();
            var connectedOutputPins = new HashSet<Pair<Element, ICircuitPin>>();
            foreach (Wire connection in Adapters.AsIEnumerable<Wire>(clones))
            {
                subCircuit.Wires.Add(connection);
                connectedInputPins.Add(new Pair<Element, ICircuitPin>(connection.InputElement, connection.InputPin));
                connectedOutputPins.Add(new Pair<Element, ICircuitPin>(connection.OutputElement, connection.OutputPin));
            }

            // Add in the input and output pins to the sub-circuit. Add in only the pins that have
            //  no connections. Only check the types that are connectors.
            foreach (Element module in subCircuit.Elements)
            {
                ElementType elementType = module.Type as ElementType;
                if (elementType != null && elementType.IsConnector)
                {
                    foreach (ICircuitPin outputPin in elementType.Outputs)
                    {
                        if (!connectedOutputPins.Contains(new Pair<Element, ICircuitPin>(module, outputPin)))
                        {
                            Pin pin = new DomNode(CircuitPinType).As<Pin>();
                            pin.Name = module.Name;
                            pin.TypeName = outputPin.TypeName;
                            pin.Index = outputPins.Count;
                            outputPins.Add(pin);
                        }
                    }
                    foreach (ICircuitPin inputPin in elementType.Inputs)
                    {
                        if (!connectedInputPins.Contains(new Pair<Element, ICircuitPin>(module, inputPin)))
                        {
                            Pin pin = new DomNode(CircuitPinType).As<Pin>();
                            pin.Name = module.Name;
                            pin.TypeName = inputPin.TypeName;
                            pin.Index = inputPins.Count;
                            inputPins.Add(pin);
                        }
                    }
                }
            }

            // create sub circuit instance to replace selection
            SubCircuitInstance subCircuitInstance =
                new DomNode(SubCircuitInstanceType).As<SubCircuitInstance>();

            subCircuitInstance.Id = "MasterInstance";
            subCircuitInstance.SubCircuit = subCircuit;

            return subCircuitInstance;
        }

        private object[] ExpandMaster(CircuitEditingContext context, SubCircuitInstance subCircuitInstance)
        {
            // remove the sub-circuit instance
            var circuit = context.CircuitContainer;
            circuit.Elements.Remove(subCircuitInstance);

            SubCircuit subCircuit = subCircuitInstance.SubCircuit as SubCircuit;

            // clone sub-circuit contents
            List<object> subCircuitContents = new List<object>();
            subCircuitContents.AddRange(Adapters.AsIEnumerable<object>(subCircuit.Elements));
            subCircuitContents.AddRange(Adapters.AsIEnumerable<object>(subCircuit.Wires));
            DomNode[] clones = DomNode.Copy(Adapters.AsIEnumerable<DomNode>(subCircuitContents));

            object[] data = new object[clones.Length];
            clones.CopyTo(data, 0);
            return data;
        }

        // returns a list of all modules, internal connections between them, and connections
        //  to external modules
        private static void GetSubCircuit(
            IEnumerable<object> objects,
            ICircuitContainer circuit,
            HashSet<Element> modules,
            ICollection<Wire> connections,
            ICollection<Wire> internalConnections,
            ICollection<Wire> externalConnections)
        {
            // build the set of modules, and add them to result
            foreach (Element module in Adapters.AsIEnumerable<Element>(objects))
                modules.Add(module);

            // add connections to modules
            foreach (Wire connection in circuit.Wires)
            {
                bool output = modules.Contains(connection.OutputElement);
                bool input = modules.Contains(connection.InputElement);
                if (output && input)
                {
                    connections.Add(connection);
                }
                else if (output)
                {
                    externalConnections.Add(connection);
                }
                else if (input)
                {
                    internalConnections.Add(connection);
                }
            }
        }

    }
}
