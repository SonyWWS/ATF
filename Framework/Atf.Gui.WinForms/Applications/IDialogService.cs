//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Forms;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Service to abstract display of parented WinForms dialog.
    /// This allows WinForms dialogs to be displayed in either a WPF
    /// or WinForms based application.</summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows WinForms dialog parented to main application window</summary>
        /// <param name="form">Form to show</param>
        /// <returns>Dialog result</returns>
        DialogResult ShowParentedDialog(Form form);
    }
    
}
