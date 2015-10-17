//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Property editor that supplies NumericTextBox controls to embed in
    /// complex property editing controls</summary>
    public class NumericEditor : IPropertyEditor
    {
        /// <summary>
        /// Constructor</summary>
        public NumericEditor()
            : this(typeof(Single))
        {
        }

        /// <summary>
        /// Constructor specifying numeric type</summary>
        /// <param name="numericType">Numeric type</param>
        public NumericEditor(Type numericType)
        {
            m_numericType = numericType;
        }

        /// <summary>
        /// Gets or sets the numeric type of the editor</summary>
        public Type NumericType
        {
            get { return m_numericType; }
            set { m_numericType = value; }
        }

        /// <summary>
        /// Gets or sets the scale factor, which is used to scale the value for presentation to the
        /// user. The inverse factor is used to scale the user entered value back. Only applies to
        /// floating point types. The integer types are not scaled.</summary>
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
        public virtual Control GetEditingControl(PropertyEditorControlContext context)
        {
            NumericTextBox editingControl = new NumericTextBox(m_numericType, context);
            editingControl.ScaleFactor = m_scaleFactor;
            SkinService.ApplyActiveSkin(editingControl);
            return editingControl;            
        }

        #endregion

        private Type m_numericType;
        private double m_scaleFactor = 1.0;

        private class NumericTextBox : Sce.Atf.Controls.NumericTextBox, ICacheablePropertyControl
        {
            public NumericTextBox(Type numericType, PropertyEditorControlContext context)
                : base(numericType)
            {
                m_context = context;

                BorderStyle = BorderStyle.None;               
                RefreshValue();               
            }

            protected override void OnValueEdited(EventArgs e)
            {
                m_context.SetValue(Value);

                base.OnValueEdited(e);
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
                object value = m_context.GetValue();
                if (value == null)
                    Enabled = false;
                else
                {
                    Value = value;
                    Enabled = !m_context.IsReadOnly;
                }
            }

            private readonly PropertyEditorControlContext m_context;
        }
    }
}
