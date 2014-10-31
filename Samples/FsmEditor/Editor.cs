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
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.Adaptable;
using Sce.Atf.Controls.Adaptable.Graphs;
using Sce.Atf.Dom;

namespace FsmEditorSample
{
    /// <summary>
    /// FSM editor, which opens and closes FSM documents and manages the
    /// document editing controls. The document client handles file operations,
    /// such as opening and closing documents.</summary>
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
        /// <param name="schemaLoader">Schema loader</param>
        /// <param name="diagramTheme">Diagram theme, which determines how elements in diagrams are rendered and picked</param>
        [ImportingConstructor]
        public Editor(
            IControlHostService controlHostService,
            ICommandService commandService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService,
            PrototypeLister prototypeLister,
            SchemaLoader schemaLoader,
            DiagramTheme diagramTheme)
        {
            m_controlHostService = controlHostService;
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;
            m_documentService = documentService;

            m_schemaLoader = schemaLoader;

            m_theme = new D2dDiagramTheme();
            m_fsmRenderer = new D2dDigraphRenderer<State, Transition>(m_theme);

            string initialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\..\\..\\components\\wws_atf\\Samples\\FsmEditor\\data");
            EditorInfo.InitialDirectory = initialDirectory;
        }

        private IControlHostService m_controlHostService;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private IDocumentService m_documentService;
        private SchemaLoader m_schemaLoader;
        
        [Import(AllowDefault = true)]
        private IStatusService m_statusService = null;
        
        // scripting related members
        [Import(AllowDefault = true)]
        private ScriptingService m_scriptingService = null;
        
        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by setting up the scripting service</summary>
        void IInitializable.Initialize()
        {
            if (m_scriptingService != null)
            {
                // load this assembly into script domain.
                m_scriptingService.LoadAssembly(GetType().Assembly);
                m_scriptingService.ImportAllTypes("FsmEditorSample");
                m_scriptingService.ImportAllTypes("Sce.Atf.Controls.Adaptable.Graphs");

                m_scriptingService.SetVariable("editor", this);

                m_contextRegistry.ActiveContextChanged += delegate
                {
                    EditingContext editingContext = m_contextRegistry.GetActiveContext<EditingContext>();
                    ViewingContext viewContext = m_contextRegistry.GetActiveContext<ViewingContext>();
                    IHistoryContext hist = m_contextRegistry.GetActiveContext<IHistoryContext>();
                    m_scriptingService.SetVariable("editingContext", editingContext);
                    m_scriptingService.SetVariable("fsm", editingContext != null ? editingContext.Fsm : null);
                    m_scriptingService.SetVariable("view", viewContext);
                    m_scriptingService.SetVariable("hist", hist);
                };
            }
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
        /// Document editor information for FSM editor</summary>
        public static DocumentClientInfo EditorInfo =
            new DocumentClientInfo(Localizer.Localize("Finite State Machine"), ".fsm", null, null);

        /// <summary>
        /// Returns whether the client can open or create a document at the given URI</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>True iff the client can open or create a document at the given URI</returns>
        public bool CanOpen(Uri uri)
        {
            return EditorInfo.IsCompatibleUri(uri);
        }

        /// <summary>
        /// Opens or creates a document at the given URI. Create an AdaptableControl with control adapters for viewing state machine.
        /// Handles application data persistence.</summary>
        /// <param name="uri">Document URI</param>
        /// <returns>Document, or null if the document couldn't be opened or created</returns>
        public IDocument Open(Uri uri)
        {
            DomNode node = null;
            string filePath = uri.LocalPath;
            string fileName = Path.GetFileName(filePath);

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
                node = new DomNode(Schema.fsmType.Type, Schema.fsmRootElement);
                // create an empty root prototype folder
                node.SetChild(
                    Schema.fsmType.prototypeFolderChild,
                    new DomNode(Schema.prototypeFolderType.Type));
            }

            Document document = null;
            if (node != null)
            {
                // set up the AdaptableControl for editing FSMs
                var control = new D2dAdaptableControl();
                control.SuspendLayout();

                control.BackColor = SystemColors.ControlLight;
                control.AllowDrop = true;

                var transformAdapter = new TransformAdapter(); // required by several of the other adapters
                transformAdapter.UniformScale = true;
                transformAdapter.MinScale = new PointF(0.25f, 0.25f);
                transformAdapter.MaxScale = new PointF(4, 4);

                var viewingAdapter = new ViewingAdapter(transformAdapter); // implements IViewingContext for framing or ensuring that items are visible

                var canvasAdapter = new CanvasAdapter(); // implements a bounded canvas to limit scrolling

                var autoTranslateAdapter = // implements auto translate when the user drags out of control's client area
                    new AutoTranslateAdapter(transformAdapter);
                var mouseTransformManipulator = // implements mouse drag translate and scale
                    new MouseTransformManipulator(transformAdapter);
                var mouseWheelManipulator = // implements mouse wheel scale
                    new MouseWheelManipulator(transformAdapter);
                var scrollbarAdapter = // adds scroll bars to control, driven by canvas and transform
                    new ScrollbarAdapter(transformAdapter, canvasAdapter);

                var hoverAdapter = new HoverAdapter(); // add hover events over pickable items
                hoverAdapter.HoverStarted += control_HoverStarted;
                hoverAdapter.HoverStopped += control_HoverStopped;

                var annotationAdaptor = new D2dAnnotationAdapter(m_theme); // display annotations under diagram

                var fsmAdapter = // adapt control to allow binding to graph data
                    new D2dGraphAdapter<State, Transition, NumberedRoute>(m_fsmRenderer, transformAdapter);

                var fsmStateEditAdapter = // adapt control to allow state editing
                    new D2dGraphNodeEditAdapter<State, Transition, NumberedRoute>(m_fsmRenderer, fsmAdapter, transformAdapter);

                var fsmTransitionEditAdapter = // adapt control to allow transition
                    new D2dGraphEdgeEditAdapter<State, Transition, NumberedRoute>(m_fsmRenderer, fsmAdapter, transformAdapter);

                var mouseLayoutManipulator = new MouseLayoutManipulator(transformAdapter);

                // apply adapters to control; ordering is from back to front, that is, the first adapter
                //  will be conceptually underneath all the others. Mouse and keyboard events are fed to
                //  the adapters in the reverse order, so it all makes sense to the user.
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
                    new KeyboardGraphNavigator<State, Transition, NumberedRoute>(),
                    //new GridAdapter(),
                    annotationAdaptor,
                    fsmAdapter,
                    fsmStateEditAdapter,
                    fsmTransitionEditAdapter,
                    new LabelEditAdapter(),
                    new SelectionAdapter(),
                    mouseLayoutManipulator,
                    new DragDropAdapter(m_statusService),
                    new ContextMenuAdapter(m_commandService, m_contextMenuCommandProviders)
                    );

                control.ResumeLayout();

                // associate the control with the viewing context; other adapters use this
                //  adapter for viewing, layout and calculating bounds.
                ViewingContext viewingContext = node.Cast<ViewingContext>();
                viewingContext.Control = control;

                // set document URI
                document = node.As<Document>();
                ControlInfo controlInfo = new ControlInfo(fileName, filePath, StandardControlGroup.Center);

                //Set IsDocument to true to prevent exception in command service if two files with the
                //  same name, but in different directories, are opened.
                controlInfo.IsDocument = true;

                document.ControlInfo = controlInfo;
                document.Uri = uri;

                // now that the data is complete, initialize the rest of the extensions to the Dom data;
                //  this is needed for adapters such as validators, which may not be referenced anywhere
                //  but still need to be initialized.
                node.InitializeExtensions();

                // set control's context to main editing context
                EditingContext editingContext = node.Cast<EditingContext>();
                control.Context = editingContext;

                // show the FSM control
                m_controlHostService.RegisterControl(control, controlInfo, this);
            }
            
