//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Class for unhandled exception dialogs</summary>
    public partial class UnhandledExceptionDialog : Form
    {
        /// <summary>
        /// Constructor that initializes component</summary>
        public UnhandledExceptionDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Processes a dialog key</summary>
        /// <param name="keyData">One of the System.Windows.Forms.Keys values that represents the key to process</param>
        /// <returns>True iff the key was processed by the control</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter || keyData == Keys.Escape)
                return true;
 
            return base.ProcessDialogKey(keyData);
        }
    }
}