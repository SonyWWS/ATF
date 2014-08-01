//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Event args to allow decoupling of view models from dialogs
    /// Rather than directly launching a dialog, a view model can
    /// provide an event subscribed to by the view using these event args.
    /// This allows for MVVM conformity and UI-less testing
    /// NOTE: this technique is only suitable in simple cases, for more complex
    /// cases a mediator pattern could be used (TODO)
    /// </summary>
    public class ShowDialogEventArgs : EventArgs
    {
        public ShowDialogEventArgs()
        {
        }

        public ShowDialogEventArgs(object viewModel)
        {
            ViewModel = viewModel;
        }

        public bool? DialogResult { get; set; }

        public object ViewModel { get; private set; }
    }
}
