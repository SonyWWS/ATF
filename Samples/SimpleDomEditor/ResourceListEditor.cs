//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace SimpleDomEditorSample
{
    /// <summary>
    /// Resource list editor, which registers a "slave" ListView control to display
    /// and edit the resources that belong to the most recently selected event. It handles
    /// drag and drop, and right-click context menus for the ListView control.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(Editor))]
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

        private IControlHostService m_controlHostService;
        private ICommandService m_commandService;
        private IContextRegistry m_contextRegistry;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        [Import(AllowDefault = true)]
        private IStatusService m_statusService = null;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by creating ListView and initializing it, subscribing to drag events,
        /// and registering the control</summary>
        void IInitializable.Initialize()
        {
            m_resourcesListView = new ListView();
            m_resourcesListView.SmallImageList = ResourceUtil.GetImageList16();
            m_resourcesListView.AllowDrop = true;
            m_resourcesListView.MultiSelect = true;
            m_resourcesListView.AllowColumnReorder = true;
            m_resourcesListView.LabelEdit = true;
            m_resourcesListView.Dock = DockStyle.Fill;

            m_resourcesListView.DragOver += resourcesListView_DragOver;
            m_resourcesListView.DragDrop += resourcesListView_DragDrop;
            m_resourcesListView.MouseUp += resourcesListView_MouseUp;
            m_resourcesListViewAdapter = new ListViewAdapter(m_resourcesListView);
            m_resourcesListViewAdapter.LabelEdited +=
                resourcesListViewAdapter_LabelEdited;

            m_resourcesControlInfo = new ControlInfo(
                "Resources".Localize(),
                "Resources for selected Event".Localize(),
                StandardControlGroup.Bottom);

            m_controlHostService.RegisterControl(m_resourcesListView, m_resourcesControlInfo, this);

            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => ListViewSettings, "ListViewSettings", "", ""));
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the resource ListView state; used to persist that state
        /// with the settings service</summary>
        public string ListViewSettings
        {
            get { return m_resourcesListViewAdapter.Settings; }
            set { m_resourcesListViewAdapter.Settings = value; }
        }

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void IControlHostClient.Activate(Control control)
        {
            // if our ListView has become active, make the last selected event
            //  the current context.
            if (control == m_resourcesListView)
            {
                if (m_event != null)
                    m_contextRegistry.ActiveContext = m_event.Cast<EventContext>();
            }
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        void IControlHostClient.Deactivate(Control control)
        {
            // nothing to do if our control is deactivated
        }

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
        bool IControlHostClient.Close(Control control)
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
                    m_contextRegistry.RemoveContext(m_event.Cast<EventContext>());

                m_event = nextEvent;

                // get next event's editing context and bind to resources list view
                EventContext eventContext = null;
                if (nextEvent != null)
                    eventContext = nextEvent.Cast<EventContext>();

                m_resourcesListViewAdapter.ListView = eventContext;
            }
        }

        private void eventSequenceContext_SelectionChanged(object sender, EventArgs e)
        {
            UpdateEvent();
        }

        private void resourcesListView_MouseUp(object sender, MouseEventArgs e)
        {
            // in case of right click 
            if (e.Button == MouseButtons.Right)
            {
                Control control = sender as Control;
                object target = null;

                IEnumerable<object> commands =
                    m_contextMenuCommandProviders.GetValues().GetCommands(m_eventSequenceContext, target);

                Point screenPoint = control.PointToScreen(new Point(e.X, e.Y));
                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

        private void resourcesListView_DragOver(object sender, DragEventArgs e)
        {
            if (m_event != null)
            {
                EventContext context = m_event.Cast<EventContext>();
                e.Effect = DragDropEffects.None;
                if (context.CanInsert(e.Data))
                {
                    e.Effect = DragDropEffects.Move;
                    ((Control)sender).Focus(); // Focus the list view; this will cause its context to become active
                }
            }
        }

        private void resourcesListView_DragDrop(object sender, DragEventArgs e)
        {
            if (m_event != null)
            {
                IInstancingContext context = m_event.Cast<IInstancingContext>();
                if (context.CanInsert(e.Data))
                {
                    ITransactionContext transactionContext = context as ITransactionContext;
                    transactionContext.DoTransaction(delegate
                    {
                        context.Insert(e.Data);
                    }, "Drag and Drop".Localize());

                    if (m_statusService != null)
                        m_statusService.ShowStatus("Drag and Drop".Localize());
                }
            }
        }

        private void resourcesListViewAdapter_LabelEdited(object sender, LabelEditedEventArgs<object> e)
        {
            Resource resource = e.Item.As<Resource>();
            m_eventSequenceContext.DoTransaction(delegate
            {
                resource.Name = e.Label;
            }, "Rename Resource".Localize());

            if (m_statusService != null)
                m_statusService.ShowStatus("Rename Resource".Localize());
        }

        private Event m_event;
        private EventSequenceContext m_eventSequenceContext;
        private ListView m_resourcesListView;
        private ListViewAdapter m_resourcesListViewAdapter;
        private ControlInfo m_resourcesControlInfo;
    }
}
