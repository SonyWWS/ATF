//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Sce.Atf.Wpf.Docking.Automation;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// Docking icons visibility regions enumeration</summary>
    public enum IconVisibility
    {
        None = 0x0,
        Header = 0x1,
        SideBar = 0x2,
        Tab = 0x4,
        All = IconVisibility.Header | IconVisibility.SideBar | IconVisibility.Tab
    }

    /// <summary>
    /// DockPanel is the root class that provides docking/undocking/collapsing windows and other things</summary>
    public class DockPanel : Control, IDockLayout, IDockable
    {
        /// <summary>
        /// Grid splitter style resource key</summary>
        public static ComponentResourceKey GridSplitterStyleKey = new ComponentResourceKey(typeof(DockPanel), "GridSplitterStyleKey");

        #region Dependency Properties

        /// <summary>
        /// IconVisibility DependencyProperty</summary>
        public static DependencyProperty IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(IconVisibility), typeof(DockPanel));
        /// <summary>
        /// HeaderIconSize DependencyProperty</summary>
        public static DependencyProperty HeaderIconSizeProperty = DependencyProperty.Register("HeaderIconSize", typeof(Size), typeof(DockPanel));
        /// <summary>
        /// RegisteredContents DependencyProperty</summary>
        public static DependencyProperty RegisteredContentsProperty = DependencyProperty.Register("RegisteredContents", typeof(ObservableCollection<IDockContent>), typeof(DockPanel));
        /// <summary>
        /// Get or set RegisteredContents DependencyProperty</summary>
        public ObservableCollection<IDockContent> RegisteredContents
        {
            get { return ((ObservableCollection<IDockContent>)(base.GetValue(DockPanel.RegisteredContentsProperty))); }
            set { base.SetValue(DockPanel.RegisteredContentsProperty, value); }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get or set HeaderIconSize DependencyProperty</summary>
        public Size HeaderIconSize
        {
            get { return ((Size)(base.GetValue(DockPanel.HeaderIconSizeProperty))); }
            set { base.SetValue(DockPanel.HeaderIconSizeProperty, value); }
        }
        /// <summary>
        /// Get or set IconVisibility DependencyProperty</summary>
        public IconVisibility IconVisibility
        {
            get { return ((IconVisibility)(base.GetValue(DockPanel.IconVisibilityProperty))); }
            set { base.SetValue(DockPanel.IconVisibilityProperty, value); }
        }

        #endregion

        private ContentPresenter PART_MainPanel { get; set; }
        private SidePopup PART_LeftCollapsePanel { get; set; }
        private SidePopup PART_TopCollapsePanel { get; set; }
        private SidePopup PART_RightCollapsePanel { get; set; }
        private SidePopup PART_BottomCollapsePanel { get; set; }
        private DockIconsLayer m_dockIconsLayer;
        private XmlReader m_layoutToApply;
        private bool m_templateApplied;
        private bool m_layoutApplied;

        /// <summary>
        /// Get icon layer window for the dockpanel</summary>
        internal DockIconsLayer DockIconsLayer
        {
            get
            {
                if (m_dockIconsLayer == null)
                {
                    m_dockIconsLayer = new DockIconsLayer(this);
                    m_dockIconsLayer.Owner = Window.GetWindow(this);
                    m_dockIconsLayer.Closing += DockIconsLayer_Closing;
                    m_dockIconsLayer.Show();
                }
                return m_dockIconsLayer;
            }
        }
        private DockIcon m_dockLeftIcon;
        private DockIcon m_dockRightIcon;
        private DockIcon m_dockTopIcon;
        private DockIcon m_dockBottomIcon;
        private DockIcon m_dockTabIcon;
        private FrameworkElement m_dockPreviewShape;
        private DockTo? m_dockPreview;
        private DockContent m_lastFocusedContent;
        private Dictionary<String, DockContent> m_registeredContents;
        private List<FloatingWindow> m_windows;

        private GridLayout GridLayout 
        { 
            get 
            { 
                return PART_MainPanel.Content as GridLayout; 
            } 
            set 
            { 
                PART_MainPanel.Content = value; 
            } 
        }

        /// <summary>
        /// Get dock icon size</summary>
        internal Size DockIconSize { get; private set; }

        /// <summary>
        /// Static constructor</summary>
        static DockPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockPanel), new FrameworkPropertyMetadata(typeof(DockPanel)));
        }		

        /// <summary>
        /// Default constructor</summary>
        public DockPanel()
        {
            IconVisibility = IconVisibility.All;
            HeaderIconSize = new Size(16, 16);
            DockIconSize = new Size(32, 32);
            Focusable = true;
            m_registeredContents = new Dictionary<String, DockContent>();
            RegisteredContents = new ObservableCollection<IDockContent>();
            m_windows = new List<FloatingWindow>();
            Loaded += DockPanel_Loaded;
        }

        /// <summary>
        /// Constructor with dock icon size</summary>
        /// <param name="dockIconSize">Dock icon size</param>
        public DockPanel(double dockIconSize)
            : base()
        {
            DockIconSize = new Size(dockIconSize, dockIconSize);
        }

        void DockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Window wnd = Window.GetWindow(this);
            if (wnd != null)
            {
                wnd.Activated += OwnerWindowActivated;
            }
            if (m_templateApplied && !m_layoutApplied)
            {
                ApplyLayout(m_layoutToApply);
            }
        }

        void DockIconsLayer_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            m_dockIconsLayer = null;
        }

        /// <summary>
        /// Return focused dock content</summary>
        /// <returns>Focused dock content; may be null</returns>
        public IDockContent GetActiveContent()
        {
            if (!IsLoaded)
            {
                return null;
            }
            if (m_lastFocusedContent != null && m_lastFocusedContent.IsFocused)
            {
                return m_lastFocusedContent;
            }
            return null;
        }

        /// <summary>
        /// Test if given content is visible</summary>
        /// <param name="content">Content to check</param>
        /// <returns>Whether content is visible</returns>
        public bool IsContentVisible(IDockContent content)
        {
            if (!IsLoaded)
            {
                return false;
            }
            return ((IDockLayout)this).HasDescendant(content);
        }

        /// <summary>
        /// Override that is called after a visual template is applied to the control,
        /// called whenever ApplyTemplate, which builds the current template's visual tree, is called</summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PART_MainPanel = (ContentPresenter)Template.FindName("PART_MainPanel", this);

            PART_LeftCollapsePanel = (SidePopup)Template.FindName("PART_LeftCollapsePanel", this);
            PART_LeftCollapsePanel.Root = this;
            PART_LeftCollapsePanel.TabsPlacement = System.Windows.Controls.Dock.Left;

            PART_TopCollapsePanel = (SidePopup)Template.FindName("PART_TopCollapsePanel", this);
            PART_TopCollapsePanel.Root = this;
            PART_TopCollapsePanel.TabsPlacement = System.Windows.Controls.Dock.Top;

            PART_RightCollapsePanel = (SidePopup)Template.FindName("PART_RightCollapsePanel", this);
            PART_RightCollapsePanel.Root = this;
            PART_RightCollapsePanel.TabsPlacement = System.Windows.Controls.Dock.Right;

            PART_BottomCollapsePanel = (SidePopup)Template.FindName("PART_BottomCollapsePanel", this);
            PART_BottomCollapsePanel.Root = this;
            PART_BottomCollapsePanel.TabsPlacement = System.Windows.Controls.Dock.Bottom;
            m_templateApplied = true;
        }
        /// <summary>
        /// Check consistency of all children. Called usually when child window is removed,
        /// to maintain the tree structure, without having nodes with no children.</summary>
        internal void CheckConsistency()
        {
            if (GridLayout != null)
            {				
                GridLayout.CheckConsistency();
            }
        }
        /// <summary>
        /// Return content of given type. Since each content type can be currently registered only
        /// once, it is one to one projection.</summary>
        /// <param name="uniqueName">Content type name</param>
        /// <returns>Reference for the content</returns>
        internal DockContent GetContent(String uniqueName)
        {
            DockContent content = null;
            if (m_registeredContents.TryGetValue(uniqueName, out content))
            {
                return content;
            }
            return null;
        }
        /// <summary>
        /// Give focus to layout and unfocus the last one</summary>
        /// <param name="content">layout to focus</param>
        internal void Focus(IDockContent content)
        {
            if (m_lastFocusedContent != content)
            {
                if (m_lastFocusedContent != null)
                {
                    m_lastFocusedContent.IsFocused = false;
                }
                if (content != null)
                {
                    ((DockContent)content).IsFocused = true;
                }
            }
        }
        private void OwnerWindowActivated(object sender, EventArgs e)
        {
            /*Window wnd = Window.GetWindow(this);
            DependencyObject element = (DependencyObject)FocusManager.GetFocusedElement(wnd);
            if (element == null || Window.GetWindow(element) != wnd)
            {
                Focus();
            }
            if (m_lastFocusedContent != null)
            {
                ((DockContent)m_lastFocusedContent).IsFocused = true;
            }*/
        }

        private void ChildWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            FloatingWindow wnd = sender as FloatingWindow;
            if (wnd != null)
            {
                wnd.Closing -= ChildWindowClosing;
                m_windows.Remove(wnd);
            }
        }
        /// <summary>
        /// Register content with docking panel</summary>
        /// <param name="content">Content to register</param>
        /// <param name="ucid">Unique name for content (identifier, won't be visible anywhere in UI)</param>
        /// <param name="dockSide">Default docking place (center, left, right, top, bottom)</param>
        /// <returns>New dock content</returns>
        public IDockContent RegisterContent(object content, String ucid, DockTo dockSide) //where T : FrameworkElement, IDockContent
        {
            return RegisterContent(content, ucid, dockSide, String.Empty, null);
        }

        /// <summary>
        /// Register content with docking panel</summary>
        /// <param name="content">Content to register</param>
        /// <param name="ucid">Unique name for content (identifier, won't be visible anywhere in UI)</param>
        /// <param name="dockSide">Default docking place (center, left, right, top, bottom)</param>
        /// <param name="header">Header information to display in the docked tab or title bar</param>
        /// <param name="icon">Icon to display</param>
        /// <returns>New dock content</returns>
        public IDockContent RegisterContent(object content, String ucid, DockTo dockSide, String header, ImageSource icon) //where T : FrameworkElement, IDockContent
        {
            if (m_registeredContents.ContainsKey(ucid))
            {
                throw new ArgumentException("Content with given id already exists, id/name must be unique");
            }
            DockContent dockContent = new DockContent(content, ucid, header, icon) { Settings = new ContentSettings(dockSide) };
            dockContent.IsFocusedChanged += DockContent_IsFocusedChanged;
            m_registeredContents.Add(ucid, dockContent);
            RegisteredContents.Add(dockContent);
            return dockContent;
        }

        void DockContent_IsFocusedChanged(object sender, BooleanArgs e)
        {
            if (e.Value == true)
            {
                if (m_lastFocusedContent != sender)
                {
                    if (m_lastFocusedContent != null && m_lastFocusedContent.IsFocused)
                    {
                        m_lastFocusedContent.IsFocused = false;
                    }
                    m_lastFocusedContent = (DockContent)sender;
                }
            }
            else
            {
                if (m_lastFocusedContent == sender)
                {
                    m_lastFocusedContent = null;
                    this.Focus();
                }
            }
        }

        /// <summary>
        /// Display dock content. If the content is not docked in main panel, collapsible panels or windows, 
        /// then it is opened in its last position when it was closed</summary>
        /// <param name="o">Content object; must be registered by calling RegisterContent</param>
        public void ShowContent(object o)
        {
            foreach (DockContent content in RegisteredContents)
            {
                if (content.Content == o)
                {
                    ShowContent(content);
                    break;
                }
            }
        }

        /// <summary>
        /// Display dock content. If the content is not docked in main panel, collapsible panels or windows, 
        /// then it is opened in its last position when it was closed</summary>
        /// <param name="ucid">Unique name for content</param>
        public void ShowContent(String ucid)
        {
            if (IsLoaded)
            {
                if (!m_registeredContents.ContainsKey(ucid))
                {
                    throw new ArgumentOutOfRangeException("Content with given name is not registered!");
                }
                ShowContent(m_registeredContents[ucid]);
            }
        }

        /// <summary>
        /// Display dock content. If the content is not docked in main panel, collapsible panels or windows, 
        /// then it is opened in its last position when it was closed</summary>
        /// <param name="content">Content to show</param>
        public void ShowContent(IDockContent content)
        {
            DockContent dockContent;
            if (IsLoaded && (dockContent = content as DockContent) != null)
            {
                if (!m_registeredContents.ContainsValue(dockContent))
                {
                    throw new ArgumentOutOfRangeException("Given content is not registered!");
                }
                if (!((IDockLayout)this).HasDescendant(content))
                {
                    ContentSettings contentSettings = dockContent.Settings;
                    if (contentSettings.Size == new Size(0, 0))
                    {
                        double ratio = contentSettings.DefaultDock == DockTo.Center ? 0.8 : 0.2;
                        contentSettings.Size = new Size(ActualWidth * ratio, ActualHeight * ratio);
                    }
                    switch (contentSettings.DockState)
                    {
                        case DockState.Docked:
                            {
                                if (GridLayout == null)
                                {
                                    ((IDockLayout)this).Dock(null, content, DockTo.Center);
                                }
                                else
                                {
                                    Point position = new Point(ActualWidth / 2, ActualHeight / 2);
                                    DockTo dockTo = contentSettings.DefaultDock;
                                    switch (dockTo)
                                    {
                                        case DockTo.Left:
                                            position.X = 1;
                                            break;
                                        case DockTo.Right:
                                            position.X = ActualWidth - 2;
                                            break;
                                        case DockTo.Top:
                                            position.Y = 1;
                                            break;
                                        case DockTo.Bottom:
                                            position.Y = ActualHeight - 2;
                                            break;
                                        case DockTo.Center:
                                            // no modification necessary
                                            break;
                                    }
                                    position = PointToScreen(position);
                                    DockContent target = ((IDockLayout)this).HitTest(position);
                                    if (target != null)
                                    {
                                        DockTo targetDock = target.Settings.DefaultDock;
                                        if (targetDock == dockTo)
                                        {
                                            ((IDockLayout)this).Dock(target, content, DockTo.Center);
                                        }
                                        else
                                        {
                                            if (dockTo == DockTo.Center && (targetDock == DockTo.Right) ||
                                                dockTo == DockTo.Left && (targetDock == DockTo.Center || targetDock == DockTo.Right))
                                            {
                                                ((IDockLayout)this).Dock(target, content, DockTo.Left);
                                            }
                                            else if (dockTo == DockTo.Center && (targetDock == DockTo.Left) ||
                                                    dockTo == DockTo.Right && (targetDock == DockTo.Center || targetDock == DockTo.Left))
                                            {
                                                ((IDockLayout)this).Dock(target, content, DockTo.Right);
                                            }
                                            else if (dockTo == DockTo.Center && (targetDock == DockTo.Bottom) ||
                                                    dockTo == DockTo.Top && (targetDock == DockTo.Center || targetDock == DockTo.Bottom))
                                            {
                                                ((IDockLayout)this).Dock(target, content, DockTo.Top);
                                            }
                                            else if (dockTo == DockTo.Center && (targetDock == DockTo.Top) ||
                                                    dockTo == DockTo.Bottom && (targetDock == DockTo.Center || targetDock == DockTo.Top))
                                            {
                                                ((IDockLayout)this).Dock(target, content, DockTo.Bottom);
                                            }
                                            else
                                            {
                                                ((IDockLayout)this).Dock(null, content, dockTo);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        ((IDockLayout)this).Dock(null, content, dockTo);
                                    }
                                }
                                UpdateLayout();
                            }
                            break;
                        case DockState.Floating:
                            {
                                FloatingWindow wnd = new FloatingWindow(this, content, contentSettings.Location, contentSettings.Size);
                                wnd.Closing += ChildWindowClosing;
                                m_windows.Add(wnd);
                                wnd.Owner = Window.GetWindow(this);
                                wnd.Show();
                            }
                            break;
                        case DockState.Collapsed:
                            Collapse(content);
                            break;
                    }
                }
                Focus(content);
            }
        }
        
        /// <summary>
        /// Hide dock content</summary>
        /// <param name="ucid">Unique name for content</param>
        public void HideContent(String ucid)
        {
            if (IsLoaded)
            {
                if (!m_registeredContents.ContainsKey(ucid))
                {
                    throw new ArgumentOutOfRangeException("Specified content name is not registered");
                }
                HideContent(m_registeredContents[ucid]);
            }
        }

        /// <summary>
        /// Hide dock content</summary>
        /// <param name="content">Content to hide</param>
        public void HideContent(IDockContent content)
        {
            DockContent dockContent;
            if (IsLoaded && (dockContent = content as DockContent) != null)
            {
                if (!m_registeredContents.ContainsValue(dockContent))
                {
                    throw new ArgumentOutOfRangeException("Specified content is not registered");
                }
                if (GridLayout != null && GridLayout.HasDescendant(content))
                {
                    GridLayout.Undock(content);
                }
                else if (PART_LeftCollapsePanel.HasChild(content))
                {
                    PART_LeftCollapsePanel.Undock(content);
                }
                else if (PART_TopCollapsePanel.HasChild(content))
                {
                    PART_TopCollapsePanel.Undock(content);
                }
                else if (PART_RightCollapsePanel.HasChild(content))
                {
                    PART_RightCollapsePanel.Undock(content);
                }
                else if (PART_BottomCollapsePanel.HasChild(content))
                {
                    PART_BottomCollapsePanel.Undock(content);
                }
                else
                {
                    foreach (FloatingWindow wnd in m_windows)
                    {
                        if (wnd.HasDescendant(content))
                        {
                            wnd.Undock(content);
                            break;
                        }
                    }
                }
                dockContent.IsVisible = ((IDockLayout)this).HasDescendant(content);
            }
        }

        /// <summary>
        /// Unregister dock content</summary>
        /// <param name="ucid">Unique name for content</param>
        public void UnregisterContent(String ucid)
        {
            if (!m_registeredContents.ContainsKey(ucid))
            {
                throw new ArgumentOutOfRangeException("Specified content name is not registered");
            }
            HideContent(ucid);
            m_registeredContents[ucid].IsFocusedChanged -= DockContent_IsFocusedChanged;
            if (m_lastFocusedContent == m_registeredContents[ucid])
            {
                m_lastFocusedContent = null;
            }
            RegisteredContents.Remove(m_registeredContents[ucid]);
            m_registeredContents.Remove(ucid);
        }

        /// <summary>
        /// Unregister dock content</summary>
        /// <param name="content">Content</param>
        public void UnregisterContent(IDockContent content)
        {
            DockContent dockContent;
            if ((dockContent = content as DockContent) == null)
            {
                throw new ArgumentException("Invalid content");
            }

            if (!m_registeredContents.ContainsValue(dockContent))
            {
                throw new ArgumentOutOfRangeException("Specified content is not registered");
            }
            UnregisterContent(content.UID);
        }

        private void CacheLayout(XmlReader reader)
        {
            if (reader == null)
            {
                m_layoutToApply = null;
            }
            else
            {
                if (reader.ReadToFollowing(this.GetType().Name, this.GetType().Namespace))
                {
                    String s = reader.ReadOuterXml();
                    MemoryStream ms = new MemoryStream();
                    XmlWriterSettings xws = new XmlWriterSettings() { Indent = true, Encoding = Encoding.UTF8 };
                    XmlWriter xw = XmlTextWriter.Create(ms, xws);
                    xw.WriteRaw(s);
                    xw.Close();
                    XmlReaderSettings xrs = new XmlReaderSettings() { IgnoreWhitespace = true };
                    ms.Seek(0, 0);
                    m_layoutToApply = XmlTextReader.Create(ms, xrs);
                }
            }
        }

        /// <summary>
        /// Read the layout from XML file and apply it to dockpanel</summary>
        /// <param name="reader">Source XML file</param>
        public void ApplyLayout(XmlReader reader)
        {
            try
            {
                if (reader == null)
                {
                    PerformDefaultLayout();
                }
                else
                {
                    if (!m_templateApplied)
                    {
                        CacheLayout(reader);
                        return;
                    }
                    if (m_layoutApplied)
                    {
                        ((IDockLayout)this).Close();
                    }
                    if (reader.ReadToFollowing(this.GetType().Name, this.GetType().Namespace))
                    {
                        reader.ReadStartElement();
                        if (reader.LocalName == "Contents")
                        {
                            if (reader.ReadToDescendant("Content"))
                            {
                                do
                                {
                                    String ucid = reader.GetAttribute("UCID");
                                    DockContent content = GetContent(ucid);
                                    if (content != null)
                                    {
                                        content.Settings.DockState = (DockState)Enum.Parse(typeof(DockState), reader.GetAttribute(typeof(DockState).Name));
                                        try
                                        {
                                            var str = reader.GetAttribute("Location");
                                            content.Settings.Location = Point.Parse(str);
                                        }
                                        catch (FormatException)
                                        {
                                        }
                                        try
                                        {
                                            var str = reader.GetAttribute("Size");
                                            content.Settings.Size = Size.Parse(str);
                                        }
                                        catch (FormatException)
                                        {
                                        }
                                    }
                                } while (reader.ReadToNextSibling("Content"));

                                // there will only be an end element if "Content" entries were listed
                                reader.ReadEndElement();
                            }
                            else
                            {
                                if (reader.IsEmptyElement)
                                {
                                    reader.Read();
                                }
                            }
                            // else, the "Contents" element was self-contained, and there is no end element to parse
                        }
                        if (reader.LocalName == typeof(GridLayout).Name)
                        {
                            GridLayout gridLayout = new GridLayout(this, reader.ReadSubtree());
                            GridLayout = gridLayout.Layouts.Count > 0 ? gridLayout : null;
                            reader.ReadEndElement();
                        }
                        if (reader.LocalName == typeof(FloatingWindow).Name)
                        {
                            do
                            {
                                FloatingWindow window = new FloatingWindow(this, reader.ReadSubtree());
                                if (window.DockedContent.Children.Count > 0)
                                {
                                    window.Closing += ChildWindowClosing;
                                    m_windows.Add(window);
                                    window.Owner = Window.GetWindow(this);
                                    window.Show();
                                    window.Left = window.Left;
                                    window.Top = window.Top;
                                }
                                else
                                {
                                    // To-do: create "dummy content" like in DockPanelSuite, so that if the
                                    //  window were to be created later, we could apply the layout. For now,
                                    // just set the owner. This ties the FloatingWindow to the main window, and 
                                    // allows the app to shut down properly with the default Application.ShutdownMode 
                                    // of OnLastWindowClose.
                                    window.Owner = Window.GetWindow(this);
                                }
                            } while (reader.ReadToNextSibling(typeof(FloatingWindow).Name));
                            reader.ReadEndElement();
                        }
                        if (reader.LocalName == typeof(SidePopup).Name)
                        {
                            do
                            {
                                Dock dockSide = (Dock)Enum.Parse(typeof(Dock), reader.GetAttribute("Side"));
                                switch (dockSide)
                                {
                                    case System.Windows.Controls.Dock.Left:
                                        PART_LeftCollapsePanel.ReadXml(reader.ReadSubtree());
                                        break;
                                    case System.Windows.Controls.Dock.Top:
                                        PART_TopCollapsePanel.ReadXml(reader.ReadSubtree());
                                        break;
                                    case System.Windows.Controls.Dock.Right:
                                        PART_RightCollapsePanel.ReadXml(reader.ReadSubtree());
                                        break;
                                    case System.Windows.Controls.Dock.Bottom:
                                        PART_BottomCollapsePanel.ReadXml(reader.ReadSubtree());
                                        break;
                                }
                            } while (reader.ReadToNextSibling(typeof(SidePopup).Name));
                            reader.ReadEndElement();
                        }
                        m_layoutApplied = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ((IDockLayout)this).Close();
                PerformDefaultLayout();
                throw ex;
            }
        }

        private void PerformDefaultLayout()
        {
            if (IsLoaded)
            {
                foreach (IDockContent content in m_registeredContents.Values)
                {
                    ((IDockLayout)this).Undock(content);
                }
                GridLayout = null;
                foreach (IDockContent content in m_registeredContents.Values)
                {
                    ShowContent(content);
                }
                m_layoutApplied = true;
            }
        }
        /// <summary>
        /// Save the layout to XML writer</summary>
        /// <param name="writer">Output XML</param>
        public void SaveLayout(XmlWriter writer)
        {
            if (IsLoaded)
            {
                Type type = this.GetType();
                writer.WriteStartElement(type.Name, type.Namespace);
                writer.WriteStartElement("Contents");
                foreach (KeyValuePair<String, DockContent> content in m_registeredContents)
                {
                    writer.WriteStartElement("Content");
                    ContentSettings contentSettings = content.Value.Settings;
                    writer.WriteAttributeString("UCID", content.Key);
                    writer.WriteAttributeString(contentSettings.DefaultDock.GetType().Name, contentSettings.DefaultDock.ToString());
                    writer.WriteAttributeString(contentSettings.DockState.GetType().Name, contentSettings.DockState.ToString());
                    writer.WriteAttributeString("Location", contentSettings.Location.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("Size", contentSettings.Size.ToString(CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                if (GridLayout != null)
                {
                    GridLayout.WriteXml(writer);
                }
                foreach (FloatingWindow floatingWindow in m_windows)
                {
                    floatingWindow.WriteXml(writer);
                }
                PART_LeftCollapsePanel.WriteXml(writer);
                PART_TopCollapsePanel.WriteXml(writer);
                PART_RightCollapsePanel.WriteXml(writer);
                PART_BottomCollapsePanel.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        private IDockLayout GetContentParent(IDockContent content)
        {
            FrameworkElement element = content as FrameworkElement;
            FrameworkElement owner = element.Parent as FrameworkElement;
            while (owner != null && !(owner is IDockLayout))
            {
                owner = owner.Parent as FrameworkElement;
            }
            return owner as IDockLayout;
        }

        /// <summary>
        /// Start dragging the given content. This includes undocking it from the UI,
        /// creating new window with the content, and then start dragging it.</summary>
        /// <param name="source"></param>
        /// <param name="content">Content to drag</param>
        internal void Drag(IDockLayout source, IDockContent content)
        {
            FrameworkElement sourceElement = (FrameworkElement)source;
            ContentSettings settings = (content is TabLayout) ? ((TabLayout)content).Children[0].Settings : ((DockContent)content).Settings;
            Size size = new Size(sourceElement.ActualWidth, sourceElement.ActualHeight);
            Point offset = new Point(settings.Size.Width / 2, 3);
            Point position = Mouse.GetPosition(this);
            position = PointToScreen(position);
            Matrix m = PresentationSource.FromVisual(Window.GetWindow(this)).CompositionTarget.TransformToDevice;
            if (m != Matrix.Identity)
            {
                m.Invert();
                position = m.Transform(position);
            }
            offset = Mouse.GetPosition(sourceElement);
            offset.Y = offset.Y > 20 ? 10 : offset.Y;
            position = (Point)(position - offset);
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                source.Undock(content);
                FloatingWindow wnd = new FloatingWindow(this, content, position, size);
                wnd.Closing += ChildWindowClosing;
                m_windows.Add(wnd);
                wnd.Owner = Window.GetWindow(this);
                wnd.Show();
                wnd.DragMove();
            }
        }
        /// <summary>
        /// This "un"collapses content, so it is undocked from the collapsible panel
        /// on the side, and then redocked to main view</summary>
        /// <param name="content">Panel to uncollapse</param>
        internal void UnCollapse(IDockContent content)
        {
            TabLayout mc = (TabLayout)content;
            while(mc.Children.Count > 0)
            {
                DockContent subContent = mc.Children[0];
                DockTo dockTo = DockTo.Center;
                if (PART_LeftCollapsePanel.HasChild(subContent))
                {
                    PART_LeftCollapsePanel.Undock(subContent);
                    dockTo = DockTo.Left;
                }
                if (PART_TopCollapsePanel.HasChild(subContent))
                {
                    PART_TopCollapsePanel.Undock(subContent);
                    dockTo = DockTo.Top;
                }
                if (PART_RightCollapsePanel.HasChild(subContent))
                {
                    PART_RightCollapsePanel.Undock(subContent);
                    dockTo = DockTo.Right;
                }
                if (PART_BottomCollapsePanel.HasChild(subContent))
                {
                    PART_BottomCollapsePanel.Undock(subContent);
                    dockTo = DockTo.Bottom;
                }
                subContent.Settings.DockState = DockState.Docked;
                ((IDockLayout)this).Dock(null, subContent, dockTo);
            }
        }
        /// <summary>
        /// Undock given content and collapse to the side that is closest to it</summary>
        /// <param name="content">Content to collapse</param>
        internal void Collapse(IDockContent content)
        {
            Image image = null;
            Rect rectFrom = Rect.Empty;
            FrameworkElement parent = null;
            if (GridLayout != null && GridLayout.HasDescendant(content))
            {
                FrameworkElement element = (FrameworkElement)content;
                parent = (FrameworkElement)element.Parent;
                if (parent != null)
                {
                    rectFrom = new Rect(parent.PointToScreen(new Point(0, 0)), parent.RenderSize);
                    // create visual image of the element that is being collapsed
                    VisualBrush elementBrush = new VisualBrush(parent);
                    DrawingVisual visual = new DrawingVisual();
                    DrawingContext dc = visual.RenderOpen();
                    dc.DrawRectangle(elementBrush, null, new Rect(0, 0, rectFrom.Width, rectFrom.Height));
                    dc.Close();
                    RenderTargetBitmap bitmap = new RenderTargetBitmap((int)rectFrom.Width, (int)rectFrom.Height, 96, 96, PixelFormats.Default);
                    bitmap.Render(visual);
                    image = new Image();
                    image.Source = bitmap;
                    // undock element
                    if (GridLayout != null)
                    {
                        GridLayout.Undock(content);
                    }
                    m_lastFocusedContent = null;
                }
            }
            Rect rectTo;
            DockContent tmp = content is TabLayout ? ((TabLayout)content).Children[0] : ((DockContent)content);
            ContentSettings settings = tmp.Settings;
            // dock to nearest side bar
            if (settings.Location.X > settings.Location.Y)
            {
                if (1.0 - settings.Location.X > settings.Location.Y)
                {
                    PART_TopCollapsePanel.Dock(null, content, DockTo.Top);
                    rectTo = PART_TopCollapsePanel.RectForContent(tmp);
                }
                else
                {
                    PART_RightCollapsePanel.Dock(null, content, DockTo.Right);
                    rectTo = PART_RightCollapsePanel.RectForContent(tmp);
                }
            }
            else
            {
                if (1.0 - settings.Location.X > settings.Location.Y)
                {
                    PART_LeftCollapsePanel.Dock(null, content, DockTo.Left);
                    rectTo = PART_LeftCollapsePanel.RectForContent(tmp);
                }
                else
                {
                    PART_BottomCollapsePanel.Dock(null, content, DockTo.Bottom);
                    rectTo = PART_BottomCollapsePanel.RectForContent(tmp);
                }
            }
            // animate!
            if (parent != null)
            {
                AnimateCollapse(image, rectFrom, rectTo);
            }
        }
        /// <summary>
        /// Creates and starts animation for collapsing, to indicate where the window is going</summary>
        /// <param name="element">Element that is being animated (image of original element)</param>
        /// <param name="from">From rectangle, original position and size</param>
        /// <param name="to">To rectangle, target position and size</param>
        private void AnimateCollapse(FrameworkElement element, Rect from, Rect to)
        {
            Duration duration = new Duration(TimeSpan.FromSeconds(0.33));
            DockIconsLayer.ClearChildren();
            element.Opacity = 1;
            Point pFrom = PointFromScreen(from.TopLeft);
            Point pTo = PointFromScreen(to.TopLeft);
            Canvas.SetLeft(element, pFrom.X);
            Canvas.SetTop(element, pFrom.Y);
            DockIconsLayer.AddChild(element);
            Storyboard sb = new Storyboard();
            sb.Children.Add(CreateDoubleAnimation(pFrom.X, pTo.X, duration, Canvas.LeftProperty, element));
            sb.Children.Add(CreateDoubleAnimation(pFrom.Y, pTo.Y, duration, Canvas.TopProperty, element));
            sb.Children.Add(CreateDoubleAnimation(from.Width, to.Width, duration, FrameworkElement.WidthProperty, element));
            sb.Children.Add(CreateDoubleAnimation(from.Height, to.Height, duration, FrameworkElement.HeightProperty, element));
            sb.Completed += CollapseCompleted;
            sb.Begin();			
        }
        /// <summary>
        /// Creates double animation; this is helper function, so we don't have to call all those setters</summary>
        /// <param name="from">From value</param>
        /// <param name="to">To value</param>
        /// <param name="duration">Duration of animation</param>
        /// <param name="property">Target element property</param>
        /// <param name="target">Target element</param>
        /// <returns>DoubleAnimation</returns>
        private DoubleAnimation CreateDoubleAnimation(double from, double to, Duration duration, DependencyProperty property, FrameworkElement target)
        {
            DoubleAnimation anim = new DoubleAnimation(from, to, duration);
            Storyboard.SetTarget(anim, target);
            Storyboard.SetTargetProperty(anim, new PropertyPath(property));
            return anim;
        }

        private void CollapseCompleted(object sender, EventArgs e)
        {
            DockIconsLayer.ClearChildren();
        }
        /// <summary>
        /// Find elements at mouse position</summary>
        /// <param name="e">Mouse event arguments</param>
        /// <returns>List of IDockable elements at the position. There can be more of them, because the 
        /// layout is tree and one grid layout can contain more layouts.</returns>
        internal List<IDockable> FindElementsAt(MouseEventArgs e)
        {
            bool isOverChild = false;
            List<IDockable> result = new List<IDockable>();
            Window mainWindow = Window.GetWindow(this);
            foreach (Window window in mainWindow.OwnedWindows)
            {
                IInputElement element = window.InputHitTest(e.GetPosition(window));
                if(element != null && !(window is DockIconsLayer) && !window.IsMouseCaptured)
                {
                    if (window is FloatingWindow)
                    {
                        DependencyObject o = element as DependencyObject;
                        while(o != null)
                        {
                            if(o is IDockable)
                            {
                                result.Add((IDockable)o);
                            }							
                            o = VisualTreeHelper.GetParent(o);
                        }
                    }
                    isOverChild = true;
                    break;
                }
            }
            if (!isOverChild)
            {
                IInputElement element = mainWindow.InputHitTest(e.GetPosition(mainWindow));
                if (element != null)
                {
                    DependencyObject o = element as DependencyObject;
                    while (o != null)
                    {
                        if (o is IDockable)
                        {
                            result.Add((IDockable)o);
                        }
                        if (o is Visual)
                        {
                            o = VisualTreeHelper.GetParent(o);
                        }
                        else
                        {
                            o = LogicalTreeHelper.GetParent(o);
                        }
                    }
                }
            }
            return result;
        }

        #region IDockLayout Members

        /// <summary>
        /// Get the root of the hierarchy</summary>
        DockPanel IDockLayout.Root
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Hit test the position and return content at that position</summary>
        /// <param name="position">Position to test</param>
        /// <returns>Content if hit, null otherwise</returns>
        DockContent IDockLayout.HitTest(Point position)
        {
            if (GridLayout != null)
            {
                return GridLayout.HitTest(position);
            }
            return null;
        }

        /// <summary>
        /// Check if the layout contains the content as direct child</summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True iff the content is child of this control</returns>
        bool IDockLayout.HasChild(IDockContent content)
        {
            if (!IsLoaded)
            {
                return false;
            }
            if (GridLayout != null && GridLayout.HasChild(content))
            {
                return true;
            }
            foreach (FloatingWindow wnd in m_windows)
            {
                if (wnd.HasChild(content))
                {
                    return true;
                }
            }
            if (PART_LeftCollapsePanel.HasChild(content))
            {
                return true;
            }
            if (PART_TopCollapsePanel.HasChild(content))
            {
                return true;
            }
            if (PART_RightCollapsePanel.HasChild(content))
            {
                return true;
            }
            if (PART_BottomCollapsePanel.HasChild(content))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the layout contains the content as child or descendant</summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True iff content is child or descendant</returns>
        bool IDockLayout.HasDescendant(IDockContent content)
        {
            if (!IsLoaded)
            {
                return false;
            }
            if (GridLayout != null && GridLayout.HasDescendant(content))
            {
                return true;
            }
            foreach (FloatingWindow wnd in m_windows)
            {
                if (wnd.HasDescendant(content))
                {
                    return true;
                }
            }
            if (PART_LeftCollapsePanel.HasDescendant(content))
            {
                return true;
            }
            if (PART_TopCollapsePanel.HasDescendant(content))
            {
                return true;
            }
            if (PART_RightCollapsePanel.HasDescendant(content))
            {
                return true;
            }
            if (PART_BottomCollapsePanel.HasDescendant(content))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Dock the new content next to content</summary>
        /// <param name="nextTo">Dock content to add new content next to</param>
        /// <param name="newContent">New content to be docked</param>
        /// <param name="dockTo">Side of nextTo content where new content should be docked</param>
        void IDockLayout.Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
        {
            if (GridLayout == null)
            {
                GridLayout = new GridLayout(this);
                GridLayout.Dock(null, newContent, DockTo.Center);

            }
            else
            {
                if (nextTo == null)
                {
                    if (GridLayout.Children.Count < 2)
                    {
                        GridLayout.Dock(null, newContent, dockTo);
                    }
                    else
                    {
                        if (dockTo == DockTo.Center)
                        {
                            dockTo = DockTo.Right;
                        }
                        GridLayout gridLayout = GridLayout;
                        GridLayout = null;
                        gridLayout = new GridLayout(this, gridLayout);
                        GridLayout = gridLayout;
                        gridLayout.Dock(null, newContent, dockTo);
                    }
                }
                else
                {
                    GridLayout.Dock(nextTo, newContent, dockTo);
                }
                CheckConsistency();
            }
        }

        /// <summary>
        /// Undock given content</summary>
        /// <param name="content">Content to undock</param>
        void IDockLayout.Undock(IDockContent content)
        {
            if (GridLayout != null && GridLayout.HasDescendant(content))
            {
                GridLayout.Undock(content);
            }
        }

        /// <summary>
        /// Undock given child layout</summary>
        /// <param name="child">Child layout to undock</param>
        void IDockLayout.Undock(IDockLayout child)
        {
            if (GridLayout == child)
            {
                GridLayout = null;
            }
        }

        /// <summary>
        /// Replace the old layout with new layout child</summary>
        /// <param name="oldLayout">Old layout to be replaced</param>
        /// <param name="newLayout">New layout that replaces old layout</param>
        void IDockLayout.Replace(IDockLayout oldLayout, IDockLayout newLayout)
        {
            if (GridLayout == oldLayout)
            {
                GridLayout = (GridLayout)newLayout;
            }
        }

        /// <summary>
        /// Close the layout</summary>
        void IDockLayout.Close()
        {
            if (GridLayout != null)
            {
                GridLayout.Close();
                GridLayout = null;
            }
            while (m_windows.Count > 0)
            {
                m_windows[0].Close();
            }
            m_windows.Clear();
            PART_LeftCollapsePanel.Close();
            PART_TopCollapsePanel.Close();
            PART_RightCollapsePanel.Close();
            PART_BottomCollapsePanel.Close();
        }

        /// <summary>
        /// Return the content's parent as an IDockLayout</summary>
        /// <param name="content">The docked content whose parent is requested</param>
        /// <returns>The parent as IDockLayout</returns>
        IDockLayout IDockLayout.FindParentLayout(IDockContent content)
        {
            if (IsLoaded)
            {
                if (GridLayout != null && GridLayout.HasDescendant(content))
                {
                    return ((IDockLayout)GridLayout).FindParentLayout(content);
                }
                if (PART_LeftCollapsePanel.HasDescendant(content))
                {
                    return ((IDockLayout)PART_LeftCollapsePanel).FindParentLayout(content);
                }
                if (PART_TopCollapsePanel.HasDescendant(content))
                {
                    return ((IDockLayout)PART_TopCollapsePanel).FindParentLayout(content);
                }
                if (PART_RightCollapsePanel.HasDescendant(content))
                {
                    return ((IDockLayout)PART_RightCollapsePanel).FindParentLayout(content);
                }
                if (PART_BottomCollapsePanel.HasDescendant(content))
                {
                    return ((IDockLayout)PART_BottomCollapsePanel).FindParentLayout(content);
                }
                foreach (FloatingWindow wnd in m_windows)
                {
                    if (wnd.HasDescendant(content))
                    {
                        return ((IDockLayout)wnd).FindParentLayout(content);
                    }
                }
            }
            return null;
        }

        #endregion

        #region IDockable Members

        /// <summary>
        /// Get nullable DockTo indicating where dockable window should be dropped relative to window it's dropped onto</summary>
        DockTo? IDockable.DockPreview
        {
            get { return m_dockPreview; }
        }

        /// <summary>
        /// Function called when dragged window enters already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void IDockable.DockDragEnter(object sender, DockDragDropEventArgs e)
        {
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);
            int space = (int)DockIconSize.Width / 4;
            ResourceDictionary rd = new ResourceDictionary();
            rd.Source = new Uri("/Atf.Gui.Wpf;component/Resources/DockIcons.xaml", UriKind.Relative);
            if (m_dockLeftIcon == null)
            {
                m_dockLeftIcon = new DockIcon((Style)rd["DockLeftIcon"], DockIconSize);
                m_dockRightIcon = new DockIcon((Style)rd["DockRightIcon"], DockIconSize);
                m_dockTopIcon = new DockIcon((Style)rd["DockTopIcon"], DockIconSize);
                m_dockBottomIcon = new DockIcon((Style)rd["DockBottomIcon"], DockIconSize);
                m_dockTabIcon = new DockIcon((Style)rd["DockTabIcon"], DockIconSize);
            }
            m_dockLeftIcon.Offset = new Point(space, center.Y - DockIconSize.Height / 2);
            m_dockRightIcon.Offset = new Point(ActualWidth - DockIconSize.Width - space, center.Y - DockIconSize.Height / 2);
            m_dockTopIcon.Offset = new Point(center.X - DockIconSize.Width / 2, space);
            m_dockBottomIcon.Offset = new Point(center.X - DockIconSize.Width / 2, ActualHeight - DockIconSize.Height - space);
            m_dockTabIcon.Offset = new Point(center.X - DockIconSize.Width / 2, center.Y - DockIconSize.Height / 2);
            if (GridLayout != null)
            {
                //if (GridLayout.Children.Count > 1)
                {
                    DockIconsLayer.AddChild(m_dockLeftIcon);
                    DockIconsLayer.AddChild(m_dockRightIcon);
                    DockIconsLayer.AddChild(m_dockTopIcon);
                    DockIconsLayer.AddChild(m_dockBottomIcon);
                }
            }
            else
            {
                DockIconsLayer.AddChild(m_dockTabIcon);
            }
        }

        /// <summary>
        /// Function called when dragged window is moved over already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void IDockable.DockDragOver(object sender, DockDragDropEventArgs e)
        {
            DockTo? previousDockPreview = m_dockPreview;
            m_dockPreview = null;
            Point pos = e.MouseEventArgs.GetPosition(this);
            if (GridLayout != null)
            {
                //if (GridLayout.Children.Count > 1)
                {
                    m_dockLeftIcon.Highlight = m_dockLeftIcon.HitTest(pos);
                    m_dockRightIcon.Highlight = m_dockRightIcon.HitTest(pos);
                    m_dockTopIcon.Highlight = m_dockTopIcon.HitTest(pos);
                    m_dockBottomIcon.Highlight = m_dockBottomIcon.HitTest(pos);
                    m_dockPreview = m_dockLeftIcon.Highlight ? DockTo.Left : m_dockPreview;
                    m_dockPreview = m_dockRightIcon.Highlight ? DockTo.Right : m_dockPreview;
                    m_dockPreview = m_dockTopIcon.Highlight ? DockTo.Top : m_dockPreview;
                    m_dockPreview = m_dockBottomIcon.Highlight ? DockTo.Bottom : m_dockPreview;
                }
            }
            else
            {
                m_dockTabIcon.Highlight = m_dockTabIcon.HitTest(pos);
                m_dockPreview = m_dockTabIcon.Highlight ? DockTo.Center : m_dockPreview;
            }
            if (m_dockPreview != null)
            {
                if (previousDockPreview != m_dockPreview)
                {
                    DockIconsLayer.RemoveChild(m_dockPreviewShape);
                    m_dockPreviewShape = null;
                    Window owner = Window.GetWindow(this);
                    Rectangle rect = new Rectangle();
                    rect.Fill = Brushes.RoyalBlue;
                    rect.Opacity = 0.3;
                    FrameworkElement fe = rect;
                    double space = 2;
                    Point p = new Point(space, space);
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
                            fe.Width = ActualWidth - space * 2;
                            fe.Height = ActualHeight - space * 2;
                            Canvas.SetLeft(fe, p.X);
                            Canvas.SetTop(fe, p.Y);
                            break;
                    }
                    DockIconsLayer.InsertChild(0, fe);
                    m_dockPreviewShape = fe;
                }
            }
            else
            {
                if (m_dockPreviewShape != null)
                {
                    DockIconsLayer.RemoveChild(m_dockPreviewShape);
                    m_dockPreviewShape = null;
                }
            }
        }

        /// <summary>
        /// Function called when dragged window leaves already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void IDockable.DockDragLeave(object sender, DockDragDropEventArgs e)
        {
            DockIconsLayer.RemoveChild(m_dockLeftIcon);
            DockIconsLayer.RemoveChild(m_dockRightIcon);
            DockIconsLayer.RemoveChild(m_dockTopIcon);
            DockIconsLayer.RemoveChild(m_dockBottomIcon);
            DockIconsLayer.RemoveChild(m_dockTabIcon);
            if (m_dockPreviewShape != null)
            {
                DockIconsLayer.RemoveChild(m_dockPreviewShape);
                m_dockPreviewShape = null;
            }
            DockIconsLayer.CloseIfEmpty();
        }

        /// <summary>
        /// Function called when dragged window is dropped over already docked window</summary>
        /// <param name="sender">Dockable window being dragged</param>
        /// <param name="e">Drag and drop arguments when window is dropped to be docked to dockpanel</param>
        void IDockable.DockDrop(object sender, DockDragDropEventArgs e)
        {
            if (e.Content is TabLayout)
            {
                foreach (DockContent subContent in ((TabLayout)e.Content).Children)
                {
                    subContent.Settings.DockState = DockState.Docked;
                }
            }
            else
            {
                ContentSettings contentSettings = (e.Content is TabLayout) ? ((TabLayout)e.Content).Children[0].Settings : ((DockContent)e.Content).Settings;
                contentSettings.DockState = DockState.Docked;
            }
            ((IDockLayout)this).Dock(null, e.Content, (DockTo)m_dockPreview);
        }

        #endregion

        #region Automation
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new DockPanelAutomationPeer(this);            
        }
        #endregion
    }
}
