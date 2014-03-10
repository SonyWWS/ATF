//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Wraps a FolderBrowserDialog for use in a PropertyGrid</summary>
    public class FolderBrowserDialogUITypeEditor : UITypeEditor
    {
        /// <summary>
        /// Constructor</summary>
        public FolderBrowserDialogUITypeEditor()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Constructor with dialog description</summary>
        /// <param name="description">Dialog description</param>
        public FolderBrowserDialogUITypeEditor(string description)
        {
            // create instance of editing control, 
            // resuse this instance for subsequence calls
            m_dialog = new FolderBrowserDialog();
            m_dialog.Description = description;
        }

        /// <summary>
        /// Gets and sets the FolderBrowserDialog's description</summary>
        public string Description
        {
            get { return m_dialog.Description; }
            set { m_dialog.Description = value; }
        }

        /// <summary>
        /// Edits the specified object's value using the editor style indicated by the
        /// System.Drawing.Design.UITypeEditor.GetEditStyle() method</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that can be used to gain
        /// additional context information</param>
        /// <param name="sp">An System.IServiceProvider that this editor can use to obtain services</param>
        /// <param name="value">The object to edit</param>
        /// <returns>The new value of the object. If the value of the object has not changed, this should
        /// return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider sp, object value)
        {
            IWindowsFormsEditorService editorService =
                (IWindowsFormsEditorService)sp.GetService(typeof(IWindowsFormsEditorService));

            if (editorService != null)
            {
                if (value != null)
                    m_dialog.SelectedPath = value.ToString();
                m_dialog.ShowDialog();
                value = m_dialog.SelectedPath;
            }

            return value;
        }

        /// <summary>
        /// Gets the editor style used by the System.Drawing.Design.UITypeEditor.EditValue(
        /// System.IServiceProvider,System.Object) method</summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that can be used to gain
        /// additional context information</param>
        /// <returns>A System.Drawing.Design.UITypeEditorEditStyle value that indicates the style
        ///     of editor used by the System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)
        ///     method. If the System.Drawing.Design.UITypeEditor does not support this method,
        ///     then System.Drawing.Design.UITypeEditor.GetEditStyle() returns System.Drawing.Design.UITypeEditorEditStyle.None.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal; // editor type is modal dialog.
        }

        private readonly FolderBrowserDialog m_dialog;
    }
}
