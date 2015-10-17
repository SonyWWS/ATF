//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Sce.Atf.Wpf.ValueConverters
{
    /// <summary>
    /// Value converter that sets the item's visibility to Collapsed if the item is null</summary>
    public class NullVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Sets Visibility based on whether the item is null</summary>
        /// <param name="value">Value of the item</param>
        /// <param name="targetType">unused</param>
        /// <param name="parameter">unused</param>
        /// <param name="culture">unused</param>
        /// <returns>Visibility.Collapsed if the value is null, otherwise Visibility.Visible</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Not implemented</summary>
        /// <param name="value">unused</param>
        /// <param name="targetType">unused</param>
        /// <param name="parameter">unused</param>
        /// <param name="culture">unused</param>
        /// <returns>null</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Value converter that sets an item's visibility based on its boolean value</summary>
    public class BoolVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Sets Visibility based on whether the item is true or false</summary>
        /// <param name="value">Value of the item</param>
        /// <param name="targetType">unused</param>
        /// <param name="parameter">unused</param>
        /// <param name="culture">unused</param>
        /// <returns>Visibility.Visible if the value is true, otherwise Visibility.Collapsed if the value is false or null</returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                bool b = (bool)value;
                return b ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the item's value based on its Visibility</summary>
        /// <param name="value">Visibility value</param>
        /// <param name="targetType">unused</param>
        /// <param name="parameter">unused</param>
        /// <param name="culture">unused</param>
        /// <returns><c>True</c> if the visibility is Visible, otherwise false</returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                Visibility v = (Visibility)value;
                return v == Visibility.Visible ? true : false;
            }
            return false;
        }

        #endregion
    }
}
