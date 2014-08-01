//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Resizable popup, that can be resized to one side depending on which side it is docked to
    /// </summary>
    public partial class ResizablePopup : Popup
    {
        public static DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(ResizablePopup));
        public Brush Background
        {
            get { return ((Brush)(base.GetValue(ResizablePopup.BackgroundProperty))); }
            set { base.SetValue(ResizablePopup.BackgroundProperty, value); }
        }

        public static DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ResizablePopup));
        public Brush BorderBrush
        {
            get { return ((Brush)(base.GetValue(ResizablePopup.BorderBrushProperty))); }
            set { base.SetValue(ResizablePopup.BorderBrushProperty, value); }
        }

        /// <summary>
        /// Gets whether the Popup is being resized
        /// </summary>
        public bool Resizing { get; private set; }
        /// <summary>
        /// Dock Side dependency property, which side this popup is docked to
        /// </summary>
        public static DependencyProperty DockSideProperty = DependencyProperty.Register("DockSide", typeof(Dock), typeof(ResizablePopup));
        /// <summary>
        /// Gets or sets the dock side of popup
        /// </summary>
        public Dock DockSide
        {
            get { return ((Dock)(base.GetValue(ResizablePopup.DockSideProperty))); }
            set 
            { 
                base.SetValue(ResizablePopup.DockSideProperty, value);
                UpdateUi();
            }
        }
        /// <summary>
        /// Content dependency property of popup
        /// </summary>
        public static DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(UIElement), typeof(ResizablePopup));
        /// <summary>
        /// Gets or sets the content of the popup
        /// </summary>
        public UIElement Content
        {
            get { return ((UIElement)(base.GetValue(ResizablePopup.ContentProperty))); }
            set { base.SetValue(ResizablePopup.ContentProperty, value); }
        }
        /// <summary>
        /// Will update the visibility of resize thumbs depending on dock side
        /// </summary>
        private void UpdateUi()
        {
            TopResizeThumb.Visibility = DockSide == Dock.Bottom ? Visibility.Visible : Visibility.Collapsed;
            BottomResizeThumb.Visibility = DockSide == Dock.Top ? Visibility.Visible : Visibility.Collapsed;
            LeftResizeThumb.Visibility = DockSide == Dock.Right ? Visibility.Visible : Visibility.Collapsed;
            RightResizeThumb.Visibility = DockSide == Dock.Left ? Visibility.Visible : Visibility.Collapsed;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public ResizablePopup()
        {
            InitializeComponent();
            Resizing = false;
            UpdateUi();
        }
        /// <summary>
        /// Callback when dragging position of the resize thumb changes
        /// </summary>
        /// <param name="sender">Thumb sender</param>
        /// <param name="e">Delta arguments</param>
        private void ThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            switch (DockSide)
            {
                case Dock.Top:
                    Height += e.VerticalChange;
                    break;
                case Dock.Bottom:					
                    Height -= e.VerticalChange;
                    break;
                case Dock.Left:
                    Width += e.HorizontalChange;
                    break;
                case Dock.Right:
                    Width -= e.HorizontalChange;
                    break;
            }
            Width = Math.Min(Width, MaxWidth);
            Height = Math.Min(Height, MaxHeight);
            Thumb t = sender as Thumb;
        }
        /// <summary>
        /// Callback when dragging of resize thumb starts (mouse down)
        /// </summary>
        /// <param name="sender">Thumb sender</param>
        /// <param name="e">Start arguments</param>
        private void ThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            Resizing = true;
        }
        /// <summary>
        /// Callback when dragging of resize thumb ends (mouse up)
        /// </summary>
        /// <param name="sender">Thumb sender</param>
        /// <param name="e">Completed arguments</param>
        private void ThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Resizing = false;
        }
    }
}
