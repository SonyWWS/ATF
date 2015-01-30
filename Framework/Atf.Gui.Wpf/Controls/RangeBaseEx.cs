//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Represents an element that has a value in a specific range. Adds extended information
    /// such as a default value, whether the value is logarithmic, and whether the value is 
    /// currently being edited.</summary>
    public abstract class RangeBaseEx : RangeBase
    {
        static RangeBaseEx()
        {
            SmallChangeProperty.OverrideMetadata(typeof(RangeBaseEx), new FrameworkPropertyMetadata(0.001));
            LargeChangeProperty.OverrideMetadata(typeof(RangeBaseEx), new FrameworkPropertyMetadata(0.1));
        }
        
        /// <summary>
        /// Constructor</summary>
        protected RangeBaseEx()
        {
            SkewFactor = 1.0;
        }

        /// <summary>
        /// Gets or sets the default value for the element</summary>
        public double DefaultValue
        {
            get { return (double)GetValue(DefaultValueProperty); }
            set { SetValue(DefaultValueProperty, value); }
        }

        /// <summary>
        /// Dependency property for the default value</summary>
        public static readonly DependencyProperty DefaultValueProperty =
            DependencyProperty.Register("DefaultValue", typeof(double), typeof(RangeBaseEx),
                new FrameworkPropertyMetadata(0.0, DefaultValueChanged));

        /// <summary>
        /// Gets or sets whether to show the slider</summary>
        public bool? ShowSlider
        {
            get { return (bool?)GetValue(ShowSliderProperty); }
            set { SetValue(ShowSliderProperty, value); }
        }

        /// <summary>
        /// Dependency property for whether to show the slider</summary>
        public static readonly DependencyProperty ShowSliderProperty =
            DependencyProperty.Register("ShowSlider", typeof(bool?), typeof(RangeBaseEx), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceShowSlider));

        /// <summary>
        /// Gets or sets whether the value is currently being edited</summary>
        public bool IsValueEditing
        {
            get { return (bool)GetValue(IsValueEditingProperty); }
            set { SetValue(IsValueEditingProperty, value); }
        }

        /// <summary>
        /// Dependency property for whether the value is currently being edited</summary>
        public static readonly DependencyProperty IsValueEditingProperty =
            DependencyProperty.Register("IsValueEditing", typeof(bool), typeof(RangeBaseEx), new UIPropertyMetadata(false, IsValueEditingChanged));

        /// <summary>
        /// Gets or sets whether the value should be displayed on a logarithmic scale</summary>
        public bool IsLogarithmic
        {
            get { return (bool)GetValue(IsLogarithmicProperty); }
            set { SetValue(IsLogarithmicProperty, value); }
        }

        /// <summary>
        /// Dependency property for whether the value should be displayed on a logarithmic scale</summary>
        public static readonly DependencyProperty IsLogarithmicProperty =
            DependencyProperty.Register("IsLogarithmic", typeof(bool), typeof(RangeBaseEx), 
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the orientation of the element - horizontal or vertical</summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Dependency property for the vertical or horizontal orientation of the element</summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(RangeBaseEx), 
                new UIPropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        /// <summary>
        /// Gets or sets the brush to use for drawing the slider</summary>
        public Brush SliderBrush
        {
            get { return (Brush)GetValue(SliderBrushProperty); }
            set { SetValue(SliderBrushProperty, value); }
        }

        /// <summary>
        /// Dependency property for the brush to use when drawing the slider</summary>
        public static readonly DependencyProperty SliderBrushProperty =
            DependencyProperty.Register("SliderBrush", typeof(Brush), typeof(RangeBaseEx), 
                new FrameworkPropertyMetadata(Brushes.Pink, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the default interval to change the value by when the slider is moved</summary>
        public double DefaultChange
        {
            get { return (double)GetValue(DefaultChangeProperty); }
            set { SetValue(DefaultChangeProperty, value); }
        }

        /// <summary>
        /// Dependency property for the default interval to change the value by</summary>
        public static readonly DependencyProperty DefaultChangeProperty =
            DependencyProperty.Register("DefaultChange", typeof(double), typeof(RangeBaseEx),
                new FrameworkPropertyMetadata(0.01, DefaultChangeChanged));

        /// <summary>
        /// Gets or sets the center value for the range</summary>
        public double Center
        {
            get { return (double)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        /// <summary>
        /// Dependency property for the center value for the range</summary>
        public static readonly DependencyProperty CenterProperty =
            DependencyProperty.Register("Center", typeof(double), typeof(RangeBaseEx),
                new FrameworkPropertyMetadata(double.NaN, OnCenterChanged, OnCoerceCenter));

        /// <summary>
        /// Gets or sets the hard maximum for the range</summary>
        public double HardMaximum
        {
            get { return (double)GetValue(HardMaximumProperty); }
            set { SetValue(HardMaximumProperty, value); }
        }

        /// <summary>
        /// Dependency property for the hard maximum</summary>
        public static readonly DependencyProperty HardMaximumProperty =
            DependencyProperty.Register("HardMaximum", typeof(double), typeof(RangeBaseEx), 
                new FrameworkPropertyMetadata(double.NaN, 
                    FrameworkPropertyMetadataOptions.AffectsRender, HardMaximumChanged, CoerceHardMaximum));

        /// <summary>
        /// Gets or sets the hard minimum for the range</summary>
        public double HardMinimum
        {
            get { return (double)GetValue(HardMinimumProperty); }
            set { SetValue(HardMinimumProperty, value); }
        }

        /// <summary>
        /// Dependency property for the hard minimum</summary>
        public static readonly DependencyProperty HardMinimumProperty =
            DependencyProperty.Register("HardMinimum", typeof(double), typeof(RangeBaseEx), 
                new FrameworkPropertyMetadata(double.NaN, 
                    FrameworkPropertyMetadataOptions.AffectsRender, HardMinimumChanged, CoerceHardMinimum));

        /// <summary>
        /// Gets or sets the command that commits an edit</summary>
        public ICommand CommitEditCommand
        {
            get { return (ICommand)GetValue(CommitEditCommandProperty); }
            set { SetValue(CommitEditCommandProperty, value); }
        }

        /// <summary>
        /// Dependency property for the command that commits an edit</summary>
        public static readonly DependencyProperty CommitEditCommandProperty =
            DependencyProperty.Register("CommitEditCommand", typeof(ICommand), typeof(RangeBaseEx), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command that cancels an edit</summary>
        public ICommand CancelEditCommand
        {
            get { return (ICommand)GetValue(CancelEditCommandProperty); }
            set { SetValue(CancelEditCommandProperty, value); }
        }

        /// <summary>
        /// Called when the dependency property is being evaluated or coercion is specifically requested</summary>
        /// <param name="value">The suggested value</param>
        /// <returns>The value adjusted to be within the defined range</returns>
        protected virtual double OnCoerceCenter(double value)
        {
            return (value < Minimum) ? Minimum : (value > Maximum) ? Maximum : value;
        }

        /// <summary>
        /// Gets or sets whether the element has cancelled changes</summary>
        protected bool HasCancelledChanges { get; set; }

        /// <summary>
        /// Gets the skew factor</summary>
        protected double SkewFactor { get; private set; }

        /// <summary>
        /// Adjusts the provided value to be within the hard limits of the range</summary>
        /// <param name="value">Suggested value</param>
        /// <returns>Adjusted value within the hard limits of the range</returns>
        protected double EnforceHardLimits(double value)
        {
            return Math.Max(HardMinimum, Math.Min(HardMaximum, value));
        }

        /// <summary>
        /// Snaps the value to the nearest multiple of the "change" interval</summary>
        /// <param name="value">Suggested value</param>
        /// <param name="change">Interval to snap to</param>
        /// <returns>Snapped value</returns>
        protected double SnapTo(double value, double change)
        {
            return (Math.Round(value / change) * change);
        }

        /// <summary>
        /// Calculates the value that the slider should have based on the desired proportion 
        /// of its total range</summary>
        /// <param name="proportion">Proportion of the total length to represent by the slider value. Should be in the range [0, 1]</param>
        /// <returns>The proportional value of the slider</returns>
        protected double ProportionOfLengthToValue(double proportion)
        {
            if (Math.Abs(SkewFactor - 1.0) > 0.000001 && proportion > 0.0)
                proportion = Math.Exp(Math.Log(proportion) / SkewFactor);

            return Minimum + (Maximum - Minimum) * proportion;
        }

        /// <summary>
        /// Calculates the proportion of the total range represented by the given value of the slider</summary>
        /// <param name="value">Value of the slider</param>
        /// <returns>Proportion of the total range</returns>
        protected double ValueToProportionOfLength(double value)
        {
            double n = (value - Minimum) / (Maximum - Minimum);
            return Math.Abs(SkewFactor - 1.0) < 0.000001 ? n : Math.Pow(n, SkewFactor);
        }

        /// <summary>
        /// Helper function for calculating logarithmic slider value to display when IsLogarithmic is true</summary>
        /// <param name="inputMinimum">Input minimum value</param>
        /// <param name="inputMaximum">Input maximum value</param>
        /// <param name="outputMinimum">Output minimum value</param>
        /// <param name="outputMaximum">Output maximum value</param>
        /// <param name="value">Number whose logarithm is calculated</param>
        /// <returns>Logarithm value to use on slider</returns>
        protected double Log(double inputMinimum, double inputMaximum, double outputMinimum, double outputMaximum, double value)
        {
            var inputRange = (Math.Exp(2.0) - 1.0) / (inputMaximum - inputMinimum);
            var outputRange = outputMaximum - outputMinimum;
            return outputMinimum + outputRange * (Math.Log(1.0 + inputRange * (value - inputMinimum)) / 2.0);
        }

        /// <summary>
        /// Helper function for calculating exponential slider value to display when IsLogarithmic is true</summary>
        /// <param name="inputMinimum">Input minimum value</param>
        /// <param name="inputMaximum">Input maximum value</param>
        /// <param name="outputMinimum">Output minimum value</param>
        /// <param name="outputMaximum">Output maximum value</param>
        /// <param name="value">Number specifying a power</param>
        /// <returns>Value for number e raised to power to use on slider</returns>
        protected double Exp(double inputMinimum, double inputMaximum, double outputMinimum, double outputMaximum, double value)
        {
            var inputRange = 2.0 / (inputMaximum - inputMinimum);
            var outputRange = outputMaximum - outputMinimum;
            return outputMinimum + outputRange * ((Math.Exp(inputRange * (value - inputMinimum)) - 1.0) / (Math.Exp(2.0) - 1.0));
        }

        /// <summary>
        /// Conversion function to parse a double from a string value</summary>
        /// <param name="stringValue">The displayed string</param>
        /// <returns>The double represented by the string, or null if the string does not represent a
        /// valid double</returns>
        protected virtual double? ParseDoubleFromValueAsString(string stringValue)
        {
            double? nullable = null;
            try
            {
                double d = Convert.ToDouble(stringValue);
                if (!double.IsNaN(d))
                {
                    nullable = d;
                }
            }
            catch (FormatException)
            {
                return nullable;
            }
            catch (Exception)
            {
                return nullable;
            }

            return !nullable.HasValue ? nullable : EnforceHardLimits(nullable.Value);
        }

        /// <summary>
        /// Indicates whether the settings define a valid range</summary>
        protected bool HasRange
        {
            get { return (((Minimum > double.MinValue) && (Maximum < double.MaxValue)) && (Minimum < Maximum)); }
        }

        /// <summary>
        /// Calls the commit command when the commit is requested</summary>
        protected virtual void OnCommitChanges()
        {
            var command = CommitEditCommand;
            if (command != null && command.CanExecute(this))
                command.Execute(this);
        }

        /// <summary>
        /// Calls the cancel command when a cancellation is requested</summary>
        protected virtual void OnCancelChanges()
        {
            var command = CancelEditCommand;
            if (command != null && command.CanExecute(this))
                command.Execute(this);
        }

        /// <summary>
        /// Event handler for when the element's value changes. The element's dependency 
        /// properties are reevaluated.</summary>
        /// <param name="oldValue">Original value</param>
        /// <param name="newValue">New value</param>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            CoerceValue(MinimumProperty);
            CoerceValue(MaximumProperty);

            base.OnValueChanged(oldValue, newValue);
        }

        /// <summary>
        /// Event handler for when the element's minimum value changes. The element's dependency 
        /// properties are reevaluated.</summary>
        /// <param name="oldValue">Original minimum</param>
        /// <param name="newValue">New minimum</param>
        protected override void OnMinimumChanged(double oldValue, double newValue)
        {
            CoerceValue(HardMinimumProperty);
            CoerceValue(ShowSliderProperty);
            CoerceValue(CenterProperty);

            base.OnMinimumChanged(oldValue, newValue);

            RecalculateSkewFactor(Center, Minimum, Maximum);
        }

        /// <summary>
        /// Event handler for when the element's maximum value changes. The element's dependency 
        /// properties are reevaluated.</summary>
        /// <param name="oldValue">Original maximum</param>
        /// <param name="newValue">New maximum</param>
        protected override void OnMaximumChanged(double oldValue, double newValue)
        {
            CoerceValue(HardMaximumProperty);
            CoerceValue(ShowSliderProperty);
            CoerceValue(CenterProperty);

            base.OnMaximumChanged(oldValue, newValue);

            RecalculateSkewFactor(Center, Minimum, Maximum);
        }

        /// <summary>
        /// Event handler for when the center value changes. Recalculates the skew factor based
        /// on the new center</summary>
        /// <param name="oldValue">Previous center value</param>
        /// <param name="newValue">New center value</param>
        protected virtual void OnCenterChanged(double oldValue, double newValue)
        {
            RecalculateSkewFactor(newValue, Minimum, Maximum);
        }

        /// <summary>
        /// Event handler for when the element's orientation changes. Does nothing.</summary>
        /// <param name="oldValue">Not used</param>
        /// <param name="newValue">Not used</param>
        protected virtual void OnOrientationChanged(Orientation oldValue, Orientation newValue)
        {
        }

        /// <summary>
        /// Callback for dependency property changed</summary>
        /// <param name="d">DependencyObject</param>
        /// <param name="e">Dependency property changed event arguments</param>
        static void DefaultValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as RangeBaseEx;
            if (slider != null)
            {
            }
        }

        private static object CoerceShowSlider(DependencyObject target, object value)
        {
            var editor = target as RangeBaseEx;
            if (editor != null)
            {
                var nullable = value as bool?;
                if (!nullable.HasValue)
                {
                    return editor.HasRange;
                }
            }
            
            return value;
        }

        private static void IsValueEditingChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var slider = o as RangeBaseEx;
            if (slider != null)
            {
                if (!slider.IsValueEditing)
                {
                    if (slider.HasCancelledChanges)
                    {
                        slider.OnCancelChanges();
                    }
                    else
                    {
                        slider.OnCommitChanges();
                    }
                }

                slider.HasCancelledChanges = false;
            }
        }
        
        private static void OnOrientationChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var slider = o as RangeBaseEx;
            if (slider != null)
            {
                slider.OnOrientationChanged((Orientation)e.OldValue, (Orientation)e.NewValue);
            }
        }

        static void DefaultChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as RangeBaseEx;
            if (slider != null)
            {
                slider.CoerceValue(SmallChangeProperty);
                slider.CoerceValue(LargeChangeProperty);
            }
        }

        private static void OnCenterChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var slider = o as RangeBaseEx;
            if (slider != null)
            {
                slider.OnCenterChanged((double)e.OldValue, (double)e.NewValue);
            }
        }

        private static object OnCoerceCenter(DependencyObject o, object value)
        {
            var slider = o as RangeBaseEx;
            if (slider != null)
            {
                return slider.OnCoerceCenter((double)value);
            }

            return value;
        }

        private static object CoerceHardMaximum(DependencyObject target, object value)
        {
            var slider = target as RangeBaseEx;
            if (slider != null)
            {
                var d = (double)value;
                if (double.IsNaN(d))
                {
                    return slider.Maximum;
                }
            }
            
            return value;
        }

        private static void HardMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as RangeBaseEx;
            if (slider != null)
            {
                slider.CoerceValue(ShowSliderProperty);
            }
        }

        private static object CoerceHardMinimum(DependencyObject target, object value)
        {
            var slider = target as RangeBaseEx;
            if (slider != null)
            {
                var d = (double)value;
                if (double.IsNaN(d))
                {
                    return slider.Minimum;
                }
            }
            
            return value;
        }

        private static void HardMinimumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = d as RangeBaseEx;
            if (slider != null)
            {
                slider.CoerceValue(ShowSliderProperty);
            }
        }
                
        static readonly DependencyProperty CancelEditCommandProperty =
            DependencyProperty.Register("CancelEditCommand", typeof(ICommand), typeof(RangeBaseEx), new PropertyMetadata(null));

        private void RecalculateSkewFactor(double centerValue, double minimum, double maximum)
        {
            if (!double.IsNaN(centerValue) && maximum > minimum)
            {
                var scale = (centerValue - minimum) / (maximum - minimum);
                if (scale > 0.0)
                {
                    SkewFactor = Math.Log(0.5) / Math.Log((centerValue - minimum) / (maximum - minimum));
                }
                else
                {
                    SkewFactor = 1.0;
                }
            }
        }
    }
}
