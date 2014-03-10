//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Source control check in form</summary>
    public partial class CheckInForm : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="sourceControlService">Source control service</param>
        /// <param name="resources">Resources under source control</param>
        public CheckInForm(ISourceControlService sourceControlService, IEnumerable<IResource> resources)
        {
            InitializeComponent();

            m_resources = new List<IResource>(resources);

            foreach (IResource controlled in resources)
                m_checkBox.Items.Add(controlled.Uri.OriginalString, true);

            m_sourceControlService = sourceControlService;
            m_submitButton.Enabled = false;
        }

        private void m_textBox_TextChanged(object sender, EventArgs e)
        {
            m_submitButton.Enabled = (m_textBox.Text.Length > 0);
        }

        private void submit_Click(object sender, EventArgs e)
        {
            List<Uri> uris = new List<Uri>();
            string description = m_textBox.Text;

            for (int i = 0; i < m_checkBox.Items.Count; i++ )
            {
                CheckState state = m_checkBox.GetItemCheckState(i);
                if (state == CheckState.Checked)
                {
                    uris.Add(m_resources[i].Uri);
                }
            }

            m_sourceControlService.CheckIn(uris, description);

            Close();
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private readonly ISourceControlService m_sourceControlService;
        private readonly List<IResource> m_resources;
    }
}