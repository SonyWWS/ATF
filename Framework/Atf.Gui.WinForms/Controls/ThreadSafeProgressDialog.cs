//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Thread-safe progress dialog</summary>
    public class ThreadSafeProgressDialog : IDisposable
    {
        /// <summary>
        /// Default constructor</summary>
        public ThreadSafeProgressDialog()
            : this(true, false, false)
        {
        }

        /// <summary>
        /// Constructor with show and cancel flags</summary>
        /// <param name="show">Whether to show dialog</param>
        /// <param name="canCancel">Whether dialog can be cancelled</param>
        public ThreadSafeProgressDialog(bool show, bool canCancel)
            : this(show, false, canCancel)
        {
        }

        /// <summary>
        /// Constructor with show, cancel and marquee flags</summary>
        /// <param name="show">Whether to show dialog</param>
        /// /// <param name="marquee">Whether ProgressBarStyle.Marquee should be used</param>
        /// <param name="canCancel">Whether dialog can be cancelled</param>
        public ThreadSafeProgressDialog(bool show, bool marquee, bool canCancel)
        {
            m_marquee = marquee;
            m_canCancel = canCancel;
            if (show)
                Show();
        }

        /// <summary>
        /// Disposes of resources</summary>
        void IDisposable.Dispose()
        {
            Close();
            Owner = null;
        }

        /// <summary>
        /// Gets and sets this dialog's parent form</summary>
        public Form Owner
        {
            get
            {
                return m_owner;
            }
            set
            {
                if (m_owner == value)
                    return;

                if (m_owner != null)
                {
                    m_owner.Resize -= OnOwnerMove;
                    m_owner.Move -= OnOwnerMove;
                }

                m_owner = value;

                if (m_owner != null)
                {
                    m_owner.Resize += OnOwnerMove;
                    m_owner.Move += OnOwnerMove;
                }

                // setup initial state now
                OnOwnerMove(value, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets and sets the title of the progress dialog</summary>
        public string Title
        {
            get
            {
                return m_title;
            }
            set
            {
                if (m_title != value)
                {
                    m_title = value;
                    Show(); // make sure dialog is visible
                    if (m_backgroundThread != null)
                        m_backgroundThread.UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Gets and sets the description of the progress dialog</summary>
        public string Description
        {
            get
            {
                return m_description;
            }
            set
            {
                if (m_description != value)
                {
                    m_description = value;
                    Show(); // make sure dialog is visible
                    if (m_backgroundThread != null)
                        m_backgroundThread.UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Gets and sets the percentage of progress so far</summary>
        public int Percent
        {
            get
            {
                if (m_marquee)
                    throw new InvalidOperationException("not allowed in Marquee mode");

                return m_percent;
            }
            set
            {
                if (m_marquee)
                    throw new InvalidOperationException("not allowed in Marquee mode");

                if (m_percent != value)
                {
                    m_percent = value;
                    Show(); // make sure dialog is visible
                    if (m_backgroundThread != null)
                        m_backgroundThread.UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Gets and sets whether the progress is cancelled</summary>
        public bool IsCanceled
        {
            get
            {
                return m_isCanceled;
            }
            set
            {
                m_isCanceled = value;
                if (m_isCanceled)
                {
                    OnCancelled(EventArgs.Empty);
                    Close();
                }
            }
        }

        /// <summary>
        /// Gets and sets whether progress dialog can be cancelled</summary>
        public bool CanCancel
        {
            get
            {
                return m_canCancel;
            }
            set
            {
                if (m_canCancel != value)
                {
                    m_canCancel = value;
                    if (m_backgroundThread != null)
                        m_backgroundThread.UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Gets and sets whether the progress is not represented by percent completed</summary>
        public bool IsMarquee
        {
            get
            {
                return m_marquee;
            }
            set
            {
                if (m_marquee != value)
                {
                    m_marquee = value;
                    if (m_backgroundThread != null)
                        m_backgroundThread.UpdateLabel();
                }
            }
        }

        /// <summary>
        /// Gets whether progress dialog is visible</summary>
        public bool IsDialogVisible
        {
            get
            {
                return m_backgroundThread != null;
            }
        }

        /// <summary>
        /// Event that is raised when progress dialog is cancelled</summary>
        /// <remarks>This event is raised from the ThreadSafeProgressDialog thread</remarks>
        public event EventHandler Cancelled;

        /// <summary>
        /// Shows progress dialog</summary>
        public void Show()
        {
            if (IsDialogVisible)
                return;

            if (m_backgroundThread == null)
                m_backgroundThread = new BackgroundThread(this);

            m_backgroundThread.UpdateLabel();
            if (m_isCanceled)
                Close();
        }

        /// <summary>
        /// Closes progress dialog</summary>
        public void Close()
        {
            if (m_backgroundThread != null)
            {
                m_backgroundThread.Stop();
                m_backgroundThread = null;
            }
        }

        /// <summary>
        /// Raises the Cancelled event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnCancelled(EventArgs e)
        {
            Cancelled.Raise(this, e);
        }

        private void OnOwnerMove(object sender, EventArgs e)
        {
            // box rectangle
            if (m_owner != null)
                m_ownerBounds = m_owner.DesktopBounds;
            else
                m_ownerBounds = null;

            if (m_backgroundThread != null)
                m_backgroundThread.UpdateLocation();
        }

        #region Private Classes

        private class BackgroundThread 
        {
            private readonly ThreadSafeProgressDialog m_parent;
            private readonly Thread m_thread;
            private bool m_alreadyStopped;
            private ProgressDialog m_dialog;//can be null if m_alreadyStopped is true.
            private ProgressBar m_progressBarControl;

            public BackgroundThread(ThreadSafeProgressDialog parent)
            {
                m_parent = parent;
                m_thread = new Thread(Run);
                m_thread.Name = "progress dialog";
                m_thread.IsBackground = true; //so that the thread can be killed if app dies.
                m_thread.SetApartmentState(ApartmentState.STA);
                m_thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
                m_thread.Start();
            } 

            public void Stop()
            {
                lock (this)
                {
                    if (m_dialog != null && m_dialog.IsHandleCreated) 
                        m_dialog.BeginInvoke(new MethodInvoker(m_dialog.Close));
                    m_alreadyStopped = true;
                }
            } 

            public void UpdateLabel()
            {
                lock (this)
                {
                    if (m_dialog != null && m_dialog.IsHandleCreated)
                        m_dialog.BeginInvoke(new MethodInvoker(ThreadUnsafeUpdate));
                }
            }

            public void UpdateLocation()
            {
                lock (this)
                {
                    if (m_dialog != null && m_dialog.IsHandleCreated)
                        m_dialog.BeginInvoke(new MethodInvoker(ThreadUnsafeUpdateLocation));
                }
            }

            private void Run()
            {
                try
                {
                    lock (this)
                    {
                        if (!m_alreadyStopped)
                        {
                            m_dialog = new ProgressDialog();
                            m_dialog.Cancelled += m_dialog_Cancelled;

                            foreach (Control cntrl in m_dialog.Controls)
                            {
                                ProgressBar prog = cntrl as ProgressBar;
                                if (prog != null)
                                {
                                    m_progressBarControl = prog;
                                    break;
                                }
                            }

                            ThreadUnsafeUpdate();
                            m_dialog.Visible = true;
                        }
                    }
                    if (!m_alreadyStopped)
                        Application.Run(m_dialog);
                }
                finally
                {
                    lock (this)
                    {
                        if (m_dialog != null)
                        {
                            // If the dialog was visible when it went away, we need to make the parent visible.  
                            // We cannot rely on Windows to do it for us, since the parent is owned by a different 
                            // thread, which means that we cannot tell windows about the parent/child relationship.
                            var visibleAtClose = m_dialog.Visible;

                            // Now make the dialog go away
                            m_dialog.Dispose();
                            m_dialog = null;
                            m_progressBarControl = null;

                            if (visibleAtClose)
                            {
                                m_parent.Owner.BeginInvoke(new MethodInvoker(m_parent.Show));
                            }
                        }
                    }
                }
            }

            void m_dialog_Cancelled(object sender, EventArgs e)
            {
                ThreadUnsafeUpdate();
            }

            // The caller must have a lock on 'this'.
            private void ThreadUnsafeUpdate()
            {
                if (m_dialog != null)
                {
                    var title = m_parent.m_title;
                    if (title != null)
                        m_dialog.Title = title;
                    m_dialog.Label = m_parent.m_description;
                    m_dialog.Percent = m_parent.m_percent;
                    m_dialog.CanCancel = m_parent.CanCancel;
                    m_dialog.CancelVisible = m_parent.CanCancel;
                    m_progressBarControl.Style = m_parent.m_marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;

                    if (m_dialog.IsCanceled)
                        m_parent.IsCanceled = true;
                }
            }

            private void ThreadUnsafeUpdateLocation()
            {
                if (m_dialog != null)
                {
                    if (m_parent.m_ownerBounds == null)
                    {
                        m_dialog.TopMost = false;
                        return;
                    }

                    // unbox rectangle
                    var bounds = (Rectangle)m_parent.m_ownerBounds;

                    // center on owner
                    Point point = bounds.Location;
                    point.Offset((bounds.Width - m_dialog.Width) / 2, (bounds.Height - m_dialog.Height) / 2);

                    m_dialog.DesktopLocation = point;
                    m_dialog.TopMost = true;
                }
            }
        }

        #endregion

        private Form m_owner = null;
        private volatile object m_ownerBounds;
        private volatile string m_title = null;
        private volatile string m_description = string.Empty;
        private volatile int m_percent;
        private BackgroundThread m_backgroundThread;
        private volatile bool m_marquee;
        private volatile bool m_isCanceled;
        private volatile bool m_canCancel;
    }
}
