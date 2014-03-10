//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;


namespace Sce.Atf.Controls.Adaptable.Graphs
{
    /// <summary>
    /// Component  to  provide convenient service to register/unregister circuit controls created for circuit groups, 
    /// synchronize UI updates for circuit controls due to group renaming,  circuit element insertion/deletion,  
    /// and the closing of documents/controls. This only works if the IDocument can be adapted to a DomNode.</summary>
    [Export(typeof(CircuitControlRegistry))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CircuitControlRegistry
    {
        [ImportingConstructor]
        public CircuitControlRegistry(
            IControlHostService controlHostService,
            IContextRegistry contextRegistry,
            IDocumentService documentService)
        {
            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;
            documentService.DocumentOpened += documentService_DocumentOpened;
            documentService.DocumentClosed += documentService_DocumentClosed;
        }

        /// <summary>
        /// Registers the control for the circuit node</summary>
        public virtual void RegisterControl(DomNode circuitNode, Control control, ControlInfo controlInfo, IControlHostClient client)
        {
            m_circuitNodeControls.Add(circuitNode, new Pair<Control, ControlInfo>(control, controlInfo));
            m_controlHostService.RegisterControl(control, controlInfo, client);
        }

        /// <summary>
        /// Unregisters the Control from the context registry and closes and disposes it.</summary>
        /// <param name="control"></param>
        /// <returns>True if the Control was previously passed in to RegisterControl. False if
        /// the Control was unrecognized in which case no change was made.</returns>
        public virtual bool UnregisterControl(Control control)
        {
            bool result = false;
            KeyValuePair<DomNode, Pair<Control, ControlInfo>> matched =
                m_circuitNodeControls.FirstOrDefault(n => n.Value.First == control);
            if (matched.Key != null)
            {
                UnregisterControl(matched.Key, matched.Value.First);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Retrieve all the DomNode/circuit-control pairs currently registered </summary>
        public IEnumerable< KeyValuePair<DomNode, Pair<Control, ControlInfo>>> CircuitNodeControls
        {
            get { return m_circuitNodeControls; }

        }

        /// <summary>
        /// Get the ControlInfo of the circuit control associated with the DomNode</summary>
        public ControlInfo GetCircuitControlInfo(DomNode domNode)
        {
            return (from ctrol in m_circuitNodeControls where ctrol.Key == domNode select ctrol.Value.Second).FirstOrDefault();
        }
        
        /// <summary>
        ///  Get the associated DomNode for the circuit control</summary>
        public DomNode GetDomNode(Control control)
        {
            return (from ctrol in m_circuitNodeControls where ctrol.Value.Second.Control == control select ctrol.Key).FirstOrDefault();
        }

        /// <summary>
        /// Unregisters the Control from the IContextRegistry and IControlHostService and disposes
        /// it and sets the circuitNode's ViewingContext's Control property to null.</summary>
        private void UnregisterControl(DomNode circuitNode, Control control)
        {
            //it's OK if the CircuitEditingContext was already removed or wasn't added to IContextRegistry.
            m_contextRegistry.RemoveContext(circuitNode.As<CircuitEditingContext>());
            m_controlHostService.UnregisterControl(control);
            control.Visible = false;
            control.Dispose();
            m_circuitNodeControls.Remove(circuitNode);
            circuitNode.Cast<ViewingContext>().Control = null;
        }

        private void documentService_DocumentOpened(object sender, DocumentEventArgs e)
        {
            if (e.Document.Is<DomNode>())
            {
                var docNode = e.Document.Cast<DomNode>();
                docNode.AttributeChanged += OnDocumentNodeAttributeChanged;
                docNode.ChildRemoved += docNode_ChildRemoved;
            }
        }

        private void docNode_ChildRemoved(object sender, ChildEventArgs e)
        {
            if (e.Child.Is<Group>())
            {
                CloseEditingContext(e.Child.As<CircuitEditingContext>());
            }
        }

        private void documentService_DocumentClosed(object sender, DocumentEventArgs e)
        {
            if (e.Document.Is<DomNode>())
            {
                var docNode = e.Document.Cast<DomNode>();
                docNode.AttributeChanged -= OnDocumentNodeAttributeChanged;
                docNode.ChildRemoved -= docNode_ChildRemoved;

                // close all subgraph controls in this document
                foreach (var circuitControl in m_circuitNodeControls.ToArray())
                {
                    if (circuitControl.Key.Lineage.Contains(docNode))
                    {
                        UnregisterControl(circuitControl.Key, circuitControl.Value.First);
                    }
                }
            }
        }

        protected virtual void OnDocumentNodeAttributeChanged(object sender, AttributeEventArgs e)
        {
            var group = e.DomNode.As<Group>();
            if (group != null && (group.IsNameAttribute(e.AttributeInfo)))
            {
                // update ControlInfo.Name for all group controls 
                foreach (var circuitControl in m_circuitNodeControls)
                {
                    if (circuitControl.Key.Is<Group>())
                    {
                        circuitControl.Value.Second.Name = CircuitUtil.GetGroupPath(circuitControl.Key.Cast<Group>());
                    }
                }
             }
        }

        private void CloseEditingContext(CircuitEditingContext editingContext)
        {
            m_contextRegistry.RemoveContext(editingContext);

            if (editingContext.Is<ViewingContext>())
            {
                var viewingContext = editingContext.Cast<ViewingContext>();

                if (viewingContext.Control != null)
                {
                    UnregisterControl(viewingContext.DomNode, viewingContext.Control);
                    viewingContext.Control = null;
                }
            }
        }

        private readonly IControlHostService m_controlHostService;
        private readonly IContextRegistry m_contextRegistry;
        private readonly Dictionary<DomNode, Pair<Control, ControlInfo>> m_circuitNodeControls = new Dictionary<DomNode, Pair<Control, ControlInfo>>();
     }
}
