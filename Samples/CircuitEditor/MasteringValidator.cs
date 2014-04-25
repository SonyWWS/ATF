//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.


using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;


namespace CircuitEditorSample
{
    /// <summary>
    /// Tracks changes to sub-circuits and sub-circuit instances in the document
    /// and throws an InvalidTransactionException for certain errors</summary>
    public class MasteringValidator : Validator
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the diagram annotation's DomNode.
        /// Raises the DomNodeAdapter NodeSet event and performs custom processing.</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNode.ChildInserting += DomNodeChildInserting;
            DomNode.ChildRemoving += DomNodeOnChildRemoving;
        }

        private void DomNodeOnChildRemoving(object sender, ChildEventArgs e)
        {
            if (Validating)
            {
                // removing a module from a sub-circuit?
                Element element = e.Child.As<Element>();
                SubCircuit subCircuit = e.Parent.As<SubCircuit>();
                if (element != null &&
                    subCircuit != null)
                {
                    ICircuitElementType type = element.Type;
                    // todo: this test isn't quite right, because not all circuit elements
                    //  necessarily have both inputs and outputs. For example, if the Speaker
                    //  element from the Circuit Editor sample app is the only element in a Master,
                    //  and then it is deleted, that will trigger this exception.
                    if (type.Inputs.Count + type.Outputs.Count == 1)
                    {
                        // Ensures that sub-circuit inputs/outputs aren't added or removed, as this would
                        //    invalidate wiring on instances of them.
                        throw new InvalidTransactionException(
                            "Can't remove connectors from sub-circuits".Localize());
                    }
                }
            }
        }

        private void DomNodeChildInserting(object sender, ChildEventArgs e)
        {
            if (Validating)
            {
                // inserting an instance of a sub-circuit into itself?
                SubCircuitInstance subCircuitInstance = e.Child.As<SubCircuitInstance>();
                SubCircuit subCircuit = e.Parent.As<SubCircuit>();
                if (subCircuitInstance != null &&
                    subCircuit != null &&
                    subCircuitInstance.SubCircuit == subCircuit)
                {
                    throw new InvalidTransactionException(
                        "Can't use a sub-circuit inside itself".Localize());
                }
            }
        }
    }
}
