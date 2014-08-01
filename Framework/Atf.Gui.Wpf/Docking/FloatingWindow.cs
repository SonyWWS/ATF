//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Xml.Serialization;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Data;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// This window holds content when it is undocked.
    /// </summary>
    public class FloatingWindow : Window, IDockLayout, IDockable, IXmlSerializable
    {
        public static DependencyProperty DockedContentProperty = DependencyProperty.Register("DockedContent", typeof(TabLayout), typeof(FloatingWindow));
        public TabLayout DockedContent
        {
            get { return ((TabLayout)(base.GetValue(FloatingWindow.DockedContentProperty))); }
            set { base.SetValue(FloatingWindow.DockedContentProperty, value); }
        }

        private const int WM_SYSCOMMAND = 0x0112;
        private const int SC_MOVE = 0xf010;

        private Point m_lastMousePos;
        private List<IDockable> m_dockOver;
        private bool m_validPosition;
        private bool m_dragging;

        private DockIcon m_dockTabIcon;
        private FrameworkElement m_dockPreviewShape;
        private DocklingsWindow m_docklings;
        /// <summary>
        /// will return the dock icons layer for this window.
        /// </summary>
        internal DocklingsWindow DockIconsLayer
        {
            get
            {
                if (m_docklings == null)
                {
                    m_docklings = new DocklingsWindow(this);
                    m_docklings.Owner = Window.GetWindow(this);
                    m_docklings.Closing += new System.ComponentModel.CancelEventHandler(Docklings_Closing);
                    m_docklings.Show();
                }
                return m_docklings;
            }
        }

        private FloatingWindow(DockPanel dockPanel)
        {
            Root = dockPanel;
            ShowInTaskbar = false;
            WindowStyle = WindowStyle.ToolWindow;
            WindowStartupLocation = WindowStartupLocation.Manual;
            m_dockOver = new List<IDockable>();
            Loaded += new RoutedEventHandler(FloatingWindow_Loaded);
            Closing += new System.ComponentModel.CancelEventHandler(FloatingWindow_Closing);
            MouseMove += new MouseEventHandler(FloatingWindow_MouseMove);
            MouseUp += new MouseButtonEventHandler(FloatingWindow_MouseUp);
            MouseLeave += new MouseEventHandler(FloatingWindow_MouseLeave);
            Activated += new EventHandler(FloatingWindow_Activated);
            LocationChanged += new EventHandler(FloatingWindow_LocationChanged);
            SizeChanged += new SizeChangedEventHandler(FloatingWindow_SizeChanged);
        }
        static FloatingWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatingWindow), new FrameworkPropertyMetadata(typeof(FloatingWindow)));
        }	
        /// <summary>
        /// Constructor used when window is created as consequence to user interaction.
        /// </summary>
        /// <param name="root">Parent dock panel</param>
        /// <param name="content">Content to host</param>
        /// <param name="origin">Origin, position on screen</param>
        /// <param name="size">Size of window</param>
        public FloatingWindow(DockPanel root, IDockContent content, Point origin, Size size)
            : this(root)
        {
            Left = origin.X;
            Top = origin.Y;
            Width = size.Width;
            Height = size.Height;
            if (content is TabLayout)
            {
                DockedContent = (TabLayout)content;
            }
            else
            {
                DockedContent = new TabLayout(Root);
                DockedContent.Dock(null, content, DockTo.Center);
            }
            foreach (DockContent subContent in DockedContent.Children)
            {
                subContent.Settings.DockState = DockState.Floating;				
            }
            Content = DockedContent;

            Binding b = new Binding("Header");
            b.Source = DockedContent;
            SetBinding(Window.TitleProperty, b);
        }
        /// <summary>
        /// Constructor used when deserializing
        /// </summary>
        /// <param name="root">Parent dock panel</param>
        /// <param name="reader">Source xml</param>
        public FloatingWindow(DockPanel root, XmlReader reader)
            : this(root)
        {
            ReadXml(reader);
            Content = DockedContent;
            Title = DockedContent.Header;

            Binding b = new Binding("Header");
            b.Source = DockedContent;
            SetBinding(Window.TitleProperty, b);
        }

        public IDockContent GetActiveContent()
        {
            return DockedContent != null ? DockedContent.GetActiveContent() : null;
        }

        void Docklings_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_docklings = null;
        }

        void FloatingWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DockedContent != null)
            {
                foreach (DockContent content in DockedContent.Children)
                {					
                    content.Settings.Size = new Size(Width, Height);
                }
            }
        }

        void FloatingWindow_LocationChanged(object sender, EventArgs e)
        {
            if (DockedContent != null)
            {
                foreach (DockContent content in DockedContent.Children)
                {					
                    content.Settings.Location = new Point(Left, Top);
                }
            }
        }

        void FloatingWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }

        void FloatingWindow_Activated(object sender, EventArgs e)
        {
            if (DockedContent != null && DockedContent.SelectedItem != null)
            {
                Root.Focus((IDockContent)DockedContent.SelectedItem);
            }
        }

        void FloatingWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DockedContent != null)
            {
                DockedContent.OnClose(this, new ContentClosedEventArgs(ContentToClose.All));
                DockedContent.Close();
                Content = null;
                DockedContent = null;
            }
        }

        void FloatingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND)
            {
                int systemCommand = (int)wParam;
                if ((systemCommand & 0xfff0) == SC_MOVE)
                {
                    handled = true;
                    Dispatcher.BeginInvoke(new EventHandler(MouseTitleDown),new object[]{this, EventArgs.Empty});
                }
            }
            return IntPtr.Zero;
        }

        private void MouseTitleDown(object o, EventArgs args)
        {
            m_lastMousePos = Mouse.GetPosition(this);
            m_validPosition = false;
            m_dragging = false;
            CaptureMouse();
        }

        void FloatingWindow_MouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);
            if (!m_validPosition)
            {
                // this is workaround for not using pInvoke for getting the position. 
                // when the draging starts, the e.GetPosition doesn't give proper values, but instead returns (0,0)
                if(pos != new Point(0, 0))
                {
                    m_lastMousePos = e.GetPosition(Owner);
                    m_validPosition = true;
                }
            }
            if (IsMouseCaptured && m_validPosition)
            {
                Topmost = true;
                pos = e.GetPosition(Owner);
                if (!m_dragging)
                {
                    if ((m_lastMousePos - pos).Length > 2)
                    {
                        m_dragging = true;
                    }
                }
                if(m_dragging)
                {
                    Left = Left + pos.X - m_lastMousePos.X;
                    Top = Top + pos.Y - m_lastMousePos.Y;
                    m_lastMousePos = pos;
                    List<IDockable> dockOver = Root.FindElementsAt(e);
                    DockDragDropEventArgs args = new DockDragDropEventArgs(DockedContent, e);
                    foreach (IDockable dockable in m_dockOver)
                    {
                        if (!dockOver.Contains(dockable))
                        {
                            dockable.DockDragLeave(this, args);
                        }
                    }
                    foreach (IDockable dockable in dockOver)
                    {
                        if (!m_dockOver.Contains(dockable))
                        {
                            dockable.DockDragEnter(this, args);
                        }
                    }
                    foreach (IDockable dockable in dockOver)
                    {
                        dockable.DockDragOver(this, args);
                    }
                    m_dockOver = dockOver;
                }
            }
        }

        void FloatingWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                Topmost = false;
                m_validPosition = false;
                m_dragging = false;
                bool dropped = false;
                DockDragDropEventArgs args = new DockDragDropEventArgs(DockedContent, e);
                foreach (IDockable dockable in m_dockOver)
                {
                    dockable.DockDragLeave(this, args);
                    if (!dropped && dockable.DockPreview != null)
                    {
                        Content = null;
                        DockedContent = null;
                        dropped = true;
                        dockable.DockDrop(this, args);
                    }					
                }
                m_dockOver = new List<IDockable>();
                if (dropped)
                {
                    Owner.Focus();
                    Owner.Activate();
                    Close();
                }
                else
                {
                    Focus();
                }
            }
        }

        #region IDockLayout Members

        public DockPanel Root
        {
            get; private set;
        }

        public DockContent HitTest(Point position)
        {
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
            if (content is DockContent)
            {
                DockedContent.RemoveItem((DockContent)content);
                if (DockedContent.Children.Count == 0)
                {
                    Close();
                }
            }
        }

        public void Undock(IDockLayout child)
        {
            throw new NotImplementedException();
        }

        public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
        {
            throw new NotImplementedException();
        }

        public void Drag(IDockContent content)
        {
            Root.Drag(this, content);
        }

        IDockLayout IDockLayout.FindParentLayout(IDockContent content)
        {
            return ((IDockLayout)DockedContent).FindParentLayout(content);
        }

        #endregion

        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.ReadToFollowing(this.GetType().Name))
            {
                reader.ReadStartElement(this.GetType().Name);
                if (reader.LocalName == typeof(TabLayout).Name || reader.LocalName == "MultiContent") // MultiContent is old name and is used here for compatibility with old saved layouts
                {
                    DockedContent = new TabLayout(Root, reader.ReadSubtree());
                    if (DockedContent.Children.Count > 0)
                    {
                        ContentSettings contentSettings = DockedContent.Children[0].Settings;
                        Left = contentSettings.Location.X;
                        Top = contentSettings.Location.Y;
                        Width = contentSettings.Size.Width;
                        Height = contentSettings.Size.Height;

                        Left = Math.Max(0, Math.Min(Left, SystemParameters.VirtualScreenWidth - Width));
                        Top = Math.Max(0, Math.Min(Top, SystemParameters.VirtualScreenHeight - Height));
                        Width = Math.Max(Math.Min(Width, SystemParameters.VirtualScreenWidth - Left), 100);
                        Height = Math.Max(Math.Min(Height, SystemParameters.VirtualScreenHeight - Top), 100);

                        reader.ReadEndElement();
                    }
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

        #region IDockable Members

        public void DockDragEnter(object sender, DockDragDropEventArgs e)
        {
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("/Atf.Gui.Wpf;component/Resources/DockIcons.xaml", UriKind.Relative);
            if (m_dockTabIcon == null)
            {
                m_dockTabIcon = new DockIcon((Style)rd["DockTabIcon"], Root.DockIconSize);
            }
            m_dockTabIcon.Offset = new Point(center.X - Root.DockIconSize.Width / 2, center.Y - Root.DockIconSize.Height / 2);
            m_dockTabIcon.Highlight = false;
            DockIconsLayer.AddChild(m_dockTabIcon);
        }

        public void DockDragOver(object sender, DockDragDropEventArgs e)
        {
            Point pos = e.MouseEventArgs.GetPosition(this);
            bool b = m_dockTabIcon.HitTest(pos);
            if (b && !m_dockTabIcon.Highlight)
            {
                m_dockTabIcon.Highlight = b;
                DockIconsLayer.RemoveChild(m_dockPreviewShape);
                m_dockPreviewShape = null;
                Window owner = Window.GetWindow(this);
                Rectangle rect = new Rectangle();
                rect.Fill = Brushes.RoyalBlue;
                rect.Opacity = 0.3;
                m_dockPreviewShape = rect;
                double space = 2;
                Point p = PointFromScreen(PointToScreen(new Point(space, space)));
                Canvas c = new Canvas();
                c.SnapsToDevicePixels = true;
                c.Width = DockedContent.ActualWidth;
                c.Height = DockedContent.ActualHeight;
                Canvas.SetLeft(c, p.X);
                Canvas.SetTop(c, p.Y);
                m_dockPreviewShape.Width = DockedContent.ActualWidth - space * 2;
                m_dockPreviewShape.Height = DockedContent.ActualHeight - 20 - space * 2;
                Canvas.SetLeft(m_dockPreviewShape, 0);
                Canvas.SetTop(m_dockPreviewShape, 0);
                c.Children.Add(m_dockPreviewShape);

                rect = new Rectangle();
                rect.Fill = Brushes.RoyalBlue;
                rect.Opacity = 0.3;
                rect.Width = Math.Min(DockedContent.ActualWidth / 4, 50);
                rect.Height = 20;
                Canvas.SetLeft(rect, 0);
                Canvas.SetTop(rect, DockedContent.ActualHeight - 20 - space * 2);
                c.Children.Add(rect);
                m_dockPreviewShape = c;

                DockIconsLayer.InsertChild(0, m_dockPreviewShape);
            }
            else if(!b)
            {
                if (m_dockPreviewShape != null)
                {
                    DockIconsLayer.RemoveChild(m_dockPreviewShape);
                    m_dockPreviewShape = null;
                }
                m_dockTabIcon.Highlight = false;				
            }
        }

        public void DockDragLeave(object sender, DockDragDropEventArgs e)
        {
            DockIconsLayer.RemoveChild(m_dockTabIcon);
            if (m_dockPreviewShape != null)
            {
                DockIconsLayer.RemoveChild(m_dockPreviewShape);
                m_dockPreviewShape = null;				
            }
            DockIconsLayer.CloseIfEmpty();
        }

        public void DockDrop(object sender, DockDragDropEventArgs e)
        {
            if (e.Content is TabLayout)
            {
                foreach (DockContent subContent in ((TabLayout)e.Content).Children)
                {
                    subContent.Settings.DockState = DockState.Floating;
                }
            }
            else
            {
                ((DockContent)e.Content).Settings.DockState = DockState.Floating;
            }
            Dock(null, e.Content, DockTo.Center);
        }

        public DockTo? DockPreview
        {
            get
            {
                DockTo? dockPreview = null;
                if (m_dockTabIcon.Highlight)
                {
                    dockPreview = DockTo.Center;
                }
                return dockPreview;
            }
        }

        #endregion
    }
}
