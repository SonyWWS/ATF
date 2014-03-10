using System;
using System.Windows.Forms;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Perforce server users list dialog</summary>
    public partial class UsersList : Form
    {
        /// <summary>
        /// Constructor</summary>
        public UsersList()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets selected users</summary>
        public string Selected
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    ListViewItem itemSelect = listView1.SelectedItems[0];
                    return itemSelect.Text;
                }
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Updates users list</summary>
        /// <param name="items">Users to add</param>
        public void UpdateList(string[] items)
        {
            //listView1.Items.Clear();

            foreach (string item in items)
            {
                // Create ListViewItem.
                ListViewItem lvi = new ListViewItem(item);
                listView1.Items.Add(lvi);
            }
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {

            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in listView1.SelectedItems)
            {

            }
        }
    }
}