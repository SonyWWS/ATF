//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Attached Property which allows a style that uses BasedOn="" to be re-based when the application
    /// theme is dynamically changed. Normally, if the BasedOn property of a style is set, once the application
    /// is loaded, changing the target style has no effect - the derived style remains based on the original style.
    /// This behavior works around this by cloning the style when a theme change occurs.</summary>
    public class ThemeStyleBehavior
    {
        #region AutoMergeStyle

        /// <summary>
        /// Whether to auto merge style dependency property</summary>
        public static readonly DependencyProperty AutoMergeStyleProperty =
            DependencyProperty.RegisterAttached("AutoMergeStyle", typeof(bool), typeof(ThemeStyleBehavior),
                new FrameworkPropertyMetadata(false, OnAutoMergeStyleChanged));

        /// <summary>
        /// Get whether to auto merge style dependency property</summary>
        /// <param name="d">DependencyObject to query</param>
        /// <returns>Value of whether to auto merge style dependency property</returns>
        public static bool GetAutoMergeStyle(DependencyObject d)
        {
            return (bool)d.GetValue(AutoMergeStyleProperty);
        }

        /// <summary>
        /// Set whether to auto merge style dependency property</summary>
        /// <param name="d">DependencyObject to set</param>
        /// <param name="value">Value to set</param>
        public static void SetAutoMergeStyle(DependencyObject d, bool value)
        {
            d.SetValue(AutoMergeStyleProperty, value);
        }

        private static void OnAutoMergeStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
            {
                return;
            }

            var control = d as FrameworkElement;
            if (control == null)
            {
                throw new NotSupportedException("AutoMergeStyle can only used in FrameworkElement");
            }

            if ((bool)e.NewValue)
            {
                Type type = d.GetType();
                control.SetResourceReference(BaseOnStyleProperty, type);
            }
            else
            {
                control.ClearValue(BaseOnStyleProperty);
            }
        }

        #endregion

        #region BaseOnStyle

        /// <summary>
        /// Based on style dependency property</summary>
        public static readonly DependencyProperty BaseOnStyleProperty =
            DependencyProperty.RegisterAttached("BaseOnStyle", typeof(Style), typeof(ThemeStyleBehavior),
                new FrameworkPropertyMetadata(null, OnBaseOnStyleChanged));

        /// <summary>
        /// Get value of based on style dependency property</summary>
        /// <param name="d">DependencyObject to query</param>
        /// <returns>Value of based on style dependency property</returns>
        public static Style GetBaseOnStyle(DependencyObject d)
        {
            return (Style)d.GetValue(BaseOnStyleProperty);
        }

        /// <summary>
        /// Set value of based on style dependency property</summary>
        /// <param name="d">DependencyObject to set</param>
        /// <param name="value">Value to set</param>
        public static void SetBaseOnStyle(DependencyObject d, Style value)
        {
            d.SetValue(BaseOnStyleProperty, value);
        }

        private static void OnBaseOnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == e.NewValue)
                return;

            var control = d as FrameworkElement;
            if (control == null)
                throw new NotSupportedException("BaseOnStyle can only used in FrameworkElement");

            var baseOnStyle = e.NewValue as Style;
            var originalStyle = GetOriginalStyle(control);
            if (originalStyle == null)
            {
                originalStyle = control.Style;
                SetOriginalStyle(control, originalStyle);
            }

            var newStyle = originalStyle;
            if (originalStyle.IsSealed)
            {
                newStyle = new Style();
                newStyle.TargetType = originalStyle.TargetType;

                //1. Copy resources, setters, triggers
                newStyle.Resources = originalStyle.Resources;
                foreach (var st in originalStyle.Setters)
                {
                    newStyle.Setters.Add(st);
                }
                foreach (var tg in originalStyle.Triggers)
                {
                    newStyle.Triggers.Add(tg);
                }

                //2. Set BaseOn Style
                newStyle.BasedOn = baseOnStyle;
            }
            else
            {
                originalStyle.BasedOn = baseOnStyle;
            }

            //control.SetResourceReference(Control.StyleProperty, Resources.ToolBarStyleKey);
            control.Style = newStyle;
        }

        #endregion

        #region OriginalStyle

        /// <summary>
        /// Original style dependency property</summary>
        public static readonly DependencyProperty OriginalStyleProperty =
            DependencyProperty.RegisterAttached("OriginalStyle", typeof(Style), typeof(ThemeStyleBehavior),
                new FrameworkPropertyMetadata((Style)null));

        /// <summary>
        /// Get original style dependency property value</summary>
        /// <param name="d">DependencyObject to query</param>
        /// <returns>Dependency property value</returns>
        public static Style GetOriginalStyle(DependencyObject d)
        {
            return (Style)d.GetValue(OriginalStyleProperty);
        }

        /// <summary>
        /// Set original style dependency property value</summary>
        /// <param name="d">DependencyObject to set</param>
        /// <param name="value">Value to set</param>
        public static void SetOriginalStyle(DependencyObject d, Style value)
        {
            d.SetValue(OriginalStyleProperty, value);
        }

        #endregion
    }
}
