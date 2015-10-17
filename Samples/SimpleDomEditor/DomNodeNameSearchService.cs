//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// Defines a simple GUI for search and replacement of DomNode names on the currently 
    /// active document. It comprises VERY loosely coupled elements, each managing a part 
    /// of this process, and in fact does little more than bind these elements to the currently 
    /// active context.</summary>
    [Export(typeof(DomNodeNameSearchService))]
    [Export(typeof(IInitializable))]
    public class DomNodeNameSearchService : IInitializable, IControlHostClient
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public DomNodeNameSearchService(
            IContextRegistry contextRegistry,
            IControlHostService controlHostService)
        {
            m_contextRegistry = contextRegistry;
            m_controlHostService = controlHostService;

            m_rootControl = new DomNodeNameSearchControl();
            m_rootControl.Name = "DomNode Name Search and Replace";
            m_rootControl.SuspendLayout();
            m_rootControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            m_rootControl.ResumeLayout();
        }

        #region IInitializable members

        /// <summary>
        /// Finishes initializing component by registering the control</summary>
        public void Initialize()
        {
            m_controlHostService.RegisterControl(m_rootControl, "Search and Replace - DomNode Names".Localize(), "Provide simple GUI for matching on, and replacing, DomNode names of the active document".Localize(), StandardControlGroup.Left, this);

            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control) { }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate(Control control) { }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns><c>True</c> if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control) { return true; }

        #endregion

        /// <summary>
        /// If possible, bind the controls to the data of the currently active context</summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event args</param>
        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            SearchUI.Bind(m_contextRegistry.GetActiveContext<IQueryableContext>());
            ResultsUI.Bind(m_contextRegistry.GetActiveContext<IQueryableResultContext>());
            ReplaceUI.Bind(m_contextRegistry.GetActiveContext<IQueryableReplaceContext>());
        }

        // access to search, results, and replace controls 
        private DomNodeNameSearchControl m_rootControl;
        private ISearchUI SearchUI { get { return m_rootControl.SearchUI; } }
        private IResultsUI ResultsUI { get { return m_rootControl.ResultsUI; } }
        private IReplaceUI ReplaceUI { get { return m_rootControl.ReplaceUI; } }

        private IContextRegistry m_contextRegistry;
        private IControlHostService m_controlHostService;
    }
}
