//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// A slider that provides a range</summary>
    [DefaultEvent("RangeSelectionChanged"),
     TemplatePart(Name = "PART_RangeSliderContainer", Type = typeof(StackPanel)),
     TemplatePart(Name = "PART_LeftEdge", Type = typeof(RepeatButton)),
     TemplatePart(Name = "PART_RightEdge", Type = typeof(RepeatButton)),
     TemplatePart(Name = "PART_LeftThumb", Type = typeof(Thumb)),
     TemplatePart(Name = "PART_MiddleThumb", Type = typeof(Thumb)),
     TemplatePart(Name = "PART_RightThumb", Type = typeof(Thumb))]
    public class RangeSlider : RangeSliderBase
    {
        /// <summary>
        /// The min range value that you can have for the range slider </summary>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when MinRange is set less than 0</exception>
        public static readonly DependencyProperty MinRangeProperty =
            DependencyProperty.Register("MinRange", typeof(double), typeof(RangeSlider),
                                        new UIPropertyMetadata(0.0,
                                                               delegate(DependencyObject sender,
                                                                        DependencyPropertyChangedEventArgs e)
                                                                   {
                                                                       if ((double)e.NewValue < 0.0)
                                                                           throw new ArgumentOutOfRangeException(
                                                                               "value",
                                                                               "value for MinRange cannot be less than 0");

                                                                       var slider = (RangeSlider) sender;
                                                                       if (!slider.m_internalUpdate)
                                                                           //check if the property is set internally
                                                                       {
                                                                           slider.m_internalUpdate = true;
                                                                           //set flag to signal that the properties are being set by the object itself
                                                                           slider.RangeStop =
                                                                               Math.Max(slider.RangeStop,slider.RangeStart + (double)e.NewValue);
                                                                           slider.Maximum = Math.Max(slider.Maximum,slider.RangeStop);
                                                                           slider.m_internalUpdate = false;
                                                                           //set flag to signal that the properties are being set by the object itself

                                                                           slider.ReCalculateRanges();
                                                                           slider.ReCalculateWidths();
                                                                       }
                                                                   }));

        /// <summary>
        /// Event raised whenever the selected range is changed </summary>
        public static readonly RoutedEvent RangeSelectionChangedEvent =
            EventManager.RegisterRoutedEvent("RangeSelectionChanged",
                                             RoutingStrategy.Bubble, typeof(RangeSelectionChangedEventHandler),
                                             typeof(RangeSlider));

        /// <summary>
        /// The min range value that you can have for the range slider </summary>
        /// <exception cref = "ArgumentOutOfRangeException">Thrown when MinRange is set less than 0</exception>
        public double MinRange
        {
            get { return (double)GetValue(MinRangeProperty); }
            set { SetValue(MinRangeProperty, value); }
        }

        /// <summary> 
        /// Event raised whenever the selected range is changed </summary>
        public event RangeSelectionChangedEventHandler RangeSelectionChanged
        {
            add { AddHandler(RangeSelectionChangedEvent, value); }
            remove { RemoveHandler(RangeSelectionChangedEvent, value); }
        }

        /// <summary>
        /// Default constructor </summary>
        public RangeSlider()
        {
            //hook to the size change event of the range slider
            DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(RangeSlider)).
                AddValueChanged(this, delegate { ReCalculateWidths(); });
        }

        /// <summary>
        /// moves the current selection with x value </summary>
        /// <param name = "isLeft"><c>True</c> if you want to move to the left</param>
        public void MoveSelection(bool isLeft)
        {
            double widthChange = m_repeatButtonMoveRatio * (RangeStop - RangeStart)
                                 * m_movableWidth / m_movableRange;

            widthChange = isLeft ? -widthChange : widthChange;
            MoveThumb(m_leftButton, m_rightButton, widthChange);
            ReCalculateRangeSelected(true, true);
        }

        /// <summary>
        /// Reset the Slider to the Start/End </summary>
        /// <param name = "isStart">Pass true to reset to start point</param>
        public void ResetSelection(bool isStart)
        {
            double widthChange = Maximum - Minimum;
            widthChange = isStart ? -widthChange : widthChange;

            MoveThumb(m_leftButton, m_rightButton, widthChange);
            ReCalculateRangeSelected(true, true);
        }

        ///<summary>
        /// Change the range selected</summary>
        ///<param name = "span">The steps</param>
        public void MoveSelection(double span)
        {
            if (span > 0)
            {
                if (RangeStop + span > Maximum)
                    span = Maximum - RangeStop;
            }
            else
            {
                if (RangeStart + span < Minimum)
                    span = Minimum - RangeStart;
            }

            if (span != 0.0)
            {
                m_internalUpdate = true; //set flag to signal that the properties are being set by the object itself
                RangeStart += span;
                RangeStop += span;
                ReCalculateWidths();
                m_internalUpdate = false; //set flag to signal that the properties are being set by the object itself

                OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
            }
        }

        /// <summary>
        /// Sets the selected range in one go. If the selection is invalid, nothing happens. </summary>
        /// <param name = "selectionStart">New selection start value</param>
        /// <param name = "selectionStop">New selection stop value</param>
        public void SetSelectedRange(double selectionStart, double selectionStop)
        {
            double start = Math.Max(Minimum, selectionStart);
            double stop = Math.Min(selectionStop, Maximum);
            start = Math.Min(start, Maximum - MinRange);
            stop = Math.Max(Minimum + MinRange, stop);
            if (stop >= start + MinRange)
            {
                m_internalUpdate = true; //set flag to signal that the properties are being set by the object itself
                RangeStart = start;
                RangeStop = stop;
                ReCalculateWidths();
                m_internalUpdate = false; //set flag to signal that the properties are being set by the object itself
                OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
            }
        }

        /// <summary>
        /// Changes the selected range to the supplied range </summary>
        /// <param name = "span">The span to zoom</param>
        public void ZoomToSpan(double span)
        {
            m_internalUpdate = true; //set flag to signal that the properties are being set by the object itself
            // Ensure new span is within the valid range
            span = Math.Min(span, Maximum - Minimum);
            span = Math.Max(span, MinRange);
            if (span == RangeStop - RangeStart)
                return; // No change

            // First zoom half of it to the right
            double rightChange = (span - (RangeStop - RangeStart)) / 2;
            double leftChange = rightChange;

            // If we will hit the right edge, spill over the leftover change to the other side
            if (rightChange > 0 && RangeStop + rightChange > Maximum)
                leftChange += rightChange - (Maximum - RangeStop);
            RangeStop = Math.Min(RangeStop + rightChange, Maximum);
            rightChange = 0;

            // If we will hit the left edge and there is space on the right, add the leftover change to the other side
            if (leftChange > 0 && RangeStart - leftChange < Minimum)
                rightChange = Minimum - (RangeStart - leftChange);
            RangeStart = Math.Max(RangeStart - leftChange, Minimum);
            if (rightChange > 0) // leftovers to the right
                RangeStop = Math.Min(RangeStop + rightChange, Maximum);

            ReCalculateWidths();
            m_internalUpdate = false; //set flag to signal that the properties are being set by the object itself
            OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
        }

        /// <summary>
        /// Override to get the visuals from the control template</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_visualElementsContainer = EnforceInstance<StackPanel>("PART_RangeSliderContainer");
            m_centerThumb = EnforceInstance<Thumb>("PART_MiddleThumb");
            m_leftButton = EnforceInstance<RepeatButton>("PART_LeftEdge");
            m_rightButton = EnforceInstance<RepeatButton>("PART_RightEdge");
            m_leftThumb = EnforceInstance<Thumb>("PART_LeftThumb");
            m_rightThumb = EnforceInstance<Thumb>("PART_RightThumb");
            InitializeVisualElementsContainer();
            ReCalculateWidths();
        }

        /// <summary>
        /// Event fired when the minimum value of the range is changed</summary>
        /// <param name="oldValue">Previous value</param>
        /// <param name="newValue">New value</param>
        protected override void OnMinimumChanged(double oldValue, double newValue)
        {
            base.OnMinimumChanged(oldValue, newValue);
            if (!m_internalUpdate)
            {
                ReCalculateRanges();
                ReCalculateWidths();
            }
        }

        /// <summary>
        /// Event fired when the maximum value of the range is changed</summary>
        /// <param name="oldValue">Previous value</param>
        /// <param name="newValue">New value</param>
        protected override void OnMaximumChanged(double oldValue, double newValue)
        {
            base.OnMaximumChanged(oldValue, newValue);
            if (!m_internalUpdate)
            {
                ReCalculateRanges();
                ReCalculateWidths();
            }
        }

        /// <summary>
        /// Event fired when the start value of the range is changed</summary>
        /// <param name="oldValue">Previous value</param>
        /// <param name="newValue">New value</param>
        protected override void OnRangeStartChanged(double oldValue, double newValue)
        {
            base.OnRangeStartChanged(oldValue, newValue);
            
            if (!m_internalUpdate)
            {
                ReCalculateWidths();
                OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
            }
        }

        /// <summary>
        /// Event fired when the end value of the range is changed</summary>
        /// <param name="oldValue">Previous value</param>
        /// <param name="newValue">New value</param>
        protected override void OnRangeStopChanged(double oldValue, double newValue)
        {
            base.OnRangeStopChanged(oldValue, newValue);

            if (!m_internalUpdate)
            {
                ReCalculateWidths();
                OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
            }
        }

        /// <summary> 
        /// Static constructor </summary>
        static RangeSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
        }

        //drag thumb from the right splitter
        private void RightThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveThumb(m_centerThumb, m_rightButton, e.HorizontalChange);
            ReCalculateRangeSelected(false, true);
        }

        //drag thumb from the left splitter
        private void LeftThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveThumb(m_leftButton, m_centerThumb, e.HorizontalChange);
            ReCalculateRangeSelected(true, false);
        }

        //left repeat button clicked
        private void LeftButtonClick(object sender, RoutedEventArgs e)
        {
            MoveSelection(true);
        }

        //right repeat button clicked
        private void RightButtonClick(object sender, RoutedEventArgs e)
        {
            MoveSelection(false);
        }

        //drag thumb from the middle
        private void CenterThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            MoveThumb(m_leftButton, m_rightButton, e.HorizontalChange);
            ReCalculateRangeSelected(true, true);
        }

        //resizes the left column and the right column
        private static void MoveThumb(FrameworkElement x, FrameworkElement y, double horizonalChange)
        {
            double change = 0;
            if (horizonalChange < 0) //slider went left
                change = GetChangeKeepPositive(x.Width, horizonalChange);
            else if (horizonalChange > 0) //slider went right if(horizontal change == 0 do nothing)
                change = -GetChangeKeepPositive(y.Width, -horizonalChange);

            x.Width += change;
            y.Width -= change;
        }

        //ensures that the new value (newValue param) is a valid value. returns false if not
        private static double GetChangeKeepPositive(double width, double increment)
        {
            return Math.Max(width + increment, 0) - width;
        }

        //recalculates the movableRange. called from the RangeStop setter, RangeStart setter and MinRange setter
        private void ReCalculateRanges()
        {
            m_movableRange = Maximum - Minimum - MinRange;
        }

        //recalculates the movableWidth. called whenever the width of the control changes
        private void ReCalculateWidths()
        {
            if (m_leftButton != null && m_rightButton != null && m_centerThumb != null)
            {
                m_movableWidth =
                    Math.Max(ActualWidth - m_rightThumb.ActualWidth - m_leftThumb.ActualWidth - m_centerThumb.MinWidth, 1);
                m_leftButton.Width = Math.Max(m_movableWidth * (RangeStart - Minimum) / m_movableRange, 0);
                m_rightButton.Width = Math.Max(m_movableWidth * (Maximum - RangeStop) / m_movableRange, 0);
                m_centerThumb.Width =
                    Math.Max(
                        ActualWidth - m_leftButton.Width - m_rightButton.Width - m_rightThumb.ActualWidth -
                        m_leftThumb.ActualWidth, 0);
            }
        }

        /// <summary>
        /// Recalculates the RangeStart and RangeStop values. Called when the left, middle, or right thumb is moved.</summary>
        /// <param name="reCalculateStart">Whether to recalculate the RangeStart value</param>
        /// <param name="reCalculateStop">Whether to recalculate the RangeStop value</param>
        private void ReCalculateRangeSelected(bool reCalculateStart, bool reCalculateStop)
        {
            m_internalUpdate = true; //set flag to signal that the properties are being set by the object itself
            if (reCalculateStart)
            {
                // Make sure to get exactly rangestart if thumb is at the start
                if (m_leftButton.Width == 0.0)
                    RangeStart = Minimum;
                else
                    RangeStart =
                        Math.Max(Minimum, (Minimum + m_movableRange * m_leftButton.Width / m_movableWidth));
            }

            if (reCalculateStop)
            {
                // Make sure to get exactly rangestop if thumb is at the end
                if (m_rightButton.Width == 0.0)
                    RangeStop = Maximum;
                else
                    RangeStop =
                        Math.Min(Maximum, (Maximum - m_movableRange * m_rightButton.Width / m_movableWidth));
            }

            m_internalUpdate = false; //set flag to signal that the properties are being set by the object itself

            if (reCalculateStart || reCalculateStop)
                //raise the RangeSelectionChanged event
                OnRangeSelectionChanged(new RangeSelectionChangedEventArgs(this));
        }

        //Raises the RangeSelectionChanged event
        private void OnRangeSelectionChanged(RangeSelectionChangedEventArgs e)
        {
            e.RoutedEvent = RangeSelectionChangedEvent;
            RaiseEvent(e);
        }

        private T EnforceInstance<T>(string partName)
            where T : FrameworkElement, new()
        {
            return GetTemplateChild(partName) as T ?? new T();
        }

        //adds all visual elements to the conatiner
        private void InitializeVisualElementsContainer()
        {
            m_visualElementsContainer.Orientation = Orientation.Horizontal;
            m_leftThumb.Width = m_defaultSplittersThumbWidth;
            m_leftThumb.Tag = "left";
            m_rightThumb.Width = m_defaultSplittersThumbWidth;
            m_rightThumb.Tag = "right";

            //handle the drag delta
            m_centerThumb.DragDelta += CenterThumbDragDelta;
            m_leftThumb.DragDelta += LeftThumbDragDelta;
            m_rightThumb.DragDelta += RightThumbDragDelta;
            m_leftButton.Click += LeftButtonClick;
            m_rightButton.Click += RightButtonClick;
        }

        //used to move the selection by x ratio when click the repeat buttons
        private const double m_repeatButtonMoveRatio = 0.1;

        private const double m_defaultSplittersThumbWidth = 10;
        private Thumb m_centerThumb; //the center thumb to move the range around
        private bool m_internalUpdate;
        private RepeatButton m_leftButton; //the left side of the control (movable left part)
        private Thumb m_leftThumb; //the left thumb that is used to expand the range selected
        private RepeatButton m_rightButton; //the right side of the control (movable right part)
        private Thumb m_rightThumb; //the right thumb that is used to expand the range selected
        private StackPanel m_visualElementsContainer; //stackpanel to store the visual elements for this control
        private double m_movableRange;
        private double m_movableWidth;

    }

    /// <summary>
    ///     Delegate for the RangeSelectionChanged event
    /// </summary>
    /// <param name = "sender">The object raising the event</param>
    /// <param name = "e">The event arguments</param>
    public delegate void RangeSelectionChangedEventHandler(object sender, RangeSelectionChangedEventArgs e);

    /// <summary>
    ///     Event arguments for the Range slider RangeSelectionChanged event
    /// </summary>
    public class RangeSelectionChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        ///     sets the range start and range stop for the event args
        /// </summary>
        /// <param name = "newRangeStart">The new range start set</param>
        /// <param name = "newRangeStop">The new range stop set</param>
        internal RangeSelectionChangedEventArgs(double newRangeStart, double newRangeStop)
        {
            NewRangeStart = newRangeStart;
            NewRangeStop = newRangeStop;
        }

        /// <summary>
        ///     sets the range start and range stop for the event args by using the slider RangeStartSelected and RangeStopSelected properties
        /// </summary>
        /// <param name = "slider">The slider to get the info from</param>
        internal RangeSelectionChangedEventArgs(RangeSlider slider)
            : this(slider.RangeStart, slider.RangeStop)
        {
        }

        /// <summary>
        ///     The new range start selected in the range slider
        /// </summary>
        public double NewRangeStart { get; set; }

        /// <summary>
        ///     The new range stop selected in the range slider
        /// </summary>
        public double NewRangeStop { get; set; }
    }
}