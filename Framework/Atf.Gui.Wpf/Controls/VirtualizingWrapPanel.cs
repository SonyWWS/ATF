//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Sce.Atf.Wpf.Controls
{
    /// <summary>
    /// Arranges and virtualizes content on a panel with wrapping</summary>
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        /// <summary>
        /// Gets and sets the height of the items in the view</summary>
        [TypeConverter(typeof(LengthConverter))]
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        /// <summary>
        /// Dependency property for ItemHeight</summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), 
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets and sets the width of items in the view</summary>
        [TypeConverter(typeof(LengthConverter))]
        public double ItemWidth
        {
            get { return (double)base.GetValue(ItemWidthProperty); }
            set { base.SetValue(ItemWidthProperty, value); }
        }

        /// <summary>
        /// Dependency property for ItemWidth</summary>
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets and sets the orientation for layout of the items</summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Dependency property for Orientation</summary>
        public static readonly DependencyProperty OrientationProperty = 
            StackPanel.OrientationProperty.AddOwner(typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the zoom delta to apply when zooming via the mouse-wheel</summary>
        public double ZoomDelta
        {
            get { return (double)GetValue(ZoomDeltaProperty); }
            set { SetValue(ZoomDeltaProperty, value); }
        }

        /// <summary>
        /// Dependency property that controls the zoom delta to apply when zooming via the mouse-wheel</summary>
        public static readonly DependencyProperty ZoomDeltaProperty =
            DependencyProperty.RegisterAttached("ZoomDelta", typeof(double), typeof(VirtualizingWrapPanel),
                                                new FrameworkPropertyMetadata(10.0d, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                     FrameworkPropertyMetadataOptions.AffectsArrange));
        /// <summary>
        /// Given the item index, scrolls the panel to make that item the first visible</summary>
        /// <param name="index">Index of item to show</param>
        public void SetFirstRowViewItemIndex(int index)
        {
            SetVerticalOffset((index) / Math.Floor((m_viewport.Width) / m_childSize.Width));
            SetHorizontalOffset((index) / Math.Floor((m_viewport.Height) / m_childSize.Height));
        }

        /// <summary>
        /// If items are divided into sections, returns the index of the first visible section</summary>
        /// <returns>Index of the first section that is currently visible</returns>
        public int GetFirstVisibleSection()
        {
            int section = (Orientation == Orientation.Horizontal) 
                ? (int) m_offset.Y 
                : (int) m_offset.X;
            return Math.Min(m_abstractPanel.Max(x => x.Section), section);
        }

        /// <summary>
        /// Gets the index of the first visible item</summary>
        /// <returns>Index of the first visible item</returns>
        public int GetFirstVisibleIndex()
        {
            int section = GetFirstVisibleSection();
            var item = m_abstractPanel.FirstOrDefault(x => x.Section == section);
            
            return (item != null) ? item.Index : 0;
        }

        #region IScrollInfo Members

        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.</summary>
        public bool CanHorizontallyScroll { get; set; }
        
        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the vertical axis is possible.</summary>
        public bool CanVerticallyScroll { get; set; }

        /// <summary>
        /// Gets the vertical size of the extent.</summary>
        public double ExtentHeight
        {
            get { return m_extent.Height; }
        }

        /// <summary>
        /// Gets the horizontal size of the extent.</summary>
        public double ExtentWidth
        {
            get { return m_extent.Width; }
        }

        /// <summary>
        /// Gets the horizontal offset of the scrolled content.</summary>
        public double HorizontalOffset
        {
            get { return m_offset.X; }
        }

        /// <summary>
        /// Gets the vertical offset of the scrolled content.</summary>
        public double VerticalOffset
        {
            get { return m_offset.Y; }
        }

        /// <summary>
        /// Scrolls down within content by one logical unit. </summary>
        public void LineDown()
        {
            if (Orientation == Orientation.Vertical)
                SetVerticalOffset(VerticalOffset + 20);
            else
                SetVerticalOffset(VerticalOffset + 1);
        }

        /// <summary>
        /// Scrolls left within content by one logical unit.</summary>
        public void LineLeft()
        {
            if (Orientation == Orientation.Horizontal)
                SetHorizontalOffset(HorizontalOffset - 20);
            else
                SetHorizontalOffset(HorizontalOffset - 1);
        }

        /// <summary>
        /// Scrolls right within content by one logical unit.</summary>
        public void LineRight()
        {
            if (Orientation == Orientation.Horizontal)
                SetHorizontalOffset(HorizontalOffset + 20);
            else
                SetHorizontalOffset(HorizontalOffset + 1);
        }

        /// <summary>
        /// Scrolls up within content by one logical unit. </summary>
        public void LineUp()
        {
            if (Orientation == Orientation.Vertical)
                SetVerticalOffset(VerticalOffset - 20);
            else
                SetVerticalOffset(VerticalOffset - 1);
        }

        /// <summary>
        /// Forces content to scroll until the coordinate space of a Visual object is visible</summary>
        /// <param name="visual">A Visual that becomes visible.</param>
        /// <param name="rectangle">A bounding rectangle that identifies the coordinate space to make visible.</param>
        /// <returns>A Rect that is visible.</returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var gen = m_generator.GetItemContainerGeneratorForPanel(this);
            var element = (UIElement)visual;
            int itemIndex = gen.IndexFromContainer(element);
            while (itemIndex == -1)
            {
                element = (UIElement)VisualTreeHelper.GetParent(element);
                itemIndex = gen.IndexFromContainer(element);
            }

            //int section = m_abstractPanel[itemIndex].Section;
            Rect elementRect = m_realizedChildLayout[element];
            if (Orientation == Orientation.Horizontal)
            {
                double viewportHeight = m_pixelMeasuredViewport.Height;
                if (elementRect.Bottom > viewportHeight)
                    m_offset.Y += 1;
                else if (elementRect.Top < 0)
                    m_offset.Y -= 1;
            }
            else
            {
                double viewportWidth = m_pixelMeasuredViewport.Width;
                if (elementRect.Right > viewportWidth)
                    m_offset.X += 1;
                else if (elementRect.Left < 0)
                    m_offset.X -= 1;
            }

            InvalidateMeasure();
            return elementRect;
        }

        /// <summary>
        /// Scrolls down within content after a user clicks the wheel button on a mouse.</summary>
        public void MouseWheelDown()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ItemWidth -= ZoomDelta;
                ItemHeight -= ZoomDelta;
            }
            else
            {
                LineDown();
            }
        }

        /// <summary>
        /// Scrolls left within content after a user clicks the wheel button on a mouse.</summary>
        public void MouseWheelLeft()
        {
            LineLeft();
        }

        /// <summary>
        /// Scrolls right within content after a user clicks the wheel button on a mouse.</summary>
        public void MouseWheelRight()
        {
            LineRight();
        }

        /// <summary>
        /// Scrolls up within content after a user clicks the wheel button on a mouse.</summary>
        public void MouseWheelUp()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                ItemWidth += ZoomDelta;
                ItemHeight += ZoomDelta;
            }
            else
            {
                LineUp();
            }
        }

        /// <summary>
        /// Scrolls down within content by one page.</summary>
        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + m_viewport.Height * 0.8);
        }

        /// <summary>
        /// Scrolls left within content by one page.</summary>
        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - m_viewport.Width * 0.8);
        }

        /// <summary>
        /// Scrolls right within content by one page.</summary>
        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + m_viewport.Width * 0.8);
        }

        /// <summary>
        /// Scrolls up within content by one page.</summary>
        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - m_viewport.Height * 0.8);
        }

        /// <summary>
        /// Gets or sets a ScrollViewer element that controls scrolling behavior.</summary>
        public ScrollViewer ScrollOwner { get; set; }

        /// <summary>
        /// Sets the amount of horizontal offset.</summary>
        /// <param name="offset">The degree to which content is horizontally offset from the containing viewport.</param>
        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || m_viewport.Width >= m_extent.Width)
            {
                offset = 0;
            }
            else
            {
                if (offset + m_viewport.Width >= m_extent.Width)
                {
                    offset = m_extent.Width - m_viewport.Width;
                }
            }

            m_offset.X = offset;

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();

            InvalidateMeasure();
            m_firstIndex = GetFirstVisibleIndex();
        }

        /// <summary>
        /// Sets the amount of vertical offset.</summary>
        /// <param name="offset">The degree to which content is vertically offset from the containing viewport.</param>
        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || m_viewport.Height >= m_extent.Height)
            {
                offset = 0;
            }
            else
            {
                if (offset + m_viewport.Height >= m_extent.Height)
                {
                    offset = m_extent.Height - m_viewport.Height;
                }
            }

            m_offset.Y = offset;

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();

            //_trans.Y = -offset;

            InvalidateMeasure();
            m_firstIndex = GetFirstVisibleIndex();
        }

        /// <summary>
        /// Gets the vertical size of the viewport for this content.</summary>
        public double ViewportHeight
        {
            get { return m_viewport.Height; }
        }

        /// <summary>
        /// Gets the horizontal size of the viewport for this content.</summary>
        public double ViewportWidth
        {
            get { return m_viewport.Width; }
        }

        #endregion

        /// <summary>
        /// Gets the permitted size for an item - ItemWidth and ItemHeight if they are set,
        /// otherwise double.PositiveInfinity.</summary>
        protected Size ChildSlotSize
        {
            get
            {
                double itemWidth = this.ItemWidth;
                double itemHeight = this.ItemHeight;
                return new Size(
                    !itemWidth.IsNaN() ? itemWidth : double.PositiveInfinity,
                    !itemHeight.IsNaN() ? itemHeight : double.PositiveInfinity);
            }
        }

        /// <summary>
        /// Removes items that are outside the desired positions</summary>
        /// <param name="minDesiredGenerated">Minimum desired generator position</param>
        /// <param name="maxDesiredGenerated">Maximum desired generator position</param>
        protected void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            for (int i = m_children.Count - 1; i >= 0; i--)
            {
                var childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = m_generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex < minDesiredGenerated || itemIndex > maxDesiredGenerated)
                {
                    m_generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        /// <summary>
        /// Computes the extent and viewport size</summary>
        /// <param name="pixelMeasuredViewportSize">Available size for the viewport</param>
        /// <param name="visibleSections">Number of sections to make visible</param>
        protected void ComputeExtentAndViewport(Size pixelMeasuredViewportSize, int visibleSections)
        {
            if (Orientation == Orientation.Horizontal)
            {
                m_viewport.Height = visibleSections;
                m_viewport.Width = pixelMeasuredViewportSize.Width;
            }
            else
            {
                m_viewport.Width = visibleSections;
                m_viewport.Height = pixelMeasuredViewportSize.Height;
            }

            if (Orientation == Orientation.Horizontal)
            {
                m_extent.Height = m_abstractPanel.SectionCount + ViewportHeight - 1;
            }
            else
            {
                m_extent.Width = m_abstractPanel.SectionCount + ViewportWidth - 1;
            }

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        /// <summary>
        /// Event fired when a key is pressed</summary>
        /// <param name="e">Event args</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    NavigateDown();
                    e.Handled = true;
                    break;
                case Key.Left:
                    NavigateLeft();
                    e.Handled = true;
                    break;
                case Key.Right:
                    NavigateRight();
                    e.Handled = true;
                    break;
                case Key.Up:
                    NavigateUp();
                    e.Handled = true;
                    break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        /// <summary>
        /// Event fired when the Items list changes</summary>
        /// <param name="sender">Originator of the event</param>
        /// <param name="args">Event args</param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            base.OnItemsChanged(sender, args);
            m_abstractPanel = null;
            ResetScrollInfo();
            
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    break;
            }
        }

        /// <summary>
        /// Event fired when the control is initialized.</summary>
        /// <param name="e">Event args</param>
        protected override void OnInitialized(EventArgs e)
        {
            SizeChanged += Resizing;
            
            base.OnInitialized(e);
            
            m_itemsControl = ItemsControl.GetItemsOwner(this);
            m_children = InternalChildren;
            m_generator = ItemContainerGenerator;
        }

        /// <summary>
        /// Measures the child elements of a WrapPanel in anticipation of arranging them during the ArrangeOverride pass.</summary>
        /// <param name="availableSize">The size available to child elements of the wrap panel</param>
        /// <returns>The size required by the WrapPanel and its child elements.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (m_itemsControl == null || m_itemsControl.Items.Count == 0)
                return availableSize;

            if (m_abstractPanel == null)
                m_abstractPanel = new WrapPanelAbstraction(m_itemsControl.Items.Count);

            m_pixelMeasuredViewport = availableSize;

            m_realizedChildLayout.Clear();

            Size realizedFrameSize = availableSize;
            int itemCount = m_itemsControl.Items.Count;
            
            int firstVisibleIndex = GetFirstVisibleIndex();
            var startPos = m_generator.GeneratorPositionFromIndex(firstVisibleIndex);
            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
            int current = firstVisibleIndex;
            int visibleSections = 1;
            using (m_generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                bool stop = false;
                bool isHorizontal = Orientation == Orientation.Horizontal;
                double currentX = 0;
                double currentY = 0;
                double maxItemSize = 0;
                Size slotSize = ChildSlotSize;
                int currentSection = GetFirstVisibleSection();
                while (current < itemCount)
                {
                    bool newlyRealized;

                    // Get or create the child                    
                    var child = m_generator.GenerateNext(out newlyRealized) as UIElement;
                    if (child == null)
                        break;
                    
                    if (newlyRealized)
                    {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= m_children.Count)
                        {
                            base.AddInternalChild(child);
                        }
                        else
                        {
                            base.InsertInternalChild(childIndex, child);
                        }

                        m_generator.PrepareItemContainer(child);
                        
                        child.Measure(slotSize);
                    }
                    else
                    {
                        // The child has already been created, let's be sure it's in the right spot
                        Debug.Assert(child == m_children[childIndex], "Wrong child was generated");

                        child.Measure(slotSize);
                    }

                    m_childSize = child.DesiredSize;
                    Rect childRect = new Rect(new Point(currentX, currentY), m_childSize);
                    if (isHorizontal)
                    {
                        maxItemSize = Math.Max(maxItemSize, childRect.Height);
                        if (childRect.Right > realizedFrameSize.Width) //wrap to a new line
                        {
                            currentY = currentY + maxItemSize;
                            currentX = 0;
                            maxItemSize = childRect.Height;
                            childRect.X = currentX;
                            childRect.Y = currentY;
                            currentSection++;
                            visibleSections++;
                        }
                            
                        if (currentY > realizedFrameSize.Height)
                            stop = true;
                        
                        currentX = childRect.Right;
                    }
                    else
                    {
                        maxItemSize = Math.Max(maxItemSize, childRect.Width);
                        if (childRect.Bottom > realizedFrameSize.Height) //wrap to a new column
                        {
                            currentX = currentX + maxItemSize;
                            currentY = 0;
                            maxItemSize = childRect.Width;
                            childRect.X = currentX;
                            childRect.Y = currentY;
                            currentSection++;
                            visibleSections++;
                        }
                            
                        if (currentX > realizedFrameSize.Width)
                            stop = true;
                        
                        currentY = childRect.Bottom;
                    }
                        
                    m_realizedChildLayout.Add(child, childRect);
                    m_abstractPanel.SetItemSection(current, currentSection);

                    if (stop)
                        break;
                    current++;
                    childIndex++;
                }
            }

            CleanUpItems(firstVisibleIndex, current - 1);
            ComputeExtentAndViewport(availableSize, visibleSections);

            return availableSize;
        }

        /// <summary>
        /// Arranges the child elements of the panel.</summary>
        /// <param name="finalSize">The area in the parent element that the panel should use to arrange its child elements.</param>
        /// <returns>The actual size of the DockPanel after the child elements are arranged. The actual size should always equal finalSize.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (m_children != null)
            {
                foreach (UIElement child in m_children)
                {
                    var layoutInfo = m_realizedChildLayout[child];
                    child.Arrange(layoutInfo);
                }
            }

            return finalSize;
        }

        /// <summary>
        /// Abstraction of an item for use by the WrapPanelAbstraction</summary>
        protected class ItemAbstraction
        {
            /// <summary>
            /// Gets the item index</summary>
            public readonly int Index;

            /// <summary>
            /// Constructor</summary>
            /// <param name="panel">The panel to use</param>
            /// <param name="index">The index of the item</param>
            public ItemAbstraction(WrapPanelAbstraction panel, int index)
            {
                m_panel = panel;
                Index = index;
            }

            /// <summary>
            /// Gets and sets the item's index within the section</summary>
            public int SectionIndex
            {
                get { return (m_sectionIndex == -1) ? (Index % m_panel.AverageItemsPerSection - 1) : m_sectionIndex; }
                set
                {
                    if (m_sectionIndex == -1)
                        m_sectionIndex = value;
                }
            }

            /// <summary>
            /// Gets and sets the item's section</summary>
            public int Section
            {
                get { return (m_section == -1) ? Index / m_panel.AverageItemsPerSection : m_section; }
                set
                {
                    if (m_section == -1)
                        m_section = value;
                }
            }

            private int m_section = -1;
            private int m_sectionIndex = -1;
            private readonly WrapPanelAbstraction m_panel;
        }

        /// <summary>
        /// Helper class to deal with wrap panel logistics</summary>
        protected class WrapPanelAbstraction : IEnumerable<ItemAbstraction>
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="itemCount">Number of items</param>
            public WrapPanelAbstraction(int itemCount)
            {
                var items = new List<ItemAbstraction>(itemCount);
                for (int i = 0; i < itemCount; i++)
                {
                    var item = new ItemAbstraction(this, i);
                    items.Add(item);
                }

                Items = new ReadOnlyCollection<ItemAbstraction>(items);
                AverageItemsPerSection = itemCount;
                ItemCount = itemCount;
            }

            /// <summary>
            /// Number of items</summary>
            public readonly int ItemCount;

            /// <summary>
            /// The average number of items in a section</summary>
            public int AverageItemsPerSection;

            /// <summary>
            /// Gets the number of sections</summary>
            public int SectionCount
            {
                get
                {
                    int ret = m_currentSetSection + 1;
                    if (m_currentSetItemIndex + 1 < Items.Count)
                    {
                        int itemsLeft = Items.Count - m_currentSetItemIndex;
                        ret += itemsLeft / AverageItemsPerSection + 1;
                    }
                    return ret;
                }
            }

            /// <summary>
            /// Assigns an item to a section</summary>
            /// <param name="index">Index of the item</param>
            /// <param name="section">Section to assign the item to</param>
            public void SetItemSection(int index, int section)
            {
                lock (m_syncRoot)
                {
                    if (section <= m_currentSetSection + 1 && index == m_currentSetItemIndex + 1)
                    {
                        m_currentSetItemIndex++;
                        Items[index].Section = section;
                        if (section == m_currentSetSection + 1)
                        {
                            m_currentSetSection = section;
                            if (section > 0)
                            {
                                AverageItemsPerSection = (index) / (section);
                            }
                            m_itemsInCurrentSecction = 1;
                        }
                        else
                            m_itemsInCurrentSecction++;
                        Items[index].SectionIndex = m_itemsInCurrentSecction - 1;
                    }
                }
            }

            /// <summary>
            /// Array indexer extension</summary>
            /// <param name="index">Index of item to retrieve</param>
            /// <returns>The item with the specified index</returns>
            public ItemAbstraction this[int index]
            {
                get { return Items[index]; }
            }

            #region IEnumerable<ItemAbstraction> Members

            /// <summary>
            /// Returns an enumerator that iterates through a collection</summary>
            /// <returns>Enumerator that iterates through a collection</returns>
            public IEnumerator<ItemAbstraction> GetEnumerator()
            {
                return Items.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            private ReadOnlyCollection<ItemAbstraction> Items { get; set; }

            private int m_currentSetSection = -1;
            private int m_currentSetItemIndex = -1;
            private int m_itemsInCurrentSecction = 0;
            private readonly object m_syncRoot = new object();

        }

        private void Resizing(object sender, EventArgs e)
        {
            if (m_viewport.Width > 0.0)
            {
                int firstIndexCache = m_firstIndex;
                m_abstractPanel = null;
                MeasureOverride(m_viewport);
                SetFirstRowViewItemIndex(m_firstIndex);
                m_firstIndex = firstIndexCache;
            }
        }

        private void ResetScrollInfo()
        {
            m_offset.X = 0;
            m_offset.Y = 0;
        }

        private int GetNextSectionClosestIndex(int itemIndex)
        {
            var abstractItem = m_abstractPanel[itemIndex];
            if (abstractItem.Section < m_abstractPanel.SectionCount - 1)
            {
                var ret = m_abstractPanel.
                    Where(x => x.Section == abstractItem.Section + 1).
                    OrderBy(x => Math.Abs(x.SectionIndex - abstractItem.SectionIndex)).
                    FirstOrDefault();
                return ret != null ? ret.Index : -1;
            }

            return itemIndex;
        }

        private int GetLastSectionClosestIndex(int itemIndex)
        {
            var abstractItem = m_abstractPanel[itemIndex];
            if (abstractItem.Section > 0)
            {
                var ret = m_abstractPanel.
                    Where(x => x.Section == abstractItem.Section - 1).
                    OrderBy(x => Math.Abs(x.SectionIndex - abstractItem.SectionIndex)).
                    FirstOrDefault();
                return ret != null ? ret.Index : -1;
            }

            return itemIndex;
        }

        private void NavigateDown()
        {
            var gen = m_generator.GetItemContainerGeneratorForPanel(this);
            var selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }

            DependencyObject next = null;
            if (Orientation == Orientation.Horizontal)
            {
                int nextIndex = GetNextSectionClosestIndex(itemIndex);
                if (nextIndex < 0)
                    return;

                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == m_abstractPanel.ItemCount - 1)
                    return;
                next = gen.ContainerFromIndex(itemIndex + 1);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex + 1);
                }
            }

            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }

            (next as UIElement).Focus();
        }

        private void NavigateLeft()
        {
            var gen = m_generator.GetItemContainerGeneratorForPanel(this);
            var selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }

            DependencyObject next = null;
            if (Orientation == Orientation.Vertical)
            {
                int nextIndex = GetLastSectionClosestIndex(itemIndex);
                if (nextIndex < 0)
                    return;

                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == 0)
                    return;
                next = gen.ContainerFromIndex(itemIndex - 1);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex - 1);
                }
            }

            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }

            (next as UIElement).Focus();
        }

        private void NavigateRight()
        {
            var gen = m_generator.GetItemContainerGeneratorForPanel(this);
            var selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }

            DependencyObject next = null;
            if (Orientation == Orientation.Vertical)
            {
                int nextIndex = GetNextSectionClosestIndex(itemIndex);
                if (nextIndex < 0)
                    return;

                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == m_abstractPanel.ItemCount - 1)
                    return;
                next = gen.ContainerFromIndex(itemIndex + 1);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset + 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex + 1);
                }
            }

            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }

            (next as UIElement).Focus();
        }

        private void NavigateUp()
        {
            var gen = m_generator.GetItemContainerGeneratorForPanel(this);
            var selected = (UIElement)Keyboard.FocusedElement;
            int itemIndex = gen.IndexFromContainer(selected);
            int depth = 0;
            while (itemIndex == -1)
            {
                selected = (UIElement)VisualTreeHelper.GetParent(selected);
                itemIndex = gen.IndexFromContainer(selected);
                depth++;
            }

            DependencyObject next = null;
            if (Orientation == Orientation.Horizontal)
            {
                int nextIndex = GetLastSectionClosestIndex(itemIndex);
                if (nextIndex < 0)
                    return;

                next = gen.ContainerFromIndex(nextIndex);
                while (next == null)
                {
                    SetVerticalOffset(VerticalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(nextIndex);
                }
            }
            else
            {
                if (itemIndex == 0)
                    return;
                next = gen.ContainerFromIndex(itemIndex - 1);
                while (next == null)
                {
                    SetHorizontalOffset(HorizontalOffset - 1);
                    UpdateLayout();
                    next = gen.ContainerFromIndex(itemIndex - 1);
                }
            }

            while (depth != 0)
            {
                next = VisualTreeHelper.GetChild(next, 0);
                depth--;
            }

            (next as UIElement).Focus();
        }

        private UIElementCollection m_children;
        private IItemContainerGenerator m_generator;
        private ItemsControl m_itemsControl;
        private Size m_pixelMeasuredViewport = new Size(0, 0);
        private Size m_childSize;
        private WrapPanelAbstraction m_abstractPanel;
        private Point m_offset = new Point(0, 0);
        private Size m_extent = new Size(0, 0);
        private Size m_viewport = new Size(0, 0);
        private int m_firstIndex = 0;
        private readonly Dictionary<UIElement, Rect> m_realizedChildLayout = new Dictionary<UIElement, Rect>();
    }
}
