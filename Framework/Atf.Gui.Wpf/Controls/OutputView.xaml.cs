//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for OutputView.xaml for dialogs to display text output to user</summary>
    public partial class OutputView : UserControl
    {
        /// <summary>
        /// Constructor</summary>
        public OutputView()
        {
            InitializeComponent();
        }

        private void CtrlCCopyCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var lbi in lb.SelectedItems)
            {
                if (lbi != null)
                {
                    sb.AppendLine(lbi.ToString());
                }
            }
            
            Clipboard.SetText(sb.ToString());
        }

        private void CtrlCCopyCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = lb.SelectedItems.Count > 0;
        }
    }
}
