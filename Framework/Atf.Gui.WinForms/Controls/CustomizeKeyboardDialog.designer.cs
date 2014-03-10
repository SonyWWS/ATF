//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog to edit and assign shortcuts to the registered commands</summary>
    partial class CustomizeKeyboardDialog
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomizeKeyboardDialog));
            this.grpCommands = new System.Windows.Forms.GroupBox();
            this.lstCommand = new System.Windows.Forms.ListBox();
            this.btnRemoveShortcut = new System.Windows.Forms.Button();
            this.btnAssignShortcut = new System.Windows.Forms.Button();
            this.txtNewShortcut = new System.Windows.Forms.TextBox();
            this.grpCurKey = new System.Windows.Forms.GroupBox();
            this.btnSetToDefault = new System.Windows.Forms.Button();
            this.lblCurShortcut = new System.Windows.Forms.Label();
            this.grpNewShortcut = new System.Windows.Forms.GroupBox();
            this.btnAddShortcut = new System.Windows.Forms.Button();
            this.grpShortUsed = new System.Windows.Forms.GroupBox();
            this.lblUsedBy = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.cxMenu = new System.Windows.Forms.ContextMenu();
            this.mnuClear = new System.Windows.Forms.MenuItem();
            this.btnOK = new System.Windows.Forms.Button();
            this.grpDescription = new System.Windows.Forms.GroupBox();
            this.lblCmdDescription = new System.Windows.Forms.Label();
            this.btnAllDefault = new System.Windows.Forms.Button();
            this.grpCommands.SuspendLayout();
            this.grpCurKey.SuspendLayout();
            this.grpNewShortcut.SuspendLayout();
            this.grpShortUsed.SuspendLayout();
            this.grpDescription.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpCommands
            // 
            this.grpCommands.Controls.Add(this.lstCommand);
            resources.ApplyResources(this.grpCommands, "grpCommands");
            this.grpCommands.Name = "grpCommands";
            this.grpCommands.TabStop = false;
            // 
            // lstCommand
            // 
            resources.ApplyResources(this.lstCommand, "lstCommand");
            this.lstCommand.FormattingEnabled = true;
            this.lstCommand.Name = "lstCommand";
            this.lstCommand.SelectedIndexChanged += new System.EventHandler(this.lstCommand_SelectedIndexChanged);
            // 
            // btnRemoveShortcut
            // 
            resources.ApplyResources(this.btnRemoveShortcut, "btnRemoveShortcut");
            this.btnRemoveShortcut.Name = "btnRemoveShortcut";
            this.btnRemoveShortcut.UseVisualStyleBackColor = true;
            this.btnRemoveShortcut.Click += new System.EventHandler(this.btnRemoveShortcut_Click);
            // 
            // btnAssignShortcut
            // 
            resources.ApplyResources(this.btnAssignShortcut, "btnAssignShortcut");
            this.btnAssignShortcut.Name = "btnAssignShortcut";
            this.btnAssignShortcut.UseVisualStyleBackColor = true;
            this.btnAssignShortcut.Click += new System.EventHandler(this.btnAssignShortcut_Click);
            // 
            // txtNewShortcut
            // 
            resources.ApplyResources(this.txtNewShortcut, "txtNewShortcut");
            this.txtNewShortcut.Name = "txtNewShortcut";
            this.txtNewShortcut.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtNewShortcut_KeyDown);
            // 
            // grpCurKey
            // 
            this.grpCurKey.Controls.Add(this.btnSetToDefault);
            this.grpCurKey.Controls.Add(this.lblCurShortcut);
            this.grpCurKey.Controls.Add(this.btnRemoveShortcut);
            resources.ApplyResources(this.grpCurKey, "grpCurKey");
            this.grpCurKey.Name = "grpCurKey";
            this.grpCurKey.TabStop = false;
            // 
            // btnSetToDefault
            // 
            resources.ApplyResources(this.btnSetToDefault, "btnSetToDefault");
            this.btnSetToDefault.Name = "btnSetToDefault";
            this.btnSetToDefault.UseVisualStyleBackColor = true;
            this.btnSetToDefault.Click += new System.EventHandler(this.btnSetToDefault_Click);
            // 
            // lblCurShortcut
            // 
            this.lblCurShortcut.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.lblCurShortcut, "lblCurShortcut");
            this.lblCurShortcut.Name = "lblCurShortcut";
            // 
            // grpNewShortcut
            // 
            this.grpNewShortcut.Controls.Add(this.btnAddShortcut);
            this.grpNewShortcut.Controls.Add(this.txtNewShortcut);
            this.grpNewShortcut.Controls.Add(this.btnAssignShortcut);
            resources.ApplyResources(this.grpNewShortcut, "grpNewShortcut");
            this.grpNewShortcut.Name = "grpNewShortcut";
            this.grpNewShortcut.TabStop = false;
            // 
            // btnAddShortcut
            // 
            resources.ApplyResources(this.btnAddShortcut, "btnAddShortcut");
            this.btnAddShortcut.Name = "btnAddShortcut";
            this.btnAddShortcut.UseVisualStyleBackColor = true;
            this.btnAddShortcut.Click += new System.EventHandler(this.btnAddShortcut_Click);
            // 
            // grpShortUsed
            // 
            this.grpShortUsed.Controls.Add(this.lblUsedBy);
            resources.ApplyResources(this.grpShortUsed, "grpShortUsed");
            this.grpShortUsed.Name = "grpShortUsed";
            this.grpShortUsed.TabStop = false;
            // 
            // lblUsedBy
            // 
            this.lblUsedBy.BackColor = System.Drawing.SystemColors.Control;
            resources.ApplyResources(this.lblUsedBy, "lblUsedBy");
            this.lblUsedBy.Name = "lblUsedBy";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // cxMenu
            // 
            this.cxMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuClear});
            this.cxMenu.Popup += new System.EventHandler(this.cxMenue_Popup);
            // 
            // mnuClear
            // 
            this.mnuClear.Index = 0;
            resources.ApplyResources(this.mnuClear, "mnuClear");
            this.mnuClear.Click += new System.EventHandler(this.mnuClear_Click);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            resources.ApplyResources(this.btnOK, "btnOK");
            this.btnOK.Name = "btnOK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // grpDescription
            // 
            this.grpDescription.Controls.Add(this.lblCmdDescription);
            resources.ApplyResources(this.grpDescription, "grpDescription");
            this.grpDescription.Name = "grpDescription";
            this.grpDescription.TabStop = false;
            // 
            // lblCmdDescription
            // 
            resources.ApplyResources(this.lblCmdDescription, "lblCmdDescription");
            this.lblCmdDescription.Name = "lblCmdDescription";
            // 
            // btnAllDefault
            // 
            resources.ApplyResources(this.btnAllDefault, "btnAllDefault");
            this.btnAllDefault.Name = "btnAllDefault";
            this.btnAllDefault.UseVisualStyleBackColor = true;
            this.btnAllDefault.Click += new System.EventHandler(this.btnAllDefault_Click);
            // 
            // CustomizeKeyboardDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnAllDefault);
            this.Controls.Add(this.grpDescription);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.grpShortUsed);
            this.Controls.Add(this.grpNewShortcut);
            this.Controls.Add(this.grpCurKey);
            this.Controls.Add(this.grpCommands);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomizeKeyboardDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.grpCommands.ResumeLayout(false);
            this.grpCurKey.ResumeLayout(false);
            this.grpNewShortcut.ResumeLayout(false);
            this.grpNewShortcut.PerformLayout();
            this.grpShortUsed.ResumeLayout(false);
            this.grpDescription.ResumeLayout(false);
            this.grpDescription.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpCommands;
        private System.Windows.Forms.ListBox lstCommand;
        private System.Windows.Forms.Button btnRemoveShortcut;
        private System.Windows.Forms.Button btnAssignShortcut;
        private System.Windows.Forms.TextBox txtNewShortcut;
        private System.Windows.Forms.GroupBox grpCurKey;
        private System.Windows.Forms.GroupBox grpNewShortcut;
        private System.Windows.Forms.GroupBox grpShortUsed;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ContextMenu cxMenu;
        private System.Windows.Forms.MenuItem mnuClear;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.GroupBox grpDescription;
        private System.Windows.Forms.Label lblCmdDescription;
        private System.Windows.Forms.Label lblCurShortcut;
        private System.Windows.Forms.Label lblUsedBy;
        private System.Windows.Forms.Button btnSetToDefault;
        private System.Windows.Forms.Button btnAddShortcut;
        private System.Windows.Forms.Button btnAllDefault;
    }
}