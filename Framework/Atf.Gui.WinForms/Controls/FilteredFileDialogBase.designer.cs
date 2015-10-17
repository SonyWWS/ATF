namespace Sce.Atf.Controls
{
    /// <summary>
    /// Base class for custom OpenFilteredFileDialog and SaveFilteredFileDialog</summary>
    /// <remarks>
    /// All properties should be equivalent to the properties with the same names in System.Windows.Forms.FileDialog. 
    /// Unlike System.Windows.Forms.FileDialog or Sce.Atf.CustomFileDialog, this is a Windows Forms implementation without going out to the Win32 methods. 
    /// It has a CustomFileFilter callback, which can be used to exclude files in the dialog's ListView.</remarks>
    partial class FilteredFileDialogBase
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilteredFileDialogBase));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripFilterButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.viewOptionsToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.listView1 = new System.Windows.Forms.ListView();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.fileTypeComboBox = new System.Windows.Forms.ComboBox();
            this.fileNameComboBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lookInComboBox = new System.Windows.Forms.ComboBox();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton2,
            this.toolStripFilterButton,
            this.toolStripButton3,
            this.viewOptionsToolStripDropDownButton});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButton2, "toolStripButton2");
            this.toolStripButton2.Name = "toolStripButton2";
            // 
            // toolStripFilterButton
            // 
            this.toolStripFilterButton.Checked = true;
            this.toolStripFilterButton.CheckOnClick = true;
            this.toolStripFilterButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripFilterButton, "toolStripFilterButton");
            this.toolStripFilterButton.Name = "toolStripFilterButton";
            this.toolStripFilterButton.CheckedChanged += new System.EventHandler(this.toolStripFilterButton_CheckedChanged);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.toolStripButton3, "toolStripButton3");
            this.toolStripButton3.Name = "toolStripButton3";
            // 
            // viewOptionsToolStripDropDownButton
            // 
            this.viewOptionsToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            resources.ApplyResources(this.viewOptionsToolStripDropDownButton, "viewOptionsToolStripDropDownButton");
            this.viewOptionsToolStripDropDownButton.Name = "viewOptionsToolStripDropDownButton";
            // 
            // listView1
            // 
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.Name = "listView1";
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // button1
            // 
            resources.ApplyResources(this.button1, "button1");
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // fileTypeComboBox
            // 
            resources.ApplyResources(this.fileTypeComboBox, "fileTypeComboBox");
            this.fileTypeComboBox.FormattingEnabled = true;
            this.fileTypeComboBox.Name = "fileTypeComboBox";
            // 
            // fileNameComboBox
            // 
            resources.ApplyResources(this.fileNameComboBox, "fileNameComboBox");
            this.fileNameComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.fileNameComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.fileNameComboBox.FormattingEnabled = true;
            this.fileNameComboBox.Name = "fileNameComboBox";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // lookInComboBox
            // 
            this.lookInComboBox.FormattingEnabled = true;
            resources.ApplyResources(this.lookInComboBox, "lookInComboBox");
            this.lookInComboBox.Name = "lookInComboBox";
            // 
            // FilteredFileDialogBase
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lookInComboBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.fileNameComboBox);
            this.Controls.Add(this.fileTypeComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "FilteredFileDialogBase";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox fileTypeComboBox;
        private System.Windows.Forms.ToolStripDropDownButton viewOptionsToolStripDropDownButton;
        private System.Windows.Forms.ComboBox fileNameComboBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox lookInComboBox;
        private System.Windows.Forms.ToolStripButton toolStripFilterButton;

    }
}