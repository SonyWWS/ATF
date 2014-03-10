//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Editor providing a hierarchical tree control, listing the contents of a loaded document</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ProjectLister))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ProjectLister : FilteredTreeControlEditor, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service</param>
        /// <param name="controlHostService">Control host service</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="documentRegistry">Document registry</param>
        [ImportingConstructor]
        public ProjectLister(
            ICommandService commandService,
            IControlHostService controlHostService,
            IContextRegistry contextRegistry,
            IDocumentRegistry documentRegistry)
            : base(commandService)
        {
            m_controlHostService = controlHostService;
            m_contextRegistry = contextRegistry;
            m_documentRegistry = documentRegistry;

            // The tree control always displays the contents of the active document
            m_documentRegistry.ActiveDocumentChanged += documentRegistry_ActiveDocumentChanged;
        }

        /// <summary>
        /// Configure the ProjectLister</summary>
        /// <param name="treeControl">TreeControl used by ProjectLister</param>
        /// <param name="treeControlAdapter">TreeControlAdapter used by ProjectLister</param>
        protected override void Configure(out Sce.Atf.Controls.TreeControl treeControl, out TreeControlAdapter treeControlAdapter)
        {
            base.Configure(out treeControl, out treeControlAdapter);
            treeControl.AllowDrop = true;
        }

        /// <summary>
        /// Custom processing for active document changing event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void documentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
        {
            ITreeView treeView = m_documentRegistry.GetMostRecentDocument<ITreeView>();
            if (treeView != null)
                treeView = new FilteredTreeView(treeView, DefaultFilter);

            // If it's different, switch to it
            if (!FilteredTreeView.Equals(TreeView, treeView))
            {
                if (TreeView != null)
                    m_contextRegistry.RemoveContext(TreeView);

                TreeView = treeView;
                
                if (treeView != null)
                {
                    // Make document the active context
                    m_contextRegistry.ActiveContext = treeView;

                    // Make sure user can see our TreeControl if it was hidden
                    m_controlHostService.Show(TreeControl);
                }
            }
        }

        #region IInitializable Members

        public void Initialize()
        {
            // on initialization, register our tree control with the hosting service
            m_controlHostService.RegisterControl(
               Control,
                new ControlInfo(
                   "Project Lister".Localize(),
                   "Lists objects in the current document".Localize(),
                   StandardControlGroup.Left),
               this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its control has been activated. Activation occurs when
        /// the control gets focus, or a parent "host" control gets focus.</summary>
        /// <param name="control">Client control that was activated</param>
        void IControlHostClient.Activate(Control control)
        {
            if (TreeView != null)
                m_contextRegistry.ActiveContext = TreeView;
        }

        /// <summary>
        /// Notifies the client that its control has been deactivated. Deactivation occurs when
        /// another control or "host" control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        void IControlHostClient.Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's control</summary>
        /// <param name="control">Client control to be closed</param>
        /// <returns>True if the control can close, or false to cancel closing</returns>
        bool IControlHostClient.Close(Control control)
        {
            return true;
        }

        #endregion

        private readonly IControlHostService m_controlHostService;
        private readonly IContextRegistry m_contextRegistry;
        private readonly IDocumentRegistry m_documentRegistry;
    }
}
