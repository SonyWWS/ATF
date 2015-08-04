//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// A "Help About..." dialog, with Sony Logo and assembly list</summary>
    public class AboutSysInfoDialog : System.Windows.Forms.Form
    {
        /// <summary>
        /// Constructor</summary>
        public AboutSysInfoDialog()
        {
            // Required for Windows Form Designer support
            InitializeComponent();

            m_subItemComparer = new SubItemComparer(assemblyListView);
            assemblyListView.ListViewItemSorter = m_subItemComparer;
            assemblyListView.ColumnClick += assemblyListView_ColumnClick;

            // Fill in assembly list
            try 
            {
                // Get all modules
                foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
                {
                    // Get version info
                    string versionString = "0.0"; //the default of Version()
                    try
                    {
                        FileVersionInfo fileVersionInfo = module.FileVersionInfo;
                        if (!String.IsNullOrEmpty(fileVersionInfo.FileVersion))
                        {
                            versionString = String.Format("{0}.{1}.{2}.{3}",
                                fileVersionInfo.FileMajorPart,
                                fileVersionInfo.FileMinorPart,
                                fileVersionInfo.FileBuildPart,
                                fileVersionInfo.FilePrivatePart);
                        }
                    }
                    catch (FileNotFoundException)
                    {
                        //detour.dll from nVidia caused this exception on a Windows 8 laptop
                    }

                    var item = new ListViewItem();
                    item.Text = module.ModuleName;


                    var subItem = new ListViewItem.ListViewSubItem(item, versionString);
                    subItem.Tag = new Version(versionString);
                    item.SubItems.Add(subItem);

                    // Get file date info
                    DateTime lastWriteDate = File.GetLastWriteTime(module.FileName);
                    string dateStr = lastWriteDate.ToString("g");

                    subItem = new ListViewItem.ListViewSubItem(item, dateStr);
                    subItem.Tag = lastWriteDate;
                    item.SubItems.Add(subItem);

                    assemblyListView.Items.Add(item);
                }
            }
            catch (Exception e)
            {
                Outputs.WriteLine(OutputMessageType.Error,e.Message);
            }
        }

        private void assemblyListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            m_subItemComparer.SortColumn = e.Column;
            assemblyListView.Sorting =
                (assemblyListView.Sorting == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending;
        }

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; 
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if( disposing )
            {
                if(components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Column Sorting

        /// <summary>
        /// Sorting support for ListView</summary>
        private class SubItemComparer : IComparer
        {
            private readonly ListView _listView;
            private int _sortColumn;

            /// <summary>
            /// Consructor</summary>
            /// <param name="listView">ListView to sort</param>
            public SubItemComparer(ListView listView)
            {
                _listView = listView;
            }

            /// <summary>
            /// Gets or set column of sort keys</summary>
            public int SortColumn
            {
                get { return _sortColumn; }
                set { _sortColumn = value; }
            }

            /// <summary>
            /// Compare function for IComparer</summary>
            /// <param name="x">First subitem</param>
            /// <param name="y">Second subitem</param>
            /// <returns>Sorting result, as per IComparer</returns>
            public int Compare(object x, object y)
            {
                ListViewItem.ListViewSubItem sub1 = ((ListViewItem)x).SubItems[_sortColumn];
                ListViewItem.ListViewSubItem sub2 = ((ListViewItem)y).SubItems[_sortColumn];
                if (_listView.Sorting == SortOrder.Descending)
                {
                    ListViewItem.ListViewSubItem temp = sub1;
                    sub1 = sub2;
                    sub2 = temp;
                }
                return CaseInsensitiveComparer.Default.Compare(sub1.Tag, sub2.Tag);
            }
        }

        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutSysInfoDialog));
            this.assemblyListView = new System.Windows.Forms.ListView();
            this.assemblyColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.versionColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.dateColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.okButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // assemblyListView
            // 
            this.assemblyListView.AllowColumnReorder = true;
            resources.ApplyResources(this.assemblyListView, "assemblyListView");
            this.assemblyListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.assemblyColumnHeader,
            this.versionColumnHeader,
            this.dateColumnHeader});
            this.assemblyListView.FullRowSelect = true;
            this.assemblyListView.Name = "assemblyListView";
            this.assemblyListView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.assemblyListView.UseCompatibleStateImageBehavior = false;
            this.assemblyListView.View = System.Windows.Forms.View.Details;
            // 
            // assemblyColumnHeader
            // 
            resources.ApplyResources(this.assemblyColumnHeader, "assemblyColumnHeader");
            // 
            // versionColumnHeader
            // 
            resources.ApplyResources(this.versionColumnHeader, "versionColumnHeader");
            // 
            // dateColumnHeader
            // 
            resources.ApplyResources(this.dateColumnHeader, "dateColumnHeader");
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // AboutSysInfoDialog
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.assemblyListView);
            this.Name = "AboutSysInfoDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.AboutSysInfoDialog_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private void AboutSysInfoDialog_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Performs custom actions when OK button clicked</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void okButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private ListView assemblyListView;
        private ColumnHeader assemblyColumnHeader;
        private ColumnHeader versionColumnHeader;
        private ColumnHeader dateColumnHeader;
        private Button okButton;
        // Required designer variable.
        private readonly System.ComponentModel.Container components = null;
        private readonly SubItemComparer m_subItemComparer;
    }
}
