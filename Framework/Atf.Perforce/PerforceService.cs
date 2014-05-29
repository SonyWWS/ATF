//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Perforce.P4;

using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Component implementing ISourceControlService, using the Perforce Client API.
    /// Considering using Outputs, OutputService, and ErrorDialogService for reporting
    /// errors and warnings.</summary>
    [Export(typeof(ISourceControlService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(PerforceService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PerforceService : SourceControlService, ICommandClient, IInitializable, IDisposable
    {
        /// <summary>
        /// Constructor</summary>
        [ImportingConstructor]
        public PerforceService()
        {
            CacheExpireTimeInSeconds = 60;
            m_connectionManager = new ConnectionManager();
            m_connectionManager.ConnectionChanged += connection_ConnectionChanged;
            m_connectionManager.LoginCanceled += connectionManager_LoginCanceled;
        }

        /// <summary> Gets or sets whether the source control server is connected</summary>
        public override bool IsConnected
        {
            get { return m_connectionManager.IsConnected; }
            set { throw new InvalidOperationException("Connection status is readonly for Perforce"); }
        }

        /// <summary>
        /// Gets or sets the settings service used to persist user settings. Is optional and
        /// may be null.</summary>
        [Import(AllowDefault = true)]
        public ISettingsService SettingsService
        {
            get { return m_settingsService; }
            set { m_settingsService = value; }
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by performing common initialization
        /// and setting up Settings Service.</summary>
        public void Initialize()
        {
            LoadFileStatusIcons();

            CreateBackgroundThread();

            if (m_settingsService == null)
                return;

            m_settingsService.Loading += m_settingsService_Loading;
            m_settingsService.Reloaded += m_settingsService_Reloaded;

            m_settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => ConnectionHistory,
                    "Perforce Connection History".Localize(),
                    null,
                    "Perforce Connection History".Localize()));

            m_settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => DefaultConnection,
                    "Perforce Default Connection".Localize(),
                    null,
                    "Perforce Default Connection".Localize()));

            m_settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => Enabled,
                    "Enable or Disable Perforce Service".Localize(),
                    null,
                    "Enable or Disable Perforce Service".Localize()));

            m_settingsService.RegisterSettings(
                this,
                new BoundPropertyDescriptor(
                    this,
                    () => CacheExpireTimeInSeconds,
                    "File Status Cache Timeout".Localize(),
                    null,
                    "Rate at which cached Perforce file records are updated".Localize()));
        }

        #endregion

        /// <summary>
        /// Gets or sets the amount of time, in seconds, before cached information from the
        /// Perforce server expires. A shorter amount of time keeps the user more up-to-date
        /// with the Perforce status of files, while a longer amount of time improves performance.
        /// The default is 60 seconds.</summary>
        public int CacheExpireTimeInSeconds
        {
            get;
            set;
        }

        #region ICommandClient
        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True iff client can do the command</returns>
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
                ConfigureConnection();
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        #endregion

        #region ISourceControlService Members

        /// <summary>
        /// Get the revision logs for an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>Returns revision history logs in a table, where each row is a revision record</returns>
        public override DataTable GetRevisionLog(Uri uri)
        {
            CheckUri(uri);

            DataTable p4DataTable = new DataTable("Perforce Revisions");

            if (Enabled && m_connectionManager.InitializeConnection())
            {
                var path = uri.Path();

                var p4RecordSet =
                    RunP4Command("changes", "-s", "submitted", "-l", "-m", "100", path);

                if (p4RecordSet.TaggedOutput.Count == 0)
                    return p4DataTable;

                p4DataTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = Type.GetType("System.Int32"),
                        ColumnName = "revision",
                        ReadOnly = true,
                        Unique = true
                    });

                p4DataTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = Type.GetType("System.String"),
                        ColumnName = "user"
                    });

                p4DataTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = Type.GetType("System.String"),
                        ColumnName = "description"
                    });

                p4DataTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = Type.GetType("System.String"),
                        ColumnName = "status"
                    });

                p4DataTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = Type.GetType("System.String"),
                        ColumnName = "client"
                    });

                p4DataTable.Columns.Add(
                    new DataColumn
                    {
                        DataType = Type.GetType("System.DateTime"),
                        ColumnName = "time"
                    });

                // Valid keys on each record in this particular record set:
                // "user", "change", "desc", "status", "client", "time"
                foreach (var record in p4RecordSet.TaggedOutput)
                {
                    string change = record["change"];
                    string user = record["user"];
                    string desc = record["desc"];
                    string status = record["status"];
                    string client = record["client"];
                    string time = record["time"];

                    DateTime date;
                    {
                        double dTime = double.Parse(time);
                        date =
                            (new DateTime(1970, 1, 1, 0, 0, 0)
                            .AddSeconds(dTime)
                            .ToLocalTime());
                    }

                    DataRow row = p4DataTable.NewRow();
                    row["revision"] = Int32.Parse(change);
                    row["user"] = user;
                    row["description"] = desc;
                    row["status"] = status;
                    row["client"] = client;
                    row["time"] = date;

                    p4DataTable.Rows.Add(row);
                }

            }
            return p4DataTable;
        }

        /// <summary>
        /// Adds an item to source control</summary>
        /// <param name="uri">URI representing the path to item</param>
        public override void Add(Uri uri)
        {
            if (!(Enabled && m_connectionManager.InitializeConnection()))
                return;

            CheckUri(uri);
        
            AddPerforceFileRequest(uri, u => RunCommand("add", u.Path()));
        }

        /// <summary>
        /// Deletes an item from source control</summary>
        /// <param name="uri">URI representing the path to item</param>
        public override void Delete(Uri uri)
        {
            CheckUri(uri);

            AddPerforceFileRequest(uri, u => RunCommand("delete", u.Path()));
        }

        /// <summary>
        /// Checks in the given items</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <param name="description">Check-in description</param>
        public override void CheckIn(IEnumerable<Uri> uris, string description)
        {
            int count = uris.Count();

            if (count == 0)
                return;

            if (string.IsNullOrEmpty(description))
                description = "No Comment!";

            if (!(Enabled && m_connectionManager.InitializeConnection()))
                return;

            try
            {
                var changeList = m_connectionManager.CreateChangelist(description);
  
                foreach (Uri uri in uris)
                {
                    CheckUri(uri);
                    string path = PathUtil.GetCanonicalPath(uri);
                    var result= RunP4Command("fstat", path);
                    if (result.Success && result.TaggedOutput != null)
                    {
                        foreach (var record in result.TaggedOutput)
                        {
                            if (record.ContainsKey("change")) //already opened in changelist
                                //reopen an open file in order to move it to a different changelist
                                RunP4Command("reopen", "-c", changeList.Id.ToString(), path);
                            if (record.ContainsKey("action"))  // opens the files for edit within the specified changelist
                                RunP4Command("edit", "-c", changeList.Id.ToString(), path);
                        }
                    }

                }

                changeList.Submit(null);
                RefreshStatus(uris); 

            }

            catch (P4Exception ex)
            {
                switch (ex.ErrorLevel)
                {
                    case ErrorSeverity.E_WARN:
                        Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                        break;
                    case ErrorSeverity.E_FAILED:
                        Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                        break;
                    case ErrorSeverity.E_INFO:
                        Outputs.WriteLine(OutputMessageType.Info, ex.Message);
                        break;
                }
                if (ThrowExceptions)
                    throw;
            }
          

        }

        /// <summary>
        /// Checks out an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        public override void CheckOut(Uri uri)
        {
            CheckUri(uri);

            FileInfo info = GetInfo(uri);
            if (info.IsLocked)
            {
                var lockUser = string.IsNullOrEmpty(info.OtherLock) ? m_connectionManager.UserName : info.OtherLock;

                // There's no guarantee that the caller is in a transaction, and so throwing an
                //  InvalidTransactionException here could crash the app. Let's just output the message.
                Outputs.WriteLine(OutputMessageType.Warning, "The document is locked by another user: " + lockUser);
                return;
            }

            AddPerforceFileRequest(
                uri,
                u => RunCommand("edit", uri.Path())
                     && AllowMultipleCheckout
                     || RunCommand("lock", uri.Path()));
        }

        /// <summary>
        /// Gets the latest version of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        public override void GetLatestVersion(Uri uri)
        {
            CheckUri(uri);

            AddPerforceFileRequest(uri, u => RunCommand("sync", u.Path()));
        }

        /// <summary>
        /// Reverts an item, i.e., undoes any checkout and gets the latest version</summary>
        /// <param name="uri">URI representing the path to item</param>
        public override void Revert(Uri uri)
        {
            CheckUri(uri);
            AddPerforceFileRequest(uri, u => RunCommand("revert", u.Path()));
        }

        /// <summary>
        /// Gets the source control status of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>Status of item</returns>
        /// <remarks>It is much more efficient to get the status of many items at once
        /// by calling GetStatus(uris).</remarks>
        public override SourceControlStatus GetStatus(Uri uri)
        {             
            FileInfo info = GetInfo(uri);
            return info.Status;
        }

        /// <summary>
        /// Gets the source control status of each item</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <returns>Status of each item, in the same order as given</returns>
        public override SourceControlStatus[] GetStatus(IEnumerable<Uri> uris)
        {
            foreach (Uri uri in uris)
                CheckUri(uri);

            return GetInfo(uris).Select<FileInfo, SourceControlStatus>(fi => fi.Status).ToArray();
        }

        /// <summary>
        /// Gets the source control status of all items under the folder</summary>
        /// <param name="uri">URI representing the folder path</param>
        /// <returns>False if not supported </returns>
        public override bool GetFolderStatus(Uri uri) { return false; }

        /// <summary>
        /// Gets whether an item is in sync with the source control version</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>True iff item is in sync with the source control version</returns>
        public override bool IsSynched(Uri uri)
        {
            CheckUri(uri);
            FileInfo info = GetInfo(uri);
            return info.HeadRevision == info.Revision;
        }

        /// <summary>
        /// From given items, gets those that are different from the revision in the depot</summary>
        /// <param name="uris">URIs representing paths to items</param>
        /// <returns>URIs of items that are different from the depot</returns>
        public override IEnumerable<Uri> GetModifiedFiles(IEnumerable<Uri> uris)
        {
            if (!(Enabled && m_connectionManager.InitializeConnection()))
                yield break;

            List<string> refreshPaths = new List<string>();
            Dictionary<string, Uri> pathToUri = new Dictionary<string, Uri>();
            foreach (Uri uri in uris)
            {
                string path = uri.Path();
                refreshPaths.Add(path);
                pathToUri.Add(path, uri);
            }

            string[] args = new string[refreshPaths.Count + 1];

            // -sa shows only the names of opened files that are different from the revision in the depot, or are missing
            args[0] = "-sa";
            for (int i = 0; i < refreshPaths.Count; ++i)
                args[i + 1] = refreshPaths[i];
            var opened = RunP4Command("diff", args);

            // -se shows only the names of unopened files in the client workspace that are different than the revision in the depot.
            args[0] = "-se";
            var unopened = RunP4Command("diff", args);

            if (opened != null && opened.TaggedOutput != null)
            {
                foreach (var record in opened.TaggedOutput)
                {
                    string path2 = record["clientFile"];
                    Uri fileuri;
                    if (pathToUri.TryGetValue(path2, out fileuri))
                        yield return fileuri;
                }
            }

            if (unopened != null && unopened.TaggedOutput != null)
            {
                foreach (var record in unopened.TaggedOutput)
                {
                    string path2 = record["clientFile"];
                    Uri fileuri;
                    if (pathToUri.TryGetValue(path2, out fileuri))
                        yield return fileuri;
                }
            }
        }

        /// <summary>
        /// Gets whether an item is locked by the client or another user</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <returns>True iff item is locked</returns>
        public override bool IsLocked(Uri uri)
        {
            CheckUri(uri);
            FileInfo info = GetInfo(uri);
            return info.IsLocked;
        }

        /// <summary>
        /// Refreshes the status of an item</summary>
        /// <param name="uri">URI representing the path to item</param>
        /// <remarks>It is much more efficient to refresh the status of many items at once
        /// by calling RefreshStatus(uris). This is a synchronous call and should not be done
        /// often.</remarks>
        public override void RefreshStatus(Uri uri)
        {
            Uncache(uri);
            GetInfo(uri);
        }

        /// <summary>
        /// Refreshes the status of given items</summary>
        /// <param name="uris">URIs representing the paths to items</param>
        /// <remarks>This is a synchronous call and should not be called often.</remarks>
        public override void RefreshStatus(IEnumerable<Uri> uris)
        {
            foreach (var uri in uris)
                Uncache(uri);
            GetInfo(uris);
        }

        /// <summary>
        /// Exports a file of the specified revision to a designated location</summary>
        /// <param name="sourceUri">Source file URI</param>
        /// <param name="destUri">Designated location URI</param>
        /// <param name="revision">Source control revision of file</param>
        public override void Export(Uri sourceUri, Uri destUri, SourceControlRevision revision)
        {
            if (!(Enabled && m_connectionManager.InitializeConnection()))
                return;
            
            string toPath = destUri.Path();
            string fromPath = sourceUri.Path();

            switch (revision.Kind)
            {
                case SourceControlRevisionKind.Number:
                    fromPath += "#" + revision.Number;
                    break;
                case SourceControlRevisionKind.Unspecified:
                    throw new NotSupportedException("Can't export revision of none ");
                //fromPath += "#none";
                //break;
                case SourceControlRevisionKind.Base:
                    fromPath += "#have";
                    break;
                case SourceControlRevisionKind.Head:
                    fromPath += "#head";
                    break;
                case SourceControlRevisionKind.Working:
                    //just file copy 
                    System.IO.File.Copy(fromPath, toPath, true);
                    return;
                case SourceControlRevisionKind.Date:
                    fromPath += "@" + revision.Date;
                    break;
                case SourceControlRevisionKind.ChangeList:
                    fromPath += "@" + revision.ChangeListNumber;
                    break;
            }

            RunP4Command("print", "-q", "-o", toPath, fromPath);

        }

        #endregion

        /// <summary>
        /// Connects to the source control server</summary>
        /// <returns>True iff connected to the source control server</returns>
        public override bool Connect()
        {
            return Enabled && ConfigureConnection();
        }

        /// <summary>
        /// Disconnects from the source control server</summary>
        public override void Disconnect()
        {
            if (Enabled &&  m_connectionManager.ValidateConnection(false))
                m_connectionManager.DestroyConnection();
        }

        /// <summary>
        /// Sets up server/client connection information</summary>
        /// <returns>True iff information set up</returns>
        public bool ConfigureConnection()
        {
            return m_connectionManager.ConfigureConnection();
        }

        /// <summary>
        /// Gets the latest version of all items in the given directory and all of its subdirectories</summary>
        /// <param name="directoryPath">Directory path</param>
        public void GetLatestVersion(string directoryPath)
        {
            if (!(Enabled && m_connectionManager.InitializeConnection()))
                return;

            // update P4 workspace to latest versions of all files under directoryPath,
            // update FileInfo cache with records returned from operation.


            var output = RunP4Command("sync", Path.GetDirectoryName(directoryPath) + "...");
            if (output != null && output.Success)
            {
                foreach (var record in output.TaggedOutput)
                {
                    if (record.ContainsKey("clientFile"))
                    {
                        Uri uri = new Uri(PathUtil.GetCanonicalPath(record["clientFile"]));
                        GetInfo(uri);
                    }
                }
            }
           
          
        }

        /// <summary>
        /// Gets or sets a value indicating recent connections to Perforce server</summary>
        public string ConnectionHistory
        {
            get { return m_connectionManager.ConnectionHistory; }
            set 
            {
                if (m_settingsLoading)
                {
                    m_requestedConnectionHistory = value;
                    return;
                }

                m_connectionManager.ConnectionHistory = value; 
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the default connection to Perforce server</summary>
        public string DefaultConnection
        {
            get { return m_connectionManager.DefaultConnection; }
            set 
            {
                if (m_settingsLoading)
                {
                    m_requestedDefaultConnection = value;
                    return;
                }

                m_connectionManager.DefaultConnection = value; 
            }
        }

        /// <summary>
        /// Gets or sets the resource root's absolute path</summary>
        /// <remarks>Note that this is a static property. Therefore, resource-root relative URI
        /// behavior is application-wide if this property is set.</remarks>
        public static string ResourceRoot
        {
            get
            {
                return s_resourceRoot;
            }
            set
            {
                if (!PathUtil.IsValidPath(value))
                    throw new ArgumentException("Resource root can only be set to a valid path");

                value = PathUtil.GetCanonicalPath(value);
                value = value.Replace('\\', '/'); // make separator match Uri separator

                s_resourceRoot = value;
            }
        }

        /// <summary>
        /// Runs the given command using Perforce's unparsed interface</summary>
        /// <param name="cmd">Command string</param>
        /// <param name="path">Path parameter for command</param>
        /// <returns>True iff command was run successfully</returns>
        protected bool RunCommand(string cmd, string path)
        {
            bool success = false;

            if (Enabled && m_connectionManager.InitializeConnection())
            {
                var results = RunP4Command(cmd, path);
                success = results != null && results.Success;
            }

            return success;
        }

        private IP4CommandResult RunP4Command(string command, params string[] args)
        {
            var cmd = m_connectionManager.CreateCommand(command, args);
            P4CommandResult results = null;
            try
            {
                results = cmd.Run();
            }
            catch (P4Exception ex)
            {
                switch (ex.ErrorLevel)
                {
                    case ErrorSeverity.E_WARN:
                        Outputs.WriteLine(OutputMessageType.Warning, ex.Message);
                        break;
                    case ErrorSeverity.E_FAILED:
                        Outputs.WriteLine(OutputMessageType.Error, ex.Message);
                        break;
                    case ErrorSeverity.E_INFO:
                        Outputs.WriteLine(OutputMessageType.Info, ex.Message);
                        break;
                }
                if (ThrowExceptions)
                    throw;

                return new DummyCommandResult(false, args);
            }
            return new P4CommandResultWrapper(cmd, results);
        }


        /// <summary>
        /// Shows an error message to the user</summary>
        /// <param name="message">Error message</param>
        protected void ShowErrorMessage(string message)
        {
            Outputs.WriteLine(OutputMessageType.Warning, message);
        }

        private void LoadFileStatusIcons()
        {
            m_statusImages = new Image[NumFileStatusIcons];
            m_statusImages[P4VFileAdd] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_add.png");
            m_statusImages[P4VFileSync] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_sync.png");
            m_statusImages[P4VFileNotsync] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_notsync.png");
            m_statusImages[P4VFileEditHead] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_edit_head.png");
            m_statusImages[P4VFileWs] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_ws.png");
            m_statusImages[P4VFileLock] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_lock.png");
            m_statusImages[P4VFileLockOther] = GdiUtil.GetImage("Atf.Perforce.Resources.p4v_file_lock_other.png");
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
                    result = m_statusImages[P4VFileAdd];
                    break;
                case SourceControlStatus.CheckedIn:
                    {
                        if (IsSynched(uri))
                        {
                            FileInfo info = GetInfo(uri);
                            if (string.IsNullOrEmpty(info.OtherLock))
                                result = m_statusImages[P4VFileSync];
                            else
                                result = m_statusImages[P4VFileLockOther];
                        }
                        else
                            result = m_statusImages[P4VFileNotsync];

                        break;
                    }
                case SourceControlStatus.CheckedOut:
                    {
                        FileInfo info = GetInfo(uri);
                        if (info.IsLocked)
                            result = m_statusImages[P4VFileLock];
                        else
                            result = m_statusImages[P4VFileEditHead];
                        break;
                    }
                case SourceControlStatus.NotControlled:
                    result = m_statusImages[P4VFileWs];
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets source control detailed information for the specified file</summary>
        /// <param name="uri">URI of the file under source control</param>
        /// <returns>Source control information for the specified file</returns>
        public override SourceControlFileInfo GetFileInfo(Uri uri)
        {
            // Get the source control information
            FileInfo fileInfo = GetInfo(uri);

            // Copy all members from the mutable PerforceService.FileInfo
            // to the immutable SourceControlFileInfo class
            return new SourceControlFileInfo(
                fileInfo.Uri,
                fileInfo.Status,
                fileInfo.HeadRevision,
                fileInfo.Revision,
                fileInfo.IsLocked,
                fileInfo.OtherUsers,
                fileInfo.OtherLock);
        }

        /// <summary>
        /// Gets or sets whether the source control service is enabled</summary>
        public override bool Enabled
        {
            get { return m_enabled; }
            set
            {
                if (m_settingsLoading)
                {
                    m_requestedEnabled = value;
                    return;
                }

                lock (m_infoCache)
                    m_infoCache.Clear();
                ValidateConnection(false);
                m_enabled = value;
                OnEnabledChanged(EventArgs.Empty);
            }
        }

        private FileInfo GetInfo(Uri uri)
        {
            List<FileInfo> infoList = GetInfo(new[] { uri });
            return infoList[0];
        }

        private List<FileInfo> GetInfo(IEnumerable<Uri> uris)
        {
            // create result list using cache where possible, and build a list of unknown paths
            var result = new List<FileInfo>();

            var getInfoFromPerforce = Enabled && m_connectionManager.InitializeConnection();

            var refreshList = new List<string>();
            lock (m_infoCache)
            {
                foreach (Uri uri in uris)
                {
                    string path = uri.Path();

                    FileInfo info;
                    if (!m_infoCache.TryGetValue(path, out info))
                        m_infoCache[path] = info = new FileInfo(uri);

                    if (!info.NotInClientView && info.RequiresRefresh(CacheExpireTimeInSeconds))
                        refreshList.Add(info.Uri.Path());

                    result.Add(info);
                }
            }
            if (refreshList.Count > 0)
            {
                var oldExcLevel = P4Exception.MinThrowLevel;
                P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;
                // update FileInfo cache with latest P4 records for selected files
                var output = (getInfoFromPerforce)
                    ? RunP4Command("fstat", refreshList.ToArray())
                    : new DummyCommandResult(true, refreshList.ToArray());
                if (output != null)
                {
                    if (output.Success)
                        UpdateFileInfoCache(refreshList, output.TaggedOutput);
                    else
                    {
                        if (refreshList.Count == 1
                            // only one file (not sure ErrorList has one-to-one correspondence for multiple files 
                            && output.ErrorList != null)
                        {
                            var filePath = refreshList[0];
                            foreach (var error in output.ErrorList)
                            {
                                switch (error.SeverityLevel)
                                {
                                    case ErrorSeverity.E_WARN:
                                        Outputs.WriteLine(OutputMessageType.Warning, error.ErrorMessage);
                                        break;
                                    case ErrorSeverity.E_FAILED:
                                        Outputs.WriteLine(OutputMessageType.Error, error.ErrorMessage);
                                        break;
                                    case ErrorSeverity.E_INFO:
                                        Outputs.WriteLine(OutputMessageType.Info, error.ErrorMessage);
                                        break;
                                }
                                if (error.ErrorCode == P4ClientError.NotUnderRoot)
                                {
                                    lock(m_infoCache)
                                        m_infoCache[filePath].NotInClientView = true;
                                }
                                else if (error.ErrorCode == P4ClientError.NotUnderClient)
                                {
                                    lock(m_infoCache)
                                        m_infoCache[filePath].NotInClientView = true;
                                }
                            }

                        }
                    }
                }
                P4Exception.MinThrowLevel = oldExcLevel;
            }

            return result;
        }

        private void UpdateFileInfoCache(List<string> refreshList, TaggedObjectList records)
        {
            if (records != null)
            {
                // update FileInfo instances with passed in records, fire StatusChanged event when applicable
                foreach (var record in records)
                {
                    FileInfo info;
                    string filePath = PathUtil.GetCanonicalPath(record["clientFile"]);
                    bool foundInfo;
                    lock (m_infoCache) //keep the lock for as short a time as possible; can't lock around OnStatusChanged().
                        foundInfo = m_infoCache.TryGetValue(filePath, out info);
                    if (foundInfo)
                    {
                        SourceControlStatus oldStatus = info.Status;
                        info.UpdateRecord(record);
                        if (info.Status != oldStatus)
                            OnStatusChanged(new SourceControlEventArgs(info.Uri, info.Status));
                        refreshList.Remove(filePath);
                    }
                }
            }
            foreach (var filePath in refreshList)
            {
                //P4API.NET does not output records for files not under version control
                FileInfo info;
                lock (m_infoCache) //keep the lock for as short a time as possible; can't lock around OnStatusChanged().
                    info = m_infoCache[filePath];
                SourceControlStatus oldStatus = info.Status;
                if (oldStatus != SourceControlStatus.NotControlled)
                {
                    info.NotControlled = true;
                    OnStatusChanged(new SourceControlEventArgs(info.Uri,
                        SourceControlStatus.NotControlled));
                }
            }
        }

        private void Uncache(Uri uri)
        {
            lock (m_infoCache)
            {
                FileInfo info;
                if (m_infoCache.TryGetValue(uri.Path(), out info))
                    info.ClearRecord();
            }
        }

        private static void CheckUri(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
        }

        private bool ValidateConnection(bool checkLogin)
        {
            return Enabled && m_connectionManager.ValidateConnection(checkLogin);
        }

  

        private void AddPerforceFileRequest(Uri uri, Func<Uri, bool> doRequest)
        {
            var path = uri.Path();
            FileInfo info;
            lock (m_infoCache)
                if (!m_infoCache.TryGetValue(path, out info))
                    m_infoCache[path] = info = new FileInfo(uri);
            lock (m_pendingRequests)
                m_pendingRequests.Enqueue(new PerforceRequest(info, doRequest));
            m_queryRequestedEvent.Set(); // Trigger the worker thread to talk to the Perforce server.
        }

        /// <summary>
        /// This is meant for internal use only, to be used by the legacy PerforceService.</summary>
        protected void InitCommon()
        {
            CreateBackgroundThread();
        }

        private void CreateBackgroundThread()
        {
            m_perforceThread = new Thread(PerforceThreadStart);
            m_perforceThread.Name = "Perforce querying thread";
            m_perforceThread.IsBackground = true; //so that the thread can be killed if app dies.
            m_perforceThread.SetApartmentState(ApartmentState.STA);
            m_perforceThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
            m_perforceThread.Start();
        }

        // A separate thread is used to execute the Perforce commands, to avoid freezing the main GUI thread.
        private void PerforceThreadStart()
        {
            while (m_queryRequestedEvent.WaitOne())
            {
                // Clear the pending request queue quickly, by copying to a local array. We don't
                //  want to lock the queue for a long time because we might block the GUI thread.
                PerforceRequest[] requests;
                lock (m_pendingRequests)
                {
                    requests = new PerforceRequest[m_pendingRequests.Count];
                    m_pendingRequests.CopyTo(requests, 0);
                    m_pendingRequests.Clear();
                }

                // Execute all of the Perforce commands.
                foreach (PerforceRequest request in requests)
                    request.Execute();

                // Update the status of the paths.
                GetInfo(requests.Select(x => x.Uri));
            }
        }

        /// <summary>
        /// Gets or sets whether the requested commands have been processed completely</summary>
        public bool RequestProcessed
        {
            get
            {
                lock (m_pendingRequests)
                    return m_pendingRequests.Count > 0;
            }
        }

        #region IDisposable
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources</summary>
        public void Dispose()
        {
        }

        #endregion

        private void connection_ConnectionChanged(object sender, EventArgs e)
        {
            lock (m_infoCache)
                m_infoCache.Clear();
            OnConnectionChanged(e);
            OnStatusChanged(new SourceControlEventArgs(null, SourceControlStatus.Unknown));
        }

        void connectionManager_LoginCanceled(object sender, EventArgs e)
        {
            Enabled = false; // since the user canceled, don't try again until they enable source control
        }

        /// <summary>
        /// Raises the StatusChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnStatusChanged(SourceControlEventArgs e)
        {
            if (MainForm != null)
                MainForm.Invoke(new MethodInvoker(() => base.OnStatusChanged(e)));
            else
                base.OnStatusChanged(e);
        }

        /// <summary>
        /// Raises the ConnectionChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnConnectionChanged(EventArgs e)
        {
            if (MainForm != null)
                MainForm.Invoke(new MethodInvoker(() => base.OnConnectionChanged(e)));
            else
                base.OnConnectionChanged(e);
        }

        /// <summary>
        /// Raises the StatusChanged event</summary>
        /// <param name="e">Event args</param>
        protected override void OnEnabledChanged(EventArgs e)
        {

            if (MainForm != null)
                MainForm.Invoke(new MethodInvoker(() => base.OnEnabledChanged(e)));
            else
                base.OnEnabledChanged(e);
        }

        /// <summary>
        /// Turns on caching of values assigned to select properties. Values will be applied later, 
        /// in the right order, when m_settingsService_Reloaded() is called.</summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs for event</param>
        protected void m_settingsService_Loading(object sender, EventArgs e)
        {
            m_settingsLoading = true;
            m_requestedDefaultConnection = DefaultConnection;
            m_requestedConnectionHistory = ConnectionHistory;
            m_requestedEnabled = Enabled;
        }

        /// <summary>
        /// Assigns cached property values to their respective properties, in the correct order. It
        /// then disables property caching.</summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">EventArgs for event</param>
        protected void m_settingsService_Reloaded(object sender, EventArgs e)
        {
            if (!m_settingsLoading)
                return;

            m_settingsLoading = false;
            DefaultConnection = m_requestedDefaultConnection;
            ConnectionHistory = m_requestedConnectionHistory;
            Enabled = m_requestedEnabled;
        }

#pragma warning disable 649 // Field is never assigned to and will always have its default value null

        /// <summary>
        /// Gets or sets main form</summary>
        [Import(AllowDefault = true)] // optional service
        protected Form MainForm { get; set; }

#pragma warning restore 649

        private const int NumFileStatusIcons = 7;
        private const int P4VFileAdd = 0;
        private const int P4VFileSync = 1;
        private const int P4VFileNotsync = 2;
        private const int P4VFileEditHead = 3;
        private const int P4VFileWs = 4;
        private const int P4VFileLock = 5;
        private const int P4VFileLockOther = 6;

        private ISettingsService m_settingsService;
        private ConnectionManager m_connectionManager;

        private static string s_resourceRoot;

        // All access to this dictionary must be by locking it first. The dictionary maps
        //  a file path (not case sensitive) to the last known Perforce status of that file.
        private readonly Dictionary<string, FileInfo> m_infoCache =
            new Dictionary<string, FileInfo>(StringComparer.InvariantCultureIgnoreCase);

        // All access to this queue must be by locking it first. 'm_perforceThread' locks it
        //  to copy its contents and then synchronously query the Perforce server. The main
        //  GUI thread locks it to append new requests.
        private readonly Queue<PerforceRequest> m_pendingRequests = new Queue<PerforceRequest>();

        private Image[] m_statusImages;

        private bool m_enabled;
        private bool m_settingsLoading;                     // when true, cache off assignment to select properties
        private bool m_requestedEnabled;                    // cached 'Enabled' property value
        private string m_requestedDefaultConnection = "";   // cached 'DefaultConnection' property value
        private string m_requestedConnectionHistory = "";   // cached 'ConnectionHistory' property value

        private Thread m_perforceThread; // Runs the actual query to the Perforce server, so that the GUI thread isn't blocked.
        private readonly AutoResetEvent m_queryRequestedEvent = new AutoResetEvent(false); // Used to trigger m_perforceThread.

        // interface to exposing only those members of Perforce.P4.P4CommandResult we require
        private interface IP4CommandResult
        {
            P4ClientErrorList ErrorList { get; }
            bool Success { get; }
            TaggedObjectList TaggedOutput { get; }
        }

        // provides P4CommandResult-like data, for when source control has been deactivated
        private class DummyCommandResult : IP4CommandResult
        {
            public DummyCommandResult(bool success, params string[] args)
            {
                Success = success;
                TaggedOutput = new TaggedObjectList();
                foreach (var item in args)
                {
                    var record = new TaggedObject();
                    record["clientFile"] = item;
                    record["dummy"] = "true";       // identifies IP4CommandResult as containing a dummy record for this item
                    TaggedOutput.Add(record);
                }
            }

            public P4ClientErrorList ErrorList { get { return null; } private set { } }
            public bool Success { get; private set; }
            public TaggedObjectList TaggedOutput { get; private set; }
        }

        // Wrapper class for Perforce.P4.P4CommandResult
        private class P4CommandResultWrapper : IP4CommandResult
        {
            public P4CommandResultWrapper(P4Command p4Command, P4CommandResult p4Result) { m_p4Command = p4Command;  m_p4Result = p4Result; }

            public P4ClientErrorList ErrorList { get { return (m_p4Result != null) ? m_p4Result.ErrorList : new P4ClientErrorList(m_p4Command.pServer, m_p4Command.CommandId); } }
            public bool Success { get { return m_p4Result != null && m_p4Result.Success; } }
            public TaggedObjectList TaggedOutput { get { return (m_p4Result != null) ? m_p4Result.TaggedOutput : new TaggedObjectList(); } }

            private P4Command m_p4Command = null;
            private P4CommandResult m_p4Result = null;
        }
    }
}
