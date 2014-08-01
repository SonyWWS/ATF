//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Shows a Sce.Atf.Wpf.WpfMessageBox, converting Sce.Atf.Applications.* types for buttons, images, 
    /// and return types to the System.Windows.* types required by Sce.Atf.Wpf.WpfMessageBox.</summary>
    [Export(typeof(IMessageBoxService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class MessageBoxService : IMessageBoxService
    {
        #region IMessageBoxService Members

        /// <summary>
        /// Shows a Sce.Atf.Wpf.WpfMessageBox, converting Sce.Atf.Applications.* types for buttons, images, 
        /// and return types to the System.Windows.* types required by Sce.Atf.Wpf.WpfMessageBox.</summary>
        /// <param name="message">The message to display</param>
        /// <param name="title">The message box title</param>
        /// <param name="button">The message box buttons to display (OK, Cancel, etc)</param>
        /// <param name="image">The image to display (Error, Warning, etc)</param>
        /// <returns>The message box result</returns>
        public MessageBoxResult Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            return m_resultMap[WpfMessageBox.Show(message, title, m_buttonMap[button], m_imageMap[image])];
        }

        #endregion

        private readonly Dictionary<System.Windows.MessageBoxResult, MessageBoxResult> m_resultMap =
            new Dictionary<System.Windows.MessageBoxResult, MessageBoxResult>()
                {
                    { System.Windows.MessageBoxResult.Cancel, MessageBoxResult.Cancel },
                    { System.Windows.MessageBoxResult.No, MessageBoxResult.No },
                    { System.Windows.MessageBoxResult.None, MessageBoxResult.None },
                    { System.Windows.MessageBoxResult.OK, MessageBoxResult.OK },
                    { System.Windows.MessageBoxResult.Yes, MessageBoxResult.Yes },
                };
        
        private readonly Dictionary<MessageBoxButton, System.Windows.MessageBoxButton> m_buttonMap =
            new Dictionary<MessageBoxButton, System.Windows.MessageBoxButton>()
                {
                    { MessageBoxButton.OK, System.Windows.MessageBoxButton.OK },
                    { MessageBoxButton.OKCancel, System.Windows.MessageBoxButton.OKCancel },
                    { MessageBoxButton.YesNo, System.Windows.MessageBoxButton.YesNo },
                    { MessageBoxButton.YesNoCancel, System.Windows.MessageBoxButton.YesNoCancel },
                };
        
        private readonly Dictionary<MessageBoxImage, System.Windows.MessageBoxImage> m_imageMap =
            new Dictionary<MessageBoxImage, System.Windows.MessageBoxImage>()
                {
                    //{ MessageBoxImage.Asterisk, System.Windows.MessageBoxImage.Asterisk },
                    { MessageBoxImage.Error, System.Windows.MessageBoxImage.Error },
                    //{ MessageBoxImage.Exclamation, System.Windows.MessageBoxImage.Exclamation },
                    //{ MessageBoxImage.Hand, System.Windows.MessageBoxImage.Hand },
                    { MessageBoxImage.Information, System.Windows.MessageBoxImage.Information },
                    { MessageBoxImage.None, System.Windows.MessageBoxImage.None },
                    { MessageBoxImage.Question, System.Windows.MessageBoxImage.Question },
                    //{ MessageBoxImage.Stop, System.Windows.MessageBoxImage.Stop },
                    { MessageBoxImage.Warning, System.Windows.MessageBoxImage.Warning },
                };
    }
}
