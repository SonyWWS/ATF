namespace Sce.Atf.Perforce
{
    /// <summary>
    /// Form for connecting to Perforce server</summary>
    partial class Connections
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True iff managed resources should be disposed</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Connections));
            this.BtnOK = new System.Windows.Forms.Button();
            this.BtnCancel = new System.Windows.Forms.Button();
            this.LabelDialog = new System.Windows.Forms.Label();
            this.comboBoxRecentSettings = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxServer = new System.Windows.Forms.TextBox();
            this.textBoxUser = new System.Windows.Forms.TextBox();
            this.textBoxWorkspace = new System.Windows.Forms.TextBox();
            this.BtnBrowseUser = new System.Windows.Forms.Button();
            this.BtnBrowseWorkspace = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // BtnOK
            // 
            resources.ApplyResources(this.BtnOK, "BtnOK");
            this.BtnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.BtnOK.Name = "BtnOK";
            this.BtnOK.UseVisualStyleBackColor = true;
            this.BtnOK.Click += new System.EventHandler(this.BtnOK_Click);
            // 
            // BtnCancel
            // 
            resources.ApplyResources(this.BtnCancel, "BtnCancel");
            this.BtnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.BtnCancel.Name = "BtnCancel";
            this.BtnCancel.UseVisualStyleBackColor = true;
            // 
            // LabelDialog
            // 
            resources.ApplyResources(this.LabelDialog, "LabelDialog");
            this.LabelDialog.Name = "LabelDialog";
            // 
            // comboBoxRecentSettings
            // 
            resources.ApplyResources(this.comboBoxRecentSettings, "comboBoxRecentSettings");
            this.comboBoxRecentSettings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRecentSettings.FormattingEnabled = true;
            this.comboBoxRecentSettings.Name = "comboBoxRecentSettings";
            this.comboBoxRecentSettings.SelectionChangeCommitted += new System.EventHandler(this.comboBoxRecentSettings_SelectionChangeCommitted);
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // textBoxServer
            // 
            resources.ApplyResources(this.textBoxServer, "textBoxServer");
            this.textBoxServer.Name = "textBoxServer";
            this.textBoxServer.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // textBoxUser
            // 
            resources.ApplyResources(this.textBoxUser, "textBoxUser");
            this.textBoxUser.Name = "textBoxUser";
            this.textBoxUser.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // textBoxWorkspace
            // 
            resources.ApplyResources(this.textBoxWorkspace, "textBoxWorkspace");
            this.textBoxWorkspace.Name = "textBoxWorkspace";
            this.textBoxWorkspace.TextChanged += new System.EventHandler(this.textBox_TextChanged);
            // 
            // BtnBrowseUser
            // 
            resources.ApplyResources(this.BtnBrowseUser, "BtnBrowseUser");
            this.BtnBrowseUser.Name = "BtnBrowseUser";
            this.BtnBrowseUser.UseVisualStyleBackColor = true;
            this.BtnBrowseUser.Click += new System.EventHandler(this.BtnBrowseUser_Click);
            // 
            // BtnBrowseWorkspace
            // 
            resources.ApplyResources(this.BtnBrowseWorkspace, "BtnBrowseWorkspace");
            this.BtnBrowseWorkspace.Name = "BtnBrowseWorkspace";
            this.BtnBrowseWorkspace.UseVisualStyleBackColor = true;
            this.BtnBrowseWorkspace.Click += new System.EventHandler(this.BtnBrowseWorkspace_Click);
            // 
            // checkBox1
            // 
            resources.ApplyResources(this.checkBox1, "checkBox1");
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // Connections
            // 
            this.AcceptButton = this.BtnOK;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.BtnCancel;
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.BtnBrowseWorkspace);
            this.Controls.Add(this.BtnBrowseUser);
            this.Controls.Add(this.textBoxWorkspace);
            this.Controls.Add(this.textBoxUser);
            this.Controls.Add(this.textBoxServer);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxRecentSettings);
            this.Controls.Add(this.LabelDialog);
            this.Controls.Add(this.BtnCancel);
            this.Controls.Add(this.BtnOK);
            this.Name = "Connections";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnOK;
        private System.Windows.Forms.Button BtnCancel;
        private System.Windows.Forms.Label LabelDialog;
        private System.Windows.Forms.ComboBox comboBoxRecentSettings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxServer;
        private System.Windows.Forms.TextBox textBoxUser;
        private System.Windows.Forms.TextBox textBoxWorkspace;
        private System.Windows.Forms.Button BtnBrowseUser;
        private System.Windows.Forms.Button BtnBrowseWorkspace;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}