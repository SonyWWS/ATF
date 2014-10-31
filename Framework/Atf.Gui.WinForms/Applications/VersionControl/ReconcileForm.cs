using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Source control reconcile form</summary>
    public partial class ReconcileForm : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="sourceControlService">Source control service used</param>
        /// <param name="modified">Enumeration of modified URIs</param>
        /// <param name="notInDepot">Enumeration of URIs not in depot</param>
        public ReconcileForm(ISourceControlService sourceControlService, IEnumerable<Uri> modified, IEnumerable<Uri> notInDepot)
        {
            InitializeComponent();

            m_sourceControlService = sourceControlService;
            m_modified = new List<Uri>(modified);
            m_notInDepot = new List<Uri>(notInDepot);

            foreach (Uri uri in modified)
                localModifiedListBox.Items.Add(uri.LocalPath, true);

            foreach (Uri uri in notInDepot)
                localfilesNotInDepotListBox.Items.Add(uri.LocalPath, true);
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void reconcileBtn_Click(object sender, EventArgs e)
        {
            // check out files that are locally modified but not opened
            for (int i = 0; i < localModifiedListBox.Items.Count; i++)
            {
                CheckState state = localModifiedListBox.GetItemCheckState(i);
                if (state == CheckState.Checked)
                    m_sourceControlService.CheckOut(m_modified[i]);
            }

            // add files that are missing in the depot
            for (int i = 0; i < localfilesNotInDepotListBox.Items.Count; i++)
            {
                CheckState state = localfilesNotInDepotListBox.GetItemCheckState(i);
                if (state == CheckState.Checked)
                    m_sourceControlService.Add(m_notInDepot[i]);
            }

            Close();

        }

        private readonly ISourceControlService m_sourceControlService;
        private readonly List<Uri> m_modified;
        private readonly List<Uri> m_notInDepot;
    }
}
