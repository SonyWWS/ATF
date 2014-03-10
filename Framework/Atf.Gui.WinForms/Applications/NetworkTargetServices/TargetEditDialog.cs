//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Net;
using System.Windows.Forms;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Form for editing targets</summary>
    public partial class TargetEditDialog : Form
    {
        private Target m_target;
             
        /// <summary>
        /// Constructor</summary>
        /// <param name="defaultPortNumber">Default port number</param>
        /// <param name="canEditPortNumber">True iff can edit port number</param>
        /// <param name="protocols">List of supported protocols</param>
        public TargetEditDialog( int defaultPortNumber, bool canEditPortNumber, string[] protocols )
            : this(null, defaultPortNumber, canEditPortNumber,protocols)
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="target">The target.</param>
        /// <param name="defaultPortNumber">Default port number</param>
        /// <param name="canEditPortNumber">True iff can edit port number</param>
        /// <param name="protocols">List of supported protocols</param>
        public TargetEditDialog( Target target, int defaultPortNumber, bool canEditPortNumber, string[] protocols )
        {
            InitializeComponent();
            if (protocols != null && protocols.Length > 0)
                cmbProtocol.DataSource = protocols;
            else
                cmbProtocol.Enabled = false;
            
            if (target != null)
            {
                Text = "Edit Target".Localize();
                txtName.Text = target.Name;
                txtHost.Text = target.Host;
                txtPort.Text = target.Port.ToString();
                txtPort.ReadOnly = !canEditPortNumber;
                if(cmbProtocol.Enabled)
                    cmbProtocol.SelectedItem = target.Protocol;
                m_target = target;
            }
            else
            {
                Text = "Add Target".Localize();
                if (defaultPortNumber > 0)
                {
                    txtPort.Text = defaultPortNumber.ToString();
                    txtPort.ReadOnly = !canEditPortNumber;
                }
                if (cmbProtocol.Enabled)
                    cmbProtocol.SelectedIndex = 0;
            }
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            if (ValidateAndFillInData())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private bool ValidateAndFillInData()
        {
            string strName = txtName.Text.Trim();
            string strHost = txtHost.Text.Trim();
            string strPort = txtPort.Text;            
            int port = 0;
            
            if (StringUtil.IsNullOrEmptyOrWhitespace(strName))
            {
                MessageBox.Show(this, "Fill target name".Localize());
                txtName.Focus();
                return false;
            }

            if (StringUtil.IsNullOrEmptyOrWhitespace(strHost))
            {
                MessageBox.Show(this, "Fill host name".Localize());
                txtHost.Focus();
                return false;
            }

           
            
            // validate port number
            try
            {
                port = int.Parse(strPort);
                if (port < IPEndPoint.MinPort || port > IPEndPoint.MaxPort)
                    throw new Exception();
            }
            catch
            {
                MessageBox.Show(this,
                    string.Format("Invalid port number".Localize(),
                    IPEndPoint.MinPort, IPEndPoint.MaxPort));
                return false;                
            }

            if (m_target == null)
            {
                m_target = new Target(strName, strHost, port);
                m_changed = true;
            }
            else
            {
                if (m_target.Name != strName
                    || m_target.Host != strHost
                    || m_target.Port != port)
                {
                    m_target.Set(strName, strHost, port);
                    m_changed = true;
                }
            }
            if (cmbProtocol.Enabled)
            {
                m_target.Protocol = (string)cmbProtocol.SelectedItem;
                m_changed = true;
            }
            return true;
        }

        /// <summary>
        /// Gets whether target changed</summary>
        public bool Changed
        {
            get { return m_changed; }
        }
        /// <summary>
        /// Gets the target</summary>
        /// <returns>Target</returns>
        public Target GetTarget()
        {
            if (m_target == null)
                throw new Exception("Target is empty");
            return m_target;
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            m_target = null;
        }

        private bool m_changed;
    }
}
