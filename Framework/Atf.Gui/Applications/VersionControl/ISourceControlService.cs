//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Data;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for source control services</summary>
    public interface ISourceControlService
    {
        /// <summary>
        /// Gets or sets whether multiple users can check out the same file</summary>
        bool AllowMultipleCheckout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether checkins are permitted</summary>
        bool AllowCheckIn
        {
            get;
            set;
        }

        /// <summary>
        /// Event that is raised after an item's status changes</summary>
        event EventHandler<SourceControlEventArgs> StatusChanged;

        /// <summary>
        /// Adds an item to source control</summary>
        /// <param name="uri">URI representing the path to item</param>
        void Add(Uri uri);

        /// <summary>
        /// Deletes an item from source control</summary>
        /// <param name="uri">URI representing the path to item</param>
        void Delete(Uri uri);

        /// <summary>
        /// Checks in the given items</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <param name="description">Check-in description</param>
        void CheckIn(IEnumerable<Uri> uris, string description);

        /// <summary>
        /// Checks out an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        void CheckOut(Uri uri);

        /// <summary>
        /// Gets the latest version of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        void GetLatestVersion(Uri uri);

        /// <summary>
        /// Reverts an item, i.e., undoes any checkout and gets the latest version</summary>
        /// <param name="uri">URI representing the path to item</param>
        void Revert(Uri uri);

        /// <summary>
        /// Gets the source control status of all items under the folder</summary>
        /// <param name="uri">URI representing the folder path</param>
        /// <returns>False if not supported </returns>
        bool GetFolderStatus(Uri uri);

        /// <summary>
        /// Gets the source control status of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>Status of item</returns>
        /// <remarks>It is much more efficient to get the status of many items at once
        /// by calling GetStatus(uris).</remarks>
        SourceControlStatus GetStatus(Uri uri);

        /// <summary>
        /// Gets the source control status of each item</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <returns>Status of each item, in the same order as given</returns>
        SourceControlStatus[] GetStatus(IEnumerable<Uri> uris);

        /// <summary>
        /// From given items, gets those that are different from the revision in the depot</summary>
        /// <param name="uris">URIs representing paths to items</param>
        /// <returns>URIs of items that are different from the depot</returns>
        IEnumerable<Uri> GetModifiedFiles(IEnumerable<Uri> uris);

        /// <summary>
        /// Gets whether an item is in sync with the source control version</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>True iff item is in sync with the source control version</returns>
        bool IsSynched(Uri uri);

        /// <summary>
        /// Gets whether an item is locked by the client or another user</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>True iff item is locked</returns>
        bool IsLocked(Uri uri);

        /// <summary>
        /// Refreshes the status of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <remarks>It is much more efficient to refresh the status of many items at once
        /// by calling RefreshStatus(uris).</remarks>
        void RefreshStatus(Uri uri);

        /// <summary>
        /// Refreshes the status of given items</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        void RefreshStatus(IEnumerable<Uri> uris);

        /// <summary>
        /// Get the revision logs for an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>Returns revision history logs in a table, where each row is a revision record</returns>
        DataTable GetRevisionLog(Uri uri);

        /// <summary>
        /// Exports a file of the specified revision to a designated location</summary>
        /// <param name="sourceUri">Source file URI</param>
        /// <param name="destUri">Designated location URI</param>
        /// <param name="revision">Source control revison of file</param>
        void Export(Uri sourceUri, Uri destUri, SourceControlRevision revision);
    }
}
