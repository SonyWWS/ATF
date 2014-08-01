//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for SettingsLoadSaveDialog.xaml
    /// </summary>
    public partial class SettingsLoadSaveDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="viewModel">View model to use</param>
        public SettingsLoadSaveDialog(DialogViewModelBase viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }

    internal enum SettingsAction
    {
        [DisplayString("Save")]
        [Description("Settings will be saved to a file so they can be loaded at a later time or on a different machine.")]
        Save,

        [DisplayString("Load")]
        [Description("Load settings from a file and apply them to the application.")]
        Load,
    }
}
