//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.VectorMath;
using Sce.Atf.Direct2D;
using Sce.Atf.Dom;

namespace CircuitEditorSample
{
    /// <summary>
    /// Circuit editor</summary>
    [Export(typeof(IDocumentClient))]
    [Export(typeof(Editor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : IDocumentClient, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="commandService">Command service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="documentService">Document service</param>
        /// <param name="prototypeLister">Prototype lister</param>
        /// <param name="layerLister">Layer lister</param>
        /// <param name="schemaLoader">Schema loader</param>
        [ImportingConstructor]
        public Editor(
            IControlHostService controlHostService,
            ICommandService commandService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService,
            PrototypeLister prototypeLister,
            LayerLister layerLister,
            SchemaLoader schemaLoader)
        {
            m_controlHostService = controlHostService;
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_documentService = documentService;
            m_prototypeLister = prototypeLister;
            m_layerLister = layerLister;
            
            m_schemaLoader = schemaLoader;

            string initialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\components\\wws_atf\\Samples\\CircuitEditor\\data");
            EditorInfo.InitialDirectory = initialDirectory;
            m_theme = new D2dDiagramTheme();
            m_circuitRenderer = new D2dCircuitRenderer<Module, Connection, ICircuitPin>(m_theme, documentRegistry);
            m_subGraphRenderer = new D2dSubCircuitRenderer<Module, Connection, ICircuitPin>(m_theme);

            // create d2dcontrol for displaying sub-circuit            
            m_d2dHoverControl = new D2dAdaptableControl();
            m_d2dHoverControl.Dock = DockStyle.Fill;
            var xformAdapter = new TransformAdapter();
            xformAdapter.EnforceConstraints = false;//to allow the canvas to be panned to view negative coordinates
            m_d2dHoverControl.Adapt(xformAdapter, new D2dGraphAdapter<Module, Connection, ICircuitPin>(m_circuitRenderer, xformAdapter));
            m_d2dHoverControl.DrawingD2d += new EventHandler(m_d2dHoverControl_DrawingD2d);
        }

        
        private IControlHostService m_controlHostService;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private IDocumentService m_documentService;
        private PrototypeLister m_prototypeLister;
        private LayerLister m_layerLister;
        private SchemaLoader m_schemaLoader;

        [Import(AllowDefault = true)]
        private IStatusService m_statusService = null;
        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        /// <summary>
        /// Circuit clipboard data type</summary>
        public static readonly string CircuitFormat = Schema.NS + ":Circuit";

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        #region IInitializable

        void IInitializable.Initialize()
        {
            if (m_scriptingService != null)
            {
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(GetType().Assembly);
                m_scriptingService.ImportAllTypes("CircuitEditorSample");
                m_scriptingService.ImportAllTypes("Sce.Atf.Controls.Adaptable.Graphs");

                m_scriptingService.SetVariable("editor", this);
                m_scriptingService.SetVariable("schemaLoader", m_schemaLoader);
                m_scriptingService.SetVariable("layerLister", m_layerLister);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    var editingContext = m_contextRegistry.GetActiveContext<CircuitEditingContext>();
                    ViewingContext viewContext = m_contextRegistry.GetActiveContext<ViewingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("circuitContainer", editingContext != null ? editingContext.CircuitContainer : null);
                    m_scriptingService.SetVariable("view", viewContext);
                    m_scriptingService.SetVariable("hist", hist);
                };
            }

            if (m_settingsService != null)
            {
                var settings = new[] 
                {
                  new BoundPropertyDescriptor(typeof (CircuitDefaultStyle),
                        () => CircuitDefaultStyle.EdgeStyle,
                        "Wire Style".Localize(), "Circuit Editor".Localize(),
                        "Default Edge Style".Localize()),
                  new BoundPropertyDescriptor(typeof (CircuitDefaultStyle),
                        () => CircuitDefaultStyle.ShowExpandedGroupPins,
                        "Show Expanded Group Pins".Localize(), "Circuit Editor".Localize(),
                        "Show group pins when a group is expanded".Localize()),
                  new BoundPropertyDescriptor(typeof (CircuitDefaultStyle),
                        () => CircuitDefaultStyle.ShowVirtualLinks,
                        "Show Virtual links".Localize(), "Circuit Editor".Localize(),
                        "Show virtual links between group pin and its associated subnodes when a group is expanded".Localize()),
                };
                m_settingsService.RegisterUserSettings("Circuit Editor", settings);
                m_settingsService.RegisterSettings(this, settings);
            }

            if (m_modulePlugin != null)
            {
                var pen = D2dFactory.CreateSolidBrush(Color.LightSeaGreen);
                m_theme.RegisterCustomBrush(m_modulePlugin.BooleanPinTypeName, pen);
                pen = D2dFactory.CreateSolidBrush(Color.LightSeaGreen);
                m_theme.RegisterCustomBrush(m_modulePlugin.FloatPinTypeName, pen);
            }

            CircuitEditingContext.CircuitFormat = CircuitFormat;

        }

        #endregion

        #region IDocumentClient Members

        /// <summary>
        /// Gets information about the document client, such as the file type and file
        /// extensions it supports, whether or not it allows multiple documents to be open, etc.</summary>
        public DocumentClientInfo Info
        {
            get { return EditorInfo; }
        }

        /// <summary>
        /// Document editor information for circuit editor</summary>
        public static DocumentClientInfo EditorInfo =
            new DocumentClientInfo("Circuit".Localize(), ".circuit", null, null);

        /// <summary>
        /// Returns whether the client can open or create a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>True iff the client can open or create a document at the given URI</returns>
        public bool CanOpen(Uri uri)
        {
            return EditorInfo.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Opens or creates a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        public IDocument Open(Uri uri)
        {
            DomNode node = null;
            string filePath = uri.LocalPath;

            if (File.Exists(filePath))
            {
                // read existing document using standard XML reader
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    DomXmlReader reader = new DomXmlReader(m_schemaLoader);
                    node = reader.Read(stream, uri);
                }
            }
            else
            {
                // create new document by creating a Dom node of the root type defined by the schema
                node = new DomNode(Schema.circuitDocumentType.Type, Schema.circuitRootElement);
                // create an empty root prototype folder
                node.SetChild(
                    Schema.circuitDocumentType.prototypeFolderChild,
                    new DomNode(Schema.prototypeFolderType.Type));
            }

            CircuitDocument circuitCircuitDocument = null;
            if (node != null)
            {
                AdaptableControl control = CreateCircuitControl(node);
                control.AddHelp("https://github.com/SonyWWS/ATF/wiki/Adaptable-Controls".Localize());

                var viewingContext = node.Cast<ViewingContext>();
                viewingContext.Control = control;

                circuitCircuitDocument = node.Cast<CircuitDocument>();
                string fileName = Path.GetFileName(filePath);
                ControlInfo controlInfo = new ControlInfo(fileName, filePath, StandardControlGroup.Center);

                //Set IsDocument to true to prevent exception in command service if two files with the
                //  same name, but in different directories, are opened.
                controlInfo.IsDocument = true;
                
                circuitCircuitDocument.ControlInfo = controlInfo;
                circuitCircuitDocument.Uri = uri;

                var editingContext = node.Cast<CircuitEditingContext>();
                editingContext.GetLocalBound = GetLocalBound;
                editingContext.GetWorldOffset = GetWorldOffset;
                editingContext.GetTitleHeight = GetTitleHeight;
                editingContext.GetLabelHeight = GetLabelHeight;
                editingContext.GetSubContentOffset = GetSubContentOffset;

                control.Context = editingContext;
                editingContext.SchemaLoader = m_schemaLoader; // schema needed for cut and paste between applications

                // now that the data is complete, initialize all other extensions to the Dom data
                node.InitializeExtensions();
                m_circuitControlRegistry.RegisterControl(node, control, controlInfo, this);

                // Set the zoom and translation to show the existing items (if any).
                var enumerableContext = editingContext.Cast<IEnumerableContext>();
                if (viewingContext.CanFrame(enumerableContext.Items))
                    viewingContext.Frame(enumerableContext.Items);
            }

            return circuitCircuitDocument;
        }

        internal D2dAdaptableControl CreateCircuitControl(DomNode circuitNode)
        {
            var control = new D2dAdaptableControl();
            control.SuspendLayout();             
            control.BackColor = SystemColors.ControlLight;
            control.AllowDrop = true;

            var transformAdapter = new TransformAdapter();
            transformAdapter.EnforceConstraints = false; //to allow the canvas to be panned to view negative coordinates
            transformAdapter.UniformScale = true;
            transformAdapter.MinScale = new PointF(0.25f, 0.25f);
            transformAdapter.MaxScale = new PointF(4, 4);
            var viewingAdapter = new ViewingAdapter(transformAdapter);
            viewingAdapter.MarginSize = new Size(25, 25);
            var canvasAdapter = new CanvasAdapter();
            ((ILayoutConstraint) canvasAdapter).Enabled = false; //to allow negative coordinates for circuit elements within groups

            var autoTranslateAdapter = new AutoTranslateAdapter(transformAdapter);
            var mouseTransformManipulator = new MouseTransformManipulator(transformAdapter);
            var mouseWheelManipulator = new MouseWheelManipulator(transformAdapter);
            var scrollbarAdapter = new ScrollbarAdapter(transformAdapter, canvasAdapter);

            var hoverAdapter = new HoverAdapter();
            hoverAdapter.HoverStarted += control_HoverStarted;
            hoverAdapter.HoverStopped += control_HoverStopped;

            var annotationAdaptor = new D2dAnnotationAdapter(m_theme); // display annotations under diagram

            if (circuitNode.Is<Circuit>())
            {
                var circuitAdapter = new D2dGraphAdapter<Module, Connection, ICircuitPin>(m_circuitRenderer, transformAdapter);
                var circuitModuleEditAdapter = new D2dGraphNodeEditAdapter<Module, Connection, ICircuitPin>(
                    m_circuitRenderer, circuitAdapter, transformAdapter);
                circuitModuleEditAdapter.DraggingSubNodes = false;

                var circuitConnectionEditAdapter =
                    new D2dGraphEdgeEditAdapter<Module, Connection, ICircuitPin>(m_circuitRenderer, circuitAdapter, transformAdapter);
                circuitConnectionEditAdapter.EdgeRouteTraverser = CircuitUtil.EdgeRouteTraverser;

                control.Adapt(
                    hoverAdapter,
                    scrollbarAdapter,
                    autoTranslateAdapter,
                    new RectangleDragSelector(),
                    transformAdapter,
                    viewingAdapter,
                    canvasAdapter,
                    mouseTransformManipulator,
                    mouseWheelManipulator,
                    new KeyboardIOGraphNavigator<Module, Connection, ICircuitPin>(),
                    new D2dGridAdapter(),
                    annotationAdaptor,
                    circuitAdapter,
                    circuitModuleEditAdapter,
                    circuitConnectionEditAdapter,
                    new LabelEditAdapter(),
                    new SelectionAdapter(),
                    new DragDropAdapter(m_statusService),
                    new ContextMenuAdapter(m_commandService, m_contextMenuCommandProviders)
                    );
            }
            else if (circuitNode.Is<Group>())
            {
                var circuitAdapter = new D2dSubgraphAdapter<Module, Connection, ICircuitPin>(m_subGraphRenderer,
                                                                                      transformAdapter);
                var circuitModuleEditAdapter = new D2dGraphNodeEditAdapter<Module, Connection, ICircuitPin>(
                    m_subGraphRenderer, circuitAdapter, transformAdapter);
                circuitModuleEditAdapter.DraggingSubNodes = false;

                var circuitConnectionEditAdapter =
                    new D2dGraphEdgeEditAdapter<Module, Connection, ICircuitPin>(m_subGraphRenderer, circuitAdapter, transformAdapter);
                circuitConnectionEditAdapter.EdgeRouteTraverser = CircuitUtil.EdgeRouteTraverser;

                var groupPinEditor = new GroupPinEditor(transformAdapter);
                groupPinEditor.GetPinOffset = m_subGraphRenderer.GetPinOffset;
 
                canvasAdapter.UpdateTranslateMinMax = groupPinEditor.UpdateTranslateMinMax;

                control.Adapt(
                  hoverAdapter,
                  scrollbarAdapter,
                  autoTranslateAdapter,
                  new RectangleDragSelector(),
                  transformAdapter,
                  viewingAdapter,
                  canvasAdapter,
                  mouseTransformManipulator,
                  mouseWheelManipulator,
                  new KeyboardIOGraphNavigator<Module, Connection, ICircuitPin>(),
                  new D2dGridAdapter(),
                  annotationAdaptor,
                  circuitAdapter,
                  circuitModuleEditAdapter,
                  circuitConnectionEditAdapter,
                  new LabelEditAdapter(),
                  groupPinEditor,
                  new SelectionAdapter(),
                  new DragDropAdapter(m_statusService),
                  new ContextMenuAdapter(m_commandService, m_contextMenuCommandProviders)
                  );
            }
            else throw new NotImplementedException(
               "graph node type is not supported!");

            control.ResumeLayout();

            control.DoubleClick += new EventHandler(control_DoubleClick);
            control.MouseDown += new MouseEventHandler(control_MouseDown);
            return control;
        }

   
        private void control_DoubleClick(object sender, EventArgs e)
        {
            AdaptableControl d2dHoverControl = (AdaptableControl)sender;
            Point clientPoint = d2dHoverControl.PointToClient(Control.MousePosition);

            D2dGraphAdapter<Module, Connection, ICircuitPin> graphAdapter =
                d2dHoverControl.As<D2dGraphAdapter<Module, Connection, ICircuitPin>>();
            GraphHitRecord<Module, Connection, ICircuitPin> hitRecord = graphAdapter.Pick(clientPoint);
            SubCircuitInstance subCircuitInstance = Adapters.As<SubCircuitInstance>(hitRecord.Node);
            if (subCircuitInstance != null)
            {
                var subCircuit = subCircuitInstance.SubCircuit;

                var viewingContext = subCircuit.Cast<ViewingContext>();
                if (viewingContext.Control != null)
                {
                    // sub-circuit is already open, just show control
                    m_controlHostService.Show(viewingContext.Control);
                }
                else
                {
                    // create new circuit editing control for sub-circuit
                    AdaptableControl subCircuitControl = CreateCircuitControl(subCircuit.DomNode);
                    viewingContext.Control = subCircuitControl;

                    CircuitDocument circuitDocument = subCircuitInstance.DomNode.GetRoot().As<CircuitDocument>();
                    string filePath = circuitDocument.Uri.LocalPath;
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string subCircuitName = subCircuit.Name;
                    string name = fileName + ":" + subCircuitName;
                    string description = filePath + "#" + subCircuitName;

                    var editingContext = subCircuit.DomNode.Cast<CircuitEditingContext>();
                    editingContext.GetLocalBound = GetLocalBound;
                    editingContext.GetWorldOffset = GetWorldOffset;
                    editingContext.GetTitleHeight = GetTitleHeight;
                    editingContext.GetLabelHeight = GetLabelHeight;
                    editingContext.GetSubContentOffset = GetSubContentOffset;

                    subCircuitControl.Context = editingContext;
                    editingContext.SchemaLoader = m_schemaLoader; // schema needed for cut and paste between applications

                    ControlInfo controlInfo = new ControlInfo(name, description, StandardControlGroup.Center);
                    m_circuitControlRegistry.RegisterControl(subCircuit.DomNode, subCircuitControl, controlInfo, this);
                }
            }
            else
            {
                Group subGraph = null;
                var subGraphReference = hitRecord.Node.As<GroupInstance>();
                if (subGraphReference != null)
                {
                    DialogResult checkResult = DialogResult.No; //direct editing 
                    if (checkResult == DialogResult.No)
                    {
                        subGraph = subGraphReference.Target.Cast<Group>();
                        var graphValidator = subGraphReference.DomNode.GetRoot().Cast<CircuitValidator>();
                        graphValidator.UpdateTemplateInfo(subGraph);
                    }
                    
                }
                else
                    subGraph = hitRecord.Node.As<Group>();
                if (subGraph != null)
                {
                    var viewingContext = subGraph.Cast<ViewingContext>();
                    if (viewingContext.Control != null)
                    {
                        // sub-graph is already open, just show control
                        m_controlHostService.Show(viewingContext.Control);
                    }
                    else
                    {
                        // create new circuit editing control for sub-circuit
                        AdaptableControl subCircuitControl = CreateCircuitControl(subGraph.DomNode);                     
                        viewingContext.Control = subCircuitControl;

                        // use group’s hierarchy as control name
                        string name=string.Empty;                                   
                        bool first = true;
                        foreach (var domNode in subGraph.DomNode.GetPath())
                        {
                            if (domNode.Is<Group>())
                            {
                                if (first)
                                {
                                    name = domNode.Cast<Group>().Name;
                                    first = false;
                                }
                                else
                                    name += "/" + domNode.Cast<Group>().Name;
                            }
                        }

                        string description = name;

                        var editingContext = subGraph.DomNode.Cast<CircuitEditingContext>();
                        editingContext.GetLocalBound = GetLocalBound;
                        editingContext.GetWorldOffset = GetWorldOffset;
                        editingContext.GetTitleHeight = GetTitleHeight;
                        editingContext.GetLabelHeight = GetLabelHeight;
                        editingContext.GetSubContentOffset = GetSubContentOffset;

                        subCircuitControl.Context = editingContext;
                        editingContext.SchemaLoader = m_schemaLoader; // schema needed for cut and paste between applications

                        ControlInfo controlInfo = new ControlInfo(name, description, StandardControlGroup.Center);
                        //controlInfo.Docking = new ControlInfo.DockingInfo() // smart docking behavior
                        //{                 
                        //    GroupTag = subGraph.DomNode.Lineage.AsIEnumerable<Group>().Last(),// use the top-level parent group
                        //    Order = subGraph.Level
                        //};
                        m_circuitControlRegistry.RegisterControl(subGraph.DomNode, subCircuitControl, controlInfo, this);

                        var enumerableContext = subGraph.DomNode.Cast<CircuitEditingContext>().Cast<IEnumerableContext>();
                        var items = (enumerableContext != null) ? enumerableContext.Items : null;
                        subCircuitControl.As<IViewingContext>().Frame(items);                                         
                    }
                }                                 
            }
        }


        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            var d2dControl = (AdaptableControl)sender;
            if (e.Button == MouseButtons.Left && d2dControl.Focused)
            {
                Point clientPoint = d2dControl.PointToClient(Control.MousePosition);
                var graphAdapter = d2dControl.As<D2dGraphAdapter<Module, Connection, ICircuitPin>>();
                GraphHitRecord<Module, Connection, ICircuitPin> hitRecord = graphAdapter.Pick(clientPoint);

                if (hitRecord.Part is DiagramExpander)
                {
                    if (e.Clicks == 1)
                    {
                        var group = hitRecord.Item.As<ICircuitGroupType<Module, Connection, ICircuitPin>>();
                        if (hitRecord.SubItem.Is<ICircuitGroupType<Module, Connection, ICircuitPin>>())
                            group = hitRecord.SubItem.Cast<ICircuitGroupType<Module, Connection, ICircuitPin>>();
                        var transaction_context = group.Cast<DomNodeAdapter>().DomNode.GetRoot().As<ITransactionContext>();
                        transaction_context.DoTransaction(() => group.Expanded = !group.Expanded, "Toggle Group Expansion");
                    }                  
                }
                else if (hitRecord.Part is DiagramPin)
                {
                    var grpPin = hitRecord.FromRoute.As<GroupPin>();
                    ITransactionContext transactionContext = grpPin.DomNode.GetRoot().As<ITransactionContext>();
                    transactionContext.DoTransaction(() => grpPin.Info.Pinned = !grpPin.Info.Pinned, "Pinned Subgraph Pin");
                    d2dControl.Invalidate();
                }
                else if (hitRecord.Part is DiagramVisibilityCheck)
                {
                    var grpPin = hitRecord.FromRoute.As<GroupPin>();
                    if (!grpPin.Info.ExternalConnected)
                    {
                        ITransactionContext transactionContext = grpPin.DomNode.GetRoot().As<ITransactionContext>();
                        transactionContext.DoTransaction(() => grpPin.Info.Visible = !grpPin.Info.Visible,
                                                         "Toggle Group Pin Visibility");
                        d2dControl.Invalidate();
                    }
                }
            }
        }


        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            var viewingContext = Adapters.Cast<ViewingContext>(document);
            m_controlHostService.Show(viewingContext.Control);
        }

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save(IDocument document, Uri uri)
        {
            CircuitDocument circuitDocument = (CircuitDocument)document;
            string filePath = uri.LocalPath;
            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                var writer = new CircuitWriter(m_schemaLoader.TypeCollection);
                writer.Write(circuitDocument.DomNode, stream, uri);
            }
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
            m_documentRegistry.Remove(document);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
            AdaptableControl adaptableControl = (AdaptableControl)control;
            var context = adaptableControl.ContextAs<CircuitEditingContext>();
            m_contextRegistry.ActiveContext = context;

