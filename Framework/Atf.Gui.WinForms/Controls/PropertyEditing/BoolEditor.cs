//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Boolean value property editor that supplies CheckBox controls to embed in complex
    /// property editing controls.</summary>
    public class BoolEditor : IPropertyEditor
    {
        #region IPropertyEditor Implementation

        /// <summary>
        /// Obtains a control to edit a given property. Changes to the selection set
        /// cause this method to be called again (and passed a new 'context'),
        /// unless ICacheablePropertyControl is implemented on the control. For
        /// performance reasons, it is highly recommended that the control implement
        /// the ICacheablePropertyControl interface.</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public Control GetEditingControl(PropertyEditorControlContext context)
        {
            m_boolControl = new BoolControl(context);
            return m_boolControl;
        }

        #endregion

        private class BoolControl : Control, ICacheablePropertyControl
        {
            public BoolControl(PropertyEditorControlContext context)
            {
                m_context = context;

                m_checkBox = new CheckBox();
                m_checkBox.Size = m_checkBox.PreferredSize;
                m_checkBox.CheckAlign = ContentAlignment.MiddleLeft;
                m_checkBox.CheckedChanged += checkBox_CheckedChanged;

                Controls.Add(m_checkBox);
                Height = m_checkBox.Height + m_topAndLeftMargin;

                RefreshValue();
            }

            public override void Refresh()
            {
                RefreshValue();

                base.Refresh();
            }

            #region ICacheablePropertyControl
            /// <summary>
            /// Gets true iff this control can be used indefinitely, regardless of whether the associated
            /// PropertyEditorControlContext's SelectedObjects property changes, i.e., the selection changes. 
            /// This property must be constant for the life of this control.</summary>
            public bool Cacheable
            {
                get { return true; }
            }
            #endregion

            protected override void OnSizeChanged(EventArgs e)
            {
                m_checkBox.Location = new Point(m_topAndLeftMargin, (Height - m_checkBox.Height) / 2 + 1); 
                base.OnSizeChanged(e);
            }

            private void checkBox_CheckedChanged(object sender, EventArgs e)
            {
                if (!m_refreshing)
                {
                    bool value = m_checkBox.Checked;
                    m_context.SetValue(value);
                }
            }

            private void RefreshValue()
            {
                try
                {
                    m_refreshing = true;
                    object value = m_context.GetValue();
                    if (value == null)
                        m_checkBox.Enabled = false;
                    else
                    {
                        m_checkBox.Checked = (bool)value;
                        m_checkBox.Enabled = (!m_context.IsReadOnly) && !DisableEditing;
                    }
                }
                finally
                {
                    m_refreshing = false;
                }
            }

            /// <summary>
            /// Gets or sets whether editing is disabled.
            /// DisableEditing can be used to lock out editing on this control, whether or not the context it was created with is read only.</summary>
            public bool DisableEditing { get; set; }

            private readonly PropertyEditorControlContext m_context;
            private readonly CheckBox m_checkBox;
            private bool m_refreshing;
            private const int m_topAndLeftMargin = 2;
        }

        /// <summary>
        /// Gets or sets whether editing is disabled. 
        /// DisableEditing can be used to lock out editing on this editor, whether or not the context it was created with is read only.</summary>
        public bool DisableEditing
        {
            get { return (m_boolControl != null) ? m_boolControl.DisableEditing : false; }
            set {  if (m_boolControl != null) m_boolControl.DisableEditing = value; }
        }

        private BoolControl m_boolControl;
    }
}
