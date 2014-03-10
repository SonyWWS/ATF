//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace Sce.Atf.Controls.PropertyEditing
{
    /// <summary>
    /// UITypeEditor class for editing date and time values</summary>
    /// <remarks>
    /// The default TypeConverter works if the date is stored in the DOM as a DateTime
    /// (e.g., if it is specified as xs:dateTime in the schema).  Other representations
    /// (string, integer number of seconds since some starting date, etc) require
    /// a custom TypeConverter specified in the schema annotation.</remarks>
    public class DateTimeEditor : System.ComponentModel.Design.DateTimeEditor
    {
        /// <summary>
        /// Edits the specified object value using the editor style provided by GetEditorStyle.
        /// A service provider is provided so that any required editing services can be obtained.</summary>
        /// <param name="context">A type descriptor context that can be used to provide additional context information</param>
        /// <param name="provider">A service provider object through which editing services may be obtained</param>
        /// <param name="value">An instance of the value being edited</param>
        /// <returns>The new value of the object. If the value of the object hasn't changed,
        /// this should return the same object it was passed.</returns>
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            TypeConverter converter = context.PropertyDescriptor.Converter;
            DateTime oldDate = GetDateTime(value, converter);
            DateTime newDate = (DateTime) base.EditValue(context, provider, oldDate);
            return GetValue(newDate, value.GetType(), converter);
        }

        /// <summary>
        /// Paints a representation of the value of an object using the specified <see cref="T:System.Drawing.Design.PaintValueEventArgs"></see></summary>
        /// <param name="e">A <see cref="T:System.Drawing.Design.PaintValueEventArgs"></see> that indicates what to paint and where to paint it</param>
        public override void PaintValue(PaintValueEventArgs e)
        {
            DateTime date = GetDateTime(e.Value, e.Context.PropertyDescriptor.Converter);
            base.PaintValue(new PaintValueEventArgs(e.Context, date, e.Graphics, e.Bounds));
        }

        private DateTime GetDateTime(object value, TypeConverter converter)
        {
            if (typeof(DateTime).IsAssignableFrom(value.GetType()))
                return (DateTime) value;
            else
                return (DateTime) converter.ConvertTo(value, typeof(DateTime));
        }

        private object GetValue(DateTime dateTime, Type type, TypeConverter converter)
        {
            if (type.IsAssignableFrom(typeof(DateTime)))
                return dateTime;
            else
                return converter.ConvertFrom(dateTime);
        }

    }
}


