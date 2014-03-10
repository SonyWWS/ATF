using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Utilities for working with PropertyDescriptors</summary>
    public static class WinFormsPropertyUtils
    {
        /// <summary>
        /// Gets a UI type editor for the given property descriptor and context</summary>
        /// <param name="descriptor">Property descriptor</param>
        /// <param name="context">Type descriptor context</param>
        /// <returns>UI type editor for the given property descriptor and context</returns>
        public static UITypeEditor GetUITypeEditor(
            PropertyDescriptor descriptor,
            ITypeDescriptorContext context)
        {
            UITypeEditor editor = descriptor.GetEditor(typeof(UITypeEditor)) as UITypeEditor;
            if (editor == null)
            {
                if (StandardValuesUIEditor.CanCreateStandardValues(descriptor, context))
                {
                    editor = new StandardValuesUIEditor(descriptor);
                }
                else
                {
                    Type type = descriptor.PropertyType;
                    editor = TypeDescriptor.GetEditor(type, typeof(UITypeEditor)) as UITypeEditor;
                }
            }

            return editor;
        }

        private class StandardValuesUIEditor : UITypeEditor
        {
            public static bool CanCreateStandardValues(
                PropertyDescriptor descriptor,
                ITypeDescriptorContext context)
            {
                TypeConverter converter = descriptor.Converter;
                return
                    converter != null &&
                    converter.GetStandardValuesSupported(context);
            }

            public StandardValuesUIEditor(PropertyDescriptor descriptor)
            {
                m_descriptor = descriptor;
                m_converter = descriptor.Converter;
                if (m_converter == null)
                    throw new ArgumentException("descriptor has no Converter");
            }

            /// <summary>
            /// Gets the editor style used by the System.Drawing.Design.UITypeEditor.EditValue(
            /// System.IServiceProvider,System.Object) method</summary>
            /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that can be used to gain
            /// additional context information</param>
            /// <returns>A System.Drawing.Design.UITypeEditorEditStyle value that indicates the style
            ///     of editor used by the System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)
            ///     method. If the System.Drawing.Design.UITypeEditor does not support this method,
            ///     System.Drawing.Design.UITypeEditor.GetEditStyle() returns System.Drawing.Design.UITypeEditorEditStyle.None.</returns>
            public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
            {
                return UITypeEditorEditStyle.DropDown;
            }

            /// <summary>
            /// Edits the specified object's value using the editor style indicated by the
            /// System.Drawing.Design.UITypeEditor.GetEditStyle() method</summary>
            /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that can be used to gain
            /// additional context information</param>
            /// <param name="provider">An System.IServiceProvider that this editor can use to obtain services</param>
            /// <param name="value">The object to edit</param>
            /// <returns>The new value of the object. If the value of the object has not changed, this should
            /// return the same object it was passed.</returns>
            public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
            {
                m_editorService = provider.GetService(
                    typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

                if (m_editorService != null)
                {
                    StandardValuesListBox listbox = new StandardValuesListBox(this);
                    listbox.SelectedIndexChanged += listbox_SelectedIndexChanged;

                    System.Collections.ICollection values = m_converter.GetStandardValues(context);
                    foreach (object item in values)
                        if (!listbox.Items.Contains(item))
                            listbox.Items.Add(item);

                    listbox.SelectedItem = value;

                    m_editorService.DropDownControl(listbox);

                    if (listbox.SelectedItem != null)
                        return listbox.SelectedItem;
                }

                return value;
            }

            private void listbox_SelectedIndexChanged(object sender, EventArgs e)
            {
                m_editorService.CloseDropDown();
            }

            private string GetValueAsText(object value)
            {
                if (value == null)
                    return string.Empty;
                if (value is string)
                    return (string)value;

                if (m_converter != null && m_converter.CanConvertTo(typeof(string)))
                    return m_converter.ConvertToString(value);

                return value.ToString();
            }

            private PropertyDescriptor m_descriptor;
            private readonly TypeConverter m_converter;
            private IWindowsFormsEditorService m_editorService;

            private class StandardValuesListBox : ListBox
            {
                readonly StandardValuesUIEditor m_editor;

                public StandardValuesListBox(StandardValuesUIEditor editor)
                {
                    m_editor = editor;
                    base.BorderStyle = BorderStyle.None;
                    base.DrawMode = DrawMode.OwnerDrawVariable;
                }

                protected override void OnDrawItem(DrawItemEventArgs e)
                {
                    e.DrawBackground();

                    // yes, this is necessary
                    if (e.Index < 0 || e.Index >= Items.Count)
                        return;

                    object value = Items[e.Index];
                    string valueString = m_editor.GetValueAsText(value);

                    Brush brush = (e.Index == base.SelectedIndex) ?
                                                                      SystemBrushes.HighlightText :
                                                                                                      SystemBrushes.WindowText;
                    e.Graphics.DrawString(valueString, base.Font, brush, e.Bounds);
                }

                protected override void OnMeasureItem(MeasureItemEventArgs e)
                {
                    e.ItemHeight += 1;
                }
            }
        }
    }
}