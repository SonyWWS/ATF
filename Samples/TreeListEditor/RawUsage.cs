//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;

namespace TreeListEditor
{
    /// <summary>
    /// Tree list view editor for hierarchical file system component</summary>
    [Export(typeof(IInitializable))]
    class RawTreeListView : IInitializable, IControlHostClient
    {
        /// <summary>
        /// Constructor with parameters. Creates and registers UserControl and adds buttons to it.
        /// Creates a TreeListView to contain tree data.</summary>
        /// <param name="mainForm">Main form</param>
        /// <param name="contextRegistry">Context registry</param>
        /// <param name="settingsService">Settings service</param>
        /// <param name="controlHostService">Control host service</param>
        [ImportingConstructor]
        public RawTreeListView(
            MainForm mainForm,
            IContextRegistry contextRegistry,
            ISettingsService settingsService,
            IControlHostService controlHostService)
        {
            m_mainForm = mainForm;
            m_contextRegistry = contextRegistry;

            m_host = new UserControl { Dock = DockStyle.Fill };

            int x = 2, y = 2;
            var buttonHeight = -1;

            // Create and add button
            {
                var btn = CreateButton(AddFolderText, ref x, ref y, ref buttonHeight);
                btn.Click += BtnAddFolderClick;
                m_host.Controls.Add(btn);
            }

            // Create remove item button
            {
                m_btnRemoveItem = CreateButton(RemoveItemText, ref x, ref y, ref buttonHeight);
                m_btnRemoveItem.Enabled = false;
                m_btnRemoveItem.Click += BtnRemoveItemClick;
                m_host.Controls.Add(m_btnRemoveItem);
            }

            {
                var btn = CreateButton(InvertSelectionText, ref x, ref y, ref buttonHeight);
                btn.Click += BtnInvertSelectionClick;
                m_host.Controls.Add(btn);
            }

            {
                m_btnExpandOrCollapseSingle = CreateButton(ExpandOrCollapseNodeText, ref x, ref y, ref buttonHeight);
                m_btnExpandOrCollapseSingle.Enabled = false;
                m_btnExpandOrCollapseSingle.Click += BtnExpandOrCollapse;
                m_host.Controls.Add(m_btnExpandOrCollapseSingle);
            }

            // Add TreeListView
            {
                m_control = new TreeListView { Name = NameText, AllowDrop = true };
                m_control.NodeSorter = new MySorter(m_control);
                m_control.Columns.Add(new TreeListView.Column("Name"));
                m_control.NodeLazyLoad += ControlNodeLazyLoad;
                m_control.Control.Location = new Point(0, buttonHeight + 2);
                m_control.Control.Width = m_host.Width;
                m_control.Control.Height = m_host.Height - buttonHeight - 2;
                m_control.Control.Anchor =
                    AnchorStyles.Left | AnchorStyles.Top |
                    AnchorStyles.Right | AnchorStyles.Bottom;

                m_control.NodeExpandedChanged += TreeListViewNodeExpandedChanged;
                m_control.NodeSelected += TreeListViewNodeSelected;

                m_control.DragEnter += ControlDragEnter;
                m_control.DragDrop += ControlDragDrop;

                m_host.Controls.Add(m_control);
            }

            // Persist column widths
            {
                var owner =
                    string.Format(
                        "{0}-{1}-TreeListView",
                        this,
                        m_control.Name);

                settingsService.RegisterSettings(
                    owner,
                    new BoundPropertyDescriptor(
                        m_control,
                        () => m_control.PersistedSettings,
                        SettingsDisplayName,
                        SettingsCategory,
                        SettingsDescription));
            }

            // Create GUI
            {
                var info =
                    new ControlInfo(
                        m_control.Name,
                        m_control.Name,
                        StandardControlGroup.CenterPermanent);

                controlHostService.RegisterControl(
                    m_host,
                    info,
                    this);
            }

            if (s_dataImageIndex == -1)
            {
                s_dataImageIndex =
                    ResourceUtil.GetImageList16().Images.IndexOfKey(
                        Resources.DataImage);
            }

            if (s_folderImageIndex == -1)
            {
                s_folderImageIndex =
                    ResourceUtil.GetImageList16().Images.IndexOfKey(
                        Resources.FolderImage);
            }
        }

