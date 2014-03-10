//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;

using Sce.Atf.Applications;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable
{
    /// <summary>
    /// Column header information</summary>
    public class ColumnInfo
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="columnId">Column ID</param>
        public ColumnInfo(object columnId)
        {
            m_columnId = columnId;
        }

        /// <summary>
        /// Gets ID</summary>
        public object Id
        {
            get { return m_columnId; }
        }

        /// <summary>
        /// Gets or sets location</summary>
        public int Location
        {
            get { return m_location; }
            set { m_location = value; }
        }

        /// <summary>
        /// Gets or sets width</summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        /// Gets or sets list sort direction</summary>
        public ListSortDirection SortDirection
        {
            get { return m_sortDirection; }
            set { m_sortDirection = value; }
        }

        private object m_columnId;

        public int m_location;

        public const int DefaultColumnWidth = 100;
        private int m_width = DefaultColumnWidth;

        private ListSortDirection m_sortDirection = ListSortDirection.Ascending;
    }

    /// <summary>
    /// Column event args</summary>
    public class ColumnEventArgs : EventArgs
    {
        public ColumnEventArgs(ColumnInfo column)
        {
            Column = column;
        }

        public readonly ColumnInfo Column;
    }

    /// <summary>
    /// Interface for column header adaptation</summary>
    public interface IColumnHeadersAdapter
    {
        /// <summary>
        /// Set columns</summary>
        /// <param name="columnIds">Column IDs</param>
        void SetColumns(IEnumerable<object> columnIds);

        /// <summary>
        /// Gets columns</summary>
        IEnumerable<ColumnInfo> Columns
        {
            get;
        }

        /// <summary>
        /// Event that is raised  when columns are reloaded</summary>
        event EventHandler Reloaded;

        /// <summary>
        /// Event that is raised  when columns are moved</summary>
        event EventHandler<ColumnEventArgs> ColumnMoved;

        /// <summary>
        /// Event that is raised  when columns are resized</summary>
        event EventHandler<ColumnEventArgs> ColumnResized;

        /// <summary>
        /// Event that is raised  when columns' sort direction changed</summary>
        event EventHandler<ColumnEventArgs> ColumnSortDirectionChanged;
    }

    /// <summary>
    /// Class for column header control adapters</summary>
    public class ColumnHeadersAdapter : ControlAdapter, IColumnHeadersAdapter
    {
        /// <summary>
        /// Constructor</summary>
        public ColumnHeadersAdapter()
        {
            m_columnHeaders = new ColumnHeaders(this);
            m_columnHeaders.Dock = DockStyle.Top;

            if (ColumnMoved == null) return;
            if (ColumnResized == null) return;
            if (ColumnSortDirectionChanged == null) return;
        }

        #region IColumnHeadersAdapter Members

        /// <summary>
        /// Set columns</summary>
        /// <param name="columnIds">Column IDs</param>
        public void SetColumns(IEnumerable<object> columnIds)
        {
            m_columns.Clear();
            foreach (object columnId in columnIds)
                m_columns.Add(new ColumnInfo(columnId));

            Event.Raise(Reloaded, this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets columns</summary>
        public IEnumerable<ColumnInfo> Columns
        {
            get { return m_columns; }
        }

        /// <summary>
        /// Event that is raised  when columns are reloaded</summary>
        public event EventHandler Reloaded;

        /// <summary>
        /// Event that is raised  when columns are moved</summary>
        public event EventHandler<ColumnEventArgs> ColumnMoved;

        /// <summary>
        /// Event that is raised  when columns are resized</summary>
        public event EventHandler<ColumnEventArgs> ColumnResized;

        /// <summary>
        /// Event that is raised  when columns' sort direction changed</summary>
        public event EventHandler<ColumnEventArgs> ColumnSortDirectionChanged;

        #endregion

        /// <summary>
        /// Binds the adapter to the adaptable control; called in the order that the adapters
        /// were defined on the control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Bind(AdaptableControl control)
        {
            m_transformAdapter = control.As<ITransformAdapter>();

            control.Controls.Add(m_columnHeaders);

            m_columnHeaders.Height = control.Font.Height;

            control.ContextChanged += new EventHandler(control_ContextChanged);
            control.FontChanged += new EventHandler(control_FontChanged);
        }

        /// <summary>
        /// Unbinds the adapter from the adaptable control</summary>
        /// <param name="control">Adaptable control</param>
        protected override void Unbind(AdaptableControl control)
        {
            control.ContextChanged -= new EventHandler(control_ContextChanged);
            control.FontChanged -= new EventHandler(control_FontChanged);
        }

        private void control_ContextChanged(object sender, EventArgs e)
        {
            
        }

        private void control_FontChanged(object sender, EventArgs e)
        {
            m_columnHeaders.Height = AdaptedControl.Font.Height;
        }

        #region ColumnHeaders Private Class

        private class ColumnHeaders : Control
        {
            public ColumnHeaders(ColumnHeadersAdapter columnHeadersAdapter)
            {
                m_columnHeadersAdapter = columnHeadersAdapter;

                base.DoubleBuffered = true;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                Font font = m_columnHeadersAdapter.AdaptedControl.Font;
                    Graphics g = e.Graphics;

                    int x = 0;
                    ITransformAdapter transformAdapter = m_columnHeadersAdapter.m_transformAdapter;
                    if (transformAdapter != null)
                        x = (int)transformAdapter.Transform.OffsetX;

                    int rowHeight = m_columnHeadersAdapter.AdaptedControl.Font.Height;
                    int xPadding = Margin.Left;
                    int yPadding = Margin.Top;

                    // draw column headers
                    int left = x;
                    foreach (ColumnInfo info in m_columnHeadersAdapter.m_columns)
                    {
                        int width = info.Width;
                        int right = left + width;

                        ControlPaint.DrawBorder3D(g, left, 0, width, rowHeight, Border3DStyle.Etched, Border3DSide.Bottom | Border3DSide.Right);

                        Rectangle textRect = new Rectangle(left, 0, width, rowHeight);
                        textRect.Width -= SortDirectionIndicatorWidth + xPadding;
                        Sce.Atf.GdiUtil.DrawSortDirectionIndicator(
                            textRect.Right + xPadding,
                            rowHeight / 2 - 1 - Sce.Atf.GdiUtil.SortDirectionIndicatorHeight / 2,
                            info.SortDirection == ListSortDirection.Descending,
                            g);

                        string text = "foo";
                        g.DrawString(text, m_columnHeadersAdapter.AdaptedControl.Font, SystemBrushes.ControlText, textRect, LeftStringFormat);

                        left += width;
                    }
             }

            //protected override void OnMouseDown(MouseEventArgs e)
            //{
            //    s_mouseDown = new Point(e.X, e.Y);

            //    HitRecord hit = m_columnHeadersAdapter.Pick(s_mouseDown);

            //    switch (hit.Type)
            //    {
            //        case HitType.ColumnHeader:
            //            m_columnHeadersAdapter.SelectedProperty = hit.Property;

            //            if (ColumnHeadersAdapter.CanSort(hit.Property))
            //            {

            //                ColumnInfo columnInfo = m_columnHeadersAdapter.GetColumnInfo(hit.Property);
            //                m_columnHeadersAdapter.Sort(hit.Property, columnInfo.NextSortDirection == ListSortDirection.Ascending);
            //                columnInfo.NextSortDirection ^= ListSortDirection.Descending; // toggle direction between ascending/descending

            //                m_columnHeadersAdapter.m_selectedRows.Clear();
            //                m_columnHeadersAdapter.Invalidate();
            //            }
            //            break;

            //        case HitType.ColumnHeaderRightEdge:
            //            m_columnHeadersAdapter.SelectedProperty = hit.Property;

            //            m_columnHeadersAdapter.Select();
            //            s_sizing = true;
            //            s_sizingProperty = hit.Property;
            //            s_sizingOriginalWidth = m_columnHeadersAdapter.GetColumnInfo(s_sizingProperty).Width;
            //            Cursor = Cursors.VSplit;
            //            break;

            //        case HitType.CategoryExpander:
            //            hit.Category.Expanded = !hit.Category.Expanded;
            //            m_columnHeadersAdapter.Invalidate();
            //            break;
            //    }

            //    base.OnMouseDown(e);
            //}

            //protected override void OnMouseMove(MouseEventArgs e)
            //{
            //    if (s_sizing && (e.Button & MouseButtons.Left) != 0)
            //    {
            //        int dx = e.X - s_mouseDown.X;
            //        int newWidth = s_sizingOriginalWidth + dx;
            //        newWidth = Math.Max(newWidth, MinimumColumnWidth);
            //        PropertyDescriptor descriptor = s_sizingProperty.Descriptor;
            //        m_columnHeadersAdapter.m_columnInfo[descriptor].Width = newWidth;

            //        m_columnHeadersAdapter.Invalidate();
            //    }
            //    else if (e.Button == MouseButtons.None)
            //    {
            //        HitRecord hit = m_columnHeadersAdapter.Pick(new Point(e.X, e.Y));
            //        if (hit.Type == HitType.ColumnHeaderRightEdge)
            //            Cursor = Cursors.VSplit;
            //        else
            //            Cursor = Cursors.Arrow;
            //    }

            //    base.OnMouseMove(e);
            //}

            //protected override void OnMouseUp(MouseEventArgs e)
            //{
            //    Cursor = Cursors.Arrow;
            //    s_sizing = false;

            //    base.OnMouseUp(e);
            //}

            private ColumnHeadersAdapter m_columnHeadersAdapter;

            //private static Point s_mouseDown;
            //private static Property s_sizingProperty;
            //private static int s_sizingOriginalWidth;
            //private static bool s_sizing;

            private const int MinimumColumnWidth = 24;

            static ColumnHeaders()
            {
                LeftStringFormat = new StringFormat();
                LeftStringFormat.Alignment = StringAlignment.Near;
                LeftStringFormat.Trimming = StringTrimming.EllipsisCharacter;
                LeftStringFormat.FormatFlags = StringFormatFlags.NoWrap;
            }

            private static StringFormat LeftStringFormat;
        }

        #endregion

        private ITransformAdapter m_transformAdapter;

        private const int BorderWidth = 1;

        private const int SortDirectionIndicatorWidth = 16;

        private List<ColumnInfo> m_columns = new List<ColumnInfo>();

        private Dictionary<PropertyDescriptor, ColumnInfo> m_columnInfo =
            new Dictionary<PropertyDescriptor, ColumnInfo>(); // cache widths for descriptors
        private Dictionary<string, int> m_savedColumnWidths =
             new Dictionary<string, int>(); // last saved column widths 

        private ColumnHeaders m_columnHeaders;

        ///// <summary>
        ///// Reads persisted control setting information from the given XML element
        ///// </summary>
        ///// <param name="root">Root element of XML settings</param>
        //protected virtual void ReadSettings(XmlElement root)
        //{
        //    XmlNodeList columns = root.SelectNodes("PropertyDescriptors");
        //    if (columns == null || columns.Count == 0)
        //        return;
        //    if (columns.Count > 1)
        //        throw new Exception("Duplicated PropertyDescriptors settings");
        //    XmlElement propertyDescriptors = (XmlElement)columns[0];
        //    foreach (XmlElement columnElement in propertyDescriptors)
        //    {
        //        string name = columnElement.GetAttribute("Name");
        //        string propertyType = columnElement.GetAttribute("PropertyType");
        //        string widthString = columnElement.GetAttribute("Width");
        //        int width;
        //        if (widthString != null && int.TryParse(widthString, out width))
        //        {
        //            m_savedColumnWidths[name + propertyType] = width;
        //        }
        //    }
        //}

        ///// <summary>
        ///// Writes persisted control setting information to the given XML element
        ///// </summary>
        ///// <param name="root">Root element of XML settings</param>
        //protected virtual void WriteSettings(XmlElement root)
        //{
        //    XmlDocument xmlDoc = root.OwnerDocument;
        //    XmlElement columnsElement = xmlDoc.CreateElement("PropertyDescriptors");
        //    root.AppendChild(columnsElement);
        //    foreach (KeyValuePair<PropertyDescriptor, ColumnInfo> pair in m_columnInfo)
        //    {
        //        XmlElement columnElement = xmlDoc.CreateElement("Descriptor");
        //        columnElement.SetAttribute("Name", pair.Key.Name);
        //        columnElement.SetAttribute("PropertyType", pair.Key.PropertyType.ToString());
        //        columnElement.SetAttribute("Width", pair.Value.Width.ToString());
        //        columnsElement.AppendChild(columnElement);
        //    }
        //}
    }
}
