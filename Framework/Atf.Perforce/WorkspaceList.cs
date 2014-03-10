using System.Windows.Forms;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Workspace list dialog</summary>
    public partial class WorkspaceList : Form
    {
        /// <summary>
        /// Constructor</summary>
        public WorkspaceList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Updates workspace list</summary>
        /// <param name="items">Workspaces to add</param>
        public void UpdateList(string[] items)
        {
            foreach (string item in items)
            {
                // Create ListViewItem.
                ListViewItem lvi = new ListViewItem(item);
                listView1.Items.Add(lvi);
            }
        }

        /// <summary>
        /// Gets selected workspaces</summary>
        public string Selected
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    ListViewItem itemSelect = listView1.SelectedItems[0];
                    return itemSelect.Text;
                }

                return string.Empty;
            }
        }
    }
}