        #region IInitialize Interface

        /// <summary>
        /// Initialize component so it is displayed</summary>
        void IInitializable.Initialize()
        {
            // So the GUI will show up since nothing else imports it...
        }

        #endregion

        #region IControlHostClient Interface

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Activate(Control control)
        {
            if (ReferenceEquals(control, m_host))
                m_contextRegistry.ActiveContext = this;
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        /// <remarks>This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.</remarks>
        public void Deactivate(Control control)
        {
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the Control can close, or false to cancel</returns>
        /// <remarks>
        /// 1. This method is only called by IControlHostService if the Control was previously
        /// registered for this IControlHostClient.
        /// 2. If true is returned, the IControlHostService calls its own
        /// UnregisterControl. The IControlHostClient has to call RegisterControl again
        /// if it wants to re-register this Control.</remarks>
        public bool Close(Control control)
        {
            return true;
        }

        #endregion

        private void TreeListViewNodeExpandedChanged(object sender, TreeListView.NodeEventArgs e)
        {
            Outputs.WriteLine(
                OutputMessageType.Info,
                "{0} {1}",
                e.Node.Label,
                e.Node.Expanded ? "expanded!" : "collapsed!");
        }

        private void TreeListViewNodeSelected(object sender, ItemSelectedEventArgs<TreeListView.Node> e)
        {
            m_btnRemoveItem.Enabled = e.Selected;
            
            m_btnExpandOrCollapseSingle.Enabled = !e.Item.IsLeaf && (m_control.SelectedNodes.Count() == 1);
        }

        private class MySorter : IComparer<TreeListView.Node>
        {
            public MySorter(TreeListView control)
            {
                m_control = control;
            }

            public int Compare(TreeListView.Node x, TreeListView.Node y)
            {
                if ((x == null) && (y == null))
                    return 0;

                if (x == null)
                    return 1;

                if (y == null)
                    return -1;

                if (ReferenceEquals(x, y))
                    return 0;

                int result;

                if ((x.Tag is DirectoryInfo) &&
                    (y.Tag is DirectoryInfo))
                {
                    var dirX = (DirectoryInfo)x.Tag;
                    var dirY = (DirectoryInfo)y.Tag;
                    result = string.Compare(dirX.Name, dirY.Name);
                }
                else if (x.Tag is DirectoryInfo)
                {
                    result = -1;
                }
                else if (y.Tag is DirectoryInfo)
                {
                    result = 1;
                }
                else
                {
                    result = string.Compare(x.Label, y.Label);
                }

                if (m_control.SortOrder == SortOrder.Descending)
                    result *= -1;

                return result;
            }

            private readonly TreeListView m_control;
        }

        private void ControlNodeLazyLoad(object sender, TreeListView.NodeEventArgs e)
        {
            if (e.Node.Tag is DirectoryInfo)
                EnumerateDirectory(e.Node);
        }

        private void BtnAddFolderClick(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = SelectFolderText;
                dialog.ShowNewFolderButton = true;
                dialog.RootFolder = Environment.SpecialFolder.Desktop;

                if (dialog.ShowDialog(m_mainForm) != DialogResult.OK)
                    return;

                m_control.Nodes.Add(CreateNodeForPath(dialog.SelectedPath, true));
            }
        }

        private void BtnRemoveItemClick(object sender, EventArgs e)
        {
            var node = m_control.LastHit as TreeListView.Node;
            if (node == null)
                return;

            if (node.Parent == null)
                m_control.Nodes.Remove(node);
            else
                node.Parent.Nodes.Remove(node);
        }

        private void BtnInvertSelectionClick(object sender, EventArgs e)
        {
            try
            {
                m_control.BeginUpdate();
                InvertSelection(m_control.Nodes);
            }
            finally
            {
                m_control.EndUpdate();
            }
        }

        private void BtnExpandOrCollapse(object sender, EventArgs e)
        {
            var node = m_control.SelectedNodes.FirstOrDefault();
            if (node != null)
                node.Expanded = !node.Expanded;
        }

        private void ControlDragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var data = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (data == null)
                return;

            if (data.Length <= 0)
                return;

            try
            {
                foreach (var item in data)
                {
                    if (!Directory.Exists(item))
                        continue;

                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "Exception in RawUsage DragEnter: {0}",
                    ex.Message);
            }
        }

