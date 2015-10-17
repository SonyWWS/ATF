//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications.NetworkTargetServices
{
    /// <summary>
    /// Dialog to add/edit Target</summary>
    partial class TargetDialog
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing"><c>True</c> if managed resources should be disposed</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TargetDialog));
            this.Targets = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.grpTargets = new System.Windows.Forms.GroupBox();
            this.m_lstTargets = new System.Windows.Forms.ListView();
            this.m_btnDelete = new System.Windows.Forms.Button();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_btnEdit = new System.Windows.Forms.Button();
            this.m_btnAdd = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.grpTargets.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTargets
            // 
            resources.ApplyResources(this.grpTargets, "grpTargets");
            this.grpTargets.Controls.Add(this.m_lstTargets);
            this.grpTargets.Name = "grpTargets";
            this.grpTargets.TabStop = false;
            // 
            // m_lstTargets
            // 
            this.m_lstTargets.CheckBoxes = true;
            this.m_lstTargets.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Targets});
            resources.ApplyResources(this.m_lstTargets, "m_lstTargets");
            this.m_lstTargets.FullRowSelect = true;
            this.m_lstTargets.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.m_lstTargets.HideSelection = false;
            this.m_lstTargets.MultiSelect = false;
            this.m_lstTargets.Name = "m_lstTargets";
            this.m_lstTargets.UseCompatibleStateImageBehavior = false;
            this.m_lstTargets.View = System.Windows.Forms.View.Details;
            this.m_lstTargets.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.m_lstTargets_ItemChecked);
            // 
            // m_btnDelete
            // 
            resources.ApplyResources(this.m_btnDelete, "m_btnDelete");
            this.m_btnDelete.Name = "m_btnDelete";
            this.m_btnDelete.UseVisualStyleBackColor = true;
            this.m_btnDelete.Click += new System.EventHandler(this.m_btnDelete_Click);
            // 
            // m_btnOK
            // 
            resources.ApplyResources(this.m_btnOK, "m_btnOK");
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
            // 
            // m_btnEdit
            // 
            resources.ApplyResources(this.m_btnEdit, "m_btnEdit");
            this.m_btnEdit.Name = "m_btnEdit";
            this.m_btnEdit.UseVisualStyleBackColor = true;
            this.m_btnEdit.Click += new System.EventHandler(this.m_btnEdit_Click);
            // 
            // m_btnAdd
            // 
            resources.ApplyResources(this.m_btnAdd, "m_btnAdd");
            this.m_btnAdd.Name = "m_btnAdd";
            this.m_btnAdd.UseVisualStyleBackColor = true;
            this.m_btnAdd.Click += new System.EventHandler(this.m_btnAdd_Click);
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // TargetDialog
            // 
            resources.ApplyResources(this, "$this");
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_btnAdd);
            this.Controls.Add(this.m_btnEdit);
            this.Controls.Add(this.m_btnDelete);
            this.Controls.Add(this.grpTargets);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TargetDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.grpTargets.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpTargets;
        private System.Windows.Forms.Button m_btnDelete;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.Button m_btnEdit;
        private System.Windows.Forms.Button m_btnAdd;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.ListView m_lstTargets;
        private System.Windows.Forms.ColumnHeader Targets;
    }
}