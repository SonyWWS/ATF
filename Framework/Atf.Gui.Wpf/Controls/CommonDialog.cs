//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Data;

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
            Background = SystemColors.ControlBrush;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            DataContextChanged += CommonDialog_DataContextChanged;
        }

        private void CommonDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as IDialogViewModel;
            if (vm != null)
            {
                vm.CloseDialog += ViewModel_CloseDialog;

                var binding = new Binding("Title");
                binding.Source = DataContext;
                SetBinding(Window.TitleProperty, binding);
            }
        }

        private void ViewModel_CloseDialog(object sender, CloseDialogEventArgs e)
        {
            DialogResult = e.DialogResult;
            Close();
        }
    }
}
