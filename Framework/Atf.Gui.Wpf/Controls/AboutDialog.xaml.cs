//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Diagnostics;
using System.Text;
using System.Windows;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml for an about dialogs</summary>
    public partial class AboutDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        public AboutDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles click navigation on the hyperlink in the About dialog</summary>
        /// <param name="sender">Object that sent the event</param>
        /// <param name="e">Navigation events arguments</param>
        private void hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (e.Uri != null && string.IsNullOrEmpty(e.Uri.OriginalString) == false)
            {
                string uri = e.Uri.AbsoluteUri;
                Process.Start(new ProcessStartInfo(uri));
                e.Handled = true;
            }
        }

        // Note: Should really use delegate command and add this code to the view model.
        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as AboutDialogViewModel;
            if (vm != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine(vm.ProductTitle);
                sb.AppendLine(vm.Version);
                foreach (var assembly in vm.Assemblies)
                    sb.AppendLine(assembly);
                
                Clipboard.SetDataObject(sb.ToString());
            }
        }
    }
}
