//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows.Controls;
using System.Windows;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Button that works with the SidePopup</summary>
    public class SideBarButton : Button
    {
        /// <summary>
        /// Dependency property for whether the button is checked</summary>
        public static DependencyProperty IsCheckedProperty = 
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(SideBarButton));
        
        /// <summary>
        /// Gets and sets whether the button is checked</summary>
        public bool IsChecked
        {
            get { return ((bool)(base.GetValue(SideBarButton.IsCheckedProperty))); }
            set { base.SetValue(SideBarButton.IsCheckedProperty, value); }
        }

        /// <summary>
        /// Dependency property for the gradient starting location</summary>
        public static DependencyProperty GradientStartPointProperty = 
            DependencyProperty.Register("GradientStartPoint", typeof(Point), typeof(SideBarButton));

        /// <summary>
        /// Gets and sets the gradient starting location</summary>
        public Point GradientStartPoint
        {
            get { return ((Point)(base.GetValue(SideBarButton.GradientStartPointProperty))); }
            set { base.SetValue(SideBarButton.GradientStartPointProperty, value); }
        }

        /// <summary>
        /// Dependency property for the gradient ending location</summary>
        public static DependencyProperty GradientEndPointProperty = 
            DependencyProperty.Register("GradientEndPoint", typeof(Point), typeof(SideBarButton));

        /// <summary>
        /// Gets and sets the gradient ending location</summary>
        public Point GradientEndPoint
        {
            get { return ((Point)(base.GetValue(SideBarButton.GradientEndPointProperty))); }
            set { base.SetValue(SideBarButton.GradientEndPointProperty, value); }
        }

        /// <summary>
        /// Dependency property for the dock position</summary>
        public static DependencyProperty DockProperty = 
            DependencyProperty.Register("Dock", typeof(Dock), typeof(SideBarButton));

        /// <summary>
        /// Gets and sets the dock position of the button</summary>
        public Dock Dock
        {
            get { return ((Dock)(base.GetValue(SideBarButton.DockProperty))); }
            set { base.SetValue(SideBarButton.DockProperty, value); }
        }

        /// <summary>
        /// Static constructor that overrides the style that is used by styling.</summary>
        static SideBarButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SideBarButton), new FrameworkPropertyMetadata(typeof(SideBarButton)));
        }

        /// <summary>
        /// Constructor</summary>
        public SideBarButton()
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="dock">Initial docking position for the button</param>
        public SideBarButton(Dock dock)
        {
            Dock = dock;
        }
    }
}
