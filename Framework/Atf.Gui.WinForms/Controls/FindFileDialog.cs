//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// Dialog to allow the user to assist an application in finding a missing file</summary>
    public partial class FindFileDialog : Form
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="originalPath">Path to original (missing) file</param>
        public FindFileDialog(string originalPath)
        {
            InitializeComponent();
            missingFileLabel.Text = originalPath;
        }

        /// <summary>
        /// Gets the user's choice of action after the OK button was pressed</summary>
        public FindFileAction Action
        {
            get
            {
                if (searchRadioButton.Checked)
                    return FindFileAction.SearchDirectory;
                if (searchAllRadioButton.Checked)
                    return FindFileAction.SearchDirectoryForAll;
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