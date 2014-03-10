using System;
using System.Windows.Forms;

namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Login dialog to Perforce server</summary>
    public partial class LoginDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        public LoginDialog()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
        }

        /// <summary>
        /// Gets password for login to Perforce server</summary>
        public string Password
        {
            get
            {
                return t.Text;
            }
        }

        internal void SetConnectionLabel(string text)
        {
            label3.Text = text;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {

        }

        private void CancelBtn_Click(object sender, EventArgs e)
        {

        }
    }
}