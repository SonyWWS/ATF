//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for UnhandledExceptionDialog.xaml for unhandled exception dialog</summary>
    public partial class UnhandledExceptionDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        public UnhandledExceptionDialog()
        {
            InitializeComponent();
        }
    }

    /// <summary>
    /// View model for UnhandledExceptionDialog</summary>
    public class UnhandledExceptionViewModel : DialogViewModelBase
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="message">Exception message to display</param>
        public UnhandledExceptionViewModel(string message)
        {
            Title = "An error has occurred".Localize();
            Message = message;
        }

        /// <summary>
        /// Gets the exception message</summary>
        public string Message { get; private set; }
    }
}
