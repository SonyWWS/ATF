//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

namespace DomTreeEditorSample
{
    /// <summary>
    /// Displays a tree view of the DOM data. Uses the context registry to track
    /// the active UI data as documents are opened and closed.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(TreeLister))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TreeLister : TreeControlEditor, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="documentRegistry">Document registry</param>
        /// <param name="documentService">Document service</param>
        [ImportingConstructor]
        public TreeLister(
            ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry,
            IDocumentService documentService)
            : base(commandService)
        {
            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;

            // the tree control always displays the contents of the active document
            m_documentRegistry.ActiveDocumentChanged += documentRegistry_ActiveDocumentChanged;

            m_documentService = documentService;
        }
        private IControlHostService m_controlHostService;
        private IContextRegistry m_contextRegistry;
        private IDocumentRegistry m_documentRegistry;
        private IDocumentService m_documentService;

        /// <summary>
        /// Create and configure TreeControl</summary>
        /// <param name="treeControl">New TreeControl</param>
        /// <param name="treeControlAdapter">Adapter for TreeControl</param>
        protected override void Configure(out TreeControl treeControl, out TreeControlAdapter treeControlAdapter)
        {
            base.Configure(out treeControl, out treeControlAdapter);

            treeControl.ShowRoot = false; // UI node can't really be edited, so hide it
            treeControl.Text = ("Add packages to the UI." + Environment.NewLine +
                                "Add forms, shaders, textures, and fonts to packages." + Environment.NewLine +
                                "Add sprites or text items to forms or sprites." + Environment.NewLine +
                                "Drag shaders, textures, and fonts onto the reference slots of sprites and text items.").Localize();
            treeControl.Dock = DockStyle.Fill;
            treeControl.AllowDrop = true;
            treeControl.SelectionMode = SelectionMode.MultiExtended;
        }

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
            // on control activation, make the UI the active context and document
            if (TreeView != null)
            {
                m_contextRegistry.ActiveContext = TreeView;
                m_documentRegistry.ActiveDocument = TreeView.As<Document>();
            }
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
            bool closed = true;

            // Get current document, if any
            Document document = TreeView.As<Document>();

            // Check if document can be closed
            if (document != null)
            {
                closed = m_documentService.Close(document);
                if (closed)
                    m_contextRegistry.RemoveContext(document);
            }

            return closed;  // app must be closing
        }

        #endregion

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering control</summary>
        void IInitializable.Initialize()
        {
            // on initialization, register our tree control with the hosting service
            m_controlHostService.RegisterControl(
                TreeControl,
                new ControlInfo(
                   "UI Tree Lister".Localize(),
                   "Displays a tree view of the UI".Localize(),
                   StandardControlGroup.CenterPermanent), // don't show close button
               this);
        }

        private void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
        {
            // get the most recent document that can provide a tree view of the UI;
            //  GetActiveDocument would also work, as long as no other component loads documents.
            TreeView treeView = m_documentRegistry.GetMostRecentDocument<TreeView>();
            // if it's different, switch to it
            if (TreeView != treeView)
            {
                TreeView = treeView;

                if (treeView != null)
                {
                    // make document the active context
                    m_contextRegistry.ActiveContext = treeView;

                    // make sure user can see our TreeControl if it was hidden
                    m_controlHostService.Show(TreeControl);
                }
            }
        }
        
        #endregion

        /// <summary>
        /// Raises the LastHitChanged event and performs custom processing</summary>
        /// <param name="e">Event args</param>
        protected override void OnLastHitChanged(EventArgs e)
        {
            base.OnLastHitChanged(e);
            // forward "last hit" information to the editing context which needs to know
            //  where to insert objects during copy/paste and drag/drop. The base tracks
            //  the last clicked and last dragged over tree objects.
            EditingContext context = TreeView.As<EditingContext>();
            if (context != null)
                context.SetInsertionParent(LastHit);
        }
    }
}
