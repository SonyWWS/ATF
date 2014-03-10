//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Sce.Atf.Wpf.Markup
{
    /// <summary>
    /// XAML markup extension implementations that can be supported by .NET Framework XAML Services and its XAML readers and XAML writers.
    /// Provides support for multibinding, in which several bindings are provided. This allows a variety of conversions for data.</summary>
    /// <typeparam name="T">Converter type</typeparam>
    /// <remarks>See http://msdn.microsoft.com/en-us/library/system.windows.markup.markupextension.aspx and 
    /// http://msdn.microsoft.com/en-us/library/system.windows.data.imultivalueconverter.aspx . </remarks>
    public abstract class MultiConverterMarkupExtension<T> : MarkupExtension, IMultiValueConverter
       where T : class, IMultiValueConverter, new()
    {
        private static T s_converter = null;

        /// <summary>
        /// Returns an object that is set as the value of the target property for this markup extension</summary>
        /// <param name="serviceProvider">An object that can provide services for the markup extension</param>
        /// <returns>The object that corresponds to the specified resource key</returns>
        /// <remarks>See http://msdn.microsoft.com/en-us/library/system.windows.markup.markupextension.providevalue.aspx. </remarks>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return s_converter ?? (s_converter = new T());
        }

        #region IMultiValueConverter Members

        /// <summary>
        /// Converts source values to a value for the binding target. The data binding engine calls this method
        /// when it propagates the values from source bindings to the binding target.</summary>
        /// <param name="values">The array of values that the source bindings in the System.Windows.Data.MultiBinding 
        /// produces. The value System.Windows.DependencyProperty.UnsetValue indicates 
        /// that the source binding has no value to provide for conversion.</param>
        /// <param name="targetType">The type of the binding target property</param>
        /// <param name="parameter">The converter parameter to use</param>
        /// <param name="culture">The culture to use in the converter</param>
        /// <returns>A converted value. If the method returns null, the null value is valid and is used. 
        /// A return value of System.Windows.DependencyProperty.System.Windows.DependencyProperty.UnsetValue 
        /// indicates that the converter did not produce a value, and that the binding 
        /// will use the System.Windows.Data.BindingBase.FallbackValue if it is available, 
        /// or else will use the default value. A return value of System.Windows.Data.Binding.System.Windows.Data.Binding.DoNothing 
        /// indicates that the binding does not transfer the value or use the System.Windows.Data.BindingBase.FallbackValue or the default value.</returns>
        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Converts a binding target value to the source binding values</summary>
        /// <param name="value">The value that the binding target produces</param>
        /// <param name="targetTypes">The array of types to convert to. The array length indicates the number and 
        /// types of values that are suggested for the method to return.</param>
        /// <param name="parameter">The converter parameter to use</param>
        /// <param name="culture">The culture to use in the converter</param>
        /// <returns>An array of values that have been converted from the target value back to the source values</returns>
        public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
