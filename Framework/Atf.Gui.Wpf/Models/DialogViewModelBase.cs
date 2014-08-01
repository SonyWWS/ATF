//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Models
{
    /// <summary>
    /// Event argument for when a dialog closes</summary>
    public class CloseDialogEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="result">True if dialog closed, false if dialog not closed, null if not known</param>
        public CloseDialogEventArgs(bool? result)
        {
            DialogResult = result;
        }

        /// <summary>
        /// Gets or sets result of closing dialog. 
        /// True if dialog closed, false if dialog not closed, null if not known.</summary>
        public bool? DialogResult { get; set; }
    }

    /// <summary>
    /// Base class for dialog view models. Implements commands and logic for OK/Cancel 
    /// and validation.</summary>
    public class DialogViewModelBase : NotifyPropertyChangedBase, IDialogViewModel
    {
        /// <summary>
        /// Constructor</summary>
        public DialogViewModelBase()
        {
            OkCommand = new DelegateCommand(ExecuteOk, CanExecuteOk, false);
            CancelCommand = new DelegateCommand(Cancel, CanCancel, false);
        }

        #region IDialogViewModel Members

        /// <summary>
        /// Gets dialog title</summary>
        public string Title
        {
            get { return m_title; }
            set
            {
                m_title = value;
                OnPropertyChanged(s_titleArgs);
            }
        }

        /// <summary>
        /// Gets the ICommand associated with the user clicking the OK button on the dialog</summary>
        public ICommand OkCommand { get; private set; }

        /// <summary>
        /// Gets the ICommand associated with the user clicking the Cancel button on the dialog</summary>
        public ICommand CancelCommand { get; private set; }

        /// <summary>
        /// Event that is raised when the dialog is closed</summary>
        public event EventHandler<CloseDialogEventArgs> CloseDialog;

        #endregion

        /// <summary>
        /// Raises the CloseDialog event and performs custom processing</summary>
        /// <param name="args">CloseDialogEventArgs containing event data</param>
        protected virtual void OnCloseDialog(CloseDialogEventArgs args)
        {
            RaiseCloseDialog(args);
        }

        /// <summary>
        /// Raises the CloseDialog event and performs custom processing</summary>
        /// <param name="args">CloseDialogEventArgs containing event data</param>
        protected void RaiseCloseDialog(CloseDialogEventArgs args)
        {
            CloseDialog.Raise(this, args);
        }

        /// <summary>
        /// Indicates whether dialog can be closed</summary>
        /// <returns>True iff dialog can be closed</returns>
        protected virtual bool CanExecuteOk()
        {
            return true;
        }

        /// <summary>
        /// Indicates whether dialog can be cancelled</summary>
        /// <returns>True iff dialog can be cancelled</returns>
        protected virtual bool CanCancel()
        {
            return true;
        }

        private void ExecuteOk()
        {
            OnCloseDialog(new CloseDialogEventArgs(true));
        }

        private void Cancel()
        {
            OnCloseDialog(new CloseDialogEventArgs(false));
        }

        private string m_title;
        private static readonly PropertyChangedEventArgs s_titleArgs
            = ObservableUtil.CreateArgs<DialogViewModelBase>(x => x.Title);

    }

    /// <summary>
    /// Generic DialogViewMode base class which has ability to manage its own
    /// window via DialogUtils </summary>
    /// <typeparam name="TDialog">Type fo Dialog</typeparam>
    public class DialogViewModelBase<TDialog> : DialogViewModelBase
        where TDialog : Window
    {

        /// <summary>
        /// Gets result of closing dialog. 
        /// True if dialog closed, false if dialog not closed, null if not known.</summary>
        public bool? DialogResult { get; protected set; }

        /// <summary>
        /// Display the dialog modally.</summary>
        /// <returns>The dialog result</returns>
        public bool? ShowDialog()
        {
            OnShowDialog();
            DialogResult = DialogUtils.ShowDialogWithViewModel<TDialog>(this);
            return DialogResult;
        }

        /// <summary>
        /// Display the dialog modelessly.</summary>
        public void Show()
        {
            OnShow();
            DialogUtils.ShowWithViewModel<TDialog>(this);
        }

        /// <summary>
        /// Attempt to close the dialog.</summary>
        public void Close()
        {
            var args = new CloseDialogEventArgs(false);
            OnCloseDialog(args);
            DialogResult = args.DialogResult;
        }

        /// <summary>
        /// Custom handling when the dialog is shown modally.</summary>
        protected virtual void OnShowDialog()
        {
        }

        /// <summary>
        /// Custom handling when the dialog is shown modelessly.</summary>
        protected virtual void OnShow()
        {
        }
    }
}
