////Sony Computer Entertainment Confidential

//using System;
//using System.Text;
//using System.Collections;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Windows.Forms;
//using System.Xml;

//using Sce.Atf.Adaptation;

//namespace Sce.Atf.Applications
//{
//    /// <summary>
//    /// Adaptor that adapts a System.Windows.Forms.ListView Control to an IListView</summary>
//    public class DataGridViewAdapter
//    {
//        /// <summary>
//        /// Constructor that creates a new ListView Control</summary>
//        public DataGridViewAdapter()
//            : this(new DataGridView())
//        {
//        }

//        /// <summary>
//        /// Constructor</summary>
//        /// <param name="listView">ListView Control to adapt</param>
//        public DataGridViewAdapter(DataGridView listView)
//        {
//            m_control = listView;

//            // default to allow sorting
//            m_allowSorting = true;
//            m_control.ListViewItemSorter = new ListViewItemSorter(m_control);

//            m_control.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler(control_ItemSelectionChanged);
//            m_control.AfterLabelEdit += new LabelEditEventHandler(control_AfterLabelEdit);
//            m_control.ColumnWidthChanged += new DataGridViewColumnEventHandler(control_ColumnWidthChanged);
//            m_control.MouseDown += new MouseEventHandler(control_MouseDown);
//            m_control.MouseUp += new MouseEventHandler(control_MouseUp);
//            m_control.DragOver += new DragEventHandler(control_DragOver);
//            m_control.DragDrop += new DragEventHandler(control_DragDrop);
//        }

//        /// <summary>
//        /// Gets the ListView Control</summary>
//        public ListView Control
//        {
//            get { return m_control; }
//        }

//        /// <summary>
//        /// Gets or sets a value specifying whether the name column should be visible</summary>
//        public bool ShowNameColumn
//        {
//            get { return m_showNameColumn; }
//            set
//            {
//                m_showNameColumn = value;
//                m_control.LabelEdit = false;
//                m_control.Invalidate();
//            }
//        }

//        /// <summary>
//        /// Gets or sets a value specifying whether the list items can be sorted</summary>
//        public bool AllowSorting
//        {
//            get { return m_allowSorting; }
//            set
//            {
//                if (m_allowSorting != value)
//                {
//                    m_allowSorting = value;
//                    m_control.ListViewItemSorter = value ? new ListViewItemSorter(m_control) : null;
//                }
//            }
//        }

//        /// <summary>
//        /// Gets or sets the list data</summary>
//        public IListView ListView
//        {
//            get { return m_listView; }
//            set
//            {
//                if (m_listView != value)
//                {
//                    if (m_listView != null)
//                    {
//                        m_itemView = null;

//                        if (m_observableContext != null)
//                        {
//                            m_observableContext.ItemInserted -= new EventHandler<ItemInsertedEventArgs<object>>(list_ItemInserted);
//                            m_observableContext.ItemRemoved -= new EventHandler<ItemRemovedEventArgs<object>>(list_ItemRemoved);
//                            m_observableContext.ItemChanged -= new EventHandler<ItemChangedEventArgs<object>>(list_ItemChanged);
//                            m_observableContext.Reloaded -= new EventHandler(list_Reloaded);
//                            m_observableContext = null;
//                        }

//                        if (m_validationContext != null)
//                        {
//                            m_validationContext.Beginning -= new EventHandler(validationContext_Beginning);
//                            m_validationContext.Ended -= new EventHandler(validationContext_Ended);
//                            m_validationContext.Cancelled -= new EventHandler(validationContext_Cancelled);
//                            m_validationContext = null;
//                        }

//                        if (m_selectionContext != null)
//                        {
//                            m_selectionContext.SelectionChanged -= new EventHandler(selection_Changed);
//                            m_selectionContext = null;
//                        }
//                    }

//                    m_listView = value;

//                    if (m_listView != null)
//                    {
//                        m_itemView = Adapters.As<IItemView>(m_listView);

