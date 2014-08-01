//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Enum specifying the type of mask to use in a numeric textbox</summary>
    public enum MaskType
    {
        /// <summary>
        /// Allow any numeric type</summary>
        Any,

        /// <summary>
        /// Allow integers</summary>
        Integer,

        /// <summary>
        /// Allow decimals</summary>
        Decimal
    }  

    /// <summary>
    /// Define behaviors for numeric entry in a TextBox, including minimum and maximum 
    /// value and whether to allow integer or decimal numbers.</summary>
    public static class TextBoxNumericBehavior
    {
        /// <summary>
        /// Gets the value of the MinimumValueProperty dependency property</summary>
        /// <param name="obj">Dependency object to query for the value</param>
        /// <returns>Value of the property</returns>
        public static double GetMinimumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MinimumValueProperty);
        }

        /// <summary>
        /// Sets the value of the MaximumValueProperty dependency property</summary>
        /// <param name="obj">Dependency object to query for the value</param>
        /// <param name="value">Value to set</param>
        public static void SetMinimumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MinimumValueProperty, value);
        }

        /// <summary>
        /// Minimum value dependency property</summary>
        public static readonly DependencyProperty MinimumValueProperty =
            DependencyProperty.RegisterAttached(
                "MinimumValue",
                typeof(double),
                typeof(TextBoxNumericBehavior),
                new FrameworkPropertyMetadata(double.NaN, MinimumValueChangedCallback)
                );

        /// <summary>
        /// Gets the value of the MaximumValueProperty dependency property</summary>
        /// <param name="obj">Dependency object to query for the value</param>
        /// <returns>Value of the property</returns>
        public static double GetMaximumValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MaximumValueProperty);
        }

        /// <summary>
        /// Sets the value of the MaximumValueProperty dependency property</summary>
        /// <param name="obj">Dependency object to query for the value</param>
        /// <param name="value">Value to set</param>
        public static void SetMaximumValue(DependencyObject obj, double value)
        {
            obj.SetValue(MaximumValueProperty, value);
        }

        /// <summary>
        /// Maximum value dependency property</summary>
        public static readonly DependencyProperty MaximumValueProperty =
            DependencyProperty.RegisterAttached(
                "MaximumValue",
                typeof(double),
                typeof(TextBoxNumericBehavior),
                new FrameworkPropertyMetadata(double.NaN, MaximumValueChangedCallback)
                );

        /// <summary>
        /// Gets the value of the MaskProperty dependency property</summary>
        /// <param name="obj">Dependency object to query for the value</param>
        /// <returns>Value of the property</returns>
        public static MaskType GetMask(DependencyObject obj)
        {
            return (MaskType)obj.GetValue(MaskProperty);
        }

        /// <summary>
        /// Sets the value of the MaskProperty dependency property</summary>
        /// <param name="obj">Dependency object to query for the value</param>
        /// <param name="value">Value to set</param>
        public static void SetMask(DependencyObject obj, MaskType value)
        {
            obj.SetValue(MaskProperty, value);
        }

        /// <summary>
        /// Mask dependency property</summary>
        public static readonly DependencyProperty MaskProperty =
            DependencyProperty.RegisterAttached(
                "Mask",
                typeof(MaskType),
                typeof(TextBoxNumericBehavior),
                new FrameworkPropertyMetadata(MaskChangedCallback)
                );

        private static void MinimumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as TextBox;
            ValidateTextBox(tb);
        }

        private static void MaximumValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as TextBox;
            ValidateTextBox(tb);
        }


        private static void MaskChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tb = d as TextBox;

            if (e.NewValue == null || (MaskType)e.NewValue == MaskType.Any)
            {
                tb.PreviewTextInput -= TextBox_PreviewTextInput;
                DataObject.RemovePastingHandler(tb, TextBoxPastingEventHandler);
            }
            else
            {
                tb.PreviewTextInput += TextBox_PreviewTextInput;
                DataObject.AddPastingHandler(tb, TextBoxPastingEventHandler);
            }

            ValidateTextBox(tb);
        }

        private static void ValidateTextBox(TextBox tb)
        {
            MaskType mask = GetMask(tb);
            if (mask != MaskType.Any)
            {
                tb.Text = ValidateValue(mask, tb.Text, GetMinimumValue(tb), GetMaximumValue(tb));
            }
        }

        private static void TextBoxPastingEventHandler(object sender, DataObjectPastingEventArgs e)
        {
            var tb = sender as TextBox;
            string clipboard = e.DataObject.GetData(typeof(string)) as string;
            clipboard = ValidateValue(GetMask(tb), clipboard, GetMinimumValue(tb), GetMaximumValue(tb));
            if (!string.IsNullOrEmpty(clipboard))
            {
                tb.Text = clipboard;
            }
            e.CancelCommand();
            e.Handled = true;
        }

        private static void TextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var tb = sender as TextBox;
            bool isValid = IsSymbolValid(GetMask(tb), e.Text);
            e.Handled = !isValid;
            if (isValid)
            {
                int caret = tb.CaretIndex;
                string text = tb.Text;
                bool textInserted = false;
                int selectionLength = 0;

                if (tb.SelectionLength > 0)
                {
                    text = text.Substring(0, tb.SelectionStart) +
                            text.Substring(tb.SelectionStart + tb.SelectionLength);
                    caret = tb.SelectionStart;
                }

                if (e.Text == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                {
                    while (true)
                    {
                        int ind = text.IndexOf(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator);
                        if (ind == -1)
                            break;

                        text = text.Substring(0, ind) + text.Substring(ind + 1);
                        if (caret > ind)
                            caret--;
                    }

                    if (caret == 0)
                    {
                        text = "0" + text;
                        caret++;
                    }
                    else
                    {
                        if (caret == 1 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign)
                        {
                            text = NumberFormatInfo.CurrentInfo.NegativeSign + "0" + text.Substring(1);
                            caret++;
                        }
                    }

                    if (caret == text.Length)
                    {
                        selectionLength = 1;
                        textInserted = true;
                        text = text + NumberFormatInfo.CurrentInfo.NumberDecimalSeparator + "0";
                        caret++;
                    }
                }
                else if (e.Text == NumberFormatInfo.CurrentInfo.NegativeSign)
                {
                    textInserted = true;
                    if (tb.Text.Contains(NumberFormatInfo.CurrentInfo.NegativeSign))
                    {
                        text = text.Replace(NumberFormatInfo.CurrentInfo.NegativeSign, string.Empty);
                        if (caret != 0)
                            caret--;
                    }
                    else
                    {
                        text = NumberFormatInfo.CurrentInfo.NegativeSign + tb.Text;
                        caret++;
                    }
                }

                if (!textInserted)
                {
                    text = text.Substring(0, caret) + e.Text +
                        ((caret < tb.Text.Length) ? text.Substring(caret) : string.Empty);

                    caret++;
                }

                try
                {
                    double val = Convert.ToDouble(text);
                    double newVal = ValidateLimits(GetMinimumValue(tb), GetMaximumValue(tb), val);
                    if (val != newVal)
                    {
                        text = newVal.ToString();
                    }
                    else if (val == 0)
                    {
                        if (!text.Contains(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator))
                            text = "0";
                    }
                }
                catch
                {
                    text = "0";
                }

                while (text.Length > 1 && text[0] == '0' && string.Empty + text[1] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                {
                    text = text.Substring(1);
                    if (caret > 0)
                        caret--;
                }

                while (text.Length > 2 && string.Empty + text[0] == NumberFormatInfo.CurrentInfo.NegativeSign && text[1] == '0' && string.Empty + text[2] != NumberFormatInfo.CurrentInfo.NumberDecimalSeparator)
                {
                    text = NumberFormatInfo.CurrentInfo.NegativeSign + text.Substring(2);
                    if (caret > 1)
                        caret--;
                }

                if (caret > text.Length)
                    caret = text.Length;

                tb.Text = text;
                tb.CaretIndex = caret;
                tb.SelectionStart = caret;
                tb.SelectionLength = selectionLength;
                e.Handled = true;
            }
        }

        private static string ValidateValue(MaskType mask, string value, double min, double max)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            value = value.Trim();
            switch (mask)
            {
                case MaskType.Integer:
                    try
                    {
                        Convert.ToInt64(value);
                        return value;
                    }
                    catch
                    {
                    }
                    return string.Empty;

                case MaskType.Decimal:
                    try
                    {
                        Convert.ToDouble(value);
                        return value;
                    }
                    catch
                    {
                    }
                    return string.Empty;
            }

            return value;
        }

        private static double ValidateLimits(double min, double max, double value)
        {
            if (!min.Equals(double.NaN))
            {
                if (value < min)
                    return min;
            }

            if (!max.Equals(double.NaN))
            {
                if (value > max)
                    return max;
            }

            return value;
        }

        private static bool IsSymbolValid(MaskType mask, string str)
        {
            switch (mask)
            {
                case MaskType.Any:
                    return true;

                case MaskType.Integer:
                    if (str == NumberFormatInfo.CurrentInfo.NegativeSign)
                        return true;
                    break;

                case MaskType.Decimal:
                    if (str == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator ||
                        str == NumberFormatInfo.CurrentInfo.NegativeSign)
                        return true;
                    break;
            }

            if (mask.Equals(MaskType.Integer) || mask.Equals(MaskType.Decimal))
            {
                foreach (char ch in str)
                {
                    if (!Char.IsDigit(ch))
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}
