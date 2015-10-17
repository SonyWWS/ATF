namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Grid control with check boxes for properties</summary>
    partial class GridControlShowHidePropertiesDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GridControlShowHidePropertiesDialog));
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.PropertiesListBox = new System.Windows.Forms.CheckedListBox();
            this.Description = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // OK
            // 
            resources.ApplyResources(this.OK, "OK");
            this.OK.Name = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancel
            // 
            resources.ApplyResources(this.Cancel, "Cancel");
            this.Cancel.Name = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // PropertiesListBox
            // 
            resources.ApplyResources(this.PropertiesListBox, "PropertiesListBox");
            this.PropertiesListBox.CheckOnClick = true;
            this.PropertiesListBox.FormattingEnabled = true;
            this.PropertiesListBox.Name = "PropertiesListBox";
            // 
            // Description
            // 
            resources.ApplyResources(this.Description, "Description");
            this.Description.Name = "Description";
            // 
            // GridControlShowHidePropertiesDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Description);
            this.Controls.Add(this.PropertiesListBox);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Name = "GridControlShowHidePropertiesDialog";
            this.Load += new System.EventHandler(this.GridControlShowHidePropertiesDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.CheckedListBox PropertiesListBox;
        private System.Windows.Forms.Label Description;
    }
}