//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Data;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Atf.Subversion
{
    /// <summary>
    /// Plugin implementing ISourceControlService, using Subversion</summary>
    [Export(typeof(ISourceControlService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(SubversionService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class SubversionService : SourceControlService, ICommandClient, IInitializable, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show errors to the user</summary>
        public bool ShowErrors
        {
            get { return m_showErrors; }
            set { m_showErrors = value; }
        }

        /// <summary>
        /// Imported ISettingsService</summary>
        [Import(AllowDefault = true)]
        public ISettingsService SettingsService
        {
            get { return m_settingsService; }
            set { m_settingsService = value; }
        }

        /// <summary>
        /// Imported IValidationContext.  
        /// If set, any SVN server file status queries that are requested during validation will be 
        /// deferred until validation has ended.  Deferred queries are processed as a single batch 
        /// call to SVN server, and will trigger StatusChanged events for each queried uri.  </summary>
        [Import(AllowDefault = true)]
        public IValidationContext ValidationContext
        {
            get { return m_validationContext; }
            set
            {
                if (m_validationContext != null)
                {
                    m_validationContext.Beginning -= OnValidationBeginning;
                    m_validationContext.Ended -= OnValidationEnded;
                }

                m_validationContext = value;

                if (m_validationContext != null)
                {
                    m_validationContext.Beginning += OnValidationBeginning;
                    m_validationContext.Ended += OnValidationEnded;
                }
            }
        }

        /// <summary> Gets or sets whether the source control server is connected</summary>
        public override bool IsConnected
        {
            get { return ClientInitialized(); }
            set { throw new InvalidOperationException("Connection status is readonly for Subversion"); }
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            LoadFileStatusIcons();
        }

        /// <summary>
        /// Set up setting service after imports have been satisfied</summary>
        public void OnImportsSatisfied()
        {
            if (!ClientInitialized() || m_settingsService == null)
                return;

            m_settingsService.Loading += m_settingsService_Loading;
            m_settingsService.Reloaded += m_settingsService_Reloaded;

            m_settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => Enabled,
                    "Enabled".Localize(),
                    null,
                    "Enabled".Localize()));

            m_settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this, 
                    () => ShowErrors, 
                    "Show Errors".Localize(),
                    null, 
                    "Show Errors".Localize()));
        }

        #endregion

        #region ISourceControlService Members

        /// <summary>
        /// Gets or sets a value indicating whether multiple users can check out the same file</summary>
//         public bool AllowMultipleCheckout
//         {
//             get { return false; } // for now, don't allow multiple checkout
//             set { }
//         }

        /// <summary>
        /// Gets or sets a value indicating whether checkin are permitted</summary>
//         public bool AllowCheckIn
//         {
//             get { return m_allowCheckIn; }
//             set { m_allowCheckIn = value; }
//         }

        /// <summary>
        /// Event that is raised after the status of a collection has changed</summary>
//         public event EventHandler<SourceControlEventArgs> StatusChanged;

        /// <summary>
        /// Refreshes cached status info for all items under the specified folder</summary>
        /// <param name="rootUri">The uri under which all cached file info will be refreshed</param>
        /// <param name="resetCacheFirst">If true, cache is cleared out before refreshing</param>
        /// <remarks>
        /// Using this call minimizes the number of queries to the source control server,
        /// by allowing large subtrees of files to be queried and cached at once</remarks>
        public override void UpdateCachedStatuses(Uri rootUri, bool resetCacheFirst)
        {
            if (!ClientInitialized())
                return;

            if (resetCacheFirst)
                m_infoCache.Clear();

            // Run 'svn list' on full tree of rootUri
            // (only lists files in repository, not unmanaged files, nor files marked for adding)
            if (!RunCommand("list", GetQuotedCanonicalPath(rootUri) + " --depth infinity -v"))
                return;

            // Create FileInfo entry for each repository item.
            // All folder paths end with a slash, which is reflected in FileInfo.Uri.  However retrieving 
            // from m_infoCache can be done with a path key does or does not include that ending slash.
            foreach (var line in m_svnOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // Reference to rootUri
                var info = new FileInfo(
                    line.EndsWith("./") ? rootUri : SvnStatusLineToAbsoluteUri(line, rootUri.LocalPath),
                    SourceControlStatus.Unknown);
                m_infoCache[info.Uri.LocalPath] = info;
            }

            // Run 'svn status' on full tree of rootUri
            // (lists both managed and unmanaged files, and their status)
            if (!RunCommand("status", GetQuotedCanonicalPath(rootUri) + " --depth infinity -u -v"))
                return;

            // Set m_infoCache statuses to values returned from svn status
            foreach (var line in m_svnOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // ignore non-file lines
                if (line.StartsWith("Status against revision:") || line.StartsWith("        >"))
                    continue;

                SourceControlStatus status;
                switch (line[0])
                {
                    case ' ':
                        status = SourceControlStatus.CheckedIn; break;
                    case 'A':
                        status = SourceControlStatus.Added; break;
                    case 'C':
                        status = SourceControlStatus.Unknown; break;
                    case 'D':
                        status = SourceControlStatus.Deleted; break;
                    case 'I':
                        status = SourceControlStatus.NotControlled; break;
                    case 'M':
                        status = SourceControlStatus.CheckedOut; break;
                    case 'R':
                        status = SourceControlStatus.CheckedOut; break;
                    case 'X':
                        status = SourceControlStatus.NotControlled; break;
                    case '?':
                        status = SourceControlStatus.NotControlled; break;
                    case '!':
                        status = SourceControlStatus.FileDoesNotExist; break;
                    case '~':
                        status = SourceControlStatus.Unknown; break;
                    default:
                        throw new Exception("Unhandled status character '" + line[0] + "' in svn status.");
                }

                // path is the last item on each line (but may contains spaces)
                // note that folder paths here do *not* have an ending slash (thanks SVN)
                var fields = line.Substring(9).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var numFields = fields.Length;
                var minFields = status != SourceControlStatus.NotControlled ? 4 : 1;
                if (numFields < minFields)
                    throw new Exception("Should be at least " + minFields + " fields, the last one being the path to the repository item");

                var i = minFields - 1;
                var path = fields[i];
                while (++i < numFields)
                    path += " " + fields[i];

                // add ending slash to folder paths that don't already have them
                Uri uri;
                if (Directory.Exists(path) && !path.EndsWith("\\") && !path.EndsWith("/"))
                    uri = new Uri(path + "\\");
                else
                    uri = new Uri(path);

                // apply the status to an existing FileInfo, or create a new one
                FileInfo info;
                if (!m_infoCache.TryGetValue(path, out info))
                {
                    info = new FileInfo(uri, status);
                    m_infoCache[path] = info;
                }
                else
                    info.Status = status;

                // folders that aren't controlled don't get their child files listed in 'svn status'.
                // if there are any cached FileInfos for such child files, mark them as not controlled as well
                // (required when a to-be-added folder is reverted, and its children were also marked for add)
                if (info.Status == SourceControlStatus.NotControlled && info.Uri.LocalPath.EndsWith("\\"))
                {
                    var childInfos = m_infoCache.Values.Where(fileInfo => !uri.Equals(fileInfo.Uri) && fileInfo.Uri.LocalPath.StartsWith(uri.LocalPath)).ToArray();
                    childInfos.ForEach(childInfo => childInfo.Status = SourceControlStatus.NotControlled);
                }
            }
        }

        /// <summary>
        /// Send StatusChanged events for all specified URIs</summary>
        /// <param name="uris">The URIs for which StatusChanged events should be fired</param>
        /// <remarks>
        /// Cached statuses are broadcast if available. For all others, the source control
        /// server is queried</remarks>
        public override void BroadcastStatuses(IEnumerable<Uri> uris)
        {
            var refreshList = new List<Uri>();
            foreach (var uri in uris)
            {
                FileInfo info;
                if (m_infoCache.TryGetValue(uri.LocalPath.TrimEnd(new[] { '\\', '/' }), out info))
                {
                    OnStatusChanged(new SourceControlEventArgs(uri, info.Status));
                    continue;
                }
                refreshList.Add(uri);
            }

            if (refreshList.Any())
                RefreshStatus(refreshList);
        }

        /// <summary>
        /// Adds an item to source control</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        public override void Add(Uri uri)
        {
            CheckUri(uri);
            if (ClientInitialized())
            {
                if (RunCommand("add", GetQuotedCanonicalPath(uri)))
                    RefreshStatus(uri);
            }
        }

        /// <summary>
        /// Deletes an item from source control</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        public override void Delete(Uri uri)
        {
            CheckUri(uri);
            if (ClientInitialized())
            {
                if (RunCommand("delete", GetQuotedCanonicalPath(uri)))
                {
                    SetStatus(uri, SourceControlStatus.Deleted);
                }
            }
        }

        /// <summary>
        /// Checks in the given items</summary>
        /// <param name="uriEnum">URIs representing the path to items</param>
        /// <param name="description">Check-in description</param>
        public override void CheckIn(IEnumerable<Uri> uriEnum, string description)
        {
            var uris = uriEnum.ToArray();
            if (!uris.Any())
                return;

            if (string.IsNullOrEmpty(description))
                description = "No Comment!";

            if (!ClientInitialized())
                return;

            try
            {
                var pathsBuilder = new StringBuilder();
                pathsBuilder.Append("\"");
                pathsBuilder.Append(description);
                pathsBuilder.Append("\"");

                foreach (var uri in uris)
                {
                    CheckUri(uri);
                    pathsBuilder.Append(" ");
                    pathsBuilder.Append(GetQuotedCanonicalPath(uri));
                }

                if (RunCommand("commit -m ", pathsBuilder.ToString()))
                    RefreshStatus(uris);

            }
            catch (Exception e)
            {
                ShowErrorMessage(e.Message);
            }
        }

        /// <summary>
        /// Checks out an item</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        public override void CheckOut(Uri uri)
        {
            CheckUri(uri);

            FileInfo info = GetInfo(uri,false);
            if (info.IsLocked)
            {
                throw new InvalidTransactionException("The document is locked by another user");
            }

            if (ClientInitialized())
            {
                //should we try to lock the file? do nothing for now
                //string url = uri.OriginalString; // TODO figure out what this url is
                //string path = GetCanonicalPath(uri);
            }
        }

        /// <summary>
        /// Gets the latest version of an item</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        public override void GetLatestVersion(Uri uri)
        {
            CheckUri(uri);
            if (ClientInitialized())
            {
                RunCommand("update", GetQuotedCanonicalPath(uri));
                RefreshStatus(uri);
            }
        }

        /// <summary>
        /// Reverts an item</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        public override void Revert(Uri uri)
        {
            CheckUri(uri);

            SourceControlStatus oldStatus = GetInfo(uri, false).Status;
            if (ClientInitialized())
            {
                RunCommand("revert", GetQuotedCanonicalPath(uri));
                SourceControlStatus newStatus = GetInfo(uri, true).Status;
                if (oldStatus != newStatus)
                {
                    OnStatusChanged(new SourceControlEventArgs(uri, newStatus));
                }
            }
        }

        /// <summary>
        /// Gets the source control status of an item</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        /// <returns>Status of item</returns>
        public override SourceControlStatus GetStatus(Uri uri)
        {
            CheckUri(uri);
            FileInfo info = GetInfo(uri, false);
            return info.Status;
        }

        /// <summary>
        /// Gets the source control status of each item.</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <returns>Status of each item, in the same order as the given URIs.</returns>
        public override SourceControlStatus[] GetStatus(IEnumerable<Uri> uris)
        {
            foreach (Uri uri in uris)
                CheckUri(uri);

            List<FileInfo> info = GetInfo(uris, false);
            SourceControlStatus[] result = GetStatusArray(info);

            return result;
        }

        /// <summary>
        /// Gets the source control status of all items under the folder</summary>
        /// <param name="uri">Uri, representing the folder path</param>
        /// <returns> false if not supported </returns>
        public override bool GetFolderStatus(Uri uri)
        {
            ClearCache();
            var fileName = GetCanonicalPath(uri);
            var directoryName = Path.GetDirectoryName(fileName);
            if (!RunCommand("status -u -v", GetQuotedPath(directoryName)))
                return true;

            GetFolderStatusRecursive(directoryName);
            return true;
        }

        private void GetFolderStatusRecursive(string directoryName)
        {

            DirectoryInfo dir = new DirectoryInfo(directoryName);
            FileSystemInfo[] items = dir.GetFileSystemInfos();
            foreach (FileSystemInfo item in items)
            {
//                     if (item is DirectoryInfo)
//                     {
//                         //Console.WriteLine("DIRECTORY: " + ((DirectoryInfo)item).FullName);
//                         //GetAllDirFilesRecurse(((DirectoryInfo)item).FullName);
//                     }
//                     if (item is System.IO.FileInfo)
                {
                    SourceControlStatus initialStatus = SourceControlStatus.Unknown;
                    FileInfo info;
                    string filename = item.FullName;

                    if (!m_infoCache.TryGetValue(filename, out info))
                    {
                        Uri fileUri = new Uri(filename + ((item is DirectoryInfo) ? "\\" : ""));
                        info = new FileInfo(fileUri, initialStatus);
                        m_infoCache[filename] = info;
                        ParseStatus(filename);
                    }

                    if (item is DirectoryInfo)
                        GetFolderStatusRecursive(filename);

                    //Console.WriteLine("FILE: " + ((FileInfo)item).FullName);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating if an item is in sync with the source control version</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        /// <returns>true, if item is in sync with the source control version</returns>
        public override bool IsSynched(Uri uri)
        {
            CheckUri(uri);
            FileInfo info = GetInfo(uri, false);
            return info.IsSynced;
        }

        /// <summary>
        /// From given items, gets those that are different from the revision in the depot.</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <returns>item that are different from the depot.</returns>
        public override IEnumerable<Uri> GetModifiedFiles(IEnumerable<Uri> uris)
        {
            GetInfo(uris, false);
            foreach (Uri uri in uris)
            {
                FileInfo info = GetInfo(uri, false);
                if (info.IsModified)
                    yield return uri;
            }

        }


        /// <summary>
        /// Gets a value indicating if an item is locked by the client or another user</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        /// <returns>true, if item is locked</returns>
        public override bool IsLocked(Uri uri)
        {
            CheckUri(uri);
            FileInfo info = GetInfo(uri, false);
            return info.IsLocked;
        }

        /// <summary>
        /// Refreshes the status of an item</summary>
        /// <param name="uri">URI representing the path to item refreshed</param>
        public override void RefreshStatus(Uri uri)
        {
            FileInfo info = GetInfo(uri, true);
        }

        /// <summary>
        /// Refreshes the status of these items</summary>
        /// <param name="uris">URIs representing the paths to items refreshed</param>
        public override void RefreshStatus(IEnumerable<Uri> uris)
        {
            GetInfo(uris, true);
        }

        /// <summary>
        /// Get the revision history of the file</summary>
        /// <param name="uri">File for which to obtain revision history</param>
        /// <returns>DataTable describing revision history</returns>
        public override DataTable GetRevisionLog(Uri uri)
        {

            DataTable svnDataTable = new DataTable("Subversion Reversions");

            //string[] targets = { uri.ToString() };
            RunCommand("log -r HEAD:0", GetQuotedCanonicalPath(uri)); // search for log from revision HEAD to 0

            // Get a pair of DataColumn and  DataRow reference
            DataColumn svnDataColumn;
            DataRow svnDataRow;

            // create and add the first column
            svnDataColumn = new DataColumn();
            svnDataColumn.DataType = Type.GetType("System.Int32");
            svnDataColumn.ColumnName = "revision";
            svnDataColumn.ReadOnly = true;
            svnDataColumn.Unique = true;
            svnDataTable.Columns.Add(svnDataColumn);

            // create and add the second column
            svnDataColumn = new DataColumn();
            svnDataColumn.DataType = Type.GetType("System.String");
            svnDataColumn.ColumnName = "author";
            svnDataTable.Columns.Add(svnDataColumn);

            // create and add the third column
            svnDataColumn = new DataColumn();
            svnDataColumn.DataType = Type.GetType("System.String");
            svnDataColumn.ColumnName = "date";
            svnDataTable.Columns.Add(svnDataColumn);

            // create and add the fourth column
            svnDataColumn = new DataColumn();
            svnDataColumn.DataType = Type.GetType("System.String");
            svnDataColumn.ColumnName = "description";
            svnDataTable.Columns.Add(svnDataColumn);

            StringReader strReader = new StringReader(m_svnOutput);
            string line = strReader.ReadLine();
            while (line != null)
            {
                // first line are fields, 2nd line is the message
                string[] tokens = line.Split(new[] { '|' });
                if (tokens.Length >= 3)
                {
                    svnDataRow = svnDataTable.NewRow();
                    string rev = tokens[0].Substring(1);
                    decimal revVal;
                    try
                    {
                        revVal = Decimal.Parse(rev);
                    }
                    catch
                    {
                        revVal = 0;
                    }
                    svnDataRow["revision"] = revVal;
                    svnDataRow["author"] = tokens[1];
                    svnDataRow["date"] = tokens[2];
                    line = strReader.ReadLine();
                    line = strReader.ReadLine();
                    svnDataRow["description"] = line;
                    svnDataTable.Rows.Add(svnDataRow);
                }
                line = strReader.ReadLine();

            }


            return svnDataTable;
        }

        /// <summary>
        /// Export a file of the specified reversion to a designated location</summary>
        /// <param name="sourceUri">Path of file to export</param>
        /// <param name="destUri">Path of file export location</param>
        /// <param name="revision">Type of file revision</param>
        public override void Export(Uri sourceUri, Uri destUri, SourceControlRevision revision)
        {
            if (ClientInitialized())
            {
                string cmd = "export";
                switch (revision.Kind)
                {
                    case SourceControlRevisionKind.Number:
                        cmd += " -r";
                        cmd += revision.Number.ToString();
                        break;
                    case SourceControlRevisionKind.Unspecified:
                        break;
                    case SourceControlRevisionKind.Base:
                        cmd += " -r BASE";
                        break;
                    case SourceControlRevisionKind.Head:
                        cmd += " -r HEAD";
                        break;
                    case SourceControlRevisionKind.Working:
                        break;
                    case SourceControlRevisionKind.Date:
                        //input date specifiers wrapped in curly braces { }
                        cmd += " -r {";
                        cmd += revision.Date.ToString();
                        cmd += "}";
                        break;
                    case SourceControlRevisionKind.ChangeList:
                        throw new NotSupportedException("Changelist not supported in Subversion ");
                }
                RunCommand(cmd, GetQuotedCanonicalPath(sourceUri) + " " + GetQuotedCanonicalPath(destUri));
            }
        }

        /// <summary>
        /// Gets source control detailed information for the specified file</summary>
        /// <param name="uri">URI of the file under source control</param>
        /// <returns>Source control information for the specified file</returns>
        public override SourceControlFileInfo GetFileInfo(Uri uri)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICommandClient

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns><c>True</c> if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            if (commandTag is Command)
                return Enabled;

            return false;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag is Command)
            {
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

        /// <summary>
        /// Turn on caching of values assigned to select properties. Values are applied later, 
        /// in the right order, when m_settingsService_Reloaded() is called.</summary>
        /// <param name="sender">Settings service</param>
        /// <param name="e">Event data</param>
        protected void m_settingsService_Loading(object sender, EventArgs e)
        {
            m_settingsLoading = true;
            m_requestedShowErrors = ShowErrors;
        }

        /// <summary>
        /// Assigns cached property values to their respective properties, in the correct order.  It
        /// then disables property caching.</summary>
        /// <param name="sender">Settings service</param>
        /// <param name="e">Event data</param>
        protected void m_settingsService_Reloaded(object sender, EventArgs e)
        {
            if (!m_settingsLoading)
                return;

            m_settingsLoading = false;
            ShowErrors = m_requestedShowErrors;
        }

//         /// <summary>
//         /// Raises the StatusChanged event</summary>
//         /// <param name="e">Event args</param>
//         protected override void OnStatusChanged(SourceControlEventArgs e)
//         {
//             base.OnStatusChanged(e);
//         }

        /// <summary>
        /// Sets the status for the given URI</summary>
        /// <param name="uri">URI of item</param>
        /// <param name="status">New status of item</param>
        protected void SetStatus(Uri uri, SourceControlStatus status)
        {
            FileInfo info = GetInfo(uri, false);
            if (status != info.Status)
            {
                info.Status = status;
                OnStatusChanged(new SourceControlEventArgs(uri, status));
            }
        }

        /// <summary>
        /// Gets the canonical path for the URI, the key into the status cache</summary>
        /// <param name="uri">URI of item</param>
        /// <returns>Canonical path for the URI</returns>
        protected static string GetCanonicalPath(Uri uri)
        {
            string result = uri.AbsolutePath;
            result = Uri.UnescapeDataString(result);
            return GetCanonicalPath(result);
        }

        private static string GetQuotedPath(string path)
        {
            return "\"" + path + "\"";
        }


        /// <summary>
        /// Gets the quoted canonical path.</summary>
        /// <param name="uri">The URI</param>
        /// <returns>Quoted canonical path</returns>
        protected static string GetQuotedCanonicalPath(Uri uri)
        {
            return GetQuotedPath(GetCanonicalPath(uri).TrimEnd(new[] { '\\', '/' }));
        }

        /// <summary>
        /// Shows the error message to the user</summary>
        /// <param name="message">Error message</param>
        protected void ShowErrorMessage(string message)
        {
            if (m_showErrors)
                MessageBox.Show(message);
        }

        private FileInfo GetInfo(Uri uri, bool refreshCache)
        {
            string path = GetCanonicalPath(uri);
            if (!refreshCache)
            {
                FileInfo info;
                if (m_infoCache.TryGetValue(path, out info))
                    return info;
            }
            List<FileInfo> infoList = GetInfo(new[] { uri }, refreshCache);
            return infoList[0];
        }

        private List<FileInfo> GetInfo(IEnumerable<Uri> uris, bool refreshCache)
        {
            // create result list using cache where possible, and build a list of unknown paths
            List<FileInfo> result = new List<FileInfo>();
            List<string> refreshPaths = new List<string>();
            foreach (Uri uri in uris)
            {
                string path = GetCanonicalPath(uri);
                SourceControlStatus initialStatus =
                    File.Exists(path) ?
                    SourceControlStatus.Unknown :
                    SourceControlStatus.FileDoesNotExist;

                FileInfo info;
                if (!m_infoCache.TryGetValue(path, out info))
                {
                    info = new FileInfo(uri, initialStatus);
                    m_infoCache[path] = info;

                    refreshPaths.Add(path);
                }
                else if (refreshCache)
                {
                    info.Status = initialStatus;
                    refreshPaths.Add(path);
                }

                result.Add(info);
            }

            if (refreshPaths.Count > 0)
            {
                // remember old status to avoid spurious StatusChanged events
                SourceControlStatus[] oldStatus = GetStatusArray(result);

                if (!RefreshInfoSuspended)
                {
                    // get info for unknown paths from Subversion
                    RefreshInfo(refreshPaths.ToArray());
                }
                else
                    m_refreshInfos.AddRange(result.Except(m_refreshInfos));

                for (int i = 0; i < result.Count; i++)
                {
                    FileInfo info = result[i];
                    if (oldStatus[i] != info.Status)
                        OnStatusChanged(new SourceControlEventArgs(info.Uri, info.Status));
                }
            }

            return result;
        }

        //private static int GetBeginningOfLine(string text, int startPointOfMatch)
        //{
        //    if (startPointOfMatch > 0)
        //    {
        //        --startPointOfMatch;
        //    }
        //    if (startPointOfMatch >= 0 && startPointOfMatch < text.Length)
        //    {
        //        // Move to the left until the first '\n char is found
        //        for (int index = startPointOfMatch; index >= 0; index--)
        //        {
        //            if (text[index] == '\n')
        //            {
        //                return (index + 1);
        //            }
        //        }
        //        return (0);
        //    }
        //    return (startPointOfMatch);
        //}

        //private static int GetEndOfLine(string text, int endPointOfMatch)
        //{
        //    if (endPointOfMatch >= 0 && endPointOfMatch < text.Length)
        //    {
        //        // Move to the right until the first '\n char is found
        //        for (int index = endPointOfMatch; index < text.Length; index++)
        //        {
        //            if (text[index] == '\n')
        //            {
        //                return (index);
        //            }
        //        }
        //        return (text.Length);
        //    }
        //    return (endPointOfMatch);
        //}

        private void ParseStatus(string path)
        {
            FileInfo info = m_infoCache[path];
            info.Status = SourceControlStatus.Unknown;
            info.IsLocked = false;
            info.IsSynced = true;

            if (!string.IsNullOrEmpty(m_svnError))
            {
                if (m_svnError.StartsWith("svn: warning: W155007"))
                    info.Status = SourceControlStatus.NotControlled;
                return;
            }

            // split the output into lines
            string[] lines = m_svnOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!line.EndsWith(path.TrimEnd(new [] {'\\', '/'})))               // skip line
                    continue;

                string trimed = line.Replace(path, ""); // cut out the path
                trimed = trimed.Trim();                 // trim line

                if (trimed.StartsWith("*")) // a newer version exist on server
                {
                    info.Status = SourceControlStatus.CheckedIn;
                    info.IsSynced = false;
                    break;
                }

                if (trimed.StartsWith("?")) //Item is not under version control
                {
                    info.Status = SourceControlStatus.NotControlled;
                    info.IsSynced = false;
                    break;
                }

                if (trimed.StartsWith("A")) //Item is scheduled for Addition.
                {
                    info.Status = SourceControlStatus.Added;
                    break;
                }
                else if (trimed.StartsWith("D")) //Item is scheduled for Deletion
                {
                    info.Status = SourceControlStatus.Deleted;
                    break;
                }
                else if (trimed.StartsWith("M")) //Item has been modified.
                {
                    info.IsModified = true;
                    if (trimed.Contains("*"))
                    {
                        info.Status = SourceControlStatus.CheckedIn;
                        info.IsSynced = false;
                        break;
                    }
                    else
                    {
                        info.Status = SourceControlStatus.CheckedOut;
                        break;
                    }
                }
                else if (trimed.StartsWith("C")) //The contents of the item conflict with updates received from the repository.
                {
                    info.Status = SourceControlStatus.CheckedOut;
                    info.IsSynced = false;
                    break;
                }

                if (trimed.StartsWith("K")) //locked by this working copy
                {
                    info.Status = SourceControlStatus.CheckedIn;
                    info.IsLocked = true;
                    break;
                }
                else if (trimed.StartsWith("O")) //locked by some other working copy
                {
                    info.Status = SourceControlStatus.CheckedIn;
                    info.IsLocked = true;
                    break;
                }
                else if (trimed.StartsWith("T")) // locked by you but stolen
                {
                    info.Status = SourceControlStatus.CheckedIn;
                    info.IsLocked = true;
                    break;
                }

                if (char.IsNumber(trimed[0]))
                {
                    info.Status = SourceControlStatus.CheckedIn;
                    break;
                }
            }
        }

        private void RefreshInfo(string[] paths)
        {
            if (!ClientInitialized())
                return;

            StringBuilder pathsBuilder = new StringBuilder();
            foreach (string path in GetCommonRootPaths(paths))
            {
                pathsBuilder.Append(" \"");
                pathsBuilder.Append(path);
                pathsBuilder.Append("\"");
            }

            if (RunCommand("status -u -v", pathsBuilder.ToString()))
            {
                Dictionary<string, SourceControlStatus> statusDict =
                    new Dictionary<string, SourceControlStatus>(StringComparer.InvariantCultureIgnoreCase);
                // scan each output record

                // analyze result in m_svnOutput
                foreach (string path in paths)
                {
                    ParseStatus(path);
                }
            }
        }

        private void Uncache(Uri uri)
        {
            string path = GetCanonicalPath(uri);
            m_infoCache.Remove(path);
        }
        private void ClearCache()
        {
            m_infoCache.Clear();
        }

        /// <summary>
        /// Checks the URI validity.</summary>
        /// <param name="uri">The URI to check.</param>
        /// <exception cref="System.ArgumentNullException">Null URI</exception>
        protected static void CheckUri(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
        }

        /// <summary>
        /// Checks if client is initialized.</summary>
        /// <returns><c>True</c> if client is initialized</returns>
        protected bool ClientInitialized()
        {
            if (!m_connectionInitialized)
            {
                m_svnAvailable = RunCommand("--version", string.Empty);
                m_connectionInitialized = m_svnAvailable;
            }

            return m_connectionInitialized;
        }
        /*
        private void client_Notification(object sender, NotificationEventArgs args)
        {
            // this is for debug purposes
            Outputs.WriteLine(OutputMessageType.Warning, args.Path);
        }*/

        private string[] GetCommonRootPaths(string[] paths)
        {
            var rootPaths = new List<Uri>();
            foreach (var path in paths.Select(p => p + "\\"))
            {
                var uri = new Uri(path);
                if (!rootPaths.Any())
                {
                    rootPaths.Add(uri);
                    continue;
                }
                foreach (var rootPath in rootPaths.ToArray())
                {
                    if (rootPath.IsBaseOf(uri))
                        continue;
                    if (uri.IsBaseOf(rootPath))
                    {
                        rootPaths.Remove(rootPath);
                        rootPaths.Add(uri);
                        continue;
                    }

                    var rootSegments = rootPath.Segments;
                    var uriSegments = uri.Segments;
                    if (rootSegments.First().Equals(uriSegments.First()))
                    {
                        var rootSegCount = rootSegments.Length;
                        var uriSegCount = uriSegments.Length;
                        var maxSegs = (rootSegCount <= uriSegCount) ? rootSegCount : uriSegCount;
                        var basePath = "";
                        for (var i = 1; i < maxSegs; i++)
                        {
                            var rootSeg = rootSegments[i];
                            if (!rootSeg.Equals(uriSegments[i]))
                                break;
                            basePath += rootSeg;
                        }
                        if (!string.IsNullOrEmpty(basePath))
                        {
                            rootPaths.Remove(rootPath);
                            rootPaths.Add(new Uri(basePath));
                            continue;
                        }
                    }
                    rootPaths.Add(uri);
                }
            }

            return rootPaths.Select(uri => uri.LocalPath.TrimEnd(new[] { '\\', '/' })).ToArray();
        }

        private static SourceControlStatus[] GetStatusArray(List<FileInfo> info)
        {
            SourceControlStatus[] result = new SourceControlStatus[info.Count];
            for (int i = 0; i < info.Count; i++)
                result[i] = info[i].Status;
            return result;
        }

        /// <summary>
        /// Class to encapsulate file information.</summary>
        protected class FileInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FileInfo"/> class.</summary>
            /// <param name="uri">URI of file</param>
            /// <param name="status">File source control status as SourceControlStatus</param>
            public FileInfo(Uri uri, SourceControlStatus status)
            {
                Uri = uri;
                Status = status;
            }

            /// <summary>
            /// File URI.</summary>
            public Uri Uri;
            /// <summary>
            /// File source control status as SourceControlStatus.</summary>
            public SourceControlStatus Status = SourceControlStatus.Unknown;
            //public int HeadRevision;
            //public int Revision;
            /// <summary>
            /// Whether file is synced.</summary>
            public bool IsSynced = false;
            /// <summary>
            /// Whether file is locked.</summary>
            public bool IsLocked = false;
            /// <summary>
            /// Whether file is modified.</summary>
            public bool IsModified = false;
        }




        /// <summary>
        /// Runs the given command using svn command line client</summary>
        /// <param name="cmd">Command string</param>
        /// <param name="path">Path parameter for command</param>
        /// <returns>true, if command was run successfully</returns>
        protected bool RunCommand(string cmd, string path)
        {
            string arguments = cmd + " " + path;
            ProcessStartInfo oInfo =
               new ProcessStartInfo("svn.exe", arguments);
            oInfo.UseShellExecute = false;
            oInfo.ErrorDialog = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;
            try
            {
                Process p = Process.Start(oInfo);
                StreamReader oReader = p.StandardOutput;
                m_svnOutput = oReader.ReadToEnd();
                oReader.Close();
                StreamReader eReader = p.StandardError;
                m_svnError = eReader.ReadToEnd();
                eReader.Close();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message);
                Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Gets the canonical version of the given path, the key into the status cache</summary>
        /// <param name="path">Path</param>
        /// <returns>canonical version of the given path</returns>
        protected static string GetCanonicalPath(string path)
        {
            path = Path.GetFullPath(path);
            string drive = path.Substring(0, 2);
            StringBuilder sb = new StringBuilder(256);
            Kernel32.QueryDosDeviceW(drive, sb, 256);
            string device = sb.ToString();

            const string cSubstDrivePrefix = @"\??\";
            if (device.StartsWith(cSubstDrivePrefix))
            {
                path = device.Substring(cSubstDrivePrefix.Length) + path.Substring(2);
            }

            return path;
        }

        private bool RefreshInfoSuspended { get; set; }

        private void OnValidationBeginning(object sender, EventArgs eventArgs)
        {
            if (RefreshInfoSuspended)
                return;
            RefreshInfoSuspended = true;
        }

        private void OnValidationEnded(object sender, EventArgs eventArgs)
        {
            if (!RefreshInfoSuspended)
                return;

            RefreshInfoSuspended = false;
            if (!m_refreshInfos.Any())
                return;

            var refreshInfos = new List<FileInfo>(m_refreshInfos);
            m_refreshInfos.Clear();

            // remember old status to avoid spurious StatusChanged events
            var oldStatus = GetStatusArray(refreshInfos);

            // get info from Subversion
            RefreshInfo(refreshInfos.Select(i => i.Uri.LocalPath).ToArray());

            for (var i = 0; i < refreshInfos.Count; i++)
            {
                var info = refreshInfos[i];
                if (oldStatus[i] != info.Status)
                    OnStatusChanged(new SourceControlEventArgs(info.Uri, info.Status));
            }
        }

        private Uri SvnStatusLineToAbsoluteUri(string line, string basePath)
        {
            var fields = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var isDirectory = fields.Last().EndsWith("/");

            var firstPathField = isDirectory ? 6 : 7;
            var lastPathField = fields.Length;
            if (lastPathField < firstPathField)
                throw new Exception("Unexpected lack of fields in line");

            var i = firstPathField - 1;
            var path = fields[i];
            while (++i < lastPathField)
                path += " " + fields[i];

            return new Uri(basePath + "/" + path);
        }

        private Image[] m_statusImages;
        private const int NumFileStatusIcons = 3;
        private const int SVNFileAdd = 0;
        private const int SVNFileSync = 1;
        private const int SVNModified = 2;
        //private const int SVNFileEditHead = 3;
        //private const int SVNFileWs = 4;
        //private const int SVNFileLock = 5;
        //private const int SVNFileLockOther =6;

        private void LoadFileStatusIcons()
        {
            m_statusImages = new Image[NumFileStatusIcons];
            m_statusImages[SVNFileAdd] = GdiUtil.GetImage("Atf.Subversion.Resources.actionadded.ico");
            m_statusImages[SVNFileSync] = GdiUtil.GetImage("Atf.Subversion.Resources.normal.png");
            m_statusImages[SVNModified] = GdiUtil.GetImage("Atf.Subversion.Resources.actionmodified.ico");
            //m_statusImages[SVNFileEditHead] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_edit_head.png");
            //m_statusImages[SVNFileWs] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_ws.png");
            //m_statusImages[SVNFileLock] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_lock.png");
            //m_statusImages[SVNFileLockOther] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_lock_other.png");
        }

        /// <summary>
        /// Gets source control status icon</summary>
        /// <param name="uri">File URI</param>
        /// <param name="status">Source control status</param>
        /// <returns>Source control status icon image</returns>
        public override Image GetSourceControlStatusIcon(Uri uri, SourceControlStatus status)
        {
            Image result = null;
            switch (status)
            {
                case SourceControlStatus.Added:
                    result = m_statusImages[SVNFileAdd];
                    break;
                case SourceControlStatus.CheckedIn:
                    {
                        result = m_statusImages[SVNFileSync];
                        break;
                    }
                case SourceControlStatus.CheckedOut:
                    {
                        result = m_statusImages[SVNModified];
                        break;
                    }
                case SourceControlStatus.NotControlled:
                    // fall through to retun null
                    break;
            }

            return result;
        }

        /// <summary>
        /// Information cache field for file managed by Subversion.</summary>
        protected InfoCache<FileInfo> m_infoCache =
            new InfoCache<FileInfo>(StringComparer.InvariantCultureIgnoreCase);

        private ISettingsService m_settingsService;
        private IValidationContext m_validationContext;
        private bool m_settingsLoading;              // when true, cache off assignment to select properties
        private bool m_requestedShowErrors;          // cached 'ShowErrors' property value
        private bool m_connectionInitialized;
        //private bool m_allowMultipleCheckout;
//         private bool m_allowCheckIn = true;
        /// <summary>
        /// String for svn command line client output.</summary>
        protected string m_svnOutput;
        private string m_svnError;
        private bool m_showErrors = true;
        private bool m_svnAvailable = false;
        private readonly List<FileInfo> m_refreshInfos = new List<FileInfo>();
      
    }

    /// <summary>
    /// Cache for information on files in Subversion</summary>
    /// <typeparam name="TValue">File information</typeparam>
    public class InfoCache<TValue>
    {
        /// <summary>
        /// Constructor with comparer</summary>
        /// <param name="comparer">Comparer function</param>
        public InfoCache(IEqualityComparer<string> comparer)
        {
            m_infoCache = new Dictionary<string, TValue>(comparer);
        }

        /// <summary>
        /// Attempt to get file information for file</summary>
        /// <param name="key">File path</param>
        /// <param name="value">File information retrieved</param>
        /// <returns><c>True</c> if file path found</returns>
        public bool TryGetValue(string key, out TValue value)
        {
            return m_infoCache.TryGetValue(NormalizeKey(key), out value);
        }

        /// <summary>
        /// Retrieve file information for path</summary>
        /// <param name="key">File path</param>
        /// <returns>File information retrieved</returns>
        public TValue this[string key]
        {
            get { return m_infoCache[NormalizeKey(key)]; }
            set { m_infoCache[NormalizeKey(key)] = value; }
        }

        /// <summary>
        /// Remove File information for given path</summary>
        /// <param name="key">Path to remove file info for</param>
        public void Remove(string key)
        {
            m_infoCache.Remove(NormalizeKey(key));
        }

        /// <summary>
        /// Clear the cache</summary>
        public void Clear()
        {
            m_infoCache.Clear();
        }

        /// <summary>
        /// List the values currently in the cache</summary>
        public IEnumerable<TValue> Values { get { return m_infoCache.Values; } }

        private string NormalizeKey(string key)
        {
            if (key.EndsWith("/") || key.EndsWith("\\"))
                key = key.Substring(0, key.Length - 1);
            return key;
        }

        private readonly Dictionary<string, TValue> m_infoCache;
    }
}
