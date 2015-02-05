//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using System.Xml;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Threading;
using Sce.Atf;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// TabLayout that represents its children as tabs</summary>
    public class TabLayout : TabControl, IDockContent, IDockLayout, IXmlSerializable
    {
        #region Private Properties

        private DateTime m_timerTime;
        private System.Timers.Timer m_timer;
        private TabItem m_lastItemOver;
        private static readonly string s_headerPropertyName
            = TypeUtil.GetProperty<DockContent>(x => x.Header).Name;
        private static readonly string s_iconPropertyName
            = TypeUtil.GetProperty<DockContent>(x => x.Icon).Name;
        private static readonly string s_isFocusedPropertyName
            = TypeUtil.GetProperty<DockContent>(x => x.IsFocused).Name;

        #endregion

        /// <summary>
        /// Header DependencyProperty, which designates header text of tab window</summary>
        public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(String), typeof(TabLayout));
        /// <summary>
        /// Icon DependencyProperty</summary>
        public static DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(TabLayout));

        /// <summary>
        /// ItemsCount DependencyProperty</summary>
        public static DependencyProperty ItemsCountProperty = DependencyProperty.Register("ItemsCount", typeof(int), typeof(TabLayout));
        /// <summary>
        /// Get or set ItemsCount DependencyProperty</summary>
        public int ItemsCount
        {
            get { return ((int)(base.GetValue(TabLayout.ItemsCountProperty))); }
            set { base.SetValue(TabLayout.ItemsCountProperty, value); }
        }

        /// <summary>
        /// ItemStyle DependencyProperty</summary>
        public static DependencyProperty ItemStyleProperty = DependencyProperty.Register("ItemStyle", typeof(Style), typeof(TabLayout));
        /// <summary>
        /// Get or set ItemStyle DependencyProperty</summary>
        public Style ItemStyle
        {
            get { return ((Style)(base.GetValue(TabLayout.ItemStyleProperty))); }
            set { base.SetValue(TabLayout.ItemStyleProperty, value); }
        }

        private TabItem m_movingTabItem;
        private Point m_dragStartPosition;

        /// <summary>
        /// Children DependencyProperty</summary>
        public static DependencyProperty ChildrenProperty = DependencyProperty.Register("Children", typeof(ObservableCollection<DockContent>), typeof(TabLayout));
        /// <summary>
        /// Get or set Children DependencyProperty</summary>
        public ObservableCollection<DockContent> Children
        {
            get { return ((ObservableCollection<DockContent>)(base.GetValue(TabLayout.ChildrenProperty))); }
            set { base.SetValue(TabLayout.ChildrenProperty, value); }
        }

        /// <summary>
        /// Get the root dock panel</summary>
        public DockPanel Root { get; private set; }

        /// <summary>
        /// Static constructor that overrides the style that is used by styling</summary>
        static TabLayout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabLayout), new FrameworkPropertyMetadata(typeof(TabLayout)));
        }

        /// <summary>
        /// Constructor with root dock panel</summary>
        /// <param name="dockPanel">Root dock panel</param>
        public TabLayout(DockPanel dockPanel)
        {
            Root = dockPanel;
            Header = String.Empty;
            Children = new ObservableCollection<DockContent>();
            ItemsCount = 0;
            SizeChanged += TabLayout_SizeChanged;
            //PreviewMouseDown += TabControl_PreviewMouseDown;
            MouseMove += TabControl_MouseMove;
            MouseUp += TabControl_MouseUp;
            MouseLeave += TabControl_MouseLeave;
            ItemsSource = Children;
            m_timer = new System.Timers.Timer();
            m_timer.AutoReset = false;
            m_timer.Interval = 500;
            m_timer.Elapsed += Timer_Elapsed;
        }

        void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Dispatcher.Thread != Thread.CurrentThread)
            {
                Dispatcher.BeginInvoke(new EventHandler<ElapsedEventArgs>(Timer_Elapsed), new object[] { sender, e });
                return;
            }
            if (m_lastItemOver != null)
            {
                Point pos = Win32Calls.GetPosition(m_lastItemOver);
                bool b = new Rect(0, 0, m_lastItemOver.ActualWidth, m_lastItemOver.ActualHeight).Contains(pos);
                if (b)
                {
                    SelectedItem = m_lastItemOver.Content;
                }
            }
        }
        /// <summary>
        /// Constructor used when deserialising</summary>
        /// <param name="dockPanel">Root dock panel</param>
        /// <param name="reader">Source XML</param>
        public TabLayout(DockPanel dockPanel, XmlReader reader)
            : this(dockPanel)
        {
            ReadXml(reader);
        }
        
        /// <summary>
        /// Handle SelectionChanged event and perform custom actions</summary>
        /// <param name="e">Arguments describing change</param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            foreach (DockContent content in Children)
            {
                if (content != SelectedItem)
                {
                    if (content.IsFocused)
                    {
                        content.IsFocused = false;
                    }
                }
                else
                {
                    if (!content.IsFocused)
                    {
                        content.IsFocused = true;
                    }
                }
            }
            UpdateIconAndHeader();
        }

        /// <summary>
        /// Get TabItem DependencyObject to serve as item container in TabLayout</summary>
        /// <returns>TabItem DependencyObject item container in TabLayout</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            TabItem tabItem = new TabItem();
            if (ItemStyle != null)
            {
                tabItem.Style = ItemStyle;
            }
            tabItem.PreviewMouseDown += TabControl_PreviewMouseDown;
            tabItem.AllowDrop = true;
            tabItem.DragEnter += TabItem_DragEnter;
            return tabItem;
        }

        void TabItem_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Scroll & e.AllowedEffects;
            e.Handled = true;
            m_lastItemOver = (TabItem)sender;
            m_timerTime = DateTime.Now;
            m_timer.Start();
        }

        void TabLayout_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (DockContent content in Children)
            {
                if (content.Settings.DockState == DockState.Docked || content.Settings.DockState == DockState.Collapsed)
                {
                    Point p = Root.PointFromScreen(PointToScreen(new Point(ActualWidth / 2, ActualHeight / 2)));
                    content.Settings.Location = new Point(p.X / Root.ActualWidth, p.Y / Root.ActualHeight);
                    content.Settings.Size = new Size(ActualWidth, ActualHeight);
                }
            }
        }

        /// <summary>Add new content; the new content is single content</summary>
        /// <param name="toItem">Next to this item</param>
        /// <param name="content"></param>
        private void AddOneItem(DockContent toItem, DockContent content)
        {
            if (toItem != null && !Children.Contains(toItem))
            {
                throw new ArgumentOutOfRangeException();
            }
            int index = toItem != null ? Children.IndexOf(toItem) : Children.Count;
            content.PropertyChanged += Content_PropertyChanged;
            Children.Insert(index, content);
            ItemsCount = Children.Count;
            SelectedIndex = Items.Count - 1;
            content.IsVisible = true;
            UpdateIconAndHeader();
        }

        void Content_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == s_headerPropertyName || e.PropertyName == s_iconPropertyName)
            {
                UpdateIconAndHeader();
            }
            if (e.PropertyName == s_isFocusedPropertyName)
            {
                DockContent found = null;
                foreach(DockContent content in Children)
                {
                    if (content.IsFocused)
                    {
                        found = content;
                        break;
                    }
                }
                bool isAnyChildFocused = found != null;
                if (found != null)
                {
                    SelectedItem = found;
                }
                if (m_isFocused != isAnyChildFocused)
                {
                    m_isFocused = isAnyChildFocused;
                    if (m_isFocusedChanged != null)
                    {
                        m_isFocusedChanged(this, new BooleanArgs(m_isFocused));
                    }
                }
            }
        }

        /// <summary>
        /// Removes given item</summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(DockContent item)
        {
            item.PropertyChanged -= Content_PropertyChanged;
            Children.Remove(item);
            ItemsCount = Children.Count;
            item.IsVisible = false;
            item.IsFocused = false;
            UpdateIconAndHeader();
        }

        private void UpdateIconAndHeader()
        {
            DockContent content = GetActiveContent();
            if (content != null)
            {
                Header = content.Header;
                Icon = content.Icon;
            }
        }

        void TabControl_PreviewMouseDown(object sender, MouseEventArgs e)
        {
            // Only allow drag undock if there is more than one item in the
            // control
            if (ItemsCount > 1)
            {
                TabItem tabItem = e.Source as TabItem;
                if (tabItem != null && e.LeftButton == MouseButtonState.Pressed)
                {
                    m_movingTabItem = tabItem;
                    m_dragStartPosition = e.GetPosition(this);
                }
            }
        }

        void TabControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_movingTabItem != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(this);
                Vector delta = m_dragStartPosition - currentPosition;
                if ((Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance) || (Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    TabItem item = m_movingTabItem;
                    m_movingTabItem = null;
                    Root.Drag(this, (IDockContent)item.Content);
                }
            }
        }

        void TabControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_movingTabItem = null;
        }
        
        void TabControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (m_movingTabItem != null && e.LeftButton == MouseButtonState.Pressed)
            {
                TabItem item = m_movingTabItem;
                m_movingTabItem = null;
                Root.Drag(this, (IDockContent)item.Content);
            }
        }

        void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedIndex < 0)
            {
                SelectedIndex = 0;
            }
            UpdateIconAndHeader();
        }
        /// <summary>
        /// Activate this content (focus)</summary>
        public void Activate()
        {
            FrameworkElement element = SelectedItem as FrameworkElement;
            if(element != null)
            {
                element.Focus();
            }
        }

        /// <summary>
        /// Deactivate this content (give up focus)</summary>
        public void Deactivate()
        {
        }

        /// <summary>
        /// Return selected dock content</summary>
        /// <returns>Selected dock content</returns>
        public DockContent GetActiveContent()
        {
            return (SelectedItem != null) ? (DockContent)SelectedItem : null;
        }

        #region IDockableContent Members

        /// <summary>
        /// Content closed event</summary>
        public event ContentClosedEvent Closing;

        /// <summary>
        /// Get or set Header DependencyProperty, which designates header text of tab window</summary>
        public String Header
        {
            get { return ((String)(base.GetValue(TabLayout.HeaderProperty))); }
            set { base.SetValue(TabLayout.HeaderProperty, value); }
        }
        /// <summary>
        /// Get or set Icon DependencyProperty</summary>
        public object Icon
        {
            get { return ((base.GetValue(TabLayout.IconProperty))); }
            set { base.SetValue(TabLayout.IconProperty, value); }
        }

        /// <summary>
        /// Get the unique ID for TabLayout</summary>
        public String UID { get { return String.Empty; } }

        /// <summary>
        /// Get the content for TabLayout</summary>
        public Object Content { get { return this; } }

        /// <summary>
        /// Get whether TabLayout visible</summary>
        bool IDockContent.IsVisible { get { return true; } }

        private bool m_isFocused;
        ///<summary> 
        ///Get focus state of TabLayout</summary>
        bool IDockContent.IsFocused
        {
            get { return m_isFocused; }
        }

        #pragma warning disable 0067
        private event EventHandler<BooleanArgs> m_isVisibleChanged;
        #pragma warning restore 0067

        /// <summary>
        /// Event triggered when the IsVisible property changes</summary>
        event EventHandler<BooleanArgs> IDockContent.IsVisibleChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add { m_isVisibleChanged += value; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove { m_isVisibleChanged -= value; }
        }

        private event EventHandler<BooleanArgs> m_isFocusedChanged;
        /// <summary>
        /// Event triggered when the IsFocused property changes</summary>
        event EventHandler<BooleanArgs> IDockContent.IsFocusedChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add { m_isFocusedChanged += value; }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove { m_isFocusedChanged -= value; }
        }

        /// <summary>
        /// Handle Close event and perform custom actions</summary>
        /// <param name="sender">Event sender</param>
        /// <param name="args">Arguments describing change</param>
        public void OnClose(object sender, ContentClosedEventArgs args)
        {
            if (args.ContentToClose == ContentToClose.Current && Children.Count > 1)
            {
                DockContent content = ((DockContent)SelectedItem);
                RemoveItem(content);
                content.OnClose(content, new ContentClosedEventArgs(ContentToClose.All));
            }
            else
            {
                while (Children.Count > 0)
                {
                    DockContent content = Children[0];
                    RemoveItem(content);
                    content.OnClose(content, new ContentClosedEventArgs(ContentToClose.All));					
                }
                if (Children.Count == 0 && Closing != null)
                {
                    Closing(this, new ContentClosedEventArgs(ContentToClose.Current));
                }
            }
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        /// Reserved and should not be used</summary>
        /// <exception cref="NotImplementedException"> is raised if called</exception>
        /// <returns>XmlSchema describing XML representation of object</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Generate object from its XML representation</summary>
        /// <param name="reader">XmlReader stream from which object is deserialized</param>
        public void ReadXml(System.Xml.XmlReader reader)
        {
            reader.Read();
            if (reader.LocalName == this.GetType().Name || reader.LocalName == "MultiContent") // MultiContent is old name and is used here for compatibility with old saved layouts
            {
                String selectedUcid = reader.GetAttribute("SelectedUID");
                DockContent found = null;
                reader.ReadStartElement();
                if (reader.LocalName == "Content")
                {
                    do
                    {
                        String ucid = reader.GetAttribute("UCID");
                        DockContent content = Root.GetContent(ucid);
                        if (content != null)
                        {
                            AddOneItem(null, content);
                            if (selectedUcid == ucid)
                            {
                                found = content;
                            }
                        }
                    } while (reader.ReadToNextSibling("Content"));
                    if (found != null)
                    {
                        SelectedItem = found;
                    }
                }
                reader.ReadEndElement();
            }
        }

        /// <summary>
        /// Convert object into its XML representation</summary>
        /// <param name="writer">XmlWriter stream to which object is serialized</param>
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(this.GetType().Name);
            writer.WriteAttributeString("SelectedUID", ((DockContent)SelectedItem).UID);
            foreach (DockContent content in Children)
            {
                writer.WriteStartElement("Content");
                writer.WriteAttributeString("UCID", content.UID);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        #endregion

        #region IDockLayout Members

        /// <summary>
        /// Hit test the position and return content at that position</summary>
        /// <param name="position">Position to test</param>
        /// <returns>Content if hit, null otherwise</returns>
        public DockContent HitTest(Point position)
        {
            Rect rect = new Rect(0, 0, ActualWidth, ActualHeight);
            Point pos = PointFromScreen(position);
            if (rect.Contains(pos))
            {
                return (DockContent)SelectedItem;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Check if the layout contains the content as direct child</summary>
        /// <param name="content">Content to search for</param>
        /// <returns>True iff the content is child of this control</returns>
        public bool HasChild(IDockContent content)
        {
            return Children.Any(x => x == content);
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
                    AddOneItem((DockContent)nextTo, content);
                    contentEnumerator = tabLayout.Children.GetEnumerator();
                }
            }
            else
            {
                AddOneItem((DockContent)nextTo, newContent as DockContent);
            }
            Focus();
            UpdateLayout();
        }

        /// <summary>
        /// Undock given content</summary>
        /// <param name="content">Content to undock</param>
        public void Undock(IDockContent content)
        {
            foreach (IDockContent contentItem in Children)
            {
                if (content == contentItem)
                {
                    RemoveItem(content as DockContent);
                    if (Parent is IDockLayout && Children.Count == 0)
                    {
                        ((IDockLayout)Parent).Undock((IDockLayout)this);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Undock given child layout</summary>
        /// <param name="child">Child layout to undock</param>
        public void Undock(IDockLayout child)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Replace the old layout with new layout child</summary>
        /// <param name="oldLayout">Old layout to be replaced</param>
        /// <param name="newLayout">New layout that replaces old layout</param>
        public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
        {
        }

        /// <summary>
        /// Close the layout</summary>
        public void Close()
        {
            while (Children.Count > 0)
            {
                RemoveItem(Children[0]);
            }
            ItemsCount = 0;
        }

        /// <summary>
        /// Return the content's parent as an IDockLayout</summary>
        /// <param name="content">The docked content whose parent is requested</param>
        /// <returns>The parent as IDockLayout</returns>
        IDockLayout IDockLayout.FindParentLayout(IDockContent content)
        {
            return (content is DockContent && Children.Contains((DockContent)content)) ? this : null;
        }

        #endregion

        #region INotifyPropertyChanged Members

        #pragma warning disable 0067
        /// <summary>
        /// Property value changed event</summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore 0067

        #endregion
    }
}
