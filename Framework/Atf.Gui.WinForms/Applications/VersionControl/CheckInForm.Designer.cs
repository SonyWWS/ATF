//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Source control check in form</summary>
    partial class CheckInForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CheckInForm));
            this.m_checkBox = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.m_submitButton = new System.Windows.Forms.Button();
            this.m_cancelButton = new System.Windows.Forms.Button();
            this.m_textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // m_checkBox
            // 
            resources.ApplyResources(this.m_checkBox, "m_checkBox");
            this.m_checkBox.FormattingEnabled = true;
            this.m_checkBox.Name = "m_checkBox";
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
            // m_submitButton
            // 
            resources.ApplyResources(this.m_submitButton, "m_submitButton");
            this.m_submitButton.Name = "m_submitButton";
            this.m_submitButton.UseVisualStyleBackColor = true;
            this.m_submitButton.Click += new System.EventHandler(this.submit_Click);
            // 
            // m_cancelButton
            // 
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.Name = "m_cancelButton";
            this.m_cancelButton.UseVisualStyleBackColor = true;
            this.m_cancelButton.Click += new System.EventHandler(this.cancel_Click);
            // 
            // m_textBox
            // 
            resources.ApplyResources(this.m_textBox, "m_textBox");
            this.m_textBox.Name = "m_textBox";
            this.m_textBox.TextChanged += new System.EventHandler(this.m_textBox_TextChanged);
            // 
            // CheckInForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_textBox);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_submitButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_checkBox);
            this.Name = "CheckInForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox m_checkBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button m_submitButton;
        private System.Windows.Forms.Button m_cancelButton;
        private System.Windows.Forms.TextBox m_textBox;
    }
}