//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows;
using Sce.Atf.Wpf.Controls;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications
{
    /// <summary>
    /// Service that displays error messages to user in an error dialog. The user
    /// can suppress error messages that aren't of interest.</summary>
    [Export(typeof(IOutputWriter))]
    [Export(typeof(ErrorDialogService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ErrorDialogService : IOutputWriter, IPartImportsSatisfiedNotification
    {
        /// <summary>
        /// Gets or sets the messages that the user has suppressed; messages are delimited by newline
        /// string sequences</summary>
        public string SuppressedMessages
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (string message in m_suppressedMessages)
                {
                    sb.Append(message);
                    sb.Append(Environment.NewLine);
                }
                
                return sb.ToString();
            }
            set
            {
                m_suppressedMessages.Clear();
                string[] messages = value.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string message in messages)
                    m_suppressedMessages.Add(message);
            }
        }

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            //if (m_settingsService != null)
            {
                // Note, there is currently no way of letting the user undo a message suppression, so we
                //  should not permanently disable an error message in my opinion. TODO: re-enable this user
                //  setting and then provide some kind of error message lister to re-enable certain error
                //  messages.
                //SettingsServices.RegisterSettings(
                //    m_settingsService,
                //    this,
                //    new BoundPropertyDescriptor(this, () => SuppressedMessages, "SuppressedMessages", null, null));
            }
        }

        #endregion

        #region IOutputWriter Members

        /// <summary>
        /// Writes an output message of the given type</summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message</param>
        public void Write(OutputMessageType type, string message)
        {
            if (type == OutputMessageType.Error ||
                type == OutputMessageType.Warning)
            {
                ShowError(message, message);
            }
        }

        /// <summary>
        /// Clears the writer</summary>
        public void Clear()
        {
        }

        #endregion

        private void ShowError(string messageId, string message)
        {
            messageId = messageId.Replace(Environment.NewLine, string.Empty);// remove newlines to ease persistence

            if (!m_suppressedMessages.Contains(messageId))
            {
                if (Application.Current.MainWindow != null &&
                    Application.Current.MainWindow.IsLoaded)
                {
                    var dlg = new ErrorDialog();

                    var vm = new ErrorDialogViewModel();
                    vm.Message = message;
                    dlg.DataContext = vm;
                    dlg.Owner = Application.Current.MainWindow;
                    dlg.ShowDialog();

                    if (vm.SuppressMessage)
                    {
                        m_suppressedMessages.Add(messageId);
                    }
                }
            }
        }

        private HashSet<string> m_suppressedMessages = new HashSet<string>();
    }
}
