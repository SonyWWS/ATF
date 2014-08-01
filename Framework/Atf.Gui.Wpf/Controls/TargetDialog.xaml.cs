//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for TargetDialog.xaml
    /// </summary>
    internal partial class TargetDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        public TargetDialog()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as TargetDialogViewModel;
            if (vm != null)
                vm.ShowFindTargetsDialog +=
                    (s, args) =>
                        {
                            var dlg = Activator.CreateInstance<FindTargetsDialog>();
                            {
                                dlg.DataContext = args.ViewModel;
                                dlg.Owner = Application.Current.MainWindow;
                                dlg.ShowDialog();
                            }
                        };
        }
    }
}