//                        m_observableContext = Adapters.As<IObservableContext>(m_listView);
//                        if (m_observableContext != null)
//                        {
//                            m_observableContext.ItemInserted += new EventHandler<ItemInsertedEventArgs<object>>(list_ItemInserted);
//                            m_observableContext.ItemRemoved += new EventHandler<ItemRemovedEventArgs<object>>(list_ItemRemoved);
//                            m_observableContext.ItemChanged += new EventHandler<ItemChangedEventArgs<object>>(list_ItemChanged);
//                            m_observableContext.Reloaded += new EventHandler(list_Reloaded);
//                        }

//                        m_validationContext = Adapters.As<IValidationContext>(m_listView);
//                        if (m_validationContext != null)
//                        {
//                            m_validationContext.Beginning += new EventHandler(validationContext_Beginning);
//                            m_validationContext.Ended += new EventHandler(validationContext_Ended);
//                            m_validationContext.Cancelled += new EventHandler(validationContext_Cancelled);
//                        }

//                        m_selectionContext = Adapters.As<ISelectionContext>(m_listView);
//                        if (m_selectionContext != null)
//                        {
//                            m_selectionContext.SelectionChanged += new EventHandler(selection_Changed);
//                        }
//                    }
//                }

//                Load();
//            }
//        }

//        /// <summary>
//        /// Gets the last object that the user clicked or dragged over</summary>
//        public object LastHit
//        {
//            get { return m_lastHit; }
//        }

//        /// <summary>
//        /// Event that is raised after the last hit object changes</summary>
//        public event EventHandler LastHitChanged;

//        /// <summary>
//        /// Raises the LastHitChanged event</summary>
//        /// <param name="e">Event args</param>
//        protected virtual void OnLastHitChanged(EventArgs e)
//        {
//            Event.Raise(LastHitChanged, this, e);
//        }

//        /// <summary>
//        /// Gets the item under the given point</summary>
//        /// <param name="clientPoint">Point, in client coordinates</param>
//        /// <returns>Item under point, or null if none</returns>
//        public object GetItemAt(Point clientPoint)
//        {
//            ListViewItem item = m_control.GetItemAt(clientPoint.X, clientPoint.Y);
//            if (item != null)
//                return item.Tag;

//            return null;
//        }

//        /// <summary>
//        /// Event that is raised when user edits an item label</summary>
//        public event EventHandler<LabelEditedEventArgs<object>> LabelEdited;

//        /// <summary>
//        /// Event that is raised when user selects or deselects an item</summary>
//        public event EventHandler<ItemSelectedEventArgs<object>> ItemSelected;

//        /// <summary>
//        /// Gets the items corresponding to the selected list view items</summary>
//        /// <returns>items corresponding to the selected list view items</returns>
//        public object[] GetSelectedItems()
//        {
//            List<object> items = new List<object>();
//            foreach (ListViewItem item in m_control.SelectedItems)
//                items.Add(item.Tag);

//            return items.ToArray();
//        }

//        /// <summary>
//        /// Gets and sets the persistent state for the control</summary>
//        public string Settings
//        {
//            get
//            {
//                XmlDocument xmlDoc = new XmlDocument();
//                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
//                XmlElement root = xmlDoc.CreateElement("Columns");
//                xmlDoc.AppendChild(root);

//                foreach (KeyValuePair<string, int> pair in m_columnWidths)
//                {
//                    XmlElement columnElement = xmlDoc.CreateElement("Column");
//                    root.AppendChild(columnElement);

//                    columnElement.SetAttribute("Name", pair.Key);
//                    columnElement.SetAttribute("Width", pair.Value.ToString());
//                }
//                return xmlDoc.InnerXml;
//            }
//            set
//            {
//                if (string.IsNullOrEmpty(value))
//                    return;

//                XmlDocument xmlDoc = new XmlDocument();
//                xmlDoc.LoadXml(value);

//                XmlElement root = xmlDoc.DocumentElement;
//                if (root == null || root.Name != "Columns")
//                    throw new Exception("Invalid ListView settings");

