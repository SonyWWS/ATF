//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Attached Property which allows a style which uses BasedOn="" to be re-based when the application
    /// theme is dynamically changed.  Normally if the BasedOn property of a style is set, once the app
    /// is loaded, changing the target style will have no effect - the derived style will remain based to
    /// the original style.
    /// This behavior works around this by cloning the style when a theme change occurs
    /// </summary>
    public class ThemeStyleBehavior
    {
        #region AutoMergeStyle

        public static readonly DependencyProperty AutoMergeStyleProperty =
            DependencyProperty.RegisterAttached("AutoMergeStyle", typeof(bool), typeof(ThemeStyleBehavior),
                new FrameworkPropertyMetadata(false, OnAutoMergeStyleChanged));

        public static bool GetAutoMergeStyle(DependencyObject d)
        {
            return (bool)d.GetValue(AutoMergeStyleProperty);
        }

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

        public static readonly DependencyProperty BaseOnStyleProperty =
            DependencyProperty.RegisterAttached("BaseOnStyle", typeof(Style), typeof(ThemeStyleBehavior),
                new FrameworkPropertyMetadata(null, OnBaseOnStyleChanged));

        public static Style GetBaseOnStyle(DependencyObject d)
        {
            return (Style)d.GetValue(BaseOnStyleProperty);
        }

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

        public static readonly DependencyProperty OriginalStyleProperty =
            DependencyProperty.RegisterAttached("OriginalStyle", typeof(Style), typeof(ThemeStyleBehavior),
                new FrameworkPropertyMetadata((Style)null));

        public static Style GetOriginalStyle(DependencyObject d)
        {
            return (Style)d.GetValue(OriginalStyleProperty);
        }

        public static void SetOriginalStyle(DependencyObject d, Style value)
        {
            d.SetValue(OriginalStyleProperty, value);
        }

        #endregion
    }
}
