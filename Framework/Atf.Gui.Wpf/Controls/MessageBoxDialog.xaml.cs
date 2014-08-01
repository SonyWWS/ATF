//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml for message boxes.
    /// An alternative to the native Win32 message box if WPF styling is required.</summary>
    public partial class MessageBoxDialog : CommonDialog
    {
        /// <summary>
        /// Constructor</summary>
        public MessageBoxDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Constructor with parameters</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Message text</param>
        /// <param name="buttons">Specifies the buttons that are displayed on the message box</param>
        /// <param name="image">Icon that is displayed by the message box</param>
        public MessageBoxDialog(string title, string message, MessageBoxButton buttons, MessageBoxImage image)
            : this()
        {
            Loaded += MessageBoxDialog_Loaded;

            MessageBoxResult = MessageBoxResult.None;

            OkButton.Visibility = (buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.OK)
                                      ? Visibility.Visible
                                      : Visibility.Collapsed;
            
            YesButton.Visibility = NoButton.Visibility =
                (buttons == MessageBoxButton.YesNo || buttons == MessageBoxButton.YesNoCancel)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            
            CancelButton.Visibility = (buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.YesNoCancel)
                                          ? Visibility.Visible
                                          : Visibility.Collapsed;

            if (OkButton.Visibility == Visibility.Visible)
            {
                OkButton.IsDefault = true;
            }
            else
            {
                YesButton.IsDefault = true;
            }

            if (message != null)
                MessageText.Text = message;

            if (title != null)
                Title = title;

            object iconSourceKey = null;
            switch (image)
            {
                case MessageBoxImage.Error:
                    iconSourceKey = Wpf.Resources.DialogErrorImageKey;
                    break;
                case MessageBoxImage.Question:
                    iconSourceKey = Wpf.Resources.DialogQuestionImageKey;
                    break;
                case MessageBoxImage.Warning:
                    iconSourceKey = Wpf.Resources.DialogWarningImageKey;
                    break;
                case MessageBoxImage.Information:
                    iconSourceKey = Wpf.Resources.DialogInformationImageKey;
                    break;
                case MessageBoxImage.None:
                default:
                    break;
            }

            if (iconSourceKey != null)
            {
                Controls.Icon.SetSourceKey(Image, iconSourceKey);
            }
            else
            {
                Image.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets and sets the text to display on the Yes button</summary>
        public string YesButtonText
        {
            get { return (YesButton.Content as TextBlock).Text; }
            set { (YesButton.Content as TextBlock).Text = value; }
        }

        /// <summary>
        /// Gets and sets the text to display on the No button</summary>
        public string NoButtonText
        {
            get { return (NoButton.Content as TextBlock).Text; }
            set { (NoButton.Content as TextBlock).Text = value; }
        }

        /// <summary>
        /// Gets and sets the text to display on the OK button</summary>
        public string OkButtonText
        {
            get { return (OkButton.Content as TextBlock).Text; }
            set { (OkButton.Content as TextBlock).Text = value; }
        }

        /// <summary>
        /// Gets and sets the text to display on the Cancel button</summary>
        public string CancelButtonText
        {
            get { return (CancelButton.Content as TextBlock).Text; }
            set { (CancelButton.Content as TextBlock).Text = value; }
        }

        /// <summary>
        /// Gets the message box button that a user clicks</summary>
        public MessageBoxResult MessageBoxResult { get; private set; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.OK;
            DialogResult = true;
            Close();
        }

        private void MessageBoxDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (OkButton.Visibility == System.Windows.Visibility.Visible)
            {
                OkButton.Focus();
            }
            else
            {
                YesButton.Focus();
            }
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.Yes;
            DialogResult = true;
            Close();
        }

        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.No;
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.Cancel;
            DialogResult = false;
            Close();
        }
    }
}
