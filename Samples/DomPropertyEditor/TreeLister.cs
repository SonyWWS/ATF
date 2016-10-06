//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Dom;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;



namespace DomPropertyEditorSample
{
    /// <summary>
    /// Tree view of objects.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(TreeLister))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class TreeLister :  TreeControlEditor, IInitializable
    {

        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        [ImportingConstructor]
        public TreeLister(ICommandService commandService)
            : base(commandService)
        {
            Configure(out m_controlInfo);
           // m_commandService = commandService;

            //m_contextMenuStrip = new ContextMenuStrip();
            //m_contextMenuStrip.AutoClose = true;

            //m_addBookmark = new ToolStripMenuItem(m_addBookMark);
            //m_addBookmark.Click += (sender, e) => AddBookmark();
            //m_addBookmark.ToolTipText = "Adds a new bookmark".Localize();

            //m_deleteBookmark = new ToolStripMenuItem("Delete".Localize());
            //m_deleteBookmark.Click += (sender, e) => Delete();
            //m_deleteBookmark.ShortcutKeys = Keys.Delete;
            //m_deleteBookmark.ShortcutKeyDisplayString = KeysUtil.KeysToString(Keys.Delete, true);
            //m_deleteBookmark.Image = ResourceUtil.GetImage16(CommandInfo.EditDelete.ImageName);

            //m_contextMenuStrip.Items.Add(m_addBookmark);
            //m_contextMenuStrip.Items.Add(m_deleteBookmark);

        }

        /// <summary>
        /// Configures the BookmarkLister</summary>
        /// <param name="controlInfo">Information about the control for the hosting service</param>
        protected virtual void Configure(
            out ControlInfo controlInfo)
        {
            
            controlInfo = new ControlInfo(
                "GameObjects".Localize(),
                "Game objects".Localize(),
                StandardControlGroup.CenterPermanent);

            TreeControl.ShowRoot = false;
            TreeControl.AllowDrop = false;
            TreeControl.SelectionMode = SelectionMode.MultiExtended;
           // TreeControl.MouseDown += TreeControl_MouseDown;
            
        }

        //protected override void OnMouseUp(MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right)
        //    {
        //        var instancingContext = TreeView.As<IInstancingContext>();
        //        m_deleteBookmark.Enabled = instancingContext.CanDelete();
        //        m_deleteBookmark.Visible = instancingContext.CanDelete();
        //        SkinService.ApplyActiveSkin(m_contextMenuStrip);
        //        m_contextMenuStrip.Show(TreeControl, e.X, e.Y);
        //    }
        //}
        
        
        #region IInitializable Members

        void IInitializable.Initialize()
        {
            m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
            m_controlHostService.RegisterControl(TreeControl, m_controlInfo,null);
            TreeControl.Text = "Objects".Localize();

            //m_commandService.ProcessingKey += (sender, e) =>
            //{
            //    if (e.KeyData == Sce.Atf.Input.Keys.Delete
            //        && m_controlRegistry.ActiveControl == ControlInfo)
            //    {
            //        e.Handled = Delete();
            //    }
            //};
        }

        #endregion
        
        private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {            
            var treeview = m_contextRegistry.GetActiveContext<ITreeView>();                        
            if (TreeView != treeview)
                TreeView = treeview;            
        }

        //private void TreeControl_MouseDown(object sender, MouseEventArgs e)
        //{
        //    Bookmark bkm = LastHit.As<Bookmark>();
        //    if (e.Button == MouseButtons.Left && bkm != null)
        //    {
        //        m_designView.ActiveView.Camera.Init(bkm.Camera);
        //    }
        //}

        //private void AddBookmark()
        //{
        //    IList<Bookmark> bookmarkList = null;
        //    Bookmark parentBookmark = LastHit.As<Bookmark>();
        //    if (parentBookmark != null)
        //    {
        //        bookmarkList = parentBookmark.Bookmarks;
        //    }
        //    else
        //    {
        //        var bookmarkContext = TreeView.As<BookmarkingContext>();
        //        if (bookmarkContext != null)
        //            bookmarkList = bookmarkContext.Bookmarks;                
        //    }

        //    if (bookmarkList != null)
        //    {
        //        Bookmark newBookmark = new DomNode(Schema.bookmarkType.Type).As<Bookmark>();
        //        newBookmark.Name = "New bookmark".Localize();
        //        newBookmark.Camera = m_designView.ActiveView.Camera;

        //        ITransactionContext transactionContext = TreeView.As<ITransactionContext>();
        //        TransactionContexts.DoTransaction(
        //            transactionContext,
        //            delegate
        //            {
        //                bookmarkList.Add(newBookmark);
        //            },
        //            m_addBookMark);
        //    }
        //}

        ///// <summary>
        ///// Delete selected objects</summary>
        //private bool Delete()
        //{
        //    var instancingContext = TreeView.As<IInstancingContext>();
        //    if (instancingContext.CanDelete())
        //    {
        //        var transactionContext = TreeView.As<ITransactionContext>();
        //        transactionContext.DoTransaction(
        //                delegate
        //                {
        //                    instancingContext.Delete();
        //                    ISelectionContext selectionContext = TreeView.As<ISelectionContext>();
        //                    if (selectionContext != null)
        //                        selectionContext.Clear();
        //                },
        //                m_deleteBookmark.Text);
        //        return true;
        //    }
        //    return false;
        //}


        //[Import(AllowDefault = false)]
        //private IControlRegistry m_controlRegistry;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService = null;

        [Import(AllowDefault = false)]
        private IContextRegistry m_contextRegistry = null;

        private readonly ControlInfo m_controlInfo;
    }
}
