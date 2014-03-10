//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;

using Sce.Atf.Wpf.Controls;

namespace Sce.Atf.Wpf
{
    /// <summary>
    /// Delegate to show a message box with owner, message, caption, button(s) and icon</summary>
    /// <param name="owner">Owning window</param>
    /// <param name="messageBoxText">Message box text</param>
    /// <param name="caption">Message box caption</param>
    /// <param name="button">Specifies which buttons to place in message box</param>
    /// <param name="icon">Icon that is displayed by message box</param>
    /// <returns>MessageBoxResult indicating which button user clicked in message box</returns>
    /// <remarks>A default delegate is provided for applications that do not need to customize this.</remarks>
    public delegate MessageBoxResult ShowMessageBoxDelegate(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

    /// <summary>
    /// WPF version of MessageBox, which displays WPF message boxes rather than Windows Forms ones.
    /// Also provides option to set a non-default message box provider to enable unit
    /// testing of view models.
    /// A WPF view model should be unit testable with no UI/user input involved.
    /// Therefore, the view model can not directly launch dialogs. When unit testing view 
    /// models that use WpfMessageBox, the default message box provider can be set to a dummy test stub.</summary>
    public static class WpfMessageBox
    {
        /// <summary>
        /// Displays a message box that has a message and returns a result</summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(string messageBoxText)
        {
            return WpfMessageBox.Show(null, messageBoxText, null, MessageBoxButton.OK, MessageBoxImage.None);
        }

        /// <summary>
        /// Displays a message and title bar caption and returns a result</summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return WpfMessageBox.Show(null, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }
        
        /// <summary>
        /// Displays a message box in front of the specified owner window. The message box
        /// displays a message and returns a result.</summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            return WpfMessageBox.Show(owner, messageBoxText, null, MessageBoxButton.OK, MessageBoxImage.None);
        }
       
        /// <summary>
        /// Displays a message, title bar caption and button(s) and returns a result</summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return WpfMessageBox.Show(null, messageBoxText, caption, button, MessageBoxImage.None);
        }
       
        /// <summary>
        /// Displays a message box in front of the specified owner window. The message box
        /// displays a message and title bar caption and returns a result.</summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            return WpfMessageBox.Show(owner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }
       
        /// <summary>
        /// Displays a message box with a message, title bar caption, button(s) and icon and returns a result.</summary>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return WpfMessageBox.Show(null, messageBoxText, caption, button, icon);
        }

        /// <summary>
        /// Displays a message box in front of the specified window. The message box
        /// displays a message, title bar caption and button(s) and returns a result.</summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            return WpfMessageBox.Show(owner, messageBoxText, caption, button, MessageBoxImage.None);
        }

        /// <summary>
        /// Displays a message box in front of the specified window. The message box
        /// displays a message, title bar caption, button(s) and icon and returns a result.</summary>
        /// <param name="owner">A System.Windows.Window that represents the owner window of the message box</param>
        /// <param name="messageBoxText">A System.String that specifies the text to display</param>
        /// <param name="caption">A System.String that specifies the title bar caption to display</param>
        /// <param name="button">A System.Windows.MessageBoxButton value that specifies which button or buttons to display</param>
        /// <param name="icon">A System.Windows.MessageBoxImage value that specifies the icon to display</param>
        /// <returns>A System.Windows.MessageBoxResult value that specifies which message box button is clicked by the user</returns>
        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            if (owner == null && Application.Current != null)
                owner = Application.Current.MainWindow;

            return s_showDelegate(owner, messageBoxText, caption, button, icon);
        }

        /// <summary>
        /// Sets message box provider delegate</summary>
        /// <param name="del">Delegate</param>
        public static void SetProvider(ShowMessageBoxDelegate del)
        {
            Requires.NotNull(del, "del");
            s_showDelegate = del;
        }

        private static MessageBoxResult ShowDefault(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            var dlg = new MessageBoxDialog(caption, messageBoxText, button, icon);
            dlg.Owner = owner;
            dlg.ShowDialog();
            return dlg.MessageBoxResult;
        }

        private static ShowMessageBoxDelegate s_showDelegate = ShowDefault;
    }
}
