//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Wpf.Models
{
    internal class ErrorDialogViewModel : DialogViewModelBase
    {
        public ErrorDialogViewModel()
        {
            Title = "Error".Localize();
        }

        public string Message { get; set; }
        public bool SuppressMessage { get; set; }
    }
}
