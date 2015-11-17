//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Dynamic;

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
    /// Circuit editor. There is just one instance of this class in this application. This class
    /// is added to the Managed Extensibility Framework's TypeCatalog. This class is in charge of:
    /// 1) Opening and closing documents (IDocumentClient).
    /// 2) Creating a circuit canvas control for each opened document.
    /// 3) Registering that circuit canvas control (D2dAdaptableControl) with a IControlHostService
    /// so that the control appears in the Windows docking framework.
    /// 4) Opening up circuit group (also known as a sub-circuit) in a separate window when the
    /// user double-clicks on the circuit group.
    /// 5) Showing a floating circuit preview window when the user hovers the mouse over a closed
    /// circuit group.</summary>
    [Export(typeof(IDocumentClient))]
    [Export(typeof(Editor))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class Editor : DynamicObject, IDocumentClient, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor. Creates circuit and subgraph renderers. 
        /// Creates and configures D2dAdaptableControl to display subcircuit when hovering over it.</summary>
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
            m_layerLister = layerLister;

            m_schemaLoader = schemaLoader;

            string initialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\components\\wws_atf\\Samples\\CircuitEditor\\data");
            EditorInfo.InitialDirectory = initialDirectory;
            m_theme = new D2dDiagramTheme();
            m_circuitRenderer = new CircuitRenderer(m_theme, documentRegistry);
            m_subGraphRenderer = new D2dSubCircuitRenderer<Module, Connection, ICircuitPin>(m_theme);

            //// Note: Santa Monica uses following render settings: 
            //m_circuitRenderer.TitleBackgroundFilled = true;
            //m_circuitRenderer.RoundedBorder = false;
            //m_circuitRenderer.PinDrawStyle = D2dCircuitRenderer<Module, Connection, ICircuitPin>.PinStyle.OnBorderFilled;

            //m_subGraphRenderer.TitleBackgroundFilled = true;
            //m_subGraphRenderer.RoundedBorder = false;
            //m_subGraphRenderer.PinDrawStyle = D2dCircuitRenderer<Module, Connection, ICircuitPin>.PinStyle.OnBorderFilled;

            // create d2dcontrol for displaying sub-circuit            
            m_d2dHoverControl = new D2dAdaptableControl();
            m_d2dHoverControl.Dock = DockStyle.Fill;
            var xformAdapter = new TransformAdapter();
            xformAdapter.EnforceConstraints = false;//to allow the canvas to be panned to view negative coordinates
            m_d2dHoverControl.Adapt(xformAdapter, new D2dGraphAdapter<Module, Connection, ICircuitPin>(m_circuitRenderer, xformAdapter));
            m_d2dHoverControl.DrawingD2d += m_d2dHoverControl_DrawingD2d;           
        }

        private IControlHostService m_controlHostService;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private IDocumentService m_documentService;
        private LayerLister m_layerLister;
        private SchemaLoader m_schemaLoader;

        [Import(AllowDefault = true)]
        private IStatusService m_statusService = null;
        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;
        [Import]
        private IFileDialogService m_fileDialogService = null;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        /// <summary>
        /// Circuit clipboard data type</summary>
        public static readonly string CircuitFormat = Schema.NS + ":Circuit";

        // scripting related members
        [Import(AllowDefault = false)]
        private ScriptingService m_scriptingService = null;

        #region IInitializable

        /// <summary>
        /// Finishes initializing component by setting up scripting service and setting service</summary>
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
                    new BoundPropertyDescriptor(this, () => InitialDirectory,
                        "Initial Directory".Localize(), "Circuit Editor".Localize(),
                        "The initial directory for documents".Localize(),
                        new System.Windows.Forms.Design.FolderNameEditor(), null), 
                };
                m_settingsService.RegisterUserSettings("Circuit Editor", settings);
                m_settingsService.RegisterSettings(this, settings);
            }

            if (m_modulePlugin != null)
            {
                // define pin/connection pens
                var pen = D2dFactory.CreateSolidBrush(Color.LightSeaGreen);
                m_theme.RegisterCustomBrush(m_modulePlugin.BooleanPinTypeName, pen);
                pen = D2dFactory.CreateSolidBrush(Color.LightSeaGreen);
                m_theme.RegisterCustomBrush(m_modulePlugin.FloatPinTypeName, pen);
            }

            D2dGradientStop[] gradstops = 
            { 
                new D2dGradientStop(Color.White, 0),
                new D2dGradientStop(Color.MediumVioletRed, 1.0f),
            };
            m_theme.RegisterCustomBrush(MissingModule.MissingTypeName, D2dFactory.CreateLinearGradientBrush(gradstops));

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
        /// Opens or creates a document at the given URI.
        /// Creates and configures with control adapters D2dAdaptableControl to display subcircuit</summary>
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
                    CircuitReader reader = new CircuitReader(m_schemaLoader);
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
                // now that the data is complete, initialize all other extensions to the Dom data
                node.InitializeExtensions();

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
            ((ILayoutConstraint)canvasAdapter).Enabled = false; //to allow negative coordinates for circuit elements within groups

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

                // The "AllFirst" policy will try to draw edges (wires) before nodes, as much as possible.
                //circuitAdapter.EdgeRenderPolicy = D2dGraphAdapter<Module, Connection, ICircuitPin>.DrawEdgePolicy.AllFirst;
                
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

            control.DoubleClick += control_DoubleClick;
            control.MouseDown += control_MouseDown;
            return control;
        }


        private void control_DoubleClick(object sender, EventArgs e)
        {
            AdaptableControl d2dHoverControl = (AdaptableControl)sender;
            Point clientPoint = d2dHoverControl.PointToClient(Control.MousePosition);

            D2dGraphAdapter<Module, Connection, ICircuitPin> graphAdapter =
                d2dHoverControl.As<D2dGraphAdapter<Module, Connection, ICircuitPin>>();
            GraphHitRecord<Module, Connection, ICircuitPin> hitRecord = graphAdapter.Pick(clientPoint);
            Group subGraph = null;
            var subGraphReference = hitRecord.Node.As<GroupReference>();
            if (subGraphReference != null)
            {
                var templatingContext = m_contextRegistry.GetMostRecentContext<TemplatingContext>();
                if (templatingContext != null && templatingContext.IsExternalTemplate(subGraphReference.Template))
                    return; // templates should only be editable in the document that owns the template
                DialogResult checkResult = DialogResult.No; //direct editing 
                if (checkResult == DialogResult.No)
                {
                    subGraph = subGraphReference.Group.As<Group>();
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
                    string name = string.Empty;
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
                        var transactionContext = group.Cast<DomNodeAdapter>().DomNode.GetRoot().As<ITransactionContext>();
                        transactionContext.DoTransaction(() => group.Expanded = !group.Expanded, "Toggle Group Expansion");
                    }
                }
                else if (hitRecord.Part is ShowPinsToggle)
                {
                    if (e.Clicks == 1)
                    {
                        var element =
                            hitRecord.HitPath != null ?
                            hitRecord.HitPath.Last.Cast<ICircuitElement>() :
                            hitRecord.Item.Cast<ICircuitElement>();
                        var transactionContext = element.Cast<DomNodeAdapter>().DomNode.GetRoot().As<ITransactionContext>();
                        transactionContext.DoTransaction(() =>
                            element.ElementInfo.ShowUnconnectedPins = !element.ElementInfo.ShowUnconnectedPins,
                            "Toggle Show Unconnected Pins");
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
            var viewingContext = document.Cast<ViewingContext>();
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
        /// Requests permission to close the client's Control.
        /// Allows user to save document before closing it.</summary>
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

        /// <summary>
        /// Gets and sets a string to be used as the initial directory for the open/save dialog box
        /// regardless of whatever directory the user may have previously navigated to. The default
        /// value is null. Set to null to cancel this behavior.</summary>
        public string InitialDirectory
        {
            get { return m_fileDialogService.ForcedInitialDirectory; }
            set { m_fileDialogService.ForcedInitialDirectory = value; }
        }

        private void control_HoverStarted(object sender, HoverEventArgs<object, object> e)
        {
            m_hoverForm = GetHoverForm(e);
        }
        private HoverBase GetHoverForm(HoverEventArgs<object, object> e)
        {
            HoverBase result = CreateHoverForm(e);

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

            if (e.SubPart.Is<GroupPin>())
            {
                sb.Append(e.SubPart.Cast<GroupPin>().Name);
                CircuitUtil.GetDomNodeName(e.SubPart.Cast<DomNode>());
            }
            else if (e.SubObject.Is<DomNode>())
            {
                CircuitUtil.GetDomNodeName(e.SubObject.Cast<DomNode>());
            }
            else if (hoverPart.Is<GroupPin>())
            {
                sb.Append(hoverPart.Cast<GroupPin>().Name);
                CircuitUtil.GetDomNodeName(hoverPart.Cast<DomNode>());
            }
            else if (hoverItem.Is<DomNode>())
            {
                CircuitUtil.GetDomNodeName(hoverItem.Cast<DomNode>());
            }

            HoverBase result = null;
            if (sb.Length > 0) // remove trailing '\n'
            {
                //sb.Length = sb.Length - 1;
                result = new HoverLabel(sb.ToString());
            }

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

        /// <summary>
        /// Component that adds module types to the editor</summary>
        [Import(AllowDefault = true)]
        protected ModulePlugin m_modulePlugin;

        [Import]
        private CircuitControlRegistry m_circuitControlRegistry = null;

        private D2dCircuitRenderer<Module, Connection, ICircuitPin> m_circuitRenderer;
        private D2dSubCircuitRenderer<Module, Connection, ICircuitPin> m_subGraphRenderer;
        private D2dDiagramTheme m_theme;
        private HoverBase m_hoverForm;
        private D2dAdaptableControl m_d2dHoverControl; // a child of hover form

        #region Expression related code

        // Allows getting circuit element by id
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // It is possible to use any of the following to get access to all 
            // the circuit elements. 
            var mgr = m_documentRegistry.GetMostRecentDocument<ExpressionManager>();
            if (mgr != null)
            {
                foreach (var node in mgr.ExpressionNodes)
                {
                    if (binder.Name == node.GetId())
                    {
                        result = new DynamicDomNode(node);
                        return true;
                    }                    
                }
            }            
            return base.TryGetMember(binder, out result);         
        }

        #endregion

    }
}