//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Rename command dialog</summary>
    partial class RenameCommandDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RenameCommandDialog));
            this.baseNameTextBox = new System.Windows.Forms.TextBox();
            this.prefixTextBox = new System.Windows.Forms.TextBox();
            this.suffixTextBox = new System.Windows.Forms.TextBox();
            this.numberCheckBox = new System.Windows.Forms.CheckBox();
            this.firstLabel = new System.Windows.Forms.Label();
            this.firstNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.renameButton = new System.Windows.Forms.Button();
            this.exampleLabel = new System.Windows.Forms.Label();
            this.previewLabel = new System.Windows.Forms.Label();
            this.previewTextBox = new System.Windows.Forms.RichTextBox();
            this.setBaseBtn = new System.Windows.Forms.RadioButton();
            this.keepBaseBtn = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.plusNumberLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.firstNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // baseNameTextBox
            // 
            resources.ApplyResources(this.baseNameTextBox, "baseNameTextBox");
            this.baseNameTextBox.Name = "baseNameTextBox";
            this.baseNameTextBox.TextChanged += new System.EventHandler(this.baseNameTextBox_TextChanged);
            // 
            // prefixTextBox
            // 
            resources.ApplyResources(this.prefixTextBox, "prefixTextBox");
            this.prefixTextBox.Name = "prefixTextBox";
            this.prefixTextBox.TextChanged += new System.EventHandler(this.prefixTextBox_TextChanged);
            // 
            // suffixTextBox
            // 
            resources.ApplyResources(this.suffixTextBox, "suffixTextBox");
            this.suffixTextBox.Name = "suffixTextBox";
            this.suffixTextBox.TextChanged += new System.EventHandler(this.suffixTextBox_TextChanged);
            // 
            // numberCheckBox
            // 
            resources.ApplyResources(this.numberCheckBox, "numberCheckBox");
            this.numberCheckBox.Checked = true;
            this.numberCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.numberCheckBox.Name = "numberCheckBox";
            this.numberCheckBox.UseVisualStyleBackColor = true;
            this.numberCheckBox.CheckedChanged += new System.EventHandler(this.numberCheckBox_CheckedChanged);
            // 
            // firstLabel
            // 
            resources.ApplyResources(this.firstLabel, "firstLabel");
            this.firstLabel.Name = "firstLabel";
            // 
            // firstNumericUpDown
            // 
            resources.ApplyResources(this.firstNumericUpDown, "firstNumericUpDown");
            this.firstNumericUpDown.Maximum = new decimal(new int[] {
            -1,
            2147483647,
            0,
            0});
            this.firstNumericUpDown.Name = "firstNumericUpDown";
            this.firstNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.firstNumericUpDown.ValueChanged += new System.EventHandler(this.firstNumericUpDown_ValueChanged);
            // 
            // renameButton
            // 
            resources.ApplyResources(this.renameButton, "renameButton");
            this.renameButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.renameButton.Name = "renameButton";
            this.renameButton.UseVisualStyleBackColor = true;
            // 
            // exampleLabel
            // 
            resources.ApplyResources(this.exampleLabel, "exampleLabel");
            this.exampleLabel.Name = "exampleLabel";
            // 
            // previewLabel
            // 
            resources.ApplyResources(this.previewLabel, "previewLabel");
            this.previewLabel.Name = "previewLabel";
            // 
            // previewTextBox
            // 
            resources.ApplyResources(this.previewTextBox, "previewTextBox");
            this.previewTextBox.Name = "previewTextBox";
            this.previewTextBox.ReadOnly = true;
            this.previewTextBox.TabStop = false;
            // 
            // setBaseBtn
            // 
            resources.ApplyResources(this.setBaseBtn, "setBaseBtn");
            this.setBaseBtn.Name = "setBaseBtn";
            this.setBaseBtn.UseVisualStyleBackColor = true;
            this.setBaseBtn.CheckedChanged += new System.EventHandler(this.setBaseBtn_CheckedChanged);
            // 
            // keepBaseBtn
            // 
            resources.ApplyResources(this.keepBaseBtn, "keepBaseBtn");
            this.keepBaseBtn.Checked = true;
            this.keepBaseBtn.Name = "keepBaseBtn";
            this.keepBaseBtn.TabStop = true;
            this.keepBaseBtn.UseVisualStyleBackColor = true;
            this.keepBaseBtn.CheckedChanged += new System.EventHandler(this.keepBaseBtn_CheckedChanged);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // plusNumberLabel
            // 
            resources.ApplyResources(this.plusNumberLabel, "plusNumberLabel");
            this.plusNumberLabel.Name = "plusNumberLabel";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // RenameCommandDialog
            // 
            this.AcceptButton = this.renameButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.plusNumberLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.keepBaseBtn);
            this.Controls.Add(this.setBaseBtn);
            this.Controls.Add(this.previewTextBox);
            this.Controls.Add(this.previewLabel);
            this.Controls.Add(this.exampleLabel);
            this.Controls.Add(this.renameButton);
            this.Controls.Add(this.firstNumericUpDown);
            this.Controls.Add(this.firstLabel);
            this.Controls.Add(this.numberCheckBox);
            this.Controls.Add(this.suffixTextBox);
            this.Controls.Add(this.prefixTextBox);
            this.Controls.Add(this.baseNameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "RenameCommandDialog";
            ((System.ComponentModel.ISupportInitialize)(this.firstNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox baseNameTextBox;
        private System.Windows.Forms.TextBox prefixTextBox;
        private System.Windows.Forms.TextBox suffixTextBox;
        private System.Windows.Forms.CheckBox numberCheckBox;
        private System.Windows.Forms.Label firstLabel;
        private System.Windows.Forms.NumericUpDown firstNumericUpDown;
        private System.Windows.Forms.Button renameButton;
        private System.Windows.Forms.Label exampleLabel;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.RichTextBox previewTextBox;
        private System.Windows.Forms.RadioButton setBaseBtn;
        private System.Windows.Forms.RadioButton keepBaseBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label plusNumberLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cancelButton;
    }
}