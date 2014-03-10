//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using Sce.Atf.Controls;

using Timer = System.Threading.Timer;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that provides status UI</summary>
    [Export(typeof(IStatusService))]
    [Export(typeof(IInitializable))]
    [Export(typeof(StatusService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StatusService : IStatusService, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="mainForm">Application main form</param>
        [ImportingConstructor]
        public StatusService(Form mainForm)
        {
            m_mainForm = mainForm;

            m_statusStrip = new StatusStrip();
            m_statusStrip.Name = "StatusBar";
            m_statusStrip.Dock = DockStyle.Bottom;
            // statusStrip items  are laid out horizontally and overflow as necessary.
            m_statusStrip.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
            m_statusStrip.ShowItemToolTips = false;

            // main status text
            m_mainPanel = new ToolStripStatusLabel();
            m_mainPanel.Width = 256;
            m_mainPanel.AutoSize = true;
            m_mainPanel.Spring = true;
            m_mainPanel.TextAlign = ContentAlignment.MiddleLeft;
            m_statusStrip.Items.Add(m_mainPanel);

            m_progressTimer = new Timer(
                progressCallback, this, Timeout.Infinite, ProgressInterval);

            m_progressDialog = new ThreadSafeProgressDialog(false, true);
            m_progressDialog.Cancelled += progressDialog_Cancelled;
        }

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // first choice is a ToolStripContainer to hold the status strip; otherwise,
            //  add as first child of main Form.
            foreach (Control control in m_mainForm.Controls)
            {
                m_toolStripContainer = control as ToolStripContainer;
                if (m_toolStripContainer != null)
                {
                    m_toolStripContainer.BottomToolStripPanel.Controls.Add(m_statusStrip);
                    break;
                }
            }

            if (m_toolStripContainer == null)
            {
                m_mainForm.Controls.Add(m_statusStrip);
            }

            ShowStatus("Ready".Localize("Application is ready"));
        }

        #endregion

        /// <summary>
        /// Gets the status bar control that displays various kinds of status information</summary>
        public  Control StatusControl
        {
            get { return m_statusStrip; }
        }

        #region IStatusService members

        /// <summary>
        /// Shows a status message in the main panel</summary>
        /// <param name="status">Status message</param>
        public void ShowStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) status = "Ready".Localize();
            m_mainPanel.Text = status;
        }

        /// <summary>
        /// Adds a new text status panel</summary>
        /// <param name="width">Text status panel width</param>
        /// <returns>Text panel</returns>
        public IStatusText AddText(int width)
        {
            TextPanel textPanel = new TextPanel(width);
            textPanel.Name = "$Status" + (s_controlCount++);
            m_statusStrip.Items.Add(textPanel);
            return textPanel;
        }

        /// <summary>
        /// Adds a new image status panel</summary>
        /// <returns>IStatusImage panel</returns>
        public IStatusImage AddImage()
        {
            ImagePanel imagePanel = new ImagePanel();
            imagePanel.Name = "$Status" + (s_controlCount++);
            m_statusStrip.Items.Add(imagePanel);
            return imagePanel;
        }

        /// <summary>
        /// Begins progress meter display where client can manually update progress.
        /// The Cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        public void BeginProgress(string message)
        {
            BeginProgress(message, 0, true);
        }

        /// <summary>
        /// Begins progress meter display where progress is updated automatically.
        /// The Cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        public void BeginProgress(string message, int expectedDuration)
        {
            BeginProgress(message, expectedDuration, true);
        }

        /// <summary>
        /// Begins progress meter display where client can manually update progress.</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="canCancel">True iff the Cancel button appears and is enabled</param>
        public void BeginProgress(string message, bool canCancel)
        {
            BeginProgress(message, 0, canCancel);
        }

        /// <summary>
        /// Begins progress display where progress is updated automatically</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        /// <param name="canCancel">True iff the Cancel button appears and is enabled</param>
        public void BeginProgress(string message, int expectedDuration, bool canCancel)
        {
            m_autoIncrement =
                expectedDuration == 0 ? 0 : (double)ProgressInterval / (double)expectedDuration;

            m_progress = 0.0;
            m_progressDialog.IsCanceled = false;
            m_progressDialog.CanCancel = canCancel;
            m_progressDialog.Description = message;

            // Don't call 'Show' until at least a little bit of time has passed.
            m_progressTimer.Change(ProgressInterval, ProgressInterval);
        }

        /// <summary>
        /// Shows progress meter</summary>
        /// <param name="progress">Progress, in the interval [0..1]</param>
        public void ShowProgress(double progress)
        {
            m_progress = progress;
        }

        /// <summary>
        /// Ends progress meter display</summary>
        public void EndProgress()
        {
            m_progress = 0.0;
            m_progressTimer.Change(Timeout.Infinite, ProgressInterval);
            m_progressDialog.Close();
        }

        /// <summary>
        /// Event that is raised when progress dialog is cancelled</summary>
        public event EventHandler ProgressCancelled;

        #endregion

        private void progressCallback(object state)
        {
            lock (this)
            {
                if (m_progress < 1.0)
                {
                    if (!m_progressDialog.IsDialogVisible)
                        m_progressDialog.Show();
                    int percent = (int)(100 * m_progress);
                    if (percent < 0)
                        percent = 0;
                    else if (percent > 100)
                        percent = 100;

                    m_progressDialog.Percent = percent;
                    m_progress += m_autoIncrement;
                }
                else
                {
                    EndProgress();
                }
            }
        }

        private void progressDialog_Cancelled(object sender, EventArgs e)
        {
            ProgressCancelled.Raise(this, e);
        }

        #region Private Classes

        private class TextPanel : ToolStripStatusLabel, IStatusText
        {
            public TextPanel(int width)
            {
                DisplayStyle = ToolStripItemDisplayStyle.Text;
                Width = width;
                AutoSize = false;
                TextAlign = ContentAlignment.MiddleLeft;
                BorderSides = ToolStripStatusLabelBorderSides.All;
            }
        }

        private class ImagePanel : ToolStripStatusLabel, IStatusImage
        {
            public ImagePanel()
            {
                DisplayStyle = ToolStripItemDisplayStyle.Image;
                AutoSize = false;
            }
        }

        #endregion

        private readonly Form m_mainForm;
        private ToolStripContainer m_toolStripContainer;
        private readonly StatusStrip m_statusStrip = new StatusStrip();
        private readonly ToolStripStatusLabel m_mainPanel;
        private double m_progress;
        private double m_autoIncrement;
        private readonly Timer m_progressTimer;
        private readonly ThreadSafeProgressDialog m_progressDialog;

        private const int ProgressInterval = 250; // Timer resolution, in ms
        private static int s_controlCount;
    }
}
