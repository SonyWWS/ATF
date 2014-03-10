//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Progress dialog</summary>
    public class ProgressDialog : System.Windows.Forms.Form
    {
        /// <summary>
        /// Gets and sets whether dialog is cancelled</summary>
        public bool IsCanceled
        {
            get { return m_canceled; }
            set { m_canceled = value; }
        }

        /// <summary>
        /// Gets and sets whether dialog can be cancelled</summary>
        public bool CanCancel
        {
            get { return m_canCancel; }
            set
            {
                m_canCancel = value;
                cancelButton.Enabled = m_canCancel;
            }
        }

        /// <summary>
        /// Gets and sets whether Cancel button is visible</summary>
        public bool CancelVisible
        {
            get { return cancelButton.Visible; }
            set { cancelButton.Visible = value; }
        }

        /// <summary>
        /// Gets and sets percentage of progress completed</summary>
        public int Percent
        {
            get { return progressBar1.Value; }
            set { progressBar1.Value = value; }
        }

        /// <summary>
        /// Gets and sets dialog label</summary>
        public string Label
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        /// <summary>
        /// Gets and sets dialog title</summary>
        public string Title
        {
            get { return this.Text; }
            set { this.Text = value; }
        }

        /// <summary>
        /// Constructor</summary>
        public ProgressDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event that is raised when progress dialog is cancelled</summary>
        public event EventHandler Cancelled;

        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressDialog));
            progressBar1 = new System.Windows.Forms.ProgressBar();
            label1 = new System.Windows.Forms.Label();
            cancelButton = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // progressBar1
            // 
            resources.ApplyResources(progressBar1, "progressBar1");
            progressBar1.Name = "progressBar1";
            progressBar1.Step = 5;
            // 
            // label1
            // 
            resources.ApplyResources(label1, "label1");
            label1.Name = "label1";
            // 
            // cancelButton
            // 
            resources.ApplyResources(cancelButton, "cancelButton");
            cancelButton.Name = "cancelButton";
            cancelButton.Click += OnCancelled;
            // 
            // ProgressDialog
            // 
            resources.ApplyResources(this, "$this");
            CausesValidation = false;
            ControlBox = false;
            Controls.Add(cancelButton);
            Controls.Add(label1);
            Controls.Add(progressBar1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ProgressDialog";
            ShowInTaskbar = false;
            TopMost = false;
            ResumeLayout(false);
        }

        /// <summary>
        /// Raises the Cancelled event</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        protected virtual void OnCancelled(object sender, EventArgs e)
        {
            m_canceled = true;
            Cancelled.Raise(this, e);
        }

        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cancelButton;
        private bool m_canceled;
        private bool m_canCancel;
    }
}
