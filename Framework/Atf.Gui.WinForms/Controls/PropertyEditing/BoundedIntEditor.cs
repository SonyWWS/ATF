//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Bounded int property editor that supplies IntInputControl instances to embed in complex
    /// property editing controls. These display a slider and textbox in the GUI.</summary>
    public class BoundedIntEditor : UITypeEditor, IPropertyEditor, IAnnotatedParams
    {

        /// <summary>
        /// Default constructor</summary>
        public BoundedIntEditor()
        {
            Min = 0;
            Max = 100;
        }

        /// <summary>
        /// Constructs BoundedIntEditor using the given arguments</summary>
        /// <param name="min">Min value</param>
        /// <param name="max">Max value</param>
        public BoundedIntEditor(int min, int max)
        {
            if (min >= max)
                throw new ArgumentOutOfRangeException("min must be less than max");
            Min = min;
            Max = max;

        }
        
        /// <summary>
        /// Gets or sets the editor's minimum value</summary>
        public int Min
        {
            get { return m_min; }
            set { m_min = value; }
        }

        /// <summary>
        /// Gets or sets the editor's maximum value</summary>
        public int Max
        {
            get { return m_max; }
            set { m_max = value; }
        }

       
        #region IPropertyEditor Members

        /// <summary>
        /// Obtains a control to edit a given property. Changes to the selection set
        /// cause this method to be called again (and passed a new 'context'),
        /// unless ICacheablePropertyControl is implemented on the control. For
        /// performance reasons, it is highly recommended that the control implement
        /// the ICacheablePropertyControl interface.</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public virtual Control GetEditingControl(PropertyEditorControlContext context)
        { 
            var control = new BoundedIntControl(context, m_min, m_max);          
            SkinService.ApplyActiveSkin(control);
            return control;
        }

        #endregion

        #region UITypeEditor Implementation

        /// <summary>
        /// Gets the editor style used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method</summary>
        /// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"></see> that can be used to gain additional context information</param>
        /// <returns>A <see cref="T:System.Drawing.Design.UITypeEditorEditStyle"></see> value that indicates the style of editor
        /// used by the <see cref="M:System.Drawing.Design.UITypeEditor.EditValue(System.IServiceProvider,System.Object)"></see> method. 
        /// If the <see cref="T:System.Drawing.Design.UITypeEditor"></see> does not support this method, 
        /// <see cref="M:System.Drawing.Design.UITypeEditor.GetEditStyle"></see> returns <see cref="F:System.Drawing.Design.UITypeEditorEditStyle.None"></see>.</returns>
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
                if (!(value is int))
                    value = m_min;

                IntInputControl intInputControl = new IntInputControl((int)value, m_min, m_max);
                editorService.DropDownControl(intInputControl);
                // be careful to return the same object if the value didn't change
                if (!intInputControl.Value.Equals(value))
                    value = intInputControl.Value;
            }

            return value;
        }

        #endregion

        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
            int min;
            int max;
            if (parameters.Length < 2 ||
                !Int32.TryParse(parameters[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out min) ||
                !Int32.TryParse(parameters[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out max) ||
                min >= max)
            {
                throw new ArgumentException("Can't parse bounds for BoundedIntEditor");
            }

            Min = min;
            Max = max;
        }

        #endregion


        /// <summary>
        /// Control for editing bounded int</summary>
        protected class BoundedIntControl : Sce.Atf.Controls.IntInputControl, ICacheablePropertyControl
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="context">Context for property editing control</param>
            /// <param name="min">Minimum value</param>
            /// <param name="max">Maximum value</param>
            public BoundedIntControl(PropertyEditorControlContext context, int min, int max)

                : base(min, min, max)
            {
                m_context = context;

                DrawBorder = false;
                DoubleBuffered = true;                
                RefreshValue();
            }

            #region ICacheablePropertyControl

            /// <summary>
            /// Gets <c>True</c> if this control can be used indefinitely, regardless of whether the associated
            /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
            /// This property must be constant for the life of this control.</summary>
            public virtual bool Cacheable
            {
                get { return true; }
            }

            #endregion

            /// <summary>
            /// Raises the ValueChanged event</summary>
            /// <param name="e">Event args</param>
            protected override void OnValueChanged(EventArgs e)
            {
                if (!m_refreshing)
                {
                    m_context.SetValue(Value);
                }

                base.OnValueChanged(e);
            }

            /// <summary>
            /// Refreshes the control</summary>
            public override void Refresh()
            {
                RefreshValue();

                base.Refresh();
            }

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
                        Value = (int)value;
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

        private int m_min;
        private int m_max = 100;
    }
}
