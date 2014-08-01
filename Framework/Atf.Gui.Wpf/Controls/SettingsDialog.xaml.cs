//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml</summary>
    public partial class SettingsDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="viewModel">View model to use</param>
        public SettingsDialog(DialogViewModelBase viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
