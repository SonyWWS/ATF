//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog for finding missing files that uses a suggested path</summary>
    public partial class FindFileWithSuggestionDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="originalPath">Path to original (missing) file</param>
        /// <param name="suggestedPath">Suggested path to search</param>
        public FindFileWithSuggestionDialog(string originalPath, string suggestedPath)
        {
            InitializeComponent();
            missingFileLabel.Text = originalPath;
            suggestedFileLabel.Text = suggestedPath;
        }

        /// <summary>
        /// Gets the user's choice of action after the OK button was pressed</summary>
        public FindFileAction Action
        {
            get
            {
                if (acceptRadioButton.Checked)
                    return FindFileAction.AcceptSuggestion;
                if (acceptAllRadioButton.Checked)
                    return FindFileAction.AcceptAllSuggestions;
                if (specifyRadioButton.Checked)
                    return FindFileAction.UserSpecify;
                if (ignoreRadioButton.Checked)
                    return FindFileAction.Ignore;
                if (ignoreAllRadioButton.Checked)
                    return FindFileAction.IgnoreAll;

                throw new InvalidOperationException("no radio buttons were checked");
            }
        }
    }
}