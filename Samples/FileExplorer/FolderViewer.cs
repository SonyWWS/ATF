//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls;

namespace FileExplorerSample
{
    /// <summary>
    /// The FolderViewer displays the contents of the user's C:\ drive using a TreeControl. It
    /// uses the ATF TreeControlAdapter to synchronize the contents of the control to C:\. In
    /// order to use TreeControlAdapter, it uses a private class, FileTreeView, to implement
    /// the required ITreeView/IItemView interfaces on C:\. When the selected tree item changes,
    /// FolderViewer sets the FileViewer's Path property to display the new folder's contents.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(FolderViewer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FolderViewer : IInitializable
    {
        /// <summary>
        /// Constructor creating a new TreeControl</summary>
        /// <param name="mainForm">Main form</param>
        /// <param name="fileViewer">File viewer</param>
        [ImportingConstructor]
        public FolderViewer(MainForm mainForm, FileViewer fileViewer)
        {
            m_mainForm = mainForm;
            m_fileViewer = fileViewer;

            m_treeControl = new TreeControl();
            m_treeControl.Text = "Folder Viewer";
            m_treeControl.ImageList = ResourceUtil.GetImageList16();
            m_treeControl.SelectionMode = SelectionMode.One;
            m_treeControl.Dock = DockStyle.Fill;
            m_treeControl.Width = 256;

            m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
            m_fileTreeView = new FileTreeView();
            m_fileTreeView.SelectionChanged += new EventHandler(fileTreeView_SelectionChanged);
            m_treeControlAdapter.TreeView = m_fileTreeView;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by adding tree control to panel</summary>
        void IInitializable.Initialize()
        {
            m_mainForm.SplitContainer.Panel1.Controls.Add(m_treeControl);
        }

        #endregion

        private void fileTreeView_SelectionChanged(object sender, EventArgs e)
        {
            Sce.Atf.Path<object> lastPath = m_fileTreeView.LastSelected as Sce.Atf.Path<object>;
            if (lastPath != null)
            {
                FileSystemInfo info = lastPath.Last as FileSystemInfo;
                if (info is DirectoryInfo)
                    m_fileViewer.Path = info.FullName;
            }
        }

        /// <summary>
        /// Adapts C:\ to a tree of selectable items</summary>
        private class FileTreeView : ITreeView, IItemView, ISelectionContext
        {
            public FileTreeView()
            {
                m_selection = new Selection<object>();
                m_selection.Changed += new EventHandler(selection_Changed);

                // suppress compiler warning
                if (SelectionChanging == null) return;
            }

            #region ITreeView Members

            public object Root
            {
                get { return new DirectoryInfo(m_path); }
            }

            public IEnumerable<object> GetChildren(object parent)
            {
                IEnumerable<object> result = null;
                DirectoryInfo directoryInfo = parent as DirectoryInfo;
                if (directoryInfo != null)
                    result = GetSubDirectories(directoryInfo); //may return null

                if (result == null)
                    return EmptyEnumerable<object>.Instance;
                return result;
            }

            #endregion

            #region IItemView Members

            public void GetInfo(object item, Sce.Atf.Applications.ItemInfo info)
            {
                DirectoryInfo directoryInfo = item as DirectoryInfo;
                if (directoryInfo != null)
                {
                    info.Label = directoryInfo.Name;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.ComputerImage);
                    DirectoryInfo[] directories = GetSubDirectories(directoryInfo);

                    info.IsLeaf =
                        directories != null &&
                        directories.Length == 0;
                }
            }

            #endregion

            #region ISelectionContext Members

            public IEnumerable<object> Selection
            {
                get { return m_selection; }
                set { m_selection.SetRange(value); }
            }

            public IEnumerable<T> GetSelection<T>()
                        where T : class
            {
                return m_selection.AsIEnumerable<T>();
            }

            public object LastSelected
            {
                get { return m_selection.LastSelected; }
            }

            public T GetLastSelected<T>()
                        where T : class
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

            private void selection_Changed(object sender, EventArgs e)
            {
                SelectionChanged.Raise(this, EventArgs.Empty);
            }

            private static DirectoryInfo[] GetSubDirectories(DirectoryInfo directoryInfo)
            {
                DirectoryInfo[] directories = null;
                try
                {
                    directories = directoryInfo.GetDirectories();
                }
                catch
                {
                }

                return directories;
            }

            private Selection<object> m_selection;
            private string m_path = @"C:\";
        }

        private MainForm m_mainForm;
        private FileViewer m_fileViewer;
        private TreeControl m_treeControl;
        private TreeControlAdapter m_treeControlAdapter;
        private FileTreeView m_fileTreeView;
    }
}