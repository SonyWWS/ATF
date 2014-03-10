//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml for unhandled exception dialog</summary>
    internal partial class UnhandledExceptionDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        public UnhandledExceptionDialog()
        {
            InitializeComponent();
        }
    }

    internal class UnhandledExceptionViewModel : DialogViewModelBase
    {
        public UnhandledExceptionViewModel(string message)
        {
            Title = "Unhandled Exception".Localize();
            Message = message;
        }

        public string Message { get; private set; }
    }
}
