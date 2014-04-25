//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Adaptation;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Editing Context for templates library; this is the context that is bound to the TemplateLister 
    /// when a circuit document becomes the active context. </summary>
    /// <remarks>This context has its own independent Selection, 
    /// The ITreeView implementation controls the hierarchy in the TemplateLister's TreeControl.
    /// The IItemView implementation controls icons and labels in the TemplateLister's TreeControl.</remarks>
    public class TemplatingContext : Sce.Atf.Dom.TemplatingContext
    {
        /// <summary>
        /// Performs initialization when the adapter is connected to the circuit's DomNode:
        /// subscribe to DomNode tree change events</summary>
        protected override void OnNodeSet()
        {
            base.OnNodeSet();
            DomNode.ChildRemoved += DomNode_ChildRemoved;
        }

        /// <summary>
        /// Gets root template folder</summary>
        public override Sce.Atf.Dom.TemplateFolder RootFolder
        {
            get
            {
                if (m_rootFolder == null)
                {
                    m_rootFolder = GetChild<TemplateFolder>(Schema.circuitDocumentType.templateFolderChild);
                    if (m_rootFolder == null) // create one if no root folder is defined yet
                    {
                        var rootFolderNode =  new DomNode(Schema.templateFolderType.Type);
                        rootFolderNode.Cast<TemplateFolder>().Name = "_TemplateRoot_";
                        DomNode.SetChild(Schema.circuitDocumentType.templateFolderChild, rootFolderNode);
                        m_rootFolder = rootFolderNode.Cast<TemplateFolder>();
                    }
                }
                return m_rootFolder;
            }
        }

        /// <summary>
        /// Gets type of template</summary>
        protected override DomNodeType TemplateType
        {
            get { return Schema.templateType.Type; }
        }

        /// <summary>
        /// Returns true iff the reference can reference the specified target item</summary>
        /// <param name="item">Template item to be referenced</param>
        /// <returns>True iff the reference can reference the specified target item</returns>
        public override bool CanReference(object item)
        {
            return item.Is<Template>() && item.Cast<Template>().Model.Is<Module>();
        }

        /// <summary>
        /// Creates a reference instance that references the specified target item</summary>
        /// <param name="item">Item to create reference for</param>
        public override object CreateReference(object item)
        {
            var template = item.Cast<Template>();
            if (template.Target.Is<Group>())
            {
                var groupReference = new DomNode(Schema.groupTemplateRefType.Type).Cast<GroupInstance>();
                groupReference.Target = template.Target.Cast<Module>();
                groupReference.Id = template.Guid.ToString();
                groupReference.Name = template.Name;
                return groupReference;
            }
            if (template.Target.Is<Module>())
            {
                var moduleReference = new DomNode(Schema.moduleTemplateRefType.Type).Cast<ModuleInstance>();
                moduleReference.Target = template.Target.Cast<Module>();
                moduleReference.Id = template.Guid.ToString();
                moduleReference.Name = template.Name;
                return moduleReference;
            }
            return null;
        }

        private void DomNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            // if a template is deleted, turn template references into copy-instances
            if (!IsMovingItems &&  e.Child.Is<Template>())
            {
                // we can use the ReferenceValidator which is attached to this (root) node to get all the references.
                // note reference validation will happen later at the end of the transaction to remove the dangling references
                var refValidator = this.As<ReferenceValidator>();
                DomNode target = e.Child.Cast<Template>().Target;
             
                foreach (var reference in refValidator.GetReferences(target))
                {
                    var targetCopies = DomNode.Copy(new[] { target }); // DOM deep copy
                    var copyInstance = targetCopies[0].Cast<Element>();

                    var templateInstance = reference.First.Cast<Element>();
                    copyInstance.Position = templateInstance.Position;
                    var circuitContainer = reference.First.Parent.Cast<ICircuitContainer>();
                    circuitContainer.Elements.Add(copyInstance);

                    // reroute original edges 
                    foreach (var wire in circuitContainer.Wires)
                    {
                        if (wire.InputElement == templateInstance)
                        {
                            wire.InputElement = copyInstance;
                            wire.InputPin = copyInstance.Type.Inputs[wire.InputPin.Index];     
                            wire.SetPinTarget();
                        }
                        if (wire.OutputElement == templateInstance)
                        {
                            wire.OutputElement = copyInstance;
                            wire.OutputPin = copyInstance.Type.Outputs[wire.OutputPin.Index];
                            wire.SetPinTarget();
                        }
                    }
                }
            }
        }

        private TemplateFolder m_rootFolder;
    }
}