//                XmlNodeList columns = root.SelectNodes("Column");
//                foreach (XmlElement columnElement in columns)
//                {
//                    string name = columnElement.GetAttribute("Name");
//                    string widthString = columnElement.GetAttribute("Width");
//                    int width;
//                    if (widthString != null && int.TryParse(widthString, out width))
//                    {
//                        m_columnWidths[name] = width;
//                    }
//                }

//                m_control.SuspendLayout();

//                foreach (ColumnHeader column in m_control.Columns)
//                    SetColumnWidth(column);

//                m_control.ResumeLayout();
//            }
//        }

//        private void control_DragDrop(object sender, DragEventArgs e)
//        {
//            Point clientPoint = m_control.PointToClient(new Point(e.X, e.Y));
//            SetLastHit(clientPoint);
//        }

//        private void control_DragOver(object sender, DragEventArgs e)
//        {
//            Point clientPoint = m_control.PointToClient(new Point(e.X, e.Y));
//            SetLastHit(clientPoint);
//        }

//        private void control_MouseUp(object sender, MouseEventArgs e)
//        {
//            Point clientPoint = new Point(e.X, e.Y);
//            SetLastHit(clientPoint);
//        }

//        private void control_MouseDown(object sender, MouseEventArgs e)
//        {
//            Point clientPoint = new Point(e.X, e.Y);
//            SetLastHit(clientPoint);
//        }

//        private void control_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
//        {
//            if (m_selectionContext != null && !m_changingSelection)
//            {
//                try
//                {
//                    m_changingSelection = true;

//                    ListViewItem listViewItem = e.Item;
//                    object item = listViewItem.Tag;
//                    if (e.IsSelected)
//                    {
//                        ItemInfo info = new ItemInfo(m_control.SmallImageList);
//                        m_itemView.GetInfo(item, info);
//                        if (info.AllowSelect)
//                        {
//                            OnItemSelected(item, true);
//                            SelectionContexts.Add(m_selectionContext, item);
//                        }
//                        else
//                        {
//                            listViewItem.Selected = false; // override the user's selection
//                        }
//                    }
//                    else
//                    {
//                        OnItemSelected(item, false);
//                        SelectionContexts.Remove(m_selectionContext, item);
//                    }
//                }
//                finally
//                {
//                    m_changingSelection = false;
//                }
//            }
//        }

//        private void OnItemSelected(object t, bool selected)
//        {
//            EventHandler<ItemSelectedEventArgs<object>> handler = ItemSelected;
//            if (handler != null)
//                handler(this, new ItemSelectedEventArgs<object>(t, selected));
//        }

//        private void control_AfterLabelEdit(object sender, LabelEditEventArgs e)
//        {
//            EventHandler<LabelEditedEventArgs<object>> handler = LabelEdited;
//            if (handler != null &&
//                e.Label != null) // this happens when column is resized during label edit
//            {
//                ListViewItem item = m_control.Items[e.Item];
//                handler(this, new LabelEditedEventArgs<object>(item.Tag, e.Label));
//            }
//        }

//        private void control_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
//        {
//            DataGridViewColumn column = e.Column;
//            m_columnWidths[column.HeaderText] = column.Width;
//        }

//        private void list_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
//        {
//            DataGridViewRow item = CreateItem(e.Item);
//            m_control.Rows.Insert(e.Index, new DataGridViewRow(
//            m_control.Items.Insert(e.Index, item);
//        }

//        private void list_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
//        {
//            ListViewItem item = m_itemToListItemMap[e.Item] as ListViewItem;
//            if (item != null)
//            {
//                m_itemToListItemMap.Remove(item.Tag);

//                // if last hit is no longer in tree, clear it
//                if (object.Equals(m_lastHit, item.Tag))
//                    SetLastHit(null);

//                m_changingSelection = true;
//                m_control.Items.Remove(item);
//                m_changingSelection = false;
//            }
//        }

//        private void list_ItemChanged(object sender, ItemChangedEventArgs<object> e)
//        {
//            ListViewItem item;
//            if (m_itemToListItemMap.TryGetValue(e.Item, out item))
//            {
//                UpdateItem(item);
//            }
//        }

