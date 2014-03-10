namespace Sce.Atf.Applications
{
    partial class ReconcileForm
    {
        /// <summary>
        /// Required designer variable
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleans up any resources being used
        /// </summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise false</param>
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
        /// the contents of this method with the code editor
        /// </summary>
        private void InitializeComponent()
        {
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
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(190, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Modifiled files(Select files to check out)";
            // 
            // localModifiedListBox
            // 
            this.localModifiedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.localModifiedListBox.CheckOnClick = true;
            this.localModifiedListBox.FormattingEnabled = true;
            this.localModifiedListBox.Location = new System.Drawing.Point(28, 42);
            this.localModifiedListBox.Name = "localModifiedListBox";
            this.localModifiedListBox.Size = new System.Drawing.Size(754, 169);
            this.localModifiedListBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 232);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(241, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Local files not in depot(Selec files to mark for add)";
            // 
            // localfilesNotInDepotListBox
            // 
            this.localfilesNotInDepotListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.localfilesNotInDepotListBox.FormattingEnabled = true;
            this.localfilesNotInDepotListBox.Location = new System.Drawing.Point(31, 270);
            this.localfilesNotInDepotListBox.Name = "localfilesNotInDepotListBox";
            this.localfilesNotInDepotListBox.Size = new System.Drawing.Size(751, 199);
            this.localfilesNotInDepotListBox.TabIndex = 3;
            // 
            // reconcileBtn
            // 
            this.reconcileBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.reconcileBtn.Location = new System.Drawing.Point(606, 501);
            this.reconcileBtn.Name = "reconcileBtn";
            this.reconcileBtn.Size = new System.Drawing.Size(75, 23);
            this.reconcileBtn.TabIndex = 4;
            this.reconcileBtn.Text = "Reconcile";
            this.reconcileBtn.UseVisualStyleBackColor = true;
            this.reconcileBtn.Click += new System.EventHandler(this.reconcileBtn_Click);
            // 
            // cancelBtn
            // 
            this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelBtn.Location = new System.Drawing.Point(704, 500);
            this.cancelBtn.Name = "cancelBtn";
            this.cancelBtn.Size = new System.Drawing.Size(75, 23);
            this.cancelBtn.TabIndex = 5;
            this.cancelBtn.Text = "Cancel";
            this.cancelBtn.UseVisualStyleBackColor = true;
            this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
            // 
            // ReconcileForm
            // 
            this.AcceptButton = this.reconcileBtn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 547);
            this.Controls.Add(this.cancelBtn);
            this.Controls.Add(this.reconcileBtn);
            this.Controls.Add(this.localfilesNotInDepotListBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.localModifiedListBox);
            this.Controls.Add(this.label1);
            this.Name = "ReconcileForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Reconcile Offline Work";
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