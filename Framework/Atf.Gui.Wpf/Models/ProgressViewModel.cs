//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// View model used for both progress dialog and for progress status item</summary>
    public class ProgressViewModel : DialogViewModelBase, IStatusItem
    {
        /// <summary>
        /// Constructor</summary>
        public ProgressViewModel()
        {
            m_worker.WorkerReportsProgress = true;
            m_worker.WorkerSupportsCancellation = true;
            m_worker.DoWork += worker_DoWork;
            m_worker.ProgressChanged += worker_ProgressChanged;
            m_worker.RunWorkerCompleted += worker_RunWorkerCompleted;

            m_isIndeterminate = true;
            m_description = "Please wait...".Localize();
            Title = "Progress".Localize();
        }

        #region Properties

        #region IsIndeterminate Property

        /// <summary>
        /// Gets or sets whether or not the progress dialog shows a specific value or not</summary>
        public bool IsIndeterminate
        {
            get { return m_isIndeterminate; }
            set
            {
                m_isIndeterminate = value;
                OnPropertyChanged(s_isIndeterminateArgs);
            }
        }

        private bool m_isIndeterminate;
        private static readonly PropertyChangedEventArgs s_isIndeterminateArgs
            = ObservableUtil.CreateArgs<ProgressViewModel>(x => x.IsIndeterminate);

        #endregion

        #region Description Property

        /// <summary>
        /// Gets or sets user-readable description of progress</summary>
        public string Description
        {
            get { return m_description; }
            set
            {
                m_description = value;
                OnPropertyChanged(s_descriptionArgs);
            }
        }

        private string m_description;
        private static readonly PropertyChangedEventArgs s_descriptionArgs
            = ObservableUtil.CreateArgs<ProgressViewModel>(x => x.Description);

        #endregion

        #region Content Property

        /// <summary>
        /// Gets or sets unique user state of progress</summary>
        public object Content
        {
            get { return m_content; }
            set
            {
                m_content = value;
                OnPropertyChanged(s_contentArgs);
            }
        }

        private object m_content;
        private static readonly PropertyChangedEventArgs s_contentArgs
            = ObservableUtil.CreateArgs<ProgressViewModel>(x => x.Content);

        #endregion

        /// <summary>
        /// Gets or sets whether background operation can be cancelled</summary>
        public bool Cancellable { get; set; }

        /// <summary>
        /// Gets or sets whether background operation is cancelled</summary>
        public bool Cancelled { get; private set; }

        /// <summary>
        /// Gets or sets result of background operation</summary>
        public object Result { get; private set; }

        /// <summary>
        /// Gets or sets exception that occurred during background operation</summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// Gets or sets internal tag</summary>
        public object Tag { get; set; }

        #region Progress Property

        /// <summary>
        /// Gets or sets progress</summary>
        public int Progress
        {
            get { return m_progress; }
            private set
            {
                m_progress = value;
                OnPropertyChanged(s_progressArgs);
            }
        }

        private int m_progress;
        private static readonly PropertyChangedEventArgs s_progressArgs
            = ObservableUtil.CreateArgs<ProgressViewModel>(x => x.Progress);

        #endregion

        #endregion

        /// <summary>
        /// Launches a worker thread that is intended to perform work in the background
        /// while progress is indicated, and displays the dialog
        /// modally in order to block the calling thread</summary>
        /// <param name="workerCallback">Background thread delegate of background operation</param>
        public void RunWorkerThread(DoWorkEventHandler workerCallback)
        {
            RunWorkerThread(null, workerCallback);
        }

        /// <summary>
        /// Launches a worker thread that is intended to perform work in the background
        /// while progress is indicated, and displays the dialog
        /// modally in order to block the calling thread</summary>
        /// <param name="argument">Worker argument</param>
        /// <param name="workerCallback">Background thread delegate of background operation</param>
        public void RunWorkerThread(object argument, DoWorkEventHandler workerCallback)
        {
            //store the UI culture
            m_uiCulture = CultureInfo.CurrentUICulture;

            //store reference to callback handler and launch worker thread
            m_workerCallback = workerCallback;
            m_worker.RunWorkerAsync(argument);

            //display modal dialog (blocks caller)
            //return ShowDialog() ?? false;
        }

        /// <summary>
        /// Requests cancellation of background thread of operation</summary>
        public void CancelAsync()
        {
            if (Cancellable)
            {
                Cancelled = true;
                m_worker.CancelAsync();
            }
        }

        /// <summary>
        /// Event that is raised when the background operation completes</summary>
        public event EventHandler RunWorkerCompleted;

        #region Event Handlers

        /// <summary>
        /// Worker method that gets called from a worker thread.
        /// Synchronously calls event listeners that may handle the work load.</summary>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                //make sure the UI culture is properly set on the worker thread
                Thread.CurrentThread.CurrentUICulture = m_uiCulture;

                //invoke the callback method with the designated argument
                m_workerCallback(sender, e);
            }
            catch (Exception ex)
            {
                //disable cancelling and rethrow the exception
                System.Diagnostics.Debug.WriteLine(ex);
                Cancelled = true;
                throw;
            }
        }

        /// <summary>
        /// Visually indicates the progress of the background operation by
        /// updating the dialog's progress bar</summary>
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage != int.MinValue)
                Progress = e.ProgressPercentage;

            Content = e.UserState;
        }

        /// <summary>
        /// Updates the user interface once an operation has been completed and
        /// sets the dialog's DialogResult depending on the value
        /// of the <see cref="AsyncCompletedEventArgs.Cancelled"/> property.</summary>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                Error = e.Error;
            }
            else if (!e.Cancelled)
            {
                //assign result if there was neither exception nor cancel
                Result = e.Result;
            }

            //update UI in case closing the dialog takes a moment
            Progress = 100;

            //set the dialog result, which closes the dialog
            //DialogResult = Error == null && !e.Cancelled;
            Cancelled = e.Cancelled;

            RunWorkerCompleted.Raise(this, EventArgs.Empty);
        }

        #endregion

        private BackgroundWorker m_worker = new BackgroundWorker();
        private CultureInfo m_uiCulture;
        private DoWorkEventHandler m_workerCallback;
    }

}
