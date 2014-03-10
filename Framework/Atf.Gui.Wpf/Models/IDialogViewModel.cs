using System;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Base interface for dialog view models</summary>
    public interface IDialogViewModel
    {
        /// <summary>
        /// Gets dialog title</summary>
        string Title { get; }

        /// <summary>
        /// Gets the ICommand associated with the user clicking the OK button on the dialog</summary>
        ICommand OkCommand { get; }

        /// <summary>
        /// Gets the ICommand associated with the user clicking the Cancel button on the dialog</summary>
        ICommand CancelCommand { get; }

        /// <summary>
        /// Event that is raised when the dialog is closed</summary>
        event EventHandler<CloseDialogEventArgs> CloseDialog;
    }
}
