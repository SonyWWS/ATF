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
    /// Event list editor, which tracks event sequence contexts and handles
    /// drag and drop, and right-click context menus for the ListView controls
    /// that display event sequences</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(Editor))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class EventListEditor : IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service for registering the ListView</param>
        /// <param name="commandService">Optional command service for running the context menu</param>
        /// <param name="contextRegistry">Context registry</param>
        [ImportingConstructor]
        public EventListEditor(
            IControlHostService controlHostService,
            ICommandService commandService,
            IContextRegistry contextRegistry)
        {
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;

            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
            m_contextRegistry.ContextAdded += contextRegistry_ContextAdded;
            m_contextRegistry.ContextRemoved += contextRegistry_ContextRemoved;
        }
            
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
        /// Finishes initializing component by initializing setting service</summary>
        void IInitializable.Initialize()
        {
            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(this, () => ListViewSettings, "ListViewSettings", "", ""));
            }
        }

        #endregion

        /// <summary>
        /// Persistent ListView settings</summary>
        public string ListViewSettings
        {
            get { return m_listViewSettings; }
            set { m_listViewSettings = value; }
        }

        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            if (m_eventSequenceContext != null)
                m_listViewSettings = m_eventSequenceContext.ListViewAdapter.Settings;

            m_eventSequenceContext = m_contextRegistry.GetMostRecentContext<EventSequenceContext>();
            if (m_eventSequenceContext != null)
                m_eventSequenceContext.ListViewAdapter.Settings = m_listViewSettings;
        }

        private void contextRegistry_ContextAdded(object sender, ItemInsertedEventArgs<object> e)
        {
            EventSequenceContext context = e.Item.As<EventSequenceContext>();
            if (context != null)
            {
                context.ListView.DragOver += listView_DragOver;
                context.ListView.DragDrop += listView_DragDrop;
                context.ListView.MouseUp += listView_MouseUp;

                context.ListViewAdapter.LabelEdited +=
                    listViewAdapter_LabelEdited;
            }
        }

        private void contextRegistry_ContextRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            EventSequenceContext context = e.Item.As<EventSequenceContext>();
            if (context != null)
            {
                context.ListView.DragOver -= listView_DragOver;
                context.ListView.DragDrop -= listView_DragDrop;
                context.ListView.MouseUp -= listView_MouseUp;

                context.ListViewAdapter.LabelEdited -=
                    listViewAdapter_LabelEdited;
            }
        }

        private void listView_MouseUp(object sender, MouseEventArgs e)
        {
            Control control = sender as Control;
            object target = null; // TODO
            if (e.Button == MouseButtons.Right)
            {
                IEnumerable<object> commands =
                    m_contextMenuCommandProviders.GetValues().GetCommands(m_eventSequenceContext, target);

                Point screenPoint = control.PointToScreen(new Point(e.X, e.Y));
                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

        private void listView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (m_eventSequenceContext.CanInsert(e.Data))
            {
                e.Effect = DragDropEffects.Move;
                ((Control)sender).Focus(); // Focus the list view; this will cause its context to become active
            }
        }

        private void listView_DragDrop(object sender, DragEventArgs e)
        {
            IInstancingContext context = m_eventSequenceContext;
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

        private void listViewAdapter_LabelEdited(object sender, LabelEditedEventArgs<object> e)
        {
            Event _event = e.Item.As<Event>();
            m_eventSequenceContext.DoTransaction(delegate
            {
                _event.Name = e.Label;
            }, "Rename Event".Localize());

            if (m_statusService != null)
                m_statusService.ShowStatus("Rename Event".Localize());
        }

        private EventSequenceContext m_eventSequenceContext;
        private string m_listViewSettings;
    }
}
