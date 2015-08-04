//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Windows.Forms.VisualStyles;
using System.Xml;

using Sce.Atf.Controls.PropertyEditing;


namespace Sce.Atf.Controls
{ 
    /// <summary>
    /// Extends Microsoft's ListView with data binding and cell editing functionalities</summary>
    public class DataBoundListView : ListView
    {
        /// <summary>
        /// Constructor</summary>
        public DataBoundListView()
        {
            View = View.Details;
            FullRowSelect = true;
            ColumnClick +=  DataBoundListView_ColumnClick;
            LabelEdit = false;
            HideSelection = false;
            DoubleBuffered = true;

            currencyManager_ListChanged = bindingManager_ListChanged;
            currencyManager_PositionChanged = bindingManager_PositionChanged;
            SelectedIndexChanged += listView_SelectedIndexChanged;
            ColumnWidthChanged += listView_ColumnWidthChanged;
            ColumnWidthChanging += listView_ColumnWidthChanging;
            m_textBox = new TextBox();
            // force creation of the window handles on the GUI thread
            // see http://forums.msdn.microsoft.com/en-US/clr/thread/fa033425-0149-4b9a-9c8b-bcd2196d5471/
            #pragma warning disable 219
            IntPtr handle = m_textBox.Handle;
            #pragma warning restore 219
            m_textBox.BorderStyle = BorderStyle.FixedSingle; //BorderStyle.None;
            m_textBox.AutoSize = false;

            m_textBox.LostFocus += textBox_LostFocus;

            // forward textbox events as if they originated with this control
            m_textBox.DragOver += textBox_DragOver;
            m_textBox.DragDrop += textBox_DragDrop;
            m_textBox.MouseHover += textBox_MouseHover;
            m_textBox.MouseLeave += textBox_MouseLeave;

            m_textBox.Visible = false;
            Controls.Add(m_textBox);

            m_comboBox = new ComboBox();
            handle = m_comboBox.Handle;
            m_comboBox.Visible = false;
            m_comboBox.DropDownClosed += comboBox_DropDownClosed;
            Controls.Add(m_comboBox);
            OwnerDraw = true;

            m_alternatingRowBrush1 = new SolidBrush(m_alternatingRowColor1);
            m_alternatingRowBrush2 = new SolidBrush(m_alternatingRowColor2);

            m_defaultBackColor = BackColor;
            NormalTextColor = Color.Black;
            HighlightTextColor = Color.White;
            ReadOnlyTextColor = Color.DimGray;
            ExternalEditorTextColor = Color.Black;
            HighlightBackColor = SystemColors.Highlight;
            ColumnHeaderTextColor = NormalTextColor;
            ColumnHeaderTextColorDisabled = ReadOnlyTextColor;
            ColumnHeaderCheckMarkColor = Color.DarkSlateGray;
            ColumnHeaderCheckMarkColorDisabled = ReadOnlyTextColor;
            ColumnHeaderSeparatorColor = Color.FromArgb(228, 229, 230);

            m_boldFont = new Font(Font, FontStyle.Bold);
            ShowGroups = false; // Must be false currently, or OnDrawItem will crash.
        }
        /// <summary>
        /// Gets or sets whether list is under sorting operation</summary>
        public bool SortingItems { get; set; }
   
        #region Skin style

        /// <summary>
        /// Gets or sets first alternating row background color</summary>
        public Color AlternatingRowColor1
        {
            get { return m_alternatingRowColor1; }
            set
            {
                m_alternatingRowBrush1.Dispose();
                m_alternatingRowColor1 = value;
                m_alternatingRowBrush1 = new SolidBrush(m_alternatingRowColor1);
            }
        }

