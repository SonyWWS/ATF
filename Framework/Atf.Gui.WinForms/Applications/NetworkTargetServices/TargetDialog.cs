//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Dialog to add/edit Target</summary>
    public partial class TargetDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="targets">Dictionary for target machine name string/Target pairs</param>
        /// <param name="singleSelectionMode">Single selection mode</param>
        /// <param name="defaultPortNumber">Default port number</param>
        /// <param name="canEditPortNumber"><c>True</c> if can edit port number</param>
        /// <param name="protocols">List of supported protocols</param>
        public TargetDialog( Dictionary<string, Target> targets, bool singleSelectionMode, int defaultPortNumber, bool canEditPortNumber, string[] protocols )
        {
            if (targets == null)
                throw new ArgumentNullException();
            
            m_protocols = protocols;
            m_originalTargets = targets;            
            m_canEditPortNumber = canEditPortNumber;
            m_defaultPortNumber = defaultPortNumber;
            m_singleSelectionMode = singleSelectionMode;
            
            m_protocols = protocols;
            InitializeComponent();
            StartPosition = FormStartPosition.CenterParent;
                        
            m_lstTargets.SuspendLayout();
            m_lstTargets.BeginUpdate();
            foreach (Target target in targets.Values)
            {
                Target newTarget = (Target)target.Clone();
                ListViewItem item = CreateListViewItem(newTarget);
                m_lstTargets.Items.Add(item);                
            }
            if(m_lstTargets.Items.Count > 0)
                m_lstTargets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            else
                m_lstTargets.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            m_lstTargets.EndUpdate();
            m_lstTargets.ResumeLayout();

            m_initing = false;
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            if (m_Changed)
            {                
                m_originalTargets.Clear();
                foreach(ListViewItem item in m_lstTargets.Items)
                {
                    Target target = (Target)item.Tag;
                    m_originalTargets.Add(target.Name,target);
                } 
            }
            DialogResult = DialogResult.OK;
        }

        private void m_btnAdd_Click(object sender, EventArgs e)
        {
            TargetEditDialog dlg = new TargetEditDialog(m_defaultPortNumber,m_canEditPortNumber,m_protocols);
            while (dlg.ShowDialog(this) == DialogResult.OK)
            {
                Target t = dlg.GetTarget();

                if (m_lstTargets.Items.ContainsKey(t.Name))
                {
                    MessageBox.Show(this, "Target name already exist".Localize());
                    continue;
                }
                else
                {                    
                    m_lstTargets.Items.Add(CreateListViewItem(t));
                    m_Changed = true;
                    break;
                }
            }
        }

        private void m_btnEdit_Click(object sender, EventArgs e)
        {
            // get selected item
            
            ListView.SelectedListViewItemCollection
                items = m_lstTargets.SelectedItems;
            Target selectedTarget = (items.Count > 0) ? (Target)items[0].Tag : null;
            if (selectedTarget == null)
            {
                MessageBox.Show(this, "Target not selected".Localize());
                return;
            }
            TargetEditDialog dlg = new TargetEditDialog(selectedTarget, m_defaultPortNumber, m_canEditPortNumber, m_protocols);
            while(dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!dlg.Changed)
                    break;                
                if (selectedTarget.Name != items[0].Name)
                {
                    if (m_lstTargets.Items.ContainsKey(selectedTarget.Name))
                    {
                        MessageBox.Show(this, "Target name already exist".Localize());
                        continue;
                    }
                    items[0].Name = selectedTarget.Name;
                }
                items[0].Text = selectedTarget.ToString();
                m_lstTargets.Update();
                m_Changed = true;
                break;
            }
        }

        private void m_lstTargets_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (m_initing) return;
            m_initing = true;
                                 
            ((Target)e.Item.Tag).Selected = e.Item.Checked;
            e.Item.Selected = true;
            if (m_singleSelectionMode)
            {       
                foreach(ListViewItem item in m_lstTargets.Items)
                {
                    if(item == e.Item)                    
                        continue;
                    item.Checked = false;
                    ((Target)item.Tag).Selected = false;
                }               
            }           
            m_initing = false;
            m_Changed = true;   
        }

               
        private void m_btnDelete_Click(object sender, EventArgs e)
        {
            ListView.SelectedListViewItemCollection
                items = m_lstTargets.SelectedItems;
            Target selectedTarget = (items.Count > 0) ? (Target)items[0].Tag : null;

            if (selectedTarget == null)
            {
                MessageBox.Show(this, "Target not selected".Localize());
                return;
            }
            if (MessageBox.Show(this, string.Format("Delete target {0}".Localize(), selectedTarget), "", MessageBoxButtons.YesNo)
                == DialogResult.Yes)
            {                
                m_lstTargets.Items.Remove(items[0]);
                m_Changed = true;
            }
        }     
        private ListViewItem CreateListViewItem(Target target)
        {
            ListViewItem item = new ListViewItem(target.ToString());
            item.Name = target.Name;
            item.Tag = target;
            item.Checked = target.Selected;
            return item;
        }
        private readonly bool m_canEditPortNumber = true;
        private readonly bool m_singleSelectionMode = true;
        private readonly int m_defaultPortNumber = -1;
        private bool m_Changed;
        private bool m_initing = true;
        private readonly Dictionary<string, Target> m_originalTargets;
        private readonly string[] m_protocols;       
    }
}