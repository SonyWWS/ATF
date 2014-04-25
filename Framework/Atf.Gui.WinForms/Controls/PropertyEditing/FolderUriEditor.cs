//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// UITypeEditor class for editing folder paths (stored as strings or URIs)</summary>
    public class FolderUriEditor : FolderNameEditor, Sce.Atf.IAnnotatedParams
    {
        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control
        /// </summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
        }

        #endregion

        /// <summary>
        /// Initialize folder browser dialog</summary>
        /// <param name="dialog">Folder browser dialog</param>
        protected override void InitializeDialog(FolderNameEditor.FolderBrowser dialog)
        {
            base.InitializeDialog(dialog);

            dialog.StartLocation = FolderBrowserFolder.MyComputer;
        }

        /// <summary>
        /// Edits the specified object using the editor style provided by <see cref="M:System.Windows.Forms.Design.FolderNameEditor.GetEditStyle(System.ComponentModel.ITypeDescriptorContext)"></see>.
        /// </summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information.</param>
        /// <param name="provider">A service object provider.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>
        /// The new value of the object, or the old value if the object couldn't be updated.
        /// </returns>
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            object editedValue = base.EditValue(context, provider, value);
            if (value is Uri)
                return new Uri(editedValue.ToString());
            else if (editedValue != null)
                return editedValue.ToString();
            return null;
        }
    }
}

