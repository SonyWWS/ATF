//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Base class for tree editors; not abstract so it can be used as a generic
    /// tree editor. Uses contexts that implement IHierarchicalInsertionContext and
    /// IInstancingContext.</summary>
    [Export(typeof(TreeControlEditor))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class TreeControlEditor
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        [ImportingConstructor]
        public TreeControlEditor(ICommandService commandService)
        {
            m_commandService = commandService;

            Configure(out m_treeControl, out m_treeControlAdapter);

            m_treeControl.MouseMove += TreeControlMouseMove;
            m_treeControl.MouseUp += TreeControlMouseUp;
            m_treeControl.DragOver += TreeControlDragOver;
            m_treeControl.DragDrop += TreeControlDragDrop;
            m_treeControl.NodeLabelEdited += TreeControlNodeLabelEdited;

            m_treeControlAdapter.LastHitChanged += TreeControlAdapterLastHitChanged;
        }

        /// <summary>
        /// Configures the editor</summary>
        /// <param name="treeControl">Control to display data</param>
        /// <param name="treeControlAdapter">Adapter to drive control. Its ITreeView should
        /// implement IInstancingContext and/or IHierarchicalInsertionContext.</param>
        /// <remarks>Default is to create a TreeControl and TreeControlAdapter,
        /// using the global image lists.</remarks>
        protected virtual void Configure(
            out TreeControl treeControl,
            out TreeControlAdapter treeControlAdapter)
        {
            treeControl =
                new TreeControl
                    {
                        ImageList = ResourceUtil.GetImageList16(),
                        StateImageList = ResourceUtil.GetImageList16()
                    };

            treeControlAdapter = new TreeControlAdapter(treeControl);
        }

        /// <summary>
        /// Gets or sets the tree view displayed by the editor</summary>
        public ITreeView TreeView
        {
            get { return m_treeControlAdapter.TreeView; }
            set { m_treeControlAdapter.TreeView = value; }
        }

        /// <summary>
        /// Gets the tree control used by the editor</summary>
        public TreeControl TreeControl
        {
            get { return m_treeControl; }
        }

        /// <summary>
        /// Gets the adapter used to adapt the ITreeView to the control</summary>
        public TreeControlAdapter TreeControlAdapter
        {
            get { return m_treeControlAdapter; }
        }

        /// <summary>
        /// Gets the last object in the tree view that the user clicked or dragged over</summary>
        public object LastHit
        {
            get { return m_treeControlAdapter.LastHit; }
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
            get
            {
                return
                    m_contextMenuCommandProviders == null
                        ? EmptyEnumerable<IContextMenuCommandProvider>.Instance
                        : m_contextMenuCommandProviders.GetValues();
            }
        }

        /// <summary>
        /// Called for every mouse move event in which drag-and-drop objects are being dragged.
        /// Begins a drag-and-drop operation on the underlying tree control.</summary>
        /// <param name="items">Enumeration of items being dragged</param>
        protected virtual void OnStartDrag(IEnumerable<object> items)
        {
            m_treeControl.DoDragDrop(items, DragDropEffects.All);
        }

        /// <summary>
        /// Called when the underlying tree control raises the MouseUp event</summary>
        /// <param name="e">Event args from the tree control's MouseUp event</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                IEnumerable<object> commands =
                    ContextMenuCommandProviders.GetCommands(TreeView, m_treeControlAdapter.LastHit);

                Point screenPoint = m_treeControl.PointToScreen(new Point(e.X, e.Y));
                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

        /// <summary>
        /// Called when the underlying tree control raises the DragOver event</summary>
        /// <param name="e">Event args from the tree control's DragOver event</param>
        protected virtual void OnDragOver(DragEventArgs e)
        {
            bool showDragBetweenCue = TreeControl.ShowDragBetweenCue;
            TreeControl.ShowDragBetweenCue = false;

            bool canInsert = ApplicationUtil.CanInsert(
                m_treeControlAdapter.TreeView,
                m_treeControlAdapter.LastHit,
                e.Data);
            e.Effect = canInsert ? DragDropEffects.Move : DragDropEffects.None;

            if (showDragBetweenCue != TreeControl.ShowDragBetweenCue)
                TreeControl.Refresh();
        }

        /// <summary>
        /// Called when the underlying tree control raises the DragDrop event</summary>
        /// <param name="e">Event args from the tree control's DragDrop event</param>
        protected virtual void OnDragDrop(DragEventArgs e)
        {
            ApplicationUtil.Insert(
                m_treeControlAdapter.TreeView, 
                m_treeControlAdapter.LastHit, 
                e.Data, 
                "Drag and Drop", 
                m_statusService);

            if (!TreeControl.ShowDragBetweenCue)
                return;

            TreeControl.ShowDragBetweenCue = false;
            TreeControl.Invalidate();
        }

        /// <summary>
        /// Called when the underlying tree control raises the NodeLabelEdited event</summary>
        /// <param name="e">Event args from the underlying tree control's NodeLabelEdited event</param>
        protected virtual void OnNodeLabelEdited(TreeControl.NodeEventArgs e)
        {
            ITreeView treeView = m_treeControlAdapter.TreeView;
            foreach (INamingContext namingContext in treeView.AsAll<INamingContext>())
            {
                if (namingContext.CanSetName(e.Node.Tag))
                {
                    ITransactionContext transactionContext = treeView.As<ITransactionContext>();
                    transactionContext.DoTransaction(
                        () => namingContext.SetName(e.Node.Tag, e.Node.Label),
                        "Edit Label".Localize());
                    break;
                }
            }
        }

        private void TreeControlAdapterLastHitChanged(object sender, EventArgs e)
        {
            OnLastHitChanged(EventArgs.Empty);
        }

        private void TreeControlMouseMove(object sender, MouseEventArgs e)
        {
            if (!m_treeControl.IsDragging)
                return;

            object[] dropObjects = null;

            // Alt-Drag, drag and drop of object under mouse
            if ((Control.ModifierKeys & Keys.Alt) != 0)
            {
                object item = m_treeControlAdapter.GetItemAt(new Point(e.X, e.Y));
                if (item != null)
                    dropObjects = new[] { item };
            }
            else // Normal drag, drag and drop selected objects
            {
                dropObjects = m_treeControlAdapter.GetSelectedItems();
            }

            if (dropObjects != null)
            {
                OnStartDrag(dropObjects);
            }
        }

        private void TreeControlMouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void TreeControlDragOver(object sender, DragEventArgs e)
        {
            OnDragOver(e);
        }

        private void TreeControlDragDrop(object sender, DragEventArgs e)
        {
            OnDragDrop(e);
        }

        private void TreeControlNodeLabelEdited(object sender, TreeControl.NodeEventArgs e)
        {
            OnNodeLabelEdited(e);
        }

        [Import(AllowDefault = true)]
        private IStatusService m_statusService;
        
        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;

        private readonly ICommandService m_commandService;
        private readonly TreeControl m_treeControl;
        private readonly TreeControlAdapter m_treeControlAdapter;
    }
}
