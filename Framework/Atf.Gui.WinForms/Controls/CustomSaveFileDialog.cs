//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf
{
    /// <summary>
    /// This class behaves the same as the System.Windows.Forms.SaveFileDialog class.
    /// Allows prompting user when file does not exist and when file already exists.</summary>
    public class CustomSaveFileDialog : CustomFileDialog
    {
        /// <summary>
        /// Constructor. Creates a Save File dialog box that prompts before overwriting an
        /// existing file.</summary>
        public CustomSaveFileDialog()
        {
            CreatePrompt = false;
            OverwritePrompt = true;
        }

        /// <summary>
        /// Gets or sets a prompt user flag.
        /// If the user specifies a file that does not exist, this flag causes a dialog box to be displayed to
        /// prompt the user for permission to create the file. If the user chooses to create the
        /// file, the dialog box closes and the function returns the specified name; otherwise, the
        /// dialog box remains open. If you use this flag with the OFN_ALLOWMULTISELECT flag, the
        /// dialog box allows the user to specify only one nonexistent file.
        /// Probably should always be false for saving a file, because it doesn't make sense to
        /// create a new document in the dialog box that is used to save an existing document.</summary>
        public bool CreatePrompt
        {
            get { return GetFlag(OFN_CREATEPROMPT); }
            set { SetFlag(OFN_CREATEPROMPT, value); }
        }

        /// <summary>
        /// Gets or sets whether the Save As dialog box generates a message box if the selected file already
        /// exists. The user must confirm whether to overwrite the file.</summary>
        public bool OverwritePrompt
        {
            get { return GetFlag(OFN_OVERWRITEPROMPT); }
            set { SetFlag(OFN_OVERWRITEPROMPT, value); }
        }

        /// <summary>
        /// Creates a SaveFileDialog, initializes its derived properties, and invokes
        /// ShowNonCustomDialogInternal to display it.</summary>
        /// <param name="owner">The owner of the dialog</param>
        /// <returns>The DialogResult from the SaveFileDialog</returns>
        internal protected override DialogResult ShowNonCustomDialog(IWin32Window owner)
        {
            var sfd = new SaveFileDialog();
            sfd.CreatePrompt = CreatePrompt;
            sfd.OverwritePrompt = OverwritePrompt;
            return ShowNonCustomDialogInternal(sfd, owner);
        }
    }
}
