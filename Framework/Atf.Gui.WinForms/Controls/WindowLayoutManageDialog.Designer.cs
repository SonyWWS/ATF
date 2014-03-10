//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog to manage window layouts</summary>
    partial class WindowLayoutManageDialog
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Cleans up any resources being used</summary>
        /// <param name="disposing">True if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var kv in m_screenshots)
                    kv.Value.Dispose();

                m_screenshots.Clear();

                m_selectLayoutLabel.Dispose();
                m_selectLayoutLabel = null;
            }

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WindowLayoutManageDialog));
            this.m_split = new System.Windows.Forms.SplitContainer();
            this.m_layouts = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.m_screenshot = new System.Windows.Forms.PictureBox();
            this.m_btnRename = new System.Windows.Forms.Button();
            this.m_btnDelete = new System.Windows.Forms.Button();
            this.m_btnClose = new System.Windows.Forms.Button();
            this.m_split.Panel1.SuspendLayout();
            this.m_split.Panel2.SuspendLayout();
            this.m_split.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_screenshot)).BeginInit();
            this.SuspendLayout();
            // 
            // m_split
            // 
            resources.ApplyResources(this.m_split, "m_split");
            this.m_split.Name = "m_split";
            // 
            // m_split.Panel1
            // 
            this.m_split.Panel1.Controls.Add(this.m_layouts);
            // 
            // m_split.Panel2
            // 
            this.m_split.Panel2.Controls.Add(this.m_screenshot);
            // 
            // m_layouts
            // 
            resources.ApplyResources(this.m_layouts, "m_layouts");
            this.m_layouts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.m_layouts.FullRowSelect = true;
            this.m_layouts.LabelEdit = true;
            this.m_layouts.Name = "m_layouts";
            this.m_layouts.UseCompatibleStateImageBehavior = false;
            this.m_layouts.View = System.Windows.Forms.View.Details;
            this.m_layouts.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.LayoutsAfterLabelEdit);
            this.m_layouts.SelectedIndexChanged += new System.EventHandler(this.LayoutsSelectedIndexChanged);
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // m_screenshot
            // 
            resources.ApplyResources(this.m_screenshot, "m_screenshot");
            this.m_screenshot.Name = "m_screenshot";
            this.m_screenshot.TabStop = false;
            // 
            // m_btnRename
            // 
            resources.ApplyResources(this.m_btnRename, "m_btnRename");
            this.m_btnRename.Name = "m_btnRename";
            this.m_btnRename.UseVisualStyleBackColor = true;
            this.m_btnRename.Click += new System.EventHandler(this.BtnRenameClick);
            // 
            // m_btnDelete
            // 
            resources.ApplyResources(this.m_btnDelete, "m_btnDelete");
            this.m_btnDelete.Name = "m_btnDelete";
            this.m_btnDelete.UseVisualStyleBackColor = true;
            this.m_btnDelete.Click += new System.EventHandler(this.BtnDeleteClick);
            // 
            // m_btnClose
            // 
            resources.ApplyResources(this.m_btnClose, "m_btnClose");
            this.m_btnClose.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnClose.Name = "m_btnClose";
            this.m_btnClose.UseVisualStyleBackColor = true;
            // 
            // WindowLayoutManageDialog
            // 
            this.AcceptButton = this.m_btnClose;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_btnClose);
            this.Controls.Add(this.m_btnDelete);
            this.Controls.Add(this.m_btnRename);
            this.Controls.Add(this.m_split);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WindowLayoutManageDialog";
            this.Load += new System.EventHandler(this.WindowLayoutManageDialogLoad);
            this.m_split.Panel1.ResumeLayout(false);
            this.m_split.Panel2.ResumeLayout(false);
            this.m_split.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.m_screenshot)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer m_split;
        private System.Windows.Forms.ListView m_layouts;
        private System.Windows.Forms.PictureBox m_screenshot;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button m_btnRename;
        private System.Windows.Forms.Button m_btnDelete;
        private System.Windows.Forms.Button m_btnClose;

    }
}