//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Grid control with check boxes for properties</summary>
    public partial class GridControlShowHidePropertiesDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="gridView">GridView for instance</param>
        public GridControlShowHidePropertiesDialog(GridView gridView)
        {
            m_gridView = gridView;
            InitializeComponent();
        }

        private void GridControlShowHidePropertiesDialog_Load(object sender, EventArgs e)
        {
            Dictionary<string, bool> userVisibility = m_gridView.GetColumnUserHiddenStates();

            int index = 0;
            foreach (KeyValuePair<string, bool> entry in userVisibility)
            {
                // Load the list with show and hide checkbox values
                PropertiesListBox.Items.Add(entry.Key);
                PropertiesListBox.SetItemChecked(index++, !entry.Value);
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Dictionary<string, bool> userVisibility = new Dictionary<string, bool>();

            // get the checked state from the control
            int index = 0;
            foreach (string item in PropertiesListBox.Items)
            { 
                userVisibility.Add(item, !PropertiesListBox.GetItemChecked(index++));
            }

            m_gridView.SetColumnUserHiddenStates(userVisibility);

            m_gridView.Invalidate();

            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private GridView m_gridView;
    }
}
