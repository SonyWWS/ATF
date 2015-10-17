//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// Property editor that supplies NumericTextBox controls to embed in complex property
    /// editing controls. This class couples (1) an EditingControlContext that considers the
    /// property as an array (of arbitrary length) of identical numeric values of type T and
    /// (2) the Control that displays a single value of type T.</summary>
    /// <typeparam name="T">The numeric type of each array element that is to be edited as a
    /// single property. All .NET numeric types are supported, although the scaling factor
    /// in NumericEditor is not applied to integer types. See Scea.Atf.Controls.NumericTextBox
    /// for the list.</typeparam>
    public class UniformArrayEditor<T> : NumericEditor
    {
        /// <summary>
        /// Constructor</summary>
        public UniformArrayEditor()
            : base(typeof(T))
        {
        }

        

        #region IPropertyEditor Members

        /// <summary>
        /// Gets a control to edit the given property</summary>
        /// <param name="context">Context for property editing control</param>
        /// <returns>Control to edit the given context</returns>
        public override Control GetEditingControl(PropertyEditorControlContext context)
        {
            UniformArrayTextBox editingControl = new UniformArrayTextBox(context);
            editingControl.ScaleFactor = ScaleFactor;
            return editingControl;            
        }

        #endregion

        /// <summary>
        /// The base class is a control for editing a single numeric value of the given type.
        /// The editing context is assumed to be for an array of numeric values of the same type
        /// (or that can be converted to an array of values of the given type).</summary>
        protected class UniformArrayTextBox : Sce.Atf.Controls.NumericTextBox, ICacheablePropertyControl
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="context">PropertyEditorControlContext</param>
            public UniformArrayTextBox(PropertyEditorControlContext context)
                : base(typeof(T))
            {
                m_context = context;

                BorderStyle = BorderStyle.None;

                RefreshValue();
            }

            /// <summary>
            /// Raises the ValueEdited event after performing custom processing</summary>
            /// <param name="e">EventArgs that contains the event data</param>
            protected override void OnValueEdited(EventArgs e)
            {
                object arrayOfValues = SingleToArray(Value);
                m_context.SetValue(arrayOfValues);

                base.OnValueEdited(e);
            }

            /// <summary>
            /// Forces the control to invalidate its client area and immediately redraw itself and any child controls</summary>
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
            public virtual bool Cacheable
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
                    Value = ArrayToSingle(value);
                    Enabled = !m_context.IsReadOnly;
                }
            }

            private object SingleToArray(object single)
            {
                int arrayLength = ((IList)m_context.GetValue()).Count;
                T singleValue = (T)single;
                T[] newArray = new T[arrayLength];
                for (int i = 0; i < arrayLength; i++)
                    newArray[i] = singleValue;
                return newArray;
            }

            private object ArrayToSingle(object array)
            {
                return ((T[])array)[0];
            }

            private readonly PropertyEditorControlContext m_context;
        }
    }
}
