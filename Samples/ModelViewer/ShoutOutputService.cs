//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Applications;

namespace ModelViewerSample
{
    /// <summary>
    /// OutputService that has a more colorful output</summary>
    [Export(typeof(IInitializable))]
    [Export(typeof(IOutputWriter))]
    [Export(typeof(ShoutOutputService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ShoutOutputService : OutputService
    {
        /// <summary>
        /// Constructor with IControlHostService</summary>
        /// <param name="controlHostService">Control host service used to register the output control</param>
        [ImportingConstructor]
        public ShoutOutputService(IControlHostService controlHostService)
            : base(controlHostService)
        {
        }

        /// <summary>
        /// Displays message to the user in a RichTextBox Control</summary>
        /// <param name="messageType">Message type, which modifies display of message</param>
        /// <param name="message">Text message to display</param>
        /// <param name="textBox">RichTextBox in which to display message</param>
        protected override void OutputMessage(OutputMessageType messageType, string message, RichTextBox textBox)
        {
            Color c;
            string messageTypeText;
            Font font;

            switch (messageType)
            {
                case OutputMessageType.Error:
                    c = Color.Red;
                    messageTypeText = "Danger!";
                    font = s_errorFont;
                    break;
                case OutputMessageType.Warning:
                    c = Color.Orange;
                    messageTypeText = "Careful.";
                    font = s_warningFont;
                    break;
                default:
                    c = Color.Beige;
                    messageTypeText = "<Yawn>";
                    font = Font;
                    break;
            }

            textBox.SelectionFont = font;
            textBox.SelectionColor = c;
            textBox.AppendText(messageTypeText + ": " + message);
        }

        /// <summary>
        /// Constructor with no parameters</summary>
        static ShoutOutputService()
        {
            s_warningFont = new Font(FontFamily.GenericSansSerif, 10.0f, FontStyle.Italic);
            s_errorFont = new Font(FontFamily.GenericSerif, 12.0f, FontStyle.Bold);
        }

        private static readonly Font s_warningFont;
        private static readonly Font s_errorFont;
    }
}
