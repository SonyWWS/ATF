//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for FindTargetsDialog.xaml </summary>
    internal partial class FindTargetsDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        public FindTargetsDialog()
        {
            InitializeComponent();
            Closing += (s, e) => CancelScan();
            Loaded += (s, e) => StartScan();
        }

        private void CancelScan()
        {
            var vm = DataContext as FindTargetsViewModel;
            if (vm != null && vm.IsScanning)
                vm.ToggleScanCommand.Execute(null);
        }

        private void StartScan()
        {
            var vm = DataContext as FindTargetsViewModel;
            if (vm != null && !vm.IsScanning)
                vm.ToggleScanCommand.Execute(null);
        }
    }
}
