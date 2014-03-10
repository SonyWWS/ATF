//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Sce.Atf.Adaptation;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// GUI component for browsing and organizing resource folders and resources (e.g., models, images, etc.)</summary>
    /// <remarks>Similar to Windows Explorer, this editor contains a folder tree and a contents view area.
    /// The contents view can be switched between details and thumbnails view. Replaces ATF 2's AssetLister.</remarks>
    [Export(typeof(ResourceLister))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Any)]
    public class ResourceLister : ICommandClient, IControlHostClient, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="commandService">Command service for opening right-click context menus</param>
        /// <param name="controlHostService">Control host service for registering windows</param>
        /// <param name="settingsService">Settings service</param>
        /// <param name="thumbnailService">Thumbnail service</param>
        [ImportingConstructor]
        public ResourceLister(
            ICommandService commandService,
            IControlHostService controlHostService,
            ISettingsService settingsService,
            ThumbnailService thumbnailService)
        {
            m_commandService = commandService;
            m_controlHostService = controlHostService;
            m_settingsService = settingsService;

            m_thumbnailService = thumbnailService;
            m_thumbnailService.ThumbnailReady += ThumbnailManager_ThumbnailReady;
        }

        /// <summary>
        /// Sets the root resource folder, registers events and refreshes controls</summary>
        /// <param name="rootFolder">Resource folder</param>
        public void SetRootFolder(IResourceFolder rootFolder)
        {
            m_treeContext = new TreeViewContext(rootFolder);
            m_treeSelectionContext = m_treeContext.As<ISelectionContext>();
            m_treeSelectionContext.SelectionChanged += TreeSelectionChanged;
            m_treeControlAdapter.TreeView = m_treeContext;

            m_listContext = new ListViewContext();
            m_listSelectionContext = m_listContext.As<ISelectionContext>();
            m_listViewAdapter.ListView = m_listContext;

            m_treeControlAdapter.Refresh(rootFolder);
        }

        #region IInitializable Members

        /// <summary>
        /// Initializes the MEF component</summary>
        public void Initialize()
        {
            m_treeControl = new TreeControl();
            m_treeControl.Dock = DockStyle.Fill;
            m_treeControl.AllowDrop = true;
            m_treeControl.SelectionMode = SelectionMode.MultiExtended;
            m_treeControl.ImageList = ResourceUtil.GetImageList16();
            m_treeControl.StateImageList = ResourceUtil.GetImageList16();
            
            m_treeControl.DragOver += treeControl_DragOver;
            m_treeControl.DragDrop += treeControl_DragDrop;
            m_treeControl.MouseUp += treeControl_MouseUp;
                 
            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);

            m_listView = new ListView();
            m_listView.View = View.Details;
            m_listView.Dock = DockStyle.Fill;
            m_listView.AllowDrop = true;
            m_listView.LabelEdit = false;
            
            m_listView.MouseUp += thumbnailControl_MouseUp;
            m_listView.MouseMove += thumbnailControl_MouseMove;
            m_listView.MouseLeave += thumbnailControl_MouseLeave;
            m_listView.DragOver += thumbnailControl_DragOver;
            
            m_listViewAdapter = new ListViewAdapter(m_listView);

            m_thumbnailControl = new ThumbnailControl();
            m_thumbnailControl.Dock = DockStyle.Fill;
            m_thumbnailControl.AllowDrop = true;
            m_thumbnailControl.BackColor = SystemColors.Window;
            
            m_thumbnailControl.SelectionChanged += thumbnailControl_SelectionChanged;
            m_thumbnailControl.MouseMove += thumbnailControl_MouseMove;
            m_thumbnailControl.MouseUp += thumbnailControl_MouseUp;
            m_thumbnailControl.MouseLeave += thumbnailControl_MouseLeave;
            m_thumbnailControl.DragOver += thumbnailControl_DragOver;
            
            m_splitContainer = new SplitContainer();
            m_splitContainer.Name = "Resources".Localize();
            m_splitContainer.Orientation = Orientation.Vertical;
            m_splitContainer.Panel1.Controls.Add(m_treeControl);
            m_splitContainer.Panel2.Controls.Add(m_thumbnailControl);
            m_splitContainer.Panel2.Controls.Add(m_listView);
            m_splitContainer.SplitterDistance = 10;

            m_listView.Hide();

            Image resourceImage = ResourceUtil.GetImage16(Resources.ResourceImage);

            // on initialization, register our tree control with the hosting service
            m_controlHostService.RegisterControl(
                m_splitContainer,
                new ControlInfo(
                   "Resources".Localize(),
                   "Lists available resources".Localize(),
                   StandardControlGroup.Left,
                   resourceImage),
               this);

            RegisterCommands(m_commandService);
            RegisterSettings();
        }

        #endregion

        /// <summary>
        /// Set/get the control group docking location; set this before</summary>
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
        /// Gets the ListView displaying the details view of resources in the selected folder</summary>
        public ListView ListView
        {
            get { return m_listView; }
        }

        /// <summary>
        /// Gets the ThumbnailControl displaying the thumbnail view of resources in the selected folder</summary>
        public ThumbnailControl ThumbnailControl
        {
            get { return m_thumbnailControl; }
        }

        /// <summary>
        /// Refreshes the tree control at the root</summary>
        /// <remarks>Call this method if the asset folder has been reloaded or synched from
        /// version control.</remarks>
        public void Refresh()
        {
            // if ResourceLister has been initialized, refresh tree control at the root
            if (m_treeControl != null)
            {
                RefreshThumbnails();
            }
        }

        private void TreeSelectionChanged(object sender, EventArgs e)
        {
            ISelectionContext selectionContext = sender.As<ISelectionContext>();
            if (selectionContext != null)
            {
                IResourceFolder selectedFolder = selectionContext.LastSelected.As<IResourceFolder>();
                if (selectedFolder != null)
                {
                    m_listContext.SelectedFolder = selectedFolder;
                    m_listViewAdapter.ListView = m_listViewAdapter.ListView; // force reload
                    RefreshThumbnails();
                }
            }
        }

        private void treeControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point clientPoint = new Point(e.X, e.Y);
                List<object> commands = new List<object>();
                commands.Add(Command.DetailsView);
                commands.Add(Command.ThumbnailView);

                Point screenPoint = m_treeControl.PointToScreen(clientPoint);
                m_commandService.RunContextMenu(commands, screenPoint);
            }             
        }

        private void treeControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
            m_lastHit = m_treeControlAdapter.GetItemAt(clientPoint);

            IResourceFolder hitFolder = GetHitFolder();
            if (hitFolder != null && !hitFolder.Folders.IsReadOnly)
                e.Effect = DragDropEffects.Move;
        }

        private void treeControl_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = m_treeControl.PointToClient(new Point(e.X, e.Y));
            m_lastHit = m_treeControlAdapter.GetItemAt(clientPoint);

            string[] droppedFiles = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (droppedFiles != null)
            {
                IResourceFolder parentFolder = GetHitFolder();
                if (parentFolder != null)
                {
                    foreach (string path in droppedFiles)
                        CreateFolder(parentFolder, new Uri(path));
                }
                m_treeControlAdapter.Refresh(m_treeContext.RootFolder);
            }
        }

        // Returns the folder represented by m_lastHit, or the root folder.
        private IResourceFolder GetHitFolder()
        {
            IResourceFolder hitFolder = m_lastHit.As<IResourceFolder>();
            if (hitFolder == null)
                hitFolder = m_treeContext.RootFolder;
            return hitFolder;
        }

        private IResourceFolder CreateFolder(IResourceFolder parentFolder, Uri uri)
        {
            return AddFolder(parentFolder, new DirectoryInfo(uri.LocalPath));
        }

        private IResourceFolder AddFolder(IResourceFolder parentFolder, DirectoryInfo dirInfo)
        {
            IResourceFolder folder = parentFolder.CreateFolder();
            if (folder != null)
            {
                folder.Name = dirInfo.Name;
                if (parentFolder != null)
                {
                    IList<IResourceFolder> folders = parentFolder.Folders;
                    if (!folders.IsReadOnly)
                        folders.Add(folder);
                }

                foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
                    AddFolder(folder, subDir);

                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                {
                    if (!fileInfo.Name.StartsWith("~")) // ignore thumbnails
                    {
                        IList<Uri> uris = folder.ResourceUris;
                        if (!uris.IsReadOnly)
                        {
                            Uri uri = new Uri(fileInfo.FullName);
                            folder.ResourceUris.Add(uri);
                        }
                    }
                }
            }

            return folder;
        }

        private Control ActiveItemControl
        {
            get
            {
                if (m_thumbnailControl.Visible)
                    return m_thumbnailControl;
                return m_listView;
            }
        }

        private void thumbnailControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_dragging && e.Button == MouseButtons.Left)
            {                
                Size dragSize = SystemInformation.DragSize;
                if (Math.Abs(m_hitPoint.X - e.X) >= dragSize.Width ||
                    Math.Abs(m_hitPoint.Y - e.Y) >= dragSize.Height)
                {
                    m_dragging = true;

                    // Get a list of the paths of:
                    // the item we've clicked on with the mouse
                    // and all selected items in the current view
                    // while making sure that the clicked item is not included twice
                    List<string> paths = new List<string>();
                    Uri clickedUri = GetClickedItemUri(ActiveItemControl, new Point(e.X, e.Y));
                    if (clickedUri != null && clickedUri.IsAbsoluteUri)
                        paths.Add(clickedUri.LocalPath);
                    foreach (Uri uri in GetSelectedItemUris(ActiveItemControl))
                    {
                        if (uri != clickedUri && uri.IsAbsoluteUri)
                            paths.Add(uri.LocalPath);
                    }

                    if (paths.Count > 0)
                    {
                        // Create the DataObject and fill it with an array of paths
                        // of the clicked and/or selected objects
                        IDataObject dataObject = new DataObject();
                        dataObject.SetData(DataFormats.FileDrop, true, paths.ToArray());
                        ActiveItemControl.DoDragDrop(dataObject, DragDropEffects.Move);
                    }
                }
            }
        }

        private void thumbnailControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point clientPoint = new Point(e.X, e.Y);
                Uri resourceUri = GetClickedItemUri(ActiveItemControl, new Point(e.X, e.Y));

                if (resourceUri != null)
                {
                    List<object> commands = new List<object>(/*GetPopupCommandTags(target)*/);
                    commands.Add(Command.DetailsView);
                    commands.Add(Command.ThumbnailView);

                    Point screenPoint = m_thumbnailControl.PointToScreen(clientPoint);
                    m_commandService.RunContextMenu(commands, screenPoint);
                }
                else
                {
                    List<object> commands = new List<object>();
                    commands.Add(Command.DetailsView);
                    commands.Add(Command.ThumbnailView);

                    Point screenPoint = m_thumbnailControl.PointToScreen(clientPoint);
                    m_commandService.RunContextMenu(commands, screenPoint);
                }
            }

            m_dragging = false;
        }

        private void thumbnailControl_MouseLeave(object sender, EventArgs e)
        {
            m_dragging = false;
        }

        private void thumbnailControl_DragOver(object sender, DragEventArgs e)
        {
        }

        // Gets an enumeration of all Uri tags of items selected in the specified control
        // or an empty enumeration if no items are selected
        private IEnumerable<Uri> GetSelectedItemUris(Control control)
        {
            ThumbnailControl thumbnailControl = control as ThumbnailControl;
            if (thumbnailControl != null)
            {
                foreach (ThumbnailControlItem item in thumbnailControl.Selection)
                {
                    Uri uri = GetUriFromTag(item.Tag);
                    if (uri != null)
                        yield return uri;
                }
            }
            else
            {
                ListView fileListView = control as ListView;
                if (fileListView != null)
                {
                    foreach (ListViewItem item in fileListView.SelectedItems)
                    {
                        Uri uri = GetUriFromTag(item.Tag);
                        if (uri != null)
                            yield return uri;
                    }
                }
            }
        }

        // Returns the Uri tag of the item at the specified point in the specified control,
        // or null if none found
        private Uri GetClickedItemUri(Control control, Point point)
        {
            Uri resourceUri = null;
            ThumbnailControl thumbnailControl = control as ThumbnailControl;
            if (thumbnailControl != null)
            {
                ThumbnailControlItem item = thumbnailControl.PickThumbnail(point);
                if (item != null)
                    resourceUri = GetUriFromTag(item.Tag);
            }
            else
            {
                ListView fileListView = control as ListView;
                if ((fileListView != null) && (fileListView.SelectedItems.Count > 0))
                {
                    ListViewItem item = fileListView.SelectedItems[0];
                    if (item != null)
                        resourceUri = GetUriFromTag(item.Tag);
                }
            }

            return resourceUri;

        }

        private void thumbnailControl_SelectionChanged(object sender, EventArgs e)
        {
            if (!m_selecting)
            {
                try
                {
                    m_selecting = true;

                    List<Path<object>> newSelection = new List<Path<object>>();
                    foreach (ThumbnailControlItem item in m_thumbnailControl.Selection)
                    {
                        IResource resource = item.Tag as IResource;
                        if (resource != null)
                            newSelection.Add(new AdaptablePath<object>(resource));
                    }
                }
                finally
                {
                    m_selecting = false;
                }
            }
        }

        private void ThumbnailManager_ThumbnailReady(object sender, ThumbnailReadyEventArgs e)
        {
            // get rid of temporary thumbnail
            Uri resourceUri = e.ResourceUri;

            if (m_requestedThumbs.Contains(resourceUri))
            {
                ThumbnailControlItem item = GetItem(resourceUri);
                if (item == null)
                {
                    item = NewItem(resourceUri, e.Image);
                    m_thumbnailControl.Items.Add(item);
                }
                else
                    item.Image = e.Image;

                m_requestedThumbs.Remove(resourceUri);
            }
        }

        #region ICommandClient

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (m_treeContext == null || m_treeContext.Root == null)// in case there is no currently opened document
                return false;

            if (commandTag is Command)
            {
                switch ((Command)commandTag)
                {
                    case Command.DetailsView:
                        return !m_listView.Visible;
                    case Command.ThumbnailView:
                        return !m_thumbnailControl.Visible;
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
                    case Command.DetailsView:
                        {
                            m_thumbnailControl.Hide();
                            m_listView.Show();
                            RefreshThumbnails();
                        }
                        break;

                    case Command.ThumbnailView:
                        {
                            m_listView.Hide();
                            m_thumbnailControl.Show();
                            RefreshThumbnails();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.</remarks>
        public void Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control.</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// * This method is only called by IControlHostService if the Control was previously
        /// registered by this IControlHostClient.
        /// * If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control)
        {
            return true;
        }

        #endregion

        private void RefreshThumbnails()
        {
            if (m_listContext != null)
            {
                IResourceFolder currentAssetFolder = m_listContext.SelectedFolder;

                if (currentAssetFolder != null)
                {
                    m_thumbnailControl.Items.Clear();

                    foreach (Uri resourceUri in currentAssetFolder.ResourceUris)
                    {
                        AddThumbnail(resourceUri);
                    }
                }
            }
        }

        private void AddThumbnail(Uri resourceUri)
        {
            if (m_thumbnailControl.Visible)
            {
                m_requestedThumbs.Add(resourceUri);
                m_thumbnailService.ResolveThumbnail(resourceUri);
                string assetPath = resourceUri.LocalPath;
                string assetFileName = Path.GetFileName(assetPath);
                Icon shellIcon = FileIconUtil.GetFileIcon(assetFileName, FileIconUtil.IconSize.Large, false);
                Bitmap tempThumbnail = shellIcon.ToBitmap();

                ThumbnailControlItem item = GetItem(resourceUri);
                if (item == null)
                {
                    item = NewItem(resourceUri, tempThumbnail);
                    m_thumbnailControl.Items.Add(item);
                }
                else
                {
                    item.Image = tempThumbnail;
                }
            }
            else if (m_listView.Visible)
            {
                string assetPath = resourceUri.OriginalString;
                string path = resourceUri.LocalPath;
            }
        }

        private ThumbnailControlItem GetItem(Uri resourceUri)
        {
            foreach (ThumbnailControlItem item in m_thumbnailControl.Items)
            {
                Uri tagUri = GetUriFromTag(item.Tag);
                if (tagUri == resourceUri)
                    return item;
            }

            return null;
        }

        private ThumbnailControlItem NewItem(Uri resourceUri, Image image)
        {
            ThumbnailControlItem item = new ThumbnailControlItem(image);
            item.Tag = resourceUri;

            item.Name = Path.GetFileName(resourceUri.LocalPath);
            item.Description = resourceUri.LocalPath;

            return item;
        }

        private void RegisterCommands(ICommandService commandService)
        {
            m_commandService.RegisterCommand(
                Command.DetailsView,
                null,
                StandardCommandGroup.FileOther,
                "Details View".Localize(),
                "Switch to details view".Localize(),
                this);

            m_commandService.RegisterCommand(
                Command.ThumbnailView,
                null,
                StandardCommandGroup.FileOther,
                "Thumbnail View".Localize(),
                "Switch to thumbnail view".Localize(),
                this);
        }

        /// <summary>
        /// Gets or sets the currently selected asset list view mode. This string is
        /// persisted in the user's settings file.</summary>
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
                // Make sure that 'value' is valid first, in case the names have changed
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
        /// Gets or sets the settings string for the ListViewAdapter</summary>
        public string ListViewSettings
        {
            get { return m_listViewAdapter.Settings; }
            set { m_listViewAdapter.Settings = value; }
        }

        /// <summary>
        /// Gets or sets the ListView control bounds</summary>
        public int SplitterDistance
        {
            get { return m_splitContainer.SplitterDistance; }
            set { m_splitContainer.SplitterDistance = value; }
        }

        private void RegisterSettings()
        {
            m_settingsService.RegisterSettings(
                GetType().ToString(), //maybe make into a settable string in case multiple instances of this class are needed
                new BoundPropertyDescriptor(this, () => AssetListViewMode, "AssetListViewMode", null, null),
                new BoundPropertyDescriptor(this, () => AssetDialogSize,    "AssetDialogSize", null, null),
                new BoundPropertyDescriptor(this, () => AssetDialogLocation,  "AssetDialogLocation", null, null),
                new BoundPropertyDescriptor(this, () => SplitterDistance,    "SplitterDistance", null, null),
                new BoundPropertyDescriptor(this, () => ListViewSettings,  "ListViewSettings", null, null)
            );
        }

        private static Uri GetUriFromTag(object tag)
        {
            Uri uri = tag as Uri;
            if (uri == null)
            {
                Path<object> path = tag as Path<object>;
                if (path != null)
                    uri = path.Last as Uri;
            }
            return uri;
        }

        private enum Command
        {
            DetailsView,
            ThumbnailView,
        }

        private SplitContainer m_splitContainer;

        private TreeViewContext m_treeContext;
        private ISelectionContext m_treeSelectionContext;
        private TreeControlAdapter m_treeControlAdapter;
        private TreeControl m_treeControl;
        
        private ListViewContext m_listContext;
        private ISelectionContext m_listSelectionContext;
        private ListView m_listView;
        private ListViewAdapter m_listViewAdapter;
        private ThumbnailControl m_thumbnailControl;
        private readonly List<Uri> m_requestedThumbs = new List<Uri>();

        private object m_lastHit;
        private Point m_hitPoint;
        private Point m_assetDialogLocation;
        private Size m_assetDialogSize;
        private bool m_dragging;
        private bool m_selecting;
        private StandardControlGroup m_controlGroup = StandardControlGroup.Bottom;

        private readonly ICommandService m_commandService;
        private readonly IControlHostService m_controlHostService;
        private readonly ISettingsService m_settingsService;
        private readonly ThumbnailService m_thumbnailService;

        private class GuiSelection<T> : AdaptableSelection<T>, ISelectionContext where T : class
        {
            #region ISelectionContext Members

            IEnumerable<U> ISelectionContext.GetSelection<U>()
            {
                foreach (object obj in this)
                {
                    U uObj = obj.As<U>();
                    if (uObj != null)
                        yield return uObj;
                }
            }

            bool ISelectionContext.SelectionContains(object item)
            {
                T tObj = item.As<T>();
                if (tObj != null)
                    return Contains(tObj);
                return false;
            }

            int ISelectionContext.SelectionCount
            {
                get { return Count; }
            }

            event EventHandler ISelectionContext.SelectionChanging
            {
                add { Changing += value; }
                remove { Changing -= value; }

            }

            event EventHandler ISelectionContext.SelectionChanged
            {
                add { Changed += value; }
                remove { Changed -= value; }
            }

            IEnumerable<object> ISelectionContext.Selection
            {
                get
                {
                    foreach (object obj in this)
                        yield return obj;
                }
                set
                {
                    Clear();
                    List<T> list = new List<T>();
                    foreach (object obj in value)
                    {
                        T tObj = obj.As<T>();
                        if (tObj != null)
                            list.Add(tObj);
                    }
                    SetRange(list);
                }
            }

            object ISelectionContext.LastSelected
            {
                get { return LastSelected; }
            }

            #endregion
        }

        private class TreeViewContext : IAdaptable, ITreeView, IItemView, IObservableContext
        {
            public TreeViewContext(IResourceFolder rootFolder)
            {
                m_rootFolder = rootFolder;
            }

            #region IAdaptable Members

            public object GetAdapter(Type type)
            {
                if (type.IsAssignableFrom(typeof(ISelectionContext)))
                    return m_selectionContext;
                if (type.IsAssignableFrom(GetType()))
                    return this;
                return null;
            }

            #endregion

            #region ITreeView Members

            public object Root
            {
                get { return m_rootFolder; }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                IResourceFolder resourceFolder = parent.As<IResourceFolder>();
                if (resourceFolder != null)
                {
                    foreach (IResourceFolder childFolder in resourceFolder.Folders)
                        yield return childFolder;
                }
            }

            #endregion

            #region IItemView Members

            /// <summary>
            /// Gets item's display information</summary>
            /// <param name="item">Item being displayed</param>
            /// <param name="info">Item info, to fill out</param>
            public virtual void GetInfo(object item, ItemInfo info)
            {
                IResourceFolder resourceFolder = item.As<IResourceFolder>();
                if (resourceFolder != null)
                {
                    info.Label = resourceFolder.Name;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.FolderImage);
                    info.AllowLabelEdit = !resourceFolder.ReadOnlyName;
                    info.IsLeaf = resourceFolder.Folders.Count == 0;
                }
            }
            #endregion

            #region IObservableContext Members

            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

            public event EventHandler Reloaded;

            protected virtual void OnItemInserted(ItemInsertedEventArgs<object> e)
            {
                if (ItemInserted != null)
                    ItemInserted(this, e);
            }

            protected virtual void OnItemRemoved(ItemRemovedEventArgs<object> e)
            {
                if (ItemRemoved != null)
                    ItemRemoved(this, e);
            }

            protected virtual void OnItemChanged(ItemChangedEventArgs<object> e)
            {
                if (ItemChanged != null)
                    ItemChanged(this, e);
            }

            protected virtual void OnReloaded(EventArgs e)
            {
                if (Reloaded != null)
                    Reloaded(this, e);
            }

            #endregion

            public IResourceFolder RootFolder
            {
                get { return m_rootFolder; }
            }

            private readonly ISelectionContext m_selectionContext = new GuiSelection<object>();
            private readonly IResourceFolder m_rootFolder;
        }

        private class ListViewContext : IListView, IItemView, IObservableContext, ISelectionContext
        {
            public ListViewContext()
            {
                m_selection.Changing += TheSelectionChanging;
                m_selection.Changed += TheSelectionChanged;
            }

            #region IAdaptable Members

            public object GetAdapter(Type type)
            {
                if (type.IsAssignableFrom(GetType()))
                    return this;
                return null;
            }

            #endregion

            #region IListView Members

            /// <summary>
            /// Gets names for table columns</summary>
            public string[] ColumnNames
            {
                get { return s_columnNames; }
            }

            private static readonly string[] s_columnNames = new[]
            {
                "Name", "Size", "Type", "Date Modified"
            };

            public IEnumerable<object> Items
            {
                get
                {
                    IResourceFolder resourceFolder = SelectedFolder;
                    if (resourceFolder != null)
                    {
                        foreach (Uri resourceUri in resourceFolder.ResourceUris)
                            yield return resourceUri;
                    }
                }
            }

            #endregion

            #region IItemView Members

            /// <summary>
            /// Gets item's display information</summary>
            /// <param name="item">Item being displayed</param>
            /// <param name="info">Item info, to fill out</param>
            public virtual void GetInfo(object item, ItemInfo info)
            {
                Uri resourceUri = ResourceLister.GetUriFromTag(item);
                if (resourceUri != null)
                {
                    FileInfo fileInfo = new FileInfo(resourceUri.LocalPath);

                    info.Label = fileInfo.Name;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Sce.Atf.Resources.ResourceImage);

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

                    info.Properties = new object[] {
                        length,
                        typeName, 
                        lastWriteTime
                    };
                }
            }
            #endregion

            #region IObservableContext Members

            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

            public event EventHandler Reloaded;

            protected virtual void OnItemInserted(ItemInsertedEventArgs<object> e)
            {
                if (ItemInserted != null)
                    ItemInserted(this, e);
            }

            protected virtual void OnItemRemoved(ItemRemovedEventArgs<object> e)
            {
                if (ItemRemoved != null)
                    ItemRemoved(this, e);
            }

            protected virtual void OnItemChanged(ItemChangedEventArgs<object> e)
            {
                if (ItemChanged != null)
                    ItemChanged(this, e);
            }

            protected virtual void OnReloaded(EventArgs e)
            {
                if (Reloaded != null)
                    Reloaded(this, e);
            }

            #endregion

            #region ISelectionContext Members

            public IEnumerable<object> Selection
            {
                get { return m_selection; }
                set { m_selection.SetRange(value); }
            }

            public IEnumerable<T> GetSelection<T>() where T : class
            {
                return m_selection.AsIEnumerable<T>();
            }

            public object LastSelected
            {
                get { return m_selection.LastSelected; }
            }

            public T GetLastSelected<T>() where T : class
            {
                return m_selection.GetLastSelected<T>();
            }

            public bool SelectionContains(object item)
            {
                return m_selection.Contains(item);
            }

            public int SelectionCount
            {
                get { return m_selection.Count; }
            }

            public event EventHandler SelectionChanging;
            public event EventHandler SelectionChanged;

            #endregion

            public IResourceFolder SelectedFolder
            {
                get { return m_selectedFolder; }
                set { m_selectedFolder = value; OnReloaded(EventArgs.Empty); }
            }

            private void TheSelectionChanging(object sender, EventArgs e)
            {
                SelectionChanging.Raise(this, EventArgs.Empty);
            }

            private void TheSelectionChanged(object sender, EventArgs e)
            {
                SelectionChanged.Raise(this, EventArgs.Empty);
            }

            private IResourceFolder m_selectedFolder;
            private readonly GuiSelection<object> m_selection = new GuiSelection<object>();
        }
    }
}

