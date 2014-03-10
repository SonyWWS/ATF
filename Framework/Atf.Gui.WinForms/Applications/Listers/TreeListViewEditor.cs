//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Combines a TreeListViewAdapter with right click context menu editing</summary>
    [InheritedExport(typeof(TreeListViewEditor))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public abstract class TreeListViewEditor : IAdaptable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="style">TreeListView style</param>
        protected TreeListViewEditor(TreeListView.Style style)
        {
            m_treeListView = new TreeListView(style);
            m_treeListView.Control.KeyDown += ControlKeyDown;
            m_treeListView.Control.KeyPress += ControlKeyPress;
            m_treeListView.Control.KeyUp += ControlKeyUp;
            m_treeListView.Control.MouseClick += ControlMouseClick;
            m_treeListView.Control.MouseDoubleClick += ControlMouseDoubleClick;
            m_treeListView.Control.MouseDown += ControlMouseDown;
            m_treeListView.Control.MouseUp += ControlMouseUp;
            m_treeListView.DragOver += TreeListViewDragOver;
            m_treeListView.DragDrop += TreeListViewDragDrop;
            m_treeListView.NodeDrag += TreeListViewNodeDrag;

            m_treeListViewAdapter = new TreeListViewAdapter(m_treeListView);
        }

        #region IAdaptable Interface

        /// <summary>
        /// Gets an adapter for a type</summary>
        /// <param name="type">Type to get adapter for</param>
        /// <returns>Adapter or null</returns>
        public virtual object GetAdapter(Type type)
        {
            // Try to convert to sub-parts

            if (type.Equals(typeof(TreeListView)))
                return TreeListView;

            if (type.Equals(typeof(Control)))
                return TreeListView;

            if (type.Equals(typeof(TreeListViewAdapter)))
                return TreeListViewAdapter;

            return type.Equals(typeof(ITreeListView)) ? View : null;
        }
        
        #endregion

        /// <summary>
        /// Gets or sets the underlying data model</summary>
        public ITreeListView View
        {
            get { return m_treeListViewAdapter.View; }
            set { m_treeListViewAdapter.View = value; }
        }

        /// <summary>
        /// Gets the underlying TreeListView</summary>
        public TreeListView TreeListView
        {
            get { return m_treeListView; }
        }

        /// <summary>
        /// Gets the underlying TreeListViewAdapter</summary>
        public TreeListViewAdapter TreeListViewAdapter
        {
            get { return m_treeListViewAdapter; }
        }

        /// <summary>
        /// Gets adapted user data at a point</summary>
        /// <param name="clientPoint">Point</param>
        /// <returns>User data or null</returns>
        public object GetItemAt(Point clientPoint)
        {
            return TreeListViewAdapter.GetItemAt(clientPoint);
        }

        /// <summary>
        /// Gets the column index of a particular item at a particular point</summary>
        /// <param name="clientPoint">Point</param>
        /// <returns>Column index, or -1 if unknown or invalid item</returns>
        public int GetItemColumnIndexAt(Point clientPoint)
        {
            return m_treeListView.GetNodeColumnIndexAt(clientPoint);
        }

        /// <summary>
        /// Gets the last selected item on the underlying TreeListView but
        /// adapted to the underlying user data type</summary>
        public object LastHit
        {
            get
            {
                object lastHit = m_treeListViewAdapter.LastHit;
                return ReferenceEquals(lastHit, TreeListViewAdapter) ? this : lastHit;
            }
        }

        /// <summary>
        /// Gets or sets the selection as adapted user data</summary>
        public IEnumerable<object> Selection
        {
            get { return TreeListViewAdapter.Selection; }
            set { TreeListViewAdapter.Selection = value; }
        }

        /// <summary>
        /// Gets the selection as the specified type</summary>
        /// <typeparam name="T">Type to try to adapt to</typeparam>
        /// <returns>Selection as the specified type</returns>
        public IEnumerable<T> SelectionAs<T>() where T : class
        {
            return Selection.Where(i => i.Is<T>()).Select(i => i.As<T>());
        }

        /// <summary>
        /// Event fired when underlying TreeListView's KeyDown event is triggered</summary>
        public event EventHandler<KeyEventArgs> KeyDown;

        /// <summary>
        /// Event fired when underlying TreeListView's KeyPress event is triggered</summary>
        public event EventHandler<KeyPressEventArgs> KeyPress;

        /// <summary>
        /// Event fired when underlying TreeListView's KeyUp event is triggered</summary>
        public event EventHandler<KeyEventArgs> KeyUp;

        /// <summary>
        /// Event fired when underlying TreelistView is clicked</summary>
        public event EventHandler<MouseEventArgs> MouseClick;

        /// <summary>
        /// Event fired when underlying TreelistView is double clicked</summary>
        public event EventHandler<MouseEventArgs> MouseDoubleClick;

        /// <summary>
        /// Event fired when mouse down event is triggered on the underlying TreelistView</summary>
        public event EventHandler<MouseEventArgs> MouseDown;

        /// <summary>
        /// Event fired when mouse up event is triggered on the underlying TreelistView</summary>
        public event EventHandler<MouseEventArgs> MouseUp;

        /// <summary>
        /// Gets or sets the command service to use</summary>
        [Import(AllowDefault = true)]
        public ICommandService CommandService { get; set; }

        /// <summary>
        /// Gets or sets the status service to use</summary>
        [Import(AllowDefault = true)]
        public IStatusService StatusService { get; set; }

        /// <summary>
        /// Gets or sets the context menu command providers to use</summary>
        public IEnumerable<IContextMenuCommandProvider> ContextMenuCommandProviders
        {
            get
            {
                return
                    m_actualContextMenuCommandProviders ??
                    (m_actualContextMenuCommandProviders = new List<IContextMenuCommandProvider>(m_contextMenuCommandProviders.GetValues()));
            }

            set { m_actualContextMenuCommandProviders = new List<IContextMenuCommandProvider>(value); }
        }

        /// <summary>
        /// Method triggered when the underlying TreeListView's KeyDown event is fired. Raises KeyDown event.</summary>
        /// <param name="e">Key event arguments</param>
        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            KeyDown.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when the underlying TreeListView's KeyPress event is fired. Raises KeyPress event.</summary>
        /// <param name="e">Key press event arguments</param>
        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            KeyPress.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when the underlying TreeListView's KeyUp event is fired. Raises KeyUp event.</summary>
        /// <param name="e">Key event arguments</param>
        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            KeyUp.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseClick event is fired on the TreeListView. Raises MouseClick event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseClick(MouseEventArgs e)
        {
            MouseClick.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseDoubleClick event is fired on the TreeListView. Raises MouseDoubleClick event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
            MouseDoubleClick.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseDown event is fired on the TreeListView. Raises MouseDown event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseDown(MouseEventArgs e)
        {
            MouseDown.Raise(this, e);
        }

        /// <summary>
        /// Method triggered when MouseUp event is fired on the TreeListView. Raises MouseUp event.</summary>
        /// <param name="e">Mouse event arguments</param>
        protected virtual void OnMouseUp(MouseEventArgs e)
        {
            MouseUp.Raise(this, e);

            if (e.Button != MouseButtons.Right)
                return;

            if (CommandService == null)
                return;

            if (ContextMenuCommandProviders == null)
                return;

            IEnumerable<object> commands =
                ContextMenuCommandProviders.GetCommands(
                    View,
                    m_treeListViewAdapter.LastHit);

            Point screenPoint = TreeListView.Control.PointToScreen(new Point(e.X, e.Y));
            CommandService.RunContextMenu(commands, screenPoint);
        }

        /// <summary>
        /// Method triggered when DragOver event is fired on the TreeListView</summary>
        /// <param name="e">Drag-and-drop event arguments</param>
        protected virtual void OnDragOver(DragEventArgs e)
        {
            bool canInsert = false;

            try
            {
                canInsert = ApplicationUtil.CanInsert(View, LastHit, e.Data);
            }
            finally
            {
                e.Effect = canInsert ? DragDropEffects.Move : DragDropEffects.None;
            }
        }

        /// <summary>
        /// Method triggered when DragDrop event is fired on the TreeListView</summary>
        /// <param name="e">Drag-and-drop event arguments</param>
        protected virtual void OnDragDrop(DragEventArgs e)
        {
            ApplicationUtil.Insert(View, LastHit, e.Data, "Drag and drop", StatusService);
        }

        /// <summary>
        /// Method trigger when NodeDrag event is fired on the TreeListView</summary>
        /// <param name="e">Node drag event arguments</param>
        protected virtual void OnNodeDrag(TreeListView.NodeDragEventArgs e)
        {
            TreeListView.DoDragDrop(e.Node.Tag, DragDropEffects.All);
        }

        private void ControlKeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        private void ControlKeyPress(object sender, KeyPressEventArgs e)
        {
            OnKeyPress(e);
        }

        private void ControlKeyUp(object sender, KeyEventArgs e)
        {
            OnKeyUp(e);
        }

        private void ControlMouseClick(object sender, MouseEventArgs e)
        {
            OnMouseClick(e);
        }

        private void ControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            OnMouseDoubleClick(e);
        }

        private void ControlMouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        private void ControlMouseUp(object sender, MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        private void TreeListViewDragOver(object sender, DragEventArgs e)
        {
            OnDragOver(e);
        }

        private void TreeListViewDragDrop(object sender, DragEventArgs e)
        {
            OnDragDrop(e);
        }

        private void TreeListViewNodeDrag(object sender, TreeListView.NodeDragEventArgs e)
        {
            OnNodeDrag(e);
        }

        private readonly TreeListView m_treeListView;
        private readonly TreeListViewAdapter m_treeListViewAdapter;

        [ImportMany]
        private IEnumerable<Lazy<IContextMenuCommandProvider>> m_contextMenuCommandProviders;
        private List<IContextMenuCommandProvider> m_actualContextMenuCommandProviders;
    }
}