        /// <summary>
        /// Gets or sets second alternating row background color</summary>
        public Color AlternatingRowColor2
        {
            get { return m_alternatingRowColor2; }
            set
            {
                m_alternatingRowBrush2.Dispose();
                m_alternatingRowColor2 = value;
                m_alternatingRowBrush2 = new SolidBrush(m_alternatingRowColor2);
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of readonly text field</summary>
        public Color ReadOnlyTextColor
        {
            get { return m_readOnlyTextColor; }
            set
            {
                m_readOnlyTextColor = value;
                if (m_readOnlyBrush != null)
                    m_readOnlyBrush.Dispose();
                m_readOnlyBrush = new SolidBrush(m_readOnlyTextColor);
            }
        }

        /// <summary>
        /// Gets or sets the foreground color of external editor text field</summary>
        public Color ExternalEditorTextColor
        {
            get { return m_externalEditorTextColor; }
            set
            {
                m_externalEditorTextColor = value;
                if (m_externalEditorBrush != null)
                    m_externalEditorBrush.Dispose();
                m_externalEditorBrush = new SolidBrush(m_externalEditorTextColor);
            }
        }

        /// <summary>
        /// Gets or sets the default background color (used when AlternatingRowColors is false)</summary>
        public Color DefaultBackgroundColor
        {
            get { return m_defaultBackColor; }
            set
            {
                m_defaultBackColor = value;
                if (m_defaultBackBrush != null)
                    m_defaultBackBrush.Dispose();
                m_defaultBackBrush = new SolidBrush(m_defaultBackColor);
            }
        }

        /// <summary>
        /// Gets or sets the highlighted text color</summary>
        public Color HighlightTextColor
        {
            get { return m_highlightTextColor; }
            set
            {
                m_highlightTextColor = value;
                if (m_highlightTextBrush != null)
                    m_highlightTextBrush.Dispose();
                m_highlightTextBrush = new SolidBrush(m_highlightTextColor);
            }
        }

        /// <summary>
        /// Gets or sets the highlighted background color</summary>
        public Color HighlightBackColor
        {
            get { return m_highlightTextColor; }
            set
            {
                m_highlightBackColor = value;
                if (m_highlightBackBrush != null)
                    m_highlightBackBrush.Dispose();
                m_highlightBackBrush = new SolidBrush(m_highlightBackColor);
            }
        }

        /// <summary>
        /// Gets or sets the normal text color (for editable field)</summary>
        public Color NormalTextColor
        {
            get { return m_normalTextColor; }
            set
            {
                m_normalTextColor = value;
                if (m_normalTextBrush != null)
                    m_normalTextBrush.Dispose();
                m_normalTextBrush = new SolidBrush(m_normalTextColor);
            }
        }

        /// <summary>
        /// Gets or sets the font to apply to all column headers text</summary>
        public Font HeaderFont
        {
            get { return m_headerFont; }
            set
            {
                if (m_headerFont != null)
                    m_headerFont.Dispose();
                m_headerFont = value;
            }
        }

        /// <summary>
        /// Gets or sets the normal header text color</summary>
        public Color ColumnHeaderTextColor
        {
            get { return m_columnHeaderTextColor; }
            set
            {
                m_columnHeaderTextColor = value;
                if (m_columnHeaderTextBrush != null)
                    m_columnHeaderTextBrush.Dispose();
                m_columnHeaderTextBrush = new SolidBrush(m_columnHeaderTextColor);
            }
        }

        /// <summary>
        /// Gets or sets the header text color when the control is disabled</summary>
        public Color ColumnHeaderTextColorDisabled
        {
            get { return m_columnHeaderTextColorDisabled; }
            set
            {
                m_columnHeaderTextColorDisabled = value;
                if (m_columnHeaderTextBrushDisabled != null)
                    m_columnHeaderTextBrushDisabled.Dispose();
                m_columnHeaderTextBrushDisabled = new SolidBrush(m_columnHeaderTextColorDisabled);
            }
        }

        /// <summary>
        /// Gets or sets the column header check mark color</summary>
        public Color ColumnHeaderCheckMarkColor
        {
            get { return m_columnHeaderCheckMarkColor; }
            set
            {
                m_columnHeaderCheckMarkColor = value;
                if (m_columnHeaderCheckMarkBrush != null)
                    m_columnHeaderCheckMarkBrush.Dispose();
                m_columnHeaderCheckMarkBrush = new SolidBrush(m_columnHeaderCheckMarkColor);
            }
        }

        /// <summary>
        /// Gets or sets the column header check mark color when the control is disabled</summary>
        public Color ColumnHeaderCheckMarkColorDisabled
        {
            get { return m_columnHeaderCheckMarkColorDisabled; }
            set
            {
                m_columnHeaderCheckMarkColorDisabled = value;
                if (m_columnHeaderCheckMarkBrushDisabled != null)
                    m_columnHeaderCheckMarkBrushDisabled.Dispose();
                m_columnHeaderCheckMarkBrushDisabled = new SolidBrush(m_columnHeaderCheckMarkColorDisabled);
            }
        }

      

        /// <summary>
        /// Gets or sets the color used to paint the column header separator lines</summary>
        public Color ColumnHeaderSeparatorColor
        {
            get { return m_columnHeaderSeparatorColor; }
            set
            {
                m_columnHeaderSeparatorColor = value;
                if (m_columnHeaderSeparatorPen != null)
                    m_columnHeaderSeparatorPen.Dispose();
                m_columnHeaderSeparatorPen = new Pen(m_columnHeaderSeparatorColor,1);
            }
        }

        private Color m_alternatingRowColor1 = Color.White;
        private Color m_alternatingRowColor2 = Color.FromArgb(233, 236, 241);
        private Color m_readOnlyTextColor;
        private Color m_externalEditorTextColor;
        private Color m_defaultBackColor;
        private Color m_highlightTextColor;
        private Color m_highlightBackColor;
        private Color m_normalTextColor;
        private Color m_columnHeaderTextColor;
        private Color m_columnHeaderTextColorDisabled;
        private Color m_columnHeaderCheckMarkColor;
        private Color m_columnHeaderCheckMarkColorDisabled;
        private Color m_columnHeaderSeparatorColor;


        private SolidBrush m_alternatingRowBrush1;
        private SolidBrush m_alternatingRowBrush2 ;
        private SolidBrush m_defaultBackBrush;
        private SolidBrush m_highlightTextBrush;
        private SolidBrush m_normalTextBrush;
        private SolidBrush m_highlightBackBrush;
        private SolidBrush m_readOnlyBrush;
        private SolidBrush m_externalEditorBrush;
        private SolidBrush m_columnHeaderTextBrush;
        private SolidBrush m_columnHeaderTextBrushDisabled;
        private SolidBrush m_columnHeaderCheckMarkBrush;
        private SolidBrush m_columnHeaderCheckMarkBrushDisabled;

        private Pen m_columnHeaderSeparatorPen;

        private Font m_headerFont = new Font("Helvetica", 9, FontStyle.Bold);
        private Font m_tickFont = new Font("Helvetica", 9, FontStyle.Regular);
        private Font m_boldFont;
        
        private int m_headerHeight;
        #endregion


        /// <summary>
        /// Gets or set the data source</summary>
        public object DataSource
        {
            get
            {
                return m_dataSource;
            }
            set
            {
                if (m_dataSource != value)
                {
                    m_dataSource = value;
                    UpdateDataBinding();
                }
            }
        }

        /// <summary>
        /// Gets or sets data member bound to the DataSource</summary>
        public string DataMember
        {
            get
            {
                return m_dataMember;
            }
            set
            {
                if (m_dataMember != value)
                {
                    m_dataMember = value;
                    UpdateDataBinding();
                }
            }
        }

        /// <summary>
        /// Gets the property descriptor collection for the underlying list</summary>
        public PropertyDescriptorCollection ItemProperties
        {
            get { return m_propertyDescriptors; }
        }


        /// <summary>
        /// Gets or sets whether to alternate row colors</summary>
        public bool AlternatingRowColors { get; set; }

      


        /// <summary>
        /// Performs custom actions when the BindingContext property is changed by either a programmatic modification or user interaction</summary>
        /// <param name="e">Event args</param>
        protected override void OnBindingContextChanged(EventArgs e)
        {
            UpdateDataBinding();
            base.OnBindingContextChanged(e);
        }

        /// <summary>
        /// Process command keys</summary>
        /// <param name="msg">System.Windows.Forms.Message for window message to process</param>
        /// <param name="keyData">System.Windows.Forms.Keys value representing key to process</param>
        /// <returns>True iff character was processed by control</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // stop the delete key bubbling up the control hierarchy. 
            if (keyData == Keys.Delete)
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Resizes ListView to fit its content so no vertical or horizontal scroll bars appear</summary>
        public void ResizeToContent()
        {
            if (Items.Count > 0)
            {
                Rectangle rect = GetItemRect(Items.Count - 1);
                Height = rect.Y + rect.Height;
            }
            else
                Height = 0;
        
            IntPtr header = User32.SendMessage(Handle, User32.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            Rectangle hdrBounds = new Rectangle();
            User32.GetClientRect(header, ref hdrBounds);
            Height += hdrBounds.Height + Margin.Top + Margin.Bottom + SystemInformation.HorizontalScrollBarHeight;

            int width = 0;
            foreach (ColumnHeader column in Columns)
            {
                width += column.Width;
            }
            Width = width + Margin.Left + Margin.Right + SystemInformation.VerticalScrollBarWidth;
        }

        #region data binding

        private void UpdateDataBinding()
        {
            if (DataSource == null || base.BindingContext == null)
                return;

            CurrencyManager currencyManager;
            try
            {
                currencyManager = (CurrencyManager)base.BindingContext[DataSource, DataMember];
            }
            catch (ArgumentException)
            {
                // no currency manager for the data source
                return;
            }

 
            if (m_currencyManager != currencyManager)
            {
                RememberSelections();
                if (m_currencyManager != null)
                {
                    m_currencyManager.ListChanged -= currencyManager_ListChanged;
                    m_currencyManager.PositionChanged -= currencyManager_PositionChanged;
                }

                m_currencyManager = currencyManager;

                if (m_currencyManager != null)
                {
                    m_currencyManager.ListChanged += currencyManager_ListChanged;
                    m_currencyManager.PositionChanged += currencyManager_PositionChanged;
                }
                
                // populate the list from data source
                BuildListColumns();
                Reload();

                RestoreSelections();
              }
        }


        private void BuildListColumns()
        {
            // remember column widths
            Dictionary<string, int> columnWidths = new Dictionary<string, int>();
            foreach (ColumnHeader column in Columns)
                columnWidths[column.Text] = column.Width;

            Columns.Clear();
            if (m_currencyManager == null)
                return;

            m_adjustingColumnWidths = true;

            m_propertyDescriptors = m_currencyManager.GetItemProperties();
            foreach (PropertyDescriptor pd in m_propertyDescriptors)
            {
                ColumnHeader column = new ColumnHeader();
                column.Name = pd.Name;
                column.Text = pd.DisplayName;
                if (columnWidths.ContainsKey(column.Text))
                    column.Width = columnWidths[column.Text];
                Columns.Add(column);
            }

            if (AutoSize)
                ResizeToContent();
            m_adjustingColumnWidths = false;
        }

        /// <summary>
        /// Reloads data objects from the data source</summary>
        private void Reload()
        {
            BeginUpdate();
            Items.Clear();

            ListViewItem[] items = new ListViewItem[m_currencyManager.Count];
            for (int i = 0; i < m_currencyManager.Count; i++)
            {
                items[i] = ListViewItemFromDataSource(i);
            }
            
            Items.AddRange(items);

            if (m_autoColumnWidth)
            {
                AutoResizeColumns();
            }

            EndUpdate();
        }

        private ListViewItem ListViewItemFromDataSource(int index)
        {
            PropertyDescriptorCollection pds = m_currencyManager.GetItemProperties();
            ArrayList items = new ArrayList();
            object dataItem = m_currencyManager.List[index];      

            foreach (ColumnHeader column in Columns)
            {
                //PropertyDescriptor pd = pds.Find(column.Text, false);
                PropertyDescriptor pd = null;
                foreach (PropertyDescriptor member in pds)
                {
                    if (member.DisplayName == column.Text)
                    {
                        pd = member;
                        break;
                    }
                }
                if (pd != null)
                {
                    items.Add(pd.GetValue(dataItem).ToString());
                }
            }
            return new ListViewItem((string[])items.ToArray(typeof(string)), GroupingDataItem(dataItem));
        }
        
        private void AddItem(int index)
        {
            ListViewItem item = ListViewItemFromDataSource(index);
            Items.Insert(index, item);

            if (m_autoColumnWidth)
            {
                AutoResizeColumns();
            }
        }
        
        private void UpdateItem(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
               
                object dataItem = m_currencyManager.List[index];
                int j = 0;
                foreach (ColumnHeader column in Columns)
                {
                    PropertyDescriptor pd = m_propertyDescriptors.Find(column.Name, false);
                    if (pd != null)
                    {
                        Items[index].SubItems[j].Text = pd.GetValue(dataItem).ToString();
                    }
                    ++j;
                }
              
               
            }
        }

        private void DeleteItem(int index)
        {
            if (index >= 0 && index < Items.Count)
                this.Items.RemoveAt(index);
        }
        
        
        private void bindingManager_PositionChanged(object sender, EventArgs e)
        {
            if ((m_currencyManager.Position >=0) &&  (Items.Count > m_currencyManager.Position))
            {
                Items[m_currencyManager.Position].Selected = true;
                EnsureVisible(m_currencyManager.Position);
            }
        }

        private void bindingManager_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.Reset || e.ListChangedType == ListChangedType.ItemMoved)
                Reload();        
            else if (e.ListChangedType == ListChangedType.ItemAdded)
                AddItem(e.NewIndex);
            else if (e.ListChangedType == ListChangedType.ItemChanged)
                UpdateItem(e.NewIndex);
            else if (e.ListChangedType == ListChangedType.ItemDeleted)
                DeleteItem(e.NewIndex);
            else
            {
                // populate the list from data source
                BuildListColumns();
                Reload();
            }
        }

        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (SelectedIndices.Count > 0 && m_currencyManager.Position != SelectedIndices[0])
                    m_currencyManager.Position = SelectedIndices[0];
            }
            catch
            {
                
            }
            Invalidate(); // repaint background 
        }


        /// <summary>
        /// Method called after column width changed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Column width changed event args</param>
        void listView_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (!m_adjustingColumnWidths)
            {
                m_autoColumnWidth = false; // user manually resizes column
                if (m_lastNewWidth > 0 && Math.Abs(m_lastNewWidth - Columns[e.ColumnIndex].Width) > 100) // avoid snapping 
                    Columns[e.ColumnIndex].Width = m_lastNewWidth;
            }
               
        }

        /// <summary>
        /// Method called before column width changed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Column width changed event args</param>
        void listView_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            DisableEditingControl(true);
            if (!m_adjustingColumnWidths)
            {
                m_lastNewWidth = e.NewWidth;
                // ensure minimum width to render header text +  sorting arrow
                using (Graphics g = CreateGraphics())
                {
                    float wMin = g.MeasureString(Columns[e.ColumnIndex].Text, HeaderFont).Width +
                                 s_sortAscendingImage.Width + 8 +2;
                    if (e.ColumnIndex ==0 && CheckBoxes)
                        wMin += CheckBoxWidth + 2;
                    if (e.NewWidth < wMin)
                    {
                        e.Cancel = true;
                        e.NewWidth = (int)wMin;

                    }
                }
            }
              
        }
       

        #endregion

        /// <summary>
        /// Gets and sets the persistent state for the control as a string of settings</summary>
        public string Settings
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("Columns");
                xmlDoc.AppendChild(root);

                foreach (ColumnHeader column in Columns)
                {
                    XmlElement columnElement = xmlDoc.CreateElement("Column");
                    root.AppendChild(columnElement);

                    columnElement.SetAttribute("Name", column.Text);
                    columnElement.SetAttribute("Width", column.Width.ToString());
                }
                return xmlDoc.InnerXml;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                SuspendLayout();
                m_adjustingColumnWidths = true;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if (root == null || root.Name != "Columns")
                    throw new Exception("Invalid ListView settings");
                
                m_autoColumnWidth = false;
                XmlNodeList columns = root.SelectNodes("Column");
                foreach (XmlElement columnElement in columns)
                {
                    string name = columnElement.GetAttribute("Name");
    
                    string widthString = columnElement.GetAttribute("Width");
                    int width;
                    if (widthString != null && int.TryParse(widthString, out width))
                    {
                        bool matched = false;
                        foreach (ColumnHeader column in Columns)
                        {
                            if (column.Text == name)
                            {
                                column.Width = width;
                                matched = true;
                                break;
                            }
                        }
                        if (!matched)
                            m_autoColumnWidth = true; // mismatch requires reset  
                    }
                  
                }

                m_adjustingColumnWidths = false;
                ResumeLayout();
                if (AutoSize)
                    ResizeToContent();
            }
        }

        #region OwnerDraw
        /// <summary>
        /// Performs custom actions when the owner draws the item</summary>
        /// <param name="e">Draw ListView item event args</param>
        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            base.OnDrawItem(e);

            // fill background
            if (AlternatingRowColors)// && e.ItemIndex == Items.Count - 1)
            {
                var lastRowBound = new Rectangle();
                int rowIndex = 0;
                if (Items.Count > 0 )
                {
                    if (Items[Items.Count - 1] == null || Items[0] == null)
                        return;
                    lastRowBound = Items[Items.Count - 1].Bounds;
                    var lastSubItemBound = Items[0].SubItems[Items[0].SubItems.Count - 1].Bounds;
                    m_rowHeight = lastRowBound.Height;

                    if (lastSubItemBound.Right < ClientRectangle.Width) // fill the width(right of rows)
                    {
                        var rect = new Rectangle(lastSubItemBound.Right, Items[0].Bounds.Top,
                                                 ClientRectangle.Width - lastSubItemBound.Right, m_rowHeight);
                        while (rect.Y < lastRowBound.Bottom)
                        {
                            if (Enabled && SelectedIndices.Contains(rowIndex))
                            {
                                e.Graphics.FillRectangle(m_highlightBackBrush, rect);

                            }
                            else
                            {
                                if (rowIndex%2 == 0)
                                    e.Graphics.FillRectangle(m_alternatingRowBrush1, rect);
                                else
                                    e.Graphics.FillRectangle(m_alternatingRowBrush2, rect);
                            }

                            ++rowIndex;
                            rect.Y += m_rowHeight;
                        }
                    }
                    rowIndex = Items.Count;
                }
                else
                    lastRowBound.Y = HeaderHeight;

                // fill the bottom
                if (lastRowBound.Top + lastRowBound.Height < ClientRectangle.Height)
                {

                    var rect = new Rectangle(0, lastRowBound.Bottom, ClientRectangle.Width, m_rowHeight);
                    while (rect.Y < ClientRectangle.Bottom)
                    {
                        if (rowIndex%2 == 0)
                            e.Graphics.FillRectangle(m_alternatingRowBrush1, rect);
                        else
                            e.Graphics.FillRectangle(m_alternatingRowBrush2, rect);

                        ++rowIndex;
                        rect.Y += m_rowHeight;
                    }
                }
            }
        }

        /// <summary>
        /// Performs custom actions when owner draws a single subitem</summary>
        /// <param name="e">Draw ListView subitem event args</param>
        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
            base.OnDrawSubItem(e);
 
            if (CheckBoxes && e.ColumnIndex == 0)
            {
                if (e.Item.Selected && Enabled)
                {
                    e.Graphics.FillRectangle(m_highlightBackBrush, e.Bounds);
                }
                else
                {
                    if (AlternatingRowColors)
                    {
                        if (e.ItemIndex % 2 == 0)
                            e.Graphics.FillRectangle(m_alternatingRowBrush1, e.Bounds);
                        else
                            e.Graphics.FillRectangle(m_alternatingRowBrush2, e.Bounds);
                    }
                    else 
                        e.Graphics.FillRectangle(m_defaultBackBrush, e.Bounds);
                }

                DrawCheckBox(e);

                var rect = e.Bounds;
                rect.X += CheckBoxWidth + TextOffset;
                rect.Width -= CheckBoxWidth + TextOffset;
               
                 // Draw the subitem text
                var font = e.Item.Checked ? m_boldFont : Font;
               
                using (StringFormat sf = new StringFormat(StringFormatFlags.LineLimit))
                {
                    // Store the column text alignment, letting it default
                    // to Left if it has not been set to Center or Right.
                    switch (e.Header.TextAlign)
                    {
                        case HorizontalAlignment.Center:
                            sf.Alignment = StringAlignment.Center;
                            break;
                        case HorizontalAlignment.Right:
                            sf.Alignment = StringAlignment.Far;
                            break;
                    }
                    if (IsCellReadOnly(e.ItemIndex, e.ColumnIndex))
                    {
                        var brush = e.Item.Selected ? m_highlightTextBrush : m_readOnlyBrush;
                        if (!Enabled)
                            brush = m_readOnlyBrush;
                        e.Graphics.DrawString(e.SubItem.Text, font, brush, rect, sf);
                    }
                    else if (IsCellExternalEditor(e.ItemIndex, e.ColumnIndex))
                    {
                        var brush = e.Item.Selected ? m_highlightTextBrush : m_externalEditorBrush;
                        if (!Enabled)
                            brush = m_readOnlyBrush;
                        e.Graphics.DrawString(e.SubItem.Text, font, brush, rect, sf);
                    }
                    else
                    {
                        var brush = e.Item.Selected ? m_highlightTextBrush : m_normalTextBrush;
                        if (!Enabled)
                            brush = m_readOnlyBrush;
                        e.Graphics.DrawString(e.SubItem.Text, font, brush, rect, sf);
                       
                    }
                }
                return;

            }

            var bound = e.Bounds;
            bound.Offset(TextOffset, 0);
            using (StringFormat sf = new StringFormat(StringFormatFlags.LineLimit))
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }


                // Draw normal text for a subitem 
                if (e.Item.Selected && Enabled)
                {
                    e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
                }
                else
                {
                    if (e.ItemIndex % 2 == 0)
                        e.Graphics.FillRectangle(m_alternatingRowBrush1, e.Bounds);
                    else
                        e.Graphics.FillRectangle(m_alternatingRowBrush2, e.Bounds);
                }

                // Draw the subitem text
                var font = e.Item.Checked ? m_boldFont : Font;
                if (IsCellReadOnly(e.ItemIndex, e.ColumnIndex))
                {
                     var brush = e.Item.Selected ? m_highlightTextBrush : m_readOnlyBrush;
                     if (!Enabled)
                         brush = m_readOnlyBrush;
                     e.Graphics.DrawString(e.SubItem.Text, font, brush, bound, sf);
                }
                else if (IsCellExternalEditor(e.ItemIndex, e.ColumnIndex))
                {
                    var brush = e.Item.Selected ? m_highlightTextBrush : m_externalEditorBrush;
                    if (!Enabled)
                        brush = m_readOnlyBrush;
                    e.Graphics.DrawString(e.SubItem.Text, font, brush, bound, sf);
                }
                else
                {
                    var brush = e.Item.Selected ? m_highlightTextBrush : m_normalTextBrush;
                    if (!Enabled)
                        brush = m_readOnlyBrush;
                    e.Graphics.DrawString(e.SubItem.Text, font, brush, bound, sf);
                  
                }
              }
        }



        /// <summary>
        /// Performs custom actions when owner draws columns header</summary>
        /// <param name="e">Draw ListView column header event args</param>
        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            base.OnDrawColumnHeader(e);
            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }


                // Draw the standard header background.
                e.DrawBackground();
                                
                // Draw the header text.
                var bound = e.Bounds;
                bound.Y += 3;
                bound.X += 8;
                if (e.ColumnIndex == 0)
                    bound.X += CheckBoxWidth;

                var brush = Enabled ? m_columnHeaderTextBrush : m_columnHeaderTextBrushDisabled;
                e.Graphics.DrawString(e.Header.Text, HeaderFont, brush, bound, sf);


                if (e.ColumnIndex == m_sortColumn) // Draw sort arrow
                {
                    float w = e.Graphics.MeasureString(e.Header.Text, HeaderFont).Width;
                    if (m_sortColumn == 0)
                        w += CheckBoxWidth + 2; 
                    if (e.Bounds.Width > w + s_sortAscendingImage.Width + 8) // get enough room left for the arrow icon
                    {
                        Point pt = new Point(e.Bounds.Location.X + e.Bounds.Width - s_sortAscendingImage.Width - 4,
                        e.Bounds.Top + (e.Bounds.Height - s_sortAscendingImage.Height) / 2);
                        if (m_sortDirection == ListSortDirection.Ascending)
                            e.Graphics.DrawImage(s_sortAscendingImage, pt);
                        else
                            e.Graphics.DrawImage(s_sortDescendingImage, pt);
                    }
                }

                if (CheckBoxes && e.ColumnIndex ==0)
                {
                    // draw check mark
                    Point pt = new Point(e.Bounds.Location.X, e.Bounds.Top + 3);
                    e.Graphics.DrawString(m_tickSymbol, m_tickFont, Enabled ? m_columnHeaderCheckMarkBrush : m_columnHeaderCheckMarkBrushDisabled, pt);
                   
                    // draw a vertical line to make check mark appear in its own column                 
                    e.Graphics.DrawLine(m_columnHeaderSeparatorPen, CheckBoxWidth, e.Bounds.Top,
                        CheckBoxWidth, e.Bounds.Bottom - 2);
                   
                }
            }
        }


        /// <summary>
        /// Performs custom actions when owner draws checkbox</summary>
        /// <param name="e">Draw ListView subitem event args</param>
        protected virtual void DrawCheckBox(DrawListViewSubItemEventArgs e)
        {
            bool onOff = Items[e.ItemIndex].Checked;
            Point location = new Point(e.Bounds.Location.X + Margin.Left, e.Bounds.Location.Y);

            if (MultiSelect)
            {
                CheckBoxState state = onOff ? CheckBoxState.CheckedNormal : CheckBoxState.UncheckedNormal;
                CheckBoxRenderer.DrawCheckBox(e.Graphics, location, state);
            }
            else
            {
                RadioButtonState rdState = onOff ? RadioButtonState.CheckedNormal : RadioButtonState.UncheckedNormal;
                if (!Enabled)
                    rdState = onOff ? RadioButtonState.CheckedDisabled : RadioButtonState.UncheckedDisabled;
                RadioButtonRenderer.DrawRadioButton(e.Graphics, location, rdState);

            }

        }

        private int HeaderHeight
        {
            get
            {

                if (m_headerHeight == 0)
                {
                    IntPtr header = User32.SendMessage(Handle, User32.LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
                    Rectangle hdrBounds = new Rectangle();
                    User32.GetClientRect(header, ref hdrBounds);
                    m_headerHeight = hdrBounds.Height;
                }
                return m_headerHeight;
            }
        }

        /// <summary>
        /// WndProc</summary>
        /// <param name="m">Message</param>
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (AlternatingRowColors && Items.Count ==0)
            {
                const int WM_PAINT = 0x000F;
                if (m.Msg == WM_PAINT)
                {
                    using (Graphics g = CreateGraphics())
                    {
                        int rowIndex = 0;
                        var rowBound = new Rectangle(0, HeaderHeight,0,0);
                        
                        if (rowBound.Top + rowBound.Height < ClientRectangle.Height)
                        {

                            var rect = new Rectangle(0, rowBound.Bottom, ClientRectangle.Width, m_rowHeight);
                            while (rect.Y < ClientRectangle.Bottom)
                            {
                                if (rowIndex%2 == 0)
                                    g.FillRectangle(m_alternatingRowBrush1, rect);
                                else
                                    g.FillRectangle(m_alternatingRowBrush2, rect);

                                ++rowIndex;
                                rect.Y += m_rowHeight;
                            }
                        }
                    }
                }

            }
        }

        #endregion


        private void DataBoundListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
 
            IBindingList bindingList = DataSource as IBindingList;
            if (bindingList != null && bindingList.SupportsSorting)
            {

                if (m_sortColumn == e.Column)
                    m_sortDirection = m_sortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;

                m_sortColumn = e.Column;
                SortingItems = true;
                DisableEditingControl(true);

                RememberSelections();
                bindingList.ApplySort(ItemProperties[e.Column], m_sortDirection);
                RestoreSelections();
                SortingItems = false;
            }
        }

        private void RememberSelections()
        {
            m_selectedObjects.Clear();
            m_checkeObjects.Clear();
            foreach (int index in SelectedIndices)
                m_selectedObjects.Add(m_currencyManager.List[index]);
            for (int index = 0; index < Items.Count; ++index )
            {
                if( Items[index].Checked)
                    m_checkeObjects.Add(m_currencyManager.List[index]);
            }
        }

        private void RestoreSelections()
        {
            foreach (var dataItem in m_selectedObjects)
            {
                int index = m_currencyManager.List.IndexOf(dataItem);
                if (index != -1)
                    Items[index].Selected = true;
            }

            foreach (var dataItem in m_checkeObjects)
            {
                int index = m_currencyManager.List.IndexOf(dataItem);
                if (index != -1)
                    Items[index].Checked = true;
            }
        }

        private void AutoResizeColumns()
        {
            m_adjustingColumnWidths = true;
            using (Graphics g = CreateGraphics())
            {
                int numColumns =  Columns.Count;
                for (int columnIndex = 0; columnIndex < numColumns; ++columnIndex)
                {
                 
                   float width = g.MeasureString(Columns[columnIndex].Text, HeaderFont).Width + 
                       2 * s_sortAscendingImage.Width + Margin.Left + Margin.Right +18;

                   for (int itemIndex = 0; itemIndex < Items.Count; ++itemIndex)
                   {
                       float w = g.MeasureString(Items[itemIndex].SubItems[columnIndex].Text, Font).Width + Margin.Left + Margin.Right +18;
                       if (w > width)
                           width = w;
                   }
             
                   Columns[columnIndex].Width = (int)width;
               }
               
            }
            if (Items.Count >0)
                m_autoColumnWidth = false; // AutoResizeColumns() should be called only once when the list is not empty
            m_adjustingColumnWidths = false;
        }

        /// <summary>
        /// Disposes of resources</summary>
        /// <param name="disposing">True to release both managed and unmanaged resources;
        /// false to release only unmanaged resources</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (m_alternatingRowBrush1 != null)
                {
                    m_alternatingRowBrush1.Dispose();
                    m_alternatingRowBrush1 = null;
                }

                if (m_alternatingRowBrush2 != null)
                {
                    m_alternatingRowBrush2.Dispose();
                    m_alternatingRowBrush2 = null;
                }

                if (m_defaultBackBrush != null)
                {
                    m_defaultBackBrush.Dispose();
                    m_defaultBackBrush = null;
                }

                if (m_highlightTextBrush != null)
                {
                    m_highlightTextBrush.Dispose();
                    m_highlightTextBrush = null;
                }

                if (m_normalTextBrush != null)
                {
                    m_normalTextBrush.Dispose();
                    m_normalTextBrush = null;
                }

                if (m_highlightBackBrush != null)
                {
                    m_highlightBackBrush.Dispose();
                    m_highlightBackBrush = null;
                }

                if (m_readOnlyBrush != null)
                {
                    m_readOnlyBrush.Dispose();
                    m_readOnlyBrush = null;
                }

                if (m_externalEditorBrush != null)
                {
                    m_externalEditorBrush.Dispose();
                    m_externalEditorBrush = null;
                }

                if (m_columnHeaderTextBrush != null)
                {
                    m_columnHeaderTextBrush.Dispose();
                    m_columnHeaderTextBrush = null;
                }

                if (m_columnHeaderTextBrushDisabled != null)
                {
                    m_columnHeaderTextBrushDisabled.Dispose();
                    m_columnHeaderTextBrushDisabled = null;
                }

                if (m_columnHeaderCheckMarkBrush != null)
                {
                    m_columnHeaderCheckMarkBrush.Dispose();
                    m_columnHeaderCheckMarkBrush = null;
                }

                if (m_columnHeaderCheckMarkBrushDisabled != null)
                {
                    m_columnHeaderCheckMarkBrushDisabled.Dispose();
                    m_columnHeaderCheckMarkBrushDisabled = null;
                }

                if (m_columnHeaderSeparatorPen != null)
                {
                    m_columnHeaderSeparatorPen.Dispose();
                    m_columnHeaderSeparatorPen = null;
                }
            }

            base.Dispose(disposing);
        }
     
        #region cell editing

        /// <summary>
        /// Performs custom actions on MouseDown events</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                m_itemAlreadySelected = false;
                ListViewItem item = this.GetItemAt(e.X, e.Y);
                if (item != null && SelectedItems.Contains(item))
                    m_itemAlreadySelected = true;
                else
                {
                    DisableEditingControl(true);             
                }
            }
        }

        /// <summary>
        /// Performs custom actions on MouseUp events</summary>
        /// <param name="e">Mouse event args</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // which cell clicked?
            ListViewItem item = this.GetItemAt(e.X, e.Y);
            if (e.Button == MouseButtons.Left && item != null && m_itemAlreadySelected)
            {
                int row = item.Index;
                int col = -1;
                Rectangle itemBound = item.GetBounds(ItemBoundsPortion.Entire);
                int cellLeft = itemBound.Left;
                foreach (ColumnHeader columnHeader in Columns)
                {
                    if (e.X < cellLeft + columnHeader.Width)
                    {
                        col = columnHeader.Index;
                        break;
                    }
                    cellLeft += columnHeader.Width;
                }
             
                if (row >= 0 && col >= 0)
                {
                    if (col == 0 && CheckBoxes)
                    {
                        if (e.X < CheckBoxWidth)
                            return;
                    }

                    if (m_currentRow != row || m_currentCol != col)
                    {
                        DisableEditingControl(true);
                    }
                    m_currentRow = row;
                    m_currentCol = col;
                    OnCellClicked(item);
                }
            }
          
        }

        /// <summary>
        /// Method called when cell clicked</summary>
        /// <param name="item">Item clicked</param>
        protected void OnCellClicked(ListViewItem item)
        {
            if (!IsCellReadOnly(m_currentRow, m_currentCol) && !IsCellExternalEditor(m_currentRow, m_currentCol))
                StartCellEdit(item);
        }

        private void StartCellEdit(ListViewItem item)
        {
            var eventArgs = new ListViewCellCancelEventArgs(m_currentRow, m_currentCol);
            OnCellBeginEdit(eventArgs);
            if (eventArgs.Cancel)
                return;

            Rectangle itemBound = item.GetBounds(ItemBoundsPortion.Entire);
            int cellLeft = itemBound.Left;
            for (int i = 0; i < m_currentCol; ++i)
                cellLeft += Columns[i].Width;

            int cellWidth = Columns[m_currentCol].Width;
            if (m_currentCol == 0 && CheckBoxes)
            {
                cellLeft += CheckBoxWidth ;
                cellWidth -= CheckBoxWidth;
            }
            cellLeft += TextOffset;
            cellWidth -=TextOffset;

            Rectangle cellBound = new Rectangle(cellLeft, itemBound.Y, cellWidth, itemBound.Height);
            if (m_propertyDescriptors[m_currentCol].PropertyType.IsEnum)
            {
                m_comboBox.DataSource = Enum.GetValues(m_propertyDescriptors[m_currentCol].PropertyType);
                m_comboBox.Bounds = cellBound;
                m_activeEditingControl = m_comboBox;
               
            }
            else
            {
                m_textBox.Bounds = cellBound;
                m_activeEditingControl = m_textBox;              
            }
            EnableEditingControl();
        }

        private bool IsCellReadOnly(int row, int col)
        {
            if (m_propertyDescriptors[col].IsReadOnly)
                return true;
            var group = Items[row].Group;
            if (group != null && m_groupReadOnlyColumns.ContainsKeyValue(group.Name, Columns[col].Text))
                return true;
            return false;

        }

        private bool IsCellExternalEditor(int row, int col)
        {
          
            var group = Items[row].Group;
            if (group != null && m_groupExternalEditorColumns.ContainsKeyValue(group.Name, Columns[col].Text))
                return true;
            return false;

        }

        private int CheckBoxWidth
        {
            get
            {
                if (m_checkBoxWidth == 0)
                     m_checkBoxWidth = CheckBoxRenderer.GetGlyphSize(CreateGraphics(), CheckBoxState.UncheckedNormal).Width + 6;
               
                return m_checkBoxWidth;
            }
        }

        /// <summary>
        /// Event that is raised when cell begins</summary>
        public event EventHandler<ListViewCellCancelEventArgs> CellBeginEdit;
        /// <summary>
        /// Event that is raised when cell ends</summary>
        public event EventHandler<ListViewCellEventArgs> CellEndEdit;
        /// <summary>
        /// Event that is raised when cell validating begins</summary>
        public event EventHandler<ListViewCellValidatingEventArgs> CellValidating;


        /// <summary>
        /// Event arguments for cell events</summary>
        public class ListViewCellEventArgs : EventArgs
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="rowIndex">Cell row index</param>
            /// <param name="columnIndex">Cell column index</param>
            public ListViewCellEventArgs(int rowIndex, int columnIndex)
            {
                RowIndex = rowIndex;
                ColumnIndex = columnIndex;
            }

            /// <summary>
            /// Row index</summary>
            public int RowIndex;
            /// <summary>
            /// Column index</summary>
            public int ColumnIndex;
        }

        /// <summary>
        /// Event arguments for cell operation cancel events</summary>
        public class ListViewCellCancelEventArgs : CancelEventArgs
        {
            /// <summary>
            /// Constructor with parameters</summary>
            /// <param name="rowIndex">Row index</param>
            /// <param name="columnIndex">Column index</param>
            public ListViewCellCancelEventArgs( int rowIndex, int columnIndex)
            {
                RowIndex = rowIndex;
                ColumnIndex = columnIndex;
            }

            /// <summary>
            /// Row index</summary>
            public int RowIndex;
            /// <summary>
            /// Column index</summary>
            public int ColumnIndex;
        }

        /// <summary>
        /// Event arguments for cell validation events.
        /// The text entered by the user through the user interface (UI) becomes the FormattedValue property value. 
        /// This is the value that you can validate before it is parsed into the cell Value property value.</summary>
        public class ListViewCellValidatingEventArgs : CancelEventArgs
        {
            /// <summary>
            /// Constructor with parameters</summary>
            /// <param name="rowIndex">Row index</param>
            /// <param name="columnIndex">Column index</param>
            /// <param name="formattedValue">Formatted value</param>
            public ListViewCellValidatingEventArgs(int rowIndex, int columnIndex, object formattedValue)
            {
                RowIndex = rowIndex;
                ColumnIndex = columnIndex;
                FormattedValue = formattedValue;
            }

            /// <summary>
            /// Row index</summary>
            public int RowIndex;
            /// <summary>
            /// Column index</summary>
            public int ColumnIndex;
            /// <summary>
            /// Formatted value</summary>
            public object FormattedValue;
        }

        /// <summary>
        /// Method called when cell begins to be edited</summary>
        /// <param name="e">ListView cell cancel event args</param>
        protected void OnCellBeginEdit(ListViewCellCancelEventArgs e)
        {
            if (CellBeginEdit != null)
                CellBeginEdit(this, e);
        }

        /// <summary>
        /// Method called when cell ends editing</summary>
        /// <param name="e">ListView cell event args</param>
        protected void OnCellEndEdit(ListViewCellEventArgs e)
        {
            if (CellEndEdit != null)
                CellEndEdit(this, e);
        }

        /// <summary>
        /// Method called when cell is being validated</summary>
        /// <param name="e">ListView cell validating event args</param>
        protected void OnCellCellValidating(ListViewCellValidatingEventArgs e)
        {
            if (CellValidating != null)
                CellValidating(this, e);
        }
        

        private void textBox_LostFocus(object sender, EventArgs e)
        {
            DisableEditingControl(true);
        }

        private void textBox_DragOver(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragOver(e);
        }

        private void textBox_DragDrop(object sender, DragEventArgs e)
        {
            // raise event on this control
            OnDragDrop(e);
        }

        private void textBox_MouseHover(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseHover(e);
        }

        private void textBox_MouseLeave(object sender, EventArgs e)
        {
            // raise event on this control
            OnMouseLeave(e);
        }

        private void EnableEditingControl()
        {
            m_activeEditingControl.Leave += activeEditingControl_Leave;
            m_activeEditingControl.KeyPress += activeEditingControl_KeyPress;
            if (m_activeEditingControl == m_textBox)
            {
                SetTextBoxFromProperty();
                EnableTextBox();
            }
            else if (m_activeEditingControl == m_comboBox)
            {
                SetComboBoxFromProperty();
                EnableComboBox();
            }
        }

        /// <summary>
        /// Method called when Control key pressed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Key press event args</param>
        void activeEditingControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verify that the user presses ESC.
            switch (e.KeyChar)
            {
                case (char)(int)Keys.Escape:
                    {
                        // Reset to the original  value, and then hide the editing control.
                        DisableEditingControl(false);
                        break;
                    }

                case (char)(int)Keys.Enter:
                    {
                        // Hide the editing control.
                        DisableEditingControl(true);
                        break;
                    }
            }
        }

        /// <summary>
        /// Method called when leaving control</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void activeEditingControl_Leave(object sender, EventArgs e)
        {
           
        }

        /// <summary>
        /// Method called when combo box drop down closed</summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        void comboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (m_comboBox.SelectedIndex >=0)
            {
                m_comboBox.Text = m_comboBox.Items[m_comboBox.SelectedIndex].ToString();
                DisableEditingControl(true);
            }
            else
                DisableEditingControl(false);
        }

        private void DisableEditingControl(bool acceptNewValue)
        {
            if (m_activeEditingControl == null)
                return;

            bool restore = !acceptNewValue;
            if (acceptNewValue)
            {
                var e = new ListViewCellValidatingEventArgs(m_currentRow,m_currentCol, m_activeEditingControl.Text);
                OnCellCellValidating(e);
                if (e.Cancel)
                    restore = true;
                else
                {
                    if (m_activeEditingControl == m_textBox)
                        SetPropertyFromTextBox();
                    else if (m_activeEditingControl == m_comboBox)
                        SetPropertyFromComboBox();
                }
            }
            if (restore)
            {
                if (m_activeEditingControl == m_textBox)
                    SetTextBoxFromProperty();
                else if (m_activeEditingControl == m_comboBox)
                    SetComboBoxFromProperty();
              
            }
            m_activeEditingControl.Leave -= activeEditingControl_Leave;
            m_activeEditingControl.KeyPress -= activeEditingControl_KeyPress;
            if (m_activeEditingControl == m_textBox)
                DisableTextBox();
            else if (m_activeEditingControl == m_comboBox)
                DisableComboBox();
            m_activeEditingControl = null;

            OnCellEndEdit(new ListViewCellEventArgs(m_currentRow, m_currentCol));

            Focus();
        }


        private void EnableTextBox()
        {
            m_textBox.Show();
            m_textBox.Focus();
        }

        private void DisableTextBox()
        {
            m_textBox.Hide();
        }

        private void EnableComboBox()
        {
            m_comboBox.DroppedDown = true;
            m_comboBox.Show();
        }

        private void DisableComboBox()
        {
            m_comboBox.Hide();
        }

        private void SetTextBoxFromProperty()
        {

            string propertyText = PropertyUtils.GetPropertyText(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol]);
            m_textBox.Text = propertyText;
            m_textBox.ReadOnly = m_propertyDescriptors[m_currentCol].IsReadOnly;
            
        }

        private void SetPropertyFromTextBox()
        {
            if (m_settingValue)
                return;

            try
            {
                m_settingValue = true;

                string newText = m_textBox.Text;
                object value;
                if (TryConvertString(newText, out value))
                    PropertyUtils.SetProperty(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol], value);
            }
            finally
            {
                m_settingValue = false;
            }
        }

        private bool TryConvertString(string newText, out object value)
        {
            bool succeeded = false;
            value = newText;
            try
            {
                TypeConverter converter = m_propertyDescriptors[m_currentCol].Converter;
                if (converter != null &&
                    value != null &&
                    converter.CanConvertFrom(value.GetType()))
                {
                    value = converter.ConvertFrom(value);
                }
                succeeded = true;
            }
            catch (Exception ex)
            {
                // NotSupportedException, FormatException, and Exception can be thrown. For example,
                // for a string "14'" being converted to an Int32. So, I made this a catch (Exception). --Ron
                //CancelEdit();
                MessageBox.Show(ex.Message, "Error".Localize());
            }

            return succeeded;
        }

        private void SetComboBoxFromProperty()
        {
            string propertyText = PropertyUtils.GetPropertyText(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol]);
            m_comboBox.Text = propertyText;
            m_comboBox.SelectedItem = m_propertyDescriptors[m_currentCol].GetValue(m_currencyManager.List[m_currentRow]);
        }

        private void SetPropertyFromComboBox()
        {
            if (m_settingValue)
                return;

            try
            {
                m_settingValue = true;
                PropertyUtils.SetProperty(m_currencyManager.List[m_currentRow], m_propertyDescriptors[m_currentCol], m_comboBox.SelectedItem);
            }
            finally
            {
                m_settingValue = false;
            }
        }

        private bool m_settingValue;
        private bool m_itemAlreadySelected;

        #endregion

        #region grouping

        private ListViewGroup GroupingDataItem(object dataItem)
        {
            ListViewGroup group = null;
            System.Reflection.MemberInfo info = dataItem.GetType();
            object[] attributes = info.GetCustomAttributes(true);
            for (int i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] is GroupAttribute)
                {
                    GroupAttribute groupAttribute = (GroupAttribute)attributes[i];
                    foreach (ListViewGroup grp in Groups)
                    {
                        if (grp.Name == groupAttribute.GroupName)
                        {
                            group = grp;
                           
                            break;
                        }
                    }

                    if (group == null)
                    {
                        group = new ListViewGroup(groupAttribute.GroupName, groupAttribute.Header);
                        Groups.Add(group);
                    }
                      
                    if (groupAttribute.ReadOnlyProperties != null)
                    {
                        string[] pdNames = groupAttribute.ReadOnlyProperties.Split(',');
                        foreach (var pdName in pdNames)
                        {
                            string propertyName = pdName.Localize(); // the string literals use Localize() elsewhere
                            m_groupReadOnlyColumns.Add(groupAttribute.GroupName, propertyName);
                        }
                    }

                    if (groupAttribute.ExternalEditorProperties != null)
                    {
                        string[] pdNames = groupAttribute.ExternalEditorProperties.Split(',');
                        foreach (var pdName in pdNames)
                        {
                            string propertyName = pdName.Localize(); // the string literals use Localize() elsewhere
                            m_groupExternalEditorColumns.Add(groupAttribute.GroupName, propertyName);
                        }
                    }
                    break;
                }
            }

            return group;
        }

        #endregion

        private Multimap<string, string> m_groupReadOnlyColumns = new Multimap<string, string>(); // keyed by group name
        private Multimap<string, string> m_groupExternalEditorColumns = new Multimap<string, string>(); // keyed by group name

        private ListChangedEventHandler currencyManager_ListChanged;
        private EventHandler currencyManager_PositionChanged;
 
        private object m_dataSource;
        private string m_dataMember;
        private CurrencyManager m_currencyManager;
        private PropertyDescriptorCollection m_propertyDescriptors;

        private readonly TextBox m_textBox;     // the default editing control
        private readonly ComboBox m_comboBox;   // for enum types
        private Control m_activeEditingControl;
        private int m_currentRow;
        private int m_currentCol;

        private ListSortDirection m_sortDirection = ListSortDirection.Ascending;
        private int m_sortColumn = -1;
        private List<object> m_selectedObjects= new List<object>();
        private List<object> m_checkeObjects = new List<object>();
        private int m_checkBoxWidth = 0;
        private bool m_autoColumnWidth = true;
        private bool m_adjustingColumnWidths;
        private int m_lastNewWidth;
        private int m_rowHeight = 17;
        private readonly string m_tickSymbol = new string('\u2714', 1);

        private const int TextOffset = 8;

        private static readonly Image s_sortAscendingImage = ResourceUtil.GetImage(Resources.SortAscendingImage);
        private static readonly Image s_sortDescendingImage = ResourceUtil.GetImage(Resources.SortDescendingImage);

    }

}
