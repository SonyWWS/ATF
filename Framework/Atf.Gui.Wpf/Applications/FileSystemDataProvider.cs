//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// A DataSourceProvider which exposes a collection of ObservableFileInfo objects, representing 
    /// the files in a particular directory. Changes made to the files in that directory are reflected 
    /// at runtime.</summary>
    public class FileSystemDataProvider : DataSourceProvider
    {
        #region Data

        private readonly FileSystemWatcher m_watcher;
        private readonly ObservableFileInfoCollection m_files;

        #endregion

        #region Ctors

        public FileSystemDataProvider()
        {
            m_files = new ObservableFileInfoCollection();

            m_watcher = new FileSystemWatcher();

            m_watcher.NotifyFilter =
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.FileName;

            m_watcher.Changed += OnFileChanged;
            m_watcher.Created += OnFileCreated;
            m_watcher.Deleted += OnFileDeleted;
            m_watcher.Renamed += OnFileRenamed;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the directory whose files is monitored.
        /// </summary>
        public string Path
        {
            get { return m_watcher.Path; }
            set
            {
                if (m_watcher.Path == value)
                    return;

                if (!Directory.Exists(value))
                {
                    System.Diagnostics.Debug.Fail("Directory does not exist: " + value);
                }
                else
                {
                    m_watcher.Path = value;

                    // Once the watcher has a valid path, then 
                    // we can turn events on for it.
                    if (!m_watcher.EnableRaisingEvents)
                        m_watcher.EnableRaisingEvents = true;

                    OnPropertyChanged(new PropertyChangedEventArgs("Path"));
                }
            }
        }

        #endregion

        #region BeginQuery

        /// <summary>
        /// Stores information about all of the files 
        /// in the target directory.
        /// </summary>
        protected override void BeginQuery()
        {
            string path = m_watcher.Path;

            if (Directory.Exists(path))
            {
                if (0 < m_files.Count)
                    m_files.Clear();

                string[] filePaths = Directory.GetFiles(path);
                foreach (string filePath in filePaths)
                    m_files.Add(new ObservableFileInfo(filePath));
            }

            // Hand off the results to the base class.
            base.OnQueryFinished(m_files);
        }

        #endregion

        #region FileSystemWatcher Event Handlers

        void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                ObservableFileInfo changedFile = m_files.GetFile(e.FullPath);
                if (changedFile != null)
                {
                    changedFile.Refresh();
                }
            });
        }

        void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                // Ignore new directories.
                if (File.Exists(e.FullPath))
                {
                    m_files.Add(new ObservableFileInfo(e.FullPath));
                }
            });
        }

        void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                ObservableFileInfo deletedFile = m_files.GetFile(e.FullPath);
                if (deletedFile != null)
                {
                    m_files.Remove(deletedFile);
                }
            });
        }

        void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            Dispatcher.InvokeIfRequired(() =>
            {
                ObservableFileInfo file = m_files.GetFile(e.OldFullPath);
                if (file != null)
                {
                    file.ChangeName(e.FullPath);
                }
            });
        }

        #endregion
    }

    /// <summary>
    /// Exposes a FileInfo object and raises a property changed notification event when the file is modified</summary>
    public class ObservableFileInfo : NotifyPropertyChangedBase
    {
        #region Data

        private bool m_isChanged;
        private string m_fileName;
        private FileInfo m_fileInfo;

        #endregion // Data

        #region Ctors

        /// <summary>
        /// Constructor with file path</summary>
        /// <param name="file">File path</param>
        public ObservableFileInfo(string file)
        {
            m_fileName = file;
            TakeSnapshot();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method is called when the file name is modified.
        /// </summary>
        /// <param name="newName">The new file name.</param>
        public void ChangeName(string newName)
        {
            m_fileName = newName;
            Refresh();
        }

        /// <summary>
        /// This method is called when the file
        /// has been changed in some way.
        /// </summary>
        public void Refresh()
        {
            TakeSnapshot();
            MarkAsChanged();
        }

        #endregion // Public Methods

        #region Public Properties

        /// <summary>
        /// Returns information about the file.
        /// </summary>
        public FileInfo FileInfo
        {
            get { return m_fileInfo; }
            private set
            {
                if (m_fileInfo != value)
                {
                    m_fileInfo = value;
                    RaisePropertyChanged("FileInfo");
                }
            }
        }

        /// <summary>
        /// Get whether file info changed</summary>
        public bool IsChanged
        {
            get { return m_isChanged; }
            private set
            {
                if (m_isChanged != value)
                {
                    m_isChanged = value;
                    RaisePropertyChanged("IsChanged");
                }
            }
        }

        #endregion

        #region Private Helpers

        private void MarkAsChanged()
        {
            IsChanged = true;
            IsChanged = false;
        }

        private void TakeSnapshot()
        {
            FileInfo = new FileInfo(m_fileName);
        }

        #endregion
    }

    /// <summary>
    /// Collection of ObservableFileInfo objects</summary>
    public class ObservableFileInfoCollection : ObservableCollection<ObservableFileInfo>
    {
        /// <summary>
        /// Return ObservableFileInfo for file path</summary>
        /// <param name="fullName">File path</param>
        /// <returns>ObservableFileInfo for file path</returns>
        public ObservableFileInfo GetFile(string fullName)
        {
            return this.FirstOrDefault(file => string.CompareOrdinal(file.FileInfo.FullName, fullName) == 0);
        }
    }
}