//        private void list_Reloaded(object sender, EventArgs e)
//        {
//            Load();
//        }

//        private void validationContext_Beginning(object sender, EventArgs e)
//        {
//        }

//        private void validationContext_Ended(object sender, EventArgs e)
//        {
//        }

//        private void validationContext_Cancelled(object sender, EventArgs e)
//        {
//        }

//        private void selection_Changed(object sender, EventArgs e)
//        {
//            if (!m_changingSelection)
//            {
//                try
//                {
//                    m_changingSelection = true;
//                    m_control.SelectedItems.Clear();
//                    foreach (object item in m_selectionContext.Selection)
//                    {
//                        ListViewItem listViewItem;
//                        if (m_itemToListItemMap.TryGetValue(item, out listViewItem))
//                            listViewItem.Selected = true;
//                    }
//                }
//                finally
//                {
//                    m_changingSelection = false;
//                }
//            }
//        }

//        /// <summary>
//        /// Add nodes for the model's root objects</summary>
//        private void Load()
//        {
//            Unload();

//            if (m_listView != null)
//            {
//                BuildListColumns();

//                List<ListViewItem> items = new List<ListViewItem>();
//                foreach (object item in m_listView.Items)
//                    items.Add(CreateItem(item));

//                ListViewItem[] array = new ListViewItem[items.Count];
//                items.CopyTo(array);

//                m_control.Items.AddRange(array);
//            }
//        }

//        /// <summary>
//        /// Clear all nodes and the tag-to-item map</summary>
//        private void Unload()
//        {
//            m_control.Items.Clear();
//            m_itemToListItemMap.Clear();
//        }

//        /// <summary>
//        /// Gets the object tags of the current selection</summary>
//        private List<object> GetSelectedObjects()
//        {
//            List<object> result = new List<object>();
//            foreach (ListViewItem listViewItem in m_control.SelectedItems)
//            {
//                if (listViewItem.Tag != null)
//                {
//                    object item = listViewItem.Tag;
//                    ItemInfo info = new ItemInfo(m_control.SmallImageList);
//                    m_itemView.GetInfo(item, info);

//                    if (info.AllowSelect)
//                        result.Add(item);
//                }
//            }

//            return result;
//        }

//        /// <summary>
//        /// Creates a new item to hold the given object</summary>
//        private ListViewItem CreateItem(object item)
//        {
//            m_changingSelection = true;
//            ListViewItem listItem = new ListViewItem();
//            m_changingSelection = false;

//            listItem.Tag = item;
//            m_itemToListItemMap.Add(item, listItem);

//            UpdateItem(listItem);

//            return listItem;
//        }

//        /// <summary>
//        /// Updates an item to the current state of its corresponding object</summary>
//        private void UpdateItem(ListViewItem item)
//        {
//            ItemInfo info = new ItemInfo(m_control.SmallImageList);
//            m_itemView.GetInfo(item.Tag, info);

//            string label = info.Label;

//            item.SubItems.Clear();
//            if (info.Properties != null)
//            {
//                int count = Math.Min(m_columnNames.Length, info.Properties.Length);
//                for (int i = 0; i < count; i++)
//                {
//                    if (i == 0 && !m_showNameColumn)
//                    {
//                        label = GetObjectString(info.Properties[0]);
//                        item.SubItems[0].Tag = label;
//                    }
//                    else
//                    {
//                        object value = info.Properties[i];
//                        ListViewItem.ListViewSubItem subItem =
//                            new ListViewItem.ListViewSubItem(item, GetObjectString(value));
//                        subItem.Tag = value;
//                        item.SubItems.Add(subItem);
//                    }
//                }
//            }
//            item.Text = label;
//            item.ImageIndex = info.ImageIndex;
//        }

//        private string GetObjectString(object obj)
//        {
//            IFormattable formattable = obj as IFormattable;
//            if (formattable != null)
//                return formattable.ToString(null, null);

//            return obj.ToString();
//        }

//        private void BuildListColumns()
//        {
//            // build new column names
//            List<string> columnNames = new List<string>();
//            if (m_listView != null)
//            {
//                if (m_showNameColumn)
//                    columnNames.Add("Name");
//                columnNames.AddRange(m_listView.ColumnNames);
//            }

