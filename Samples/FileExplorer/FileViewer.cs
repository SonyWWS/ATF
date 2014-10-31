//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace FileExplorerSample
{
    /// <summary>
    /// The FileViewer displays the contents of a folder using a ListView control. It uses
    /// the ATF ListViewAdapter to synchronize the contents of the control to the folder. In
    /// order to use ListViewAdapter, it uses a private class, FileListView, to implement
    /// the required IListView/IItemView interfaces on a file folder. IObservableContext is
    /// also implemented and could be hooked up to file system events using file watcher
    /// support in .NET.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(FileViewer))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FileViewer : IInitializable
    {
        /// <summary>
        /// Constructor that creates a standard WinForms ListView control</summary>
        /// <param name="mainForm">Main form</param>
        [ImportingConstructor]
        public FileViewer(MainForm mainForm)
        {
            m_mainForm = mainForm;

            // create a standard WinForms ListView control
            m_listView = new ListView();
            m_listView.Dock = DockStyle.Fill;
            m_listView.Text = "File Viewer";
            m_listView.BackColor = SystemColors.Window;
            m_listView.SmallImageList = ResourceUtil.GetImageList16();
            m_listView.AllowColumnReorder = true;

            // create an adapter to drive the ListView control
            m_listViewAdapter = new ListViewAdapter(m_listView);

            m_fileListView = new FileListView();
        }

        [Import(AllowDefault=true)] // optional service
        private ISettingsService m_settingsService = null;

        [ImportMany] // gets all file data extensions
        private IEnumerable<Lazy<IFileDataExtension>> m_extensions = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by creating data extension list and set control parameters</summary>
        void IInitializable.Initialize()
        {
            // pass all file data extensions to adapter
            List<IFileDataExtension> list = new List<IFileDataExtension>();
            foreach (Lazy<IFileDataExtension> extension in m_extensions)
                list.Add(extension.Value);

            m_fileListView.FileDataExtensions = list.ToArray();

            // set the adapter's ListView to an adapter that returns directory contents
            m_listViewAdapter.ListView = m_fileListView;

            m_mainForm.SplitContainer.Panel2.Controls.Add(m_listView);

            SettingsServices.RegisterSettings(
                m_settingsService,
                this,
                new BoundPropertyDescriptor(this, () => ListViewSettings, "ListViewSettings", null, null));
        }

        #endregion

        /// <summary>
        /// Gets or sets the path to the folder whose contents are displayed</summary>
        public string Path
        {
            get { return m_fileListView.Path; }
            set
            {
                m_fileListView.Path = value;
                m_mainForm.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the ListView's state for saving/restoring its state. This property
        /// is registered with the SettingsService.</summary>
        public string ListViewSettings
        {
            get { return m_listViewAdapter.Settings; }
            set { m_listViewAdapter.Settings = value; }
        }

        /// <summary>
        /// Adapts a directory to an observable list of items</summary>
        private class FileListView : IListView, IItemView, IObservableContext
        {
            public FileListView()
            {
                // inhibit compiler warnings; we never raise these events, though it would be
                //  possible, using the file watcher support in .Net
                if (ItemInserted == null) return;
                if (ItemRemoved == null) return;
                if (ItemChanged == null) return;
            }

            /// <summary>
            /// Gets or sets the directory path to adapt to a list</summary>
            public string Path
            {
                get { return m_path; }
                set
                {
                    if (m_path != value)
                    {
                        m_path = value;

                        Reloaded.Raise(this, EventArgs.Empty);
                    }
                }
            }
            private string m_path;

            public IFileDataExtension[] FileDataExtensions
            {
                get { return m_fileDataExtensions; }
                set { m_fileDataExtensions = value; }
            }
            private IFileDataExtension[] m_fileDataExtensions;

            #region IListView Members

            /// <summary>
            /// Gets names for file list view columns</summary>
            public string[] ColumnNames
            {
                get
                {
                    string[] result = new string[m_fileDataExtensions.Length];
                    for (int i = 0; i < result.Length; i++)
                        result[i] = m_fileDataExtensions[i].ColumnName;
                    return result;
                }
            }

            /// <summary>
            /// Gets the items in the list</summary>
            public IEnumerable<object> Items
            {
                get
                {
                    if (m_path == null ||
                        !Directory.Exists(m_path))
                    {
                        return EmptyEnumerable<object>.Instance;
                    }

                    DirectoryInfo directory = new DirectoryInfo(m_path);
                    DirectoryInfo[] subDirectories = null;
                    try
                    {
                        subDirectories = directory.GetDirectories();
                    }
                    catch
                    {
                    }
                    if (subDirectories == null)
                        subDirectories = new DirectoryInfo[0];

                    FileInfo[] files = null;
                    try
                    {
                        files = directory.GetFiles();
                    }
                    catch
                    {
                    }
                    if (files == null)
                        files = new FileInfo[0];

                    List<object> children = new List<object>(subDirectories.Length + files.Length);
                    children.AddRange(subDirectories);
                    children.AddRange(files);
                    return children;
                }
            }

            #endregion

            #region IItemView Members

            /// <summary>
            /// Gets display info for the item</summary>
            /// <param name="item">Item</param>
            /// <param name="info">Display info for item</param>
            public void GetInfo(object item, ItemInfo info)
            {
                // set the first column info (name) 
                FileSystemInfo fileSystemInfo = item as FileSystemInfo;
                if (fileSystemInfo is DirectoryInfo)
                {
                    info.Label = fileSystemInfo.Name;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.FolderImage);
                }
                else if (fileSystemInfo is FileInfo)
                {
                    info.Label = fileSystemInfo.Name;
                    info.ImageIndex = info.GetImageList().Images.IndexOfKey(Resources.DocumentImage);
                    info.IsLeaf = true;
                }

                // set the 2nd and 3nd columns info (size and creation time) 
                info.Properties = new string[m_fileDataExtensions.Length-1];
                for (int i = 0; i < info.Properties.Length; i++)
                    info.Properties[i] = m_fileDataExtensions[i+1].GetValue(fileSystemInfo);
            }

            #endregion

            #region IObservableContext Members

            /// <summary>
            /// Event that is raised when an item is inserted</summary>
            public event EventHandler<ItemInsertedEventArgs<object>> ItemInserted;

            /// <summary>
            /// Event that is raised when an item is removed</summary>
            public event EventHandler<ItemRemovedEventArgs<object>> ItemRemoved;

            /// <summary>
            /// Event that is raised when an item is changed</summary>
            public event EventHandler<ItemChangedEventArgs<object>> ItemChanged;

            /// <summary>
            /// Event that is raised when the collection has been reloaded</summary>
            public event EventHandler Reloaded;

            #endregion

            static FileListView()
            {
                #pragma warning disable 0219
                string dummy = Resources.FolderImage; // force initialization of image resources
            }
        }

        private MainForm m_mainForm;
        private ListView m_listView;
        private FileListView m_fileListView;
        private ListViewAdapter m_listViewAdapter;
    }
}