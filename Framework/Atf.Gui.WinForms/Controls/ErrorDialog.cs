//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog to display error messages to user; used by ErrorDialogService</summary>
    public partial class ErrorDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        public ErrorDialog()
        {
            InitializeComponent();

            okButton.Click += okButton_Click;
            checkBox1.Click += checkBox1_Click;
        }

        /// <summary>
        /// Constructor where the Message property will be set to the given message text
        /// plus the exception's text</summary>
        /// <param name="message">Text to be prepended to the exception's text</param>
        /// <param name="caption">Text to set the window's caption to</param>
        /// <param name="exception">Exception whose error message and call stack will be displayed</param>
        public ErrorDialog(string message, string caption, Exception exception)
            : this()
        {
            Text = caption;

            // Call ToString() to get the call stack. The Message property may not include that.
            var sb = new StringBuilder(message);
            sb.AppendLine();
            sb.AppendLine(exception.ToString());
            textBox1.Text = sb.ToString();

            // Seems like for an exception, the default should be not to offer the option of supressing these.
            checkBox1.Visible = false;

            // Exceptions with the call stack can be large.
            textBox1.ScrollBars = ScrollBars.Both;
        }

        /// <summary>
        /// Shows a modal error dialog with the given message followed by the exception's text</summary>
        /// <param name="message">Text to be prepended to the exception's text</param>
        /// <param name="caption">Text to set the window's caption to</param>
        /// <param name="exception">Exception whose error message and call stack will be displayed</param>
        /// <returns>Currently, only DialogResult.OK will be returned</returns>
        public static DialogResult Show(string message, string caption, Exception exception)
        {
            var dlg = new ErrorDialog(message, caption, exception);
            return dlg.ShowDialog();
        }

        /// <summary>
        /// Gets or sets message ID, identifying message type</summary>
        public string MessageId
        {
            get { return m_messageId; }
            set { m_messageId = value; }
        }

        /// <summary>
        ///  Gets or sets message that is displayed in error dialog</summary>
        public string Message
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        /// <summary>
        /// Event that is raised when the Supress Message checkbox has been clicked</summary>
        public event EventHandler SuppressMessageClicked;

        /// <summary>
        ///  Gets or sets whether user has requested that message not be shown in the future</summary>
        public bool SuppressMessage
        {
            get { return checkBox1.Checked; }
            set { checkBox1.Checked = value; }
        }

        /// <summary>
        /// Gets or sets whether the suppress message checkbox is visible</summary>
        public bool ShowSupressMessageCheckbox
        {
            get { return checkBox1.Visible; }
            set { checkBox1.Visible = value; }
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void checkBox1_Click(object sender, System.EventArgs e)
        {
            SuppressMessageClicked.Raise(this, EventArgs.Empty);
        }

        private string m_messageId;
    }
}
