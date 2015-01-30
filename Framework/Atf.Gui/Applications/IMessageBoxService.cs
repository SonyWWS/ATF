//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// MessageBox result enumeration</summary>
    public enum MessageBoxResult
    {
        // Summary:
        //     The message box returns no result.
        None = 0,
        //
        // Summary:
        //     The result value of the message box is OK.
        OK = 1,
        //
        // Summary:
        //     The result value of the message box is Cancel.
        Cancel = 2,
        //
        // Summary:
        //     The result value of the message box is Yes.
        Yes = 6,
        //
        // Summary:
        //     The result value of the message box is No.
        No = 7,
    }

    /// <summary>
    /// Specify buttons displayed on a message box. Used as an argument
    /// of the Overload:System.Windows.MessageBox.Show method.</summary>
    public enum MessageBoxButton
    {
        // Summary:
        //     The message box displays an OK button.
        OK = 0,
        //
        // Summary:
        //     The message box displays OK and Cancel buttons.
        OKCancel = 1,
        //
        // Summary:
        //     The message box displays Yes, No, and Cancel buttons.
        YesNoCancel = 3,
        //
        // Summary:
        //     The message box displays Yes and No buttons.
        YesNo = 4,
    }

    /// <summary>
    /// Specify the icon that is displayed by a message box</summary>
    public enum MessageBoxImage
    {
        // Summary:
        //     No icon is displayed.
        None = 0,
        //
        // Summary:
        //     The message box displays an error icon.
        Error = 16,
        //
        // Summary:
        //     The message box displays a hand icon.
        Hand = 16,
        //
        // Summary:
        //     The message box displays a stop icon.
        Stop = 16,
        //
        // Summary:
        //     The message box displays a question mark icon.
        Question = 32,
        //
        // Summary:
        //     The message box displays an exclamation mark icon.
        Exclamation = 48,
        //
        // Summary:
        //     The message box displays a warning icon.
        Warning = 48,
        //
        // Summary:
        //     The message box displays an information icon.
        Information = 64,
        //
        // Summary:
        //     The message box displays an asterisk icon.
        Asterisk = 64,
    }

    /// <summary>
    /// MessageBox display interface</summary>
    public interface IMessageBoxService
    {
        /// <summary>
        /// Show the MessageBox to the client</summary>
        /// <param name = "message">Message to be shown</param>
        /// <param name = "title">Title of the MessageBox</param>
        /// <param name = "buttons">Buttons to display on box</param>
        /// <param name = "image">Image to be shown on box</param>
        /// <returns>MessageBox result</returns>
        MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image);
    }

    /// <summary>
    /// Class for MessageBoxes</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(MessageBoxes))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MessageBoxes : IInitializable, IPartImportsSatisfiedNotification
    {
        #region Imports

        [Import(AllowRecomposition = true)]
        private Lazy<IMessageBoxService> m_messageBoxService = null;

        #endregion

        #region IInitializable Members

        void IInitializable.Initialize()
        {
            // implement IInitializable to bring component into existence
        }

        #endregion

        #region IPartImportsSatisfiedNotification Members

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            ms_messageBoxService = m_messageBoxService.Value;
        }

        #endregion

        /// <summary>
        /// Display message box</summary>
        /// <param name="message">Message text</param>
        /// <param name="title">Message box title</param>
        /// <param name="buttons">Buttons to display on box</param>
        /// <param name="image">Image to be shown on box</param>
        /// <returns>MessageBox result</returns>
        public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
        {
            return (ms_messageBoxService == null)
                       ? MessageBoxResult.Cancel
                       : ms_messageBoxService.Show(message, title, buttons, image);
        }

        /// <summary>
        /// Display message box</summary>
        /// <param name="message">Message text</param>
        /// <returns>MessageBox result</returns>
        public static MessageBoxResult Show(string message)
        {
            return Show(message, "Error".Localize(), MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #region Private

        private static IMessageBoxService ms_messageBoxService = null;

        #endregion
    }
}
