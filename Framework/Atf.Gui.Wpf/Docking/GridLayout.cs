//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System.Xml.Serialization;
using System.Xml;

namespace Sce.Atf.Wpf.Docking
{
    /// <summary>
    /// extended Dock enum, with extra entry Center
    /// </summary>
    public enum DockTo
    {
        Left,
        Right,
        Top,
        Bottom,
        Center
    }
    /// <summary>
    /// Layout that lays its children horizontally or vertically separated with separators. Each child
    /// is DockedWindow, or another GridLayout.
    /// </summary>
    internal class GridLayout : Grid, IDockLayout, IXmlSerializable
    {
        private Size m_minGridSize = new Size(40, 20);
        private List<IDockLayout> m_children;
        /// <summary>
        /// List of children layouts
        /// </summary>
        public List<IDockLayout> Layouts
        {
            get { return m_children; }
        }
        private Orientation m_orientation;
        /// <summary>
        /// Orientation of this layout, either horizontal or vertical
        /// </summary>
        public Orientation Orientation
        {
            get { return m_orientation; }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dockPanel">Parent dock panel</param>
        public GridLayout(DockPanel dockPanel)
        {
            Root = dockPanel;
            //Background = Root.Background;
            SetZIndex(this, 0);
            m_children = new List<IDockLayout>();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dockPanel">Parent dock panel</param>
        /// <param name="child">First child</param>
        public GridLayout(DockPanel dockPanel, IDockLayout child)
            : this(dockPanel)
        {
            AddFirstChild(child);
        }
        /// <summary>
        /// Constructor when deserializing
        /// </summary>
        /// <param name="dockPanel">Parent dock panel</param>
        /// <param name="reader">Source xml</param>
        public GridLayout(DockPanel dockPanel, XmlReader reader)
            : this(dockPanel)
        {
            ReadXml(reader);
        }
        /// <summary>
        /// When first child is added, it sets up first properties
        /// </summary>
        /// <param name="child">Child to add</param>
        private void AddFirstChild(IDockLayout child)
        {
            m_children.Add(child);
            Children.Add((FrameworkElement)child);
            ColumnDefinitions.Add(NewColumnDefinition(new GridLength(1, GridUnitType.Star), m_minGridSize.Width));
            RowDefinitions.Add(NewRowDefinition(new GridLength(1, GridUnitType.Star), m_minGridSize.Height));
            ((FrameworkElement)child).SetValue(Grid.ColumnProperty, 0);
            ((FrameworkElement)child).SetValue(Grid.RowProperty, 0);
        }
        /// <summary>
        /// Will create and return new row definition with given height and minimum height boundary
        /// </summary>
        /// <param name="length">Length (height) of the row</param>
        /// <param name="minHeight">Minimum height of the row</param>
        /// <returns>New row definition</returns>
        private RowDefinition NewRowDefinition(GridLength length, double minHeight)
        {
            RowDefinition rowDefinition = new RowDefinition();
            rowDefinition.Height = length;
            rowDefinition.MinHeight = minHeight;
            return rowDefinition;
        }
        /// <summary>
        /// Will create and return new column definition with given width and minimum width boundary
        /// </summary>
        /// <param name="length">Length (width) of the column</param>
        /// <param name="minHeight">Minimum width of the column</param>
        /// <returns>New column definition</returns>
        private ColumnDefinition NewColumnDefinition(GridLength length, double minWidth)
        {
            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = length;
            columnDefinition.MinWidth = minWidth;
            return columnDefinition;
        }
        /// <summary>
        /// Will return new grid splitter
        /// </summary>
        /// <param name="orientation">Orientation (horizontal or vertical)</param>
        /// <returns>New grid splitter</returns>
        private GridSplitter NewGridSplitter(Orientation orientation)
        {
            GridSplitter splitter = new GridSplitter();
            splitter.Style = TryFindResource(DockPanel.GridSplitterStyleKey) as Style;
            //splitter.Background = Brushes.Transparent;
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            splitter.VerticalAlignment = VerticalAlignment.Stretch;
            splitter.SnapsToDevicePixels = true;
            splitter.Focusable = false;
            splitter.ResizeDirection = orientation == Orientation.Horizontal ? GridResizeDirection.Columns : GridResizeDirection.Rows;
            return splitter;
        }
        /// <summary>
        /// Will merge itself with given grid. Basicly it takes child grid, removes that grid, and all
        /// children of removed grid will be added to this grid. The child grid must have same orientation
        /// or maximum of 1 child. 
        /// </summary>
        /// <param name="grid">Grid to merge with</param>
        internal void MergeWith(GridLayout grid)
        {
            // get the index of the grid to merge with (it is child of this grid)
            int index = m_children.IndexOf(grid);
            double totalValue = 0;
            int indexOffset = 0;
            if (Orientation == Orientation.Horizontal)
            {
                // get the column definition of child grid
                ColumnDefinition oldColumnDef = ColumnDefinitions[index * 2];
                // count the sum of columns widths of childs children
                foreach (IDockLayout newLayout in grid.Layouts)
                {
                    FrameworkElement element = (FrameworkElement)newLayout;
                    int columnIndex = Grid.GetColumn(element);
                    ColumnDefinition layoutColumnDef = grid.ColumnDefinitions[columnIndex];
                    totalValue += layoutColumnDef.Width.Value;
                }
                // remove the child from our list, from view children adn from column definitions
                m_children.RemoveAt(index);
                Children.RemoveAt(index * 2);
                ColumnDefinitions.RemoveAt(index * 2);
                // move all childs children to this grid and create column definitions for each of them
                foreach (IDockLayout newLayout in grid.Layouts)
                {
                    FrameworkElement element = (FrameworkElement)newLayout;
                    int columnIndex = Grid.GetColumn(element);
                    ColumnDefinition layoutColumnDef = grid.ColumnDefinitions[columnIndex];
                    double newValue = oldColumnDef.Width.Value * layoutColumnDef.Width.Value / totalValue;
                    ColumnDefinition newColumnDef = NewColumnDefinition(new GridLength(newValue, GridUnitType.Star), m_minGridSize.Width);
                    grid.Children.Remove(element);
                    ColumnDefinitions.Insert((index + indexOffset) * 2, newColumnDef);
                    Children.Insert((index + indexOffset) * 2, element);
                    m_children.Insert(index + indexOffset, newLayout);
                    if (indexOffset < grid.Layouts.Count - 1)
                    {
                        // if we move more than one child, then we need to add new splitters too
                        GridSplitter splitter = NewGridSplitter(Orientation.Horizontal);
                        ColumnDefinitions.Insert((index + indexOffset) * 2 + 1, NewColumnDefinition(new GridLength(1, GridUnitType.Auto), 0));
                        Children.Insert((index + indexOffset) * 2 + 1, splitter);
                    }
                    indexOffset ++;
                }
                // set the association of column indexes
                for (int i = 0; i < Children.Count; i++)
                {
                    Grid.SetColumn(Children[i], i);
                }
            }
            else
            {
                // get the row definition of child grid
                RowDefinition oldRowDef = RowDefinitions[index * 2];
                // count the sum of row heights of childs children
                foreach (IDockLayout newLayout in grid.Layouts)
                {
                    FrameworkElement element = (FrameworkElement)newLayout;
                    int RowIndex = Grid.GetRow(element);
                    RowDefinition layoutRowDef = grid.RowDefinitions[RowIndex];
                    totalValue += layoutRowDef.Height.Value;
                }
                // remove the child from our list, from view children adn from row definitions
                m_children.RemoveAt(index);
                Children.RemoveAt(index * 2);
                RowDefinitions.RemoveAt(index * 2);
                // move all childs children to this grid and create row definitions for each of them
                foreach (IDockLayout newLayout in grid.Layouts)
                {
                    FrameworkElement element = (FrameworkElement)newLayout;
                    int RowIndex = Grid.GetRow(element);
                    RowDefinition layoutRowDef = grid.RowDefinitions[RowIndex];
                    double newValue = oldRowDef.Height.Value * layoutRowDef.Height.Value / totalValue;
                    RowDefinition newRowDef = NewRowDefinition(new GridLength(newValue, GridUnitType.Star), m_minGridSize.Height);
                    grid.Children.Remove(element);
                    RowDefinitions.Insert((index + indexOffset) * 2, newRowDef);
                    Children.Insert((index + indexOffset) * 2, element);
                    m_children.Insert(index + indexOffset, newLayout);
                    if (indexOffset < grid.Layouts.Count - 1)
                    {
                        // if we move more than one child, then we need to add new splitters too
                        GridSplitter splitter = NewGridSplitter(Orientation.Vertical);
                        RowDefinitions.Insert((index + indexOffset) * 2 + 1, NewRowDefinition(new GridLength(1, GridUnitType.Auto), 0));
                        Children.Insert((index + indexOffset)* 2 + 1, splitter);
                    }
                    indexOffset ++;
                }
                // set the association of row indexes
                for (int i = 0; i < Children.Count; i++)
                {
                    Grid.SetRow(Children[i], i);
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
            Rect rect = new Rect(0, 0, ActualWidth, ActualHeight);
            Point pos = PointFromScreen(position);
            if (rect.Contains(pos))
            {
                foreach (IDockLayout content in m_children)
                {
                    DockContent result = content.HitTest(position);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public bool HasChild(IDockContent content)
        {
            // Grid doesn't have own dockable contents
            return false;
        }

        public bool HasDescendant(IDockContent content)
        {
            foreach (IDockLayout child in m_children)
            {
                if (child.HasDescendant(content))
                {
                    return true;
                }
            }
            return false;
        }

        public void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
        {
            IDockLayout targetChild = null;
            if (nextTo != null)
            {
                foreach (IDockLayout child in m_children)
                {
                    if (child.HasChild(nextTo))
                    {
                        targetChild = child;
                        break;
                    }
                }
            }
            if (targetChild == null)
            {
                foreach (IDockLayout child in m_children)
                {
                    if (child.HasDescendant(nextTo))
                    {
                        child.Dock(nextTo, newContent, dockTo);
                        // child was docked, nothing else is necessary
                        return;
                    }
                }
            }
            if (dockTo == DockTo.Center && m_children.Count == 1)
            {
                m_children[0].Dock(null, newContent, dockTo);
            }
            else if (m_children.Count < 2)
            {
                if (dockTo == DockTo.Center)
                {
                    dockTo = DockTo.Right;
                }
                if (dockTo == DockTo.Top || dockTo == DockTo.Bottom)
                {
                    m_orientation = Orientation.Vertical;
                }
                else
                {
                    m_orientation = Orientation.Horizontal;
                }
                DockedWindow newChild = new DockedWindow(Root, newContent);
                if (Children.Count == 0)
                {
                    AddFirstChild(newChild);
                }
                else
                {
                    if (targetChild == null)
                    {
                        if (dockTo == DockTo.Top || dockTo == DockTo.Left)
                        {
                            targetChild = m_children[0];
                        }
                        else
                        {
                            targetChild = m_children[m_children.Count - 1];
                        }
                    }
                    FrameworkElement control = (FrameworkElement)targetChild;
                    int index = m_children.IndexOf(targetChild);
                    if (dockTo == DockTo.Left || dockTo == DockTo.Right)
                    {
                        GridSplitter splitter = NewGridSplitter(Orientation.Horizontal);
                        int column = (int)control.GetValue(Grid.ColumnProperty);
                        ColumnDefinition oldColumn = ColumnDefinitions[index * 2];

                        ContentSettings contentSettings = (newContent is TabLayout) ? ((TabLayout)newContent).Children[0].Settings : ((DockContent)newContent).Settings;
                        double totalWidth = ((FrameworkElement)targetChild).ActualWidth;
                        double width = Math.Max(Math.Min(contentSettings.Size.Width, (totalWidth - splitter.Width) / 2), (totalWidth - splitter.Width) / 5);
                        double ratioNew = width / totalWidth;
                        double ratioOld = (totalWidth - width - splitter.Width) / totalWidth;

                        if (dockTo == DockTo.Left)
                        {
                            ColumnDefinition leftColumn = NewColumnDefinition(new GridLength(oldColumn.Width.Value * ratioNew, oldColumn.Width.GridUnitType), m_minGridSize.Width);
                            ColumnDefinition rightColumn = NewColumnDefinition(new GridLength(oldColumn.Width.Value * ratioOld, oldColumn.Width.GridUnitType), m_minGridSize.Width);
                            ColumnDefinitions[index * 2] = leftColumn;
                            ColumnDefinitions.Insert(index * 2 + 1, rightColumn);
                            ColumnDefinitions.Insert(index * 2 + 1, NewColumnDefinition(new GridLength(1, GridUnitType.Auto), 0));
                            m_children.Insert(index, newChild);
                            
                            Children.Insert(index * 2, splitter);
                            Children.Insert(index * 2, newChild);
                        }
                        else
                        {
                            ColumnDefinition leftColumn = NewColumnDefinition(new GridLength(oldColumn.Width.Value * ratioOld, oldColumn.Width.GridUnitType), m_minGridSize.Width);
                            ColumnDefinition rightColumn = NewColumnDefinition(new GridLength(oldColumn.Width.Value * ratioNew, oldColumn.Width.GridUnitType), m_minGridSize.Width);
                            ColumnDefinitions[index * 2] = leftColumn;
                            ColumnDefinitions.Insert(index * 2 + 1, rightColumn);
                            ColumnDefinitions.Insert(index * 2 + 1, NewColumnDefinition(new GridLength(1, GridUnitType.Auto), 0));
                            m_children.Insert(index + 1, newChild);
                            Children.Insert(index * 2 + 1, newChild);
                            Children.Insert(index * 2 + 1, splitter);
                        }
                        for (int i = index * 2; i < Children.Count; i++)
                        {
                            Grid.SetColumn(Children[i], i);
                        }
                    }
                    else
                    {
                        GridSplitter splitter = NewGridSplitter(Orientation.Vertical);
                        int row = (int)control.GetValue(Grid.RowProperty);
                        RowDefinition oldRow = RowDefinitions[index * 2];

                        ContentSettings contentSettings = (newContent is TabLayout) ? ((TabLayout)newContent).Children[0].Settings : ((DockContent)newContent).Settings;
                        double totalHeight = ((FrameworkElement)targetChild).ActualHeight;
                        double height = Math.Max(Math.Min(contentSettings.Size.Height, (totalHeight - splitter.Height) / 2), (totalHeight - splitter.Height) / 5);
                        double ratioNew = height / totalHeight;
                        double ratioOld = (totalHeight - height - splitter.Height) / totalHeight;

                        if (dockTo == DockTo.Top)
                        {
                            RowDefinition topRow = NewRowDefinition(new GridLength(oldRow.Height.Value * ratioNew, oldRow.Height.GridUnitType), m_minGridSize.Height);
                            RowDefinition bottomRow = NewRowDefinition(new GridLength(oldRow.Height.Value *ratioOld, oldRow.Height.GridUnitType), m_minGridSize.Height);
                            RowDefinitions[index * 2] = topRow;
                            RowDefinitions.Insert(index * 2 + 1, bottomRow);
                            RowDefinitions.Insert(index * 2 + 1, NewRowDefinition(new GridLength(1, GridUnitType.Auto), 0));
                            m_children.Insert(index, newChild);
                            Children.Insert(index * 2, splitter);
                            Children.Insert(index * 2, newChild);
                        }
                        else
                        {
                            RowDefinition topRow = NewRowDefinition(new GridLength(oldRow.Height.Value * ratioOld, oldRow.Height.GridUnitType), m_minGridSize.Height);
                            RowDefinition bottomRow = NewRowDefinition(new GridLength(oldRow.Height.Value * ratioNew, oldRow.Height.GridUnitType), m_minGridSize.Height);
                            RowDefinitions[index * 2] = topRow;
                            RowDefinitions.Insert(index * 2 + 1, bottomRow);
                            RowDefinitions.Insert(index * 2 + 1, NewRowDefinition(new GridLength(1, GridUnitType.Auto), 0));
                            m_children.Insert(index + 1, newChild);
                            Children.Insert(index * 2 + 1, newChild);
                            Children.Insert(index * 2 + 1, splitter);
                        }
                        for (int i = index * 2; i < Children.Count; i++)
                        {
                            Grid.SetRow(Children[i], i);
                        }
                    }
                }
            }
            else if (dockTo == DockTo.Left || dockTo == DockTo.Right || dockTo == DockTo.Top || dockTo == DockTo.Bottom)
            {
                DockedWindow dockedWindow = (DockedWindow)targetChild;
                int index = m_children.IndexOf(targetChild);
                GridLayout gridLayout = new GridLayout(Root);
                gridLayout.SetValue(Grid.ColumnProperty, dockedWindow.GetValue(Grid.ColumnProperty));
                gridLayout.SetValue(Grid.RowProperty, dockedWindow.GetValue(Grid.RowProperty));
                Children.Remove(dockedWindow);
                IDockContent content = dockedWindow.DockedContent;
                dockedWindow.Undock(content);
                m_children[index] = gridLayout;
                Children.Insert(index * 2, gridLayout);
                gridLayout.Dock(null, content, DockTo.Center);
                UpdateLayout();
                gridLayout.Dock(content, newContent, dockTo);
            }
            else if (targetChild != null)
            {
                targetChild.Dock(nextTo, newContent, dockTo);
            }
        }

        public void Undock(IDockContent content)
        {
            IDockLayout targetChild = null;
            foreach (IDockLayout child in m_children)
            {
                if (child.HasChild(content))
                {
                    targetChild = child;
                    break;
                }
            }
            if (targetChild != null)
            {
                targetChild.Undock(content);
            }
            else
            {
                foreach (IDockLayout child in m_children)
                {
                    if (child.HasDescendant(content))
                    {
                        targetChild = child;
                        break;
                    }
                }
                if (targetChild != null)
                {
                    targetChild.Undock(content);
                }
            }
        }

        public void Undock(IDockLayout child)
        {
            int index = m_children.IndexOf(child);
            if (Orientation == Orientation.Horizontal)
            {
                ColumnDefinitions.RemoveAt(index * 2);
                Children.RemoveAt(index * 2);
                if (m_children.Count > 1)
                {
                    ColumnDefinitions.RemoveAt(index * 2 + (index == 0 ? 0 : -1));
                    Children.RemoveAt(index * 2 + (index == 0 ? 0 : -1));

                    for (int i = 0; i < Children.Count; i++)
                    {
                        Grid.SetColumn(Children[i], i);
                    }
                }
            }
            else
            {
                RowDefinitions.RemoveAt(index * 2);
                Children.RemoveAt(index * 2);
                if (m_children.Count > 1)
                {
                    RowDefinitions.RemoveAt(index * 2 + (index == 0 ? 0 : -1));
                    Children.RemoveAt(index * 2 + (index == 0 ? 0 : -1));
                    for (int i = 0; i < Children.Count; i++)
                    {
                        Grid.SetRow(Children[i], i);
                    }
                }
            }
            m_children.RemoveAt(index);

            Root.CheckConsistency();
        }

        public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
        {
            FrameworkElement element = (FrameworkElement)newLayout;
            element.SetValue(Grid.ColumnProperty, (oldLayout as FrameworkElement).GetValue(Grid.ColumnProperty));
            element.SetValue(Grid.RowProperty, (oldLayout as FrameworkElement).GetValue(Grid.RowProperty));
            m_children[m_children.IndexOf(oldLayout)] = newLayout;
            int index = Children.IndexOf((FrameworkElement)oldLayout);
            Children.RemoveAt(index);
            Children.Insert(index, (FrameworkElement)newLayout);
        }

        internal bool CheckConsistency()
        {
            IEnumerator<IDockLayout> layoutEn = m_children.GetEnumerator();
            while (layoutEn.MoveNext())
            {
                GridLayout layout = layoutEn.Current as GridLayout;
                if(layout != null)
                {
                    if (!layout.CheckConsistency())
                    {
                        layoutEn = m_children.GetEnumerator();
                    }
                }
            }
            if (m_children.Count == 0)
            {
                if (Parent != null)
                {
                    ((IDockLayout)Parent).Undock(this);
                }
                else
                {
                    ((IDockLayout)Root).Undock(this);
                }
                return false;
            }
            else if (m_children.Count == 1)
            {
                if (Parent is IDockLayout || m_children[0] is IDockLayout)
                {
                    IDockLayout lastChild = m_children[0];
                    if (!(lastChild is DockedWindow && Parent == null))
                    {
                        m_children.Clear();
                        Children.Clear();
                        if (Parent != null)
                        {
                            ((IDockLayout)Parent).Replace(this, lastChild);
                        }
                        else
                        {
                            ((IDockLayout)Root).Replace(this, lastChild);
                        }
                        return false;
                    }
                }
            }
            else if (Parent is GridLayout && Orientation == ((GridLayout)Parent).Orientation)
            {
                ((GridLayout)Parent).MergeWith(this);
                return false;
            }
            return true;
        }

        public void Close()
        {
            foreach (IDockLayout layout in m_children)
            {
                layout.Close();
            }
            m_children.Clear();
            Children.Clear();
        }

        IDockLayout IDockLayout.FindParentLayout(IDockContent content)
        {
            foreach (IDockLayout child in m_children)
            {
                IDockLayout found = null;
                if ((found = child.FindParentLayout(content)) != null)
                {
                    return found;
                }
            }
            return null;
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
                String s = reader.GetAttribute(Orientation.GetType().Name);
                m_orientation = (Orientation)(Enum.Parse(Orientation.GetType(), s));
                switch (m_orientation)
                {
                    case Orientation.Horizontal:
                        if (reader.ReadToDescendant("Column"))
                        {
                            RowDefinitions.Add(NewRowDefinition(new GridLength(1, GridUnitType.Star), m_minGridSize.Height));
                            do
                            {
                                double width = double.Parse(reader.GetAttribute("Width"));
                                IDockLayout layout = null;
                                reader.ReadStartElement();
                                if (reader.LocalName == typeof(DockedWindow).Name)
                                {
                                    DockedWindow dockedWindow = new DockedWindow(Root, reader.ReadSubtree());
                                    layout = dockedWindow.DockedContent.Children.Count != 0 ? dockedWindow : null;
                                    reader.ReadEndElement();
                                }
                                else if (reader.LocalName == typeof(GridLayout).Name)
                                {
                                    GridLayout gridLayout = new GridLayout(Root, reader.ReadSubtree());
                                    layout = gridLayout.Layouts.Count > 0 ? gridLayout : null;
                                    reader.ReadEndElement();
                                }
                                if (layout != null)
                                {
                                    if (Children.Count > 0)
                                    {
                                        ColumnDefinitions.Add(NewColumnDefinition(new GridLength(1, GridUnitType.Auto), 0));
                                        Children.Add(NewGridSplitter(Orientation));
                                    }
                                    ColumnDefinitions.Add(NewColumnDefinition(new GridLength(width, GridUnitType.Star), m_minGridSize.Width));
                                    m_children.Add(layout);
                                    Children.Add((FrameworkElement)layout);
                                }
                            } while (reader.ReadToNextSibling("Column"));
                        }
                        break;
                    case Orientation.Vertical:
                        if (reader.ReadToDescendant("Row"))
                        {
                            ColumnDefinitions.Add(NewColumnDefinition(new GridLength(1, GridUnitType.Star), m_minGridSize.Width));
                            do
                            {
                                double height = double.Parse(reader.GetAttribute("Height"));
                                IDockLayout layout = null;
                                reader.ReadStartElement();
                                if (reader.LocalName == typeof(DockedWindow).Name)
                                {
                                    DockedWindow dockedWindow = new DockedWindow(Root, reader.ReadSubtree());
                                    layout = dockedWindow.DockedContent.Children.Count != 0 ? dockedWindow : null;
                                    reader.ReadEndElement();
                                }
                                else if (reader.LocalName == typeof(GridLayout).Name)
                                {
                                    GridLayout gridLayout = new GridLayout(Root, reader.ReadSubtree());
                                    layout = gridLayout.Layouts.Count > 0 ? gridLayout : null;
                                    reader.ReadEndElement();
                                }
                                if (layout != null)
                                {
                                    if (Children.Count > 0)
                                    {
                                        RowDefinitions.Add(NewRowDefinition(new GridLength(1, GridUnitType.Auto), 0));
                                        Children.Add(NewGridSplitter(Orientation));
                                    }
                                    RowDefinitions.Add(NewRowDefinition(new GridLength(height, GridUnitType.Star), m_minGridSize.Height));
                                    m_children.Add(layout);
                                    Children.Add((FrameworkElement)layout);
                                }
                            } while (reader.ReadToNextSibling("Row"));
                        }
                        break;
                }
                for(int i = 0; i < Children.Count; i++)
                {
                    Grid.SetColumn(Children[i], Orientation == Orientation.Horizontal ? i : 0);
                    Grid.SetRow(Children[i], Orientation == Orientation.Vertical ? i : 0);
                }
                reader.ReadEndElement();
            }
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(this.GetType().Name);
            writer.WriteAttributeString(Orientation.GetType().Name, Orientation.ToString());
            switch (Orientation)
            {
                case Orientation.Horizontal:
                    foreach (IXmlSerializable layout in m_children)
                    {
                        writer.WriteStartElement("Column");
                        writer.WriteAttributeString("Width", ColumnDefinitions[Grid.GetColumn((FrameworkElement)layout)].Width.Value.ToString());
                        layout.WriteXml(writer);
                        writer.WriteEndElement();
                    }
                    break;
                case Orientation.Vertical:
                    foreach (IXmlSerializable layout in m_children)
                    {
                        writer.WriteStartElement("Row");
                        writer.WriteAttributeString("Height", RowDefinitions[Grid.GetRow((FrameworkElement)layout)].Height.Value.ToString());
                        layout.WriteXml(writer);
                        writer.WriteEndElement();
                    }
                    break;
            }
            writer.WriteEndElement();
        }

        #endregion
    }
}
