//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Forms.Integration;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// WindowsFormsHost (object that hosts a Windows Forms control on a WPF page) that provides child binding for XAML</summary>
    public class WindowsFormsHostEx : WindowsFormsHost
    {
        /// <summary>
        /// Gets or sets Windows Forms control bound child</summary>
        public System.Windows.Forms.Control BoundChild
        {
            get { return (System.Windows.Forms.Control)GetValue(BoundChildProperty); }
            set { SetValue(BoundChildProperty, value); }
        }

        /// <summary>
        /// Windows Forms control bound child dependency property</summary>
        public static readonly DependencyProperty BoundChildProperty =
            DependencyProperty.Register("BoundChild", typeof(System.Windows.Forms.Control), typeof(WindowsFormsHostEx), new PropertyMetadata(BoundChildPropertyChanged));

        private static void BoundChildPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((WindowsFormsHostEx)sender).Child = e.NewValue as System.Windows.Forms.Control;
        }
    }
}
