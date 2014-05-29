using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Form to set up Perforce server connection: server, user, and workspace</summary>
    public partial class Connections : Form
    {
        /// <summary>
        /// Constructor with ConnectionManager</summary>
        /// <param name="connectionManager">Perforce ConnectionManager</param>
        public Connections(ConnectionManager connectionManager)
        {
            InitializeComponent();

            m_connectionManager = connectionManager;
            ConnectionSelected = (m_connectionManager.RecentConnections.Count == 0) ?
                m_connectionManager.CurrentConnection : m_connectionManager.RecentConnections[0];
            UseAsDefaultConnection = (ConnectionSelected == m_connectionManager.DefaultConnection);
            if (m_connectionManager.RecentConnections.Count > 0)
                InitRecentConnections(m_connectionManager.RecentConnections);
            MinimumSize = new Size(2 * Width / 3, Height);
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            checkBox1.Checked = (ConnectionSelected == m_connectionManager.DefaultConnection);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            UseAsDefaultConnection = checkBox1.Checked;
        }

        /// <summary>
        /// Gets or sets current or most recent connection as "server,user,workspace" string</summary>
        public string ConnectionSelected
        {
            get
            {
                // compose a connection as server,user,workspace
                if (String.IsNullOrEmpty(Server))
                    return "";

                return Server + "," + User + "," + Workspace;
            }

            set
            {
                char[] separator = new char[1] { ',' };
                string[] tokens = value.Split(separator);
                if (tokens.Length == 3)
                {
                    Server = tokens[0];
                    User = tokens[1];

                    if (tokens[2].EndsWith(m_connectionManager.DefaultConnectionMarker))
                        Workspace = tokens[2].Substring(0, tokens[2].Length - m_connectionManager.DefaultConnectionMarker.Length);
                    else
                        Workspace = tokens[2];

                    checkBox1.Checked = ConnectionSelected == m_connectionManager.DefaultConnection;
                }
            }
        }

        internal bool UseAsDefaultConnection
        {
            get { return m_useAsDefaultConnection; }
            set { m_useAsDefaultConnection = value; }
        }

        internal void InitRecentConnections(List<string> recentConnections)
        {
            //m_recentConnections = recentConnections;
            foreach (string item in recentConnections)
            {
                if (item == m_connectionManager.DefaultConnection)
                    comboBoxRecentSettings.Items.Add(item + m_connectionManager.DefaultConnectionMarker);
                else
                    comboBoxRecentSettings.Items.Add(item);
            }
            if (!string.IsNullOrEmpty(ConnectionSelected))
                comboBoxRecentSettings.SelectedIndex = m_connectionManager.RecentConnections.IndexOf(ConnectionSelected);
        }

        private void comboBoxRecentSettings_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox senderComboBox = (ComboBox)sender;
            ConnectionSelected = senderComboBox.Text;
        }


        private void BtnBrowseUser_Click(object sender, EventArgs e)
        {

            string[] userNames = m_connectionManager.GetUsers(Server).ToArray();

            UsersList usersList = new UsersList();
            if (Icon != null)
                usersList.Icon = Icon;
            usersList.SuspendLayout();
            usersList.UpdateList(userNames);
            usersList.ResumeLayout(false);
            usersList.PerformLayout();
            DialogResult dr = usersList.ShowDialog(this);
            if (dr == DialogResult.OK)
                if (!String.IsNullOrEmpty(usersList.Selected))
                {
                    if (User != usersList.Selected)
                    {
                        User = usersList.Selected;
                        Workspace = string.Empty;
                    }
                }


        }

        private void BtnBrowseWorkspace_Click(object sender, EventArgs e)
        {
         
                WorkspaceList workspaceList = new WorkspaceList();
                if (Icon != null)
                    workspaceList.Icon = Icon;
                workspaceList.UpdateList( m_connectionManager.GetWorkspaces(Server, User).ToArray());
                DialogResult dr = workspaceList.ShowDialog(this);
                if (dr == DialogResult.OK)
                    if (!String.IsNullOrEmpty(workspaceList.Selected))
                        Workspace = workspaceList.Selected;


          
          
        }

 
        private string Server { get { return textBoxServer.Text; } set { textBoxServer.Text = value; } }

        private string User { get { return textBoxUser.Text; } set { textBoxUser.Text = value; } }

        private string Workspace { get { return textBoxWorkspace.Text; } set { textBoxWorkspace.Text = value; } }

        private bool m_useAsDefaultConnection;
        private ConnectionManager m_connectionManager;
    }
}