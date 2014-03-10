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
    /// in the schema annotation. Otherwise, you can write your own custom TypeConverter.</remarks>
    public class ColorPickerEditor : Sce.Atf.Controls.ColorPickerEditor, IAnnotatedParams
    {
        #region IAnnotatedParams Members

        /// <summary>
        /// Initializes the control</summary>
        /// <param name="parameters">Editor parameters</param>
        public void Initialize(string[] parameters)
        {
            if (parameters.Length >= 1)
            {
                EnableAlpha = Boolean.Parse(parameters[0]);
            }
        }

        #endregion

        /// <summary>
        /// Edits the value</summary>
        /// <param name="context">The type descriptorcontext</param>
        /// <param name="provider">The service provider</param>
        /// <param name="value">The value</param>
        /// <returns>The new value</returns>
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            TypeConverter converter = context.PropertyDescriptor.Converter;
            Color oldColor = (Color) converter.ConvertTo(value, typeof(Color)); // convert from underlying type to Color
            Color newColor = (Color) base.EditValue(context, provider, oldColor);
            return converter.ConvertFrom(newColor);
        }

        /// <summary>
        /// Performs custom actions on PaintValue events. Paints a representative value of the given object to the provided canvas</summary>
        /// <param name="e">PaintValueEventArgs indicating what to paint and where to paint it</param>
        public override void PaintValue(PaintValueEventArgs e)
        {
            TypeConverter converter = e.Context.PropertyDescriptor.Converter;
            object value = converter.ConvertTo(e.Value, typeof(Color)); // convert from underlying type to Color
            if (value != null && value is Color)
            {
                Color color = (Color)value;
                base.PaintValue(new PaintValueEventArgs(e.Context, color, e.Graphics, e.Bounds));
            }
        }
    }
}