        private void ControlDragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var data = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (data == null)
                return;

            if (data.Length <= 0)
                return;

            try
            {
                foreach (var item in data)
                {
                    if (!Directory.Exists(item))
                        continue;

                    var node = CreateNodeForPath(item, true);
                    m_control.Nodes.Add(node);
                }
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "Exception in RawUsage DragDrop: {0}",
                    ex.Message);
            }
        }

        private static TreeListView.Node CreateNodeForPath(string path, bool directory)
        {
            var node = new TreeListView.Node {Label = Path.GetFileName(path)};

            if (directory)
                node.Tag = new DirectoryInfo(path);
            else
                node.Tag = new FileInfo(path);

            node.ImageIndex =
                directory
                    ? s_folderImageIndex
                    : s_dataImageIndex;

            // Whether the item has children. If it
            // has children the expander/collapser
            // icon will be drawn next to the item.
            node.IsLeaf =
                !directory
                    ? true
                    : (GetDirectoriesWrapper(path, "*.*", SearchOption.TopDirectoryOnly).Length <= 0) &&
                      (GetFilesWrapper(path, "*.*", SearchOption.TopDirectoryOnly).Length <= 0);

            return node;
        }

        private static void EnumerateDirectory(TreeListView.Node node)
        {
            if (!(node.Tag is DirectoryInfo))
                return;

            var di = (DirectoryInfo)node.Tag;

            var directories =
                GetDirectoriesWrapper(
                    di.FullName,
                    "*.*",
                    SearchOption.TopDirectoryOnly);

            foreach (var directory in directories)
                node.Nodes.Add(CreateNodeForPath(directory, true));

            var files =
                GetFilesWrapper(
                    di.FullName,
                    "*.*",
                    SearchOption.TopDirectoryOnly);

            foreach (var file in files)
                node.Nodes.Add(CreateNodeForPath(file, false));
        }

        private static string[] GetDirectoriesWrapper(string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                return Directory.GetDirectories(path, searchPattern, searchOption);
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "Exception enumerating path \"{0}\": {1}",
                    path,
                    ex.Message);

                return new string[0];
            }
        }

        private static string[] GetFilesWrapper(string path, string searchPattern, SearchOption searchOption)
        {
            try
            {
                return Directory.GetFiles(path, searchPattern, searchOption);
            }
            catch (Exception ex)
            {
                Outputs.WriteLine(
                    OutputMessageType.Error,
                    "Exception enumerating path \"{0}\": {1}",
                    path,
                    ex.Message);

                return new string[0];
            }
        }

        private static Button CreateButton(string text, ref int x, ref int y, ref int height)
        {
            var btn = new Button {Text = text};

            var size = TextRenderer.MeasureText(btn.Text, btn.Font);
            btn.Width = size.Width + 20;

            btn.Location = new Point(x, y);
            btn.Anchor = AnchorStyles.Left | AnchorStyles.Top;

            x += btn.Width + 2;

            if (height == -1)
                height = btn.Height;

            return btn;
        }

        private static void InvertSelection(IEnumerable<TreeListView.Node> collection)
        {
            foreach (var node in collection)
            {
                node.Selected = !node.Selected;

                if (!node.HasChildren)
                    continue;

                InvertSelection(node.Nodes);
            }
        }
        
        private readonly MainForm m_mainForm;
        private readonly UserControl m_host;
        private readonly TreeListView m_control;
        private readonly IContextRegistry m_contextRegistry;
        
        private readonly Button m_btnRemoveItem;
        private readonly Button m_btnExpandOrCollapseSingle;

        private static int s_dataImageIndex = -1;
        private static int s_folderImageIndex = -1;

        private const string NameText = "Raw TreeListView Usage";
        private const string AddFolderText = "Add Folder";
        private const string SelectFolderText = "Select a folder!";
        private const string RemoveItemText = "Remove Item";
        private const string InvertSelectionText = "Invert Selection";
        private const string ExpandOrCollapseNodeText = "Expand or Collapse Node";
        private const string SettingsDisplayName = "TreeListView Persisted Settings";
        private const string SettingsCategory = "TreeListView";
        private const string SettingsDescription = "TreeListView Persisted Settings";
    }
}