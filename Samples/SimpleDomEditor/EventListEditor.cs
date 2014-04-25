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
            m_controlHostService = controlHostService;
            m_commandService = commandService;
            m_contextRegistry = contextRegistry;

            m_contextRegistry.ActiveContextChanged += new EventHandler(contextRegistry_ActiveContextChanged);
            m_contextRegistry.ContextAdded += new EventHandler<ItemInsertedEventArgs<object>>(contextRegistry_ContextAdded);
            m_contextRegistry.ContextRemoved += new EventHandler<ItemRemovedEventArgs<object>>(contextRegistry_ContextRemoved);
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
        /// Finishes initializing component by initializing setting service</summary>
        void IInitializable.Initialize()
        {
            if (m_settingsService != null)
            {
                SettingsServices.RegisterSettings(
                    m_settingsService,
                    this,
                    new BoundPropertyDescriptor(this, () => ListViewSettings, "ListViewSettings", "", "")
                );
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
            EventSequenceContext context = Adapters.As<EventSequenceContext>(e.Item);
            if (context != null)
            {
                context.ListView.DragOver += new DragEventHandler(listView_DragOver);
                context.ListView.DragDrop += new DragEventHandler(listView_DragDrop);
                context.ListView.MouseUp += new MouseEventHandler(listView_MouseUp);

                context.ListViewAdapter.LabelEdited +=
                    new EventHandler<LabelEditedEventArgs<object>>(listViewAdapter_LabelEdited);
            }
        }

        private void contextRegistry_ContextRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            EventSequenceContext context = Adapters.As<EventSequenceContext>(e.Item);
            if (context != null)
            {
                context.ListView.DragOver -= new DragEventHandler(listView_DragOver);
                context.ListView.DragDrop -= new DragEventHandler(listView_DragDrop);
                context.ListView.MouseUp -= new MouseEventHandler(listView_MouseUp);

                context.ListViewAdapter.LabelEdited -=
                    new EventHandler<LabelEditedEventArgs<object>>(listViewAdapter_LabelEdited);
            }
        }

        private void listView_MouseUp(object sender, MouseEventArgs e)
        {
            Control control = sender as Control;
            object target = null; // TODO
            if (e.Button == MouseButtons.Right)
            {
                IEnumerable<object> commands =
                    ContextMenuCommandProvider.GetCommands(
                        Lazies.GetValues(m_contextMenuCommandProviders), m_eventSequenceContext, target);

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
                TransactionContexts.DoTransaction(transactionContext,
                    delegate
                    {
                        context.Insert(e.Data);
                    },
                    Localizer.Localize("Drag and Drop"));

                if (m_statusService != null)
                    m_statusService.ShowStatus(Localizer.Localize("Drag and Drop"));
            }
        }

        private void listViewAdapter_LabelEdited(object sender, LabelEditedEventArgs<object> e)
        {
            Event _event = Adapters.As<Event>(e.Item);
            TransactionContexts.DoTransaction(
                m_eventSequenceContext,
                delegate
                {
                    _event.Name = e.Label;
                },
                Localizer.Localize("Rename Event"));

            if (m_statusService != null)
                m_statusService.ShowStatus(Localizer.Localize("Rename Event"));
        }

        private EventSequenceContext m_eventSequenceContext;
        private string m_listViewSettings;
    }
}
