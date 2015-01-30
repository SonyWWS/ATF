//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Windows.Data;

using Sce.Atf.Adaptation;
using Sce.Atf.Collections;

namespace Sce.Atf.Wpf.ValueConverters
{
    /// <summary>
    /// Value converter to convert backwards and forwards between two types
    /// using the IAdaptable interface.</summary>
    /// <typeparam name="T">Original type</typeparam>
    /// <typeparam name="U">Adapted type</typeparam>
    public class AdaptingValueConverter<T,U> : IValueConverter
        where T : class
        where U : class
    {
        #region IValueConverter Members

        /// <summary>
        /// Attempt to convert a value to a target type</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value or null if no conversion done</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.As<U>();
        }

        /// <summary>
        /// Convert back value</summary>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Value converted back or null if no conversion done</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.As<T>();
        }

        #endregion
    }

    /// <summary>
    /// Value converter to convert one-way to an adapted type using IAdaptable</summary>
    /// <typeparam name="U">Adapted type</typeparam>
    public class SimpleAdaptingValueConverter<U> : IValueConverter
        where U : class
    {
        #region IValueConverter Members

        /// <summary>
        /// Attempt to convert a value to a target type</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value or null if no conversion done</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.As<U>();
        }

        /// <summary>
        /// Convert back value</summary>
        /// <exception cref="NotSupportedException"> is thrown</exception>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Value converted back or null if no conversion done</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    /// <summary>
    /// Value converter to convert an IObservableCollection of one type to an IObservableCollection
    /// of another type using the IAdaptable interface.</summary>
    /// <typeparam name="T">Source Type</typeparam>
    /// <typeparam name="U">Adapted type</typeparam>
    public class AdaptingCollectionValueConverter<T, U> : IValueConverter
        where T : class
        where U : class
    {
        #region IValueConverter Members

        /// <summary>
        /// Attempt to convert IObservableCollection of one type to an IObservableCollection
        /// of another type using the IAdaptable interface.</summary>
        /// <param name="value">Object to convert</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Converted value or null if no conversion done</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var collection = value as IObservableCollection<T>;
            if (collection != null)
            {
                return new AdaptableObservableCollection<T, U>(collection);
            }
            
            return System.Windows.DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// Convert back value</summary>
        /// <exception cref="NotSupportedException"> is thrown</exception>
        /// <param name="value">Value to convert back</param>
        /// <param name="targetType">Type of target (unused)</param>
        /// <param name="parameter">Converter parameter to use (unused)</param>
        /// <param name="culture">Culture to use in the converter (unused)</param>
        /// <returns>Value converted back or null if no conversion done</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
   
}
