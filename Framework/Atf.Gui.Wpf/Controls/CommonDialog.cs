//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Base class for dialogs</summary>
    public class CommonDialog : Window
    {
        /// <summary>
        /// Constructor</summary>
        public CommonDialog()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContextChanged += CommonDialog_DataContextChanged;
        }

        /// <summary>
        /// Called after initialization is complete. Sets the dialog's style.</summary>
        /// <param name="e">not used</param>
        protected override void OnInitialized(EventArgs e)
        {
            if (IsOverridingWindowsChrome)
            {
                SetResourceReference(StyleProperty, typeof(CommonDialog));
            }
            
            base.OnInitialized(e);
        }

        /// <summary>
        /// Always returns true</summary>
        protected virtual bool IsOverridingWindowsChrome
        {
            get { return true; }
        }

        /// <summary>
        /// Event handler that is called when the dialog is about to close. </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            var vm = m_viewModel;

            if (!m_closing && vm != null && vm.CancelCommand != null)
            {
                // Catch case where dialog is closed using the close button via windows
                // Feed this through the view model
                if (!vm.CancelCommand.CanExecute(null))
                {
                    e.Cancel = true;
                }
                else
                {
                    try
                    {
                        m_closing = true;
                        vm.CancelCommand.Execute(null);

                        // Only set dialog result if this is a modal dialog
                        if (ComponentDispatcher.IsThreadModal)
                            DialogResult = e.Cancel;
                    }
                    finally
                    {
                        m_closing = false;
                    }
                }
            }
            
            // Prevent main application disappearing behind other windows:
            // http://stackoverflow.com/questions/13209526/main-window-disappears-behind-other-applications-windows-after-a-sub-window-use
            if (!e.Cancel && Owner != null) Owner.Focus();
        }

        private void CommonDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (m_viewModel != null)
            {
                m_viewModel.CloseDialog -= ViewModel_CloseDialog;
            }

            m_viewModel = DataContext as IDialogViewModel;
            if (m_viewModel != null)
            {
                m_viewModel.CloseDialog += ViewModel_CloseDialog;

                var binding = new Binding("Title");
                binding.Source = DataContext;
                SetBinding(Window.TitleProperty, binding);
            }
        }

        private void ViewModel_CloseDialog(object sender, CloseDialogEventArgs e)
        {
            if (!m_closing)
            {
                try
                {
                    m_closing = true;

                    // DAN: Workaround as for some reason even after dialog is closed
                    // The command manager keeps querying the commands in the data model!
                    DataContext = null;

                    // Only set dialog result if this is a modal dialog
                    if (ComponentDispatcher.IsThreadModal)
                    {
                        try
                        {
                            DialogResult = e.DialogResult;

                        }
                        catch (InvalidOperationException)
                        {
                            // Occassioal strange behavior when trying to set DialogResult
                            // when the window does not think it is modal?
                        }
                    }

                    // Is this required?
                    Close();
                }
                finally
                {
                    m_closing = false;
                }
            }
        }

        private IDialogViewModel m_viewModel;
        private bool m_closing;
    }
}