//            // check if there is a change
//            bool columnsInvalid = m_columnNames.Length != columnNames.Count;
//            if (!columnsInvalid)
//            {
//                for (int i = 0; i < m_columnNames.Length; i++)
//                {
//                    if (m_columnNames[i] != columnNames[i])
//                    {
//                        columnsInvalid = true;
//                        break;
//                    }
//                }
//            }

//            if (columnsInvalid)
//            {
//                m_control.Columns.Clear();

//                m_columnNames = columnNames.ToArray();
//                foreach (string name in m_columnNames)
//                {
//                    ColumnHeader column = new ColumnHeader();
//                    column.Text = name;
//                    SetColumnWidth(column);
//                    m_control.Columns.Add(column);
//                }
//            }
//        }

//        private void SetColumnWidth(ColumnHeader column)
//        {
//            int width;
//            if (m_columnWidths.TryGetValue(column.Text, out width))
//                column.Width = width;
//            else
//                m_columnWidths[column.Text] = column.Width;
//        }

//        private void SetLastHit(Point clientPoint)
//        {
//            object lastHit = GetItemAt(clientPoint);
//            if (lastHit == null)
//                lastHit = ListView;

//            SetLastHit(lastHit);
//        }

//        private void SetLastHit(object lastHit)
//        {
//            if (!object.Equals(lastHit, m_lastHit))
//            {
//                m_lastHit = lastHit;
//                OnLastHitChanged(EventArgs.Empty);
//            }
//        }

//        private class ListViewItemSorter : IComparer
//        {
//            public ListViewItemSorter(ListView listView)
//            {
//                m_control = listView;
//                listView.ColumnClick += new ColumnClickEventHandler(control_ColumnClick);
//            }

//            public int Compare(object x, object y)
//            {
//                ListViewItem.ListViewSubItem subItem1 = ((ListViewItem)x).SubItems[m_column];
//                ListViewItem.ListViewSubItem subItem2 = ((ListViewItem)y).SubItems[m_column];

//                object value1, value2;
//                if (m_column == 0) // ListViewItem label?
//                {
//                    value1 = subItem1.Text;
//                    value2 = subItem2.Text;
//                }
//                else
//                {
//                    value1 = subItem1.Tag;
//                    value2 = subItem2.Tag;
//                }

//                int result;

//                IComparable comparable = value1 as IComparable;
//                if (comparable != null)
//                {
//                    result = comparable.CompareTo(value2);
//                }
//                else
//                {
//                    string str1 = GetObjectString(value1);
//                    string str2 = GetObjectString(value2);
//                    result = string.Compare(str1, str2, StringComparison.CurrentCultureIgnoreCase);
//                }

//                return result * m_direction;
//            }

//            private string GetObjectString(object obj)
//            {
//                IFormattable formattable = obj as IFormattable;
//                if (formattable != null)
//                    return formattable.ToString(null, null);

//                return obj.ToString();
//            }

//            private void control_ColumnClick(object sender, ColumnClickEventArgs e)
//            {
//                int newColumn = e.Column;
//                if (m_column == newColumn)
//                    m_direction = -m_direction;

//                m_column = newColumn;

//                m_control.ListViewItemSorter = null;
//                m_control.ListViewItemSorter = this;
//            }

//            private ListView m_control;
//            private int m_column;
//            private int m_direction = 1;
//        }

//        private DataGridView m_control;

//        private IListView m_listView;
//        private IItemView m_itemView;
//        private IValidationContext m_validationContext;
//        private IObservableContext m_observableContext;
//        private ISelectionContext m_selectionContext;

//        private object m_lastHit;

//        private Dictionary<object, ListViewItem> m_itemToListItemMap = new Dictionary<object, ListViewItem>();
//        private string[] m_columnNames = new string[0];
//        private Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();
//        private bool m_showNameColumn;
//        private bool m_allowSorting;
//        private bool m_changingSelection;
//    }
//}
