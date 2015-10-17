namespace Sce.Atf.Applications
{
    /// <summary>
    /// Source control reconcile form</summary>
    partial class ReconcileForm
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used</summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReconcileForm));
            this.label1 = new System.Windows.Forms.Label();
            this.localModifiedListBox = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.localfilesNotInDepotListBox = new System.Windows.Forms.CheckedListBox();
            this.reconcileBtn = new System.Windows.Forms.Button();
            this.cancelBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // localModifiedListBox
            // 
            resources.ApplyResources(this.localModifiedListBox, "localModifiedListBox");
            this.localModifiedListBox.CheckOnClick = true;
            this.localModifiedListBox.FormattingEnabled = true;
            this.localModifiedListBox.Name = "localModifiedListBox";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // localfilesNotInDepotListBox
            // 
            resources.ApplyResources(this.localfilesNotInDepotListBox, "localfilesNotInDepotListBox");
            this.localfilesNotInDepotListBox.FormattingEnabled = true;
            this.localfilesNotInDepotListBox.Name = "localfilesNotInDepotListBox";
            // 
            // reconcileBtn
            // 
            this.reconcileBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.reconcileBtn, "reconcileBtn");
            this.reconcileBtn.Name = "reconcileBtn";
            this.reconcileBtn.UseVisualStyleBackColor = true;
            this.reconcileBtn.Click += new System.EventHandler(this.reconcileBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.cancelBtn, "cancelBtn");
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // ReconcileForm
            // 
            this.AcceptButton = this.reconcileBtn;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.reconcileBtn);
            this.Controls.Add(this.localfilesNotInDepotListBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.localModifiedListBox);
            this.Controls.Add(this.label1);
            this.Name = "ReconcileForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox localModifiedListBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox localfilesNotInDepotListBox;
        private System.Windows.Forms.Button reconcileBtn;
        private System.Windows.Forms.Button cancelBtn;
    }
}