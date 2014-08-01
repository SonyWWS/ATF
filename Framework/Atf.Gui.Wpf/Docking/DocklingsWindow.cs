//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// This is transparent window, that hosts dock icons when user wants to dock window. It is topmost,
    /// above all windows so user can see the icons and previews. The window size and position is same 
    /// as the control that it is requested for. 
    /// This class mimics the same behavior as AdornerLayer, but is topmost above all other windows too.
    /// </summary>
    internal class DocklingsWindow : Window
    {
        private Canvas m_canvas;
        /// <summary>
        /// Constructor</summary>
        /// <param name="element">Source element to create window for</param>
        public DocklingsWindow(FrameworkElement element)
        {
            ShowInTaskbar = false;
            ShowActivated = false;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            m_canvas = new Canvas();
            Content = m_canvas;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Point position = element.PointToScreen(new Point(0, 0));
            Matrix m = PresentationSource.FromVisual(Window.GetWindow(element)).CompositionTarget.TransformToDevice;
            m.Invert();
            position = m.Transform(position);
            Left = position.X;
            Top = position.Y;
            Width = element.ActualWidth;
            Height = element.ActualHeight;
        }

        /// <summary>
        /// Add icon to the window</summary>
        /// <param name="icon">The icon to add</param>
        public void AddChild(FrameworkElement icon)
        {
            m_canvas.Children.Add(icon);
        }

        /// <summary>
        /// Insert the icon at the given index. This index is used as Z-Index too.</summary>
        /// <param name="index">Index at which to insert the icon</param>
        /// <param name="icon">Icon to insert</param>
        public void InsertChild(int index, FrameworkElement icon)
        {
            m_canvas.Children.Insert(index, icon);
        }

        /// <summary>
        /// Remove all children.</summary>
        public void ClearChildren()
        {
            m_canvas.Children.Clear();
        }

        /// <summary>
        /// Remove the given child icon.</summary>
        /// <param name="icon">Child icon to be removed from window</param>
        public void RemoveChild(FrameworkElement icon)
        {
            m_canvas.Children.Remove(icon);
        }

        /// <summary>
        /// The window will close when empty</summary>
        internal void CloseIfEmpty()
        {
            if (m_canvas.Children.Count == 0)
            {
                Close();
            }
        }
    }
}
