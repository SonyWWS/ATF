//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Property editor that can provide controls for editing numeric tuples (vectors) of arbitrary
    /// dimension. All numeric types except Decimal are supported. Can be used with the standard
    /// .NET PropertyGrid, DataGrid, and DataGridView, as well as the
    /// Sce.Atf.Controls.PropertyEditing.PropertyGrid control.</summary>
    public class NumericTupleEditor : UITypeEditor, IPropertyEditor, IAnnotatedParams
    {
        /// <summary>
        /// Constructs instance with the default ability of editing tuples
        /// with 3 single-precision components, named "x", "y", and "z"</summary>
        public NumericTupleEditor()
            : this(typeof(Single), new[] { "x", "y", "z" })
        {
        }

        /// <summary>
        /// Constructor for the given numeric type, component names, and dimension</summary>
        /// <param name="numericType">Numeric type of tuple coordinates</param>
        /// <param name="names">Tuple coordinate names. The length of this array determines the dimension of the tuples.</param>
        /// <remarks>All numeric types, except Decimal, are supported</remarks>
        public NumericTupleEditor(Type numericType, string[] names)
        {
            Define(numericType, names);
        }

        /// <summary>
        /// Sets the tuple coordinate names and the dimension of the tuple</summary>
        /// <param name="numericType">Numeric type of tuple coordinates</param>
        /// <param name="names">Tuple coordinate names. The length of this array determines the dimension of the tuples.</param>
        public void Define(Type numericType, string[] names)
        {
            m_numericType = numericType;
            m_names = names;
            m_labelColors = new Color[]
            {
                Color.FromArgb(200,40,0),
                Color.FromArgb(100,160,0),
                Color.FromArgb(40,120,240),
                Color.FromArgb(20,20,20),
            };
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

        #region IAnnotatedParams Members

        void IAnnotatedParams.Initialize(string[] parameters)
        {
            Type numericType = Type.GetType(parameters[0]);
            string[] names = new string[parameters.Length - 1];
            Array.Copy(parameters, 1, names, 0, names.Length);
            Define(numericType, names);
        }

        #endregion

        
        /// <summary>
        /// Gets or sets whether axis labels should be hidden. The labels are visible by default.</summary>
        public bool HideAxisLabel
        {
            get;
            set;
        }

        /// <summary>
        /// Sets background colors used for drawing labels</summary>
        /// <param name="color">The array of colors to be used for drawing the background text color
        /// for the component labels. By default, 'x' has a red background, 'y' green, and 'z' blue.
        /// The actual text of the label is always white. The minimum length of 'color' is 1. If there
        /// are more components than colors in the array, the chosen index will cycle back to the
        /// beginning of the array.</param>
        public void SetLabelBackColors(Color[] color)
        {
            m_labelColors = color;
        }
        private Color[] m_labelColors;
        #region IPropertyEditor Members

        /// <summary>
        /// Gets a control to edit the given property</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public Control GetEditingControl(PropertyEditorControlContext context)
        {
            NumericTupleControl editingControl =
                new NumericTupleControl(m_numericType, m_names, context);

            editingControl.Height = editingControl.Font.Height + 2;
            editingControl.ScaleFactor = m_scaleFactor;
            editingControl.HideAxisLabel = HideAxisLabel;
            editingControl.SetLabelBackColors(m_labelColors);
            SkinService.ApplyActiveSkin(editingControl);
            return editingControl;            
        }

        #endregion

        #region UITypeEditor Implementation

        /// <summary>
        /// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <returns>A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"></see> value that indicates the style of editor used 
        /// by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method. 
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
                Sce.Atf.Controls.NumericTupleControl control =
                    new Sce.Atf.Controls.NumericTupleControl(m_numericType, m_names);
                control.HideAxisLabel = HideAxisLabel;
                control.SetLabelBackColors(m_labelColors);
                if (value != null)
                    control.Value = value;

                editorService.DropDownControl(control);
                value = control.LastEdit; // kind of tricky way to ensure that we don't create a new object with the same value
            }

            return value;
        }

        #endregion

        private Type m_numericType;
        private string[] m_names;
        private double m_scaleFactor = 1.0;

        private class NumericTupleControl : Sce.Atf.Controls.NumericTupleControl, ICacheablePropertyControl
        {
            public NumericTupleControl(Type type, string[] names, PropertyEditorControlContext context)
                : base(type, names)
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

            private readonly PropertyEditorControlContext m_context;
            private bool m_refreshing;
        }     
    }
}
