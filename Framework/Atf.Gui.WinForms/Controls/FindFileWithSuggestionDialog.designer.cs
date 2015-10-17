//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog for finding missing files that uses a suggested path</summary>
    partial class FindFileWithSuggestionDialog
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleans up any resources being used</summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindFileWithSuggestionDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.specifyRadioButton = new System.Windows.Forms.RadioButton();
            this.ignoreRadioButton = new System.Windows.Forms.RadioButton();
            this.ignoreAllRadioButton = new System.Windows.Forms.RadioButton();
            this.OkButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.acceptAllRadioButton = new System.Windows.Forms.RadioButton();
            this.acceptRadioButton = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.missingFileLabel = new System.Windows.Forms.TextBox();
            this.suggestedFileLabel = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
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
            // specifyRadioButton
            // 
            resources.ApplyResources(this.specifyRadioButton, "specifyRadioButton");
            this.specifyRadioButton.Name = "specifyRadioButton";
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
            // acceptAllRadioButton
            // 
            resources.ApplyResources(this.acceptAllRadioButton, "acceptAllRadioButton");
            this.acceptAllRadioButton.Checked = true;
            this.acceptAllRadioButton.Name = "acceptAllRadioButton";
            this.acceptAllRadioButton.TabStop = true;
            this.acceptAllRadioButton.UseVisualStyleBackColor = true;
            // 
            // acceptRadioButton
            // 
            resources.ApplyResources(this.acceptRadioButton, "acceptRadioButton");
            this.acceptRadioButton.Name = "acceptRadioButton";
            this.acceptRadioButton.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // missingFileLabel
            // 
            resources.ApplyResources(this.missingFileLabel, "missingFileLabel");
            this.missingFileLabel.Name = "missingFileLabel";
            this.missingFileLabel.ReadOnly = true;
            // 
            // suggestedFileLabel
            // 
            resources.ApplyResources(this.suggestedFileLabel, "suggestedFileLabel");
            this.suggestedFileLabel.Name = "suggestedFileLabel";
            this.suggestedFileLabel.ReadOnly = true;
            // 
            // FindFileWithSuggestionDialog
            // 
            this.AcceptButton = this.OkButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.suggestedFileLabel);
            this.Controls.Add(this.missingFileLabel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.acceptRadioButton);
            this.Controls.Add(this.acceptAllRadioButton);
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
            this.Name = "FindFileWithSuggestionDialog";
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
        private System.Windows.Forms.RadioButton acceptAllRadioButton;
        private System.Windows.Forms.RadioButton acceptRadioButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox missingFileLabel;
        private System.Windows.Forms.TextBox suggestedFileLabel;
    }
}