//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service that displays text output to the user in a RichTextBox. If this component is added to a MEF
    /// catalog, the Outputs component should probably be added as well.</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IOutputWriter))]
    [Export(typeof(OutputService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class OutputService : IControlHostClient, ICommandClient, IOutputWriter, IInitializable
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="controlHostService">Control host service used to register the output Control</param>
        [ImportingConstructor]
        public OutputService(IControlHostService controlHostService)
        {
            m_controlHostService = controlHostService;

            m_textBox = new RichTextBox();
            m_textBox.Multiline = true;
            m_textBox.ScrollBars = RichTextBoxScrollBars.Both;
            m_textBox.WordWrap = false;
            m_textBox.ReadOnly = true;
            m_textBox.MouseUp += textBox_MouseUp;
            m_textBox.BorderStyle = BorderStyle.None;
            //m_textBox.FontChanged += textBox_FontChanged;
            //m_textBox.ParentChanged += textBox_ParentChanged;
            
            // Force creation of the handle to be sure that the RichTextBox's handle is created on the GUI thread.
            var hwnd = m_textBox.Handle;
        }

        #region IInitializable Members

        /// <summary>
        /// Finishes initializing component by registering Output window control</summary>
        public virtual void Initialize()
        {
            m_controlHostService.RegisterControl(m_textBox,
                "Output".Localize("title of Output window"),
                "View errors, warnings, and informative messages".Localize(),
                StandardControlGroup.Bottom,
                this);

            RegisterCommands();
        }

        #endregion

        #region IOutputWriter Members

        /// <summary>
        /// Writes an output message of the given type</summary>
        /// <param name="type">Message type</param>
        /// <param name="message">Message</param>
        public void Write(OutputMessageType type, string message)
        {
            OutputMessage(type, message);
        }

        /// <summary>
        /// Formats and writes an output message of the given type</summary>
        /// <param name="type">Message type</param>
        /// <param name="formatString">Message format string</param>
        /// <param name="args">Message arguments</param>
        /// <remarks>Use writer formatting when possible to help writers correctly classify
        /// output messages. For example, a writer that presents a dialog to the user can
        /// suppress messages of a given class, even though they may differ in specifics such
        /// as file name, exception message, etc.</remarks>
        public void Write(OutputMessageType type, string formatString, params object[] args)
        {
            string message = string.Format(formatString, args);
            Write(type, message);
        }

        /// <summary>
        /// Clears the writer</summary>
        public void Clear()
        {
            if (m_textBox.InvokeRequired)
            {
                // we must do this asynchronously in case the owning thread is blocking on this thread
                m_textBox.BeginInvoke(new ThreadStart(Clear));
                return;
            }

            m_textBox.Text = string.Empty;
            m_textBox.ScrollToCaret();
        }

        #endregion

        /// <summary>
        /// Gets and sets the default font to be used to display the text. Can be set either before
        /// or after Init() is called. This default font can be overridden on a case by case basis by
        /// OutputMessage().</summary>
        public Font Font
        {
            get { return m_textBox.Font; }
            set { m_textBox.Font = value; }
        }

        /// <summary>
        /// Gets and sets the background color of the text box</summary>
        public Color BackColor
        {
            get { return m_textBox.BackColor; }
            set { m_textBox.BackColor = value; }
        }

        /// <summary>
        /// Gets the RichTextBox used to display the text</summary>
        public RichTextBox TextBox
        {
            get { return m_textBox; }
        }

        /// <summary>
        /// Appends the desired message to the text box (using AppendText) and optionally sets the font
        /// and font color to be used for this text. Can alter the message or take other actions
        /// depending on the content of the message.</summary>
        /// <param name="messageType">Type of message</param>
        /// <param name="message">Text intended to be read by the user</param>
        /// <param name="textBox">Ccontrol used to display text to the user</param>
        protected virtual void OutputMessage(OutputMessageType messageType, string message, RichTextBox textBox)
        {
            Color c;
            string messageTypeText;

            switch (messageType)
            {
                case OutputMessageType.Error:
                    c = Color.Red;
                    messageTypeText = "Error".Localize("Label for error message");
                    break;
                case OutputMessageType.Warning:
                    c = Color.Orange;
                    messageTypeText = "Warning".Localize("Label for warning message");
                    break;
                default:
                    c = textBox.ForeColor;
                    messageTypeText = "Info".Localize("Label for informative message");
                    break;
            }

            textBox.SelectionColor = c;
            textBox.AppendText(messageTypeText + ": " + message);            
        }

        /// <summary>
        /// Displays message to the user in a text control</summary>
        /// <param name="messageType">Message type, which modifies display of message</param>
        /// <param name="message">Text message to display</param>
        public void OutputMessage(OutputMessageType messageType, string message)
        {
            if (m_textBox.InvokeRequired)
            {
                // we must do this asynchronously in case the owning thread is blocking on this thread
                m_textBox.BeginInvoke(new Action<OutputMessageType,string>(OutputMessage), messageType, message);
                return;
            }

            if (m_textBox.IsDisposed)
                return;

            // for performance reasons, clear output if there is too much text
            if (m_textBox.TextLength > 1048576)
                m_textBox.Text = string.Empty;

            OutputMessage(messageType, message, m_textBox);

            m_textBox.ScrollToCaret();
        }

        #region IControlHostClient Members

        /// <summary>
        /// Notifies the client that its Control has been activated. Activation occurs when
        /// the Control gets focus, or a parent "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was activated</param>
        public void Activate(Control control)
        {
            if (m_commandService!= null)
                m_commandService.SetActiveClient(this);
        }

        /// <summary>
        /// Notifies the client that its Control has been deactivated. Deactivation occurs when
        /// another Control or "host" Control gets focus.</summary>
        /// <param name="control">Client Control that was deactivated</param>
        public void Deactivate(Control control)
        {
            if (m_commandService != null)
                m_commandService.SetActiveClient(null);
        }

        /// <summary>
        /// Requests permission to close the client's Control</summary>
        /// <param name="control">Client Control to be closed</param>
        /// <returns>True if the control can close, or false to cancel</returns>
        public bool Close(Control control)
        {
            return true;
        }

        #endregion

        #region ICommandClient Interface

        /// <summary>
        /// Checks whether the client can do the command if it handles it</summary>
        /// <param name="commandTag">Command to be done</param>
        /// <returns>True if client can do the command</returns>
        public bool CanDoCommand(object commandTag)
        {
            bool canDo = false;

            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.EditCopy:
                        canDo = m_textBox.Focused && (m_textBox.SelectionLength > 0);
                        break;

                    case StandardCommand.EditSelectAll:
                        canDo = m_textBox.Focused && (m_textBox.TextLength > 0);
                        break;
                }
            }

            return canDo;
        }

        /// <summary>
        /// Does the command</summary>
        /// <param name="commandTag">Command to be done</param>
        public void DoCommand(object commandTag)
        {
            if (commandTag is StandardCommand)
            {
                switch ((StandardCommand)commandTag)
                {
                    case StandardCommand.EditCopy:
                        m_textBox.Copy();
                        break;

                    case StandardCommand.EditSelectAll:
                        m_textBox.SelectAll();
                        break;
                }
            }
        }

        /// <summary>
        /// Updates command state for given command</summary>
        /// <param name="commandTag">Command</param>
        /// <param name="state">Command state to update. See <see cref="CommandState"/>.</param>
        public void UpdateCommand(object commandTag, CommandState state)
        {
        }

        /// <summary>
        /// Gets command tags for context menu (right click) commands</summary>
        /// <param name="target">Command target that owns the popup menu</param>
        /// <param name="clicked">Object clicked on by mouse right click</param>
        /// <returns>Enumeration of command tags for context menu</returns>
        public  IEnumerable<object> GetPopupCommandTags(object target, object clicked)
        {
            return new object[]
                {
                    StandardCommand.EditCopy,
                    StandardCommand.EditSelectAll,
                };
        }


        #endregion

        private void RegisterCommands()
        {
            if (m_commandService != null)
            {
                m_commandService.RegisterCommand(StandardCommand.EditCopy, CommandVisibility.All, this);
                m_commandService.RegisterCommand(StandardCommand.EditSelectAll, CommandVisibility.Menu, this);
            }
        }

        private void textBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Point clientPoint = new Point(e.X, e.Y);
                List<object> commands = new List<object>(GetPopupCommandTags(this, null));

                Point screenPoint = m_textBox.PointToScreen(clientPoint);
                m_commandService.RunContextMenu(commands, screenPoint);
            }
        }

       

        private readonly IControlHostService m_controlHostService;

        [Import(AllowDefault = true)]
        private ICommandService m_commandService;

        private readonly RichTextBox m_textBox;
        
    }
}