            CircuitDocument circuitDocument = context.As<CircuitDocument>();
            if (circuitDocument != null)
                m_documentRegistry.ActiveDocument = circuitDocument;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control either can close or is already closed. False to cancel.</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control)
        {
            var adaptableControl = (AdaptableControl)control;

            bool closed = true;
            CircuitDocument circuitDocument = adaptableControl.ContextAs<CircuitDocument>();
            if (circuitDocument != null)
            {
                closed = m_documentService.Close(circuitDocument);
                if (closed)
                    Close(circuitDocument);
            }
            else
            {
                // We don't care if the control was already unregistered. 'closed' should be true.
                m_circuitControlRegistry.UnregisterControl(control);
            }            
            return closed;
        }

        #endregion

        private void control_HoverStarted(object sender, HoverEventArgs<object, object> e)
        {
            m_hoverForm = GetHoverForm(e);
        }
        private HoverBase GetHoverForm(HoverEventArgs<object, object> e)
        {
            HoverBase result = null;

            // handle sub-circuit instance
            SubCircuitInstance subCircuitInstance = Adapters.As<SubCircuitInstance>(e.Object);
            if (subCircuitInstance != null)
            {
                result = CreateHoverForm(subCircuitInstance);
            }
            else
            {
                result = CreateHoverForm(e);
            }

            if (result != null)
            {
                Point p = Control.MousePosition;
                result.Location = new Point(p.X - (result.Width + 12), p.Y + 12);
                result.ShowWithoutFocus();
            }
            return result;
        }

        // create hover form for module or connection
        private HoverBase CreateHoverForm(HoverEventArgs<object, object> e)
        {
            StringBuilder sb = new StringBuilder();

            var hoverItem = e.Object;
            var hoverPart = e.Part;

            string itemName= string.Empty;
            string partName= string.Empty;

            if (e.SubPart.Is<GroupPin>())
            {
                sb.Append(e.SubPart.Cast<GroupPin>().Name);
                partName = CircuitUtil.GetDomNodeName(e.SubPart.Cast<DomNode>());
            }
            else if (e.SubObject.Is<DomNode>())
            {
                itemName = CircuitUtil.GetDomNodeName(e.SubObject.Cast<DomNode>());
            }
            else if (hoverPart.Is<GroupPin>())
            {
                sb.Append(hoverPart.Cast<GroupPin>().Name);
                partName = CircuitUtil.GetDomNodeName(hoverPart.Cast<DomNode>());
            }
            else if (hoverItem.Is<DomNode>())
            {
                itemName = CircuitUtil.GetDomNodeName(hoverItem.Cast<DomNode>());
            }
           

            //Trace.TraceInformation("hoverItem {0} hoverPart  {1}", itemName, partName);

            //StringBuilder sb = new StringBuilder();
            //ICustomTypeDescriptor customTypeDescriptor = Adapters.As<ICustomTypeDescriptor>(hoverItem);
            //if (customTypeDescriptor != null)
            //{
            //    // Get properties interface
            //    foreach (System.ComponentModel.PropertyDescriptor property in customTypeDescriptor.GetProperties())
            //    {
            //        object value = property.GetValue(hoverItem);
            //        if (value != null)
            //        {
            //            sb.Append(property.Name);
            //            sb.Append(": ");
            //            sb.Append(value.ToString());
            //            sb.Append("\n");
            //        }
            //    }
            //}

            HoverBase result = null;
            if (sb.Length > 0) // remove trailing '\n'
            {
                //sb.Length = sb.Length - 1;
                result = new HoverLabel(sb.ToString());
            }

            return result;
        }

        // create hover form for sub-circuit instance
        private HoverBase CreateHoverForm(SubCircuitInstance subCircuitInstance)
        {
            const float MAX_SIZE = 420;
            const int CircuitMargin = 8;

            TransformAdapter xformAdapter = m_d2dHoverControl.As<TransformAdapter>();
            xformAdapter.Transform.Reset();
            m_d2dHoverControl.D2dGraphics.Transform = Matrix3x2F.Identity;

            m_d2dHoverControl.Context = subCircuitInstance.SubCircuit;

            RectangleF bounds = m_circuitRenderer.GetBounds(subCircuitInstance.SubCircuit.Elements.AsIEnumerable<Module>()
                , m_d2dHoverControl.D2dGraphics);
                                  
            float boundRatio = bounds.Width / bounds.Height;

            Size size =(boundRatio > 1) ? new Size((int)MAX_SIZE, (int)(MAX_SIZE / boundRatio))
                : new Size((int)(MAX_SIZE * boundRatio), (int)MAX_SIZE);

            float scale = (float)size.Width / (float)bounds.Width;
            xformAdapter.Transform.Translate(CircuitMargin, CircuitMargin);
            xformAdapter.Transform.Scale(scale, scale);
            xformAdapter.Transform.Translate(-bounds.X , -bounds.Y );

            HoverBase result = new HoverBase();
            result.Size = new Size(size.Width + 2 * CircuitMargin, size.Height + 2 * CircuitMargin);
            result.Controls.Add(m_d2dHoverControl);
            return result;
        }


        private void control_HoverStopped(object sender, EventArgs e)
        {
            if (m_hoverForm != null)
            {
                m_d2dHoverControl.Context = null;
                m_hoverForm.Controls.Clear();
                m_hoverForm.Close();
                m_hoverForm.Dispose();
            }
        }

        private void m_d2dHoverControl_DrawingD2d(object sender, EventArgs e)
        {
            m_d2dHoverControl.D2dGraphics.Transform = Matrix3x2F.Identity;
            m_d2dHoverControl.D2dGraphics.DrawRectangle(m_d2dHoverControl.ClientRectangle,
                Color.Black, 2.0f, null);
        }

        //CircuitEditingContext callback 
        internal static RectangleF GetLocalBound(AdaptableControl adaptableControl, Element moudle)
        {
            var graphAdapter = adaptableControl.Cast<D2dGraphAdapter<Module, Connection, ICircuitPin>>();
            return graphAdapter.GetLocalBound(moudle.DomNode.Cast<Module>());
        }

        //CircuitEditingContext callback 
        internal static Point GetWorldOffset(AdaptableControl adaptableControl, IEnumerable<Element> graphPath)
        {
            var render = adaptableControl.Cast<D2dGraphAdapter<Module, Connection, ICircuitPin>>().Renderer
            .Cast<D2dCircuitRenderer<Module, Connection, ICircuitPin>>();
            return render.WorldOffset(graphPath.AsIEnumerable<Module>());
        }

        //CircuitEditingContext callback 
        internal static int GetTitleHeight(AdaptableControl adaptableControl)
        {
            var render = adaptableControl.Cast<D2dGraphAdapter<Module, Connection, ICircuitPin>>().Renderer
            .Cast<D2dCircuitRenderer<Module, Connection, ICircuitPin>>();
            return render.TitleHeight;
        }

        //CircuitEditingContext callback 
        internal static int GetLabelHeight(AdaptableControl adaptableControl)
        {
            var render = adaptableControl.Cast<D2dGraphAdapter<Module, Connection, ICircuitPin>>().Renderer
            .Cast<D2dCircuitRenderer<Module, Connection, ICircuitPin>>();
            return render.LabelHeight;
        }

        //CircuitEditingContext callback 
        internal static Point GetSubContentOffset(AdaptableControl adaptableControl)
        {
            var render = adaptableControl.Cast<D2dGraphAdapter<Module, Connection, ICircuitPin>>().Renderer
            .Cast<D2dCircuitRenderer<Module, Connection, ICircuitPin>>();
            return render.SubContentOffset;
        }
      
        // 
        /// <summary>
        /// This custom writer only writes out the sub-circuits that are actually referenced 
        /// by a SubCircuitInstance</summary>
        private class CircuitWriter : DomXmlWriter
        {
            public CircuitWriter(XmlSchemaTypeCollection typeCollection)
                : base(typeCollection)
            {
                PreserveSimpleElements = true;
            }

            // Scan for all sub-circuits that are referenced directly or indirectly by a module in
            //  the root of the document
            public override void Write(DomNode root, Stream stream, Uri uri)
            {
                m_usedSubCircuits = new HashSet<Sce.Atf.Controls.Adaptable.Graphs.SubCircuit>();

                foreach (var module in root.Cast<Circuit>().Elements)
                    FindUsedSubCircuits(module.DomNode);

                base.Write(root, stream, uri);
            }

            private void FindUsedSubCircuits(DomNode rootNode)
            {
                foreach (DomNode node in rootNode.Subtree)
                {
                    var instance = node.As<SubCircuitInstance>();
                    if (instance != null)
                    {
                        if (m_usedSubCircuits.Add(instance.SubCircuit))
                        {
                            // first time seeing this sub-circuit, so let's recursively add whatever it references
                            foreach (Module module in instance.SubCircuit.Elements)
                                FindUsedSubCircuits(module.DomNode);
                        }
                    }
                }
            }

            // Filter out sub-circuits that are not actually needed
            protected override void WriteElement(DomNode node, System.Xml.XmlWriter writer)
            {
                var subCircuit = node.As<SubCircuit>();
                if (subCircuit != null && !m_usedSubCircuits.Contains(subCircuit))
                    return;

                base.WriteElement(node, writer);
            }

            private HashSet<Sce.Atf.Controls.Adaptable.Graphs.SubCircuit> m_usedSubCircuits;
        }

        /// <summary>
        /// Component that adds module types to the editor</summary>
        [Import(AllowDefault = true)]
        protected ModulePlugin m_modulePlugin;

        [Import] 
        private CircuitControlRegistry m_circuitControlRegistry=null;

        private D2dCircuitRenderer<Module, Connection, ICircuitPin> m_circuitRenderer;
        private D2dSubCircuitRenderer<Module, Connection, ICircuitPin> m_subGraphRenderer;
        private D2dDiagramTheme m_theme;
        private HoverBase m_hoverForm;
        private D2dAdaptableControl m_d2dHoverControl; // a child of hover form

    }
}