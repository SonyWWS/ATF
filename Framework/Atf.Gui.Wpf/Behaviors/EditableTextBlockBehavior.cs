//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Behaviors
{
    /// <summary>
    /// Editable text block behavior</summary>
    public static class EditableTextBlockBehavior
    {
        #region IsInEditMode Property

        /// <summary>
        /// Returns whether text block is in edit mode (IsInEditMode dependency property)</summary>
        /// <param name="obj">Dependency object to obtain property for</param>
        /// <returns>Whether text block is in edit mode</returns>
        public static bool GetIsInEditMode(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsInEditModeProperty);
        }

        /// <summary>
        /// Sets whether text block is in edit mode (IsInEditMode dependency property)</summary>
        /// <param name="obj">Dependency object to set property for</param>
        /// <param name="value">Whether text block is in edit mode</param>
        public static void SetIsInEditMode(DependencyObject obj, bool value)
        {
            obj.SetValue(IsInEditModeProperty, value);
        }

        /// <summary>
        /// IsInEditMode dependency property</summary>
        public static readonly DependencyProperty IsInEditModeProperty =
            DependencyProperty.RegisterAttached("IsInEditMode", typeof(bool), typeof(EditableTextBlockBehavior), new UIPropertyMetadata(false, OnIsInEditModePropertyChanged ));
        
        #endregion

        #region EditOnDoubleClick Property

        /// <summary>
        /// Returns whether edit mode activated by double mouse click (EditOnDoubleClick dependency property)</summary>
        /// <param name="obj">Dependency object with property</param>
        /// <returns>Whether edit mode activated by double mouse click</returns>
        public static bool GetEditOnDoubleClick(DependencyObject obj)
        {
            return (bool)obj.GetValue(EditOnDoubleClickProperty);
        }

        /// <summary>
        /// Sets whether edit mode activated by double mouse click (EditOnDoubleClick dependency property)</summary>
        /// <param name="obj">Dependency object to set property for</param>
        /// <param name="value">Whether edit mode activated by double mouse click</param>
        public static void SetEditOnDoubleClick(DependencyObject obj, bool value)
        {
            obj.SetValue(EditOnDoubleClickProperty, value);
        }

        /// <summary>
        /// EditOnDoubleClick dependency property</summary>
        public static readonly DependencyProperty EditOnDoubleClickProperty =
            DependencyProperty.RegisterAttached("EditOnDoubleClick", typeof(bool), typeof(EditableTextBlockBehavior), new UIPropertyMetadata(false, OnEditOnDoubleClickPropertyChanged));

        #endregion

        private static void OnEditOnDoubleClickPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;
            if (textBlock == null)
                throw new ArgumentException("EditOnDoubleClick property can only be set on TextBlock");

            if ((bool)e.NewValue)
            {
                textBlock.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(textBlock_MouseLeftButtonUp);
                textBlock.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(textBlock_PreviewMouseLeftButtonDown);
            }
            else
            {
                textBlock.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(textBlock_MouseLeftButtonUp);
                textBlock.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler(textBlock_PreviewMouseLeftButtonDown);
            }
        }

        private static void textBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                (sender as TextBlock).SetValue(IsInEditModeProperty, true);
                e.Handled = true;
            }
        }

        private static void textBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                (sender as TextBlock).SetValue(IsInEditModeProperty, true);
            }
        }

        private static void OnIsInEditModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = obj as TextBlock;
            if (textBlock == null)
                throw new ArgumentException("IsInEditMode property can only be set on TextBlock");
                
            //Get the adorner layer of the uielement (here TextBlock)
            var layer = AdornerLayer.GetAdornerLayer(textBlock);

            //If the IsInEditMode set to true means the user has enabled the edit mode then
            //add the adorner to the adorner layer of the TextBlock.
            if (layer != null)
            {
                if ((bool)e.NewValue)
                {
                    layer.Add(new EditableTextBlockAdorner(textBlock));
                    textBlock.CaptureMouse();
                }
                else if ((bool)e.OldValue)
                {
                    textBlock.ReleaseMouseCapture();
                    var adorner = GetAdorner(textBlock);
                    if (adorner != null)
                    {
                        layer.Remove(adorner);
                        adorner.Dispose();
                    }

                    // Update the textblock's text binding.
                    var expression = textBlock.GetBindingExpression(TextBlock.TextProperty);
                    if (null != expression)
                    {
                        expression.UpdateTarget();
                    }

                }
            }
        }

        private static EditableTextBlockAdorner GetAdorner(TextBlock textBlock)
        {
            var layer = AdornerLayer.GetAdornerLayer(textBlock);
            Adorner[] adorners = layer.GetAdorners(textBlock);
            if (adorners != null)
            {
                return (EditableTextBlockAdorner)adorners.FirstOrDefault<Adorner>(x => x is EditableTextBlockAdorner);
            }
            return null;
        }
    }

    /// <summary>
    /// Adorner class that shows TextBox over the text block when edit mode is on</summary>
    public class EditableTextBlockAdorner : Adorner, IDisposable
    {
        private readonly VisualCollection m_collection;
        private readonly TextBox m_textBox;
        private readonly TextBlock m_textBlock;

        /// <summary>
        /// Constructor</summary>
        /// <param name="adornedElement">Adorned element</param>
        public EditableTextBlockAdorner(TextBlock adornedElement)
            : base(adornedElement)
        {
            m_collection = new VisualCollection(this);
            m_textBox = new TextBox();
            m_textBox.FontSize = adornedElement.FontSize;
            m_textBox.FontFamily = adornedElement.FontFamily;
            m_textBox.FontStretch = adornedElement.FontStretch;
            m_textBox.FontStyle = adornedElement.FontStyle;
            m_textBox.FontWeight = adornedElement.FontWeight;
            m_textBox.Width = adornedElement.Width;
            m_textBox.Height = adornedElement.Height;
            m_textBox.HorizontalAlignment = HorizontalAlignment.Left;
            m_textBox.VerticalAlignment = VerticalAlignment.Top;
            m_textBox.Padding = new Thickness(0);
            m_textBlock = adornedElement;
            Binding binding = new Binding("Text") { Source = adornedElement };
            m_textBox.SetBinding(TextBox.TextProperty, binding);
            m_textBox.AcceptsReturn = false;
            m_textBox.AcceptsTab = false;
            //_textBox.MaxLength = adornedElement.MaxLength;
            m_textBox.KeyUp += TextBox_KeyUp;
            m_textBox.LostFocus += new RoutedEventHandler(TextBox_LostFocus);
            m_textBox.SelectAll();
            m_collection.Add(m_textBox);
        }

        /// <summary>
        /// Performs custom actions on TextBox LostFocus events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">RoutedEventArgs that contains the event data</param>
        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            m_textBlock.SetValue(EditableTextBlockBehavior.IsInEditModeProperty, false);
        }

        /// <summary>
        /// Performs custom actions on TextBox KeyUp events</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">KeyEventArgs that contains the event data</param>
        void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                m_textBox.Text = m_textBox.Text.Replace("\r\n", string.Empty);
                BindingExpression expression = m_textBox.GetBindingExpression(TextBox.TextProperty);
                if (null != expression)
                {
                    expression.UpdateSource();
                }
                m_textBlock.SetValue(EditableTextBlockBehavior.IsInEditModeProperty, false);
            }
            else if (e.Key == Key.Escape)
            {
                m_textBlock.SetValue(EditableTextBlockBehavior.IsInEditModeProperty, false);
            }
        }

        /// <summary>
        /// Obtains the Visual object at the index</summary>
        /// <param name="index">Index of Visual object to obtain</param>
        /// <returns>Visual object at index</returns>
        protected override Visual GetVisualChild(int index)
        {
            return m_collection[index];
        }

        /// <summary>
        /// Obtains count of Visual objects</summary>
        /// <returns>Count of Visual objects</returns>
        protected override int VisualChildrenCount
        {
            get { return m_collection.Count; }
        }

        /// <summary>
        /// Sets desired size for adorned element</summary>
        /// <param name="constraint">Desired size</param>
        /// <returns>Actual size of adorned element</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            //m_textBox.HorizontalAlignment = HorizontalAlignment.Left;
            m_textBox.MinWidth = Math.Max(45.0, AdornedElement.DesiredSize.Width + 4);
            m_textBox.Measure(new Size(double.PositiveInfinity, constraint.Height + 4));
            //Size size = m_textBox.DesiredSize;
            
            //m_textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
            //m_textBox.Measure(new Size(Math.Max(size.Width, AdornedElement.DesiredSize.Width), size.Height));

            return m_textBox.DesiredSize;
        }

        /// <summary>
        /// Arranges (positions and determines size) of adorned element</summary>
        /// <param name="finalSize">Adorned element's final size</param>
        /// <returns>Adorned element's final size</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {

            //double width = Math.Max(AdornedElement.DesiredSize.Width, m_textBox.DesiredSize.Width);
            //double height = Math.Min(AdornedElement.DesiredSize.Height, finalSize.Height);

            //double width = AdornedElement.DesiredSize.Width + 50;
            //double height = AdornedElement.DesiredSize.Height;

            var rect = new Rect(-4, -2, m_textBox.DesiredSize.Width, m_textBox.DesiredSize.Height + 2 + 4);
            
            m_textBox.Arrange(rect);
            m_textBox.Focus();

            return finalSize;
        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    drawingContext.DrawRectangle(null, new Pen
        //    {
        //        Brush = Brushes.Gold,
        //        Thickness = 2
        //    }, new Rect(0, 0, m_textBlock.DesiredSize.Width + 50, m_textBlock.DesiredSize.Height * 1.2));
        //}


        public void Dispose()
        {
            BindingOperations.ClearBinding(m_textBox, TextBox.TextProperty);
        }
    }
}
