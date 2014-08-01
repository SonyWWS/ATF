//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Input;

using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Interaction logic for ConfirmationDialog. Adds a "No" button to the CommonDialog's 
    /// standard Yes/Ok and Cancel buttons.</summary>
    public class ConfirmationDialogViewModel : DialogViewModelBase<ConfirmationDialog>
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="title">Text shown in the dialog's title bar</param>
        /// <param name="message">Message shown in the dialog</param>
        public ConfirmationDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
            YesButtonText = "Yes".Localize();
            NoButtonText = "No".Localize();
            CancelButtonText = "Cancel".Localize();
            NoCommand = new DelegateCommand(ExecuteNo, CanExecuteNo, false);
        }

        /// <summary>
        /// Gets the ICommand associated with the user clicking the No button on the dialog</summary>
        public ICommand NoCommand { get; private set; }

        /// <summary>
        /// Gets and sets whether to hide the Cancel button</summary>
        public bool HideCancelButton { get; set; }

        /// <summary>
        /// Gets and sets the message to display</summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets and sets the text to display on the Yes/Ok button</summary>
        public string YesButtonText { get; set; }

        /// <summary>
        /// Gets and sets the text to display on the No button</summary>
        public string NoButtonText { get; set; }

        /// <summary>
        /// Gets and sets the text to display on the Cancel button</summary>
        public string CancelButtonText { get; set; }

        /// <summary>
        /// Gets the result of the user's selection: true if they clicked Yes, otherwise false</summary>
        public MessageBoxResult Result { get; private set; }

        /// <summary>
        /// Sets the (boolean) Result before the dialog closes.</summary>
        /// <param name="args">Event args</param>
        protected override void OnCloseDialog(CloseDialogEventArgs args)
        {
            base.OnCloseDialog(args);
            if (Result != MessageBoxResult.No)
                Result = args.DialogResult == true ? MessageBoxResult.Yes : MessageBoxResult.Cancel;
        }

        private bool CanExecuteNo()
        {
            return true;
        }

        private void ExecuteNo()
        {
            Result = MessageBoxResult.No;
            OnCloseDialog(new CloseDialogEventArgs(true));
        }
    }
}
