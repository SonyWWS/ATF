//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Dom;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Allows the search/replace UI for the currently active context to be displayed in a control host client</summary>
    [Export(typeof(DomNodePropertySearchService))]
    [Export(typeof(IInitializable))]
    public class DomNodePropertySearchService : IInitializable, IControlHostClient
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public DomNodePropertySearchService(
            IContextRegistry contextRegistry,
            IControlHostService controlHostService)
        {
            m_contextRegistry = contextRegistry;
            m_controlHostService = controlHostService;

            // define root control
            m_rootControl = new UserControl();
            m_rootControl.Name = "Search and Replace";
            m_rootControl.SuspendLayout();
            m_rootControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // Create and add the search input control
            var domNodeSearchToolStrip = new DomNodeSearchToolStrip();
            SearchUI = domNodeSearchToolStrip;
            SearchUI.Control.Dock = DockStyle.None;
            m_rootControl.Controls.Add(SearchUI.Control);
            SearchUI.UIChanged += UIElement_Changed;

            // Create and add the replace input control
            var domNodeReplaceToolStrip = new DomNodeReplaceToolStrip();
            domNodeReplaceToolStrip.DomNodeSearchToolStrip = domNodeSearchToolStrip;
            ReplaceUI = domNodeReplaceToolStrip;
            ReplaceUI.Control.Dock = DockStyle.None;
            m_rootControl.Controls.Add(ReplaceUI.Control);
            ReplaceUI.UIChanged += UIElement_Changed;
            

            // Create and add the results output control
            ResultsUI = new DomNodeSearchResultsListView(m_contextRegistry);
            ResultsUI.Control.Dock = DockStyle.None;
            m_rootControl.Controls.Add(ResultsUI.Control);
            ResultsUI.UIChanged += UIElement_Changed;            
            
            m_rootControl.Layout += controls_Layout;
            m_rootControl.ResumeLayout();
        }

        /// <summary>
        /// Shows the main control</summary>
        public void Show()
        {
            m_controlHostService.Show(m_rootControl);
        }

        /// <summary>
        /// Callback to force the results UI to take up the remaining space in its parent control</summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event args</param>
        private void controls_Layout(object sender, LayoutEventArgs e)
        {
            DoLayout();
        }

        /// <summary>
        /// Forces the UI elements to take up the remaining space in their parent control</summary>
        protected virtual void DoLayout()
        {
            if (SearchUI != null)
            {
                SearchUI.Control.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                Rectangle scb = SearchUI.Control.Bounds;
                if (ResultsUI != null)
                {
                    Rectangle resultsBounds = scb;
                    resultsBounds.Y += scb.Height;
                    resultsBounds.Width = m_rootControl.Width - (m_rootControl.Margin.Left + m_rootControl.Margin.Right);
                    resultsBounds.Height = m_rootControl.Height - (m_rootControl.Margin.Top + m_rootControl.Margin.Bottom + scb.Height + 2);
                    ResultsUI.Control.Bounds = resultsBounds;
                    ResultsUI.Control.Anchor = AnchorStyles.None;
                }

                if (ReplaceUI != null)
                {
                    Rectangle replaceBounds = scb;
                    replaceBounds.X += scb.Width;
                    ReplaceUI.Control.Bounds = replaceBounds;
                    ReplaceUI.Control.Anchor = AnchorStyles.None;
                }
            }
        }

        #region IInitializable Members

        /// <summary>
        /// Initialize the service by registering the control</summary>
        void IInitializable.Initialize()
        {
            Initialize();
        }

        #endregion

        /// <summary>
        /// Initialize instance</summary>
        protected virtual void Initialize()
        {
            m_controlHostService.RegisterControl(m_rootControl,
                "Search and Replace".Localize(),
                "Search for elements managed within the currently selected subwindow, and optionally replace their values".Localize(),
                StandardControlGroup.Left,
                this);

            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
        }

        /// <summary>
        /// Performs processing when UI element changed</summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event args</param>
        public void UIElement_Changed(object sender, EventArgs e)
        {
            DoLayout();
        }

        /// <summary>
        /// Tracks the new active context if it is ISearchableContext</summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Event args</param>
        protected virtual void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            SearchUI.Bind(m_contextRegistry.GetActiveContext<IQueryableContext>());
            ResultsUI.Bind(m_contextRegistry.GetActiveContext<IQueryableResultContext>());
            ReplaceUI.Bind(m_contextRegistry.GetActiveContext<IQueryableReplaceContext>());
        }

        ISearchUI m_searchUI;
        IReplaceUI m_replaceUI;
        IResultsUI m_resultsUI;

        /// <summary>
        /// Gets search UI</summary>
        protected virtual ISearchUI SearchUI { get { return m_searchUI; } private set { m_searchUI = value; } }
        /// <summary>
        /// Gets replace UI</summary>
        protected virtual IReplaceUI ReplaceUI { get { return m_replaceUI; } private set { m_replaceUI = value; } }
        /// <summary>
        /// Gets search results UI</summary>
        protected virtual IResultsUI ResultsUI { get { return m_resultsUI; } private set { m_resultsUI = value; } }

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        public virtual void Activate(Control control) { }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        public virtual void Deactivate(Control control) { }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns><c>True</c> if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public virtual bool Close(Control control) { return true; }

        #endregion

        /// <summary>
        /// Root UserControl for replacement control</summary>
        protected UserControl m_rootControl;
        /// <summary>
        /// IContextRegistry</summary>
        protected IContextRegistry m_contextRegistry;
        /// <summary>
        /// IControlHostService</summary>
        protected IControlHostService m_controlHostService;
    }
}
