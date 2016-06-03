//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// UITypeEditor to handle dynamic flag "enumerations"</summary>
    public class FlagsUITypeEditor : UITypeEditor, IAnnotatedParams
    {
        /// <summary>
        /// Default constructor</summary>
        public FlagsUITypeEditor()
        {
        }

        /// <summary>
        /// Constructor with flag names</summary>
        /// <param name="names">Array of flag definitions, such as "FlagA=2", or just the name, as in "FlagA"</param>
        /// <remarks>Flag values default to successive powers of 2, starting with 1</remarks>
        public FlagsUITypeEditor(string[] names)
        {
            DefineFlags(names);
        }

        /// <summary>
        /// Constructor with flag names and values</summary>
        /// <param name="names">Array of flag names, such as "FlagNameA". Specifying the value in the string
        /// (as in "FlagNameA=2") is not allowed.</param>
        /// <param name="values">Flag values array</param>
        public FlagsUITypeEditor(string[] names, int[] values)
        {
            DefineFlags(names, values);
        }

        /// <summary>
        /// Defines the flag names and values</summary>
        /// <param name="definitions">Flag definitions array</param>
        /// <remarks>Flag values default to successive powers of 2, starting with 1. Flag names
        /// with the format "FlagName=X" are parsed so that FlagName gets the value X, where X is
        /// an int.</remarks>
        public void DefineFlags(string[] definitions)
        {
            EnumUtil.ParseFlagDefinitions(definitions, out m_names, out m_displayNames, out m_values);
        }

        /// <summary>
        /// Defines the flag names and values</summary>
        /// <param name="names">Flag names array</param>
        /// <param name="values">Flag values array</param>
        public void DefineFlags(string[] names, int[] values)
        {
            if (names == null || values == null || names.Length != values.Length)
                throw new ArgumentException("names and/or values null, or of unequal length");

            m_names = names;
            m_displayNames = names;
            m_values = values;
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
            m_editorService =
                provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (m_editorService != null)
            {
                CheckedListBox checkedListBox = new CheckedListBox();
                checkedListBox.CheckOnClick = true;
                foreach (string displayName in m_displayNames)
                    checkedListBox.Items.Add(displayName);

                // size control so all strings are completely visible
                using (System.Drawing.Graphics g = checkedListBox.CreateGraphics())
                {
                    float width = 0f;

                    foreach (string displayName in m_displayNames)
                    {
                        float w = g.MeasureString(displayName, checkedListBox.Font).Width;
                        width = Math.Max(width, w);
                    }

                    float height = m_displayNames.Length * checkedListBox.ItemHeight;
                    int scrollBarThickness = SystemInformation.VerticalScrollBarWidth;
                    if (height > checkedListBox.Height - 4) // vertical scrollbar?
                        width += SystemInformation.VerticalScrollBarWidth;

                    if (width > checkedListBox.Width)
                        checkedListBox.Width = (int)width + 31; // magic number from Windows.Forms dll
                }

                if (value is string)
                    FillCheckedListBoxFromString(value, checkedListBox);
                else if (value is int || value is uint)
                    FillCheckedListBoxFromInt(value, checkedListBox);
                // otherwise, ignore value

                m_editorService.DropDownControl(checkedListBox);

                object newValue;
                if (value is string)
                    newValue = ExtractStringFromCheckedListBox(checkedListBox);
                else
                    newValue = ExtractIntFromCheckedListBox(checkedListBox);
                // be careful to return the same object if the value didn't change
                if (!newValue.Equals(value))
                    value = newValue;
            }

            return value;
        }

        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
            DefineFlags(parameters);
        }

        #endregion

        private object ExtractIntFromCheckedListBox(CheckedListBox checkedListBox)
        {
            CheckedListBox.CheckedIndexCollection indices = checkedListBox.CheckedIndices;

            int intValue = 0;
            foreach (int index in indices)
                intValue |= m_values[index];

            return intValue;
        }

        private object ExtractStringFromCheckedListBox(CheckedListBox checkedListBox)
        {
            CheckedListBox.CheckedIndexCollection indices = checkedListBox.CheckedIndices;
            StringBuilder sb = new StringBuilder();

            foreach (int index in indices)
            {
                sb.Append(m_names[index]);
                sb.Append("|");
            }

            if (sb.Length > 0)
                sb.Length--; // trim last "|"
            else
                sb.Append(NoFlags);

            return sb.ToString();
        }

        private void FillCheckedListBoxFromInt(object value, CheckedListBox listBox)
        {
            int flags = (int)value;
            for (int i = 0; i < m_values.Length; ++i)
            {
                bool isChecked = (flags & m_values[i]) == m_values[i];
                listBox.SetItemChecked(i, isChecked);
            }
        }

        private void FillCheckedListBoxFromString(object obj, CheckedListBox listBox)
        {
            string value = obj.ToString();
            if (value == NoFlags)
                return;

            char[] delimiters = new[] { '|', ';', ',', ':' };
            string[] flags = value.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (string flag in flags)
            {
                for (int i = 0; i < m_names.Length; i++)
                {
                    if (flag == m_names[i])
                    {
                        listBox.SetItemChecked(i, true);
                        break;
                    }
                }
            }
        }
        
        private string[] m_names;
        private string[] m_displayNames;
        private int[] m_values;
        private IWindowsFormsEditorService m_editorService;

        private static readonly string NoFlags = "(none)".Localize("No flags");
    }
}
