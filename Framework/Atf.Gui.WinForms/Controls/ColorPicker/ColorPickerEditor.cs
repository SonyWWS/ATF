//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Sce.Atf.Controls.ColorEditing;

namespace Sce.Atf.Controls
{
    /// <summary>
    /// UITypeEditor for editing colors with the ColorPicker class</summary>
    public class ColorPickerEditor : ColorEditor
    {
        /// <summary>
        /// Gets the editing style of the Edit method. If the method is not supported, this returns <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"></see>.</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <returns>An enum value indicating the provided editing style</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// Edits the given object value using the editor style provided by the <see cref="M:System.Drawing.Design.ColorEditor.GetEditStyle(System.ComponentModel.ITypeDescriptorContext)"></see> method</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <param name="provider">An <see cref="T:System.IServiceProvider"></see> through which editing services may be obtained</param>
        /// <param name="value">An instance of the value being edited</param>
        /// <returns>The new value of the object. If the value of the object has not changed, this should return the same object it was passed.</returns>
        public override object EditValue(
            ITypeDescriptorContext context,
            IServiceProvider provider,
            object value)
        {
            IWindowsFormsEditorService editorService =
                provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService != null)
            {
                ColorPicker picker = new ColorPicker((Color)value, m_enableAlpha);
                if (DialogResult.OK == editorService.ShowDialog(picker))
                    value = picker.PrimaryColor;
            }
            return value;
        }

        /// <summary>
        /// Gets or sets whether alpha is enabled. If true, user can edit the alpha value of the color in the editor. 
        /// If false, the alpha is not editable, and is set to 255.</summary>
        public bool EnableAlpha
        {
            get { return m_enableAlpha; }
            set { m_enableAlpha = value; }
        }

        private bool m_enableAlpha;
    }
}
