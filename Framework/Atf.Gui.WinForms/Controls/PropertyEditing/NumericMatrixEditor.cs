//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Property editor that can provide controls for editing numeric matrices. All numeric types
    /// except Decimal are supported. Can be used with the standard .NET PropertyGrid, DataGrid,
    /// and DataGridView, as well as the Sce.Atf.Controls.PropertyEditing.PropertyGrid control.</summary>
    public class NumericMatrixEditor : UITypeEditor, IPropertyEditor
    {
        /// <summary>
        /// Constructor</summary>
        /// <remarks>Edits 4 x 4 matrices of Single</remarks>
        public NumericMatrixEditor()
            : this(typeof(Single), 4, 4)
        {
        }

        /// <summary>
        /// Constructor specifying numeric type and matrix dimension</summary>
        /// <param name="numericType">Numeric type of matrix elements</param>
        /// <param name="rows">Number of matrix rows</param>
        /// <param name="columns">Number of matrix columns</param>
        public NumericMatrixEditor(Type numericType, int rows, int columns)
        {
            Define(numericType, rows, columns);
        }

        /// <summary>
        /// Sets the matrix element names and the dimension of the matrix</summary>
        /// <param name="numericType">Numeric type of matrix elements</param>
        /// <param name="rows">Number of matrix rows</param>
        /// <param name="columns">Number of matrix columns</param>
        public void Define(Type numericType, int rows, int columns)
        {
            m_numericType = numericType;
            m_rows = rows;
            m_columns = columns;
        }

        /// <summary>
        /// Gets or sets the scale factor, which is used to scale the value for presentation to the
        /// user. The inverse factor is used to scale the user entered value back.</summary>
        public double ScaleFactor
        {
            get { return m_scaleFactor; }
            set
            {
                if (value == 0)
                    throw new ArgumentException("value must be non-zero");

                m_scaleFactor = value;
            }
        }

        #region IPropertyEditor Members

        /// <summary>
        /// Gets a control to edit the given property</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public Control GetEditingControl(PropertyEditorControlContext context)
        {
            NumericMatrixControl result =
                new NumericMatrixControl(m_numericType, m_rows, m_columns, context);

            // Controls[0] is the 1st TextBox for 1st matrix element
           // result.Height = result.Controls[0].Font.Height * m_rows + result.Margin.Top + result.Margin.Bottom + 2;
            result.ScaleFactor = m_scaleFactor;
            SkinService.ApplyActiveSkin(result);
            return result;
        }

        #endregion

        #region UITypeEditor Implementation

        /// <summary>
        /// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <returns>A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"></see> value that indicates the style of editor used by
        /// the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method.
        /// If the <see cref="T:System.Drawing.Design.UITypeEditor"></see> does not support this method, 
        /// <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"></see> returns 
        /// <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"></see>.</returns>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        /// <summary>
        /// Edits the specified object's value using the editor style indicated by the <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"></see> method</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <param name="provider">An <see cref="T:System.IServiceProvider"></see> that this editor can use to obtain services</param>
        /// <param name="value">The object to edit</param>
        /// <returns>The new value of the object. If the value of the object has not changed, this should return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService =
                provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (editorService != null)
            {
                Sce.Atf.Controls.NumericMatrixControl control =
                    new Sce.Atf.Controls.NumericMatrixControl(m_numericType, m_rows, m_columns);

                if (value != null)
                    control.Value = value;

                editorService.DropDownControl(control);
                value = control.LastEdit; // kind of tricky way to ensure that we don't create a new object with the same value
            }

            return value;
        }

        #endregion

        private Type m_numericType;
        private int m_rows;
        private int m_columns;
        private double m_scaleFactor = 1.0;

        private class NumericMatrixControl : Sce.Atf.Controls.NumericMatrixControl, ICacheablePropertyControl
        {
            public NumericMatrixControl(Type type, int rows, int columns, PropertyEditorControlContext context)
                : base(type, rows, columns)
            {
                m_context = context;

                RefreshValue();
            }

            protected override void OnValueChanged(EventArgs e)
            {
                if (!m_refreshing)
                {
                    m_context.SetValue(LastChange, (Array)Value);
                }

                base.OnValueChanged(e);
            }

            public override void Refresh()
            {
                RefreshValue();

                base.Refresh();
            }

            #region ICacheablePropertyControl

            /// <summary>
            /// Gets <c>True</c> if this control can be used indefinitely, regardless of whether the associated
            /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
            /// This property must be constant for the life of this control.</summary>
            public bool Cacheable
            {
                get { return true; }
            }

            #endregion

            private void RefreshValue()
            {
                try
                {
                    m_refreshing = true;

                    object value = m_context.GetValue();
                    if (value == null)
                        Enabled = false;
                    else
                    {
                        base.Value = value;
                        Enabled = !m_context.IsReadOnly;
                    }
                }
                finally
                {
                    m_refreshing = false;
                }
            }

            readonly PropertyEditorControlContext m_context;
            private bool m_refreshing;
        }
    }
}
