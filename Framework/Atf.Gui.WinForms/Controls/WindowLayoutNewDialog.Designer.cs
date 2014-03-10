//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls
{
    /// <summary>
    /// New window layout dialog</summary>
    partial class WindowLayoutNewDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WindowLayoutNewDialog));
            this.m_grpLayout = new System.Windows.Forms.GroupBox();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnOk = new System.Windows.Forms.Button();
            this.m_txtLayout = new System.Windows.Forms.TextBox();
            this.m_grpLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_grpLayout
            // 
            resources.ApplyResources(this.m_grpLayout, "m_grpLayout");
            this.m_grpLayout.Controls.Add(this.m_btnCancel);
            this.m_grpLayout.Controls.Add(this.m_btnOk);
            this.m_grpLayout.Controls.Add(this.m_txtLayout);
            this.m_grpLayout.Name = "m_grpLayout";
            this.m_grpLayout.TabStop = false;
            // 
            // m_btnCancel
            // 
            resources.ApplyResources(this.m_btnCancel, "m_btnCancel");
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            // 
            // m_btnOk
            // 
            resources.ApplyResources(this.m_btnOk, "m_btnOk");
            this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOk.Name = "m_btnOk";
            this.m_btnOk.UseVisualStyleBackColor = true;
            this.m_btnOk.Click += new System.EventHandler(this.BtnOkClick);
            // 
            // m_txtLayout
            // 
            resources.ApplyResources(this.m_txtLayout, "m_txtLayout");
            this.m_txtLayout.Name = "m_txtLayout";
            // 
            // LayoutNewDialog
            // 
            this.AcceptButton = this.m_btnOk;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.Controls.Add(this.m_grpLayout);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LayoutNewDialog";
            this.m_grpLayout.ResumeLayout(false);
            this.m_grpLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox m_grpLayout;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnOk;
        private System.Windows.Forms.TextBox m_txtLayout;
    }
}