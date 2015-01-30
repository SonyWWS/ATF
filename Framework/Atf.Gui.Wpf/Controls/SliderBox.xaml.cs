//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Combines a slider with a formatted textbox.
    /// Also provides the option of acting in deferred mode, 
    /// where the slider value is not propagated to the binding source while being dragged.</summary>
    public partial class SliderBox : UserControl
    {
        /// <summary>
        /// Gets or sets the slider value</summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Slider value dependency property</summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(SliderBox), 
            new FrameworkPropertyMetadata(0D, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets the text box formatting string</summary>
        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        /// <summary>
        /// Text box formatting string dependency property</summary>
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat",
                typeof(string), typeof(SliderBox), new UIPropertyMetadata("{0:0.00}"));

        /// <summary>
        /// Gets or sets whether to defer updating the value while dragging the slider</summary>
        public bool DeferDragUpdate
        {
            get { return (bool)GetValue(DeferDragUpdateProperty); }
            set { SetValue(DeferDragUpdateProperty, value); }
        }

        /// <summary>
        /// Whether to defer updating the value while dragging the slider dependency property</summary>
        public static readonly DependencyProperty DeferDragUpdateProperty =
            DependencyProperty.Register("DeferDragUpdate", typeof(bool), typeof(SliderBox), new UIPropertyMetadata(false));

        /// <summary>
        /// Gets or sets slider maximum value</summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        /// <summary>
        /// Slider maximum value dependency property</summary>
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(SliderBox), new UIPropertyMetadata(100.0));


        /// <summary>
        /// Gets or sets slider minimum value</summary>
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        /// <summary>
        /// Slider minimum value dependency property</summary>
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(SliderBox), new UIPropertyMetadata(0.0));

        /// <summary>
        /// Gets or sets the value to be added to or subtracted from slider value when the slider is moved a small distance</summary>
        public double SmallChange
        {
            get { return (double)GetValue(SmallChangeProperty); }
            set { SetValue(SmallChangeProperty, value); }
        }

        /// <summary>
        /// Value to be added to or subtracted from slider value when the slider is moved a small distance dependency property</summary>
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register("SmallChange", typeof(double), typeof(SliderBox), new UIPropertyMetadata(1.0));

        /// <summary>
        /// Gets or sets the value to be added to or subtracted from slider value when the slider is moved a large distance</summary>
        public double LargeChange
        {
            get { return (double)GetValue(LargeChangeProperty); }
            set { SetValue(LargeChangeProperty, value); }
        }

        /// <summary>
        /// Value to be added to or subtracted from slider value when the slider is moved a large distance dependency property</summary>
        public static readonly DependencyProperty LargeChangeProperty =
            DependencyProperty.Register("LargeChange", typeof(double), typeof(SliderBox), new UIPropertyMetadata(10.0));
        
        /// <summary>
        /// Constructor</summary>
        public SliderBox()
        {
            InitializeComponent();
            PositionSlider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler(Slider_DragStarted));
            PositionSlider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler(Slider_DragCompleted));
            PositionSlider.ValueChanged += PositionSlider_ValueChanged;
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            m_dragging = true;
        }

        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            m_dragging = false;
            if (DeferDragUpdate)
            {
                UpdateValueTarget(this);
            }
        }

        private void PositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DeferDragUpdate)
            {
                if (!m_dragging)
                {
                    UpdateValueTarget(this);
                }
            }
            else
            {
                UpdateValueTarget(this);
            }
        }

        private static void UpdateValueTarget(SliderBox sliderBox)
        {

            BindingExpression b = sliderBox.PositionSlider.GetBindingExpression(Slider.ValueProperty);
            if (b != null)
            {
                b.UpdateSource();
            }
        }

        private bool m_dragging;

    }
}
