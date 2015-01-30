//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Event arguments to allow decoupling of view models from dialogs.
    /// Rather than directly launching a dialog, a view model can
    /// provide an event subscribed to by the view using these event arguments.
    /// This allows for MVVM conformity and UI-less testing.
    /// NOTE: this technique is only suitable in simple cases; for more complex
    /// cases a mediator pattern could be used (TODO)</summary>
    public class ShowDialogEventArgs : EventArgs
    {
        /// <summary>
        /// Default constructor</summary>
        public ShowDialogEventArgs()
        {
        }

        /// <summary>
        /// Constructor with view model object</summary>
        /// <param name="viewModel">View model object</param>
        public ShowDialogEventArgs(object viewModel)
        {
            ViewModel = viewModel;
        }

        /// <summary>
        /// Get or set dialog result</summary>
        public bool? DialogResult { get; set; }

        /// <summary>
        /// Get view model object</summary>
        public object ViewModel { get; private set; }
    }
}
