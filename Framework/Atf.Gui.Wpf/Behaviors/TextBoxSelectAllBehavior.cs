//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Behavior to select all text in a textbox when it receives focus</summary>
    public static class TextBoxSelectAllBehavior
    {
        /// <summary>
        /// Gets whether to select all text when the textbox receives focus.</summary>
        /// <param name="obj">DependencyObject to query for the property value</param>
        /// <returns>Value of the dependency property</returns>
        public static bool GetSelectAllOnFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(SelectAllOnFocusProperty);
        }

        /// <summary>
        /// Sets whether to select all text when the textbox receives focus</summary>
        /// <param name="obj">DependencyObject on which to set the property value</param>
        /// <param name="value">The value to set</param>
        public static void SetSelectAllOnFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        /// <summary>
        /// Dependency property that indicates whether to select all text when the textbox receives focus</summary>
        public static readonly DependencyProperty SelectAllOnFocusProperty =
            DependencyProperty.RegisterAttached("SelectAllOnFocus", typeof(bool), typeof(TextBoxSelectAllBehavior), new UIPropertyMetadata(false, OnSelectAllOnFocusPropertyChanged));

        private static void OnSelectAllOnFocusPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var tb = obj as TextBox;
            if(tb == null)
                throw new Exception("Invalid type for SelectAllOnFocus property");

            if ((bool)e.NewValue)
            {
                tb.PreviewMouseLeftButtonDown += tb_PreviewMouseLeftButtonDown;
                tb.GotKeyboardFocus += tb_GotKeyboardFocus;
            }
            else
            {
                tb.PreviewMouseLeftButtonDown -= tb_PreviewMouseLeftButtonDown;
                tb.GotKeyboardFocus -= tb_GotKeyboardFocus;
            }
        }

        private static void tb_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = sender as TextBox;
            tb.SelectAll();
        }

        private static void tb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBox;

            if (tb.IsFocused) 
                return;

            tb.SelectAll();
            Keyboard.Focus(tb);
            e.Handled = true;
        }
    }
}
