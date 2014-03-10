//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// XAML markup extension implementations that can be supported by .NET Framework XAML Services and its XAML readers and XAML writers</summary>
    /// <typeparam name="T">Converter type</typeparam>
    /// <remarks>See http://msdn.microsoft.com/en-us/library/system.windows.markup.markupextension.aspx. </remarks>
    public abstract class ConverterMarkupExtension<T> : MarkupExtension, IValueConverter
         where T : class, IValueConverter, new()
    {
        private static T s_converter = null;

        /// <summary>
        /// Returns an object that is set as the value of the target property for this markup extension</summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension</param>
        /// <returns>Object value to set on the property where the extension is applied</returns>
        /// <remarks>See http://msdn.microsoft.com/en-us/library/system.windows.markup.markupextension.providevalue.aspx. </remarks>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return s_converter ?? (s_converter = new T());
        }

        #region IValueConverter Members

        /// <summary>
        /// Converts a value</summary>
        /// <param name="value">Value produced by the binding source</param>
        /// <param name="targetType">Type of the binding target property</param>
        /// <param name="parameter">Converter parameter to use</param>
        /// <param name="culture">Culture to use in the converter</param>
        /// <returns>Converted value. If the method returns null, the valid null value is used.</returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Converts back a value</summary>
        /// <param name="value">Value that is produced by the binding target</param>
        /// <param name="targetType">Type to convert to</param>
        /// <param name="parameter">Converter parameter to use</param>
        /// <param name="culture">Culture to use in the converter</param>
        /// <returns>Converted value from the target value back to the source value. If the method returns null, the valid null value is used.</returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
