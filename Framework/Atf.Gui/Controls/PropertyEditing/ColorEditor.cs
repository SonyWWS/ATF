//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// UITypeEditor class for editing colors</summary>
    /// <remarks>
    /// If the color is stored as an ARGB int, you can specify the Sce.Atf.ColorConverter
    /// in the schema annotation. Otherwise, you can write your own custom TypeConverter.
    /// You need to write custom code in the schema loader to process the annotation.</remarks>
    public class ColorEditor : System.Drawing.Design.ColorEditor
    {
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
            TypeConverter converter = context.PropertyDescriptor.Converter;
            if (value == null)
                value = Color.Transparent;
            Color oldColor = (Color)converter.ConvertTo(value, typeof(Color)); // convert from underlying type to Color
            Color newColor = (Color)base.EditValue(context, provider, oldColor);

            return converter.ConvertFrom(newColor);
        }

        /// <summary>
        /// Paints a representative value of the given object to the provided canvas</summary>
        /// <param name="e">What to paint and where to paint it</param>
        public override void PaintValue(PaintValueEventArgs e)
        {
            if (e.Value is Color)
            {
                base.PaintValue(e);
            }
            else
            {
                TypeConverter converter = e.Context.PropertyDescriptor.Converter;
                object value = converter.ConvertTo(e.Value, typeof(Color)); // convert from underlying type to Color
                if (value is Color)
                {
                    Color color = (Color)value;
                    base.PaintValue(new PaintValueEventArgs(e.Context, color, e.Graphics, e.Bounds));
                }
            }
        }

    }
}

