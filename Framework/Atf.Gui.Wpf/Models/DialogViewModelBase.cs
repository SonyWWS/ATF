//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
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
            OkCommand = new DelegateCommand(Ok, CanOk, false);
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
            CloseDialog.Raise<CloseDialogEventArgs>(this, args);
        }

        /// <summary>
        /// Indicates whether dialog can be closed</summary>
        /// <returns>True iff dialog can be closed</returns>
        protected virtual bool CanOk()
        {
            return true;
        }

        private void Ok()
        {
            OnCloseDialog(new CloseDialogEventArgs(true));
        }

        /// <summary>
        /// Indicates whether dialog can be cancelled</summary>
        /// <returns>True iff dialog can be cancelled</returns>
        protected virtual bool CanCancel()
        {
            return true;
        }

        private void Cancel()
        {
            OnCloseDialog(new CloseDialogEventArgs(false));
        }

        private string m_title;
        private static readonly PropertyChangedEventArgs s_titleArgs
            = ObservableUtil.CreateArgs<DialogViewModelBase>(x => x.Title);

    }
}
