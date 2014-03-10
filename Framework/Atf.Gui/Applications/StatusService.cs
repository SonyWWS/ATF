//Sony Computer Entertainment Confidential

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
        /// <param name="mainForm">Application Main Form</param>
        [ImportingConstructor]
        public StatusService(Form mainForm)
		{
            m_mainForm = mainForm;

            m_statusStrip = new StatusStrip();
            m_statusStrip.Name = "StatusBar";
            m_statusStrip.Dock = DockStyle.Bottom;

            // main status text
            m_mainPanel = new ToolStripStatusLabel();
            m_mainPanel.Width = 256;
            m_mainPanel.AutoSize = true;
            m_mainPanel.Spring = true;
            m_mainPanel.TextAlign = ContentAlignment.MiddleLeft;
            m_statusStrip.Items.Add(m_mainPanel);

            m_progressTimer = new Timer(
                new TimerCallback(progressCallback), this, Timeout.Infinite, ProgressInterval);

            m_progressDialog = new ThreadSafeProgressDialog(false, true);
            m_progressDialog.Cancelled += new EventHandler(progressDialog_Cancelled);
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

            ShowStatus(Localizer.Localize("Ready", "Application is ready"));
        }

        #endregion

        #region IStatusService members

        /// <summary>
        /// Shows a status message in the main panel</summary>
        /// <param name="status">Status message</param>
        public void ShowStatus(string status)
        {
            m_mainPanel.Text = status;
        }

        /// <summary>
        /// Adds a new text status panel
        /// </summary>
        /// <param name="width">Width of panel</param>
        /// <returns>Text panel</returns>
        public IStatusText AddText(int width)
        {
            TextPanel textPanel = new TextPanel(width);
            textPanel.Name = "$Status" + (s_controlCount++).ToString();
            m_statusStrip.Items.Add(textPanel);
            return textPanel;
        }

        /// <summary>
        /// Adds a new image status panel
        /// </summary>
        /// <returns>Image status panel</returns>
        public IStatusImage AddImage()
        {
            ImagePanel imagePanel = new ImagePanel();
            imagePanel.Name = "$Status" + (s_controlCount++).ToString();
            m_statusStrip.Items.Add(imagePanel);
            return imagePanel;
        }

        /// <summary>
        /// Begins progress display where client manually updates progress.
        /// The cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        public void BeginProgress(string message)
        {
            BeginProgress(message, true);
        }

        /// <summary>
        /// Begins progress display where progress is updated automatically.
        /// The cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        public void BeginProgress(string message, int expectedDuration)
        {
            BeginProgress(message, expectedDuration, true);
        }

        /// <summary>
        /// Begins progress display where client manually updates progress</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        public void BeginProgress(string message, bool canCancel)
        {
            m_progress = 0.0;
            m_autoIncrement = 0.0;
            m_progressDialog.IsCanceled = false;
            m_progressDialog.Show();
            m_progressDialog.CanCancel = canCancel;
            m_progressDialog.Description = message;
            m_progressTimer.Change(0, ProgressInterval);
        }

        /// <summary>
        /// Begins progress display where progress is updated automatically.</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        public void BeginProgress(string message, int expectedDuration, bool canCancel)
        {
            m_progress = 0.0;
            m_autoIncrement = (double)ProgressInterval / (double)expectedDuration;
            m_progressDialog.IsCanceled = false;
            m_progressDialog.Show();
            m_progressDialog.CanCancel = canCancel;
            m_progressDialog.Description = message;
            m_progressTimer.Change(0, ProgressInterval);
        }

        /// <summary>
        /// Shows progress</summary>
        /// <param name="progress">Progress, in the interval [0..1]</param>
        public void ShowProgress(double progress)
        {
            m_progress = progress;
        }

        /// <summary>
        /// Ends progress display</summary>
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
            Event.Raise(ProgressCancelled, this, e);
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

        private Form m_mainForm;
        private ToolStripContainer m_toolStripContainer;
        private StatusStrip m_statusStrip = new StatusStrip();
        private ToolStripStatusLabel m_mainPanel;
        private double m_progress;
        private double m_autoIncrement;
        private Timer m_progressTimer;
        private ThreadSafeProgressDialog m_progressDialog;

        private const int ProgressInterval = 250; // Timer resolution, in ms
        private static int s_controlCount;
    }
}
