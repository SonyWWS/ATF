//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Interface for service that manages a status display</summary>
    /// <remarks>The status service provides a global status text panel that is at the far
    /// left side of the application's StatusBar.</remarks>
    public interface IStatusService
    {
        /// <summary>
        /// Displays status of an operation (e.g., "File successfully saved.")</summary>
        /// <param name="status">Status message</param>
        void ShowStatus(string status);

        /// <summary>
        /// Begins progress display where client manually updates progress</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        /// <param name="argument">Worker argument</param>
        /// <param name="workHandler">Background thread delegate</param>
        /// <param name="autoIncrement">Whether to auto increment the progress meter</param>
        /// <returns><c>True</c> if the thread was cancelled</returns>
        bool RunProgressDialog(string message, bool canCancel, object argument, DoWorkEventHandler workHandler, bool autoIncrement);

        /// <summary>
        /// Begins asynchronous progress display in the progress bar</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="argument">Worker argument</param>
        /// <param name="workHandler">Background thread delegate</param>
        /// <param name="progressCompleteHandler">Event handler for work completion event</param>
        /// <param name="autoIncrement">Whether to auto increment the progress meter</param>
        void RunProgressInStatusBarAsync(string message, object argument, DoWorkEventHandler workHandler, EventHandler<ProgressCompleteEventArgs> progressCompleteHandler, bool autoIncrement);

        /// <summary>
        /// Gets exception indicating whether the progress was successful</summary>
        Exception ProgressError { get; }

        /// <summary>
        /// Result of progress or null</summary>
        object ProgressResult { get; }
    }
}
