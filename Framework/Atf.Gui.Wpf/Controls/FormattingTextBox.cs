//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// A TextBox with a bindable StringFormat property</summary>
    public class FormattingTextBox : TextBox
    {
        #region public IFormatProvider FormatProvider

        /// <summary>
        /// Gets or sets IFormatProvider that controls formatting</summary>
        public IFormatProvider FormatProvider
        {
            get { return (IFormatProvider)GetValue(FormatProviderProperty); }
            set { SetValue(FormatProviderProperty, value); }
        }

        /// <summary>
        /// Format provider dependency property</summary>
        public static readonly DependencyProperty FormatProviderProperty =
            DependencyProperty.Register("FormatProvider", 
                typeof(IFormatProvider), typeof(FormattingTextBox), new UIPropertyMetadata(CultureInfo.InvariantCulture));

        #endregion

        #region public string StringFormat

        /// <summary>
        /// Gets or sets formatting string</summary>
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        /// <summary>
        /// Formatting string dependency property</summary>
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", 
                typeof(string), typeof(FormattingTextBox), new UIPropertyMetadata(null, StringFormatChanged));

        #endregion

        #region public object Value

        /// <summary>
        /// Gets or sets text in TextBox</summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Text in TextBox dependency property</summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", 
                typeof(object), typeof(FormattingTextBox),
                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValueChangedCallback));

        #endregion

        #region Ctors

        /// <summary>
        /// Constructor</summary>
        static FormattingTextBox()
        {
            TextProperty.OverrideMetadata(typeof(FormattingTextBox), new FrameworkPropertyMetadata(TextChangedCallback));
        }

        #endregion

        #region Events

        private static void StringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((FormattingTextBox)d).UpdateText();
        }

        #endregion

        #region Private Methods

        private void UpdateText()
        {
            userIsChanging = false;

            string f = StringFormat;
            if (f != null)
            {
                if (!f.Contains("{0"))
                    f = string.Format("{{0:{0}}}", f);
                Text = String.Format(FormatProvider, f, Value);
            }
            else
            {
                Text = Value != null ? String.Format(FormatProvider, "{0}", Value) : null;
            }
            userIsChanging = true;
        }

        private string UnFormat(string s)
        {
            if (StringFormat == null)
                return s;
            var r = new Regex(@"(.*)(\{0.*\})(.*)");
            var match = r.Match(StringFormat);
            if (!match.Success)
                return s;
            if (match.Groups.Count > 1 && !String.IsNullOrEmpty(match.Groups[1].Value))
                s = s.Replace(match.Groups[1].Value, "");
            if (match.Groups.Count > 3 && !String.IsNullOrEmpty(match.Groups[3].Value))
                s = s.Replace(match.Groups[3].Value, "");
            return s;
        }

        private void UpdateValue()
        {
            if (userIsChanging)
                Value = UnFormat(Text);
        }

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ftb = (FormattingTextBox)d;
            if (ftb.userIsChanging)
                ftb.UpdateText();
        }

        private static void TextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ftb = (FormattingTextBox)d;
            ftb.UpdateValue();
        }

        private bool userIsChanging = true;

        #endregion
    }
}
