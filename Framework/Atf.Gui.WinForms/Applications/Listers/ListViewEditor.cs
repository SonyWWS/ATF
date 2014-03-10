//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Base class for list editors; not abstract so it can be used as a generic
    /// list editor</summary>
    [Export(typeof(ListViewEditor))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class ListViewEditor : IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        [ImportingConstructor]
        public ListViewEditor(ICommandService commandService)
        {
            m_commandService = commandService;

            Configure(out m_listView, out m_listViewAdapter);

            m_listView.MouseDown += listView_MouseDown;
            m_listView.MouseMove += listView_MouseMove;
            m_listView.MouseUp += listView_MouseUp;
            m_listView.MouseLeave += listView_MouseLeave;
            m_listView.DragOver += listView_DragOver;
            m_listView.DragDrop += listView_DragDrop;
            m_listView.AfterLabelEdit += listView_AfterLabelEdit;
            m_listView.ColumnWidthChanged += listView_ColumnWidthChanged;

            m_listViewAdapter.LastHitChanged += listViewAdapter_LastHitChanged;
        }

        /// <summary>
        /// Configures the editor</summary>
        /// <param name="listView">Control to display data</param>
        /// <param name="listViewAdapter">Adapter to drive control</param>
        /// <remarks>Default is to create a ListView and ListViewAdapter,
        /// using the global image lists.</remarks>
        protected virtual void Configure(
            out ListView listView,
            out ListViewAdapter listViewAdapter)
        {
            listView = new ListView();
            listView.SmallImageList = ResourceUtil.GetImageList16();
            listView.LargeImageList = ResourceUtil.GetImageList32();

            listViewAdapter = new ListViewAdapter(listView);
        }
       

        #region IInitializable Members

        public virtual void Initialize()
        {
            // Register settings to persist column widths
            // Control.Name must be set before Initialize() to take advantage of this feature
            if (m_settingsService != null && Control != null && !string.IsNullOrEmpty(Control.Name))
            {
                m_settingsService.RegisterSettings(
                    Control.Name,
                    new BoundPropertyDescriptor(this, ()=>Settings, "Settings", "", ""));
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the list view displayed by the editor</summary>
        public IListView ListView
        {
            get { return m_listViewAdapter.ListView; }
            set { m_listViewAdapter.ListView = value; }
        }

        /// <summary>
        /// Gets the list control used by the editor</summary>
        public ListView Control
        {
            get { return m_listView; }
        }

        /// <summary>
        /// Gets the adapter used to adapt the IListView to the control</summary>
        public ListViewAdapter ListViewAdapter
        {
            get { return m_listViewAdapter; }
        }

        /// <summary>
        /// Gets the last object in the list view that the user clicked or dragged over</summary>
        public object LastHit
        {
            get { return m_listViewAdapter.LastHit; }
        }

        /// <summary>
        /// Gets or sets the persistent state for the control</summary>
        internal string Settings
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("Columns");
                xmlDoc.AppendChild(root);

                foreach (var pair in m_columnWidths)
                {
                    XmlElement columnElement = xmlDoc.CreateElement("Column");
                    root.AppendChild(columnElement);

                    columnElement.SetAttribute("Name", pair.Key);
                    columnElement.SetAttribute("Width", pair.Value.ToString());
                }
                return xmlDoc.InnerXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if (root == null || root.Name != "Columns")
                    throw new Exception("Invalid GridModelControl settings");

                XmlNodeList columns = root.SelectNodes("Column");
                foreach (XmlElement columnElement in columns)
                {
                    string name = columnElement.GetAttribute("Name");
                    if (string.IsNullOrEmpty(name))
                        continue;

                    string widthString = columnElement.GetAttribute("Width");
                    int width;
                    if (widthString != null && int.TryParse(widthString, out width))
                    {
                        m_columnWidths[name] = width;
                    }
                }

                Control.SuspendLayout();

                foreach (ColumnHeader column in Control.Columns)
                {
                    string name = column.Text;
                    if (string.IsNullOrEmpty(name))
                        continue;

                    int width;
                    if (m_columnWidths.TryGetValue(name, out width))
                        column.Width = width;
                    else
                        m_columnWidths[name] = column.Width;
                }

                Control.ResumeLayout();
            }
        }

        /// <summary>
        /// Event that is raised after LastHit changes</summary>
        public event EventHandler LastHitChanged;

        /// <summary>
        /// Raises the LastHitChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnLastHitChanged(EventArgs e)
        {
            LastHitChanged.Raise(this, e);
        }

        /// <summary>
        /// Gets the command service, or null if none</summary>
        protected ICommandService CommandService
        {
            get { return m_commandService; }
        }

        /// <summary>
        /// Gets the status service, or null if none</summary>
        protected IStatusService StatusService
        {
            get { return m_statusService; }
        }

        /// <summary>
        /// Gets all context menu command providers</summary>
        protected IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders
        {
            get { return m_contextMenuCommandProviders.GetValues(); }
        }

        /// <summary>
        /// Called for every mouse move event in which drag-and-drop objects are dragged.
        /// Begins a drag-and-drop operation on the underlying ListView.</summary>
        /// <param name="items">Enumeration of items being dragged</param>
        protected virtual void OnStartDrag(IEnumerable<object> items)
        {
            m_listView.DoDragDrop(items, DragDropEffects.All);
        }

        /// <summary>
        /// Called when the underlying ListView raises the MouseUp event</summary>
        /// <param name="e">Event args from the ListView's MouseUp event</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                IEnumerable<object> commands =
                    ContextMenuCommandProviders.GetCommands(ListView, m_listViewAdapter.LastHit);

                Point screenPoint = m_listView.PointToScreen(new Point(e.X, e.Y));
                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

        /// <summary>
        /// Called when the underlying ListView raises the DragOver event</summary>
        /// <param name="e">Event args from the ListView's DragOver event</param>
        protected virtual void OnDragOver(DragEventArgs e)
        {
            if (ApplicationUtil.CanInsert(m_listViewAdapter.ListView, null, e.Data))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Called when the underlying ListView raises the DragDrop event</summary>
        /// <param name="e">Event args from the ListView's DragDrop event</param>
        protected virtual void OnDragDrop(DragEventArgs e)
        {
            ApplicationUtil.Insert(
                m_listViewAdapter.ListView,
                null,
                e.Data,
                "Drag and Drop".Localize(),
                m_statusService);
        }

        /// <summary>
        /// Called when the underlying ListView raises the NodeLabelEdited event</summary>
        /// <param name="e">Event args from the underlying ListView's NodeLabelEdited event</param>
        protected virtual void OnNodeLabelEdited(LabelEditEventArgs e)
        {
            IListView listView = m_listViewAdapter.ListView;
            ListViewItem item = m_listView.Items[e.Item];
            INamingContext namingContext = listView.As<INamingContext>();
            if (namingContext != null &&
                namingContext.CanSetName(item.Tag))
            {
                ITransactionContext transactionContext = listView.As<ITransactionContext>();
                transactionContext.DoTransaction(
                    delegate
                    {
                        namingContext.SetName(item.Tag, e.Label);
                    }, "Edit Label".Localize());
            }
        }

        /// <summary>
        /// Called when the underlying ListView raises the ColumnWidthChanged event</summary>
        /// <param name="e">Event args from the underlying ListView's ColumnWidthChanged event</param>
        protected virtual void OnColumnWidthChanged(ColumnWidthChangedEventArgs e)
        {
            ColumnHeader column = Control.Columns[e.ColumnIndex];
            string name = column.Text;
            if (!string.IsNullOrEmpty(name))
                m_columnWidths[name] = column.Width;
        }

        private void listViewAdapter_LastHitChanged(object sender, EventArgs e)
        {
            OnLastHitChanged(EventArgs.Empty);
        }

        private void listView_MouseDown(object sender, MouseEventArgs e)
        {
            m_mouseDownPoint = new Point(e.X, e.Y);
        }

        private void listView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left &&
                !m_dragging &&
                m_listView.AllowDrop)
            {
                Size dragSize = SystemInformation.DragSize;
                if (Math.Abs(e.X - m_mouseDownPoint.X) > dragSize.Width ||
                    Math.Abs(e.Y - m_mouseDownPoint.Y) > dragSize.Height)
                {
                    m_dragging = true;
                }
            }

            if (m_dragging)
            {
                object[] dropObjects = null;

                // Alt-Drag, drag and drop of object under mouse
                if ((System.Windows.Forms.Control.ModifierKeys & Keys.Alt) != 0)
                {
                    object item = m_listViewAdapter.GetItemAt(new Point(e.X, e.Y));
                    if (item != null)
                        dropObjects = new[] { item };
                }
                else // Normal drag, drag and drop selected objects
                {
                    dropObjects = m_listViewAdapter.GetSelectedItems();
                }

                if (dropObjects != null)
                {
                    OnStartDrag(dropObjects);
                }
            }
        }

        private void listView_MouseUp(object sender, MouseEventArgs e)
        {
            m_dragging = false;

            OnMouseUp(e);
        }

        private void listView_MouseLeave(object sender, EventArgs e)
        {
            m_dragging = false;
        }

        private void listView_DragOver(object sender, DragEventArgs e)
        {
            OnDragOver(e);
        }

        private void listView_DragDrop(object sender, DragEventArgs e)
        {
            OnDragDrop(e);
        }

        private void listView_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            OnNodeLabelEdited(e);
        }

        private void listView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            OnColumnWidthChanged(e);
        }

        [Import(AllowDefault = true)]
        private IStatusService m_statusService;

        [Import(AllowDefault = true)] 
        private ISettingsService m_settingsService;
        
        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

        private readonly ICommandService m_commandService;
        private readonly ListView m_listView;
        private readonly ListViewAdapter m_listViewAdapter;
        private readonly Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();

        private Point m_mouseDownPoint;
        private bool m_dragging;
    }
}
