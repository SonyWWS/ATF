//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Direct2D;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace StatechartEditorSample
{
    /// <summary>
    /// Editor class that creates and saves statechart documents. 
    /// There is just one instance of this class in this application.
    /// It creates a D2dAdaptableControl with control adapters for each opened document.
    /// It registers this control with the hosting service so that the control appears in the Windows docking framework.
    /// This document client handles file operations, such as saving and closing a document, and
    /// handles application data persistence.</summary>
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
        /// <param name="schemaLoader">Schema loader</param>
        /// <param name="prototypeLister">Prototype lister</param>
        [ImportingConstructor]
        public Editor(
            IControlHostService controlHostService,
            ICommandService commandService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService,
            SchemaLoader schemaLoader,
            PrototypeLister prototypeLister)
        {
            m_controlHostService = controlHostService;
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_documentService = documentService;
            m_prototypeLister = prototypeLister;

            m_schemaLoader = schemaLoader;

            m_diagramTheme = new D2dDiagramTheme();

            D2dGradientStop[] gradStops =
            {
                new D2dGradientStop(Color.WhiteSmoke,0),
                new D2dGradientStop(Color.LightGray,1)
            };                
            m_diagramTheme.FillGradientBrush  = D2dFactory.CreateLinearGradientBrush(gradStops);
            m_diagramTheme.FillBrush = D2dFactory.CreateSolidBrush(Color.WhiteSmoke);
            m_statechartRenderer = new D2dStatechartRenderer<StateBase, Transition>(m_diagramTheme);

            string initialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\components\\wws_atf\\Samples\\StatechartEditor\\data");
            EditorInfo.InitialDirectory = initialDirectory;
        }

        private IControlHostService m_controlHostService;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private IDocumentService m_documentService;
        private PrototypeLister m_prototypeLister;

        [Import(AllowDefault = true)]
        private IStatusService m_statusService = null;

        [Import(AllowDefault = true)]
        private MainForm m_mainForm = null;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;

        [Import(AllowDefault = true)]
        private SingleInstanceService m_singleInstanceService = null;

        #region IInitializable

        /// <summary>
        /// Finishes initializing component by setting up scripting service</summary>
        void IInitializable.Initialize()
        {
            if (m_scriptingService != null)
            {
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(GetType().Assembly);
                m_scriptingService.ImportAllTypes("StatechartEditorSample");
                m_scriptingService.ExecuteStatement("from Sce.Atf.Controls.Adaptable.Graphs import *");
                m_scriptingService.SetVariable("editor", this);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    EditingContext editingContext = m_contextRegistry.GetActiveContext<EditingContext>();
                    ViewingContext viewContext = m_contextRegistry.GetActiveContext<ViewingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("stateChart", editingContext != null ? editingContext.Statechart : null);
                    m_scriptingService.SetVariable("view", viewContext);
                    m_scriptingService.SetVariable("hist", hist);
                };
            }

            if (m_singleInstanceService != null)
                m_singleInstanceService.CommandLineChanged += m_singleInstanceService_CommandLineChanged;
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
        /// Document editor information for statechart editor</summary>
        public static DocumentClientInfo EditorInfo =
            new DocumentClientInfo(Localizer.Localize("Statechart"), ".statechart", null, null);

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
        /// It creates a D2dAdaptableControl with control adapters for the document.
        /// It registers this control with the hosting service so that the control appears 
        /// in the Windows docking framework.</summary>
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
                node = new DomNode(Schema.statechartDocumentType.Type, Schema.statechartRootElement);
                // create an empty root prototype folder
                node.SetChild(
                    Schema.statechartDocumentType.prototypeFolderChild,
                    new DomNode(Schema.prototypeFolderType.Type));
            }

            Document statechartDocument = null;
            if (node != null)
            {
                // AdaptableControl was registered as an extension by the schema loader
                var control = new D2dAdaptableControl();
                control.SuspendLayout();

                control.BackColor = SystemColors.ControlLight;
                control.AllowDrop = true;

                var transformAdapter = new TransformAdapter();
                transformAdapter.UniformScale = true;
                transformAdapter.MinScale = new PointF(0.25f, 0.25f);
                transformAdapter.MaxScale = new PointF(4, 4);
                var viewingAdapter = new ViewingAdapter(transformAdapter);
                var canvasAdapter = new CanvasAdapter(new Rectangle(0, 0, 1000, 1000));

                var autoTranslateAdapter = new AutoTranslateAdapter(transformAdapter);
                var mouseTransformManipulator = new MouseTransformManipulator(transformAdapter);
                var mouseWheelManipulator = new MouseWheelManipulator(transformAdapter);

                var gridAdapter = new D2dGridAdapter();
                gridAdapter.Enabled = false;
                gridAdapter.Visible = true;

                var scrollbarAdapter = new ScrollbarAdapter(transformAdapter, canvasAdapter);

                var hoverAdapter = new HoverAdapter();
                hoverAdapter.HoverStarted += control_HoverStarted;
                hoverAdapter.HoverStopped += control_HoverStopped;

                var annotationAdaptor =                      // display annotations under diagram
                    new D2dAnnotationAdapter(m_diagramTheme);

                var statechartAdapter = new StatechartGraphAdapter(m_statechartRenderer, transformAdapter);

                var statechartStateEditAdapter =
                    new D2dGraphNodeEditAdapter<StateBase, Transition, BoundaryRoute>(m_statechartRenderer, statechartAdapter, transformAdapter);

                var statechartTransitionEditAdapter =
                    new D2dGraphEdgeEditAdapter<StateBase, Transition, BoundaryRoute>(m_statechartRenderer, statechartAdapter, transformAdapter);

                var mouseLayoutManipulator = new MouseLayoutManipulator(transformAdapter);

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
                    new KeyboardGraphNavigator<StateBase, Transition, BoundaryRoute>(),
                    gridAdapter,
                    annotationAdaptor,
                    statechartAdapter,
                    statechartStateEditAdapter,
                    statechartTransitionEditAdapter,
                    new SelectionAdapter(),
                    mouseLayoutManipulator,
                    new LabelEditAdapter(),
                    new DragDropAdapter(m_statusService),
                    new ContextMenuAdapter(m_commandService, m_contextMenuCommandProviders)
                    );

                control.ResumeLayout();

                // associate the control with the data; several of the adapters need the
                //  control for viewing, layout and calculating bounds.
                var viewingContext = node.Cast<ViewingContext>();
                viewingContext.Control = control;

                var boundsValidator = node.Cast<BoundsValidator>();
                boundsValidator.StatechartRenderer = m_statechartRenderer;

                statechartDocument = node.Cast<Document>();
                string fileName = Path.GetFileName(filePath);
                var controlInfo = new ControlInfo(fileName, filePath, StandardControlGroup.Center);
                
                //Set IsDocument to true to prevent exception in command service if two files with the
                //  same name, but in different directories, are opened.
                controlInfo.IsDocument = true;

                statechartDocument.ControlInfo = controlInfo;
                statechartDocument.Uri = uri;

                // now that the data is complete, initialize the rest of the extensions to the Dom data;
                //  this is needed for adapters such as validators, which may not be referenced anywhere
                //  but still need to be instantiated.
                node.InitializeExtensions();

                // set control's context to main editing context
                var context = node.Cast<EditingContext>();
                control.Context = context;

                m_controlHostService.RegisterControl(control, controlInfo, this);
            }

            return statechartDocument;
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            ViewingContext viewingContext = Adapters.Cast<ViewingContext>(document);
            m_controlHostService.Show(viewingContext.Control);
        }

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save(IDocument document, Uri uri)
        {
            Document statechartDocument = document as Document;
            string filePath = uri.LocalPath;
            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                DomXmlWriter writer = new DomXmlWriter(m_schemaLoader.TypeCollection);
                writer.Write(statechartDocument.DomNode, stream, uri);
            }
        }

        /// <summary>
        /// Closes the document and removes any views of it from the UI</summary>
        /// <param name="document">Document to close</param>
        public void Close(IDocument document)
        {
            EditingContext context = Adapters.As<EditingContext>(document);

            // close all active EditingContexts in the document
            foreach (DomNode node in context.DomNode.Subtree)
                foreach (EditingContext editingContext in node.AsAll<EditingContext>())
                    m_contextRegistry.RemoveContext(editingContext);

            // close the document
            m_documentRegistry.Remove(document);

            // finally unregister the control
            ViewingContext viewingContext = Adapters.Cast<ViewingContext>(document);
            m_controlHostService.UnregisterControl(viewingContext.Control);
            viewingContext.Control.Dispose();
            viewingContext.Control = null;
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
            Document document = adaptableControl.ContextAs<Document>();

            m_contextRegistry.ActiveContext = document;
            m_documentRegistry.ActiveDocument = document;
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
        /// Allows the user to save the document before it closes.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control)
        {
            AdaptableControl adaptableControl = (AdaptableControl)control;
            Document document = adaptableControl.ContextAs<Document>();

            bool closed = true;
            if (document != null)
            {
                closed = m_documentService.Close(document);
                if (closed)
                    m_contextRegistry.RemoveContext(document);
            }

            return closed;
        }

        #endregion

        private class StatechartGraphAdapter : D2dGraphAdapter<StateBase, Transition, BoundaryRoute>
        {
            public StatechartGraphAdapter(D2dStatechartRenderer<StateBase, Transition> renderer, TransformAdapter transformAdapter)
                : base(renderer,transformAdapter)
            {
            }

            /// <summary>
            /// Renders entire graph</summary>
            protected override void OnRender()
            {
                D2dGraphics.AntialiasMode = D2dAntialiasMode.PerPrimitive;
                Matrix3x2F invMtrx = D2dGraphics.Transform;
                invMtrx.Invert();
                RectangleF boundsGr = Matrix3x2F.Transform(invMtrx, this.AdaptedControl.ClientRectangle);

                // Draw normal nodes on top of edges
                foreach (StateBase node in Graph.Nodes)
                {
                    RectangleF nodeBounds = Renderer.GetBounds(node, D2dGraphics);
                    if (!boundsGr.IntersectsWith(nodeBounds)) continue;
                    DiagramDrawingStyle style = GetStyle(node);
                    Renderer.Draw(node, style, D2dGraphics);
                }

                // Draw edges last for now. Todo: draw in layers, with edges and then nodes for each layer
                foreach (Transition edge in Graph.Edges)
                {
                    if (edge == HiddenEdge) continue;

                    RectangleF edgeBounds = Renderer.GetBounds(edge, D2dGraphics);
                    if (!boundsGr.IntersectsWith(edgeBounds)) continue;

                    DiagramDrawingStyle style = GetStyle(edge);
                    Renderer.Draw(edge, style, D2dGraphics);
                }
            }
        }

        private void control_HoverStarted(object sender, HoverEventArgs<object, object> e)
        {
            m_hoverForm = GetHoverForm(e.Object);
        }

        private void control_HoverStopped(object sender, EventArgs e)
        {
            if (m_hoverForm != null)
            {
                m_hoverForm.Close();
                m_hoverForm.Dispose();
            }
        }

        private HoverBase GetHoverForm(object hoverItem)
        {
            HoverBase result = CreateHoverForm(hoverItem);
            if (result != null)
            {
                Point p = Control.MousePosition;
                result.Location = new Point(p.X - (result.Width + 12), p.Y + 12);
                result.ShowWithoutFocus();
            }
            return result;
        }

        // create hover form for primitive state or transition
        private static HoverBase CreateHoverForm(object hoverTarget)
        {
            // handle states and transitions
            StringBuilder sb = new StringBuilder();
            ICustomTypeDescriptor customTypeDescriptor = Adapters.As<ICustomTypeDescriptor>(hoverTarget);
            if (customTypeDescriptor != null)
            {
                // Get properties interface
                foreach (System.ComponentModel.PropertyDescriptor property in customTypeDescriptor.GetProperties())
                {
                    object value = property.GetValue(hoverTarget);
                    if (value != null)
                    {
                        sb.Append(property.Name);
                        sb.Append(": ");
                        sb.Append(value.ToString());
                        sb.Append("\n");
                    }
                }
            }

            HoverBase result = null;
            if (sb.Length > 0) // remove trailing '\n'
            {
                sb.Length = sb.Length - 1;
                result = new HoverLabel(sb.ToString());
            }

            return result;
        }

        /// <summary>
        /// Performs custom actions on CommandLineChanged events.
        /// This is also called whenever another instance of this application launches.</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">EventArgs containing event data</param>
        void m_singleInstanceService_CommandLineChanged(object sender, EventArgs e)
        {
            // attempt to load a document whose path is on the command line
            var msg = new StringBuilder(
                "Another instance of StatechartEditorSample was launched. The command line changed to: ".Localize());

            string documentPath = null;
            for (int i = 0; i < m_singleInstanceService.CommandLine.Length; i++)
            {
                string line = m_singleInstanceService.CommandLine[i];

                // assume the last parameter is the filename
                if (i > 0 && i == m_singleInstanceService.CommandLine.Length - 1)
                    documentPath = line;

                msg.AppendLine(line);
            }

            Outputs.WriteLine(OutputMessageType.Info, msg.ToString());

            // we should bring our app to the forefront
            if (m_mainForm != null)
            {
                WinFormsUtil.InvokeIfRequired(m_mainForm, () =>
                    {
                        m_mainForm.BringToFront(); //needs to be called on GUI thread
                        if (documentPath != null)
                        {
                            Uri uri = new Uri(documentPath);
                            if (CanOpen(uri))
                                Open(uri); //needs to be called on GUI thread
                        }
                    });
            }
        }

        private D2dDiagramTheme m_diagramTheme;
        private D2dStatechartRenderer<StateBase, Transition> m_statechartRenderer;

        private SchemaLoader m_schemaLoader;

        private HoverBase m_hoverForm;
    }
}
