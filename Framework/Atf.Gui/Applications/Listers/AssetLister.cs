//Sony Computer Entertainment Confidential

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class to list assets in a tree control
    /// </summary>
    [Export(typeof(AssetLister))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AssetLister : TreeControlEditor, IControlHostClient, ICommandClient, IInitializable, IPartImportsSatisfiedNotification
    {
        [ImportingConstructor]
        public AssetLister(
            IControlHostService controlHostService,
            IContextService contextService,
            ICommandService commandService,
            IFileDialogService fileDialogService)
        {
            m_controlHostService = controlHostService;
            m_contextService = contextService;
            m_commandService = commandService;
            m_fileDialogService = fileDialogService;

            m_treeControl = new TreeControl();
            m_treeControl.Dock = DockStyle.Fill;
            m_treeControl.AllowDrop = true;
            m_treeControl.SelectionMode = SelectionMode.MultiExtended;
            m_treeControl.ImageList = ResourceUtil.GetImageList16();
            m_treeControl.StateImageList = ResourceUtil.GetImageList16();

            m_treeControl.DragOver += new DragEventHandler(treeControl_DragOver);
            m_treeControl.DragDrop += new DragEventHandler(treeControl_DragDrop);
            m_treeControl.MouseDown += new MouseEventHandler(treeControl_MouseDown);
            m_treeControl.MouseUp += new MouseEventHandler(treeControl_MouseUp);
            m_treeControl.NodeLabelEdited += new EventHandler<TreeControl.NodeEventArgs>(treeControl_NodeLabelEdited);

            m_listView = new ListView();
            m_listView.View = View.Details;
            m_listView.Dock = DockStyle.Fill;
            m_listView.AllowDrop = true;
            m_listView.LabelEdit = false;

            m_listView.MouseDown += new MouseEventHandler(thumbnailControl_MouseDown);
            m_listView.MouseUp += new MouseEventHandler(thumbnailControl_MouseUp);
            m_listView.MouseMove += new MouseEventHandler(thumbnailControl_MouseMove);
            m_listView.MouseLeave += new EventHandler(thumbnailControl_MouseLeave);
            m_listView.DoubleClick += new EventHandler(thumbnailControl_DoubleClick);
            m_listView.DragOver += new DragEventHandler(thumbnailControl_DragOver);
            m_listView.DragDrop += new DragEventHandler(thumbnailControl_DragDrop);

            m_thumbnailControl = new ThumbnailControl();
            m_thumbnailControl.IndicatorImageList = ResourceUtil.GetImageList16();
            m_thumbnailControl.Dock = DockStyle.Fill;
            m_thumbnailControl.AllowDrop = true;
            m_thumbnailControl.BackColor = SystemColors.Window;
            m_thumbnailControl.SelectionChanged += new EventHandler(thumbnailControl_SelectionChanged);
            m_thumbnailControl.MouseDown += new MouseEventHandler(thumbnailControl_MouseDown);
            m_thumbnailControl.MouseMove += new MouseEventHandler(thumbnailControl_MouseMove);
            m_thumbnailControl.MouseUp += new MouseEventHandler(thumbnailControl_MouseUp);
            m_thumbnailControl.MouseLeave += new EventHandler(thumbnailControl_MouseLeave);
            m_thumbnailControl.DoubleClick += new EventHandler(thumbnailControl_DoubleClick);
            m_thumbnailControl.DragOver += new DragEventHandler(thumbnailControl_DragOver);
            m_thumbnailControl.DragDrop += new DragEventHandler(thumbnailControl_DragDrop);

            m_splitContainer = new SplitContainer();
            m_splitContainer.Name = "Asset";
            m_splitContainer.Orientation = Orientation.Vertical;
            m_splitContainer.Panel1.Controls.Add(m_treeControl);
            m_splitContainer.Panel2.Controls.Add(m_thumbnailControl);
            m_splitContainer.Panel2.Controls.Add(m_listView);

            m_listView.Hide();

            m_selection.Changed += new EventHandler(selection_Changed);

            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            m_assetFolderTreeView = new AssetFolderTreeView(this);
            m_treeControlAdapter.TreeView = m_assetFolderTreeView;

            m_listViewAdaptor = new ListViewAdaptor(m_listView);
            m_assetItemListView = new AssetItemListView(this);
        }

        private void Contexts_ActiveItemChanged(object sender, EventArgs e)
        {
        }

        private IControlHostService m_controlHostService;
        private IContextService m_contextService;
        private ICommandService m_commandService;
        private IFileDialogService m_fileDialogService;

        [Import(AllowDefault = true, AllowRecomposition = true)]
        public ISourceControlService m_sourceControlService = null;

        [Import(AllowDefault = true)]
        private ThumbnailService m_thumbnailService = null;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService = null;

        [Import(AllowDefault = true)]
        private IStatusService m_statusService = null;

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<IContextMenuCommandProvider> m_contextMenuCommandProviders = null;

        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<IDataObjectConverter> m_dataObjectConverters = null;

        #region IInitializable Members

        /// <summary>
        /// Initialize instance
        /// </summary>
        void IInitializable.Initialize()
        {
            m_contextService.Contexts.ActiveItemChanged += new EventHandler(Contexts_ActiveItemChanged);

            m_thumbnailService.ThumbnailReady += new EventHandler<ThumbnailReadyEventArgs>(ThumbnailManager_ThumbnailReady);

            m_controlHostService.RegisterControl(m_splitContainer, "Assets", "Views asset list", m_controlGroup, this);

            RegisterCommands(m_commandService);
            RegisterSettings();
        }

        #endregion

        #region IPartImportsSatisfiedNotification Members

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            // TODO unhook using old reference
            if (m_sourceControlService != null)
            {
                m_sourceControlService.StatusChanged +=
                    new EventHandler<SourceControlEventArgs>(sourceControl_StatusChanged);
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the asset folder
        /// </summary>
        public IAssetFolder AssetFolder
        {
            get { return m_rootAssetFolder; }
            set
            {
                if (m_rootAssetFolder != value)
                {
                    m_rootAssetFolder = value;
                    TreeView = new AssetFolderTreeView(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the control group docking location; set this before initialization
        /// </summary>
        public StandardControlGroup ControlGroup
        {
            get { return m_controlGroup; }
            set { m_controlGroup = value; }
        }

        /// <summary>
        /// Gets the tree control displaying the asset folder hierarchy</summary>
        public TreeControl TreeControl
        {
            get { return m_treeControl; }
        }

        /// <summary>
        /// Gets the ListView displaying the details view of assets in the selected folder
        /// </summary>
        public ListView ListView
        {
            get { return m_listView; }
        }

        /// <summary>
        /// Gets the ThumbnailControl displaying the thumbnail view of assets in the selected folder
        /// </summary>
        public ThumbnailControl ThumbnailControl
        {
            get { return m_thumbnailControl; }
        }

        #region IControlHostClient Members

        /// <summary>
        /// Activate control
        /// </summary>
        /// <param name="control">Control</param>
        public void Activate(Control control)
        {
        }

        /// <summary>
        /// Close control
        /// </summary>
        /// <param name="control">Control</param>
        /// <returns></returns>
        public bool Close(Control control)
        {
            return true;
        }

        #endregion

        /// <summary>
        /// Refreshes the tree control at the root</summary>
        /// <remarks>Call this method if the asset folder has been reloaded or synched from
        /// version control</remarks>
        public void Refresh()
        {
            // if AssetLister has been initialized, refresh tree control at the root
            if (m_treeControl != null)
            {
                m_treeControlAdapter.Refresh(m_rootAssetFolder);
                RefreshRightPane();
            }
        }
     
        #region Event Handlers

        /// <summary>
        /// Called after selection changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void selection_Changed(object sender, EventArgs e)
        {
            Selection selection = (Selection)sender;
            List<Path<object>> newSelection = selection.LastSelected as List<Path<object>>;
            if (newSelection != null && newSelection.Count > 0)
            {
                IAssetFolder assetFolder = newSelection[0].Last as IAssetFolder;
                if (assetFolder != null)
                {
                    SetCurrentAssetFolder(assetFolder);
                }
            }
        }

        private void treeControl_MouseDown(object sender, MouseEventArgs e)
        {
            m_lastHit = m_treeControlAdapter.GetItemAt(new Point(e.X, e.Y));
        }

        private void treeControl_MouseUp(object sender, MouseEventArgs e)
        {
            Point clientPoint = new Point(e.X, e.Y);
            m_lastHit = m_treeControlAdapter.GetItemAt(clientPoint);
            OnMouseUp(e);
        }

        /// <summary>
        /// Called on mouse up event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnMouseUp(MouseEventArgs e) // for tree control on the left pane
        {
            if (e.Button == MouseButtons.Right)
            {
                List<object> commands = ApplicationUtil.GetPopupCommandTags(m_lastHit, m_contextMenuCommandProviders);
                Point screenPoint = m_treeControl.PointToScreen(new Point(e.X, e.Y));
                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

        private void treeControl_DragOver(object sender, DragEventArgs e)
        {
            Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
            m_lastHit = m_treeControlAdapter.GetItemAt(clientPoint);

            OnDragOver(e);
        }

        /// <summary>
        /// Called on dragging mouse over event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDragOver(DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            IInstancingContext instancingContext = m_rootAssetFolder.As<IInstancingContext>();
            IEnumerable converted = ApplicationUtil.Convert(e.Data, instancingContext, m_dataObjectConverters);
            if (converted != null)
                e.Effect = DragDropEffects.Move;
        }

        private void treeControl_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
            m_lastHit = m_treeControlAdapter.GetItemAt(clientPoint);

            OnDragDrop(e);
        }

        /// <summary>
        /// Called on drag and drop event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnDragDrop(DragEventArgs e)
        {
            IInstancingContext instancingContext = m_rootAssetFolder.As<IInstancingContext>();
            IEnumerable<object> converted = ApplicationUtil.Convert(e.Data, instancingContext, m_dataObjectConverters);
            ApplicationUtil.Drop(converted, instancingContext, m_statusService);
        }

        private void treeControl_NodeLabelEdited(object sender, TreeControl.NodeEventArgs e)
        {
            OnNodeLabelEdited(e);
        }

        /// <summary>
        /// Called on label edited event
        /// </summary>
        /// <param name="e">Event args</param>
        protected virtual void OnNodeLabelEdited(TreeControl.NodeEventArgs e)
        {
            INameable nameable = e.Node.Tag.As<INameable>();
            if (nameable != null)
            {
                ITransactionContext transactionContext = Adapters.As<ITransactionContext>(m_treeControlAdapter.TreeView);
                transactionContext.DoTransaction(
                    delegate
                    {
                        nameable.Name = e.Node.Label;
                    },
                    Localizer.Localize("Edit Label"));
            }
        }

        private void thumbnailControl_MouseDown(object sender, MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);
            if (e.Button == MouseButtons.Left)
            {
                m_hitPoint = point;
            }
            else if (e.Button == MouseButtons.Right)
            {
                IResource asset = null;
                if (m_thumbnailControl.Visible)
                    asset = SelectedAsset(m_thumbnailControl, point);
                else
                    asset = SelectedAsset(m_listView, point);

                if (asset != null)
                {
                    if (m_thumbnailControl.Visible)
                    {
                        ThumbnailControlItem item = m_thumbnailControl.PickThumbnail(point);
                        Keys modifiers = Control.ModifierKeys;
                        if ((modifiers & Keys.Control) != 0)
                            m_thumbnailControl.Selection.Toggle(item);
                        else if ((modifiers & Keys.Shift) != 0)
                            m_thumbnailControl.Selection.Add(item);
                        else
                            m_thumbnailControl.Selection.Set(item);
                        m_thumbnailControl.Invalidate();
                    }
                    IResource assetObj = asset as IResource;
                    if (assetObj != null)
                    {
                        ISelectionContext selectionContext = m_rootAssetFolder.As<ISelectionContext>();
                        selectionContext.Selection.Set(assetObj);
                    }
                }

            }
        }

        private void thumbnailControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_dragging && e.Button == MouseButtons.Left)
            {
                Size dragSize = SystemInformation.DragSize;
                if (Math.Abs(m_hitPoint.X - e.X) >= dragSize.Width ||
                    Math.Abs(m_hitPoint.Y - e.Y) >= dragSize.Height)
                {   /*ATF3
                    m_dragging = true;
                   
                    Path<DomObject> assetPath = null;
                    bool dragSelection = false;

                    List<Path<DomObject>> assetPaths = new List<Path<DomObject>>();

                    IResource asset = null;
                    if (m_thumbnailControl.Visible)
                        asset = SelectedAsset(m_thumbnailControl, new Point(e.X, e.Y));
                    else
                        asset = SelectedAsset(m_listControl, new Point(e.X, e.Y));

                    if (asset != null)
                    {
                        assetPath = new Path<DomObject>(asset.InternalObject.GetPath());
                        assetPaths.Add(assetPath);

                        // Determine if we need to multi drag
                        foreach (Path<DomObject> path in Context.Selection)
                        {
                            if (path.Last == assetPath.Last)
                            {
                                dragSelection = true;
                                break;
                            }
                        }

                        if (dragSelection)
                        {
                            foreach (Path<DomObject> path in Context.Selection)
                            {
                                if (path.Last != assetPath.Last)
                                {
                                    if (path.Last.CanCreateInterface<IResource>())
                                    {
                                        assetPaths.Add(path);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Context.Selection.Select(assetPath, Control.ModifierKeys);
                        }

                        if (m_thumbnailControl.Visible)
                            m_thumbnailControl.DoDragDrop(assetPaths.ToArray(), DragDropEffects.All | DragDropEffects.Link);
                        else
                            m_listControl.DoDragDrop(assetPaths.ToArray(), DragDropEffects.All | DragDropEffects.Link);
                     
                    }
                */
                }
            }
        }

        private void thumbnailControl_MouseUp(object sender, MouseEventArgs e)
        {
            IResource asset = null;

            if (e.Button == MouseButtons.Right)
            {
                Point screenPoint;
                Point clientPoint = new Point(e.X, e.Y);
                if (m_thumbnailControl.Visible)
                {
                    asset = SelectedAsset(m_thumbnailControl, clientPoint);
                    screenPoint = m_thumbnailControl.PointToScreen(clientPoint);
                }
                else
                {
                    asset = SelectedAsset(m_listView, clientPoint);
                    screenPoint = m_listView.PointToScreen(clientPoint);
                }                   

                if (asset != null)
                {
                    IResource assetObj = asset as IResource;
                    if (assetObj != null)
                    {
                        List<object> commands = ApplicationUtil.GetPopupCommandTags(assetObj, m_contextMenuCommandProviders);
                        commands.Add(Command.ReloadAsset);
                        commands.Add(Command.UnloadAssets);
                        commands.Add(Command.DetailsView);
                        commands.Add(Command.ThumbnailView);
                        commands.Add(Command.AddExistingAsset);
                      
                        m_commandService.RunContextMenu(commands, screenPoint);
                    }
                }
                else
                {
                    List<object> commands = new List<object>();
                    commands.Add(Command.ReloadAsset);
                    commands.Add(Command.UnloadAssets);
                    commands.Add(Command.DetailsView);
                    commands.Add(Command.ThumbnailView);
                    commands.Add(Command.AddExistingAsset);

                    m_commandService.RunContextMenu(commands, screenPoint);
                }
            }

            m_dragging = false;
        }

        private void thumbnailControl_MouseLeave(object sender, EventArgs e)
        {
            m_dragging = false;
        }

        private void thumbnailControl_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs args = e as MouseEventArgs;
            IResource asset = null;
            if (m_thumbnailControl.Visible)
                asset = SelectedAsset(m_thumbnailControl, new Point(args.X, args.Y));
            else
                asset = SelectedAsset(m_listView, new Point(0, 0));
            m_assetDoubleClicked = asset;
            if (m_modal)
            {
                // close the form dialog when double clicked on
                Control control = m_listView;
                if (m_thumbnailControl.Visible)
                    control = m_thumbnailControl;
                if (control != null && control.Parent != null &&
                    control.Parent.Parent != null && control.Parent.Parent.Parent != null)
                {
                    Form dialog = control.Parent.Parent.Parent as Form;
                    if (dialog != null)
                        dialog.DialogResult = DialogResult.OK;
                }

                return;
            }
            /*ATF3 if (asset != null)
            {
                Context.Selection.Select(
                    new Path<DomObject>(asset.InternalObject.GetPath()),
                    Control.ModifierKeys);

                foreach (IDomDocumentEditor editor in Plugins.GetPlugins<IDomDocumentEditor>())
                {
                    if (asset.Source != null)
                    {
                        IDomDocument document = editor.OpenDocument(asset.Source.Collection, false);
                        if (document != null)
                            break;
                    }
                }
            }*/
        }


        private void thumbnailControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            m_lastHit = m_currentAssetFolder;
            IInstancingContext instancingContext = m_rootAssetFolder.As<IInstancingContext>();
            IEnumerable converted = ApplicationUtil.Convert(e.Data, instancingContext, m_dataObjectConverters);
            if (converted != null)
                e.Effect = DragDropEffects.Move;
        }

        private void thumbnailControl_DragDrop(object sender, DragEventArgs e)
        {
            m_lastHit = m_currentAssetFolder;
            IInstancingContext instancingContext = m_rootAssetFolder.As<IInstancingContext>();
            IEnumerable<object> converted = ApplicationUtil.Convert(e.Data, instancingContext, m_dataObjectConverters);
            ApplicationUtil.Drop(converted, instancingContext, m_statusService);
        }

        private IResource SelectedAsset(Control control, Point point)
        {
            IResource asset = null;
            ThumbnailControl thumbnailControl = control as ThumbnailControl;
            if (thumbnailControl != null)
            {
                ThumbnailControlItem item = thumbnailControl.PickThumbnail(point);
                if (item != null)
                {
                    asset = item.Tag as IResource;
                }
            }
            else
            {
                ListView fileListView = control as ListView;
                if ((fileListView != null) && (fileListView.SelectedItems.Count > 0))
                {
                    ListViewItem listItem = fileListView.SelectedItems[0];
                    asset = listItem.Tag as IResource;
                }
            }

            return asset;

        }

        private void thumbnailControl_SelectionChanged(object sender, EventArgs e)
        {
            /*ATF3 if (!m_selecting && Context != null)
            {
                try
                {
                    m_selecting = true;

                    List<Path<DomObject>> newSelection = new List<Path<DomObject>>();
                    foreach (ThumbnailControlItem item in m_thumbnailControl.Selection)
                    {
                        IResource asset = item.Tag as IResource;
                        if (asset != null)
                            newSelection.Add(new Path<DomObject>(asset.InternalObject.GetPath()));
                    }

                    Context.Selection.Set(newSelection);
                }
                finally
                {
                    m_selecting = false;
                }
            }*/
        }

        private void ThumbnailManager_ThumbnailReady(object sender, ThumbnailReadyEventArgs e)
        {
            // get rid of temporary thumbnail
            IResource asset = e.Resource;

            if (m_requestedThumbs.Contains(asset))
            {
                // Make sure that asset is still valid
                //DomCollection assetCollection = asset.InternalObject.Collection;
                //if (assetCollection != null &&
                //    assetCollection.Repository != null)
                {
                    ThumbnailControlItem item = GetItem(asset);
                    if (item == null)
                    {
                        item = NewItem(asset, e.Image);
                        m_thumbnailControl.Items.Add(item);
                    }
                    else
                        item.Image = e.Image;
                }

                m_requestedThumbs.Remove(asset);
            }
        }

        private void sourceControl_StatusChanged(object sender, SourceControlEventArgs e)
        {
            // Find the item
            foreach (ThumbnailControlItem item in m_thumbnailControl.Items)
            {
                IResource resource = item.Tag as IResource;
                if (resource != null && resource.Uri == e.Uri)
                {
                    item.Indicator = SetSourceControlIndicator(resource.Uri, e.Status);
                    RefreshThumbnail(resource);
                    break;
                }
            }

            foreach (ListViewItem item in m_listView.Items)
            {
                IResource resource = item.Tag as IResource;
                if (resource != null && resource.Uri == e.Uri)
                {
                    //item.Indicator = SetSourceControlIndicator(src.FilePath, e.Status);
                    break;
                }
            }

            //if (m_listControl.Visible)
            //    m_listViewAdaptor.Refresh();
        }

        #endregion

        #region ICommandClient

        /// <summary>
        /// Checks whether the client can do the command</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (m_rootAssetFolder == null)// in case there is no currently opened document
                return false;
            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    /*ATF3case Command.ReloadAsset:
                        {
                            foreach (Path<DomObject> path in LogicalSelection)
                            {
                                IResource asset = path.Last.CreateInterface<IResource>();
                                if (asset != null)
                                {
                                    return true;
                                }
                            }
                            return false;
                        }
                    */
                    case Command.UnloadAssets:
                        return true;

                    case Command.DetailsView:
                        if (m_listView.Visible)
                            return false;
                        else return true;

                    case Command.ThumbnailView:
                        if (m_thumbnailControl.Visible)
                            return false;
                        else return true;

                    case Command.AddExistingAsset:
                        return m_currentAssetFolder != null;

                    default:
                        return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    /*ATF3 case Command.ReloadAsset:
                        {
                            foreach (Path<DomObject> path in LogicalSelection)
                            {
                                IResource asset = path.Last.CreateInterface<IResource>();
                                if (asset != null)
                                {
                                    ReloadAsset(asset);
                                }
                            }
                        }
                        break;
                    */
                    case Command.UnloadAssets:
                        {
                            //UnloadUnusedAssets(m_rootAssetFolder);
                        }
                        break;

                    case Command.DetailsView:
                        {
                            m_thumbnailControl.Hide();
                            m_listViewAdaptor.ListView = m_assetItemListView;
                            m_listView.Show();
                            RefreshRightPane();
                        }
                        break;

                    case Command.ThumbnailView:
                        {
                            m_listView.Hide();
                            m_listViewAdaptor.ListView = null;
                            m_thumbnailControl.Show();
                            RefreshRightPane();
                        }
                        break;

                    case Command.AddExistingAsset:
                        AddExistingAsset();
                        break;

                }
            }
        }

        /// <summary>
        /// Updates command state for given command
        /// </summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
            //if (commandTag is Command)
            //{
            //    switch ((Command)commandTag)
            //    {
            //        case Command.ReloadAsset:
            //            break;
            //    }
            //}
        }

        #endregion

        private void AddExistingAsset()
        {
            string path = null;
            DialogResult result = m_fileDialogService.OpenFileName(ref path, Localizer.Localize("All Files") + "  (*.*)|*.*");
            if (result == DialogResult.OK)
            {
                IAssetFolder parent = m_currentAssetFolder;
                IResource asset = parent.CreateAsset();
                asset.Uri = new Uri(path);
                //OnObjectInserted(new ItemInsertedEventArgs<object>(parent.Assets.Count - 1, asset, parent));

            }
        }

      

        private void UnloadAllAssets(IAssetFolder folder)
        {
            /*foreach (IResource asset in folder.Assets)
                asset.Unload();

            foreach (IAssetFolder subFolder in folder.Folders)
                UnloadAllAssets(subFolder);*/
        }

        private void SetCurrentAssetFolder(IAssetFolder assetFolder)
        {
            if (m_currentAssetFolder != assetFolder)
            {
                m_currentAssetFolder = assetFolder; 

                m_requestedThumbs.Clear();
                RefreshRightPane();
            }
        }

        internal IAssetFolder CurrentAssetFolder
        {
            get { return m_currentAssetFolder; }
        }

       
        private void RefreshRightPane()
        {
            if (m_currentAssetFolder != null)
            {
                m_thumbnailControl.Items.Clear();

                // It's much more efficient to call the source control service with many uris at once.
                // Doing this will cache the results for the calls to RightPaneAddItem().
                if (m_sourceControlService != null)
                {
                    bool folderProcessed = false;

                    if (m_currentAssetFolder.Assets.Count > 0)
                    {
                        IResource asset = m_currentAssetFolder.Assets[0];
                        folderProcessed = m_sourceControlService.GetFolderStatus(asset.Uri);
                    }
                    if (!folderProcessed)
                    {
                        List<Uri> uris = new List<Uri>();
                        foreach (IResource resource in m_currentAssetFolder.Assets)
                        {
                            uris.Add(resource.Uri);
                        }
                        m_sourceControlService.GetStatus(uris);
                    }
                }

                foreach (IResource resource in m_currentAssetFolder.Assets)
                {
                    RightPaneAddItem(resource);
                }

                if (m_listView.Visible)
                    m_listViewAdaptor.Load();
 
            }
        }

        private void ReloadAsset(IResource resource)
        {
            /*ATF3 DomUri src = asset.Uri;
            if (src.ResolvedObject != null)
            {
                DomCollection collection = src.ResolvedObject.Collection;
                collection.Reload();
            }*/
        }

        private void RightPaneAddItem(IResource resource)
        {
            if (m_thumbnailControl.Visible)
            {
                m_requestedThumbs.Add(resource);
                m_thumbnailService.ResolveThumbnail(resource);
                string assetPath = resource.GetPathName();
                string assetFileName = Path.GetFileName(assetPath);
                Icon shellIcon = FileIconUtil.GetFileIcon(assetFileName, FileIconUtil.IconSize.Large, false);
                Bitmap tempThumbnail = shellIcon.ToBitmap();

                ThumbnailControlItem item = GetItem(resource);
                if (item == null)
                {
                    item = NewItem(resource, tempThumbnail);
                    m_thumbnailControl.Items.Add(item);
                }
                else
                {
                    item.Image = tempThumbnail;
                }
            }
            else if (m_listView.Visible)
            {
                string assetPath = resource.Uri.OriginalString;
                string indicator = null;
                if (m_sourceControlService != null)
                {
                    SourceControlStatus status = m_sourceControlService.GetStatus(resource.Uri);
                    indicator = SetSourceControlIndicator(resource.Uri, status);
                }
                string path = resource.GetPathName();
            }
        }

        private void RefreshThumbnail(IResource asset)
        {
            m_requestedThumbs.Add(asset);
            m_thumbnailService.ResolveThumbnail(asset);
            string assetPath = asset.GetPathName();
            string assetFileName = Path.GetFileName(assetPath);
            Icon shellIcon = FileIconUtil.GetFileIcon(assetFileName, FileIconUtil.IconSize.Large, false);
            Bitmap tempThumbnail = shellIcon.ToBitmap();
            ThumbnailControlItem item = GetItem(asset);
            if (item == null)
            {
                item = NewItem(asset, tempThumbnail);
                m_thumbnailControl.Items.Add(item);
            }
            else
            {
                item.Image = tempThumbnail;
            }
        }

        private ThumbnailControlItem GetItem(IResource asset)
        {
            foreach (ThumbnailControlItem item in m_thumbnailControl.Items)
            {
                IResource assetTag = item.Tag as IResource;
                if (assetTag == asset)
                    return item;
            }

            return null;
        }

        private ThumbnailControlItem NewItem(IResource resource, Image image)
        {
            ThumbnailControlItem item = new ThumbnailControlItem(image);
            item.Tag = resource;

            item.Name = Path.GetFileName(resource.GetPathName());
            item.Description = resource.GetPathName();

            // Set source control status
            if (m_sourceControlService != null)
            {
                SourceControlStatus status = m_sourceControlService.GetStatus(resource.Uri);
                item.Indicator = SetSourceControlIndicator(resource.Uri, status);
            }

            return item;
        }

        private string SetSourceControlIndicator(Uri uri, SourceControlStatus status)
        {
            string indicator = null;
            switch (status)
            {
                case SourceControlStatus.Added:
                    indicator = Resources.DocumentAddImage;
                    break;

                case SourceControlStatus.CheckedIn:
                    {
                        if (m_sourceControlService.IsSynched(uri))
                            indicator = Resources.DocumentLockImage;
                        else
                            indicator = Resources.DocumentWarningImage;
                    }
                    break;

                case SourceControlStatus.CheckedOut:
                    indicator = Resources.DocumentCheckOutImage;
                    break;

                case SourceControlStatus.NotControlled:
                    indicator = null;
                    break;
            }
            return indicator;
        }

        private enum Command
        {
            ReloadAsset,
            UnloadAssets,
            DetailsView,
            ThumbnailView,
            AddExistingAsset,
        }

        private void RegisterCommands(ICommandService commandService)
        {
            commandService.RegisterCommand(
                Command.ReloadAsset,
                StandardMenu.File,
                StandardCommandGroup.FileOther,
                Localizer.Localize("Assets/Reload Asset"),
                Localizer.Localize("Reload Asset"),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            commandService.RegisterCommand(
                Command.UnloadAssets,
                StandardMenu.File,
                StandardCommandGroup.FileOther,
                Localizer.Localize("Assets/Unload Unused Assets"),
                Localizer.Localize("Unload unused Assets"),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            commandService.RegisterCommand(
                  Command.DetailsView,
                  null,//StandardMenu.View,
                  StandardCommandGroup.ViewControls,
                  Localizer.Localize("Details View", "Show detailed view of assets"),
                  Localizer.Localize("Show detailed view of assets"),
                  Keys.None,
                  null,
                  CommandVisibility.Menu,
                  this);

            commandService.RegisterCommand(
                Command.ThumbnailView,
                null,//StandardMenu.View,
                StandardCommandGroup.ViewControls,
                Localizer.Localize("Thumbnail View"),
                Localizer.Localize("Show thumbnail view of assets"),
                Keys.None,
                null,
                CommandVisibility.Menu,
                this);

            commandService.RegisterCommand(
               Command.AddExistingAsset,
               StandardMenu.Edit,
               StandardCommandGroup.EditOther,
               Localizer.Localize("Add/Existing Asset"),
               Localizer.Localize("Add an existing asset"),
               Keys.None,
               Resources.Asset,
               CommandVisibility.All,
               this);
        }

        /// <summary>
        /// Gets or sets the root asset folder
        /// </summary>
        public IAssetFolder RootAssetFolder
        {
            get { return m_rootAssetFolder; }
            set { m_rootAssetFolder = value; }
        }

        private class AssetFolderTreeView : ITreeView, IItemView
        {
            public AssetFolderTreeView(AssetLister assetLister)       
            {
                m_assetLister = assetLister;
            }

            #region ITreeView Members

            /// Gets the root object of the tree</summary>
            public object Root
            {
                get { return m_assetLister.RootAssetFolder; }
            }

            /// <summary>
            /// Gets an enumeration of the children of the given parent object</summary>
            /// <param name="parent">Parent object</param>
            /// <returns>Enumeration of children of the parent object</returns>
            public IEnumerable<object> GetChildren(object parent)
            {
                IAssetFolder folder = Adapters.As<IAssetFolder>(parent);
                foreach (IAssetFolder childfolder in folder.Folders)
                    yield return childfolder;
            }

            #endregion

            #region IItemView Members

            /// <summary>
            /// Fills in or modifies the given display info for the item
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="info">Display info to update</param>
            public void GetInfo(object item, ItemInfo info)
            {
                IAssetFolder assetFolder = item as IAssetFolder;

                if (assetFolder != null)
                {
                    string label = assetFolder.Name;
                    info.Label = label;

                    info.AllowLabelEdit = false;
                    //info.ImageIndex = info.ImageList.Images.IndexOfKey(Sce.Atf.Resources.DataImage);
                }
            }

            #endregion

            AssetLister m_assetLister;
        }

        private class AssetItemListView : IListView, IItemView
        {
            public AssetItemListView(AssetLister assetLister)       
            {
                m_assetLister = assetLister;
            }

            #region IListView members

            /// <summary>
            /// Gets names for table columns</summary>
            public string[] ColumnNames
            {
                get { return s_columnNames; }
            }

            /// <summary>
            /// Gets enumeration for the items in the list</summary>
            public IEnumerable<object> Items
            {
                get
                {
                    if (m_assetLister.CurrentAssetFolder != null)
                    {
                        foreach (IResource asset in m_assetLister.CurrentAssetFolder.Assets)
                            yield return asset;
                    }

                }
            }

            #endregion // IListView members

            #region IItemView Members

            /// <summary>
            /// Fills in or modifies the given display info for the item
            /// </summary>
            /// <param name="item">Item</param>
            /// <param name="info">Display info to update</param>
            public void GetInfo(object item, ItemInfo info)
            {
                IResource asset = item as IResource;
                if (asset != null)
                {
                    FileInfo fileInfo = new FileInfo(asset.Uri.LocalPath);
                    info.Label = fileInfo.Name;
                    info.ImageIndex = 0; //info.ImageList.Images.IndexOfKey(item.Indicator);
                    info.AllowLabelEdit = false;

                    Shell32.SHFILEINFO shfi = new Shell32.SHFILEINFO();
                    uint flags = Shell32.SHGFI_TYPENAME | Shell32.SHGFI_USEFILEATTRIBUTES;
                    Shell32.SHGetFileInfo(fileInfo.FullName,
                        Shell32.FILE_ATTRIBUTE_NORMAL,
                        ref shfi,
                        (uint)System.Runtime.InteropServices.Marshal.SizeOf(shfi),
                        flags);

                    string typeName = shfi.szTypeName;
                    long length;
                    DateTime lastWriteTime;
                    try
                    {
                        length = fileInfo.Length;
                        lastWriteTime = fileInfo.LastWriteTime;
                    }
                    catch (IOException)
                    {
                        length = 0;
                        lastWriteTime = new DateTime();
                    }

                    info.Properties = new object[] { fileInfo.Name, length, typeName, lastWriteTime };
                }
            }

            #endregion


            AssetLister m_assetLister;
        }

        /// <summary>
        /// Gets or sets the currently selected asset list view mode. This string is
        /// persisted in the user's settings file.
        /// </summary>
        public string AssetListViewMode
        {
            get
            {
                if (m_thumbnailControl.Visible)
                    return "Thumbnail";
                else
                    return "Details";
            }

            set
            {
                // Make sure that 'value' is valid first, in case the the names have changed
                //  or the settings file is otherwise invalid.
                if ("Details" == value)
                {
                    DoCommand(Command.DetailsView);
                }
                else if ("Thumbnail" == value)
                {
                    DoCommand(Command.ThumbnailView);
                }
            }
        }

        /// <summary>
        /// Gets or sets the asset chooser dialog location</summary>
        public Point AssetDialogLocation
        {
            get { return m_assetDialogLocation; }
            set { m_assetDialogLocation = value; }
        }

        /// <summary>
        /// Gets or sets the asset chooser dialog size</summary>
        public Size AssetDialogSize
        {
            get { return m_assetDialogSize; }
            set { m_assetDialogSize = value; }
        }

        /// <summary>
        /// Gets or sets the list view settings
        /// </summary>
        public string ListViewSettings
        {
            get { return m_listViewAdaptor.Settings; }
            set { m_listViewAdaptor.Settings = value; }
        }


        /// <summary>
        /// Gets or sets the list view control bounds</summary>
        public int SplitterDistance
        {
            get { return m_splitContainer.SplitterDistance; }
            set { m_splitContainer.SplitterDistance = value; }
        }

        private void RegisterSettings()
        {
            if (m_settingsService != null)
            {
                m_settingsService.RegisterSettings(
                    this,
                    new BoundPropertyDescriptor(this, "AssetListViewMode", "AssetListViewMode", null, null),
                    new BoundPropertyDescriptor(this, "AssetDialogSize", "AssetDialogSize", null, null),
                    new BoundPropertyDescriptor(this, "AssetDialogLocation", "AssetDialogLocation", null, null),
                    new BoundPropertyDescriptor(this, "SplitterDistance", "SplitterDistance", null, null),
                    new BoundPropertyDescriptor(this, "ListViewSettings", "ListViewSettings", null, null));
            }
        }

        private static string[] s_columnNames = new string[]
        {
            "Name", "Size", "Type", "Date Modified"
        };

        private SplitContainer m_splitContainer;

        private TreeControlAdapter m_treeControlAdapter;
        private TreeControl m_treeControl;

        private ThumbnailControl m_thumbnailControl;
        private List<IResource> m_requestedThumbs = new List<IResource>();

        private ListView m_listView;
        private ListViewAdaptor  m_listViewAdaptor;
        private IAssetFolder m_rootAssetFolder;
        private IAssetFolder m_currentAssetFolder;

        private AssetFolderTreeView m_assetFolderTreeView;
        private AssetItemListView m_assetItemListView;


        private object m_lastHit;

        private Point m_hitPoint;
        private StandardControlGroup m_controlGroup = StandardControlGroup.Bottom;

        private IResource m_assetDoubleClicked;
        private Point m_assetDialogLocation;
        private Size m_assetDialogSize;

        private Selection m_selection = new Selection();

        private bool m_dragging;
        private bool m_selecting;
        private bool m_modal;
    }
}

