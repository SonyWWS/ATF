//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Form for presenting the load and save settings operations to the user. Used only
    /// by SettingsService.</summary>
    partial class SettingsLoadSaveDialog
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsLoadSaveDialog));
            this.lblTitle = new System.Windows.Forms.Label();
            this.m_saveRadioButton = new System.Windows.Forms.RadioButton();
            this.m_loadRadioButton = new System.Windows.Forms.RadioButton();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnProceed = new System.Windows.Forms.Button();
            this.m_lblExport = new System.Windows.Forms.Label();
            this.m_lblImport = new System.Windows.Forms.Label();
            this.m_grpImportExport = new System.Windows.Forms.GroupBox();
            this.m_grpImportExport.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            resources.ApplyResources(this.lblTitle, "lblTitle");
            this.lblTitle.Name = "lblTitle";
            // 
            // m_saveRadioButton
            // 
            resources.ApplyResources(this.m_saveRadioButton, "m_saveRadioButton");
            this.m_saveRadioButton.Name = "m_saveRadioButton";
            this.m_saveRadioButton.TabStop = true;
            this.m_saveRadioButton.UseVisualStyleBackColor = true;
            // 
            // m_loadRadioButton
            // 
            resources.ApplyResources(this.m_loadRadioButton, "m_loadRadioButton");
            this.m_loadRadioButton.Name = "m_loadRadioButton";
            this.m_loadRadioButton.TabStop = true;
            this.m_loadRadioButton.UseVisualStyleBackColor = true;
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_btnProceed
            // 
            resources.ApplyResources(this.m_btnProceed, "m_btnProceed");
            this.m_btnProceed.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnProceed.Name = "m_btnProceed";
            this.m_btnProceed.UseVisualStyleBackColor = true;
            this.m_btnProceed.Click += new System.EventHandler(this.m_btnProceed_Click);
            // 
            // m_lblExport
            // 
            resources.ApplyResources(this.m_lblExport, "m_lblExport");
            this.m_lblExport.Name = "m_lblExport";
            this.m_lblExport.Tag = "";
            // 
            // m_lblImport
            // 
            resources.ApplyResources(this.m_lblImport, "m_lblImport");
            this.m_lblImport.Name = "m_lblImport";
            // 
            // m_grpImportExport
            // 
            this.m_grpImportExport.Controls.Add(this.m_saveRadioButton);
            this.m_grpImportExport.Controls.Add(this.m_lblImport);
            this.m_grpImportExport.Controls.Add(this.m_loadRadioButton);
            this.m_grpImportExport.Controls.Add(this.m_lblExport);
            resources.ApplyResources(this.m_grpImportExport, "m_grpImportExport");
            this.m_grpImportExport.Name = "m_grpImportExport";
            this.m_grpImportExport.TabStop = false;
            // 
            // SettingsLoadSaveDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_grpImportExport);
            this.Controls.Add(this.m_btnProceed);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SettingsLoadSaveDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.m_grpImportExport.ResumeLayout(false);
            this.m_grpImportExport.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.RadioButton m_saveRadioButton;
        private System.Windows.Forms.RadioButton m_loadRadioButton;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnProceed;
        private System.Windows.Forms.Label m_lblExport;
        private System.Windows.Forms.Label m_lblImport;
        private System.Windows.Forms.GroupBox m_grpImportExport;
    }
}