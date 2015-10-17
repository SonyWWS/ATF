//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Abstract base class for source control service</summary>
    [InheritedExport(typeof(SourceControlService))]
    public abstract class SourceControlService : ISourceControlService
    {
        /// <summary>
        /// Constructor</summary>
        protected SourceControlService()
        {
            AllowCheckIn = true;
        }

        /// <summary>
        /// Connect to the source control server</summary>
        /// <returns>True when the connection is successful; false when the connection is canceled or failed</returns>
        public virtual bool Connect()
        {
            return true;
        }

        /// <summary>
        /// Disconnect from the source control server</summary>
        public virtual void Disconnect()
        {
        }

        /// <summary>
        /// Gets or sets whether the source control server is connected</summary>
        public virtual bool IsConnected { get; set; }

        /// <summary>
        /// Gets whether the source control server connection can be configured</summary>
        public virtual bool CanConfigure
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets whether the source control service is enabled</summary>
        public virtual bool Enabled
        {
            get { return m_enabled; }
            set
            {
                if (value != Enabled)
                {
                    m_enabled = value;
                    OnEnabledChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the source control service should throw exceptions 
        /// caused by run time errors from the server</summary>
        /// <remarks>The default value is false (turnoff exceptions)</remarks>
        public bool ThrowExceptions { get; set; }


        /// <summary>
        /// Event that is raised after the connection to the server is changed</summary>
        public event EventHandler ConnectionChanged;

        /// <summary>
        /// Event that is raised after Enabled property changes.</summary>
        public event EventHandler EnabledChanged;

        #region ISourceControlService Members

        /// <summary>
        /// Gets or sets whether multiple users can check out the same file</summary>
        public bool AllowMultipleCheckout { get; set; }

        /// <summary>
        /// Gets or sets whether checkins are permitted</summary>
        public bool AllowCheckIn { get; set; }

        /// <summary>
        /// Event that is raised after an item's status changes</summary>
        public event EventHandler<SourceControlEventArgs> StatusChanged;

        /// <summary>
        /// Refreshes cached status info for all items under the specified folder</summary>
        /// <param name="rootUri">The uri under which all cached file info will be refreshed</param>
        /// <param name="resetCacheFirst">If true, cache is cleared out before refreshing</param>
        /// <remarks>
        /// Using this call minimizes the number of queries to the source control server,
        /// by allowing large subtrees of files to be queried and cached at once</remarks>
        public abstract void UpdateCachedStatuses(Uri rootUri, bool resetCacheFirst);

        /// <summary>
        /// Send StatusChanged events for all specified uris</summary>
        /// <param name="uris">The uris for which StatusChanged events should be fired</param>
        /// <remarks>
        /// Cached statuses are broadcast if available. For all others, the source control
        /// server is queried</remarks>
        public abstract void BroadcastStatuses(IEnumerable<Uri> uris);

        /// <summary>Adds an item to source control</summary>
        /// <param name="uri">URI representing the path to item</param>
        public abstract void Add(Uri uri);

        /// <summary>
        /// Deletes an item from source control</summary>
        /// <param name="uri">URI representing the path to item</param>
        public abstract void Delete(Uri uri);

        /// <summary>
        /// Checks in the given items</summary>
        /// <param name="uris">URIs representing the path to items</param>
        /// <param name="description">Check-in description</param>
        public abstract  void CheckIn(IEnumerable<Uri> uris, string description);

        /// <summary>
        /// Checks out an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        public abstract void CheckOut(Uri uri);

        /// <summary>Gets the latest version of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        public abstract void GetLatestVersion(Uri uri);
 
        /// <summary>
        /// Reverts an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        public abstract void Revert(Uri uri);

        /// <summary>
        /// Gets the source control status of all items under the folder</summary>
        /// <param name="uri">URI representing the folder path</param>
        /// <returns>False if not supported</returns>
        public abstract bool GetFolderStatus(Uri uri);

        /// <summary>
        /// Gets the source control status of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>Status of item</returns>
        public abstract SourceControlStatus GetStatus(Uri uri);

        /// <summary>
        /// Gets the source control status of each items.</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <returns>Status of each item, in the same order as given</returns>
        public abstract SourceControlStatus[] GetStatus(IEnumerable<Uri> uris);

        /// <summary>
        /// From given items, gets those that are different from the revision in the depot.</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <returns>Items that are different from the depot</returns>
        public abstract IEnumerable<Uri> GetModifiedFiles(IEnumerable<Uri> uris);

        /// <summary>
        /// Gets whether an item is in sync with the source control version</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns><c>True</c> if item is in sync with the source control version</returns>
        public abstract bool IsSynched(Uri uri);

        /// <summary>
        /// Gets whether an item is locked by the client or another user</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns><c>True</c> if item is locked</returns>
        public abstract bool IsLocked(Uri uri);

        /// <summary>
        /// Refreshes the status of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <remarks>It is much more efficient to refresh the status of many items at once
        /// by calling RefreshStatus(uris).</remarks>
        public abstract void RefreshStatus(Uri uri);

        /// <summary>
        /// Refreshes the status of given items</summary>
        /// <param name="uris">Enumeration of URIs representing paths to items</param>
        /// <remarks>It is much more efficient to refresh the status of many items at once
        /// by calling RefreshStatus(uris).</remarks>
        public abstract void RefreshStatus(IEnumerable<Uri> uris);

        /// <summary>
        /// Gets the revision logs for an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>Revision logs in a table, where each row is a revision record</returns>
        public abstract DataTable GetRevisionLog(Uri uri);

        /// <summary>
        /// Exports a file of the specified reversion to a designated location</summary>
        /// <param name="sourceUri">URI of file to be exported</param>
        /// <param name="destUri">URI of export location</param>
        /// <param name="revision">Source control revision</param>
        public abstract void Export(Uri sourceUri, Uri destUri, SourceControlRevision revision);

        #endregion

        /// <summary>
        /// Gets source control detailed information for the specified file</summary>
        /// <param name="uri">URI of the file under source control</param>
        /// <returns>Source control information for the specified file</returns>
        public abstract SourceControlFileInfo GetFileInfo(Uri uri);

        /// <summary>
        /// Gets source control status icon</summary>
        /// <param name="uri">File URI</param>
        /// <param name="status">Source control status</param>
        /// <returns>Source control status icon image</returns>
        public virtual Image GetSourceControlStatusIcon(Uri uri, SourceControlStatus status)
        {
            return null;
        }

        /// <summary>
        /// Raises the StatusChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnStatusChanged(SourceControlEventArgs e)
        {
            StatusChanged.Raise(this, e);
        }

        /// <summary>
        /// Raises the ConnectionChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnConnectionChanged(EventArgs e)
        {
            ConnectionChanged.Raise(this, e);
        }

        /// <summary>
        /// Raises the StatusChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnEnabledChanged(EventArgs e)
        {
            EnabledChanged.Raise(this, e);
        }

        private bool m_enabled;
    }
}
