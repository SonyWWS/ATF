//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Attached property replacement for the standard Button.IsDefault to work around a bug in WPF detailed here
    /// http://www.thomasclaudiushuber.com/blog/2008/05/02/lostfocus-textbox-vs-buttons-isdefault-property/
    /// </summary>
    /// <remarks>
    /// To use, add as a property of the button declaration in XAML:
    ///     <Button x:Name="button_OK" DockPanel.Dock="Right"
    ///             Content="OK" b:DialogButtonWorkaround.IsDefault="True" />
    /// </remarks>
    public static class DialogButtonWorkaround
    {
        public static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.RegisterAttached("IsDefault", typeof(bool), typeof(DialogButtonWorkaround), new PropertyMetadata(default(bool), OnIsDefaultPropertyChanged));

        public static void SetIsDefault(UIElement element, bool value)
        {
            element.SetValue(IsDefaultProperty, value);
        }

        public static bool GetIsDefault(UIElement element)
        {
            return (bool)element.GetValue(IsDefaultProperty);
        }

        private static void OnIsDefaultPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = d as Button;
            if(button == null)
                throw new InvalidOperationException("DialogButtonWorkaround.IsDefault attached property only allowed on Buttons");

            if((bool)e.NewValue)
            {
                button.Click += button_Click;
                button.IsDefault = true;
            }
            else
            {
                button.Click -= button_Click;
                button.IsDefault = false;
            }
        }

        private static void button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if(button != null && button.IsDefault)
            {
                var fwe = Keyboard.FocusedElement as FrameworkElement;
                if (fwe is TextBox)
                {
                    var expression = fwe.GetBindingExpression(TextBox.TextProperty);
                    if (expression != null)
                    {
                        expression.UpdateSource();
                    }
                }
            }
        }
    }
}
