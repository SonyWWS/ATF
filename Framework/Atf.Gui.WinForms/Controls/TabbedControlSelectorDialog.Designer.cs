namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog that enumerates all controls accessable by the specified IControlHostService.
    /// 
    /// A TabbedControlSelectorDialog persists until the Ctrl button is released. Until then, 
    /// it consumes Ctrl+[Tab|Up|Down|Left|Right] key presses to switch the currently selected 
    /// control in the enumeration. When Ctrl is released, the control corresponding to the 
    /// selection in the enumeration is given input focus.
    /// 
    /// Enumeration of controls is separated into two lists: one including all controls in the currently
    /// active pane, and another for all other controls (i.e., those NOT in the active pane). Use
    /// Ctrl+Left and Ctrl+Right to jump between the two lists.</summary>
    partial class TabbedControlSelectorDialog
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
            System.Windows.Forms.Label label1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TabbedControlSelectorDialog));
            System.Windows.Forms.Label label2;
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.unfocusedPaneListBox = new System.Windows.Forms.ListBox();
            this.focusedPaneListBox = new System.Windows.Forms.ListBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // label2
            // 
            resources.ApplyResources(label2, "label2");
            label2.Name = "label2";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.SizingGrip = false;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            resources.ApplyResources(this.toolStripStatusLabel, "toolStripStatusLabel");
            // 
            // unfocusedPaneListBox
            // 
            this.unfocusedPaneListBox.BackColor = System.Drawing.SystemColors.Window;
            this.unfocusedPaneListBox.FormattingEnabled = true;
            resources.ApplyResources(this.unfocusedPaneListBox, "unfocusedPaneListBox");
            this.unfocusedPaneListBox.Name = "unfocusedPaneListBox";
            this.unfocusedPaneListBox.TabStop = false;
            this.unfocusedPaneListBox.UseTabStops = false;
            this.unfocusedPaneListBox.SelectedIndexChanged += new System.EventHandler(this.unfocusedPaneListBox_SelectionChanged);
            this.unfocusedPaneListBox.SelectedValueChanged += new System.EventHandler(this.unfocusedPaneListBox_SelectionChanged);
            // 
            // focusedPaneListBox
            // 
            this.focusedPaneListBox.BackColor = System.Drawing.SystemColors.Window;
            this.focusedPaneListBox.FormattingEnabled = true;
            resources.ApplyResources(this.focusedPaneListBox, "focusedPaneListBox");
            this.focusedPaneListBox.Name = "focusedPaneListBox";
            this.focusedPaneListBox.TabStop = false;
            this.focusedPaneListBox.UseTabStops = false;
            this.focusedPaneListBox.SelectedIndexChanged += new System.EventHandler(this.focusedPaneListBox_SelectionChanged);
            this.focusedPaneListBox.SelectedValueChanged += new System.EventHandler(this.focusedPaneListBox_SelectionChanged);
            // 
            // TabbedControlSelectorDialog
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ControlBox = false;
            this.Controls.Add(label2);
            this.Controls.Add(label1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.focusedPaneListBox);
            this.Controls.Add(this.unfocusedPaneListBox);
            this.DoubleBuffered = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TabbedControlSelectorDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.TopMost = true;
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TabbedControlSelectorDialog_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TabbedControlSelectorDialog_KeyUp);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.ListBox unfocusedPaneListBox;
        private System.Windows.Forms.ListBox focusedPaneListBox;
    }
}