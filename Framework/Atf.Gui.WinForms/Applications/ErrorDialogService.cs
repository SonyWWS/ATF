//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows.Forms;

using Sce.Atf.Controls;

namespace Sce.Atf.Applications
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
                string[] messages = value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string message in messages)
                    m_suppressedMessages.Add(message);
            }
        }

        #region IPartImportsSatisfiedNotification Members

        /// <summary>
        /// Notification when part's imports have been satisfied</summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            if (m_settingsService != null)
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
                ShowError(message, message,type);
            }

        }

        /// <summary>
        /// Clears the writer</summary>
        public void Clear()
        {
        }

        #endregion

        private void ShowError(string messageId, string message, OutputMessageType messageType)
        {
            messageId = messageId.Replace(Environment.NewLine, string.Empty);// remove newlines to ease persistence

            if (!m_suppressedMessages.Contains(messageId))
            {
                // lazily create error dialog
                if (m_errorDialog == null)
                {
                    m_errorDialog = new ErrorDialog();
                    m_errorDialog.StartPosition = FormStartPosition.CenterScreen;
                    m_errorDialog.SuppressMessageClicked += errorDialog_SuppressMessageClicked;
                    m_errorDialog.FormClosed += errorDialog_FormClosed;
                }

                if (messageType == OutputMessageType.Error)
                    m_errorDialog.Text = "Error!".Localize();
                else if (messageType == OutputMessageType.Warning)
                    m_errorDialog.Text = "Warning".Localize();
                else if (messageType == OutputMessageType.Info)
                    m_errorDialog.Text = "Info".Localize();           

                m_errorDialog.MessageId = messageId;
                m_errorDialog.Message = message;
                m_errorDialog.Visible = false; //Just in case a second error message comes through, because...
                m_errorDialog.Show(m_owner); //if Visible is true, Show() crashes. Should this be the modal ShowDialog(m_owner)?
            }
        }

        private void errorDialog_SuppressMessageClicked(object sender, EventArgs e)
        {
            if (m_errorDialog.SuppressMessage)
            {
                m_suppressedMessages.Add(m_errorDialog.MessageId);
            }
        }

        private void errorDialog_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_errorDialog.Dispose();
            m_errorDialog = null;
        }

        [Import(AllowDefault = true)]
        private IWin32Window m_owner;

        [Import(AllowDefault = true)]
        private ISettingsService m_settingsService;

        private ErrorDialog m_errorDialog;
        private readonly HashSet<string> m_suppressedMessages = new HashSet<string>();
    }
}
