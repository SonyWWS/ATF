//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Interface for service that manages a status display</summary>
    /// <remarks>The status service provides a global status text panel that is at the far
    /// left side of the Application's StatusBar. It also allows clients to register custom
    /// text and image panels. The client requests these and is given an interface to update
    /// the text and images.</remarks>
    public interface IStatusService
    {
        /// <summary>
        /// Displays status of an operation (e.g., "File successfully saved.")</summary>
        /// <param name="status">Operation status to display</param>
        void ShowStatus(string status);

        /// <summary>
        /// Adds a text status item to the status bar</summary>
        /// <param name="width">Text panel width</param>
        /// <returns>Text panel</returns>
        IStatusText AddText(int width);

        /// <summary>
        /// Adds an image status item to the status bar</summary>
        /// <returns>Image status item</returns>
        IStatusImage AddImage();

        /// <summary>
        /// Begins progress display where client manually updates progress.
        /// The cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        void BeginProgress(string message);

        /// <summary>
        /// Begins progress display where progress is updated automatically.
        /// The cancel button appears and is enabled.</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        void BeginProgress(string message, int expectedDuration);

        /// <summary>
        /// Begins progress display where client manually updates progress</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        void BeginProgress(string message, bool canCancel);

        /// <summary>
        /// Begins progress display where progress is updated automatically</summary>
        /// <param name="message">Message to display with progress meter</param>
        /// <param name="expectedDuration">Expected length of operation, in milliseconds</param>
        /// <param name="canCancel">Should the cancel button appear and be enabled?</param>
        void BeginProgress(string message, int expectedDuration, bool canCancel);

        /// <summary>
        /// Shows progress</summary>
        /// <param name="progress">Progress, in the interval [0..1]</param>
        void ShowProgress(double progress);

        /// <summary>
        /// Ends progress display</summary>
        void EndProgress();

        /// <summary>
        /// Event that is raised when progress dialog is canceled</summary>
        event EventHandler ProgressCancelled;
    }
}


