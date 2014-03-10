//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml to display progress to the user</summary>
    public partial class ProgressDialog : Window
    {
        /// <summary>
        /// Constructor</summary>
        public ProgressDialog()
        {
            InitializeComponent();
            Loaded += ProgressDialog_Loaded;
            DataContextChanged += ProgressDialog_DataContextChanged;
        }

        /// <summary>
        /// Raises the Closing event and performs custom processing</summary>
        /// <param name="e">CancelEventArgs that contains event data</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            var vm = DataContext as ProgressViewModel;

            // If this close is due to Close button/cancel button/ Alt+F4
            // then DialogResult will not be set
            if (!DialogResult.HasValue)
            {
                if (vm.Cancellable)
                {
                    // If cancellation is allowed then CancelAsync the worker
                    vm.CancelAsync();
                }

                // Cancel the operation - only allow dialog to close when
                // worker is done and DialogResult is set
                e.Cancel = true;
            }
            else
            {
                if (vm != null)
                {
                    vm.RunWorkerCompleted -= DataContext_RunWorkerCompleted;
                }
            }
        }

        #region Event Handlers

        private void ProgressDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as ProgressViewModel;
            if (vm != null)
            {
                vm.RunWorkerCompleted += DataContext_RunWorkerCompleted;
                TrySetCancellable();
            }
        }

        private void ProgressDialog_Loaded(object sender, RoutedEventArgs e)
        {
            TrySetCancellable();
        }

        private void DataContext_RunWorkerCompleted(object sender, EventArgs e)
        {
            var vm = (ProgressViewModel)sender;
            DialogResult = vm.Error == null && !vm.Cancelled;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        private void TrySetCancellable()
        {
            var vm = DataContext as ProgressViewModel;
            if (vm != null && IsLoaded)
            {
                // Some interop to remove the Close button from the window if it is
                // not cancelable
                if (!vm.Cancellable)
                {
                    var hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
                }
            }
        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    }
}
