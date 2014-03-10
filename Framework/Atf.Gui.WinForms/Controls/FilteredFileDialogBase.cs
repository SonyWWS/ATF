using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Base class for custom OpenFilteredFileDialog and SaveFilteredFileDialog</summary>
    /// <remarks>
    /// All properties should be equivalent to the properties with the same names in System.Windows.Forms.FileDialog. 
    /// Unlike System.Windows.Forms.FileDialog or Sce.Atf.CustomFileDialog, this is a Windows Forms implementation without going out to the Win32 methods. 
    /// It has a CustomFileFilter callback, which can be used to exclude files in the dialog's ListView.</remarks>
    public partial class FilteredFileDialogBase : Form
    {
        /// <summary>
        /// Constructor</summary>
        public FilteredFileDialogBase()
        {
            InitializeComponent();

            // Initialize ListView
            listView1.View = View.Details;
            listView1.VirtualMode = true; // virtual mode is necessary for processing large number of files in a reasonable time  

            // Suspending automatic refreshes as items are added/removed.
            listView1.BeginUpdate();
            listView1.Columns.Add("Name", 250, HorizontalAlignment.Left);
            listView1.Columns.Add("Date Modified", 130, HorizontalAlignment.Left);
            listView1.Columns.Add("Size", listView1.Width - 250 - 130 - 20, HorizontalAlignment.Right);
            listView1.ColumnClick += new ColumnClickEventHandler(listView1_ColumnClick);
            m_listViewItemComparer = new ListViewItemComparer();

            listView1.SmallImageList = new ImageList();
            listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.FolderIcon));
            listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.DocumentImage));
            listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.DiskDriveImage));
            listView1.SmallImageList.Images.Add(ResourceUtil.GetImage16(Resources.ComputerImage));
            listView1.LargeImageList = new ImageList();
            listView1.LargeImageList.Images.Add(ResourceUtil.GetImage32(Resources.FolderImage));
            listView1.LargeImageList.Images.Add(ResourceUtil.GetImage32(Resources.DocumentImage));
            listView1.LargeImageList.Images.Add(ResourceUtil.GetImage32(Resources.DiskDriveImage));


            listView1.DoubleClick += listView1_DoubleClick;
            listView1.VirtualItemsSelectionRangeChanged += listView1_VirtualItemsSelectionRangeChanged;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            listView1.AfterLabelEdit += listView1_AfterLabelEdit;

            listView1.RetrieveVirtualItem += listView1_RetrieveVirtualItem;
            listView1.CacheVirtualItems += listView1_CacheVirtualItems;


            // Re-enable the display.
            listView1.EndUpdate();

            toolStripButton1.Click += backButton_Click;
            toolStripButton2.Click += upButton_Click;
            toolStripButton3.Click += newFolder_Click;
            fileTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            fileTypeComboBox.SelectedIndexChanged += fileTypeComboBox_SelectedIndexChanged;

            fileNameComboBox.SelectedIndexChanged += fileNameComboBox_SelectedIndexChanged;
            fileNameComboBox.PreviewKeyDown += fileNameComboBox_PreviewKeyDown;
            fileNameComboBox.TextChanged += new EventHandler(fileNameComboBox_TextChanged);

            lookInComboBox.SelectedIndexChanged += path_SelectedIndexChanged;
            lookInComboBox.DrawMode = DrawMode.OwnerDrawFixed;
            lookInComboBox.DrawItem += lookInComboBox_DrawItem;

            m_timer = new Timer { Interval = SELECTION_DELAY }; // timer is needed to avoid excessive delay of fileNameComboBox update when large number of files are selected
            m_timer.Tick += timer_Tick;

            this.Load += fileDialogBase_Load;

            m_backgroundWorker = new BackgroundWorker();
            m_backgroundWorker.WorkerSupportsCancellation = true;
            m_backgroundWorker.DoWork += backgroundWorker_DoWork; // cache time-consuming ListView file information items asynchronously 
        }


        /// <summary>
        /// Handler you can use to provide custom logic for excluding files in the dialog's ListView. 
        /// If the handler is set, those files whose callback returns false are excluded.</summary>
        public Predicate<string> CustomFileFilter;

        /// <summary>
        /// Sets whether multiple items can be selected</summary>
        public bool Multiselect
        {
            set
            {
                listView1.MultiSelect = value;
            }
        }

        /// <summary>
        /// Gets or sets a string that is used to set the initial directory that the user sees</summary>
        public string InitialDirectory
        {
            get { return m_initialDirectory; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (Directory.Exists(value))
                    {
                        m_initialDirectory = value;
                    }
                }
            }
        }
       
        /// <summary>
        /// Gets or sets whether the current directory is restored to its original value if the user changed
        /// the directory while searching for files</summary>
        public bool RestoreDirectory { get; set; }


        /// <summary>
        /// Gets and sets a filter string whose format is pairs of strings. The first string of each
        /// pair is a user-readable description, which by convention includes the extension filter. The
        /// second string of each pair is the extension filter; if there are multiple strings, they are
        /// separated by ';'. The first and second string in each pair is separated by '|'. Each pair is
        /// separated by '|'.
        /// For example:
        /// "Setting file(*.xml)|*.xml" or
        /// "Code files (*.txt;*.cs;*.lua;*.nut;*.py;*.xml;*.dae;*.cg)|*.txt;*.cs;*.lua;*.nut;*.py;*.xml;*.dae;*.cg" or
        /// "Setting file (*.xml;*.txt)|*.xml;*.txt|Any (*.*)|*.*"</summary>
        public string Filter
        {
            get { return m_filter; }
            set
            {
                if (value != null)
                    m_filter = value;
                else
                    m_filter = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the index of the currently selected filter</summary>
        public int FilterIndex
        {
            get { return m_filterIndex; }
            set { m_filterIndex = value; }
        }

        /// <summary>
        /// Gets the resulting selected file name(s)</summary>
        public IEnumerable<string> SelectedFileNames
        {
            get
            {
                return m_selectedFiles;

            }
        }

        internal int[] ColumnWidths
        {
            get
            {
                s_columnWidths = new int[listView1.Columns.Count];
                for (int i = 0; i < listView1.Columns.Count; ++i)
                    s_columnWidths[i] = listView1.Columns[i].Width;
                return s_columnWidths;
            }

            set
            {
                for (int i = 0; i < listView1.Columns.Count; ++i)
                {
                    if (i < value.Length)
                        listView1.Columns[i].Width = value[i];
                }
            }
        }

        /// <summary>
        /// Method called after the selection range of virtual items changed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void listView1_VirtualItemsSelectionRangeChanged(object sender, ListViewVirtualItemsSelectionRangeChangedEventArgs e)
        {
            UpdateSelectedFiles();
        }

        /// <summary>
        /// Method called after the selected index changed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedFiles();
        }

        /// <summary>
        /// Update selected files in the ListView</summary>
        void UpdateSelectedFiles()
        {
            m_selectedFiles.Clear();
            foreach (int index in listView1.SelectedIndices)
            {
                ListViewItem listViewItem = listView1.Items[index];
                if (listViewItem.Tag is FileInfo)
                    m_selectedFiles.Add(((FileInfo)listViewItem.Tag).FullName);
            }

            // update fileNameComboBox.Text to the selected files
            if (listView1.SelectedIndices.Count > 0)
            {
                // restart delay timer
                m_timer.Stop();
                m_timer.Start();
            }
        }

        /// <summary>
        /// Method called when combo box preview key pressed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void fileNameComboBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Return)
            {
                // as new search pattern
                if (StringUtil.IsNullOrEmptyOrWhitespace(fileNameComboBox.Text))
                    ResetActiveFileTypePattern();
                else
                {
                    m_activeFileTypePattern = fileNameComboBox.Text;
                    UpdateFolderListView();
                }
            }
        }

        /// <summary>
        /// Method called when combo box text changed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void fileNameComboBox_TextChanged(object sender, EventArgs e)
        {
            if (StringUtil.IsNullOrEmptyOrWhitespace(fileNameComboBox.Text))
            {
                ResetActiveFileTypePattern();
                UpdateFolderListView();
            }
        }

        private void ResetActiveFileTypePattern()
        {
            if (FilterIndex > 0 && FilterIndex <= fileTypeComboBox.Items.Count)
            {
                //restore predefined pattern
                m_activeFileTypePattern = m_patterns[FilterIndex - 1];
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            m_timer.Stop();

            // Perform selection end logic: update fileNameComboBox.Text to the selected files

            int selectedFiles = SelectedFileNames.Count();
            if (selectedFiles == 1)
                fileNameComboBox.Text = Path.GetFileName(SelectedFileNames.First());
            else if (selectedFiles > 1)
            {
                var sb = new StringBuilder();
                foreach (var fileName in SelectedFileNames)
                {
                    sb.Append(@"""");
                    sb.Append(Path.GetFileName(fileName));
                    sb.Append(@""" ");
                }
                fileNameComboBox.Text = sb.ToString();
            }
            else
                fileNameComboBox.Text = string.Empty;
        }

        /// <summary>
        /// Method called when combo box item drawn</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void lookInComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();

            // Determine the forecolor based on whether or not the item is selected.
            Brush brush;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                brush = Brushes.White;
            }
            else
            {
                brush = Brushes.Black;
            }

            if (e.Index == 0)
                e.Graphics.DrawImage(listView1.SmallImageList.Images[3], new Point(e.Bounds.X + e.Index * 16, e.Bounds.Y));
            else if (e.Index == 1)
                e.Graphics.DrawImage(listView1.SmallImageList.Images[2], new Point(e.Bounds.X + e.Index * 16, e.Bounds.Y));
            else
                e.Graphics.DrawImage(listView1.SmallImageList.Images[0], new Point(e.Bounds.X + e.Index * 16, e.Bounds.Y));
            if (e.Index >= 0)
            {
                // Get the item text.
                string text = ((ComboBox)sender).Items[e.Index].ToString();

                // Draw the item text.
                e.Graphics.DrawString(text, ((Control)sender).Font,
                  brush, e.Bounds.X + (e.Index + 1) * 16, e.Bounds.Y);
            }
        }

        /// <summary>
        /// Method called when combo box column clicked</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == m_listViewItemComparer.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (m_listViewItemComparer.Order == SortOrder.Ascending)
                {
                    m_listViewItemComparer.Order = SortOrder.Descending;
                }
                else
                {
                    m_listViewItemComparer.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                m_listViewItemComparer.SortColumn = e.Column;
                m_listViewItemComparer.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            var list = new List<ListViewItem>(m_cache);
            list.Sort(m_listViewItemComparer);
            list.CopyTo(m_cache, 0);
            listView1.VirtualListSize = 0;
            listView1.VirtualListSize = m_cache.Length;
        }


        /// <summary>
        /// Method called when dialog displayed the first time</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void fileDialogBase_Load(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(Filter))
            {
                var patterns = Filter.Split('|');
                if (patterns.Length > 1)
                {
                    for (int i = 0; i < patterns.Length; i += 2)
                    {
                        fileTypeComboBox.Items.Add(patterns[i]);
                        m_patterns.Add(patterns[i + 1]);
                    }
                    if (FilterIndex > 0 && FilterIndex <= fileTypeComboBox.Items.Count)
                    {
                        fileTypeComboBox.Text = fileTypeComboBox.Items[FilterIndex - 1].ToString();
                        m_activeFileTypePattern = m_patterns[FilterIndex - 1];
                    }

                }

            }

            if (string.IsNullOrEmpty(m_initialDirectory) || !Directory.Exists(m_initialDirectory))
                m_initialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
       
            GoToDirectory(Directory.CreateDirectory(m_initialDirectory));
        }

        private bool IsFileTypeMatch(string fileName)
        {
            if (string.IsNullOrEmpty(m_activeFileTypePattern))
                return true;

            bool matched = false;
            foreach (var pattern in m_activeFileTypePattern.Split(';'))
            {
                var regPattern = pattern.Replace(".", "\\.");//escape '.'
                regPattern = regPattern.Replace("*", ".*");
                regPattern += @"\Z"; // file type match must occur at the end of the string
                if (Regex.Match(fileName, regPattern, RegexOptions.IgnoreCase).Success)
                {
                    matched = true;
                    break;
                }

            }


            return matched;
        }

        private void path_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!m_selecting)
            {
                m_selecting = true;
                if (lookInComboBox.SelectedIndex == 0) // select "Computer"
                {
                    GoToComputer();
                }
                else if (lookInComboBox.SelectedIndex == 1) // select the drive
                {

                    string disk = lookInComboBox.Items[lookInComboBox.SelectedIndex] as string;
                    foreach (var drive in DriveInfo.GetDrives())
                    {
                        if (drive.Name.StartsWith(disk, StringComparison.InvariantCultureIgnoreCase))
                        {
                            GotoDrive(drive);
                            break;
                        }
                    }
                }
                else if (lookInComboBox.SelectedIndex > 1)
                {
                    // assemble the path  
                    var sb = new StringBuilder(lookInComboBox.Items[1] + @"\");
                    for (int i = 2; i <= lookInComboBox.SelectedIndex; ++i)
                        sb.Append(lookInComboBox.Items[i] + @"\");
                    GoToDirectory(Directory.CreateDirectory(sb.ToString()));
                }
                m_selecting = false;
            }

        }

        /// <summary>
        /// Method called after combo box selected index changed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void fileTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            m_activeFileTypePattern = m_patterns[fileTypeComboBox.SelectedIndex];
            UpdateFolderListView();
        }

        void fileNameComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!listView1.Focused)
            {
                listView1.Focus();
                foreach (var item in listView1.Items)
                {
                    ListViewItem listViewItem = item as ListViewItem;
                    if (string.Equals(listViewItem.Text, fileNameComboBox.Text, StringComparison.CurrentCultureIgnoreCase))
                    {

                        listViewItem.Selected = true;
                    }
                    else
                    {
                        listViewItem.Selected = false;
                    }
                }
            }
        }

        private void upButton_Click(object sender, EventArgs e)
        {
            if (m_curDir != null)
            {
                GoToDirectory(m_curDir.Parent);
            }
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            if (m_lastDir != null)
                GoToDirectory(m_lastDir);
        }

        private void newFolder_Click(object sender, EventArgs e)
        {
            if (m_curDir != null)
            {
                string newFolderName = "New folder".Localize();
                int count = 0;
                string tryFolderName = newFolderName;

                bool noMatch;
                do
                {
                    noMatch = true;
                    foreach (var dir in m_curDir.GetDirectories())
                    {
                        if (string.Equals(dir.Name, tryFolderName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            tryFolderName = newFolderName + " (" + count + ")";
                            ++count;
                            noMatch = false;
                            break;
                        }

                    }
                } while (!noMatch);

                m_curDir.CreateSubdirectory(tryFolderName);
                UpdateFolderListView();

                foreach (var item in listView1.Items)
                {
                    ListViewItem listViewItem = item as ListViewItem;
                    if (listViewItem.Text == tryFolderName)
                    {
                        listViewItem.Selected = true;
                        listView1.LabelEdit = true;
                        listViewItem.BeginEdit();
                    }
                }
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {

            if (listView1.SelectedIndices.Count == 1)
            {
                int index = listView1.SelectedIndices[0];
                if (listView1.Items[index].Tag is DirectoryInfo) // Double-click over a folder opens that folder
                {
                    GoToDirectory((DirectoryInfo)listView1.Items[index].Tag);
                }
                else if (listView1.Items[index].Tag is FileInfo) // Double-click over a file opens that file
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if (listView1.Items[index].Tag is DriveInfo)
                {
                    GotoDrive((DriveInfo)listView1.Items[index].Tag);
                }
            }

        }

        /// <summary>
        /// Method called after label edited</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            listView1.LabelEdit = false;
            // Determine if label is changed by checking for null.
            if (e.Label == null)
                return;

            var listViewItem = listView1.Items[e.Item] as ListViewItem;
            if (listViewItem.Tag is DirectoryInfo)
            {
                bool matched = false;
                foreach (var dir in m_curDir.GetDirectories())
                {
                    if (string.Equals(dir.Name, e.Label, StringComparison.CurrentCultureIgnoreCase))
                    {
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                {
                    string sourceDir = ((DirectoryInfo)listViewItem.Tag).FullName;
                    string destDir = m_curDir.FullName + "\\" + e.Label;
                    Directory.Move(sourceDir, destDir);
                }
                else
                {
                    e.CancelEdit = true;
                }
            }
        }

        private string GetRecentFilePath()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly == null)
                assembly = Assembly.GetExecutingAssembly();

            AssemblyName assemblyName = assembly.GetName();
            Version version = assemblyName.Version;
            var versionString = version.Major + "." + version.Minor;


            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return string.Format("{0}\\{1}\\{2}\\RecentFile.lnk", appDataPath, assemblyName.Name, versionString);
        }


        private void GoToDirectory(DirectoryInfo dirInfo)
        {
            if (dirInfo == null)
                return;

            m_lastDir = m_curDir;
            m_curDir = dirInfo;

            UpdateFolderListView();
            UpdateCurrentPath();
        }

        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            e.Item = GetListItem(e.ItemIndex);
        }


        /// <summary>
        /// Method called when virtual items cached</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void listView1_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            for (int i = e.StartIndex; i <= e.EndIndex; ++i)
                GetListItem(i);
        }

        private ListViewItem GetListItem(int i)
        {
            ListViewItem item = null;

            if (m_cache != null && i >= 0 && i < m_cache.Length && m_cache[i] != null)
                return m_cache[i];

            if (i < m_subdirInfos.Length)
            {
                DirectoryInfo dir = m_subdirInfos[i];
                item = new ListViewItem(dir.Name, 0);
                item.ImageIndex = 0;
                item.Tag = dir;
                var subItems = new ListViewItem.ListViewSubItem[]
                        {
                            new ListViewItem.ListViewSubItem(item, dir.LastWriteTime.ToString()),
                            new ListViewItem.ListViewSubItem(item, "")
                        };
                item.SubItems.AddRange(subItems);
            }
            else if (i < +m_subdirInfos.Length + m_fileInfos.Length)
            {
                FileInfo file = m_fileInfos[i - m_subdirInfos.Length];
                item = new ListViewItem(file.Name, 0);
                item.Tag = file;
                item.ImageIndex = 1;
                float kb = file.Length / 1024.0f;
                var subItems = new ListViewItem.ListViewSubItem[]
                        {
                            new ListViewItem.ListViewSubItem(item,file.LastWriteTime.ToString()),
                            new ListViewItem.ListViewSubItem(item, kb.ToString("N0")+ " KB")
                        };
                item.SubItems.AddRange(subItems);
            }

            m_cache[i] = item;
            ++m_numCachedItems;
            return item;
        }



        private void UpdateFolderListView()
        {
            if (m_backgroundWorker.IsBusy)
            {
                m_backgroundWorker.CancelAsync();
                m_resetEvent.WaitOne(); // will block until m_resetEvent.Set() call made              
            }


            // update the folder list view
            listView1.VirtualListSize = 0;
            listView1.Items.Clear();
            if (m_curDir == null) return;
            m_cache = null;
            m_numCachedItems = 0;
            try
            {
                m_subdirInfos = m_curDir.GetDirectories();

            }
            catch (Exception)
            {
                m_subdirInfos = new DirectoryInfo[0];
            }

            try
            {
                fileNameComboBox.Items.Clear();

                List<FileInfo> matchedFileInfos = new List<FileInfo>();
                bool filtering = toolStripFilterButton.Checked;
                foreach (var file in m_curDir.GetFiles())
                {
                    bool matched = !filtering;
                    if (filtering)
                    {                        
                        if (IsFileTypeMatch(file.Name))
                        {
                            if (CustomFileFilter != null )
                                matched = CustomFileFilter(file.FullName);
                            else
                                matched = true;
                        }

                        if (!matched)
                            continue;
                    }

                    if (matched)
                        matchedFileInfos.Add(file);

                }
                m_fileInfos = new FileInfo[matchedFileInfos.Count];
                matchedFileInfos.CopyTo(m_fileInfos);
                listView1.VirtualListSize = m_subdirInfos.Length + m_fileInfos.Length;
                m_cache = new ListViewItem[listView1.VirtualListSize];
            }
            catch (Exception)
            {


            }

            m_backgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Method called when doing work in background</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel &&  m_numCachedItems < m_cache.Length)
            {

                for (int i = 0; i < m_cache.Length; ++i)
                {
                    if (m_cache[i] == null)
                        GetListItem(i);
                }
            }
            m_resetEvent.Set(); // signal that worker is done
        }


        private void UpdateCurrentPath()
        {
            // fill the Look in: comobox
            lookInComboBox.Items.Clear();
            lookInComboBox.Items.Add("Computer".Localize());
            lookInComboBox.Items.AddRange(PathUtil.GetPathElements(m_curDir.FullName));
            m_selecting = true; // set ComboBox test will change the selection index
            lookInComboBox.Text = m_curDir.Name.TrimEnd('\\');
            m_selecting = false;
        }

        private void GoToComputer()
        {
            lookInComboBox.Text = "Computer".Localize();
            listView1.VirtualListSize = 0;
            listView1.Items.Clear();
            m_cache = new ListViewItem[DriveInfo.GetDrives().Count()];
            int index = 0;
            foreach (var drive in DriveInfo.GetDrives())
            {
                var item = new ListViewItem(drive.Name, 0);
                item.ImageIndex = 2;
                item.Tag = drive;
                var subItems = new ListViewItem.ListViewSubItem[] {
                                new ListViewItem.ListViewSubItem(item, "Drive"), 
                                new ListViewItem.ListViewSubItem(item,  drive.Name)};
                item.SubItems.AddRange(subItems);
                m_cache[index++] = item;
            }
            listView1.VirtualListSize = m_cache.Length;
            m_lastDir = m_curDir;
            m_curDir = null;
        }

        private void GotoDrive(DriveInfo drive)
        {
            GoToDirectory(drive.RootDirectory);

        }


        private List<string> m_patterns = new List<string>();
        private List<string> m_selectedFiles = new List<string>();
        private DirectoryInfo m_lastDir;
        private DirectoryInfo m_curDir;
        private string m_activeFileTypePattern;
        private ListViewItemComparer m_listViewItemComparer;
        private Timer m_timer;
        private const int SELECTION_DELAY = 100;


        private string m_filter = string.Empty;
        private int m_filterIndex = 1;
        private string m_initialDirectory;
        private bool m_selecting;

        private ListViewItem[] m_cache;
        private DirectoryInfo[] m_subdirInfos;
        private FileInfo[] m_fileInfos;

        private int m_numCachedItems;

        private BackgroundWorker m_backgroundWorker;
        private AutoResetEvent m_resetEvent = new AutoResetEvent(false);

        private static int[] s_columnWidths;

        internal class ListViewItemComparer : IComparer<ListViewItem>
        {
            private int m_columnToSort;
            private SortOrder m_orderOfSort;
            private readonly CaseInsensitiveComparer m_objectCompare;

            public ListViewItemComparer()
            {
                m_columnToSort = 0;
                m_orderOfSort = SortOrder.None;
                m_objectCompare = new CaseInsensitiveComparer();
            }

            public int Compare(ListViewItem listviewX, ListViewItem listviewY)
            {
                int compareResult = 0;


                if (listviewX.Tag is DirectoryInfo && listviewY.Tag is FileInfo)
                    compareResult = -1; // directory goes before files
                else if (listviewX.Tag is FileInfo && listviewY.Tag is DirectoryInfo)
                    compareResult = 1;
                else if (m_columnToSort == 0) // Case insensitive Compare for file name
                    compareResult = m_objectCompare.Compare(listviewX.SubItems[m_columnToSort].Text,
                                                          listviewY.SubItems[m_columnToSort].Text);
                else if (m_columnToSort == 1) //  modified datetime
                {
                    if (listviewX.Tag is DirectoryInfo && listviewY.Tag is DirectoryInfo)
                    {
                        compareResult = DateTime.Compare(((DirectoryInfo)listviewX.Tag).LastWriteTime,
                                                         ((DirectoryInfo)listviewY.Tag).LastWriteTime);
                    }
                    else if (listviewX.Tag is FileInfo && listviewY.Tag is FileInfo)
                    {
                        compareResult = DateTime.Compare(((FileInfo)listviewX.Tag).LastWriteTime,
                                                         ((FileInfo)listviewY.Tag).LastWriteTime);
                    }
                }
                else if (m_columnToSort == 2) //  size
                {
                    if (listviewX.Tag is DirectoryInfo && listviewY.Tag is DirectoryInfo)
                    {
                        compareResult = m_objectCompare.Compare(listviewX.SubItems[m_columnToSort].Text,
                                                        listviewY.SubItems[m_columnToSort].Text);
                    }
                    else if (listviewX.Tag is FileInfo && listviewY.Tag is FileInfo)
                    {
                        compareResult = ((FileInfo)listviewX.Tag).Length.CompareTo(((FileInfo)listviewY.Tag).Length);
                    }
                }

                // Calculate correct return value based on object comparison
                if (m_orderOfSort == SortOrder.Ascending)
                {
                    // Ascending sort is selected, return normal result of compare operation
                    return compareResult;
                }
                if (m_orderOfSort == SortOrder.Descending)
                {
                    // Descending sort is selected, return negative result of compare operation
                    return (-compareResult);
                }

                // Return '0' to indicate they are equal
                return 0;

            }

            public int SortColumn
            {
                set
                {
                    m_columnToSort = value;
                }
                get
                {
                    return m_columnToSort;
                }
            }

            public SortOrder Order
            {
                set
                {
                    m_orderOfSort = value;
                }
                get
                {
                    return m_orderOfSort;
                }
            }
        }

        private void toolStripFilterButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFolderListView();
        }

    }

}
