//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// A decorator that respects SizeToContent parameters.</summary>
    public class SizeToContentDecorator : Border //Decorator
    {
        /// <summary>
        /// Backing store for the SizeToContent property</summary>
        public static readonly DependencyProperty SizeToContentProperty = DependencyProperty.Register(
            "SizeToContent",
            typeof(SizeToContent),
            typeof(SizeToContentDecorator),
            new FrameworkPropertyMetadata(SizeToContent.Manual, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Backing store for the DesiredSizeChanged event</summary>
        public static readonly RoutedEvent DesiredSizeChangedEvent = EventManager.RegisterRoutedEvent(
            "DesiredSizeChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(SizeToContentDecorator));

        /// <summary>
        /// Gets or sets the content of the size to.</summary>
        /// <value>The content of the size to.</value>
        public SizeToContent SizeToContent
        {
            get { return (SizeToContent)GetValue(SizeToContentProperty); }
            set { SetValue(SizeToContentProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SizeToContentDecorator"/> class.</summary>
        public SizeToContentDecorator()
            : base()
        {
        }

        /// <summary>
        /// Adds or removes handlers to the DesiredSizedChanged event</summary>
        public event RoutedEventHandler DesiredSizeChanged
        {
            add { AddHandler(DesiredSizeChangedEvent, value); }
            remove { RemoveHandler(DesiredSizeChangedEvent, value); }
        }

        /// <summary>
        /// Measures the child element of a <see cref="T:System.Windows.Controls.Decorator"/> to prepare for arranging it 
        /// during the <see cref="M:System.Windows.Controls.Decorator.ArrangeOverride(System.Windows.Size)"/> pass.</summary>
        /// <param name="constraint">An upper limit <see cref="T:System.Windows.Size"/> that should not be exceeded.</param>
        /// <returns>The target <see cref="T:System.Windows.Size"/> of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size measureConstraint = constraint;

            switch (this.SizeToContent)
            {
                case SizeToContent.Height:
                    measureConstraint = new Size(constraint.Width, double.PositiveInfinity);
                    break;
                case SizeToContent.Width:
                    measureConstraint = new Size(double.PositiveInfinity, constraint.Height);
                    break;
                case SizeToContent.WidthAndHeight:
                    measureConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);
                    break;
            }

            return base.MeasureOverride(measureConstraint);
        }

        /// <summary>
        /// Supports layout behavior when a child element is resized.</summary>
        /// <param name="child">The child element that is being resized.</param>
        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            this.RaiseDesiredSizedChangedEvent();
        }

        /// <summary>
        /// Raises the desired sized changed event.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Consistency with existing WPF APIs and implementation for RoutedEvents.")]
        protected void RaiseDesiredSizedChangedEvent()
        {
            var desiredSizeEventArgs = new RoutedEventArgs(DesiredSizeChangedEvent);
            this.RaiseEvent(desiredSizeEventArgs);
        }
    }
}