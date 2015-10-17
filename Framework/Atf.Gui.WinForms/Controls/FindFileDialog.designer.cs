//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog to allow the user to assist an application in finding a missing file</summary>
    partial class FindFileDialog
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used</summary>
        /// <param name="disposing"><c>True</c> if managed resources should be disposed; otherwise, false.</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindFileDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.specifyRadioButton = new System.Windows.Forms.RadioButton();
            this.ignoreRadioButton = new System.Windows.Forms.RadioButton();
            this.ignoreAllRadioButton = new System.Windows.Forms.RadioButton();
            this.OkButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.searchRadioButton = new System.Windows.Forms.RadioButton();
            this.searchAllRadioButton = new System.Windows.Forms.RadioButton();
            this.missingFileLabel = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoEllipsis = true;
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // specifyRadioButton
            // 
            resources.ApplyResources(this.specifyRadioButton, "specifyRadioButton");
            this.specifyRadioButton.Checked = true;
            this.specifyRadioButton.Name = "specifyRadioButton";
            this.specifyRadioButton.TabStop = true;
            this.specifyRadioButton.UseVisualStyleBackColor = true;
            // 
            // ignoreRadioButton
            // 
            resources.ApplyResources(this.ignoreRadioButton, "ignoreRadioButton");
            this.ignoreRadioButton.Name = "ignoreRadioButton";
            this.ignoreRadioButton.UseVisualStyleBackColor = true;
            // 
            // ignoreAllRadioButton
            // 
            resources.ApplyResources(this.ignoreAllRadioButton, "ignoreAllRadioButton");
            this.ignoreAllRadioButton.Name = "ignoreAllRadioButton";
            this.ignoreAllRadioButton.UseVisualStyleBackColor = true;
            // 
            // OkButton
            // 
            this.OkButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.OkButton, "OkButton");
            this.OkButton.Name = "OkButton";
            this.OkButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // searchRadioButton
            // 
            resources.ApplyResources(this.searchRadioButton, "searchRadioButton");
            this.searchRadioButton.Name = "searchRadioButton";
            this.searchRadioButton.UseVisualStyleBackColor = true;
            // 
            // searchAllRadioButton
            // 
            resources.ApplyResources(this.searchAllRadioButton, "searchAllRadioButton");
            this.searchAllRadioButton.Name = "searchAllRadioButton";
            this.searchAllRadioButton.UseVisualStyleBackColor = true;
            // 
            // missingFileLabel
            // 
            resources.ApplyResources(this.missingFileLabel, "missingFileLabel");
            this.missingFileLabel.Name = "missingFileLabel";
            this.missingFileLabel.ReadOnly = true;
            // 
            // FindFileDialog
            // 
            this.AcceptButton = this.OkButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.missingFileLabel);
            this.Controls.Add(this.searchAllRadioButton);
            this.Controls.Add(this.searchRadioButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.OkButton);
            this.Controls.Add(this.ignoreAllRadioButton);
            this.Controls.Add(this.ignoreRadioButton);
            this.Controls.Add(this.specifyRadioButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindFileDialog";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton specifyRadioButton;
        private System.Windows.Forms.RadioButton ignoreRadioButton;
        private System.Windows.Forms.RadioButton ignoreAllRadioButton;
        private System.Windows.Forms.Button OkButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.RadioButton searchRadioButton;
        private System.Windows.Forms.RadioButton searchAllRadioButton;
        private System.Windows.Forms.TextBox missingFileLabel;
    }
}