//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service to watch for changes to directories; it requires an ISynchronizeInvoke
    /// component to ensure that notification events are raised on the UI thread.</summary>
    /// <remarks>
    /// For WinForms apps, the MainForm provides the ISynchronizeInvoke component. For
    /// WPF apps, include Sce.Atf.Wpf.Applications.SynchronizeInvoke in your TypeCatalog.</remarks>
    [Export(typeof(IDirectoryWatcherService))]
    [Export(typeof(DirectoryWatcherService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DirectoryWatcherService : IDirectoryWatcherService
    {
        /// <summary>
        /// Default constructor, required for Managed Extensibility Framework (MEF). The
        /// ISynchronizeInvoke object is imported if it's available.</summary>
        [ImportingConstructor]
        public DirectoryWatcherService()
        {
        }

        /// <summary>
        /// Constructor for applications not using the Managed Extensibility Framework (MEF)</summary>
        /// <param name="synchronizationObject">Synchronization object, for example, the main form. Is used so that
        /// FileChanged events are raised on the main UI thread. Can be null.</param>
        public DirectoryWatcherService(ISynchronizeInvoke synchronizationObject)
        {
            m_syncObject = synchronizationObject;
        }

        /// <summary>
        /// Gets or sets the synchronizing object (typically the main form) which allows
        /// FileChanged events to be raised on the main UI thread</summary>
        /// created this SyncObject.</summary>
        public ISynchronizeInvoke SynchronizingObject
        {
            get { return m_syncObject; }
            set { m_syncObject = value; }
        }

        /// <summary>
        /// Internal buffer size in KB for FileSystemWatchers used. Default is 8192KB </summary>
        [DefaultValue(8192)]
        public int InternalBufferSize
        {
            get { return m_internalBufferSize; }
            set { m_internalBufferSize = value; }
        }

        /// <summary>
        /// Registers a file to be watched</summary>
        /// <param name="filePath">Path of file to watch</param>
        /// <param name="includeSubdirectories">True to watch sub directories too</param>
        public void Register(string directory, IEnumerable<string> extensions, bool includeSubdirectories)
        {
            List<FileSystemWatcher> watchers;
            if (!m_watchers.TryGetValue(directory, out watchers))
            {
                watchers = new List<FileSystemWatcher>();
                m_watchers.Add(directory, watchers);
            }
            
            foreach (var ext in extensions)
            {
                FileSystemWatcher watcher = new FileSystemWatcher(directory, ext);

                // watch for changes to file
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName;
                watcher.SynchronizingObject = m_syncObject;
                watcher.IncludeSubdirectories = includeSubdirectories;
                watcher.InternalBufferSize = InternalBufferSize;

                watcher.Changed += new FileSystemEventHandler(watcher_Changed);
                watcher.Renamed += new RenamedEventHandler(watcher_Changed);
                watcher.Deleted += new FileSystemEventHandler(watcher_Changed);
                
                //Note: Disabled created since this will be fired before a file has finished copying which can create
                //bad side effects. The changed event should fire when the file has closed.
                //watcher.Created += new FileSystemEventHandler(watcher_Changed);

                watchers.Add(watcher);
                watcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Unregisters a file, so it is no longer watched</summary>
        /// <param name="filePath">Path of file to no longer watch</param>
        public void Unregister(string directory)
        {
            List<FileSystemWatcher> watchers;
            if (m_watchers.TryGetValue(directory, out watchers))
            {
                foreach (var watcher in watchers)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
                
                m_watchers.Remove(directory);
            }
        }

        /// <summary>
        /// Event that is raised when a file is changed, renamed, or deleted. This event is raised synchronously, 
        /// allowing one file changed event to complete before the same event is raised again.</summary>
        public event FileSystemEventHandler FileChanged;

        /// <summary>
        /// Called after a file has changed. Raises the FileChanged event. Is called synchronously,
        /// to allow handling of one file changed event before another is raised.</summary>
        /// <param name="sender">The sender (a <see cref="System.IO.FileSystemWatcher"/>)</param>
        /// <param name="e">The <see cref="System.IO.FileSystemEventArgs"/> instance containing the event data.</param>
        protected virtual void OnChanged(object sender, FileSystemEventArgs e)
        {
            FileSystemEventHandler handler = FileChanged;
            if (handler != null)
                handler(this, e);
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Make sure the user isn't alerted twice to the same file being changed.
            // DAN: BUG - Getting a null ref sometimes here
            lock (m_queue)
            {
                if (m_queue.Any(eventInfo => eventInfo.Second.FullPath == e.FullPath))
                {
                    return;
                }

                m_queue.Enqueue(new Pair<object, FileSystemEventArgs>(sender, e));

                // Check if this is a recursive call by the main thread due to waiting for a dialog box.
                if (m_queue.Count > 1)
                    return;

                // Raise the events serially in the order they were received.
                while (m_queue.Count > 0)
                {
                    Pair<object, FileSystemEventArgs> eventInfo = m_queue.Peek();
                    OnChanged(eventInfo.First, eventInfo.Second);
                    m_queue.Dequeue();
                }
            }
        }

        private Dictionary<string, List<FileSystemWatcher>> m_watchers =
            new Dictionary<string, List<FileSystemWatcher>>(StringComparer.InvariantCultureIgnoreCase);

        [Import(AllowDefault = true)]
        private ISynchronizeInvoke m_syncObject;
        private int m_internalBufferSize = 8192;

        // Queue of file change event information, as pairs of 'sender' and 'e', so as to not lose
        //  file change events for when multiple watched files change while the user is responding
        //  to a dialog box on the main UI thread.
        private Queue<Pair<object, FileSystemEventArgs>> m_queue =
            new Queue<Pair<object, FileSystemEventArgs>>();
    }
}
