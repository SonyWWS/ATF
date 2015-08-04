//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service to watch for changes to files; it requires an ISynchronizeInvoke
    /// component to ensure that notification events are raised on the UI thread</summary>
    /// <remarks>For WinForms applications, the MainForm provides the ISynchronizeInvoke component. For
    /// WPF applications, include Sce.Atf.Wpf.Applications.SynchronizeInvoke in your TypeCatalog.</remarks>
    [Export(typeof(IFileWatcherService))]
    [Export(typeof(FileWatcherService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class FileWatcherService : IFileWatcherService
    {
        /// <summary>
        /// Default constructor, required for Managed Extensibility Framework (MEF). The
        /// ISynchronizeInvoke object is imported if it's available.</summary>
        [ImportingConstructor]
        public FileWatcherService()
        {
        }

        /// <summary>
        /// Constructor for applications not using the Managed Extensibility Framework (MEF)</summary>
        /// <param name="synchronizationObject">Synchronization object, for example, the main form. Is used so that
        /// FileChanged events are raised on the main UI thread. Can be null.</param>
        public FileWatcherService(ISynchronizeInvoke synchronizationObject)
        {
            m_syncObject = synchronizationObject;
        }

        /// <summary>
        /// Registers a file to be watched</summary>
        /// <param name="filePath">Path of file to watch. It must exist.</param>
        public void Register(string filePath)
        {
            if (!m_watchers.ContainsKey(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                string filter = Path.GetFileName(filePath);
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(filePath);
                var watcher = new FileSystemWatcher(directory, filter);
                watcher.SynchronizingObject = m_syncObject;

                watcher.Changed += watcher_Changed;
                watcher.Renamed += OnRenamed;

                var watcherInfo = new FileWatcherInfo
                {
                    Watcher = watcher,
                    LastWriteTime = lastWriteTime
                };
                m_watchers.Add(filePath, watcherInfo);

                watcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Unregisters a file, so it is no longer watched</summary>
        /// <param name="filePath">Path of file no longer watched</param>
        public void Unregister(string filePath)
        {
            FileWatcherInfo watcherInfo;
            if (m_watchers.TryGetValue(filePath, out watcherInfo))
            {
                watcherInfo.Watcher.EnableRaisingEvents = false;
                watcherInfo.Watcher.Dispose();
                m_watchers.Remove(filePath);
            }
        }

        /// <summary>
        /// Event that is raised when a file is changed (not renamed or deleted). This event is
        /// raised synchronously, allowing one file changed event to complete before the same event
        /// is raised again.</summary>
        /// <remarks>If a watched file is deleted and then another file is renamed to the original
        /// watched file's name (e.g., if there's a Perforce update), that causes this event
        /// to be raised and the FileSystemEventArgs have the WatcherChangeTypes.Changed flag set.</remarks>
        public event FileSystemEventHandler FileChanged;

        /// <summary>
        /// Method called after a file has changed. Raises the FileChanged event. Is called synchronously,
        /// to allow handling of one file changed event before another is raised.</summary>
        /// <param name="sender">The sender, (a <see cref="System.IO.FileSystemWatcher"/>), whose
        /// Path and Filter match e.FullPath</param>
        /// <param name="e">The <see cref="System.IO.FileSystemEventArgs"/> instance containing the event data</param>
        protected virtual void OnChanged(object sender, FileSystemEventArgs e)
        {
            FileSystemEventHandler handler = FileChanged;
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Method called after a file has been renamed</summary>
        /// <param name="sender">The sender (a <see cref="System.IO.FileSystemWatcher"/>) whose
        /// Path and Filter matches either e.OldFullPath or e.FullPath</param>
        /// <param name="e">The <see cref="System.IO.RenamedEventArgs"/> instance containing the event data</param>
        protected virtual void OnRenamed(object sender, RenamedEventArgs e)
        {
            // The FileChanged event should only be raised when the contents of a file have
            //  (probably) changed. Version control software, like Perforce, might be deleting
            //  the original file then renaming a temporary file to be the name of the original.
            //  So, if the new name matches a file that we are watching, assume it was changed.
            // Just in case it matters, get the watcher actually associated with FullPath,
            //  which might be different than 'sender', because we don't know if 'sender' is
            //  watching e.OldFullPath or e.FullPath.
            FileWatcherInfo watcherInfo;
            if (m_watchers.TryGetValue(e.FullPath, out watcherInfo))
            {
                watcher_Changed(watcherInfo.Watcher,
                    new FileSystemEventArgs(WatcherChangeTypes.Changed, //our FileChanged event is not for renaming
                        watcherInfo.Watcher.Path, watcherInfo.Watcher.Filter));
            }
        }

        /// <summary>
        /// Gets or sets the synchronizing object (typically the main form) which allows
        /// FileChanged events to be raised on the main UI thread</summary>
        /// <remarks>Setting this has no effect on files that are already being watched.
        /// This property is for applications that are using the ATF 2 plugin service
        /// or, in general, for applications that must construct this FileWatcherService before
        /// the synchronizing object is available.</remarks>
        protected ISynchronizeInvoke SynchronizingObject
        {
            get { return m_syncObject; }
            set { m_syncObject = value; }
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Make sure the listener hasn't already seen the change. DateTime has a resolution
            //  of 100 nanoseconds. Should be good enough!
            FileWatcherInfo watcherInfo;
            if (m_watchers.TryGetValue(e.FullPath, out watcherInfo))
            {
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(e.FullPath);
                if (lastWriteTime == watcherInfo.LastWriteTime)
                    return;
                watcherInfo.LastWriteTime = lastWriteTime;
            }

            // Although this method is called on one thread (because of SynchronizingObject)
            //  the listener may have a dialog box open which allows for reentrancy. In that
            //  situation, we want to avoid queuing multiple change events for the same file.
            foreach(Pair<object, FileSystemEventArgs> eventInfo in m_queue)
                if (eventInfo.Second.FullPath == e.FullPath)
                    return;

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

        private class FileWatcherInfo
        {
            public FileSystemWatcher Watcher;
            public DateTime LastWriteTime;
        }

        // This is used as a synchronizing object for Register() calls, so that FileChanged events
        //  are raised on the main UI thread.
        [Import(AllowDefault = true)]
        private ISynchronizeInvoke m_syncObject;

        private readonly Dictionary<string, FileWatcherInfo> m_watchers =
            new Dictionary<string, FileWatcherInfo>(StringComparer.InvariantCultureIgnoreCase);
        
        // Queue of file change event information, as pairs of 'sender' and 'e', so as to not lose
        //  file change events for when multiple watched files change while the user is responding
        //  to a dialog box on the main UI thread.
        private readonly Queue<Pair<object,FileSystemEventArgs>> m_queue =
            new Queue<Pair<object,FileSystemEventArgs>>();
    }
}
