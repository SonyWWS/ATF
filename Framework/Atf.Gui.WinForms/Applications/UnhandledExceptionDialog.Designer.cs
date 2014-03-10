namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class for unhandled exception dialogs</summary>
    partial class UnhandledExceptionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnhandledExceptionDialog));
            this.ExceptionTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ContinueBtn = new System.Windows.Forms.Button();
            this.QuitBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ExceptionTextBox
            // 
            resources.ApplyResources(this.ExceptionTextBox, "ExceptionTextBox");
            this.ExceptionTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.ExceptionTextBox.Name = "ExceptionTextBox";
            this.ExceptionTextBox.ReadOnly = true;
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
            // ContinueBtn
            // 
            resources.ApplyResources(this.ContinueBtn, "ContinueBtn");
            this.ContinueBtn.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.ContinueBtn.Name = "ContinueBtn";
            this.ContinueBtn.UseVisualStyleBackColor = true;
            // 
            // QuitBtn
            // 
            resources.ApplyResources(this.QuitBtn, "QuitBtn");
            this.QuitBtn.DialogResult = System.Windows.Forms.DialogResult.No;
            this.QuitBtn.Name = "QuitBtn";
            this.QuitBtn.UseVisualStyleBackColor = true;
            // 
            // UnhandledExceptionDialog
            // 
            this.AcceptButton = this.ContinueBtn;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.QuitBtn;
            this.Controls.Add(this.QuitBtn);
            this.Controls.Add(this.ContinueBtn);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ExceptionTextBox);
            this.Name = "UnhandledExceptionDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ContinueBtn;
        private System.Windows.Forms.Button QuitBtn;
        public System.Windows.Forms.TextBox ExceptionTextBox;
    }
}