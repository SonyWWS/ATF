//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;

using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Event argument for completion of work monitored on a progress display</summary>
    public class ProgressCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="progressError">Exception raised</param>
        /// <param name="progressResult">Result of progress or null</param>
        /// <param name="cancelled">Whether or not work cancelled</param>
        public ProgressCompleteEventArgs(Exception progressError, object progressResult, bool cancelled)
        {
            ProgressError = ProgressError;
            ProgressResult = progressResult;
            Cancelled = cancelled;
        }

        /// <summary>
        /// Gets or sets exception raised</summary>
        public Exception ProgressError { get; set; }

        /// <summary>
        /// Gets or sets result of progress</summary>
        public object ProgressResult { get; set; }

        /// <summary>
        /// Gets or sets whether or not work cancelled</summary>
        public bool Cancelled { get; set; }
    }

    /// <summary>
    /// Service that provides status UI</summary>
    [Export(typeof(IStatusService))]
    [Export(typeof(IInitializable))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StatusService : IStatusService, IInitializable
    {
        [Export(typeof(IStatusItem))]
        private StatusText m_mainStatusText = new StatusText(100);

        [Import]
        private IComposer m_composer = null;

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by showing status</summary>
        public void Initialize()
        {
            m_mainStatusText.IsLeftDock = true;

            ShowStatus(Localizer.Localize("Ready", "Application is ready"));
        }

        #endregion

        #region IStatusService members

        /// <summary>
        /// Shows a status message in the main panel (e.g., "File successfully saved.")</summary>
        /// <param name="status">Status message</param>
        public void ShowStatus(string status)
        {
            m_mainStatusText.Text = status;
        }

        /// <summary>
        /// Begins progress display where client manually updates progress</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        /// <param name="argument">Worker argument</param>
        /// <param name="workHandler">Background thread delegate</param>
        /// <param name="autoIncrement">Whether to auto increment the progress meter</param>
        /// <returns>True if the thread was cancelled</returns>
        public bool RunProgressDialog(string message, bool canCancel, object argument, DoWorkEventHandler workHandler, bool autoIncrement)
        {
            var vm = new ProgressViewModel() { 
                Cancellable = canCancel, 
                Description = message, 
                IsIndeterminate = autoIncrement };

            vm.RunWorkerThread(argument, workHandler);

            DialogUtils.ShowDialogWithViewModel<ProgressDialog>(vm);

            ProgressResult = vm.Result;
            ProgressError = vm.Error;

            return vm.Cancelled;
        }

        /// <summary>
        /// Begins asynchronous progress display in the progress bar</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="argument">Worker argument</param>
        /// <param name="workHandler">Background thread delegate</param>
        /// <param name="progressCompleteHandler">Event handler for work completion event</param>
        /// <param name="autoIncrement">Whether to auto increment the progress meter</param>
        public void RunProgressInStatusBarAsync(string message, object argument, DoWorkEventHandler workHandler, EventHandler<ProgressCompleteEventArgs> progressCompleteHandler, bool autoIncrement)
        {
            var statusItem = new ProgressViewModel()
            {
                Cancellable = false,
                Description = message,
                IsIndeterminate = autoIncrement
            };

            // Add the part to the status bar
            ComposablePart part = m_composer.AddPart(statusItem);
            statusItem.Tag = new StatusBarProgressContext(progressCompleteHandler, part);

            statusItem.RunWorkerThread(argument, workHandler);
            statusItem.RunWorkerCompleted += new EventHandler(statusItem_RunWorkerCompleted);
        }

        /// <summary>
        /// Gets exception indicating whether the progress was successful</summary>
        public Exception ProgressError { get; private set; }

        /// <summary>
        /// Result of progress or null</summary>
        public object ProgressResult { get; private set; }

        #endregion

        private void statusItem_RunWorkerCompleted(object sender, EventArgs e)
        {
            var statusItem = (ProgressViewModel)sender;
            var ctxt = (StatusBarProgressContext)statusItem.Tag;

            // remove the item from the status bar
            m_composer.RemovePart(ctxt.StatusItemPart);

            var args = new ProgressCompleteEventArgs(statusItem.Error, statusItem.Result, statusItem.Cancelled);
            
            ctxt.ProgressComplete.Raise(this, args);

            statusItem.RunWorkerCompleted -= statusItem_RunWorkerCompleted;
        }

        private struct StatusBarProgressContext
        {
            public StatusBarProgressContext(
                EventHandler<ProgressCompleteEventArgs> progressComplete,
                ComposablePart statusItemPart)
            {
                ProgressComplete = progressComplete;
                StatusItemPart = statusItemPart;
            }

            public readonly EventHandler<ProgressCompleteEventArgs> ProgressComplete;

            public readonly ComposablePart StatusItemPart;
        }
    }
   
}
