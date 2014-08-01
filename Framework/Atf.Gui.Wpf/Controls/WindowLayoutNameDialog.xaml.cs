//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for WindowLayoutNameDialog.xaml
    /// </summary>
    public partial class WindowLayoutNewDialog :CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="vm">View model to use</param>
        public WindowLayoutNewDialog(DialogViewModelBase vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }

}
