using System.Windows;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Represents a slider with a range of allowed values that can be turned on or off.</summary>
    public class RangeSliderBase : RangeBaseEx
    {
        /// <summary>
        /// Gets and sets whether the range is enabled</summary>
        public bool RangeEnabled
        {
            get { return (bool)GetValue(RangeEnabledProperty); }
            set { SetValue(RangeEnabledProperty, value); }
        }

        /// <summary>
        /// Dependency property for whether the range is enabled</summary>
        public static readonly DependencyProperty RangeEnabledProperty =
            DependencyProperty.Register("RangeEnabled",
                typeof(bool), typeof(RangeSliderBase), new UIPropertyMetadata(false));

        /// <summary>
        /// Gets and sets the starting value for the range</summary>
        public double RangeStart
        {
            get { return (double)GetValue(RangeStartProperty); }
            set { SetValue(RangeStartProperty, value); }
        }

        /// <summary>
        /// Dependency property for the starting value of the range</summary>
        public static readonly DependencyProperty RangeStartProperty =
            DependencyProperty.Register("RangeStart",
                typeof(double), typeof(RangeSliderBase), new UIPropertyMetadata(0.0, OnRangeStartChanged, OnCoerceRangeStart));

        /// <summary>
        /// Gets and sets the ending value for the range</summary>
        public double RangeStop
        {
            get { return (double)GetValue(RangeStopProperty); }
            set { SetValue(RangeStopProperty, value); }
        }

        /// <summary>
        /// Dependency property for the ending value of the range</summary>
        public static readonly DependencyProperty RangeStopProperty =
            DependencyProperty.Register("RangeStop",
                typeof(double), typeof(RangeSliderBase), new UIPropertyMetadata(0.0, OnRangeStopChanged, OnCoerceRangeStop));

        /// <summary>
        /// Event handler that causes the dependency properties to be reevaluated when
        /// the starting value of the range changes</summary>
        /// <param name="oldValue">Original start value</param>
        /// <param name="newValue">New start value</param>
        protected virtual void OnRangeStartChanged(double oldValue, double newValue)
        {
            CoerceValue(MinimumProperty);
            CoerceValue(RangeStopProperty);
        }

        /// <summary>
        /// Called when the range start dependency property is being reevaluated or 
        /// explicitly coerced</summary>
        /// <param name="value">Suggested value</param>
        /// <returns>Adjusted value</returns>
        protected virtual double OnCoerceRangeStart(double value)
        {
            return (value > RangeStop) ? RangeStop : value;
        }

        /// <summary>
        /// Called when the range stop dependency property is being reevaluated or 
        /// explicitly coerced</summary>
        /// <param name="value">Suggested value</param>
        /// <returns>Adjusted value</returns>
        protected virtual double OnCoerceRangeStop(double value)
        {
            return (value < RangeStart) ? RangeStart : value;
        }

        /// <summary>
        /// Event handler that causes the dependency properties to be reevaluated when
        /// the ending value of the range changes</summary>
        /// <param name="oldValue">Original end value</param>
        /// <param name="newValue">New end value</param>
        protected virtual void OnRangeStopChanged(double oldValue, double newValue)
        {
            CoerceValue(RangeStartProperty);
            CoerceValue(MaximumProperty);
        }

        static void OnRangeStartChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var slider = o as RangeSliderBase;
            if (slider != null)
            {
                slider.OnRangeStartChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        static void OnRangeStopChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var slider = o as RangeSliderBase;
            if (slider != null)
            {
                slider.OnRangeStopChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        static object OnCoerceRangeStart(DependencyObject o, object value)
        {
            var slider = o as RangeSliderBase;
            if (slider != null)
            {
                return slider.OnCoerceRangeStart((double)value);
            }

            return value;
        }

        static object OnCoerceRangeStop(DependencyObject o, object value)
        {
            var slider = o as RangeSliderBase;
            if (slider != null)
            {
                return slider.OnCoerceRangeStop((double)value);
            }

            return value;
        }
    }
}
