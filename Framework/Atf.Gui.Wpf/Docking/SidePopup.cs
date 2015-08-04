//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading;
using System.Timers;
using System.Xml.Serialization;
using System.Windows.Media.Animation;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Collapsible panel on the side</summary>
    public class SidePopup : Selector, IDockLayout, IXmlSerializable
    {
        /// <summary>
        /// ThumbBrush DependencyProperty, which designates the thumb brush</summary>
        public static DependencyProperty ThumbBrushProperty = DependencyProperty.Register("ThumbBrush", typeof(Brush), typeof(SidePopup));
        /// <summary>
        /// Get or set ThumbBrush DependencyProperty, which designates the thumb brush</summary>
        public Brush ThumbBrush
        {
            get { return ((Brush)(base.GetValue(SidePopup.ThumbBrushProperty))); }
            set { base.SetValue(SidePopup.ThumbBrushProperty, value); }
        }
        /// <summary>
        /// Placement side of the panel
        /// </summary>
        /// <summary>
        /// TabsPlacement DependencyProperty, which designates tabs placement</summary>
        public static DependencyProperty TabsPlacementProperty = DependencyProperty.Register("TabsPlacement", typeof(Dock), typeof(SidePopup), new PropertyMetadata(TabsPlacementPropertyChanged));
        /// <summary>
        /// Get or set TabsPlacement DependencyProperty, which designates tabs placement</summary>
        public Dock TabsPlacement
        {
            get { return ((Dock)(base.GetValue(SidePopup.TabsPlacementProperty))); }
            set { base.SetValue(SidePopup.TabsPlacementProperty, value); }
        }
        /// <summary>
        /// Callback for TabsPlacement DependencyProperty changed</summary>
        /// <param name="o">Side popup DependencyObject</param>
        /// <param name="args">Arguments describing change</param>
        public static void TabsPlacementPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            SidePopup sidePopup = (SidePopup)o;
            switch ((Dock)args.NewValue)
            {
                case System.Windows.Controls.Dock.Top:
                case System.Windows.Controls.Dock.Bottom:
                    sidePopup.Orientation = Orientation.Horizontal;
                    break;
                case System.Windows.Controls.Dock.Right:
                case System.Windows.Controls.Dock.Left:
                    sidePopup.Orientation = Orientation.Vertical;
                    break;
            }
            sidePopup.UpdatePopupProperties();
        }		
        /// <summary>
        /// Orientation of flow of items (horizontal, vertical) DependencyProperty</summary>
        public static DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SidePopup));
        /// <summary>
        /// Get or set Orientation DependencyProperty, the flow of items (horizontal, vertical)</summary>
        public Orientation Orientation
        {
            get { return ((Orientation)(base.GetValue(SidePopup.OrientationProperty))); }
            private set { base.SetValue(SidePopup.OrientationProperty, value); }
        }

        private List<DockContent> m_children;
        private SideBarButton m_lastItemOver;
        //private ResizablePopup m_popup;
        private System.Timers.Timer m_timer;
        private Grid PART_Grid;
        private ResizablePopup PART_Popup;
        private DateTime m_timerTime;

        /// <summary>
        /// Static constructor that overrides the style that is used by styling</summary>
        static SidePopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SidePopup), new FrameworkPropertyMetadata(typeof(SidePopup)));
        }
        /// <summary>
        /// Default constructor</summary>
        public SidePopup()
        {
            m_children = new List<DockContent>();
            Orientation = Orientation.Vertical;
            TabsPlacement = System.Windows.Controls.Dock.Left;
            m_timer = new System.Timers.Timer();
            m_timer.AutoReset = false;
            m_timer.Interval = 50;
            m_timer.Elapsed += Timer_Elapsed;

            Application.Current.MainWindow.Deactivated += MainWindow_Deactivated;
        }

        void Window_Closing(object sender, EventArgs e)
        {
            IDockContent content = (IDockContent)PART_Popup.Tag;
            ClosePopup();
            m_lastItemOver = null;
            Undock(content);
        }

        void MainWindow_Deactivated(object sender, EventArgs e)
        {
            m_timer.Stop();
            
            if (PART_Popup != null)
            {
                PART_Popup.IsOpen = false;
            }
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(new EventHandler<ElapsedEventArgs>(Timer_Elapsed), new object[] { sender, e });
                return;
            }
            try
            {
                Point curMousePos = m_lastItemOver != null ? Win32Calls.GetPosition(m_lastItemOver) : new Point(-1, -1);
                if (m_lastItemOver != null && new Rect(0, 0, m_lastItemOver.ActualWidth, m_lastItemOver.ActualHeight).Contains(curMousePos))
                {
                    TimeSpan ts = DateTime.Now - m_timerTime;
                    if (ts.TotalMilliseconds > 500 || Mouse.LeftButton == MouseButtonState.Pressed)
                    {
                        ShowPopup();
                        m_lastItemOver.IsChecked = true;
                        m_lastItemOver = null;
                        m_timerTime = DateTime.Now;
                    }
                    m_timer.Start();
                }
                else
                {
                    var app = Application.Current;
                    if (app == null)
                        return;

                    var window = app.MainWindow;
                    if (window == null || !window.IsActive)
                        return;

                    if (PART_Popup.IsOpen && !PART_Popup.Resizing)
                    {
                        var pt32 = new User32.POINT();
                        Win32Calls.GetCursorPos(ref pt32);
                        Point mousePos = new Point(pt32.X, pt32.Y);
                        Point pos = PointToScreen(new Point(0, 0));
                        Matrix m = PresentationSource.FromVisual(Window.GetWindow(this)).CompositionTarget.TransformToDevice;
                        if (m != Matrix.Identity)
                        {
                            m.Invert();
                            mousePos = m.Transform(mousePos);
                            pos = m.Transform(pos);
                        }
                        switch (TabsPlacement)
                        {
                            case System.Windows.Controls.Dock.Top:
                                pos.Y += ActualHeight;
                                break;
                            case System.Windows.Controls.Dock.Bottom:
                                pos.Y -= PART_Popup.Height;
                                break;
                            case System.Windows.Controls.Dock.Left:
                                pos.X += ActualWidth;
                                break;
                            case System.Windows.Controls.Dock.Right:
                                pos.X -= PART_Popup.Width;
                                break;
                        }
                        bool b2 = new Rect(pos.X, pos.Y, PART_Popup.Width, PART_Popup.Height).Contains(mousePos);
                        Point posThis = Win32Calls.GetPosition(this);
                        bool b3 = new Rect(0, 0, ActualWidth, ActualHeight).Contains(posThis);
                        if (!b2 && !b3)
                        {
                            TimeSpan ts = DateTime.Now - m_timerTime;
                            if(ts.TotalMilliseconds > 1000)
                            {
                                // if mouse is outside for more than some time, then collapse this popup
                                ClosePopup();
                            }
                            else
                            {
                                m_timer.Start();
                            }
                        }
                        else
                        {
                            m_timerTime = DateTime.Now;
                            m_timer.Start();
                        }
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Can be thrown when closing the application and popup is still open...
            }
        }
        /// <summary>
        /// Show or expand popup</summary>
        private void ShowPopup()
        {
            DockContent dockContent = (DockContent)m_lastItemOver.Tag;
            ShowPopup(dockContent);
        }

        /// <summary>
        /// Show or expand popup</summary>
        /// <param name="dockContent">Dock content</param>
        internal void ShowPopup(DockContent dockContent)
        {
            if (PART_Popup.IsOpen)
            {
                if (!((DockedWindow)PART_Popup.Content).HasChild(dockContent))
                {
                    ClosePopup();
                }
                else
                {
                    return;
                }
            }
            ContentSettings settings = dockContent.Settings;
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    PART_Popup.MaxWidth = RenderSize.Width;
                    PART_Popup.MaxHeight = 0.75 * Math.Min(Root.RenderSize.Height, System.Windows.SystemParameters.PrimaryScreenHeight);
                    PART_Popup.Width = RenderSize.Width;
                    PART_Popup.Height = Math.Min(settings.Size.Height + 25, PART_Popup.MaxHeight);
                    break;
                case Orientation.Vertical:
                    PART_Popup.MaxWidth = 0.75 * Math.Min(Root.RenderSize.Width, System.Windows.SystemParameters.PrimaryScreenWidth);
                    PART_Popup.MaxHeight = RenderSize.Height;
                    PART_Popup.Width = Math.Min(settings.Size.Width + 7, PART_Popup.MaxWidth);
                    PART_Popup.Height = RenderSize.Height;
                    break;
            }
            DockedWindow window = new DockedWindow(Root, dockContent);
            window.Closing += Window_Closing;
            window.Focused = true;
            window.IsCollapsed = true;
            PART_Popup.Content = window;
            PART_Popup.Tag = dockContent;
            BooleanAnimationUsingKeyFrames booleanAnimation = new BooleanAnimationUsingKeyFrames();
            booleanAnimation.Duration = TimeSpan.FromSeconds(0.1);
            booleanAnimation.KeyFrames.Add(
                new DiscreteBooleanKeyFrame(
                    false, // Target value (KeyValue)
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.0))) // KeyTime
                );
            booleanAnimation.KeyFrames.Add(
                new DiscreteBooleanKeyFrame(
                    true, // Target value (KeyValue)
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.1))) // KeyTime
                );
            PART_Popup.IsOpen = true;
            Root.Focus(dockContent);
            m_timer.Start();
        }
        /// <summary>
        /// Close popup</summary>
        private void ClosePopup()
        {
            PART_Popup.IsOpen = false;
            DockedWindow window = PART_Popup.Content as DockedWindow;			
            if (window != null)
            {
                DockContent content = (DockContent)PART_Popup.Tag;
                window.Closing -= Window_Closing;
                window.Close();
                content.IsVisible = true;
                PART_Popup.Tag = null;
                PART_Popup.Content = null;
            }			
            foreach (SideBarButton button in Items)
            {
                button.IsChecked = false;
            }
        }

        void TabItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            m_lastItemOver = (SideBarButton)sender;
            m_timerTime = DateTime.Now;
            m_timer.Start();
        }

        void TabItem_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Scroll & e.AllowedEffects;
            e.Handled = true;
            m_lastItemOver = (SideBarButton)sender;
            m_timerTime = DateTime.Now;
            m_timer.Start();
        }

        void TabItem_Click(object sender, RoutedEventArgs e)
        {
            m_timerTime = DateTime.Now;
            Timer_Elapsed(m_timer, null);
        }
        /// <summary>
        /// Add new content to side tab</summary>
        /// <param name="content">New dock content</param>
        private void AddContent(DockContent content)
        {
            m_children.Add(content);
            content.Settings.DockState = DockState.Collapsed;
            FrameworkElement fe = CreateHeader(content);
            SideBarButton tabItem = new SideBarButton(TabsPlacement)
            {
                Content = fe,
                Tag = content,
            };
            tabItem.MouseEnter += TabItem_MouseEnter;
            tabItem.AllowDrop = true;
            tabItem.DragEnter += TabItem_DragEnter;
            tabItem.Click += TabItem_Click;
            Items.Add(tabItem);
            content.IsVisible = true;
            content.IsFocused = false;
            content.IsFocusedChanged += DockContent_IsFocusedChanged;
        }

        void DockContent_IsFocusedChanged(object sender, BooleanArgs e)
        {
            if (e.Value)
            {
                DockContent content = (DockContent)sender;
                if (m_children.Contains(content))
                {
                    ShowPopup(content);
                }
            }
            else
            {
                ClosePopup();
            }
        }

        private FrameworkElement CreateHeader(IDockContent content)
        {
            StackPanel header = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal /*this.Orientation*/ };
            if (content.Icon is ImageSource && ((Root.IconVisibility & IconVisibility.SideBar) == IconVisibility.SideBar))
            {
                Image image = new Image()
                {
                    Source = (ImageSource)content.Icon,
                    Width = Root.HeaderIconSize.Width,
                    Height = Root.HeaderIconSize.Height,
                    IsHitTestVisible = false,
                };
                header.Children.Add(image);
            }
            TextBlock tb = new TextBlock() 
            { 
                Text = content.Header, 
                IsHitTestVisible = false, 
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            tb.RenderTransformOrigin = new Point(0.5, 0.5);
            header.Children.Add(tb);
            header.LayoutTransform = new RotateTransform(Orientation == Orientation.Horizontal ? 0 : 90);
            return header;
        }

        /// <summary>
        /// Override that is called after a visual template is applied to the control,
        /// called whenever ApplyTemplate, which builds the current template's visual tree, is called</summary>
        public override void OnApplyTemplate()
        {
            PART_Grid = (Grid)Template.FindName("PART_Grid", this);
            PART_Popup = (ResizablePopup)Template.FindName("PART_Popup", this);
            UpdatePopupProperties();
        }

        /// <summary>
        /// Update properties of PART_Popup, so it appears in proper place</summary>
        private void UpdatePopupProperties()
        {
            if (PART_Popup != null)
            {
                PART_Popup.DockSide = TabsPlacement;
                switch (TabsPlacement)
                {
                    case System.Windows.Controls.Dock.Top:
                        PART_Popup.Placement = PlacementMode.Bottom;
                        break;
                    case System.Windows.Controls.Dock.Bottom:
                        PART_Popup.Placement = PlacementMode.Top;
                        break;
                    case System.Windows.Controls.Dock.Left:
                        PART_Popup.Placement = PlacementMode.Right;
                        break;
                    case System.Windows.Controls.Dock.Right:
                        PART_Popup.Placement = PlacementMode.Left;
                        break;
                }
            }
        }
        /// <summary>
        /// Return screen position and size rectangle for button representing the content</summary>
        /// <param name="content">Content to create rectangle for</param>
        /// <returns>Rectangle containing screen position and size</returns>
        public Rect RectForContent(IDockContent content)
        {
            UpdateLayout();
            Rect rect = new Rect();			
            foreach (SideBarButton button in Items)
            {
                if (button.Tag == content)
                {
                    rect = new Rect(button.PointToScreen(new Point(0, 0)), button.RenderSize);
                    break;
                }
            }
            return rect;
        }

        #region IDockLayout Members

        /// <summary>
        /// Get the root of the hierarchy</summary>
        public DockPanel Root { get; set; }

        /// <summary>
        /// Hit test the position and return content at that position</summary>
        /// <param name="position">Position to test</param>
        /// <returns>Content if hit, null otherwise</returns>
        DockContent IDockLayout.HitTest(Point position)
        {
            return null;
        }

        /// <summary>
        /// Check if the layout contains the content as direct child</summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True iff the content is child of this control</returns>
        public bool HasChild(IDockContent content)
        {
            return m_children.Any(x => x == content);
        }

        /// <summary>
        /// Check if the layout contains the content as child or descendant</summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True iff content is child or descendant</returns>
        public bool HasDescendant(IDockContent content)
        {
            return HasChild(content);
        }

        /// <summary>
        /// Dock the new content next to content</summary>
        /// <param name="nextTo">Dock content to add new content next to</param>
        /// <param name="newContent">New content to be docked</param>
        /// <param name="dockTo">Side of nextTo content where new content should be docked</param>
        public void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
        {
            TabLayout tabLayout = newContent as TabLayout;
            if (tabLayout != null)
            {
                IEnumerator<DockContent> contentEnumerator = tabLayout.Children.GetEnumerator();
                while (contentEnumerator.MoveNext())
                {
                    DockContent content = contentEnumerator.Current;
                    tabLayout.RemoveItem(content);
                    AddContent(content);
                    contentEnumerator = tabLayout.Children.GetEnumerator();
                }
            }
            else
            {
                AddContent((DockContent)newContent);
            }
        }

        /// <summary>
        /// Undock given content</summary>
        /// <param name="content">Content to undock</param>
        public void Undock(IDockContent content)
        {
            ClosePopup();
            DockContent dockContent;
            if ((dockContent = (content as DockContent)) != null)
            {
                dockContent.IsVisible = false;
                dockContent.IsFocusedChanged -= DockContent_IsFocusedChanged;
                m_children.Remove(dockContent);
                foreach (SideBarButton b in Items)
                {
                    if (b.Tag == dockContent)
                    {
                        if (m_lastItemOver == b)
                        {
                            m_timer.Stop();
                            m_lastItemOver = null;
                        }
                        Items.Remove(b);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Undock given child layout</summary>
        /// <param name="child">Child layout to undock</param>
        void IDockLayout.Undock(IDockLayout child)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replace the old layout with new layout child</summary>
        /// <param name="oldLayout">Old layout to be replaced</param>
        /// <param name="newLayout">New layout that replaces old layout</param>
        void IDockLayout.Replace(IDockLayout oldLayout, IDockLayout newLayout)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Close the layout</summary>
        public void Close()
        {
            m_children.Clear();
            Items.Clear();
        }

        /// <summary>
        /// Return the content's parent as an IDockLayout</summary>
        /// <param name="content">The docked content whose parent is requested</param>
        /// <returns>The parent as IDockLayout</returns>
        IDockLayout IDockLayout.FindParentLayout(IDockContent content)
        {
            return (content is DockContent && m_children.Contains((DockContent)content)) ? this : null;
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Reserved and should not be used</summary>
        /// <exception cref="NotImplementedException"> is raised if called</exception>
        /// <returns>XmlSchema describing XML representation of object</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Generate object from its XML representation</summary>
        /// <param name="reader">XmlReader stream from which object is deserialized</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.ReadToFollowing(this.GetType().Name))
            {
                if (reader.ReadToDescendant("Content"))
                {
                    do
                    {
                        String ucid = reader.GetAttribute("UCID");
                        DockContent content = Root.GetContent(ucid);
                        if (content != null)
                        {
                            AddContent(content);
                        }
                    } while (reader.ReadToNextSibling("Content"));
                }
                reader.Read();
            }
        }

        /// <summary>
        /// Convert object into its XML representation</summary>
        /// <param name="writer">XmlWriter stream to which object is serialized</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(this.GetType().Name);
            writer.WriteAttributeString("Side", TabsPlacement.ToString());
            foreach (IDockContent content in m_children)
            {
                writer.WriteStartElement("Content");
                writer.WriteAttributeString("UCID", content.UID);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        #endregion
    }
}
