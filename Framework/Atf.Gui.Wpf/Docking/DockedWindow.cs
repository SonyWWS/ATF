//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Controls.Primitives;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// This class represents a window while it is docked into the hierarchy. It provides the title bar,
    /// close and collapse icons, and can be resized within the hierarchy.</summary>
    public class DockedWindow : ContentControl, IDockLayout, IDockable, IXmlSerializable
    {
        #region Dependency Properties

        public static DependencyProperty DockedContentProperty = DependencyProperty.Register("DockedContent", typeof(TabLayout), typeof(DockedWindow), new UIPropertyMetadata(new PropertyChangedCallback(DockedContentPropertyChanged)));
        public static DependencyProperty FocusedProperty = DependencyProperty.Register("Focused", typeof(bool), typeof(DockedWindow), new FrameworkPropertyMetadata(FocusedPropertyChanged));
        public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(DockedWindow));
        public static DependencyProperty ShowIconProperty = DependencyProperty.Register("ShowIcon", typeof(bool), typeof(DockedWindow));

        #endregion

        #region Public Properties

        public bool ShowIcon
        {
            get { return ((bool)(base.GetValue(DockedWindow.ShowIconProperty))); }
            set { base.SetValue(DockedWindow.ShowIconProperty, value); }
        }
        public TabLayout DockedContent
        {
            get { return ((TabLayout)(base.GetValue(DockedWindow.DockedContentProperty))); }
            private set { base.SetValue(DockedWindow.DockedContentProperty, value); }
        }
        /// <summary>
        /// Collapsed state of the window.
        /// </summary>
        public bool IsCollapsed
        {
            get
            {
                return PART_CollapseButton.IsChecked != null ? (bool)PART_CollapseButton.IsChecked : false;
            }
            set
            {
                m_isCollapsed = value;
                if (PART_CollapseButton != null)
                {
                    PART_CollapseButton.IsChecked = value;
                }
            }
        }
        /// <summary>
        /// Focused state of the window. Only one window in docked hierarchy can be Focused.
        /// </summary>
        public bool Focused
        {
            get { return ((bool)(base.GetValue(DockedWindow.FocusedProperty))); }
            set { base.SetValue(DockedWindow.FocusedProperty, value); }
        }
        /// <summary>
        /// Header text of window
        /// </summary>
        public String Header
        {
            get { return ((String)(base.GetValue(DockedWindow.HeaderProperty))); }
            set { base.SetValue(DockedWindow.HeaderProperty, value); }
        }

        #endregion

        #region Private Properties

        private DockIcon m_dockLeftIcon;
        private DockIcon m_dockRightIcon;
        private DockIcon m_dockTopIcon;
        private DockIcon m_dockBottomIcon;
        private DockIcon m_dockTabIcon;
        private FrameworkElement m_dockPreviewShape;
        private DockTo? m_dockPreview;
        private Point m_mouseClickPosition;
        private bool m_mouseClickInside;
        private Button PART_CloseButton;
        private ToggleButton PART_CollapseButton;
        private bool m_isCollapsed;

        #endregion

        #region Events

        /// <summary>
        /// Event triggered when the window is being closed
        /// </summary>
        public event EventHandler Closing;

        #endregion

        /// <summary>
        /// Static constructor that overrides the style that is used by styling.
        /// </summary>
        static DockedWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockedWindow), new FrameworkPropertyMetadata(typeof(DockedWindow)));
        }		
        /// <summary>
        /// Constructor used when the window is created as a consequence of user interaction with the ui.
        /// </summary>
        /// <param name="dockPanel">Parent dockpanel</param>
        /// <param name="content">Dockable content</param>
        public DockedWindow(DockPanel dockPanel, IDockContent content)
        {
            Root = dockPanel;
            DockedContent = content as TabLayout;
            if (DockedContent == null)
            {
                DockedContent = new TabLayout(Root);
                DockedContent.Dock(null, content, DockTo.Center);
            }
            Content = DockedContent;
            ShowIcon = (DockedContent.Icon != null && ((Root.IconVisibility & IconVisibility.Header) == IconVisibility.SideBar));
            DockedContent.Closing += new ContentClosedEvent(Content_Closing);
            
            PreviewMouseDown += new MouseButtonEventHandler(DockedWindow_PreviewMouseDown);
            Focused = ((IDockContent)DockedContent).IsFocused;
        }
        /// <summary>
        /// Constructor used when deserialising from settings
        /// </summary>
        /// <param name="dockPanel">Parent dock panel</param>
        /// <param name="reader">Xml source</param>
        public DockedWindow(DockPanel dockPanel, XmlReader reader)
        {
            Root = dockPanel;
            ReadXml(reader);
            ShowIcon = (DockedContent.Icon != null && ((Root.IconVisibility & IconVisibility.Header) == IconVisibility.SideBar));
            DockedContent.Closing += new ContentClosedEvent(Content_Closing);
            PreviewMouseDown += new MouseButtonEventHandler(DockedWindow_PreviewMouseDown);
            Focused = ((IDockContent)DockedContent).IsFocused;
        }

        private static void FocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var content = ((DockedWindow)d).DockedContent;

            if (content != null)
            {
                if ((bool)e.NewValue)
                {
                    content.Activate();
                }
                else
                {
                    content.Deactivate();
                }
            }
        }
        
        void DockedWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Root.Focus((IDockContent)DockedContent.SelectedItem);
        }

        void Content_Closing(object sender, ContentClosedEventArgs args)
        {
            if (Parent is IDockLayout)
            {
                ((IDockLayout)Parent).Undock((IDockContent)DockedContent);
            }
            Close();
        }

        public IDockContent GetActiveContent()
        {
            return DockedContent != null ? DockedContent.GetActiveContent() : null;
        }

        /// <summary>
        /// Override that is called after a visual template is applied to the control
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            FrameworkElement titleBar = (FrameworkElement)Template.FindName("PART_TitleBar", this);
            titleBar.MouseDown += new MouseButtonEventHandler(TitleBarMouseDown);
            titleBar.MouseMove += new MouseEventHandler(TitleBarMouseMove);

            PART_CloseButton = (Button)Template.FindName("PART_CloseButton", this);
            PART_CloseButton.Click += new RoutedEventHandler(CloseButton_Click);

            PART_CollapseButton = (ToggleButton)Template.FindName("PART_CollapseButton", this);
            PART_CollapseButton.Click += new RoutedEventHandler(CollapseButton_Click);
            PART_CollapseButton.IsChecked = m_isCollapsed;
        }

        void CollapseButton_Click(object sender, RoutedEventArgs e)
        {
            if (m_isCollapsed)
            {
                Root.UnCollapse(DockedContent);
            }
            else
            {
                Root.Collapse(DockedContent);
            }
        }

        void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Closing != null)
            {
                Closing(this, EventArgs.Empty);
            }
            if (DockedContent != null)
            {
                DockedContent.OnClose(this, new ContentClosedEventArgs(ContentToClose.Current));
            }
        }

        void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !IsCollapsed)
            {
                m_mouseClickInside = true;
                m_mouseClickPosition = e.GetPosition(this);
                e.Handled = true;
            }
        }

        private void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && m_mouseClickInside && !IsCollapsed)
            {
                Point currentPosition = e.GetPosition(this);
                Vector delta = m_mouseClickPosition - currentPosition;
                if ((Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance) || (Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    m_mouseClickInside = false;
                    e.Handled = true;
                    Root.Drag(this, DockedContent);
                }
            }
            else
            {
                m_mouseClickInside = false;
            }
        }

        public static void DockedContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            DockedWindow window = null;
            if ((window = sender as DockedWindow) != null)
            {
                IDockContent content;
                if ((content = args.OldValue as IDockContent) != null)
                {
                    content.IsFocusedChanged -= window.DockContent_IsFocusChanged;
                }
                if ((content = args.NewValue as IDockContent) != null)
                {
                    content.IsFocusedChanged += window.DockContent_IsFocusChanged;
                }
            }
        }

        void DockContent_IsFocusChanged(object sender, BooleanArgs e)
        {
            Focused = ((IDockContent)DockedContent).IsFocused;
        }

        #region IDockLayout Members

        public DockPanel Root
        {
            get; private set;
        }

        public DockContent HitTest(Point position)
        {
            if (new Rect(0, 20, ActualWidth, ActualHeight).Contains(PointFromScreen(position)))
            {
                return DockedContent.Children[0];
            }
            return null;
        }

        public bool HasChild(IDockContent content)
        {
            if (DockedContent == content || DockedContent.Children.Any(x => x == content))
            {
                return true;
            }
            return false;
        }

        public bool HasDescendant(IDockContent content)
        {
            return HasChild(content);
        }

        public void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
        {
            DockedContent.Dock(nextTo as DockContent, newContent, dockTo);
        }

        public void Undock(IDockContent content)
        {
            if (DockedContent == content)
            {
                Undock((IDockLayout)content);
            }
            else if(DockedContent != null)
            {
                DockedContent.Undock(content);
            }
        }

        public void Undock(IDockLayout child)
        {
            if (DockedContent == child)
            {
                DockedContent = null;
                Content = null;
                if (Parent is IDockLayout)
                {
                    ((IDockLayout)Parent).Undock(this);
                }
            }
        }

        public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            Root.Focus(null);
            if (DockedContent != null)
            {
                DockedContent.Close();
                Content = null;
                DockedContent = null;
            }
        }

        IDockLayout IDockLayout.FindParentLayout(IDockContent content)
        {
            return ((IDockLayout)DockedContent).FindParentLayout(content);
        }

        #endregion

        #region IDockable Members

        public DockTo? DockPreview
        {
            get { return m_dockPreview; }
        }

        public void DockDragEnter(object sender, DockDragDropEventArgs e)
        {
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);
            int space = (int)Root.DockIconSize.Width / 4;
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("/Atf.Gui.Wpf;component/Resources/DockIcons.xaml", UriKind.Relative);
            if (m_dockLeftIcon == null)
            {
                m_dockLeftIcon = new DockIcon((Style)rd["DockLeftIcon"], Root.DockIconSize);
                m_dockRightIcon = new DockIcon((Style)rd["DockRightIcon"], Root.DockIconSize);
                m_dockTopIcon = new DockIcon((Style)rd["DockTopIcon"], Root.DockIconSize);
                m_dockBottomIcon = new DockIcon((Style)rd["DockBottomIcon"], Root.DockIconSize);
                m_dockTabIcon = new DockIcon((Style)rd["DockTabIcon"], Root.DockIconSize);
            }
            Window owner = Window.GetWindow(this);
            Point offset = ((UIElement)Root).PointFromScreen(PointToScreen(new Point(0, 0)));
            m_dockLeftIcon.Offset = new Point(offset.X + center.X - Root.DockIconSize.Width / 2 - Root.DockIconSize.Width - space, offset.Y + center.Y - Root.DockIconSize.Height / 2);
            m_dockRightIcon.Offset = new Point(offset.X + center.X + Root.DockIconSize.Width / 2 + space, offset.Y + center.Y - Root.DockIconSize.Height / 2);
            m_dockTopIcon.Offset = new Point(offset.X + center.X - Root.DockIconSize.Width / 2, offset.Y + center.Y - Root.DockIconSize.Height / 2 - Root.DockIconSize.Height - space);
            m_dockBottomIcon.Offset = new Point(offset.X + center.X - Root.DockIconSize.Width / 2, offset.Y + center.Y + Root.DockIconSize.Height / 2 + space);
            m_dockTabIcon.Offset = new Point(offset.X + center.X - Root.DockIconSize.Width / 2, offset.Y + center.Y - Root.DockIconSize.Height / 2);

            DocklingsWindow dockIconLayer = Root.DockIconsLayer;
            dockIconLayer.AddChild(m_dockLeftIcon);
            dockIconLayer.AddChild(m_dockRightIcon);
            dockIconLayer.AddChild(m_dockTopIcon);
            dockIconLayer.AddChild(m_dockBottomIcon);
            dockIconLayer.AddChild(m_dockTabIcon);
        }

        public void DockDragOver(object sender, DockDragDropEventArgs e)
        {
            DockTo? previousDockPreview = m_dockPreview;
            m_dockPreview = null;
            Point pos = e.MouseEventArgs.GetPosition(Root);

            m_dockLeftIcon.Highlight = m_dockLeftIcon.HitTest(pos);
            m_dockRightIcon.Highlight = m_dockRightIcon.HitTest(pos);
            m_dockTopIcon.Highlight = m_dockTopIcon.HitTest(pos);
            m_dockBottomIcon.Highlight = m_dockBottomIcon.HitTest(pos);
            m_dockTabIcon.Highlight = m_dockTabIcon.HitTest(pos);

            m_dockPreview = m_dockLeftIcon.Highlight ? DockTo.Left : m_dockPreview;
            m_dockPreview = m_dockRightIcon.Highlight ? DockTo.Right : m_dockPreview;
            m_dockPreview = m_dockTopIcon.Highlight ? DockTo.Top : m_dockPreview;
            m_dockPreview = m_dockBottomIcon.Highlight ? DockTo.Bottom : m_dockPreview;
            m_dockPreview = m_dockTabIcon.Highlight ? DockTo.Center : m_dockPreview;

            if (m_dockPreview != null)
            {
                if (previousDockPreview != m_dockPreview)
                {
                    Root.DockIconsLayer.RemoveChild(m_dockPreviewShape);
                    m_dockPreviewShape = null;
                    Window owner = Window.GetWindow(this);
                    Rectangle rect = new Rectangle();
                    rect.Fill = Brushes.RoyalBlue;
                    rect.Opacity = 0.3;
                    FrameworkElement fe = rect;
                    double space = 2;
                    Point p = Root.PointFromScreen(PointToScreen(new Point(space, space)));
                    ContentSettings contentSettings = (e.Content is TabLayout) ? ((TabLayout)e.Content).Children[0].Settings : ((DockContent)e.Content).Settings;
                    double width = Math.Max(Math.Min(contentSettings.Size.Width, ActualWidth / 2), ActualWidth / 5);
                    double height = Math.Max(Math.Min(contentSettings.Size.Height, ActualHeight / 2), ActualHeight / 5);
                    double ratioWidth = width / ActualWidth;
                    double ratioHeight = height / ActualHeight;
                    switch (m_dockPreview)
                    {
                        case DockTo.Left:
                            fe.Width = ActualWidth * ratioWidth - space * 2;
                            fe.Height = ActualHeight - space * 2;
                            Canvas.SetLeft(fe, p.X);
                            Canvas.SetTop(fe, p.Y);
                            break;
                        case DockTo.Right:
                            fe.Width = ActualWidth * ratioWidth - space * 2;
                            fe.Height = ActualHeight - space * 2;
                            Canvas.SetLeft(fe, p.X + ActualWidth * (1 - ratioWidth));
                            Canvas.SetTop(fe, p.Y);
                            break;
                        case DockTo.Top:
                            fe.Width = ActualWidth - space * 2;
                            fe.Height = ActualHeight * ratioHeight - space * 2;
                            Canvas.SetLeft(fe, p.X);
                            Canvas.SetTop(fe, p.Y);
                            break;
                        case DockTo.Bottom:
                            fe.Width = ActualWidth - space * 2;
                            fe.Height = ActualHeight * ratioHeight - space * 2;
                            Canvas.SetLeft(fe, p.X);
                            Canvas.SetTop(fe, p.Y + ActualHeight * (1 - ratioHeight));
                            break;
                        case DockTo.Center:
                            Canvas c = new Canvas();
                            c.SnapsToDevicePixels = true;
                            c.Width = ActualWidth;
                            c.Height = ActualHeight;
                            Canvas.SetLeft(c, p.X);
                            Canvas.SetTop(c, p.Y);
                            fe.Width = ActualWidth - space * 2;
                            fe.Height = ActualHeight - 20 - space * 2;
                            Canvas.SetLeft(fe, 0);
                            Canvas.SetTop(fe, 0);
                            c.Children.Add(fe);

                            rect = new Rectangle();
                            rect.Fill = Brushes.RoyalBlue;
                            rect.Opacity = 0.3;
                            rect.Width = Math.Min(ActualWidth / 4, 50);
                            rect.Height = 20;
                            Canvas.SetLeft(rect, 0);
                            Canvas.SetTop(rect, ActualHeight - 20 - space * 2);
                            c.Children.Add(rect);
                            fe = c;
                            break;
                    }
                    Root.DockIconsLayer.InsertChild(0, fe);
                    m_dockPreviewShape = fe;
                }
            }
            else
            {
                if (m_dockPreviewShape != null)
                {
                    Root.DockIconsLayer.RemoveChild(m_dockPreviewShape);
                    m_dockPreviewShape = null;
                }
            }
        }

        public void DockDragLeave(object sender, DockDragDropEventArgs e)
        {
            DocklingsWindow dockIconLayer = Root.DockIconsLayer;
            dockIconLayer.RemoveChild(m_dockLeftIcon);
            dockIconLayer.RemoveChild(m_dockRightIcon);
            dockIconLayer.RemoveChild(m_dockTopIcon);
            dockIconLayer.RemoveChild(m_dockBottomIcon);
            dockIconLayer.RemoveChild(m_dockTabIcon);
            if (m_dockPreviewShape != null)
            {
                Root.DockIconsLayer.RemoveChild(m_dockPreviewShape);
                m_dockPreviewShape = null;
            }
            dockIconLayer.CloseIfEmpty();
        }

        public void DockDrop(object sender, DockDragDropEventArgs e)
        {
            DockPanel parent = Root;
            DockTo dockTo = (DockTo)m_dockPreview;
            if (e.Content is TabLayout)
            {
                foreach (DockContent subContent in ((TabLayout)e.Content).Children)
                {
                    ContentSettings contentSettings = subContent.Settings;
                    contentSettings.DockState = DockState.Docked;
                }
            }
            else
            {
                ContentSettings contentSettings = ((DockContent)e.Content).Settings;
                contentSettings.DockState = DockState.Docked;
            }
            switch (dockTo)
            {
                case DockTo.Left:
                case DockTo.Right:
                case DockTo.Top:
                case DockTo.Bottom:
                    ((IDockLayout)Parent).Dock(DockedContent, e.Content, dockTo);
                    break;
                case DockTo.Center:
                    Dock(null, e.Content, dockTo);
                    break;
            }
            parent.CheckConsistency();
        }

        #endregion

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.ReadToFollowing(this.GetType().Name))
            {
                reader.ReadStartElement();
                if (reader.LocalName == typeof(TabLayout).Name || reader.LocalName == "MultiContent") // MultiContent is old name and is used here for compatibility with old saved layouts
                {
                    DockedContent = new TabLayout(Root, reader.ReadSubtree());
                    Content = DockedContent;
                    reader.ReadEndElement();
                }
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(this.GetType().Name);
            DockedContent.WriteXml(writer);
            writer.WriteEndElement();
        }

        #endregion
    }
}
