//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// A simple customizable Yes/No/Cancel dialog box</summary>
    public class ConfirmationDialog : System.Windows.Forms.Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="title">Title of dialog</param>
        /// <param name="message">Message to display inside dialog</param>
        public ConfirmationDialog(string title, string message)
        {
            // Required for Windows Form Designer support
            InitializeComponent();
            
            // Make ESC key press the Cancel button.
            CancelButton = m_cancelButton;

            Text = title;
            m_textBox.Text = message;
        }

        /// <summary>
        /// Gets and sets the text that appears on the Yes button. "Yes" is the default.</summary>
        public string YesButtonText
        {
            get { return m_yesButton.Text; }
            set { m_yesButton.Text = value; }
        }

        /// <summary>
        /// Gets and sets the text that appears on the No button. "No" is the default.</summary>
        public string NoButtonText
        {
            get { return m_noButton.Text; }
            set { m_noButton.Text = value; }
        }

        /// <summary>
        /// Gets and sets the text that appears on the Cancel button. "Cancel" is the default.</summary>
        public string CancelButtonText
        {
            get { return m_cancelButton.Text; }
            set { m_cancelButton.Text = value; }
        }

        /// <summary>
        /// Disables the Cancel button, so that is not seen in the dialog</summary>
        public void HideCancelButton()
        {
            m_cancelButton.Hide();

            // Reposition the 'no' and 'yes' buttons to the right, in the place of the hidden cancel button
            int interButtonSpace = m_cancelButton.Location.X - (m_noButton.Location.X + m_noButton.Width);
            int offset = m_cancelButton.Width + interButtonSpace;
            m_noButton.Location = new Point(m_noButton.Location.X + offset, m_noButton.Location.Y);
            m_yesButton.Location = new Point(m_yesButton.Location.X + offset, m_yesButton.Location.Y);
        }

        /// <summary>
        /// Resizes the dialog's width and height (independently) by the specified deltas</summary>
        /// <param name="inWidthDelta">Width delta</param>
        /// <param name="inHeightDelta">Height delta</param>
        public void ResizeContent(int inWidthDelta, int inHeightDelta)
        {
            Size = new Size(Width + inWidthDelta, Height + inHeightDelta);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method
        /// with the code editor</summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmationDialog));
            this.m_yesButton = new System.Windows.Forms.Button();
            this.m_cancelButton = new System.Windows.Forms.Button();
            this.m_noButton = new System.Windows.Forms.Button();
            this.m_textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // m_yesButton
            // 
            resources.ApplyResources(this.m_yesButton, "m_yesButton");
            this.m_yesButton.AutoEllipsis = true;
            this.m_yesButton.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.m_yesButton.Name = "m_yesButton";
            // 
            // m_cancelButton
            // 
            resources.ApplyResources(this.m_cancelButton, "m_cancelButton");
            this.m_cancelButton.AutoEllipsis = true;
            this.m_cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_cancelButton.Name = "m_cancelButton";
            // 
            // m_noButton
            // 
            resources.ApplyResources(this.m_noButton, "m_noButton");
            this.m_noButton.AutoEllipsis = true;
            this.m_noButton.DialogResult = System.Windows.Forms.DialogResult.No;
            this.m_noButton.Name = "m_noButton";
            // 
            // m_textBox
            // 
            resources.ApplyResources(this.m_textBox, "m_textBox");
            this.m_textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.m_textBox.Name = "m_textBox";
            this.m_textBox.ReadOnly = true;
            // 
            // ConfirmationDialog
            // 
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.m_textBox);
            this.Controls.Add(this.m_noButton);
            this.Controls.Add(this.m_cancelButton);
            this.Controls.Add(this.m_yesButton);
            this.Name = "ConfirmationDialog";
            this.ShowInTaskbar = false;
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button m_cancelButton;
        private System.Windows.Forms.Button m_yesButton;
        private System.Windows.Forms.TextBox m_textBox;
        private System.Windows.Forms.Button m_noButton;
    }
}
