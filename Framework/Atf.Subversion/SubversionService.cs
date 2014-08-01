//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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

        [Import(AllowDefault = true)]
        public ISettingsService SettingsService
        {
            get { return m_settingsService; }
            set { m_settingsService = value; }
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
        }

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
        /// Adds an item to source control</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        public override void Add(Uri uri)
        {
            CheckUri(uri);
            if (ClientInitialized())
            {
                if (RunCommand("add", GetCanonicalPath(uri)))
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
                if (RunCommand("delete", GetCanonicalPath(uri)))
                {
                    SetStatus(uri, SourceControlStatus.Deleted);
                }
            }
        }

        /// <summary>
        /// Checks in the given items</summary>
        /// <param name="uriEnum">Uris representing the path to items</param>
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
                    var path = GetCanonicalPath(uri);
                    pathsBuilder.Append(' ');
                    pathsBuilder.Append(path);
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

            FileInfo info = GetInfo(uri, true);
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
                string path = GetCanonicalPath(uri);
                RunCommand("update", path);
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
                string path = GetCanonicalPath(uri);
                RunCommand("revert", path);
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
        /// <param name="uris">Uris representing the paths to items</param>
        /// <returns>Status of each item, in the same order as the given uris.</returns>
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
            if (!RunCommand("status -u -v", directoryName))
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
                        Uri fileUri = new Uri(filename);
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
        /// <returns>true, iff item is in sync with the source control version</returns>
        public override bool IsSynched(Uri uri)
        {
            CheckUri(uri);
            FileInfo info = GetInfo(uri, false);
            return info.IsSynced;
        }

        /// <summary>
        /// From given items, gets those that are different from the revision in the depot </summary>
        /// <param name="uris">Uris representing the paths to items</param>
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
        /// <returns>true, iff item is locked</returns>
        public override bool IsLocked(Uri uri)
        {
            CheckUri(uri);
            FileInfo info = GetInfo(uri, false);
            return info.IsLocked;
        }

        /// <summary>
        /// Refreshes the status of an item</summary>
        /// <param name="uri">Uri, representing the path to item</param>
        public override void RefreshStatus(Uri uri)
        {
            FileInfo info = GetInfo(uri, true);
        }

        /// <summary>
        /// Refreshes the status of these items</summary>
        public override void RefreshStatus(IEnumerable<Uri> uris)
        {
            GetInfo(uris, true);
        }

        // Get the revision history of the file
        public override DataTable GetRevisionLog(Uri uri)
        {

            DataTable svnDataTable = new DataTable("Subversion Reversions");

            //string[] targets = { uri.ToString() };
            string path = GetCanonicalPath(uri);
            RunCommand("log -r HEAD:0", path); // search for log from revision HEAD to 0

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
        /// Export a file of the specified reversion to a designated location </summary>
        public override void Export(Uri sourceUri, Uri destUri, SourceControlRevision revision)
        {

            string fromPath = GetCanonicalPath(sourceUri);
            string toPath = GetCanonicalPath(destUri);
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
                RunCommand(cmd, fromPath + " " + toPath);
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
        /// Turns on caching of values assigned to select properties. Values will be applied later, 
        /// in the right order, when m_settingsService_Reloaded() is called.</summary>
        protected void m_settingsService_Loading(object sender, EventArgs e)
        {
            m_settingsLoading = true;
            m_requestedShowErrors = ShowErrors;
        }

        /// <summary>
        /// Assigns cached property values to their respective properties, in the correct order.  It
        /// then disables property caching.</summary>
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
        /// <returns>canonical path for the URI</returns>
        protected static string GetCanonicalPath(Uri uri)
        {
            string result = uri.AbsolutePath;
            result = Uri.UnescapeDataString(result);
            return GetCanonicalPath(result);
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

                // get info for unknown paths from Subversion
                RefreshInfo(refreshPaths.ToArray());

                for (int i = 0; i < result.Count; i++)
                {
                    FileInfo info = result[i];
                    if (oldStatus[i] != info.Status)
                        OnStatusChanged(new SourceControlEventArgs(info.Uri, info.Status));
                }
            }

            return result;
        }

        private static int GetBeginningOfLine(string text, int startPointOfMatch)
        {
            if (startPointOfMatch > 0)
            {
                --startPointOfMatch;
            }
            if (startPointOfMatch >= 0 && startPointOfMatch < text.Length)
            {
                // Move to the left until the first '\n char is found
                for (int index = startPointOfMatch; index >= 0; index--)
                {
                    if (text[index] == '\n')
                    {
                        return (index + 1);
                    }
                }
                return (0);
            }
            return (startPointOfMatch);
        }

        private static int GetEndOfLine(string text, int endPointOfMatch)
        {
            if (endPointOfMatch >= 0 && endPointOfMatch < text.Length)
            {
                // Move to the right until the first '\n char is found
                for (int index = endPointOfMatch; index < text.Length; index++)
                {
                    if (text[index] == '\n')
                    {
                        return (index);
                    }
                }
                return (text.Length);
            }
            return (endPointOfMatch);
        }

        private void ParseStatus(string path)
        {
            FileInfo info = m_infoCache[path];
            info.Status = SourceControlStatus.Unknown;
            info.IsLocked = false;
            info.IsSynced = true;

            if (!string.IsNullOrEmpty(m_svnError))
            {
                return;
            }

            // split the output into lines
            string[] lines = m_svnOutput.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                if (!line.EndsWith(path))               // skip line
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
            foreach (string path in paths)
            {
                pathsBuilder.Append(' ');
                pathsBuilder.Append(path);
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

        private static void CheckUri(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
        }

        private bool ClientInitialized()
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

        private static SourceControlStatus[] GetStatusArray(List<FileInfo> info)
        {
            SourceControlStatus[] result = new SourceControlStatus[info.Count];
            for (int i = 0; i < info.Count; i++)
                result[i] = info[i].Status;
            return result;
        }

        private class FileInfo
        {
            public FileInfo(Uri uri, SourceControlStatus status)
            {
                Uri = uri;
                Status = status;
            }

            public Uri Uri;
            public SourceControlStatus Status = SourceControlStatus.Unknown;
            //public int HeadRevision;
            //public int Revision;
            public bool IsSynced = false;
            public bool IsLocked = false;
            public bool IsModified = false;
        }




        /// <summary>
        /// Runs the given command using svn command line client</summary>
        /// <param name="cmd">Command string</param>
        /// <param name="path">Path parameter for command</param>
        /// <returns>true, iff command was run successfully</returns>
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

        private InfoCache<FileInfo> m_infoCache =
            new InfoCache<FileInfo>(StringComparer.InvariantCultureIgnoreCase);

        private ISettingsService m_settingsService;
        private bool m_settingsLoading;              // when true, cache off assignment to select properties
        private bool m_requestedShowErrors;          // cached 'ShowErrors' property value
        private bool m_connectionInitialized;
        //private bool m_allowMultipleCheckout;
//         private bool m_allowCheckIn = true;
        private string m_svnOutput;
        private string m_svnError;
        private bool m_showErrors = true;
        private bool m_svnAvailable = false;

    }

    public class InfoCache<TValue>
    {
        public InfoCache(IEqualityComparer<string> comparer)
        {
            m_infoCache = new Dictionary<string, TValue>(comparer);
        }

        public bool TryGetValue(string key, out TValue value)
        {
            return m_infoCache.TryGetValue(NormalizeKey(key), out value);
        }

        public TValue this[string key]
        {
            get { return m_infoCache[NormalizeKey(key)]; }
            set { m_infoCache[NormalizeKey(key)] = value; }
        }

        public void Remove(string key)
        {
            m_infoCache.Remove(NormalizeKey(key));
        }

        public void Clear()
        {
            m_infoCache.Clear();
        }

        private string NormalizeKey(string key)
        {
            if (key.EndsWith("/") || key.EndsWith("\\"))
                key = key.Substring(0, key.Length - 1);
            return key;
        }

        private readonly Dictionary<string, TValue> m_infoCache;
    }
}
