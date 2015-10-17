//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class to hold command state, which ICommandClients can update before menus are displayed</summary>
    public class CommandState
    {
        /// <summary>
        /// Constructor</summary>
        public CommandState()
        {
        }

        /// <summary>
        /// Constructor using command text and check indicator</summary>
        /// <param name="text">Command text</param>
        /// <param name="check"><c>True</c> if command has check</param>
        public CommandState(string text, bool check)
        {
            m_text = text;
            m_check = check;
        }

        /// <summary>
        /// Gets or sets command text for display in menu or tooltip</summary>
        public string Text
        {
            get { return m_text; }
            set { m_text = value; }
        }

        /// <summary>
        /// Gets or sets whether command should have a check in menu</summary>
        public bool Check
        {
            get { return m_check; }
            set { m_check = value; }
        }

        private string m_text = "";
        private bool m_check;
    }
}
