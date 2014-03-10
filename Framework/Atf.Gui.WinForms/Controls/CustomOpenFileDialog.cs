//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf
{
    /// <summary>
    /// This class behaves the same as the System.Windows.Forms.OpenFileDialog class.
    /// Allows setting "Read Only" check box. Can indicate whether supports multiple file name selections.</summary>
    public class CustomOpenFileDialog : CustomFileDialog
    {
        /// <summary>
        /// Constructor. Sets CheckFileExists to true.</summary>
        public CustomOpenFileDialog()
        {
            CheckFileExists = true;
        }

        /// <summary>
        /// Gets or sets a boolean that determines the initial state of the Read Only check box.
        /// This property is not updated to reflect any user action.</summary>
        public bool ReadOnlyChecked
        {
            get { return GetFlag(OFN_READONLY); }
            set { SetFlag(OFN_READONLY, value); }
        }

        /// <summary>
        /// Gets or sets a boolean that determines if the Read Only check box is visible</summary>
        public bool ShowReadOnly
        {
            get { return !GetFlag(OFN_HIDEREADONLY); }
            set { SetFlag(OFN_HIDEREADONLY, !value); }
        }

        /// <summary>
        /// Gets or sets whether the File Name list box allows multiple selections</summary>
        public bool Multiselect
        {
            get { return GetFlag(OFN_ALLOWMULTISELECT); }
            set { SetFlag(OFN_ALLOWMULTISELECT, value); }
        }

        /// <summary>
        /// Creates an OpenFileDialog, initializes its derived properties, and invokes
        /// ShowNonCustomDialogInternal to display it.</summary>
        /// <param name="owner">The owner of the dialog</param>
        /// <returns>The DialogResult from the OpenFileDialog</returns>
        internal protected override DialogResult ShowNonCustomDialog(IWin32Window owner)
        {
            var ofd = new OpenFileDialog();
            ofd.ReadOnlyChecked = ReadOnlyChecked;
            ofd.ShowReadOnly = ShowReadOnly;
            ofd.Multiselect = Multiselect;
            return ShowNonCustomDialogInternal(ofd, owner);
        }
    }
}