            return document;
        }

        /// <summary>
        /// Makes the document visible to the user</summary>
        /// <param name="document">Document to show</param>
        public void Show(IDocument document)
        {
            ViewingContext viewingContext = Adapters.As<ViewingContext>(document);
            m_controlHostService.Show(viewingContext.Control);
        }

        /// <summary>
        /// Saves the document at the given URI</summary>
        /// <param name="document">Document to save</param>
        /// <param name="uri">New document URI</param>
        public void Save(IDocument document, Uri uri)
        {
            string filePath = uri.LocalPath;
            FileMode fileMode = File.Exists(filePath) ? FileMode.Truncate : FileMode.OpenOrCreate;
            using (FileStream stream = new FileStream(filePath, fileMode))
            {
                DomXmlWriter writer = new DomXmlWriter(m_schemaLoader.TypeCollection);
                Document fsmDocument = (Document)document;
                writer.Write(fsmDocument.DomNode, stream, uri);
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

            // finally unregister the control and release the reference to it
            ViewingContext viewingContext = Adapters.As<ViewingContext>(document);
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
            EditingContext context = adaptableControl.ContextAs<EditingContext>();
            m_contextRegistry.ActiveContext = context;

            Document document = context.As<Document>();
            if (document != null)
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
        /// Allows user to save document before closing.</summary>
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
            EditingContext context = adaptableControl.ContextAs<EditingContext>();
            Document document = context.As<Document>();

            bool closed = true;
            if (document != null)
                closed = m_documentService.Close(document);

            if (closed)
                m_contextRegistry.RemoveContext(document);

            return closed;
        }

        #endregion

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

        private D2dDigraphRenderer<State, Transition> m_fsmRenderer;
        private HoverBase m_hoverForm;
        private D2dDiagramTheme m_theme;
    }
}
