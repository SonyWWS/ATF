//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Collections.Generic;
using System.Drawing;

namespace Sce.Atf.Controls.ConsoleBox
{
    /// <summary>
    /// Interface for a text box for a console</summary>
    public interface IConsoleTextBox
    {
        /// <summary>
        /// Gets and sets command prompts. Default prompt is ">>> ".</summary>
        string Prompt { get; set; }

        /// <summary>
        /// Sets command handler</summary>
        CmdHandler CommandHandler { set; }

        /// <summary>
        /// Clears console window</summary>
        void Clear();

        /// <summary>
        /// Enter command programatically.
        /// The result is equivalent to manually typing the command and pressing the Enter key.
        /// The command doesn't need to terminate with a newline.</summary>
        /// <param name="cmd">Command</param>
        void EnterCommand(string cmd);

        /// <summary>
        /// Gets or sets background color</summary>
        Color BackColor { get; set; }
        /// <summary>
        /// Gets or sets foreground color</summary>
        Color ForeColor { get; set; }

        /// <summary>
        /// Writes to the console window</summary>
        /// <param name="text">Text to write</param>
        void Write(string text);

        /// <summary>
        /// Writes to the console window, appending a newline character</summary>
        /// <param name="text">Text to write</param>
        void WriteLine(string text);

        /// <summary>
        /// Gets the underlying control</summary>
        System.Windows.Forms.Control Control { get; }

        /// <summary>
        /// Sets the SuggestionHandler used for auto complete suggestions</summary>
        SuggestionHandler SuggestionHandler { set; }
    }

    /// <summary>
    /// Command handler. This delegate is called when command entered.</summary>
    public delegate void CmdHandler(string cmd);

    /// <summary>
    /// Suggestion handler.  This gets called when the user types Ctrl-space in the console.</summary>
    public delegate IEnumerable<string> SuggestionHandler(string obj, string trigger);
}
