//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Simple string input UI for initiating string-based searches. Users of the class match against
    /// the inputted string via the Matches() method.</summary>
    public class StringSearchInputUI : ToolStrip
    {
        /// <summary>
        /// Constructor</summary>
        public StringSearchInputUI()
        {
            m_patternTextRegex = string.Empty;

            Visible = true;
            GripStyle = ToolStripGripStyle.Hidden;
            RenderMode = ToolStripRenderMode.System;

            ToolStripDropDownButton dropDownButton = new ToolStripDropDownButton();
            dropDownButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            dropDownButton.Image = ResourceUtil.GetImage16(Resources.SearchImage);
            dropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            dropDownButton.Name = "SearchButton";
            dropDownButton.Size = new System.Drawing.Size(29, 22);
            dropDownButton.Text = "Search".Localize("'Search' is a verb");

            ToolStripButton clearSearchButton = new ToolStripButton();
            clearSearchButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            clearSearchButton.Image = ResourceUtil.GetImage16(Resources.DeleteImage);
            dropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            clearSearchButton.Name = "ClearSearchButton";
            clearSearchButton.Size = new System.Drawing.Size(29, 22);
            clearSearchButton.Text = "Clear Search".Localize("'Clear' is a verb");
            clearSearchButton.Click += clearSearchButton_Click;

            m_patternTextBox = new ToolStripAutoFitTextBox();
            m_patternTextBox.KeyUp += patternTextBox_KeyUp;
            m_patternTextBox.TextChanged += patternTextBox_TextChanged;
            m_patternTextBox.TextBox.PreviewKeyDown += textBox_PreviewKeyDown;
            m_patternTextBox.MaximumWidth = 1080;

            Items.AddRange(new ToolStripItem[] { 
                    dropDownButton, 
                    m_patternTextBox,
                    clearSearchButton
                    });
        }

        /// <summary>
        /// Event that is raised after text control is updated</summary>
        public event EventHandler Updated;

        /// <summary>
        /// Gets whether or not the textbox contains any input</summary>
        /// <returns>True if any sort of string is in the textbox</returns>
        public bool IsNullOrEmpty()
        {
            return m_textBoxEmpty;
        }

        /// <summary>
        /// Returns whether the specified string matches the pattern string in the text box</summary>
        /// <param name="inputString">The string to test for match</param>
        /// <returns>True iff the text box pattern string matches with inputString</returns>
        public bool Matches(string inputString)
        {
            return Regex.Match(inputString, m_patternTextRegex, RegexOptions.IgnoreCase).Success;
        }

        /// <summary>
        ///  Gets the search pattern</summary>
        public string SearchPattern
        {
            get { return m_patternTextBox.Text; }
        }

        /// <summary>
        /// Clears search results</summary>
        public void ClearSearch()
        {
            m_patternTextBox.Text = string.Empty;
            m_patternTextRegex = string.Empty;
            Updated.Raise(this, EventArgs.Empty);
        }



        /// <summary>
        /// Callback that performs custom actions when the 'clear' button has been pressed</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments related to the event</param>
        private void clearSearchButton_Click(object sender, System.EventArgs e)
        {
            ClearSearch();
        }

        /// <summary>
        /// Callback that performs custom actions when any keypress is completed in the text box</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments related to the event</param>
        private void patternTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            m_patternTextRegex = string.Empty;

            if (!string.IsNullOrEmpty(m_patternTextBox.Text))
            {
                bool patternValid = true;
                m_patternTextRegex = m_patternTextBox.Text.Replace("*", "[\\w\\s]+");

                // test that the regex pattern is valid by running a match, and checking for an exception
                try
                {
                    Regex.Match(String.Empty, m_patternTextRegex);
                }
                catch (ArgumentException)
                {
                    patternValid = false;
                }
                m_patternTextRegex = (patternValid) ? m_patternTextRegex : Regex.Escape(m_patternTextBox.Text);
            }

            Updated.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Callback that performs custom actions after any text changed in the text box</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments related to the event</param>
        void patternTextBox_TextChanged(object sender, EventArgs e)
        {
            m_textBoxEmpty = string.IsNullOrEmpty(m_patternTextBox.Text);
        }

        /// <summary>
        /// Callback that performs custom actions after the preview key is pressed</summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">Arguments related to the event</param>
        void textBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyData == Keys.Escape)
                clearSearchButton_Click(sender, e);
        }

        private readonly ToolStripAutoFitTextBox m_patternTextBox;
        private string m_patternTextRegex;
        private bool m_textBoxEmpty=true;
    }
}