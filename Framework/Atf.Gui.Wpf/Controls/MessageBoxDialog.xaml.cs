//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml for message boxes.
    /// An alternative to the native Win32 message box if WPF styling is required.</summary>
    public partial class MessageBoxDialog : Window
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
            Loaded += new RoutedEventHandler(MessageBoxDialog_Loaded);

            MessageBoxResult = MessageBoxResult.None;

            OkButton.Visibility = (buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.OK) ? Visibility.Visible : Visibility.Collapsed;
            YesButton.Visibility = NoButton.Visibility = (buttons == MessageBoxButton.YesNo || buttons == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;
            CancelButton.Visibility = (buttons == MessageBoxButton.OKCancel || buttons == MessageBoxButton.YesNoCancel) ? Visibility.Visible : Visibility.Collapsed;

            if (OkButton.Visibility == System.Windows.Visibility.Visible)
            {
                OkButton.IsDefault = true;
            }
            else
            {
                YesButton.IsDefault = true;
            }

            if(message != null)
                MessageText.Text = message;

            if(title != null)
                Title = title;

            Icon icon = null;
            switch (image)
            {
                case MessageBoxImage.Error:
                    icon = SystemIcons.Error;
                    break;
                case MessageBoxImage.Question:
                    icon = SystemIcons.Question;
                    break;
                case MessageBoxImage.Warning:
                    icon = SystemIcons.Warning;
                    break;
                case MessageBoxImage.Asterisk:
                    icon = SystemIcons.Asterisk;
                    break;
                case MessageBoxImage.None:
                default:
                    break;
            }

            if (icon != null)
            {
                BitmapSource bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                Image.Source = bs;
            }
            else
            {
                Image.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Performs custom actions on MessageBoxDialog Loaded event</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">RoutedEventArgs containing event data</param>
        void MessageBoxDialog_Loaded(object sender, RoutedEventArgs e)
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

        /// <summary>
        /// Gets the message box button that a user clicks</summary>
        public MessageBoxResult MessageBoxResult { get; private set; }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult = MessageBoxResult.OK;
            DialogResult = true;
            Close();
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
