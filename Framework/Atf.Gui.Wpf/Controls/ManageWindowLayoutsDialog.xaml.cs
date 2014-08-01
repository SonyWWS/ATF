//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for ManageWindowLayoutsDialog.xaml
    /// </summary>
    public partial class WindowLayoutManageDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="vm">The view model for data binding</param>
        public WindowLayoutManageDialog(DialogViewModelBase vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
