//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications
{
    /// <summary>
    /// Adapter that adapts a System.Windows.Forms.ListView control to an IListView</summary>
    public class ListViewAdapter
    {
        /// <summary>
        /// Constructor that creates a new ListView control</summary>
        public ListViewAdapter()
            : this(new ListView())
        {
        }

        /// <summary>
        /// Constructor</summary>
        /// <param name="listView">ListView control to adapt</param>
        public ListViewAdapter(ListView listView)
        {
            m_control = listView;
            m_control.View = View.Details;
            m_control.FullRowSelect = true;
            m_control.HideSelection = false;

            // default to allow sorting
            m_allowSorting = true;
            m_control.ListViewItemSorter = new ListViewItemSorter(m_control);

            m_control.AfterLabelEdit += control_AfterLabelEdit;
            m_control.ColumnWidthChanged += control_ColumnWidthChanged;
            m_control.MouseDown += control_MouseDown;
            m_control.MouseUp += control_MouseUp;
            m_control.DragOver += control_DragOver;
            m_control.DragDrop += control_DragDrop;
        }

        /// <summary>
        /// Gets the ListView control</summary>
        public ListView Control
        {
            get { return m_control; }
        }

        /// <summary>
        /// Sets a column's width</summary>
        /// <param name="columnName">The displayed name of the column</param>
        /// <param name="columnWidth">The desired width, in pixels</param>
        public void SetColumnWidth(string columnName, int columnWidth)
        {
            m_columnWidths[columnName] = columnWidth;

            m_control.SuspendLayout();
            foreach (ColumnHeader column in m_control.Columns)
            {
                if (column.Text == columnName)
                {
                    SetColumnWidth(column);
                    break;
                }
            }
            m_control.ResumeLayout();
        }

        /// <summary>
        /// Gets or sets whether the list items can be sorted</summary>
        public bool AllowSorting
        {
            get { return m_allowSorting; }
            set
            {
                if (m_allowSorting != value)
                {
                    m_allowSorting = value;
                    if (!value)
                    {
                        ((ListViewItemSorter)m_control.ListViewItemSorter).DetachFromListView();
                    }
                    m_control.ListViewItemSorter = value ? new ListViewItemSorter(m_control) : null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the list data</summary>
        public IListView ListView
        {
            get { return m_listView; }
            set
            {
                if (m_listView != value)
                {
                    if (m_listView != null)
                    {
                        m_itemView = null;

                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted -= list_ItemInserted;
                            m_observableContext.ItemRemoved -= list_ItemRemoved;
                            m_observableContext.ItemChanged -= list_ItemChanged;
                            m_observableContext.Reloaded -= list_Reloaded;
                            m_observableContext = null;
                        }

                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning -= validationContext_Beginning;
                            m_validationContext.Ended -= validationContext_Ended;
                            m_validationContext.Cancelled -= validationContext_Cancelled;
                            m_validationContext = null;
                        }

                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanged -= selection_Changed;
                            m_selectionContext = null;
                        }
                    }

                    m_listView = value;

                    if (m_listView != null)
                    {
                        m_itemView = m_listView.As<IItemView>();

                        m_observableContext = m_listView.As<IObservableContext>();
                        if (m_observableContext != null)
                        {
                            m_observableContext.ItemInserted += list_ItemInserted;
                            m_observableContext.ItemRemoved += list_ItemRemoved;
                            m_observableContext.ItemChanged += list_ItemChanged;
                            m_observableContext.Reloaded += list_Reloaded;
                        }

                        m_validationContext = m_listView.As<IValidationContext>();
                        if (m_validationContext != null)
                        {
                            m_validationContext.Beginning += validationContext_Beginning;
                            m_validationContext.Ended += validationContext_Ended;
                            m_validationContext.Cancelled += validationContext_Cancelled;
                        }

                        m_selectionContext = m_listView.As<ISelectionContext>();
                        if (m_selectionContext != null)
                        {
                            m_selectionContext.SelectionChanged += selection_Changed;
                        }
                    }
                }

                Load();
            }
        }

        /// <summary>
        /// Gets the last object that the user clicked or dragged over</summary>
        public object LastHit
        {
            get { return m_lastHit; }
        }

        /// <summary>
        /// Event that is raised after the last hit object changes</summary>
        public event EventHandler LastHitChanged;

        /// <summary>
        /// Raises the LastHitChanged event</summary>
        /// <param name="e">Event args</param>
        protected virtual void OnLastHitChanged(EventArgs e)
        {
            LastHitChanged.Raise(this, e);
        }

        /// <summary>
        /// Gets the item under the given point</summary>
        /// <param name="clientPoint">Point in client coordinates</param>
        /// <returns>Item under point, or null if none</returns>
        public object GetItemAt(Point clientPoint)
        {
            ListViewItem item = m_control.GetItemAt(clientPoint.X, clientPoint.Y);
            if (item != null)
                return item.Tag;

            return null;
        }

        /// <summary>
        /// Event that is raised when user edits an item label</summary>
        public event EventHandler<LabelEditedEventArgs<object>> LabelEdited;

        /// <summary>
        /// Event that is raised when user selects or deselects an item</summary>
        public event EventHandler<ItemSelectedEventArgs<object>> ItemSelected;

        /// <summary>
        /// Gets the items corresponding to the selected list view items</summary>
        /// <returns>Items corresponding to the selected list view items</returns>
        public object[] GetSelectedItems()
        {
            List<object> items = new List<object>();
            foreach (ListViewItem item in m_control.SelectedItems)
                items.Add(item.Tag);

            return items.ToArray();
        }

        /// <summary>
        /// Gets and sets the persistent state for the control</summary>
        public string Settings
        {
            get
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
                XmlElement root = xmlDoc.CreateElement("Columns");
                xmlDoc.AppendChild(root);

                foreach (KeyValuePair<string, int> pair in m_columnWidths)
                {
                    XmlElement columnElement = xmlDoc.CreateElement("Column");
                    root.AppendChild(columnElement);

                    columnElement.SetAttribute("Name", pair.Key);
                    columnElement.SetAttribute("Width", pair.Value.ToString());
                }
                return xmlDoc.InnerXml;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    return;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(value);

                XmlElement root = xmlDoc.DocumentElement;
                if (root == null || root.Name != "Columns")
                    throw new Exception("Invalid ListView settings");

                XmlNodeList columns = root.SelectNodes("Column");
                foreach (XmlElement columnElement in columns)
                {
                    string name = columnElement.GetAttribute("Name");
                    string widthString = columnElement.GetAttribute("Width");
                    int width;
                    if (widthString != null && int.TryParse(widthString, out width))
                    {
                        m_columnWidths[name] = width;
                    }
                }

                m_control.SuspendLayout();

                foreach (ColumnHeader column in m_control.Columns)
                    SetColumnWidth(column);

                m_control.ResumeLayout();
            }
        }

        /// <summary>
        /// Looks up the ListViewItem associated with the specified item; returns null if not found</summary>
        /// <param name="item">Item as provided by ListView.Items</param>
        /// <returns>ListViewItem for the item if found, null otherwise</returns>
        public ListViewItem GetListViewItem(object item)
        {
            ListViewItem listViewItem;
            m_itemToListItemMap.TryGetValue(item, out listViewItem);
            return listViewItem;
        }

        private void control_DragDrop(object sender, DragEventArgs e)
        {
            Point clientPoint = m_control.PointToClient(new Point(e.X, e.Y));
            SetLastHit(clientPoint);
        }

        private void control_DragOver(object sender, DragEventArgs e)
        {
            Point clientPoint = m_control.PointToClient(new Point(e.X, e.Y));
            SetLastHit(clientPoint);
        }

        private void control_MouseUp(object sender, MouseEventArgs e)
        {
            if (m_selectionContext == null)
                return;
            
            Point clientPoint = new Point(e.X, e.Y);
            SetLastHit(clientPoint);

            ListViewItem hitItem = Control.GetItemAt(e.X, e.Y);
            if (hitItem == null)
                return;

            object item = hitItem.Tag;
            if (item == null)
                return;

            HashSet<object> oldSelection = null;
            HashSet<object> newSelection = null;
            var handler = ItemSelected;
            if (handler != null)
                oldSelection = new HashSet<object>(m_selectionContext.Selection);

            Keys keys = System.Windows.Forms.Control.ModifierKeys;
            if (keys == Keys.Shift)
            {
                if (m_selectionStartItem != null)
                {
                    int startIndex = Math.Min(m_selectionStartItem.Index, hitItem.Index);
                    int endIndex = Math.Max(m_selectionStartItem.Index, hitItem.Index);
                    newSelection = new HashSet<object>();
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        var listViewItem = Control.Items[i];
                        if (listViewItem != null && listViewItem.Tag != null)
                            newSelection.Add(listViewItem.Tag);
                    }
                    m_selectionContext.SetRange(newSelection);
                }
                else
                {
                    m_selectionContext.Set(hitItem);
                    m_selectionStartItem = hitItem;
                }
            }
            else if (keys == Keys.Control)
            {
                if (m_selectionContext.SelectionContains(item))
                    m_selectionContext.Remove(item);
                else
                    m_selectionContext.Add(item);
                m_selectionStartItem = hitItem;
            }
            else
            {
                m_selectionContext.Set(item);
                m_selectionStartItem = hitItem;
            }

            if (handler != null)
            {
                if (newSelection == null)
                    newSelection = new HashSet<object>(m_selectionContext.Selection);

                foreach (object removed in oldSelection.Except(newSelection))
                    OnItemSelected(removed, false);

                foreach (object added in newSelection.Except(oldSelection))
                    OnItemSelected(added, true);
            }
        }

        private void control_MouseDown(object sender, MouseEventArgs e)
        {
            Point clientPoint = new Point(e.X, e.Y);
            SetLastHit(clientPoint);
        }

        private void OnItemSelected(object t, bool selected)
        {
            EventHandler<ItemSelectedEventArgs<object>> handler = ItemSelected;
            if (handler != null)
                handler(this, new ItemSelectedEventArgs<object>(t, selected));
        }

        private void control_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            EventHandler<LabelEditedEventArgs<object>> handler = LabelEdited;
            if (handler != null &&
                e.Label != null) // this happens when column is resized during label edit
            {
                ListViewItem item = m_control.Items[e.Item];
                handler(this, new LabelEditedEventArgs<object>(item.Tag, e.Label));
            }
        }

        private void control_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            ColumnHeader column = m_control.Columns[e.ColumnIndex];
            m_columnWidths[column.Text] = column.Width;
        }

        private void list_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            if (m_inTransaction && !m_itemsInserted.ContainsKey(e.Item))
                m_itemsInserted.Add(e.Item, e.Index);
            else
                OnItemInserted(e.Item, e.Index);
        }

        private void OnItemInserted(object item, int index)
        {
            if (GetListViewItem(item) == null)
            {
                ListViewItem listViewItem = CreateItem(item);
                if (index < m_control.Items.Count)
                    m_control.Items.Insert(index, listViewItem);
                else
                    m_control.Items.Add(listViewItem);
            }
        }

        private void list_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            if (m_inTransaction)
                m_itemsRemoved.Add(e.Item);
            else
                OnItemRemoved(e.Item);
        }

        private void OnItemRemoved(object item)
        {
            ListViewItem listViewItem;
            if (m_itemToListItemMap.TryGetValue(item, out listViewItem))
            {
                m_itemToListItemMap.Remove(listViewItem.Tag);

                // if last hit is no longer in tree, clear it
                if (object.Equals(m_lastHit, listViewItem.Tag))
                    SetLastHit(null);

                m_changingSelection = true;
                m_control.Items.Remove(listViewItem);
                m_changingSelection = false;
            }
        }

        private void list_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            if (m_inTransaction)
                m_itemsChanged.Add(e.Item);
            else
                OnItemChanged(e.Item);
        }

        private void OnItemChanged(object item)
        {
            ListViewItem listViewItem;
            if (m_itemToListItemMap.TryGetValue(item, out listViewItem))
            {
                UpdateItem(listViewItem);
            }
        }

        private void list_Reloaded(object sender, EventArgs e)
        {
            Load();
        }

        private void validationContext_Beginning(object sender, EventArgs e)
        {
            m_inTransaction = true;
        }

        private void validationContext_Ended(object sender, EventArgs e)
        {
            m_inTransaction = false;

            // Don't process changes for items that have been added or removed
            m_itemsChanged.RemoveWhere(item => m_itemsInserted.ContainsKey(item) || m_itemsRemoved.Contains(item));

            if (!m_itemsInserted.Any() && !m_itemsRemoved.Any() && !m_itemsChanged.Any())
                return;

            m_control.SuspendLayout();
            m_control.BeginUpdate();

            foreach (var insertPair in m_itemsInserted)
                OnItemInserted(insertPair.Key, insertPair.Value);
            m_itemsInserted.Clear();

            foreach (object removedItem in m_itemsRemoved)
                OnItemRemoved(removedItem);
            m_itemsRemoved.Clear();

            foreach (object changedItem in m_itemsChanged)
                OnItemChanged(changedItem);
            m_itemsChanged.Clear();

            if (m_selectionContext != null)
            {
                foreach (var pair in m_itemToListItemMap)
                    pair.Value.Selected = m_selectionContext.SelectionContains(pair.Key);
            }
            m_control.FocusedItem = null;

            m_control.EndUpdate();
            m_control.ResumeLayout();
        }

        private void validationContext_Cancelled(object sender, EventArgs e)
        {
            m_inTransaction = false;

            // Clear all pending lists, as the transaction has been cancelled
            m_itemsInserted.Clear();
            m_itemsRemoved.Clear();
            m_itemsChanged.Clear();
        }

        private void selection_Changed(object sender, EventArgs e)
        {
            if (!m_changingSelection)
            {
                try
                {
                    m_changingSelection = true;
                    m_control.SelectedItems.Clear();
                    foreach (object item in m_selectionContext.Selection)
                    {
                        ListViewItem listViewItem;
                        if (m_itemToListItemMap.TryGetValue(item, out listViewItem))
                            listViewItem.Selected = true;
                    }
                }
                finally
                {
                    m_changingSelection = false;
                }
            }
        }

        /// <summary>
        /// Adds nodes for the model's root objects</summary>
        private void Load()
        {
            Unload();

            if (m_listView != null)
            {
                BuildListColumns();

                List<ListViewItem> items = new List<ListViewItem>();
                foreach (object item in m_listView.Items)
                    items.Add(CreateItem(item));

                ListViewItem[] array = new ListViewItem[items.Count];
                items.CopyTo(array);

                m_control.Items.AddRange(array);
            }
        }

        /// <summary>
        /// Clear all nodes and the tag-to-item map</summary>
        private void Unload()
        {
            m_control.Items.Clear();
            m_itemToListItemMap.Clear();
        }

        /// <summary>
        /// Gets the object tags of the current selection</summary>
        private List<object> GetSelectedObjects()
        {
            List<object> result = new List<object>();
            foreach (ListViewItem listViewItem in m_control.SelectedItems)
            {
                if (listViewItem.Tag != null)
                {
                    object item = listViewItem.Tag;
                    ItemInfo info = new WinFormsItemInfo(m_control.SmallImageList);
                    m_itemView.GetInfo(item, info);

                    if (info.AllowSelect)
                        result.Add(item);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a new item to hold the given object</summary>
        private ListViewItem CreateItem(object item)
        {
            m_changingSelection = true;
            ListViewItem listItem = new ListViewItem();
            m_changingSelection = false;

            listItem.Tag = item;
            m_itemToListItemMap.Add(item, listItem);

            UpdateItem(listItem);

            return listItem;
        }

        /// <summary>
        /// Updates an item to the current state of its corresponding object</summary>
        private void UpdateItem(ListViewItem item)
        {
            ItemInfo info = new WinFormsItemInfo(m_control.SmallImageList);
            m_itemView.GetInfo(item.Tag, info);

            string label = info.Label;

            item.SubItems.Clear();
            if (info.Properties != null)
            {
                int count = Math.Min(m_columnNames.Length, info.Properties.Length);
                for (int i = 0; i < count; i++)
                {
                    object value = info.Properties[i];
                    ListViewItem.ListViewSubItem subItem =
                        new ListViewItem.ListViewSubItem(item, GetObjectString(value));
                    subItem.Tag = value;
                    item.SubItems.Add(subItem);
                }
            }
            item.Text = label;
            item.ImageIndex = info.ImageIndex;
            item.Checked = info.Checked;
            if (item.Font.Style != info.FontStyle)
                item.Font = new Font(item.Font, info.FontStyle);
        }

        private string GetObjectString(object obj)
        {
            IFormattable formattable = obj as IFormattable;
            if (formattable != null)
                return formattable.ToString(null, null);

            return obj.ToString();
        }

        private void BuildListColumns()
        {
            // build new column names
            List<string> columnNames = new List<string>();
            if (m_listView != null)
                columnNames.AddRange(m_listView.ColumnNames);

            // check if there is a change
            bool columnsInvalid = m_columnNames.Length != columnNames.Count;
            if (!columnsInvalid)
            {
                for (int i = 0; i < m_columnNames.Length; i++)
                {
                    if (m_columnNames[i] != columnNames[i])
                    {
                        columnsInvalid = true;
                        break;
                    }
                }
            }

            if (columnsInvalid)
            {
                m_control.Columns.Clear();

                m_columnNames = columnNames.ToArray();
                foreach (string name in m_columnNames)
                {
                    ColumnHeader column = new ColumnHeader();
                    column.Text = name;
                    SetColumnWidth(column);
                    m_control.Columns.Add(column);
                }
            }
        }

        private void SetColumnWidth(ColumnHeader column)
        {
            int width;
            if (m_columnWidths.TryGetValue(column.Text, out width))
                column.Width = width;
            else
                m_columnWidths[column.Text] = column.Width;
        }

        private void SetLastHit(Point clientPoint)
        {
            object lastHit = GetItemAt(clientPoint);
            if (lastHit == null)
                lastHit = ListView;

            SetLastHit(lastHit);
        }

        private void SetLastHit(object lastHit)
        {
            if (!object.Equals(lastHit, m_lastHit))
            {
                m_lastHit = lastHit;
                OnLastHitChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Comparer class for sorting list view items</summary>
        public class ListViewItemSorter : IComparer
        {
            /// <summary>
            /// Constructor</summary>
            /// <param name="listView">ListView</param>
            public ListViewItemSorter(ListView listView)
            {
                m_listView = listView;
                listView.ColumnClick += listview_ColumnClick;
            }

            /// <summary>
            /// Compare method for objects</summary>
            /// <param name="x">Object 1</param>
            /// <param name="y">Object 2</param>
            /// <returns>-1 if Object 1 before Object 2, 0 if objects identical, 1 if Object 1 after Object 2</returns>
            public int Compare(object x, object y)
            {
                if (m_column ==-1)
                    return 0;
                ListViewItem.ListViewSubItem subItem1 = ((ListViewItem)x).SubItems[m_column];
                ListViewItem.ListViewSubItem subItem2 = ((ListViewItem)y).SubItems[m_column];

                if (subItem1 == null && subItem2 == null)
                    return 0;
                if (subItem1 == null)
                    return -1;
                if (subItem2 == null)
                    return 1;

                object value1, value2;
                if (subItem1.Tag == null && subItem2.Tag ==null) 
                {
                    value1 = subItem1.Text;
                    value2 = subItem2.Text;
                }
                else
                {
                    value1 = subItem1.Tag;
                    value2 = subItem2.Tag;
                }

                int result;

                IComparable comparable = value1 as IComparable;
                if (comparable != null)
                {
                    result = comparable.CompareTo(value2);
                }
                else
                {
                    string str1 = GetObjectString(value1);
                    string str2 = GetObjectString(value2);
                    result = string.Compare(str1, str2, StringComparison.CurrentCultureIgnoreCase);
                }

                return result * m_direction;
            }

            /// <summary>
            /// Removes this ListViewItemSorter as a listener to the ListView events.</summary>
            public void DetachFromListView()
            {
                m_listView.ColumnClick -= listview_ColumnClick;
            }

            private string GetObjectString(object obj)
            {
                IFormattable formattable = obj as IFormattable;
                if (formattable != null)
                    return formattable.ToString(null, null);

                return obj.ToString();
            }

            private void listview_ColumnClick(object sender, ColumnClickEventArgs e)
            {
                IntPtr hHeader = (IntPtr) Sce.Atf.User32.SendMessage(m_listView.Handle, Sce.Atf.User32.LVM_GETHEADER, (int)IntPtr.Zero, (int)IntPtr.Zero);
                IntPtr newColumn = new IntPtr(e.Column);
                IntPtr prevColumn = new IntPtr(m_column);
                User32.HDITEM hdItem;

                if (m_column == e.Column)
                    m_direction = -m_direction;
                else if (m_column != -1) // Only update the previous item if it existed and if it was a different one.
                {
                    // Clear icon from the previous column.
                    hdItem = new User32.HDITEM();
                    hdItem.mask = User32.HDI_FORMAT;
                    User32.SendMessageITEM(hHeader, User32.HDM_GETITEM, prevColumn, ref hdItem);
                    hdItem.fmt &= ~User32.HDF_SORTDOWN & ~User32.HDF_SORTUP;
                    User32.SendMessageITEM(hHeader, User32.HDM_SETITEM, prevColumn, ref hdItem);
                }

                // Set icon on the new column.
                hdItem = new User32.HDITEM();
                hdItem.mask = User32.HDI_FORMAT;
                User32.SendMessageITEM(hHeader, User32.HDM_GETITEM, newColumn, ref hdItem);
                if (m_direction == 1)
                {
                    hdItem.fmt &= ~User32.HDF_SORTDOWN;
                    hdItem.fmt |= User32.HDF_SORTUP;
                }
                else
                {
                    hdItem.fmt &= ~User32.HDF_SORTUP;
                    hdItem.fmt |= User32.HDF_SORTDOWN;
                }
                User32.SendMessageITEM(hHeader, User32.HDM_SETITEM, newColumn, ref hdItem);
              

                m_column = e.Column;

                m_listView.ListViewItemSorter = null;
                m_listView.ListViewItemSorter = this;
            }

            private readonly ListView m_listView;
            private int m_column = -1; 
            private int m_direction = 1;
        }

        private readonly ListView m_control;

        private IListView m_listView;
        private IItemView m_itemView;
        private IValidationContext m_validationContext;
        private IObservableContext m_observableContext;
        private ISelectionContext m_selectionContext;

        private bool m_inTransaction;
        private readonly Dictionary<object, int> m_itemsInserted = new Dictionary<object, int>();
        private readonly HashSet<object> m_itemsRemoved = new HashSet<object>();
        private readonly HashSet<object> m_itemsChanged = new HashSet<object>();

        private object m_lastHit;

        private readonly Dictionary<object, ListViewItem> m_itemToListItemMap = new Dictionary<object, ListViewItem>();
        private string[] m_columnNames = EmptyArray<string>.Instance;
        private readonly Dictionary<string, int> m_columnWidths = new Dictionary<string, int>();
        private bool m_allowSorting;
        private bool m_changingSelection;

        private ListViewItem m_selectionStartItem;
    }
}
