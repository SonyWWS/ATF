//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;

using IControlHostService = Sce.Atf.Wpf.Applications.IControlHostService;
using IControlHostClient = Sce.Atf.Wpf.Applications.IControlHostClient;
using IStatusService = Sce.Atf.Wpf.Applications.IStatusService;

namespace SimpleDomEditorWpfSample
{
    /// <summary>
    /// Resource list editor, which registers a "slave" ListView control to display
    /// and edit the resources that belong to the most recently selected event.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(ResourceListEditor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ResourceListEditor : IInitializable, IControlHostClient
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service for registering the ListView</param>
        /// <param name="commandService">Optional command service for running the context menu</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public ResourceListEditor(
            IControlHostService controlHostService,
            ICommandService commandService,
            IContextRegistry contextRegistry)
        {
            m_controlHostService = controlHostService;
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;

            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by creating ListView and registering the control</summary>
        void IInitializable.Initialize()
        {
            m_resourceListView = new ResourceListView();

            m_resourcesControlDef = new ControlDef()
            {
                Name = "Resources".Localize(),
                Description = "Resources for selected Event".Localize(),
                Group = StandardControlGroup.Bottom,
                Id = s_resourceListEditorId.ToString()
            };

            m_controlHostService.RegisterControl(m_resourcesControlDef, m_resourceListView, this);
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void IControlHostClient.Activate(object control)
        {
            // if our ListView has become active, make the last selected event
            //  the current context.
            if (control == m_resourceListView)
            {
                if (m_event != null)
                    m_contextRegistry.ActiveContext = m_event.Cast<ResourceListContext>();
            }
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void IControlHostClient.Deactivate(object control)
        {
            // nothing to do if our control is deactivated
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <param name="mainWindowClosing">True if the application main window is closing</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.
        /// </remarks>
        bool IControlHostClient.Close(object control, bool mainWindowClosing)
        {
            // always allow control to close
            return true;
        }

        #endregion

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            // make sure we're always tracking the most recently active EventSequenceContext
            EventSequenceContext context = m_contextRegistry.GetMostRecentContext<EventSequenceContext>();
            if (m_eventSequenceContext != context)
            {
                if (m_eventSequenceContext != null)
                {
                    m_eventSequenceContext.SelectionChanged -= eventSequenceContext_SelectionChanged;
                }

                m_eventSequenceContext = context;

                if (m_eventSequenceContext != null)
                {
                    // track the most recently active EventSequenceContext's selection to get the most recently
                    //  selected event.
                    m_eventSequenceContext.SelectionChanged += eventSequenceContext_SelectionChanged;
                }

                UpdateEvent();
            }
        }

        private void UpdateEvent()
        {
            Event nextEvent = null;
            if (m_eventSequenceContext != null)
                nextEvent = m_eventSequenceContext.Selection.GetLastSelected<Event>();

            if (m_event != nextEvent)
            {
                // remove last event's editing context in case it was activated
                if (m_event != null)
                    m_contextRegistry.RemoveContext(m_event.Cast<ResourceListContext>());

                m_event = nextEvent;

                // get next event's editing context and bind to resources list view
                ResourceListContext eventContext = null;
                if (nextEvent != null)
                {
                    eventContext = nextEvent.Cast<ResourceListContext>();
                    eventContext.View = m_resourceListView;
                }

                m_resourceListView.DataContext = eventContext;
            }
        }

        private void eventSequenceContext_SelectionChanged(object sender, EventArgs e)
        {
            UpdateEvent();
        }

        private IControlHostService m_controlHostService;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;

        private Event m_event;
        private EventSequenceContext m_eventSequenceContext;
        private ResourceListView m_resourceListView;
        private ControlDef m_resourcesControlDef;
        
        private static Guid s_resourceListEditorId = new Guid("BEFD9894-BFE1-4AE6-BF9A-570EFF3BCAA7");
    }
}
