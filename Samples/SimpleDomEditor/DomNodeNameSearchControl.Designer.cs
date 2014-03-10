namespace SimpleDomEditorSample
{
    /// <summary>
    /// DomNode name search controls</summary>
    partial class DomNodeNameSearchControl
    {
        /// <summary>
        /// Required designer variable</summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used</summary>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DomNodeNameSearchControl));
            this.domNodeSearchTextBox1 = new Sce.Atf.Dom.DomNodeNameSearchTextBox();
            this.domNodeReplaceTextBox1 = new Sce.Atf.Applications.ReplaceTextBox();
            this.domNodeSearchResultsListView1 = new Sce.Atf.Dom.DomNodeSearchResultsListView();
            this.searchToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.searchLabel = new System.Windows.Forms.Label();
            this.replaceLabel = new System.Windows.Forms.Label();
            this.replaceToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.resultsToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // domNodeSearchTextBox1
            // 
            this.domNodeSearchTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.domNodeSearchTextBox1.Location = new System.Drawing.Point(77, 13);
            this.domNodeSearchTextBox1.Name = "domNodeSearchTextBox1";
            this.domNodeSearchTextBox1.Size = new System.Drawing.Size(87, 20);
            this.domNodeSearchTextBox1.TabIndex = 1;
            this.searchToolTip.SetToolTip(this.domNodeSearchTextBox1, resources.GetString("domNodeSearchTextBox1.ToolTip"));
            // 
            // domNodeReplaceTextBox1
            // 
            this.domNodeReplaceTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.domNodeReplaceTextBox1.Location = new System.Drawing.Point(257, 13);
            this.domNodeReplaceTextBox1.Name = "domNodeReplaceTextBox1";
            this.domNodeReplaceTextBox1.Size = new System.Drawing.Size(136, 20);
            this.domNodeReplaceTextBox1.TabIndex = 3;
            this.replaceToolTip.SetToolTip(this.domNodeReplaceTextBox1, "This component defines a simple TextBox control class that implements IReplaceUI." +
        "  ");
            // 
            // domNodeSearchResultsListView1
            // 
            this.domNodeSearchResultsListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.domNodeSearchResultsListView1.FullRowSelect = true;
            this.domNodeSearchResultsListView1.GridLines = true;
            this.domNodeSearchResultsListView1.Location = new System.Drawing.Point(3, 46);
            this.domNodeSearchResultsListView1.MultiSelect = false;
            this.domNodeSearchResultsListView1.Name = "domNodeSearchResultsListView1";
            this.domNodeSearchResultsListView1.Size = new System.Drawing.Size(390, 69);
            this.domNodeSearchResultsListView1.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.domNodeSearchResultsListView1.TabIndex = 5;
            this.resultsToolTip.SetToolTip(this.domNodeSearchResultsListView1, "This component defines a simple ListBox control class that implements IResultsUI." +
        "  ");
            this.domNodeSearchResultsListView1.UseCompatibleStateImageBehavior = false;
            this.domNodeSearchResultsListView1.View = System.Windows.Forms.View.Details;
            // 
            // searchToolTip
            // 
            this.searchToolTip.AutomaticDelay = 0;
            this.searchToolTip.AutoPopDelay = 500000000;
            this.searchToolTip.InitialDelay = 500;
            this.searchToolTip.ReshowDelay = 120;
            this.searchToolTip.ToolTipTitle = "Search Input UI";
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(2, 13);
            this.searchLabel.Margin = new System.Windows.Forms.Padding(3);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Padding = new System.Windows.Forms.Padding(3);
            this.searchLabel.Size = new System.Drawing.Size(68, 19);
            this.searchLabel.TabIndex = 6;
            this.searchLabel.Text = "Search For:";
            // 
            // replaceLabel
            // 
            this.replaceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.replaceLabel.AutoSize = true;
            this.replaceLabel.Location = new System.Drawing.Point(170, 14);
            this.replaceLabel.Margin = new System.Windows.Forms.Padding(3);
            this.replaceLabel.Name = "replaceLabel";
            this.replaceLabel.Padding = new System.Windows.Forms.Padding(3);
            this.replaceLabel.Size = new System.Drawing.Size(81, 19);
            this.replaceLabel.TabIndex = 6;
            this.replaceLabel.Text = "Replace With:";
            // 
            // replaceToolTip
            // 
            this.replaceToolTip.AutomaticDelay = 0;
            this.replaceToolTip.AutoPopDelay = 500000000;
            this.replaceToolTip.InitialDelay = 500;
            this.replaceToolTip.ReshowDelay = 120;
            this.replaceToolTip.ToolTipTitle = "Replace Input UI";
            // 
            // resultsToolTip
            // 
            this.resultsToolTip.AutomaticDelay = 0;
            this.resultsToolTip.AutoPopDelay = 500000000;
            this.resultsToolTip.InitialDelay = 500;
            this.resultsToolTip.ReshowDelay = 120;
            this.resultsToolTip.ToolTipTitle = "Results Output UI";
            // 
            // SimpleSearchControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Controls.Add(this.replaceLabel);
            this.Controls.Add(this.searchLabel);
            this.Controls.Add(this.domNodeSearchResultsListView1);
            this.Controls.Add(this.domNodeReplaceTextBox1);
            this.Controls.Add(this.domNodeSearchTextBox1);
            this.MinimumSize = new System.Drawing.Size(411, 131);
            this.Name = "SimpleSearchControl";
            this.Size = new System.Drawing.Size(407, 127);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>
        /// Gets search UI text box</summary>
        public Sce.Atf.Dom.DomNodeNameSearchTextBox SearchUI  { get { return domNodeSearchTextBox1; } }
        /// <summary>
        /// Gets search results list view</summary>
        public Sce.Atf.Dom.DomNodeSearchResultsListView ResultsUI { get { return domNodeSearchResultsListView1; } }
        /// <summary>
        /// Gets replace text box</summary>
        public Sce.Atf.Applications.ReplaceTextBox ReplaceUI { get { return domNodeReplaceTextBox1; } }
        private Sce.Atf.Dom.DomNodeNameSearchTextBox domNodeSearchTextBox1;
        private Sce.Atf.Applications.ReplaceTextBox domNodeReplaceTextBox1;
        private Sce.Atf.Dom.DomNodeSearchResultsListView domNodeSearchResultsListView1;
        private System.Windows.Forms.ToolTip searchToolTip;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.Label replaceLabel;
        private System.Windows.Forms.ToolTip replaceToolTip;
        private System.Windows.Forms.ToolTip resultsToolTip;
    }
}